# TODO

Numbered so we can refer to items fast ("do T3", "skip T7"). Two tracks:
exhibits (content) and infra (the roadmap). Content first, per curator's
call - a stocked museum sells better than empty CI.

## Track A - exhibit batch (next)

Pull from `backlog.md` recommended batch. Assign ids at commit time.

- **A1.** the-captive-scoped -> opens **di-lifetimes** hall, 🔴 flagship, `#:package` #2.
- **A2.** the-zero-priced-order -> opens **serialization** hall, 🟡.
- **A3.** whenall-hides-exceptions -> async, 🔴.
- **A4.** the-finally-that-lied -> exceptions, 🔴 (sibling of 0005).
- **A5.** path-combine-betrayal -> opens **security** hall, 🔴.
- **A6.** length-lies-about-emoji -> opens **strings-memory** hall, 🟡.

After A1-A6: 15 exhibits, 10 halls, first 🔴s in. Ready for a stronger LinkedIn launch.

## Track B - infra roadmap (steps 8-10, not started)

From the original 10-step plan; steps 1-7 done (env -> first exhibits -> playbook).

- **B8. TOC generator.** Script reads every exhibit's front-matter (`id/title/category/level/summary`) and regenerates the front-page tables + stats line. Kills manual sync. Likely `tools/gen-toc.cs`. Also emit a tags/archetype cross-index once tags are populated. Run via CI on push.
- **B9. CI (GitHub Actions).** On push/PR: `dotnet build` (or run) every exhibit so bad examples don't rot on SDK bumps; run `tools/next-id.cs` to fail on dup numbers; optionally run B8 and fail if README is stale. Note: package exhibits (EF) need restore; watch CI time.
- **B10. Launch polish.** Badges (exhibit count, last added), issue templates OFF (solo repo), final proofread, LinkedIn poll copy (<=30 chars, 4 options - see chat history for the ironic set).

## Track C - open decisions / parking lot

- **C1.** ~~Commit / gitignore / push `claude-calibration/`?~~ RESOLVED 2026-07-17: committed public. Curator's call - "so obscure nobody looks anyway," and it fits the open "AI executes" brand. Backlog spoiler risk accepted.
- **C2.** New halls to formalize when their first exhibit lands: value-types, events, serialization. Add to `docs/playbook.md` halls list at that point.
- **C3.** LinkedIn poll extra ironic options - brainstormed set is in chat; not yet finalized.
- **C4.** When tags matter (dozens of exhibits), backfill consistent `tags` across old exhibits so B8 cross-index is clean.

## Done (for orientation)

- Steps 1-7 of roadmap. Exhibits 0001-0009. next-id tool. playbook. Front-page redesign. Dash cleanup. This calibration folder.
