using GramVetCRM.Model;
using GramVetCRM.Model.DTOs.Conversacion;
using GramVetCRM.Model.DTOs.Mensaje;
using GramVetCRM.Repository.Repositories;
using GramVetCRM.Service.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace GramVetCRM.Service
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly IMensajeRepository _mensajeRepo;
        private readonly IConversacionRepository _conversacionRepo;
        private readonly IContactoRepository _contactoRepo;
        private readonly ILogger<WhatsAppService> _logger;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly IR2StorageService _r2Storage;

        public WhatsAppService(
            IMensajeRepository mensajeRepo,
            IConversacionRepository conversacionRepo,
            IContactoRepository contactoRepo,
            ILogger<WhatsAppService> logger,
            IHubContext<ChatHub> hubContext,
            IConfiguration config,
            IHttpClientFactory httpClientFactory,
            IR2StorageService r2Storage)
        {
            _mensajeRepo = mensajeRepo;
            _conversacionRepo = conversacionRepo;
            _contactoRepo = contactoRepo;
            _logger = logger;
            _hubContext = hubContext;
            _config = config;
            _httpClient = httpClientFactory.CreateClient("WhatsApp");
            _r2Storage = r2Storage;
        }

        public async Task ProcesarMensaje(JsonElement body)
        {
            try
            {
                var entry = body
                    .GetProperty("entry")[0]
                    .GetProperty("changes")[0]
                    .GetProperty("value");

                if (!entry.TryGetProperty("messages", out var messages))
                {
                    _logger.LogInformation("Webhook recibido sin mensajes — posiblemente status update");
                    return;
                }

                var message = messages[0];
                var externalId = message.GetProperty("id").GetString();
                var from = message.GetProperty("from").GetString();
                var tipo = message.GetProperty("type").GetString();

                _logger.LogInformation("Mensaje recibido de {From}, tipo: {Tipo}", from, tipo);

                // Reacción a un mensaje existente — no crea fila, actualiza el target
                if (tipo == "reaction")
                {
                    await ProcesarReaccion(message);
                    return;
                }

                string? contenido = null;
                string? mediaUrl = null;

                if (tipo == "text")
                {
                    contenido = message
                        .GetProperty("text")
                        .GetProperty("body")
                        .GetString();
                }
                else if (tipo == "image")
                {
                    var mediaId = message.GetProperty("image").GetProperty("id").GetString();
                    mediaUrl = await DescargarMedia(mediaId!, "image");
                    if (message.GetProperty("image").TryGetProperty("caption", out var caption))
                        contenido = caption.GetString();
                }
                else if (tipo == "audio")
                {
                    var mediaId = message.GetProperty("audio").GetProperty("id").GetString();
                    mediaUrl = await DescargarMedia(mediaId!, "audio");
                }
                else if (tipo == "video")
                {
                    var mediaId = message.GetProperty("video").GetProperty("id").GetString();
                    mediaUrl = await DescargarMedia(mediaId!, "video");
                    if (message.GetProperty("video").TryGetProperty("caption", out var caption))
                        contenido = caption.GetString();
                }
                else if (tipo == "location")
                {
                    var loc = message.GetProperty("location");
                    var lat = loc.GetProperty("latitude").GetDouble();
                    var lng = loc.GetProperty("longitude").GetDouble();
                    mediaUrl = MapsUrl(lat, lng);

                    var partes = new List<string>();
                    if (loc.TryGetProperty("name", out var nm) && !string.IsNullOrWhiteSpace(nm.GetString()))
                        partes.Add(nm.GetString()!);
                    if (loc.TryGetProperty("address", out var ad) && !string.IsNullOrWhiteSpace(ad.GetString()))
                        partes.Add(ad.GetString()!);
                    contenido = partes.Count > 0 ? string.Join(" — ", partes) : null;
                }

                var resumen = ResumenMensaje(tipo!, contenido);

                // Buscar o crear contacto
                var contacto = await _contactoRepo.GetByTelefono(from!);
                if (contacto == null)
                {
                    _logger.LogInformation("Contacto no encontrado para {From}, creando nuevo", from);
                    contacto = new Contacto
                    {
                        Nombre = from!,
                        Telefono = from!,
                        EsNuevo = true,
                        Usercr = "whatsapp",
                        Fechacr = DateTime.Now,
                        Active = true
                    };
                    await _contactoRepo.Add(contacto);
                    await _contactoRepo.Save();
                    _logger.LogInformation("Contacto creado con Id: {ContactoId}", contacto.Id);
                }

                //  Buscar conversación abierta
                var conversacion = await _conversacionRepo.GetByTelefono(from!);
                if (conversacion == null)
                {
                    var conversacionCerrada = await _conversacionRepo.GetUltimaByContacto(contacto.Id);
                    if (conversacionCerrada != null)
                    {
                        _logger.LogInformation("Reabriendo conversación cerrada Id: {Id}", conversacionCerrada.Id);
                        conversacionCerrada.Estado = "Abierta";
                        conversacionCerrada.Userup = "whatsapp";
                        conversacionCerrada.Fechaup = DateTime.Now;
                        await _conversacionRepo.Save();
                        conversacion = conversacionCerrada;
                    }
                    else
                    {
                        _logger.LogInformation("Creando nueva conversación para contacto Id: {Id}", contacto.Id);
                        conversacion = new Conversacion
                        {
                            ContactoId = contacto.Id,
                            CanalId = 1,
                            Estado = "Abierta",
                            UltimoMensaje = resumen,
                            FechaUltimoMensaje = DateTime.Now,
                            CantidadNoLeidos = 0,
                            Usercr = "whatsapp",
                            Fechacr = DateTime.Now,
                            Active = true
                        };
                        await _conversacionRepo.Add(conversacion);
                        await _conversacionRepo.Save();
                        _logger.LogInformation("Conversación creada con Id: {Id}", conversacion.Id);
                    }
                }

                // Guardar mensaje
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
                    Usercr = "whatsapp",
                    Fechacr = DateTime.Now,
                    Active = true
                };

                await _mensajeRepo.Add(mensaje);
                await _mensajeRepo.Save();
                _logger.LogInformation("Mensaje guardado con Id: {Id}", mensaje.Id);

                // Actualizar conversación
                conversacion.UltimoMensaje = resumen;
                conversacion.FechaUltimoMensaje = DateTime.Now;
                conversacion.CantidadNoLeidos += 1;
                await _conversacionRepo.Save();

                // Notificar frontend via SignalR
                var mensajeDto = new MensajeDto
                {
                    Id = mensaje.Id,
                    ConversacionId = mensaje.ConversacionId,
                    Contenido = mensaje.Contenido,
                    MediaUrl = mensaje.MediaUrl,
                    TipoMensaje = mensaje.TipoMensaje,
                    Direccion = mensaje.Direccion,
                    FechaEnvio = mensaje.FechaEnvio,
                    UsuarioId = mensaje.UsuarioId,
                    Reaccion = mensaje.Reaccion
                };

                // Grupos destino: staff (admin/secretario) + veterinario asignado (si lo hay)
                var grupos = new List<string> { ChatGroups.Staff };
                if (conversacion.UsuarioAsignadoId.HasValue)
                    grupos.Add(ChatGroups.Usuario(conversacion.UsuarioAsignadoId.Value));

                // Notificar mensaje nuevo
                await _hubContext.Clients.Groups(grupos).SendAsync("NuevoMensaje", mensajeDto);

                // Recargar conversación con includes para el DTO
                conversacion = await _conversacionRepo.GetById(conversacion.Id) ?? conversacion;

                // Notificar conversación actualizada (para actualizar lista)
                var conversacionDto = new ConversacionDto
                {
                    Id = conversacion.Id,
                    ContactoId = conversacion.ContactoId,
                    NombreContacto = conversacion.Contacto?.Nombre ?? "",
                    ApellidoContacto = conversacion.Contacto?.Apellido,
                    Telefono = conversacion.Contacto?.Telefono ?? "",
                    Estado = conversacion.Estado,
                    UltimoMensaje = conversacion.UltimoMensaje,
                    FechaUltimoMensaje = conversacion.FechaUltimoMensaje,
                    CantidadNoLeidos = conversacion.CantidadNoLeidos,
                    Canal = conversacion.Canal?.Nombre ?? "WhatsApp",
                    UsuarioAsignado = conversacion.UsuarioAsignado != null
                        ? $"{conversacion.UsuarioAsignado.Nombre} {conversacion.UsuarioAsignado.Apellido}".Trim()
                        : null,
                    UsuarioAsignadoId = conversacion.UsuarioAsignadoId,
                    EsNuevo = conversacion.Contacto?.EsNuevo ?? true
                };

                // Recalcular grupos por si la conversación recargada trae asignado
                var gruposConv = new List<string> { ChatGroups.Staff };
                if (conversacion.UsuarioAsignadoId.HasValue)
                    gruposConv.Add(ChatGroups.Usuario(conversacion.UsuarioAsignadoId.Value));

                await _hubContext.Clients.Groups(gruposConv).SendAsync("ConversacionActualizada", conversacionDto);

                _logger.LogInformation("Notificaciones SignalR enviadas para conversación {Id}", conversacion.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando mensaje de WhatsApp");
            }
        }

        // Procesa una reacción entrante: ubica el mensaje por su wamid y guarda el emoji
        private async Task ProcesarReaccion(JsonElement message)
        {
            try
            {
                var reaction = message.GetProperty("reaction");
                var targetId = reaction.GetProperty("message_id").GetString();
                var emoji = reaction.TryGetProperty("emoji", out var e) ? e.GetString() : null;
                if (string.IsNullOrEmpty(targetId)) return;

                var mensaje = await _mensajeRepo.GetByExternalId(targetId);
                if (mensaje == null)
                {
                    _logger.LogWarning("Reacción a mensaje desconocido {Id}", targetId);
                    return;
                }

                // emoji vacío = el usuario quitó la reacción
                mensaje.Reaccion = string.IsNullOrEmpty(emoji) ? null : emoji;
                await _mensajeRepo.Save();

                var conv = await _conversacionRepo.GetById(mensaje.ConversacionId);
                var grupos = new List<string> { ChatGroups.Staff };
                if (conv?.UsuarioAsignadoId.HasValue == true)
                    grupos.Add(ChatGroups.Usuario(conv.UsuarioAsignadoId.Value));

                await _hubContext.Clients.Groups(grupos).SendAsync("MensajeReaccionado", new
                {
                    MensajeId = mensaje.Id,
                    Reaccion = mensaje.Reaccion,
                    ConversacionId = mensaje.ConversacionId
                });

                _logger.LogInformation("Reacción {Emoji} aplicada al mensaje {Id}", emoji, mensaje.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando reacción entrante");
            }
        }

        public async Task<string?> EnviarMensajeTexto(string telefono, string mensaje)
        {
            try
            {
                var phoneNumberId = _config["WhatsApp:PhoneNumberId"];
                var accessToken = _config["WhatsApp:AccessToken"];
                var url = $"https://graph.facebook.com/v25.0/{phoneNumberId}/messages";

                var payload = new
                {
                    messaging_product = "whatsapp",
                    to = telefono,
                    type = "text",
                    text = new { body = mensaje }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Mensaje texto enviado a {Telefono}", telefono);
                    return ExtraerWamid(responseBody);
                }

                _logger.LogError("Error enviando texto a {Telefono}: {Error}", telefono, responseBody);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción enviando texto a {Telefono}", telefono);
                return null;
            }
        }

        public async Task<string?> EnviarImagen(string telefono, string mediaId, string? caption)
        {
            try
            {
                var phoneNumberId = _config["WhatsApp:PhoneNumberId"];
                var accessToken = _config["WhatsApp:AccessToken"];
                var url = $"https://graph.facebook.com/v25.0/{phoneNumberId}/messages";

                var payload = new
                {
                    messaging_product = "whatsapp",
                    to = telefono,
                    type = "image",
                    image = new { id = mediaId, caption }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Imagen enviada a {Telefono}", telefono);
                    return ExtraerWamid(responseBody);
                }

                _logger.LogError("Error enviando imagen a {Telefono}: {Error}", telefono, responseBody);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción enviando imagen a {Telefono}", telefono);
                return null;
            }
        }

        public async Task<string?> SubirMedia(Stream fileStream, string fileName, string mimeType)
        {
            try
            {
                var phoneNumberId = _config["WhatsApp:PhoneNumberId"];
                var accessToken = _config["WhatsApp:AccessToken"];
                var url = $"https://graph.facebook.com/v25.0/{phoneNumberId}/media";

                var formData = new MultipartFormDataContent();
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
                formData.Add(fileContent, "file", fileName);
                formData.Add(new StringContent("whatsapp"), "messaging_product");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.PostAsync(url, formData);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var jsonDoc = JsonDocument.Parse(responseBody);
                    var mediaId = jsonDoc.RootElement.GetProperty("id").GetString();
                    _logger.LogInformation("Media subida a WhatsApp, id: {MediaId}", mediaId);
                    return mediaId;
                }

                _logger.LogError("Error subiendo media a WhatsApp: {Error}", responseBody);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción subiendo media a WhatsApp");
                return null;
            }
        }

        public async Task<string?> EnviarUbicacion(string telefono, double latitud, double longitud, string? nombre)
        {
            try
            {
                var phoneNumberId = _config["WhatsApp:PhoneNumberId"];
                var accessToken = _config["WhatsApp:AccessToken"];
                var url = $"https://graph.facebook.com/v25.0/{phoneNumberId}/messages";

                var payload = new
                {
                    messaging_product = "whatsapp",
                    to = telefono,
                    type = "location",
                    location = new
                    {
                        latitude = latitud,
                        longitude = longitud,
                        name = nombre
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Ubicación enviada a {Telefono}", telefono);
                    return ExtraerWamid(responseBody);
                }

                _logger.LogError("Error enviando ubicación a {Telefono}: {Error}", telefono, responseBody);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción enviando ubicación a {Telefono}", telefono);
                return null;
            }
        }

        public async Task<bool> EnviarReaccion(string telefono, string messageId, string emoji)
        {
            try
            {
                var phoneNumberId = _config["WhatsApp:PhoneNumberId"];
                var accessToken = _config["WhatsApp:AccessToken"];
                var url = $"https://graph.facebook.com/v25.0/{phoneNumberId}/messages";

                var payload = new
                {
                    messaging_product = "whatsapp",
                    to = telefono,
                    type = "reaction",
                    reaction = new { message_id = messageId, emoji }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Reacción enviada a {Telefono}", telefono);
                    return true;
                }

                _logger.LogError("Error enviando reacción a {Telefono}: {Error}", telefono, responseBody);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción enviando reacción a {Telefono}", telefono);
                return false;
            }
        }

        // Extrae el wamid (messages[0].id) de la respuesta de envío de WhatsApp
        private static string? ExtraerWamid(string responseBody)
        {
            try
            {
                var json = JsonDocument.Parse(responseBody).RootElement;
                if (json.TryGetProperty("messages", out var msgs) && msgs.GetArrayLength() > 0 &&
                    msgs[0].TryGetProperty("id", out var idEl))
                    return idEl.GetString();
            }
            catch { /* ignore */ }
            return null;
        }

        // URL de Google Maps a partir de coordenadas (InvariantCulture para no usar coma decimal)
        public static string MapsUrl(double lat, double lng) =>
            $"https://www.google.com/maps?q={lat.ToString(CultureInfo.InvariantCulture)},{lng.ToString(CultureInfo.InvariantCulture)}";

        // Texto resumen para "último mensaje" según el tipo
        public static string ResumenMensaje(string tipo, string? contenido) => tipo switch
        {
            "text" => contenido ?? "",
            "location" => "📍 Ubicación",
            "image" => "📷 Imagen",
            "audio" => "🎵 Audio",
            "video" => "🎬 Video",
            _ => $"[{tipo}]"
        };

        private async Task<string?> DescargarMedia(string mediaId, string tipo)
        {
            try
            {
                var accessToken = _config["WhatsApp:AccessToken"];

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                // Obtener URL del media desde WhatsApp
                var metaResponse = await _httpClient.GetStringAsync(
                    $"https://graph.facebook.com/v25.0/{mediaId}"
                );

                var metaJson = JsonDocument.Parse(metaResponse);
                var mediaUrl = metaJson.RootElement.GetProperty("url").GetString();
                var mimeType = metaJson.RootElement.GetProperty("mime_type").GetString();

                // Descargar el archivo
                var fileBytes = await _httpClient.GetByteArrayAsync(mediaUrl);

                // Determinar extensión
                var extension = tipo switch
                {
                    "image" => ".jpg",
                    "audio" => ".ogg",
                    "video" => ".mp4",
                    _ => ".bin"
                };

                // Subir a Cloudflare R2
                var fileName = $"{tipo}/{Guid.NewGuid()}{extension}";
                using var stream = new MemoryStream(fileBytes);
                var publicUrl = await _r2Storage.SubirArchivo(stream, fileName, mimeType ?? "application/octet-stream");

                _logger.LogInformation("Media subida a R2: {Url}", publicUrl);
                return publicUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error descargando media {MediaId}", mediaId);
                return null;
            }
        }
    }
}