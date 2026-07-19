# Backlog

Candidate exhibits the curator picks from. `A` = archetype (see
`archetypes.md`). `propose-exhibits` turns this into a hall-grouped menu; the
curator selects. Ids are assigned at build time. No difficulty levels - every
mistake is equally able to take down prod.

Everything here has already passed the filters: not primer-level, not a vacuum
scenario, reproducible deterministically in a single console file, not proven by
timing, not dependent on an unpinned machine environment.

## Opened halls

### collections
| slug | A | the twist |
|------|:--:|---|
| sort-is-unstable | 4 | List.Sort silently reorders equal elements, so a "sort by date, then by name" two-pass gives an order neither pass asked for; OrderBy is stable, Sort is not. |
| array-covariance-betrayal | 4 | `object[] a = new string[3]; a[0] = 42;` compiles and throws at runtime - a type-safety hole left open on purpose. |
| dictionary-order-illusion | 6 | Enumeration order looks like insertion order until one Remove; the next Add reuses the freed slot and the sequence rearranges. |

### numbers
| slug | A | the twist |
|------|:--:|---|
| math-round-banker | 4 | `Math.Round(2.5)` is 2, not 3 - banker's rounding by default, and the invoice total drifts from the accountant's paper math. *Curator questioned it as primer-level; discussion pending.* |
| nan-poisons-comparison | 4 | One NaN and Max, sorting and `>=` filters all disagree, because NaN is not equal to itself. |

### async
| slug | A | the twist |
|------|:--:|---|
| the-collected-timer | 6 | A Timer with no stored reference gets collected mid-run; the "every minute" job stops with no error. |
| lock-on-a-string | 2 | Two unrelated classes locking on "cache" share one lock through string interning; prove it with ReferenceEquals. |
| semaphore-never-released | 5 | An exception between Wait and Release leaks a permit forever; the next caller waits on a semaphore nobody will free. |

### events
| slug | A | the twist |
|------|:--:|---|
| one-handler-kills-the-rest | 5 | A subscriber that throws stops the invocation list; everyone registered after it never runs, and the publisher never knows. |
| invoke-race-on-null-check | 1 | `if (E != null) E(...)`: the last subscriber unsubscribes between the check and the call - NRE from an event that "cannot be null". |

### value-types
| slug | A | the twist |
|------|:--:|---|
| the-vanishing-mutation | 3 | Mutating a struct taken from a List edits a copy; the same code on an array works, so nobody suspects the collection. |

### exceptions
| slug | A | the twist |
|------|:--:|---|
| activator-hides-the-real-error | 5 | Reflection wraps the constructor's exception in TargetInvocationException, so the catch for the real type never fires. |

### orm
| slug | A | the twist |
|------|:--:|---|
| stale-tracked-entity | 5 | The change tracker returns the entity it cached earlier, so a fresh query hands back data that is already out of date. |
| untranslatable-where | 4 | A helper method inside Where cannot become SQL; EF stops the query dead rather than quietly pulling the whole table. |

### serialization
| slug | A | the twist |
|------|:--:|---|
| datetime-kind-round-trip | 4 | A Local timestamp goes into JSON and comes back as something else: the value survives, the meaning does not. |

### di-lifetimes
| slug | A | the twist |
|------|:--:|---|
| the-silent-override | 4 | Two registrations of one interface are legal; the last wins, and the handler you debugged for an hour never resolved. |

### datetime
| slug | A | the twist |
|------|:--:|---|
| kind-blind-equality | 4 | 14:00 UTC equals 14:00 Local, because `==` compares ticks and ignores Kind. |
| the-25-hour-day | 6 | AddHours(24) is not "tomorrow, same time" across DST; the daily job fires an hour off, twice a year. |

## Planned halls (a candidate here opens the hall)

| hall | slug | A | the twist |
|------|------|:--:|---|
| nullability | null-forgiving-lies | 5 | `!` silences the compiler and changes nothing at runtime: a promise you made, not a check you performed. |
| generics | static-field-per-closed-type | 6 | A static field in `Cache<T>` is separate per T, so the "global" cache silently splits into many. |
| enums | enum-accepts-undefined | 5 | A cast or Enum.Parse produces a value not in the enum, and every switch on it falls to default. |
| inheritance | virtual-call-in-constructor | 1 | The base constructor calls an override that runs before the derived fields exist; the object sees its own state as null. |
| pattern-matching | switch-expression-not-exhaustive | 5 | One added enum member turns a compile-time warning into a runtime exception in a switch that "covered everything". |
| records | with-copies-the-reference | 3 | `with` returns a new record sharing the same List; editing the "copy" changes the original. |
| equality | null-comparisons-are-always-false | 4 | For nullable numbers both `x > 5` and `x <= 5` are false when x is null; two opposite filters together drop rows. |
| disposal | dispose-what-you-dont-own | 5 | Wrapping an injected service in `using` breaks it for everyone else: you freed what wasn't yours. |
| boxing | mutating-a-boxed-struct | 3 | Calling a mutating method through an interface changes the box, not your variable. |
| memory | the-closure-that-held-everything | 6 | A lambda that captured one small variable keeps the whole captured state alive, big array included. |
| http | baseaddress-eats-your-path | 4 | A BaseAddress without a trailing slash silently drops its last segment: every `/v1/users` call goes to `/users`. Pure Uri math, no server. |
| configuration | binding-fails-silently | 5 | One typo'd key and the setting is the default; nothing throws, nothing logs, the feature is "off in prod only". |
| logging | interpolated-log-loses-everything | 4 | `$"..."` formats before the logger sees it, so the structured fields you search by never exist. |
| regex | missing-anchors-pass-anything | 5 | A "digits only" check without anchors accepts abc123def, because IsMatch looks for a match anywhere. |
| testing | async-void-test-always-passes | 5 | A failed assertion in an async void test is never observed: the suite stays green while the code is broken. |
| io | readalltext-guesses-encoding | 5 | Reading a file written in another encoding mangles the text instead of failing, and the corruption ships downstream. |
| strings | length-lies-about-emoji | 4 | `"👍".Length` is 2, so a 50-character truncate cuts an emoji in half and sends a replacement char to production. |
| strings | mojibake-factory | 4 | Text encoded twice comes back as "ÐŸÑ€Ð¸Ð²Ñ–Ñ‚" - readable proof of what a double round-trip does to bytes. |
| security | interpolated-injection | 4 | The same `$"...{name}..."` is safe in FromSqlInterpolated and an injection in FromSqlRaw: identical syntax, opposite fate. |
| security | guessable-random | 6 | `Random` for password-reset tokens: seed it the same way and you reproduce someone else's token. |

## Seeds (brainstorm before proposing)

- **exceptions:** exception filters (`when`) side effects · rethrow across await boundaries.
- **linq / collections:** GroupBy on reference-equality keys · OrderBy with a non-deterministic key.
- **reflection, equality, memory:** halls still thin - restock when their first exhibit lands.
