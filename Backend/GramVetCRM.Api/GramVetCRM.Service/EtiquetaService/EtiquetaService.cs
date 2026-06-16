using GramVetCRM.Model;
using GramVetCRM.Model.DTOs.Etiqueta;
using GramVetCRM.Repository.Repositories;

namespace GramVetCRM.Service
{
    public class EtiquetaService : IEtiquetaService
    {
        private readonly IEtiquetaRepository _repo;

        public EtiquetaService(IEtiquetaRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<EtiquetaDto>> GetAll()
        {
            var etiquetas = await _repo.GetAll();
            return etiquetas.Select(ToDto).ToList();
        }

        public async Task<EtiquetaDto> Crear(CrearEtiquetaDto dto, int usuarioId)
        {
            var etiqueta = new Etiqueta
            {
                Nombre = dto.Nombre,
                Color = dto.Color ?? "#6c8a7a",
                Descripcion = dto.Descripcion,
                Usercr = usuarioId.ToString(),
                Fechacr = DateTime.Now,
                Active = true
            };

            await _repo.Add(etiqueta);
            await _repo.Save();

            return ToDto(etiqueta);
        }

        public async Task<EtiquetaDto> Editar(int id, CrearEtiquetaDto dto, int usuarioId)
        {
            var etiqueta = await _repo.GetById(id)
                ?? throw new Exception($"Etiqueta {id} no encontrada");

            etiqueta.Nombre = dto.Nombre;
            etiqueta.Color = dto.Color ?? etiqueta.Color;
            etiqueta.Descripcion = dto.Descripcion;
            etiqueta.Userup = usuarioId.ToString();
            etiqueta.Fechaup = DateTime.Now;

            await _repo.Save();
            return ToDto(etiqueta);
        }

        public async Task Eliminar(int id, int usuarioId)
        {
            var etiqueta = await _repo.GetById(id)
                ?? throw new Exception($"Etiqueta {id} no encontrada");

            etiqueta.Userup = usuarioId.ToString();
            await _repo.Delete(etiqueta);
            await _repo.Save();
        }

        public async Task<List<EtiquetaDto>> GetByContacto(int contactoId)
        {
            var lista = await _repo.GetByContacto(contactoId);
            return lista.Select(ce => ToDto(ce.Etiqueta)).ToList();
        }

        public async Task AsignarEtiqueta(AsignarEtiquetaDto dto, int usuarioId)
        {
            // Verificar que la etiqueta existe
            var etiqueta = await _repo.GetById(dto.EtiquetaId)
                ?? throw new Exception($"Etiqueta {dto.EtiquetaId} no encontrada");

            // Verificar que no está ya asignada
            var existente = await _repo.GetContactoEtiqueta(dto.ContactoId, dto.EtiquetaId);
            if (existente != null) return; // ya asignada, no hacer nada

            var ce = new ContactoEtiqueta
            {
                ContactoId = dto.ContactoId,
                EtiquetaId = dto.EtiquetaId,
                Usercr = usuarioId.ToString(),
                Fechacr = DateTime.Now,
                Active = true
            };

            await _repo.AddContactoEtiqueta(ce);
            await _repo.Save();
        }

        public async Task QuitarEtiqueta(int contactoId, int etiquetaId)
        {
            var ce = await _repo.GetContactoEtiqueta(contactoId, etiquetaId);
            if (ce == null) return;

            await _repo.RemoveContactoEtiqueta(ce);
            await _repo.Save();
        }

        private static EtiquetaDto ToDto(Etiqueta e) => new EtiquetaDto
        {
            Id = e.Id,
            Nombre = e.Nombre,
            Color = e.Color,
            Descripcion = e.Descripcion
        };
    }
}