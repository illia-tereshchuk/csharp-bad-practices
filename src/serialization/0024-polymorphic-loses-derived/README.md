---
id: "0024"
title: Serializing through a base-typed reference
category: serialization
tags: [System.Text.Json, polymorphism, inheritance]
rule: "never serialize a polymorphic value without declaring the hierarchy"
---

# #0024 - Serializing Through a Base-Typed Reference

## 💥 Symptom

Customers receive receipts with no amount on them. The webhook your partner
consumes is missing half its fields. Nothing failed: the API returned 200, the
JSON is valid, the logs are clean, and the object in the debugger has every
property filled in right up to the moment it's written. The data doesn't get
corrupted - it never leaves the building.

## 🔍 The Offending Code

```csharp
class Receipt
{
    public PaymentMethod Payment { get; set; }   // declared as the base type
}

var json = JsonSerializer.Serialize(receipt);
// {"OrderId":1042,"Payment":{"Provider":"Visa"}}   - no card, no amount
```

## 🧠 What's Actually Going On

`System.Text.Json` serializes by the **declared** type, not the runtime one. The
property says `PaymentMethod`, so the serializer writes exactly the contract
`PaymentMethod` advertises - `Provider` - and never looks at the `CardPayment`
sitting in there.

That is a deliberate design decision, not an oversight. Writing whatever
happens to be in the field at runtime would mean an object could silently widen
its own public contract: attach a derived type with an `InternalNotes` or
`ApiKey` property and it ships to the client. The serializer refuses to leak
what the declared type didn't promise.

So nothing is "lost" - the derived data was never requested. And because
omitting properties is normal JSON behavior (there is no such thing as a
missing-field error on write), the failure has no place to surface. Same family
as [0012-zero-priced-order](../../serialization/0012-zero-priced-order/): the
serializer's defaults are safe and silent, and silent is what hurts.

## ✅ The Fix

Declare the hierarchy so the serializer is allowed to write the runtime shape:

```csharp
[JsonDerivedType(typeof(CardPayment), "card")]
class PaymentMethod { ... }

// {"Payment":{"$type":"card","Last4":"4242","Amount":149.99,"Provider":"Visa"}}
```

Full version in [Good.cs](Good.cs). The `$type` discriminator is what lets the
value come *back* as a `CardPayment` too. The toolbox:

| Option | When it's the right call |
|---|---|
| `[JsonDerivedType]` on the base (.NET 7+) | The default. Explicit, round-trips, works for collections and nested properties |
| `Serialize(obj, obj.GetType())` | One-off write where you control the call site and don't need to deserialize back |
| Don't be polymorphic at the boundary | Map to a flat DTO that has every field. The wire contract stops depending on your class hierarchy |

## 😈 The Even Worse Sibling

The version that passes every test. Unit tests serialize the concrete object -
`Serialize(cardPayment)` - where the generic argument *is* `CardPayment`, so all
the data appears and the assertion is green. Only production serializes through
the base-typed property. Now scale it: a `List<PaymentMethod>` of mixed kinds
flattens every element to its base shape at once, and the endpoint that returns
"all payment methods" returns a list of near-empty objects with a 200.

## 🎓 Advanced Nuance

Change the property's declared type from `PaymentMethod` to `object` and the
derived data comes back - for `object`, the serializer resolves the runtime type
instead. So two properties in the same class can follow opposite rules, and the
weaker-typed one is the one that keeps your data. That asymmetry is the tell
that this is a *contract* decision, not a capability gap.

One more: the discriminator is not decoration. Without `$type` in the payload,
deserialization has no way to pick the derived type, so a "fixed" writer paired
with an old reader still round-trips into a base instance. Fix both ends, or the
data survives the write and dies on the read.

## 🔎 How to Find It in Your Codebase

- Any DTO property, collection element, or return type declared as a base class
  or interface and handed to a serializer - that declaration *is* the contract.
- Serialization tests that pass the concrete type directly. Make the test
  serialize through the same declared type production uses, or it cannot see
  this bug.
- Diff a real captured payload against the object you think you sent. Missing
  properties with no error is the signature.
