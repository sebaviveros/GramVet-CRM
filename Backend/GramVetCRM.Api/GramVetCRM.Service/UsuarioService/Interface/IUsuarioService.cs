using GramVetCRM.Model.DTOs.Usuario;

namespace GramVetCRM.Service
{
    public interface IUsuarioService
    {
        Task<List<UsuarioDto>> GetAll();
        Task<List<UsuarioDto>> GetVeterinarios();
        Task<UsuarioDto> Crear(CrearUsuarioDto dto);
        Task<UsuarioDto> Editar(int id, EditarUsuarioDto dto);
        Task Eliminar(int id);
        Task ResetPassword(int id);
        Task<bool> CambiarPassword(int usuarioId, CambiarPasswordDto dto);
    }
}
