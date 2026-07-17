---
id: "0004"
title: Mutating an object that lives as a dictionary key
category: collections
level: 🟡
tags: [Dictionary, GetHashCode, records]
summary: "change a field on the key - `foreach` still shows the entry, lookups can't find it"
---

# #0004 - Mutating an Object That Lives as a Dictionary Key

## 💥 Symptom

After a profile rename, the player's score is gone from the leaderboard -
but monitoring says the cache holds the same number of entries. You attach
a debugger: the entry is *right there* in the dictionary. `ContainsKey`
returns `false` anyway. The exception message shows the key it "couldn't
find" - character for character identical to the one in the collection.
At this point someone on the team says the word "haunted".

## 🔍 The Offending Code

```csharp
var scores = new Dictionary<Player, int>();
scores[player] = 1500;

player.Nickname = "SeniorSlayer"; // rebranding mid-season

scores[player]; // 💥 KeyNotFoundException

record Player
{
    public required string Nickname { get; set; } // mutable key material
}
```

## 🧠 What's Actually Going On

A dictionary is an array of **buckets**. On insert it calls
`GetHashCode()` on the key and files the entry into bucket
`hash % bucketCount`. On lookup it calls `GetHashCode()` *again* and goes
straight to that bucket - that's what makes it O(1).

`record` types auto-generate `Equals` and `GetHashCode` from **all** their
properties. So the rename changed the key's hash - and lookups now walk to
a *different* bucket, which is empty. The entry itself never moved: it
still sits in the old bucket, which is why `foreach` (which walks every
bucket) happily prints it. You've created a Schrödinger's entry: visible
when you enumerate, unreachable when you ask for it.

## ✅ The Fix

Key material must be immutable. A positional record gives you that for
free - and a rename becomes an explicit *replace*, not a mutation:

```csharp
record Player(string Nickname); // init-only, hash can never drift

var renamed = player with { Nickname = "SeniorSlayer" };
scores[renamed] = scores[player];
scores.Remove(player);
```

Full version in [Good.cs](Good.cs). Choosing the shape of the key:

| Option | When it's the right call |
|---|---|
| Immutable record + explicit re-key | The whole object *is* the identity |
| Key by a stable ID (`int`, `Guid`) | Identity is conceptual; nickname is just data that changes. The default in real systems |
| Custom `GetHashCode` over immutable fields only | Legacy classes you can't rewrite. Fragile - document loudly |

## 😈 The Even Worse Sibling

After the rename, insert the "same" player again: `scores[player] = 100;`.
No exception - the dictionary now holds **two entries** whose keys compare
as equal. Every future operation picks one of them semi-randomly, exports
show duplicates, and no single line of code will ever look wrong again.
The crash in this exhibit is the *lucky* outcome.

## 🎓 Senior Nuance

Records made this trap *more* ergonomic, not less: a pre-records class
without overrides used reference equality, so mutating its fields was
harmless for hashing. Give a `record` settable properties and you've built
value-based hashing over mutable state - a footgun with excellent developer
experience. Same story applies to `HashSet<T>` items. And this is exactly
why `string` is the world's favorite dictionary key: it can't change.

## 🔎 How to Find It in Your Codebase

- Every `Dictionary<K,V>` / `HashSet<T>` where the key type is a class or
  record with a setter - that's the whole audit.
- `record` + `{ get; set; }` is a smell on its own; records want `init`.
- Rider / ReSharper warn when `GetHashCode` depends on mutable state -
  treat that one as an error, not a suggestion.

## 📚 Dig Deeper

- [Object.GetHashCode - the "must not change while in a hash table" contract](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)
- [Records: value equality - Microsoft Learn](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/record)
