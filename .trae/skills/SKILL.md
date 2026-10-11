---
name: blackdigital-domain-dev
description: Developer guidelines for the BlackDigital.Domain library. Mandates XML summaries and unit tests for every public API change. Use when implementing, modifying, or reviewing code in this library repository.
---

# BlackDigital.Domain — Developer Guide

This skill is for **developers of the `BlackDigital.Domain` library itself**, not for consumers of the library.

## Use this skill

Apply this skill whenever you are:
- adding, modifying, or removing public APIs in `src/`
- fixing bugs or refactoring library code
- reviewing library changes
- writing or updating unit tests in `test/`

For consumer-facing questions (how to use the library), use the `blackdigital-domain` skill instead.

## Non-negotiable rules

### 1. XML summaries are mandatory

**Every public type, method, property, parameter, and return value in `src/` must have XML documentation.**

- Public classes, interfaces, enums: `<summary>`
- Public methods: `<summary>`, `<param>` for each parameter, `<returns>` for non-void, `<exception>` for thrown exceptions
- Public properties: `<summary>`
- Public constructor parameters: documented via `<param>` on the constructor

Use the existing documented files as style guides. Before submitting any change, run a mental (or IDE) check for missing doc comments on new or modified public members.

If a public member genuinely cannot be documented, call it out explicitly in your change summary.

### 2. Unit tests are mandatory

**Every public behavior change or new public API must have corresponding unit tests in `test/`.**

- New public method/class → at least happy-path and failure-path tests
- Bug fix → a regression test that fails without the fix
- Refactor → existing tests must still pass; add tests for newly uncovered branches when reasonable

Tests live in `test/` and mirror the `src/` folder structure (e.g. `src/Security/JwtTokenService.cs` → `test/Security/JwtTokenServiceTest.cs`).

Do not add a public API without tests under the same PR/change.

### 3. Source of truth is `src/`

When `src/`, `README*`, and `doc/` disagree, `src/` wins. Fix the docs to match.

### 4. Keep bilingual docs in sync

If you change a documented behavior, update both `doc/<file>.md` and `doc/<file>.pt.md`, and both `README.md` and `README.pt.md` where relevant.

### 5. No ASP.NET Core dependencies

This library must remain usable from any .NET project type (APIs, jobs, functions, workers). Do not introduce dependencies on `Microsoft.AspNetCore.App` or web-specific types. Use `Microsoft.Extensions.*` abstractions instead.

## Testing conventions

- Framework: **xUnit** + **Moq** + **coverlet.collector** (see `test/BlackDigital.Domain.Test.csproj`)
- Naming: `{ClassName}Test.cs` in a folder mirroring `src/`
- Test method naming: `{MethodUnderTest}_{Condition}_{ExpectedResult}` (e.g. `Constructor_WhenSecretKeyTooShort_ShouldThrowArgumentException`)
- Use `[Theory]` + `[InlineData]` for parameterized cases
- Mock external dependencies (ILogger, IConfiguration, IHostEnvironment, IEmailService, etc.)
- Prefer testing public API only
- Run `dotnet test` from the repo root before declaring a change complete

See [references/testing.md](references/testing.md) for the current test coverage map and missing test targets.

## Change checklist

Before completing any library change, verify:

- [ ] All new/modified public members have XML doc comments
- [ ] Unit tests exist for new or changed behavior
- [ ] `dotnet build` succeeds
- [ ] `dotnet test` passes
- [ ] If documented behavior changed, both English and Portuguese docs are updated
- [ ] No new ASP.NET Core dependencies were introduced

## Reference routing

- [references/testing.md](references/testing.md) — test conventions and coverage gaps
- [references/summaries.md](references/summaries.md) — XML documentation conventions and audit status
