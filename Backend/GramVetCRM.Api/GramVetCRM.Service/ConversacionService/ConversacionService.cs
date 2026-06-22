using GramVetCRM.Model;
using GramVetCRM.Model.DTOs.Conversacion;
using GramVetCRM.Model.DTOs.Mensaje;
using GramVetCRM.Model.DTOs.Etiqueta;
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
        private readonly IEtiquetaRepository _etiquetaRepo;

        public ConversacionService(
            IConversacionRepository conversacionRepo,
            IMensajeRepository mensajeRepo,
            IWhatsAppService whatsAppService,
            IHubContext<ChatHub> hubContext,
            IEtiquetaRepository etiquetaRepo)
        {
            _conversacionRepo = conversacionRepo;
            _mensajeRepo = mensajeRepo;
            _whatsAppService = whatsAppService;
            _hubContext = hubContext;
            _etiquetaRepo = etiquetaRepo;
        }

        public async Task<List<ConversacionDto>> GetAll(int? filtroUsuarioAsignadoId = null)
        {
            var conversaciones = await _conversacionRepo.GetAll(filtroUsuarioAsignadoId);
            var dtos = conversaciones.Select(MapToDto).ToList();

            // Cargar etiquetas de todos los contactos en lote (para el filtro por etiqueta)
            var contactoIds = conversaciones.Select(c => c.ContactoId).Distinct().ToList();
            if (contactoIds.Count > 0)
            {
                var ces = await _etiquetaRepo.GetByContactos(contactoIds);
                var porContacto = ces
                    .GroupBy(ce => ce.ContactoId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(ce => new EtiquetaDto
                        {
                            Id = ce.Etiqueta.Id,
                            Nombre = ce.Etiqueta.Nombre,
                            Color = ce.Etiqueta.Color,
                            Descripcion = ce.Etiqueta.Descripcion
                        }).ToList());

                foreach (var dto in dtos)
                    if (porContacto.TryGetValue(dto.ContactoId, out var lista))
                        dto.Etiquetas = lista;
            }

            return dtos;
        }

        public async Task<ConversacionDto> AsignarUsuario(int conversacionId, int? usuarioAsignadoId, int actorUsuarioId)
        {
            var conversacion = await _conversacionRepo.GetById(conversacionId)
                ?? throw new Exception($"Conversación {conversacionId} no encontrada");

            // Guardar el veterinario anterior antes de reasignar
            var asignadoAnterior = conversacion.UsuarioAsignadoId;

            conversacion.UsuarioAsignadoId = usuarioAsignadoId;
            conversacion.Userup = actorUsuarioId.ToString();
            conversacion.Fechaup = DateTime.Now;
            await _conversacionRepo.Save();

            // Recargar con navegación para devolver el nombre del asignado
            var actualizada = await _conversacionRepo.GetById(conversacionId) ?? conversacion;
            var dto = MapToDto(actualizada);

            await _hubContext.Clients.Groups(GruposDestino(actualizada.UsuarioAsignadoId))
                .SendAsync("ConversacionActualizada", dto);

            // Si había un veterinario anterior distinto del nuevo, avisarle que
            // la conversación ya no es suya para que la quite de su lista
            if (asignadoAnterior.HasValue && asignadoAnterior != usuarioAsignadoId)
            {
                await _hubContext.Clients.Group(ChatGroups.Usuario(asignadoAnterior.Value))
                    .SendAsync("ConversacionDesasignada", conversacionId);
            }

            return dto;
        }

        // Grupos a los que se debe emitir un evento de una conversación:
        // siempre staff (admin/secretario) + el veterinario asignado (si lo hay)
        private static List<string> GruposDestino(int? usuarioAsignadoId)
        {
            var grupos = new List<string> { ChatGroups.Staff };
            if (usuarioAsignadoId.HasValue)
                grupos.Add(ChatGroups.Usuario(usuarioAsignadoId.Value));
            return grupos;
        }

        private static ConversacionDto MapToDto(Conversacion c) => new ConversacionDto
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
            UsuarioAsignado = c.UsuarioAsignado != null
                ? $"{c.UsuarioAsignado.Nombre} {c.UsuarioAsignado.Apellido}".Trim()
                : null,
            UsuarioAsignadoId = c.UsuarioAsignadoId,
            Canal = c.Canal.Nombre,
            EsNuevo = c.Contacto.EsNuevo
        };

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

            // Notificar via SignalR (dirigido por rol: staff + veterinario asignado)
            var grupos = GruposDestino(conversacion.UsuarioAsignadoId);
            await _hubContext.Clients.Groups(grupos).SendAsync("NuevoMensaje", mensajeDto);

            var conversacionDto = MapToDto(conversacion);

            await _hubContext.Clients.Groups(grupos).SendAsync("ConversacionActualizada", conversacionDto);

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