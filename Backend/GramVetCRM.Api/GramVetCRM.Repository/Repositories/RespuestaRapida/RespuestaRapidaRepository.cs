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

        public async Task<List<RespuestaRapida>> GetAllForVeterinario(int usuarioId)
        {
            return await _context.RespuestaRapida
                .Where(r => r.Active && _context.RespuestaRapidaUsuario
                    .Any(ru => ru.RespuestaRapidaId == r.Id && ru.UsuarioId == usuarioId && ru.Active))
                .OrderBy(r => r.Comando)
                .ToListAsync();
        }

        public async Task<RespuestaRapida?> GetById(int id)
        {
            return await _context.RespuestaRapida
                .FirstOrDefaultAsync(r => r.Id == id && r.Active);
        }

        // Visibilidad por veterinario (RespuestaRapidaUsuario)

        public async Task<List<RespuestaRapidaUsuario>> GetAllVeterinarioLinks()
        {
            return await _context.RespuestaRapidaUsuario
                .Where(ru => ru.Active)
                .ToListAsync();
        }

        public async Task SetVeterinarios(int respuestaRapidaId, List<int> usuarioIds, int actorUsuarioId)
        {
            var nuevos = usuarioIds.Distinct().ToHashSet();
            var existentes = await _context.RespuestaRapidaUsuario
                .Where(ru => ru.RespuestaRapidaId == respuestaRapidaId && ru.Active)
                .ToListAsync();
            var existentesIds = existentes.Select(ru => ru.UsuarioId).ToHashSet();

            foreach (var link in existentes.Where(ru => !nuevos.Contains(ru.UsuarioId)))
            {
                link.Active = false;
                link.Userup = actorUsuarioId.ToString();
                link.Fechaup = DateTime.Now;
            }

            foreach (var uid in nuevos.Where(id => !existentesIds.Contains(id)))
            {
                await _context.RespuestaRapidaUsuario.AddAsync(new RespuestaRapidaUsuario
                {
                    RespuestaRapidaId = respuestaRapidaId,
                    UsuarioId = uid,
                    Usercr = actorUsuarioId.ToString(),
                    Fechacr = DateTime.Now,
                    Active = true
                });
            }
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
