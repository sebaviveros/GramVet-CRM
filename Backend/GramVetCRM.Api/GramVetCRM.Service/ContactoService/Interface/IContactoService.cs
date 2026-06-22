using GramVetCRM.Model.DTOs.Contacto;

namespace GramVetCRM.Service
{
    public interface IContactoService
    {
        Task<ContactoDto?> GetById(int id);
        Task<ContactoDto> Editar(int id, EditarContactoDto dto, int usuarioId);
    }
}
