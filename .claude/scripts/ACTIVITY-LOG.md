# The activity log (`activity_log` option)

One line per subagent start and stop, appended to `.agent-activity/<YYYY-MM-DD>.jsonl` in the project root. The
`work-monitor` skill reads it to report each subagent's run time, and any subagent that started and never stopped.

## What a line holds, and what it never holds

```json
{"t":"2026-11-18T13:21:25.163Z","tool":"copilot","event":"start","session":"…","agent_id":"…","agent_type":"explore","parent":"…"}
```

- **Only these seven fields:** time (UTC), which tool, start or stop, the session id, the subagent's id and type,
  and its parent's id. Copilot CLI sends the parent; Claude Code does not, so `parent` is `null` there.
- **Never content.** No response, no transcript path, no file name. Claude Code passes the subagent's last message
  and transcript path, and Copilot CLI its response; the script drops them.
- **The folder ignores itself.** On its first write the script creates `.agent-activity/.gitignore` holding `*`,
  so nothing in it is committed. It stays on this machine. Delete the folder at any time; nothing else reads it.
- **It is still activity data:** when someone's agents ran. It is off by default for that reason, and it records
  nothing about who.

## Where it runs

| Tool | Registration | Event |
|---|---|---|
| Claude Code | `.claude/settings.json` → `hooks.SubagentStart`, `hooks.SubagentStop` (`async`, so it never delays the work) | logged where `CLAUDE_PROJECT_DIR` is set and `COPILOT_HOOK_DEPTH` is not |
| Copilot CLI | `.github/hooks/activity-log.json` → `subagentStart`, `subagentStop` (`-Event start` / `-Event stop`) | logged from the project root (`cwd: "."`) |

Copilot CLI also runs the `.claude/settings.json` entries. Copilot CLI 1.0.112 runs them with `CLAUDE_PROJECT_DIR`
set, and also adds `COPILOT_HOOK_DEPTH` to the hooks it runs (measured 2026-11-18). Since 0.199.2 the entry's
gate tests both, so under Copilot the script does not start. As a second line, the script skips a Claude-shaped
payload that carries a `timestamp`: Copilot's has one, and Claude Code's has none (its hooks docs). Nor does it
write a start twice for the same session and agent id. **This rests on a field Claude
Code does not send today:** if it ever adds a `timestamp`, Claude lines stop without an error, so a re-delivery's
live check (one subagent, one start and one stop line) is what catches it. A subagent can stop twice, when the
waiting check sends it back: both stops are written, and the last one is its end.

**Cost.** One PowerShell start per subagent start and stop, and **none per tool call**. On Copilot CLI each hook
runs before the work continues. It took about 3 s per hook in the 2026-11-18 measurement, one Windows machine, so
a subagent costs about 6 s there, plus one start each for the Claude entries, which write nothing there. On Claude Code the entries
run in the background.

**Why not per tool call.** The same measurement put a per-tool hook at about 3.5 s per tool call on Copilot CLI,
whatever its matcher: Copilot maps its tools to Claude's names and runs both files' hooks. Tool counts and
durations on Copilot come from its OpenTelemetry file instead (`work-monitor`, *Spend*).

## Contract

- Reads the payload on stdin; prints nothing; **always exits 0**, also on a payload it cannot read. Copilot CLI
  denies the work of a hook that fails.
- Windows PowerShell 5.1 or later; nothing else. Windows only, like the other guards here.

## Tests

```
powershell -NoProfile -ExecutionPolicy RemoteSigned -File .claude/scripts/tests/test_activity_log.ps1
```

An exact `permissions.allow` rule lets an agent run this command without a prompt, in auto mode too, so type it as shown.

They run the script in a fresh Windows PowerShell (`powershell.exe`) reading the payload on stdin, as both
registrations do: by `-File` as Copilot's does, and once through the delivered Claude entry's `-Command` form. They
use the payload shapes measured on 2026-11-18 and include a run under a culture whose time separator is not `:`. Every content field is a marker that must not reach the log. Output: one `PASS` / `FAIL`
line per case, then `N/N passed`.

## Turning it off

Re-deliver and answer no to the activity-log question. Or remove the two `.claude/settings.json` entries that name
`activity_log.ps1`, the two allow rules that name `test_activity_log.ps1` and the `Edit(/.claude/scripts/tests/**)` ask rule too, if no other guard's test is left, and delete `.github/hooks/activity-log.json`. Either way `.agent-activity/` stays where it is:
delete it yourself. The log keeps no retention of its own; one file per day accumulates until you remove them.
