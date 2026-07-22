# 🧬 generics

> Status: **planned**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### static-field-per-closed-type (A6)

- **Twist:** A static field in `Cache<T>` is not one field - it is one field
  *per T*, and the "global" cache silently shards itself by type argument.
- **Mechanic:** statics live on the closed constructed type: `Cache<int>` and
  `Cache<string>` each get their own copy. A limit, pool, or registry in a
  generic base class multiplies invisibly.
- **Who hits it:** generic base classes with static counters/caches/config -
  `Repository<T>.ConnectionCount` - where the author meant one number for
  the process.
- **Repro:** increment the "shared" static through two type arguments; print
  both copies diverging. Deterministic, no packages.
- **Damage:** connection limits that don't limit, singletons that aren't
  single, caches that miss because the entry went into a sibling.
- **Verified:** CLR-specified behavior; verify at build.
