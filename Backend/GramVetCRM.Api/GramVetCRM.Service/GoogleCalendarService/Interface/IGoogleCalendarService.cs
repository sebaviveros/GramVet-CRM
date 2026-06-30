using GramVetCRM.Model.DTOs.Cita;

namespace GramVetCRM.Service
{
    public interface IGoogleCalendarService
    {
        // Crea un evento en el calendario madre. Si no hay credenciales, devuelve un resultado simulado.
        Task<CitaCreadaDto> CrearEvento(string titulo, string descripcion, string? ubicacion, DateTime inicio, DateTime fin);
    }
}
