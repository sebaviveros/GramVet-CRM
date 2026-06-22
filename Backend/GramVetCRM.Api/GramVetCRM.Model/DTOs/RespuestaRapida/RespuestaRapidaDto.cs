namespace GramVetCRM.Model.DTOs.RespuestaRapida
{
    public class RespuestaRapidaDto
    {
        public int Id { get; set; }
        public string Comando { get; set; }
        public string Texto { get; set; }
    }

    public class CrearRespuestaRapidaDto
    {
        public string Comando { get; set; }
        public string Texto { get; set; }
    }
}
