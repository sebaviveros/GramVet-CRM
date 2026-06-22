using GramVetCRM.Model;

namespace GramVetCRM.Repository.Repositories
{
    public interface IMascotaRepository
    {
        Task<List<Mascota>> GetByContacto(int contactoId);
        Task<Mascota?> GetById(int id);
        Task Add(Mascota mascota);
        Task Delete(Mascota mascota);
        Task Save();
    }
}
