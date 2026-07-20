---
name: contribute
description: Guide an outside contributor through adding one exhibit end to end - check their setup, pick a mistake from a menu, draft it, prove it runs, open a pull request. Use when someone says they want to add an exhibit, contribute, or asks how to help.
---

# Guide a contributor

Someone cloned the repo and wants to add an exhibit. Make one evening's work end
in a pull request, and make it feel good on the way. They may never have opened
a PR before - assume nothing, explain the reasoning as you go, and do the
tedious parts for them.

## 0. Resume before you start

A fresh session has no memory of yesterday's chat, but the disk remembers. Run
`git status` first: if there is a half-built exhibit folder under `src/`, say so
and continue that one instead of starting over.

## 1. Preflight (do this before anything else)

Three failures cost a beginner twenty minutes each if they surface late. Check
all of them now, quietly, and fix what you can:

```bash
dotnet --list-sdks                 # need .NET 10
gh auth status                     # need a logged-in GitHub CLI
git remote -v && git branch --show-current
```

- **No .NET 10** - point them at https://dotnet.microsoft.com/download and stop.
- **`gh` not authenticated** - have them run `gh auth login` now, not at the end.
- **On `main`** - create the working branch immediately:
  `git switch -c exhibit/<slug>`. Never build on `main`.
- **No push access** - they need to be a collaborator on the repo. If a push
  would fail, tell them to ask the maintainer for access before they invest an
  evening.

## 2. Offer the menu

Show candidates from `.claude/memory/backlog.md`, grouped by hall, one line each
with the hook. Mark which halls nobody has opened yet - being first in a hall is
the best prize on offer.

Tell them plainly: **pick the one you like most and feel most sure about.** They
will be explaining it to other developers, so genuine interest shows in the
writing. If they bring their own idea, take it - just confirm it can fail
deterministically in a single console file, which is the one hard requirement.

<!-- Internal: silently skip anything listed in .claude/memory/rejected.md when
building the menu. Do not present that file, explain it, or discuss the curation
history with contributors - it is the maintainer's editorial record. -->

## 3. Take the next number

Numbers are automatic - no waiting on anyone:

```bash
dotnet run tools/next-id.cs
```

Use what it returns. The maintainer double-checks the number at review; if two
contributions collide, renumbering is one `git mv` on his side. Never leave the
contributor blocked on a number.

## 4. Show them the shape, then draft it

Point at one finished exhibit first - `src/records/0028-with-copies-the-reference/`
is a good one: short, no packages, and the Bad/Good pair differs by a single
expression. One real example teaches the format better than any description.

Then **write a working first draft yourself** - `Bad.cs`, `Good.cs`, `README.md`
- and hand it to them to read, question and change. A blank page stalls a
newcomer; a draft they can argue with does not. Make clear it is theirs to
redirect: the domain, the story, the wording are all open, and their judgement
decides what ships.

Explain the *why* as you go - the reasoning is half of what they came for.
The detailed file conventions load on their own as you edit each file.

## 5. Prove it

From the exhibit folder, run both and show them the real output:

```bash
dotnet run Bad.cs    # must fail, exactly as the README promises
dotnet run Good.cs   # must pass
```

Then from the repo root:

```bash
dotnet run tools/check-links.cs
dotnet run tools/gen-frontpage.cs
```

Put their GitHub username in the README front-matter so the generator credits
them:

```yaml
author: their-github-username
```

Show them the new line on the front page with their name on it. That is the
moment the work becomes theirs.

## 6. Ship it

Commit (subject `Add exhibit #NNNN: <slug>`), push the branch, open the PR:

```bash
git push -u origin exhibit/<slug>
gh pr create --fill
```

Walk them through each command if it is their first time. Then set expectations:
the maintainer reviews every exhibit personally and may ask for changes or
decline it - that is normal for a curated collection and says nothing about
their code.
