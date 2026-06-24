using GramVetCRM.Model.DTOs.Contacto;
using GramVetCRM.Repository.Repositories;

namespace GramVetCRM.Service
{
    public class ContactoService : IContactoService
    {
        private readonly IContactoRepository _repo;

        public ContactoService(IContactoRepository repo)
        {
            _repo = repo;
        }

        public async Task<ContactoDto?> GetById(int id)
        {
            var contacto = await _repo.GetById(id);
            if (contacto == null) return null;
            return ToDto(contacto);
        }

        public async Task<ContactoDto> Editar(int id, EditarContactoDto dto, int usuarioId)
        {
            var contacto = await _repo.GetById(id)
                ?? throw new Exception($"Contacto {id} no encontrado");

            contacto.Nombre = dto.Nombre;
            contacto.Apellido = dto.Apellido;
            contacto.Email = dto.Email;
            contacto.Direccion = dto.Direccion;
            contacto.ReferenciaDireccion = dto.ReferenciaDireccion;
            contacto.Userup = usuarioId.ToString();
            contacto.Fechaup = DateTime.Now;

            await _repo.Save();
            return ToDto(contacto);
        }

        private static ContactoDto ToDto(Model.Contacto c) => new ContactoDto
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Apellido = c.Apellido,
            Telefono = c.Telefono,
            Email = c.Email,
            Direccion = c.Direccion,
            ReferenciaDireccion = c.ReferenciaDireccion
        };
    }
}
