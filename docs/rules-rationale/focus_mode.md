# `focus_mode` — rationale

Moved verbatim out of the `focus_mode` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Relationship to other rules

Pairs with the **clarify** skill (the explicit stop) and the **note** skill (side-thought
capture). Complements the **`compaction_stop`** rule — a paused task must be written to disk before
any context compaction/reset (a Claude Code `/compact`, a Copilot reset, or a session handoff).
