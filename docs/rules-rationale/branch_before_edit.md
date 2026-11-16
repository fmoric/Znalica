# `branch_before_edit` — rationale

Moved verbatim out of the `branch_before_edit` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Why the edit boundary

A session's "branch first" intent is set at the start, when no work is planned yet. When a task begins
mid-conversation (the user says "ok, add it" after a question), that intent is gone and nothing re-checks.
The first edit is the only reliable gate — so the check fires there, on every response, not once at start.

## Moved at 0.105.0 tightening

From the rule body (verbatim):

- This is a hard gate at the **write boundary**, not an initialization-time suggestion.
- Never check the call against a list of tool names: a list is correct only until a new tool appears, and it
  fails **silently** — nothing announces that the gate stopped covering something.
- If a code-navigation server is enabled, note that such servers commonly ship file-*editing* tools alongside
  their read-only ones (symbol replace/insert, memory writes), and those are subject to this gate like any
  other write.

## The `shutdown` exemption (0.190.0)

A session record is not a code change (the maintainer, 2026-11-12). Before 0.190.0 `/shutdown` wrote its record into
whatever branch the session had checked out, so a record written on a feature branch reached the default branch only
when that branch merged, or never. 0.190.0 has it write in a temporary worktree and land on the default branch.
Since 0.190.1 that worktree is on its own `docs/session-…` branch, so the write is on a feature branch and the rule
needs no exemption for it; 0.190.0 had used a detached worktree of the default branch, and a push that failed left
the commit on no branch. Only the files `shutdown` writes are written there. The rule exists
so that new work is never authored straight onto the shared branch. A dated record of what a session did is not new
work in that sense, and keeping it off the default branch was the cost the rule imposed, not the protection it gave.
