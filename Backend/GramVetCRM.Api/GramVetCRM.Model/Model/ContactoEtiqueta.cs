namespace GramVetCRM.Model
{
    public class ContactoEtiqueta
    {
        public int Id { get; set; }
        public int ContactoId { get; set; }
        public int EtiquetaId { get; set; }
        public string Usercr { get; set; }
        public string? Userup { get; set; }
        public DateTime Fechacr { get; set; }
        public DateTime? Fechaup { get; set; }
        public bool Active { get; set; }

        // Navegación
        public Contacto Contacto { get; set; }
        public Etiqueta Etiqueta { get; set; }
    }
}