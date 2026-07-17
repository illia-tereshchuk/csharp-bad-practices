---
id: "0002"
title: Calculating money with double
category: numbers
level: 🟢 junior trap
tags: [double, decimal, floating-point]
summary: "`0.1 + 0.2 != 0.3` — binary floats can't hold decimal cents, and the audit won't reconcile."
---

# #0002 — Calculating Money with Double

## 💥 Symptom

The daily payment report is off by a fraction of a cent, and the
reconciliation check refuses to sign it. Support digs in and finds invoices
where ten payments of 0.10 somehow don't add up to 1.00. Finance is not
amused, and neither is the auditor.

## 🔍 The Offending Code

```csharp
var payments = Enumerable.Repeat(0.10, 10); // ten dimes

double total = payments.Sum(); // 0.9999999999999999 — not a dollar
```

Reproduce:

```bash
dotnet run Bad.cs
```

## 🧠 What's Actually Going On

`double` is a **binary** floating-point type: it stores numbers as sums of
powers of two. Decimal fractions like 0.1 have no finite binary
representation — the same way 1/3 has no finite decimal one (0.3333…).

So what the code actually computes:

- `0.1` is stored as `0.1000000000000000055511151231257827…`
- add ten of those and the errors pile up to `0.9999999999999999`
- the literal `1.00` *is* exactly representable — so the audit comparison fails

Even the textbook one-liner misbehaves: `0.1 + 0.2` yields
`0.30000000000000004`, and `0.1 + 0.2 == 0.3` is `false` — the sum and the
literal round to *different* nearest doubles. Floating-point comparison is
exact bit-pattern comparison; close-but-not-equal is still `false`.

None of this is a .NET bug — it's the IEEE 754 standard, identical in every
mainstream language. The bug is picking a base-2 type for base-10 money.

## ✅ The Fix

Money is decimal by nature, so store it in `decimal` — a base-10
floating-point type designed exactly for this:

```csharp
var payments = Enumerable.Repeat(0.10m, 10); // ten dimes, in decimal

decimal total = payments.Sum(); // exactly 1.00
```

Full version in [Good.cs](Good.cs). The `m` suffix matters: `0.1` without it
is a `double` literal. Bonus: `decimal` preserves scale, so the total prints
as `1.00`, not `1` — the type literally remembers the cents.

| Type | The right job for it |
|---|---|
| `decimal` | Money, invoices, anything humans count in base 10 |
| `double` | Physics, geometry, statistics — tiny relative error is fine |
| `float` | Same niche as `double`, when memory beats precision |

And if you *must* compare doubles, compare with a tolerance, never with `==`.

## 😈 The Even Worse Sibling

Delete the reconciliation check and nobody notices anything: every
transaction drifts by a quadrillionth, thousands of transactions drift by
real cents, and the error surfaces months later in an annual report. The
crash in this exhibit is the *lucky* outcome.

## 🎓 Senior Nuance

`decimal` is not "infinite precision" — it's a 128-bit base-10 float with
28–29 significant digits. `1m / 3m * 3m` yields `0.9999999999999999999999999999`,
not `1`. It doesn't remove rounding; it makes rounding behave the way
accountants expect and represents 0.1 exactly. Division still needs an
explicit rounding policy — see `Math.Round` and `MidpointRounding`.

## 🔎 How to Find It in Your Codebase

- Search domain models and DTOs for `double` / `float` fields with money
  names: price, amount, total, balance, fee.
- Check the database too: `FLOAT` / `REAL` columns holding currency are the
  same crime one layer down.
- Any `==` between floating-point expressions is a red flag on its own.

## 📚 Dig Deeper

- [Floating-point numeric types — Microsoft Learn](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types)
- [What Every Computer Scientist Should Know About Floating-Point Arithmetic](https://docs.oracle.com/cd/E19957-01/806-3568/ncg_goldberg.html)
