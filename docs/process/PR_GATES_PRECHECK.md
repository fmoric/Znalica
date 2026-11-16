# Gates on each pull request — the pre-check and run budgets — Znalica

This is part of `AGENTS.md` → *Gates on each pull request*, moved here because it is needed only around a
pre-check or a gate run: when its log folder is chosen, when it is selected, estimated, narrowed or given a budget,
when a pre-check collects no tests, and when a reviewer reports that a running suite will fail. Open it then. A gate rule the
project records for itself (such as `docs/process/INTEGRATION_BRANCH_GATES.md`) still wins over it, for what it
covers. The user's yes for a full-suite run and the other gates are in `AGENTS.md`; what to do after a failed,
stopped or over-budget run is in `docs/process/PR_GATES_RECOVERY.md`.

- **A fast, targeted pre-check comes before each gate run.** It runs the test files the change adds or edits, the
  test files of the modules it changes (and of their callers, where it changes a contract), and the listing tests in
  any package that pin what the change registers, removes or renames (a setting, table, route, entity, grant) —
  **never a whole package's tests folder**. Take them from what the project declares (package manifests, test
  markers or fixtures, or its selection script where it has one), not from memory. Then search the test folders for
  the old value the change moves and add every test that pins it.
- **Estimate it, and narrow it by hand.** Before it starts, estimate its time from the number of listed files and
  any per-test durations an earlier run reported, against a budget of about 8 minutes, or the project's gate rule's
  figure if lower; an estimate over budget means the selection is too wide, so narrow it by hand: drop the tests
  that only import or call what changed, keep the rest of the selection, by name, and say what was left out; where a
  changed contract's callers are most of the suite, name that untested risk in the gate report. Too wide is never a
  reason for a full run, and a tool's refusal is no gate decision. Run every run that has a budget under a timeout
  set to it.
- **Every pre-check and gate run writes each test's result to a log file as it goes**, each run its own new file in
  a folder inside the worktree that git ignores. The main session chooses that folder and names it in the brief:
  `git check-ignore <folder>/` confirms it, and where the project ignores no such folder, it adds one to the file
  `git rev-parse --git-path info/exclude` names.
- **A change every test depends on, and a schema change.** A change to something every test depends on (the lock
  file, root build or test config, CI config) is a reason to ask for the full run, never to start it: read that diff
  first and say whether a test can depend on what it changes (module names added to a linter list cannot). The
  pre-check stays targeted. A schema or migration change adds every test that uses the database; over budget, run
  those of the changed tables or modules and say so in the ask for the full run (`AGENTS.md`).
- **A pre-check that collects no tests.** A pre-check that collects no tests has not passed: check the selection,
  and if the change truly has no tests to select, say so. The gate run confirms: it is never the first run of the
  change's tests.
- **The gate run's budget.** The gate run's budget is its measured time (the wall-clock time of the last full local
  run, setup included, as the project's gate file or that run's log records it — never a figure from memory) and
  half again; a budget the gate file states is used as it stands. A run past its budget is stopped and reported, and
  its selection is checked before anything re-runs. A gate run not yet measured has no budget to stop at: one past
  about 20 minutes is reported to the user, who decides. Only the user raises a budget.
- **A full-suite run that is not yet measured or whose budget is over about 8 minutes is the main session's**: on
  the user's yes it starts it in the background — under its timeout where it has a budget; an unmeasured one runs
  without one and is watched — with a watcher, stops it itself when fixes come, and the developer runs only a
  targeted pre-check.
- **A reviewer reports that the running suite will fail.** When a reviewer reports that the running suite will fail,
  send that to the developer with the findings at once, as a fix that touches code, and the run still going is
  stopped by whoever started it. A fix that changes code then follows `AGENTS.md` → *After fixes that change code*;
  with no such fix, the main session reads that run's log, follows any failure there as after a failed run
  (`docs/process/PR_GATES_RECOVERY.md`), and otherwise asks the developer for the pre-check, then the user for the
  gate run.
