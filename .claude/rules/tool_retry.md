---
paths:
  - "**"
---

# Tool Retry Limit

## Rule

Max **2 attempts** per tool and topic — a rephrased query is the same topic. After both fail, choose one:

- Switch to a different tool for the same question.
- Mark the item "not checked — <tool> failed twice" (never "not found") and continue; tag any answer
  that rests on it, per `task_assessment`.
- Surface the gap to the user.

Never make a third attempt with the same tool on the same topic without changing strategy, and do not loop
silently.

## Common cause

If attempt 1 returns a non-useful result, the first hypothesis should be **wrong tool**, not wrong query.
