---
name: task-assessment
description: Use at the start of non-trivial work to route it to the right specialized tool, when a quick task turns out complex, or when an answer rests on a fallback. The full tool-routing gate and how to tag a fallback answer.
---

# Task Assessment Gate

*The always-on `task_assessment` rule carries the trigger and the decision. This is its full text, moved here verbatim in 0.139.0 so it loads only when needed; later changes are marked with their version.*

## Rule

Before starting any non-trivial task, run this check — **at task start**, not after already reaching for
built-in tools.

### Trigger 1 — pre-task check (fires at task start)

1. **Scan the available specialized tools** — the tool list, or a tool-search if the environment has one.
2. **Read their descriptions** — the descriptions ARE the routing table. Identify any tool designed for this
   kind of operation. **A list of bare names is not a routing table**: if any name plausibly matches the
   operation, fetch its description before falling back to a built-in. If none does, say you routed from
   names alone.
3. **Try the matching tool first** — fall back to the built-in search/read tools only if no specialized tool fits.
4. **For complex or multi-step work** — reason it through first (a sequential-thinking tool, if one is present).

The check is tool-agnostic: a new tool's description tells you when to use it.

A task is trivial (skip the gate) only for a single-line/typo change or a one-sentence factual answer.
Everything else — investigation, discovery, multi-file work — hits the gate.

### Trigger 2 — mid-task escalation (fires when complexity is discovered)

If a task assessed as "quick" hits unexpected complexity (something not understood, unexpected dependencies,
more than ~3 files need changing) → **STOP**. Re-run the pre-task check before continuing.

## When the answer rests on a fallback — tag the answer, don't stop

**The trigger is the OUTCOME, not how you found out.** It fires whenever an answer rests on a fallback — the
tool was absent, failed, unconnected, exhausted its retries (`tool_retry`'s *"mark it not checked and
continue"* path ends here too), **or was present and working and you reached for the built-in anyway**. If
the answer would have been different with the tool, this applies. **Disclosing that last case is not
permission to skip the tool** — Trigger 1 step 3 still stands; the tag is what you owe when you did not.

This names what you could **not** see; `report_what_you_examined` names what you **did** — write both clauses
in the same sentence.

1. **Tag the answer — inline, with the finding itself**, not only in a note at the start. Name **what the
   fallback cannot see**, not that something was degraded:
   - ❌ *"No duplicates found (degraded — knowledge tool unavailable)."*
   - ✅ *"No duplicates found — **text search only**. A semantic search would catch an article covering this
     under different wording; text search structurally cannot."*

   The blind spot is the fallback's, whatever put you on it — the same sentence works whether the tool was
   missing or merely unused. If you cannot name what it misses, say *"I don't know what this misses."*
2. **Say it once, plainly**, the first time it comes up in a session.
3. **Keep working.** Never block — not on a missing capability, not on one you skipped. Report what you
   *could* establish.

**A stricter local rule wins.** If this project says **stop and wait** for a specific capability, that rule
governs. This is the default for everything else.

### A tool you expected and do not have

**`no_silent_tool_loss` owns this case** — withheld from you, a personal server, or a team server that did
not load, and which remedy class applies. Read it before you name any remedy. A tool you **had** and did not
use needs only the tag above. When the remedy is an install, say what makes it attach. Claude Code binds MCP
servers at launch, so add **"restart the session"**. On Copilot CLI add **"run `/mcp reload`, or restart the
session"**. *(0.148.0: Copilot 1.0.93 applies config changes between turns, but an install changes no config.)*

*Rationale and neighbouring rules: `docs/rules-rationale/task_assessment.md`.*
