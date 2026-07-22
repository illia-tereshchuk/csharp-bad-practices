# 🧵 strings

> Status: **planned**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### length-lies-about-emoji (A4)

- **Twist:** `"👍".Length` is 2 - so a 50-character truncate slices an emoji
  in half and sends a lone surrogate to production, where it renders as �
  or breaks the downstream encoder.
- **Mechanic:** Length counts UTF-16 code units; astral-plane characters
  (all modern emoji) take two (a surrogate pair), ZWJ sequences take many.
  `Substring` cuts between units without complaint, producing an invalid
  lone surrogate. Honest APIs: `EnumerateRunes`, `StringInfo` (text
  elements).
- **Who hits it:** truncation for column widths, SMS limits, UI previews -
  any `s.Substring(0, 50)` over user-generated text, which now always
  contains emoji.
- **Repro:** show `"👍".Length == 2`; truncate a string mid-pair; print the
  result and re-encode to UTF-8 to show the replacement. Deterministic, no
  packages.
- **Damage:** corrupted text stored and re-served; downstream strict systems
  (JSON encoders, databases) reject or mangle the payload - a data-quality
  bug seeded by an innocent-looking truncate.
- **😈 seed:** the family emoji `"👨‍👩‍👧‍👦".Length` is 11 - "one character" by
  any human count, eleven by the API the code trusts.
- **Verified:** UTF-16 representation facts; verify at build.

### mojibake-factory (A4)

- **Twist:** Decode bytes with the wrong encoding, save the result, and the
  text is gone for good: "Привіт" becomes "ÐŸÑ€Ð¸Ð²Ñ–Ñ‚" - readable proof
  of what one wrong round-trip does.
- **Mechanic:** UTF-8 bytes read as Latin-1/1252 turn each multi-byte
  character into two garbage-but-valid characters; the mistake is invisible
  to the type system (it's all "string") and *reversible* until the first
  save re-encodes the garbage as genuine UTF-8 - then the original bytes no
  longer exist.
- **Who hits it:** any boundary with a charset assumption: files, HTTP
  bodies, DB connections - plus the "fix" where someone saves the mangled
  text back "corrected".
- **Repro:** pin both encodings in code (UTF-8 bytes of a Ukrainian string,
  decoded as `Encoding.Latin1`): print the mojibake; round-trip once more to
  show the point of no return. Deterministic, no packages, no CodePages
  needed with Latin1.
- **Damage:** permanent corruption of every non-ASCII name in the batch -
  and the demo doubles as the museum's most shareable screenshot.
- **BUILDER NOTE:** adjacent to readalltext-guesses-encoding (silent U+FFFD
  substitution vs. reversible-then-baked mojibake). Both can live in the
  hall, but cross-link and keep the mechanics distinct; if only one is
  wanted, the curator picks.
- **Verified:** encoding math; verify at build.

## Seeds

Not yet a full candidate - brainstorm before proposing.

- **trim-is-a-charset-not-a-prefix** (A4,5) -
  `url.TrimStart("https://".ToCharArray())` strips any leading character in
  that *set*, not the prefix - a URL starting with 's', 't', 'p' or '/'
  gets eaten letter by letter.

- **unnormalized-strings-not-equal** (A2,4) - "é" as one code point and as
  "e" plus combining accent print identically but ordinal `==` says they
  differ - a uniqueness check waves through a duplicate it cannot see.

- **split-keeps-empty-entries** (A5) - `"a,,b".Split(',')` returns three
  items, not two: the empty middle field is kept by default, so a positional
  parser reads every later column shifted by one.

- **interpolation-is-culture-sensitive** (A4) - `$"{amount}"` formats with
  the current culture, so the same code builds "1,50" or "1.50" per machine -
  pin two cultures explicitly to stay CI-honest.
