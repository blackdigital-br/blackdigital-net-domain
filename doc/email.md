# Email

[🇧🇷 Português](email.pt.md) | 🇺🇸 English

The Email module provides an SMTP email service (via MailKitLite) with optional HTML template support. It supports multiple providers with fallback configuration.

## Configuration

### appsettings.json

```json
{
  "Email": {
    "Host": "smtp.sendgrid.net",
    "Port": 587,
    "User": "apikey",
    "Password": "your-sendgrid-api-key",
    "From": "no-reply@example.com",
    "Name": "My App",
    "UseStartTls": true
  }
}
```

Multiple providers are supported by nesting provider-specific configuration. If a provider-specific value is not found, the service falls back to the default `Email` section:

```json
{
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "User": "you@gmail.com",
    "Password": "your-app-password",
    "From": "no-reply@example.com",
    "Name": "My App",
    "UseStartTls": true,
    "Outlook": {
      "Host": "smtp.office365.com",
      "User": "you@outlook.com",
      "Password": "your-password"
    }
  }
}
```

### Register the service

```csharp
using BlackDigital.Domain.Infrastructures;

// Register SMTP email service
services.AddTransient<IEmailService, SmtpEmailService>();

// Optional: register HTML template email service
services.AddTransient<ITemplateEmailService, HtmlTemplateEmailService>();
```

## Sending a Plain Email

Inject `IEmailService` and build an email with the fluent `Email` API:

```csharp
public class NotificationService
{
    private readonly IEmailService _emailService;

    public NotificationService(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendWelcomeAsync(string toEmail, string name)
    {
        var email = Email.Create()
            .To(toEmail, name)
            .WithSubject("Welcome to My App")
            .WithBody($"<h1>Hello, {name}!</h1><p>Thanks for signing up.</p>", isHtml: true);

        await _emailService.SendEmailAsync(email);
    }
}
```

### Email Fluent API

| Method | Description |
|--------|-------------|
| `Email.Create()` | Starts a new email |
| `.To(address, name?)` | Adds a recipient |
| `.WithSubject(subject)` | Sets the subject |
| `.WithBody(body, isHtml)` | Sets the body content |
| `.WithProvider(provider)` | Selects a configured provider |

## HTML Template Emails

For transactional emails that share a common layout, use `ITemplateEmailService` with HTML templates.

### Template location

Templates are loaded from the `templates/email/` directory at the application content root. Each template is an HTML file with `{{ key }}` placeholders.

Example `templates/email/welcome.html`:

```html
<!DOCTYPE html>
<html>
<body>
  <h1>Welcome, {{ userName }}!</h1>
  <p>Thank you for registering at {{ appName }}.</p>
  <p>Click <a href="{{ confirmUrl }}">here</a> to confirm your account.</p>
</body>
</html>
```

### Sending a template email

```csharp
public class NotificationService
{
    private readonly ITemplateEmailService _templateEmail;

    public NotificationService(ITemplateEmailService templateEmail)
    {
        _templateEmail = templateEmail;
    }

    public async Task SendWelcomeAsync(string toEmail, string userName, string confirmUrl)
    {
        var email = new TemplateEmail()
            .UseTemplate("welcome")
            .To(toEmail)
            .WithSubject("Welcome to My App")
            .AddParameter("userName", userName)
            .AddParameter("appName", "My App")
            .AddParameter("confirmUrl", confirmUrl);

        await _templateEmail.SendTemplateEmailAsync(email);
    }
}
```

The service loads `templates/email/welcome.html`, replaces each `{{ key }}` with the corresponding parameter value, and sends the resulting HTML.

### TemplateEmail Fluent API

Inherits all methods from `Email` and adds:

| Method | Description |
|--------|-------------|
| `.UseTemplate(templateName)` | Selects the template `templates/email/{templateName}.html` |
| `.AddParameter(key, value)` | Adds a placeholder value (`{{ key }}` → value) |
| `.AddParameters(dictionary, overwrite)` | Adds multiple placeholder values |

## IEmailService Members

| Member | Description |
|--------|-------------|
| `SendEmailAsync(Email email)` | Sends the email using the configured provider |

## Related Documentation

- [Main Documentation](../README.md)
