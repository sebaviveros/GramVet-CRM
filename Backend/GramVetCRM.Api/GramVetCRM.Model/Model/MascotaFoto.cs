namespace GramVetCRM.Model
{
    public class MascotaFoto
    {
        public int Id { get; set; }
        public int MascotaId { get; set; }
        public string Url { get; set; }
        public string? Descripcion { get; set; }
        public string Usercr { get; set; }
        public string? Userup { get; set; }
        public DateTime Fechacr { get; set; }
        public DateTime? Fechaup { get; set; }
        public bool Active { get; set; }

        // Navegación
        public Mascota Mascota { get; set; }
    }
}
