---
id: "0033"
title: A switch expression that only ever warned
category: pattern-matching
tags: [switch-expression, enum, CS8509]
rule: "never trust a **switch expression** to be exhaustive just because it once compiled clean"
---

# #0033 - A switch expression that only ever warned

## 💥 Symptom

The notification service has sent order-status emails for months without a
single crash. Then, one morning, it starts throwing on a small but steady
slice of orders - always the same status, always unhandled. Nobody touched
this file recently. The team that *did* just ship a change was fulfillment,
in a different repository entirely, and their release notes don't mention
this service at all.

## 🔍 The Offending Code

```csharp
enum OrderStatus { Placed, Shipped, Delivered, Returned }

string Notify(OrderStatus status) => status switch
{
    OrderStatus.Placed => "Your order has been placed.",
    OrderStatus.Shipped => "Your order is on its way.",
    OrderStatus.Delivered => "Your order has been delivered.",
    // CS8509: not exhaustive - a warning, not an error.
};
```

## 🧠 What's Actually Going On

A switch expression over an enum, with every *currently defined* member
handled, compiles clean - no arm, no default, nothing. But `OrderStatus` isn't
owned by this file. It lives in a shared contract package that both the
fulfillment centre and this notification service reference, and fulfillment
just added `Returned`. Their build is green. This service's build is also
green - the compiler only emits `CS8509`, a *warning*, not an error, when a
switch expression skips a member. Warnings don't fail CI unless the project
opts into treating them as errors, so the missing arm ships. It sits there,
correct on the day it was written and quietly wrong from the moment the enum
grew, until the first `Returned` order reaches `Notify` and the runtime
throws `SwitchExpressionException` because nothing matched.

The trap is that "exhaustive" here is a snapshot, not a guarantee. The
compiler checks the switch against the enum *as declared in this compilation*
- it has no idea the enum is a moving target shared across repositories on
independent release schedules.

## ✅ The Fix

Add the missing arm - obviously - but also decide what should happen for a
status nobody has invented yet. `Good.cs` handles `Returned` explicitly and
adds a `_ => throw new ArgumentOutOfRangeException(...)` arm underneath: the
next status this service doesn't know about still crashes, but loudly and
immediately, with the exact unmatched value in the message, instead of via a
generic `SwitchExpressionException` pointing at a compiler-generated helper
three frames up the stack.

| Option | When it's the right call |
|---|---|
| Handle every current member, `_ => throw` with a clear message | Default. Keeps the crash, but makes it diagnosable in one line. |
| Enable `CS8509` as an error (`<WarningsAsErrors>CS8509</WarningsAsErrors>` or `.editorconfig` severity) | You own both the enum and the switch, and want the *build* to fail the day someone adds a member, not the *runtime*. |
| `_ => default` / `_ => someFallbackValue` | Almost never for a shared-contract enum - see the sibling below. |

## 😈 The Even Worse Sibling

The `_ => default` arm is the fix everyone reaches for once the warning gets
annoying, because it makes `CS8509` disappear forever - including for every
*future* member, not just `Returned`. From that point on, an unhandled status
doesn't crash; it silently returns whatever `default` means for the return
type (`null` for a `string`, `0` for an `int`, the first case for another
enum). The notification service now sends no email, or the wrong email, for
every status it doesn't recognize, and ships a clean exit code while doing
it. The crash in this exhibit is the *lucky* outcome.

## 🎓 Advanced Nuance

`CS8509` only fires for a *switch expression* (`x switch { ... }`). A
classic `switch` *statement* over the same enum gets no such warning even
when it's missing cases entirely - falling off the bottom of a statement
switch with no matching `case` and no `default` just does nothing, silently.
Migrating from a statement switch to an expression switch is, in that narrow
sense, a strict improvement: it's the only one of the two forms that ever
tells you what it's missing, and even then, only as a warning.

## 🔎 How to Find It in Your Codebase

- Grep for `switch\s*$` followed by `{` right after a shared/external enum
  type name, and check whether the project treats `CS8509` as an error.
- Build with `-warnaserror` (or check `dotnet build /p:TreatWarningsAsErrors=true`
  locally) and see what lights up - a good one-time audit even if you don't
  keep it on permanently.
- `.editorconfig`: `dotnet_diagnostic.CS8509.severity = error` promotes just
  this one warning without turning on warnings-as-errors project-wide.
- Any switch expression over an enum that comes from a NuGet package or a
  separately-versioned project is a candidate - the further the enum's owner
  is from the switch's owner, the more likely a member and its handler drift
  apart unnoticed.
