using GramVetCRM.Model;

namespace GramVetCRM.Repository.Repositories
{
    public interface IEtiquetaRepository
    {
        Task<List<Etiqueta>> GetAll();
        Task<List<Etiqueta>> GetAllForVeterinario(int usuarioId);
        Task<Etiqueta?> GetById(int id);
        Task Add(Etiqueta etiqueta);
        Task Delete(Etiqueta etiqueta);
        Task Save();

        // Visibilidad por veterinario (EtiquetaUsuario)
        Task<List<EtiquetaUsuario>> GetAllVeterinarioLinks();
        Task SetVeterinarios(int etiquetaId, List<int> usuarioIds, int actorUsuarioId);

        // ContactoEtiqueta
        Task<List<ContactoEtiqueta>> GetByContacto(int contactoId);
        Task<List<ContactoEtiqueta>> GetByContactos(List<int> contactoIds);
        Task<ContactoEtiqueta?> GetContactoEtiqueta(int contactoId, int etiquetaId);
        Task AddContactoEtiqueta(ContactoEtiqueta ce);
        Task RemoveContactoEtiqueta(ContactoEtiqueta ce);
    }
}