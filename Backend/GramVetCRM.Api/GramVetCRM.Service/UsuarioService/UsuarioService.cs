using GramVetCRM.Model;
using GramVetCRM.Repository.Repositories;

namespace GramVetCRM.Service
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repository;

        public UsuarioService(IUsuarioRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Usuario>> GetAll()
        {
            return await _repository.GetAll();
        }
    }
}