---
id: "0007"
title: async void and the uncatchable exception
category: async
tags: [async, exceptions, fire-and-forget]
rule: "never write `async void` outside event handlers"
---

# #0007 - `async void` and the Uncatchable Exception

## 💥 Symptom

The service dies at random moments with an unhandled exception from a helper
that is wrapped in `try/catch` at every call site. The crash dump doesn't
even mention your calling code - just thread-pool plumbing. In desktop apps
the window simply vanishes. Restarts happen at night, the bug never
reproduces in the debugger, and the catch block has perfect test coverage.

## 🔍 The Offending Code

```csharp
try
{
    SendWelcomeEmail(address);         // async void - returns instantly
    Console.WriteLine("All good.");    // prints. it's lying
}
catch (Exception ex)                   // covers nothing that matters
{
    logger.Error(ex);
}

async void SendWelcomeEmail(string address)
{
    await Task.Delay(100);
    throw new InvalidOperationException($"SMTP rejected {address}");
}
```

## 🧠 What's Actually Going On

A normal `async Task` method stores its exception **inside the returned
Task**, where it waits patiently until someone `await`s it. `async void`
has no Task - the exception has nowhere to live. So the state machine
throws it *raw on the thread pool*, and an unhandled thread-pool exception
terminates the process. Look at the crash trace in this exhibit: it goes
through `Task.ThrowAsync` and `ThreadPoolWorkQueue.Dispatch` - your call
site isn't even in it.

The `try/catch` only guards the synchronous slice of the call: an async
method runs on the caller's thread **until its first await**, then returns
control. By the time the bomb goes off, the caller left the `try` block
half a second ago. 

## ✅ The Fix

Give the exception a home and a reader:

```csharp
await SendWelcomeEmail(address);       // inside the try

async Task SendWelcomeEmail(string address) { ... }
```

Full version in [Good.cs](Good.cs). The toolbox:

| Option | When it's the right call |
|---|---|
| `async Task` + `await` | The default. Everywhere |
| `async void` | Event handlers only - the platform's contract. Wrap the *entire body* in try/catch |
| Deliberate fire-and-forget | Store the `Task`, observe it later - or a `SafeFireAndForget` helper that logs faults |

## 😈 The Even Worse Sibling

```csharp
customers.ForEach(async c => await NotifyAsync(c));
```

Nobody typed `async void` here - but `List<T>.ForEach` takes an
`Action<T>`, so the async lambda *compiles into one*. Same trap in a lambda
costume, and it walks straight through code review. Any API with an
`Action` parameter does this: `Parallel.For`, timer callbacks, `ForEach`.

## 🎓 Advanced Nuance

An async method runs **synchronously until its first await**. Which means:
if the `throw` happens *before* the first `await`, the caller's catch
*does* catch it - even from `async void`. This exhibit would behave
completely differently if two lines swapped places. A bug that appears and
disappears when someone reorders statements is the most expensive kind to
hunt.

## 🔎 How to Find It in Your Codebase

- `grep -rn "async void"` - everything that is not an event handler is a
  defect, no exceptions to the rule.
- Async lambdas passed where an `Action` is expected - `ForEach`,
  `Parallel.*`, timers, `IObservable.Subscribe`.
- The `Microsoft.VisualStudio.Threading.Analyzers` package flags this as
  **VSTHRD100** - worth adding to any async-heavy codebase.
