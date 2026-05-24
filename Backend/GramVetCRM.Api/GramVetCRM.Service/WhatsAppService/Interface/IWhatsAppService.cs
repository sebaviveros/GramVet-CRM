using System.Text.Json;

namespace GramVetCRM.Service
{
    public interface IWhatsAppService
    {
        Task ProcesarMensaje(JsonElement body);
        Task<bool> EnviarMensajeTexto(string telefono, string mensaje);
        Task<bool> EnviarImagen(string telefono, string mediaId, string? caption);
        Task<string?> SubirMedia(Stream fileStream, string fileName, string mimeType);
    }
}