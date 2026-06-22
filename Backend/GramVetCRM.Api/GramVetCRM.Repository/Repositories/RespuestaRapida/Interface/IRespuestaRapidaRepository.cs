using GramVetCRM.Model;

namespace GramVetCRM.Repository.Repositories
{
    public interface IRespuestaRapidaRepository
    {
        Task<List<RespuestaRapida>> GetAll();
        Task<RespuestaRapida?> GetById(int id);
        Task Add(RespuestaRapida respuesta);
        Task Delete(RespuestaRapida respuesta);
        Task Save();
    }
}
