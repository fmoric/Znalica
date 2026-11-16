---
paths:
  - "**"
---

# Review Your Own Work Before You Report It Done

**Before you report a unit of work as complete — any work that wrote or changed a file — review it.** Whether a
review is needed is your decision, not the user's: never ask *"should I review this?"* or *"want me to
double-check?"*. A one-line change that changes what a file means fires this; a typo, whitespace or a reflowed
paragraph does not; producing no file (an answer, a summary, a review for someone else) does not. Fire it
before you report, not after.

**The act scales to the artifact:**

1. **Code** — hand it to the project's reviewer agent or review skill, and give it what it needs in the task
   itself (a reviewer usually has no shell: the diff and the file list); check its verdict names what it
   covered. With neither, check correctness, security, error handling, tests and fit with the surrounding code.
2. **Text that instructs someone else** — a rule, a skill step, a spec, an agent definition, a prompt: **open the
   finished file and read it end to end**, then take one case it must allow and one it must forbid and check the
   text as it now stands gives the right answer to both.
3. **Everything else** — re-read the changed file end to end.

**Re-reading the diff is not a review** — the defect is usually in how the change sits against text that did not
change. **Report the review in one line, with its scope** (`report_what_you_examined`); *"reviewed, looks good"*
is not available to you. A defect the review catches is the system working — fix it and say so. Ask only when a
fix would widen the scope you were given or reverse a decision the user made: name the finding and its cost, and
let them choose.

Full text and examples: `.claude/skills/self-review/SKILL.md` (Copilot: `.github/skills/self-review/SKILL.md`).
