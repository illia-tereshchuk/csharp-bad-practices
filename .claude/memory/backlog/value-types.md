# 📦 value-types

> Status: **opened**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### the-vanishing-mutation (A3)

- **Twist:** Mutating a struct taken from a List edits a temporary copy; the
  identical line against an array works fine - so the collection is the last
  thing anyone suspects.
- **Mechanic:** `list[i]` calls the indexer, which *returns a copy* of the
  struct; `arr[i]` is direct storage access. BUILDER WARNING: the assignment
  form `list[i].X = 5` does not even compile (CS1612) - the compiler blocks
  the obvious spelling. The trap that ships is the method form:
  `list[i].Translate(5)` compiles without a whisper and mutates the copy. So
  Bad.cs must use a mutating *method* (or a `var tmp = list[i]; tmp.X = 5;`
  sequence), not direct member assignment.
- **Who hits it:** structs in Lists - points, money amounts, game entities.
  The array version worked yesterday; today someone changed `T[]` to
  `List<T>` in one place and every mutation became a no-op.
- **Repro:** same mutating method called on `arr[i]` (works) and `list[i]`
  (silently does nothing); print both. Deterministic, no packages.
- **Damage:** updates that no-op silently - balances never change, positions
  never move - while the identical code elsewhere (arrays) works, actively
  pointing the investigation away from the cause.
- **😈 seed:** `foreach` over a List of structs hands out copies too - the
  "fix everything in a loop" pass fixes nothing.
- **Verified:** CS1612 vs method-call nuance is language-specified; verify at
  build (the CS1612 note is load-bearing for Bad.cs).

### the-skipped-initializer (A4)

- **Twist:** Struct field initializers run for `new S()` but not for `default`
  or array elements - the same struct is born with different values depending
  on who created it.
- **Mechanic:** field initializers on a struct execute only as part of a
  constructor call. `new S()` invokes the parameterless constructor, so
  initializers run; `default(S)` and `new S[n]` just zero memory - no
  constructor, no initializers. BUILDER WARNING: a struct with field
  initializers and no declared constructor does not compile (CS8983), so the
  demo struct must declare `public S() { }`.
- **Who hits it:** structs given "sensible defaults" via initializers
  (`Rate = 1.0m`, `Enabled = true`) then materialized through arrays, `out`
  parameters, or `default` - every such instance carries zeros and falses
  where the author promised 1.0 and true.
- **Repro:** `struct WithInit { public decimal Rate = 1.5m; public WithInit() {} }`;
  print `new WithInit().Rate` (1.5), `default(WithInit).Rate` (0), and
  `new WithInit[1][0].Rate` (0). Three lines, deterministic, no packages.
- **Damage:** a multiplier that "defaults to 1" is 0 in every array-born
  instance: totals multiply to zero - silent money-math corruption.
- **😈 seed:** `Enabled = true` flips to false the same way - a permission or
  feature silently defaults OFF only on the code path that used an array.
- **Verified:** ran on .NET 10 (2026-07-22): 1.5 / 0 / 0 exactly as above.
