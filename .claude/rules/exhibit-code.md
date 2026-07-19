---
description: How to write Bad.cs / Good.cs - the mirror, the self-audit throw, determinism, escape hatches. Auto-loads when editing exhibit C#.
paths:
  - "src/**/*.cs"
---

# Exhibit code conventions (Bad.cs / Good.cs)

## Shape

- **Single file, top-level statements.** No `.csproj`. Pull NuGet with a
  `#:package Name@ver` directive at the top of the file.
- **Believable domain.** OrderService, kiosks, a leaderboard, subscribers -
  never `Foo`/`Bar`. The reader should recognize their own codebase.

## The mirror rule

`Good.cs` is `Bad.cs` with the same domain, data, scenario, and line count. The
**only** visible difference is the approach - the reader diffs them by eye. Keep
them symmetric down to the variable names.

## Visible failure via self-audit throw

The signature pattern: the program prints believable output, then asserts the
invariant and throws a labeled reason when it breaks. The reader sees both the
wrong result *and* why it's wrong.

```csharp
if (printed != headerCount)
    throw new InvalidOperationException(
        $"header promised {headerCount}, body printed {printed}");

Console.WriteLine("Header and body agree. Report shipped."); // Good.cs reaches this
```

- **Never throw bare `Exception`** - use `InvalidOperationException` and
  friends. This repo is under a microscope; a bad practice in `Good.cs` is a
  comment goldmine against us. The fix must be CA-clean.
- **`Good.cs` ends on a short plain closing line** ("... As it should be.") -
  it's part of the show, but keep it boring. Jokes belong in the README.

## Determinism

Failure must fire on **every** run:

- Races: loop to 100k+ iterations so the interleaving is reliable.
- Never rely on timing ("trust me, it's slow" is banned - there's no assertion).
- Never depend on machine culture/timezone unless the code pins it explicitly,
  or CI will disagree with your machine.

## Comments

Plain and boring. State a non-obvious constraint, not wit. The `// 💥` marker on
the offending line is welcome; prose wordplay is not.

## Escape hatches (learned the hard way)

- **Reflection / dynamic-code exhibits need `#:property PublishAot=false`.**
  File-based apps default to Native AOT semantics, which disable EF Core model
  building and reflection-based System.Text.Json. Add the directive
  preemptively whenever the exhibit touches reflection.
- **Transitive CVE (NU1903):** if a pulled package flags an advisory, bump the
  offending transitive explicitly. A CVE in the fix is too ironic to ship.
- **First run of a package exhibit** downloads NuGet (~1 min); warn the curator.

## Verify before handing off

Run **both** files. `Bad.cs` must fail exactly as the README promises; `Good.cs`
must pass. Paste the real output - never claim it works unseen. Then, from the
repo root, run `dotnet run tools/check-links.cs`.
