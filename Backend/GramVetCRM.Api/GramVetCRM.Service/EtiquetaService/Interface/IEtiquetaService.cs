using GramVetCRM.Model.DTOs.Etiqueta;

namespace GramVetCRM.Service
{
    public interface IEtiquetaService
    {
        Task<List<EtiquetaDto>> GetAll(int? veterinarioId);
        Task<EtiquetaDto> Crear(CrearEtiquetaDto dto, int usuarioId);
        Task<EtiquetaDto> Editar(int id, CrearEtiquetaDto dto, int usuarioId);
        Task Eliminar(int id, int usuarioId);

        Task<List<EtiquetaDto>> GetByContacto(int contactoId);
        Task AsignarEtiqueta(AsignarEtiquetaDto dto, int usuarioId);
        Task QuitarEtiqueta(int contactoId, int etiquetaId);
    }
}