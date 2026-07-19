---
id: "0018"
title: Tasks mistaken for results
category: async
level: 🟡
tags: [async, LINQ, Task, deferred-execution]
summary: "`Select(async ...)` yields tasks, not results - the report says 5 sent, the counter says 0"
rule: "never mistake a collection of tasks for a collection of results"
---

# #0018 - Tasks Mistaken for Results

## 💥 Symptom

The batch job reports "5 invoices sent" and exits green. The customers
received nothing. Sometimes *some* of them receive something - whatever
happened to finish before the process shut down. The code looks obviously
correct in review: the lambda calls the sender, the `await` is right
there, the count matches the customer list.

## 🔍 The Offending Code

```csharp
var sent = customers.Select(async customer =>
{
    await SendInvoice(customer);   // this await is INSIDE each task
    return customer;
});

Console.WriteLine($"Invoices sent: {sent.Count()}");  // counting tasks
```

## 🧠 What's Actually Going On

An async lambda returns a `Task<T>` - so this `Select` produces
`IEnumerable<Task<string>>`, and `var` politely hides that from you. The
`await` inside the lambda awaits *inside each task*; nothing here awaits
**the tasks themselves**. Two awaits at two different altitudes, and the
outer one is missing.

Then deferred execution stacks on top (exhibit [0009-multiple-enumeration](../../linq/0009-multiple-enumeration/)'s cousin): the tasks
don't even exist until something enumerates the query. `Count()` is that
something - it *starts* five sends as a side effect and instantly reports
"5", which is true of tasks and says nothing about deliveries. The program
reads the counter while five `Task.Delay`-shaped SMTP calls are still in
flight, prints 0, and would have exited with the work half-done. Every
box is counted; no box is opened.

## ✅ The Fix

Turn the tasks into results exactly once, at a visible point:

```csharp
var sent = await Task.WhenAll(sendTasks);   // IEnumerable<Task<string>> -> string[]
```

Full version in [Good.cs](Good.cs). The toolbox:

| Option | When it's the right call |
|---|---|
| `await Task.WhenAll(...)` | Run in parallel, gather all results, one await |
| `foreach` + `await` inside | Sequential and ordered - the simplest honest loop |
| `Parallel.ForEachAsync` (.NET 6+) | Many items against a rate-limited dependency - bounded concurrency |

## 😈 The Even Worse Sibling

Enumerate that query **twice** - `Count()` for the report header, `foreach`
for the body - and deferred execution re-runs the async lambda per
enumeration: every customer gets the invoice *twice*. Exhibit [0009-multiple-enumeration](../../linq/0009-multiple-enumeration/)'s
multiple enumeration, upgraded from "wrong count" to "double charge". A
lazy query with side effects is a button that fires every time anyone
looks at it.

## 🎓 Senior Nuance

Two altitude markers. First: `Task.WhenAll` on ten thousand items is
unbounded concurrency - you just DoS'd your own SMTP server; that's what
`Parallel.ForEachAsync` with `MaxDegreeOfParallelism` is for. Second, the
family tree: `Select(async ...)` at least *hands you the tasks* - the
negligence is recoverable, one `WhenAll` away. Its cousin
`List.ForEach(async ...)` (exhibit [0007-async-void](../../async/0007-async-void/)) compiles the lambda to
`async void` and throws the tasks away entirely - nothing to await even
if you remember. Recoverable versus unrecoverable, one method name apart.

## 🔎 How to Find It in Your Codebase

- Hover every `var` holding a `Select(async ...)` - if the IDE says
  `IEnumerable<Task<...>>` and there's no `WhenAll` nearby, it's this
  exhibit.
- Grep `.Select(async` - each hit must be followed by an await of the
  tasks, not just of the lambda's insides.
- Reports that claim more work than downstream systems received - count
  the two independently once, and this bug has nowhere to hide.
