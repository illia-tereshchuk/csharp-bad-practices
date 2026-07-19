---
id: "0017"
title: A finally block that throws
category: exceptions
tags: [exceptions, finally, cleanup]
rule: "never let a finally block throw"
---

# #0017 - A Finally Block That Throws

## 💥 Symptom

Production incident: the logs show a `NullReferenceException` in a cleanup
helper. The team fixes the NRE, closes the ticket, everyone moves on. A
week later the *same* incident happens again - because the NRE was never
the disease, only the bandage tearing. The actual error (a misconfigured
gateway key) existed for exactly one stack frame and was thrown away
before any logger saw it. Sibling of exhibit [0005-throw-ex-stack-amnesia](../../exceptions/0005-throw-ex-stack-amnesia/): there the trace lied
about the *where*; here the exception lies about the *what*.

## 🔍 The Offending Code

```csharp
GatewayConnection? connection = null;
try
{
    connection = GatewayConnection.Open(); // throws: invalid merchant key
    connection.Charge(amount);
}
finally
{
    connection.Close(); // 💥 NRE - and the gateway error is gone
}
```

## 🧠 What's Actually Going On

When the `try` block throws, the exception starts flying up the stack -
but `finally` runs *first*, while the original is still in flight. If the
cleanup throws its own exception, the runtime has two candidates and keeps
exactly one: **the new one**. The original isn't wrapped, isn't attached
as `InnerException`, isn't logged - it simply stops existing.

Here the chain is self-inflicted: `Open()` failed *before* assigning
`connection`, so the cleanup dereferences `null` - the failure in `try` is
precisely what armed the failure in `finally`. That's the typical shape of
this bug in the wild: the cleanup assumes a state that only exists on the
happy path. The compiler even warned: CS8602, "dereference of a possibly
null reference", points at the exact line.

## ✅ The Fix

Cleanup must survive every state the `try` block can leave behind:

```csharp
finally
{
    connection?.Close(); // guards its own assumptions
}
```

Full version in [Good.cs](Good.cs). The toolbox:

| Option | When it's the right call |
|---|---|
| Null-guard the cleanup (`?.`) | Cleanup depends on state that may not exist yet |
| `using` / `await using` | Disposal with compiler-generated null checks |
| Cleanup in its own try/catch that *logs* | Cleanup that can genuinely fail (network close, file delete) |

The axiom behind all three rows: a `finally` block has one job - to leave
quietly. Anything in it that can throw needs its own containment.

## 😈 The Even Worse Sibling

The lazy version of row three:

```csharp
finally
{
    try { connection.Close(); } catch { }  // "can't hurt, right?"
}
```

Now cleanup failures are invisible *forever*. The connection that never
closes stops being an exception and becomes a statistic - three weeks
later the pool is exhausted and nobody knows since when. A throwing
`finally` lies about the past; a silent one lies about the future. Catch -
and **log**.

## 🎓 Advanced Nuance

`using` is a `finally` in disguise - so a throwing `Dispose` replaces the
in-flight exception in exactly the same way, and the pretty syntax exempts
you from nothing. And a language-design footnote: Java's
try-with-resources keeps both exceptions (the second goes into
`getSuppressed()`); C# chose replacement - there is no suppressed-exception
chain. In C#, whatever your cleanup throws *is* the story the logs will
tell.

## 🔎 How to Find It in Your Codebase

- Read every `finally` (and every `Dispose`) asking one question: "can any
  line here throw?" Method calls on possibly-null state are suspects one.
- CS8602 warnings pointing into `finally` blocks - promote nullable
  warnings to errors and this exhibit stops compiling.
- In incident reviews: if the logged exception comes from cleanup code,
  assume it masks the real cause and reconstruct what `try` was doing.
