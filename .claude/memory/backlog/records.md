# 📇 records

> Status: **opened**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### record-equality-skin-deep (A4,5)

- **Twist:** Record value equality is one property deep: add a List and two
  identical-looking records stop being equal, Distinct stops deduplicating,
  and dictionary lookups quietly miss.
- **Mechanic:** record equality compares each member with
  `EqualityComparer<T>.Default`; for List/array/Dictionary members that is
  *reference* equality. GetHashCode composes the same way, so hashes differ
  too and everything stays self-consistent - wrong, but never inconsistent
  enough to throw.
- **Who hits it:** records as DTOs and value objects with a `List<string>
  Tags` or `Items` array: test assertions, Distinct, records as cache keys.
- **Repro:** `record Order(int Id, List<string> Tags)`; two same-content
  instances: `!=` is true, `Distinct()` keeps both, a Dictionary keyed by
  one misses the other, hashes differ. Deterministic, no packages.
- **Damage:** cache keyed by records: 100% miss rate (a cost bug that looks
  like a traffic increase); test assertions that fail on equal data - or
  pass on unequal data once someone "fixes" the test with reference reuse.
- **😈 seed:** cross-link #0028: `with` copies the *reference*, so the two
  records that ARE equal share one list - mutate one, both change.
  Equal-when-they-shouldn't-be and unequal-when-they-should-be, from the
  same design gap.
- **Verified:** ran on .NET 10 (2026-07-22): records unequal, hashes differ.

## Seeds

Not yet a full candidate - brainstorm before proposing.

- **record-struct-is-mutable** (A3,4) - `record struct Point(int X, int Y)`
  has settable properties by default - the immutability people assume from
  "record" applies to record *classes* only.

- **with-skips-validation** (A5) - validation in a record's constructor body
  does not run on `with`: the copy uses the compiler's copy constructor, so
  an "impossible" invalid state is one `with { ... }` away.

- **record-tostring-leaks-fields** (A5) - a record's generated ToString
  prints every property, so the moment a Password or Token member joins the
  record it shows up verbatim in every log line that interpolates the object.
