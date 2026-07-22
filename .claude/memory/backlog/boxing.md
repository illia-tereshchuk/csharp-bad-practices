# 🥊 boxing

> Status: **planned**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### mutating-a-boxed-struct (A3)

- **Twist:** Call a mutating method on a struct through an interface and the
  box mutates - your variable never changes, and each new cast makes a fresh
  box, so the mutation isn't even *somewhere*: it's nowhere.
- **Mechanic:** casting a struct to an interface copies it into a heap box;
  interface dispatch mutates the box. Cast again - new box, old state. The
  variable on the stack is never touched.
- **Who hits it:** structs stored as interfaces: `List<IShape>`,
  `IEnumerator` implementations (the classic), method parameters typed as
  the interface.
- **Repro:** counter struct implementing IIncrement; increment through the
  interface-typed reference and through the variable; print the divergence,
  then show two casts producing independent boxes. Deterministic, no
  packages.
- **Damage:** state machines that never advance, counters stuck at zero -
  and the same code with a class works, pointing suspicion anywhere but the
  cast.
- **Verified:** CLR boxing semantics; verify at build.

## Seeds

Not yet a full candidate - brainstorm before proposing.

- **unbox-must-match-exact-type** (A4,5) - `(int)(object)42L` throws
  InvalidCastException: unboxing demands the *exact* boxed type, not a
  convertible one. Crash-cousin of pattern-matching's boxed-five-is-not-five
  (same box, silent miss there, loud throw here) - coordinate, don't
  duplicate.

- **boxed-values-are-equal-not-same** (A2) - box the same int twice and
  Equals says equal while ReferenceEquals says no - each box is a fresh heap
  object, so identity-based caching or locking on boxed values treats one
  value as many.
