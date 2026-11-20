---
name: work-monitor
description: Use when asked how work is going, where the time or money went, what is stalled, or how much rework a change took — reports runs over budget, stalls, spend and rework from records already on disk. Reports only.
---

# Work monitor

A summary, after the fact, of where time, spend and effort went in this repository. It reads what the tools and the
project already record. It never watches another session live, and it **changes nothing**: no process stopped, no
agent steered, no file written unless the user asks for one.

## 1 — Fix the window and say it

Use the window the user names: today, this pull request, since a commit or a date. With none named, use **this
branch since it left the default branch** (`git merge-base HEAD origin/<default branch>`). On the default branch
itself that window is empty: use today instead. Say which window you used, in the report's first line.

## 2 — Read the sources

**Give each source its own state**, never one state for a whole section: **read** (what was read, how many),
**not run** (the source is not here, say which), or **could not run** (it is here and failed, say how). A source
you did not read is never *nothing found*, and a count is good news only with what it counted.

**Runs over budget.**
- **Find the run log folder.** It is the folder the main session named in each task brief, where the developer
  agent writes each run's per-test results, one new file per run. A brief from an earlier session is not readable
  from here, so look on disk: the folders listed in the file `git rev-parse --git-path info/exclude` names (where
  `docs/process/PR_GATES_PRECHECK.md` has the main session add it), then a git-ignored folder inside the worktree
  that holds test logs, confirmed with `git check-ignore <folder>/`. Ask the user if neither settles it. With no
  folder found: *not run: no run log folder found*.
- **Measure each run file in the window.** Its start is the first line's time, or else the file's creation time.
  Its end is the last write.
- **Compare it with its own budget, from `docs/process/PR_GATES_PRECHECK.md`.**
  - A **pre-check** gets about 8 minutes, or the project's gate rule's figure if lower.
  - A **gate run** gets its measured time and half again, or the budget the gate file states. A gate run not yet
    measured has no budget: past about 20 minutes, it is one to report.
  - Where the file does not say which kind of run it was, say *kind unknown* and give its length against both.
  - Name each budget's source. The one hour in `AGENTS.md` → *When a subagent that writes goes quiet* is a whole
    task's budget, not a test run's: don't use it here.
- Report, for example: *"runs: 7 files read in `<folder>`; 2 over budget — `<file>`: 14 min, pre-check, 8-min
  budget"*.

**Stalled or idle.**
- **Stall candidates.** A run file that stopped growing more than 10 minutes ago while no line says the run ended
  is a stall candidate, not a finding: the file cannot say whether its process is still alive.
- **Live processes, on Windows.** Run the report the `leftover-processes` skill describes:
  `powershell -NoProfile -ExecutionPolicy RemoteSigned -File .claude/scripts/leftover_processes.ps1`, alone and
  exactly as written, with no `timeout`, pipe or `2>&1` around it: `.claude/settings.json` allows only that string. With no
  script at that path, the search guard was not delivered: *not run*. On another OS: *not run: Windows only*. Show
  its lists as that skill says.
- **Subagent run times,** where the `activity_log` option is on, which `.claude/scripts/activity_log.ps1` being
  there shows: read `.agent-activity/*.jsonl` for the window, one line per subagent start and stop. A line's keys
  are `t` (UTC time), `tool`, `event` (`start` or `stop`), `session`, `agent_id`, `agent_type` and `parent`, and no
  others: print one line's keys before a script reads them. Pair each `start` with the **last** `stop` of the same
  `session` and `agent_id`; a subagent the waiting check sent back stops twice. Report each subagent's type and
  run time. **A subagent stopped by an API error writes no stop line** (two code-reviewer runs, 2026-11-20). So a
  start with no stop, followed by later lines from its session, is *no stop line: still running beside them, or ended by an API error*, never a stall finding: the session's own
  report says which. A start with no stop and no later line from its session is a stall candidate, never a finding:
  the session may have ended, or the subagent died the same way. No script: *not run: activity log off
  (`activity_log` option)*. Script there but no folder yet: *read: no subagent has run since it was delivered*.
- **Watchers.** Name any watcher this session armed and is still running. Another session's watchers and tool calls
  are not visible from here: say so, instead of reporting none.

**Spend.**
- **Copilot CLI:** if `COPILOT_OTEL_FILE_EXPORTER_PATH` is set and the file exists, read its **metric records
  only**, never span or log content: that file can hold prompts when content capture is on. Total the
  counters `gen_ai.client.inference.usage.input_tokens` and `…usage.output_tokens`, per agent where the record names
  one, over the window. Give the cache-read, cache-write and reasoning counters on their own lines, never added in;
  whether they overlap input and output is not documented. Leave out the `…operation.input_tokens` and
  `…operation.output_tokens` histograms, which count the same tokens again. If a
  counter's exported values only grow, it is a running total: take its last value minus its first in the window,
  never the sum of its lines. Otherwise *not measured: Copilot's OpenTelemetry file export is off*. It is turned on
  with `COPILOT_OTEL_ENABLED=true` and `COPILOT_OTEL_FILE_EXPORTER_PATH=<file>` (`copilot help monitoring`).
- **Claude Code:** *not measured: no collector*, unless the user names a collector's output file. Then read it
  for `claude_code.token.usage` and `claude_code.cost.usage`, applying the same running-total rule. Claude Code's
  docs describe usage export through OpenTelemetry (`CLAUDE_CODE_ENABLE_TELEMETRY=1` and an OTLP endpoint) and
  document no file exporter: <https://code.claude.com/docs/en/monitoring-usage.md>.
- Never read session transcripts for spend. Their format is internal to each tool and changes between versions.

**Repeated rework.**
- **Commits.** Run `git log --oneline --name-only <since-commit>..HEAD`, or `--since=<date>` for a date window.
  Count reverts, commits that fix a commit from the same window, and files touched by more than three commits.
  Name the commits.
- **The open pull request,** when `gh` is installed and signed in: `gh pr view --json reviews,commits`. Count
  review rounds that asked for changes, and the commits after each. In this packet most review rounds happen
  in the session (the `code-reviewer` agent) and never reach the forge, so **0 here means none were posted, not
  none happened**: say that with the count. Without `gh`: *not run*.
- **Re-runs.** Count run files in the run log folder that ran the same tests again after a failure.

## 3 — Report

1. **Open with the window**, then the line that says where most of the time or effort went, naming the sources it
   rests on: *"from rework only — runs not run, spend not measured"*, when that is the case.
2. **Then the four sections**, in the order above, each source with its state and scope.
3. **Then the next steps.** Where anything is over budget, stalled or reworked, give one suggested next step per
   finding, as a question: a narrower test selection, the `retro` skill on the session, or a look at leftover
   processes. On a yes to stopping processes, hand over to the `leftover-processes` skill: it stops only the PIDs
   the user names, and never one from its *possibly a session's background job* list. Take no other step yourself.

The report goes in the reply. Write it to a file only when the user asks, at the path they name.

## What it does not do

Stop, restart or steer an agent; read another repository; send anything off the machine; parse transcripts; keep
a log of its own. A recurring check is the user's choice of their tool's own scheduler (Claude Code's `/loop`,
Copilot CLI's experimental `/every`), never something this skill sets up.
