namespace BlackDigital.Domain.Security
{
    /// <summary>
    /// Defines a service for hashing and verifying passwords.
    /// </summary>
    public interface IPasswordHashService
    {
        /// <summary>
        /// Hashes the specified password.
        /// </summary>
        /// <param name="password">The plain-text password to hash.</param>
        /// <returns>The hashed password representation.</returns>
        string HashPassword(string password);

        /// <summary>
        /// Verifies a plain-text password against a previously hashed password.
        /// </summary>
        /// <param name="password">The plain-text password to verify.</param>
        /// <param name="hashedPassword">The previously hashed password to compare against.</param>
        /// <returns><c>true</c> if the password matches the hash; otherwise <c>false</c>.</returns>
        bool VerifyPassword(string password, string hashedPassword);
    }
}
