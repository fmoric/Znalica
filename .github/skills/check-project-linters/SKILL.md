---
name: check-project-linters
description: Use when changed code needs the project's configured linters or analyzers run, or their errors and warnings fixed — eslint, ruff, mypy, AL analyzers — config unchanged.
---

# Check project-configured linters

**The target project's configuration wins.** Discover and run checks it already uses; do not
install a tool, turn on an analyzer, invent an AL cop list, or change a ruleset. This skill
owns discovery, execution and fixing findings in changed code — not the project's coding
standards or build/test procedure.

**Two modes — the caller decides which:**

| Mode | Who uses it | What it does |
|---|---|---|
| **check** | `pre-commit`, `al-verify`, a review, or a user asking "does this pass lint?" | find, run, report; change no file |
| **fix** | the `developer` agent after writing code, or a user asking to fix lint findings | check, then the fix loop below, then report |

In check mode, stop after the report. Fixing during a commit check would change files
after they were staged.

## Find what applies to this change

1. Identify the changed code files, their app/package roots, and the relevant project
   instructions. A monorepo's root config may not govern a nested app. Name the paths you
   examined; for a docs-only change with no applicable lint, report **not applicable**.
2. Find **project-owned evidence** of an actual check: a documented command or CI job,
   package script and its lockfile, Python project/test-tool configuration, pre-commit config,
   or AL analyzer settings and the ruleset they reference. Examples are discovery clues,
   **not** commands to impose on every project. A `ruleset.json` or config filename alone
   does not prove it is enabled, that its tool is installed, or that it covers these files.
   For example, inspect `package.json` scripts with `eslint.config.*`/`.eslintrc.*` for
   TypeScript, or `pyproject.toml` `[tool.ruff]`, `ruff.toml`, and `[flake8]` in
   `tox.ini`/`setup.cfg` for Python; an individual file is not a runnable command.
3. Match each configured check to the exact invocation and working directory used by
   that project. For the check step, prefer the project's existing non-fixing check over hand-assembling flags.
   If the only known entrypoint rewrites files, installs packages, publishes to a BC
   environment, or launches a remote pipeline, **do not run it as lint**; report what is
   configured and why it was not run. No guessed fallback counts as equivalent.

## Execute without changing the project's setup

Check that the required executable and dependencies are present *before* the invocation;
do not let a package runner fetch missing tools silently. Run the non-fixing command on
the intended files/package (or on its project-defined scope). Capture its exit status,
error and warning diagnostics, and evidence it actually examined relevant code. Never
expose credential values from settings or command output in a report.

For **AL / Business Central extensions** (`al-bc`), **read
[AL analyzer activation and invocation](references/al-bc.md) before selecting
compiler options or claiming analyzers ran**. Its detail loads only for AL;
bare build success does not prove a CodeCop or custom analyzer ran.

**C/AL / NAV** (`cal-nav`) is a different toolchain: do not call the AL MCP
compiler or assume AL CodeCop analyzes its C/SIDE object files. Look only for
checks the NAV project actually configures; build/test belongs to the
`nav-container` skill, present only in a C/AL / NAV delivery.

For other languages, use the project's actual script/CI command (for example a
`package.json` lint script or a command declared in Python project configuration).
Do not assume every TypeScript project runs ESLint, every Python project runs Ruff,
or that a formatter's default command is read-only.

## Fix findings in changed code (fix mode only)

```
check ──► project's own fixer on the changed files ──► fix the rest by hand ──► check again ──► report
```

1. **Scope is the change.** Fix findings in the files and lines this task changed. A
   pre-existing finding elsewhere is reported, not fixed — unless the user asked for it.
   Without a baseline run, tell this task's findings from older ones by the diff
   (`git diff`), and say that is how you told them apart.
2. **The project's own fixer first** — only one the project declares: a `lint:fix` or
   `format` script, a pre-commit hook, a CI fix step, or the fix flag of the linter the
   project already runs (`eslint --fix`, `ruff check --fix`, `ruff format`, `dotnet format`)
   **with the project's own config**, and within the limits of discovery step 3 — never one
   that installs packages, publishes or launches a remote pipeline. Run it on the changed
   files only. Then read `git diff`: revert any change it made outside the changed files,
   and say so; on untouched lines of a changed file, keep formatting and give its count in
   the report, and revert any other change, so older findings stay reported, not fixed. A fixer that cannot be limited to the changed files is
   skipped — fix by hand instead.
3. **Fix the rest by hand**, one diagnostic at a time, with the smallest change that makes
   the rule pass and keeps the behavior. Re-read the rule's documentation when its message
   is unclear; do not guess what it wants. For AL, read *Fixing AL diagnostics* in
   [the AL reference](references/al-bc.md) first.
4. **Check again** with the same command and settings as the first run — a check with
   different settings proves nothing about the first one. Stop after **three** rounds;
   report what remains instead of looping.
5. **Run the tests again** when a fix changed more than formatting — except on a fix round
   after review, where tests wait for the run after the last review passes.

**Never, in fix mode:**

- edit linter or analyzer configuration, a ruleset, `.editorconfig`, or a build property
  that turns a check off or a warning down;
- add a suppression — `// eslint-disable…`, `# noqa`, `# type: ignore`, `#pragma warning
  disable`, `[SuppressMessage]`, an AL `#pragma warning disable AA0xxx` — unless the user
  asked for that specific one; a finding you think is wrong is reported with your reason;
- install or update a tool, or let a package runner fetch one;
- change behavior to satisfy a rule: if the only fix changes what the code does, report it
  and leave it.

## Report each check, including missing evidence

| Check | Config and command source | Files/package examined | Result | Found | Fixed (fixer / by hand) | Remaining, with reason |
|---|---|---|---|---|---|---|
| project-defined analyzer | path and verified invocation | count and scope | pass / fail / not run / n/a | errors / warnings, or unknown | counts, or `check mode` | counts and why each was left |

**Pass** requires an executed check and evidence it covered the stated scope. A
configured check that could not run is **not run**, not a pass; an absent config is
**not configured in the paths inspected**, not "all clear". If user-level settings
or CI variables were not available, name them as unchecked. Report warnings even if
the command exits successfully; follow the project's warning policy before marking
the result passing. Only classify warnings as new or pre-existing when you checked
a baseline; leave unrelated pre-existing fixes out of scope. Name any missing
coverage or command that needs the project owner's input. In fix mode, **Result** is
the final check's, and a fixed count without that final check is **not run**.
