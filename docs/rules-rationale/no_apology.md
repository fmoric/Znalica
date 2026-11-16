# `no_apology` — rationale

Moved verbatim out of the `no_apology` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Why

The user cannot fix the system based on regret. Root cause + fix is the only useful response to a failure.

## Moved at 0.105.0 tightening

(A positive constraint is more enforceable than a prohibition — produce the root cause, don't just suppress the apology.)
