---
name: propose-exhibits
description: Generate a menu of candidate exhibits for the curator or a contributor to choose from, grouped by hall with its emoji badge, each with a one-line hook. Use when the curator asks "what next", wants options to pick from, wants to restock the backlog, or wants to see coverage across halls.
---

# Propose exhibits

Produce a menu someone picks from. This skill only *proposes* - the curator
names the specific cases to build, then the `add-exhibit` skill builds each
one. Outside developers see the same menu through the `contribute` skill.

## Sources and filters

- Pull queued ideas from `.claude/memory/backlog/` - one file per hall
  (`backlog/<slug>.md`); `backlog/README.md` defines the entry format.
- **Exclude everything in `.claude/memory/rejected.md`** - never re-propose a
  declined idea, and pre-filter anything that trips its reason categories
  (predictable finale, can't reproduce, timing-only, CI-would-lie,
  doesn't-happen-in-real-code, premise-doesn't-hold, no-real-damage).
  If a candidate has since landed in `rejected.md`, delete its block from the
  hall file - the two must never disagree.
- Apply the curation bar from `CLAUDE.md`: one broken mental model, reject
  predictable finales, prefer accessible + axiomatic, prefer silent-wrongness /
  money stakes.
- Balance against `.claude/memory/archetypes.md` - spread across the 7
  archetypes, don't stack one.
- **When restocking:** follow the entry format in `backlog/README.md`, add the
  candidate to its hall file (`backlog/<slug>.md`, creating the file if the
  hall had none), and run every new candidate's core premise as real code
  before adding it (record the result in the entry's **Verified** field).
  Two past rejections were premise errors that a 20-line scratchpad run
  would have caught - never trust a mechanic from memory.

## Format (hall-grouped, badged)

The menu has two audiences: the curator restocking, and a first-time
contributor picking their exhibit. Both must be able to scan it and land on
"their" mistake without knowing the museum's internals.

- Group candidates under their hall - one hall = one `backlog/<slug>.md` file.
  Every hall header carries the hall's emoji badge and display name **exactly
  as registered in `.claude/memory/halls.md`** (each hall file repeats its
  emoji in the title line, but halls.md is the source of truth) - the badges
  are the navigation, never invent or swap them.
- Two tiers, in this order:
  1. **Opened halls** - candidates joining an existing hall.
  2. **New halls 🚪** - planned halls; picking a candidate there opens the
     hall. Say so plainly: being first in a hall is the best prize on offer.
- One line per candidate:

  ```
  **slug** - the twist (the mechanic, not a restated title)
  ```

  Use the candidate's **Twist** field from the hall file verbatim - it is
  written to be the menu line. The rest of the entry (Mechanic, Who hits it,
  Repro, Damage, Verified) is builder context: keep it out of the menu, but
  read it before answering questions about a candidate.
- Skip halls with no candidates - the menu shows choices, not gaps. Mention
  empty halls only when the curator asks about coverage.
- Archetype numbers stay internal: use them to balance the batch, never
  print them in the menu.
- Keep each tier scannable in about one screenful.

## After proposing

Stop and let the picker select. Do not start building. When cases are named,
switch to `add-exhibit` (or continue `contribute` for an outside developer).
Delete chosen ideas from their hall file as they ship; add declined ones to
`rejected.md` with a reason (and remove their block in the same edit).
