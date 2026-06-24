namespace GramVetCRM.Model.DTOs.Mensaje
{
    public class EnviarMensajeDto
    {
        public int ConversacionId { get; set; }
        public string? Contenido { get; set; }
        public string TipoMensaje { get; set; } = "text";
        public string? MediaId { get; set; }
        public string? MediaUrl { get; set; }
        public string? Caption { get; set; }

        // Ubicación (TipoMensaje == "location")
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
        public string? NombreUbicacion { get; set; }
    }

    public class ReaccionDto
    {
        public string Emoji { get; set; } = "";
    }
}