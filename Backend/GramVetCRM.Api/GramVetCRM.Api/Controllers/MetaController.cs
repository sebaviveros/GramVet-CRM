using Microsoft.AspNetCore.Mvc;
using GramVetCRM.Service;
using System.Text.Json;

namespace GramVetCRM.Api.Controllers
{
    // Webhook de Facebook Messenger e Instagram (Meta Graph API).
    // Endpoint separado del de WhatsApp para no afectar su configuración.
    [ApiController]
    [Route("api/[controller]")]
    public class MetaController : ControllerBase
    {
        private readonly IMetaMessagingService _metaService;
        private readonly IConfiguration _config;

        public MetaController(IMetaMessagingService metaService, IConfiguration config)
        {
            _metaService = metaService;
            _config = config;
        }

        // Verificación del webhook (Meta hace un GET al configurarlo)
        [HttpGet("webhook")]
        public IActionResult Verificar(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.verify_token")] string token,
            [FromQuery(Name = "hub.challenge")] string challenge)
        {
            var verifyToken = _config["Meta:VerifyToken"];

            if (mode == "subscribe" && token == verifyToken)
                return Ok(int.Parse(challenge));

            return Forbid();
        }

        // Recepción de mensajes entrantes de Messenger/Instagram
        [HttpPost("webhook")]
        public async Task<IActionResult> RecibirMensaje([FromBody] JsonElement body)
        {
            await _metaService.ProcesarMensaje(body);
            return Ok();
        }
    }
}
