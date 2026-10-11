using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BlackDigital.Domain.Security
{
    /// <summary>
    /// JWT-based implementation of <see cref="ITokenService"/> using HMAC-SHA256 signing.
    /// Enforces a minimum 32-byte (256-bit) secret key and zero clock skew for validation.
    /// </summary>
    public class JwtTokenService : ITokenService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly TimeSpan _defaultExpiration;
        private readonly ILogger<JwtTokenService>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenService"/> class.
        /// </summary>
        /// <param name="secretKey">The secret key used to sign tokens. Must be at least 32 bytes (256 bits).</param>
        /// <param name="issuer">The token issuer claim value.</param>
        /// <param name="audience">The token audience claim value.</param>
        /// <param name="expirationMinutes">The default token lifetime in minutes. Must be greater than zero. Defaults to 1440 (24 hours).</param>
        /// <param name="logger">Optional logger for recording validation failures.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="secretKey"/>, <paramref name="issuer"/> or <paramref name="audience"/> is null or empty, or when <paramref name="secretKey"/> is shorter than 32 bytes.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="expirationMinutes"/> is less than or equal to zero.</exception>
        public JwtTokenService(string secretKey, string issuer, string audience, int expirationMinutes = 1440, ILogger<JwtTokenService>? logger = null)
        {
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException("Secret key cannot be null or empty.", nameof(secretKey));

            // HMAC-SHA256 requires a key of at least 256 bits (32 bytes). Enforce
            // it so callers don't accidentally weaken signing with short secrets.
            var keyBytes = Encoding.ASCII.GetBytes(secretKey);
            if (keyBytes.Length < 32)
                throw new ArgumentException("Secret key must be at least 32 bytes (256 bits) long for HMAC-SHA256.", nameof(secretKey));

            if (string.IsNullOrWhiteSpace(issuer))
                throw new ArgumentException("Issuer cannot be null or empty.", nameof(issuer));

            if (string.IsNullOrWhiteSpace(audience))
                throw new ArgumentException("Audience cannot be null or empty.", nameof(audience));

            if (expirationMinutes <= 0)
                throw new ArgumentOutOfRangeException(nameof(expirationMinutes), "Expiration must be greater than zero minutes.");

            _secretKey = secretKey;
            _issuer = issuer;
            _audience = audience;
            _defaultExpiration = TimeSpan.FromMinutes(expirationMinutes);
            _logger = logger;
        }

        /// <summary>
        /// Generates a signed JWT token containing the specified claims.
        /// </summary>
        /// <param name="tokenId">The unique identifier written into the <c>jti</c> claim.</param>
        /// <param name="claims">The claims to include in the token.</param>
        /// <param name="expiration">Optional token lifetime. When null, the service's default expiration is used.</param>
        /// <returns>The serialized JWT token string.</returns>
        public string GenerateToken(Guid tokenId, IEnumerable<Claim> claims, TimeSpan? expiration = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var now = DateTime.UtcNow;
            var expirationSpan = expiration ?? _defaultExpiration;

            var expires = now.Add(expirationSpan);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, tokenId.ToString())
                }),
                Expires = expires,
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            if (claims != null)
            {
                foreach (var claim in claims)
                {
                    tokenDescriptor.Subject.AddClaim(claim);
                }
            }

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Validates a JWT token and returns the associated claims principal.
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <returns>A <see cref="ClaimsPrincipal"/> if the token is valid; otherwise null.</returns>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "JWT token validation failed.");
                return null;
            }
        }

        /// <summary>
        /// Generates a cryptographically random refresh token.
        /// </summary>
        /// <returns>A base64-encoded 32-byte random string.</returns>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// Extracts the token identifier (<c>jti</c> claim) from a JWT without validating its signature.
        /// </summary>
        /// <param name="token">The JWT token to read.</param>
        /// <returns>The token identifier as a <see cref="Guid"/>, or null if it cannot be parsed.</returns>
        public Guid? GetTokenIdFromJwt(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);

                var tokenIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
                if (tokenIdClaim != null && Guid.TryParse(tokenIdClaim.Value, out var tokenId))
                {
                    return tokenId;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Failed to extract token id from JWT.");
                return null;
            }
        }

        /// <summary>
        /// Determines whether a JWT token has expired based on its <c>exp</c> claim.
        /// </summary>
        /// <param name="token">The JWT token to inspect.</param>
        /// <returns><c>true</c> if the token is expired or cannot be read; otherwise <c>false</c>.</returns>
        public bool IsTokenExpired(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);

                return jsonToken.ValidTo <= DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Failed to determine JWT expiration state; treating as expired.");
                return true;
            }
        }

        /// <summary>
        /// Reads the expiration date (<c>exp</c> claim) from a JWT token.
        /// </summary>
        /// <param name="token">The JWT token to inspect.</param>
        /// <returns>The token's expiration date in UTC, or <see cref="DateTime.MinValue"/> if it cannot be read.</returns>
        public DateTime GetTokenExpiration(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);

                return jsonToken.ValidTo;
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Failed to read JWT expiration; returning MinValue.");
                return DateTime.MinValue;
            }
        }
    }
}
