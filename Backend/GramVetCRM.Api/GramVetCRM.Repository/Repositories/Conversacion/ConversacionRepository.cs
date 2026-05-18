using GramVetCRM.Model;
using GramVetCRM.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace GramVetCRM.Repository.Repositories
{
    public class ConversacionRepository : IConversacionRepository
    {
        private readonly GramVetDbContext _context;

        public ConversacionRepository(GramVetDbContext context)
        {
            _context = context;
        }

        public async Task<List<Conversacion>> GetAll()
        {
            return await _context.Conversacion
                .Include(c => c.Contacto)
                .Include(c => c.Canal)
                .Where(c => c.Active)
                .OrderByDescending(c => c.FechaUltimoMensaje)
                .ToListAsync();
        }

        public async Task<Conversacion?> GetById(int id)
        {
            return await _context.Conversacion
                .Include(c => c.Contacto)
                .Include(c => c.Canal)
                .FirstOrDefaultAsync(c => c.Id == id && c.Active);
        }

        public async Task<Conversacion?> GetByTelefono(string telefono)
        {
            return await _context.Conversacion
                .Include(c => c.Contacto)
                .Include(c => c.Canal)
                .Where(c => c.Active && c.Estado == "Abierta")
                .FirstOrDefaultAsync(c => c.Contacto.Telefono == telefono);
        }

        public async Task Add(Conversacion conversacion)
        {
            await _context.Conversacion.AddAsync(conversacion);
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Conversacion?> GetUltimaByContacto(int contactoId)
        {
            return await _context.Conversacion
                .Include(c => c.Contacto)
                .Include(c => c.Canal)
                .Where(c => c.ContactoId == contactoId && c.Active)
                .OrderByDescending(c => c.Fechacr)
                .FirstOrDefaultAsync();
        }
    }
}