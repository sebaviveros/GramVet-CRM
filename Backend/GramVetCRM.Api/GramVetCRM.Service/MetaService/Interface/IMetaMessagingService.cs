using System.Text.Json;

namespace GramVetCRM.Service
{
    public interface IMetaMessagingService
    {
        // Procesa un webhook entrante de Messenger o Instagram
        Task ProcesarMensaje(JsonElement body);

        // Envía un mensaje de texto a un usuario de Messenger/Instagram (recipient = PSID/IGSID)
        Task<bool> EnviarMensaje(string destinatarioId, string mensaje);
    }
}
