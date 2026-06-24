using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GramVetCRM.Service;
using GramVetCRM.Model.DTOs.Usuario;
using System.Security.Claims;

namespace GramVetCRM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _service;

        public UsuarioController(IUsuarioService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }

        [HttpGet("veterinarios")]
        public async Task<IActionResult> GetVeterinarios()
        {
            var result = await _service.GetVeterinarios();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearUsuarioDto dto)
        {
            var actorId = GetUsuarioId();
            if (actorId == null) return Unauthorized();
            try
            {
                var result = await _service.Crear(dto, actorId.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] EditarUsuarioDto dto)
        {
            var actorId = GetUsuarioId();
            if (actorId == null) return Unauthorized();
            try
            {
                var result = await _service.Editar(id, dto, actorId.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var actorId = GetUsuarioId();
            if (actorId == null) return Unauthorized();
            await _service.Eliminar(id, actorId.Value);
            return Ok();
        }

        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var actorId = GetUsuarioId();
            if (actorId == null) return Unauthorized();
            await _service.ResetPassword(id, actorId.Value);
            return Ok(new { mensaje = "Se envió una nueva contraseña al correo del usuario" });
        }

        [HttpPost("cambiar-password")]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto)
        {
            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized();

            var ok = await _service.CambiarPassword(usuarioId.Value, dto);
            if (!ok)
                return BadRequest(new { mensaje = "La contraseña actual es incorrecta" });

            return Ok(new { mensaje = "Contraseña actualizada correctamente" });
        }

        private int? GetUsuarioId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return claim != null ? int.Parse(claim) : null;
        }
    }
}
