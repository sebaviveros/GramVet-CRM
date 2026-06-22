using GramVetCRM.Model.DTOs.Etiqueta;

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
        public int? UsuarioAsignadoId { get; set; }
        public string Canal { get; set; }
        public bool EsNuevo { get; set; }
        public List<EtiquetaDto> Etiquetas { get; set; } = new();
    }

    public class AsignarUsuarioDto
    {
        public int? UsuarioAsignadoId { get; set; }
    }
}