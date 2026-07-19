---
id: "0021"
title: await Task.WhenAll shows only the first fault
category: async
tags: [async, Task.WhenAll, AggregateException]
rule: "never trust await Task.WhenAll to report more than one failure"
---

# #0021 - await Task.WhenAll Shows Only the First Fault

## 💥 Symptom

A fan-out job ships an order to every warehouse at once. Three of them
fail; the handler logs one error, retries that one warehouse, and reports
the batch as "one failure, handled". Two orders were never shipped and
nobody knows. The reconciliation gap shows up days later, and the logs
contain exactly one of the three errors that actually happened - so the
investigation starts one-third informed.

## 🔍 The Offending Code

```csharp
try
{
    await Task.WhenAll(shipments); // three of these throw
}
catch (Exception ex)
{
    logger.Error(ex);   // logs one. the other two are gone from here
}
```

## 🧠 What's Actually Going On

`Task.WhenAll` faithfully collects *all* the failures - the task it
returns holds an `AggregateException` with every fault inside
`.InnerExceptions`. The loss happens at `await`.

`await` is designed to make async code read like sync code, so it unwraps
the aggregate and rethrows a **single** exception - the first one. That's
the right call for the common case (you rarely want to catch an
`AggregateException` by hand), but on `WhenAll` it means the awaited throw
carries one fault and drops the rest on the floor of your `catch`. The
full set is still there, on the task object you awaited - you just threw
away the reference to it by writing `await Task.WhenAll(...)` inline.

Contrast the old `.Wait()` / `.Result`: those throw the raw
`AggregateException`, all faults visible but wrapped. `await` trades
completeness for ergonomics, and the trade is invisible until more than
one task fails at once.

## ✅ The Fix

Keep the task, and read every fault off it instead of trusting the single
rethrow:

```csharp
var all = Task.WhenAll(shipments);
try { await all; }
catch
{
    foreach (var t in shipments.Where(s => s.IsFaulted))
        foreach (var ex in t.Exception!.InnerExceptions)
            Report(ex);
}
```

Full version in [Good.cs](Good.cs). The toolbox:

| Option | When it's the right call |
|---|---|
| Inspect each task's `.Exception` after WhenAll | You need every failure - the default for fan-out |
| Hold the WhenAll task, read `.Exception.InnerExceptions` | Same idea, one aggregate instead of per-task |
| Give each task its own try/catch inside | You want a result *and* an error per item, not all-or-nothing |

## 😈 The Even Worse Sibling

Nobody awaits it at all - `_ = Task.WhenAll(...)` or a forgotten task
(exhibit [0019-forgotten-task](../../async/0019-forgotten-task/)). Now *all* the faults vanish, not just the extras, and the
`AggregateException` becomes an unobserved-task exception that at best
surfaces at some later GC. From "saw one of three" down to "saw none of
three" - the same API, one missing `await` worse.

## 🎓 Advanced Nuance

`Task.WhenAll` also flattens **cancellations** into the mix: if some tasks
are canceled and others faulted, the single rethrown exception might be an
`OperationCanceledException` while real errors hide behind it - the
misdiagnosis of exhibit [0015-cancellation-eaten-by-catch](../../exceptions/0015-cancellation-eaten-by-catch/), now nondeterministic about which one you
see. And `WhenAll<TResult>` has a quiet twist: on failure its result array
is unavailable, but tasks that *did* succeed still ran their side effects -
a partial completion wearing the mask of total failure.

## 🔎 How to Find It in Your Codebase

- Every `await Task.WhenAll(...)` inside a `try` - ask whether the `catch`
  needs all failures or is happy with one. Fan-out that reports status
  needs all.
- `catch (Exception)` right after a WhenAll is the tell: it can structurally
  see only the first fault.
- Test a batch where *two or more* items fail at once - suites that fail
  one at a time never reveal this.
