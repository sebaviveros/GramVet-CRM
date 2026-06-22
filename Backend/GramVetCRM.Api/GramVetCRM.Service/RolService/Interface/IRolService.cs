using GramVetCRM.Model.DTOs.Rol;

namespace GramVetCRM.Service
{
    public interface IRolService
    {
        Task<List<RolDto>> GetAll();
    }
}
