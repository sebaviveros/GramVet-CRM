using GramVetCRM.Model;

namespace GramVetCRM.Repository.Repositories
{
    public interface IContactoRepository
    {
        Task<Contacto?> GetByTelefono(string telefono);
        Task<Contacto?> GetById(int id);
        Task Add(Contacto contacto);
        Task Save();
    }
}
