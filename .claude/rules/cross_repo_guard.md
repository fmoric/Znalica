---
paths:
  - "**"
---

# Cross-Repo Task Guard

## Rule

**Applies only when this project is a git repository.**

When a task contains a file path outside the current session's git root, **STOP immediately** — before
planning, exploration, or any tool calls — and ask which repository the work should land in:

> "This task references a path in `[other-repo]`. Before I start:
> - Should changes land **in `[other-repo]`**?
> - Or should I **read that path as a guide** and apply the work to the current repo (`[current-repo]`)?
>
> I'll wait for an explicit answer before proceeding."

Proceed only once the user names the target repo explicitly; "yes" or "proceed" does not count.

## What this covers

**The trigger is where the path LANDS, not how it is written.** Resolve it against the working directory;
fires when it lands outside the git root — another repo on the same drive, or a `..` escape such as
`../other-repo/config.md`. Resolving is reading, not starting the work. Where a path does not resolve on
sight, run the read-only `git rev-parse --show-toplevel` — **never guess to skip it**; stop and ask on
its answer.

Does NOT fire for:

- Paths that **resolve** inside the git root — including `..` that walks out and back in
  (`src/../docs/x.md`).
- Explicit lookup-only path flags passed to tools.

**Never decide this by pattern-matching the string** ("starts with a drive letter or `/`", "contains `..`").
Resolve the path.

## Git operations outside the current repo are always blocked without confirmation

Even after that answer, any `git add`, `git commit`, `git checkout -b`, or `git push` outside the session's
git root needs a second explicit confirmation:

> "I'm about to commit to `[other-repo]` on branch `[branch]`. Confirm?"

**A linked worktree of this same repository is not another repository** (`git rev-parse --git-common-dir` there
names this one): the `shutdown` skill's temporary worktree does not fire this.
