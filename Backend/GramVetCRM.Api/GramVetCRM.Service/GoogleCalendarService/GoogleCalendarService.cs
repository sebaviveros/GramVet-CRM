using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using GramVetCRM.Model.DTOs.Cita;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GramVetCRM.Service
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<GoogleCalendarService> _logger;

        public GoogleCalendarService(IConfiguration config, ILogger<GoogleCalendarService> logger)
        {
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Citas para hoy o mañana: sin colorId, o sea el color por defecto del
        /// calendario (el verde que la veterinaria ya usa). De pasado mañana en
        /// adelante: "7" = Peacock, el celeste de la paleta fija de Google.
        /// </summary>
        private static string? ColorSegunCercania(DateTime inicio)
        {
            var diasHasta = (inicio.Date - DateTime.Today).Days;
            return diasHasta <= 1 ? null : "7";
        }

        public async Task<CitaCreadaDto> CrearEvento(
            string titulo, string descripcion, string? ubicacion, DateTime inicio, DateTime fin)
        {
            var section = _config.GetSection("GoogleCalendar");
            var keyPath = section["ServiceAccountKeyPath"];
            var calendarId = section["CalendarId"];
            var tz = section["TimeZone"] ?? "America/Santiago";

            // Fallback simulado: sin credenciales o sin archivo de clave, no se llama a Google.
            if (string.IsNullOrWhiteSpace(keyPath) ||
                string.IsNullOrWhiteSpace(calendarId) ||
                !File.Exists(keyPath))
            {
                _logger.LogWarning(
                    "[CALENDAR SIMULADO - GoogleCalendar no configurado]\nTítulo: {Titulo}\nInicio: {Inicio:g} Fin: {Fin:g}",
                    titulo, inicio, fin);
                return new CitaCreadaDto
                {
                    EventoId = "SIMULADO",
                    EventoLink = null,
                    Inicio = inicio,
                    Fin = fin,
                    Simulada = true
                };
            }

            GoogleCredential credential;
            using (var stream = new FileStream(keyPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(CalendarService.Scope.Calendar);
            }

            using var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "GramVet CRM"
            });

            var evento = new Event
            {
                Summary = titulo,
                Description = descripcion,
                Location = ubicacion,
                ColorId = ColorSegunCercania(inicio),
                Start = new EventDateTime
                {
                    DateTimeDateTimeOffset = new DateTimeOffset(inicio, TimeZoneInfo.Local.GetUtcOffset(inicio)),
                    TimeZone = tz
                },
                End = new EventDateTime
                {
                    DateTimeDateTimeOffset = new DateTimeOffset(fin, TimeZoneInfo.Local.GetUtcOffset(fin)),
                    TimeZone = tz
                }
            };

            var creado = await service.Events.Insert(evento, calendarId).ExecuteAsync();
            _logger.LogInformation("Evento creado en calendar {CalendarId}: {Id}", calendarId, creado.Id);

            return new CitaCreadaDto
            {
                EventoId = creado.Id,
                EventoLink = creado.HtmlLink,
                Inicio = inicio,
                Fin = fin,
                Simulada = false
            };
        }
    }
}
