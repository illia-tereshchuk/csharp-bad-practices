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
- **Solo project.** No external-contributor scaffolding (no CONTRIBUTING.md,
  issue templates, "PRs welcome"). Curated by one person.
- **Commits:** imperative subject <=50 chars, no trailing period; body explains
  *why*; one logical change per commit; trailer
  `Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>`. Exhibit commits are
  `Add exhibit #NNNN: <slug>`.

## Curation bar (when proposing exhibits)

- **Reject predictable finales.** If the reader guesses the outcome from the
  title, it's a listicle item, not an exhibit. An exhibit needs a mechanic
  twist - a "wait, WHAT?" even for someone who knows the bug.
- **Prefer accessible + axiomatic:** everyday contracts everyone touches, with a
  rule that reads like an axiom (`never write async void outside event
  handlers`). Favor these over exotic API footguns.
- **Prefer silent-wrongness and money/audit stakes** - they screenshot well and
  land emotionally.

## Tools (run from repo root)

- `dotnet run tools/next-id.cs` - next free exhibit number; fails on duplicates.
- `dotnet run tools/check-links.cs` - cross-references are links, links resolve.

## Memory (read before proposing or generating)

Project memory lives in `claude-calibration/` - plain data I maintain by hand:

- `state.md` - current counts, halls, the exhibit table, next id.
- `backlog.md` - candidate exhibits, prioritized.
- `rejected.md` - what the curator declined and why. **Check before proposing.**
- `archetypes.md` - the 7 bug archetypes; the curation taxonomy.
- `todo.md` - numbered roadmap (exhibits, infra, open decisions).

After an exhibit lands: update `state.md`, and move the candidate from
`backlog.md` to done (or to `rejected.md` with a reason if declined).
