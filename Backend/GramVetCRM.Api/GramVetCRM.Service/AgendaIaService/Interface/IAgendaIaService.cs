using GramVetCRM.Model.DTOs.Cita;

namespace GramVetCRM.Service
{
    public interface IAgendaIaService
    {
        // Extrae los datos de una cita a partir de la conversación usando IA (Claude).
        // Si no hay API key configurada, devuelve un borrador simulado.
        Task<CitaExtraidaDto> ExtraerCita(int conversacionId);

        // Crea la cita confirmada: registra las mascotas nuevas del cliente y crea el
        // evento en el Google Calendar madre en la posición codificada (móvil + slot).
        Task<CitaCreadaDto> CrearCita(int conversacionId, CrearCitaDto dto, int actorUsuarioId);
    }
}
