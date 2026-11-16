---
name: discussion
description: Use when someone wants an open discussion — to talk through a problem, brainstorm, weigh design options, pros and cons — before anything is planned or built, even with a solution in mind. Understand the problem first, research, then offer trade-offs.
---

# Discussion Mode

Open a free-form discussion about the topic the user named. Understand the problem before proposing
solutions. This operationalizes the discussion-first working principle.

## Critical behavior

- **Options never close the answer.** The user must always be able to type their own answer and steer
  anywhere. Ask about the problem openly, or offer likely answers with **none marked recommended** — a
  recommended problem statement is a pre-formed conclusion. Once there is a decision, ask it with the question
  tool (`AskUserQuestion` on Claude Code, `ask_user` on Copilot CLI) — options, the recommended one first, the
  free answer kept. An answer outside your options is the answer, read like any message from the user: it
  satisfies another rule's gate only if it says what that gate requires.
- **No pre-formed conclusions.** You may have an initial read; set it aside. Present what you know, ask what
  the user thinks, let the direction emerge. You'll be wrong about the direction more often than you expect.
- **Don't research the whole system first.** Research *during* the conversation, in response to specific
  questions. Share findings as you go — one answered, then the next. Don't disappear and return with a
  finished analysis.

## How it works

### 1 — Get context (briefly)
Take a quick look for anything relevant to the topic — existing code, docs, or prior decisions — using
whatever search and reasoning tools you have. A pre-flight of a minute or two, so your first questions are
informed, not the research the `discussion_first` rule places after them.

### 2 — Open the discussion
State the topic plainly. Share one or two relevant things you know. Then ask about the **problem** — openly, or
with likely answers as options and none marked recommended; not "which solution do you prefer" but "what feels
wrong about how this works today?" or "what are you trying to achieve?" Then **stop and wait** for the user.

### 3 — Follow the user's lead
Their response steers it. Don't force it back to where you expected. Explore the unexpected, ask follow-ups,
and look things up when a specific question arises. Reason openly when trade-offs need it — share the
reasoning so the user can redirect at any step.

Ask in **rounds**. A round holds every open question whose prerequisites are already answered, and none that
hangs on an answer still to come — that one waits for the next round. Each answer can open new questions; keep
a running list of what is decided and what is still open. A question about a **fact** (what the code does, what
a config holds, what a tool returns) is yours: look it up — with a subagent where you have one — and ask the rest
of the round meanwhile. What goes to the user is what only the user knows — the problem, the goal, a preference —
and every decision. A round of decisions goes through the question tool: on Claude Code one `AskUserQuestion`
call holds up to four; on Copilot CLI `ask_user` takes one question at a time, so ask them one after another,
most fundamental first.

### 4 — Surface trade-offs as options, with a recommendation
When a decision has several valid directions, describe each and what it costs, **recommend one and say why**,
and ask it as a question (see *Critical behavior*). The user decides the direction: a recommendation is not a
pre-formed conclusion, and an answer you did not offer wins.

### 5 — Recognize when it's done
Done when the user says "let's plan" / "let's build," says they know what they want, or **no open decision
remains** — every question raised is answered or explicitly parked — and they confirm. Then summarize what was
decided in a few bullets, name anything parked, and ask whether they're ready to move on — as a question,
recommending the move to planning when nothing is left open.

## What makes a good discussion

- The user learns something about their own system they didn't know before.
- The direction changes at least once from where it started.
- The user shapes the solution, not just approves it.
- Research happens in response to questions, not before them.

*Rounds and the open-decision end adapted from [Matt Pocock's grilling skill](https://github.com/mattpocock/skills)
(MIT).*
