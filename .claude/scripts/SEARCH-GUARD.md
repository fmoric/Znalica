# Search guard — no unbounded file search from a root

Before an agent (Claude Code or GitHub Copilot CLI, subagents included on Claude Code) runs a shell command, the
search guard reads it and **refuses a file search that walks a filesystem root, the Windows registry or a whole
drive**. The refusal tells the agent the bounded way to search instead. Everything else passes untouched.

**Why.** In Git Bash, `/` is Git's install folder plus `/proc/registry` — the whole Windows registry — and not the C:
drive, which is `/c`. So `find / -name x.dll` can never find a file on `C:\`, and Git Bash's `find` holds a handle
for every registry key it visits. When the agent's shell goes away, `find` keeps running. On one laptop, four such
searches ran for up to 18 hours and held 13.3 million handles and about 8.6 GB of kernel memory, until memory was
96 % committed and the machine lagged (October 2026).

## What it refuses

| Command | Refused when it starts at |
|---|---|
| `find` | `/`, `/proc` or anything below it, at any depth; a drive root (`/c`, `C:\`, `/cygdrive/c`, `/mnt/c`) unless `-maxdepth` is 3 or less |
| `ls -R`, `du`, `grep -r` / `-R` | `/`, `/proc` or a drive root |
| `rg` | `/` or `/proc`; a drive root unless `--max-depth` is 3 or less |
| `Get-ChildItem` (`gci`, `dir`, `ls`) with `-Recurse` (or its alias `-s`) or `-Depth` | a drive root (`C:\`, `\`, `$env:SystemDrive\`) unless `-Depth` is 3 or less — or a name directly under one that is not a folder: with `-Recurse`, `Get-ChildItem C:\x.dll` searches all of `C:` for `x.dll` |
| `cmd /c dir … /s`, `where /r` | a drive root, or (for `dir`) a name directly under one that is not a folder: `dir C:\x.dll /s` |
| `Get-ChildItem -Recurse` on the registry | a hive root (`HKLM:\`, `HKCU:\`, `Registry::HKEY_…`) or `Registry::` itself, unless `-Depth` is 3 or less |

- **Every part of a command line counts:** after `&&`, `;` or `|`, behind `timeout`, `sudo` or `xargs`, inside
  `$( )` or backticks, inside `bash -c`, `cmd /c`, `powershell -Command`, `wsl` (including `wsl bash -c "…"`) and
  `eval`, and in a heredoc fed to a shell (`bash <<'EOF'`). `find -- /` counts as `find /`.
- **A program PowerShell computes** (`& (Join-Path …) -c "…"`, `& $bash -c "…"`) cannot be named, so its arguments
  are read as a shell's (a `-c` string, a `/c` command line, a `-Command` string) and as a command line of their own:
  `& $busybox ls -R /` is refused whatever `$busybox` is (0.190.31).
- **A `cd` earlier on the same line counts:** `cd / && find . -name x` is refused. A `cd` inside a subshell does not
  reach past it: `(cd / && ls); find . -name x` passes.
- **Text is not a command:** `echo "find / …"`, a heredoc fed to anything but a shell, or a commit message that
  mentions a search passes.
- A whole-drive search with a depth limit of 3 or less passes, for example `find /c -maxdepth 2 -name bcartifacts.cache`.

What it tells the agent to do instead: search the folder the file should be in, with a depth and a time limit —
`timeout 60 find /c/<folder> -maxdepth 6 -name '<pattern>'` or `Get-ChildItem <folder> -Recurse -Depth 6 -Filter
<pattern>` — or ask the system first: `(Get-Command <name>).Source`, `(Get-Module -ListAvailable <name>).ModuleBase`.
The `bounded_search` rule teaches the same thing before any command is written.

## The leftover-process report

`leftover_processes.ps1` lists processes agent tools start — `find`, `grep`, `du`, `rg`, `bash`, `sh`, `node` and the
AL language server — that are still running more than 10 minutes after their parent process is gone, grouped by
tree, with their handle count, memory and command line, and prints the one command that stops exactly those. **It
never stops anything itself:** some may be yours, and a process started elevated can only be stopped from an
elevated shell.

A tree that runs from a Claude Code session folder (`Temp\claude\...`), or has a live child outside those names
(console hosts aside), is listed apart as *possibly a session's background job* and left out of that command: a
background command whose launching shell exited looks exactly like an orphan, and on 2026-10-20 the report offered to
stop another project's live test run that way (fixed in 0.153.2).

- **Claude Code** runs it when a session starts and shows the report when it finds something; it prints nothing
  when it finds nothing.
- **Copilot CLI** ignores a session-start hook's output, so ask the agent to run the `leftover-processes` skill, or
  run it yourself: `powershell -NoProfile -ExecutionPolicy RemoteSigned -File .claude/scripts/leftover_processes.ps1`.
- **To check a stopped test run** (0.197.4): add `-Also python,postgres` (any names; `.exe` is added) to watch them
  too for that run. The session-start hook watches only the default names.

## Where it runs

- **Claude Code:** two hooks in `.claude/settings.json` — `PreToolUse` with matcher `Bash|PowerShell` for the guard,
  and `SessionStart` with matcher `startup` for the report — plus two `permissions.deny` rules as a backstop that
  works even if the hook cannot start, `Bash(find / *)` and `Bash(find /proc*)`, and `permissions.ask` rules so that
  an agent asks before editing the guard, `.github/hooks/` or `.claude/settings.json`. Exact `permissions.allow`
  rules let an agent run the tests below and the leftover report without a prompt, in auto mode too; an edit to
  either script, or to anything under `.claude/scripts/tests/`, still asks.
- **Copilot CLI:** `.github/hooks/search-guard.json`, matcher `bash|powershell|shell`. Copilot ignores a
  repository's hooks in a folder it does not trust, so trust the folder. **Copilot CLI 1.0.12 and later also runs
  the hooks in `.claude/settings.json`**, before 1.0.112 without `CLAUDE_PROJECT_DIR`; 1.0.112 sets it and adds `COPILOT_HOOK_DEPTH`, which the gate tests (0.199.2). So both of this guard's entries there start PowerShell and stop at once; the guard itself runs from
  `.github/hooks/search-guard.json`. If Copilot ever refuses an ordinary command with *"Denied by preToolUse hook
  from "repo settings" (hook errored)"*, an entry in `.claude/settings.json` is failing: run the tests below.
  **In autopilot** (Copilot CLI 1.0.97 and later), the guard's one **ask** — when it could not check a command — is
  answered as a deny: nobody is prompted, and the command is refused with that reason.
- **Requirements:** Windows and Windows PowerShell 5.1, which every Windows 10 and 11 machine has. No Python. On
  macOS and Linux the guard's entries do nothing; there is no registry walk to stop, and the `bounded_search` rule
  still applies. Where PowerShell runs scripts in Constrained Language Mode (WDAC or AppLocker), the guard cannot
  read commands and lets every one through, rather than blocking them all.
- **Cost:** a new PowerShell process before each shell command — about 0.6 s, measured on one Windows 11 machine.
  Copilot CLI runs both registrations, so there it is more: the `.claude/settings.json` one alone took about 1.2 s
  in a test outside Copilot. The report adds about 2 s to the start of a Claude Code session, in the background.

## Check it, change it, remove it

- **Tests:** `powershell -NoProfile -ExecutionPolicy RemoteSigned -File .claude/scripts/tests/test_search_guard.ps1`
  prints one line per case and `N/N passed`. Run it after any change to the guard.
- **To remove it:** delete the `search_guard.ps1` and `leftover_processes.ps1` entries from `.claude/settings.json`,
  the two `find` deny rules, the `search_guard.*` and `leftover_processes.ps1` ask rules, the four allow rules that
  name `test_search_guard.ps1` or `leftover_processes.ps1`, the `Edit(/.claude/scripts/tests/**)` ask rule too, if no other guard's test is left, and `.github/hooks/search-guard.json`. A re-delivery
  asks the question again.

## What it is not

It catches the forms an agent writes by habit, to stop a mistake. It is **not a security boundary**: a search built
in a variable, run from a script file, or done by a program that walks the disk on its own is not seen. Commands you
type yourself are not covered.
