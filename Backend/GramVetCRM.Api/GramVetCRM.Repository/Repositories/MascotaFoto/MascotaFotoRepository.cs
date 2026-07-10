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

        public async Task<List<MascotaFoto>> GetByBitacora(int bitacoraId)
        {
            return await _context.MascotaFoto
                .Where(f => f.BitacoraId == bitacoraId && f.Active)
                .OrderBy(f => f.Fechacr)
                .ToListAsync();
        }

        // En lote: sin esto habría una consulta por anotación al abrir la bitácora
        public async Task<List<MascotaFoto>> GetByBitacoras(List<int> bitacoraIds)
        {
            if (bitacoraIds.Count == 0) return new List<MascotaFoto>();

            return await _context.MascotaFoto
                .Where(f => bitacoraIds.Contains(f.BitacoraId) && f.Active)
                .OrderBy(f => f.Fechacr)
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
