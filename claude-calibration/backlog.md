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
| the-collected-timer | 🔴 | 6 | Timer with no stored ref gets GC'd; force GC.Collect, ticks stop. |
| lock-on-a-string | 🔴 | 2 | `lock("cache")` in 2 classes = same interned object. Prove via ReferenceEquals. |

### datetime (hall opened by #0020)
| slug | lvl | A | twist |
|------|:--:|:--:|-------|
| kind-blind-equality | 🔴 | 4 | 14:00 UTC == 14:00 Local; `==` compares ticks, ignores Kind. |
| the-25-hour-day | 🔴 | 6 | AddHours(24) != "tomorrow same time" across DST. Pin TimeZoneInfo or CI lies. |

### strings-memory (closed)
| slug | lvl | A | twist |
|------|:--:|:--:|-------|
| length-lies-about-emoji | 🟡 | 4 | "👍".Length==2; 50-char truncate splits a grapheme, � in the push. StringInfo. |
| mojibake-factory | 🟢 | 4 | double-encode: "Привіт" -> "ÐŸÑ€Ð¸Ð²Ñ–Ñ‚". simple but byte-level finale. |

### exceptions
_Empty - the-finally-that-lied shipped as #0017, cancellation-eaten-by-catch
as #0015. Seeds: exception filters (`when`) side effects · GC.KeepAlive
myths · rethrow across await boundaries._

### di-lifetimes (hall opened by #0014)
| slug | lvl | A | twist |
|------|:--:|:--:|-------|
| the-silent-override | 🟡 | 4 | two registrations of one iface; last wins silently. |

### security (closed)
| slug | lvl | A | twist |
|------|:--:|:--:|-------|
| interpolated-injection | 🟡 | 4 | same `$"...{x}"` safe in FromSqlInterpolated, injection in FromSqlRaw. |
| guessable-random | 🟡 | 6 | `Random` for reset tokens; seed predictable, token reproducible. |
| _(path-combine-betrayal rejected - see rejected.md: "doesn't happen in real code")_ | | | |

### value-types (hall opened by #0011)
| slug | lvl | A | twist |
|------|:--:|:--:|-------|
| the-vanishing-mutation | 🔴 | 3 | mutate struct from a collection -> mutates a copy; array works, List doesn't. Sibling of #0011. |

### events (hall opened by #0010)
_Empty - unremovable-lambda shipped as #0023. Seeds: event handler that
throws takes down the rest of the invocation list · `?.Invoke` race between
null-check and call._

### serialization (hall opened by #0012; pool needs restocking)
| slug | lvl | A | twist |
|------|:--:|:--:|-------|
| _seeds:_ DateTime Kind lost in JSON roundtrip · polymorphic `$type` handling · reference loops | | | brainstorm before proposing |

### collections / linq (misc pool)
_Empty - distinct-that-didnt shipped as #0013. Seeds for restocking:
GroupBy on reference-equality keys · Contains vs Any confusion ·
OrderBy with a non-deterministic key._
