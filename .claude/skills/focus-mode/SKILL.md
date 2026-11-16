---
name: focus-mode
description: Use when the user sends a message mid-task (a tool call already made, the result not yet reported) that is not an answer to your own question — an "oh, and while you're at it…" aside, a "sorry to interrupt, that's the wrong one" correction, a complaint about the work, or an unrelated side thought — and decides what to do with each.
---

# Focus Mode — Mid-Task Interruption Handling

*The always-on `focus_mode` rule carries the trigger and the short form; this is the full procedure.*

## Rule

While a task is actively being executed (editing, running a command, delegating to a sub-agent,
fetching a URL) and the user sends a message that is **not** a response to a direct question you
asked:

### Step 1 — Assess the message

**A complaint about how the work is going** (*"you run a job and do nothing"*, *"this is taking long"*,
*"why can't you just answer"*) — check this before the correction row; a complaint can carry "wait" too:
→ **It is a finding, not a question to disambiguate.** Name what went wrong in one line and change course
   now — start the parallel work, give the answer you already hold, drop the step it objects to.
→ Ask only if you still cannot tell what the objection is, and then ask about the objection. A menu of
   documents or files in reply to a complaint answers a question nobody asked (2026-10-19: two such menus
   in a row, before the user named the point — *"Idle wait and literal read"*).

**Correction signals** (any present): "wait", "no", "wrong", "stop", "hold on", "not that",
negations, or any phrase suggesting the current direction may be wrong.
→ Surface it: a one-line acknowledgment + "Real interrupt? Reply 'yes' or use the **clarify**
   skill to stop me — otherwise I'll continue."
→ If the user replies with any affirmation ("yes", "yes stop", "that's right") → treat it as the
   clarify skill and stop.

**Input to the task in flight** (no correction signal, but it bears on what you are building right
now — an added requirement, a changed constraint, an extra case: *"also make sure it handles nulls"*):
→ **Take it into the work. Never auto-note it.** If it fits what you are doing, apply it and say in one
   line that you did. If it changes the scope or contradicts the plan, **surface it and wait** — that is
   the correction path, reached by effect rather than by wording.
→ **This is the most common mid-task message and the easiest to mis-file.** It carries no "wait" or "no",
   so it does not read as a correction; it is not about another topic, so it is not a side thought. Filing
   it to a notes file and answering "Noted, continuing." means finishing the superseded version of the task.

**Side thought** (an idea, observation, or question on a genuinely **unrelated** topic, "by the way" —
nothing that changes what you are building right now):
→ Auto-capture with the **note** skill (it lands in `docs/notes/PENDING_NOTES.md`, or on a protected branch is held in the git directory until `/shutdown`) + its one-line
   confirmation, which says what it replaced if anything, + "continuing." → resume the task immediately.
→ **Unrelated is the test, not the absence of correction words.** If you are unsure whether it bears on the
   current work, treat it as input and use the branch above — applying something the user did not need costs
   a sentence; deferring something they did need costs the task.

### Step 2 — the clarify skill is invoked

Stop the current task immediately. Engage the user's question or concern fully. Do not resume the
interrupted task until the clarification is resolved and the user confirms to continue.

### Hard interrupt

If the tool provides a hard interrupt (e.g. ESC mid-generation in Claude Code), it stops
generation immediately — no rule action needed.

## What counts as "mid-task"

At least one tool call (an edit, a command, a sub-agent, a fetch) has been made on the current
task and the task is not yet complete. A task is complete when its final result has been reported
to the user.

## What does NOT trigger focus mode

- A message that is a direct answer to a question you asked.
- The user invoking the clarify skill (that is the explicit interrupt path, not a side thought).
- An unsolicited one-word acknowledgment ("ok", "yes", "go ahead") — those are continuations, not
  interruptions. (A "yes" reply to your own "Real interrupt?" prompt is different — that stops the
  task, per Step 1.)

## Safe-word summary

| Signal | Behaviour |
|--------|-----------|
| A complaint about how the work is going | Name what went wrong, change course now — no artifact menu |
| Correction signals present | Surface + ask if it's a real interrupt |
| Bears on the task in flight (no correction signal) | Apply it and say so — or surface it if it changes scope. **Never auto-note** |
| Side thought — a genuinely **unrelated** topic | Auto-note + continue |
| The clarify skill | Always stop, fully engage |
| Hard interrupt (ESC, if available) | Stops generation immediately |

**Read the middle row before the third.** These rows are not "correction or not" — the side-thought row is
the narrow case, and an earlier version of this table implied otherwise by keying it on the absence of
correction signals. Anything touching the current work goes in the middle row.

*Rationale and neighbouring rules: `docs/rules-rationale/focus_mode.md`.*
