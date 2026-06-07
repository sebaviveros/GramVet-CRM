using GramVetCRM.Model.DTOs.Conversacion;
using GramVetCRM.Model.DTOs.Mensaje;

namespace GramVetCRM.Service
{
    public interface IConversacionService
    {
        Task<List<ConversacionDto>> GetAll();
        Task<List<MensajeDto>> GetMensajes(int conversacionId, int page, int pageSize);
        Task<MensajeDto> EnviarMensaje(EnviarMensajeDto dto, int usuarioId);
        Task MarcarComoLeida(int conversacionId);
    }
}