using System.Text;
using System.Text.Json;
using GramVetCRM.Model;
using GramVetCRM.Model.DTOs.Cita;
using GramVetCRM.Repository.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GramVetCRM.Service
{
    public class AgendaIaService : IAgendaIaService
    {
        private readonly IConversacionRepository _conversacionRepo;
        private readonly IMensajeRepository _mensajeRepo;
        private readonly IContactoRepository _contactoRepo;
        private readonly IMascotaRepository _mascotaRepo;
        private readonly IConfiguration _config;
        private readonly ILogger<AgendaIaService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IGoogleCalendarService _googleCalendar;

        // Cantidad de mensajes recientes que se le pasan a la IA como contexto
        private const int MaxMensajes = 40;

        public AgendaIaService(
            IConversacionRepository conversacionRepo,
            IMensajeRepository mensajeRepo,
            IContactoRepository contactoRepo,
            IMascotaRepository mascotaRepo,
            IConfiguration config,
            ILogger<AgendaIaService> logger,
            IHttpClientFactory httpClientFactory,
            IGoogleCalendarService googleCalendar)
        {
            _conversacionRepo = conversacionRepo;
            _mensajeRepo = mensajeRepo;
            _contactoRepo = contactoRepo;
            _mascotaRepo = mascotaRepo;
            _config = config;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("Anthropic");
            _googleCalendar = googleCalendar;
        }

        public async Task<CitaExtraidaDto> ExtraerCita(int conversacionId)
        {
            var conversacion = await _conversacionRepo.GetById(conversacionId)
                ?? throw new Exception($"Conversación {conversacionId} no encontrada");

            var contacto = await _contactoRepo.GetById(conversacion.ContactoId);
            var mascotas = contacto != null
                ? await _mascotaRepo.GetByContacto(contacto.Id)
                : new List<Mascota>();

            // Últimos N mensajes de texto (los que aportan contenido a la extracción)
            var mensajes = await _mensajeRepo.GetByConversacion(conversacionId, 1, MaxMensajes);
            var mensajesTexto = mensajes
                .Where(m => !string.IsNullOrWhiteSpace(m.Contenido))
                .ToList();

            var apiKey = _config["Anthropic:ApiKey"];

            // Fallback simulado: sin API key, devolvemos un borrador con los datos del
            // contacto para poder desarrollar/probar el flujo sin gastar créditos.
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("[IA SIMULADA - Anthropic ApiKey no configurada] Conversación {Id}", conversacionId);
                return ConstruirSimulada(contacto, mascotas);
            }

            try
            {
                var dto = await LlamarClaude(apiKey, contacto, mascotas, mensajesTexto);
                ArmarTituloYDescripcion(dto);
                return dto;
            }
            catch (Exception ex)
            {
                // Ante cualquier fallo de la IA, no romper el flujo: devolver el borrador simulado.
                _logger.LogError(ex, "Error llamando a Claude para conversación {Id}, usando fallback simulado", conversacionId);
                return ConstruirSimulada(contacto, mascotas);
            }
        }

        // ── Crear la cita confirmada (mascotas + evento en el calendar) ──────────
        public async Task<CitaCreadaDto> CrearCita(int conversacionId, CrearCitaDto dto, int actorUsuarioId)
        {
            var conversacion = await _conversacionRepo.GetById(conversacionId)
                ?? throw new Exception($"Conversación {conversacionId} no encontrada");

            // 1. Calcular la posición física en el calendar (valida móvil/slot/día).
            var (inicio, fin) = AgendaSlots.CalcularPosicion(dto.Fecha, dto.Movil, dto.SlotIndex);

            // 2. Crear SOLO las mascotas que el cliente todavía no tiene (dedup por nombre,
            //    para no duplicar las ya registradas ni las repetidas dentro del request).
            var existentes = await _mascotaRepo.GetByContacto(conversacion.ContactoId);
            var nombresExistentes = existentes
                .Select(m => m.Nombre?.Trim().ToLowerInvariant())
                .Where(n => !string.IsNullOrEmpty(n))
                .ToHashSet();

            var creadas = 0;
            var vistas = new HashSet<string>();
            foreach (var m in dto.Mascotas ?? new List<MascotaCitaDto>())
            {
                var nombre = m.Nombre?.Trim();
                if (string.IsNullOrEmpty(nombre)) continue;

                var clave = nombre.ToLowerInvariant();
                if (nombresExistentes.Contains(clave) || !vistas.Add(clave))
                    continue; // ya existe en el cliente, o repetida en este request

                DateTime? fechaNac = null;
                if (!string.IsNullOrWhiteSpace(m.FechaNacimiento) &&
                    DateTime.TryParse(m.FechaNacimiento, out var fn))
                    fechaNac = fn;

                await _mascotaRepo.Add(new Mascota
                {
                    ContactoId = conversacion.ContactoId,
                    Nombre = nombre,
                    Especie = string.IsNullOrWhiteSpace(m.Especie) ? null : m.Especie.Trim(),
                    FechaNacimiento = fechaNac,
                    Usercr = actorUsuarioId.ToString(),
                    Fechacr = DateTime.Now,
                    Active = true
                });
                creadas++;
            }
            if (creadas > 0)
                await _mascotaRepo.Save();

            // 3. Completar los campos VACÍOS del perfil del cliente (no pisar los existentes).
            var camposPerfil = 0;
            var contacto = await _contactoRepo.GetById(conversacion.ContactoId);
            if (contacto != null)
            {
                if (string.IsNullOrWhiteSpace(contacto.Direccion) && !string.IsNullOrWhiteSpace(dto.Direccion))
                { contacto.Direccion = dto.Direccion.Trim(); camposPerfil++; }

                if (string.IsNullOrWhiteSpace(contacto.ReferenciaDireccion) && !string.IsNullOrWhiteSpace(dto.ReferenciasDireccion))
                { contacto.ReferenciaDireccion = dto.ReferenciasDireccion.Trim(); camposPerfil++; }

                if (string.IsNullOrWhiteSpace(contacto.Email) && !string.IsNullOrWhiteSpace(dto.Correo))
                { contacto.Email = dto.Correo.Trim(); camposPerfil++; }

                // Nombre: solo si el contacto entró sin nombre o como su número de teléfono.
                var sinNombre = string.IsNullOrWhiteSpace(contacto.Nombre) || contacto.Nombre == contacto.Telefono;
                if (sinNombre && !string.IsNullOrWhiteSpace(dto.NombreCliente))
                {
                    var partes = dto.NombreCliente.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    contacto.Nombre = partes[0];
                    if (partes.Length > 1) contacto.Apellido = partes[1];
                    contacto.EsNuevo = false;
                    camposPerfil++;
                }

                if (camposPerfil > 0)
                {
                    contacto.Userup = actorUsuarioId.ToString();
                    contacto.Fechaup = DateTime.Now;
                    await _contactoRepo.Save();
                }
            }

            // 4. Crear el evento en el Google Calendar madre (o simulado si no está configurado).
            var resultado = await _googleCalendar.CrearEvento(
                dto.TituloEvento, dto.DescripcionEvento, dto.Ubicacion, inicio, fin);

            resultado.MascotasCreadas = creadas;
            resultado.CamposPerfilActualizados = camposPerfil;
            return resultado;
        }

        // ── Llamada a la Messages API de Anthropic (structured outputs) ──────────
        private async Task<CitaExtraidaDto> LlamarClaude(
            string apiKey, Contacto? contacto, List<Mascota> mascotas, List<Mensaje> mensajes)
        {
            var model = _config["Anthropic:Model"] ?? "claude-haiku-4-5";

            var transcript = ConstruirTranscript(contacto, mascotas, mensajes);

            var payload = new
            {
                model,
                max_tokens = 2048,
                system = SystemPrompt,
                messages = new[]
                {
                    new { role = "user", content = transcript }
                },
                output_config = new
                {
                    format = new
                    {
                        type = "json_schema",
                        schema = JsonSchema
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var response = await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Anthropic respondió {(int)response.StatusCode}: {responseBody}");

            // El primer bloque de texto trae el JSON validado por el schema
            using var doc = JsonDocument.Parse(responseBody);
            var texto = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(texto))
                throw new Exception("Respuesta de Claude sin contenido");

            var dto = JsonSerializer.Deserialize<CitaExtraidaDto>(texto,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new Exception("No se pudo parsear el JSON de Claude");

            dto.Simulada = false;
            return dto;
        }

        // ── Fallback simulado (mismo patrón que EmailService) ────────────────────
        private static CitaExtraidaDto ConstruirSimulada(Contacto? contacto, List<Mascota> mascotas)
        {
            var pacientes = mascotas.Count > 0
                ? string.Join(", ", mascotas.Select(m => m.Nombre))
                : "";

            var dto = new CitaExtraidaDto
            {
                NombreCliente = contacto != null
                    ? $"{contacto.Nombre} {contacto.Apellido}".Trim()
                    : "",
                Telefono = contacto?.Telefono ?? "",
                Direccion = contacto?.Direccion ?? "",
                ReferenciasDireccion = contacto?.ReferenciaDireccion ?? "",
                Correo = contacto?.Email ?? "",
                Pacientes = pacientes,
                // En simulado NO inventamos mascotas para la cita; la IA real las detecta del chat.
                Mascotas = new List<MascotaCitaDto>(),
                Simulada = true
            };

            ArmarTituloYDescripcion(dto);
            // Marca visible para distinguir el borrador simulado del real
            dto.TituloEvento = "[IA SIMULADA] " + dto.TituloEvento;
            return dto;
        }

        // ── Armado del título y descripción en el formato del cliente ────────────
        private static void ArmarTituloYDescripcion(CitaExtraidaDto dto)
        {
            // Título: [rango horario] [comuna/sector]: [dirección] +[teléfono] [nombre cliente]
            var partesTitulo = new List<string>();
            if (!string.IsNullOrWhiteSpace(dto.FechaHoraSugerida)) partesTitulo.Add(dto.FechaHoraSugerida.Trim());
            if (!string.IsNullOrWhiteSpace(dto.ComunaSector)) partesTitulo.Add(dto.ComunaSector.Trim() + ":");

            var resto = new List<string>();
            if (!string.IsNullOrWhiteSpace(dto.Direccion)) resto.Add(dto.Direccion.Trim());
            if (!string.IsNullOrWhiteSpace(dto.Telefono)) resto.Add(FormatearTelefono(dto.Telefono));
            if (!string.IsNullOrWhiteSpace(dto.NombreCliente)) resto.Add(dto.NombreCliente.Trim());

            dto.TituloEvento = string.Join(" ", partesTitulo.Concat(resto)).Trim();

            // Descripción: secciones fijas, en orden
            var sb = new StringBuilder();
            sb.AppendLine($"Paciente(s): {dto.Pacientes}");
            sb.AppendLine($"Cliente solicitó: {dto.ClienteSolicito}");
            sb.AppendLine($"Se cobró: {dto.Cobros}");
            sb.AppendLine($"Total mínimo a cobrar: {dto.TotalMinimo}");
            sb.AppendLine($"Referencias para encontrar el domicilio: {dto.ReferenciasDireccion}");
            sb.AppendLine($"Observaciones: {dto.Observaciones}");
            sb.Append($"Correo: {dto.Correo}");
            dto.DescripcionEvento = sb.ToString();
        }

        private static string FormatearTelefono(string telefono)
        {
            var t = telefono.Trim();
            return t.StartsWith("+") ? t : "+" + t;
        }

        // ── Construcción del transcript que ve la IA ─────────────────────────────
        private static string ConstruirTranscript(Contacto? contacto, List<Mascota> mascotas, List<Mensaje> mensajes)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DATOS DEL CONTACTO:");
            sb.AppendLine($"- Nombre: {contacto?.Nombre} {contacto?.Apellido}".TrimEnd());
            sb.AppendLine($"- Teléfono: {contacto?.Telefono}");
            if (!string.IsNullOrWhiteSpace(contacto?.Direccion))
                sb.AppendLine($"- Dirección: {contacto.Direccion}");
            if (!string.IsNullOrWhiteSpace(contacto?.ReferenciaDireccion))
                sb.AppendLine($"- Referencias: {contacto.ReferenciaDireccion}");
            if (!string.IsNullOrWhiteSpace(contacto?.Email))
                sb.AppendLine($"- Correo: {contacto.Email}");
            if (mascotas.Count > 0)
                sb.AppendLine($"- Mascotas: {string.Join(", ", mascotas.Select(m => $"{m.Nombre} ({m.Especie})".Replace(" ()", "")))}");

            sb.AppendLine();
            sb.AppendLine("CONVERSACIÓN (más antigua arriba):");
            foreach (var m in mensajes)
            {
                var quien = m.Direccion == "inbound" ? "Cliente" : "Veterinaria";
                sb.AppendLine($"{quien}: {m.Contenido}");
            }

            return sb.ToString();
        }

        private const string SystemPrompt =
            "Eres un asistente de una veterinaria a domicilio en Chile. A partir de la conversación " +
            "con un cliente, extrae los datos para agendar una visita. Completa cada campo solo con " +
            "información presente en la conversación o en los datos del contacto. Si un dato no aparece, " +
            "déjalo como string vacío (o false en los booleanos, o lista vacía); NO inventes datos. " +
            "Para 'fechaHoraSugerida' usa el rango horario o día que el cliente haya mencionado " +
            "(ej. '1 - 3', 'viernes en la tarde'). Para 'cobros' y 'totalMinimo' usa montos en pesos " +
            "chilenos si se mencionan. Para 'mascotas' lista cada mascota mencionada con su nombre y " +
            "especie ('perro' o 'gato'); si no se menciona el nombre o la especie, deja ese campo vacío. " +
            "'ubicacionGps' es un link de Google Maps o coordenadas si el cliente compartió ubicación. " +
            "'seguroMascota' = true solo si menciona que la atención involucra un seguro de mascota. " +
            "'estacionamientoVisita' = true solo si menciona que vive en condominio con estacionamiento " +
            "de visita. Responde en español.";

        // Schema de salida: los campos que la IA debe devolver.
        // additionalProperties:false y required en todos (requisito de structured outputs).
        private static readonly object JsonSchema = new
        {
            type = "object",
            properties = new
            {
                nombreCliente = new { type = "string" },
                telefono = new { type = "string" },
                direccion = new { type = "string" },
                comunaSector = new { type = "string" },
                referenciasDireccion = new { type = "string" },
                correo = new { type = "string" },
                pacientes = new { type = "string" },
                clienteSolicito = new { type = "string" },
                cobros = new { type = "string" },
                totalMinimo = new { type = "string" },
                observaciones = new { type = "string" },
                fechaHoraSugerida = new { type = "string" },
                ubicacionGps = new { type = "string" },
                seguroMascota = new { type = "boolean" },
                estacionamientoVisita = new { type = "boolean" },
                mascotas = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            nombre = new { type = "string" },
                            especie = new { type = "string" }
                        },
                        required = new[] { "nombre", "especie" },
                        additionalProperties = false
                    }
                }
            },
            required = new[]
            {
                "nombreCliente", "telefono", "direccion", "comunaSector", "referenciasDireccion",
                "correo", "pacientes", "clienteSolicito", "cobros", "totalMinimo",
                "observaciones", "fechaHoraSugerida", "ubicacionGps", "seguroMascota",
                "estacionamientoVisita", "mascotas"
            },
            additionalProperties = false
        };
    }
}
