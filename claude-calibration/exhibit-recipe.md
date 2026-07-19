# Exhibit recipe

The proven anatomy after 9 exhibits. This is the generation template - follow
it and the output fits without rework.

## Files per exhibit

`src/<hall>/<NNNN>-<slug>/` containing `Bad.cs`, `Good.cs`, `README.md`.
Plus one row on the front-page README table for that hall.

## Bad.cs / Good.cs

- **Single file, top-level statements.** No .csproj. NuGet via `#:package Name@ver`.
- **Mirror rule.** Good.cs is Bad.cs with the same domain, data, scenario, line count. The ONLY visible diff is the approach. Reader diffs them by eye.
- **Visible failure via self-audit throw.** This is the signature pattern I use: the program prints the believable output, then asserts the invariant and `throw`s when it breaks. Reader sees both the wrong result AND a labeled reason. Example:
  ```csharp
  if (printed != headerCount)
      throw new InvalidOperationException($"header promised {headerCount}, body printed {printed}");
  Console.WriteLine("Header and body agree. Report shipped."); // Good.cs reaches this
  ```
- **Never throw bare `Exception`.** Use `InvalidOperationException` etc. This repo is under a microscope; a bad practice in Good.cs = a comment goldmine against us. (CA-level cleanliness in the fix.)
- **Believable domain.** OrderService / kiosks / leaderboard / subscribers. Never Foo/Bar.
- **Determinism.** Failure must fire every run. Races: loop to 100k+. Never rely on timing ("trust me it's slow" = banned). Never depend on machine culture/timezone unless the code pins it explicitly (else CI lies).
- **Good.cs prints a short closing line** ("... As it should be.") - part of the show; output is screenshot-fodder. Keep it plain, no wordplay (jokes go in README).

## README.md structure (fixed order)

Front-matter (YAML) first - the future TOC generator reads it:
```yaml
---
id: "0009"                 # quoted string, keeps leading zeros
title: Enumerating a LINQ query twice
category: linq             # == hall folder
level: 🟡                  # bare emoji, no words
tags: [LINQ, IEnumerable, deferred-execution]
summary: "one line on what breaks - kept for the future index (curator edits these)"
rule: "never enumerate a LINQ query twice - materialize it once"  # == the front-page cell; lowercase "never"
---
```
Then sections (😈 and 🎓 optional but strongly preferred - they carry the senior audience):
1. `# #NNNN - Title`
2. `## 💥 Symptom` - the production pain, not theory. "oh, I've seen this."
3. `## 🔍 The Offending Code` - minimal incriminating snippet. NO "Reproduce" block.
4. `## 🧠 What's Actually Going On` - the mechanic. The educational core.
5. `## ✅ The Fix` - idiomatic fix + `[Good.cs](Good.cs)` + a "when to use which" table.
6. `## 😈 The Even Worse Sibling` - the silent/nastier variant. Recurring punchline: "the crash in this exhibit is the *lucky* outcome."
7. `## 🎓 Senior Nuance` - the twist that surprises experts (version history, edge case, myth-bust).
8. `## 🔎 How to Find It in Your Codebase` - grep patterns, analyzer IDs (CA2200, VSTHRD100), IDE inspections, .editorconfig recipes. This is the LAST section - no link sections after it (Dig Deeper was removed by curator's call 2026-07-18: "nobody opens them").

**Cross-references must be clickable links, never bare `#NNNN`** (curator's
call 2026-07-19: hunting numbers by hand is unusable). Write them as
`[NNNN-slug](../../<hall>/<NNNN-slug>/)` - the `../../` form works from any
exhibit README, same hall or not. The exhibit's own H1 keeps the bare
`# #NNNN - Title` form. Non-exhibit numbers in prose (order #1002, prices)
stay bare.

## Front-page row

Add under the right hall's table (create the hall section if new). Tables are
**headerless**: an empty header row + delimiter (GFM can't render a table
without them), then data rows. The last column is the exhibit's "Never ..."
commandment, == front-matter `rule`:
```
| | | | |
|--:|---|---|---|
| 0009 | [Enumerating a LINQ query twice](src/linq/0009-multiple-enumeration/) | 🟡 | never enumerate a LINQ query twice - materialize it once |
```
Then bump the stats line: `**N** exhibits in **M** halls, latest addition - **#NNNN**`.
Link sits on the TITLE, not the number.

Hall emojis in use: 🗂 collections · 🔢 numbers · ⚡ async · 🔗 linq · 💥 exceptions · 🗄 orm.

## Escape hatches learned the hard way

- **Anything reflection/dynamic-code based needs `#:property PublishAot=false`:** file-based apps default to Native AOT semantics. Known victims: EF Core model building (#0008), System.Text.Json reflection-based (de)serialization (#0012). Add the directive preemptively whenever the exhibit touches reflection.
- **Transitive CVE (NU1903):** if a pulled package flags an advisory, bump the offending transitive explicitly (`#:package SQLitePCLRaw.bundle_e_sqlite3@2.*` -> 2.1.12). A CVE in the fix is too ironic to ship.
- **First run of a package exhibit** downloads NuGet (~1 min). Warn the curator; subsequent runs cache.

## Verify before handing off

Always run both files. Bad must fail exactly as the README promises; Good must pass. Paste the real output. Never claim it works unseen.
