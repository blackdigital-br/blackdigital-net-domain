namespace BlackDigital.Domain.Infrastructures
{
    /// <summary>
    /// Represents an email message with a fluent builder API for setting recipients, subject, body and provider.
    /// </summary>
    public class Email
    {
        /// <summary>
        /// Gets or sets the name of the email provider to use for sending (e.g. "Outlook", "Gmail").
        /// When null or empty, the default provider configuration is used.
        /// </summary>
        public string? Provider { get; set; }

        /// <summary>
        /// Gets or sets the recipients as a dictionary mapping email address to display name.
        /// </summary>
        public Dictionary<string, string> Tos { get; set; } = new();

        /// <summary>
        /// Gets or sets the email subject line.
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email body content.
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the body should be sent as HTML.
        /// Defaults to <c>true</c>.
        /// </summary>
        public bool IsHtml { get; set; } = true;

        /// <summary>
        /// Sets the email provider name.
        /// </summary>
        /// <param name="provider">The provider name (e.g. "Outlook", "Gmail").</param>
        /// <returns>This <see cref="Email"/> instance for method chaining.</returns>
        public Email WithProvider(string provider)
        {
            Provider = provider;
            return this;
        }

        /// <summary>
        /// Adds a single recipient to the email.
        /// </summary>
        /// <param name="email">The recipient email address.</param>
        /// <param name="name">Optional display name for the recipient. Defaults to the email address when null.</param>
        /// <returns>This <see cref="Email"/> instance for method chaining.</returns>
        public Email To(string email, string? name = null)
        {
            Tos[email] = name ?? email;
            return this;
        }

        /// <summary>
        /// Adds multiple recipients to the email using their address as the display name.
        /// </summary>
        /// <param name="emails">The collection of recipient email addresses.</param>
        /// <returns>This <see cref="Email"/> instance for method chaining.</returns>
        public Email To(IEnumerable<string> emails)
        {
            foreach (var email in emails)
                Tos[email] = email;

            return this;
        }

        /// <summary>
        /// Sets the email subject.
        /// </summary>
        /// <param name="subject">The subject line.</param>
        /// <returns>This <see cref="Email"/> instance for method chaining.</returns>
        public Email WithSubject(string subject)
        {
            Subject = subject;
            return this;
        }

        /// <summary>
        /// Sets the email body and whether it should be sent as HTML.
        /// </summary>
        /// <param name="body">The body content.</param>
        /// <param name="isHtml">Whether the body is HTML. Defaults to <c>true</c>.</param>
        /// <returns>This <see cref="Email"/> instance for method chaining.</returns>
        public Email WithBody(string body, bool isHtml = true)
        {
            Body = body;
            IsHtml = isHtml;
            return this;
        }

        /// <summary>
        /// Creates a new empty <see cref="Email"/> instance.
        /// </summary>
        /// <returns>A new <see cref="Email"/> instance.</returns>
        public static Email Create() => new();

        /// <summary>
        /// Creates a new <see cref="Email"/> with a single recipient, subject and body.
        /// </summary>
        /// <param name="to">The recipient email address.</param>
        /// <param name="subject">The subject line.</param>
        /// <param name="body">The body content.</param>
        /// <param name="isHtml">Whether the body is HTML. Defaults to <c>true</c>.</param>
        /// <param name="provider">Optional provider name. Defaults to null (default provider).</param>
        /// <returns>A configured <see cref="Email"/> instance.</returns>
        public static Email Create(string to, string subject, string body, bool isHtml = true, string? provider = null)
        {
            return new Email
            {
                Provider = provider,
                Tos = new Dictionary<string, string> { { to, to } },
                Subject = subject,
                Body = body,
                IsHtml = isHtml
            };
        }

        /// <summary>
        /// Creates a new <see cref="Email"/> with multiple recipients, subject and body.
        /// </summary>
        /// <param name="tos">A dictionary mapping recipient email addresses to display names.</param>
        /// <param name="subject">The subject line.</param>
        /// <param name="body">The body content.</param>
        /// <param name="isHtml">Whether the body is HTML. Defaults to <c>true</c>.</param>
        /// <param name="provider">Optional provider name. Defaults to null (default provider).</param>
        /// <returns>A configured <see cref="Email"/> instance.</returns>
        public static Email Create(Dictionary<string, string> tos, string subject, string body, bool isHtml = true, string? provider = null)
        {
            return new Email
            {
                Provider = provider,
                Tos = tos,
                Subject = subject,
                Body = body,
                IsHtml = isHtml
            };
        }
    }
}
