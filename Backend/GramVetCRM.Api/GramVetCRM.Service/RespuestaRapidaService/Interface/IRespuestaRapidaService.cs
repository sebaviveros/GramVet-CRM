using GramVetCRM.Model.DTOs.RespuestaRapida;

namespace GramVetCRM.Service
{
    public interface IRespuestaRapidaService
    {
        Task<List<RespuestaRapidaDto>> GetAll(int? veterinarioId);
        Task<RespuestaRapidaDto> Crear(CrearRespuestaRapidaDto dto, int usuarioId);
        Task<RespuestaRapidaDto> Editar(int id, CrearRespuestaRapidaDto dto, int usuarioId);
        Task Eliminar(int id, int usuarioId);
    }
}
