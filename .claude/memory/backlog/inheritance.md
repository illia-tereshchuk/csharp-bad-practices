# 🪆 inheritance

> Status: **planned**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### virtual-call-in-constructor (A1)

- **Twist:** The base constructor calls a virtual method that runs on the
  derived class *before the derived constructor body has run* - the override
  reads its own fields and finds nulls.
- **Mechanic:** construction order in C#: derived *field initializers* run
  first, then the base constructor, then the derived constructor *body*. A
  virtual call from the base constructor dispatches to the derived override
  (no "partial" dispatch exists), which executes against an object whose
  constructor-body assignments have not happened yet. BUILDER WARNING: get
  the order right in the README - fields set via *initializers* ARE visible
  (C# differs from Java here); only constructor-*body* state is missing.
- **Who hits it:** template-method base classes ("call Initialize() in the
  base ctor, let derived classes override it") - a design that looks like
  good OO and is a construction-order trap.
- **Repro:** derived class assigning a field in its constructor body; base
  constructor calls the virtual; the override NREs (or, nastier, computes
  with the default). Deterministic, no packages.
- **Damage:** NRE at construction in the loud version; in the quiet version
  the override caches a decision computed from default values, and the
  object is subtly misconfigured for its whole lifetime.
- **Verified:** language-specified construction order; verify at build,
  including the initializer-vs-body distinction.
