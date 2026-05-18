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

        public ConversacionController(IConversacionService service)
        {
            _service = service;
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
            // obtener el id del usuario desde el JWT
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (usuarioIdClaim == null)
                return Unauthorized();

            var usuarioId = int.Parse(usuarioIdClaim);
            var result = await _service.EnviarMensaje(dto, usuarioId);

            return Ok(result);
        }
    }
}