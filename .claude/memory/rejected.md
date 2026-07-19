# Rejected

What the curator declined, and WHY - so I learn his taste and stop
re-proposing. Add a row whenever he turns something down. Watch for patterns
in the "reason" column; they encode the curation bar.

| candidate | reason category | detail |
|-----------|-----------------|--------|
| turkish-i-login | too banal / primer-level | everyone typed a password in the wrong keyboard layout as a kid - common knowledge from the moment you touch a computer, below the museum's floor. |
| int-overflow-in-cart | too banal / primer-level | every junior has already hit integer overflow; it's a primer, not an exhibit - people know it before they're paid to code. |
| path-combine-betrayal | vacuum example / no who-where-how | you can't say who attacks, from where, or how - a vacuum scenario with no reproducible real-world context, so not interesting. Built and reverted at #0021 before commit. |
| .Result deadlock | cannot reproduce honestly | needs a SynchronizationContext (UI/legacy ASP.NET); a console app can't show it. Rule: no exhibit that doesn't reproduce. |
| StringBuilder-in-a-loop | proven only by timing | "trust me it's slow" is banned; timings flicker across machines. |
| quadratic ElementAt | proven only by timing | same. |
| culture/timezone bug w/o pinning | CI would lie | if the code doesn't pin culture/zone, the demo's outcome depends on the runner. Only ship if the code fixes the environment explicitly. |
| firstordefault-on-structs | premise doesn't hold | "nobody null-checks a struct - nonsense." He is right, and stronger than that: `found == null` does not even compile for a non-nullable struct, so the bug as I framed it cannot happen. My framing dressed a real nuance (default(T) is zeros, not null) in an impossible scenario. |

## Reason categories (the bar, distilled)

1. **Predictable finale / primer-level** - either the reader guesses the outcome from the title, OR the bug is so universally experienced (integer overflow, wrong keyboard layout) that it's common knowledge from day one - a primer, below the museum's floor. An exhibit needs a mechanic twist AND a topic that isn't already in everyone's bones. When proposing, pre-filter anything a person learns "the moment they touch a computer."
2. **Cannot reproduce honestly** - won't fail deterministically in a single console file.
3. **Proven only by timing** - performance claim with no hard assertion; banned.
4. **CI would lie** - outcome depends on machine culture/zone/GC without the code pinning it.
5. **Doesn't happen in real code** - technically real, but the author judges it too contrived to occur in actual projects. His lived-experience filter overrides textbook correctness; when unsure whether a bug is "real enough", prefer everyday-contract scenarios over exotic API footguns.
6. **Premise doesn't hold** - my own error: the proposed scenario cannot occur as described (the compiler forbids it, the API prevents it). Before proposing, mentally compile the bad code - if it wouldn't build, the exhibit doesn't exist.

If a new idea trips any of these, pre-filter it before proposing.
