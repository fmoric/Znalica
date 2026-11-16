# `report_what_you_examined` — rationale

Moved verbatim out of the `report_what_you_examined` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## The neighbouring rules

Two halves of one obligation — **a report line carries its scope.** `task_assessment` names what you could
**not** see; this names what you **did** see. They compose, and one sentence often carries both, so do not
spend effort deciding which one you are in. **`no_fabrication` outranks both**: this rule asks for a number,
that one forbids inventing one — when they pull against each other, take the no-denominator branch above.
`no_silent_tool_loss` is the same discipline for a missing *capability*.

## Why the failure is silent

A step that crashes gets fixed the first time it runs. A step that silently passes accretes debt on exactly
the axis it was written to control, and reports a clean bill of health while doing it. 2026-07-27/28: a
session count written for one heading level, run against logs using another, reported **0** against an
actual **10**; a branch check piped through an uninstalled tool reported "no stale branches" for a check
that had never once run. Neither errored; both were found by luck.

## Why a declaration check is named (0.134.0)

The failure is not always an empty result. In a delivered project an interface sat with **no caller** for
seven months while a spec ticked *"interface architecture ✅"* and a module analysis graded it **PASS** because
the implementation *"declares implements"* it — a check that reads the same whether every caller uses the
interface or none does. Every diff in between copied the bypass already in the file, so every per-diff review
passed too. In packets that ship `code-reviewer` and `design-reviewer` (developer, and librarian through
it), those agents gained the matching *Seams* and caller-count checks in the same release.

## Moved at 0.105.0 tightening

Passages cut from the rule body, verbatim:

> The second is the one people skip, and it is the one that catches the real failures.

> - ❌ *"Branch is up to date with the remote."*
> - ✅ *"Up to date — **fetch succeeded, refs refreshed just now**."* A fetch that failed prints the same
>   "up to date" off refs nobody updated. **The degenerate result does not have to be empty.**

> A check you could not perform is not a check that passed, and **a check that cannot see what it verifies is
> worse than no check**: it spends the reader's trust and returns nothing.

> The cost is one clause in a sentence you were writing anyway.

> **That is the property; the strings are only its common forms.** [...] **a bare `✅`**, because a tick in a
> checklist row is a verdict too, and it is the form this failure concentrates in.

> That standard does two jobs: it rules out padding, and it keeps you honest about the number.

> A scope you never measured is a fabrication wearing a number, and it convinces better than the vague
> version, which makes it worse.

> **When you are unsure, produce the count** — this branch is not the cheap way out of the one above.

> A stale index returns zero hits exactly like a clean corpus does. The terms are what let a reader tell which
> one happened.

> Sometimes you cannot answer the second obligation — a first look at an unfamiliar codebase gives you no idea
> what a healthy result looks like.

> A pattern set that finds nothing in a file you planted a match in is broken, and one deliberate hit settles
> in seconds what a paragraph of hedging cannot.

> A habit survives one author; a template survives everyone. Build the scope into the **shape** — a column, a
> required suffix — rather than trusting each writer to remember.
