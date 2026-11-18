---
name: review
description: Use for a structured review of staged or recent changes, findings by severity. A quick in-context pass; for an isolated review, the code-reviewer agent.
---

# Code Review

Review the changes the user specified — a branch, a commit range, or a path. With no argument, review the
staged and recent changes.

## Get the diff

Obtain the diff yourself and read it in full before reviewing:

Run the form for the scope you were given, first with `--stat`, then without:

- **A branch** — `git diff <default>...<branch>` (three dots: what the branch changed since it left the default
  branch). `<default>` is `origin/main` or `origin/master`, whichever `git branch -r` lists; say which you used.
  A plain `git diff <branch>` is the wrong command: on that branch it is empty, and from another it is the change
  reversed plus everything else that differs.
- **A range** (`a..b`, `a...b`) — `git diff <range>`, as given.
- **A path** — `git diff HEAD -- <path>` for uncommitted changes to it. If that is empty, ask which commits to
  review it across, or use the branch form with `-- <path>` added.
- **No scope** — `git diff HEAD` (staged + unstaged). If that is empty, review the most recent commit (`git show`).

**An empty diff is never a review.** Whatever the scope, if the command prints nothing, say so with the exact
command, and stop or ask: do not grade it, and do not hand it to a reviewer.

## Review standard

Apply the same standard as the packet's **code-reviewer** agent. **First, a spec pass of its own** — is
anything asked for missing, anything beyond the ask (judged against the spec's *Out of scope* list where it
has one), anything doing the wrong thing; where the spec is
silent, say what you could not judge. Then check each changed file across:

- **Correctness** — does it do what the spec says? off-by-one, null/empty, boundary conditions, a value
  used before it is set.
- **Security** — no secrets in source; external input validated; risky operations guarded; safe/parameterized queries.
  When the change touches input, auth, secrets, files, URLs or queries — in AL, permissions, `IsolatedStorage`,
  events or outbound HTTP — judge each candidate with `references/security-lens.md` beside this file: source →
  sink → missing guard, then a false-positive test.
- **Error handling** — failures surfaced, not swallowed; correct inclusive/exclusive comparisons, on
  date and time ranges too.
- **Tests** — happy path, error, and edge cases covered; the tests actually assert the behavior. A test that
  only restates the implementation, or mocks the thing it claims to test, proves nothing.
  Also a finding: a bound where the exact value is knowable; an expected value computed the same way the code
  computes it; a mock that does not check the real signature where the language's mocking library can; and an
  expected value changed with no reason in a comment beside it. A snapshot or golden-file update is reviewed as a
  change in behavior, not as a copied value.
- **Scale** — a query or call per item in a loop, unbounded growth, work repeated per request; name the
  load you assumed.
- **Duplication** — does this add a second copy of something the repo, or this same change, already has?
  Invisible in a diff, obvious in the file — so check the file, not only the diff.
- **Seams** — where the change calls code that sits behind an abstraction (an interface, a strategy, a
  dispatch table, an enum that picks an implementation), does it call *through* it? A new direct reference to
  one concrete implementation, from outside that implementation, is a finding **even when the surrounding
  code does the same** — and the existing bypass is a second finding. Not a finding: the implementation's own
  tests, and the one place that picks the implementation.
- **Fit** — matches surrounding code and conventions; no dead code; user-facing text externalized.
  **Matching an existing pattern is not evidence the pattern is right**: where the surrounding code bypasses
  its own abstraction, say so rather than grading the copy as fit.

This is the quick, in-context pass. For a thorough, isolated review, delegate to the **code-reviewer**
agent — it owns this standard in full (and keeping the two aligned is why the dimensions are named here,
not re-specified).

**When you delegate, pass the diff you already obtained above, and the list of changed files.** The agent
has no shell and cannot fetch either one for itself; paste the diff for a small change, or write it to a file
and give it the path. **You are the door that already has them** — handing off without them turns a thorough
review into a verdict over nothing.

**Name its MCP servers too** — its `tools:` list is in its agent definition, and a server you don't name
tends not to get used. That is the granted half of the same handoff; `AGENTS.md` → *Delegating to an agent*
has it in full, including the one case where you deliberately name no tools — which never suspends the diff
hand-off above.

## Output

For each finding:

- **Severity** — 🔴 Critical (must fix) · 🟡 Warning (should fix) · 🔵 Suggestion (consider)
- **File:line** — location
- **Issue** — one sentence naming the defect
- **Fix** — a concrete resolution (describe it; don't apply it here)

Report nothing you can't point to in the diff. If you find nothing at a severity, say so. End with a
one-line verdict: **pass** / **needs-fixes**.
