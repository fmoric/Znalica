---
name: clarify
description: Force a real stop mid-task — drop the current task and fully engage the user's question or correction. The explicit safe word for focus mode.
---

# clarify — Force Stop

You have been invoked as an explicit interrupt signal: stop the current task and engage fully with
the user's question or correction.

## Immediate action

1. At the next response boundary, drop the task — do not start the next step.
2. Acknowledge the interrupt: "Stopping [task name]. What's the question?"
3. Engage the user's message fully.
4. When the clarification is resolved, ask: "Ready to resume [task name], or is there something
   else first?"

## Do not

- Finish the current edit or tool call before stopping.
- Ask the user to repeat themselves.
- Auto-resume the task after answering — always wait for explicit confirmation.

## Related

The explicit stop path for the **`focus_mode`** rule. If a paused task must survive a context
compaction/reset, write it to disk first per the **`compaction_stop`** rule.
