# Backlog

Candidate exhibits, waiting to be picked. This folder is the cache that
`propose-exhibits` and `contribute` read. It is written to be self-sufficient:
a fresh session - possibly a different, smaller model - must be able to render
the menu and build any candidate from these files alone, without re-deriving
the mechanics. When in doubt, spell things out; these files are allowed to be
long.

## Layout

**One file per hall**, named by the hall slug: `async.md`, `security.md`,
`value-types.md`, and so on. Each file holds every candidate for that hall, and
(at the bottom, under `## Seeds`) that hall's not-yet-vetted ideas. This split
exists so additions to a single hall are a single-file diff - commit one hall
at a time.

- The canonical hall registry - emoji badge, display name, opened/planned
  status - is `.claude/memory/halls.md`, not here. Each file repeats its emoji
  and status in the title line for convenience, but `halls.md` is the source of
  truth; if they ever disagree, `halls.md` wins.
- A hall with no candidates yet has no file. Adding the first candidate to a
  planned hall creates its file (and the `add-exhibit` flow flips the hall to
  opened in `halls.md` when it actually ships).

## How to read an entry

Each candidate is one `###` block, headed `### <slug> (A n)`. The `(A n)` is
the archetype tag from `archetypes.md` - internal curation data, never shown in
any menu. Fields:

- **Twist** - the one-line hook. Menus show this line verbatim after the slug.
  Keep it under ~35 words, mechanic first.
- **Mechanic** - what precisely happens and why, so the builder does not have
  to rediscover the behavior. Include known compiler/API gotchas that would
  trip a naive Bad.cs.
- **Who hits it** - who / where / how in the real world. If this cannot be
  answered, the candidate does not belong here (vacuum-example rule).
- **Repro** - how Bad.cs fails deterministically in one console file: the
  approach, plus any `#:package` / `#:property` directives known to be needed.
  Note: file-based `dotnet run` uses an AOT profile by default, so anything
  using reflection-based JSON needs `#:property PublishAot=false` (precedent:
  #0012). DI needs `#:package Microsoft.Extensions.DependencyInjection@10.*`;
  EF/SQLite setup is in #0008's files.
- **Damage** - what the reader actually loses. "Reproduces a quirk" is not
  damage (no-real-damage rule; that is what killed sort-is-unstable).
- **😈 seed** (optional) - the one-rung-nastier angle for the README's 😈
  section (fear ladder: crash < wrong < silently wrong).
- **Verified** - what was actually executed versus taken from documentation.
  "ran on .NET 10 (date)" means the core premise was proven by running real
  code that day. Never write that without having run it: two past rejections
  (`datetime-kind-round-trip`, `firstordefault-on-structs`) were premise
  errors that running code would have caught.

## Maintenance rules

- Everything here already passed the curation filters: not primer-level, not
  a vacuum scenario, deterministic in a single console file, not proven by
  timing, not dependent on an unpinned environment, absent from `rejected.md`.
- Borderline-by-taste candidates are still included, flagged in the entry -
  the bar says: propose and let the curator judge, don't pre-cut.
- When the curator declines a candidate: record it in `rejected.md` and delete
  its block from the hall file **in the same edit**. The two must never
  disagree.
- When an exhibit ships: delete its block from the hall file, update
  `state.md`, and (if it opened the hall) flip the hall in `halls.md`.
- Before adding a new candidate: run its core premise as real code (a
  scratchpad file is fine) and record the result in **Verified**. Add it to the
  matching hall file, creating that file if the hall had none yet.
