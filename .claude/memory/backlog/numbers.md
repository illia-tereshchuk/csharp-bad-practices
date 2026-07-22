# 🔢 numbers

> Status: **opened**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### remainder-is-not-modulo (A4)

- **Twist:** `%` is remainder, not modulo: a negative hash `% 10` is a
  negative bucket index. And the obvious fix, Math.Abs, throws on
  int.MinValue - the axiom is wrong twice.
- **Mechanic:** C# `%` keeps the sign of the dividend: `-7 % 3 == -1`, never
  2. `GetHashCode()` legitimately returns negatives for about half of all
  values, so `hash % buckets` is negative about half the time.
  `Math.Abs(int.MinValue)` throws OverflowException because +2147483648 does
  not fit in int. The correct form is `(int)((uint)hash % (uint)n)`.
- **Who hits it:** hand-rolled sharding and partitioning - "pick a
  queue/shard/bucket by key.GetHashCode() % N". Works for every key the dev
  tried, crashes (or mis-shards) on the first negative hash in production.
- **Repro:** IMPORTANT for the builder: do NOT use string hashes in the demo -
  string hashing is randomized per process, which would make the demo
  nondeterministic. Use keys whose hash you control: an int id (an int is its
  own hash, so a negative customer id like -12345 gives `-12345 % 10 == -5`)
  or a type with a hardcoded GetHashCode. Index an array with the result:
  IndexOutOfRangeException. No packages.
- **Damage:** crash on the first negative key; with the sloppier "fix"
  (`Math.Abs` or re-hashing), keys silently land in a different shard than
  the one that already holds their data.
- **😈 seed:** `Math.Abs(int.MinValue)` throws - and int.MinValue is a hash
  real values actually have. The crash hides for years behind its 1-in-4-billion
  trigger.
- **Verified:** ran on .NET 10 (2026-07-22): -7%3 == -1, -12345%10 == -5,
  Abs(int.MinValue) threw OverflowException.

## Seeds

Not yet a full candidate - brainstorm before proposing.

- **double-to-decimal-carries-error** (A4) - reading a price as `double` and
  then casting to `decimal` freezes the binary rounding error into the money
  type - the `decimal` is exact about a number that was already wrong.
