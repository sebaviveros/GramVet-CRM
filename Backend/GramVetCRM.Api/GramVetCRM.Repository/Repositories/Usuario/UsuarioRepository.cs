using GramVetCRM.Model;
using GramVetCRM.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace GramVetCRM.Repository.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly GramVetDbContext _context;

        public UsuarioRepository(GramVetDbContext context)
        {
            _context = context;
        }

        public async Task<List<Usuario>> GetAll()
        {
            return await _context.Usuario.ToListAsync();
        }

        public async Task<Usuario?> GetByUsername(string username)
        {
            return await _context.Usuario
                .FirstOrDefaultAsync(x => x.Username == username && x.Active);
        }

        public async Task Add(Usuario usuario)
        {
            await _context.Usuario.AddAsync(usuario);
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}