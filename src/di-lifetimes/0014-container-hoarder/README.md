---
id: "0014"
title: Transient disposables resolved from the root container
category: di-lifetimes
tags: [dependency-injection, IDisposable, lifetimes, memory-leak]
rule: "never resolve transient disposables from the root container"
---

# #0014 - Transient Disposables Resolved From the Root Container

## 💥 Symptom

Memory grows linearly with traffic and goes flat only after a restart. The
dump is full of "short-lived" per-request objects that finished their work
hours ago. No static events, no obvious roots - the retention paths all
lead into the DI container itself. And a strange detail in the logs: not a
single `Dispose` during the day, then thousands of them in one avalanche
at shutdown.

## 🔍 The Offending Code

```csharp
using var root = services.BuildServiceProvider();

// somewhere in a background worker, per request:
var buffer = root.GetRequiredService<ReportBuffer>(); // transient + IDisposable
buffer.Render(request);
// done with it. so we think
```

## 🧠 What's Actually Going On

The container **owns** every disposable it creates - it must call
`Dispose` when the lifetime ends. For a transient, "when the lifetime
ends" means *when the provider that resolved it is disposed*. Resolve from
the root provider, and that moment is application shutdown.

So on every resolve the root container appends the new buffer to its
internal disposables list - a strong reference. That list is why the GC
can't collect a single one of the 100 buffers, and why `Dispose` runs zero
times during the run and 100 times at exit. **"Transient" describes how
often objects are created, not when they are released.** The leak isn't an
accident; it's bookkeeping working exactly as designed, pointed at the
wrong lifetime.

## ✅ The Fix

Give every unit of work its own scope - the request boundary becomes a
`using` block:

```csharp
using var scope = root.CreateScope();
var buffer = scope.ServiceProvider.GetRequiredService<ReportBuffer>();
buffer.Render(request);
// scope disposed: Dispose runs NOW, the reference is released NOW
```

Full version in [Good.cs](Good.cs). The toolbox:

| Option | When it's the right call |
|---|---|
| `CreateScope()` per unit of work | Background workers, consumers, console jobs - anywhere without a framework scope |
| Constructor injection inside a framework scope | In ASP.NET Core the request scope already exists - take dependencies, don't resolve them |
| Rethink the lifetime | If every request needs the same heavy resource, it's a singleton or a pool, not a transient |

## 😈 The Even Worse Sibling

Inject `IServiceProvider` into a singleton and resolve transients inside
it "on demand". Same hoard, now invisible: the registration list looks
innocent, every review passes, and the service-locator call sits three
layers deep. The container the singleton captured is the root one - every
disposable it hands out is pinned forever, and no registration audit will
ever show it.

## 🎓 Advanced Nuance

Two twists worth knowing. First: `BuildServiceProvider(validateScopes: true)`
catches scoped-from-root - but **not** this. Transients from the root are
perfectly legal, so the hoard survives even in teams with validation
switched on. Second, the scarier one: the container only tracks
disposables. Add `IDisposable` to a previously plain class during a
refactor, change nothing else - and every root-resolved transient of that
type across the codebase silently becomes a leak. A one-line interface
addition with repo-wide memory consequences.

## 🔎 How to Find It in Your Codebase

- Grep `GetRequiredService` / `GetService` outside of a `CreateScope`
  block - especially on an injected or stored root `IServiceProvider`.
- In a memory profiler, look at the retention path: root
  `ServiceProvider` -> internal disposables list -> your objects. That
  signature is this exhibit.
- The WeakReference + forced-GC pattern from this exhibit works as a
  regression test for any "does my container release this?" question.
