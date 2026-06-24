using GramVetCRM.Model.DTOs.Conversacion;
using GramVetCRM.Model.DTOs.Mensaje;

namespace GramVetCRM.Service
{
    public interface IConversacionService
    {
        Task<List<ConversacionDto>> GetAll(int? filtroUsuarioAsignadoId = null);
        Task<ConversacionDto> AsignarUsuario(int conversacionId, int? usuarioAsignadoId, int actorUsuarioId);
        Task<List<MensajeDto>> GetMensajes(int conversacionId, int page, int pageSize);
        Task<MensajeDto> EnviarMensaje(EnviarMensajeDto dto, int usuarioId);
        Task ReaccionarMensaje(int mensajeId, string emoji, int usuarioId);
        Task MarcarComoLeida(int conversacionId);
    }
}