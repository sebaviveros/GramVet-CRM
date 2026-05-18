using Microsoft.AspNetCore.Mvc;
using GramVetCRM.Service;
using System.Text.Json;

namespace GramVetCRM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WhatsAppController : ControllerBase
    {
        private readonly IWhatsAppService _whatsAppService;
        private readonly IConfiguration _config;

        public WhatsAppController(
            IWhatsAppService whatsAppService,
            IConfiguration config)
        {
            _whatsAppService = whatsAppService;
            _config = config;
        }

        [HttpGet("webhook")]
        public IActionResult Verificar(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.verify_token")] string token,
            [FromQuery(Name = "hub.challenge")] string challenge)
        {
            var verifyToken = _config["WhatsApp:VerifyToken"];

            if (mode == "subscribe" && token == verifyToken)
                return Ok(int.Parse(challenge));

            return Forbid();
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> RecibirMensaje([FromBody] JsonElement body)
        {
            await _whatsAppService.ProcesarMensaje(body);
            return Ok();
        }
    }
}