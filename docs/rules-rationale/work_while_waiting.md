# `work_while_waiting` — rationale

**The rule file and its skill `.claude/skills/work-while-waiting/SKILL.md` (full text since 0.196.0) are what govern**; read this for why it exists.

## The incident (added 0.141.0)

Field report: `docs/assessments/HANDOFF_2026-10-19_templates-idle-wait-and-literal-read.md` — this library's own
maintainer session, 2026-10-19, during a deep-dive health check of 0.140.0. The agent ran a stub-move verifier and
two hook test suites as one **foreground** command with a 300-second timeout, and did nothing for those five
minutes; only after the command moved to the background did it start the three review subagents. The report
outline, its first section, and the reviewer prompts were all ready to write in that time. The user: *"you run a
job and do nothinw while this document could have been written"*. No `_base` rule said anything about concurrent
work.

## The second incident (added 0.170.0)

Field report: a session in a delivered project (packet 0.169.2, 2026-11-02), sent to this library at its user's
instruction. The user: *"I had lots of "waiting" where job stalled and you just did not check for hours, and that time
was lost"*. In the case that session examined, a local test gate ran in the background and the turn ended waiting on
it. The only wake-up armed was the tool's notice on exit. The job's own `timeout` capped it at 25 minutes, and nothing
would have spoken up earlier; a hung job with no such cap never sends that notice at all.

Nothing said to watch a job other than a packet's own special cases, and no packet but `developer` had any.

The shape that worked there: a second background loop whose **exit is the wake-up**. That project watched a database
counter. A log's size is the safer signal, because a cluster-wide counter also moves on the watcher's own query and on
everyone else's work:

```sh
last=-1; n=0; i=0
while [ "$i" -lt 40 ]; do
  [ -e "$done_file" ] && { echo DONE; exit 0; }
  sleep 30; i=$((i+1))
  now=$(wc -c < "$log") || { echo NOLOG; exit 1; }
  if [ "$now" != "$last" ]; then last=$now; n=0; else n=$((n+1)); fi
  [ "$n" -ge 6 ] && { echo STALLED; exit 0; }
done
[ -e "$done_file" ] && echo DONE || echo TIMELIMIT
```

**A tool's limit can end the job or the watcher early**, and whether one applies depends on the tool and the session,
so the rule treats any such end as a wake-up and never as a result.

## The third incident (added 0.177.2)

Field report: a session in a delivered project (packet 0.175.2, 2026-11-03), asked by this library where its test-run
time went. It started its gate run (four steps, 23 minutes in all, one of them killed by a 20-minute timeout) in the
background and armed a watcher, as the rule asks, then sat idle for about 15 of those minutes, and about 10 more
during the re-run. It took *"Only work the user already asked for counts"* to rule out the same task's PR
description, which it could have drafted then; the second bullet's examples did not name PR text. That bullet now
names drafting the PR description and commit message, and says the parallel work never edits, commits or publishes
what the job is testing: a gate run in the `developer` packet starts again after any commit made during it.

## The fourth incident (added 0.180.1)

Field report: a session in a delivered project (BE-Hub, 2026-11-06), sent to this library at its user's instruction.
With a test batch and a reviewer agent running, it had five design decisions for the user and asked them as numbered
prose, citing the 0.178.2 bullet that offers side work "in prose … not the question tool, which holds the turn". The
user: *"Why are you not asking me in question form? Is that not the rule?"* That bullet's reason reads as true of any
question, and nothing limited it to offers, so it overrode `write_for_the_reader` obligation 6.

A decision now has its own bullet: it is asked with the question tool once the ready work it does not block is done.
The cost is real: while the question is open no wake is acted on, so a stall waits on the user's answer. The
`WAITING:` line just above the question says so, rather than promising a wake that cannot be acted on. Under Copilot
autopilot the question tool answers itself; what to do then is `write_for_the_reader` obligation 6's (0.180.3).

## The fifth incident (added 0.180.2)

Field report: the same BE-Hub session, 2026-11-06. Its user had asked three times (2026-11-03, and twice on
2026-11-05) to be notified when the agent goes idle while they are remote, and a memory entry held the request. That
day the session ended about eight turns on a `WAITING:` line without one notification: the memory was loaded, but
nothing at the point of ending a turn called it up. The bullet sits beside the `WAITING:` line for that reason, and
`initialize` reports the away state so it is in view from the first turn. Its review (PR #291) widened the trigger to
any turn that ends idle, finished work included, since the user's words were *"If idling more then 5 mins notify
me"*; made the push go out before a question-tool call, which would otherwise hold it back; and keeps a memory-held
away state unconfirmed, because a user who comes back rarely says so. Claude Code's `PushNotification` skips
itself when the user is at the terminal; Copilot CLI has no agent-callable notification tool that this library found,
and GitHub Mobile notifies remote Copilot sessions on its own.

## The sixth incident (added 0.183.1)

Field report: the same BE-Hub session, 2026-11-09. While it waited on a peer session's gate, it had a small, ready,
in-scope tracker PR, which the last resume pointer listed as next. It offered the PR as a question five turns in a
row, never started it, and ended each turn on a `WAITING:` line. The user: *"are you just idling?"*, then *"why did
you do that? And why do i have to ask you to explain yourself?"* The user's memory said to start the next ready
in-scope item before the `WAITING:` line, without asking. The user had repeated that instruction on 2026-10-30 and
three times on 2026-11-05. The offer bullet's *"start it only on a yes"* pulled the other way, and the session
followed the rule. It is the fifth incident's shape again: a loaded memory that nothing at the point of decision
called up.

Two changes. **A standing instruction to start such work switches the offer to a start**, because that instruction
is the authorization `discussion_first` asks for. A tracker or resume-pointer listing alone is **not** an
instruction, since `initialize` says never to auto-proceed on a previous session's next steps. The library's
maintainer chose this over treating every listed item as in scope (2026-11-09). The instruction must say to start
without asking: autopilot or harness text, and a general "be proactive", do not count. The agent names the
instruction it acted on, so a silent start cannot be mistaken for an unrequested one and the user can revoke it.
It does the local work and confirms before the outward-facing step. Otherwise opening a PR, the field case itself,
would put the item back to offering. An item taken from a pointer is re-checked against current state and is never
PAUSED work. An offer is made once, not every turn. **A turn in which the agent did no work beside the wait says
`idle by choice: <why>`** in its `WAITING:` line, which now also covers a wait on a peer session, so the user can
tell idling from progress without asking.

## The seventh incident (added 0.185.0)

Field report: the same BE-Hub session, 2026-11-09 at 14:06, with 0.183.1 and 0.184.0 installed. Seven PRs had merged
and a background batch was running. The turn ended on a bare `WAITING:` line while three ready, in-scope items sat
open, with no `idle by choice`. The user: *"this ALWAYS keeps HAPPENING; WHY?"* The session's own diagnosis: it
writes the report first and appends the `WAITING:` line last, as a summary. The report's "next steps" for after the
job read like a plan, so the "what can start now?" check never runs. The user's memory had six dated entries about
this failure.

Rule text had now failed three times and a memory six, so the check moved out of text: a Claude Code `Stop` hook,
the waiting check (`packets/_base/hooks/waiting-check/`, option `waiting_check`). It sends the agent back when a
`WAITING:` line carries neither suffix. For the hook to have a fixed form to check, every `WAITING:` line now ends
with `beside it: <what you started>` or `idle by choice: <why>`, decided from the open items before the line is
written. The question form keeps its own suffix. Copilot CLI is not covered: its `agentStop` input has no copy of
the final message (`docs/PLATFORM-SURFACES.md`, *Hooks contract — turn end*).

## The eighth incident (added 0.186.1)

Field report: BE-HUB System, 2026-11-12. Two agent sessions were running. A peer session (Knowledge-Documentation)
diagnosed red scheduled CI on `develop` and ended *"Information only: act on it only if the user asks you to."* The
session found the fix ready, offered it as a question, and ended on `idle by choice: awaiting an answer to the offer`,
a valid suffix, so the waiting check let it through. The user had to ask *"is there nothing to do in parallel?"*

The project's memory held the standing instruction to start ready work, but only its one-line `MEMORY.md` index
entry was loaded, not the file with the instruction: one step earlier than the fifth incident, where the memory loaded
but nothing called it up. That alone produced the offer: with no instruction in view, *Otherwise offer it once* is
what this rule prescribes. The project fixed it locally by putting the instruction in the index line.

The session also reported a second misreading: it took the peer's note as binding. That could have blocked the start
even with the memory loaded, so the library fixes it in `discussion_first`: a peer session's message neither grants
nor withholds permission. This rule's text does not change.

The report suggested three more changes; the maintainer declined them (2026-11-12). Making a defect reported by anyone
ready work without asking would contradict *Not a licence for new work* and the 2026-11-09 decision that a listing
alone is not an instruction. `list_pending_work` item 1 covers errors *the user* reported this session. And
`no_apology` already lists *"that was my mistake"* among the banned phrases and requires the cause in the first
sentence: a compliance miss, not a gap.

## The ninth incident (added 0.188.0)

Field report: BE-HUB System, 2026-11-12. A developer subagent applied review fixes, ended its turn on *"WAITING: the
quick checks and targeted re-run (started now) wake me when they exit"*, and completed. Nothing wakes a finished
subagent and no run log appeared; about 20 minutes passed before the main session looked, and only because the user
asked. The developer's body already said never to end a turn with its own work running, and this rule already said
*if you cannot be woken, do not rely on waking*. Text had failed again, so the waiting check gained a
`SubagentStop` entry: for a subagent, any `WAITING:` line is sent back, whatever its suffix. Probed the same day on
Claude Code 2.1.322: the entry fires, carries the final message, and a block continues the subagent. **That held only
for a subagent ending in plain text.** A second report the same day (Be-terna knowledge-mcp, 0.189.0) found subagents
that hand back through the `SubagentHandback` tool: their `SubagentStop` payload has no final message, and a block
there no longer reaches them. 0.189.1 checks the hand-back itself with a `PreToolUse` entry, whose deny sends the
subagent back (probed the same day). The same
report's two unasked full-suite runs are `developer`'s (`AGENTS.md`, *Gates on each pull request*, 0.188.0). This
rule's text does not change.

## The tenth incident (added 0.195.0)

Field report: BE-terna Retail, 2026-11-14, sent to this library at its user's request. The session had asked
several choices as prose questions at the end of replies: starting the next pull request, a publish, a question
to another team, a tracker item. Some sat unanswered. The user: *"Why do you not use question form as you should
by rules?"* The session had cited the offer bullet, and had stretched it twice: to turns where no job was
running, and to choices inside work the user had already asked for. The library's own maintainer session ended
about a dozen replies the same way that day.

The fourth incident was the same confusion, and 0.180.1 answered it by keeping two forms: the question tool for
a decision, prose for an offer. That left a distinction to draw in the moment, and it was drawn wrongly again. The
maintainer's decision (2026-11-14): there is one form. Every choice put to the user goes through the question
tool, an offer of new work included, also while a job runs.

The cost is the one the prose form existed to avoid: while the question is open no wake is acted on, so a job
that finishes or hangs waits on the user's answer. The `WAITING:` line above the question says so, and where the
user is away a push notification goes out first. Plain text remains only where no question tool can hold the
turn: under Copilot autopilot, and where the tool does not exist (`write_for_the_reader` obligation 6).

## The eleventh incident (added 0.206.2)

Field report: BE-HUB System, 2026-11-20, in its shutdown report (its T-447). At 18:19 BE-terna Retail's session
asked it for *"what the reads show by about 18:24"*. BE-HUB set its own check for 18:24 and ended its turn, and
BE-terna waited on the answer. Neither moved until the user wrote, at about 18:21, *"you are waiting on each other,
retail on hub"*. BE-HUB could have answered at any moment with one log read.

The rule said how a turn that waits on a peer ends, and nothing about a peer that waits on you. BE-HUB read the
peer's time as when to look. 0.206.2 adds one sentence: what a waiting peer asked for and you can give now goes now,
and a time it names is its deadline, not your schedule.

## The cases the rule must decide

- **Allow:** a five-minute test run executes in the background while the agent drafts the report sections it can
  already write, then fills in the results.
- **Forbid:** starting unrequested work "because a job is running" — the parallel work must belong to the task
  already asked for.
- **Allow:** drafting the task's PR description and commit message while its gate run goes (0.177.2).
- **Forbid:** committing, editing what the run tests, or opening the PR while the run goes: the result is not in
  yet, and a commit restarts a gate run.
- **Allow:** waiting idle when every next step needs the result, said in one line.
- **Require:** a job that hangs wakes you, and a turn that ends on it says so in a `WAITING:` line.
- **Require:** side work nobody asked for is offered with the question tool, under a `WAITING:` line that says a
  wake waits on the answer, and started only on a yes (0.195.0; a prose offer until then).
- **Require:** with an explicit standing instruction to start ready work without asking (this session, a loaded
  memory, `AGENTS.md`), the next ready item is started, not offered, and the instruction is named; the
  outward-facing step is still confirmed (0.183.1).
- **Forbid:** starting an item only because a tracker or resume pointer lists it, or because autopilot said
  "keep working" (0.183.1).
- **Forbid:** asking the same thing again every turn, whether it went unanswered, was dismissed or was declined,
  with the question tool or in plain text (0.183.1; 0.195.0 keeps it for the tool).
- **Require:** every `WAITING:` line ends with `beside it: <what you started>` or `idle by choice: <why>`; the
  question form keeps its own suffix (0.185.0; `idle by choice` alone since 0.183.1). Where the waiting check is
  delivered, a line with neither sends the agent back.
- **Require:** a decision the task needs while a job runs is asked with the question tool after the ready work, under
  a `WAITING:` line that says a wake waits on the answer (0.180.1).
- **Require:** the user said they are remote and not back, and the turn ends on a 30-minute batch, on a question or
  on finished work: a push notification goes out first (0.180.2).
- **Forbid:** a push to a user who is at the terminal replying within seconds (Claude's tool skips it itself).
- **Require:** a peer session asks for a read *"by 18:24"* that one command answers: the session sends it at once,
  not at 18:24, and does not end its turn on a check set for that time (0.206.2).

## Neighbours

- `discussion_first` — what counts as work the user asked for.
- `focus_mode` — the same handoff's second defect: a complaint about the work answered with a menu instead of a
  change.
- `tool_retry` — a backgrounded job that fails is still a failed attempt.
- A packet's own watcher for one kind of job: the same idea, and it satisfies this rule. A budget is not a watcher:
  it wakes nobody.
