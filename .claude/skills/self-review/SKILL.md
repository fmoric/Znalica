---
name: self-review
description: Use before reporting as complete any work that wrote or changed a file. How to review code, instructions and other files, why re-reading the diff is not a review, and how to report what the review covered.
---

# Review Your Own Work Before You Report It Done

*The always-on `self_review` rule carries the trigger and the decision. This is its full text, moved here verbatim in 0.139.0 so it loads only when needed.*

## Rule

Before you report a unit of work as complete — **any work that wrote or changed a file** — review it.

**Whether a review is needed is your decision, not the user's.** Do not ask *"Should I review this?"*, *"Want
me to double-check the changes?"* or *"Let me know if you'd like me to verify it."* — like the *"what's
next?"* that `list_pending_work` bans, it hands the user a decision you already have the information to make.

## When this fires

**Trigger: you are about to report a unit of work as complete, and that work wrote or changed a file** — it
turns on *what you produced*, not on its size:

- **Fires** — you wrote or changed a file: code, a rule, a spec, a config, a document, a prompt, an agent
  definition. **A one-line change that changes what the file means fires this**, however short it was.
- **Does not fire** — you produced no file: a factual answer, a summary of what you read, a search result, a
  review you performed for someone else, a recommendation the user has not acted on.
- **Does not fire** — a change with no semantic content: a typo, whitespace, a reflowed paragraph.

**Narrower than `task_assessment`'s carve-out**, which skips *"a single-line change"*: what exempts a change
here is that it carries no meaning — not that it is small.

**Fire it before you report, not after** — once the message is sent, a review produces a correction rather
than a check.

## What "review" means — sized to what you produced

The trigger is broad; **the act scales to the artifact.** Three sizes:

### 1. Code — hand it to the reviewer

If this project has a reviewer agent, delegate to it. If it has a review skill, run that. If it has neither,
apply the dimensions yourself: correctness, security, error handling, tests, and fit with the surrounding
code. **Where your project defines that standard, read it there rather than working from this list.**

**Delegating is not handing over.** A reviewer agent is usually read-only, often with **no shell** — it cannot
fetch the diff, the file list, or the branch. **Give it what it needs in the task itself**, and check its
verdict says what it covered: a report that names no scope cannot be told apart from a review that received
nothing.

### 2. Text that instructs someone else — re-read the FINISHED ARTIFACT, whole

A rule, a skill step, a spec, an agent definition, a prompt, a README section — any document that tells a
reader what to do. **Open the finished file and read it end to end.** Then use it: take one case it must allow
and one it must forbid, and check that the text *as it now stands* gives the right answer to both. No code
reviewer covers this branch.

### 3. Everything else — re-read the changed file end to end

Config, data, a one-file edit.

## Re-reading the diff is not a review

**The defect is usually in how the change sits against the text around it** — a new paragraph that contradicts
an older one, an added branch that duplicates an existing one, a bolded lead that overrides the operative test
below it. None of that appears in a diff, because the thing it conflicts with did not change.

## Say what the review covered

Report the review in one line, with its scope in the sentence (`report_what_you_examined`):

> *"Re-read the rule end to end, not just the diff: the new lead sentence protected any root file, which
> contradicts the test three sentences below it. Fixed."*

> *"Reviewed — reviewer agent over the 4 changed files; two suggestions, no correctness findings."*

**"Reviewed, looks good" is not available to you** — it reads identically whether you re-read the file or
skipped it.

**A defect in the work that this review catches is the system working** — fix it and say so. It is not a
`violation_response` trigger unless what you caught is a broken rule.

## What you may still ask about

Not *whether* to review — that is settled. Ask when the review **finds something whose fix would widen the
scope you were given**: a defect in code you were not asked to touch, or a fix that reverses a decision the
user already made. Name the finding, name what fixing it would cost, and let them choose.

*Rationale and neighbouring rules: `docs/rules-rationale/self_review.md`.*
