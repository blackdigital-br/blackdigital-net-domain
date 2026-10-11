using BlackDigital.Domain.Security;
using Microsoft.Extensions.Logging;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BlackDigital.Domain.Test.Security
{
    public class JwtTokenServiceTest
    {
        private const string ValidSecret = "SuperSecretKey_AtLeast32BytesLong!!";
        private const string ValidIssuer = "test-issuer";
        private const string ValidAudience = "test-audience";

        private readonly Mock<ILogger<JwtTokenService>> _mockLogger;

        public JwtTokenServiceTest()
        {
            _mockLogger = new Mock<ILogger<JwtTokenService>>();
        }

        private JwtTokenService CreateService(int expirationMinutes = 1440)
            => new(ValidSecret, ValidIssuer, ValidAudience, expirationMinutes, _mockLogger.Object);

        /// <summary>
        /// Builds a signed JWT with an expiration in the past, bypassing the
        /// service's GenerateToken (which rejects Expires &lt; NotBefore).
        /// </summary>
        private string CreateExpiredToken()
        {
            var handler = new JwtSecurityTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes(ValidSecret);

            var descriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) }),
                NotBefore = DateTime.UtcNow.AddHours(-2),
                Expires = DateTime.UtcNow.AddHours(-1),
                Issuer = ValidIssuer,
                Audience = ValidAudience,
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                    new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };

            return handler.WriteToken(handler.CreateToken(descriptor));
        }

        // ---------------------------------------------------------------
        // Constructor validation
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Constructor_WithNullOrWhitespaceSecret_ShouldThrow(string? secret)
        {
            Assert.Throws<ArgumentException>(() =>
                new JwtTokenService(secret!, ValidIssuer, ValidAudience, 1440, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithShortSecret_ShouldThrow()
        {
            var shortSecret = new string('a', 31);

            Assert.Throws<ArgumentException>(() =>
                new JwtTokenService(shortSecret, ValidIssuer, ValidAudience, 1440, _mockLogger.Object));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Constructor_WithNullOrWhitespaceIssuer_ShouldThrow(string? issuer)
        {
            Assert.Throws<ArgumentException>(() =>
                new JwtTokenService(ValidSecret, issuer!, ValidAudience, 1440, _mockLogger.Object));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Constructor_WithNullOrWhitespaceAudience_ShouldThrow(string? audience)
        {
            Assert.Throws<ArgumentException>(() =>
                new JwtTokenService(ValidSecret, ValidIssuer, audience!, 1440, _mockLogger.Object));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Constructor_WithNonPositiveExpiration_ShouldThrow(int expirationMinutes)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new JwtTokenService(ValidSecret, ValidIssuer, ValidAudience, expirationMinutes, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithMinimumValidSecretLength_ShouldNotThrow()
        {
            var secret32Bytes = new string('a', 32);

            var service = new JwtTokenService(secret32Bytes, ValidIssuer, ValidAudience, 1440, _mockLogger.Object);

            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldNotThrow()
        {
            var service = new JwtTokenService(ValidSecret, ValidIssuer, ValidAudience, 1440, null);

            Assert.NotNull(service);
        }

        // ---------------------------------------------------------------
        // GenerateToken
        // ---------------------------------------------------------------

        [Fact]
        public void GenerateToken_ShouldReturnNonEmptyString()
        {
            var service = CreateService();

            var token = service.GenerateToken(Guid.NewGuid(), Array.Empty<Claim>());

            Assert.False(string.IsNullOrEmpty(token));
        }

        [Fact]
        public void GenerateToken_ShouldProduceValidJwtReadableByHandler()
        {
            var service = CreateService();
            var tokenId = Guid.NewGuid();

            var token = service.GenerateToken(tokenId, Array.Empty<Claim>());

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.Equal(ValidIssuer, jwt.Issuer);
            Assert.Equal(ValidAudience, jwt.Audiences.First());
            Assert.Equal(tokenId.ToString(), jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value);
        }

        [Fact]
        public void GenerateToken_WithClaims_ShouldIncludeClaimsInToken()
        {
            var service = CreateService();
            var claims = new List<Claim>
            {
                new("user_name", "John Doe"),
                new("user_role", "Admin"),
                new("custom", "value")
            };

            var token = service.GenerateToken(Guid.NewGuid(), claims);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.Equal("John Doe", jwt.Claims.First(c => c.Type == "user_name").Value);
            Assert.Equal("Admin", jwt.Claims.First(c => c.Type == "user_role").Value);
            Assert.Equal("value", jwt.Claims.First(c => c.Type == "custom").Value);
        }

        [Fact]
        public void GenerateToken_WithNullClaims_ShouldStillProduceToken()
        {
            var service = CreateService();

            var token = service.GenerateToken(Guid.NewGuid(), null!);

            Assert.False(string.IsNullOrEmpty(token));
        }

        [Fact]
        public void GenerateToken_WithCustomExpiration_ShouldUseProvidedExpiration()
        {
            var service = CreateService();
            var customExpiration = TimeSpan.FromMinutes(5);

            var token = service.GenerateToken(Guid.NewGuid(), Array.Empty<Claim>(), customExpiration);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // ValidTo should be approximately now + 5 minutes (allow some clock drift)
            Assert.True(jwt.ValidTo > DateTime.UtcNow.AddMinutes(4));
            Assert.True(jwt.ValidTo < DateTime.UtcNow.AddMinutes(6));
        }

        [Fact]
        public void GenerateToken_DifferentTokenIds_ShouldProduceDifferentTokens()
        {
            var service = CreateService();

            var token1 = service.GenerateToken(Guid.NewGuid(), Array.Empty<Claim>());
            var token2 = service.GenerateToken(Guid.NewGuid(), Array.Empty<Claim>());

            Assert.NotEqual(token1, token2);
        }

        // ---------------------------------------------------------------
        // ValidateToken
        // ---------------------------------------------------------------

        [Fact]
        public void ValidateToken_WithValidToken_ShouldReturnPrincipal()
        {
            var service = CreateService();
            var token = service.GenerateToken(Guid.NewGuid(), new[] { new Claim(ClaimTypes.Name, "John") });

            var principal = service.ValidateToken(token);

            Assert.NotNull(principal);
            Assert.Equal("John", principal!.FindFirst(ClaimTypes.Name)?.Value);
        }

        [Fact]
        public void ValidateToken_WithInvalidToken_ShouldReturnNull()
        {
            var service = CreateService();

            var principal = service.ValidateToken("not-a-valid-token");

            Assert.Null(principal);
        }

        [Fact]
        public void ValidateToken_WithTamperedToken_ShouldReturnNull()
        {
            var service = CreateService();
            var token = service.GenerateToken(Guid.NewGuid(), Array.Empty<Claim>());

            // Tamper with the token by changing the last character of the signature
            var tampered = token[..^1] + (token[^1] == 'a' ? 'b' : 'a');

            var principal = service.ValidateToken(tampered);

            Assert.Null(principal);
        }

        [Fact]
        public void ValidateToken_WithExpiredToken_ShouldReturnNull()
        {
            var service = CreateService();
            var token = CreateExpiredToken();

            var principal = service.ValidateToken(token);

            Assert.Null(principal);
        }

        [Fact]
        public void ValidateToken_WithTokenFromDifferentIssuer_ShouldReturnNull()
        {
            var service1 = new JwtTokenService(ValidSecret, "issuer-a", ValidAudience, 1440, _mockLogger.Object);
            var service2 = new JwtTokenService(ValidSecret, "issuer-b", ValidAudience, 1440, _mockLogger.Object);

            var token = service1.GenerateToken(Guid.NewGuid(), Array.Empty<Claim>());

            var principal = service2.ValidateToken(token);

            Assert.Null(principal);
        }

        [Fact]
        public void ValidateToken_WithTokenFromDifferentAudience_ShouldReturnNull()
        {
            var service1 = new JwtTokenService(ValidSecret, ValidIssuer, "audience-a", 1440, _mockLogger.Object);
            var service2 = new JwtTokenService(ValidSecret, ValidIssuer, "audience-b", 1440, _mockLogger.Object);

            var token = service1.GenerateToken(Guid.NewGuid(), Array.Empty<Claim>());

            var principal = service2.ValidateToken(token);

            Assert.Null(principal);
        }

        [Fact]
        public void ValidateToken_WithTokenFromDifferentSecret_ShouldReturnNull()
        {
            var secretA = new string('a', 32);
            var secretB = new string('b', 32);

            var service1 = new JwtTokenService(secretA, ValidIssuer, ValidAudience, 1440, _mockLogger.Object);
            var service2 = new JwtTokenService(secretB, ValidIssuer, ValidAudience, 1440, _mockLogger.Object);

            var token = service1.GenerateToken(Guid.NewGuid(), Array.Empty<Claim>());

            var principal = service2.ValidateToken(token);

            Assert.Null(principal);
        }

        // ---------------------------------------------------------------
        // GenerateRefreshToken
        // ---------------------------------------------------------------

        [Fact]
        public void GenerateRefreshToken_ShouldReturnNonEmptyString()
        {
            var service = CreateService();

            var refreshToken = service.GenerateRefreshToken();

            Assert.False(string.IsNullOrEmpty(refreshToken));
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnDifferentValuesEachTime()
        {
            var service = CreateService();

            var token1 = service.GenerateRefreshToken();
            var token2 = service.GenerateRefreshToken();

            Assert.NotEqual(token1, token2);
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnValidBase64()
        {
            var service = CreateService();

            var refreshToken = service.GenerateRefreshToken();

            // Should not throw when converting from base64
            var bytes = Convert.FromBase64String(refreshToken);
            Assert.Equal(32, bytes.Length);
        }

        // ---------------------------------------------------------------
        // GetTokenIdFromJwt
        // ---------------------------------------------------------------

        [Fact]
        public void GetTokenIdFromJwt_WithValidToken_ShouldReturnTokenId()
        {
            var service = CreateService();
            var tokenId = Guid.NewGuid();
            var token = service.GenerateToken(tokenId, Array.Empty<Claim>());

            var result = service.GetTokenIdFromJwt(token);

            Assert.Equal(tokenId, result);
        }

        [Fact]
        public void GetTokenIdFromJwt_WithInvalidToken_ShouldReturnNull()
        {
            var service = CreateService();

            var result = service.GetTokenIdFromJwt("invalid-token");

            Assert.Null(result);
        }

        [Fact]
        public void GetTokenIdFromJwt_WithTokenWithoutJtiClaim_ShouldReturnNull()
        {
            // Build a manual JWT without JTI claim
            var handler = new JwtSecurityTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes(ValidSecret);
            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }),
                Expires = DateTime.UtcNow.AddMinutes(10),
                Issuer = ValidIssuer,
                Audience = ValidAudience,
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                    new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };
            var token = handler.WriteToken(handler.CreateToken(tokenDescriptor));

            var service = CreateService();

            var result = service.GetTokenIdFromJwt(token);

            Assert.Null(result);
        }

        // ---------------------------------------------------------------
        // IsTokenExpired
        // ---------------------------------------------------------------

        [Fact]
        public void IsTokenExpired_WithValidToken_ShouldReturnFalse()
        {
            var service = CreateService();
            var token = service.GenerateToken(Guid.NewGuid(), Array.Empty<Claim>());

            var result = service.IsTokenExpired(token);

            Assert.False(result);
        }

        [Fact]
        public void IsTokenExpired_WithExpiredToken_ShouldReturnTrue()
        {
            var service = CreateService();
            var token = CreateExpiredToken();

            var result = service.IsTokenExpired(token);

            Assert.True(result);
        }

        [Fact]
        public void IsTokenExpired_WithInvalidToken_ShouldReturnTrue()
        {
            var service = CreateService();

            var result = service.IsTokenExpired("invalid-token");

            Assert.True(result);
        }

        // ---------------------------------------------------------------
        // GetTokenExpiration
        // ---------------------------------------------------------------

        [Fact]
        public void GetTokenExpiration_WithValidToken_ShouldReturnFutureDate()
        {
            var service = CreateService();
            var token = service.GenerateToken(Guid.NewGuid(), Array.Empty<Claim>(), TimeSpan.FromMinutes(10));

            var expiration = service.GetTokenExpiration(token);

            Assert.True(expiration > DateTime.UtcNow);
        }

        [Fact]
        public void GetTokenExpiration_WithInvalidToken_ShouldReturnMinValue()
        {
            var service = CreateService();

            var expiration = service.GetTokenExpiration("invalid-token");

            Assert.Equal(DateTime.MinValue, expiration);
        }

        [Fact]
        public void GetTokenExpiration_ShouldMatchApproximatelyProvidedExpiration()
        {
            var service = CreateService();
            var expectedExpiration = DateTime.UtcNow.AddMinutes(10);

            var token = service.GenerateToken(Guid.NewGuid(), Array.Empty<Claim>(), TimeSpan.FromMinutes(10));
            var expiration = service.GetTokenExpiration(token);

            // Allow a small tolerance for processing time
            Assert.True((expiration - expectedExpiration).Duration() < TimeSpan.FromSeconds(5));
        }
    }
}
