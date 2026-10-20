# BlackDigital.Domain

[🇧🇷 Português](README.pt.md) | 🇺🇸 English

![BlackDigital.Domain Logo](https://raw.githubusercontent.com/blackdigital-br/blackdigital-net-domain/main/doc/images/Logo128.png)

**Reusable domain building blocks for any .NET backend**

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0%2B-purple.svg)](https://dotnet.microsoft.com/)

## About the Project

**BlackDigital.Domain** is a shared library of infrastructure and security building blocks designed to be reused across any .NET backend project — REST APIs, background jobs, worker services, Azure Functions, and more. It has no dependency on ASP.NET Core, so it can be referenced from any project type.

### Key Features

- **Email Infrastructure**: SMTP email service (via MailKitLite) with multi-provider support and HTML template rendering
- **JWT Token Service**: Generate, validate, and inspect JWT tokens
- **Password Hashing**: Argon2id password hashing interface

## Installation

### Via .NET CLI

```bash
dotnet add package BlackDigital.Domain
```

### Via PackageReference

```xml
<PackageReference Include="BlackDigital.Domain" Version="26.10.11.01" />
```

## Features

### Email

Send transactional emails with SMTP and optional HTML templates.

```csharp
using BlackDigital.Domain.Infrastructures;

var email = Email.Create()
    .To("user@example.com", "John Doe")
    .WithSubject("Welcome")
    .WithBody("<h1>Hello, John!</h1>", isHtml: true);

await emailService.SendEmailAsync(email);
```

[Full Email Documentation](doc/email.md)

### Security (JWT + Password Hashing)

```csharp
using BlackDigital.Domain.Security;
using System.Security.Claims;

var tokenService = new JwtTokenService(secretKey, issuer, audience);

var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, "123"),
    new Claim(ClaimTypes.Name, "john@example.com")
};

string token = tokenService.GenerateToken(Guid.NewGuid(), claims);
```

[Full Security Documentation](doc/security.md)

## Project Structure

```
BlackDigital.Domain/
├── src/
│   ├── Infrastructures/   # Email services (SMTP + templates)
│   └── Security/          # JWT and password hashing services
├── test/                  # Unit tests
├── doc/                   # Documentation
└── README.md
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
