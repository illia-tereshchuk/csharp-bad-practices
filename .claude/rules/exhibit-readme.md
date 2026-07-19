---
description: How to write an exhibit README - structure, front-matter, cross-links, tone. Auto-loads when editing an exhibit README.
paths:
  - "src/**/README.md"
---

# Exhibit README conventions

## Front-matter (YAML, first thing in the file)

```yaml
---
id: "0009"                 # quoted string, keeps leading zeros
title: Enumerating a LINQ query twice
category: linq             # == the hall folder name
level: 🟡                  # bare emoji only - no words like "junior trap"
tags: [LINQ, IEnumerable, deferred-execution]
summary: "one line on what breaks - kept for the future index"
rule: "never enumerate a LINQ query twice - materialize it once"
---
```

- `rule` is the exhibit's commandment - lowercase `never ...`. It is copied
  verbatim into the front-page table cell.
- `summary` trends short (curator edits these himself - don't over-write).

## Section order (fixed)

1. `# #NNNN - Title` (this H1 keeps the bare `#NNNN` form)
2. `## 💥 Symptom` - the production pain, not theory. Make the reader say
   "oh, I've seen this."
3. `## 🔍 The Offending Code` - the minimal incriminating snippet. No
   "Reproduce: dotnet run" block - the run command is obvious.
4. `## 🧠 What's Actually Going On` - the mechanic. The educational core.
5. `## ✅ The Fix` - idiomatic fix + `[Good.cs](Good.cs)` + a "when is this the
   right call" table.
6. `## 😈 The Even Worse Sibling` *(optional, strongly preferred)* - the
   silent/nastier variant. Recurring punchline: "the crash in this exhibit is
   the *lucky* outcome."
7. `## 🎓 Senior Nuance` *(optional, strongly preferred)* - the twist that
   surprises experts: version history, an edge case, a myth to bust. These two
   optional sections carry the senior audience - include them when you can.
8. `## 🔎 How to Find It in Your Codebase` - grep patterns, analyzer IDs
   (CA2200, VSTHRD100), IDE inspections, `.editorconfig` recipes.

**This is the LAST section.** No "Dig Deeper" or external-link section after it
- nobody opens them.

## Cross-references

Reference another exhibit as a clickable link, never a bare `#NNNN` (the number
alone is unsearchable by hand):

```
[0010-immortal-subscriber](../../events/0010-immortal-subscriber/)
```

The `../../<hall>/<NNNN-slug>/` form works from any exhibit README. Non-exhibit
numbers in prose (order #1002, prices) stay bare. `tools/check-links.cs`
enforces this.

## Tone

Restrained "lab / museum" voice. Jokes and punchlines are saved for LinkedIn,
not the repo. The 💥 / 😈 / 🎓 section markers carry the personality; the prose
stays informative.

## Front-page row (added to the root README, done with every exhibit)

Under the hall's table (create the hall section if new). Tables are
**headerless** - an empty header row keeps GitHub rendering them as tables:

```
| | | | |
|--:|---|---|---|
| 0009 | [Enumerating a LINQ query twice](src/linq/0009-multiple-enumeration/) | 🟡 | never enumerate a LINQ query twice - materialize it once |
```

Last column == the front-matter `rule`. The link sits on the **title**, not the
number. Then bump the stats line:
`**N** exhibits in **M** halls, latest addition - **#NNNN**`. The front page
stays minimal - no "How to Run", no "Contributing", no disclaimer.
