namespace GramVetCRM.Model.DTOs.Mascota
{
    public class MascotaDto
    {
        public int Id { get; set; }
        public int ContactoId { get; set; }
        public string Nombre { get; set; }
        public string? Especie { get; set; }
        public string? Raza { get; set; }
        public DateTime? FechaNacimiento { get; set; }
    }

    public class CrearMascotaDto
    {
        public int ContactoId { get; set; }
        public string Nombre { get; set; }
        public string? Especie { get; set; }
        public string? Raza { get; set; }
        public DateTime? FechaNacimiento { get; set; }
    }

    public class EditarMascotaDto
    {
        public string Nombre { get; set; }
        public string? Especie { get; set; }
        public string? Raza { get; set; }
        public DateTime? FechaNacimiento { get; set; }
    }
}
