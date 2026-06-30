namespace GramVetCRM.Model.DTOs.Cita
{
    // Resultado de la extracción IA de una cita a partir de la conversación.
    // Los campos mapean al formato exacto del evento de calendario de la veterinaria.
    public class CitaExtraidaDto
    {
        // Datos del cliente / domicilio
        public string? NombreCliente { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? ComunaSector { get; set; }
        public string? ReferenciasDireccion { get; set; }
        public string? Correo { get; set; }

        // Datos de la cita
        public string? Pacientes { get; set; }          // ej. "1 gato"
        public string? ClienteSolicito { get; set; }
        public string? Cobros { get; set; }             // ej. "$19.000 triple / $6.000 ida"
        public string? TotalMinimo { get; set; }        // ej. "$25.000"
        public string? Observaciones { get; set; }
        public string? FechaHoraSugerida { get; set; }  // texto libre, ej. "viernes 1-3 pm"

        // Datos extra que pide el secretario
        public string? UbicacionGps { get; set; }            // link/coords de Google Maps
        public bool SeguroMascota { get; set; }              // ¿la atención involucra seguro?
        public string? SeguroNota { get; set; }
        public bool EstacionamientoVisita { get; set; }      // condominio con estacionamiento de visita
        public string? EstacionamientoNota { get; set; }

        // Mascotas detectadas en la conversación (se crean al confirmar)
        public List<MascotaCitaDto> Mascotas { get; set; } = new();

        // Slot sugerido por la IA (índice en la lista fija de horarios), si lo pudo inferir
        public int? SlotSugerido { get; set; }

        // Texto final del evento (armado en el formato del cliente)
        public string TituloEvento { get; set; } = "";
        public string DescripcionEvento { get; set; } = "";

        // true cuando se usó el fallback simulado (sin API key de Anthropic)
        public bool Simulada { get; set; }
    }

    // Mascota detectada/ingresada al agendar (edad opcional)
    public class MascotaCitaDto
    {
        public string Nombre { get; set; } = "";
        public string? Especie { get; set; }            // "perro" | "gato" | otro
        public string? FechaNacimiento { get; set; }    // opcional (no siempre se sabe la edad)
    }
}
