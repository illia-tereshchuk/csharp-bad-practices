# 🪵 logging

> Status: **opened**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

## Seeds

Not yet a full candidate - brainstorm before proposing.

- **log-args-evaluated-when-disabled** (A5) -
  `logger.LogDebug("state {S}", ExpensiveDump())` still calls ExpensiveDump()
  when Debug is off - the level check happens inside the logger, after the
  argument was computed.

- **log-args-bind-by-position** (A4,5) - structured-log placeholders bind by
  position, not name, so `"{User} did {Action}"` called with (action, user)
  records both fields under the wrong keys.
