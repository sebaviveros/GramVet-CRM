using GramVetCRM.Model;

namespace GramVetCRM.Repository.Repositories
{
    public interface IConversacionRepository
    {
        Task<List<Conversacion>> GetAll();
        Task<Conversacion?> GetById(int id);
        Task<Conversacion?> GetByTelefono(string telefono);
        Task Add(Conversacion conversacion);
        Task Save();
        Task<Conversacion?> GetUltimaByContacto(int contactoId);
    }
}