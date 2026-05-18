using GramVetCRM.Model;
using GramVetCRM.Model.DTOs.Conversacion;
using GramVetCRM.Model.DTOs.Mensaje;
using GramVetCRM.Repository.Repositories;

namespace GramVetCRM.Service
{
    public class ConversacionService : IConversacionService
    {
        private readonly IConversacionRepository _conversacionRepo;
        private readonly IMensajeRepository _mensajeRepo;

        public ConversacionService(
            IConversacionRepository conversacionRepo,
            IMensajeRepository mensajeRepo)
        {
            _conversacionRepo = conversacionRepo;
            _mensajeRepo = mensajeRepo;
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
                TipoMensaje = m.TipoMensaje,
                Direccion = m.Direccion,
                FechaEnvio = m.FechaEnvio,
                UsuarioId = m.UsuarioId
            }).ToList();
        }

        public async Task<MensajeDto> EnviarMensaje(EnviarMensajeDto dto, int usuarioId)
        {
            var mensaje = new Mensaje
            {
                ConversacionId = dto.ConversacionId,
                Contenido = dto.Contenido,
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

            return new MensajeDto
            {
                Id = mensaje.Id,
                ConversacionId = mensaje.ConversacionId,
                Contenido = mensaje.Contenido,
                TipoMensaje = mensaje.TipoMensaje,
                Direccion = mensaje.Direccion,
                FechaEnvio = mensaje.FechaEnvio,
                UsuarioId = mensaje.UsuarioId
            };
        }
    }
}