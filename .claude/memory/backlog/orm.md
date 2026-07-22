# 🗄 orm

> Status: **opened**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### stale-tracked-entity (A5)

- **Twist:** The change tracker returns the entity it cached earlier: your
  fresh query runs real SQL, fetches fresh rows - and hands you back the old
  object with the old values.
- **Mechanic:** EF Core's identity map guarantees one instance per key per
  DbContext. When a query materializes a row whose key is already tracked,
  EF discards the just-fetched scalar values and returns the tracked
  instance unchanged. The SELECT visibly runs; its results are thrown away.
- **Who hits it:** long-lived contexts - background jobs, desktop apps,
  captive contexts (#0022) - re-reading "current" data that another process
  updated in between.
- **Repro:** two DbContexts over one SQLite database file: context A loads
  the entity; context B updates the row and saves; context A queries again
  and still sees the old value; `Entry(...).Reload()` or a fresh context sees
  the new one. Packages and setup as in #0008 (`Microsoft.EntityFrameworkCore.Sqlite`,
  `SQLitePCLRaw.bundle_e_sqlite3`, `#:property PublishAot=false`).
  Deterministic.
- **Damage:** decisions (price checks, stock checks, permission checks) made
  against yesterday's values while the SQL log shows the fresh SELECT that
  "fetched" them - an audit trail that actively lies.
- **😈 seed:** nothing short of Reload or a new context fixes it - the same
  context that lied to you also reports the entity as Unchanged.
- **Verified:** documented identity-map behavior; verify at build with the
  #0008 setup.

### untranslatable-where (A4)

- **Twist:** Extract a predicate into a helper method - the refactor every
  reviewer approves - and the query that compiled and passed every unit test
  throws at runtime: EF cannot translate your method to SQL.
- **Mechanic:** EF Core builds SQL from expression trees; a call to your own
  method inside `Where` has no translation, and since EF Core 3 the query
  throws InvalidOperationException ("could not be translated") instead of
  silently downloading the table. The same predicate written inline
  translates fine - the difference is invisible in the code's meaning, only
  in its shape.
- **Who hits it:** everyone who refactors shared predicates ("IsActive(c)")
  out of queries. Compiles; green against in-memory lists; explodes on the
  first real database query.
- **Repro:** SQLite EF setup as #0008; `.Where(c => IsVip(c))` throws; the
  same expression inlined returns rows. Deterministic.
- **Damage:** honest crash, but at runtime in production, in a query the
  type system and the test suite both blessed. The exhibit's lesson is the
  displaced failure point.
- **😈 seed:** the pre-3.0 behavior was *silent* client evaluation - and it
  still exists today: insert `.AsEnumerable()` before the Where and the
  "fix" quietly downloads the entire table to filter it in memory.
- **Verified:** documented EF Core 3+ behavior; verify at build.
