using GramVetCRM.Model;
using GramVetCRM.Model.DTOs.RespuestaRapida;
using GramVetCRM.Repository.Repositories;

namespace GramVetCRM.Service
{
    public class RespuestaRapidaService : IRespuestaRapidaService
    {
        private readonly IRespuestaRapidaRepository _repo;

        public RespuestaRapidaService(IRespuestaRapidaRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<RespuestaRapidaDto>> GetAll(int? veterinarioId)
        {
            // Veterinario: solo las respuestas que el admin le habilitó.
            // Admin / Secretario (veterinarioId == null): todas.
            var lista = veterinarioId.HasValue
                ? await _repo.GetAllForVeterinario(veterinarioId.Value)
                : await _repo.GetAll();

            var links = await _repo.GetAllVeterinarioLinks();
            var porRespuesta = links
                .GroupBy(l => l.RespuestaRapidaId)
                .ToDictionary(g => g.Key, g => g.Select(l => l.UsuarioId).ToList());

            return lista.Select(r => ToDto(r,
                porRespuesta.TryGetValue(r.Id, out var ids) ? ids : new List<int>())).ToList();
        }

        public async Task<RespuestaRapidaDto> Crear(CrearRespuestaRapidaDto dto, int usuarioId)
        {
            var respuesta = new RespuestaRapida
            {
                Comando = dto.Comando.TrimStart('/').ToLower(),
                Texto = dto.Texto,
                Usercr = usuarioId.ToString(),
                Fechacr = DateTime.Now,
                Active = true
            };

            await _repo.Add(respuesta);
            await _repo.Save();

            await _repo.SetVeterinarios(respuesta.Id, dto.VeterinarioIds, usuarioId);
            await _repo.Save();

            return ToDto(respuesta, dto.VeterinarioIds);
        }

        public async Task<RespuestaRapidaDto> Editar(int id, CrearRespuestaRapidaDto dto, int usuarioId)
        {
            var respuesta = await _repo.GetById(id)
                ?? throw new Exception($"RespuestaRapida {id} no encontrada");

            respuesta.Comando = dto.Comando.TrimStart('/').ToLower();
            respuesta.Texto = dto.Texto;
            respuesta.Userup = usuarioId.ToString();
            respuesta.Fechaup = DateTime.Now;

            await _repo.SetVeterinarios(id, dto.VeterinarioIds, usuarioId);
            await _repo.Save();
            return ToDto(respuesta, dto.VeterinarioIds);
        }

        public async Task Eliminar(int id, int usuarioId)
        {
            var respuesta = await _repo.GetById(id)
                ?? throw new Exception($"RespuestaRapida {id} no encontrada");

            respuesta.Userup = usuarioId.ToString();
            await _repo.Delete(respuesta);
            await _repo.Save();
        }

        private static RespuestaRapidaDto ToDto(RespuestaRapida r, List<int> veterinarioIds) => new RespuestaRapidaDto
        {
            Id = r.Id,
            Comando = r.Comando,
            Texto = r.Texto,
            VeterinarioIds = veterinarioIds
        };
    }
}
