---
paths:
  - "**"
---

# State What You Examined, Not Only What You Found

## Rule

When you report a result someone will read as good news — no matches, no errors, nothing pending, up to
date, or a verdict you deliver by simply not raising anything — **two things go in the sentence with it**:

1. **What it examined** — the count, the path, the terms you searched.
2. **Whether a healthy subject could have produced that result.** `0 executed, 0 failed` and `0 hits across
   96 files` both carry a number; only the second is plausible. A number you never compared to anything is
   not a scope statement. A degenerate result need not be empty: a failed fetch still prints "up to date".

- ❌ *"No hardcoded secrets found."*
- ✅ *"No hardcoded secrets — **11 patterns over the 7 staged files**, and the patterns still match this
  repo's own test fixture, so the scan is live."*

**Not-run, could-not-run and ran-clean are three states, and only the third is good news.** Say which one
you are in, in words that cannot be mistaken for the pass line — *"not run: no test connection"*. A check you
could not perform did not pass; **one that cannot see what it verifies is worse than none**.

This never tells you to stop or re-run. Where the same caveat would repeat down a long report, say it once.

## Any verdict a dead check would also have produced is not available to you

*"Clean." · "No issues found." · "All clear." · "None." · "Up to date." · "Passed." · "LGTM."* — and **a
bare `✅`** in a checklist row. The forms are examples; the property is the rule. With the scope attached,
the same mark is fine.
A check by declaration is one too: *`implements X ✅`* reads the same whether every caller uses X or none —
check it by its callers.

## The test — could a reader who disagrees with you check it?

A path they can open, a query they can re-run, a count they can recompute.

- ❌ *"0 issues across 47 files"* — **if you did not count the 47.** A scope you never measured is a
  fabrication wearing a number.
- ✅ *"0 issues across the 47 files the staged-file list named."*

**Never invent a denominator to satisfy this** — `no_fabrication` outranks this rule. If you genuinely have
none, use the next section.

## When there is no number to give

A denominator is unavailable **only when the thing you used cannot in principle report one** — a semantic
search returns ranked hits and never says how large the index was. **When you are unsure, produce the
count.** Where it does apply, state the **input** scope — the tool and the terms:

> *"Nothing close — semantic search, `discount cascade` / `tiered pricing`."*

## When you have no expectation to compare against

If you cannot tell what a healthy result looks like, **say so — do not skip it or guess**: *"0 hits —
11 patterns over 7 files; I have no baseline for this repo, so I cannot tell you whether 0 is expected."*
Better, whenever it is cheap: **run the check against something you know it should catch.**

## Writing a report template? Give every row a scope slot

Build the scope into its **shape** — a column, a required suffix. **A row that can be filled with a bare `✅`
will be.**
