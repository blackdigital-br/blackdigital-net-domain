namespace BlackDigital.Domain.Infrastructures
{
    /// <summary>
    /// Defines a service for sending email messages.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends the specified email asynchronously.
        /// </summary>
        /// <param name="email">The email to send.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        Task SendEmailAsync(Email email);

        /// <summary>
        /// Sends an email built from a fluent builder function asynchronously.
        /// </summary>
        /// <param name="sendEmail">A function that configures the <see cref="Email"/> instance.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        Task SendEmailAsync(Func<Email, Email> sendEmail);
    }
}
