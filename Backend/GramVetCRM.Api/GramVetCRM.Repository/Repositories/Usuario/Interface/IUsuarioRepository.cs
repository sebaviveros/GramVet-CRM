using GramVetCRM.Model;

namespace GramVetCRM.Repository.Repositories
{
    public interface IUsuarioRepository
    {
        Task<List<Usuario>> GetAll();
        Task<List<Usuario>> GetByRolNombre(string rolNombre);
        Task<Usuario?> GetByUsername(string username);
        Task<Usuario?> GetById(int id);
        Task<Usuario?> GetByEmail(string email);
        Task Add(Usuario usuario);
        Task Delete(Usuario usuario);
        Task Save();
    }
}
