# 📅 datetime

> Status: **opened**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### kind-blind-equality (A4)

- **Twist:** 14:00 UTC equals 14:00 local - `==` compares ticks and
  completely ignores Kind, so two different instants in time are "equal" and
  two representations of the same instant are not.
- **Mechanic:** DateTime is a tick count plus a Kind flag; `==`, `<`,
  CompareTo, GetHashCode all use ticks only. Every comparison, sort, and
  dictionary lookup inherits the blindness. DateTimeOffset compares the
  actual instant - the type choice is the fix.
- **Who hits it:** codebases mixing DateTime.Now and DateTime.UtcNow (all of
  them), and values loaded from databases as Kind=Unspecified compared
  against UtcNow.
- **Repro:** two DateTimes with equal ticks and different Kinds: `==` true.
  BUILDER WARNING: do not call `.ToUniversalTime()` or `.ToLocalTime()` to
  show they differ - those depend on the machine's zone (CI-would-lie rule).
  Pin everything: convert with `TimeZoneInfo.FindSystemTimeZoneById` on a
  fixed zone, or contrast with DateTimeOffset values built from explicit
  offsets. Deterministic once pinned. No packages.
- **Damage:** expiry checks ("token still valid?") pass or fail by wall-clock
  coincidence - security-adjacent silent wrongness that flips with the
  server's timezone.
- **Verified:** `==` semantics documented; verify at build with pinned zones.

### the-25-hour-day (A6)

- **Twist:** AddHours(24) is not "tomorrow, same time" - across a DST
  transition the same wall time is 23 or 25 hours away, and the daily job
  drifts an hour off, twice a year.
- **Mechanic:** DateTime arithmetic is pure tick math; wall-clock time is
  ticks *interpreted through a zone*, and on two days a year the mapping
  shifts. "next run = last + 24h" lands on 02:00 instead of 03:00 after the
  spring transition. Correct scheduling converts through TimeZoneInfo at each
  occurrence instead of adding a fixed duration.
- **Who hits it:** hand-rolled daily schedulers, "24-hour" token lifetimes,
  billing cut-offs - anything that *means* "03:00 local tomorrow" but
  *computes* +24 hours.
- **Repro:** pin `TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv")` (never
  TimeZoneInfo.Local - CI-would-lie), pick the known transition date, show
  last+24h converts to 02:00 wall time, not 03:00. Deterministic because zone
  and date are constants. No packages.
- **Damage:** the maintenance window fires during business hours; daily
  boundaries shift so one "day" of records is 23 hours long.
- **BUILDER WARNING:** #0020 (shrinking-billing-day) lives in this hall and
  is also DST-driven. Before building, read #0020 and aim this exhibit at
  the *scheduler drift* (next-run computation), not the day-length itself;
  if the overlap still feels too close, propose replacing rather than
  duplicating.
- **Verified:** timezone math documented; verify at build with pinned zone,
  after the #0020 overlap check.
