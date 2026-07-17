# Curator's Playbook

This museum is a personal collection — one curator, one voice. This file is
the internal checklist for adding an exhibit, so future-me doesn't have to
reverse-engineer the process from old commits.

## Add an Exhibit in 10 Minutes

1. **Get a number.** Numbers are global, four digits, never reused:

   ```bash
   dotnet run tools/next-id.cs
   ```

2. **Copy the template** into the right hall (category). The slug is
   kebab-case and names the crime — `0002-doubles-for-money`, not
   `0002-my-bug`:

   ```bash
   cp -r docs/template src/<category>/<NNNN>-<slug>
   ```

3. **Write `Bad.cs`.** `dotnet run Bad.cs` must end in **visible failure**:
   an exception, an obviously wrong number, a demonstrable hang.
   "Trust me, it's slow" is not an exhibit.

4. **Write `Good.cs`** as a mirror: same domain, same data, same scenario.
   The only difference the reader should see is the approach.

5. **Fill in `README.md`**, front-matter first — the front page is built
   from it. Keep the template's section order; the 😈 and 🎓 sections are
   optional.

6. **Verify everything runs** (from the exhibit folder):

   ```bash
   dotnet run Bad.cs    # fails exactly as the README promises
   dotnet run Good.cs   # works
   ```

7. **Update the front page**: the exhibit row in the right hall's table and
   the stats line *(temporary — a generator will take this over)*.

8. **Commit** as `Add exhibit #NNNN: <slug>`.

## Museum Rules

- **One exhibit = one broken mental model.** Not a typo, not a whole
  architecture review. If explaining it needs two different "why"s,
  it's two exhibits.
- **30–100 lines per file.** Big enough to feel real, small enough to
  read over coffee.
- **Single file, no project.** Exhibits are file-based apps. Need a NuGet
  package? Use a directive at the top of the file: `#:package Dapper@2.*`.
  A folder with a full `.csproj` needs a good reason — note it in the
  commit message.
- **Believable domain.** An `OrderService` with subscribers beats `Foo`
  with `Bar`. Readers should recognize their own codebase.
- **English only** — code, comments, docs.
- **No shaming.** We all wrote this code at some point. Exhibits mock the
  code, never the author.
- **Jokes live in the README.** Code comments stay plain and boring.

## Halls (Categories)

`numbers` · `collections` · `linq` · `async` · `orm` · `datetime` ·
`strings-memory` · `exceptions` · `di-lifetimes` · `security`

Opening a new hall: create the folder, add it to this list.
