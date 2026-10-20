# Security Reference

## JWT token service

`JwtTokenService` implements `ITokenService` and provides JWT generation, validation, and inspection.

> Note: This was renamed from `JwtService` in BlackDigital.AspNet to `JwtTokenService` in BlackDigital.Domain.

### Constructor

```csharp
var tokenService = new JwtTokenService(
    secretKey: "your-very-long-secret-key-at-least-32-chars",
    issuer: "https://your-app.com",
    audience: "https://your-app.com",
    expirationMinutes: 1440,
    logger: null // optional ILogger<JwtTokenService>
);
```

Register in DI:

```csharp
services.AddSingleton<ITokenService>(new JwtTokenService(secretKey, issuer, audience));
```

There are **no `AddJwtService` extension methods** in this library — register manually or in the consuming project.

### Constructor constraints

- `secretKey` must be at least **32 bytes (256 bits)** — `ArgumentException` otherwise (HMAC-SHA256 requirement).
- `issuer` and `audience` cannot be null/empty — `ArgumentException`.
- `expirationMinutes` must be greater than zero — `ArgumentOutOfRangeException`.

### API

| Method | Description |
|--------|-------------|
| `GenerateToken(Guid tokenId, IEnumerable<Claim> claims, TimeSpan? expiration)` | Creates a signed JWT; the `jti` claim is set from `tokenId`; defaults to the configured expiration |
| `ValidateToken(string token)` | Returns a `ClaimsPrincipal` or `null` on failure |
| `GenerateRefreshToken()` | Returns a 32-byte random base64 string |
| `GetTokenIdFromJwt(string token)` | Extracts the `jti` claim as a `Guid?` |
| `IsTokenExpired(string token)` | Returns `true` if `ValidTo <= DateTime.UtcNow` (or on read failure) |
| `GetTokenExpiration(string token)` | Returns `ValidTo` (or `DateTime.MinValue` on failure) |

### Validation parameters

`ValidateToken` uses:
- `ValidateIssuerSigningKey = true` (symmetric key from the secret)
- `ValidateIssuer = true`, `ValidIssuer` = configured issuer
- `ValidateAudience = true`, `ValidAudience` = configured audience
- `ValidateLifetime = true`
- `ClockSkew = TimeSpan.Zero`

Signing algorithm: `HmacSha256Signature`.

## Password hashing

`IPasswordHashService` defines the contract:

```csharp
public interface IPasswordHashService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}
```

There is **no built-in implementation** in this library. Consuming projects provide their own (e.g. Argon2id, BCrypt).

## Authorization attribute

A single dependency-free metadata attribute marks access requirements on classes, interfaces and methods:

```csharp
[Authorize]                                            // any authenticated user
[Authorize(Roles = new[] { "Admin", "Manager" })]      // at least one of the listed roles
[Authorize(Policy = "Category.Write")]                 // named authorization policy
[Authorize(Roles = new[] { "Admin" }, Policy = "X")]   // both
```

- It is **metadata only** — this library never enforces it; the consuming application reads it.
- `Roles` (`string[]?`) and `Policy` (`string?`) are nullable; when omitted, only authentication is required.
- `AttributeUsage`: `Class | Interface | Method`, `AllowMultiple = false`, `Inherited = true`.
- No validation is performed: the attribute holds the declared values as-is.

## Source files

- `src/Security/JwtTokenService.cs`
- `src/Security/ITokenService.cs`
- `src/Security/IPasswordHashService.cs`
- `src/Security/AuthorizeAttribute.cs`
