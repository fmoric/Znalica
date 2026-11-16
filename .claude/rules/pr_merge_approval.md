---
paths:
  - "**"
---

# Merge Approval — Landing Work on a Protected Branch

**Never land work on a protected branch without explicit user instruction.** Do the work on your own branch,
report what is ready and where it sits — branch and commit, or the pull-request URL — then **stop**. A
protected branch is the one everyone else takes from: typically `main`/`master`, or the repo's declared
primary. (If the project isn't a git repository, this doesn't apply.)

**The trigger is the act, not the mechanism:** *does this move work onto the protected branch?* A merge into it
(a local one too — it has already landed), a push or force-push to it, a fast-forward, `gh pr merge`, a squash
button in a web UI, committing changes you did not author straight onto it. Fire before the command runs.
**Without pull requests this gate matters more, not less.**

**Does not fire:** committing, amending, rebasing or pushing **your feature branch**; bringing other people's
work down (`git pull`). Not firing is not general permission — a push still leaves the machine
(`discussion_first`'s outward-facing trigger). **Nor** the `shutdown` skill landing its own
session record (0.190.0, its Step 6): the user's `/shutdown` is the instruction to land those files, and only those.

**Land only on an instruction to land** — *"merge"*, *"ok merge"*, *"go ahead"*, *"ship it"*; *"can you merge
this?"* is an instruction wearing a question mark. A yes to your offer to land that also asks for more
work (*"yes, fix the heading too"*) is an instruction for that work: do it, report, and stop, unless the message
tells you to land afterwards (*"fix that, then merge"*). **Each landing needs its own instruction.** Three things
read like authorization and are not:

1. **A question about whether to land** — *"do I merge?"*, *"is it ready to land?"*: answer it, then stop. What
   stops is the act, not the reply.
2. **Approval already spent** — on an earlier merge, or on the plan that produced this work.
3. **Task-completion pressure** — the feeling that the work is finished. Your own *"yes, merge"* is your half of
   the exchange, not theirs; never call `gh pr merge` in the same turn as `gh pr create`.

**A bare number names no repository.** If *"merge 818"* is not a pull request of this session's repository, or
does not fit what was discussed, do not merge it: say so, and where it appears to be if you can tell. You may
tell that repository's session where your tool can; it still needs the user's word there (`discussion_first`).

**Once instructed, carry the landing out** — landing approved work is not authoring it, so do not branch away
from it; `branch_before_edit` still forbids making the edit itself there. **Once a PR is merged — by you or
by anyone — clean up:** confirm the merge first, in a command run on its own, and go on only if it is confirmed; switch to the protected branch and fast-forward it (only if the session
is on the merged branch), delete the merged local branch, and say which branch the working copy is on now. The steps are in the skill.

Full procedure and the whose-decision table: `.claude/skills/pr-merge-approval/SKILL.md` (Copilot: `.github/skills/pr-merge-approval/SKILL.md`).
