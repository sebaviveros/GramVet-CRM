namespace GramVetCRM.Model.DTOs.Mascota
{
    public class BitacoraEntradaDto
    {
        public int Id { get; set; }
        public int MascotaId { get; set; }
        public string Contenido { get; set; }
        public DateTime Fecha { get; set; }
        public string? Autor { get; set; }
    }

    public class CrearBitacoraDto
    {
        public int MascotaId { get; set; }
        public string Contenido { get; set; }
    }
}
