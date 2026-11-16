# `self_review` — rationale

Moved verbatim out of the `self_review` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Relationship to the neighbouring rules

- **`list_pending_work` fires after this one.** Review the work, report it, *then* list what is pending.
  Sequential, not alternatives — and *sequential* describes the **acts**, not two messages: the pending
  list belongs in the report you are already writing, not in a separate later one. What has to come first is
  the review.
- **`violation_response` does not fire on an ordinary finding.** That rule is for a broken *rule*. A defect
  your own review caught is the system working — fix it and say so; do not open a root-cause protocol for it.
- **`report_what_you_examined` owns the reporting sentence**, and **`no_fabrication` outranks both**: if you
  did not re-read it, do not write that you did.

## Why

A defect you catch costs a fix. A defect the user catches costs the fix *plus* their trust in everything you
did not ask them to check. And the asymmetry that produced this rule is the whole argument: when someone has
to ask *"is a review needed?"* and the answer is nearly always yes, **the default was wrong — not the
question.**

## Field notes moved from the rule body

 Three consecutive fixes to instruction
text in one session (2026-08-02) each closed the reported gap and opened a new one: a discriminator that
misfiled an entire legitimate configuration as a broken install; a new trigger that contradicted its own
rule's stated timing three paragraphs above it; and a lead sentence that quietly exempted every case the
rule's own test had been written to catch. **All three were invisible in the diff and obvious in the finished
text**, and every one was caught only because a human thought to ask for a review.

## Moved at 0.105.0 tightening

This is the same move `list_pending_work` makes when it bans *"what's next?"* — **both hand the user a
decision you already have the information to make.** And when the honest answer to *"is a review needed?"*
is almost always yes, asking is not caution; it is a gate the user is holding on your behalf.

**This carve-out is deliberately narrower than `task_assessment`'s.** That gate skips *"a single-line
change"*; this one does not, because a one-line change to text that instructs someone else is exactly where
the failures below came from.

**Fire it before you report, not after.** Once the message is sent, the user has started acting on it — a
review that runs afterwards produces a correction rather than a check, and costs them the work they did in
between.

**Yes, this is a broad trigger, and it is meant to be.** What makes it usable is that it names an observable
*moment* (you are about to report) and an observable *artifact* (a file changed), so there is always a point
at which you can tell whether you owe anything. *"Review after a change"* names neither, which is why it
never fires. A broad trigger is only survivable when what it demands is proportionate — that is the next
section's whole job, and dropping it turns this rule back into a reminder.

One flat obligation would be ignored, because most of what you finish is small.

— one
definition kept current beats two that drift apart.

**No code reviewer covers this branch, and the reason is the diff.** Defects in instruction text are
typically **invisible in the diff and obvious in the finished text**.
