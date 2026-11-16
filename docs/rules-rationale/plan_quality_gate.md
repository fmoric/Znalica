# `plan_quality_gate` — rationale

Moved verbatim out of the `plan_quality_gate` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Relationship to the task-assessment gate

The `task_assessment` gate fires **first** — at task start, before any plan exists (it picks the right tool
for the job). This gate fires **after** — once a plan is formed, it decides whether the plan is ready to
implement. The two are sequential, not alternatives.

## Why the Scope list is the trigger (0.132.3)

The rule used to open with *"before writing non-trivial code"* and then list a narrower Scope, so an
ordinary two-file feature was in the gate by one sentence and outside it by the next. The sentence that
settled it — unlisted work falls back to the lighter stop-and-ask default — left the rule in the
0.104.0/0.105.0 cuts and survives only in the skill, which loads when it is run.
The Scope list is now the whole trigger: a gate that grades every small change, with a reviewer run each
time, is a gate that gets ignored, and smaller work already has `discussion_first` and `self_review`.
