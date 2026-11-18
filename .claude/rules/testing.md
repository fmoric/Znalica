---
paths:
  - "Assets/Znalica/Tests/**/*.cs"
---

# Testing Rules

- Every behavior change has a test — happy path, error case, edge case.
- Tests must **assert behavior**, not just execute code. No assertion-free tests.
- **Assert the exact value when it is knowable**, not a bound (`<=`, "not empty", "not null"). Derive the expected
  value from the requirement, never from the code under test or by copying its output. Snapshot, golden-file and
  characterization tests are the exception: their recorded output is the expectation, so each update is reviewed
  as a change in behavior.
- **Test through the interface callers use.** An internal refactor should change no test. Never change a test's
  expected value to get it green (a snapshot update is the exception above). When the requirement really changed,
  name it in a comment beside the test, where a reviewer reading the diff will see it.
- **Prefer the real collaborator** when it is fast and deterministic, and a fake for a slow or external one. Mock
  only where the call itself is the behavior, and where the language's mocking library can check the real
  signature, use that.
- **A test that fails in a full run and passes alone has an order or state problem**, and a re-run only hides it.
  Tell your regression from an old flaky test as `docs/process/PR_GATES_RECOVERY.md` → *After a failed run* says. Where to look:
  dependence on test order or leftover state, a clock read inside the code under test, unseeded randomness.
- Run your change's tests before and after it (the full suite needs the user's yes); fix tests you break in the **same** change, by fixing the code, or
  by updating calls to an interface the task changed, or by updating the expectation only where the requirement
  changed (above). A suite that reads or writes
  a database or service is `production_is_read_only`'s: a production signal on its target stops the run; no
  signal, proceed.
- Name each test for the scenario it verifies.
- Never disable or skip a test without a written reason.
