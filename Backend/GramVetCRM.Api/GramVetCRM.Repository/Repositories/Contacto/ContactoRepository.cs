using GramVetCRM.Model;
using GramVetCRM.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace GramVetCRM.Repository.Repositories
{
    public class ContactoRepository : IContactoRepository
    {
        private readonly GramVetDbContext _context;

        public ContactoRepository(GramVetDbContext context)
        {
            _context = context;
        }

        public async Task<Contacto?> GetByTelefono(string telefono)
        {
            return await _context.Contacto
                .FirstOrDefaultAsync(c => c.Telefono == telefono && c.Active);
        }

        public async Task<Contacto?> GetById(int id)
        {
            return await _context.Contacto
                .FirstOrDefaultAsync(c => c.Id == id && c.Active);
        }

        public async Task Add(Contacto contacto)
        {
            await _context.Contacto.AddAsync(contacto);
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}
