using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GramVetCRM.Service
{
    /// <summary>
    /// Verifica el captcha de Cloudflare Turnstile contra el endpoint siteverify.
    /// Config en appsettings, sección "Turnstile": Enabled / SecretKey.
    /// </summary>
    public class TurnstileService : ITurnstileService
    {
        private const string SiteVerifyUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<TurnstileService> _logger;

        public TurnstileService(
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            ILogger<TurnstileService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Turnstile");
            _config = config;
            _logger = logger;
        }

        public async Task<bool> Validar(string? token, string? remoteIp = null)
        {
            var habilitado = _config.GetValue<bool?>("Turnstile:Enabled") ?? false;
            if (!habilitado)
                return true;

            var secretKey = _config["Turnstile:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                // Sin secret no se puede verificar nada. Se deja pasar para no
                // dejar el login inaccesible por un error de configuración.
                _logger.LogWarning("Turnstile habilitado pero sin SecretKey. Se omite la validación.");
                return true;
            }

            if (string.IsNullOrWhiteSpace(token))
                return false;

            try
            {
                var campos = new Dictionary<string, string>
                {
                    ["secret"] = secretKey,
                    ["response"] = token
                };

                if (!string.IsNullOrWhiteSpace(remoteIp))
                    campos["remoteip"] = remoteIp;

                using var contenido = new FormUrlEncodedContent(campos);
                var respuesta = await _httpClient.PostAsync(SiteVerifyUrl, contenido);
                var cuerpo = await respuesta.Content.ReadAsStringAsync();

                if (!respuesta.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Turnstile devolvió {Status}: {Cuerpo}", respuesta.StatusCode, cuerpo);
                    return false;
                }

                using var json = JsonDocument.Parse(cuerpo);
                var exito = json.RootElement.TryGetProperty("success", out var okProp) && okProp.GetBoolean();

                if (!exito && json.RootElement.TryGetProperty("error-codes", out var errores))
                    _logger.LogWarning("Captcha rechazado por Cloudflare: {Errores}", errores.ToString());

                return exito;
            }
            catch (Exception ex)
            {
                // Ante una caída de Cloudflare se rechaza: es un login, no conviene
                // abrirlo de par en par si no se pudo verificar al visitante.
                _logger.LogError(ex, "Error verificando el captcha de Turnstile");
                return false;
            }
        }
    }
}
