namespace BlackDigital.Domain.Infrastructures
{
    /// <summary>
    /// Defines a service for sending template-based email messages.
    /// </summary>
    public interface ITemplateEmailService
    {
        /// <summary>
        /// Sends the specified template email asynchronously.
        /// </summary>
        /// <param name="email">The template email to send.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        Task SendTemplateEmailAsync(TemplateEmail email);

        /// <summary>
        /// Sends a template email built from a fluent builder function asynchronously.
        /// </summary>
        /// <param name="emailBuilder">A function that configures the <see cref="TemplateEmail"/> instance.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        Task SendTemplateEmailAsync(Func<TemplateEmail, TemplateEmail> emailBuilder);
    }
}
