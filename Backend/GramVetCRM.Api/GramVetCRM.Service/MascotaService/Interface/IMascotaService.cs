using GramVetCRM.Model.DTOs.Mascota;

namespace GramVetCRM.Service
{
    public interface IMascotaService
    {
        Task<List<MascotaDto>> GetByContacto(int contactoId);
        Task<MascotaDto> Crear(CrearMascotaDto dto, int usuarioId);
        Task<MascotaDto> Editar(int id, EditarMascotaDto dto, int usuarioId);
        Task Eliminar(int id, int usuarioId);
    }
}
