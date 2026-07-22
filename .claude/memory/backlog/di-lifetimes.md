# 💉 di-lifetimes

> Status: **opened**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### the-silent-override (A4)

- **Twist:** Two registrations of one interface are both legal; the last one
  wins silently - the handler you spent an hour debugging never resolved at
  all.
- **Mechanic:** MS.DI accepts duplicate registrations: single resolution
  (`GetRequiredService<T>`) returns the *last* registration;
  `GetServices<T>` returns all of them. Which implementation runs is decided
  by registration order across Program.cs and every AddXyz extension - a
  global, invisible, order-sensitive contract.
- **Who hits it:** two teams' extension methods both registering
  IEmailSender; or a test override that stops overriding after an innocent
  reorder of builder calls.
- **Repro:** register two implementations of one interface; resolve - only
  the last runs; swap the two registration lines - behavior flips with a diff
  that looks like formatting. `#:package Microsoft.Extensions.DependencyInjection@10.*`,
  `#:property PublishAot=false`. Deterministic.
- **Damage:** production behavior decided by call order in composition code,
  ungreppable from any use site; debugging happens in an implementation that
  is never actually invoked.
- **😈 seed:** `TryAdd*` exists precisely for this and *inverts* the rule -
  first wins. Two idioms, opposite winners, both silent.
- **Verified:** documented container behavior; verify at build.

### the-twin-singletons (A4)

- **Twist:** One class registered under two interfaces yields two
  "singletons" - the state you thought was shared is quietly split in half.
- **Mechanic:** `AddSingleton<IReader, Store>()` plus
  `AddSingleton<IWriter, Store>()` creates one instance *per registration*,
  not per class: "singleton" binds to the service type, not the
  implementation. The fix is registering the concrete once and forwarding:
  `AddSingleton<Store>(); AddSingleton<IReader>(sp => sp.GetRequiredService<Store>()); ...`
- **Who hits it:** the reader/writer interface split: a class implementing
  ICache + ICacheInvalidator, registered under both. Invalidation goes to
  one instance; reads come from the other.
- **Repro:** resolve both interfaces, `ReferenceEquals` is false; write
  through one, read through the other, the write is invisible. DI package +
  `#:property PublishAot=false`. Deterministic.
- **Damage:** caches that never invalidate, metrics split across two
  collectors - each half internally consistent, jointly wrong, and the class
  in question really is instantiated "once"... per interface.
- **Verified:** ran on .NET 10 (2026-07-22): two registrations produced two
  instances (ReferenceEquals false).

## Seeds

Not yet a full candidate - brainstorm before proposing.

- **scoped-from-root-lives-forever** (A6,5) - resolving a scoped service
  straight from the root IServiceProvider gives it singleton lifetime by
  accident: never disposed, per-request state leaking across requests.
  Mirror image of shipped #0022 (the-captive-scoped) - check its README/😈
  overlap before promoting.
