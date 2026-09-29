using System.Net;
using System.Net.Mail;

namespace AppAjuntament.Services
{
    /// <summary>
    /// Paràmetres de connexió SMTP per a un enviament concret. Venen de configuració
    /// guardada a BD (editable des de la web), no d'appsettings: així es pot canviar
    /// el servidor de correu sense haver de publicar de nou l'aplicació.
    /// </summary>
    public class EmailEnviamentParams
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool Ssl { get; set; } = true;
        public string? User { get; set; }
        public string? Password { get; set; }
        public string From { get; set; } = string.Empty;
        public string? FromName { get; set; }
    }

    /// <summary>Enviament de correus per SMTP. Genèric: no és específic de cap mòdul.</summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envia un correu HTML amb els paràmetres de connexió indicats. Retorna false
        /// (sense llançar) si falten dades de connexió, perquè les crides massives puguin
        /// continuar amb la resta de destinataris.
        /// </summary>
        Task<bool> EnviarAsync(EmailEnviamentParams parametres, string destinatari, string assumpte, string cosHtml);
    }

    public class SmtpEmailService : IEmailService
    {
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(ILogger<SmtpEmailService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> EnviarAsync(EmailEnviamentParams parametres, string destinatari, string assumpte, string cosHtml)
        {
            if (string.IsNullOrWhiteSpace(destinatari))
                return false;

            if (string.IsNullOrWhiteSpace(parametres.Host) || string.IsNullOrWhiteSpace(parametres.From))
            {
                _logger.LogWarning(
                    "Enviament de correu no configurat (falta el servidor SMTP o el remitent); no s'ha enviat el correu a {Destinatari}.",
                    destinatari);
                return false;
            }

            using var missatge = new MailMessage
            {
                From = new MailAddress(parametres.From, parametres.FromName),
                Subject = assumpte,
                Body = cosHtml,
                IsBodyHtml = true
            };
            missatge.To.Add(destinatari);

            using var client = new SmtpClient(parametres.Host, parametres.Port) { EnableSsl = parametres.Ssl };
            if (!string.IsNullOrWhiteSpace(parametres.User))
                client.Credentials = new NetworkCredential(parametres.User, parametres.Password);

            try
            {
                await client.SendMailAsync(missatge);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviant correu a {Destinatari}", destinatari);
                throw;
            }
        }
    }
}
