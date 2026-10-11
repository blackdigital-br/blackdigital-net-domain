# XML Summaries Reference

## Why summaries matter

- Public APIs without XML comments are invisible to IntelliSense.
- The library is consumed by other developers; every public member must be self-documenting.
- `GenerateDocumentationFile` is enabled in the `.csproj`, so missing docs produce `CS1591` warnings.

## Required tags

Every **public** member in `src/` must have:

| Member type | Required tags |
|-------------|---------------|
| Class / interface / enum / struct | `<summary>` |
| Enum value | `<summary>` |
| Method | `<summary>`, `<param>` per parameter, `<returns>` (non-void), `<exception>` for each thrown type |
| Constructor | `<summary>`, `<param>` per parameter, `<exception>` |
| Property | `<summary>` |
| Field | `<summary>` |
| Event | `<summary>`, `<event>` |

### Example

```csharp
/// <summary>
/// Sends an email using the configured SMTP provider.
/// </summary>
/// <param name="email">The email to send. Must not be null.</param>
/// <returns>A task that completes when the email has been sent.</returns>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/> is null.</exception>
public override Task SendEmailAsync(Email email)
{
    // ...
}
```

## Style rules

- Write summaries in **third-person present** ("Gets", "Sends", "Returns").
- Start with a verb.
- Keep `<summary>` to 1–2 sentences; put detail in `<remarks>` if needed.
- `<param>` descriptions should state the constraint or role, not repeat the name.
- Use `<paramref name="foo"/>` when referring to a parameter inside prose.
- Use `<see cref="OtherType"/>` for cross-references.
- For async methods, document the `Task`/`ValueTask` result in `<returns>`.
- For methods that throw, list **every** exception type with the condition that triggers it.

## Current audit status

The following files have **incomplete or missing** XML documentation and should be prioritized:

| File | Status |
|------|--------|
| `Infrastructures/Email.cs` | **No docs** — all public members need summaries |
| `Infrastructures/TemplateEmail.cs` | **No docs** |
| `Infrastructures/IEmailService.cs` | **No docs** |
| `Infrastructures/ITemplateEmailService.cs` | **No docs** |
| `Infrastructures/BaseEmailService.cs` | **No docs** |
| `Infrastructures/BaseTemplateEmailService.cs` | **No docs** |
| `Infrastructures/HtmlTemplateEmailService.cs` | **No docs** |
| `Infrastructures/SmtpEmailService.cs` | Good — already has XML docs |
| `Security/JwtTokenService.cs` | **No docs** |
| `Security/ITokenService.cs` | **No docs** |
| `Security/IPasswordHashService.cs` | **No docs** |

## How to add summaries efficiently

1. Place the cursor on a public member.
2. Type `///` above it — the IDE auto-generates the skeleton tags.
3. Fill in `<summary>`, `<param>`, `<returns>`, `<exception>`.
4. For methods that throw, check the implementation and `throw` statements.
5. For extension methods, document the `this` parameter like any other parameter.

## Enforcement

There is no automated enforcement yet (only `CS1591` warnings). As a developer working on this codebase:

- **Never** add a new public member without XML comments.
- When modifying a public member, review and update its existing comments.
- If you touch a file with missing docs, add them for the members you change (and ideally the whole file).
