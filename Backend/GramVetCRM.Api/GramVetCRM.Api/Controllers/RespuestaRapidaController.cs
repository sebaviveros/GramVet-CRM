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

        // Veterinario: solo las habilitadas para él. Admin / Secretario: todas.
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll(GetVeterinarioFiltro());
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

        // Devuelve el Id del veterinario si el rol lo es (para filtrar); null si es admin/secretario.
        private int? GetVeterinarioFiltro()
        {
            var rolNombre = User.FindFirst("rolNombre")?.Value ?? "";
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (rolNombre.ToLower().Contains("veterinario") && usuarioIdClaim != null)
                return int.Parse(usuarioIdClaim);
            return null;
        }
    }
}
