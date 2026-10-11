using System.Security.Claims;

namespace BlackDigital.Domain.Security
{
    /// <summary>
    /// Defines a service for generating, validating and inspecting JWT tokens.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a signed JWT token containing the specified claims.
        /// </summary>
        /// <param name="tokenId">The unique identifier written into the <c>jti</c> claim.</param>
        /// <param name="claims">The claims to include in the token.</param>
        /// <param name="expiration">Optional token lifetime. When null, the service's default expiration is used.</param>
        /// <returns>The serialized JWT token string.</returns>
        string GenerateToken(Guid tokenId, IEnumerable<Claim> claims, TimeSpan? expiration = null);

        /// <summary>
        /// Validates a JWT token and returns the associated claims principal.
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <returns>A <see cref="ClaimsPrincipal"/> if the token is valid; otherwise null.</returns>
        ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// Generates a cryptographically random refresh token.
        /// </summary>
        /// <returns>A base64-encoded 32-byte random string.</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Extracts the token identifier (<c>jti</c> claim) from a JWT without validating its signature.
        /// </summary>
        /// <param name="token">The JWT token to read.</param>
        /// <returns>The token identifier as a <see cref="Guid"/>, or null if it cannot be parsed.</returns>
        Guid? GetTokenIdFromJwt(string token);

        /// <summary>
        /// Determines whether a JWT token has expired based on its <c>exp</c> claim.
        /// </summary>
        /// <param name="token">The JWT token to inspect.</param>
        /// <returns><c>true</c> if the token is expired or cannot be read; otherwise <c>false</c>.</returns>
        bool IsTokenExpired(string token);

        /// <summary>
        /// Reads the expiration date (<c>exp</c> claim) from a JWT token.
        /// </summary>
        /// <param name="token">The JWT token to inspect.</param>
        /// <returns>The token's expiration date in UTC, or <see cref="DateTime.MinValue"/> if it cannot be read.</returns>
        DateTime GetTokenExpiration(string token);
    }
}
