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

## A subagent stopped by an API error

A subagent that ends with an API error instead of its report (a safeguard flag, an overload) **has not run**: it
left no findings and no pass, and whatever waits on its result keeps waiting. **It is not an attempt under the limit
above: nothing is sent again without a yes.** Report the error text and request ID to the user, and ask before
sending it again; another model is an option to offer, not a fix to assume. Several stops in one batch get **one**
question, not one per run. A subagent whose own subagent stopped puts this in its report to its caller and does not
send the run again itself.
