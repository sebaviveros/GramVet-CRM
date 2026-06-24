using GramVetCRM.Model.DTOs.Usuario;

namespace GramVetCRM.Service
{
    public interface IUsuarioService
    {
        Task<List<UsuarioDto>> GetAll();
        Task<List<UsuarioDto>> GetVeterinarios();
        Task<UsuarioDto> Crear(CrearUsuarioDto dto, int actorUsuarioId);
        Task<UsuarioDto> Editar(int id, EditarUsuarioDto dto, int actorUsuarioId);
        Task Eliminar(int id, int actorUsuarioId);
        Task ResetPassword(int id, int actorUsuarioId);
        Task<bool> CambiarPassword(int usuarioId, CambiarPasswordDto dto);
    }
}
