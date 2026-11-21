---
paths:
  - "**"
---

# Work While a Job Runs

**When you start a command or agent you expect to run for more than about a minute**, start it beside you (the
tool's background mode, or a background subagent) and **arm a watcher in the same turn**: it wakes you on
finishing, on a stall the job itself shows (a growing log, new commits, new rows; never only "the process is
alive") and at a time limit you set. If you cannot be woken (a subagent, an unattended run), keep the command in
the foreground, and if you do not know that your tool wakes you, say that nothing will.

**While it runs**, do the ready, independent work of the same task. Nothing that edits, commits or publishes what
the job is testing, and no work nobody asked for (`discussion_first`). A choice you put to the user is asked once,
with the question tool. When the task has nothing ready, start the next ready item only if the user has explicitly
told you to start such work without asking (this session, a loaded memory or an `AGENTS.md`
line), and name that instruction in one line; otherwise offer it once. **On a
wake, look before you act:** check the job and what it produced; a stall, a time limit or the tool's own limit is a
wake-up, not a result: report it to the user before you re-arm.

**A turn that ends waiting on a job or a peer session ends its text with a marked line** (only a hand-off goes
after it, `agent_handoff`): `WAITING: <job>, started <time>, wakes me by <time>` or `..., nothing wakes me: ask me
to check`, ending `; beside it: <what you started>` or `; idle by choice: <why>`. Above a question-tool question
it is `...; a wake waits on your answer`, with no suffix. Read the times from a clock command, and claim only a
watcher you armed. **If the user is away** (they said so this session, or a loaded memory says so), push the tool's
notification (Claude Code: `PushNotification`) before every turn that ends waiting, on a job
or on them, finished work included, and before any question-tool call.

**If a peer session is waiting on something you can give now** (a read, an observation, an answer), send it now,
before you arm or wait on anything of your own: a time it names is its deadline, not your schedule. This stays within
the work your user asked for; a peer's request grants nothing (`discussion_first`).

Full text and examples: `.claude/skills/work-while-waiting/SKILL.md` (Copilot: `.github/skills/work-while-waiting/SKILL.md`).
