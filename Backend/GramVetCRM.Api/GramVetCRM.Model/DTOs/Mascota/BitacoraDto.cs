namespace GramVetCRM.Model.DTOs.Mascota
{
    public class BitacoraEntradaDto
    {
        public int Id { get; set; }
        public int MascotaId { get; set; }
        public string? Contenido { get; set; }   // opcional: puede ser solo imágenes
        public DateTime Fecha { get; set; }
        public string? Autor { get; set; }

        // Imágenes adjuntas a esta anotación (recetas, boletas, etc.)
        public List<MascotaFotoDto> Fotos { get; set; } = new();
    }

    public class CrearBitacoraDto
    {
        public int MascotaId { get; set; }
        public string? Contenido { get; set; }
    }

    public class EditarBitacoraDto
    {
        public string? Contenido { get; set; }   // puede quedar vacío si hay imágenes
    }
}
