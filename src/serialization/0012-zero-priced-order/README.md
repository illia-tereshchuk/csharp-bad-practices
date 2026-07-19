---
id: "0012"
title: A JSON payload that maps to nothing
category: serialization
tags: [System.Text.Json, deserialization, case-sensitivity]
rule: "never deserialize without pinning the naming contract"
---

# #0012 - A JSON Payload That Maps to Nothing

## 💥 Symptom

The payment webhook "processed successfully": no exception, HTTP 200, order
shipped. The recorded amount is 0.00 and the customer name is empty. The
books stop balancing at month end. The cruelest detail: the API controllers
handle the *exact same JSON* perfectly - only the background worker with a
hand-rolled `Deserialize` call produces zeroes.

## 🔍 The Offending Code

```csharp
var json = """{ "orderId": 1042, "amount": 149.99 }""";

var order = JsonSerializer.Deserialize<Order>(json)!;

order.Amount;   // 0. The JSON had "amount"; the class has Amount

class Order
{
    public decimal Amount { get; set; }
}
```

## 🧠 What's Actually Going On

`System.Text.Json` matches property names **exactly and case-sensitively**
by default. Two silent behaviors then stack:

1. a JSON key with no matching property is *skipped* - tolerant reading is
   by design;
2. a property with no matching key keeps `default(T)` - also by design.

A camelCase payload against a PascalCase class is a 100% miss, and the two
silences combine into a "successful" deserialization of nothing.

Why do controllers survive? ASP.NET Core configures
`JsonSerializerDefaults.Web` under the hood: camelCase naming policy plus
case-insensitive matching. A manual `JsonSerializer.Deserialize` call gets
the strict defaults instead. Same library, two personalities - which is
why this bug lives exactly where serialization is done by hand: webhooks,
queue consumers, `HttpClient` calls, cache layers.

## ✅ The Fix

Pin the contract instead of hoping the names align:

```csharp
var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
var order = JsonSerializer.Deserialize<Order>(json, options)!;
```

Full version in [Good.cs](Good.cs) - it wears belt *and* suspenders: `Web`
defaults for matching, `required` on every property so missing data throws
instead of defaulting. The toolbox:

| Option | When it's the right call |
|---|---|
| `JsonSerializerDefaults.Web` | Hand-rolled deserialization of web-shaped JSON; matches ASP.NET behavior |
| `[JsonPropertyName("...")]` | Names no naming policy can derive; pins the contract explicitly |
| `required` properties | Missing data becomes a loud `JsonException`, not a silent 0 |
| `[JsonUnmappedMemberHandling(Disallow)]` | Unknown JSON keys throw too - tolerant reading, revoked (.NET 8+) |

One habit on top: reuse `JsonSerializerOptions` in a `static readonly`
field - constructing options per call quietly costs performance.

## 😈 The Even Worse Sibling

The Newtonsoft migration. `Json.NET` matched names case-insensitively by
default; `System.Text.Json` does not. A service "migrated without changes"
compiles, and its unit tests stay green - because tests love round-trips,
and STJ deserializes its own PascalCase output flawlessly. Only real
camelCase payloads from the outside world turn into zeroes. A test suite
that serializes its own fixtures cannot see this bug *by construction*.

## 🎓 Advanced Nuance

`required` (C# 11) is enforced by the serializer: a missing required member
fails with a `JsonException` that lists the absent properties - the crash
moves to the boundary, where it's cheap and obvious. And a meta-note: this
exhibit needs `#:property PublishAot=false`, because under Native AOT
defaults reflection-based serialization is disabled entirely - the
long-term cure for that is source generation (`JsonSerializerContext`),
which also happens to make the naming contract explicit at compile time.

## 🔎 How to Find It in Your Codebase

- Grep `JsonSerializer.Deserialize` and `.Serialize` calls with **no
  options argument** - each one is running on strict defaults.
- DTOs for external payloads without `required` or `[JsonPropertyName]`.
- Integration tests must use *captured real payloads*, never round-tripped
  fixtures - the round-trip hides exactly this class of bug.
- Turn on `JsonUnmappedMemberHandling.Disallow` in dev builds to surface
  contract drift the moment it happens.
