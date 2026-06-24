namespace GramVetCRM.Model.DTOs.Mascota
{
    public class MascotaFotoDto
    {
        public int Id { get; set; }
        public int MascotaId { get; set; }
        public string Url { get; set; }
        public string? Descripcion { get; set; }
    }
}
