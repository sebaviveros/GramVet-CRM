using GramVetCRM.Model;
using GramVetCRM.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace GramVetCRM.Repository.Repositories
{
    public class RespuestaRapidaRepository : IRespuestaRapidaRepository
    {
        private readonly GramVetDbContext _context;

        public RespuestaRapidaRepository(GramVetDbContext context)
        {
            _context = context;
        }

        public async Task<List<RespuestaRapida>> GetAll()
        {
            return await _context.RespuestaRapida
                .Where(r => r.Active)
                .OrderBy(r => r.Comando)
                .ToListAsync();
        }

        public async Task<RespuestaRapida?> GetById(int id)
        {
            return await _context.RespuestaRapida
                .FirstOrDefaultAsync(r => r.Id == id && r.Active);
        }

        public async Task Add(RespuestaRapida respuesta)
        {
            await _context.RespuestaRapida.AddAsync(respuesta);
        }

        public async Task Delete(RespuestaRapida respuesta)
        {
            respuesta.Active = false;
            respuesta.Fechaup = DateTime.Now;
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}
