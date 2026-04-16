using Microsoft.AspNetCore.Mvc;
using GramVetCRM.Model.DTOs.Auth;
using GramVetCRM.Service;

namespace GramVetCRM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var token = await _service.Login(request);

            if (token == null)
                return Unauthorized("Usuario o contraseña incorrectos");

            return Ok(new { token });
        }
    }
}