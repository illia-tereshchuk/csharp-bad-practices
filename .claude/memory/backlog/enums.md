# 🏷️ enums

> Status: **planned**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### the-overlapping-flags (A5)

- **Twist:** [Flags] values numbered 1, 2, 3: the third flag IS the first two
  OR-ed together, so granting Delete silently grants Read and Write - and
  every HasFlag check happily agrees.
- **Mechanic:** flags combine by bitwise OR, so members must be powers of
  two. Sequential numbering makes 3 == 1|2: setting "flag 3" sets both lower
  bits; checking it answers true whenever both others are present. Nothing
  in the language or runtime objects.
- **Who hits it:** whoever adds the third member to a two-member [Flags] enum
  by continuing the sequence 1, 2, 3 - the single most natural wrong move in
  the API.
- **Repro:** `[Flags] enum Perm { Read = 1, Write = 2, Delete = 3 }`;
  `(Read | Write).HasFlag(Delete)` is true - a user granted read+write can
  delete. Deterministic, no packages.
- **Damage:** permission escalation - security stakes, screenshots well.
- **😈 seed:** the enum prints correctly (`Delete`), logs look right, and
  audits confirm the user "had the Delete flag" - the corruption extends
  into the investigation.
- **Verified:** ran on .NET 10 (2026-07-22): (Read|Write).HasFlag(Delete)
  == true.

## Seeds

Not yet a full candidate - brainstorm before proposing.

- **hasflag-zero-always-true** (A5) - `permissions.HasFlag(Permission.None)`
  is always true because every set contains the zero flag, so the guard
  meant to detect "no permissions" passes for everyone.

- **enum-default-is-zero** (A5) - `default(Status)` is 0 whatever you named
  it; if `Active` is the first member, every uninitialized DTO and struct
  field arrives already "Active" without anyone setting it.
