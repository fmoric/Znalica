# `pr_merge_approval` — rationale

Moved verbatim out of the `pr_merge_approval` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Why

Landing work on the protected branch is irreversible without extra effort and changes shared state visible
to everyone who pulls. Doing it in the same turn as the work itself gives the user no chance to review or
redirect — and "I finished, so I merged" is the most common way that happens.

## Why the landing includes the cleanup *(0.150.0)*

Until 0.150.0 the rule stopped at the merge, so the default end state was a session still on a branch that no
longer exists upstream, with both copies of the branch left behind. Two field reports from delivered projects:
- six stale merged branches built up before anyone counted, and a project's own 19-line cleanup section was
  erased by a re-scaffold seven weeks before anyone noticed;
- the next task's branch check was answered from memory, and edits landed on `main`.

**Why it starts on "merged by anyone":** the git guard denies `gh pr merge` to agents, so in those projects the
user merges — and a cleanup tied to the agent's own landing would never run.

**Why the local delete needs no separate approval:** an agent that reads branch deletion as destructive asks,
and then the cleanup does not happen — the same end state as no rule. The safety comes from the forge, not from
git: `git branch -d` checks only that the tip is reachable from the branch's upstream if it has one, otherwise
from HEAD. So, once the upstream ref is pruned, it refuses a squash-merged branch whose work landed; while a stale
upstream ref remains, it deletes the branch, commits pushed after the merge included; and it deletes a pushed
branch that never landed (all reproduced 2026-10-19, git 2.54). So the merge is confirmed from `gh pr view`, the
decision is made by comparing the local tip with the PR's merged head *before* any delete, `-D` runs only when
every commit is in that head — then nothing that did not land can be lost — and the `(was <sha>)` is reported for
a one-step undo.
**Why the remote branch needs an instruction:** its deletion leaves the machine, so it goes only through the
landing itself (`--delete-branch`) or an instruction that covers it. Decided by the maintainer on 2026-10-19,
both splits.

## Field notes moved from the rule body

 That reading is exactly what this rule was rewritten to remove:
it used to declare itself inert wherever pull requests were absent, so in precisely the projects with no
other gate, it excused itself and the merge went unasked. *(The file is named `pr_merge_approval` because
pull requests were the first case it covered. The rule is about landing work; a PR is one way to do it.)*

## Moved at 0.105.0 tightening

Passages cut from the rule body because they explain *why* rather than instruct. Verbatim, grouped by the
section they came from.

**From *Rule*:**

> Never check the operation against a list of command names: a list is correct only until
> a new way of landing appears, and it fails **silently** — nothing announces that the gate stopped covering
> something.

> **Committing straight onto it counts, and is worth naming because a list-matcher would miss it.** In practice
> `branch_before_edit` should have moved you to a feature branch long before this, so the case that survives is
> committing changes you did not author — someone else's edits, already in the working tree.

> waiting until the push is too late to be a gate.

**From *What does NOT fire it*:**

> None of that changes what anyone else pulls by default. [...] A gate that fires on routine, easily-undone acts is a gate that gets ignored.

> This rule
> declines that case; it does not clear it.

**From *No pull requests?*:**

> Plenty of projects have none: a solo practice, a docs repo, a project that commits straight to a branch.
> [...] Those projects are the ones this rule most
> needs to cover

> **The rule is about landing work; a PR is one way to do it.**

> **The general form, worth carrying to any rule that excuses itself:** [...] "We don't use pull
> requests" describes how work is reviewed, not whether it reaches `main`.

**From *What does NOT authorize a landing*:**

> This gate is rigorous about the **act** having many
> spellings; be equally rigorous here, because **approval has many non-spellings** — and a list of the ones
> that count will silently miss them.

> `discussion_first` settles that boundary in general
> (*"Grammar is the signal, not the test"*); this is the same test applied to **authorizing an act** rather
> than to starting work.

> Replying *"yes, merge"* and then merging in the same turn is
> the failure this clause exists for

> A session that ends with the work committed on a
> branch and reported is a complete session.

**From *How this composes with the branch rules*:**

> Between them the branch's whole lifecycle is covered, and neither hands the case off
> to the other: a gate that delegates to a rule which can excuse itself is a gate with a hole in it.

> An instructed merge or push touches files on the
> protected branch, which is `branch_before_edit`'s shape — but that rule exists so *new work* is never
> authored straight onto the shared branch, and landing approved work is not authoring it.

## The `shutdown` landing (0.190.0)

`/shutdown` lands its own session record on the default branch, with the user's `/shutdown` as the instruction
(the maintainer's decision, 2026-11-12: a session record is not a code change). Every other landing still needs its
own instruction. Each limit closes one way the exemption could grow:
- `shutdown` stages only the paths it wrote and checks the staged list before committing;
- it never force-pushes, merges or opens a pull request;
- where the push is refused, the record stays on its session branch rather than working around the refusal
  (0.190.1: the commit is made on that branch from the start, so a failed push cannot lose it).

## The merge-state check is a gate of its own (0.194.3)

The cleanup after a landing starts by reading the pull request's state from the forge. On 2026-11-14 a session
was told three pull requests were merged and ran that query and the branch deletes in one shell command. The
query printed `OPEN` three times; the deletes ran anyway, since a shell does not stop for what a command prints;
and deleting each remote branch closed its pull request. They were restored from their commits. So the query
runs alone, its answer decides, and anything but a confirmed merge ends the cleanup, whoever reported the merge.
A check that failed or could not run counts as not confirmed.
Deleting the branch of a pull request that is still open closes it: that is the cost the gate protects against.

## A pull request in another repository (0.195.3)

On 2026-11-15 a user typed "merge 818" in one session; pull request 818 was in another repository, where a second
session was working. The first session passed the instruction on instead of merging, on its own judgment, since
the rule did not cover it. The maintainer's decision: the instruction is not carried out from the first session,
and a relayed instruction is information, so the pull request lands when the user says so in the session that
works in its repository. That is `discussion_first`'s peer clause applied to a landing, which is why this rule
only points at it. The check that a bare number is the pull request the user means was added with it and was
not part of what happened: the report does not say whether the first repository had its own 818. One occurrence.

## A yes that asks for more work (0.195.8)

A session reported on 2026-11-16 that it had written "say merge and I'll land it", and the user answered "yes, fix the price-law
line too". The session read that as the yes and started the merge; auto mode's classifier denied it, and the user
then typed "merge" (relayed by that session; one occurrence). The rule's landing words did not cover an answer
that approves and adds work in one sentence. The maintainer's decision: such an answer is an instruction for the
work, and the landing waits for a later instruction, because the user has not seen what would land. Two
neighbours were settled with it: a message that tells the agent to land after the change is an instruction for
both, and a plain yes to the agent's own question or offer to land is an instruction to land, as *"go ahead"*
already was.
