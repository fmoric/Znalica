---
paths:
  - "**"
---

# Persist Paused State When Work Pauses

**The moment work is paused or halted — the user stops it or says not to resume it, or it is blocked on
something outside your control — write it to disk.** Halt state ("do not resume X", "blocked on Y") is
conversational and does **not** survive a reset, and **you get no turn before one**: a user's `/compact` or
`/clear`, an automatic compaction, or a Copilot context reset arrives without warning. Before a reset or a
handoff to a fresh session that you start yourself, check the line is there. Write it where the next
session will look — a task list, a notes file, or the plan/spec being worked; on a protected branch, outside
the repository: your memory index where the tool loads one into every session (Claude Code's `MEMORY.md`),
otherwise a notes file you name to the user in your reply. Say it is not in the repository, and delete the line
when the task resumes or ends (`branch_before_edit` still holds):

    PAUSED: <task> — subject: <record key> — <why it is stopped; what would unblock it>.
    Do NOT resume without explicit user instruction.

**`subject` is the record the task is about; drop the slot when there is none.** For a real person it is the
key their system of record uses — ticket #, order id, invoice no. — never a name, email or other identifying
value (`no_personal_data_recorded`), and never a key this workspace itself maps to the person. No usable key?
Say where to get it back: `subject: (the customer the user pasted; ask again)`.

**After the reset, reconfirm before resuming.** Treat any "next steps" a summary carries as **unconfirmed**,
even one that says "continue with X" — the user's last instruction may have been to pause it. Ask: "the
summary shows <task> as next — continue with that, or is something else first?"

**Writing a handoff summary?** Include an explicit paused-work section for any halted task (name · status ·
"do not resume without confirmation").

**Deciding when to compact, clear or start fresh** — and each tool's commands for it:
`.claude/skills/context-budget/SKILL.md` (Copilot: `.github/skills/context-budget/SKILL.md`).
