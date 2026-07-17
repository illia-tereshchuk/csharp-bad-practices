# claude-calibration

Persistent working context for Claude on the **C# Bad Practices** museum.
Read this folder first at the start of any session; it restores context
without re-reading every exhibit.

## Read order

1. `state.md` - where the project is right now (counts, halls, next id).
2. `exhibit-recipe.md` - how to generate an exhibit that fits. THE generation template.
3. `conventions.md` - hard style rules the curator enforces. Never violate silently.
4. `archetypes.md` - the 7 bug archetypes; the taxonomy behind curation.
5. `backlog.md` - candidate exhibits, dense table, prioritized.
6. `todo.md` - numbered next actions (exhibit batch + infra roadmap).
7. `rejected.md` - what the curator turned down and why. Check before proposing.

## Who / how

- Curator: Illia (junior .NET dev, Ukraine). Chats in Ukrainian, wants English deliverables.
- Workflow: **I generate full exhibits (Bad + Good + README + front-page row), verify by running, leave uncommitted. He reads/tests, then commits (or asks me to).**
- He drives the idea, I execute; on-brand and openly co-authored (`Co-Authored-By: Claude`).
- Teaching mode: he wants to understand each step, not just receive code. Explain the "why."

## Maintenance rules for this folder

- Update `state.md` after each exhibit lands (or just re-run `dotnet run tools/next-id.cs`; it is the source of truth for numbering).
- Move a candidate from `backlog.md` to done when committed; move to `rejected.md` (with reason) when he declines.
- Keep everything hyphens-only, English, table-dense. These files are for fast reload, not prose.
