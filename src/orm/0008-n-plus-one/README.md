---
id: "0008"
title: The N+1 query problem
category: orm
tags: [EFCore, SQL, performance]
rule: "never query the database inside a loop"
---

# #0008 - The N+1 Query Problem

## 💥 Symptom

The orders page renders in 40 ms on a laptop and in 4 seconds in production.
The DBA shows you the log: the same tiny SELECT repeated hundreds of times,
once per row, each with a different id. Nobody wrote a loop of queries - at
least nobody remembers writing one. Local dev has 20 orders; production has
50,000.

## 🔍 The Offending Code

```csharp
var orders = db.Orders.ToList();                  // 1 query

foreach (var order in orders)
{
    var customer = db.Customers                   // +1 query per row
        .Single(c => c.Id == order.CustomerId);
    Console.WriteLine($"{customer.Name} paid {order.Total}");
}
```

## 🧠 What's Actually Going On

An ORM makes a database table feel like a list in memory - and hides the
fact that every query is a network round-trip: send SQL, wait, parse the
reply. The report above pays that cost 21 times: once for the order list,
then once *per order* inside the loop. N rows means N+1 queries; the name
of the problem is its formula.

The trap is that each individual query is fast, so nothing looks broken in
the profiler's "slowest query" view. The damage is the *count*: 21 queries
at 2 ms of network latency each is 42 ms of pure waiting, and it grows
linearly with the data. The database is fine. The chattiness is the bug.

This exhibit measures it honestly: the DbContext counts real
`CommandExecuting` events, and the audit fails when the report costs more
than a couple of queries.

## ✅ The Fix

Ask for everything in one trip:

```csharp
var orders = db.Orders.Include(o => o.Customer).ToList(); // 1 query, one JOIN
```

Full version in [Good.cs](Good.cs). Same report, same 20 rows, one query.
The toolbox:

| Option | When it's the right call |
|---|---|
| `Include(...)` | You need whole entities on both sides |
| `Select` projection | Reports and read models - fetches only the columns you use |
| `AsSplitQuery()` | Wide graphs where one giant JOIN would multiply rows |

## 😈 The Even Worse Sibling

Turn on `.UseLazyLoadingProxies()` and the same N+1 hides inside an
innocent property access: `order.Customer.Name` silently fires a query
mid-loop. Nobody wrote a query at all - the ORM did, on every iteration.
That is exactly why lazy loading is off by default in EF Core.

## 🎓 Advanced Nuance

The opposite extreme fails too: chain enough `Include(...).ThenInclude(...)`
and the single JOIN multiplies rows into a cartesian explosion - one query
that transfers more data than the 21 would have. `AsSplitQuery()` exists
precisely for that trade-off. The metric that catches both diseases is the
same: watch *queries per request* and *rows transferred*, not query
duration.

A note on the file itself: this is a real EF Core + SQLite app in a single
file. The `#:package` directives pull NuGet packages, and
`#:property PublishAot=false` opts out of the file-based default - EF Core
builds its model with dynamic code, which Native AOT forbids.

## 🔎 How to Find It in Your Codebase

- Any database access inside a `foreach` - that grep is the whole audit.
- Enable EF Core command logging in staging and count queries per request;
  a list endpoint that sends more than a handful is this exhibit.
- With lazy proxies on: any navigation property touched inside a loop.
