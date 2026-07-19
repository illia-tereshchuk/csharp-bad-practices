---
id: "XXXX"
title: Name the crime in a few words
category: collections
tags: [TypeInvolved, MethodInvolved]
rule: "never <the commandment this exhibit teaches> - this goes to the front-page table"
---

# #XXXX - Title of the Exhibit

## 💥 Symptom

What the victim sees in production. Start with the pain, not the theory:
weird logs, wrong totals, a service that dies at 3 AM. Make the reader say
"oh, I've seen this".

## 🔍 The Offending Code

```csharp
// The minimal incriminating snippet - a few lines, not the whole file
```

## 🧠 What's Actually Going On

The mechanics. Why does the runtime / compiler / library behave this way?
Which wrong assumption does the author of this code hold? This section is
the educational core - be precise, link to docs if needed.

## ✅ The Fix

The idiomatic solution and *why* it wins. Full version in [Good.cs](Good.cs).
If there are several valid options, show a table: option → when it's the
right call.

## 😈 The Even Worse Sibling *(optional)*

A related variant that is even nastier - e.g. one that fails silently
instead of crashing.

## 🎓 Advanced Nuance *(optional)*

The detail that surprises even experienced folks: version-specific behavior,
a counterintuitive edge case, a myth to bust.

## 🔎 How to Find It in Your Codebase

Concrete detection advice: what to grep for, what analyzers/IDE inspections
flag it, what to watch for in code review.
