---
name: reject-exhibit
description: Record a candidate the curator declined, in their own words, into rejected.md so future proposals learn their taste. Use when the curator turns a candidate down, says "skip this", "not interested", or explains why an idea doesn't fit the museum.
---

# Reject an exhibit

Capture a declined candidate and **why**, in the curator's own words. This is the
primary channel for learning his subjective judgment - his taste lives in
individual choices, not in categories, so every rejection is signal, not noise.

## Steps

1. **Get the reason in his words.** If it's one word, ask a brief follow-up - the
   "why" is the entire point of this skill. Don't paraphrase away his framing.
2. **Append a row** to `.claude/memory/rejected.md`:
   `| <slug> | <reason category> | <his detailed reason, close to verbatim> |`
3. **New category if needed.** If the reason doesn't fit an existing category
   (predictable finale, can't reproduce, timing-only, CI-would-lie,
   doesn't-happen-in-real-code), add a numbered one with a one-line definition.
4. **Remove its block from `.claude/memory/backlog/<hall>.md`** if it was
   queued there - the backlog and `rejected.md` must never disagree.
5. **Confirm** the recorded reason back to him in one line, so he can correct the
   framing before it hardens into my model of his taste.

## Why this matters

The reason column is a growing model of the curator's mind. Over time, patterns
in it should let `propose-exhibits` pre-filter candidates he'd reject - fewer
misses, better menus. Treat accuracy here as more important than speed: a
mis-recorded reason teaches me the wrong lesson.
