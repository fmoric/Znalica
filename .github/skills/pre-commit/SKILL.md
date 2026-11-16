---
name: pre-commit
description: Use before any commit, or when asked whether staged changes are ready to commit — even a one-line change. Checks secrets, debug leftovers, the build, tests, linters and docs, and reports pass / fail / not run with the scope each check covered.
---

# Pre-Commit Validation

Run these quality checks on the **staged** changes before committing. Obtain the staged diff yourself
(`git diff --cached`) and inspect it.

**This skill is the one definition of the commit checks** — the always-on `security` rule points here and
lists none. It covers what can be **run or searched**. Whether the code is *correct* — a value used before
it is set, a range bound off by one, a failure swallowed — is code review's (the `review` skill, the
`code-reviewer` agent), and "Ready to commit" below does not claim it. **If this change has had no review
and none is planned, say so beside the verdict.**

## 1. Scan the staged diff

- **Staged files** — `git diff --cached --name-only`. **Count them.** If the count is zero, nothing is
  staged: stage the files this commit is meant to hold (by path, never `git add -A`) and start again, or, if you
  do not know which those are, stop and say so. Every check below would otherwise report a clean pass over an empty scope.
- **Name the languages in that file list, and write down the pattern set you will use — before you search.**
  The list below is a starting point, not the list. Extend it for what is actually in the diff (AL:
  `Message(`, `Error('TEST`; C#: `Console.WriteLine`, `Debugger.`; Python: `breakpoint()`, `pdb`; Go:
  `fmt.Println`).
- **Debug artifacts** — search the staged diff for that pattern set: `console.log`, `print(`, `debugger`,
  `TODO`, `FIXME`, `HACK`, plus whatever you added above.
- **Secrets** — search for `password`, `secret`, `api key`, `token = …`, `private key`, or any credential
  literal — and for provider key shapes, which match with far fewer false alarms: `sk-ant-`, `sk-`, `ghp_`,
  `gho_`, `github_pat_`, `AKIA`, `AIza`, `xoxb-`/`xoxp-`, `-----BEGIN … PRIVATE KEY-----`. A placeholder
  (`<your-key>`, `changeme`, `xxx`, `${VAR}`) is not a hit — say which you set aside. Any real hit is a
  blocker until cleared.
- **Weakened checks** — a staged change that loosens the project's own linter or analyzer configuration (a
  rule disabled, a severity lowered, a suppression or `#pragma warning disable` added) is a finding: report it
  in the *Project linters/analyzers* row below, never as a silent pass.
- **Tests made easier** — a staged change that makes the suite easier to pass without making the code better is
  a finding too: a test skipped or focused (`skip`, `xfail`, `.only`, `[Ignore]`, a `[Test]` attribute removed),
  a test file deleted, an assertion removed or loosened. Report it in the *Tests passing* row, with file and line.

**Record the scope in one line before moving on** — the file count, the languages, and the pattern set:

> *"Adapted to AL + TypeScript: searched 7 staged files for 11 patterns (`console.log`, `debugger`,
> `Message(`, `Error('TEST`, …)."*

**Zero hits from a pattern list that was never adapted is byte-identical to a clean diff** — and the output
of this skill is a green light to commit. Report what you searched, not only what you found.

## 1a. Build and type-check

For staged code, run the project's build and read its output — the command under `AGENTS.md` →
*Commands*, or the procedure in its *Build & test* section where the project has one. This is the step
behind "imports and references resolve" and "types hold": unresolved imports, missing symbols and type
errors are what a build reports, and no other step in this skill looks for them.

- **Where type-checking is a separate command** (a typed layer over an interpreted language), find the
  project's own non-fixing one the way `check-project-linters` finds a linter — project-owned evidence,
  never a guessed command — and run it too. If §2a's discovery turns up the same command, run it once
  and report it in this row.
- **Record what ran and the result its procedure tells you to read** — for a command-line build, the
  command, its exit status and its error count; where `AGENTS.md` has a *Build & test* section, the tool
  call and whatever that section says to count. Reuse a build from earlier in this session only if it
  covered the same staged code; otherwise build again — an earlier build of different code does not count.
- **§2a's working-tree caveat applies here unchanged:** a build reads the working tree, not Git's staged
  snapshot. If the two differ in files the build reads, report **run on the working tree; staged result
  unverified**.
- **Nothing builds or type-checks the staged files** (a docs-only diff, a language the project neither
  compiles nor type-checks) → **n/a**, naming where you looked. A build command that exists and could not
  run is **not run**, never n/a.

## 2. Tests

Identify the staged source files and check that the tests covering them exist and pass. **Run the tests
that cover the staged code** — the whole suite only where the project cannot run a subset, and then on the user's yes for that run (`AGENTS.md`) — with the
project's test command, or the test procedure in `AGENTS.md`'s *Build & test* section where it has one (or
state why you can't). §2a's working-tree caveat applies to the test run too.

- **A run from earlier in this session counts** only if it covered the same staged code; otherwise run
  again. Record what ran and what it covered.
- **This is not the full-suite gate.** Where `AGENTS.md` says which suite run gates a pull request, that
  run is its; this step asks only for the tests the staged code needs.
- **`n/a` only when the diff stages no source code.** "No test covers it" is a finding, not a reason the
  check does not apply: staged code that no test covers is ❌, naming the files.
- **Code staged and the project has no test command or procedure** → **not run**, saying so. The user
  decides.

## 2a. Configured linters

For staged code, follow `check-project-linters` in **check mode** — report, fix nothing — to find the
project's own non-fixing checks. Reuse an existing run from this session **only if it covered the
same staged code with the same config**;
otherwise run the applicable checks and report the diagnostics. A configured analyzer that was not
invoked is **not run**, even when the build completed or diagnostics are empty. Do not install a
missing linter or run an auto-fixing hook as a harmless check.
**A fresh command reads the working tree, not Git's staged snapshot.** Before marking staged
lint ✅, verify the index and working-tree contents match for *every file the check reads*:
the staged source, its linter command/config, referenced rulesets, and other in-scope files.
Check unstaged differences and untracked files in that scope; if they can affect the result,
do not treat a working-tree pass as a staged pass. Report the check as **run on the working
tree; staged result unverified**, and recommend re-staging or an approved isolated staged
snapshot rather than discarding someone's unstaged edits. Apply this same freshness test
to a reused earlier run.
If none is configured for the staged code in the paths examined, mark **n/a** with those paths,
not a green lint result; a configured but unavailable check remains **not run**.

## 3. Documentation

- Changed public APIs documented?
- Complex new logic commented?
- Breaking changes noted?

## 4. Summary

Report a status for each row **and what stands behind it**. Four statuses, and the last two are not the
same thing:

- **✅ / ❌** — the check ran on this diff.
- **`n/a`** — the check has nothing to act on in this diff: a docs-only change has no tests to run and
  nothing to build. Ordinary, and it **must not block** — a gate that fires on every docs commit is a gate
  that gets ignored, and being ignored is how the unearned ✅ comes back.
- **`not run`** — the check applies and did not run: a build or test command exists and failed to start,
  an analyzer is configured but missing, or a build, test or lint **passed** only on a working tree that
  differs from the staged snapshot — write *run on the working tree; staged result unverified* in the
  third column. A failure is ❌ whichever tree it read. **A check you could not perform is not a check
  that passed.**

| Check | Status | What ran / what it covered |
|-------|--------|----------------------------|
| No secrets / credentials | ✅ / ❌ / not run | §1's scope line: file count, pattern set |
| No debug artifacts | ✅ / ❌ / not run | §1's scope line: file count, pattern set |
| Build / type-check clean | ✅ / ❌ / not run / n/a | what ran and its result (§1a) — or where you looked |
| Tests passing | ✅ / ❌ / not run / n/a | what ran, what it covered, its result — or why none apply |
| Project linters/analyzers | ✅ / ❌ / not run / n/a | which ran and their warnings — or the paths inspected |
| Documentation updated | ✅ / ❌ / not run / n/a | what you checked — or why nothing needed documenting |

**A ✅ or an `n/a` with nothing in the third column is `not run`.** The column is what separates a check
that passed, or genuinely had nothing to do, from a row that was filled in.

- **Any ❌** — list the specific issues and suggest fixes; do not recommend committing.
- **Any `not run`** — name the check and what stopped it. Do not report "Ready to commit". Report what is
  still unknown and let the user decide; a subagent reports it to its caller.
- **Every row ✅ or `n/a`, each with its third column filled, and §1 recorded the file count, languages
  and pattern set** — report "Ready to commit" and suggest a commit message; say beside it if no review
  has run and none is planned. Without that scope line the ✅s are unsupported; go back and produce it.
