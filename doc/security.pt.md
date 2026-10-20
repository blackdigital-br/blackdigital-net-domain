# Segurança

🇧🇷 Português | [🇺🇸 English](security.md)

O módulo de Segurança oferece dois blocos prontos para uso: um **serviço de tokens JWT** e um **serviço de hash de senhas**.

## Serviço de Token JWT

O `ITokenService` / `JwtTokenService` cuida da geração, validação e inspeção de tokens JWT.

### Configuração

O serviço é configurado diretamente via parâmetros do construtor:

```csharp
using BlackDigital.Domain.Security;

var tokenService = new JwtTokenService(
    secretKey: "sua-chave-secreta-bem-longa-com-pelo-menos-32-caracteres",
    issuer: "https://seu-app.com",
    audience: "https://seu-app.com",
    expirationMinutes: 60
);
```

Registre-o no container de DI:

```csharp
services.AddSingleton<ITokenService>(new JwtTokenService(
    secretKey: "sua-chave-secreta-bem-longa-com-pelo-menos-32-caracteres",
    issuer: "https://seu-app.com",
    audience: "https://seu-app.com",
    expirationMinutes: 60
));
```

### Gerando Tokens

Injete `ITokenService` e gere tokens de acesso e de atualização:

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

### Validando e Inspecionando Tokens

```csharp
// Valida um token e retorna o ClaimsPrincipal
ClaimsPrincipal? principal = _tokenService.ValidateToken(token);

if (principal != null)
{
    // Token é válido
}

// Verifica se um token expirou
bool expired = _tokenService.IsTokenExpired(token);

// Obtém a data de expiração do token
DateTime expiration = _tokenService.GetTokenExpiration(token);

// Obtém o id do token (claim jti) de um token
Guid? tokenId = _tokenService.GetTokenIdFromJwt(token);
```

### Membros do ITokenService

| Membro | Descrição |
|--------|-----------|
| `GenerateToken(Guid tokenId, IEnumerable<Claim> claims, TimeSpan? expiration = null)` | Gera um token JWT de acesso assinado |
| `GenerateRefreshToken()` | Gera um refresh token aleatório (Base64) |
| `ValidateToken(string token)` | Valida a assinatura e o tempo de vida do token; retorna o `ClaimsPrincipal` ou `null` |
| `GetTokenIdFromJwt(string token)` | Extrai o valor da claim `jti` como `Guid?` |
| `IsTokenExpired(string token)` | Retorna `true` se o token já expirou |
| `GetTokenExpiration(string token)` | Retorna a data de expiração do token, ou `DateTime.MinValue` |

## Hash de Senhas

O `IPasswordHashService` define um contrato para fazer hash e verificar senhas.

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
        // Armazene o hash no banco de dados
    }

    public bool VerifyPassword(string plainPassword, string storedHash)
    {
        return _passwordHash.VerifyPassword(plainPassword, storedHash);
    }
}
```

### Membros do IPasswordHashService

| Membro | Descrição |
|--------|-----------|
| `HashPassword(string password)` | Retorna o hash da senha |
| `VerifyPassword(string password, string hashedPassword)` | Retorna `true` se a senha corresponde ao hash |

## Atributo de Autorização

A biblioteca disponibiliza um único atributo sem dependências que marca classes, interfaces e métodos com seus requisitos de acesso. Ele é **apenas metadado** — a biblioteca nunca o aplica. A aplicação consumidora (um middleware REST, um filtro do ASP.NET Core, um job runner, etc.) é quem o lê e rejeita requisições não autorizadas.

```csharp
using BlackDigital.Domain.Security;

[Authorize]                                            // qualquer usuário autenticado
public interface IUserService
{
    [Authorize(Roles = new[] { "Admin", "Manager" })]  // pelo menos um dos papéis listados
    Task<IEnumerable<User>> ListUsersAsync();

    [Authorize(Policy = "Category.Write")]             // política de autorização nomeada
    Task SaveUserAsync([Body] SaveUser user);
}
```

| Propriedade | Requisito |
|-------------|-----------|
| *(nenhuma)* | O chamador precisa estar autenticado. Nenhum papel ou política é avaliado. |
| `Roles` | O usuário autenticado precisa ter pelo menos um dos papéis listados. |
| `Policy` | O usuário autenticado precisa satisfazer a política nomeada. |

Observações:

- `Roles` e `Policy` são anuláveis e podem ser combinados em um único atributo: `[Authorize(Roles = new[] { "Admin" }, Policy = "Category.Write")]`.
- O atributo pode ser aplicado em classes, interfaces e métodos e é herdado — um requisito declarado em uma classe base ou em um membro sobrescrito continua visível via reflection.

## Documentação Relacionada

- [Documentação Principal](../README.pt.md)
