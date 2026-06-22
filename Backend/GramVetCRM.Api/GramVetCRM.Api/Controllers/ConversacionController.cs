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

        public ConversacionController(
            IConversacionService service,
            IWhatsAppService whatsAppService,
            IR2StorageService r2Storage)
        {
            _service = service;
            _whatsAppService = whatsAppService;
            _r2Storage = r2Storage;
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

            // Solo Admin y Secretario pueden asignar
            var rolLower = rolNombre.ToLower();
            if (!rolLower.Contains("admin") && !rolLower.Contains("secretario"))
                return Forbid();

            if (usuarioIdClaim == null) return Unauthorized();

            var result = await _service.AsignarUsuario(id, dto.UsuarioAsignadoId, int.Parse(usuarioIdClaim));
            return Ok(result);
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

        // PUT api/Conversacion/5/leer
        [HttpPut("{id}/leer")]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            await _service.MarcarComoLeida(id);
            return Ok();
        }
    }
}