---
id: "0011"
title: A mutable struct behind a readonly field
category: value-types
level: 🔴
tags: [structs, readonly, defensive-copy]
summary: "every method call on the readonly field runs on a hidden copy - points added, balance forever 0"
rule: "Never write a mutable struct"
---

# #0011 - A Mutable Struct Behind a Readonly Field

## 💥 Symptom

Loyalty points never accrue. The step counter stays at zero. No exception,
ever - the mutation *runs*, completes, and changes nothing. The code passed
review, because `readonly` on a field looks like a virtue, and the struct
looks like an innocent little value holder. Debugging is maddening: step
into `Add`, watch `_value` become 100... then watch the field still say 0.

## 🔍 The Offending Code

```csharp
class LoyaltyCard
{
    private readonly PointsBalance _balance;   // readonly field...

    public void Register(int points) => _balance.Add(points); // ...mutates a copy
}

struct PointsBalance
{
    private int _value;
    public void Add(int points) => _value += points;  // ...a mutable struct
}
```

## 🧠 What's Actually Going On

`readonly` is a promise: this field's *value* never changes after
construction. For a class-typed field the value is just a reference, and
the promise is cheap. For a struct the field **is** the data - so calling
`Add` on it directly would break the promise.

The compiler resolves the conflict quietly: it cannot know whether a
method mutates the struct, so before *any* member call on a readonly
struct field it makes a **defensive copy** and calls the method on that.
`Add` runs, `_value += points` happens - on a temporary that dies at the
end of the statement. The field keeps its promise. Your points keep dying.

No warning, no error. The mutation is legal, complete, and pointless.

## ✅ The Fix

Stop mutating structs; replace them. A `readonly struct` makes the
compiler enforce it, and the mutation becomes an explicit reassignment of
the field:

```csharp
class LoyaltyCard
{
    private PointsBalance _balance;  // the field mutates; the struct never does

    public void Register(int points) => _balance = _balance.Add(points);
}

readonly struct PointsBalance
{
    public PointsBalance Add(int points) => new(_value + points);
}
```

Full version in [Good.cs](Good.cs). The toolbox:

| Option | When it's the right call |
|---|---|
| `readonly struct` + methods returning new values | The default for any struct you write |
| Just use a class | The type has identity or you want shared mutable state |
| `readonly` members on a legacy struct (C# 8) | Can't rewrite the struct; mark non-mutating members so copies disappear |

Bonus: for a `readonly struct`, the compiler *skips* defensive copies
entirely - the readonly field and the readonly struct are a happy,
copy-free couple.

## 😈 The Even Worse Sibling

```csharp
card.Balance.Add(100);   // Balance is a property returning the struct
```

Property getters return structs **by value**, so this mutates a copy too.
The cruel part: the compiler catches the assignment form -
`card.Balance.Value = 5` is error CS1612 - but lets the method call sail
through. Same bug, and this one doesn't even need `readonly` to bite.

## 🎓 Senior Nuance

Defensive copies are also a silent performance tax: they happen for
`in` parameters and `ref readonly` locals too, and a fat struct copied on
every member access in a hot loop shows up in profilers as "time spent
nowhere". C# 8's `readonly` members exist precisely to declare, member by
member, "this call is safe, skip the copy". And note: none of this is JIT
magic - it's compile-time semantics, identical in Debug and Release.

## 🔎 How to Find It in Your Codebase

- Any `struct` with a method or setter that writes a field - mutable
  structs are the root crime; `readonly struct` should be your default.
- Rider / ReSharper: "Impure method is called for readonly field of value
  type" - this exhibit as a single inspection. Treat it as an error.
- Grep `readonly` fields whose type is a struct you own, then check what
  gets called on them.

## 📚 Dig Deeper

- [Write safe and efficient C# code - Microsoft Learn](https://learn.microsoft.com/dotnet/csharp/write-safe-efficient-code)
- [Choosing between class and struct - Microsoft Learn](https://learn.microsoft.com/dotnet/standard/design-guidelines/choosing-between-class-and-struct)
