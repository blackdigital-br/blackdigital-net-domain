# Security

[🇧🇷 Português](security.pt.md) | 🇺🇸 English

The Security module provides two ready-to-use building blocks: a **JWT token service** and a **password hashing service**.

## JWT Token Service

The `ITokenService` / `JwtTokenService` handles JWT generation, validation, and inspection.

### Configuration

The service is configured directly via constructor parameters:

```csharp
using BlackDigital.Domain.Security;

var tokenService = new JwtTokenService(
    secretKey: "your-very-long-secret-key-at-least-32-chars",
    issuer: "https://your-app.com",
    audience: "https://your-app.com",
    expirationMinutes: 60
);
```

Register it in the DI container:

```csharp
services.AddSingleton<ITokenService>(new JwtTokenService(
    secretKey: "your-very-long-secret-key-at-least-32-chars",
    issuer: "https://your-app.com",
    audience: "https://your-app.com",
    expirationMinutes: 60
));
```

### Generating Tokens

Inject `ITokenService` and generate access and refresh tokens:

```csharp
public class AuthService
{
    private readonly ITokenService _tokenService;

    public AuthService(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public (string accessToken, string refreshToken) Login(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var accessToken = _tokenService.GenerateToken(Guid.NewGuid(), claims);
        var refreshToken = _tokenService.GenerateRefreshToken();

        return (accessToken, refreshToken);
    }
}
```

### Validating and Inspecting Tokens

```csharp
// Validate a token and get the claims principal
ClaimsPrincipal? principal = _tokenService.ValidateToken(token);

if (principal != null)
{
    // Token is valid
}

// Check whether a token has expired
bool expired = _tokenService.IsTokenExpired(token);

// Get the token's expiration date
DateTime expiration = _tokenService.GetTokenExpiration(token);

// Get the token id (jti claim) from a token
Guid? tokenId = _tokenService.GetTokenIdFromJwt(token);
```

### ITokenService Members

| Member | Description |
|--------|-------------|
| `GenerateToken(Guid tokenId, IEnumerable<Claim> claims, TimeSpan? expiration = null)` | Generates a signed JWT access token |
| `GenerateRefreshToken()` | Generates a random refresh token (Base64) |
| `ValidateToken(string token)` | Validates the token signature and lifetime; returns the `ClaimsPrincipal` or `null` |
| `GetTokenIdFromJwt(string token)` | Extracts the `jti` claim value as `Guid?` |
| `IsTokenExpired(string token)` | Returns `true` if the token is past its expiration |
| `GetTokenExpiration(string token)` | Returns the token's expiration date, or `DateTime.MinValue` |

## Password Hashing

The `IPasswordHashService` defines a contract for hashing and verifying passwords.

```csharp
public class UserService
{
    private readonly IPasswordHashService _passwordHash;

    public UserService(IPasswordHashService passwordHash)
    {
        _passwordHash = passwordHash;
    }

    public void CreateUser(string email, string plainPassword)
    {
        string hash = _passwordHash.HashPassword(plainPassword);
        // Store hash in the database
    }

    public bool VerifyPassword(string plainPassword, string storedHash)
    {
        return _passwordHash.VerifyPassword(plainPassword, storedHash);
    }
}
```

### IPasswordHashService Members

| Member | Description |
|--------|-------------|
| `HashPassword(string password)` | Returns the hash of the password |
| `VerifyPassword(string password, string hashedPassword)` | Returns `true` if the password matches the hash |

## Related Documentation

- [Main Documentation](../README.md)
