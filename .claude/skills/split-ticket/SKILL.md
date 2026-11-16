---
name: split-ticket
description: Use when asked to split or break down a ticket, story or plan into subtasks — vertical slices, one session each, in blocked-by order.
---

# Split a Ticket into Subtasks

Turns one piece of work into subtasks someone can pick up one at a time. Each subtask is a **vertical slice**:
a thin path through every layer the change needs (data, logic, interface, tests), finished and checkable on its
own — not a layer of the whole change. The architect's *Work sequence* cuts its steps by these same rules.

The result is text to paste into the tracker. This skill stops at text by design: creating tickets leaves the
machine, so it is a separate step the user starts and confirms first (`discussion_first`), whatever tracker
connection the project has.

## 1 — Read the source

The ticket text, a spec path, or the conversation so far. A ticket link you cannot open: ask for its text — do
not reconstruct it from the title. Then read the code it touches, so every subtask names real modules, and look
for a refactor that would make the change easy before it is made.

Done when you can state the whole change in two or three sentences and name the modules it touches.

## 2 — Cut vertical slices

- **Each slice delivers one behaviour end to end** and can be verified on its own — by a test, a command, or a
  check in the running program. Name that check in the slice.
- **Each slice fits one fresh agent session.** A slice that needs more is two slices.
- **A refactor that makes the change easy comes first,** as its own slice, so the behaviour slices stay small.
- **A wide mechanical change** — a rename or a type change that touches many callers at once — cannot be a thin
  slice without breaking the build. Sequence it in three steps instead: add the new form beside the old one;
  move the callers over in batches (one slice per package or folder), the build passing after each; remove the
  old form once nothing uses it.

Done when every part of the change sits in exactly one slice and every slice names its check.

## 3 — Give each slice its blockers

For every slice, the slices that must be finished before it can start: *"Blocked by: 2, 3"*, or *"none — can
start now"*. A blocker is a real dependency, not the order you happened to write them in, so slices without one
can run in parallel.

Done when every slice has a *Blocked by* line.

## 4 — Check the cut with the user

Show the slices as a numbered list — title, what it delivers, blocked by — and ask through the question tool
(`AskUserQuestion` on Claude Code, `ask_user` on Copilot CLI) whether to keep this cut, recommending it, with
merging or splitting named slices as the alternatives — where no question tool is available, ask in prose with
the same recommendation. Change the list and ask again until the user confirms it. An answer Copilot autopilot
chose is not a confirmation: re-ask as `write_for_the_reader` obligation 6's autopilot bullet says.
When the user asked for the breakdown as a proposal to review — in a file or in the reply — that proposal is this
step's question: write it with step 5's blocks and say what to confirm or change.

Done when the user confirms the list, or the proposal they asked for is written.

## 5 — Write the subtasks

For each slice, in blocker order, a block the user can paste into Jira or Azure DevOps:

```
<N>. <Title>
What it delivers: <the behaviour, from the user's side — not a list of files>
Acceptance criteria:
- [ ] <checkable criterion>
- [ ] <the check named in step 2>
Blocked by: <numbers, or "none — can start now">
```

Leave out file paths and code: they go stale before the subtask is picked up. Give the blocks in the reply;
write them to a Markdown file only when the user asks for one. If the user asks you to create the tickets in the
tracker, say that this skill does not, and why (above).

Done when every confirmed slice has its block, in blocker order.

*Adapted from [Matt Pocock's to-tickets skill](https://github.com/mattpocock/skills) (MIT).*
