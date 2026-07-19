---
id: "0026"
title: Disposing a dependency you were only handed
category: disposal
tags: [IDisposable, using, dependency-injection, lifetimes]
rule: "never dispose what you didn't **create**"
---

# #0026 - Disposing a Dependency You Were Only Handed

## 💥 Symptom

The first request after a deploy works. Every request after it fails with
`ObjectDisposedException` on a client that the container is supposed to own -
and the only fix anyone finds is a restart, which buys exactly one more good
request. Load tests pass, because they hit a fresh process. The handler that
causes it is the tidiest code in the file.

## 🔍 The Offending Code

```csharp
static string Quote(IServiceProvider provider, string sku)
{
    using var api = provider.GetRequiredService<PricingApi>();  // 💥
    return api.GetPrice(sku);
}
```

## 🧠 What's Actually Going On

`using` is not a scoping tool. It is a statement of **ownership**: "I created
this, I will destroy it." Here the handler created nothing - it asked the
container for the shared instance and then destroyed it on the way out.

The container registered `PricingApi` as a singleton, so every later resolve
hands back the same object - now disposed. The damage isn't local to the
request that made the mistake; it is permanent for the process. That is the
asymmetry that makes this expensive: one handler's tidiness, applied once,
takes down every caller of a service it doesn't own, forever.

Nothing warns you. `using` on an `IDisposable` is the pattern every tutorial
teaches, the analyzer approves, and code review nods at - the mistake is
invisible because it looks exactly like the correct habit applied one level
too high.

## ✅ The Fix

Borrow it and leave it alone - the container disposes what the container
created:

```csharp
var api = provider.GetRequiredService<PricingApi>();
return api.GetPrice(sku);
```

Full version in [Good.cs](Good.cs) - the diff is one keyword. Who owns what:

| Situation | Who disposes |
|---|---|
| You called `new` | You do - `using` is correct |
| It was injected or resolved | The container. Touch nothing |
| You created a scope (`CreateScope`) | You dispose the **scope**, not the services in it |

## 😈 The Even Worse Sibling

The same mistake on a *scoped* or *transient* service instead of a singleton.
Now it only breaks when two consumers happen to share one instance, so it fails
under concurrency and passes every time you step through it in a debugger.
The singleton version at least fails on the second request, loudly and always -
this one waits for traffic.

## 🎓 Advanced Nuance

The reverse mistake is the one from
[0014-container-hoarder](../../di-lifetimes/0014-container-hoarder/): resolving
disposables from the root container and *never* disposing them. Together they
frame the actual rule - disposal is not a habit you apply everywhere, it follows
ownership, and the container's job is to know which is which.

Note also what `using var` does to the timing: it disposes at the end of the
enclosing **method**, not at the end of the block you were thinking about. In a
long method that borrows a shared service early, the object is already gone for
everyone else long before the method returns.

## 🔎 How to Find It in Your Codebase

- Grep for `using` on the same line as `GetRequiredService`, `GetService`, or
  `Resolve` - that combination is always wrong.
- Any `using` wrapped around a constructor parameter or an injected field. If
  the object arrived from outside, you are not its owner.
- `ObjectDisposedException` on a singleton is the signature: something disposed
  a shared instance, and the stack trace points at the victim, not the culprit.
