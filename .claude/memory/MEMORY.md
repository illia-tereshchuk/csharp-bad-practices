# Memory index

Committed project memory for **C# Mistakes Explained**. This index is
auto-loaded every session (imported from `CLAUDE.md`); the topic files are read
on demand.

- `halls.md` - canonical hall registry (the front-page generator reads it).
- `state.md` - current exhibit count, the exhibit table, next id.
- `backlog/` - candidate exhibits to pick from, **one file per hall**
  (`backlog/<slug>.md`); format and rules in `backlog/README.md`.
- `rejected.md` - declined candidates + the curator's reasons. **Read before proposing.**
- `archetypes.md` - the 7 bug archetypes; the curation taxonomy.
- `todo.md` - remaining framework/infra work.

After an exhibit lands: update `state.md` and delete the candidate's block from
its `backlog/<slug>.md`. Memory is committed **separately** from exhibit
commits; the per-hall split lets each hall's additions be their own commit.
