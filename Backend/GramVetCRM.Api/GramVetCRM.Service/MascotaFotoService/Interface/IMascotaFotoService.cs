using GramVetCRM.Model.DTOs.Mascota;

namespace GramVetCRM.Service
{
    public interface IMascotaFotoService
    {
        Task<List<MascotaFotoDto>> GetByMascota(int mascotaId);
        Task<MascotaFotoDto?> Subir(int mascotaId, Stream archivo, string nombreOriginal, string contentType, string? descripcion, int usuarioId);
        Task Eliminar(int id, int usuarioId);
    }
}
