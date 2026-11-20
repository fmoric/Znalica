---
name: pr-merge-approval
description: Use before merging into, pushing to or committing onto main or another protected branch, or when deciding whether a message authorizes a landing, and after a pull request is merged. The full merge-approval procedure and what is, and is not, an instruction to land.
---

# Merge Approval — Landing Work on a Protected Branch

*The always-on `pr_merge_approval` rule carries the trigger and the decision. This is its full text, moved here verbatim in 0.139.0 so it loads only when needed; later changes are marked with their version.*

## Rule

**Never land work on a protected branch without explicit user instruction.** Do the work on your own branch,
report what is ready and where it sits, then **stop** and let the user decide when it lands.

A protected branch is the one everyone else takes from — typically `main`/`master`, or the repo's declared
primary. (If the project isn't a git repository, this doesn't apply.)

**The trigger is the act, not the mechanism:** *"does this move work onto the protected branch?"* If yes, the
gate fires. Never check the operation against a list of command names. Merging your branch into it, pushing or
force-pushing to it, fast-forwarding it to your branch, `gh pr merge`, `git push origin HEAD:main`, a squash
button in a web UI — one act, many spellings, all gated.

**Committing straight onto it counts** — in practice, committing changes you did not author that are already
in the working tree. That is still work landing on the shared branch, and it must have been asked for.

**It fires before the command runs, including a purely local one.** A `git merge` into `main` on your own
machine has already landed the work.

## What does NOT fire it

Work confined to your own branch — committing, amending, rebasing it, or pushing **your feature branch** so
someone can look at it. Nor bringing *other* people's work down (`git pull`, updating your branch from
upstream): the gate is about work moving **onto** the shared branch, not off it.

**Not firing this gate is not "go ahead" in general.** A push still leaves the machine, and
`discussion_first`'s outward-facing trigger covers whatever no narrower rule names.

## What to do instead

1. Do the work on a feature branch and commit it there.
2. Report what is ready, **where it sits** (branch and commit), and what landing it would do.
3. **Stop.** The session is complete at this point.
4. Land it only on an **instruction** to land it — "merge", "ok merge", "go ahead", "ship it", or equivalent.
   **Each landing needs its own instruction.** See *What does NOT authorize a landing* below. **A stacked PR
   based on this branch:** retarget it to `<primary>` *before* landing — deleting its base closes it. Retargeting
   changes the forge, so the instruction to land must cover it, or you ask. *(0.150.0)*
5. **Once it has landed — by you or by anyone — clean up:** *After the landing* below. *(0.150.0)*

**With pull requests it is the same steps with a URL:** open the PR, report the URL, stop, merge only
when told, then clean up. Do not call `gh pr merge` in the same turn as `gh pr create`. *(0.150.0: "then clean
up".)*

## After the landing — clean up *(0.150.0)*

**It starts when the pull request is merged, whoever merged it** — you on an instruction, the user in the web UI
(*"merged"*), or a forge auto-merge. A project with the git guard denies `gh pr merge` to agents, so there the
user's merge is the only route. Without this step the session stays on a branch that no longer exists upstream,
merged branches pile up, and the next task starts from the wrong branch.

1. **Confirm it landed from the forge**, and say so: `gh pr view <n> --json state,headRefOid,baseRefName` reads
   `MERGED`. Do not use `git branch --merged` for a forge merge — a squash or rebase merge never lists there.
   **No forge** (a local `git merge`): `git merge-base --is-ancestor <branch> <primary>` after the merge.
   `<primary>` is the PR's `baseRefName`, or the repository's declared primary — not `origin/HEAD`, which can
   dangle.
   **Run that check alone, and read its answer before anything else runs.** Never put it in one command with a
   switch or a delete: a shell runs the next command whatever the check printed. **Anything but a confirmed
   merge → stop** (`gh`: any state but `MERGED`, an error, or no answer; another forge: anything but its own
   word for a completed merge; no forge: a non-zero exit). Whoever said it was merged, report what the check
   returned and clean up nothing: a check that could not run is not a merge, and neither is the user's word.
   **Several pull requests → one answer each**, and steps 2–5 only for those confirmed. No pull request number
   → ask for it, or find it from the branch (`gh pr list --head <branch> --state all`) and say which you took.
   *(Real case, 2026-11-14: told "merged all three", a session ran the query and the deletes in one command;
   all three read `OPEN`, and the deletes closed them.)* *(0.194.3)*
2. **Only if the session is on the merged branch** (`git branch --show-current`) — otherwise leave the current
   branch alone and go to step 4; a branch that is not checked out can be deleted from anywhere. **Check the
   working tree first:** `git status --porcelain` must be empty. Uncommitted changes would move onto
   `<primary>` with the switch — stop and report them.
3. **Still only on the merged branch — get onto `<primary>` and update it:** `git switch <primary>`, then
   **`git pull --ff-only`**. A plain pull can create a merge commit on the protected branch; if `--ff-only` fails,
   stop and report. In a linked worktree
   where `<primary>` is checked out elsewhere, `git switch` refuses: report it and stop there. Updating a branch
   from upstream authors nothing, so `branch_before_edit` does not fire on it.
3b. **A linked worktree on the merged branch — keep its run logs, then remove it** (0.205.1). Find it:
   `git worktree list --porcelain` lists `branch refs/heads/<branch>` under its `worktree <path>` line. None → go to
   step 4. Run this step from the main checkout; if this session is working inside that worktree, report it and
   stop here. Git deletes a worktree's ignored files with it, so a run log written there is lost unless it is copied
   out first. *(BE-HUB-System, 2026-11-19 and 2026-11-20: a pre-check's and a full run's per-test logs in
   `<worktree>/.test-runs/` went with the worktree.)*
   - **Check it first:** `git -C "<worktree>" status --porcelain` must be empty. Changes there → stop and report
     them, as step 2 does.
   - **Copy the run log folder out.** It is the folder the project names for run logs: its gate rule, `AGENTS.md`
     or the brief that started the run (for example `.test-runs/`). Where it exists in the worktree, copy its
     files into the same folder of the main checkout, never over a file of the same name:

     ```bash
     mkdir -p "<main checkout>/<run log folder>" && cp -rn "<worktree>/<run log folder>/." "<main checkout>/<run log folder>/"; ls -A "<worktree>/<run log folder>" | wc -l; ls -A "<main checkout>/<run log folder>" | wc -l
     ```

     `cp -n` can exit non-zero just because it skipped a file of the same name. The second count must be at least the
     first; a smaller one means files did not arrive: stop and report. Report how many files the worktree's folder held, and from where. **No run log folder named** → do not guess one. List what
     would be lost (`git -C "<worktree>" status --porcelain --ignored`), show it to the user, and remove the
     worktree only on their yes.
   - **Remove it:** `git worktree remove "<worktree>"`, **never `--force`**. A refusal is reported, not worked
     around.
4. **Delete the local branch — no separate approval:**
   - **Already gone** (`git branch --list <branch>` is empty; `gh pr merge --delete-branch` deletes it) → report
     it deleted by the landing.
   - **Decide by the commits, before any delete — not by `git branch -d`.** `-d` checks only that the tip is
     reachable from the branch's upstream if it has one, otherwise from HEAD: it deletes a squash-merged branch
     while a stale `origin/<branch>` is still there (a plain pull leaves it), commits pushed after the merge
     included, and refuses one whose upstream was pruned. So:
     - **Forge merge:** take `git rev-parse <branch>`. It equals the PR's `headRefOid`, or `git merge-base
       --is-ancestor <branch> <headRefOid>` holds → every commit on it is in what the forge merged: `git branch
       -D <branch>`. **Otherwise** → commits that never landed: leave it, and report it as pending with both shas.
     - **No forge:** step 1's `merge-base --is-ancestor` held → `git branch -D <branch>` (`-d` can refuse even
       then, when the branch is ahead of its upstream).
   - **Refused because a worktree uses it** → step 3b has not removed it: go back to step 3b, or report why it
     could not.
   - **Report git's `(was <sha>)`**, so the delete is undone in one step: `git branch <branch> <sha>`.
   - Only the branch that just landed is in scope; a branch someone keeps on purpose is never touched.
5. **The remote branch** goes only through the landing itself — `gh pr merge --delete-branch`, or a forge that
   deletes merged branches — or when the instruction to land covered it. Otherwise **list it as pending**:
   deleting it leaves the machine (`discussion_first`). **Before it goes, or is offered:** `gh pr list --base
   <branch>` — an open PR based on it is closed when it goes, so name it and retarget it first (the
   instruction must cover that too). **Deleting it also closes any pull request opened from this branch that is
   still open:** `gh pr list --head <branch> --state open` must be empty too, since step 1 confirmed one pull
   request and a branch can carry a later one. *(0.194.3)*
6. **Report the new state in the same message** — *"the working copy is now on `<primary>` at `<sha>`;
   `<branch>` deleted locally (was `<sha>`)"*, the worktree removed with its run logs copied (*"3 files from
   `<worktree>/.test-runs/`"*), and remotely if it was — and **name** any tracker item the work
   closed. **The cleanup writes no file:** marking the item is new work, and it needs a branch
   (`branch_before_edit`).
7. **The next task checks the branch again** — `git branch --show-current`, not memory. After a merge you may be
   on the protected branch, or still on the dead feature branch if the cleanup did not run.

## No pull requests? Then this rule matters MORE, not less

**Without PRs the PR *procedure* is inert; the gate is not.** With no PR, no review step and no forge,
nothing but this rule stands between a feature branch and `main`. **Do not read "no PRs" as "not
applicable."**

**The general form:** a guard may go inert only on a condition under which **the act it gates cannot happen**
(*"not a git repository"* — no branch to land on), never on a *practice* the act survives.

## What does NOT authorize a landing

Three things read like authorization and are not — approval has many non-spellings.

**1. A question about *whether* to land — even one that names the act.** *"Do I merge?"*, *"should I push
this?"*, *"is it ready to land?"* — the user is **holding** the decision and asking for your read. **Answer
it; what stops is the act, not the reply** — other questions bundled with it still get answered. The
discriminator is **whose decision the sentence is about**, never whether it ends in a question mark:

| the message | what it asks for | then |
|---|---|---|
| *"merge"* · *"ok merge"* · *"can you merge this?"* · *"go ahead"* | **you to act** | land it |
| *"do I merge?"* · *"should we merge now?"* · *"is it ready to merge?"* | **what they should decide** | answer, then stop |
| *"yes, fix the heading too"* · *"ok, and rename the file too"* | **more work first** | do the work, report, then stop |

*"Can you merge this?"* is an instruction wearing a question mark and lands normally — do not re-confirm a
plain instruction.

**A yes that asks for more work is not an instruction to land.** *"Yes, fix the heading too"*, in answer to your
*"say merge and I'll land it"*, approves the fix: what it would land does not exist yet. Make the change, report
it, and stop; the landing takes an instruction given after that. A message that tells you to land after the
change (*"fix that, then merge"*) is an instruction to do both, in that order; *"fix that before I merge"* keeps
the landing with the user. If the change did not go as asked (a failing test, more changed than was named), report
and stop instead of landing. A plain *"yes"* from the user to your own question or offer to land now (*"shall I
merge?"*, *"say merge and I'll land it"*), with nothing added, is an instruction to land; a yes to any other
question is not, and an answer autopilot gave is not the user's (`write_for_the_reader`). *(Real case, reported
2026-11-16: a session read "yes, fix the price-law line too" as the yes and started the merge.)* *(0.195.8)*

**Your own answer is not their instruction.** Replying *"yes, merge"* and then merging in the same turn is the
failure: the recommendation **was** the deliverable. An unqualified yes, however obviously correct, is your
half of the exchange, not theirs.

**2. Approval already spent.** Each landing needs its own instruction. Approval of a previous merge — or of
the plan that produced this work — is not approval to land it.

**3. Task-completion pressure.** A task is complete when the work is presented and its location reported; the
feeling that "this is finished" is **not** authorization. Do not land work to "close out" a task — the correct
end state is: work reported, session stopped, user holding the next action.

## The one landing that carries its own instruction — `/shutdown`'s session record

*(Added in 0.190.0 — not part of the rule's former text.)* The `shutdown` skill commits the files it wrote, and only
those (the dated session record, the resume pointer and its tracker edits), on a session branch in a temporary
worktree, and pushes that commit to the default branch (its Step 6). The user's `/shutdown` is the instruction for that landing: a
session record is not a code change, and it has to be on the default branch for the next session to start from it
(maintainer's decision, 2026-11-12). It covers nothing else. The session's work still lands only on its own
instruction, and `shutdown` force-pushes nothing, merges nothing, and opens no pull request. Where the push is
refused, or the git guard is on, the record stays on its `docs/session-…` branch for the user to land.

## A pull request that is not in this repository

*(Added in 0.195.3 — not part of the rule's former text.)* An instruction such as *"merge 818"* names a number, not
a repository. Before landing on a bare number:

1. **Check it is the pull request the user means.** Read it from the forge: it belongs to this session's
   repository (the current git root; a remote of it, such as a fork's upstream, counts), and its title and
   branch fit what was being discussed. A number alone can match an unrelated pull request here; one that does
   not fit is asked about, not merged.
2. **Not in this repository: do not merge it from here.** Say so, and name the repository it appears to be in,
   or say that you cannot tell.
3. **You may tell that repository's own session**, where your tool can message another session; where it cannot,
   the user takes the instruction there. Your message is information for that session, not the user's word: it
   confirms with the user before it lands anything (`discussion_first`, the peer clause). Report that you passed
   it on and that the merge is **not done**.

An instruction that names another repository outright (*"merge 818 in the retail repository"*) is an instruction
to land there. Because it acts outside this session's repository, confirm the repository and the pull request
once before merging.

## How this composes with the branch rules

`branch_before_edit` gates **authoring changes while on** the protected branch; this rule gates **landing
finished work onto** it. Neither hands the case off to the other.

**They do not deadlock.** An instructed merge or push touches the protected branch, but landing approved work
is not authoring it. **Once this rule is satisfied, carry out the landing; do not branch away from it.**
`branch_before_edit` still forbids making the edit itself there.

*Rationale and neighbouring rules: `docs/rules-rationale/pr_merge_approval.md`.*
