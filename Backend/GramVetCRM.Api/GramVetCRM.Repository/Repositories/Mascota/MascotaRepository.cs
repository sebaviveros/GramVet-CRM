using GramVetCRM.Model;
using GramVetCRM.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace GramVetCRM.Repository.Repositories
{
    public class MascotaRepository : IMascotaRepository
    {
        private readonly GramVetDbContext _context;

        public MascotaRepository(GramVetDbContext context)
        {
            _context = context;
        }

        public async Task<List<Mascota>> GetByContacto(int contactoId)
        {
            return await _context.Mascota
                .Where(m => m.ContactoId == contactoId && m.Active)
                .OrderBy(m => m.Nombre)
                .ToListAsync();
        }

        public async Task<Mascota?> GetById(int id)
        {
            return await _context.Mascota
                .FirstOrDefaultAsync(m => m.Id == id && m.Active);
        }

        public async Task Add(Mascota mascota)
        {
            await _context.Mascota.AddAsync(mascota);
        }

        public async Task Delete(Mascota mascota)
        {
            mascota.Active = false;
            mascota.Fechaup = DateTime.Now;
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}
