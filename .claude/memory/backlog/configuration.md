# ⚙️ configuration

> Status: **planned**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### binding-fails-silently (A5)

- **Twist:** One typo'd key and the setting silently stays at its default:
  nothing throws, nothing logs, and the feature is "off in prod only".
- **Mechanic:** the configuration binder matches keys to properties by name;
  unmatched keys are ignored and unmatched properties keep their defaults -
  both directions are silent by design. The failure value is always the
  most plausible-looking one: false, 0, null.
- **Who hits it:** everyone with appsettings.json: `"MaxRetires": 5` binds
  nothing; MaxRetries is 0; retries are disabled; the review reads the JSON
  and sees the intent, not the typo.
- **Repro:** in-memory configuration dictionary with one typo'd key; Bind to
  an options class; the property holds its default. Packages:
  `Microsoft.Extensions.Configuration` + `.Binder` (+
  `#:property PublishAot=false`). Deterministic.
- **Damage:** feature flags off, limits at zero, endpoints null - each
  reading as a deliberate choice, none logged anywhere.
- **😈 seed:** `ErrorOnUnknownConfiguration` exists in BinderOptions and
  almost nobody turns it on - strictness is opt-in, silence is the default.
- **Verified:** documented binder behavior; verify at build.

### the-list-that-would-not-shrink (A5)

- **Twist:** appsettings.Production.json "replaces" the CORS allowlist with
  the one real origin - but arrays overlay index by index, so every dev
  origin past index 0 survives into production, invisible in any file.
- **Mechanic:** configuration has no arrays, only flat key-value pairs:
  `Origins:0`, `Origins:1`, `Origins:2`. A later provider overrides exactly
  the keys it defines and leaves the rest - so a shorter override rewrites
  the first N slots and the base's tail lives on. Layering cannot shrink a
  list: a JSON `null` element blanks its slot (the entry turns empty) but
  the array keeps its length. The only real replace is renaming the key or
  clearing every index explicitly.
- **Who hits it:** every layered-appsettings ASP.NET Core app that overrides
  a list per environment - CORS origins, allowed hosts, IP allowlists,
  redirect URI lists, logging filters. The base+override file pair is the
  platform's own recommended pattern.
- **Repro:** two `AddJsonStream` providers modeling appsettings.json +
  appsettings.Production.json; bind `string[]`; print the effective list -
  prod's element 0 followed by dev's tail. Packages:
  `Microsoft.Extensions.Configuration` + `.Json` + `.Binder`,
  `#:property PublishAot=false`. Deterministic.
- **Damage:** dev and staging origins silently allowed in production - a
  security allowlist *widened* by the very file that was written to narrow
  it, while the prod diff shows exactly the intended single entry.
- **😈 seed:** review is structurally blind here: the merge result exists in
  no file, so the only place the bug is visible is a running process -
  and the workaround people find (padding with nulls) leaves empty-string
  entries in the bound array instead.
- **Verified:** ran on .NET 10 (2026-07-22): `["https://app.example.com"]`
  over a 3-element base bound to `[app.example.com, staging.corp,
  localhost:3000]`; null-element override blanked slots but kept 3 entries.

### thirty-means-thirty-days (A4,5)

- **Twist:** `"Timeout": 30` bound to a TimeSpan is thirty *days* -
  TimeSpan's bare-number format is days, so every "seconds, obviously"
  value comes out ~86400x longer, and nothing throws or logs.
- **Mechanic:** the binder converts strings through the TimeSpan
  TypeConverter, i.e. `TimeSpan.Parse` (invariant), whose bare-number form
  means days: `"30"` -> `30.00:00:00`. The identical literal bound to an
  int property next to it is just 30. JSON number vs string changes
  nothing - providers stringify all values. The correct spellings are
  `"00:00:30"` - or an int property named `TimeoutSeconds`, which is why
  half the ecosystem uses ints.
- **Who hits it:** anyone with TimeSpan options - HttpClient timeout, cache
  TTL, retry delay, token lifetime - filling the JSON by analogy with every
  library that takes plain seconds.
- **Repro:** bind `{"Ttl": "30", "Limit": "30"}` to a class with
  `TimeSpan Ttl` and `int Limit`; print both plus `TotalHours` (720).
  Packages: `Microsoft.Extensions.Configuration` + `.Json` + `.Binder`,
  `#:property PublishAot=false`. Deterministic.
- **Damage:** a cache TTL of "5" is five days of stale prices; a timeout of
  "30" is a month - effectively no timeout, so the outage it was meant to
  contain cascades - while the config file reads exactly as intended.
- **😈 seed:** the log line that would expose it - `Timeout=30.00:00:00` -
  scans as normal at a glance, and integration tests never wait long
  enough to distinguish "30 seconds" from "30 days": the assertion that
  the value *bound* passes either way.
- **Verified:** ran on .NET 10 (2026-07-22): `"30"` -> 30.00:00:00 (720
  hours) for TimeSpan, 30 for int; JSON number 30 identical; `"00:00:30"`
  -> 30 seconds.

### the-guard-that-cannot-fire (A4,5)

- **Twist:** both obvious required-config checks lie, in opposite
  directions: `config["Smtp"]` is null while the section is fully present,
  and `GetSection("Nope")` is *never* null while nothing is there - so the
  startup guard either always throws or can never fire.
- **Mechanic:** a section node carries no Value - the indexer returns the
  value at that exact key, and a key that has only children has none. And
  `GetSection` is documented to never return null: it hands back an empty
  stub for any name. The one honest existence test is
  `GetSection(...).Exists()` (has a value or children). Two intuitive
  spellings, two opposite lies, one correct method nobody reaches for
  first.
- **Who hits it:** fail-fast startup validation: `if
  (config.GetSection("Smtp") == null) throw` ships in real codebases and
  never throws; the sibling `if (config["Smtp"] == null) throw` blocks
  boot with perfectly good config. Same pattern in library code accepting
  an IConfiguration.
- **Repro:** in-memory collection with `Smtp:Host`/`Smtp:Port`; print the
  checks side by side: indexer null for present section, GetSection
  non-null for missing one, `Exists()` telling the truth for both.
  Package: `Microsoft.Extensions.Configuration` only,
  `#:property PublishAot=false`. Deterministic.
- **Damage:** fail-fast silently degrades to fail-late: the app boots
  clean with the section missing, binds an empty options object, and dies
  hours later at first send - in a code path far from configuration, on
  the on-call engineer's clock.
- **NOTE on adjacency:** complements `binding-fails-silently` above - that
  entry is the binder keeping defaults; this one is the validation people
  write to *catch* that, also lying. Two candidates, two mental models
  (binder matching vs section existence); flagged in case the curator
  wants only one of the pair.
- **😈 seed:** while debugging, the missing section and the present one
  answer identically in the watch window - `config["Smtp"]` and
  `config["Nope"]` are both null - so the investigation itself runs on the
  same broken instrument.
- **Verified:** ran on .NET 10 (2026-07-22): indexer null for a populated
  section, GetSection non-null for a missing one, Exists() true/false
  respectively; the ==null guard never fired.

### options-miss-the-reload (A1,4)

- **Twist:** the config file hot-reloads and IConfiguration proves the new
  value is inside the process - but IOptions<T> keeps serving the boot-time
  value until restart: one key, two answers, in the same app, forever.
- **Mechanic:** `IOptions<T>` is a singleton computed at first resolve and
  cached; it subscribes to no change tokens. `IOptionsMonitor<T>` follows
  reloads via `CurrentValue`; `IOptionsSnapshot<T>` recomputes per scope.
  So a live config edit is *half*-applied: raw IConfiguration readers and
  monitor users see the new value, every IOptions consumer keeps the old
  one - and which camp a class is in is an injection-time detail.
- **Who hits it:** teams that edit appsettings.json on a running service
  because "reloadOnChange is on by default, no restart needed" - rate
  limits, log levels, integration toggles - with services written against
  IOptions, the variant every tutorial injects.
- **Repro:** in-memory config + `ServiceCollection.Configure<T>`; resolve
  IOptions (30); set the key to 99 and call `Reload()` (modeling the file
  watcher); print IConfiguration=99, IOptions=30, IOptionsMonitor=99,
  IOptionsSnapshot (new scope)=99. Packages:
  `Microsoft.Extensions.Configuration` + `.Binder` +
  `Microsoft.Extensions.DependencyInjection` + `Microsoft.Extensions.Options`
  + `.Options.ConfigurationExtensions`, `#:property PublishAot=false`.
  Deterministic.
- **Damage:** the emergency change visibly lands - the operator checks a
  log line backed by IConfiguration, sees the new limit, and stops
  looking - while the throttling middleware injected with IOptions keeps
  enforcing yesterday's number through the whole incident.
- **😈 seed:** after one live edit the process is a mosaic: two services
  disagree about the same limit because one took IOptions and the other
  IOptionsMonitor - a config value that differs *between classes*, which
  no config tooling can even represent.
- **Verified:** ran on .NET 10 (2026-07-22): after Reload -
  IConfiguration 99, IOptions 30, IOptionsMonitor 99, IOptionsSnapshot 99.

## Seeds

Not yet a full candidate - brainstorm before proposing.

- **config-array-gap-truncates** (A5) - bind a config array whose indices
  skip a number (0, 1, 3) and the binder reportedly stops at the gap: index
  3 silently dropped. Premise NOT yet run - verify before promoting.
