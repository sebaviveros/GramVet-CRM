namespace GramVetCRM.Model
{
    public class RespuestaRapida
    {
        public int Id { get; set; }
        public string Comando { get; set; } // /hola, /gracias
        public string Texto { get; set; }
        public string Usercr { get; set; }
        public string? Userup { get; set; }
        public DateTime Fechacr { get; set; }
        public DateTime? Fechaup { get; set; }
        public bool Active { get; set; }
    }
}