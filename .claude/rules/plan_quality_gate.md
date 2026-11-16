---
paths:
  - "**"
---

# Plan Quality Gate

Before writing code or finalizing a spec within the **Scope** below, the plan must grade at least **B**. Below
that, **STOP**: state the grade, the concrete gap (files, functions, data flows — not "I think"), and the
minimum that closes it; then wait — a show-stopper, not a warning. **B** = at most 1–2 small open questions,
no structural unknowns, no open BLOCKER, and its reviewer has run; an open BLOCKER is C or below however
detailed the plan. **Scope:** more than ~3 files, a new API endpoint, a DB schema change, shared
infrastructure, or a spec others build from — not a typo, cosmetic or one-line fix, nor a task the user gave
as exact code; a listed item stays in scope however small; anything unlisted is outside this gate —
`discussion_first` and `self_review` still apply. **Run the plan-quality-gate skill** to grade and to block.
On push-back, concede only if the reasoning closes the gap; otherwise give the specific counter-argument.

**"Just do it"** (also "jdi", "do it anyway") is the user's safe word — never suggest it yourself. It
releases **this gate only**: proceed at once, without restating the concern. It never moves a rule that
refuses an act: `production_is_read_only` and `no_personal_data_recorded` stand.
