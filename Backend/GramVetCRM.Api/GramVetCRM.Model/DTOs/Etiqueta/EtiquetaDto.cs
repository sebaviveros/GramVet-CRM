namespace GramVetCRM.Model.DTOs.Etiqueta
{
    public class EtiquetaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string? Color { get; set; }
        public string? Descripcion { get; set; }
    }

    public class CrearEtiquetaDto
    {
        public string Nombre { get; set; }
        public string? Color { get; set; }
        public string? Descripcion { get; set; }
    }

    public class AsignarEtiquetaDto
    {
        public int ContactoId { get; set; }
        public int EtiquetaId { get; set; }
    }
}