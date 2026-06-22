using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GramVetCRM.Service;
using GramVetCRM.Model.DTOs.Contacto;
using System.Security.Claims;

namespace GramVetCRM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContactoController : ControllerBase
    {
        private readonly IContactoService _service;
        private readonly IMascotaService _mascotaService;

        public ContactoController(IContactoService service, IMascotaService mascotaService)
        {
            _service = service;
            _mascotaService = mascotaService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetById(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] EditarContactoDto dto)
        {
            var usuarioId = GetUsuarioId();
            if (usuarioId == null) return Unauthorized();
            var result = await _service.Editar(id, dto, usuarioId.Value);
            return Ok(result);
        }

        [HttpGet("{id}/mascotas")]
        public async Task<IActionResult> GetMascotas(int id)
        {
            var result = await _mascotaService.GetByContacto(id);
            return Ok(result);
        }

        private int? GetUsuarioId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return claim != null ? int.Parse(claim) : null;
        }
    }
}
