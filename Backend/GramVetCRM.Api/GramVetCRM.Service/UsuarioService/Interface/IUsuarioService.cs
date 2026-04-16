using GramVetCRM.Model;

namespace GramVetCRM.Service
{
    public interface IUsuarioService
    {
        Task<List<Usuario>> GetAll();
    }
}