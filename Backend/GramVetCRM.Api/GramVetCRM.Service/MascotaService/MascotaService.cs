using GramVetCRM.Model;
using GramVetCRM.Model.DTOs.Mascota;
using GramVetCRM.Repository.Repositories;

namespace GramVetCRM.Service
{
    public class MascotaService : IMascotaService
    {
        private readonly IMascotaRepository _repo;

        public MascotaService(IMascotaRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<MascotaDto>> GetByContacto(int contactoId)
        {
            var lista = await _repo.GetByContacto(contactoId);
            return lista.Select(ToDto).ToList();
        }

        public async Task<MascotaDto> Crear(CrearMascotaDto dto, int usuarioId)
        {
            var mascota = new Mascota
            {
                ContactoId = dto.ContactoId,
                Nombre = dto.Nombre,
                Especie = dto.Especie,
                Raza = dto.Raza,
                FechaNacimiento = dto.FechaNacimiento,
                Usercr = usuarioId.ToString(),
                Fechacr = DateTime.Now,
                Active = true
            };

            await _repo.Add(mascota);
            await _repo.Save();
            return ToDto(mascota);
        }

        public async Task<MascotaDto> Editar(int id, EditarMascotaDto dto, int usuarioId)
        {
            var mascota = await _repo.GetById(id)
                ?? throw new Exception($"Mascota {id} no encontrada");

            mascota.Nombre = dto.Nombre;
            mascota.Especie = dto.Especie;
            mascota.Raza = dto.Raza;
            mascota.FechaNacimiento = dto.FechaNacimiento;
            mascota.Userup = usuarioId.ToString();
            mascota.Fechaup = DateTime.Now;

            await _repo.Save();
            return ToDto(mascota);
        }

        public async Task Eliminar(int id, int usuarioId)
        {
            var mascota = await _repo.GetById(id)
                ?? throw new Exception($"Mascota {id} no encontrada");

            mascota.Userup = usuarioId.ToString();
            await _repo.Delete(mascota);
            await _repo.Save();
        }

        private static MascotaDto ToDto(Mascota m) => new MascotaDto
        {
            Id = m.Id,
            ContactoId = m.ContactoId,
            Nombre = m.Nombre,
            Especie = m.Especie,
            Raza = m.Raza,
            FechaNacimiento = m.FechaNacimiento
        };
    }
}
