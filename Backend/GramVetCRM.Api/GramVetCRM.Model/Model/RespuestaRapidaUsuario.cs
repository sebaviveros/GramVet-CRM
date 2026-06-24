namespace GramVetCRM.Model
{
    public class RespuestaRapidaUsuario
    {
        public int Id { get; set; }
        public int RespuestaRapidaId { get; set; }
        public int UsuarioId { get; set; }
        public string Usercr { get; set; }
        public string? Userup { get; set; }
        public DateTime Fechacr { get; set; }
        public DateTime? Fechaup { get; set; }
        public bool Active { get; set; }

        // Navegación
        public RespuestaRapida RespuestaRapida { get; set; }
        public Usuario Usuario { get; set; }
    }
}
