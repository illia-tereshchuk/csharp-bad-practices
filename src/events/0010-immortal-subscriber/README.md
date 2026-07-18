---
id: "0010"
title: A static event that never lets go
category: events
level: 🔴
tags: [events, memory-leak, GC, WeakReference]
summary: "a closed widget keeps reacting to events - the static event pins it in memory forever"
rule: "Never subscribe to a long-lived event without unsubscribing"
---

# #0010 - A Static Event That Never Lets Go

## 💥 Symptom

The desktop app's memory grows all day; the nightly restart "fixes" it.
Users report popups from windows they closed an hour ago. A memory dump is
full of view models for screens nobody has open. The server-side flavor: a
"hub" service with a static event, and every subscriber since startup is
still in memory, still handling every message.

## 🔍 The Offending Code

```csharp
class DashboardWidget
{
    public DashboardWidget(string name)
    {
        OrderFeed.OrderShipped += OnOrderShipped; // subscribed in the ctor...
    }
    // ...unsubscribed never
}
```

## 🧠 What's Actually Going On

`+=` stores a delegate inside the *publisher*. A delegate is two things: a
method pointer and a **reference to the target object**. So after
subscribing, the publisher references the subscriber - the exact opposite
of what intuition says. Events feel like loose coupling; for the garbage
collector, a subscription is the strongest coupling there is.

The chain in this exhibit: a static field (a GC root, alive as long as the
process) -> the event's delegate -> the widget -> everything the widget
references. The widget cannot die while the feed lives, and the feed lives
forever.

And it's worse than a memory leak: the "closed" widget still *executes*
its handler on every event - that's the ghost rendering of order #1002.
Not a corpse taking up space; a zombie doing work.

## ✅ The Fix

Closing must mean unsubscribing - a `-=` symmetric to every `+=`:

```csharp
class DashboardWidget : IDisposable
{
    public void Dispose() => OrderFeed.OrderShipped -= OnOrderShipped;
}
```

Full version in [Good.cs](Good.cs). The toolbox:

| Option | When it's the right call |
|---|---|
| `-=` in `Dispose` / `Close` | The default whenever the subscriber dies before the publisher |
| Weak event patterns (`WeakEventManager` in WPF) | You can't control when subscribers get disposed |
| Match the lifetimes by design | An instance event on a publisher that dies *with* its subscribers can't leak |

## 😈 The Even Worse Sibling

```csharp
OrderFeed.OrderShipped += order => Render(order);
```

Subscribe with a lambda and the leak becomes *unfixable*: there is no name
to `-=`. Writing the "same" lambda again produces a different delegate, so
the unsubscribe silently removes nothing. Immortal and unremovable, one
line of code.

## 🎓 Senior Nuance

The exhibit demonstrates leak-test hygiene it had to learn the hard way:
the widget is created and dropped inside a separate
`[MethodImpl(NoInlining)]` method. In unoptimized Debug builds the current
frame's stack slots keep locals reachable until the method returns - even
after `widget = null` - so a naive `WeakReference` test reports a leak
that isn't there. Drop the object in its own frame, then collect. The
double `GC.Collect()` with `WaitForPendingFinalizers` in between is the
standard incantation: finalizers can briefly resurrect objects.

## 🔎 How to Find It in Your Codebase

- Every `+=` in a constructor without a matching `-=` anywhere. That's the
  whole audit.
- `grep -rn "static event"` - the red-flag tier: publishers that live
  forever.
- Memory profiler retention paths (dotMemory, PerfView) read this leak as
  `static field -> EventHandler -> your object`.
- Turn this exhibit's WeakReference trick into a regression test for your
  disposables.

## 📚 Dig Deeper

- [Weak event patterns - Microsoft Learn](https://learn.microsoft.com/dotnet/desktop/wpf/events/weak-event-patterns)
- [Fundamentals of garbage collection - Microsoft Learn](https://learn.microsoft.com/dotnet/standard/garbage-collection/fundamentals)
