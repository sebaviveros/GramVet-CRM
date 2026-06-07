using GramVetCRM.Model;
using GramVetCRM.Model.DTOs.Conversacion;
using GramVetCRM.Model.DTOs.Mensaje;
using GramVetCRM.Repository.Repositories;
using GramVetCRM.Service.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GramVetCRM.Service
{
    public class ConversacionService : IConversacionService
    {
        private readonly IConversacionRepository _conversacionRepo;
        private readonly IMensajeRepository _mensajeRepo;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ConversacionService(
            IConversacionRepository conversacionRepo,
            IMensajeRepository mensajeRepo,
            IWhatsAppService whatsAppService,
            IHubContext<ChatHub> hubContext)
        {
            _conversacionRepo = conversacionRepo;
            _mensajeRepo = mensajeRepo;
            _whatsAppService = whatsAppService;
            _hubContext = hubContext;
        }

        public async Task<List<ConversacionDto>> GetAll()
        {
            var conversaciones = await _conversacionRepo.GetAll();

            return conversaciones.Select(c => new ConversacionDto
            {
                Id = c.Id,
                ContactoId = c.ContactoId,
                NombreContacto = c.Contacto.Nombre,
                ApellidoContacto = c.Contacto.Apellido,
                Telefono = c.Contacto.Telefono,
                Estado = c.Estado,
                UltimoMensaje = c.UltimoMensaje,
                FechaUltimoMensaje = c.FechaUltimoMensaje,
                CantidadNoLeidos = c.CantidadNoLeidos,
                Canal = c.Canal.Nombre,
                EsNuevo = c.Contacto.EsNuevo
            }).ToList();
        }

        public async Task<List<MensajeDto>> GetMensajes(int conversacionId, int page, int pageSize)
        {
            var mensajes = await _mensajeRepo.GetByConversacion(conversacionId, page, pageSize);

            return mensajes.Select(m => new MensajeDto
            {
                Id = m.Id,
                ConversacionId = m.ConversacionId,
                Contenido = m.Contenido,
                MediaUrl = m.MediaUrl,
                TipoMensaje = m.TipoMensaje,
                Direccion = m.Direccion,
                FechaEnvio = m.FechaEnvio,
                UsuarioId = m.UsuarioId
            }).ToList();
        }

        public async Task<MensajeDto> EnviarMensaje(EnviarMensajeDto dto, int usuarioId)
        {
            var conversacion = await _conversacionRepo.GetById(dto.ConversacionId);
            if (conversacion == null)
                throw new Exception($"Conversación {dto.ConversacionId} no encontrada");

            // Guardar mensaje en DB — MediaUrl viene del dto (URL pública de R2)
            var mensaje = new Mensaje
            {
                ConversacionId = dto.ConversacionId,
                Contenido = dto.Contenido,
                MediaUrl = dto.MediaUrl,
                TipoMensaje = dto.TipoMensaje,
                Direccion = "outbound",
                UsuarioId = usuarioId,
                FechaEnvio = DateTime.Now,
                Usercr = usuarioId.ToString(),
                Fechacr = DateTime.Now,
                Active = true
            };

            await _mensajeRepo.Add(mensaje);
            await _mensajeRepo.Save();

            // Actualizar resumen de la conversación
            conversacion.UltimoMensaje = dto.TipoMensaje == "image" ? "📷 Imagen" : dto.Contenido;
            conversacion.FechaUltimoMensaje = mensaje.FechaEnvio;
            conversacion.Userup = usuarioId.ToString();
            conversacion.Fechaup = DateTime.Now;
            await _conversacionRepo.Save();

            // Enviar via WhatsApp
            var telefono = conversacion.Contacto.Telefono;

            if (dto.TipoMensaje == "text" && dto.Contenido != null)
            {
                await _whatsAppService.EnviarMensajeTexto(telefono, dto.Contenido);
            }
            else if (dto.TipoMensaje == "image" && dto.MediaId != null)
            {
                await _whatsAppService.EnviarImagen(telefono, dto.MediaId, dto.Caption);
            }

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

            // Notificar via SignalR
            await _hubContext.Clients.All.SendAsync("NuevoMensaje", mensajeDto);

            var conversacionDto = new ConversacionDto
            {
                Id = conversacion.Id,
                ContactoId = conversacion.ContactoId,
                NombreContacto = conversacion.Contacto.Nombre,
                ApellidoContacto = conversacion.Contacto.Apellido,
                Telefono = conversacion.Contacto.Telefono,
                Estado = conversacion.Estado,
                UltimoMensaje = conversacion.UltimoMensaje,
                FechaUltimoMensaje = conversacion.FechaUltimoMensaje,
                CantidadNoLeidos = conversacion.CantidadNoLeidos,
                Canal = conversacion.Canal.Nombre,
                EsNuevo = conversacion.Contacto.EsNuevo
            };

            await _hubContext.Clients.All.SendAsync("ConversacionActualizada", conversacionDto);

            return mensajeDto;
        }

        public async Task MarcarComoLeida(int conversacionId)
        {
            var conversacion = await _conversacionRepo.GetById(conversacionId);
            if (conversacion == null) return;

            conversacion.CantidadNoLeidos = 0;
            conversacion.Userup = "system";
            conversacion.Fechaup = DateTime.Now;
            await _conversacionRepo.Save();
        }
    }
}