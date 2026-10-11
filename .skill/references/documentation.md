# Documentation Reference

## Goal

Keep examples and docs aligned with the real API surface of BlackDigital.Domain.

## Documentation rules

1. Read the relevant implementation files before editing docs.
2. Treat `src/` as authoritative when there is any mismatch.
3. Keep `README.md` and `README.pt.md` synchronized for core examples.
4. Keep `doc/*.md` and `doc/*.pt.md` synchronized when the same topic exists in both languages.
5. Prefer examples that compile against the current public API.
6. When a convention is surprising, explain it directly instead of hiding it.

## Known mismatch patterns to watch for

- Calling the JWT service `JwtService` instead of `JwtTokenService` (renamed)
- Using `Email.Create(templateName)` — `TemplateEmail` uses `new TemplateEmail().UseTemplate(name)`
- Assuming `Add*` DI extension methods exist (they don't in this library yet)
- JWT secrets shorter than 32 bytes (the constructor rejects them)
- Email template paths outside `{contentRoot}/templates/email/`
- Confusing `SendEmailAsync` (IEmailService) with `SendTemplateEmailAsync` (ITemplateEmailService)
- Using `<div>` or `<img>` tags in READMEs — NuGet's markdown renderer strips them; use plain Markdown image syntax with absolute URLs for the logo

## Recommended update flow

1. inspect source files
2. inspect the matching docs
3. fix incorrect examples first
4. expand coverage second
5. verify both English and Portuguese versions when needed

## File map

- root guides
  - `README.md`
  - `README.pt.md`
- detailed docs
  - `doc/security*.md`
  - `doc/email*.md`
