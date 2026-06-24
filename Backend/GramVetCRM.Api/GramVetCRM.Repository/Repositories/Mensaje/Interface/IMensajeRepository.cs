using GramVetCRM.Model;

namespace GramVetCRM.Repository.Repositories
{
    public interface IMensajeRepository
    {
        Task<List<Mensaje>> GetByConversacion(int conversacionId, int page, int pageSize);
        Task<Mensaje?> GetById(int id);
        Task<Mensaje?> GetByExternalId(string externalId);
        Task Add(Mensaje mensaje);
        Task Save();
    }
}