# 🧩 pattern-matching

> Status: **planned**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### switch-expression-not-exhaustive (A5)

- **Twist:** Add one enum member and a switch expression that "covered
  everything" starts throwing in production - the compiler only ever warned,
  and the warning was easy to ship.
- **Mechanic:** a switch expression over an enum with all members handled
  compiles clean; when a new member appears, callers get warning CS8509 (not
  an error) and, at runtime, SwitchExpressionException for the unhandled
  value. Teams without warnings-as-errors ship it. The tempting "fix" -
  a `_ => default` arm - silences the warning forever and converts future
  crashes into silently wrong values.
- **Who hits it:** enums in shared contract libraries: the enum grows in one
  repo, the switch lives in another; each compiles happily on its own
  schedule.
- **Repro:** simulate the two-versions situation in one file (the
  renumbered-status trick): switch over a value cast from an int the enum
  does not define, or model V1/V2 enums; the switch throws
  SwitchExpressionException. Deterministic, no packages.
- **Damage:** runtime crash in code everyone believed total; with the `_`
  arm, silently wrong routing instead - one rung down the fear ladder.
- **Verified:** compiler and runtime behavior documented; verify at build.
