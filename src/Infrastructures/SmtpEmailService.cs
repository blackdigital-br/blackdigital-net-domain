using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace BlackDigital.Domain.Infrastructures
{
    /// <summary>
    /// Email service implementation using the SMTP protocol via MailKit.
    /// Supports multiple email providers (Gmail, Outlook, etc.) with flexible per-provider
    /// configuration and fallback to the default <c>Email:</c> section.
    /// </summary>
    public class SmtpEmailService : BaseEmailService
    {
        private readonly ILogger<SmtpEmailService> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Gets the name of the email provider to use. When null or empty, the default
        /// provider configuration is used.
        /// </summary>
        public string? Provider { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpEmailService"/> class.
        /// </summary>
        /// <param name="logger">Logger for recording information and errors.</param>
        /// <param name="configuration">Application configuration containing the email settings.</param>
        public SmtpEmailService(ILogger<SmtpEmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Sets the email provider to use (e.g. "Outlook", "Gmail").
        /// When null or empty, the default provider configuration is used.
        /// </summary>
        /// <param name="providerName">The provider name, or null to use the default configuration.</param>
        public void SetProvider(string? providerName)
        {
            Provider = providerName;
            _logger.LogInformation("Email provider changed to: {Provider}",
                string.IsNullOrEmpty(providerName) ? "Default" : providerName);
        }

        /// <summary>
        /// Sends an email asynchronously using the SMTP protocol via MailKit.
        /// Supports per-provider configuration with fallback to the default <c>Email:</c> section.
        /// </summary>
        /// <param name="email">The <see cref="Email"/> containing recipients, subject, body and settings.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        public override async Task SendEmailAsync(Email email)
        {
            try
            {
                var provider = email.Provider ?? Provider;

                // Determina o prefixo da configuração baseado no provedor
                var configPrefix = string.IsNullOrEmpty(provider) ? "Email" : $"Email:{provider}";

                // Lê as configurações com fallback para configuração padrão
                var host = GetConfigValue("Host", configPrefix);
                var portStr = GetConfigValue("Port", configPrefix) ?? "587";
                var port = int.Parse(portStr);
                var user = GetConfigValue("User", configPrefix);
                var password = GetConfigValue("Password", configPrefix);
                var fromEmail = GetConfigValue("From", configPrefix);
                var fromName = GetConfigValue("Name", configPrefix);
                var sslRaw = GetConfigValue("UseSsl", configPrefix);
                var startTlsRaw = GetConfigValue("UseStartTls", configPrefix);

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fromEmail))
                {
                    _logger.LogWarning("Email configuration is incomplete. Email not sent to {Recipients}", string.Join(", ", email.Tos.Keys));
                    return;
                }

                using var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName ?? fromEmail, fromEmail));

                foreach (var to in email.Tos)
                {
                    message.To.Add(new MailboxAddress(to.Value, to.Key));
                }

                message.Subject = email.Subject;

                var bodyBuilder = new BodyBuilder();
                if (email.IsHtml)
                    bodyBuilder.HtmlBody = email.Body;
                else
                    bodyBuilder.TextBody = email.Body;

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                // Decide secure socket option: defaults to STARTTLS on port 587,
                // implicit SSL on 465, and auto-negotiation otherwise. The caller
                // can override per-provider via UseSsl / UseStartTls flags.
                var secureSocket = ResolveSecureSocketOption(port, sslRaw, startTlsRaw);

                await client.ConnectAsync(host, port, secureSocket);
                await client.AuthenticateAsync(user, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(quit: true);

                _logger.LogInformation("Email sent successfully to with subject: {Subject}", email.Subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to with subject: {Subject}", email.Subject);
                throw;
            }
        }

        /// <summary>
        /// Resolves the <see cref="SecureSocketOptions"/> to use for the SMTP
        /// connection. Explicit per-provider flags take precedence; otherwise the
        /// common defaults (implicit SSL on 465, STARTTLS on 587, auto otherwise)
        /// are applied.
        /// </summary>
        private static SecureSocketOptions ResolveSecureSocketOption(int port, string? useSslRaw, string? useStartTlsRaw)
        {
            if (bool.TryParse(useSslRaw, out var useSsl) && useSsl)
                return SecureSocketOptions.SslOnConnect;

            if (bool.TryParse(useStartTlsRaw, out var useStartTls) && useStartTls)
                return SecureSocketOptions.StartTls;

            if (port == 465)
                return SecureSocketOptions.SslOnConnect;

            if (port == 587)
                return SecureSocketOptions.StartTls;

            return SecureSocketOptions.Auto;
        }

        /// <summary>
        /// Gets a configuration value with fallback to the default <c>Email:</c> section.
        /// First tries the provider-specific key; if not found, falls back to the default section.
        /// </summary>
        /// <param name="key">The configuration key (e.g. "Host", "Port").</param>
        /// <param name="configPrefix">The configuration prefix (e.g. "Email" or "Email:Outlook").</param>
        /// <returns>The configuration value, or null if not found.</returns>
        private string? GetConfigValue(string key, string configPrefix)
        {
            var fullKey = $"{configPrefix}:{key}";
            var value = _configuration[fullKey];

            // Se não encontrou e não é a configuração padrão, tenta fallback
            if (string.IsNullOrEmpty(value) && configPrefix != "Email")
            {
                var defaultKey = $"Email:{key}";
                value = _configuration[defaultKey];

                if (!string.IsNullOrEmpty(value))
                {
                    _logger.LogDebug("Using fallback configuration for {Key}: Email:{Key} (provider {Provider} not configured)",
                        key, key, Provider);
                }
            }

            return value;
        }
    }
}
