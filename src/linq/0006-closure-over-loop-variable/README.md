---
id: "0006"
title: A closure capturing the loop variable
category: linq
level: 🟢
tags: [closures, lambdas, delegates]
summary: "five callbacks, one shared `i` - every lambda reads the value after the loop ended"
---

# #0006 - A Closure Capturing the Loop Variable

## 💥 Symptom

A batch job prepares personalized notifications in a loop and fires them
later. In production it either crashes on the very first send - or, in the
sneakier variant, **every customer gets the last customer's message**.
Worked perfectly in testing, where the batch had one item.

## 🔍 The Offending Code

```csharp
for (int i = 0; i < winners.Length; i++)
{
    congratulations.Add(
        () => Console.WriteLine($"Congrats, {winners[i]}!"));
}
// ...later...
congratulations[0](); // 💥 IndexOutOfRangeException
```

## 🧠 What's Actually Going On

A closure captures the **variable, not its value**. The compiler lifts `i`
out of the method into a hidden object (look at the crash trace: the frame
is literally named `<>c__DisplayClass...` - that's it), and all five
lambdas hold a reference to the *same* object with the *same single* `i`.

The loop then mutates that one `i` five times and exits - and a `for` loop
exits only when its condition fails, so `i` lands on `5`. When the
callbacks finally run, each one reads the shared `i` *as it is now*:
`winners[5]`, one past the end. Five personalized greetings turned into
five copies of an array crash.

The timeline is the trap: **capture happens in the loop, the read happens
after it.** Anything that delays execution - a stored delegate, an event
subscription, a `Task` - spans that gap.

## ✅ The Fix

Give each iteration its own variable; the closure will capture the copy:

```csharp
for (int i = 0; i < winners.Length; i++)
{
    var winner = winners[i]; // fresh variable each pass
    congratulations.Add(() => Console.WriteLine($"Congrats, {winner}!"));
}
```

Full version in [Good.cs](Good.cs). The toolbox:

| Option | When it's the right call |
|---|---|
| Local copy inside the loop body | You're staying with `for` |
| `foreach` | The modern default - its iteration variable is per-pass by language design |
| Pass state explicitly: `Select((w, i) => ...)` | Pipelines, parallel work - no ambient capture at all |

## 😈 The Even Worse Sibling

```csharp
for (int i = 0; i < orders.Count; i++)
{
    Task.Run(() => Process(orders[i])); // tasks start whenever they please
}
```

The parallel flavor. Each task reads `i` at its own leisure: some orders
get processed twice, some never, and it only misbehaves under load. The
sequential crash in this exhibit is the *lucky* version - this one passes
every test you'll ever write for it.

## 🎓 Senior Nuance

`foreach` had exactly this bug until C# 5: one shared iteration variable
per loop. In 2012 the language team made a rare **breaking change** - the
same code compiles to different behavior on different compilers - because
the old semantics produced nothing but bug reports. `for` was left alone
deliberately: its counter genuinely *is* one mutable variable; that's the
loop's identity. So the safety of your closure depends on which loop
keyword sits above it.

## 🔎 How to Find It in Your Codebase

- Any lambda, local function, or event subscription inside a `for` body
  that mentions the counter - especially with `Task.Run` or `+=`.
- Rider / ReSharper flag it as "Access to modified closure" - that warning
  is this exhibit in one sentence.
- Test batches with more than one item. The one-item batch hides this bug
  perfectly.

## 📚 Dig Deeper

- [Eric Lippert - Closing over the loop variable considered harmful](https://ericlippert.com/2009/11/12/closing-over-the-loop-variable-considered-harmful-part-one/)
- [Lambda expressions and captured variables - Microsoft Learn](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/lambda-expressions)
