using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GramVetCRM.Service;
using GramVetCRM.Model.DTOs.RespuestaRapida;
using System.Security.Claims;

namespace GramVetCRM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RespuestaRapidaController : ControllerBase
    {
        private readonly IRespuestaRapidaService _service;

        public RespuestaRapidaController(IRespuestaRapidaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearRespuestaRapidaDto dto)
        {
            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized();
            var result = await _service.Crear(dto, usuarioId.Value);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] CrearRespuestaRapidaDto dto)
        {
            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized();
            var result = await _service.Editar(id, dto, usuarioId.Value);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized();
            await _service.Eliminar(id, usuarioId.Value);
            return Ok();
        }

        private int? GetUsuarioId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return claim != null ? int.Parse(claim) : null;
        }
    }
}
