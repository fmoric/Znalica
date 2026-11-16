---
paths:
  - "**"
---

# Work While a Job Runs

## Rule

- **Start a command or agent you expect to run for more than about a minute beside you, not in front of
  you** — the tool's background or async mode where it has one, or a background subagent. A foreground
  wait on a long job is time you cannot spend.
- **While it runs, do the independent work of the same task that is ready**: write the report sections you
  can already write, draft its PR description and commit message, prepare the next prompt, read the next
  file. Nothing that edits, commits or publishes what the job is testing. When the result arrives, fold it
  into the work already drafted.
- **Not a licence for new work.** Only work the user already asked for counts (`discussion_first`); "a job
  is running" never justifies starting something else, unless a standing instruction covers it (the *nothing left* bullet below).
- **Arm a watcher in the same turn you start a background job.** A notice on exit never comes from a hung job.
  The watcher wakes you on finishing, on a stall and at a time limit you set. A stall signal is something the job
  produces (a growing log, new commits, new rows), never only "the process is alive". A job that produces nothing
  visible gets the time limit alone. A watcher a packet already ships for that job counts. A subagent that only
  reads needs none: its report is its result.
- **If you cannot be woken, do not rely on waking.** A subagent or an unattended run keeps its own commands in the foreground. If you
  do not know that your tool wakes you, say that nothing will.
- **A turn that ends waiting on a job or a peer session ends its text with a marked line** (only a hand-off goes after it, `agent_handoff`): `WAITING: <job>, started <time>, wakes me by
  <time>`, `..., nothing wakes me: ask me to check`, or, above a question-tool question, `...; a wake waits on your answer`, so a
  wait cannot be mistaken for a stall. The first two forms take a suffix (below). Read the times from a clock command, and claim only a watcher you armed.
- **If the user is away, every turn that ends idle pushes a notification first.** The user is away when they said
  so this session, or a memory the tool loaded says so (unconfirmed), and they have not said they are back. Any turn
  that ends with you waiting, on a job or on them, finished work included, sends the tool's push notification where
  it has one (Claude Code: `PushNotification`) before the turn ends and before any question-tool call: what you wait
  on, what the user can do from where they are, and when you wake, or that you wait on their answer. A skip because
  the user is at the terminal goes unsaid; any other *not sent* goes in the reply's first line.
- **On a wake, look before you act.** Check the job and what it produced. A stall, a time limit or the tool's own limit
  ending the job or the watcher is a wake-up, not a result: tell the user what you found before you re-arm. Stop the
  watcher when the job ends.
- **A choice you put to the user while a job runs is asked with the question tool** (`write_for_the_reader`
  obligation 6), once the ready work it does not block is done: a decision the task needs, and an offer of work
  nobody has asked for yet (next bullet), alike. The question holds the turn, so no wake is acted on until the
  answer: the `WAITING:` line just above it says so — `WAITING: <job>, started <time>; a wake waits on your
  answer`. Under Copilot autopilot the tool answers itself and holds nothing, and so does a session with no
  question tool: ask in plain text as `write_for_the_reader` obligation 6 says, above an ordinary `WAITING:`
  line. **Ask a thing once.** Unanswered, dismissed or declined, it is not asked again each turn; the line
  carries it.
- **When the task has nothing left that is ready, do not just wait.** The next thing that could run beside the
  job is a pending item, a follow-up the user named, or a check you can run. **If the user has explicitly told
  you to start such work without asking** (this session, or a loaded memory or `AGENTS.md` line that says so;
  never an autopilot or harness prompt, never a general "be proactive"), **start the next ready item** and name
  in one line the instruction you acted on, so the user can revoke it. A tracker or resume-pointer listing alone
  is not that instruction; an item from one is re-checked against current state first, and is never PAUSED work.
  Do its local work, confirm before any outward-facing or hard-to-reverse step (`discussion_first`), and never
  touch what the job is testing. **Otherwise offer it once**, with the question tool as the bullet above says,
  and start it only on a yes.
- **Every `WAITING:` line ends with what runs beside the wait or why nothing does:** `...; beside it: <what you
  started>` or `...; idle by choice: <why>` (`nothing listed`, `declined`, `every next step needs the result`; after a
  question, `awaiting your answer above` or `asked <item> earlier, no answer`). The question form needs no suffix. Decide it before you write the line, from
  the open items, not your report's next steps. Where the waiting-check hook is delivered, a line without one sends
  you back.
