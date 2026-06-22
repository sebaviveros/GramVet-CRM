namespace GramVetCRM.Model.DTOs.Contacto
{
    public class ContactoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string? Apellido { get; set; }
        public string Telefono { get; set; }
        public string? Email { get; set; }
    }

    public class EditarContactoDto
    {
        public string Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Email { get; set; }
    }
}
