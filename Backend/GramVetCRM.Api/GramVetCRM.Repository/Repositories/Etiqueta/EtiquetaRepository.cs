using GramVetCRM.Model;
using GramVetCRM.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace GramVetCRM.Repository.Repositories
{
    public class EtiquetaRepository : IEtiquetaRepository
    {
        private readonly GramVetDbContext _context;

        public EtiquetaRepository(GramVetDbContext context)
        {
            _context = context;
        }

        public async Task<List<Etiqueta>> GetAll()
        {
            return await _context.Etiqueta
                .Where(e => e.Active)
                .OrderBy(e => e.Nombre)
                .ToListAsync();
        }

        public async Task<Etiqueta?> GetById(int id)
        {
            return await _context.Etiqueta
                .FirstOrDefaultAsync(e => e.Id == id && e.Active);
        }

        public async Task Add(Etiqueta etiqueta)
        {
            await _context.Etiqueta.AddAsync(etiqueta);
        }

        public async Task Delete(Etiqueta etiqueta)
        {
            // Soft delete
            etiqueta.Active = false;
            etiqueta.Fechaup = DateTime.Now;
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }

        // ContactoEtiqueta 

        public async Task<List<ContactoEtiqueta>> GetByContacto(int contactoId)
        {
            return await _context.ContactoEtiqueta
                .Include(ce => ce.Etiqueta)
                .Where(ce => ce.ContactoId == contactoId && ce.Active && ce.Etiqueta.Active)
                .ToListAsync();
        }

        public async Task<List<ContactoEtiqueta>> GetByContactos(List<int> contactoIds)
        {
            return await _context.ContactoEtiqueta
                .Include(ce => ce.Etiqueta)
                .Where(ce => contactoIds.Contains(ce.ContactoId) && ce.Active && ce.Etiqueta.Active)
                .ToListAsync();
        }

        public async Task<ContactoEtiqueta?> GetContactoEtiqueta(int contactoId, int etiquetaId)
        {
            return await _context.ContactoEtiqueta
                .FirstOrDefaultAsync(ce =>
                    ce.ContactoId == contactoId &&
                    ce.EtiquetaId == etiquetaId &&
                    ce.Active);
        }

        public async Task AddContactoEtiqueta(ContactoEtiqueta ce)
        {
            await _context.ContactoEtiqueta.AddAsync(ce);
        }

        public async Task RemoveContactoEtiqueta(ContactoEtiqueta ce)
        {
            ce.Active = false;
            ce.Fechaup = DateTime.Now;
        }
    }
}