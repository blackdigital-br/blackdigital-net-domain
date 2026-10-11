namespace BlackDigital.Domain.Infrastructures
{
    /// <summary>
    /// Base implementation of <see cref="IEmailService"/> that provides a convenience overload
    /// for building emails via a fluent builder function.
    /// </summary>
    public abstract class BaseEmailService : IEmailService
    {
        /// <inheritdoc/>
        public abstract Task SendEmailAsync(Email email);

        /// <summary>
        /// Sends an email configured through a fluent builder function.
        /// </summary>
        /// <param name="sendEmail">A function that configures the <see cref="Email"/> instance.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        public async Task SendEmailAsync(Func<Email, Email> sendEmail)
        {
            var email = sendEmail(Email.Create());
            await SendEmailAsync(email);
        }
    }
}
