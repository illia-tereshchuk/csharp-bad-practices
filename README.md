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

> Museum stats: **1** exhibit, latest — **#0001**.

### 🗂 Collections

| # | Exhibit | Level | The pain |
|--:|---------|-------|----------|
| [0001](src/collections/0001-modify-while-enumerating/) | Modifying a collection while iterating | 🟢 junior trap | `foreach` + `Remove` on the same list — partial execution and a crash. |

More halls under construction: **async**, **ORM**, **datetime**,
**strings & memory**, **exceptions**, **DI lifetimes**, **security**.

## Difficulty Levels

| Badge | Meaning |
|-------|---------|
| 🟢 junior trap | Everyone steps into it during their first year |
| 🟡 mid-level trap | Routinely survives code review |
| 🔴 senior trap | Interview-grade; people argue about it in the comments |

## Contributing

Got a production scar to share? Exhibit numbers are global — grab the next
free one with:

```bash
dotnet run tools/next-id.cs
```

Full guide: CONTRIBUTING.md *(coming soon)*.

## Disclaimer

All bad practices in this museum are performed by trained professionals on
synthetic data. Don't try this in production. You already have.
