using GramVetCRM.Model.DTOs.Mascota;

namespace GramVetCRM.Service
{
    public interface IMascotaBitacoraService
    {
        Task<List<BitacoraEntradaDto>> GetByMascota(int mascotaId);
        Task<BitacoraEntradaDto> Crear(CrearBitacoraDto dto, int usuarioId);
        Task Eliminar(int id, int usuarioId);
    }
}
