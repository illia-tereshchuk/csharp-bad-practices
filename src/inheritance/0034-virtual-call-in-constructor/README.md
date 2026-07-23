---
id: "0034"
title: Calling a virtual method from a base constructor
category: inheritance
tags: [inheritance, constructors, virtual, construction-order]
author: alejandro-capel
rule: "never call a **virtual method** from a constructor - the override runs before its own fields are set"
---

# #0034 - Calling a virtual method from a base constructor

## 💥 Symptom

A regional billing plan is created with a multiplier of 3, and every order in
that region should get a 30% surcharge. Instead the surcharge is **zero** -
silently, for the whole life of the object. No exception at the call site, no
bad input: the constructor was handed `3`, the field holds `3` afterwards, and
yet the cached rate is `0.00`. The plan looks fully built and is quietly
misconfigured.

## 🔍 The Offending Code

```csharp
abstract class BillingPlan
{
    protected BillingPlan()
    {
        SurchargeRate = ComputeSurchargeRate(); // virtual call from the base ctor
    }
    public decimal SurchargeRate { get; }
    protected abstract decimal ComputeSurchargeRate();
}

class RegionalPlan : BillingPlan
{
    private readonly decimal _regionMultiplier;
    public RegionalPlan(decimal regionMultiplier) => _regionMultiplier = regionMultiplier;
    protected override decimal ComputeSurchargeRate() => 0.10m * _regionMultiplier;
}
```

## 🧠 What's Actually Going On

The order of construction in C# is fixed, and it is not the order you write:

1. the derived class's **field initializers** run,
2. the **base** constructor runs,
3. the derived constructor **body** runs.

The base constructor calls `ComputeSurchargeRate()`. That call is **virtual**,
so it dispatches to the *derived* override - there is no "half-constructed"
dispatch that stops at the base class. The override runs at step 2, reads
`_regionMultiplier`, and finds `0`: the field is assigned in the derived
constructor **body**, which is step 3 and has not happened yet. So
`0.10 * 0 = 0`, and that zero is frozen into `SurchargeRate` forever.

The trap is that this looks like clean OO - a template-method base class that
"primes" itself by asking the subclass. It reads as good design and is a
construction-order landmine.

## ✅ The Fix

Don't run overridable code during construction. Compute the value **on demand**,
after all constructor bodies have finished. Full version in [Good.cs](Good.cs) -
`SurchargeRate` becomes a computed property:

```csharp
public decimal SurchargeRate => ComputeSurchargeRate();
```

| Approach | When it's the right call |
|---|---|
| Compute on demand (property getter) | The value depends on subclass state. Default choice. |
| Pass the value into the base ctor as a parameter | The base genuinely needs it at construction and the subclass can supply it before `base(...)`. |
| `sealed` class + non-virtual method | There is no subclass to dispatch to - the call is safe, and the compiler agrees. |

## 😈 The Even Worse Sibling

Make the field a **reference type** instead of a `decimal`, and the override
dereferences a `null` at step 2 - a `NullReferenceException` thrown *from inside
a constructor*, before you even hold a reference to the object. That crash is
the **lucky** outcome: it is loud and points at construction. The `decimal`
version here is the nastier one - it never throws, and the object ships to
production computing every surcharge from a zero it was never given.

## 🎓 Advanced Nuance

There is one exception to "the fields are all default," and it surprises people
who came from Java. Fields set through **initializers** (`private readonly int _x
= 5;`) *are* already set when the base constructor runs, because initializers
are step 1. Only state assigned in the derived constructor **body** is missing.
Java is the opposite: there, the base constructor sees derived fields at their
default even for initializers, because Java runs derived initializers *after*
the base constructor. So the same "virtual call in a base constructor" bug
behaves differently across the two languages - a C# developer who learned the
rule as "all derived fields are null" is half wrong, and a Java developer
porting code will misjudge which fields are safe.

## 🔎 How to Find It in Your Codebase

- **Analyzer:** **CA2214** - "Do not call overridable methods in constructors."
  It flags exactly this pattern; promote it to a warning in `.editorconfig`:
  `dotnet_diagnostic.CA2214.severity = warning`.
- **Grep:** look inside constructor bodies for calls to `virtual` / `abstract`
  members of the same type - especially base classes named `*Base` with an
  `Initialize()` / `Configure()` / `Compute*()` the derived class overrides.
- **Design smell:** a base constructor that calls a method the subclass is meant
  to override ("template method in the constructor"). Move that call out of
  construction - to a factory method, an explicit `Initialize()` the caller
  invokes, or a lazily computed property.
