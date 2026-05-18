using GramVetCRM.Model;

namespace GramVetCRM.Repository.Repositories
{
    public interface IMensajeRepository
    {
        Task<List<Mensaje>> GetByConversacion(int conversacionId, int page, int pageSize);
        Task Add(Mensaje mensaje);
        Task Save();
    }
}