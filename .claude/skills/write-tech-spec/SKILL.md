---
name: write-tech-spec
description: Use when a functional spec (a path) needs its technical spec — the architect works from the real code; the design-reviewer must grade it B or better. A requirement given only in chat needs a functional spec written first.
---

# Write Technical Spec

You have a **functional spec** (what the business needs, in plain language — no code detail) and need the
**technical spec** (how to build it, grounded in this codebase). That translation is the **architect's** job,
because it requires reading the real modules and symbols — something only this developer context can do. This
skill validates the functional spec and **hands it to the architect**; it does not design itself.

## 1 — Resolve the functional spec

Take the functional-spec reference from the argument (a path). If none was given, ask for it in one line and
wait.

Confirm the file **exists** and **reads as a functional spec** — the shape the consultant packet's
`write-spec` skill produces (that skill is not delivered with this packet; the list below is the shape):
**Goal · Process · Acceptance criteria · Data · Forms/screens · Business rules & edge cases · Out of scope · Open
questions**, in business language (*Out of scope* is new in 0.146.0 — an older spec without it still counts).
If the file is missing, is already a *technical* spec, or is plainly not a functional
spec, **stop and say what's needed** — do not hand the architect a bad or absent input. This is a coarse
type-check only; the architect owns the *quality* gate (below).

## 2 — Hand off to the architect

The architect runs its full method: assess the functional spec (STOP and return gaps if it's below-bar — don't
design on ambiguity), read the real code it will touch, write the technical spec to disk, and self-check it
against every functional point. Invoke it per your tool:

- **Claude Code** — spawn the **`architect`** subagent (via the Agent tool) with a prompt that gives the
  functional-spec path and asks for the technical spec. Let it work in its own context; take back its result.
- **GitHub Copilot CLI** — start the **`architect`** custom agent with the `task` tool, with the same prompt, and
  take back its result. **With no `task` tool** (a custom agent whose `tools:` lacks `agent`), **or when the `task`
  call fails**, hand the user `copilot --agent architect` with a ready-to-paste prompt naming the functional-spec
  path, and say the architect runs when they launch it.

**The prompt names the functional-spec path and the code tools this project has for reading it** (its language
server or code graph, as `AGENTS.md` → *Delegating to an agent* says), **and carries no figure** — no estimate, budget or time limit — **and
no conclusion for the architect to confirm.** A number in the prompt anchors the design. A design constraint the
user stated (no new tables, use the standard API) and a user's answer to one of the spec's open questions do go
in, each marked as the user's words — and your report names those answers, so whoever owns the functional spec
can add them to it. Estimating is the separate `estimate-effort` skill.

Do **not** reproduce the architect's method here — it owns the design standard in full; this skill only feeds
it the input and reports the result.

## 3 — Hand the design to the design-reviewer

A technical spec is not build-ready when the architect finishes it. It must be **graded and reach `B` before
the developer builds** — and that grading happens **here, in this project**, because half of it is whether the
design fits the real codebase.

- If the architect **stopped** on a below-bar functional spec, relay its specific gaps to the user (so they can
  take them back to whoever supplied the requirement) and report that **no** technical spec was written. There
  is nothing to grade; stop here.
- On success, report **where** the technical spec landed and the architect's verdict, then hand it to the
  **`design-reviewer`**. Invoke it per your tool:
  - **Claude Code** — spawn the **`design-reviewer`** subagent (via the Agent tool), giving it the path to
    the technical spec **and** the path to the functional spec it came from.
  - **GitHub Copilot CLI** — start the **`design-reviewer`** custom agent with the `task` tool, giving it both
    paths, and take back its grade. With no `task` tool, or when the call fails, hand the user `copilot --agent design-reviewer` with a ready-to-paste prompt
    naming both paths, and say the grading runs when they launch it.
- **It has no shell: hand it both paths.** Given no input it returns no verdict, by design.
- **Both documents always exist by the time you reach this step** — §1 refused to proceed without a functional
  spec, and §2 produced the technical one. So the grade you get back here covers **both** lenses. (The
  `design-reviewer` agent also accepts being invoked directly with no functional spec, and then grades
  code-fit alone and says so — but that path does not come through this skill, so do not offer it as an
  option here.)
- Report the grade you get back. **At `B` or better the developer can build; below `B` it goes back to the
  architect**, not to the developer.

Do **not** grade the design yourself, and do not route it to a consultant packet — `design-reviewer` ships in
this packet precisely so the gate has someone to perform it in a developer-only project.

## What good looks like

- The technical spec is **code-grounded** (real modules/symbols), because the architect — not a code-blind
  quick pass — produced it.
- A weak functional spec is **stopped at the input**, not turned into a confident-but-wrong design.
- The design is **graded by a different agent than the one that wrote it**, and the grade reaches the user
  with the build decision attached — not an implication that the design is build-ready because it exists.
- The grade reaching the user **says what it covered**. Through this skill that is both lenses; where
  `design-reviewer` was invoked directly without a functional spec, a code-fit-only grade never reaches
  anyone looking like a full one.
