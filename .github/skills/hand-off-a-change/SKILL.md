---
name: hand-off-a-change
description: Use when handing a change through its pull-request gates — the task to the developer, review rounds, findings back, "PR is ready" to the user.
---

# Hand a Change Through Its Gates

The main session uses these blocks when it hands a change on: to the developer, to the code-reviewer, back to the
developer with the findings, and to the user when the pull request is ready. Fill every `<…>`. Delete a line that
does not apply rather than leave it empty. The rules behind each step are in `AGENTS.md` → *Gates on each pull
request* and *Delegating to an agent*, in `docs/process/PR_GATES_PRECHECK.md` for selecting, narrowing and budgeting a run, and after a failed, stopped or slow run in `docs/process/PR_GATES_RECOVERY.md`; this skill only puts them in order. **A gate rule the project records for
itself wins over these briefs** (`AGENTS.md`): fill the blocks to match it.

## The order

1. **The developer builds, commits, and rebases once.** Every change the task asks for is committed before any gate
   starts; then the branch is rebased onto the latest default branch, so the review and the runs cover the rebased
   commits. Fix rounds do not rebase; if the PR can no longer merge, a new rebase is the main session's call, and
   its resolved conflicts are handled like a fix round's changes (`AGENTS.md`).
2. **Code review and the pre-check start together.** When the developer's final rebased commit lands, the main
   session sends the review (**final means the developer has reported the change done**, or its brief named the
   commit that ends it: a commit landing is not that signal, and on a branch cut from an unmoved default branch the
   "rebased" test already passes on the first commit); the developer runs the pre-check (`docs/process/PR_GATES_PRECHECK.md`: targeted, never a whole package's
   tests folder — the test files the change adds or edits, the test files of the modules it changes and, where
   it changes a contract, of their callers, the listing tests in any package that pin what the change
   registers, removes or renames, and the tests that pin the old value the change moves; within a budget of
   about 8 minutes; it stays targeted, step 5). **The full suite after that commit starts only on the user's yes
   for that run** (`AGENTS.md`: no agent starts a full-suite run on its own decision): when the pre-check
   **and the review** have passed (review fixes would void an earlier yes), the main session asks, with the commit,
   the reason, the measured time and what the pre-check covered. On a no, the PR-ready report says the full suite
   was not run and why, and the ask is not repeated for that commit. No ask where the user approved the pre-check
   as this PR's gate (step 5). A full suite
   not yet measured or whose budget is over about 8 minutes is run by the main session
   in the background with a watcher, never left to the developer (`AGENTS.md`). A review
   only reads the change, so it cannot disturb the run. Check the run started after the last commit against
   `git log -1 --format=%cd` (the commit date; an amend or rebase keeps the author date).
3. **Fixes are collected, then applied together.** The developer applies every finding from one review in one go. If
   any fix touches code (production code, a test, a fixture, config), the run still going **is stopped by whoever started it** (the developer its own, the main session a
   full suite it runs), because that run is testing code that is about to change, and the main session checks that
   run's processes are gone before calling it stopped (`docs/process/PR_GATES_RECOVERY.md`). A problem the review marks **pre-existing** is not this change's: it is named to
   the user, not applied. When a reviewer reports that the running suite will fail (a stale
   expectation, a missing fixture), that counts as a fix touching code in any round: the main session sends the
   findings at once, and the run is stopped as above. If no code fix follows, the main session reads that run's
   log, follows any failure there as after a failed run (`docs/process/PR_GATES_RECOVERY.md`), and otherwise asks the developer for the pre-check and, when it passes, the user for the full run (§3b). A run predicted to fail is not left to finish. **After fixes
   that change code, the developer compiles, proves each new or changed test, commits, and runs nothing more:** the
   pre-check and the full run wait until the review of those fixes passes, because that review can bring another
   round, and a run before it may be testing code that changes again.
4. **A second review covers only what the fixes changed**, and only when a fix changed code. For a wording-only fix,
   the developer re-reads it whole. **When the review of the last code fixes passes, the main session asks the
   developer for the pre-check and then, on the user's yes (step 2), runs the full suite itself or gives it to the developer** (§3b).
   **A review that returns no verdict has not run**, in any round: a reviewer stopped by an API error (a safeguard
   flag, an overload) leaves no findings and no pass. Report it to the user with the error text and the request
   ID, and ask before sending it again; another model is an option to offer, not a fix to assume. The gates that wait
   for a passing review keep waiting.
5. **One full run after the last code commit, behind a passing pre-check.** That run is the local gate, and it is
   never the first run of the change's tests: the pre-check passes on that same commit before it starts. The
   pre-check takes its tests from what the project declares and stays targeted: a change to something every test
   depends on (the lock file, root build or test config, CI config) is a reason to ask the user for the full run
   after reading that diff, never to start one, and a pre-check estimated or refused as too wide is narrowed by
   hand, as `docs/process/PR_GATES_PRECHECK.md` says. It adds every database test for a schema or migration change (same file). A run past its budget is stopped and reported, and its selection
   checked before anything re-runs; watching a run means checking its elapsed time against that budget. A gate
   run not yet measured has none: the main session reports it to the user once past about 20 minutes. Each run
   writes every test's result as it goes to its own new file in the task's run log folder, because a report written
   at a run's end can be lost with a stop. After a failed run,
   re-run the failing tests by name until they pass. If that took a code change, commit it and treat it as a fix
   round (steps 3–4): it is reviewed, then the pre-check, and the full run on a new yes from the user. A
   test that passes on re-run with nothing changed is not fixed (`docs/process/PR_GATES_RECOVERY.md`: a regression or an old flaky test).
   With no code change, commit any change, then the pre-check, and the full run on a new yes from the user. A test found flaky that fails
   them too ends the repeats: the gate stays open, and the main session reports it to the user, who decides
   (`docs/process/PR_GATES_RECOVERY.md`), as for a test that also fails on the commit before the change.
   **A run stopped before its end, for any reason but fixes, is read before anything re-runs** (`docs/process/PR_GATES_RECOVERY.md`): its
   log gives the failures, a failure printed at the moment of the stop is checked first, and with the names lost a
   narrower selection runs. It is never repeated unchanged to recover names, and that is never recommended; with no
   failure to follow, `docs/process/PR_GATES_RECOVERY.md` says when it runs again and when the user decides.
   The pre-check alone is the gate only when the user approved that for this PR's slow gate (`docs/process/PR_GATES_RECOVERY.md`), and
   then the PR waits for CI's full run to pass. Nobody commits to the branch while a gate run is going: a
   commit made during it means a new run, on a new yes, after that commit's review if it changes code. If the developer was stopped before its run finished —
   for any reason but fixes, which steps 3–5 handle — the main session reads that run's log and follows that rule —
   first checking that the stopped run's processes are gone, as `docs/process/PR_GATES_RECOVERY.md` says, since two runs can share one test
   database.
6. **A wording-only commit after that run** needs `pre-commit`'s checks, not another full run.
7. **A change to a web UI gets its look in a browser** (`browser-walkthrough`) before the pull request, when a
   browser tool is connected.
8. **Push and open or update the pull request when the user's request covered it; otherwise ask first**
   (`discussion_first`). Then tell the user. The merge waits for the user's own instruction (`pr_merge_approval`).

Optional review suggestions that would cost another full run may be left undone. When they are, the PR description
names them and says why.

## 1. To the developer — the task

```text
Task on branch <branch> (<new | already pushed, PR #<n> open>; push nothing, the main session pushes).
Problem: <what is wrong, in the user's words where they gave them>.
Do: <what to build>. Not in scope: <what to leave alone>.
Decided already: <decisions the user made; do not reopen them>.
Tools: <each MCP server in the developer's tools: list, and what to use it for>.
Shared with other work: <"nothing: no other run or agent is active" | "nothing shared with <the run or agent>" |
"<what you share> with <the run or agent>; take turns by <the lock or cap a gate rule of the project records>">.
Tests: <what each new test proves, and that it fails on a broken implementation>.
Proof runs: select the tests by name (the runner's filter), one mutation at a time if you like. The test command
with no selection is a full-suite run however fast it is, and needs the user's yes for that run (`AGENTS.md`):
never use it to prove a test.
Review: do not request a code review; the main session sends it when your final rebased commit lands, and that
review is the one `self_review` asks for on this code.
Rebase: after your last commit for the task, and before the pre-check, rebase onto the latest default branch once;
fix rounds do not rebase (`AGENTS.md`).
Gate: first the pre-check, targeted and never a whole package's tests folder: the test files the change adds or
edits, the test files of the modules it changes (and of their callers, where it changes a contract), and the listing
tests in any package that pin what the change registers, removes or renames, taken from what the project declares;
then search the test folders for the old value the change moves (a count, a name, a path) and add every test that
pins it: a change adding one setting moves a pinned count from 94 to 95, and a screen test pinning 94 counts too. <For a schema or
migration change: also every test that uses the database.> Estimate its time from the number of listed files and any
per-test durations an earlier run reported, before it starts; its budget is about <8, or the project's own figure if
lower> minutes; run it under a timeout set to that budget, and a run past it is stopped
and reported. Over budget, or refused by the selection tool as too wide: narrow it by hand as `docs/process/PR_GATES_PRECHECK.md` says
(drop the tests that only import or call what changed; keep the listing, pinning and database tests), report what was
left out, and never run the full suite in its place. Write each run's per-test results as it goes (the runner's per-test output redirected to a file, such as `pytest -v`) to its own new file in <run log folder, relative to the
worktree and ignored by git>; a report written at a run's end, and the runner's last-failed record, can be lost with a stop. A pre-check that collects no tests has not passed: check the
selection.
<"no full run: the user approved the pre-check as this PR's gate; CI runs the full suite" | "no full run from
you: report the pre-check and end your turn. A full run needs the user's yes for that run, and I ask for it once
your pre-check has passed">. Keep every command in the foreground, and never end your turn waiting on a run: nothing wakes you.
If a run fails,
re-run the failing tests by name until they pass (one that passes with nothing changed is not fixed, `AGENTS.md`).
A code change for it is a fix: commit, report and end your turn; it is reviewed first. Otherwise commit any change,
then the pre-check, and report; if a test found flaky fails it too, stop and report it. If a run is stopped for any reason
but fixes, read its log and follow `docs/process/PR_GATES_RECOVERY.md`; never repeat it unchanged to recover names.
Commit: <the project's commit convention>.
Report: the commits, the numbers of each run you made (passed, skipped and why, time) with the commit each run
covered and the log file it wrote and, for the pre-check, what it selected, how many tests it collected, and its
estimate and time against its budget, the default-branch commit you rebased onto, the files changed, what you verified and what you left
unverified.
```

Do not quote the full suite's measured time in the developer's brief: a fast full suite reads as an invitation to
run it. The time belongs in the ask to the user (step 2).

Before you start a developer, or a run of your own, while another run or agent is active (a full suite, a batch, a
second developer in its own worktree), name what the two would share: a test server, a test database, a build output,
the machine's cores. A separate worktree keeps their files apart, not these. The brief's *Shared with other work* line
is never deleted: with nothing else active, or nothing shared, it says so. Otherwise wait for the first to end, or
have the one you are starting take turns the way a gate rule the project records says (a lock, a worker cap), written
into its brief. A run already going cannot be changed, and an agent already running learns of it only from a
message, where your tool can send one, and follows it from its next run on. Where the project records no way to take
turns, ask the user before starting the second.

When the developer runs in the background, arm its watcher in the same turn. On Claude Code, use `CLAUDE.md`'s
*Starting a subagent that writes in the background*, and give it the task's run log folder, on every re-arm too. Always name that folder in the task brief: the
watcher counts a growing log as progress, and a folder the developer picks itself is one it cannot see. On a tool that cannot wake you, run the developer in the
foreground (`AGENTS.md` → *Delegating to an agent*).

On Claude Code, a notice that the developer *called SubagentHandback again … and was refused* means that report did
not reach you through the hand-back. Unless the developer has messaged you since, send it a message asking for the
report, as the notice says, and read the branch and the run log folder meanwhile. One measured cause: the developer
reported while a background command of its own was still running.

## 2. To the code-reviewer — round one

**Build the caller list before you send it** (0.199.1). The code-reviewer holds no `serena` (it has no read-only
mode), so without your list its caller search is text search or a graph built before the change. Where `serena` is
connected, run `find_referencing_symbols` yourself for each symbol whose signature or behaviour the change alters,
and paste the result. Without it, search the text yourself and write *"text search, a graph query not run"*; where
`graphify` is connected, add that the reviewer queries it and whether its graph predates the change.

```text
Review <commit range> on branch <branch> (PR #<n>). You have no shell: the diff is at <absolute path to the diff
file>.
Changed files: <list>.
What it is for: <the problem and the fix, two or three sentences>. Already accepted by the user, do not flag: <…>.
Look hard at: <the risky parts>.
Tools: <each MCP server in the code-reviewer's tools: list, and what to use it for>.
Callers of the changed symbols: <symbol: callers, from <the query, named> | text search, a graph query not run>.
Report findings by severity with file:line, and state what you examined.
```

## 3. To the developer — the findings

```text
The review of <range> <passed with N suggestions | found N findings>. Apply <all | these>: <numbered list, each with
file:line and the fix — this change's findings only; the review's pre-existing problems go to the user, not to you>.
<If any fix touches code, or the review says the running suite will fail:> If a run of yours is still
going, stop it now <and say so; I stop the full suite I am running>: the gate runs once, after the last fixes are
reviewed. <Wording-only fixes, with a run of yours still going:>
Commit nothing until your runs end.
Then: <if a run failed: re-run the failing tests by name until they pass, and report any that passed with nothing
changed (`AGENTS.md`);> <for code fixes: compile, prove each new or changed test — it passes as is and fails with
its fix reverted, each run selected by name, never the unselected test command; a test changed with no fix to revert (a stale expectation, a fixture, a rename) is run once, and
you say why — and commit as before, without rebasing. Do not run the pre-check or the full suite: these fixes
are reviewed first, and I will ask for the runs when that review passes. | for wording-only fixes: commit, then
pre-commit's checks.>
Report the commits, each test you proved and how, and whether you stopped the earlier run. Then end your turn:
the runs come in a later brief.
```

## 3b. To the developer — the runs, once the last review passes (or, for the full run, once the user has said yes)

```text
<The review of <range> passed. | Your pre-check passed on <commit>, and the user said yes to a full run on it.> Now <the
pre-check only: report it and end your turn; a full run needs the user's yes, and I ask once your pre-check has
passed | the full suite once: the user said yes to this run on <commit>; its budget is <measured gate time and half
again, about 8 minutes or less> | the pre-check alone, the gate the user approved for this PR | the pre-check only:
the full suite is not yet measured or its budget is over about 8 minutes, so I run it, on the user's yes, after
you report>, on <commit>. The pre-check stays targeted and within its budget, as
in the task. Shared with other work: <as in the task brief, checked again now>. Run each under a timeout set to its budget, with per-test durations and its per-test results written as it
goes to its own new file in <run log folder, as in the task>, and commit nothing while it runs. If a run fails,
re-run the failing tests by name until they pass (one that passes with nothing changed is not fixed, `AGENTS.md`).
A code change for it is a new fix round: commit, report and end your turn. Otherwise commit any change, run the
pre-check again, then report and end your turn: a second full run is a new ask to the user. If a test found flaky
fails again, stop and report it. If a run is stopped for any
reason but fixes, read its log and follow `docs/process/PR_GATES_RECOVERY.md`; never repeat it unchanged to recover names.
Report the pre-check and gate numbers with the commit each run covered, skipped tests and why, what the pre-check
selected (how many tests), each run's time against its budget and the log file it wrote.
```

## 4. To the code-reviewer — round two

```text
Second round, only what the fixes changed: <range>, diff at <absolute path>. Changed files: <list>.
The fixes answer: <numbered list of the round-one findings and the failed tests they answer>.
The developer's proof of each changed test: <how each was shown to fail without its fix, or, for one run once,
why>.
<If a fix changed a symbol's signature or behaviour:> Callers of the symbols a fix changed: <as in round one>.
Check each was done correctly, that each proof holds, and that nothing else changed.
Report findings by severity with file:line, and state what you examined.
```

## 5. To the user — the pull request is ready

The user's own actions go last, after the checks.

```text
PR #<n> is ready: <link>. Not merged; the merge waits for you.

Checks:
- Code review: <rounds and results; suggestions left undone and why>.
- Pre-check on that commit (<commit>, rebased onto <default branch>): <what it selected and why>, <N tests>,
  <passed>, <time> against a budget of <budget> minutes.
- Full test run after the last code commit (<commit>): <passed>, <skipped and why>, <time>, on <where> | <"full
  suite not run locally: you approved the pre-check above as this PR's gate; CI's full run passed on <commit>,
  <link>"> | <"full suite not run: you declined it on <date>"> | <"full suite not run: it runs in the next batch">.
- Full-suite time: <measured, from this PR's full run, or the open finding's figure for a scoped gate, or "not run">: <"under
  ~20 minutes" | "over ~20 minutes: <where the time goes>, raised on <date>">.
- pre-commit checks after the wording-only commit <commit>: <result, or "none made">.
- Browser look: <what was walked through, or "not a web UI change" | "no browser tool connected">.
- CI: <result, or why none ran — for a scoped gate, its full run must have passed (above) before this report>.

Next steps:
- <Anything the user must check by hand, if any.>
- Merge: say "merge" when you want it landed. (Where the git guard is on, it denies agents `gh pr merge`: write
  "Merge: yours to do, in the web UI or with `gh pr merge`" instead.)
```
