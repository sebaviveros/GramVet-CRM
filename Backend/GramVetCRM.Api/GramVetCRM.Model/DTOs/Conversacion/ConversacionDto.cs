namespace GramVetCRM.Model.DTOs.Conversacion
{
    public class ConversacionDto
    {
        public int Id { get; set; }
        public int ContactoId { get; set; }
        public string NombreContacto { get; set; }
        public string? ApellidoContacto { get; set; }
        public string Telefono { get; set; }
        public string Estado { get; set; }
        public string? UltimoMensaje { get; set; }
        public DateTime? FechaUltimoMensaje { get; set; }
        public int CantidadNoLeidos { get; set; }
        public string? UsuarioAsignado { get; set; }
        public string Canal { get; set; }
        public bool EsNuevo { get; set; }
    }
}