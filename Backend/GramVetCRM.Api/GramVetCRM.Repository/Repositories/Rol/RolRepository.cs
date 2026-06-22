using GramVetCRM.Model;
using GramVetCRM.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace GramVetCRM.Repository.Repositories
{
    public class RolRepository : IRolRepository
    {
        private readonly GramVetDbContext _context;

        public RolRepository(GramVetDbContext context)
        {
            _context = context;
        }

        public async Task<List<Rol>> GetAll()
        {
            return await _context.Rol
                .Where(r => r.Active)
                .OrderBy(r => r.Id)
                .ToListAsync();
        }
    }
}
