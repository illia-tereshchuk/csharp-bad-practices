# C# Bad Practices

> A museum of C# code that compiles fine, passes review, and ruins your week anyway.

"Ideal" tutorials teach SOLID, KISS and DRY on code no one has ever seen in
the wild. This museum works the other way around: every exhibit is a small,
runnable disaster. You watch it fail, then you watch it fixed, then you read
the autopsy. Bad code sticks in memory better than good code — that's the
whole pedagogy.

## How to Run an Exhibit

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download) — nothing
else. Every exhibit is a single runnable file: no solutions, no project
ceremony.

```bash
cd src/collections/0001-modify-while-enumerating
dotnet run Bad.cs    # watch it fail
dotnet run Good.cs   # watch it behave
```

Each exhibit folder has its own README: symptom → mechanics → fix.

## Exhibits

> Museum stats: **2** exhibits in **2** halls, latest addition — **#0002**.

### 🗂 Collections

| # | Exhibit | Level | The pain |
|--:|---------|-------|----------|
| 0001 | [Modifying a collection while iterating](src/collections/0001-modify-while-enumerating/) | 🟢 junior trap | `foreach` + `Remove` on the same list — partial execution and a crash. |

### 🔢 Numbers

| # | Exhibit | Level | The pain |
|--:|---------|-------|----------|
| 0002 | [Calculating money with double](src/numbers/0002-doubles-for-money/) | 🟢 junior trap | `0.1 + 0.2 != 0.3` — binary floats can't hold decimal cents, and the audit won't reconcile. |

More halls under construction: **async**, **ORM**, **datetime**,
**strings & memory**, **exceptions**, **DI lifetimes**, **security**.

## Difficulty Levels

| Badge | Meaning |
|-------|---------|
| 🟢 junior trap | Everyone steps into it during their first year |
| 🟡 mid-level trap | Routinely survives code review |
| 🔴 senior trap | Interview-grade; people argue about it in the comments |

## Disclaimer

All bad practices in this museum are performed by trained professionals on
synthetic data. Don't try this in production. You already have.
