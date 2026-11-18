---
paths:
  - "**"
---

# List Pending Work Rule

After a unit of work, **check pending work instead of asking "what's next?"** Use this session and current
disk, in order:

1. Open bugs, failed tests or errors the user reported this session.
2. Uncommitted or unpushed work on this branch (`git status`, `git log`).
3. User-named follow-ups, related `TODO`/`FIXME` markers and tracker items this work touched.

Recheck memory against `git log` and current files. **List pending items only, each with its source**;
describe checked-empty sources outside the list, and **not run is not ran empty**. An item its source marks as
someone else's or out of scope is not pending: name it with its owner outside the list, or as a dependency if it
blocks your work. If you read only part of a source, name the part, count the whole, and say the rest is
unread. A quick mid-session status question is answered from what this session has read plus at most one
lookup, sources named. Recommend the next action **as a question** (`write_for_the_reader` obligation 6).

**Never end with "What's next?"** or any open-ended question that asks the user to recall context you have —
**except** when every source was checked to its end and no pending item or blocking dependency remains; then
name the checked sources. This rule owns the list's contents; where it meets `write_for_the_reader` obligation
5, this rule wins on content.

Full text and examples: `.claude/skills/list-pending-work/SKILL.md` (Copilot: `.github/skills/list-pending-work/SKILL.md`).
