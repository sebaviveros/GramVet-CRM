namespace GramVetCRM.Model
{
    public class Conversacion
    {
        public int Id { get; set; }
        public int ContactoId { get; set; }
        public int CanalId { get; set; }
        public int? UsuarioAsignadoId { get; set; }
        public string? UltimoMensaje { get; set; }
        public DateTime? FechaUltimoMensaje { get; set; }
        public int CantidadNoLeidos { get; set; } = 0;
        public string Estado { get; set; } = "Abierta"; // Abierta, Cerrada, Pendiente
        public string Usercr { get; set; }
        public string? Userup { get; set; }
        public DateTime Fechacr { get; set; }
        public DateTime? Fechaup { get; set; }
        public bool Active { get; set; }

        // Navegación
        public Contacto Contacto { get; set; }
        public Canal Canal { get; set; }
        public Usuario? UsuarioAsignado { get; set; }
    }
}