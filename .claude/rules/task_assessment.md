---
paths:
  - "**"
---

# Task Assessment Gate

**Trigger 1 — at the start of any non-trivial task**, not after already reaching for built-in tools:

1. **Scan the available specialized tools** — the tool list, or a tool search.
2. **Read their descriptions** — they are the routing table; a list of bare names is not. If you routed from
   names alone, say so.
3. **Try the matching tool first** — fall back to the built-in search and read tools only if none fits.
4. **For complex or multi-step work**, reason it through first.

A task is trivial (skip the gate) only for a single-line/typo change or a one-sentence factual answer.

**Trigger 2 — mid-task:** a task assessed as quick hits unexpected complexity (something not understood,
unexpected dependencies, more than ~3 files) → **STOP** and re-run the pre-task check.

**When the answer rests on a fallback — tag the answer, don't stop.** It fires on the outcome: the tool was
absent, failed, unconnected, out of retries, **or present and working and you used a built-in anyway** —
disclosing that is not permission to skip the tool; Trigger 1 step 3 still stands. Tag the finding inline with
**what the fallback cannot see** (*"no duplicates — text search only; a semantic search would catch other
wording"*), say it once plainly, and keep working; never block. A stricter local rule wins. A tool you expected
and do not have is `no_silent_tool_loss`'s.

Examples, and how this pairs with `report_what_you_examined`: `.claude/skills/task-assessment/SKILL.md` (Copilot: `.github/skills/task-assessment/SKILL.md`).
