using GramVetCRM.Model;
using GramVetCRM.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace GramVetCRM.Repository.Repositories
{
    public class MascotaBitacoraRepository : IMascotaBitacoraRepository
    {
        private readonly GramVetDbContext _context;

        public MascotaBitacoraRepository(GramVetDbContext context)
        {
            _context = context;
        }

        public async Task<List<MascotaBitacora>> GetByMascota(int mascotaId)
        {
            return await _context.MascotaBitacora
                .Where(b => b.MascotaId == mascotaId && b.Active)
                .OrderByDescending(b => b.Fechacr)
                .ToListAsync();
        }

        public async Task<MascotaBitacora?> GetById(int id)
        {
            return await _context.MascotaBitacora
                .FirstOrDefaultAsync(b => b.Id == id && b.Active);
        }

        public async Task Add(MascotaBitacora entrada)
        {
            await _context.MascotaBitacora.AddAsync(entrada);
        }

        public async Task Delete(MascotaBitacora entrada)
        {
            entrada.Active = false;
            entrada.Fechaup = DateTime.Now;
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}
