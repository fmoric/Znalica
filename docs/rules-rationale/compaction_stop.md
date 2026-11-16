# `compaction_stop` — rationale

Moved verbatim out of the `compaction_stop` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Why

A context reset is goal-oriented: it preserves planned tasks and next steps but discards conversational
state — including "do not proceed." Making halt state file-based is the only reliable fix. Pairs with
`agent_handoff` (stop and wait at a handoff), `list_pending_work` (surface what's pending), and `focus_mode`
(a task paused by a mid-task interruption is one kind of halt state to persist before a reset).

## The `subject` slot (0.117.0)

Added to settle a collision with `no_personal_data_recorded` (review O4): a pause note about a real person
either named them — a forbidden write — or pointed at nobody, which is the failure this rule exists to
prevent. The slot carries the record's key instead, which resumes the task without holding the person. It is
optional because most paused work has no subject; `/shutdown`'s PAUSED block and the session docs copy the
line verbatim, and `/initialize` matches on the word `PAUSED`, not the full shape.

## When it fires (0.139.0)

The trigger moved from *"before an action that resets the context"* to *the moment work pauses*. The old
wording could fire only when the agent itself started the reset: a user's `/compact` or `/clear`, an
automatic compaction, or a Copilot context reset gives the model no turn first, so the line it was meant
to write was never written. Found in the library's 0.139.0 review of public skill libraries — the
planning-with-files project keeps its plan state current on disk for the same reason. A reset or handoff
the agent starts itself keeps a check, so the old moment is still covered.
