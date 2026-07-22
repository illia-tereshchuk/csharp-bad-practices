# 🔒 security

> Status: **planned**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### interpolated-injection (A4)

- **Twist:** The same `$"...{name}..."` is parameterized SQL in
  FromSqlInterpolated and an injection in FromSqlRaw - identical syntax,
  opposite fate, and the compiler happily allows both.
- **Mechanic:** FromSqlInterpolated receives FormattableString and turns
  each hole into a DbParameter; FromSqlRaw receives the already-formatted
  string, holes pre-concatenated. An interpolated string converts to string
  implicitly, so passing it to the Raw overload compiles without a hint.
- **Who hits it:** EF Core raw-SQL users; reviewers cannot tell safe from
  unsafe without reading the method name, which is exactly one word
  different.
- **Repro:** SQLite EF setup as #0008; a name input of `' OR '1'='1` -
  Interpolated returns one row, Raw returns the whole table. Deterministic,
  self-contained database.
- **Damage:** SQL injection - the museum's clearest security stakes, and the
  Bad/Good diff is a single method name.
- **Verified:** documented EF API design; verify at build with #0008 setup.

### guessable-random (A6)

- **Twist:** `Random` for password-reset tokens: same seed, same "random"
  token - and the seed is guessable, so the attacker doesn't break your
  crypto, they just run your line of code.
- **Mechanic:** System.Random is a deterministic PRNG: the seed fully
  determines the sequence, and its state is recoverable from observed
  outputs. The legacy pattern `new Random(someTimeValue)` narrows the seed
  to a searchable window. RandomNumberGenerator is the unpredictable one -
  the class name is the entire fix.
- **Who hits it:** reset tokens, invite codes, temporary passwords, CSRF-ish
  nonces built with `Random` because it was already imported.
- **Repro:** two `new Random(seed)` with the same seed generate identical
  "tokens" - frame one as the server, one as the attacker who guessed the
  seed. Deterministic by construction, no packages. (Do not claim
  parameterless `new Random()` is time-seeded on modern .NET - it isn't;
  the exhibit is about Random being deterministic and non-cryptographic,
  and about the legacy explicit-seed pattern.)
- **Damage:** account takeover via predicted reset token - maximal stakes,
  minimal code.
- **Verified:** PRNG determinism by definition; the honesty note about
  modern seeding recorded so the README doesn't overclaim. Verify at build.
