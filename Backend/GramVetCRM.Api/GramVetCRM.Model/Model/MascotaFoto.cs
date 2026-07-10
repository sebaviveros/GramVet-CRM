namespace GramVetCRM.Model
{
    // Una foto cuelga de una ANOTACIÓN de bitácora (no de la mascota):
    // una anotación puede llevar varias imágenes y ya sabe de qué mascota es.
    public class MascotaFoto
    {
        public int Id { get; set; }
        public int BitacoraId { get; set; }
        public string Url { get; set; }
        public string? Descripcion { get; set; }
        public string Usercr { get; set; }
        public string? Userup { get; set; }
        public DateTime Fechacr { get; set; }
        public DateTime? Fechaup { get; set; }
        public bool Active { get; set; }

        // Navegación
        public MascotaBitacora Bitacora { get; set; }
    }
}
