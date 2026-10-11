# Email

🇧🇷 Português | [🇺🇸 English](email.md)

O módulo de Email fornece um serviço de email SMTP (via MailKit) com suporte opcional a templates HTML. Ele suporta múltiplos provedores com configuração de fallback.

## Configuração

### appsettings.json

```json
{
  "Email": {
    "Host": "smtp.sendgrid.net",
    "Port": 587,
    "User": "apikey",
    "Password": "sua-api-key-do-sendgrid",
    "From": "no-reply@exemplo.com",
    "Name": "Meu App",
    "UseStartTls": true
  }
}
```

Múltiplos provedores são suportados aninhando configurações específicas por provedor. Se um valor específico do provedor não for encontrado, o serviço faz fallback para a seção padrão `Email`:

```json
{
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "User": "voce@gmail.com",
    "Password": "sua-senha-de-app",
    "From": "no-reply@exemplo.com",
    "Name": "Meu App",
    "UseStartTls": true,
    "Outlook": {
      "Host": "smtp.office365.com",
      "User": "voce@outlook.com",
      "Password": "sua-senha"
    }
  }
}
```

### Registrar o serviço

```csharp
using BlackDigital.Domain.Infrastructures;

// Registra o serviço de email SMTP
services.AddTransient<IEmailService, SmtpEmailService>();

// Opcional: registra o serviço de email com template HTML
services.AddTransient<ITemplateEmailService, HtmlTemplateEmailService>();
```

## Enviando um Email Simples

Injete `IEmailService` e construa um email com a API fluente `Email`:

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
            .WithSubject("Bem-vindo ao Meu App")
            .WithBody($"<h1>Olá, {name}!</h1><p>Obrigado por se cadastrar.</p>", isHtml: true);

        await _emailService.SendEmailAsync(email);
    }
}
```

### API Fluente Email

| Método | Descrição |
|--------|-----------|
| `Email.Create()` | Inicia um novo email |
| `.To(address, name?)` | Adiciona um destinatário |
| `.WithSubject(subject)` | Define o assunto |
| `.WithBody(body, isHtml)` | Define o conteúdo do corpo |
| `.WithProvider(provider)` | Seleciona um provedor configurado |

## Emails com Template HTML

Para emails transacionais que compartilham um layout comum, use `ITemplateEmailService` com templates HTML.

### Localização dos templates

Templates são carregados do diretório `templates/email/` na raiz de conteúdo da aplicação. Cada template é um arquivo HTML com placeholders `{{ chave }}`.

Exemplo `templates/email/welcome.html`:

```html
<!DOCTYPE html>
<html>
<body>
  <h1>Bem-vindo, {{ userName }}!</h1>
  <p>Obrigado por se cadastrar no {{ appName }}.</p>
  <p>Clique <a href="{{ confirmUrl }}">aqui</a> para confirmar sua conta.</p>
</body>
</html>
```

### Enviando um email com template

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
            .WithSubject("Bem-vindo ao Meu App")
            .AddParameter("userName", userName)
            .AddParameter("appName", "Meu App")
            .AddParameter("confirmUrl", confirmUrl);

        await _templateEmail.SendTemplateEmailAsync(email);
    }
}
```

O serviço carrega `templates/email/welcome.html`, substitui cada `{{ chave }}` pelo valor do parâmetro correspondente e envia o HTML resultante.

### API Fluente TemplateEmail

Herda todos os métodos de `Email` e adiciona:

| Método | Descrição |
|--------|-----------|
| `.UseTemplate(templateName)` | Seleciona o template `templates/email/{templateName}.html` |
| `.AddParameter(key, value)` | Adiciona um valor de placeholder (`{{ chave }}` → valor) |
| `.AddParameters(dictionary, overwrite)` | Adiciona múltiplos valores de placeholder |

## Membros do IEmailService

| Membro | Descrição |
|--------|-----------|
| `SendEmailAsync(Email email)` | Envia o email usando o provedor configurado |

## Documentação Relacionada

- [Documentação Principal](../README.pt.md)
