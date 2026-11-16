---
paths:
  - "**"
---

# Violation Response Rule

## Rule

When a rule violation is identified — by the user or self-detected — **stop the current task**. Do not
acknowledge and continue.

## Protocol

1. **Stop** the original task.
2. **Root-cause analysis** (no-apology's what happened / why / what's next):
   - Which rule was violated, and at what exact moment should the gate have fired?
   - Why didn't it fire? (wrong timing, missing trigger, unclear condition?)
   - What systemic fix closes the gap?
3. **Propose the fix** — a new rule, an updated rule, or a durable note in the project's docs. Be specific.
4. **Implement the fix.** New work → get user confirmation first. The fix *is* the task in progress →
   complete it without re-asking. Either way, branch-before-edit applies to the fix file.
5. **Resume** — only after the fix is written to disk.

## What NOT to do

- "I'll remember now" — context doesn't survive sessions; only files do.
- Treat the violation as closed because you acknowledged or named it.
