using GramVetCRM.Model;
using GramVetCRM.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace GramVetCRM.Repository.Repositories
{
    public class MensajeRepository : IMensajeRepository
    {
        private readonly GramVetDbContext _context;

        public MensajeRepository(GramVetDbContext context)
        {
            _context = context;
        }

        public async Task<List<Mensaje>> GetByConversacion(int conversacionId, int page, int pageSize)
        {
            return await _context.Mensaje
                .Where(m => m.ConversacionId == conversacionId && m.Active)
                .OrderByDescending(m => m.FechaEnvio)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(m => m.FechaEnvio) // reordenar asc para mostrar en chat
                .ToListAsync();
        }

        public async Task Add(Mensaje mensaje)
        {
            await _context.Mensaje.AddAsync(mensaje);
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}