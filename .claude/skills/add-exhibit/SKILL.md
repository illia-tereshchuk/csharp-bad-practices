---
name: add-exhibit
description: Create a new C# Mistakes Explained exhibit end to end - scaffold from the template, write Bad.cs/Good.cs/README.md, verify by running, then wire the front page and memory. Use when adding an exhibit or when the curator names a candidate to build.
---

# Add an exhibit

The end-to-end procedure for producing one exhibit. The *conventions* for each
file load automatically from `.claude/rules/` when you edit it - this skill is
the *workflow* that ties them together, so it stays procedural and does not
restate those rules.

## 1. Define and check the candidate

- **One exhibit = one broken mental model** (see the curation bar in
  `CLAUDE.md`).
- Read `.claude/memory/rejected.md` first - never re-propose a declined
  idea. Then `.claude/memory/backlog/<hall>.md` for the queued candidates and
  `archetypes.md` for balance.
- Fix the hall (category) and a kebab-case slug that names the crime:
  `0002-doubles-for-money`, not `0002-my-bug`.

## 2. Get a number

```bash
dotnet run tools/next-id.cs
```

Numbers are global, four digits, never reused.

## 3. Scaffold from the template

```bash
cp -r .claude/skills/add-exhibit/template src/<hall>/<NNNN>-<slug>
```

## 4. Write Bad.cs, then Good.cs

Editing `.cs` under `src/` auto-loads `exhibit-code.md` (mirror rule, self-audit
throw, determinism, escape hatches). Bad must end in a visible, deterministic
failure; Good is its mirror.

## 5. Write README.md

Editing the exhibit README auto-loads `exhibit-readme.md` (front-matter, fixed
section order, cross-links, tone).

## 6. Verify by running

```bash
dotnet run Bad.cs    # fails exactly as the README promises
dotnet run Good.cs   # works
```

Paste the real output - never claim it runs unseen. Then from the repo root:

```bash
dotnet run tools/check-links.cs
```

## 7. Wire the front page and memory

- If the hall is new, flip its row in `.claude/memory/halls.md` from planned
  to opened first.
- Regenerate the front page - never hand-edit it:

  ```bash
  dotnet run tools/gen-frontpage.cs
  ```

- Update `.claude/memory/state.md` (count, exhibit table, next id) and delete
  the candidate's block from `.claude/memory/backlog/<hall>.md` (gone because
  shipped, or moved to `rejected.md` with a reason if dropped mid-build).

## 8. Hand off

Leave the work uncommitted. Report the summary and the commit message
`Add exhibit #NNNN: <slug>`. Commit only when the curator says so.

## Halls

Valid categories, their emoji, and open/planned status live in the canonical
registry `.claude/memory/halls.md`. Pick the hall from there. Opening a
planned hall: build its first exhibit, then flip its row from planned to opened.
