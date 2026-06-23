using GramVetCRM.Model;
using GramVetCRM.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace GramVetCRM.Repository.Repositories
{
    public class CanalRepository : ICanalRepository
    {
        private readonly GramVetDbContext _context;

        public CanalRepository(GramVetDbContext context)
        {
            _context = context;
        }

        // Devuelve el canal por nombre; si no existe, lo crea (siembra IG/Messenger automáticamente)
        public async Task<Canal> GetOrCreate(string nombre)
        {
            var canal = await _context.Canal.FirstOrDefaultAsync(c => c.Nombre == nombre && c.Active);
            if (canal != null) return canal;

            canal = new Canal
            {
                Nombre = nombre,
                Usercr = "system",
                Fechacr = DateTime.Now,
                Active = true
            };
            await _context.Canal.AddAsync(canal);
            await _context.SaveChangesAsync();
            return canal;
        }
    }
}
