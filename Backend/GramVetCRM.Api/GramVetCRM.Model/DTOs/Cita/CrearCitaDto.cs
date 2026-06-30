namespace GramVetCRM.Model.DTOs.Cita
{
    // Request que envía el secretario al confirmar la cita (Fase 2).
    public class CrearCitaDto
    {
        public DateTime Fecha { get; set; }              // día elegido para la cita
        public int Movil { get; set; }                   // 1 | 2 (define el posicionamiento)
        public int SlotIndex { get; set; }               // índice en la lista fija de horarios

        public string TituloEvento { get; set; } = "";   // ya editado por el secretario
        public string DescripcionEvento { get; set; } = "";
        public string? Ubicacion { get; set; }           // dirección o GPS para el location del evento

        // Mascotas a crear y asociar al cliente
        public List<MascotaCitaDto> Mascotas { get; set; } = new();
    }

    // Resultado de crear la cita.
    public class CitaCreadaDto
    {
        public string EventoId { get; set; } = "";
        public string? EventoLink { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        public int MascotasCreadas { get; set; }
        public bool Simulada { get; set; }               // true si el calendar no está configurado
    }
}
