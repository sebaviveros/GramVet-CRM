using GramVetCRM.Model;

namespace GramVetCRM.Repository.Repositories
{
    public interface IMascotaBitacoraRepository
    {
        Task<List<MascotaBitacora>> GetByMascota(int mascotaId);
        Task<MascotaBitacora?> GetById(int id);
        Task Add(MascotaBitacora entrada);
        Task Delete(MascotaBitacora entrada);
        Task Save();
    }
}
