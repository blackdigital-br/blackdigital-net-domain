# Testing Reference

## Test stack

- **xUnit**
- **Moq**
- **coverlet.collector** for coverage
- `Microsoft.NET.Test.Sdk`
- Target framework: `net10.0`

Test project: `test/BlackDigital.Domain.Test.csproj`. It references `..\src\BlackDigital.Domain.csproj`.

## Folder structure

Tests mirror `src/`:

```
test/
├── Infrastructures/
│   ├── SmtpEmailServiceTest.cs
│   └── HtmlTemplateEmailServiceTest.cs
└── Security/
    └── JwtTokenServiceTest.cs
```

## Naming conventions

- Class: `{SourceClassName}Test` (e.g. `JwtTokenServiceTest`)
- Method: `{MethodUnderTest}_{Condition}_{ExpectedResult}`
  - `Constructor_WhenSecretKeyTooShort_ShouldThrowArgumentException`
  - `SendTemplateEmailAsync_WithValidTemplate_ShouldSendProcessedEmail`
  - `ValidateToken_WithInvalidSignature_ShouldReturnNull`

## Arrange / Act / Assert

Use the AAA pattern with comments:

```csharp
[Fact]
public void Constructor_WhenSecretKeyTooShort_ShouldThrowArgumentException()
{
    // Arrange
    var shortKey = "short";

    // Act & Assert
    Assert.Throws<ArgumentException>(() =>
        new JwtTokenService(shortKey, "issuer", "audience"));
}
```

## Mocking patterns

Mock `ILogger<T>` by verifying `Log` calls:

```csharp
_mockLogger.Verify(
    x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("expected message")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
    Times.Once);
```

Mock `IConfiguration` by setting up the indexer:

```csharp
var mockConfig = new Mock<IConfiguration>();
mockConfig.Setup(x => x["Email:Host"]).Returns("smtp.example.com");
mockConfig.Setup(x => x["Email:Port"]).Returns("587");
```

Mock `IHostEnvironment` for `HtmlTemplateEmailService`:

```csharp
var mockEnv = new Mock<IHostEnvironment>();
mockEnv.Setup(x => x.ContentRootPath).Returns(Path.GetTempPath());
```

## Coverage gaps — tests that do NOT exist yet

These modules have **zero or incomplete** test coverage. Adding tests here is high-value work:

### Infrastructures (partial)
- `Email` / `TemplateEmail` — fluent builder API
- `BaseEmailService` / `BaseTemplateEmailService` — builder overloads
- `IEmailService` / `ITemplateEmailService` — interface contracts

### Security (partial)
- `IPasswordHashService` — interface contract (no implementation to test yet)

## Running tests

```powershell
dotnet test
```

With coverage:

```powershell
dotnet test --collect:"XPlat Code Coverage"
```
