# `bounded_search` — rationale

Why the `bounded_search` rule exists (added 0.140.0). **The rule file is what governs**; read this for why it exists
and how it composes with the rules around it.

## Why

When an agent does not know where a file is, it reaches for the Linux idiom `find / -name <file>`. On Windows that
search is useless and harmful at once.

- **Useless:** in Git Bash, `/` is Git's install folder plus `/proc/registry`, the whole Windows registry. The C:
  drive is `/c` and is not listed under `/`, so `find /` can never reach `C:\Program Files` or the repositories. The
  agent gets "not found" for a file that exists, and may conclude that something is not installed when it is.
- **Harmful:** Git Bash's `find` holds one handle open per registry key it visits. When the agent's shell goes away,
  at a tool timeout or at session end, Windows does not stop its children, and `find` keeps running.

| Git Bash path | What it actually is |
|---|---|
| `/` | Git's install folder: `bin cmd dev etc mingw64 mnt proc tmp usr` |
| `/c` | the C: drive, **not** listed under `/`, so `find /` never enters it |
| `/proc/registry` | `HKEY_CLASSES_ROOT`, `HKEY_CURRENT_USER`, `HKEY_LOCAL_MACHINE`, `HKEY_USERS`: the whole registry |
| `/mnt/c` | **not** the C: drive (that is WSL syntax): a plain folder inside Git's install |
| `/tmp` | the user's Temp folder |

## Source

A brief from one Windows 11 developer laptop, 15–16 October 2026 (Git for Windows 2.47.0):

- **Four `find /` searches in about 17 hours, across sessions**, each orphaned: their parent process was gone. They
  ran for up to 18 hours and held 13.3 million handles and about 8.6 GB of kernel paged pool between them, which
  pushed committed memory to 96 % and made the machine lag. Minutes after they were killed: 0.3 million handles,
  89 %.
- **Measured:** `timeout 12 find /proc/registry -name zzz` grew from 2 034 to 11 052 handles in 12 seconds, about one
  per key; `timeout 12 find /c/Windows -name zzz` stayed flat at about 140. `timeout N` did stop `find` both times.
- **Inferred, not measured:** the searches ran elevated (their command lines were readable only from an elevated
  shell), which widens the registry walk and means only an elevated shell can stop the orphans; and they were
  started by Claude Code sessions, the only agent on that machine running commands through Git Bash.

The brief also recorded files written to `/` and to `/mnt/c/...` by an agent that evidently believed `/mnt/c` was
`C:\` — the same wrong path model, which is why the rule names `/mnt/c` too.

## Why a rule *and* a hook

Instructions shape what an agent tries; they do not change what the tool allows. The `developer` packet therefore
also ships the **search guard** (opt-in per delivery, on by default on Windows): a `PreToolUse` hook on both tools
that refuses these searches before they start and answers with the bounded form, plus a report of leftover agent
processes at session start. This rule ships in every packet because any session with a shell can run the search;
the hook needs a delivery option and Windows PowerShell. The rule's last bullet exists because a refused search
invites a workaround — the same search built in a variable or a script, which the guard cannot read.

## Neighbouring rules

- **`report_what_you_examined`** — a bounded search that finds nothing has a scope, and the report names it.
- **`tool_retry`** — a refused or timed-out search is a reason to change strategy (ask the system, read the
  project's settings), not to retry the same walk.
- **`external_tool_state_change`** — stopping a leftover process changes state beyond the repository: the leftover
  process report prints the command and never runs it.

In an `al-bc` delivery the rule carries a *Where things live* table for Business Central files (analyzer DLLs,
BcContainerHelper folders, artifact caches), so an agent looks up before it searches.

## Moved from the rule (0.178.1)

Why `find /` on Git Bash is dangerous, cut from the always-on rule: it can run for hours after its shell is gone,
holding a handle for every registry key it visits under `/proc/registry`.
