using GramVetCRM.Model;

namespace GramVetCRM.Repository.Repositories
{
    public interface IMascotaFotoRepository
    {
        Task<List<MascotaFoto>> GetByMascota(int mascotaId);
        Task<MascotaFoto?> GetById(int id);
        Task Add(MascotaFoto foto);
        Task Delete(MascotaFoto foto);
        Task Save();
    }
}
