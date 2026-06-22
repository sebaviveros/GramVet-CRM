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

        public async Task<List<RespuestaRapidaDto>> GetAll()
        {
            var lista = await _repo.GetAll();
            return lista.Select(ToDto).ToList();
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

            return ToDto(respuesta);
        }

        public async Task<RespuestaRapidaDto> Editar(int id, CrearRespuestaRapidaDto dto, int usuarioId)
        {
            var respuesta = await _repo.GetById(id)
                ?? throw new Exception($"RespuestaRapida {id} no encontrada");

            respuesta.Comando = dto.Comando.TrimStart('/').ToLower();
            respuesta.Texto = dto.Texto;
            respuesta.Userup = usuarioId.ToString();
            respuesta.Fechaup = DateTime.Now;

            await _repo.Save();
            return ToDto(respuesta);
        }

        public async Task Eliminar(int id, int usuarioId)
        {
            var respuesta = await _repo.GetById(id)
                ?? throw new Exception($"RespuestaRapida {id} no encontrada");

            respuesta.Userup = usuarioId.ToString();
            await _repo.Delete(respuesta);
            await _repo.Save();
        }

        private static RespuestaRapidaDto ToDto(RespuestaRapida r) => new RespuestaRapidaDto
        {
            Id = r.Id,
            Comando = r.Comando,
            Texto = r.Texto
        };
    }
}
