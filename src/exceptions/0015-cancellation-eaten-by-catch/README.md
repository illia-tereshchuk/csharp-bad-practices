---
id: "0015"
title: A catch-all that swallows cancellation
category: exceptions
tags: [cancellation, exceptions, retry]
rule: "never let a catch-all eat OperationCanceledException"
---

# #0015 - A Catch-All That Swallows Cancellation

## 💥 Symptom

A user cancels a long export - and the monitoring lights up: five failed
attempts, an error alert, the job marked "failed" in the dashboard. At 3 AM
someone investigates a "crash" that was actually a person clicking Cancel.
In busier systems the retry policy turns every cancellation into a small
error storm, and the on-call rotation slowly learns to ignore alerts.

## 🔍 The Offending Code

```csharp
for (int attempt = 1; attempt <= MaxRetries; attempt++)
{
    try
    {
        await ExportReport(token);
        break;
    }
    catch (Exception ex) // cancellation lands here too
    {
        logger.Warn($"attempt {attempt} failed, retrying");
    }
}
```

## 🧠 What's Actually Going On

Cancellation in .NET is **cooperative**, and it travels up the call stack
as an exception - `OperationCanceledException` - because unwinding the
stack is exactly what an exception is for. It's a control-flow signal
wearing an exception costume.

`catch (Exception)` can't tell the signal from a genuine failure, so the
retry wrapper "handles" it: logs an error, tries again - **against a token
that is still cancelled**. Every retry dies instantly on the first
`ThrowIfCancellationRequested`, which is why the exhibit's log shows one
real attempt and four zero-millisecond ones. The user asked for silence
and got five alarms.

Note the runtime itself respects the difference: a task that throws an OCE
for its own token ends up `Canceled`, not `Faulted`. It's only the
catch-all that flattens the two into one.

## ✅ The Fix

Let cancellation pass through; retry only real failures:

```csharp
catch (OperationCanceledException)
{
    // a signal, not a failure: stop, don't retry
    break;
}
catch (Exception ex)
{
    logger.Warn($"attempt {attempt} failed, retrying");
}
```

Full version in [Good.cs](Good.cs). The toolbox:

| Option | When it's the right call |
|---|---|
| Dedicated `catch (OperationCanceledException)` first | Retry wrappers and workers - stop cleanly, report "cancelled" |
| `catch (Exception ex) when (ex is not OperationCanceledException)` | One catch block that *declares* it handles errors only |
| `when (token.IsCancellationRequested)` on the OCE catch | Distinguish real cancellation from OCE-shaped timeouts (see below) |

## 😈 The Even Worse Sibling

The same catch-all *inside* the operation, with no rethrow: cancellation
is swallowed mid-work, the method returns normally, and the half-finished
export is marked **delivered**. The failure mode upgrades from "noisy
false alarm" to "silent partial success" - a cancelled invoice run that
posts half the invoices and reports green.

## 🎓 Advanced Nuance

Not every `OperationCanceledException` is a cancellation you asked for:
`HttpClient` reports a **timeout** as `TaskCanceledException` too. A catch
that treats every OCE as "user cancelled" will classify network timeouts
as intentional stops - the opposite misdiagnosis of this exhibit. The
telling filter is `when (token.IsCancellationRequested)`: your token
cancelled means a real cancel; OCE without it means something timed out.

## 🔎 How to Find It in Your Codebase

- Every `catch (Exception)` inside a method that takes a
  `CancellationToken` - the token in the signature is the tell.
- Retry policies (Polly and hand-rolled) that don't exclude OCE from
  "transient failures".
- Dashboards where "failed jobs" spike together with user cancellations -
  correlate the two once and you'll find this exhibit.
