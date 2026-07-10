using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GramVetCRM.Service;
using GramVetCRM.Model.DTOs.Mensaje;
using GramVetCRM.Model.DTOs.Conversacion;
using System.Security.Claims;

namespace GramVetCRM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConversacionController : ControllerBase
    {
        private readonly IConversacionService _service;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IR2StorageService _r2Storage;
        private readonly IAgendaIaService _agendaIaService;

        public ConversacionController(
            IConversacionService service,
            IWhatsAppService whatsAppService,
            IR2StorageService r2Storage,
            IAgendaIaService agendaIaService)
        {
            _service = service;
            _whatsAppService = whatsAppService;
            _r2Storage = r2Storage;
            _agendaIaService = agendaIaService;
        }

        // GET api/Conversacion
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rolNombre = User.FindFirst("rolNombre")?.Value ?? "";

            // Veterinario: solo ve las conversaciones asignadas a él.
            // Admin / Secretario: ven todas.
            int? filtro = null;
            if (rolNombre.ToLower().Contains("veterinario") && usuarioIdClaim != null)
                filtro = int.Parse(usuarioIdClaim);

            var result = await _service.GetAll(filtro);
            return Ok(result);
        }

        // PUT api/Conversacion/5/asignar  (body: { usuarioAsignadoId: int|null })
        [HttpPut("{id}/asignar")]
        public async Task<IActionResult> Asignar(int id, [FromBody] AsignarUsuarioDto dto)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rolNombre = User.FindFirst("rolNombre")?.Value ?? "";

            if (usuarioIdClaim == null) return Unauthorized();

            // Admin y Secretario asignan a cualquiera. El veterinario solo puede
            // desasignarse a sí mismo; el servicio valida ese caso puntual.
            var rolLower = rolNombre.ToLower();
            var esStaff = rolLower.Contains("admin") || rolLower.Contains("secretario");

            try
            {
                var result = await _service.AsignarUsuario(id, dto.UsuarioAsignadoId, int.Parse(usuarioIdClaim), esStaff);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // GET api/Conversacion/5/mensajes?page=1&pageSize=15
        [HttpGet("{id}/mensajes")]
        public async Task<IActionResult> GetMensajes(int id, int page = 1, int pageSize = 15)
        {
            var result = await _service.GetMensajes(id, page, pageSize);
            return Ok(result);
        }

        // POST api/Conversacion/mensaje
        [HttpPost("mensaje")]
        public async Task<IActionResult> EnviarMensaje([FromBody] EnviarMensajeDto dto)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdClaim == null) return Unauthorized();

            var usuarioId = int.Parse(usuarioIdClaim);
            var result = await _service.EnviarMensaje(dto, usuarioId);
            return Ok(result);
        }

        // POST api/Conversacion/upload-imagen
        // Sube a WhatsApp Media API (para enviar) y a Cloudflare R2 (para mostrar en el CRM)
        [HttpPost("upload-imagen")]
        public async Task<IActionResult> SubirImagen(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Archivo inválido");

            // Leer a bytes una sola vez para poder usar dos streams independientes
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            var fileName = $"image/{Guid.NewGuid()}.jpg";

            var waTask = _whatsAppService.SubirMedia(
                new MemoryStream(fileBytes), file.FileName, file.ContentType);
            var r2Task = _r2Storage.SubirArchivo(
                new MemoryStream(fileBytes), fileName, file.ContentType);

            await Task.WhenAll(waTask, r2Task);

            var mediaId = waTask.Result;
            var mediaUrl = r2Task.Result;

            if (mediaId == null)
                return StatusCode(500, "Error subiendo imagen a WhatsApp");

            return Ok(new { mediaId, mediaUrl });
        }

        // POST api/Conversacion/mensaje/5/reaccion  (body: { emoji: "❤️" })
        [HttpPost("mensaje/{id}/reaccion")]
        public async Task<IActionResult> Reaccionar(int id, [FromBody] ReaccionDto dto)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdClaim == null) return Unauthorized();

            try
            {
                await _service.ReaccionarMensaje(id, dto.Emoji, int.Parse(usuarioIdClaim));
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // PUT api/Conversacion/5/leer
        [HttpPut("{id}/leer")]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            await _service.MarcarComoLeida(id);
            return Ok();
        }

        // POST api/Conversacion/5/extraer-cita
        // Extrae con IA los datos de una cita a partir de la conversación (Fase 1: solo devuelve el borrador).
        [HttpPost("{id}/extraer-cita")]
        public async Task<IActionResult> ExtraerCita(int id)
        {
            var rolNombre = User.FindFirst("rolNombre")?.Value ?? "";
            var rolLower = rolNombre.ToLower();

            // Solo Admin y Secretario pueden usar la extracción IA (control de costo).
            if (!rolLower.Contains("admin") && !rolLower.Contains("secretario"))
                return Forbid();

            try
            {
                var result = await _agendaIaService.ExtraerCita(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // POST api/Conversacion/5/crear-cita
        // Confirma la cita: crea las mascotas del cliente y el evento en el Google Calendar madre.
        [HttpPost("{id}/crear-cita")]
        public async Task<IActionResult> CrearCita(int id, [FromBody] GramVetCRM.Model.DTOs.Cita.CrearCitaDto dto)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rolNombre = User.FindFirst("rolNombre")?.Value ?? "";
            var rolLower = rolNombre.ToLower();

            if (!rolLower.Contains("admin") && !rolLower.Contains("secretario"))
                return Forbid();
            if (usuarioIdClaim == null) return Unauthorized();

            try
            {
                var result = await _agendaIaService.CrearCita(id, dto, int.Parse(usuarioIdClaim));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}