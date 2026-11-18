---
name: leftover-processes
description: Use when the machine lags or runs short of memory during agent work, or after a search or command timed out — lists orphaned agent processes and how to stop them. Reports only.
---

# Leftover processes

When an agent's shell goes away - a tool timeout, a session that ended - Windows does not stop what it started. A
`find` left walking the registry has held millions of handles for hours, and nothing reported it until the machine
lagged. This skill finds such processes and tells the user how to stop them. **It never stops one itself.**

Claude Code runs the same report when a session starts and shows it when it finds something. Copilot CLI ignores a
session-start hook's output, so on Copilot this skill is how the report is run.

## Steps

1. **Run the report** (Windows; the script ships with the project's search guard):
   `powershell -NoProfile -ExecutionPolicy RemoteSigned -File .claude/scripts/leftover_processes.ps1`
   - No script at that path: the search guard was not delivered here. Say so and stop.
   - Add `-MinutesOld <n>` to change the age threshold (default 10 minutes).
   - Add `-Also python,postgres` to watch more names for this run: a stopped test run's `python.exe` or
     `postgres.exe` is not watched by default (0.197.4).
2. **Show the user the table as printed:** PID, name, age, handles, memory and command line. Each tree starts with
   the process whose parent is gone; the rows under it say `[under <PID>]`, the process that started them. A command
   line *"not readable from this shell"* usually means the process runs elevated. Point at any entry that may be the
   user's own - a `node` dev server, a `bash` window they opened.
   - **The "Possibly a session's background job" list is not leftovers to stop.** A tree lands there when it runs
     from a Claude Code session folder (`Temp\claude\...`) or has a live child outside the watched names: usually a
     background command of this or another session whose launching shell exited, still working. Say whose it may
     be, and never add its PIDs to a stop command.
3. **Give the user the `Stop-Process` line the report printed**, and say it needs an elevated PowerShell for any
   process that runs elevated. It covers the first list only; when every tree is a possible background job, the
   report prints no stop line, and you give none.
4. **Stop nothing yourself unless the user names the PIDs to stop.** Stopping a process changes state beyond this
   repository. When they do name PIDs, stop exactly those; a non-elevated shell cannot stop an elevated one, so say
   which ones remain.
5. **"No leftover agent processes" is a scoped result.** The script names the process names and the age it
   checked; repeat them.
