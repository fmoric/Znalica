---
paths:
  - "**"
---

# Answer It Yourself Before You Ask Anyone

**Before you ask a person for a fact, answer it from what you can already read** — the repository, the source
or object export, the specification, the connected knowledge base, the tool output in front of you. Only what
survives that is a real question.

**The first trigger is writing a question** — in a questionnaire, a vendor e-mail, a spec's *Open questions*
section, a status report, a meeting agenda, **or a message to the person you are working with right now** —
whether you invented it thirty seconds ago or inherited it. **The one test: is any part of this answerable from
something I can open?**

**"Something I can open" is every source a loaded tool reaches, not the repository alone** — and a deferred tool
you can load counts as loaded: the whole repository, docs and past notes included, then, where a web or package
search tool exists, the package indexes and the web. Never production (`production_is_read_only`). A web query is
a read only while it is generic — no customer or personal data, no internal names — otherwise it leaves the
machine and `discussion_first` asks first. *"Does a ready-made tool for X exist?"* is answered by searching those
in that order and saying where, never by an option that asks the person to go and look.

**Test the property, never the addressee.** *"It is a question for the vendor"* names who would answer, not
whether the answer is in front of you: a third-party component's call sites, and the arguments passed across
the boundary, are in our code.

**Questions that are not requests for a fact, so never delayed by this:** the problem or the direction
(`discussion_first` asks those first); permission — a merge, a change to shared state, anything
outward-facing (reading the code never substitutes for a confirmation; a production-signalled target is
refused, not asked: `production_is_read_only`); and a decision, preference or
priority only that person can make.

**Split the question before you ask it:** answer the readable part, ask only the remainder, and say what you
established. Apply it hardest where the question leaves the team. The readable-versus-external table, the two
kinds of question that survive (data you cannot reach — first ask what the answer would change, and never request
production data for development; behaviour nothing we hold records) and worked examples: `.claude/skills/answer-before-you-ask/SKILL.md` (Copilot: `.github/skills/answer-before-you-ask/SKILL.md`).

**The second trigger is handing the user a step to get past a failure** — elevate, restart, reinstall, log in
again, re-run a setup step, change a setting, or any other. Its cause is the fact: check it against what you can
open — the project's scripts and docs, the session records, how the same step worked before. If you cannot,
state the step as a condition (*"if this account lacks X, …"*), never as the cause. Such a step is not a request
for permission; if it also needs confirmation, ask for that too, once its cause is checked. Procedure: the
section *A fix you hand the user* in that skill.

**The third trigger is an answer that says or implies a fact is unknown** — *"it can't be looked up"*, *"not
available"*. It hands the question back to the reader, so the same test applies, and this project's own records
count as something you can open — when they are about the same system, not only the same kind of thing. Where
they are, and what not to read: the section *An answer that says a fact is unknown* in that skill.
