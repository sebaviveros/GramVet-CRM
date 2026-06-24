using GramVetCRM.Model;
using GramVetCRM.Model.DTOs.Mascota;
using GramVetCRM.Repository.Repositories;

namespace GramVetCRM.Service
{
    public class MascotaBitacoraService : IMascotaBitacoraService
    {
        private readonly IMascotaBitacoraRepository _repo;
        private readonly IUsuarioRepository _usuarioRepo;

        public MascotaBitacoraService(IMascotaBitacoraRepository repo, IUsuarioRepository usuarioRepo)
        {
            _repo = repo;
            _usuarioRepo = usuarioRepo;
        }

        public async Task<List<BitacoraEntradaDto>> GetByMascota(int mascotaId)
        {
            var entradas = await _repo.GetByMascota(mascotaId);
            if (entradas.Count == 0) return new List<BitacoraEntradaDto>();

            // Resolver el nombre del autor (usercr guarda el Id del usuario)
            var usuarios = await _usuarioRepo.GetAll();
            var nombres = usuarios.ToDictionary(
                u => u.Id.ToString(),
                u => $"{u.Nombre} {u.Apellido}".Trim());

            return entradas.Select(e => ToDto(e, NombreAutor(nombres, e.Usercr))).ToList();
        }

        public async Task<BitacoraEntradaDto> Crear(CrearBitacoraDto dto, int usuarioId)
        {
            var entrada = new MascotaBitacora
            {
                MascotaId = dto.MascotaId,
                Contenido = dto.Contenido,
                Usercr = usuarioId.ToString(),
                Fechacr = DateTime.Now,
                Active = true
            };

            await _repo.Add(entrada);
            await _repo.Save();

            var autor = await _usuarioRepo.GetById(usuarioId);
            var nombre = autor != null ? $"{autor.Nombre} {autor.Apellido}".Trim() : null;
            return ToDto(entrada, nombre);
        }

        public async Task Eliminar(int id, int usuarioId)
        {
            var entrada = await _repo.GetById(id)
                ?? throw new Exception($"Entrada de bitácora {id} no encontrada");

            entrada.Userup = usuarioId.ToString();
            await _repo.Delete(entrada);
            await _repo.Save();
        }

        private static string? NombreAutor(Dictionary<string, string> nombres, string? usercr)
            => usercr != null && nombres.TryGetValue(usercr, out var n) ? n : null;

        private static BitacoraEntradaDto ToDto(MascotaBitacora b, string? autor) => new BitacoraEntradaDto
        {
            Id = b.Id,
            MascotaId = b.MascotaId,
            Contenido = b.Contenido,
            Fecha = b.Fechacr,
            Autor = autor
        };
    }
}
