# BlackDigital.Domain Overview

## Purpose

BlackDigital.Domain is a shared library of infrastructure and security building blocks designed to be reused across any .NET backend project — REST APIs, background jobs, worker services, Azure Functions, and more. It has **no dependency on ASP.NET Core**, so it can be referenced from any project type.

It provides:
- **Email infrastructure**: SMTP email service (via MailKitLite) with multi-provider support and HTML template rendering
- **JWT token service**: Generate, validate, and inspect JWT tokens
- **Password hashing interface**: `IPasswordHashService` contract for password hashing

## Main source areas

- `src/Infrastructures/`
  - `IEmailService`, `Email`, `BaseEmailService`, `SmtpEmailService` — SMTP sending via MailKitLite with multi-provider fallback
  - `ITemplateEmailService`, `TemplateEmail`, `BaseTemplateEmailService`, `HtmlTemplateEmailService` — HTML template email with `{{ key }}` placeholders
- `src/Security/`
  - `JwtTokenService` / `ITokenService` — JWT generation, validation, refresh tokens, token id extraction
  - `IPasswordHashService` — password hashing contract
  - `AuthorizeAttribute` — dependency-free authorization metadata (class/interface/method) with nullable `Roles` and `Policy`; not enforced by the library

## Main docs

- `README.md`
- `README.pt.md`
- `doc/security.md` / `doc/security.pt.md`
- `doc/email.md` / `doc/email.pt.md`

## Source-first files to consult

- Security behavior
  - `src/Security/JwtTokenService.cs`
  - `src/Security/ITokenService.cs`
  - `src/Security/IPasswordHashService.cs`
  - `src/Security/AuthorizeAttribute.cs`
- Email behavior
  - `src/Infrastructures/SmtpEmailService.cs`
  - `src/Infrastructures/HtmlTemplateEmailService.cs`
  - `src/Infrastructures/Email.cs`
  - `src/Infrastructures/TemplateEmail.cs`

## Common LLM mistakes to avoid

- Calling the service `JwtService` instead of `JwtTokenService` (it was renamed)
- Using short JWT secrets (the service requires at least 32 bytes)
- Placing email templates outside `{contentRoot}/templates/email/`
- Assuming DI helpers exist (there are no `Add*` extension methods in this library yet — register types manually)
- Using `Email.Create(templateName)` — `TemplateEmail` uses `new TemplateEmail().UseTemplate(name)` instead
- Confusing `SendEmailAsync` (IEmailService) with `SendTemplateEmailAsync` (ITemplateEmailService)
