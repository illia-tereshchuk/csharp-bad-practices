# 🌐 http

> Status: **planned**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### baseaddress-eats-your-path (A4)

- **Twist:** A BaseAddress without a trailing slash silently drops its last
  segment: every `/v1/users` call goes to `/users` - pure Uri math, wrong by
  RFC, no server needed to prove it.
- **Mechanic:** Uri composition follows RFC 3986: resolving a relative
  reference against a base *replaces the last segment* unless the base ends
  with `/`. And a relative path starting with `/` discards the base path
  entirely. Both rules are correct standard behavior and both delete parts
  of your URL.
- **Who hits it:** every HttpClient configured with
  `BaseAddress = new Uri("https://api.example.com/v1")` and called with
  relative paths.
- **Repro:** `new Uri(new Uri("https://api.example.com/v1"), "users")` →
  no `/v1`. Show all four slash combinations in a table of printed Uris.
  Deterministic, no packages, no network.
- **Damage:** with a versioned API, `/v1` quietly disappears - and if the
  unversioned route exists (latest version), the calls *succeed* against
  the wrong API version: silent contract drift, not even a 404 to save you.
- **Verified:** RFC-specified Uri math; verify at build (trivial).

## Seeds

Not yet a full candidate - brainstorm before proposing.

- **http:** most HttpClient lore (socket exhaustion, stale DNS) fails the
  single-file determinism bar; wait for a pure in-process angle before
  proposing anything here beyond baseaddress-eats-your-path.

- **leading-slash-ignores-baseaddress** (A4) - a request path starting with
  "/" throws away the BaseAddress path entirely: with base `.../v1`,
  `GetAsync("/users")` goes to `/users`. Pure Uri math - likely the second
  act or 😈 of baseaddress-eats-your-path rather than its own exhibit.

- **no-ensuresuccess-reads-error-body** (A5) - `GetAsync` does not throw on
  404/500; without EnsureSuccessStatusCode the code deserializes the error
  page as if it were the payload. In-process repro via a fake
  HttpMessageHandler.

- **timeout-looks-like-cancellation** (A4,5) - HttpClient's own timeout
  surfaces as TaskCanceledException, the same type a user cancel throws, so
  the catch treats a server timeout as "user changed their mind" and skips
  the retry.
