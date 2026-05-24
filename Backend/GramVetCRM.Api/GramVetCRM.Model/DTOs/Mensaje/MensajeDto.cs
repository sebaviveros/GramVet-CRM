namespace GramVetCRM.Model.DTOs.Mensaje
{
    public class MensajeDto
    {
        public int Id { get; set; }
        public int ConversacionId { get; set; }
        public string? Contenido { get; set; }
        public string? TipoMensaje { get; set; }
        public string Direccion { get; set; }
        public string? MediaUrl { get; set; }
        public DateTime FechaEnvio { get; set; }
        public int? UsuarioId { get; set; }
    }
}