using GramVetCRM.Model.DTOs.Mascota;

namespace GramVetCRM.Service
{
    public interface IMascotaFotoService
    {
        // Las fotos cuelgan de una anotación de bitácora, no de la mascota.
        Task<List<MascotaFotoDto>> GetByBitacora(int bitacoraId);
        Task<MascotaFotoDto?> Subir(int bitacoraId, Stream archivo, string nombreOriginal, string contentType, string? descripcion, int usuarioId);
        Task Eliminar(int id, int usuarioId);
    }
}
