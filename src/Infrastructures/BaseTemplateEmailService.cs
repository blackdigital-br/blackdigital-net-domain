namespace BlackDigital.Domain.Infrastructures
{
    /// <summary>
    /// Base implementation of <see cref="ITemplateEmailService"/> that provides a convenience overload
    /// for building template emails via a fluent builder function.
    /// </summary>
    public abstract class BaseTemplateEmailService : ITemplateEmailService
    {
        /// <inheritdoc/>
        public abstract Task SendTemplateEmailAsync(TemplateEmail email);

        /// <summary>
        /// Sends a template email configured through a fluent builder function.
        /// </summary>
        /// <param name="emailBuilder">A function that configures the <see cref="TemplateEmail"/> instance.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        public Task SendTemplateEmailAsync(Func<TemplateEmail, TemplateEmail> emailBuilder)
        {
            var email = emailBuilder(new TemplateEmail());
            return SendTemplateEmailAsync(email);
        }
    }
}
