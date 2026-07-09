namespace GramVetCRM.Model.DTOs.Auth
{
    public class LoginRequestDto
    {
        public string Username { get; set; }
        public string Password { get; set; }

        /// <summary>Token que devuelve el widget de Cloudflare Turnstile.</summary>
        public string? CaptchaToken { get; set; }
    }
}