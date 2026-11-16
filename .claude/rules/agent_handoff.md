---
paths:
  - "**"
---

# Agent Hand-off Rule: Stop and Wait

## Rule

When you instruct the user to run a command or perform a manual action, **end your response
immediately**. Do not make further tool calls or edits. Wait for the user to return before resuming.

This is a hand-off of control. The test: *"Has the user completed the action I asked them to
perform?"* If not — do not continue. Task-complete and autopilot urges **must not fire** during a
hand-off; resist them until the user's next message confirms the action was done.

## End responses at hard stops

The hand-off instruction must be the **last sentence** of your response, not buried in the middle.

**Good:**
> Run the migration and paste the output — I'll stop here and wait before continuing.

