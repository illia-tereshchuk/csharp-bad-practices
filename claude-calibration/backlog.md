# Backlog

Candidate exhibits. `A` = archetype (see `archetypes.md`). Status: `next` =
in the recommended batch, `idea` = pool, else moves to done or `rejected.md`.
Ids are assigned at commit time, not here.

## Recommended next batch (0010-0015)

Order chosen to: open 4 new halls, spread archetypes, introduce 🔴.

1. **the-captive-scoped** (di-lifetimes, 🔴) - flagship, opens di. `#:package` #2.
2. ~~the-zero-priced-order~~ DONE as #0012.
3. **whenall-hides-exceptions** (async, 🔴)
4. **the-finally-that-lied** (exceptions, 🔴)
5. **path-combine-betrayal** (security, 🔴) - opens security.
6. **length-lies-about-emoji** (strings-memory, 🟡) - opens strings-memory.

## Full candidate pool

### async / threading
| slug | lvl | A | the twist (mechanic, not just name) |
|------|:--:|:--:|-------------------------------------|
| whenall-hides-exceptions | 🔴 | 5 | WhenAll surfaces ONE of N faults; the rest hide in `.Exception`. |
| token-tourism | 🟡 | 5 | token passed through 5 sigs, never *checked*; Cancel does nothing. |
| the-collected-timer | 🔴 | 6 | Timer with no stored ref gets GC'd; force GC.Collect, ticks stop. |
| the-forgotten-task | 🟡 | 1,5 | unstored un-awaited Task; exception buried, "save" never happened. |
| lock-on-a-string | 🔴 | 2 | `lock("cache")` in 2 classes = same interned object. Prove via ReferenceEquals. |
| tasks-are-not-results | 🟡 | 1 | `Select(async ...)` yields Task<T>[], code treats as results. |

### datetime (hall closed - open with a strong one)
| slug | lvl | A | twist |
|------|:--:|:--:|-------|
| kind-blind-equality | 🔴 | 4 | 14:00 UTC == 14:00 Local; `==` compares ticks, ignores Kind. Best opener. |
| shrinking-billing-day | 🟡 | 3 | Jan31 +1mo = Feb28, billing day slides to 28 forever; -1mo != back. |
| the-25-hour-day | 🔴 | 6 | AddHours(24) != "tomorrow same time" across DST. Pin TimeZoneInfo or CI lies. |

### strings-memory (closed)
| slug | lvl | A | twist |
|------|:--:|:--:|-------|
| length-lies-about-emoji | 🟡 | 4 | "👍".Length==2; 50-char truncate splits a grapheme, � in the push. StringInfo. |
| mojibake-factory | 🟢 | 4 | double-encode: "Привіт" -> "ÐŸÑ€Ð¸Ð²Ñ–Ñ‚". simple but byte-level finale. |

### exceptions
| slug | lvl | A | twist |
|------|:--:|:--:|-------|
| the-finally-that-lied | 🔴 | 5,7 | throw in finally REPLACES the real cause; sibling of 0005. |
| cancellation-eaten-by-catch | 🟡 | 5 | `catch(Exception)` swallows OperationCanceledException; retries loop forever. |

### di-lifetimes (closed - flagship hall)
| slug | lvl | A | twist |
|------|:--:|:--:|-------|
| the-captive-scoped | 🔴 | 6 | singleton captures scoped -> "current user" frozen; B sees A's cart. |
| the-silent-override | 🟡 | 4 | two registrations of one iface; last wins silently. |
| the-container-hoarder | 🔴 | 5 | transient IDisposable from root lives till process death; WeakReference+GC proof. |

### security (closed)
| slug | lvl | A | twist |
|------|:--:|:--:|-------|
| interpolated-injection | 🟡 | 4 | same `$"...{x}"` safe in FromSqlInterpolated, injection in FromSqlRaw. |
| path-combine-betrayal | 🔴 | 5 | `Path.Combine(root, input)` silently DROPS root if input is absolute. No "..". |
| guessable-random | 🟡 | 6 | `Random` for reset tokens; seed predictable, token reproducible. |

### value-types (hall opened by #0011)
| slug | lvl | A | twist |
|------|:--:|:--:|-------|
| the-vanishing-mutation | 🔴 | 3 | mutate struct from a collection -> mutates a copy; array works, List doesn't. Sibling of #0011. |

### events (hall opened by #0010)
| slug | lvl | A | twist |
|------|:--:|:--:|-------|
| the-unremovable-lambda | 🟡 | 1 | `Click -= (s,e)=>...` never unsubscribes; different delegate instance. Teased in #0010's 😈 section. |

### serialization (hall opened by #0012; pool needs restocking)
| slug | lvl | A | twist |
|------|:--:|:--:|-------|
| _seeds:_ DateTime Kind lost in JSON roundtrip · polymorphic `$type` handling · reference loops | | | brainstorm before proposing |

### collections / linq (misc pool)
_Empty - distinct-that-didnt shipped as #0013. Seeds for restocking:
GroupBy on reference-equality keys · Contains vs Any confusion ·
OrderBy with a non-deterministic key._
