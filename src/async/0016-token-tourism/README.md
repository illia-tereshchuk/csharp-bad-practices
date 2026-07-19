---
id: "0016"
title: A cancellation token nobody reads
category: async
tags: [cancellation, CancellationToken, async]
rule: "never accept a token you don't pass down or check"
---

# #0016 - A Cancellation Token Nobody Reads

## 💥 Symptom

The user clicks Cancel, the spinner disappears - and the job quietly runs
to the end anyway: the emails go out, the export lands in the bucket, all
twenty statements print. Or the ops flavor: graceful shutdown requests
cancellation, every service "supports" it (every signature says so!), and
the host still waits the full timeout before killing anything.
Code review approved it, because the `CancellationToken` is right there in
every method.

## 🔍 The Offending Code

```csharp
async Task PrintStatements(CancellationToken token)     // welcomed here...
{
    for (int i = 1; i <= 20; i++)
        await RenderStatement(i, token);                // ...forwarded here...
}

async Task RenderStatement(int n, CancellationToken token)
{
    await Task.Delay(25);                               // 💥 ...and dropped here
}
```

## 🧠 What's Actually Going On

A `CancellationToken` is just a readable flag with a callback list.
Cancellation in .NET is **cooperative**: it happens only at the points
where code *observes* the token - by handing it to a cancellable API or by
calling `ThrowIfCancellationRequested`. Nothing observes it here, so
nothing cancels. The compiler is fine with an unused parameter; the token
rides through five signatures like a tourist - sees every method,
participates in nothing.

Two properties make this bug nasty. First, **the chain is only as
cancellable as its least cooperative link**: one leaf that drops the token
severs the entire call graph above it. Second, the signature now
advertises a capability the body doesn't have - callers trust the
contract, wire up their `CancellationTokenSource`, and get nothing. An
honest method without a token parameter would at least tell the truth.

## ✅ The Fix

Carry the token to where the work actually happens:

```csharp
await Task.Delay(25, token);   // the leaf is now cancellable
```

...and translate the resulting `OperationCanceledException` at the
boundary (exhibit [0015-cancellation-eaten-by-catch](../../exceptions/0015-cancellation-eaten-by-catch/) covers that half). The toolbox:

| Option | When it's the right call |
|---|---|
| Forward the token to every cancellable API | The default: `Task.Delay`, `HttpClient`, EF, streams all take one |
| `token.ThrowIfCancellationRequested()` in the loop | CPU-bound work with no awaits to carry the token |
| Catch OCE at the boundary, report "cancelled" | Pairs with [0015-cancellation-eaten-by-catch](../../exceptions/0015-cancellation-eaten-by-catch/): let it flow, then translate |

## 😈 The Even Worse Sibling

```csharp
async Task PrintStatements(CancellationToken token = default)
```

The default-value epidemic. Make the parameter optional and callers stop
passing it *silently* - every call site compiles, every signature still
looks cancellable, and the whole call graph actually runs on
`CancellationToken.None`. The tourist at least travelled; this token never
left home.

## 🎓 Advanced Nuance

Even with perfect token plumbing, cancellation stays a race by design: the
cancel can arrive after the last page rendered, and "completed" is then an
honest answer. What's dishonest is only what this exhibit shows -
completing *without ever looking*. And know your tooling: analyzer
**CA2016** ("Forward the CancellationToken parameter") flags calls that
have a token-taking overload you didn't use - it would have pointed
straight at the naked `Task.Delay(25)`.

## 🔎 How to Find It in Your Codebase

- Promote CA2016 to error in `.editorconfig` - it catches the dropped
  leaf calls automatically.
- Review any method that accepts a token: the body must either forward it
  or check it. If neither, the signature is lying.
- Cheap regression test: cancel *before* calling, then assert the work
  threw OCE or left partial results - a job that returns fully complete
  failed the test.
