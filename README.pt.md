# BlackDigital.Domain

🇧🇷 Português | [🇺🇸 English](README.md)

![BlackDigital.Domain Logo](https://raw.githubusercontent.com/blackdigital-br/blackdigital-net-domain/main/doc/images/Logo128.png)

**Blocos de construção de domínio reutilizáveis para qualquer backend .NET**

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0%2B-purple.svg)](https://dotnet.microsoft.com/)

## Sobre o Projeto

O **BlackDigital.Domain** é uma biblioteca compartilhada de blocos de infraestrutura e segurança projetada para ser reutilizada em qualquer projeto backend .NET — APIs REST, jobs em background, worker services, Azure Functions e mais. Não possui dependência do ASP.NET Core, podendo ser referenciada por qualquer tipo de projeto.

### Principais Funcionalidades

- **Infraestrutura de Email**: Serviço de email SMTP (via MailKit) com suporte a múltiplos provedores e renderização de templates HTML
- **Serviço de Tokens JWT**: Gere, valide e inspecione tokens JWT
- **Hash de Senhas**: Interface de hash de senhas Argon2id

## Instalação

### Via .NET CLI

```bash
dotnet add package BlackDigital.Domain
```

### Via PackageReference

```xml
<PackageReference Include="BlackDigital.Domain" Version="26.10.11.01" />
```

## Funcionalidades

### Email

Envie emails transacionais com SMTP e templates HTML opcionais.

```csharp
using BlackDigital.Domain.Infrastructures;

var email = Email.Create()
    .To("usuario@exemplo.com", "João Silva")
    .WithSubject("Bem-vindo")
    .WithBody("<h1>Olá, João!</h1>", isHtml: true);

await emailService.SendEmailAsync(email);
```

[Documentação Completa de Email](doc/email.pt.md)

### Segurança (JWT + Hash de Senhas)

```csharp
using BlackDigital.Domain.Security;
using System.Security.Claims;

var tokenService = new JwtTokenService(secretKey, issuer, audience);

var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, "123"),
    new Claim(ClaimTypes.Name, "joao@exemplo.com")
};

string token = tokenService.GenerateToken(Guid.NewGuid(), claims);
```

[Documentação Completa de Segurança](doc/security.pt.md)

## Estrutura do Projeto

```
BlackDigital.Domain/
├── src/
│   ├── Infrastructures/   # Serviços de email (SMTP + templates)
│   └── Security/          # Serviços de JWT e hash de senhas
├── test/                  # Testes unitários
├── doc/                   # Documentação
└── README.md
```

## Licença

Este projeto está licenciado sob a Licença MIT - veja o arquivo [LICENSE](LICENSE) para detalhes.
