using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GramVetCRM.Service;
using GramVetCRM.Model.DTOs.Mensaje;
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

        public ConversacionController(
            IConversacionService service,
            IWhatsAppService whatsAppService)
        {
            _service = service;
            _whatsAppService = whatsAppService;
        }

        // GET api/Conversacion
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
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
        [HttpPost("upload-imagen")]
        public async Task<IActionResult> SubirImagen(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Archivo inválido");

            // Subir a WhatsApp Media API para obtener mediaId
            using var stream = file.OpenReadStream();
            var mediaId = await _whatsAppService.SubirMedia(stream, file.FileName, file.ContentType);

            if (mediaId == null)
                return StatusCode(500, "Error subiendo imagen a WhatsApp");

            return Ok(new { mediaId });
        }
    }
}