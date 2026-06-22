using GramVetCRM.Model.DTOs.Rol;
using GramVetCRM.Repository.Repositories;

namespace GramVetCRM.Service
{
    public class RolService : IRolService
    {
        private readonly IRolRepository _repo;

        public RolService(IRolRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<RolDto>> GetAll()
        {
            var roles = await _repo.GetAll();
            return roles.Select(r => new RolDto
            {
                Id = r.Id,
                Nombre = r.Nombre,
                Descripcion = r.Descripcion
            }).ToList();
        }
    }
}
