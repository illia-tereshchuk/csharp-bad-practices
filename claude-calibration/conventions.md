# Conventions (curator-enforced)

Hard rules Illia established by correcting my output. Violating these silently
wastes his time. Mirror of the memory file `museum-style-conventions`, kept
in-repo for fast reload.

## Typography / language

- **Hyphens only.** No em/en dashes anywhere - prose, comments, front-matter. Enforced repo-wide 2026-07-17.
- **English** for everything committed (code, comments, README, commit messages). Ukrainian only in chat and his LinkedIn posts.
- **Difficulty = bare emoji** (🟢🟡🔴). No "junior trap" / "mid-level" wording, no legend table. Labels risk offending readers.

## README / front page

- **No "Reproduce: dotnet run Bad.cs" blocks.** The run command is obvious; the block was noise.
- **Front page is deliberately minimal:** one-line manifesto, stats line, hall tables, big `# To Be Continued` H1 (intentional second H1). No "How to Run", no "Contributing", no disclaimer.
- **Punchlines are saved for LinkedIn**, not the repo. Repo tone = "lab / museum," a touch more restrained than my instinct.
- **Front-matter summaries trend short** ("TikTok generation" - his words). He edits wording himself; don't over-write.
- **Front-page tables are headerless** (empty header row `| | | | |` + delimiter - GFM needs them to render) and the last column is a "Never ..." commandment, sourced from front-matter `rule:`. Established 2026-07-18.

## Code comments

- **Jokes live in the README. Code comments stay plain and boring.** He cut "don't saw off the branch you're walking on" from a header. The 💥 emoji marker on the offending line is fine; prose wit is not.

## Project shape

- **Solo project.** No external contributors. CONTRIBUTING.md was deleted; the checklist lives in `docs/playbook.md` as an internal "Curator's Playbook."
- **He commits himself** (or explicitly asks me to). Default: I leave work uncommitted and tell him the commit command / message.

## Curation bar (from the backlog discussion)

- **Reject predictable finales.** If the reader can guess the outcome from the title, it's a "top-10 mistakes" listicle item, not an exhibit. An exhibit needs a mechanic twist - a "wait, WHAT?" even for someone who knows the bug. (This killed turkish-i and int-overflow.)
- Prefer silent-wrongness and money/audit stakes; they screenshot well and land emotionally.

## Commit style (when I commit)

- Imperative subject <=50 chars, no trailing period. Blank line. Body explains WHY.
- `Add exhibit #NNNN: <slug>` for exhibits. Trailer: `Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>`.
- Atomic: one logical change per commit. Exhibit separate from policy/infra changes.
