# 🕳️ nullability

> Status: **planned**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### null-forgiving-lies (A5)

- **Twist:** `!` silences the compiler and changes nothing at runtime: a
  promise you made, not a check anyone performs - and the flow analysis now
  propagates your lie forward.
- **Mechanic:** the null-forgiving operator is erased at compilation; it
  emits no check. Worse, it *teaches* the nullable flow analysis that the
  value is non-null, so warnings downstream of the `!` disappear too - one
  suppression hides a family of them.
- **Who hits it:** nullable-migration codebases: `FirstOrDefault()!`,
  `Config["key"]!`, `default!` in constructors - each one a warning paid off
  with a promise.
- **Repro:** warning-free code with one `!` that NREs at runtime; sibling
  code without `!` that the compiler correctly flags. Deterministic, no
  packages.
- **Damage:** the annotation system reports the codebase clean while the
  NREs it exists to prevent ship anyway - false confidence at project scale.
- **Verified:** language-level erasure; verify at build.

## Seeds

Not yet a full candidate - brainstorm before proposing.

- **default-of-t-is-null** (A5,6) - a generic `T Get<T>()` returning
  `default` hands back null for every reference T despite the non-nullable
  annotation: the "never null" contract is a compile-time fiction.
