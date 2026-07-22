# 🪵 logging

> Status: **planned**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### interpolated-log-loses-everything (A4)

- **Twist:** `$"user {userId} failed"` formats before the logger ever sees
  it: the structured field you would search by never exists, and every log
  line becomes a unique string.
- **Mechanic:** string interpolation happens at the call site; the logger
  receives a finished string. Message templates
  (`"user {UserId} failed", userId`) pass the values as structured state
  that sinks index. Same rendered text, entirely different queryability -
  the two spellings are one `$` apart.
- **Who hits it:** everyone; analyzer CA2254 flags it but ships as a
  suggestion most builds ignore.
- **Repro:** hand-rolled ILogger (or a capturing provider) printing the
  state payload: the template call yields KeyValuePairs including UserId;
  the interpolated call yields only the pre-rendered string. Package:
  `Microsoft.Extensions.Logging.Abstractions` (or implement ILogger by hand,
  zero packages). Deterministic. Do NOT argue the perf angle (timing ban) -
  the lost structure is the exhibit.
- **Damage:** during the incident, searching logs by user/order id finds
  nothing - observability was silently discarded months earlier, one call
  site at a time.
- **Verified:** mechanism is call-site language semantics; verify at build.

## Seeds

Not yet a full candidate - brainstorm before proposing.

- **log-args-evaluated-when-disabled** (A5) -
  `logger.LogDebug("state {S}", ExpensiveDump())` still calls ExpensiveDump()
  when Debug is off - the level check happens inside the logger, after the
  argument was computed.

- **log-args-bind-by-position** (A4,5) - structured-log placeholders bind by
  position, not name, so `"{User} did {Action}"` called with (action, user)
  records both fields under the wrong keys.
