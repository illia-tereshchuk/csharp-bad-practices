# 📄 serialization

> Status: **opened**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### tuples-serialize-to-nothing (A5)

- **Twist:** System.Text.Json serializes properties; tuples are all fields -
  so your (id, total) goes over the wire as {} and comes back as zeros, with
  no error in either direction.
- **Mechanic:** STJ ignores public fields unless `IncludeFields = true`.
  `ValueTuple`'s Item1/Item2 are fields, so a tuple serializes to `{}`. The
  friendly names (`(int id, decimal total)`) are compiler fiction that never
  exists at runtime, so even with IncludeFields you get Item1/Item2, never
  your names. Deserializing `{}` into a struct yields all defaults - #0012's
  tolerant reading completes the silent round trip.
- **Who hits it:** quick internal APIs, cache layers, queue messages where
  someone returns a tuple "for now"; plus older DTOs using public fields
  instead of properties - same rule, same empty object.
- **Repro:** `JsonSerializer.Serialize((1042, 149.99m)) == "{}"` - one line.
  Needs `#:property PublishAot=false`. Deterministic.
- **Damage:** order id 0, amount 0.00, HTTP 200 everywhere; data loss with
  every status green.
- **😈 seed:** `IncludeFields = true` "fixes" it into
  `{"Item1":1042,"Item2":149.99}` - the data survives but the contract is
  still garbage, and every consumer now binds to Item1/Item2 forever.
- **Verified:** ran on .NET 10 (2026-07-22): Serialize((1, "a")) == "{}".

### the-renumbered-status (A2,5)

- **Twist:** STJ writes enums as bare numbers; insert one member and
  yesterday's stored "Cancelled" deserializes as today's "Shipped" - every
  archived record silently rewrites its own history.
- **Mechanic:** default enum serialization is the underlying integer - a
  *positional* identity. Reordering members, inserting one, or alphabetizing
  the file re-maps every number to whichever member now wears it. No error is
  possible: any integer deserializes into an enum, defined or not. The bug
  spans deploys, which is why no single-version test can ever catch it.
- **Who hits it:** anyone persisting JSON - documents in a DB, messages in a
  queue, cached API responses - across more than one release of the code.
- **Repro:** simulate two deploys in one file: serialize
  `StatusV1 { Pending=0, Shipped=1, Cancelled=2 }`, deserialize the same
  string as `StatusV2 { Pending=0, OnHold=1, Shipped=2, Cancelled=3 }`
  (someone inserted OnHold) - stored Cancelled(2) now reads as Shipped.
  Needs `#:property PublishAot=false`. Deterministic.
- **Damage:** cancelled orders start shipping; the audit trail stays
  internally consistent and entirely wrong - textbook silent data
  corruption with money stakes.
- **😈 seed:** `JsonStringEnumConverter` protects new writes but cannot fix
  the numbers already stored - by the time the bug is noticed, the corruption
  is baked into the archive.
- **Verified:** ran on .NET 10 (2026-07-22): V1.Cancelled round-tripped into
  V2.Shipped.

## Seeds

Not yet a full candidate - brainstorm before proposing.

- **json-cycle-throws** (A5) - a parent referencing children that reference
  the parent serializes fine right up until JsonException at runtime: the
  default serializer has no cycle handling.

- **json-case-sensitive-by-default** (A4,5) - System.Text.Json matches
  property names case-sensitively (Newtonsoft did not); one `"userId"` vs
  `"UserId"` and the field stays default with nothing logged - a migration
  that "changed only the library" drops data.
