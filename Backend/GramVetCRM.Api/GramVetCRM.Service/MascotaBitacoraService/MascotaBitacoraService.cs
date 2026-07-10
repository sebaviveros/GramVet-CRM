using GramVetCRM.Model;
using GramVetCRM.Model.DTOs.Mascota;
using GramVetCRM.Repository.Repositories;

namespace GramVetCRM.Service
{
    public class MascotaBitacoraService : IMascotaBitacoraService
    {
        private readonly IMascotaBitacoraRepository _repo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IMascotaFotoRepository _fotoRepo;

        public MascotaBitacoraService(
            IMascotaBitacoraRepository repo,
            IUsuarioRepository usuarioRepo,
            IMascotaFotoRepository fotoRepo)
        {
            _repo = repo;
            _usuarioRepo = usuarioRepo;
            _fotoRepo = fotoRepo;
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

            // Las fotos se cargan en LOTE: una consulta por anotación sería N+1
            var fotos = await _fotoRepo.GetByBitacoras(entradas.Select(e => e.Id).ToList());
            var fotosPorEntrada = fotos
                .GroupBy(f => f.BitacoraId)
                .ToDictionary(g => g.Key, g => g.Select(ToFotoDto).ToList());

            return entradas.Select(e =>
            {
                var dto = ToDto(e, NombreAutor(nombres, e.Usercr));
                if (fotosPorEntrada.TryGetValue(e.Id, out var lista)) dto.Fotos = lista;
                return dto;
            }).ToList();
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

        public async Task<BitacoraEntradaDto> Editar(int id, EditarBitacoraDto dto, int usuarioId)
        {
            var entrada = await _repo.GetById(id)
                ?? throw new Exception($"Entrada de bitácora {id} no encontrada");

            var fotos = await _fotoRepo.GetByBitacora(id);

            // Una anotación sin texto Y sin imágenes no tiene razón de existir:
            // para eso está el botón de eliminar.
            if (string.IsNullOrWhiteSpace(dto.Contenido) && fotos.Count == 0)
                throw new InvalidOperationException(
                    "La anotación no puede quedar vacía: escribe un texto o deja al menos una imagen.");

            entrada.Contenido = string.IsNullOrWhiteSpace(dto.Contenido) ? null : dto.Contenido.Trim();
            entrada.Userup = usuarioId.ToString();
            entrada.Fechaup = DateTime.Now;
            await _repo.Save();

            // El autor sigue siendo quien la creó, no quien la editó
            var autor = await ResolverAutor(entrada.Usercr);
            var resultado = ToDto(entrada, autor);
            resultado.Fotos = fotos.Select(ToFotoDto).ToList();
            return resultado;
        }

        private async Task<string?> ResolverAutor(string? usercr)
        {
            if (usercr == null || !int.TryParse(usercr, out var id)) return null;
            var u = await _usuarioRepo.GetById(id);
            return u != null ? $"{u.Nombre} {u.Apellido}".Trim() : null;
        }

        public async Task Eliminar(int id, int usuarioId)
        {
            var entrada = await _repo.GetById(id)
                ?? throw new Exception($"Entrada de bitácora {id} no encontrada");

            // Sus imágenes se van con ella: si no, quedarían filas apuntando a una
            // anotación borrada. (Los objetos en R2 quedan huérfanos, como en el
            // resto del proyecto.)
            foreach (var foto in await _fotoRepo.GetByBitacora(id))
            {
                foto.Userup = usuarioId.ToString();
                await _fotoRepo.Delete(foto);
            }
            await _fotoRepo.Save();

            entrada.Userup = usuarioId.ToString();
            await _repo.Delete(entrada);
            await _repo.Save();
        }

        private static MascotaFotoDto ToFotoDto(MascotaFoto f) => new MascotaFotoDto
        {
            Id = f.Id,
            BitacoraId = f.BitacoraId,
            Url = f.Url,
            Descripcion = f.Descripcion
        };

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
