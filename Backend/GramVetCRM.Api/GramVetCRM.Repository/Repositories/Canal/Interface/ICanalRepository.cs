using GramVetCRM.Model;

namespace GramVetCRM.Repository.Repositories
{
    public interface ICanalRepository
    {
        Task<Canal> GetOrCreate(string nombre);
    }
}
