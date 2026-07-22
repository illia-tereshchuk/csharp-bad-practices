# 📁 io

> Status: **planned**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### readalltext-guesses-encoding (A5)

- **Twist:** Read a legacy-encoded file with File.ReadAllText and the text
  comes back mangled instead of failing - the decoder quietly substitutes
  replacement characters and the corruption ships downstream.
- **Mechanic:** ReadAllText defaults to UTF-8; bytes that aren't valid UTF-8
  decode to U+FFFD without any exception (the strict-throwing UTF8Encoding
  variant exists and is not the default). The damage happens at ingestion
  and is baked in by the first save.
- **Who hits it:** ingesting exports from legacy systems - bank CSVs,
  ERP dumps in Windows-125x encodings - on the .NET side that assumes UTF-8.
- **Repro:** BUILDER TIP: avoid the CodePages package - use
  `Encoding.Latin1` (built-in) to write "café" as Latin-1 bytes, read with
  ReadAllText: "caf�". Both encodings pinned in code, fully deterministic,
  no packages.
- **Damage:** customer names and addresses permanently corrupted in the
  database; the earliest anyone notices is a customer complaint, long after
  the original files are gone.
- **😈 seed:** strict decoding (`new UTF8Encoding(false, true)`) would have
  crashed honestly at ingestion - silence is the default, correctness is
  opt-in.
- **Verified:** documented decoder defaults; verify at build with the Latin1
  approach.

## Seeds

Not yet a full candidate - brainstorm before proposing.

- **io:** relative paths resolve against the current working directory, not
  the exe location - real (services, schedulers), but needs a framing that
  clears the primer floor.

- **utf8-bom-breaks-parser** (A5) - UTF-8-with-BOM prepends three invisible
  bytes; the JSON or CSV reader downstream treats them as part of the first
  field, and the parse fails on a file that looks identical in every editor.

- **read-without-seeking-to-start** (A5) - write to a stream, then read it
  back without seeking to position 0: the cursor is at the end, so the
  "round trip" returns empty with no error.

- **io:** `File.Exists` then `File.Open` TOCTOU, and `Directory.GetFiles`
  order not being guaranteed - both race- or environment-shaped; promote
  only with a hard assertion.
