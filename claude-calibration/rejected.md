# Rejected

What the curator declined, and WHY - so I learn his taste and stop
re-proposing. Add a row whenever he turns something down. Watch for patterns
in the "reason" column; they encode the curation bar.

| candidate | reason category | detail |
|-----------|-----------------|--------|
| turkish-i-login | too banal / predictable finale | classic listicle item; no mechanic twist, reader guesses the end. |
| int-overflow-in-cart | too banal / predictable finale | same - "int overflows" surprises no one. |
| .Result deadlock | cannot reproduce honestly | needs a SynchronizationContext (UI/legacy ASP.NET); a console app can't show it. Rule: no exhibit that doesn't reproduce. |
| StringBuilder-in-a-loop | proven only by timing | "trust me it's slow" is banned; timings flicker across machines. |
| quadratic ElementAt | proven only by timing | same. |
| culture/timezone bug w/o pinning | CI would lie | if the code doesn't pin culture/zone, the demo's outcome depends on the runner. Only ship if the code fixes the environment explicitly. |

## Reason categories (the bar, distilled)

1. **Predictable finale** - reader guesses the outcome from the title. Needs a "wait, WHAT?" twist in the mechanic.
2. **Cannot reproduce honestly** - won't fail deterministically in a single console file.
3. **Proven only by timing** - performance claim with no hard assertion; banned.
4. **CI would lie** - outcome depends on machine culture/zone/GC without the code pinning it.

If a new idea trips any of these, pre-filter it before proposing.
