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
        private readonly IMetaMessagingService _metaService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IEtiquetaRepository _etiquetaRepo;

        public ConversacionService(
            IConversacionRepository conversacionRepo,
            IMensajeRepository mensajeRepo,
            IWhatsAppService whatsAppService,
            IMetaMessagingService metaService,
            IHubContext<ChatHub> hubContext,
            IEtiquetaRepository etiquetaRepo)
        {
            _conversacionRepo = conversacionRepo;
            _mensajeRepo = mensajeRepo;
            _whatsAppService = whatsAppService;
            _metaService = metaService;
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
                UsuarioId = m.UsuarioId,
                Reaccion = m.Reaccion,
                EstadoEntrega = m.EstadoEntrega
            }).ToList();
        }

        public async Task<MensajeDto> EnviarMensaje(EnviarMensajeDto dto, int usuarioId)
        {
            var conversacion = await _conversacionRepo.GetById(dto.ConversacionId);
            if (conversacion == null)
                throw new Exception($"Conversación {dto.ConversacionId} no encontrada");

            // Ubicación: arma la URL de Maps y usa el nombre como contenido (no hay cambio de BD)
            var contenido = dto.Contenido;
            var mediaUrl = dto.MediaUrl;
            if (dto.TipoMensaje == "location" && dto.Latitud.HasValue && dto.Longitud.HasValue)
            {
                mediaUrl = WhatsAppService.MapsUrl(dto.Latitud.Value, dto.Longitud.Value);
                contenido = dto.NombreUbicacion;
            }

            // Guardar mensaje en DB — MediaUrl viene del dto (URL pública de R2) o de la ubicación
            var mensaje = new Mensaje
            {
                ConversacionId = dto.ConversacionId,
                Contenido = contenido,
                MediaUrl = mediaUrl,
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
            conversacion.UltimoMensaje = WhatsAppService.ResumenMensaje(dto.TipoMensaje, contenido);
            conversacion.FechaUltimoMensaje = mensaje.FechaEnvio;
            conversacion.Userup = usuarioId.ToString();
            conversacion.Fechaup = DateTime.Now;
            await _conversacionRepo.Save();

            // Enviar por el canal correspondiente
            var telefono = conversacion.Contacto.Telefono;
            var canalNombre = (conversacion.Canal?.Nombre ?? "").ToLower();

            string? externalId = null;
            var esMeta = canalNombre.Contains("messenger") || canalNombre.Contains("instagram");

            if (esMeta)
            {
                // Messenger / Instagram: el id del destinatario va en Telefono con prefijo (FB:/IG:)
                var destinatarioId = telefono;
                var idx = destinatarioId.IndexOf(':');
                if (idx >= 0) destinatarioId = destinatarioId.Substring(idx + 1);

                if (dto.TipoMensaje == "text" && dto.Contenido != null)
                {
                    externalId = await _metaService.EnviarMensaje(destinatarioId, dto.Contenido);
                }
                else if (dto.TipoMensaje == "location" && mediaUrl != null)
                {
                    // Messenger/IG no tienen ubicación nativa al enviar → va como link de Maps
                    externalId = await _metaService.EnviarMensaje(destinatarioId, mediaUrl);
                }
                // (envío de imagen por Meta queda como mejora futura)
            }
            else
            {
                // WhatsApp (comportamiento existente, sin cambios)
                if (dto.TipoMensaje == "text" && dto.Contenido != null)
                {
                    externalId = await _whatsAppService.EnviarMensajeTexto(telefono, dto.Contenido);
                }
                else if (dto.TipoMensaje == "image" && dto.MediaId != null)
                {
                    externalId = await _whatsAppService.EnviarImagen(telefono, dto.MediaId, dto.Caption);
                }
                else if (dto.TipoMensaje == "location" && dto.Latitud.HasValue && dto.Longitud.HasValue)
                {
                    externalId = await _whatsAppService.EnviarUbicacion(telefono, dto.Latitud.Value, dto.Longitud.Value, dto.NombreUbicacion);
                }
            }

            // Guardar el id externo (wamid / message_id) para poder mapear reacciones y estados.
            // WhatsApp: arranca en "sent" (luego avanza a delivered/read por webhook).
            if (!string.IsNullOrEmpty(externalId))
            {
                mensaje.ExternalId = externalId;
                if (!esMeta) mensaje.EstadoEntrega = "sent";
                await _mensajeRepo.Save();
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
                UsuarioId = mensaje.UsuarioId,
                Reaccion = mensaje.Reaccion,
                EstadoEntrega = mensaje.EstadoEntrega
            };

            // Notificar via SignalR (dirigido por rol: staff + veterinario asignado)
            var grupos = GruposDestino(conversacion.UsuarioAsignadoId);
            await _hubContext.Clients.Groups(grupos).SendAsync("NuevoMensaje", mensajeDto);

            var conversacionDto = MapToDto(conversacion);

            await _hubContext.Clients.Groups(grupos).SendAsync("ConversacionActualizada", conversacionDto);

            return mensajeDto;
        }

        public async Task ReaccionarMensaje(int mensajeId, string emoji, int usuarioId)
        {
            var mensaje = await _mensajeRepo.GetById(mensajeId)
                ?? throw new Exception($"Mensaje {mensajeId} no encontrado");

            var conversacion = await _conversacionRepo.GetById(mensaje.ConversacionId)
                ?? throw new Exception("Conversación no encontrada");

            var canalNombre = (conversacion.Canal?.Nombre ?? "").ToLower();
            var esMeta = canalNombre.Contains("messenger") || canalNombre.Contains("instagram");

            // Solo WhatsApp permite enviar reacciones por API de páginas
            if (esMeta)
                throw new Exception("Las reacciones solo están disponibles en WhatsApp");

            if (string.IsNullOrEmpty(mensaje.ExternalId))
                throw new Exception("No se puede reaccionar a este mensaje");

            await _whatsAppService.EnviarReaccion(conversacion.Contacto.Telefono, mensaje.ExternalId, emoji);

            mensaje.Reaccion = emoji;
            mensaje.Userup = usuarioId.ToString();
            mensaje.Fechaup = DateTime.Now;
            await _mensajeRepo.Save();

            var grupos = GruposDestino(conversacion.UsuarioAsignadoId);
            await _hubContext.Clients.Groups(grupos).SendAsync("MensajeReaccionado", new
            {
                MensajeId = mensaje.Id,
                Reaccion = emoji,
                ConversacionId = mensaje.ConversacionId
            });
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