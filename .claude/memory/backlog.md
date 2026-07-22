# Backlog

Candidate exhibits, waiting to be picked. This file is the cache that
`propose-exhibits` and `contribute` read. It is written to be self-sufficient:
a fresh session - possibly a different, smaller model - must be able to render
the menu and build any candidate from this file alone, without re-deriving the
mechanics. When in doubt, spell things out; this file is allowed to be long.

## How to read an entry

Each candidate is one `###` block. The `(A n)` in its heading is the archetype
tag from `archetypes.md` - internal curation data, never shown in any menu.
Fields:

- **Twist** - the one-line hook. Menus show this line verbatim after the slug.
  Keep it under ~35 words, mechanic first.
- **Mechanic** - what precisely happens and why, so the builder does not have
  to rediscover the behavior. Include known compiler/API gotchas that would
  trip a naive Bad.cs.
- **Who hits it** - who / where / how in the real world. If this cannot be
  answered, the candidate does not belong here (vacuum-example rule).
- **Repro** - how Bad.cs fails deterministically in one console file: the
  approach, plus any `#:package` / `#:property` directives known to be needed.
  Note: file-based `dotnet run` uses an AOT profile by default, so anything
  using reflection-based JSON needs `#:property PublishAot=false` (precedent:
  #0012). DI needs `#:package Microsoft.Extensions.DependencyInjection@10.*`;
  EF/SQLite setup is in #0008's files.
- **Damage** - what the reader actually loses. "Reproduces a quirk" is not
  damage (no-real-damage rule; that is what killed sort-is-unstable).
- **😈 seed** (optional) - the one-rung-nastier angle for the README's 😈
  section (fear ladder: crash < wrong < silently wrong).
- **Verified** - what was actually executed versus taken from documentation.
  "ran on .NET 10 (date)" means the core premise was proven by running real
  code that day. Never write that without having run it: two past rejections
  (`datetime-kind-round-trip`, `firstordefault-on-structs`) were premise
  errors that running code would have caught.

## Maintenance rules

- Everything here already passed the curation filters: not primer-level, not
  a vacuum scenario, deterministic in a single console file, not proven by
  timing, not dependent on an unpinned environment, absent from `rejected.md`.
- Borderline-by-taste candidates are still included, flagged in the entry -
  the bar says: propose and let the curator judge, don't pre-cut.
- When the curator declines a candidate: record it in `rejected.md` and delete
  its block here **in the same edit**. The two files must never disagree.
- When an exhibit ships: delete its block, update `state.md`.
- Before adding a new candidate: run its core premise as real code (a
  scratchpad file is fine) and record the result in **Verified**.

---

## 🗂 collections

### dictionary-order-illusion (A6)

- **Twist:** Enumeration order looks like insertion order until one Remove;
  the next Add reuses the freed slot and the new key surfaces in the middle of
  the sequence.
- **Mechanic:** `Dictionary<K,V>` stores entries in an internal array and
  enumerates it in storage order. With no removals, storage order happens to
  equal insertion order, which trains the illusion that order is guaranteed.
  `Remove` puts the slot on a free list; the next `Add` fills the freed slot,
  so the newest entry enumerates where the deleted one used to be.
- **Who hits it:** anyone printing or exporting a dictionary and trusting the
  visible order - CSV exports, config dumps, dropdowns built from a
  Dictionary. Every test passes (tests rarely delete), production breaks
  after the first delete.
- **Repro:** build a small dictionary, print keys; Remove one entry, Add a new
  one, print again - the new key appears mid-sequence. No packages,
  deterministic.
- **Damage:** ordered output (menus, exports, hash-over-serialized payloads)
  silently reorders after the first delete in the data's lifetime.
- **😈 seed:** the layout is an implementation detail - a runtime upgrade may
  legally change observed order with zero code changes.
- **Verified:** documented internal layout, widely reproduced; verify at build.

### getoradd-runs-twice (A5,6)

- **Twist:** ConcurrentDictionary is thread-safe, your factory is not: two
  threads enter GetOrAdd together, the "runs exactly once" factory runs twice,
  and one result is silently discarded.
- **Mechanic:** `GetOrAdd(key, valueFactory)` invokes the factory *outside*
  the internal lock (documented). Two threads asking for the same missing key
  can both run the factory; only one produced value is stored, the loser is
  thrown away - but the loser's *side effects* are not undone.
- **Who hits it:** caches of expensive resources: connections, sessions,
  "create the customer row on first order". The factory opens a socket or
  INSERTs a row; under concurrency it does so twice.
- **Repro:** the factory increments a counter and blocks on a `Barrier(2)`, so
  the demo *proves* both threads are inside the factory simultaneously, then
  returns; assert factory ran 2 times while the dictionary holds 1 value.
  Deterministic - the barrier replaces any timing assumption. No packages.
- **Damage:** duplicate side effects (two rows, two charges, two connections)
  under a green log; the dictionary itself looks perfectly consistent
  afterwards, so nothing points at the cache.
- **😈 seed:** the standard fix is caching `Lazy<T>` - which walks straight
  into `the-cached-failure` (async hall). The two exhibits cross-link.
- **Verified:** ran on .NET 10 (2026-07-22): barrier repro, factory ran 2x,
  one value stored.

## 🔢 numbers

### remainder-is-not-modulo (A4)

- **Twist:** `%` is remainder, not modulo: a negative hash `% 10` is a
  negative bucket index. And the obvious fix, Math.Abs, throws on
  int.MinValue - the axiom is wrong twice.
- **Mechanic:** C# `%` keeps the sign of the dividend: `-7 % 3 == -1`, never
  2. `GetHashCode()` legitimately returns negatives for about half of all
  values, so `hash % buckets` is negative about half the time.
  `Math.Abs(int.MinValue)` throws OverflowException because +2147483648 does
  not fit in int. The correct form is `(int)((uint)hash % (uint)n)`.
- **Who hits it:** hand-rolled sharding and partitioning - "pick a
  queue/shard/bucket by key.GetHashCode() % N". Works for every key the dev
  tried, crashes (or mis-shards) on the first negative hash in production.
- **Repro:** IMPORTANT for the builder: do NOT use string hashes in the demo -
  string hashing is randomized per process, which would make the demo
  nondeterministic. Use keys whose hash you control: an int id (an int is its
  own hash, so a negative customer id like -12345 gives `-12345 % 10 == -5`)
  or a type with a hardcoded GetHashCode. Index an array with the result:
  IndexOutOfRangeException. No packages.
- **Damage:** crash on the first negative key; with the sloppier "fix"
  (`Math.Abs` or re-hashing), keys silently land in a different shard than
  the one that already holds their data.
- **😈 seed:** `Math.Abs(int.MinValue)` throws - and int.MinValue is a hash
  real values actually have. The crash hides for years behind its 1-in-4-billion
  trigger.
- **Verified:** ran on .NET 10 (2026-07-22): -7%3 == -1, -12345%10 == -5,
  Abs(int.MinValue) threw OverflowException.

## ⚡ async

### the-collected-timer (A6)

- **Twist:** A Timer with no stored reference gets collected mid-run; the
  "every minute" job stops with no error, no exception, no log line.
- **Mechanic:** `System.Threading.Timer` does not root itself. If the only
  reference is a local in the method that created it, the timer is garbage as
  soon as that reference dies, and its callbacks simply stop after the GC
  runs. Nothing observable fails at the moment it happens.
- **Who hits it:** "schedule a heartbeat/cleanup in Main or a constructor and
  ignore the return value". Survives all day on a dev machine (little GC
  pressure), dies quietly in production.
- **Repro:** create the timer inside a `[MethodImpl(MethodImplOptions.NoInlining)]`
  helper and do not store the returned reference (this matters: file-based
  `dotnet run` builds Debug, where locals stay alive to end of method - the
  helper method's return is what frees the timer). Then `GC.Collect()` +
  `GC.WaitForPendingFinalizers()`, wait two ticks' worth via a
  CountdownEvent on a *stored* control timer, and show the abandoned timer's
  counter stopped while the stored one kept counting. Forced GC keeps it
  deterministic. No packages.
- **Damage:** the recurring job - billing sweep, queue pump, heartbeat -
  silently stops. Discovered days later by absence, the hardest kind of
  evidence.
- **Verified:** documented GC-root behavior; the forced-GC repro is standard.
  Verify at build.

### semaphore-never-released (A5)

- **Twist:** An exception between Wait and Release leaks a permit forever; the
  next caller waits on a semaphore nobody will ever free.
- **Mechanic:** `SemaphoreSlim` has no ownership: nothing ties a permit to the
  code that acquired it, and nothing returns it automatically. If the guarded
  code throws and `Release()` is not in a `finally` (or the `try` starts too
  late), the count is down by one forever. After maxCount such failures every
  `WaitAsync` blocks indefinitely.
- **Who hits it:** throttling - `SemaphoreSlim(4)` around "at most 4
  concurrent calls to the payment API", with Release on the happy path only.
- **Repro:** `SemaphoreSlim(1)`; the guarded operation throws; catch and
  continue; the next `WaitAsync(TimeSpan.FromMilliseconds(200))` returns
  false - the permit is provably gone. No real waiting, no packages,
  deterministic.
- **Damage:** capacity shrinks one failure at a time until the system stands
  still. Logs show every original exception *handled* - the incident report
  says "it just got slower and slower until restart".
- **😈 seed:** the drained state outlives its cause: the flaky dependency
  recovers in seconds, your process never does.
- **Verified:** follows directly from SemaphoreSlim semantics; verify at build.

### parallel-foreach-async-lie (A1,5)

- **Twist:** Parallel.ForEach happily accepts an async lambda as async void:
  the loop "completes" before any body finishes, reports success, and has
  processed exactly nothing.
- **Mechanic:** `Parallel.ForEach` takes `Action<T>`; an async lambda
  converts to async void. An async void method returns to its caller at the
  first `await`, so ForEach sees every invocation "finish" instantly and
  returns. The real work continues unobserved on the thread pool; its
  exceptions are async-void exceptions (see #0007) and can kill the process
  later.
- **Who hits it:** "make this loop of API calls parallel" - the most common
  wrong answer to that request. Compiles clean, no warning, looks concurrent.
- **Repro:** the body awaits a gate (`TaskCompletionSource`) and only then
  increments a counter. Assert the counter is 0 on the line *after*
  Parallel.ForEach returns and print "batch finished OK" - then open the gate
  and show the work trickling in after "success". The TCS gate makes it fully
  deterministic - no sleeps, no races. No packages.
- **Damage:** the batch reports success having done nothing yet; failures are
  unobservable; and the "processed" items may still be mid-flight when the
  process exits, losing them entirely.
- **😈 seed:** Good.cs is `Parallel.ForEachAsync` (.NET 6+) - the fix has
  existed the whole time, one identifier away.
- **Verified:** ran on .NET 10 (2026-07-22): ForEach returned with 0 of 5
  items processed.

### the-cached-failure (A1,5)

- **Twist:** Lazy&lt;Task&gt; caches the task, not the value: one transient
  failure and every caller after it receives the same stale exception until
  the process restarts.
- **Mechanic:** the Lazy factory runs once; its return value - the Task
  *object* - is cached forever. If that task faults, the fault is now the
  cached "value": every subsequent await observes the same exception, and the
  factory never runs again. Identical trap with `ConcurrentDictionary<K,
  Task<V>>` caches.
- **Who hits it:** async caches - `Lazy<Task<Config>>`, task-per-key
  dictionaries - the textbook "share one flight between concurrent callers"
  pattern, minus failure eviction.
- **Repro:** a `Lazy<Task<string>>` whose factory counts invocations and
  throws; await it three times; the factory ran once and all three awaits got
  the same exception. Deterministic, no packages.
- **Damage:** a 2-second network blip at 09:00 becomes an outage lasting until
  someone restarts the process. The dependency is healthy; your cache
  re-serves the corpse of its one failure.
- **😈 seed:** health checks stay green - they probe the dependency, not your
  cache.
- **Verified:** ran on .NET 10 (2026-07-22): 3 awaits, 1 factory call, same
  cached exception each time.

### the-eliminated-await (A1,5)

- **Twist:** Delete a "redundant" await and return the task directly - the
  using block disposes the connection before the query touches it: the
  code-review tip that quietly breaks the method.
- **Mechanic:** `return await task` inside a method can be shortened to
  `return task` - a real optimization (skips one state machine) that blogs,
  analyzers and reviewers genuinely recommend. But `using`, `try/finally`
  and `catch` are part of the *method*: return the bare task and the method
  exits immediately, running Dispose while the returned task is still
  mid-flight. The awaited version kept the scope alive until the work
  finished. The elision is only safe when nothing after the return point -
  disposal, catch, finally - matters.
- **Who hits it:** anyone applying the well-known "elide async/await"
  advice; the advice is correct in plain pass-through methods and wrong
  inside any scope, and nothing in the code marks the difference.
- **Repro:** a helper with `using var conn = new FakeConnection()` returns
  `QueryAsync(conn)` where QueryAsync awaits a TaskCompletionSource gate and
  then calls `conn.Use()`; complete the gate after the helper returns -
  ObjectDisposedException. Fully deterministic, no packages.
- **Damage:** ObjectDisposedException at best; with resources that don't
  guard themselves, a query against a closed/recycled handle -
  use-after-free semantics in managed clothing.
- **😈 seed:** the `catch` sibling: with the await elided, your catch block
  never sees the task's exception - the method unwound long ago (pairs with
  the-eager-throw).
- **Verified:** ran on .NET 10 (2026-07-22): ODE from the disposed fake
  connection, gated and deterministic.

### the-timeout-that-stopped-nothing (A1,5)

- **Twist:** The classic WhenAny timeout pattern reports "timed out" and
  walks away - the abandoned work keeps running, its charge lands a second
  later, and its exception has no one left to crash.
- **Mechanic:** `Task.WhenAny(work, Task.Delay(t))` completes when the
  first task does; the loser is not cancelled - nothing even tries to stop
  it. The caller logs a timeout and usually retries, while the original
  work finishes anyway (double side effect) or faults (unobserved
  exception). "Timeout" in this pattern means "I stopped watching", not
  "it stopped happening".
- **Who hits it:** the standard timeout idiom around payments, HTTP calls,
  and "if it takes more than 5s, retry" logic - one of the most-pasted
  async snippets in existence.
- **Repro:** gate the work with a TaskCompletionSource; let
  Task.CompletedTask play the Delay that already expired; WhenAny declares
  timeout while the side-effect counter is 0; open the gate - the counter
  hits 1 *after* "timed out" was reported. Deterministic, no packages.
- **Damage:** retry-after-timeout doubles the charge: the "timed out"
  operation succeeded too, so reconciliation finds one order paid twice -
  silent money damage from a snippet everyone trusts.
- **😈 seed:** the loser's exception surfaces minutes later as
  TaskScheduler.UnobservedTaskException - a crash report pointing at
  nothing (cross-link #0019, #0021).
- **Verified:** ran on .NET 10 (2026-07-22): charge landed after the
  timeout verdict was already printed.

### the-self-deadlock (A4)

- **Twist:** SemaphoreSlim is the async replacement for lock - minus
  reentrancy: the method takes the "lock", calls a helper that takes it
  again, and the code waits forever for the permit it is holding.
- **Mechanic:** `lock`/Monitor are reentrant per thread; SemaphoreSlim(1,1)
  - the standard async mutex - has no ownership concept at all, so a nested
  WaitAsync in the same logical flow blocks on itself. Migrating locked
  code to async silently deletes reentrancy from the contract, and the
  compiler forbidding `await` inside `lock` is what pushes everyone onto
  SemaphoreSlim in the first place.
- **Who hits it:** codebases converting synchronized code to async: a
  guarded public method calls another guarded public method - a call graph
  that was legal for years under lock.
- **Repro:** SemaphoreSlim(1,1); WaitAsync; a nested
  `WaitAsync(TimeSpan.FromMilliseconds(200))` returns false - the
  self-deadlock, proven without hanging the demo (same technique as
  semaphore-never-released). Deterministic, no packages.
- **Damage:** a production hang with zero CPU, no exception, no log entry -
  the process just stops answering on the one code path where the nested
  call occurs.
- **😈 seed:** the reentrant path can hide behind a feature flag or a rare
  branch for months - the deadlock ships long before it fires.
- **Verified:** ran on .NET 10 (2026-07-22): nested WaitAsync timed out
  while the permit was held.

### the-double-wrapped-task (A4,1)

- **Twist:** Task.Factory.StartNew with an async lambda returns
  Task&lt;Task&gt;: awaiting it waits for the work to *start*, not finish -
  the await completes instantly, the work is unfinished, the exceptions are
  nobody's.
- **Mechanic:** StartNew knows nothing about async delegates: it runs the
  lambda, and an async lambda "returns" at its first await - handing back
  the real Task as a *result*. Awaiting the outer shell observes only "the
  lambda started". `Task.Run` exists precisely because of this: it unwraps
  automatically. One method name apart, opposite meaning.
- **Who hits it:** code cargo-culting StartNew "because it takes options",
  or predating Task.Run; someone adds `async` to the lambda during a
  refactor and every await of the result quietly stops meaning anything.
- **Repro:** `Task.Factory.StartNew(async () => { await gate; flag = true; })`;
  await the outer - flag is still false; only `Unwrap()`/awaiting the inner
  task observes the real work. Gate makes it deterministic, no packages.
- **Damage:** "completed" batches with zero work done and inner-task
  exceptions unobserved - the same lie as parallel-foreach-async-lie
  wearing a more respectable API.
- **Verified:** ran on .NET 10 (2026-07-22): outer task completed with the
  work provably not done.

### threadlocal-doesnt-follow (A6)

- **Twist:** ThreadLocal state does not follow the code across an await:
  the method resumes on another thread and the "per-request" cache is
  suddenly empty - or, worse, holds a different request's data.
- **Mechanic:** in a console/server app an await captures no thread
  affinity; the continuation runs wherever the scheduler puts it.
  ThreadLocal and [ThreadStatic] belong to the physical thread, so the
  async flow walks away from its own state - and the next unrelated work
  item scheduled onto the OLD thread inherits it. AsyncLocal is the
  flow-following twin (with its own trap: writes flow down the async call
  tree, never back up).
- **Who hits it:** per-request ambient state written pre-async and still
  running: current user, current tenant, thread-keyed caches and buffers.
- **Repro:** BUILDER WARNING - the naive `await Task.Yield()` demo does NOT
  guarantee a thread hop (verified live: it happily resumed on the same
  pool thread). Deterministic technique: set the ThreadLocal on a dedicated
  `new Thread` which starts the async method (it runs synchronously to the
  first await, then the thread *exits*); complete the gate from the main
  thread. The continuation cannot run on a dead thread, so the hop is
  guaranteed, and the ThreadLocal reads its default after the await. No
  packages.
- **Damage:** the empty-state case is the lucky one; the unlucky one is
  cross-request bleed - tenant A resumes on a pool thread still warm with
  tenant B's ThreadLocal. Correctness and privacy, fully silent.
- **Verified:** ran on .NET 10 (2026-07-22) with the dedicated-thread
  technique; the Task.Yield version was tried and rejected by that same
  run.

### the-hijacked-completion (A6,5)

- **Twist:** TaskCompletionSource.SetResult is not a notification - it
  synchronously runs every waiting continuation on YOUR thread before
  returning: the "signal" line just executed foreign code inside your
  critical section.
- **Mechanic:** by default, completing a TCS runs attached continuations
  inline on the completing thread. An innocuous `tcs.SetResult(value)`
  while holding a lock (or any mid-flight invariant) reenters arbitrary
  awaiter code right there: reentrancy, deadlocks, and stack dives under
  completion chains. `TaskCreationOptions.RunContinuationsAsynchronously`
  is the one-argument axiom fix.
- **Who hits it:** infrastructure code - hand-rolled async queues, caches,
  pub-sub - anywhere a producer completes a TCS that consumers await.
- **Repro:** an async consumer awaits tcs.Task and records its thread id;
  SetResult from the main flow; the recorded id equals the setter's, and
  print ordering shows the consumer ran *inside* the SetResult call.
  Deterministic, no packages.
- **Damage:** the producer "signals" while holding a lock; the awakened
  consumer takes the same lock - reentrancy corrupting state (same
  thread), or instant deadlock (SemaphoreSlim). Production hangs traced to
  a line that looks incapable of blocking.
- **😈 seed:** CancellationToken.Register callbacks are the same trap -
  Cancel() runs them inline too.
- **Verified:** ran on .NET 10 (2026-07-22): continuation executed inside
  SetResult, on the setter's thread.

### the-eager-throw (A4)

- **Twist:** Delete the "pointless" async keyword from a one-line method
  and exceptions change their address: validation now throws at the call
  site, not at the await - and the tasks you had already collected are
  abandoned mid-flight.
- **Mechanic:** an async method routes *every* exception - including one
  thrown before the first await - into the returned task. A non-async
  Task-returning method throws synchronously at the call. Identical
  success-path behavior, different failure address; nothing in the
  signature reveals which one you are calling.
- **Who hits it:** `var tasks = items.Select(x => client.SendAsync(x)).ToList();
  await Task.WhenAll(tasks);` - if SendAsync validates eagerly (elided
  form), one bad item throws during ToList: the try/catch around WhenAll
  never runs, and the requests already started are left running unobserved
  (#0019's damage, reached through a different broken model).
- **Repro:** two methods with identical bodies, one `async`, one not; call
  both with a bad argument: the elided one throws at the call, the
  keyworded one returns a task with IsFaulted true. Then the Select/WhenAll
  shape to show the abandoned in-flight work. Deterministic, no packages.
- **Damage:** error handling sits in the reviewed-and-approved wrong place;
  half a batch runs unobserved after the "handled" crash.
- **Verified:** ran on .NET 10 (2026-07-22): call-site throw vs faulted
  task, exactly as described.

### the-linked-leak (A6)

- **Twist:** CreateLinkedTokenSource hooks your per-request token to the
  app-lifetime token - forget Dispose and the app token holds that hook
  forever: the request's object graph outlives the request, by design.
- **Mechanic:** a linked CTS registers a callback on its parent token; that
  registration roots the linked CTS - and everything its own registrations
  capture - until the parent dies or the linked CTS is disposed. With a
  process-lifetime parent (shutdown/app token), "forgot Dispose" means
  "leaks until restart", one request at a time.
- **Who hits it:** the per-request timeout pattern -
  `CreateLinkedTokenSource(appStoppingToken)` + CancelAfter - dropped
  without `using`: a famous slow-leak shape in long-running services.
- **Repro:** a NoInlining helper creates a linked CTS over a long-lived
  parent, registers a callback capturing a payload, returns a WeakReference
  to the payload; forced GC: payload alive without Dispose, collected with
  it - both branches in one run. Deterministic, no packages.
- **Damage:** memory grows request by request through a retained path that
  runs entirely inside framework registration lists - the profiler shows no
  reference from user code; the "restart cures it" leak.
- **😈 seed:** those forgotten registrations all *fire* at shutdown -
  thousands of stale per-request callbacks executing at the worst possible
  moment.
- **Verified:** ran on .NET 10 (2026-07-22): payload rooted while
  undisposed, collected once disposed.

### the-overlapping-timer (A6,5)

- **Twist:** System.Threading.Timer does not wait for your callback: let
  the work outgrow the period and two invocations run concurrently - the
  "every minute" job starts racing itself.
- **Mechanic:** the timer fires on schedule regardless of whether the
  previous callback returned, so a slow tick overlaps the next one: two
  threads inside code written as if it runs once at a time. `PeriodicTimer`
  (`await WaitForNextTickAsync` in a loop) is the modern shape that cannot
  overlap - the axiom fix.
- **Who hits it:** cleanup/billing/queue-pump jobs on Timer - correct for
  years while the table was small, self-racing the week it grew.
- **Repro:** determinism note: this one needs real ticks (short period),
  but the assertion is structural, not a timing measurement - the first
  callback blocks on a CountdownEvent(2) that only the second callback's
  arrival can open, *proving* two are inside simultaneously; generous
  timeouts bound the wait. No packages.
- **Damage:** the billing sweep processes the same rows twice,
  concurrently - double charges produced by the job that existed to prevent
  them.
- **😈 seed:** overlap compounds: each slow tick makes the database
  slower, which makes more ticks overlap - the job DDoSes itself.
- **Verified:** ran on .NET 10 (2026-07-22): CountdownEvent proof, two
  callbacks inside at once.

### the-pool-that-ate-itself (A5,6)

- **Twist:** One .Result "just this once" per request, and under load the
  thread pool deadlocks itself: every thread is blocked waiting for a
  continuation that needs a thread - the outage with zero CPU and zero
  errors.
- **Mechanic:** sync-over-async parks a pool thread until an async
  continuation completes - but the continuation needs a pool thread too.
  When blockers hold the whole pool, nothing can ever complete: a circular
  wait through the scheduler, with no lock anywhere in the code. In real
  services the pool is larger, so it appears only under load as a
  mysterious stall with idle CPU.
- **REVISIT NOTE for the curator:** `rejected.md` contains ".Result
  deadlock", declined because the SynchronizationContext version cannot
  reproduce in a console app. This is the *other* .Result deadlock -
  starvation-based, no context involved - and it reproduces
  deterministically by pinning the pool, which is the explicitly-allowed
  "code fixes the environment" pattern. Flagged openly rather than
  re-proposed silently; the curator judges whether the objection is
  answered.
- **Who hits it:** "we just need the value here" - .Result / .Wait() /
  GetAwaiter().GetResult() in constructors, property getters, and sync
  interface implementations over async code.
- **Repro:** `ThreadPool.SetMinThreads(1,1)` + `SetMaxThreads(2,2)` (the
  pin that makes at-scale behavior reproducible on two threads); two
  Task.Run blockers each doing `Inner().GetAwaiter().GetResult()` over an
  `await Task.Delay(100)`; `Task.WaitAll(blockers, 3s)` returns false -
  zero progress, ever. BUILDER WARNING: this demo wrecks the pool - it
  must be the last thing Bad.cs does. Deterministic, no packages.
- **Damage:** total service stall under load with zero CPU, zero
  exceptions, nothing in logs - among the hardest production incidents to
  diagnose, and restart "fixes" it until the next traffic peak.
- **😈 seed:** the real pool grows ~1 thread per second trying to save
  you, so production sees slow-motion collapse instead of a clean hang -
  which is exactly why staging never reproduces it.
- **Verified:** ran on .NET 10 (2026-07-22): pinned pool, 3-second budget,
  both workers wedged, no progress.

## 🔗 linq

### average-ignores-the-nulls (A4,5)

- **Twist:** Average over a nullable column divides by the non-null count, not
  the row count - the more data goes missing, the better the metric looks.
- **Mechanic:** `Enumerable.Average(IEnumerable<int?>)` skips nulls entirely:
  {10, null, 20} averages to 15, not 10. Overload resolution picks the
  nullable version silently because the element type is `int?` - the code
  reads identically to the non-nullable case. On an all-null sequence it
  returns null (while the non-nullable overload on an empty sequence throws) -
  so the failure modes differ too.
- **Who hits it:** any report over a database column that allows NULL -
  average rating, average response time - where the business reads "average"
  as covering all rows, but null rows quietly leave the denominator.
- **Repro:** one `int?[]`; show `Average() == 15` while the intended
  per-row average is 10; then an all-null array where the KPI comes back null
  instead of raising any flag. Deterministic, no packages.
- **Damage:** KPIs that *improve* as data collection breaks - the dashboard
  rewards the outage. Sum has the same skip rule, so Sum/Count cross-checks
  disagree with Average on the same table.
- **😈 seed:** same business question, two column types: non-nullable empty
  crashes loudly, nullable all-null returns a polite null - the silent one is
  the production one.
- **Verified:** ran on .NET 10 (2026-07-22): Average of {10, null, 20} == 15.

### oftype-eats-the-evidence (A4,5)

- **Twist:** Cast&lt;T&gt; throws on the first wrong element; OfType&lt;T&gt;
  silently drops it - the "safer" spelling of the same line quietly deletes
  records.
- **Mechanic:** both filter an untyped sequence to T. `Cast<T>` throws
  InvalidCastException at the first non-T; `OfType<T>` skips non-Ts (that is
  its contract - but people reach for it as "Cast that doesn't crash").
  Swapping one for the other converts a loud type bug into silent record
  loss. Extra nuance: OfType also drops nulls; Cast passes them through.
- **Who hits it:** legacy non-generic collections (ArrayList, DataTable
  rows), heterogeneous object graphs, deserialized payloads - anywhere
  someone "fixes" a Cast crash by switching to OfType instead of asking why a
  wrong-typed item exists at all.
- **Repro:** an object[] of order lines with one wrong-typed element; the
  Cast version crashes honestly; the OfType version totals one line short and
  reports success. Deterministic, no packages.
- **Damage:** totals and exports silently missing records - and the type bug
  OfType was hiding ships unfixed, forever.
- **Verified:** ran on .NET 10 (2026-07-22): Cast threw InvalidCastException,
  OfType returned 2 of 3.

## 🔔 events

### one-handler-kills-the-rest (A5)

- **Twist:** A subscriber that throws stops the invocation list: everyone
  registered after it never runs, and neither the publisher nor the surviving
  subscribers ever find out.
- **Mechanic:** raising an event walks the invocation list in subscription
  order, synchronously, on one thread. An unhandled exception in handler #2
  aborts the walk: handlers #3..#N are skipped, and the exception surfaces at
  the *publisher's* raise line, in code that has no idea what subscribers
  exist.
- **Who hits it:** in-process pub/sub: an order-placed event feeding email +
  audit + analytics handlers. Whoever subscribed last is at the mercy of
  everyone who subscribed earlier.
- **Repro:** three subscribers appending to a list; the middle one throws;
  show the third never ran and the publisher's post-raise code was skipped
  too. Deterministic, no packages.
- **Damage:** audit/bookkeeping handlers silently skipped whenever an
  unrelated handler fails - the compliance record has holes exactly where
  incidents happened.
- **😈 seed:** subscription order - effectively DI registration order -
  decides who dies. A reordered registration list is a behavior change no
  diff reviewer sees.
- **Verified:** language-level delegate semantics; verify at build.

### invoke-race-on-null-check (A1)

- **Twist:** `if (E != null) E(...)` - the last subscriber unsubscribes
  between the check and the call: NullReferenceException from an event that
  "was just checked".
- **Mechanic:** delegates are immutable; subscribe/unsubscribe swap the field.
  Check-then-invoke reads the field twice, and the value can change between
  the reads. `E?.Invoke(...)` (or copy-to-local) reads once - that single
  read is the entire fix.
- **Who hits it:** any event raised while subscribers come and go: UI
  teardown, service shutdown, plugin unload.
- **Repro:** determinism note for the builder - do NOT race two threads in a
  loop (nondeterministic per-iteration; the timing ban applies). Stage the
  interleaving single-threaded instead: perform the null check, then
  unsubscribe the last handler (this is the other thread's step, frozen in
  time), then perform the invoke from the field - NRE, 100% reproducible.
  It is honest because the staged interleaving is exactly what the two-thread
  version does when it loses.
- **Damage:** shutdown-time crashes that reproduce monthly in production and
  never on a dev machine.
- **Verified:** language-level; staged repro chosen to satisfy determinism.
  Verify at build.

## 📦 value-types

### the-vanishing-mutation (A3)

- **Twist:** Mutating a struct taken from a List edits a temporary copy; the
  identical line against an array works fine - so the collection is the last
  thing anyone suspects.
- **Mechanic:** `list[i]` calls the indexer, which *returns a copy* of the
  struct; `arr[i]` is direct storage access. BUILDER WARNING: the assignment
  form `list[i].X = 5` does not even compile (CS1612) - the compiler blocks
  the obvious spelling. The trap that ships is the method form:
  `list[i].Translate(5)` compiles without a whisper and mutates the copy. So
  Bad.cs must use a mutating *method* (or a `var tmp = list[i]; tmp.X = 5;`
  sequence), not direct member assignment.
- **Who hits it:** structs in Lists - points, money amounts, game entities.
  The array version worked yesterday; today someone changed `T[]` to
  `List<T>` in one place and every mutation became a no-op.
- **Repro:** same mutating method called on `arr[i]` (works) and `list[i]`
  (silently does nothing); print both. Deterministic, no packages.
- **Damage:** updates that no-op silently - balances never change, positions
  never move - while the identical code elsewhere (arrays) works, actively
  pointing the investigation away from the cause.
- **😈 seed:** `foreach` over a List of structs hands out copies too - the
  "fix everything in a loop" pass fixes nothing.
- **Verified:** CS1612 vs method-call nuance is language-specified; verify at
  build (the CS1612 note is load-bearing for Bad.cs).

### the-skipped-initializer (A4)

- **Twist:** Struct field initializers run for `new S()` but not for `default`
  or array elements - the same struct is born with different values depending
  on who created it.
- **Mechanic:** field initializers on a struct execute only as part of a
  constructor call. `new S()` invokes the parameterless constructor, so
  initializers run; `default(S)` and `new S[n]` just zero memory - no
  constructor, no initializers. BUILDER WARNING: a struct with field
  initializers and no declared constructor does not compile (CS8983), so the
  demo struct must declare `public S() { }`.
- **Who hits it:** structs given "sensible defaults" via initializers
  (`Rate = 1.0m`, `Enabled = true`) then materialized through arrays, `out`
  parameters, or `default` - every such instance carries zeros and falses
  where the author promised 1.0 and true.
- **Repro:** `struct WithInit { public decimal Rate = 1.5m; public WithInit() {} }`;
  print `new WithInit().Rate` (1.5), `default(WithInit).Rate` (0), and
  `new WithInit[1][0].Rate` (0). Three lines, deterministic, no packages.
- **Damage:** a multiplier that "defaults to 1" is 0 in every array-born
  instance: totals multiply to zero - silent money-math corruption.
- **😈 seed:** `Enabled = true` flips to false the same way - a permission or
  feature silently defaults OFF only on the code path that used an array.
- **Verified:** ran on .NET 10 (2026-07-22): 1.5 / 0 / 0 exactly as above.

## 💥 exceptions

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

## 🗄 orm

### stale-tracked-entity (A5)

- **Twist:** The change tracker returns the entity it cached earlier: your
  fresh query runs real SQL, fetches fresh rows - and hands you back the old
  object with the old values.
- **Mechanic:** EF Core's identity map guarantees one instance per key per
  DbContext. When a query materializes a row whose key is already tracked,
  EF discards the just-fetched scalar values and returns the tracked
  instance unchanged. The SELECT visibly runs; its results are thrown away.
- **Who hits it:** long-lived contexts - background jobs, desktop apps,
  captive contexts (#0022) - re-reading "current" data that another process
  updated in between.
- **Repro:** two DbContexts over one SQLite database file: context A loads
  the entity; context B updates the row and saves; context A queries again
  and still sees the old value; `Entry(...).Reload()` or a fresh context sees
  the new one. Packages and setup as in #0008 (`Microsoft.EntityFrameworkCore.Sqlite`,
  `SQLitePCLRaw.bundle_e_sqlite3`, `#:property PublishAot=false`).
  Deterministic.
- **Damage:** decisions (price checks, stock checks, permission checks) made
  against yesterday's values while the SQL log shows the fresh SELECT that
  "fetched" them - an audit trail that actively lies.
- **😈 seed:** nothing short of Reload or a new context fixes it - the same
  context that lied to you also reports the entity as Unchanged.
- **Verified:** documented identity-map behavior; verify at build with the
  #0008 setup.

### untranslatable-where (A4)

- **Twist:** Extract a predicate into a helper method - the refactor every
  reviewer approves - and the query that compiled and passed every unit test
  throws at runtime: EF cannot translate your method to SQL.
- **Mechanic:** EF Core builds SQL from expression trees; a call to your own
  method inside `Where` has no translation, and since EF Core 3 the query
  throws InvalidOperationException ("could not be translated") instead of
  silently downloading the table. The same predicate written inline
  translates fine - the difference is invisible in the code's meaning, only
  in its shape.
- **Who hits it:** everyone who refactors shared predicates ("IsActive(c)")
  out of queries. Compiles; green against in-memory lists; explodes on the
  first real database query.
- **Repro:** SQLite EF setup as #0008; `.Where(c => IsVip(c))` throws; the
  same expression inlined returns rows. Deterministic.
- **Damage:** honest crash, but at runtime in production, in a query the
  type system and the test suite both blessed. The exhibit's lesson is the
  displaced failure point.
- **😈 seed:** the pre-3.0 behavior was *silent* client evaluation - and it
  still exists today: insert `.AsEnumerable()` before the Where and the
  "fix" quietly downloads the entire table to filter it in memory.
- **Verified:** documented EF Core 3+ behavior; verify at build.

## 📄 serialization

### tuples-serialize-to-nothing (A5)

- **Twist:** System.Text.Json serializes properties; tuples are all fields -
  so your (id, total) goes over the wire as {} and comes back as zeros, with
  no error in either direction.
- **Mechanic:** STJ ignores public fields unless `IncludeFields = true`.
  `ValueTuple`'s Item1/Item2 are fields, so a tuple serializes to `{}`. The
  friendly names (`(int id, decimal total)`) are compiler fiction that never
  exists at runtime, so even with IncludeFields you get Item1/Item2, never
  your names. Deserializing `{}` into a struct yields all defaults - #0012's
  tolerant reading completes the silent round trip.
- **Who hits it:** quick internal APIs, cache layers, queue messages where
  someone returns a tuple "for now"; plus older DTOs using public fields
  instead of properties - same rule, same empty object.
- **Repro:** `JsonSerializer.Serialize((1042, 149.99m)) == "{}"` - one line.
  Needs `#:property PublishAot=false`. Deterministic.
- **Damage:** order id 0, amount 0.00, HTTP 200 everywhere; data loss with
  every status green.
- **😈 seed:** `IncludeFields = true` "fixes" it into
  `{"Item1":1042,"Item2":149.99}` - the data survives but the contract is
  still garbage, and every consumer now binds to Item1/Item2 forever.
- **Verified:** ran on .NET 10 (2026-07-22): Serialize((1, "a")) == "{}".

### the-renumbered-status (A2,5)

- **Twist:** STJ writes enums as bare numbers; insert one member and
  yesterday's stored "Cancelled" deserializes as today's "Shipped" - every
  archived record silently rewrites its own history.
- **Mechanic:** default enum serialization is the underlying integer - a
  *positional* identity. Reordering members, inserting one, or alphabetizing
  the file re-maps every number to whichever member now wears it. No error is
  possible: any integer deserializes into an enum, defined or not. The bug
  spans deploys, which is why no single-version test can ever catch it.
- **Who hits it:** anyone persisting JSON - documents in a DB, messages in a
  queue, cached API responses - across more than one release of the code.
- **Repro:** simulate two deploys in one file: serialize
  `StatusV1 { Pending=0, Shipped=1, Cancelled=2 }`, deserialize the same
  string as `StatusV2 { Pending=0, OnHold=1, Shipped=2, Cancelled=3 }`
  (someone inserted OnHold) - stored Cancelled(2) now reads as Shipped.
  Needs `#:property PublishAot=false`. Deterministic.
- **Damage:** cancelled orders start shipping; the audit trail stays
  internally consistent and entirely wrong - textbook silent data
  corruption with money stakes.
- **😈 seed:** `JsonStringEnumConverter` protects new writes but cannot fix
  the numbers already stored - by the time the bug is noticed, the corruption
  is baked into the archive.
- **Verified:** ran on .NET 10 (2026-07-22): V1.Cancelled round-tripped into
  V2.Shipped.

## 💉 di-lifetimes

### the-silent-override (A4)

- **Twist:** Two registrations of one interface are both legal; the last one
  wins silently - the handler you spent an hour debugging never resolved at
  all.
- **Mechanic:** MS.DI accepts duplicate registrations: single resolution
  (`GetRequiredService<T>`) returns the *last* registration;
  `GetServices<T>` returns all of them. Which implementation runs is decided
  by registration order across Program.cs and every AddXyz extension - a
  global, invisible, order-sensitive contract.
- **Who hits it:** two teams' extension methods both registering
  IEmailSender; or a test override that stops overriding after an innocent
  reorder of builder calls.
- **Repro:** register two implementations of one interface; resolve - only
  the last runs; swap the two registration lines - behavior flips with a diff
  that looks like formatting. `#:package Microsoft.Extensions.DependencyInjection@10.*`,
  `#:property PublishAot=false`. Deterministic.
- **Damage:** production behavior decided by call order in composition code,
  ungreppable from any use site; debugging happens in an implementation that
  is never actually invoked.
- **😈 seed:** `TryAdd*` exists precisely for this and *inverts* the rule -
  first wins. Two idioms, opposite winners, both silent.
- **Verified:** documented container behavior; verify at build.

### the-twin-singletons (A4)

- **Twist:** One class registered under two interfaces yields two
  "singletons" - the state you thought was shared is quietly split in half.
- **Mechanic:** `AddSingleton<IReader, Store>()` plus
  `AddSingleton<IWriter, Store>()` creates one instance *per registration*,
  not per class: "singleton" binds to the service type, not the
  implementation. The fix is registering the concrete once and forwarding:
  `AddSingleton<Store>(); AddSingleton<IReader>(sp => sp.GetRequiredService<Store>()); ...`
- **Who hits it:** the reader/writer interface split: a class implementing
  ICache + ICacheInvalidator, registered under both. Invalidation goes to
  one instance; reads come from the other.
- **Repro:** resolve both interfaces, `ReferenceEquals` is false; write
  through one, read through the other, the write is invisible. DI package +
  `#:property PublishAot=false`. Deterministic.
- **Damage:** caches that never invalidate, metrics split across two
  collectors - each half internally consistent, jointly wrong, and the class
  in question really is instantiated "once"... per interface.
- **Verified:** ran on .NET 10 (2026-07-22): two registrations produced two
  instances (ReferenceEquals false).

## 📅 datetime

### kind-blind-equality (A4)

- **Twist:** 14:00 UTC equals 14:00 local - `==` compares ticks and
  completely ignores Kind, so two different instants in time are "equal" and
  two representations of the same instant are not.
- **Mechanic:** DateTime is a tick count plus a Kind flag; `==`, `<`,
  CompareTo, GetHashCode all use ticks only. Every comparison, sort, and
  dictionary lookup inherits the blindness. DateTimeOffset compares the
  actual instant - the type choice is the fix.
- **Who hits it:** codebases mixing DateTime.Now and DateTime.UtcNow (all of
  them), and values loaded from databases as Kind=Unspecified compared
  against UtcNow.
- **Repro:** two DateTimes with equal ticks and different Kinds: `==` true.
  BUILDER WARNING: do not call `.ToUniversalTime()` or `.ToLocalTime()` to
  show they differ - those depend on the machine's zone (CI-would-lie rule).
  Pin everything: convert with `TimeZoneInfo.FindSystemTimeZoneById` on a
  fixed zone, or contrast with DateTimeOffset values built from explicit
  offsets. Deterministic once pinned. No packages.
- **Damage:** expiry checks ("token still valid?") pass or fail by wall-clock
  coincidence - security-adjacent silent wrongness that flips with the
  server's timezone.
- **Verified:** `==` semantics documented; verify at build with pinned zones.

### the-25-hour-day (A6)

- **Twist:** AddHours(24) is not "tomorrow, same time" - across a DST
  transition the same wall time is 23 or 25 hours away, and the daily job
  drifts an hour off, twice a year.
- **Mechanic:** DateTime arithmetic is pure tick math; wall-clock time is
  ticks *interpreted through a zone*, and on two days a year the mapping
  shifts. "next run = last + 24h" lands on 02:00 instead of 03:00 after the
  spring transition. Correct scheduling converts through TimeZoneInfo at each
  occurrence instead of adding a fixed duration.
- **Who hits it:** hand-rolled daily schedulers, "24-hour" token lifetimes,
  billing cut-offs - anything that *means* "03:00 local tomorrow" but
  *computes* +24 hours.
- **Repro:** pin `TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv")` (never
  TimeZoneInfo.Local - CI-would-lie), pick the known transition date, show
  last+24h converts to 02:00 wall time, not 03:00. Deterministic because zone
  and date are constants. No packages.
- **Damage:** the maintenance window fires during business hours; daily
  boundaries shift so one "day" of records is 23 hours long.
- **BUILDER WARNING:** #0020 (shrinking-billing-day) lives in this hall and
  is also DST-driven. Before building, read #0020 and aim this exhibit at
  the *scheduler drift* (next-run computation), not the day-length itself;
  if the overlap still feels too close, propose replacing rather than
  duplicating.
- **Verified:** timezone math documented; verify at build with pinned zone,
  after the #0020 overlap check.

## 🗑️ disposal

### the-unflushed-tail (A5)

- **Twist:** Forget to dispose a StreamWriter and the file ends mid-sentence:
  the last buffer never flushes, the process exits with code 0, and the
  export is quietly short.
- **Mechanic:** StreamWriter buffers characters; Dispose/Flush writes the
  tail. Nothing else does: there is no flushing finalizer, and normal process
  exit does not flush live writers. The file contains a whole number of
  buffer-fulls - in the verification run, exactly 4096 of 8400 bytes. A
  small file whose content fits one buffer is 0 bytes: created, empty,
  "successfully written".
- **Who hits it:** quick export/report scripts and log writers - `new
  StreamWriter(path)` without `using`, in code where "it worked when I
  tested it" (the test data fit the buffer... or didn't, and the file being
  short went unnoticed).
- **Repro:** write ~200 lines, don't dispose, read the file back (StreamWriter
  opens with FileShare.Read, so a same-process FileStream with
  FileShare.ReadWrite can read it): length is less than written. Also show
  the 10-line case: 0 bytes. Keep the writer alive with GC.KeepAlive so the
  finalizer story doesn't muddy the demo. Deterministic, no packages.
- **Damage:** the nightly export is missing its last N rows; the file exists,
  opens, and parses, so reconciliation finds the loss weeks later - money and
  audit stakes, screenshots well.
- **😈 seed:** how *much* is lost depends on buffer alignment, so every run
  loses a different tail - the bug report is never reproducible.
- **Verified:** ran on .NET 10 (2026-07-22): 4096 &lt; 8400 bytes; small file
  0 bytes.

### the-cleanup-that-never-came (A5,6)

- **Twist:** "The GC will clean it up" - the GC provably collects the object
  and provably never calls Dispose: collected and leaked at the same time.
- **Mechanic:** the GC runs *finalizers*, and Dispose is not a finalizer -
  they are unrelated methods that most classes never connect. A type with
  Dispose and no finalizer (most application classes) simply never runs its
  cleanup when abandoned. A WeakReference proves collection happened; a flag
  in Dispose proves cleanup did not.
- **Who hits it:** everyone who has answered "what if I forget Dispose?"
  with "the GC handles it eventually": connections not returned to pools,
  transactions never completed, file locks held.
- **Repro:** class whose Dispose sets a static flag, no finalizer; create it
  in a `[MethodImpl(MethodImplOptions.NoInlining)]` helper, keep only a
  WeakReference; `GC.Collect()` + `WaitForPendingFinalizers()` + `Collect()`;
  assert the reference is dead AND the flag is false. Forced GC keeps it
  deterministic. No packages.
- **Damage:** pooled resources drain until the "restart fixes it" incident;
  the leak survives every memory profiler check, because the *memory* is
  fine.
- **BORDERLINE flag:** closest of the current batch to the primer floor -
  "GC doesn't call Dispose" is teachable-quote material. Kept because the
  collected-yet-not-cleaned double proof is a genuine "wait, both?"; the
  curator judges.
- **Verified:** ran on .NET 10 (2026-07-22): WeakReference dead, Dispose flag
  false.

## ⚖️ equality

### equals-but-not-equal (A4)

- **Twist:** The same two objects are equal inside a HashSet and unequal in an
  if-statement: Equals was overridden, == was not, and both spellings look
  interchangeable.
- **Mechanic:** overriding Equals/GetHashCode does not touch `operator ==`;
  for classes it remains reference comparison. Collections and LINQ
  (Contains, Distinct, HashSet, Dictionary) call Equals; hand-written ifs use
  `==`. One value-object type, two equality regimes, selected by syntax - and
  no compiler warning exists for the mismatch.
- **Who hits it:** value objects written as classes with Equals overridden -
  Money, Address, DateRange - and then `if (total == expected)` in a check
  somewhere. Records exist precisely to kill this bug, which makes Good.cs a
  one-word rewrite.
- **Repro:** Money class overriding Equals/GetHashCode: `a.Equals(b)` true,
  `a == b` false, `new HashSet<Money> { a }.Contains(b)` true - three
  contradictory-looking lines in a row. Deterministic, no packages.
- **Damage:** a payment-matching branch that silently never matches while
  every collection lookup around it works - behavior differs between "is it
  in the set" and "is it the same", on identical data.
- **😈 seed:** the trap inverts for strings: `==` on string is value equality
  - so developers *trained on strings* expect `==` to follow Equals
  everywhere else.
- **Verified:** ran on .NET 10 (2026-07-22): Equals true, == false, HashSet
  Contains true.

## 📇 records

### record-equality-skin-deep (A4,5)

- **Twist:** Record value equality is one property deep: add a List and two
  identical-looking records stop being equal, Distinct stops deduplicating,
  and dictionary lookups quietly miss.
- **Mechanic:** record equality compares each member with
  `EqualityComparer<T>.Default`; for List/array/Dictionary members that is
  *reference* equality. GetHashCode composes the same way, so hashes differ
  too and everything stays self-consistent - wrong, but never inconsistent
  enough to throw.
- **Who hits it:** records as DTOs and value objects with a `List<string>
  Tags` or `Items` array: test assertions, Distinct, records as cache keys.
- **Repro:** `record Order(int Id, List<string> Tags)`; two same-content
  instances: `!=` is true, `Distinct()` keeps both, a Dictionary keyed by
  one misses the other, hashes differ. Deterministic, no packages.
- **Damage:** cache keyed by records: 100% miss rate (a cost bug that looks
  like a traffic increase); test assertions that fail on equal data - or
  pass on unequal data once someone "fixes" the test with reference reuse.
- **😈 seed:** cross-link #0028: `with` copies the *reference*, so the two
  records that ARE equal share one list - mutate one, both change.
  Equal-when-they-shouldn't-be and unequal-when-they-should-be, from the
  same design gap.
- **Verified:** ran on .NET 10 (2026-07-22): records unequal, hashes differ.

---

## Planned halls (first candidate opens the hall 🚪)

### 🕳️ nullability - null-forgiving-lies (A5)

- **Twist:** `!` silences the compiler and changes nothing at runtime: a
  promise you made, not a check anyone performs - and the flow analysis now
  propagates your lie forward.
- **Mechanic:** the null-forgiving operator is erased at compilation; it
  emits no check. Worse, it *teaches* the nullable flow analysis that the
  value is non-null, so warnings downstream of the `!` disappear too - one
  suppression hides a family of them.
- **Who hits it:** nullable-migration codebases: `FirstOrDefault()!`,
  `Config["key"]!`, `default!` in constructors - each one a warning paid off
  with a promise.
- **Repro:** warning-free code with one `!` that NREs at runtime; sibling
  code without `!` that the compiler correctly flags. Deterministic, no
  packages.
- **Damage:** the annotation system reports the codebase clean while the
  NREs it exists to prevent ship anyway - false confidence at project scale.
- **Verified:** language-level erasure; verify at build.

### 🧬 generics - static-field-per-closed-type (A6)

- **Twist:** A static field in `Cache<T>` is not one field - it is one field
  *per T*, and the "global" cache silently shards itself by type argument.
- **Mechanic:** statics live on the closed constructed type: `Cache<int>` and
  `Cache<string>` each get their own copy. A limit, pool, or registry in a
  generic base class multiplies invisibly.
- **Who hits it:** generic base classes with static counters/caches/config -
  `Repository<T>.ConnectionCount` - where the author meant one number for
  the process.
- **Repro:** increment the "shared" static through two type arguments; print
  both copies diverging. Deterministic, no packages.
- **Damage:** connection limits that don't limit, singletons that aren't
  single, caches that miss because the entry went into a sibling.
- **Verified:** CLR-specified behavior; verify at build.

### 🏷️ enums - the-overlapping-flags (A5)

- **Twist:** [Flags] values numbered 1, 2, 3: the third flag IS the first two
  OR-ed together, so granting Delete silently grants Read and Write - and
  every HasFlag check happily agrees.
- **Mechanic:** flags combine by bitwise OR, so members must be powers of
  two. Sequential numbering makes 3 == 1|2: setting "flag 3" sets both lower
  bits; checking it answers true whenever both others are present. Nothing
  in the language or runtime objects.
- **Who hits it:** whoever adds the third member to a two-member [Flags] enum
  by continuing the sequence 1, 2, 3 - the single most natural wrong move in
  the API.
- **Repro:** `[Flags] enum Perm { Read = 1, Write = 2, Delete = 3 }`;
  `(Read | Write).HasFlag(Delete)` is true - a user granted read+write can
  delete. Deterministic, no packages.
- **Damage:** permission escalation - security stakes, screenshots well.
- **😈 seed:** the enum prints correctly (`Delete`), logs look right, and
  audits confirm the user "had the Delete flag" - the corruption extends
  into the investigation.
- **Verified:** ran on .NET 10 (2026-07-22): (Read|Write).HasFlag(Delete)
  == true.

### 🪆 inheritance - virtual-call-in-constructor (A1)

- **Twist:** The base constructor calls a virtual method that runs on the
  derived class *before the derived constructor body has run* - the override
  reads its own fields and finds nulls.
- **Mechanic:** construction order in C#: derived *field initializers* run
  first, then the base constructor, then the derived constructor *body*. A
  virtual call from the base constructor dispatches to the derived override
  (no "partial" dispatch exists), which executes against an object whose
  constructor-body assignments have not happened yet. BUILDER WARNING: get
  the order right in the README - fields set via *initializers* ARE visible
  (C# differs from Java here); only constructor-*body* state is missing.
- **Who hits it:** template-method base classes ("call Initialize() in the
  base ctor, let derived classes override it") - a design that looks like
  good OO and is a construction-order trap.
- **Repro:** derived class assigning a field in its constructor body; base
  constructor calls the virtual; the override NREs (or, nastier, computes
  with the default). Deterministic, no packages.
- **Damage:** NRE at construction in the loud version; in the quiet version
  the override caches a decision computed from default values, and the
  object is subtly misconfigured for its whole lifetime.
- **Verified:** language-specified construction order; verify at build,
  including the initializer-vs-body distinction.

### 🧩 pattern-matching - switch-expression-not-exhaustive (A5)

- **Twist:** Add one enum member and a switch expression that "covered
  everything" starts throwing in production - the compiler only ever warned,
  and the warning was easy to ship.
- **Mechanic:** a switch expression over an enum with all members handled
  compiles clean; when a new member appears, callers get warning CS8509 (not
  an error) and, at runtime, SwitchExpressionException for the unhandled
  value. Teams without warnings-as-errors ship it. The tempting "fix" -
  a `_ => default` arm - silences the warning forever and converts future
  crashes into silently wrong values.
- **Who hits it:** enums in shared contract libraries: the enum grows in one
  repo, the switch lives in another; each compiles happily on its own
  schedule.
- **Repro:** simulate the two-versions situation in one file (the
  renumbered-status trick): switch over a value cast from an int the enum
  does not define, or model V1/V2 enums; the switch throws
  SwitchExpressionException. Deterministic, no packages.
- **Damage:** runtime crash in code everyone believed total; with the `_`
  arm, silently wrong routing instead - one rung down the fear ladder.
- **Verified:** compiler and runtime behavior documented; verify at build.

### 🥊 boxing - mutating-a-boxed-struct (A3)

- **Twist:** Call a mutating method on a struct through an interface and the
  box mutates - your variable never changes, and each new cast makes a fresh
  box, so the mutation isn't even *somewhere*: it's nowhere.
- **Mechanic:** casting a struct to an interface copies it into a heap box;
  interface dispatch mutates the box. Cast again - new box, old state. The
  variable on the stack is never touched.
- **Who hits it:** structs stored as interfaces: `List<IShape>`,
  `IEnumerator` implementations (the classic), method parameters typed as
  the interface.
- **Repro:** counter struct implementing IIncrement; increment through the
  interface-typed reference and through the variable; print the divergence,
  then show two casts producing independent boxes. Deterministic, no
  packages.
- **Damage:** state machines that never advance, counters stuck at zero -
  and the same code with a class works, pointing suspicion anywhere but the
  cast.
- **Verified:** CLR boxing semantics; verify at build.

### 🪞 reflection - setvalue-into-the-void (A3)

- **Twist:** PropertyInfo.SetValue on a struct writes into the box reflection
  just created and throws it away - your variable never changes, and no API
  anywhere reports that the write went nowhere.
- **Mechanic:** SetValue takes `object`: passing a struct variable boxes a
  copy; the setter runs against the box; the box is discarded. Classes work
  fine through the same code path, so the mapper "works" until the first
  struct DTO. (The fix that keeps structs: box once explicitly, SetValue
  into that box, unbox at the end.)
- **Who hits it:** hand-rolled mappers, config binders, test data builders -
  reflective property-setting loops written for classes that one day meet a
  struct.
- **Repro:** struct with an auto-property; GetProperty + SetValue; the
  variable still holds the old value. Deterministic, no packages.
- **Damage:** every reflected write silently no-ops: settings objects full
  of defaults, mapped DTOs half-empty - and only for the struct-typed ones,
  which makes the pattern look haunted.
- **Verified:** ran on .NET 10 (2026-07-22): SetValue on the boxed copy,
  variable unchanged.

### 💾 memory - the-closure-that-held-everything (A6)

- **Twist:** A lambda that captured one small int keeps a 100 MB array alive
  - because the compiler put every captured variable of the scope into one
  shared closure object, and your little callback owns all of it.
- **Mechanic:** the compiler generates one display class per scope; all
  variables captured by *any* lambda in that scope live on it. A long-lived
  delegate that captured only `retryCount` also roots the giant buffer a
  neighboring lambda captured. Restructuring the scopes (or copying to
  locals in a nested block) breaks the tie.
- **Who hits it:** event handlers and callbacks registered inside methods
  that also touched large data - upload handlers, report generators.
- **Repro:** WeakReference to the big array; store only the small lambda;
  forced GC; the array is still alive. Move the lambda into a separate
  method - collected. Forced GC keeps it deterministic. No packages.
- **Damage:** memory leaks with no reference to the big object anywhere in
  user code - the retained path exists only in compiler-generated classes,
  where nobody looks.
- **Verified:** documented compiler lowering (shared display class per
  scope); verify at build with WeakReference proof.

### 🌐 http - baseaddress-eats-your-path (A4)

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

### ⚙️ configuration - binding-fails-silently (A5)

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

### 🪵 logging - interpolated-log-loses-everything (A4)

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

### 🔤 regex - missing-anchors-pass-anything (A5)

- **Twist:** A "digits only" check without anchors accepts abc123def -
  IsMatch looks for a match *anywhere* - and even the anchored `^\d+$`
  still accepts "123\n", because `$` matches before a final newline.
- **Mechanic:** IsMatch answers "does a match exist somewhere in the
  input" - validation semantics require `^...$`. Second layer, verified:
  `$` (and `Z`) match before a string-final `\n`; only `\z` means true
  end-of-string. So the standard fix still lets a trailing newline through.
- **Who hits it:** input validation on ids, amounts, codes - regexes copied
  from a matcher context into a validator context.
- **Repro:** `Regex.IsMatch("abc123def", @"\d+")` true;
  `Regex.IsMatch("123\n", @"^\d+$")` ALSO true; only `@"^\d+\z"` rejects
  both. Deterministic, no packages.
- **Damage:** validated input that still carries payloads (injection
  prefixes, trailing newlines corrupting line-based formats downstream) -
  the check exists, reviewed, and passes garbage.
- **Verified:** ran on .NET 10 (2026-07-22): `^\d+$` accepted "123\n".

### 🧪 testing - async-void-test-always-passes (A5)

- **Twist:** A failed assertion in an async void test is never observed by
  the runner: the suite is green while the code under test is broken.
- **Mechanic:** an async void method returns at its first await; the runner
  sees a normal return and marks the test passed; the assertion failure
  later surfaces as an unobserved async-void exception (crashing something
  unrelated, or nothing). HONESTY NOTE for the proposer: modern frameworks
  (xUnit v3, NUnit) detect and fail async void *test methods*, so frame the
  who around delegate-based helpers, custom runners, and
  `Assert.ThrowsAsync`-style lambdas passed as `Action` - or present the
  history angle openly and let the curator judge.
- **Who hits it:** teams with homegrown test helpers taking `Action`
  callbacks that someone hands an async lambda; the same shape as
  parallel-foreach-async-lie, in the one place where silent success costs
  the most trust.
- **Repro:** a minimal hand-rolled runner (reflection over methods, no
  framework packages) invoking an async void "test" whose assertion fails
  after `await Task.Yield()`; runner prints PASS; gate with a TCS to show
  the failure arriving after the verdict, deterministically.
- **Damage:** the safety net reports safety it is not providing - the whole
  point of a test suite, inverted.
- **Verified:** async-void semantics verified in batch 1 (parallel-foreach);
  the runner framing to verify at build.

### 📁 io - readalltext-guesses-encoding (A5)

- **Twist:** Read a legacy-encoded file with File.ReadAllText and the text
  comes back mangled instead of failing - the decoder quietly substitutes
  replacement characters and the corruption ships downstream.
- **Mechanic:** ReadAllText defaults to UTF-8; bytes that aren't valid UTF-8
  decode to U+FFFD without any exception (the strict-throwing UTF8Encoding
  variant exists and is not the default). The damage happens at ingestion
  and is baked in by the first save.
- **Who hits it:** ingesting exports from legacy systems - bank CSVs,
  ERP dumps in Windows-125x encodings - on the .NET side that assumes UTF-8.
- **Repro:** BUILDER TIP: avoid the CodePages package - use
  `Encoding.Latin1` (built-in) to write "café" as Latin-1 bytes, read with
  ReadAllText: "caf�". Both encodings pinned in code, fully deterministic,
  no packages.
- **Damage:** customer names and addresses permanently corrupted in the
  database; the earliest anyone notices is a customer complaint, long after
  the original files are gone.
- **😈 seed:** strict decoding (`new UTF8Encoding(false, true)`) would have
  crashed honestly at ingestion - silence is the default, correctness is
  opt-in.
- **Verified:** documented decoder defaults; verify at build with the Latin1
  approach.

### 🧵 strings - length-lies-about-emoji (A4)

- **Twist:** `"👍".Length` is 2 - so a 50-character truncate slices an emoji
  in half and sends a lone surrogate to production, where it renders as �
  or breaks the downstream encoder.
- **Mechanic:** Length counts UTF-16 code units; astral-plane characters
  (all modern emoji) take two (a surrogate pair), ZWJ sequences take many.
  `Substring` cuts between units without complaint, producing an invalid
  lone surrogate. Honest APIs: `EnumerateRunes`, `StringInfo` (text
  elements).
- **Who hits it:** truncation for column widths, SMS limits, UI previews -
  any `s.Substring(0, 50)` over user-generated text, which now always
  contains emoji.
- **Repro:** show `"👍".Length == 2`; truncate a string mid-pair; print the
  result and re-encode to UTF-8 to show the replacement. Deterministic, no
  packages.
- **Damage:** corrupted text stored and re-served; downstream strict systems
  (JSON encoders, databases) reject or mangle the payload - a data-quality
  bug seeded by an innocent-looking truncate.
- **😈 seed:** the family emoji `"👨‍👩‍👧‍👦".Length` is 11 - "one character" by
  any human count, eleven by the API the code trusts.
- **Verified:** UTF-16 representation facts; verify at build.

### 🧵 strings - mojibake-factory (A4)

- **Twist:** Decode bytes with the wrong encoding, save the result, and the
  text is gone for good: "Привіт" becomes "ÐŸÑ€Ð¸Ð²Ñ–Ñ‚" - readable proof
  of what one wrong round-trip does.
- **Mechanic:** UTF-8 bytes read as Latin-1/1252 turn each multi-byte
  character into two garbage-but-valid characters; the mistake is invisible
  to the type system (it's all "string") and *reversible* until the first
  save re-encodes the garbage as genuine UTF-8 - then the original bytes no
  longer exist.
- **Who hits it:** any boundary with a charset assumption: files, HTTP
  bodies, DB connections - plus the "fix" where someone saves the mangled
  text back "corrected".
- **Repro:** pin both encodings in code (UTF-8 bytes of a Ukrainian string,
  decoded as `Encoding.Latin1`): print the mojibake; round-trip once more to
  show the point of no return. Deterministic, no packages, no CodePages
  needed with Latin1.
- **Damage:** permanent corruption of every non-ASCII name in the batch -
  and the demo doubles as the museum's most shareable screenshot.
- **BUILDER NOTE:** adjacent to readalltext-guesses-encoding (silent U+FFFD
  substitution vs. reversible-then-baked mojibake). Both can live in the
  hall, but cross-link and keep the mechanics distinct; if only one is
  wanted, the curator picks.
- **Verified:** encoding math; verify at build.

### 🔒 security - interpolated-injection (A4)

- **Twist:** The same `$"...{name}..."` is parameterized SQL in
  FromSqlInterpolated and an injection in FromSqlRaw - identical syntax,
  opposite fate, and the compiler happily allows both.
- **Mechanic:** FromSqlInterpolated receives FormattableString and turns
  each hole into a DbParameter; FromSqlRaw receives the already-formatted
  string, holes pre-concatenated. An interpolated string converts to string
  implicitly, so passing it to the Raw overload compiles without a hint.
- **Who hits it:** EF Core raw-SQL users; reviewers cannot tell safe from
  unsafe without reading the method name, which is exactly one word
  different.
- **Repro:** SQLite EF setup as #0008; a name input of `' OR '1'='1` -
  Interpolated returns one row, Raw returns the whole table. Deterministic,
  self-contained database.
- **Damage:** SQL injection - the museum's clearest security stakes, and the
  Bad/Good diff is a single method name.
- **Verified:** documented EF API design; verify at build with #0008 setup.

### 🔒 security - guessable-random (A6)

- **Twist:** `Random` for password-reset tokens: same seed, same "random"
  token - and the seed is guessable, so the attacker doesn't break your
  crypto, they just run your line of code.
- **Mechanic:** System.Random is a deterministic PRNG: the seed fully
  determines the sequence, and its state is recoverable from observed
  outputs. The legacy pattern `new Random(someTimeValue)` narrows the seed
  to a searchable window. RandomNumberGenerator is the unpredictable one -
  the class name is the entire fix.
- **Who hits it:** reset tokens, invite codes, temporary passwords, CSRF-ish
  nonces built with `Random` because it was already imported.
- **Repro:** two `new Random(seed)` with the same seed generate identical
  "tokens" - frame one as the server, one as the attacker who guessed the
  seed. Deterministic by construction, no packages. (Do not claim
  parameterless `new Random()` is time-seeded on modern .NET - it isn't;
  the exhibit is about Random being deterministic and non-cryptographic,
  and about the legacy explicit-seed pattern.)
- **Damage:** account takeover via predicted reset token - maximal stakes,
  minimal code.
- **Verified:** PRNG determinism by definition; the honesty note about
  modern seeding recorded so the README doesn't overclaim. Verify at build.

---

## Seeds (ideas not yet vetted enough for a block - brainstorm before proposing)

- **exceptions:** a throw inside `finally` *replaces* the in-flight
  exception - the original error vanishes entirely. Real and deterministic;
  MUST check overlap with #0017 (finally-that-lied) before promoting.
- **linq / collections:** GroupBy on reference-equality keys (every item its
  own group) - real, but the damage is loud, not silent; needs a framing
  where it stays wrong quietly before it clears the bar.
- **equality:** default struct Equals on floating-point fields is *bitwise*
  when the struct has no reference fields - so +0.0 vs -0.0 and NaN behave
  opposite to `==` on the same values. Deep-weeds; needs a floor-clearing
  frame and a premise run before proposing.
- **async:** ValueTask must be awaited exactly once, immediately - a second
  await (or a stored one) over a pooled IValueTaskSource throws or returns
  another operation's data. Real and modern, but needs a deterministic
  pooled-source repro and a digestibility check before promoting.
- **async:** Monitor and Mutex are thread-affine - releasing after an await
  can throw SynchronizationLockException because the continuation changed
  threads. Deterministic via the inline-continuation technique proven in
  the-hijacked-completion; promote once framed below the ceiling.
- **async:** Task.Delay(0) completes synchronously and never yields, while
  Task.Yield always does - "give others a turn" written with Delay(0) does
  nothing. Probably a 😈 section inside another exhibit, not a standalone.
- **disposal:** Environment.Exit skips finally blocks and using disposal -
  pairs naturally with the-unflushed-tail as cause of the same symptom.
- **io:** relative paths resolve against the current working directory, not
  the exe location - real (services, schedulers), but needs a framing that
  clears the primer floor.
- **http:** most HttpClient lore (socket exhaustion, stale DNS) fails the
  single-file determinism bar; wait for a pure in-process angle before
  proposing anything here beyond baseaddress-eats-your-path.
