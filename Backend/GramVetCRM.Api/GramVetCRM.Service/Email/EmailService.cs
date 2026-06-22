using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GramVetCRM.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml)
        {
            var section = _config.GetSection("Email");
            var host = section["Host"];
            var portStr = section["Port"];
            var user = section["User"];
            var password = section["Password"];
            var fromName = section["FromName"] ?? "GramVet CRM";

            // Fallback de desarrollo: si no hay credenciales SMTP configuradas,
            // se loggea el correo en consola en lugar de enviarlo. Esto permite
            // trabajar sin App Password hasta que se configure el SMTP real.
            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning(
                    "[EMAIL SIMULADO - SMTP no configurado]\nPara: {Destinatario}\nAsunto: {Asunto}\nCuerpo:\n{Cuerpo}",
                    destinatario, asunto, cuerpoHtml);
                return;
            }

            var port = int.TryParse(portStr, out var p) ? p : 587;

            using var mensaje = new MailMessage
            {
                From = new MailAddress(user, fromName),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true
            };
            mensaje.To.Add(destinatario);

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, password),
                EnableSsl = true
            };

            await client.SendMailAsync(mensaje);
            _logger.LogInformation("Correo enviado a {Destinatario}", destinatario);
        }
    }
}
