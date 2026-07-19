---
id: "0020"
title: A billing day that shrinks
category: datetime
tags: [DateTime, AddMonths, recurrence]
rule: "never compute the next date from the previous one - keep the anchor"
---

# #0020 - A Billing Day That Shrinks

## 💥 Symptom

A customer signs up on January 31 and notices, some spring, that their
"monthly on the 31st" subscription now bills on the 28th. Every month.
Forever. Support finds thousands of accounts quietly migrated to the
27th and 28th over the years - each one passed through one short month
at some point and never recovered. No exception was ever thrown; every
individual charge looked perfectly valid.

## 🔍 The Offending Code

```csharp
var billing = signup;                 // 2026-01-31
foreach (var cycle in cycles)
{
    billing = billing.AddMonths(1);   // 💥 Feb 28, then Mar 28, Apr 28...
    Charge(customer, billing);
}
```

## 🧠 What's Actually Going On

`AddMonths` must answer an impossible question - "what is January 31 plus
one month?" - and it answers the only sane way: it clamps the day to the
target month's length. February 28. No exception; a total, quiet
function.

The clamp is not the bug. The bug is **iterating**: computing each date
from the *previous* one. The clamped `Feb 28` doesn't remember it wanted
to be the 31st - a `DateTime` carries no "intended day" field - so March
is computed from the 28th, and the information is gone. A lossy operation
applied iteratively accumulates its loss permanently. One short month
acts as a ratchet: the day can shrink and can never grow back.

Watch the two schedules the exhibit prints: both bill on Feb 28 - that
part is correct in each. They diverge in March, and only one of them
remembers who the customer is.

## ✅ The Fix

Derive every occurrence from the **anchor**, never from the last
occurrence:

```csharp
var billing = signup.AddMonths(cycleNumber);   // clamp per month, no memory loss
```

Full version in [Good.cs](Good.cs). The toolbox:

| Option | When it's the right call |
|---|---|
| `anchor.AddMonths(n)` | Recurring schedules - store the anchor and the cycle number |
| Store `(anchorDay, nextMonth)` explicitly | The schedule lives in a database - persist the *intent*, not the result |
| `DateOnly` for the anchor | Billing dates have no time and no timezone - don't invite either |

## 😈 The Even Worse Sibling

The same iteration, persisted: a `NextBillingDate` column updated each
cycle with `NextBillingDate.AddMonths(1)`. Now the drift is *data* -
spread across the fleet, survived every deploy, and unfixable without a
migration that has to guess what each customer's original day was. The
in-memory version of this bug loses information for a request; the
persisted version loses it for good.

## 🎓 Advanced Nuance

`AddMonths` is not invertible: `Jan 31 -> AddMonths(1) -> AddMonths(-1)`
lands on January **28**. Any round-trip logic ("shift the period forward,
then back") silently moves month-end dates. And a subtler consequence of
anchoring: customers who signed up on Jan 30 and Jan 31 both bill on
Feb 28, but anchor-based math keeps them distinct again in March (30th vs
31st) - iterative math would have merged them permanently. The anchor
doesn't just fix your schedule; it preserves the *identity* of every
schedule.

## 🔎 How to Find It in Your Codebase

- Grep `.AddMonths(` and check what it's applied to: a stored "previous"
  value in a loop or a `NextX` column is this exhibit; an anchor is fine.
- Any recurrence stored as "the next date" instead of "the rule + the
  anchor" - the schema itself is the smell.
- Test schedules with month-end anchors: Jan 31, Aug 31, Oct 31. Test
  data loves the 1st and the 15th, which is exactly why this survives
  every suite.
