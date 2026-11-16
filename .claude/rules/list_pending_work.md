---
paths:
  - "**"
---

# List Pending Work Rule

After a unit of work, **check pending work instead of asking "what's next?"** Use this session and current
disk, in order:

1. Open bugs, failed tests or errors the user reported this session.
2. Uncommitted or unpushed work on this branch (`git status`, `git log`).
3. User-named follow-ups, related `TODO`/`FIXME` markers and tracker items this work touched.

Recheck memory against `git log` and current files; skill/tool summaries are no substitute. Confirming
an item exists neither finds unnamed ones nor checks **who owns it**. List pending items with sources and
state partial coverage. Recommend a rankable next action **as a question** (`write_for_the_reader`
obligation 6); list the rest. This rule owns contents/sources, not rendering; where it meets
`write_for_the_reader` obligation 5, this rule wins on content.

## An item the source marks as someone else's

Read sources, not previous summaries. Carry status **with ownership**: "not confirmed" **and** "not about
this code" is not your open task. If assigned elsewhere or out of scope, an item is not pending, an "open
unknown", or a choice for the user. If useful, name it and its owner **outside** the list. Ownership follows
the source, not the item type.

An item belonging to someone else **that blocks your work** still belongs in the report as a dependency
**with its owner named** (`task_assessment`), not as your next action. If excluding the last candidate
leaves your list empty, name the excluded item and owner plus checked sources; never claim a bare
"nothing pending."

## The usual case: some sources have items, some are empty

**List pending items only.** Describe checked-empty sources **outside** the list, not as numbered items;
**not run** is not **ran empty**. "No failures reported this session" does not claim tests ran.

## The other middle: you read PART of a source

When you read only **part** of a tracker, issue list or backlog, name the part examined **and count the
whole open-item denominator** using its marker, own total or issue-list command. State that the rest is
unread; otherwise a shortlist reads as the whole backlog. **Count, do not estimate or recall a number**;
if you cannot count it, say "a backlog I have not read and did not count." An exhausted source (`git
status`, test output or a tracker read to the end) needs no partial-source caveat, however large or small.

## A quick question is not a full report

A mid-session status question ("anything else?", "is X fixed?") is answered from what this session has
already read, plus at most one targeted lookup. Name those sources in the answer
(`report_what_you_examined`) and offer the full check instead of running it first. Reading every source to
its end and counting the denominator apply only to a session-open or end-of-work report, or when the user
asks for the full list. A scoped answer is never a bare "nothing pending".

## What NOT to do

Do not end a response with:

- "What's next?"
- "What would you like to do next?"
- "Let me know what you'd like to work on."
- Any open-ended question that asks the user to recall context you already have.

## Exception

Only after checking **every source to its end**, finding no pending item or blocking dependency, may you
close with "what's next?" instead of a pending list. Name the checked sources and any **non-blocking**
excluded items and their owners. If a source was not checked, **name it as unchecked**, not empty. A
sampled source is unfinished: "no open items in the section I read; the rest is unread" cannot become
"no open items in the tracker." If anything remains, report it instead of an open-ended request.
