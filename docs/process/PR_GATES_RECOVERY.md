# Gates on each pull request — recovery — Znalica

This is part of `AGENTS.md` → *Gates on each pull request*, moved here because it is needed only after a run goes
wrong. Open it after a failed, stopped or over-budget run, before anything re-runs. A gate rule the project records
for itself (such as `docs/process/INTEGRATION_BRANCH_GATES.md`) still wins over it, for what it covers. *After fixes
that change code* and the other gates are in `AGENTS.md`. Where a bullet below says a log is read, the main session
gives the log's full path to `reader` (`AGENTS.md` → *Delegating to an agent*) and opens the lines it cites.

- **After a failed run, re-run only what failed** — the failing tests or files by name, since a runner's
  last-failed option can fall back to the whole suite — until it passes. A test that then passes with nothing
  changed is not fixed: compare with the commit before the change (an earlier full run on it, or the same tests in
  the same order there). Passing there means the change caused it, so fix it; failing there too means it was flaky
  already, so report it as a finding and do not fix it unasked. Then commit any change. A code change is a fix,
  so its review passes first (*After fixes that change code*). Then the pre-check, and the gate run on a new yes from the user. If a test found flaky this way fails the
  pre-check or the gate run too, neither is repeated: report it, and the user decides. A test that fails by name and
  also fails on the commit before the change is reported the same way. A run stopped to fix a failure it printed is a
  failed run: its log is read first, so one fix round answers every failure there. Never use a full run to learn
  whether a fix worked. **The same failure twice after your edits is a fix loop, not bad luck:** stop patching
  and follow the `diagnose-bug` skill from step 3, even in the middle of a feature.
- **Any stopped run, fixes included: before you report it stopped, check that its processes are gone** — list the
  processes whose command line names the runner (its interpreter, such as `python` or `dotnet`, and the test command)
  and that started with the run; stop those of a run you started yourself, and anything else only when the user
  names it. Stopping a shell or an agent can leave the
  runner and its workers running, and a run still going is reported as running.
- **A run stopped before its end**, for any reason but fixes (`AGENTS.md` covers those), **is read before
  anything re-runs: its failures are in its log.** Run each by name, as after a failed run; first check any printed
  at the moment of the stop, since a worker killed by the stop can show as one. Where the names are lost, run a
  narrower selection than the stopped run: the tests the output points to and the test files the change adds or
  edits. **A run is never repeated unchanged to recover names, and that is never recommended.** With no failure to
  follow: a run whose agent or shell ended within its budget, not stopped by the user, is run by the main session (a
  full-suite run on a new yes from the user, `AGENTS.md`); a
  targeted pre-check that went over its budget is narrowed by whoever ran it (`docs/process/PR_GATES_PRECHECK.md`) and runs again. Any other run, such as a gate run stopped at its budget
  or by the user, is reported with what you checked: the gate is still open, and the user decides what happens next,
  including any change to the time limit.
- **A slow gate is a finding.** Have the gate run report per-test durations. When it takes over about 20
  minutes, say where its time goes (setup apart from the test body) and raise that with the user as its own
  piece of work; do not fix it unasked. While that finding is open, and only where CI runs the full suite on
  the PR, the user may approve the pre-check as this PR's local gate; the approval covers one PR, and the PR
  is reported ready only once CI's full run has passed.
