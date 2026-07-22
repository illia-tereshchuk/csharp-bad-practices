# 🪞 reflection

> Status: **planned**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### setvalue-into-the-void (A3)

- **Twist:** PropertyInfo.SetValue on a struct writes into the box reflection
  just created and throws it away - your variable never changes, and no API
  anywhere reports that the write went nowhere.
- **Mechanic:** SetValue takes `object`: passing a struct variable boxes a
  copy; the setter runs against the box; the box is discarded. Classes work
  fine through the same code path, so the mapper "works" until the first
  struct DTO. (The fix that keeps structs: box once explicitly, SetValue
  into that box, unbox at the end.)
- **Who hits it:** hand-rolled mappers, config binders, test data builders -
  reflective property-setting loops written for classes that one day meet a
  struct.
- **Repro:** struct with an auto-property; GetProperty + SetValue; the
  variable still holds the old value. Deterministic, no packages.
- **Damage:** every reflected write silently no-ops: settings objects full
  of defaults, mapped DTOs half-empty - and only for the struct-typed ones,
  which makes the pattern look haunted.
- **Verified:** ran on .NET 10 (2026-07-22): SetValue on the boxed copy,
  variable unchanged.

## Seeds

Not yet a full candidate - brainstorm before proposing.

- **gettype-is-exact-not-assignable** (A4,5) -
  `obj.GetType() == typeof(Base)` is false for a derived instance, so a
  type-keyed handler dictionary misses every subclass and falls through to
  the default.

- **getproperty-misses-nonpublic** (A5) - `Type.GetProperty("x")` returns
  null for non-public or static members because the default binding flags
  exclude them - the reflective mapper skips fields it was written to copy.

- **activator-needs-parameterless-ctor** (A5) -
  `Activator.CreateInstance<T>()` compiles for any T but throws
  MissingMethodException the moment T lacks a public parameterless
  constructor.
