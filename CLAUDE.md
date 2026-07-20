# C# Mistakes Explained - project guide

A museum of small, runnable C# bad-practice exhibits. Each teaches one broken
mental model: a `Bad.cs` that fails when you run it, a mirror `Good.cs` that
does the same job right, and a `README.md` that explains the mechanic.

## How we work

- **I generate a full exhibit** (Bad.cs + Good.cs + README.md + the front-page
  row + calibration updates), **verify it by running both files**, and leave it
  uncommitted. The curator reads, tests, and commits - or tells me to commit.
- Detailed conventions load automatically when I edit exhibit files (see
  `.claude/rules/`). The end-to-end workflow lives in the `add-exhibit` skill.

## Always-on conventions

- **English** for everything committed (code, comments, docs, commit messages).
  Ukrainian stays in chat.
- **Hyphens only** - never em/en dashes, anywhere.
- **Curated by one person, open to contributors.** Outside developers add
  exhibits through the `contribute` skill and a pull request, on a branch in
  this repo (they are added as collaborators - no forks). Numbers come from
  `next-id`; the curator verifies them at review and reviews every exhibit
  personally. The
  curation memory (`rejected.md` and the taste it encodes) is his editorial
  record - use it to filter what you propose, never present or explain it to a
  contributor.
- **Commits:** imperative subject <=50 chars, no trailing period; body explains
  *why*; one logical change per commit; trailer
  `Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>`. Exhibit commits are
  `Add exhibit #NNNN: <slug>`.

## Curation bar (when proposing exhibits)

The curator's taste is **subjective and case-by-case** - it lives in his
individual picks and rejections, not in categories. The `reject-exhibit` skill
and `.claude/memory/rejected.md` are the main channel for learning it: study
which specific cases he declines and why, and pre-filter future proposals
accordingly. The bar below is the current distillation.

- **One exhibit = one broken mental model.** Not a typo, not a whole
  architecture review. If explaining it needs two different "why"s, it's two
  exhibits.
- **Reject predictable finales.** If the reader guesses the outcome from the
  title, it's a listicle item, not an exhibit. An exhibit needs a mechanic
  twist - a "wait, WHAT?" even for someone who knows the bug.
- **Mind the floor and the ceiling.** A *popular* encyclopedia for marketing and
  self-teaching. Floor: primer-level bugs everyone knows from day one (integer
  overflow, null reference). Ceiling: not topic depth - deep topics are welcome -
  but digestibility. Too high = can't be swallowed in ~10 minutes, or needs too
  many non-obvious interconnected parts before you can even see where it
  reproduces. Keep it graspable and relevant, leaving one memory: "you must not
  do this."
- **The sweet spot is a non-obvious puzzle that still reproduces.** The whole art
  is the balance between primer and deep-weeds: enough of a head-scratcher to be
  a puzzle, concrete enough to run and watch fail. That balance is subjective -
  when a candidate is borderline, propose it and let the curator judge rather
  than pre-cutting it.
- **Prefer accessible + axiomatic:** everyday contracts everyone touches, with a
  rule that reads like an axiom (`never write async void outside event
  handlers`). Favor these over exotic API footguns.
- **A bug must answer who / where / how.** If you can't say who hits it, from
  where, and how, it's a vacuum example - not reproducible in any real context,
  so not interesting (this is why path-combine-betrayal was cut).
- **Prefer silent-wrongness and money/audit stakes** - they screenshot well and
  land emotionally.

## Tools (run from repo root)

- `dotnet run tools/next-id.cs` - next free exhibit number; fails on duplicates.
- `dotnet run tools/check-links.cs` - cross-references are links, links resolve.

## Memory

Committed project memory lives in `.claude/memory/`, indexed by `MEMORY.md`,
which is imported below so its index loads every session. Read `rejected.md`
before proposing; the topic files are read on demand. Memory updates are
committed **separately** from exhibit commits.

@.claude/memory/MEMORY.md
