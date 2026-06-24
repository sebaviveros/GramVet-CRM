using GramVetCRM.Model;

namespace GramVetCRM.Repository.Repositories
{
    public interface IRespuestaRapidaRepository
    {
        Task<List<RespuestaRapida>> GetAll();
        Task<List<RespuestaRapida>> GetAllForVeterinario(int usuarioId);
        Task<RespuestaRapida?> GetById(int id);
        Task Add(RespuestaRapida respuesta);
        Task Delete(RespuestaRapida respuesta);
        Task Save();

        // Visibilidad por veterinario (RespuestaRapidaUsuario)
        Task<List<RespuestaRapidaUsuario>> GetAllVeterinarioLinks();
        Task SetVeterinarios(int respuestaRapidaId, List<int> usuarioIds, int actorUsuarioId);
    }
}
