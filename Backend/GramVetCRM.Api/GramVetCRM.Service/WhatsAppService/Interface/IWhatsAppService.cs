using System.Text.Json;

namespace GramVetCRM.Service
{
    public interface IWhatsAppService
    {
        Task ProcesarMensaje(JsonElement body);
    }
}