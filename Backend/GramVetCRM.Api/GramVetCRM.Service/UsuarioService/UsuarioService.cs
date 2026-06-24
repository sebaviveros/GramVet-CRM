using GramVetCRM.Model;
using GramVetCRM.Model.DTOs.Usuario;
using GramVetCRM.Repository.Repositories;
using GramVetCRM.Service.Helpers;

namespace GramVetCRM.Service
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repository;
        private readonly IEmailService _emailService;

        public UsuarioService(IUsuarioRepository repository, IEmailService emailService)
        {
            _repository = repository;
            _emailService = emailService;
        }

        public async Task<List<UsuarioDto>> GetAll()
        {
            var usuarios = await _repository.GetAll();
            return usuarios.Select(ToDto).ToList();
        }

        public async Task<List<UsuarioDto>> GetVeterinarios()
        {
            var usuarios = await _repository.GetByRolNombre("Veterinario");
            return usuarios.Select(ToDto).ToList();
        }

        public async Task<UsuarioDto> Crear(CrearUsuarioDto dto, int actorUsuarioId)
        {
            // Validar unicidad
            var existeUsername = await _repository.GetByUsername(dto.Username);
            if (existeUsername != null)
                throw new Exception($"El nombre de usuario '{dto.Username}' ya está en uso");

            var existeEmail = await _repository.GetByEmail(dto.Email);
            if (existeEmail != null)
                throw new Exception($"El correo '{dto.Email}' ya está en uso");

            // Generar contraseña aleatoria
            var passwordPlano = RandomPasswordHelper.Generar();

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Email = dto.Email,
                Username = dto.Username,
                RolId = dto.RolId,
                PasswordHash = PasswordHelper.Hash(passwordPlano),
                Usercr = actorUsuarioId.ToString(),
                Fechacr = DateTime.Now,
                Active = true
            };

            await _repository.Add(usuario);
            await _repository.Save();

            // Recargar con rol para devolver nombre del rol
            var creado = await _repository.GetById(usuario.Id) ?? usuario;

            await EnviarCorreoCredenciales(usuario.Email, usuario.Nombre, usuario.Username, passwordPlano, esNuevo: true);

            return ToDto(creado);
        }

        public async Task<UsuarioDto> Editar(int id, EditarUsuarioDto dto, int actorUsuarioId)
        {
            var usuario = await _repository.GetById(id)
                ?? throw new Exception($"Usuario {id} no encontrado");

            // Validar que el email no pertenezca a OTRO usuario
            var existeEmail = await _repository.GetByEmail(dto.Email);
            if (existeEmail != null && existeEmail.Id != id)
                throw new Exception($"El correo '{dto.Email}' ya está en uso por otro usuario");

            usuario.Nombre = dto.Nombre;
            usuario.Apellido = dto.Apellido;
            usuario.Email = dto.Email;
            usuario.RolId = dto.RolId;
            usuario.Userup = actorUsuarioId.ToString();
            usuario.Fechaup = DateTime.Now;

            await _repository.Save();

            var actualizado = await _repository.GetById(id) ?? usuario;
            return ToDto(actualizado);
        }

        public async Task Eliminar(int id, int actorUsuarioId)
        {
            var usuario = await _repository.GetById(id)
                ?? throw new Exception($"Usuario {id} no encontrado");

            usuario.Userup = actorUsuarioId.ToString();
            usuario.Fechaup = DateTime.Now;
            await _repository.Delete(usuario);
            await _repository.Save();
        }

        public async Task ResetPassword(int id, int actorUsuarioId)
        {
            var usuario = await _repository.GetById(id)
                ?? throw new Exception($"Usuario {id} no encontrado");

            var passwordPlano = RandomPasswordHelper.Generar();
            usuario.PasswordHash = PasswordHelper.Hash(passwordPlano);
            usuario.Userup = actorUsuarioId.ToString();
            usuario.Fechaup = DateTime.Now;

            await _repository.Save();

            await EnviarCorreoCredenciales(usuario.Email, usuario.Nombre, usuario.Username, passwordPlano, esNuevo: false);
        }

        public async Task<bool> CambiarPassword(int usuarioId, CambiarPasswordDto dto)
        {
            var usuario = await _repository.GetById(usuarioId)
                ?? throw new Exception($"Usuario {usuarioId} no encontrado");

            if (!PasswordHelper.Verify(dto.PasswordActual, usuario.PasswordHash))
                return false;

            usuario.PasswordHash = PasswordHelper.Hash(dto.PasswordNueva);
            usuario.Userup = usuarioId.ToString();
            usuario.Fechaup = DateTime.Now;
            await _repository.Save();
            return true;
        }

        private async Task EnviarCorreoCredenciales(string email, string nombre, string username, string password, bool esNuevo)
        {
            var titulo = esNuevo ? "Bienvenido a GramVet CRM" : "Restablecimiento de contraseña - GramVet CRM";
            var intro = esNuevo
                ? "Se ha creado tu cuenta en GramVet CRM. Estas son tus credenciales de acceso:"
                : "Tu contraseña ha sido restablecida. Estas son tus nuevas credenciales de acceso:";

            var cuerpo = $@"
                <div style='font-family: Arial, sans-serif; color: #1a2e25;'>
                    <h2 style='color: #235347;'>{titulo}</h2>
                    <p>Hola {nombre},</p>
                    <p>{intro}</p>
                    <div style='background: #f4f7f5; padding: 16px; border-radius: 8px; margin: 16px 0;'>
                        <p style='margin: 4px 0;'><strong>Usuario:</strong> {username}</p>
                        <p style='margin: 4px 0;'><strong>Contraseña:</strong> {password}</p>
                    </div>
                    <p>Te recomendamos cambiar tu contraseña después de iniciar sesión.</p>
                    <p style='color: #6c8a7a; font-size: 12px;'>GramVet CRM</p>
                </div>";

            await _emailService.EnviarAsync(email, titulo, cuerpo);
        }

        private static UsuarioDto ToDto(Usuario u) => new UsuarioDto
        {
            Id = u.Id,
            Nombre = u.Nombre,
            Apellido = u.Apellido,
            Email = u.Email,
            Username = u.Username,
            RolId = u.RolId,
            RolNombre = u.Rol?.Nombre
        };
    }
}
