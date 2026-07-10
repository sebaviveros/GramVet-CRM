using GramVetCRM.Model;

namespace GramVetCRM.Repository.Repositories
{
    public interface IMascotaFotoRepository
    {
        Task<List<MascotaFoto>> GetByBitacora(int bitacoraId);
        // En lote: evita N+1 al listar la bitácora de una mascota
        Task<List<MascotaFoto>> GetByBitacoras(List<int> bitacoraIds);
        Task<MascotaFoto?> GetById(int id);
        Task Add(MascotaFoto foto);
        Task Delete(MascotaFoto foto);
        Task Save();
    }
}
