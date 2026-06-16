using GramVetCRM.Model;

namespace GramVetCRM.Repository.Repositories
{
    public interface IEtiquetaRepository
    {
        Task<List<Etiqueta>> GetAll();
        Task<Etiqueta?> GetById(int id);
        Task Add(Etiqueta etiqueta);
        Task Delete(Etiqueta etiqueta);
        Task Save();

        // ContactoEtiqueta
        Task<List<ContactoEtiqueta>> GetByContacto(int contactoId);
        Task<ContactoEtiqueta?> GetContactoEtiqueta(int contactoId, int etiquetaId);
        Task AddContactoEtiqueta(ContactoEtiqueta ce);
        Task RemoveContactoEtiqueta(ContactoEtiqueta ce);
    }
}