---
id: "0005"
title: Rethrowing with throw ex
category: exceptions
level: 🟡
tags: [exceptions, stack-trace, CA2200]
summary: "`throw ex` wipes the stack trace - the investigation starts at the wrong line"
---

# #0005 - Rethrowing with `throw ex`

## 💥 Symptom

Production incident, 3 AM. The stack trace in the logs points at a logging
helper in the middle of the payment pipeline. The on-call engineer spends an
hour reading a perfectly innocent method, because the *actual* crime - three
calls deeper - is not in the trace. It was, once. Someone erased it while
"just passing the exception along".

## 🔍 The Offending Code

```csharp
try
{
    ChargeCard();
}
catch (Exception ex)
{
    logger.Warn($"payment failed: {ex.Message}");
    throw ex; // 💥 the stack trace resets right here
}
```

The exhibit prints the trace and then checks it for the origin method -
in `Bad.cs` the check fails: `ValidateCard` is gone from its own crime scene.

## 🧠 What's Actually Going On

An exception object carries its stack trace in a field, and that field is
written at the moment the exception is **thrown** - not when it's created.
`throw ex;` is a brand-new throw of an existing object, so the runtime
overwrites the field with the current location. Everything below the catch
block vanishes.

`throw;` (no variable) compiles to a different IL instruction - `rethrow` -
which means "let the same exception continue on its way". The original
frames survive.

Compare the two runs:

```text
Bad.cs                                Good.cs
   at ProcessPayment()  ← rethrow        at ValidateCard()   ← the real crime
   at Main()                             at ChargeCard()
                                         at ProcessPayment()
                                         at Main()
```

## ✅ The Fix

One keyword, zero characters of variable name:

```csharp
catch (Exception ex)
{
    logger.Warn($"payment failed: {ex.Message}");
    throw; // the same exception continues, trace intact
}
```

Full version in [Good.cs](Good.cs). The wider toolbox:

| Option | When it's the right call |
|---|---|
| `throw;` | Log-and-propagate. The default |
| `throw new PaymentException("charge #42 failed", ex)` | Add context; the original rides along as `InnerException` |
| `ExceptionDispatchInfo.Capture(ex).Throw()` | Rethrow *outside* the catch block - deferred or marshalled to another thread |

## 😈 The Even Worse Sibling

```csharp
catch (Exception ex)
{
    throw new Exception(ex.Message); // type gone, trace gone, InnerException gone
}
```

This one destroys everything at once: the exception type (goodbye
`catch (SqlException)` upstream), the trace, *and* the inner exception.
All that survives is a string. `throw ex` at least leaves the type alive.

## 🎓 Senior Nuance

Even the virtuous `throw;` rewrites one thing: the line number of the frame
that contains the catch block - it will show the rethrow line, not the
original call line within that method. Deeper frames stay intact. And the
reason `await` doesn't suffer from any of this: the async machinery rethrows
exceptions via `ExceptionDispatchInfo`, which is exactly why traces survive
crossing threads.

## 🔎 How to Find It in Your Codebase

- The compiler already tells you: **CA2200** ("Re-throwing caught exception
  changes stack information") fires on every `throw ex;` - it even fired
  while building this exhibit. Promote it from warning to error in
  `.editorconfig`: `dotnet_diagnostic.CA2200.severity = error`.
- Grep candidates: `throw ex;`, `throw e;`, `throw exception;`.
- In review: any catch block that mentions its own variable after `throw`.

## 📚 Dig Deeper

- [CA2200 - Microsoft Learn](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2200)
- [ExceptionDispatchInfo - Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.runtime.exceptionservices.exceptiondispatchinfo)
