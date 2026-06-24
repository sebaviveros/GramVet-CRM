using System.Text.Json;

namespace GramVetCRM.Service
{
    public interface IWhatsAppService
    {
        Task ProcesarMensaje(JsonElement body);
        Task<string?> EnviarMensajeTexto(string telefono, string mensaje);
        Task<string?> EnviarImagen(string telefono, string mediaId, string? caption);
        Task<string?> EnviarUbicacion(string telefono, double latitud, double longitud, string? nombre);
        Task<bool> EnviarReaccion(string telefono, string messageId, string emoji);
        Task<string?> SubirMedia(Stream fileStream, string fileName, string mimeType);
    }
}