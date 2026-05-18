namespace GramVetCRM.Model
{
    public class Contacto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string? Apellido { get; set; }
        public string Telefono { get; set; }
        public string? Email { get; set; }
        public bool EsNuevo { get; set; } = false; // true cuando lo crea el webhook
        public string Usercr { get; set; }
        public string? Userup { get; set; }
        public DateTime Fechacr { get; set; }
        public DateTime? Fechaup { get; set; }
        public bool Active { get; set; }

        // Navegación
        public ICollection<Mascota> Mascotas { get; set; }
    }
}