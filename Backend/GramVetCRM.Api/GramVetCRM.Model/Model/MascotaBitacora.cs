namespace GramVetCRM.Model
{
    public class MascotaBitacora
    {
        public int Id { get; set; }
        public int MascotaId { get; set; }
        // Opcional: una anotación puede ser solo imágenes, sin texto.
        public string? Contenido { get; set; }
        public string Usercr { get; set; }
        public string? Userup { get; set; }
        public DateTime Fechacr { get; set; }
        public DateTime? Fechaup { get; set; }
        public bool Active { get; set; }

        // Navegación
        public Mascota Mascota { get; set; }
    }
}
