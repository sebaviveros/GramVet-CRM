using GramVetCRM.Model;

namespace GramVetCRM.Repository.Repositories
{
    public interface IRolRepository
    {
        Task<List<Rol>> GetAll();
    }
}
