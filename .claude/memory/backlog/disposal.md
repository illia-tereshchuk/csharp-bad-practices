# 🗑️ disposal

> Status: **opened**. Canonical hall registry (emoji, display name, opened/planned) is `.claude/memory/halls.md`.
> Entry format and maintenance rules are in `.claude/memory/backlog/README.md`.

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

## Seeds

Not yet a full candidate - brainstorm before proposing.

- **disposal:** Environment.Exit skips finally blocks and using disposal -
  pairs naturally with the-unflushed-tail as cause of the same symptom.

- **using-var-disposes-late** (A6,1) - `using var conn = Open();` does not
  dispose at the next blank line - it disposes at the end of the whole
  method, so the connection you "closed" stays open across everything below.

- **double-dispose-crashes** (A5) - a `using` block plus one explicit
  `Dispose()` calls Dispose twice; a non-idempotent implementation throws
  ObjectDisposedException, turning tidy cleanup into a crash.
