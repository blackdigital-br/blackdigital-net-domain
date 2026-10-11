# Infrastructure Reference

## Email services

The email infrastructure has two layers: a low-level `IEmailService` (sends raw emails) and a higher-level `ITemplateEmailService` (renders HTML templates then sends).

### Registration

There are **no `Add*` extension methods** in this library. Register manually:

```csharp
// SMTP sender (MailKit)
services.AddTransient<IEmailService, SmtpEmailService>();

// HTML template email service (depends on IEmailService)
services.AddTransient<ITemplateEmailService, HtmlTemplateEmailService>();
```

### `Email` fluent API

```csharp
var email = Email.Create()
    .To("user@example.com", "User Name")
    .To(new[] { "a@x.com", "b@x.com" })
    .WithSubject("Welcome")
    .WithBody("<h1>Hi</h1>", isHtml: true)
    .WithProvider("Outlook");

await emailService.SendEmailAsync(email);

// Or with a builder lambda
await emailService.SendEmailAsync(e => e
    .To("user@example.com")
    .WithSubject("Hi")
    .WithBody("Hello"));
```

`Email` properties: `Provider`, `Tos` (dictionary of email → display name), `Subject`, `Body`, `IsHtml` (default `true`).

### `TemplateEmail`

`TemplateEmail` extends `Email` with:
- `TemplateName` — name of the template file (without path; `.html` appended if missing)
- `Parameters` — dictionary of placeholder values

```csharp
var email = new TemplateEmail()
    .UseTemplate("welcome")
    .AddParameter("name", "John")
    .AddParameters(new Dictionary<string, string> { { "code", "123" } })
    .To("user@example.com")
    .WithSubject("Welcome");

await templateEmailService.SendTemplateEmailAsync(email);

// Or with a builder lambda
await templateEmailService.SendTemplateEmailAsync(e => e
    .UseTemplate("welcome")
    .AddParameter("name", "John")
    .To("user@example.com")
    .WithSubject("Welcome"));
```

### SmtpEmailService

Reads SMTP settings from `IConfiguration`. For a given provider (from `email.Provider` or `SetProvider`), it reads:

- `Email:{Provider}:Host`, `Email:{Provider}:Port`, `Email:{Provider}:User`, `Email:{Provider}:Password`, `Email:{Provider}:From`, `Email:{Provider}:Name`, `Email:{Provider}:UseSsl`, `Email:{Provider}:UseStartTls`

Any missing per-provider key falls back to the default `Email:{Key}` section.

Default port is `587` when not configured.

**Secure socket resolution** (per-provider flags take precedence):
- `UseSsl=true` → `SslOnConnect`
- `UseStartTls=true` → `StartTls`
- port `465` → `SslOnConnect`
- port `587` → `StartTls`
- otherwise → `Auto`

If `Host`, `User`, `Password`, or `From` are missing, the email is **not sent** (a warning is logged).

### HtmlTemplateEmailService

- Loads templates from `{contentRoot}/templates/email/{TemplateName}.html`.
- Rejects template names containing `..`, `/`, or `\` (path traversal protection).
- Replaces `{{ key }}` placeholders with values from `Parameters`; unmatched placeholders are removed.
- Delegates sending to the injected `IEmailService`.
- Constructor takes `IEmailService`, `ILogger<HtmlTemplateEmailService>`, and `IHostEnvironment`.

## Source files

- `src/Infrastructures/Email.cs`
- `src/Infrastructures/TemplateEmail.cs`
- `src/Infrastructures/IEmailService.cs`
- `src/Infrastructures/ITemplateEmailService.cs`
- `src/Infrastructures/BaseEmailService.cs`
- `src/Infrastructures/BaseTemplateEmailService.cs`
- `src/Infrastructures/SmtpEmailService.cs`
- `src/Infrastructures/HtmlTemplateEmailService.cs`
