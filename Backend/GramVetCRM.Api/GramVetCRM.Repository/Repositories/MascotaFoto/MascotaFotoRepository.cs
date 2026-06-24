using GramVetCRM.Model;
using GramVetCRM.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace GramVetCRM.Repository.Repositories
{
    public class MascotaFotoRepository : IMascotaFotoRepository
    {
        private readonly GramVetDbContext _context;

        public MascotaFotoRepository(GramVetDbContext context)
        {
            _context = context;
        }

        public async Task<List<MascotaFoto>> GetByMascota(int mascotaId)
        {
            return await _context.MascotaFoto
                .Where(f => f.MascotaId == mascotaId && f.Active)
                .OrderByDescending(f => f.Fechacr)
                .ToListAsync();
        }

        public async Task<MascotaFoto?> GetById(int id)
        {
            return await _context.MascotaFoto
                .FirstOrDefaultAsync(f => f.Id == id && f.Active);
        }

        public async Task Add(MascotaFoto foto)
        {
            await _context.MascotaFoto.AddAsync(foto);
        }

        public async Task Delete(MascotaFoto foto)
        {
            foto.Active = false;
            foto.Fechaup = DateTime.Now;
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}
