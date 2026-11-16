# `name_from_concept` — rationale

Moved verbatim out of the `name_from_concept` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Why no gate catches this

A name is semantically free to the compiler — any identifier compiles. No build, analyzer, test, or
code-review agent checks whether a caption *describes the right concept*; that is a domain judgment only an
owner can make. So a misdescriptive name is invisible to every automated line of defence — the same shape as
this project's other agent-failure findings. The only lever is deriving the name from the concept **before**
you write it.

## Source

`shallow-naming` assessment, 2026-07-20 (R1). Complements a reuse-first check such as `look_before_you_write`
(where a packet carries one). AL/BC caption specifics
(a Caption is user-facing UI — no code identifiers, no implementation units) are a separate `al-bc` concern,
not restated here.

## Moved at 0.105.0 tightening

From the rule body (verbatim):

- The tell that a carried word is an accident is that it describes a **neighbouring action** rather than the
  thing itself — in the failure below, 'Label' is the print that happens *after* the delay, not what the
  field stores. When that is true, the word is being copied forward, not chosen.
- Documented failure: `Sleep(3000)` refactored into `GetLabelCreationDelayMs()` ('Label' only because the
  delay precedes a label print), then surfaced as a setup field captioned `Label Creation Delay (ms)` — for a
  field that waits on the carrier to register the parcel, nothing to do with labels. It compiled, built clean,
  and passed automated review; only the domain owner caught it.
- This rule is the **net** for when an artifact is born *outside* that step (typically a review comment —
  "make this a setup field" — handled mid-implementation with no design pass).
- This is the naming counterpart to a **reuse-first** check: reuse governs *whether to create at all* (find the
  existing thing, don't recreate it); this rule governs *how to name what is genuinely new*. Reuse first; then
  name the new from the concept. Where a packet ships `look_before_you_write`, that is its reuse-first check.

## Moved from the rule (0.178.1)

The example and the backstop reasoning, cut from the always-on rule to save budget:

- **Example:** `Sleep(3000)` became `GetLabelCreationDelayMs()`, then a setup field `Label Creation Delay (ms)` — for
  a wait on the carrier registering the parcel; 'Label' was only the print that happens *after* the delay.
- **This is the backstop — naming belongs at the design step.** New artifacts are named at the architect /
  technical-design step. This rule is the net for an artifact born outside that step (typically a review comment
  handled mid-implementation). Naming a new domain artifact with no design decision behind it signals skipped
  design — prefer routing it back there.
