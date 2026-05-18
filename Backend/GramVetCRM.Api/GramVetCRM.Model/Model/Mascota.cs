namespace GramVetCRM.Model
{
    public class Mascota
    {
        public int Id { get; set; }
        public int ContactoId { get; set; }
        public string Nombre { get; set; }
        public string? Especie { get; set; }
        public string? Raza { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Usercr { get; set; }
        public string? Userup { get; set; }
        public DateTime Fechacr { get; set; }
        public DateTime? Fechaup { get; set; }
        public bool Active { get; set; }

        // Navegación
        public Contacto Contacto { get; set; }
    }
}