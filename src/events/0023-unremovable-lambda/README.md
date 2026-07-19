---
id: "0023"
title: Unsubscribing from an event with a lambda
category: events
level: 🟡
tags: [events, delegates, lambdas, unsubscribe]
summary: "the -= gets an identical-looking lambda, removes nothing, and the cancelled alert keeps firing"
rule: "never unsubscribe with a lambda - name the handler"
---

# #0023 - Unsubscribing From an Event With a Lambda

## 💥 Symptom

A customer turns off a price alert and keeps getting notified. Support
checks the database: the alert is off. The code plainly unsubscribes -
the `-=` is right there in the cancel handler, reviewed and approved.
Worse, users who toggle the setting a few times start receiving
*duplicate* notifications: every toggle adds a subscriber and removes
none.

## 🔍 The Offending Code

```csharp
feed.PriceChanged += price => Notify(price);   // subscribe

feed.PriceChanged -= price => Notify(price);   // 💥 identical text, different delegate
```

## 🧠 What's Actually Going On

`-=` doesn't remove "the handler that looks like this" - it scans the
invocation list for an entry **equal** to the delegate you hand it.
Delegate equality compares two things: the target object and the compiled
method.

Every lambda expression in your source compiles to its own generated
method. Two identical-looking lambdas sit at two different places in the
file, so the compiler emits two different methods - and two delegates
whose `Method` differs are not equal. `-=` searches, finds no match, and
removes nothing.

Then the second half of the trap: **removing a handler that isn't there
is a legal no-op**. No exception, no return value, no warning. The API
gives you no way to notice you failed. The exhibit has to ask
`GetInvocationList().Length` to reveal the truth - `Subscribers: 1` after
a "successful" cancel.

## ✅ The Fix

Give the handler a name so both sides can refer to the same delegate:

```csharp
Action<decimal> onPriceChanged = price => Notify(price);

feed.PriceChanged += onPriceChanged;
feed.PriceChanged -= onPriceChanged;   // the same instance - actually removed
```

Full version in [Good.cs](Good.cs). The toolbox:

| Option | When it's the right call |
|---|---|
| Store the delegate in a field or variable | You subscribe and unsubscribe at different times - the default |
| A named method (`+= OnPriceChanged`) | The handler is real logic; see the nuance below for why this works |
| Wrap the subscription in an `IDisposable` | Complex lifetimes: disposing is the unsubscribe (pairs with [0010-immortal-subscriber](../../events/0010-immortal-subscriber/)) |

## 😈 The Even Worse Sibling

Combine this exhibit with [0010-immortal-subscriber](../../events/0010-immortal-subscriber/): a lambda that captures `this` and
subscribes to a long-lived event. Now the subscriber is *immortal* (the
publisher pins it) **and** unremovable (there is no delegate to pass to
`-=`). Nothing short of destroying the publisher can release it - the
leak has no fix at the call site, only a rewrite.

## 🎓 Senior Nuance

A named method **does** work, and for a reason worth knowing: a method
group conversion creates a new delegate object each time, but with the
same target and the same method - and since equality compares those, not
references, `-=` matches it. So `-= OnPriceChanged` succeeds while
`-= price => ...` fails, even though both look like "pass the handler".

The related myth: "non-capturing lambdas are cached, so they're fine."
Roslyn does cache them - but per lambda expression in the source. Writing
the same body twice gives you two cache slots and two methods, so the
caching never saves you here. And an async lambda handler
(`+= async price => ...`) is unremovable *and* `async void` - exhibit
[0007-async-void](../../async/0007-async-void/) riding along for free.

## 🔎 How to Find It in Your Codebase

- Grep for `-=` on the same line as `=>`. That match is always a bug -
  the operation is guaranteed to remove nothing.
- Any `+=` with an inline lambda that has a cancel, close, or dispose
  path somewhere: check what that path passes to `-=`.
- No analyzer reliably flags this, precisely because a no-op removal is
  legal. Add `GetInvocationList().Length` assertions in tests around
  subscribe/unsubscribe pairs - that turns the silence into a red test.
