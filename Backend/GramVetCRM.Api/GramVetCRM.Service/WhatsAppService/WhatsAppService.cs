using GramVetCRM.Service.Hubs;
using GramVetCRM.Model;
using GramVetCRM.Model.DTOs.Mensaje;
using GramVetCRM.Repository.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
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

        public WhatsAppService(
            IMensajeRepository mensajeRepo,
            IConversacionRepository conversacionRepo,
            IContactoRepository contactoRepo,
            ILogger<WhatsAppService> logger,
            IHubContext<ChatHub> hubContext)
        {
            _mensajeRepo = mensajeRepo;
            _conversacionRepo = conversacionRepo;
            _contactoRepo = contactoRepo;
            _logger = logger;
            _hubContext = hubContext;
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

                string? contenido = null;
                if (tipo == "text")
                {
                    contenido = message
                        .GetProperty("text")
                        .GetProperty("body")
                        .GetString();
                }

                //  Buscar o crear contacto
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
                            UltimoMensaje = contenido,
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

                //  Guardar mensaje
                var mensaje = new Mensaje
                {
                    ConversacionId = conversacion.Id,
                    Contenido = contenido,
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

                //  Actualizar conversación
                conversacion.UltimoMensaje = contenido;
                conversacion.FechaUltimoMensaje = DateTime.Now;
                conversacion.CantidadNoLeidos += 1;
                await _conversacionRepo.Save();

                //  Notificar al frontend via SignalR
                var mensajeDto = new MensajeDto
                {
                    Id = mensaje.Id,
                    ConversacionId = mensaje.ConversacionId,
                    Contenido = mensaje.Contenido,
                    TipoMensaje = mensaje.TipoMensaje,
                    Direccion = mensaje.Direccion,
                    FechaEnvio = mensaje.FechaEnvio,
                    UsuarioId = mensaje.UsuarioId
                };

                await _hubContext.Clients.All.SendAsync("NuevoMensaje", mensajeDto);
                _logger.LogInformation("Notificación SignalR enviada para conversación {Id}", conversacion.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando mensaje de WhatsApp");
            }
        }
    }
}