---
paths:
  - "**"
---

# Branch Before Edit

## Rule

Before the first file change in any response — **by any means, using any tool** — verify the current branch
is **not** a protected branch (typically `main`/`master`, or the repo's declared primary). If it is, create a
feature branch first — no exceptions — then write. Already on a feature branch → proceed. (Not a git
repository → does not apply.) **Check it every time, never from memory:** after a merge the working copy may be
on the protected branch, or on a feature branch that no longer exists upstream. Switching branches and
fast-forwarding one from upstream (`git switch`, `git pull --ff-only`) author nothing and do not fire it.

**The `shutdown` skill's record files (0.190.0).** It writes the session record, the resume pointer and its tracker
edits in a temporary worktree on its own `docs/session-…` branch (its Step 4), which is a feature branch, so this
rule is met there; landing them is `pr_merge_approval`'s. Nothing else is written there but held notes (below),
and a write in the
session's own checkout still needs a feature branch.

**The `note` skill's held notes (0.190.23).** On a protected branch it writes its entry to `pending-notes.md` in the
git directory (`git rev-parse --path-format=absolute --git-common-dir`), never into the working tree, and `shutdown` moves those entries
into the committed notes file. That one file is this rule's exception; any other write still needs a feature branch.

This is a hard gate at the **write boundary**: check before the **first** call that writes, creates,
replaces, renames, or deletes a file.

**The trigger is the effect, not the tool name:** *"does this call change a file on disk?"* Never check it
against a list of tool names. It covers built-in edit/write tools, shell commands that write or replace
files (`mv`, `cp`, `sed -i`, `>`, `tee`, `touch`), and **an MCP server's write tools** — `mcp__*` /
`<server>/*` exempts nothing, including a code-navigation server's editing tools (symbol replace/insert,
memory writes).

Read-only calls never trigger it: reads, searches, symbol lookups, `git status`, `--help`. **If you are
unsure whether a tool writes, read its description before calling it, not after.**
