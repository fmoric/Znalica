# The waiting check

A Claude Code hook on **Stop**, and for subagents on **SubagentStop** (0.188.0) and on the **hand-back** (0.189.1). When an agent ends its turn on a `WAITING:` line that says neither what runs beside the
wait nor why nothing does, the hook sends the agent back into the turn. It then decides from the open items: ready
work of the task it was asked to do; a pending item only if you told it to start such work without asking, or else a
question to you with the question tool (0.195.0; until then an offer in prose); or a reason. It ends with one of these:

- `WAITING: <job>, started <time>, wakes me by <time>; beside it: <what it started>`
- `WAITING: …; idle by choice: <why>`, but only when nothing qualifies.

A `WAITING:` line above a question (`…; a wake waits on your answer`) passes as it is.

**Why it exists.** The `work_while_waiting` rule asks for that suffix. In the field, the rule text and the user's
own memory both failed repeatedly. The agent wrote its report, listed "next steps" for after the job, and appended a
bare `WAITING:` line, while ready work sat open. The hook turns the rule into a check.

**What it does not do.**
- **It never traps a turn.** It does nothing when the turn already continues because of a Stop hook. Any error, or
  input it cannot read, lets the turn end as before. Claude Code also stops honouring a Stop hook after eight blocks
  in a row.
- **It does not judge whether the work started was the right work.** It checks only that the line says something.
- **For a subagent it is stricter, not absent:** nothing wakes a subagent whose turn has ended, so any `WAITING:`
  line in its report is sent back once, whatever its suffix. The subagent is told to run the command in the
  foreground, or to report that the run is its caller's. A subagent ends in one of two ways, and each has its own
  entry:
  - **in plain text** — the `SubagentStop` entry (0.188.0) checks the final message;
  - **through its hand-back tool (`SubagentHandback`)** — the `PreToolUse` entry (0.189.1) checks the report before it
    is delivered. A `SubagentStop` check comes too late for this shape: the report has already reached the caller.
    It runs once per subagent; a small marker file in the temp folder's `waiting_check` folder records that it did.
- **It does not cover GitHub Copilot CLI.** Copilot runs this project's `.claude/settings.json` hooks in PowerShell,
  where the `Stop` and `SubagentStop` entries fail to parse and are skipped, and the `PreToolUse` entry does nothing.
  Copilot's own `agentStop` hook gives no copy of the final message to check.

**Where it lives.**
- the script: `.claude/scripts/waiting_check.ps1`;
- its tests: `.claude/scripts/tests/test_waiting_check.ps1`, which print `N/N passed`;
- the entries in `.claude/settings.json`: one `hooks.Stop` entry and one `hooks.SubagentStop` entry with the same
  command, and one `hooks.PreToolUse` entry with matcher `SubagentHandback`.

It needs Git Bash (where Claude Code runs hooks on Windows) and Windows PowerShell. The `Stop` and `SubagentStop`
entries are a bash command that starts PowerShell only when the final message contains `WAITING:`, because PowerShell
can take several seconds to start. Where hooks run without Git Bash, they fail harmlessly: a failing Stop hook never
blocks. The `PreToolUse` entry starts PowerShell on every subagent hand-back, and is written so it parses in both
shells, because a failing tool hook can deny the call.

**To turn it off**, delete the `hooks.Stop`, `hooks.SubagentStop` and `hooks.PreToolUse` (`SubagentHandback`)
entries that name `waiting_check.ps1` from `.claude/settings.json`, and answer no to the waiting-check question on
the next re-delivery.
