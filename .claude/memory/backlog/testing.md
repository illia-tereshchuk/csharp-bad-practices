# 🧪 testing

> Status: **planned**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

### async-void-test-always-passes (A5)

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
