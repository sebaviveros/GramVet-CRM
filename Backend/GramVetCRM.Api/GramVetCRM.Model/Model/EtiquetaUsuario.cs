namespace GramVetCRM.Model
{
    public class EtiquetaUsuario
    {
        public int Id { get; set; }
        public int EtiquetaId { get; set; }
        public int UsuarioId { get; set; }
        public string Usercr { get; set; }
        public string? Userup { get; set; }
        public DateTime Fechacr { get; set; }
        public DateTime? Fechaup { get; set; }
        public bool Active { get; set; }

        // Navegación
        public Etiqueta Etiqueta { get; set; }
        public Usuario Usuario { get; set; }
    }
}
