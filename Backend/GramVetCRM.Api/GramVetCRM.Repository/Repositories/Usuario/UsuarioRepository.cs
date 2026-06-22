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
            return await _context.Usuario
                .Include(u => u.Rol)
                .Where(u => u.Active)
                .OrderBy(u => u.Nombre)
                .ToListAsync();
        }

        public async Task<List<Usuario>> GetByRolNombre(string rolNombre)
        {
            return await _context.Usuario
                .Include(u => u.Rol)
                .Where(u => u.Active && u.Rol != null && u.Rol.Nombre.ToLower() == rolNombre.ToLower())
                .OrderBy(u => u.Nombre)
                .ToListAsync();
        }

        public async Task<Usuario?> GetByUsername(string username)
        {
            return await _context.Usuario
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(x => x.Username == username && x.Active);
        }

        public async Task<Usuario?> GetById(int id)
        {
            return await _context.Usuario
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(x => x.Id == id && x.Active);
        }

        public async Task<Usuario?> GetByEmail(string email)
        {
            return await _context.Usuario
                .FirstOrDefaultAsync(x => x.Email == email && x.Active);
        }

        public async Task Add(Usuario usuario)
        {
            await _context.Usuario.AddAsync(usuario);
        }

        public async Task Delete(Usuario usuario)
        {
            usuario.Active = false;
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}
