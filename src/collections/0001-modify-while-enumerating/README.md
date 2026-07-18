---
id: "0001"
title: Modifying a collection while iterating
category: collections
level: 🟢
tags: [List, foreach, InvalidOperationException]
summary: foreach + Remove on the same list - partial execution and a crash.
rule: "Never modify a collection while iterating it"
---

# #0001 - Modifying a Collection While Iterating

## 💥 Symptom

The inactive-subscriber cleanup ran "halfway" in production: one subscriber
got removed, the rest survived, and the logs show
`InvalidOperationException: Collection was modified`. And if someone swallowed
the exception with a `try/catch` - no logs either, just a half-cleaned database.

## 🔍 The Offending Code

```csharp
foreach (var subscriber in subscribers)
{
    if (!subscriber.IsActive)
    {
        subscribers.Remove(subscriber); // 💥
    }
}
```

## 🧠 What's Actually Going On

`foreach` works through an **enumerator** - a cursor stepping through the list.
`List<T>` keeps an internal version counter: every mutation (Add, Remove,
Clear...) bumps it. The enumerator captures the version when created and
compares it on every step. After `Remove()` the versions no longer match - and
the next loop iteration throws.

This is a **safety net, not sabotage**: after a removal the elements shift, and
the cursor would either skip over a neighbor or read garbage. An immediate
crash is more honest than a silently corrupted result.

Note: the exception isn't thrown at the `Remove()` line but on the *next*
iteration. That's why the operation manages to run partially.

## ✅ The Fix

Tell the list *what* you want instead of dodging the trap manually:

```csharp
subscribers.RemoveAll(s => !s.IsActive);
```

Full version in [Good.cs](Good.cs). Alternatives for when `RemoveAll` doesn't
fit:

| Option | When it's the right call |
|---|---|
| `RemoveAll(predicate)` | Just remove by condition. The default choice |
| `foreach` over a snapshot: `.Where(...).ToList()` | You need to do something per removed item (log, email, event) |
| `for` loop from the end backwards | Hot path where a copy is unacceptable. Hardest to read |

## 😈 The Even Worse Sibling

```csharp
for (int i = 0; i < subscribers.Count; i++)
{
    if (!subscribers[i].IsActive)
        subscribers.RemoveAt(i); // doesn't crash. SILENTLY skips the next element
}
```

After `RemoveAt(i)` everything shifts left and `i++` jumps over the neighbor.
No exception - just a bug quietly living in production for years. The classic
crutch `i--` after removal works, but that's already the second trick in three
lines of code.

## 🎓 Senior Nuance

Starting with .NET Core 3.0, `Dictionary<TKey, TValue>` **allows** `Remove()`
during enumeration (but not `Add()`). So "any collection mutation inside
foreach throws" is a broken mental model too. Every collection has its own
contract.

## 🔎 How to Find It in Your Codebase

- Code-review eye: any `Remove` / `Add` / `Clear` inside a `foreach` over the
  same collection.
- Rider / ReSharper flag this statically - don't ignore the yellow squiggles.
- Suspicious pattern to search for: a loop whose body mentions the same
  variable it iterates over.

## 📚 Dig Deeper

- [List&lt;T&gt;.RemoveAll - Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1.removeall)
- [IEnumerator&lt;T&gt; and the enumeration contract](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerator-1)
