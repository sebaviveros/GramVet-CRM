using GramVetCRM.Model;
using GramVetCRM.Model.DTOs.Conversacion;
using GramVetCRM.Model.DTOs.Mensaje;
using GramVetCRM.Repository.Repositories;
using GramVetCRM.Service.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace GramVetCRM.Service
{
    // Maneja la mensajería de Facebook Messenger e Instagram Direct (Meta Graph API).
    // Es el equivalente de WhatsAppService pero para los canales de página (Messenger/IG),
    // que comparten el mismo formato de webhook y el mismo endpoint de envío.
    public class MetaMessagingService : IMetaMessagingService
    {
        private readonly IMensajeRepository _mensajeRepo;
        private readonly IConversacionRepository _conversacionRepo;
        private readonly IContactoRepository _contactoRepo;
        private readonly ICanalRepository _canalRepo;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<MetaMessagingService> _logger;

        private const string GraphVersion = "v21.0";

        public MetaMessagingService(
            IMensajeRepository mensajeRepo,
            IConversacionRepository conversacionRepo,
            IContactoRepository contactoRepo,
            ICanalRepository canalRepo,
            IHubContext<ChatHub> hubContext,
            IConfiguration config,
            IHttpClientFactory httpClientFactory,
            ILogger<MetaMessagingService> logger)
        {
            _mensajeRepo = mensajeRepo;
            _conversacionRepo = conversacionRepo;
            _contactoRepo = contactoRepo;
            _canalRepo = canalRepo;
            _hubContext = hubContext;
            _config = config;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        public async Task ProcesarMensaje(JsonElement body)
        {
            try
            {
                // object: "page" => Messenger ; "instagram" => Instagram
                var objeto = body.TryGetProperty("object", out var o) ? o.GetString() : null;
                var plataforma = objeto == "instagram" ? "Instagram" : "Messenger";

                if (!body.TryGetProperty("entry", out var entries) || entries.GetArrayLength() == 0)
                {
                    _logger.LogInformation("Webhook Meta sin 'entry'");
                    return;
                }

                foreach (var entry in entries.EnumerateArray())
                {
                    if (!entry.TryGetProperty("messaging", out var messagingArr))
                        continue;

                    foreach (var messaging in messagingArr.EnumerateArray())
                    {
                        await ProcesarEvento(messaging, plataforma);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando webhook de Meta ({Plataforma})", "Messenger/IG");
            }
        }

        private async Task ProcesarEvento(JsonElement messaging, string plataforma)
        {
            // Ignorar echoes (mensajes que la propia página envía, ya los persistimos nosotros)
            if (messaging.TryGetProperty("message", out var msgCheck) &&
                msgCheck.TryGetProperty("is_echo", out var echo) && echo.GetBoolean())
            {
                return;
            }

            if (!messaging.TryGetProperty("sender", out var sender) ||
                !sender.TryGetProperty("id", out var senderIdEl))
                return;

            var senderId = senderIdEl.GetString();
            if (string.IsNullOrEmpty(senderId)) return;

            if (!messaging.TryGetProperty("message", out var message))
                return; // puede ser delivery/read/postback — fuera de alcance del scaffold

            string? contenido = message.TryGetProperty("text", out var textEl) ? textEl.GetString() : null;
            string? externalId = message.TryGetProperty("mid", out var midEl) ? midEl.GetString() : null;

            // Adjuntos (imagen/audio/video) — tomamos la url del primero
            string? mediaUrl = null;
            string tipo = "text";
            if (message.TryGetProperty("attachments", out var attachments) && attachments.GetArrayLength() > 0)
            {
                var att = attachments[0];
                if (att.TryGetProperty("type", out var attType)) tipo = attType.GetString() ?? "file";
                if (att.TryGetProperty("payload", out var payload) &&
                    payload.TryGetProperty("url", out var urlEl))
                    mediaUrl = urlEl.GetString();
            }

            // Clave única del contacto por plataforma (no hay teléfono en Messenger/IG)
            var prefijo = plataforma == "Instagram" ? "IG:" : "FB:";
            var telefonoKey = prefijo + senderId;

            // Contacto
            var contacto = await _contactoRepo.GetByTelefono(telefonoKey);
            if (contacto == null)
            {
                // Traer nombre real del perfil vía Graph API (requiere pages_read_engagement)
                var (nombrePerfil, apellidoPerfil) = await ObtenerPerfil(senderId!, plataforma);

                contacto = new Contacto
                {
                    Nombre = nombrePerfil,
                    Apellido = apellidoPerfil,
                    Telefono = telefonoKey,
                    EsNuevo = true,
                    Usercr = plataforma.ToLower(),
                    Fechacr = DateTime.Now,
                    Active = true
                };
                await _contactoRepo.Add(contacto);
                await _contactoRepo.Save();
            }

            // Canal (siembra IG/Messenger si no existen)
            var canal = await _canalRepo.GetOrCreate(plataforma);

            // Conversación abierta (o reabrir / crear)
            var conversacion = await _conversacionRepo.GetByTelefono(telefonoKey);
            if (conversacion == null)
            {
                var cerrada = await _conversacionRepo.GetUltimaByContacto(contacto.Id);
                if (cerrada != null)
                {
                    cerrada.Estado = "Abierta";
                    cerrada.Userup = plataforma.ToLower();
                    cerrada.Fechaup = DateTime.Now;
                    await _conversacionRepo.Save();
                    conversacion = cerrada;
                }
                else
                {
                    conversacion = new Conversacion
                    {
                        ContactoId = contacto.Id,
                        CanalId = canal.Id,
                        Estado = "Abierta",
                        UltimoMensaje = tipo == "text" ? contenido : $"[{tipo}]",
                        FechaUltimoMensaje = DateTime.Now,
                        CantidadNoLeidos = 0,
                        Usercr = plataforma.ToLower(),
                        Fechacr = DateTime.Now,
                        Active = true
                    };
                    await _conversacionRepo.Add(conversacion);
                    await _conversacionRepo.Save();
                }
            }

            // Guardar mensaje entrante
            var mensaje = new Mensaje
            {
                ConversacionId = conversacion.Id,
                Contenido = contenido,
                MediaUrl = mediaUrl,
                TipoMensaje = tipo,
                Direccion = "inbound",
                ExternalId = externalId,
                UsuarioId = null,
                FechaEnvio = DateTime.Now,
                Usercr = plataforma.ToLower(),
                Fechacr = DateTime.Now,
                Active = true
            };
            await _mensajeRepo.Add(mensaje);
            await _mensajeRepo.Save();

            conversacion.UltimoMensaje = tipo == "text" ? contenido : $"[{tipo}]";
            conversacion.FechaUltimoMensaje = DateTime.Now;
            conversacion.CantidadNoLeidos += 1;
            await _conversacionRepo.Save();

            // SignalR dirigido por grupos (igual que WhatsApp): staff + veterinario asignado
            var grupos = new List<string> { ChatGroups.Staff };
            if (conversacion.UsuarioAsignadoId.HasValue)
                grupos.Add(ChatGroups.Usuario(conversacion.UsuarioAsignadoId.Value));

            var mensajeDto = new MensajeDto
            {
                Id = mensaje.Id,
                ConversacionId = mensaje.ConversacionId,
                Contenido = mensaje.Contenido,
                MediaUrl = mensaje.MediaUrl,
                TipoMensaje = mensaje.TipoMensaje,
                Direccion = mensaje.Direccion,
                FechaEnvio = mensaje.FechaEnvio,
                UsuarioId = mensaje.UsuarioId
            };
            await _hubContext.Clients.Groups(grupos).SendAsync("NuevoMensaje", mensajeDto);

            // Recargar con includes para el DTO de conversación
            var conv = await _conversacionRepo.GetById(conversacion.Id) ?? conversacion;
            var conversacionDto = new ConversacionDto
            {
                Id = conv.Id,
                ContactoId = conv.ContactoId,
                NombreContacto = conv.Contacto?.Nombre ?? "",
                ApellidoContacto = conv.Contacto?.Apellido,
                Telefono = conv.Contacto?.Telefono ?? "",
                Estado = conv.Estado,
                UltimoMensaje = conv.UltimoMensaje,
                FechaUltimoMensaje = conv.FechaUltimoMensaje,
                CantidadNoLeidos = conv.CantidadNoLeidos,
                UsuarioAsignado = conv.UsuarioAsignado != null
                    ? $"{conv.UsuarioAsignado.Nombre} {conv.UsuarioAsignado.Apellido}".Trim()
                    : null,
                UsuarioAsignadoId = conv.UsuarioAsignadoId,
                Canal = conv.Canal?.Nombre ?? plataforma,
                EsNuevo = conv.Contacto?.EsNuevo ?? true
            };
            await _hubContext.Clients.Groups(grupos).SendAsync("ConversacionActualizada", conversacionDto);

            _logger.LogInformation("Mensaje {Plataforma} procesado para conversación {Id}", plataforma, conv.Id);
        }

        // Obtiene el nombre real del perfil del usuario (Messenger: first/last name; Instagram: name/username)
        private async Task<(string nombre, string? apellido)> ObtenerPerfil(string id, string plataforma)
        {
            try
            {
                var token = _config["Meta:PageAccessToken"];
                if (string.IsNullOrWhiteSpace(token))
                    return (id, null);

                var fields = plataforma == "Instagram" ? "name,username" : "first_name,last_name";
                var url = $"https://graph.facebook.com/{GraphVersion}/{id}?fields={fields}&access_token={token}";

                var resp = await _httpClient.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("No se pudo obtener perfil de {Id} ({Plataforma})", id, plataforma);
                    return (id, null);
                }

                var body = await resp.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(body).RootElement;

                if (plataforma == "Instagram")
                {
                    var nombre = json.TryGetProperty("name", out var n) ? n.GetString() : null;
                    if (string.IsNullOrWhiteSpace(nombre) && json.TryGetProperty("username", out var u))
                        nombre = u.GetString();
                    return (string.IsNullOrWhiteSpace(nombre) ? id : nombre!, null);
                }
                else
                {
                    var fn = json.TryGetProperty("first_name", out var f) ? f.GetString() : null;
                    var ln = json.TryGetProperty("last_name", out var l) ? l.GetString() : null;
                    return (string.IsNullOrWhiteSpace(fn) ? id : fn!, ln);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo perfil de {Id}", id);
                return (id, null);
            }
        }

        public async Task<bool> EnviarMensaje(string destinatarioId, string mensaje)
        {
            try
            {
                var pageToken = _config["Meta:PageAccessToken"];
                if (string.IsNullOrWhiteSpace(pageToken))
                {
                    _logger.LogWarning("[META SIMULADO - sin PageAccessToken] Para {Id}: {Msg}", destinatarioId, mensaje);
                    return false;
                }

                var url = $"https://graph.facebook.com/{GraphVersion}/me/messages?access_token={pageToken}";

                var payload = new
                {
                    recipient = new { id = destinatarioId },
                    messaging_type = "RESPONSE",
                    message = new { text = mensaje }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Mensaje Meta enviado a {Id}", destinatarioId);
                    return true;
                }

                _logger.LogError("Error enviando mensaje Meta a {Id}: {Error}", destinatarioId, responseBody);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción enviando mensaje Meta a {Id}", destinatarioId);
                return false;
            }
        }
    }
}
