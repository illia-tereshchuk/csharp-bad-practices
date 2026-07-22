# 🔔 events

> Status: **opened**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### one-handler-kills-the-rest (A5)

- **Twist:** A subscriber that throws stops the invocation list: everyone
  registered after it never runs, and neither the publisher nor the surviving
  subscribers ever find out.
- **Mechanic:** raising an event walks the invocation list in subscription
  order, synchronously, on one thread. An unhandled exception in handler #2
  aborts the walk: handlers #3..#N are skipped, and the exception surfaces at
  the *publisher's* raise line, in code that has no idea what subscribers
  exist.
- **Who hits it:** in-process pub/sub: an order-placed event feeding email +
  audit + analytics handlers. Whoever subscribed last is at the mercy of
  everyone who subscribed earlier.
- **Repro:** three subscribers appending to a list; the middle one throws;
  show the third never ran and the publisher's post-raise code was skipped
  too. Deterministic, no packages.
- **Damage:** audit/bookkeeping handlers silently skipped whenever an
  unrelated handler fails - the compliance record has holes exactly where
  incidents happened.
- **😈 seed:** subscription order - effectively DI registration order -
  decides who dies. A reordered registration list is a behavior change no
  diff reviewer sees.
- **Verified:** language-level delegate semantics; verify at build.

### invoke-race-on-null-check (A1)

- **Twist:** `if (E != null) E(...)` - the last subscriber unsubscribes
  between the check and the call: NullReferenceException from an event that
  "was just checked".
- **Mechanic:** delegates are immutable; subscribe/unsubscribe swap the field.
  Check-then-invoke reads the field twice, and the value can change between
  the reads. `E?.Invoke(...)` (or copy-to-local) reads once - that single
  read is the entire fix.
- **Who hits it:** any event raised while subscribers come and go: UI
  teardown, service shutdown, plugin unload.
- **Repro:** determinism note for the builder - do NOT race two threads in a
  loop (nondeterministic per-iteration; the timing ban applies). Stage the
  interleaving single-threaded instead: perform the null check, then
  unsubscribe the last handler (this is the other thread's step, frozen in
  time), then perform the invoke from the field - NRE, 100% reproducible.
  It is honest because the staged interleaving is exactly what the two-thread
  version does when it loses.
- **Damage:** shutdown-time crashes that reproduce monthly in production and
  never on a dev machine.
- **Verified:** language-level; staged repro chosen to satisfy determinism.
  Verify at build.
