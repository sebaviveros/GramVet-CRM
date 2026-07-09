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
        private readonly ITurnstileService _turnstile;

        public AuthController(IAuthService service, ITurnstileService turnstile)
        {
            _service = service;
            _turnstile = turnstile;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            // El captcha se valida ANTES de mirar las credenciales: si no, un bot
            // podría seguir probando contraseñas y el captcha no serviría de nada.
            var captchaOk = await _turnstile.Validar(
                request.CaptchaToken,
                HttpContext.Connection.RemoteIpAddress?.ToString());

            if (!captchaOk)
                return BadRequest("Verificación de seguridad fallida. Recargue la página e intente de nuevo.");

            var token = await _service.Login(request);

            if (token == null)
                return Unauthorized("Usuario o contraseña incorrectos");

            return Ok(new { token });
        }
    }
}