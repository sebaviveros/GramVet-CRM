using GramVetCRM.Model.DTOs.Mascota;

namespace GramVetCRM.Service
{
    public interface IMascotaBitacoraService
    {
        Task<List<BitacoraEntradaDto>> GetByMascota(int mascotaId);
        Task<BitacoraEntradaDto> Crear(CrearBitacoraDto dto, int usuarioId);
        // El texto se edita aparte de las imágenes: no van necesariamente ligados.
        Task<BitacoraEntradaDto> Editar(int id, EditarBitacoraDto dto, int usuarioId);
        Task Eliminar(int id, int usuarioId);
    }
}
