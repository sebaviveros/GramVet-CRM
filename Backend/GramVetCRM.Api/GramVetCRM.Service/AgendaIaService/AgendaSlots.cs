namespace GramVetCRM.Service
{
    // Catálogo fijo de horarios y la lógica de posicionamiento en el Google Calendar madre.
    // La hora real va en el título; el evento se posiciona en un bloque codificado de 1 hora
    // según el móvil (1 = franja AM desde 1:00; 2 = franja PM desde 13:00).
    // Confirmado con el cliente (2026-06-29).
    public static class AgendaSlots
    {
        public record Slot(int Index, string Label);

        public static readonly IReadOnlyList<Slot> Lista = new List<Slot>
        {
            new(0, "10 - 11:30"),
            new(1, "11 - 1"),
            new(2, "12 - 2"),
            new(3, "1 - 3"),
            new(4, "2 - 4"),
            new(5, "3 - 5"),
            new(6, "4 - 6"),
            new(7, "5 - 7"),
            new(8, "6 - 7:30"),
            new(9, "6 - 7:30 (2º)")  // solo Móvil 1 y solo miércoles a sábado
        };

        // Devuelve el inicio/fin físico (bloque de 1 hora) del evento en el calendar.
        // Lanza ArgumentException si la combinación día/móvil/slot es inválida.
        public static (DateTime inicio, DateTime fin) CalcularPosicion(DateTime dia, int movil, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Lista.Count)
                throw new ArgumentException("Horario (slot) inválido.");
            if (movil != 1 && movil != 2)
                throw new ArgumentException("El móvil debe ser 1 o 2.");

            // Slot 9 (segundo 6-7:30): solo Móvil 1 y solo de miércoles a sábado.
            if (slotIndex == 9)
            {
                if (movil != 1)
                    throw new ArgumentException("El segundo bloque 6-7:30 es solo para Móvil 1.");
                var d = dia.DayOfWeek;
                var mierASab = d == DayOfWeek.Wednesday || d == DayOfWeek.Thursday
                            || d == DayOfWeek.Friday || d == DayOfWeek.Saturday;
                if (!mierASab)
                    throw new ArgumentException("El segundo bloque 6-7:30 solo aplica de miércoles a sábado.");
            }

            var horaBase = movil == 1 ? 1 : 13;            // 1:00 AM o 1:00 PM
            var inicio = dia.Date.AddHours(horaBase + slotIndex);
            var fin = inicio.AddHours(1);                  // bloque físico de 1 hora
            return (inicio, fin);
        }
    }
}
