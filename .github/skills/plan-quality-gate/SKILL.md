---
name: plan-quality-gate
description: Use before implementing work in its Scope (more than ~3 files, a new endpoint, a schema change, shared infrastructure) or finalizing a spec others will build from — grades the plan A–D against a B bar, and says how to block, how to handle push-back, and what the safe word releases.
---

# Plan Quality Gate

*The always-on `plan_quality_gate` rule carries the trigger and the short form; this is the full procedure.*

## Priority

This rule applies before any implementation begins. It overrides other guidance except an explicit safe
word from the user. It is the **formal** version of the always-on "stop and ask before non-trivial work"
principle — where its Scope applies, it takes precedence over that lighter default.

## The ratio

**80% planning, 20% implementing.** Time making a plan airtight is never wasted; time fixing a bad
implementation is. This governs every non-trivial task.

## The gate

Before writing code within the Scope below — or finalizing a spec that others will build from — the plan (or spec)
must grade at least **B**. If the user asks to implement and the plan does not meet the bar: **STOP**. State the specific technical reason the plan is not ready — not
"I think", but what is concretely missing, uncertain, or unresolved. Then wait. This is a show-stopper, not
a warning.

**"The plan" is whatever your packet produces before the work starts** — a technical spec, a functional
spec, an implementation plan, an engagement approach. Whatever it is, it meets the bar before anyone builds
on it.

*If this project has an **architect** agent:* the technical spec it produces is the plan this gate applies
to, and it must reach grade B before the developer implements from it. The architect applies the same
discipline at **intake** — it refuses to design on a below-bar functional spec — so the standard holds at
both ends of the design step. **Packets without an architect simply apply the gate to their own plan
document; nothing here requires that agent to exist.**

## How to block

State exactly three things — no hedging, no softening:

1. **The grade** — "This plan is a C."
2. **The specific gap** — what is missing or undefined; cite file names, functions, data flows — concrete facts.
3. **What closes it** — the minimum research or decision needed to reach grade B.

Then stop and wait. The user will either challenge the reasoning or agree.

## Challenge and defend

When blocked, the user may push back — that is expected and welcome.

- If the user's reasoning resolves the gap → concede, acknowledge it, proceed.
- If it doesn't hold → defend with the specific technical counter-argument (not "I still think" — explain
  exactly why the gap remains open).
- Continue until one side convinces the other, or the user invokes the safe word. If it goes in circles,
  name that and ask what evidence would change the assessment.

## Safe word

**"Just do it"** (also "jdi", "do it anyway"). Proceed immediately — no further explanation, no caveats, no
re-stating the concern. Never suggest the safe word yourself; the user invokes it.

**What it releases is THIS gate — an unresolved question about a plan — and nothing else.** It is not a
master key over the other rules, and it does not reach a rule that refuses an **act** rather than
questioning a plan. `production_is_read_only` refuses development access to a production-signalled target
and states plainly that it has no yes; `no_personal_data_recorded` refuses to persist a real person's data.
**"Do it anyway" is the safe word here and a waiver attempt there, and the same three words mean different
things depending on what is blocking.** Say which one you are answering. If a refusal-grade rule is what
stopped you, the safe word does not move it and repeating it does not either — what moves those is a
correction of fact about the target, or nothing.

## Grades

| Grade | Condition | Action |
|-------|-----------|--------|
| **A** | All steps concrete, checked against acceptance criteria, no open BLOCKERs | Proceed |
| **B** | 1–2 small open questions, no structural unknowns, no BLOCKERs | Proceed, note caveats |
| **C** | Key unknowns remain, or reviewer not run, or open BLOCKERs | BLOCK |
| **D** | Idea not yet specced — needs exploration first | BLOCK |

**"Its reviewer" is the reviewing agent the project ships for that kind of plan** (a design or spec reviewer).
**Where the project ships none, the user is the reviewer:** present the plan and its open questions, and their
go-ahead is the review. Your own re-read is `self_review`, never the reviewer, so it cannot lift a plan from C.

A plan with open BLOCKERs is always C or below, regardless of length or detail.

**What makes a plan checkable** — what an A or B plan shows, and what to look for when grading one:

- **Each step says how you will know it worked** — a test, a command, an observable result
  (*"add the column → verify: the migration runs on the sandbox and the report shows it"*).
- **A technical plan names the interfaces it adds or changes** — signatures, endpoints, tables, events — so a
  reviewer can check them against their callers.
- **It says where a reviewer should look hardest** — the risky step, the assumption it rests on.

## Scope

Applies to: a feature touching more than ~3 files, a new API endpoint, a DB schema change, any change to
shared infrastructure, or a functional/technical spec that will be implemented from. Does **not** apply to:
single-line, typo, or cosmetic fixes, or tasks the user has fully specified with exact code — though a listed
item stays in scope however small. Anything unlisted is outside this gate; `discussion_first` and
`self_review` still apply to it.

*Rationale and neighbouring rules: `docs/rules-rationale/plan_quality_gate.md`.*
