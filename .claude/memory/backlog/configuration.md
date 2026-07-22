# ⚙️ configuration

> Status: **planned**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### binding-fails-silently (A5)

- **Twist:** One typo'd key and the setting silently stays at its default:
  nothing throws, nothing logs, and the feature is "off in prod only".
- **Mechanic:** the configuration binder matches keys to properties by name;
  unmatched keys are ignored and unmatched properties keep their defaults -
  both directions are silent by design. The failure value is always the
  most plausible-looking one: false, 0, null.
- **Who hits it:** everyone with appsettings.json: `"MaxRetires": 5` binds
  nothing; MaxRetries is 0; retries are disabled; the review reads the JSON
  and sees the intent, not the typo.
- **Repro:** in-memory configuration dictionary with one typo'd key; Bind to
  an options class; the property holds its default. Packages:
  `Microsoft.Extensions.Configuration` + `.Binder` (+
  `#:property PublishAot=false`). Deterministic.
- **Damage:** feature flags off, limits at zero, endpoints null - each
  reading as a deliberate choice, none logged anywhere.
- **😈 seed:** `ErrorOnUnknownConfiguration` exists in BinderOptions and
  almost nobody turns it on - strictness is opt-in, silence is the default.
- **Verified:** documented binder behavior; verify at build.
