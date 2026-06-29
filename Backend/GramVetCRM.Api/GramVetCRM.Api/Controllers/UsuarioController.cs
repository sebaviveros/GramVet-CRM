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

        // GET api/Usuario/me — datos del usuario logueado (incluye foto)
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized();
            var result = await _service.GetById(usuarioId.Value);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // POST api/Usuario/foto — el usuario logueado cambia su propia foto de perfil
        [HttpPost("foto")]
        public async Task<IActionResult> SubirFoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Archivo inválido");

            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized();

            using var stream = file.OpenReadStream();
            var url = await _service.ActualizarFoto(usuarioId.Value, stream, file.FileName, file.ContentType);
            if (url == null) return StatusCode(500, "Error subiendo la foto");
            return Ok(new { fotoUrl = url });
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
