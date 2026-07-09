namespace GramVetCRM.Service
{
    public interface ITurnstileService
    {
        /// <summary>
        /// Valida el token del captcha contra Cloudflare.
        /// Devuelve true si el captcha está desactivado por configuración.
        /// </summary>
        Task<bool> Validar(string? token, string? remoteIp = null);
    }
}
