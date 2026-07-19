---
id: "0009"
title: Enumerating a LINQ query twice
category: linq
level: 🟡
tags: [LINQ, IEnumerable, deferred-execution]
summary: "the header counted 3 rows, the body printed 2 - each enumeration reruns the query"
rule: "never enumerate a LINQ query twice - materialize it once"
---

# #0009 - Enumerating a LINQ Query Twice

## 💥 Symptom

Support sends a screenshot of a report: the header says "3 premium
products", the table below has two rows. Sometimes the numbers match,
sometimes they don't - it depends on what else touched the data in the
milliseconds between the two. Nobody can reproduce it locally, because
locally nothing writes to the data mid-report.

## 🔍 The Offending Code

```csharp
var premium = products.Where(p => p.Price > 100m);

int headerCount = premium.Count();  // runs the query
// ...prices change here...
foreach (var product in premium)    // runs the query AGAIN
```

## 🧠 What's Actually Going On

`Where` filters nothing when it's called. It builds a small object that
*knows how* to filter - a recipe, not a result. That's deferred execution,
and it's the foundation LINQ stands on.

Every enumeration cooks the recipe from scratch on whatever the data is
*at that moment*. `Count()` is the first enumeration; the `foreach` is the
second. Between them Black Friday cut the prices, so the same recipe
produced a different dish: the Grinder was premium for the header and
ordinary for the body.

The type tells you which side you're holding: `IEnumerable<T>` is a
promise, `List<T>` is a result. The bug is treating the promise as if it
were the result.

## ✅ The Fix

Run the recipe once and keep the plate:

```csharp
var premium = products.Where(p => p.Price > 100m).ToList();
```

Full version in [Good.cs](Good.cs). The toolbox:

| Option | When it's the right call |
|---|---|
| `ToList()` / `ToArray()` at the seam | You read the result more than once |
| Stay lazy (`IEnumerable<T>`) | Exactly one pass, streaming, huge data |
| Project into a record, then materialize | You need values frozen too, not just membership |

The third row exists because of a subtlety our own Good.cs demonstrates:
its snapshot still prints "Grinder: 60.0". `ToList()` froze *which objects*
are in the report, not their fields - the discount mutated the same
instances. A snapshot of values needs a projection:
`Select(p => new PriceTagRow(p.Name, p.Price)).ToList()`.

## 😈 The Even Worse Sibling

Point the same habit at a database and every enumeration is a full SQL
query: `Any()` + `Count()` + `foreach` over one `IQueryable` is three
round-trips (exhibit [0008-n-plus-one](../../orm/0008-n-plus-one/)'s cousin). Deadlier still across method
boundaries: you pass an `IEnumerable<T>` into a method that enumerates it,
having already enumerated it yourself - the double execution is now
invisible in any single screen of code.

## 🎓 Senior Nuance

`Count()` has a fast path: for an `ICollection` it just reads the stored
size. So `list.Count()` is free, while `query.Count()` is a full
execution - same method name, wildly different cost. And some enumerables
are one-shot by nature: an iterator over a network stream won't survive a
second pass at all, while `File.ReadLines` will happily *reopen the file*
and read the new contents. Multiple enumeration isn't just extra work; it's
a different answer each time.

## 🔎 How to Find It in Your Codebase

- Rider / ReSharper's "Possible multiple enumeration" inspection is this
  entire exhibit as a squiggle. Treat it seriously.
- Grep for `.Count()` and `.Any()` on variables that are later iterated.
- At API boundaries, accept `IReadOnlyCollection<T>` when you intend to
  enumerate more than once - the signature then enforces materialization.
