# 🔤 regex

> Status: **planned**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### missing-anchors-pass-anything (A5)

- **Twist:** A "digits only" check without anchors accepts abc123def -
  IsMatch looks for a match *anywhere* - and even the anchored `^\d+$`
  still accepts "123\n", because `$` matches before a final newline.
- **Mechanic:** IsMatch answers "does a match exist somewhere in the
  input" - validation semantics require `^...$`. Second layer, verified:
  `$` (and `Z`) match before a string-final `\n`; only `\z` means true
  end-of-string. So the standard fix still lets a trailing newline through.
- **Who hits it:** input validation on ids, amounts, codes - regexes copied
  from a matcher context into a validator context.
- **Repro:** `Regex.IsMatch("abc123def", @"\d+")` true;
  `Regex.IsMatch("123\n", @"^\d+$")` ALSO true; only `@"^\d+\z"` rejects
  both. Deterministic, no packages.
- **Damage:** validated input that still carries payloads (injection
  prefixes, trailing newlines corrupting line-based formats downstream) -
  the check exists, reviewed, and passes garbage.
- **Verified:** ran on .NET 10 (2026-07-22): `^\d+$` accepted "123\n".

## Seeds

Not yet a full candidate - brainstorm before proposing.

- **dot-misses-newline** (A5) - `.` does not match `\n` by default, so a
  clean-looking validator passes an input whose second line is the payload -
  the check only ever saw the first line.

- **unescaped-regex-input** (A4,5) - building a pattern from user text
  without Regex.Escape turns their `.` into "any char" and their `(` into a
  runtime ArgumentException.

- **slash-d-matches-unicode-digits** (A5) - `\d` matches every Unicode
  decimal digit, so a "digits only" check accepts Arabic-Indic or fullwidth
  numerals that blow up int.Parse two layers downstream.
