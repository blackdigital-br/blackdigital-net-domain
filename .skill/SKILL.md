---
name: blackdigital-domain
description: Understand and work with the BlackDigital.Domain library in this repository. Use when the user asks about BlackDigital.Domain APIs, email infrastructure (SMTP, templates), security (JWT, password hashing), or code changes in this library. Do not use for unrelated projects or for the BlackDigital.AspNet framework.
---

# BlackDigital.Domain

Work from the source code first and use the documentation as supporting context.

## Use this skill

Apply this skill when the task is about the `BlackDigital.Domain` library in this repository, including:
- explaining email services (SMTP, HTML templates), JWT token service, or password hashing
- writing examples or documentation for email or security features
- implementing or updating features in the library
- reviewing consistency between `src/`, `README*`, and `doc/`

Do not use this skill for the `BlackDigital.AspNet` framework (REST middleware, OpenAPI, transforms) — that lives in a separate repository.

## Core rules

1. Treat `src/` as the source of truth when docs and code disagree.
2. This library has **no ASP.NET Core dependency** — it can be referenced from any project type (APIs, jobs, functions, workers).
3. When changing docs or examples, keep English and Portuguese files aligned unless the task explicitly scopes only one language.
4. When you mention library behavior, cite the real implementation files whenever possible.

## High-signal conventions

### Email infrastructure

- `Email` and `TemplateEmail` use a **fluent builder** API (`Email.Create().To(...).WithSubject(...).WithBody(...)`).
- `SmtpEmailService` resolves per-provider config with fallback to the default `Email:` section; secure socket defaults are implicit SSL on 465, STARTTLS on 587, auto otherwise.
- `HtmlTemplateEmailService` loads templates from `{contentRoot}/templates/email/` with `{{ key }}` placeholders and rejects path traversal in template names.
- `IEmailService` and `ITemplateEmailService` have a `Func<T, T>` builder overload for convenience.

Read [references/infrastructure.md](references/infrastructure.md) for details before editing email code or examples.

### Security

- `JwtTokenService` enforces a **32-byte (256-bit)** minimum secret key for HMAC-SHA256 and uses `ClockSkew = TimeSpan.Zero`.
- `JwtTokenService` is the renamed `JwtService` from BlackDigital.AspNet.
- `IPasswordHashService` defines the contract; implementations (e.g. Argon2id) live in consuming projects or other libraries.
- `AuthorizeAttribute` is a dependency-free metadata marker for classes, interfaces and methods, with nullable `Roles` and `Policy` properties (both null means authenticated only). It is never enforced by this library — consumers read it.
- Constructor validation fails fast: `ArgumentException` for short secret, empty issuer/audience; `ArgumentOutOfRangeException` for non-positive expiration.

Read [references/security.md](references/security.md) before editing security code or examples.

## Documentation workflow

When the task touches documentation:

1. Read the relevant implementation files in `src/`.
2. Read the matching docs in `README.md`, `README.pt.md`, and `doc/`.
3. Fix mismatches before expanding coverage.
4. Prefer examples that compile against the current API shape.
5. Call out non-obvious conventions explicitly when they are easy for users or LLMs to misunderstand.

Read [references/documentation.md](references/documentation.md) before larger documentation edits.

## Reference routing

- Use [references/overview.md](references/overview.md) for the project map and file locations.
- Use [references/security.md](references/security.md) for JWT and password hashing.
- Use [references/infrastructure.md](references/infrastructure.md) for email services (SMTP + templates).
- Use [references/documentation.md](references/documentation.md) for documentation maintenance rules.
