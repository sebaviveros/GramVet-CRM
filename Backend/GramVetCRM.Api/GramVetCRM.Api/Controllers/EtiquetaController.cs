using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GramVetCRM.Service;
using GramVetCRM.Model.DTOs.Etiqueta;
using System.Security.Claims;

namespace GramVetCRM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EtiquetaController : ControllerBase
    {
        private readonly IEtiquetaService _service;

        public EtiquetaController(IEtiquetaService service)
        {
            _service = service;
        }

        // GET api/Etiqueta — todas las etiquetas disponibles
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }

        // POST api/Etiqueta — crear etiqueta nueva
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearEtiquetaDto dto)
        {
            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized();

            var result = await _service.Crear(dto, usuarioId.Value);
            return Ok(result);
        }

        // PUT api/Etiqueta/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] CrearEtiquetaDto dto)
        {
            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized();

            var result = await _service.Editar(id, dto, usuarioId.Value);
            return Ok(result);
        }

        // DELETE api/Etiqueta/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized();

            await _service.Eliminar(id, usuarioId.Value);
            return Ok();
        }

        // GET api/Etiqueta/contacto/3 — etiquetas asignadas a un contacto
        [HttpGet("contacto/{contactoId}")]
        public async Task<IActionResult> GetByContacto(int contactoId)
        {
            var result = await _service.GetByContacto(contactoId);
            return Ok(result);
        }

        // POST api/Etiqueta/asignar — asignar etiqueta a contacto
        [HttpPost("asignar")]
        public async Task<IActionResult> Asignar([FromBody] AsignarEtiquetaDto dto)
        {
            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized();

            await _service.AsignarEtiqueta(dto, usuarioId.Value);
            return Ok();
        }

        // DELETE api/Etiqueta/quitar/contactoId/etiquetaId
        [HttpDelete("quitar/{contactoId}/{etiquetaId}")]
        public async Task<IActionResult> Quitar(int contactoId, int etiquetaId)
        {
            await _service.QuitarEtiqueta(contactoId, etiquetaId);
            return Ok();
        }

        private int? GetUsuarioId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return claim != null ? int.Parse(claim) : null;
        }
    }
}