# Znalica — Agent Instructions

Znalica is an educational game with a learning program for Croatian primary school pupils, grades 1–8. It is written in C# / Unity. These instructions apply to
every agent working in it.

## Commands

- Build: `Unity.exe -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod BuildScript.Build`
- Test: `Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode -testResults TestResults.xml`

## Working principles (always on)

- **Verify before "done".** Do not report success without proof — the build ran, the tests passed, the
  file is on disk. If it didn't run clean, say so and give the cause. A change to a web UI also gets a look
  in a browser when a browser tool is connected (the `browser-walkthrough` skill).
- **Small commits, pull requests sized to their cost.** Keep each commit small, coherent and verifiable.
  A pull request carries a fixed cost — design grade, review, a full test run, a merge — so group small,
  related steps into one PR, and keep a large or risky step in a PR of its own.
- **Stop and ask** when blocked, or when scope grows beyond the original task (`task_assessment`). Ask only what
  you cannot read for yourself (`answer_before_you_ask`).
- **Read the system map first.** If `docs/system-map.md` (or a map named here) exists, read it before
  searching the code. A change that moves structure refreshes it (`/system-map`, or hand it to doc-writer).

## Standards

- Match the surrounding code — naming, structure, error handling.
- Follow C# / Unity conventions and type-safety. No secrets in source. Validate external input.
- Decide whether a value is a constant **at all** before naming it one — a port or a service name named as
  a constant is still hardcoded (see the `look_before_you_write` rule). Where it genuinely is a constant:
  prefer enums/constants over magic strings; externalize user-facing text.
- **Change only what the task needs.** Every changed line traces to the request; no speculative features or
  abstractions; name an unrelated problem rather than fixing it. A bug fix finds every caller and fixes the
  shared cause once.

## Agents in this packet

- **architect** — translates a functional (end-user) spec into a technical spec: architecture, component
  boundaries, interfaces, and work sequence, grounded in the real codebase. Upstream of the developer;
  authors the design, writes no implementation code, and never delegates.
- **design-reviewer** — grades the architect's technical spec A–D **before** the developer builds, on two
  lenses: coverage against the functional spec it came from, and fit against the real codebase. Leaf node:
  grades, never redesigns. Fixes route back to the architect. **It has no shell — hand it the spec paths.**
  Given no functional spec it still grades code-fit, and says coverage went unchecked.
- **developer** — implements from specs and writes tests. Delegates a finished change to the
  **code-reviewer**; never delegates the implementation itself. (Two agents here are reviewers — the other,
  `design-reviewer`, runs before the code exists.)
- **code-reviewer** — reviews the diff **it is given** (no shell — hand it one) and reports findings by
  severity. Leaf node: reports, never fixes,
  never delegates. Fixes route back to the developer.
- **reader** — finds and reads code and log files on a cheap model and returns `path:line` with the few lines that
  matter. Leaf node: reads, never edits, never delegates, no shell. **Give it the question and the folders to
  search, or a log file's full path.**
- **translator** — fills translations (translation files, or captions kept in source) on a cheaper model, keeping
  source text and placeholders exact. Leaf node: edits translations only, never code (in C/AL, only the caption text
  in object files). **Give it the language and the files** (C/AL: and whether the object files are UTF-8).
- **workflow** — GitHub Actions / CI-CD specialist: designs, debugs, and hardens workflows. Leaf
  specialist — does its own analysis, implementation, and verification; never delegates.
- **doc-writer** — writes runbooks, troubleshooting (symptom → cause → fix) and "how it works" pages for
  whoever runs, supports or maintains the system; keeps the system map and roadmap current. Checks every
  claim against the code; never delegates.

## Delegating to an agent

A subagent inherits your **tool list, not your routing decision** — it starts with none of your context and
no idea which of its tools you meant it to use. So whenever the task is to **find, read or judge code** — a
search, an investigation, a mapping job, a review — **name the MCP servers in that agent's `tools:` list and
say what each is for**. Left unnamed, they tend to go unused: text search is the zero-friction path, and the
agent takes it. The roster above names the input each reviewer needs; without it, a reviewer returns a verdict
over nothing.

**Exception — when the delegation is a test of whether the agent's own instructions work, name no tools.**
Telling it what to use hands it the answer, and the run then measures your prompt rather than the agent.
Pass the task as written, and record that the prompt named no tools. **This suspends the tool-naming above
and nothing else** — the diff, the file list and the task itself are still yours to hand over, and a review
starved of them is a verdict over nothing whether or not it was a test.

**When a subagent that writes goes quiet, check the work, not the agent.** *Writes* means the task: a
write-capable agent given a task that changes no files and starts no test run needs no watcher. You will not notice silence, so arm the
check as you start it: if it runs in the background, start a watcher that wakes you once new commits on its branch have stopped for 2 minutes,
after 10 minutes with no commit, no file change and no growth in its run log, and at its budget (an hour if it has none) whatever it is doing.
Where your tool cannot wake you, run the agent in the foreground. Before saying it is "still working", or waiting on its report, look at what it
produces — `git log` on
its branch, the files it writes. If the final commit or file the task asked for is there, stop waiting: hand the
output to its reviewer, or run the next gate yourself where it has none. A suite run the agent starts after that
commit is still its own: take it over only when the agent ended without reporting it, and read its log before
anything starts again (`docs/process/PR_GATES_RECOVERY.md` → *A run stopped before its end*). When its run goes over its budget, stop the agent where your
tool can, check the run's processes, and report it, as *Gates on each pull request* says. **Watching a test run means comparing its elapsed time with
its budget, not only checking that it is alive**; what happens past it is in *Gates on each pull request*. A grade
stays the reviewer's. A writing agent's final report is a summary, not a gate; a
reviewer writes nothing, so its report is its output — wait for it, or run it again.

**Ask for a condensed result, and on long work for the file first.** Whatever a subagent returns lands in your
context, so ask for findings with paths and line numbers, not pasted content. For a long job given to an agent
that writes, have it create its output file first and fill it as it goes: its context limit can be smaller than
yours, and an agent that runs out before writing anything loses all of its work.

**Keep your own context small: every line in it is paid for again on each later turn.** Send finding and reading
code to `reader` — a whole file, several files, a search across folders — not to a general-purpose agent, which
runs on your own model. **Wherever a gate says to read a run's log, give `reader` the log's full path:** it lists
every failure, and the decision stays yours. Read yourself only the lines you are about to change, and open the lines a reader cites
before you edit on them. Code changes go to `developer`; translating texts (a translation file, or captions kept in
source) goes to `translator`.

**Never run on another project's environment** (its interpreter, venv, `node_modules` or tools; this repository's
own worktrees are not another project). A refused or failed install or setup is reported, not worked around. The
user, or a subagent's caller, may allow it only read-only (nothing installed, no cache or bytecode written), and
only after that project's session, or the user, was told what will run.

## Gates on each pull request

Keep every gate; repeat one only for a reason. **A gate rule this project records for itself** — in a section of
this file it added, or a file such a section names — **wins over the bullets below and over the
`hand-off-a-change` briefs**, for what it covers (when the full run is due, what a fix round re-runs); it replaces the ask below only as that bullet says. **Open it
before a pre-check or gate run and take the budget and log command from it**: a time in agent memory or a note is a
pointer to check there, never the budget. Where `docs/process/INTEGRATION_BRANCH_GATES.md` exists, it is such a rule.

- **Code review: one round, then only what the fixes changed.** The developer fixes the findings. A fix
  that changes code — production code, a test, a fixture, config — is re-reviewed, as `self_review`
  requires; a test can pass without the fix it claims to guard. A fix that changes no code (wording,
  comments, docs) is met by the developer re-reading it whole.
- **No agent starts a full-suite run on its own decision.** That is the test command with no selection, or any
  selection estimated over the pre-check's budget, run locally (not CI's). It needs the
  user's yes **for that run**. These are not that yes: a rule saying a run is due or required, or that a pre-check
  is the full suite; a standing "start without asking"; an autopilot answer; an earlier yes. Only a gate rule saying
  in those words that a run starts without asking replaces the ask. Ask after the pre-check passes: commit, reason,
  measured time, what the pre-check covered. A re-run is a new ask. On a no, report the gate as scoped and do not ask again for that commit.
- **One full suite run after the last code commit gates the PR**, and reports per-test durations. Nobody
  commits to the branch while it runs; a commit made during it means a new run, after that commit's review if it changes code. If review fixes will
  touch code, stop a run still going — it is testing code about to change. A wording-only commit after the
  gate run needs `pre-commit`'s checks, not another full run.
  **After fixes that change code — a review's, or a failed
  run's — the pre-check and the gate run wait until the review of those fixes passes**, since that review can bring
  another round: the developer compiles, proves each new or changed test by name and commits; it runs no pre-check
  and no full suite. Tests it runs by name are its own call, not gates.
- **Rebase onto the latest default branch once: after the last commit the task asks for, before the pre-check and
  the review.** The review, the pre-check and the gate run then all cover the rebased commits. Fix rounds do not rebase. The default branch moving later does not restart a run
  by itself. If the PR can no longer merge, a new rebase is the main session's call: it asks the developer for one,
  and the conflicts it resolved are then handled as a fix round's changes (review of only those, the pre-check and
  the gate run). A branch already pushed then needs a force push: the main session, which pushes, uses
  `--force-with-lease`, never a bare `--force`, and the git guard asks the user first.
- **A fast, targeted pre-check comes before each gate run** — never a whole package's tests folder, and never the
  full suite in its place. Its budget is about 8 minutes, or the project's gate rule's figure if lower; one
  estimated over it is narrowed by hand, not run, and one that collects no tests has not passed. **Every pre-check
  and gate run writes each test's result to a log file as it goes, each run its own new file**, in a git-ignored
  folder the main session chooses and names in the brief. **A full-suite run that is not yet measured or whose
  budget is over about 8 minutes is the main session's**, on the user's yes; the developer runs only the pre-check.
  Only the user raises a budget. **Before you choose that folder, or select, estimate, narrow or set the budget of
  a run — and when a reviewer reports that a running suite will fail — open `docs/process/PR_GATES_PRECHECK.md` and
  follow it.**
- **After a failed, stopped or over-budget run, or a gate run over about 20 minutes — before anything re-runs or
  you stop or report a run — open `docs/process/PR_GATES_RECOVERY.md` and follow it.** In short: re-run only what
  failed, by name; a test that then passes unchanged is compared with the commit before the change, and one flaky
  there too is reported, never fixed unasked; the same failure twice is a fix loop (`diagnose-bug`); stop only your
  own run's processes, check they are gone, and read its log first; never repeat a run unchanged; a slow gate is
  its own finding, and only the user may approve the pre-check as one PR's local gate.
- **A gate report names the commit it ran on and what it covered.** A scoped run says so, in the form
  *"pre-check gate: <what ran>, N tests, on <commit>; full suite not run"*, never *"tests pass"*.
- **Use the `hand-off-a-change` skill** whenever you hand a change to the developer or the code-reviewer,
  route review findings back, or tell the user a PR is ready: it has the gate order and a block for each.
- **Overlap is allowed.** While a PR waits for merge, the next PR's spec work may start — in a separate
  worktree, so the branches don't collide. A worktree beside the project is outside this repository's
  root, so `cross_repo_guard` applies: before delegating spec work there, the main session gets the user's
  answer naming that worktree and passes it in the task, and each git write there needs its own
  confirmation.

## Rules

Topic- and path-scoped rules live alongside these instructions (`.claude/rules/`, plus
`.github/instructions/` copies if delivered). They extend — and where stricter, override — the standards above. Why a rule exists, where it has a
rationale file: `docs/rules-rationale/<rule>.md`. A skill named in these
instructions or an agent's lives at `.claude/skills/<name>/SKILL.md` (Copilot: `.github/skills/<name>/SKILL.md`):
an agent with no skill tool reads it there.
