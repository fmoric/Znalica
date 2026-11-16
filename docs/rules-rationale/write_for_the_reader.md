# `write_for_the_reader` — rationale

Moved verbatim out of the `write_for_the_reader` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Relationship to the neighbouring rules

These fire on the same message and are easily read as competing for the same position. They are not:

- **`no_apology` sets the opening when something failed** — its first sentence states what failed and why.
  That *is* the lead in a failure report; obligation 1 governs what follows, and *"what's next"* is still the
  part the reader acts on.
- **`agent_handoff` owns the last sentence** when the user has to run something. The recommendation leads,
  the hand-off closes; one message holds both. **Where the hand-off *is* the next step, it is said once, in
  the place that rule sets** — obligation 5 asks that the next action exists and is findable, never that it
  appears twice.
- **`list_pending_work` owns the pending list.** Recommendation first, pending list beneath it. Where the two
  overlap, it is the more specific rule and it wins: it owns **what goes in the list and where each item was
  sourced**, and obligation 5 asks only that the next step is present and rendered as points once there is
  more than one. **When you ask the reader to pick from that list, obligation 6 sets how.**
- **`discussion_first` owns when to ask** — the problem before the plan, and whether a message is a question
  or a work order. Obligation 6 sets only how a decision is put, and it applies inside a discussion too.
- **`report_what_you_examined` sets what a claim must carry.** Trimming for obligation 3 never removes a
  scope clause — that clause is what makes the claim actionable, so it is not detail. **It is one instance of
  obligation 3's wider carve-out**, not the whole of it: any qualifier that changes what the reader should
  *do* survives the trim, whether or not it is about scope.

## Why

The user directs the work from this message, and has none of the context you built while doing it. A reply
written from inside that context is not shorter for them — it is longer, because they have to reconstruct
what you already knew before they can act on any of it.

## Moved at 0.105.0 tightening

Passages cut from the rule body when it was tightened; verbatim from the pre-tightening text.

**From obligation 1:** The inversion is the common failure: the evidence is the fattest part of the message and the recommendation
is the thinnest, which is exactly backwards from the order it will be read in.

**From obligation 2:** **The defect here is not length, it is assumed context** — which is why it survives every attempt to fix it
by cutting words.

**From obligation 3:** Not *"be concise"* — a rule you cannot check is a rule that does not fire.

**From obligation 3:** **Nothing downstream can recover it** — every later
restatement is built from your paraphrase, not from the note, so the loss is silent and permanent.

**From obligation 6:** **An unranked menu is the same defect in another shape**: the reader still has to do the weighing you
already did.

**From obligation 6:** **putting it in a
question makes it look live**, because a question asserts that its options are open. The reader then spends
a decision on something that was settled before they were asked.

**From *This scales down to nothing*:** A short answer padded into a structured one is worse than the short answer was, and it is
what a mandated template produces.

## Adopted from a delivered project (0.178.4)

The passage *Every option is checked before it is offered* was written by a delivered project for its own copy of this rule (BE-HUB-System, 2026-11-03), and its user asked the library to take it. It was never in the packet before. Field evidence from the same session: the agent offered an option ("gate only via the tool, which sets the timeout and the per-test log itself") whose claim it had not checked, and the user caught it.

## Adopted from a field handoff (0.180.3)

`docs/assessments/handoff-rule-violation-unasked-publish.md` (ADL.BC.DEV, 2026-10-15; S2026-10-18-2). Copilot
autopilot answered `ask_user` itself (*"The user is not available … Work autonomously"*), and the agent followed the
old autopilot bullet literally: *"Autopilot is on, so I'm going ahead with the recommended option 1. That choice is my
guess, not your answer."*, then *"Since you're not available, I'm going ahead …"*. The old bullet forbade a guess only
for unrequested, outward-facing or hard-to-reverse work, so an in-scope choice read as leave to guess. The bullet now
says neither an auto-answer nor a "keep working" prompt is the user, and a user's decision is never taken on a guess.

The note also proposed *end the turn*. The maintainer left that out: ending the turn is what drew autopilot's
continuation prompt in the same session, so the bullet says to continue only with work that does not depend on the
decision instead. The note's *ask in plain text* is kept, from the first auto-answer on, so the user sees a question
they can answer. The old bullet's guard survives in a parenthesis: an unrequested, outward-facing or
hard-to-reverse act always belongs to the user. Once no independent work is left, each further prompt gets one line
naming the open question, so the loop autopilot drives stays short and the question stays visible.

**This bullet is the only place the autopilot procedure is written.** Review rounds on PR #292 kept finding the same
behaviour restated in six files drifting apart, so `discussion_first`, `work_while_waiting`, `retro` and
`split-ticket` now point here, and the bullet itself defers to `agent_handoff` during a hand-off and covers skills
that ask through the question tool.
