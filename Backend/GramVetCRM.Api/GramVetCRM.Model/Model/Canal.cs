namespace GramVetCRM.Model
{
    public class Canal
    {
        public int Id { get; set; }
        public string Nombre { get; set; } // WhatsApp, Instagram, Messenger
        public string Usercr { get; set; }
        public string? Userup { get; set; }
        public DateTime Fechacr { get; set; }
        public DateTime? Fechaup { get; set; }
        public bool Active { get; set; }
    }
}