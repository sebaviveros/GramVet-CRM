namespace GramVetCRM.Model.DTOs.Usuario
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public int RolId { get; set; }
        public string? RolNombre { get; set; }
    }

    public class CrearUsuarioDto
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public int RolId { get; set; }
    }

    public class EditarUsuarioDto
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public int RolId { get; set; }
    }

    public class CambiarPasswordDto
    {
        public string PasswordActual { get; set; }
        public string PasswordNueva { get; set; }
    }
}
