# 💥 exceptions

> Status: **opened**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### activator-hides-the-real-error (A5)

- **Twist:** Reflection wraps the constructor's exception in
  TargetInvocationException - so the catch block written for the real
  exception type never fires, and the retry logic retries the unretryable.
- **Mechanic:** `Activator.CreateInstance` and `MethodInfo.Invoke` wrap any
  exception thrown inside the invoked member in TargetInvocationException;
  the real error is `.InnerException`. A `catch (ValidationException)` around
  the reflective call is dead code. (`BindingFlags.DoNotWrapExceptions`
  exists for Invoke precisely because of this.)
- **Who hits it:** plugin loaders, convention-based factories, serializers
  and test harnesses - any code that constructs types reflectively and wants
  typed error handling around it.
- **Repro:** a type whose constructor throws InvalidOperationException;
  `catch (InvalidOperationException)` around CreateInstance does not fire;
  the TargetInvocationException escapes. Deterministic, no packages.
- **Damage:** typed recovery paths never execute; generic handlers retry
  permanently-broken plugins forever, or log "TargetInvocationException"
  while the actionable error hides one level deeper.
- **Verified:** documented wrapping behavior; verify at build.

### the-swallowed-filter (A5)

- **Twist:** An exception thrown inside a `when` filter is silently discarded
  and the filter counts as false - your catch just doesn't match, and nothing
  anywhere records why.
- **Mechanic:** exception filters run during the first pass of exception
  handling; if the filter itself throws, the runtime swallows the secondary
  exception and treats the filter as false. The original exception continues
  to outer handlers. The filter's own bug is unobservable by design - no log,
  no fail-fast, nothing.
- **Who hits it:** `catch (ApiException e) when (e.Code == config.RetryCode)`
  styles - filters touching config or state that can be null. The filter's
  NRE fires exactly and only when the exception it filters is in flight, i.e.
  exactly when you needed the handler.
- **Repro:** inner `throw new InvalidOperationException("original")`; a catch
  whose filter dereferences null; an outer catch that receives the *original*
  exception; print that the filter's NRE was observed nowhere. Deterministic,
  no packages.
- **Damage:** the retry/fallback path silently never triggers, and the reason
  is invisible in every log - among the most expensive classes of bug to
  diagnose in production.
- **😈 seed:** fear-ladder inversion: here the *crash* is the silent case - a
  filter that merely computed the wrong boolean could at least be read and
  debugged.
- **Verified:** ran on .NET 10 (2026-07-22): throwing filter treated as
  false, original exception reached the outer catch, filter's NRE
  unobservable.

## Seeds

Not yet a full candidate - brainstorm before proposing.

- **exceptions:** a throw inside `finally` *replaces* the in-flight
  exception - the original error vanishes entirely. Real and deterministic;
  MUST check overlap with #0017 (finally-that-lied) before promoting.
