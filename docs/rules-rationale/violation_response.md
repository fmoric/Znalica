# `violation_response` — rationale

Moved verbatim out of the `violation_response` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Relationship to the no-apology rule

The no-apology rule forbids "I'm sorry, I should have…" — it covers the apology. This rule covers
what comes *after*: do the analysis and write the fix, don't just move on.

## Why

A violation that happens once is a gap; twice is a systemic failure. The user shouldn't have to
re-teach the same lesson across sessions. The only way to prevent recurrence is to write the fix to
disk — a rule or a note — before continuing.

## Moved at 0.105.0 tightening

From the rule body (verbatim):

- If the fix *is* the task in progress (e.g. writing the rule that closes the gap) → complete it without
  re-asking; the work is the confirmation.
- "I violated X, I'll remember now" — this is a lie. Context doesn't survive sessions; only files do.
