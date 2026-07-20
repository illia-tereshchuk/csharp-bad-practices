# TODO

Remaining work, current as of the framework migration (2026-07-19).

## Next stage: open the museum to contributors

Curator's new direction (2026-07-19): invite other developers to add exhibits.
A contributor clones the repo, runs Claude, gets offered a menu of candidate
mistakes, is guided through the same build workflow, and ends at a pull request.
Their GitHub username appears on the front page next to the rule they added. It
should feel like a game.

The framework already carries most of this: `.claude/` is committed, so a
cloner inherits the rules, the skills, the tools and - most valuable -
`rejected.md`, which pre-filters the ideas the curator would decline.

**Open decisions (ask before building):**

1. **Who is the gate?** Recommended: the curator stays the curator and the PR is
   the gate. Contributors are guided all the way to a PR; he accepts or rejects;
   rejections keep feeding `rejected.md`. Self-service would erode the bar,
   because the hard part is his lived-experience judgment, not the mechanics.
2. **Number collisions.** Two contributors both run `next-id` and both get the
   same id. Options: assign the number at merge time (folder uses the slug until
   then), curator assigns on claim, or first merged PR wins and the second
   renumbers.
3. **Claiming.** The backlog needs to show what is taken and by whom, so two
   people don't build the same exhibit.

**Work items once decided:**

- Reverse the solo-project stance: `CLAUDE.md` currently says "no
  external-contributor scaffolding", and `CONTRIBUTING.md` was deliberately
  deleted. A new thin CONTRIBUTING (the skills do the teaching) plus a
  contributor-facing entry point.
- A contributor skill (or a contributor mode on the existing ones) that
  onboards, shows the menu, builds, and walks to the PR.
- `author` field in exhibit front-matter; `tools/gen-frontpage.cs` appends
  `(@username)` after the rule on the front page; update
  `.claude/rules/exhibit-readme.md` and the template.
- Game layer: opening one of the remaining planned halls is the rare
  achievement; the commandment list doubles as a scoreboard.

## Open

- **CI (GitHub Actions).** *Deferred by the curator (2026-07-19) - he will raise
  it again; do not start unprompted.* On push/PR: build/run every exhibit so they
  don't rot on SDK bumps; run `next-id.cs`, `check-links.cs`, and
  `gen-frontpage.cs` (fail if the front page is stale) - all three exit 1 on
  failure. Package exhibits (EF, DI, STJ) need restore; watch CI time.
- **Launch polish.** Badges (exhibit count), final proofread, LinkedIn poll copy
  (<=30 chars, 4 options).
- **Tags cross-index.** Once tags are consistent across exhibits, generate a
  tag/archetype index alongside the front page.

## Done

- Full framework migration to native Claude Code mechanisms: root `CLAUDE.md`,
  path-scoped `.claude/rules/`, the `add-exhibit` / `propose-exhibits` /
  `reject-exhibit` skills. Retired the homemade `conventions.md`,
  `exhibit-recipe.md`, `playbook.md`.
- Tools: `next-id.cs`, `check-links.cs`, `gen-frontpage.cs` (front page is now
  generated, list-style, no difficulty levels).
- Hall taxonomy expanded to ~30 in `halls.md`.
- Memory relocated from the freestyled `claude-calibration/` into `.claude/memory/`,
  indexed by `MEMORY.md` and auto-loaded via a `CLAUDE.md` import.
- Exhibits 0001-0023.
