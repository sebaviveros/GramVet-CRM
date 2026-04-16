using GramVetCRM.Model;

namespace GramVetCRM.Repository.Repositories
{
    public interface IUsuarioRepository
    {
        Task<List<Usuario>> GetAll();
        Task<Usuario?> GetByUsername(string username);
        Task Add(Usuario usuario);
        Task Save();
    }
}