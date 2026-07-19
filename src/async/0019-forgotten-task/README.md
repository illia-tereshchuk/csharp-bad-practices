---
id: "0019"
title: The fire-and-forgotten task
category: async
tags: [async, fire-and-forget, CS4014]
rule: "never drop a Task - await it or hand it to someone who will"
---

# #0019 - The Fire-and-Forgotten Task

## 💥 Symptom

The monthly reconciliation finds holes: orders without audit records,
signups without welcome emails, payments without receipts. No errors in
the logs - not one, ever. The gaps correlate with nothing: not load, not
deploys, not time of day. It's the quietest class of incident that
exists: nothing crashed, nothing logged, the data is simply *absent*.

## 🔍 The Offending Code

```csharp
async Task ProcessOrder(string orderId)
{
    await ChargeCard(orderId);
    SaveAuditRecord(orderId); // "audit must not slow the request"
}
```

## 🧠 What's Actually Going On

Calling an async method *starts* the work and immediately hands back a
`Task`. Drop that task and two things follow. First: when the work fails,
the exception is stored **inside the task object**, waiting for an
observer - an `await`, a `.Wait()`, anyone. No observer ever comes, so
the failure is never seen by anything. Second: nothing ties the work to
the caller's lifetime - the process can exit mid-write (this exhibit
grants a 300 ms grace period precisely to prove the silence is not about
timing).

Compare the family: `async void` (exhibit [0007-async-void](../../async/0007-async-void/)) has no task at all, so
its exception detonates on the thread pool and *kills the process* -
loud. The forgotten task is the same negligence one rung further down
the fear ladder: same lost work, and even the crash is gone. The
invariant this exhibit audits is worth pinning above any team's door:
**either the record exists, or someone was told it doesn't.** Bad.cs
violates both halves at once.

## ✅ The Fix

Observe the task - its failure becomes your failure, which is the point:

```csharp
await SaveAuditRecord(orderId);
```

Full version in [Good.cs](Good.cs) - note that the audit *still fails*
there; the difference is that the pipeline now reports it loudly.
A visible failure is a fixable failure. The toolbox:

| Option | When it's the right call |
|---|---|
| `await` it | The default. Audit that must happen is part of the request |
| Store the tasks, `await Task.WhenAll` later | Fan-out batches - see exhibit [0018-tasks-are-not-results](../../async/0018-tasks-are-not-results/) |
| A `SafeFireAndForget` helper that catches and logs | True fire-and-forget: someone must still observe the faults |

## 😈 The Even Worse Sibling

Fire-and-forget on a web request path. The request ends, the DI scope is
disposed (exhibit [0014-container-hoarder](../../di-lifetimes/0014-container-hoarder/)'s territory), and the forgotten task wakes up to
find its `DbContext` dead - it now fails with `ObjectDisposedException`
*instead of* doing the work, still silently, and only under load, when
the task loses the race against request teardown. A bug that exists only
in production traffic patterns.

## 🎓 Advanced Nuance

The compiler warns - CS4014 fired while building this exhibit - but only
inside `async` methods: route the same call through a synchronous caller
and even the warning disappears. And the idiomatic silencer,
`_ = SaveAuditRecord(orderId);`, is a double lie: it suppresses the
warning *and* tells reviewers "intentional" while still observing
nothing. A discard is only honest when it points at a helper that logs
faults. As for `TaskScheduler.UnobservedTaskException` - it fires on
garbage collection, if subscribed, maybe: it's a coroner, not a doctor.

## 🔎 How to Find It in Your Codebase

- Promote CS4014 to error; add VSTHRD110 ("observe the awaitable
  result") from Microsoft.VisualStudio.Threading.Analyzers.
- Grep `_ = ` discards of task-returning calls - each one must justify
  itself with a fault-logging helper.
- Reconcile independently-counted pairs (orders vs audits, signups vs
  emails) once a week; this bug cannot hide from arithmetic.
