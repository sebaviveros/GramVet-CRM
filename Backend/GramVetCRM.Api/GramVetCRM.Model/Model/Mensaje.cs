namespace GramVetCRM.Model
{
    public class Mensaje
    {
        public int Id { get; set; }
        public int ConversacionId { get; set; }
        public string? Contenido { get; set; }
        public string? TipoMensaje { get; set; } // texto, imagen, audio
        public string Direccion { get; set; }    // inbound / outbound
        public string? ExternalId { get; set; }
        public int? UsuarioId { get; set; }
        public string? MediaUrl { get; set; }
        public string? Reaccion { get; set; } // emoji de reacción al mensaje
        public DateTime FechaEnvio { get; set; }
        public string Usercr { get; set; }
        public string? Userup { get; set; }
        public DateTime Fechacr { get; set; }
        public DateTime? Fechaup { get; set; }
        public bool Active { get; set; }

        // Navegación
        public Conversacion Conversacion { get; set; }
    }
}