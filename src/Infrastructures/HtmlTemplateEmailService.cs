using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace BlackDigital.Domain.Infrastructures
{
    /// <summary>
    /// Renders HTML email templates with <c>{{ key }}</c> placeholder substitution and delegates
    /// the actual sending to an <see cref="IEmailService"/> instance.
    /// Templates are loaded from <c>{contentRoot}/templates/email/</c>.
    /// </summary>
    public class HtmlTemplateEmailService : BaseTemplateEmailService
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<HtmlTemplateEmailService> _logger;
        private readonly string _templateBasePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlTemplateEmailService"/> class.
        /// </summary>
        /// <param name="emailService">The email service used to send the rendered email.</param>
        /// <param name="logger">Logger for recording send operations and errors.</param>
        /// <param name="environment">The host environment used to resolve the content root path for templates.</param>
        public HtmlTemplateEmailService(IEmailService emailService,
                                        ILogger<HtmlTemplateEmailService> logger,
                                        IHostEnvironment environment)
        {
            _emailService = emailService;
            _logger = logger;

            // Templates live under the host's content root (e.g. templates/email).
            var contentRoot = environment?.ContentRootPath ?? Directory.GetCurrentDirectory();
            _templateBasePath = Path.Combine(contentRoot, "templates", "email");
        }

        /// <summary>
        /// Loads the specified HTML template, replaces <c>{{ key }}</c> placeholders with the
        /// provided parameters, and sends the rendered email through the configured
        /// <see cref="IEmailService"/>.
        /// </summary>
        /// <param name="email">The template email containing the template name and parameters.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the template name is empty or contains path traversal characters.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the template file does not exist.</exception>
        public override async Task SendTemplateEmailAsync(TemplateEmail email)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(email);

                // Load the template
                email.Body = await LoadTemplateAsync(email.TemplateName);

                // Process the template with parameters
                email.Body = ProcessTemplate(email.Body, email.Parameters);

                // Send the email
                await _emailService.SendEmailAsync(email);

                _logger.LogInformation("Template email sent successfully. Template: {TemplateName}, To: {To}", email.TemplateName, string.Join(", ", email.Tos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending template email. Template: {TemplateName}, To: {To}",
                    email?.TemplateName ?? "<null>",
                    email != null ? string.Join(", ", email.Tos) : "<null>");
                throw;
            }
        }

        private async Task<string> LoadTemplateAsync(string templateName)
        {
            // Validate template name (no path traversal, empty or whitespace)
            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentException("Invalid template name", nameof(templateName));
            }

            // Check for path traversal attempts
            if (templateName.Contains("..") || templateName.Contains("/") || templateName.Contains("\\"))
            {
                throw new ArgumentException("Invalid template name", nameof(templateName));
            }

            // Ensure template name is safe (no path traversal)
            var safeTemplateName = Path.GetFileName(templateName);
            if (string.IsNullOrEmpty(safeTemplateName))
            {
                throw new ArgumentException("Invalid template name", nameof(templateName));
            }

            // Add .html extension if not present
            if (!safeTemplateName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                safeTemplateName += ".html";
            }

            var templatePath = Path.Combine(_templateBasePath, safeTemplateName);

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template not found: {safeTemplateName}");
            }

            return await File.ReadAllTextAsync(templatePath);
        }

        // Matches {{ key }} placeholders, capturing the trimmed key name.
        // Compiled once and reused across all template renders to avoid
        // recompiling a regex per parameter on every send.
        private static readonly Regex TemplateVariableRegex =
            new(@"\{\{\s*([^}]+?)\s*\}\}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private string ProcessTemplate(string templateContent, Dictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                // Remove all template variables if no parameters provided
                return TemplateVariableRegex.Replace(templateContent, string.Empty);
            }

            // Replace placeholders in a single pass, looking up each captured
            // key in the parameters dictionary. Unmatched keys are removed.
            return TemplateVariableRegex.Replace(templateContent, match =>
            {
                var key = match.Groups[1].Value;
                return parameters.TryGetValue(key, out var value) ? (value ?? string.Empty) : string.Empty;
            });
        }
    }
}
