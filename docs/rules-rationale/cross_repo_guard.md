# `cross_repo_guard` — rationale

Moved verbatim out of the `cross_repo_guard` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Why this cannot be inferred

A path into another repository appears in two lexically identical patterns:

| Pattern | Example | Target |
|---------|---------|--------|
| **Target** | "update `<other-repo>/config.md`" | the other repo |
| **Guide** | "read `<other-repo>/config.md` and apply it here" | the current repo |

Both contain the same cross-repo path. Only the user knows which is intended. Do not guess.

## Moved at 0.105.0 tightening

From the rule body (verbatim):

- If it isn't, there is no root to point outside of and this rule is inert.
- Do not proceed until the user names the target repo explicitly. "Yes" or "proceed" does not count —
  it is ambiguous.
- **Resolving is reading a path, not starting the work** — so it does not conflict with the "before any tool
  calls" above, which is about not beginning the task. You already know the working directory, and `..`
  segments and drive prefixes resolve on sight; almost every case is settled without running anything. Where a
  case genuinely is not, `git rev-parse --show-toplevel` is a read-only call and taking it is correct — **stop
  and ask on the answer, not before you have one.** Guessing to avoid a read-only call is the failure this
  rule is about.
- An absolute path into another repo is the obvious form and not the only one. **A relative path containing
  `..` can leave the root just as easily**, and on a machine where repositories sit side by side under one
  parent — the normal layout — `../other-repo/config.md` is the *most* natural way to write it.
- Explicit lookup-only path flags passed to tools (not ambiguous about target).
- **Never decide this by pattern-matching the string.** "Starts with a drive letter or `/`" and "contains `..`"
  are both proxies for the real question, and each is wrong in one direction: the first misses the relative
  escape, the second fires on paths that never leave. Resolve the path — that is the property this guard is
  actually about, and it is one operation.
