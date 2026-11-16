---
paths:
  - "**"
---

# Trace a Behaviour Claim Before Anyone Relies on It

**Before text that leaves the team says what the system does, will do, or what a change will achieve, trace the
path from the trigger to the effect — every step, with a source you can point to:** `file:line` in code; otherwise
the document section, screen, record or test run. Text that leaves the team: a client or vendor mail, a client
document, a reply you draft for the user to send onward, a spec others outside the team build from. **Within that
text, the trigger is the kind of sentence, not whether a client is named:** *"X happens"*, *"you get error Y"*,
*"the fix makes Z work"*, *"nothing changes for customers with W"*.

**A root cause given to the user to act on, a code-review finding included, a design or options built on a claim
about what the system does, and telling the user a plan or a brief will work where that rests on what the system
does, are traced the same way**, whoever owns the code on the path.

- **Where a setting, version, flag or dispatch selects the path, name it first** and state its value. If you cannot
  read it, the conclusion stays conditional on it (*"on Version 1.0 the cause is X; Version 2.0 is not traced"*).
- **What rests on a conditional or a hypothesis waits:** run the check that would settle it, or offer it if you
  cannot, not the design or options, unless the user asks for them anyway, and then say what they rest on.
- **Reading only the procedure that holds the defect is not a trace.** Start where the user or the system starts
  and follow every lookup, filter and gate to the claimed effect — the step that decides it is often earlier.
- **A finding you set aside as "separate" is re-tested:** is it on this path?
- **Keep the exact case:** *"has a row X"* and *"has only a row X"* are different claims.
- **A described fix is traced as if made:** another step with the same fault means the fix needs both.
- **Not traced, or it depends on setup → state it as a condition, or cut the claim.** A conclusion carries the mark
  of its weakest link: a tag on one sentence does not settle what is built on it.

Not in scope: a commitment that claims no cause or effect (what we will build), repeating the reader's words, a
hypothesis labelled as one, with what would confirm it, and text that stays in the team and brings the user no new
conclusion to act on — a status report, a PR description, an internal note.

Full procedure and a worked example: `.claude/skills/trace-before-you-claim/SKILL.md` (Copilot: `.github/skills/trace-before-you-claim/SKILL.md`).
