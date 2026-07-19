---
id: "0022"
title: A singleton that captured a scoped dependency
category: di-lifetimes
tags: [dependency-injection, lifetimes, captive-dependency, thread-safety]
rule: "never inject a scoped service into a singleton"
---

# #0022 - A Singleton That Captured a Scoped Dependency

## 💥 Symptom

Users report seeing *other people's* data: someone else's cart, another
tenant's dashboard, a name that isn't theirs. It's intermittent, it never
reproduces for one developer clicking alone, and it gets worse under load.
Security escalates it as a data-leak incident - which it is - but there is
no missing auth check anywhere. The leak is in the wiring, not the code.

## 🔍 The Offending Code

```csharp
services.AddScoped<CurrentUser>();     // one per request
services.AddSingleton<CartService>();  // one for the whole app...

class CartService(CurrentUser user)    // ...capturing one CurrentUser forever
{
    public string Owner => user.Name;
}
```

## 🧠 What's Actually Going On

Three lifetimes, three promises: **singleton** is one instance for the
application, **scoped** is one per request, **transient** is one per
resolve. They compose safely in one direction only - a shorter life may
depend on a longer one. Go the other way and you get a **captive
dependency**: the singleton is built once, and whatever `CurrentUser` it
received at that moment is welded into it for the life of the process.

Every request resolves the *same* singleton `CartService`, which holds the
*same* captured `CurrentUser`. So when Olena's request writes her name and
her laptop through the cart, they land in that one shared instance - and
Ivan's request, reading the same singleton, sees them. The scoped service
promised "one per request"; the singleton made it "one, forever, for
everyone". The per-request isolation that the whole design relied on is
silently gone.

Note what leaked: not a copy, the actual object. `CurrentUser` was meant
to be the safest thing in the system - request-local by construction - and
the lifetime mismatch turned it into shared global state wearing a
per-request costume.

## ✅ The Fix

Make the consumer's lifetime match its shortest dependency - a service
that needs per-request state cannot be a singleton:

```csharp
services.AddScoped<CartService>(); // one per request, like the CurrentUser it holds

root = services.BuildServiceProvider(new ServiceProviderOptions
{
    ValidateScopes = true,
    ValidateOnBuild = true,  // the container refuses the bad graph at startup
});
```

Full version in [Good.cs](Good.cs). The toolbox:

| Option | When it's the right call |
|---|---|
| Match lifetimes (scoped consumer for scoped data) | The default fix - the consumer is as short-lived as what it holds |
| `IServiceScopeFactory` inside the singleton | The service is genuinely singleton but needs per-operation data: create a scope per unit of work |
| Pass the scoped data as a method argument | The singleton is stateless plumbing; hand it the request context per call, don't inject it |

The real safety net is `ValidateOnBuild = true` + `ValidateScopes = true`:
the container then throws *at startup* - "Cannot consume scoped service
`CurrentUser` from singleton `CartService`" - turning a production data
leak into a failed boot. ASP.NET Core turns both on in Development for
exactly this reason.

## 😈 The Even Worse Sibling

Give the singleton an `IServiceProvider` and resolve on demand:

```csharp
class CartService(IServiceProvider sp)
{
    public string Owner => sp.GetRequiredService<CurrentUser>().Name; // from the ROOT scope
}
```

Now `ValidateOnBuild` sees a clean constructor and passes - the graph
*looks* correct. But the service locator resolves `CurrentUser` from the
root scope, so every request still shares one instance. Same leak, now
invisible to the one tool that would have caught it. (Its cousin haunts
exhibit [0014-container-hoarder](../../di-lifetimes/0014-container-hoarder/).)

## 🎓 Advanced Nuance

Two escalations a review should weigh. First, **concurrency**: a singleton
is touched by many requests at once, so a captured scoped service isn't
just stale - it's mutated from multiple threads with no synchronization,
which is exhibit [0003-race-on-shared-counter](../../async/0003-race-on-shared-counter/) hiding inside a lifetimes bug. Second, **disposal**:
if the captured scoped service is `IDisposable`, it now lives and dies with
the singleton instead of the request - the leak from exhibit [0014-container-hoarder](../../di-lifetimes/0014-container-hoarder/), arrived
by a different road. And the subtle truth beneath the demo: the captured
instance is the *root* scope's `CurrentUser`, an instance that belongs to
no request at all - the singleton didn't capture Olena's user, it captured
a ghost that Olena's request happened to write to first.

## 🔎 How to Find It in Your Codebase

- Read every `AddSingleton<T>` and check T's constructor: any parameter
  registered as `AddScoped` is this exhibit. That grep is the whole audit.
- Turn on `ValidateScopes` and `ValidateOnBuild` in every environment, not
  just Development - a failed boot is the cheapest possible version of
  this bug.
- Treat an injected `IServiceProvider` inside a singleton as a red flag:
  it is the escape hatch that smuggles the captive dependency past
  validation.
