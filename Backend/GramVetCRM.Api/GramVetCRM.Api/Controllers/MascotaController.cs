using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GramVetCRM.Service;
using GramVetCRM.Model.DTOs.Mascota;
using System.Security.Claims;

namespace GramVetCRM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MascotaController : ControllerBase
    {
        private readonly IMascotaService _service;

        public MascotaController(IMascotaService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearMascotaDto dto)
        {
            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized();
            var result = await _service.Crear(dto, usuarioId.Value);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] EditarMascotaDto dto)
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
