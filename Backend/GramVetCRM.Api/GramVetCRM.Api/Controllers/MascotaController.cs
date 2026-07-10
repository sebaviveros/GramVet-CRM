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
        private readonly IMascotaBitacoraService _bitacoraService;
        private readonly IMascotaFotoService _fotoService;

        public MascotaController(
            IMascotaService service,
            IMascotaBitacoraService bitacoraService,
            IMascotaFotoService fotoService)
        {
            _service = service;
            _bitacoraService = bitacoraService;
            _fotoService = fotoService;
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

        // ── Bitácora ──────────────────────────────────────────────────────

        [HttpGet("{id}/bitacora")]
        public async Task<IActionResult> GetBitacora(int id)
        {
            var result = await _bitacoraService.GetByMascota(id);
            return Ok(result);
        }

        [HttpPost("bitacora")]
        public async Task<IActionResult> CrearBitacora([FromBody] CrearBitacoraDto dto)
        {
            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized();
            var result = await _bitacoraService.Crear(dto, usuarioId.Value);
            return Ok(result);
        }

        // PUT api/Mascota/bitacora/5  (body: { contenido: string|null })
        [HttpPut("bitacora/{id}")]
        public async Task<IActionResult> EditarBitacora(int id, [FromBody] EditarBitacoraDto dto)
        {
            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized();

            try
            {
                var result = await _bitacoraService.Editar(id, dto, usuarioId.Value);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpDelete("bitacora/{id}")]
        public async Task<IActionResult> EliminarBitacora(int id)
        {
            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized();
            await _bitacoraService.Eliminar(id, usuarioId.Value);
            return Ok();
        }

        // ── Fotos (adjuntas a una anotación de bitácora) ──────────────────

        // POST api/Mascota/bitacora/5/fotos
        [HttpPost("bitacora/{bitacoraId}/fotos")]
        public async Task<IActionResult> SubirFoto(int bitacoraId, IFormFile file, [FromForm] string? descripcion)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Archivo inválido");

            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized();

            using var stream = file.OpenReadStream();
            var result = await _fotoService.Subir(
                bitacoraId, stream, file.FileName, file.ContentType, descripcion, usuarioId.Value);

            if (result == null) return StatusCode(500, "Error subiendo la foto");
            return Ok(result);
        }

        [HttpDelete("fotos/{id}")]
        public async Task<IActionResult> EliminarFoto(int id)
        {
            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized();
            await _fotoService.Eliminar(id, usuarioId.Value);
            return Ok();
        }

        private int? GetUsuarioId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return claim != null ? int.Parse(claim) : null;
        }
    }
}
