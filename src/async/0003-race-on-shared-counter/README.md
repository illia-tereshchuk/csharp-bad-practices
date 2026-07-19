---
id: "0003"
title: Incrementing a shared counter from parallel threads
category: async
tags: [threading, race-condition, Interlocked]
rule: "never mutate shared state without synchronization"
---

# #0003 - Incrementing a Shared Counter from Parallel Threads

## 💥 Symptom

Two ticket kiosks each registered 100,000 sales, but the end-of-day counter
shows around 110,000 instead of 200,000. No exceptions in the logs. Tomorrow
the number will be wrong by a *different* amount. On the test stand, where one
tester clicks one button, everything adds up perfectly.

## 🔍 The Offending Code

```csharp
void RunKiosk()
{
    for (int i = 0; i < SalesPerKiosk; i++)
    {
        ticketsSold++; // one sale, one increment. What could go wrong?
    }
}

var kioskA = Task.Run(RunKiosk);
var kioskB = Task.Run(RunKiosk);
```

## 🧠 What's Actually Going On

`ticketsSold++` looks like one operation, but the CPU executes three:

1. **read** the current value into a register
2. **add** one to it
3. **write** the result back

Now interleave two threads: both read `41`, both compute `42`, both write
`42` - two sales happened, the counter grew by one. At 200,000 increments,
these collisions happen tens of thousands of times, and every run loses a
different amount. That's the signature of a race: **silent, wrong, and
irreproducible** - the bug disappears exactly when you attach a debugger or
run it slowly on one thread.

## ✅ The Fix

Make the increment atomic - one indivisible CPU instruction instead of three:

```csharp
Interlocked.Increment(ref ticketsSold);
```

Full version in [Good.cs](Good.cs). Picking the right tool:

| Option | When it's the right call |
|---|---|
| `Interlocked.Increment / Add` | A single shared number. The default for counters |
| `lock` | An invariant spans *several* variables or statements |
| No sharing at all | Each thread counts locally, sum at the end - fastest and simplest to reason about |

## 😈 The Even Worse Sibling

"I'll just mark it `volatile`" - and the counter is still wrong. `volatile`
guarantees *visibility* (no stale reads from caches), but not *atomicity*:
read-add-write is still three steps, and threads still interleave between
them. A `volatile` counter with `++` is the same bug wearing a safety vest.

## 🎓 Advanced Nuance

Individual reads and writes of an `int` **are** atomic in .NET - you will
never see a half-written value. That's precisely what makes `++` feel safe:
atomic read + atomic write is still not an atomic read-modify-write. And
`long` on a 32-bit process loses even that guarantee - its reads can tear.

## 🔎 How to Find It in Your Codebase

- Any `++`, `--`, or `+=` on a field that's reachable from `Task.Run`,
  `Parallel.*`, timers, or event handlers.
- Counters and totals that "drift" under load but behave in tests.
- Search for `volatile` used as a race fix - it almost never is one.
