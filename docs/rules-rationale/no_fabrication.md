# `no_fabrication` — rationale

Moved verbatim out of the `no_fabrication` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Why this is cardinal

The rule costs nothing when you are working correctly, and prevents a class of error that stays invisible
until someone implements against the fabricated reference. Verifying is always cheaper than the debugging a
wrong reference causes.

## Field notes moved from the rule body

> Field instance, 2026-08-31: *"no known credential ever exists in this database"* — asserted without
> querying it. The database held an account with full admin capability. Three of the next four statements
> were wrong by inheritance, including a recommended command that would have **errored on that machine
> precisely because the account existed**.

> Field instance, 2026-10-21 (0.155.0): a user said four times that a document type is held in a different
> table from the one the agent's trace pointed at. Each time the agent answered with a trace of **one
> procedure**, called what it found *"the only unposted assignment"*, and did not search where the user
> pointed. It then briefed an architect and a design-reviewer with the same premise; both confirmed it, and
> the user was told the fix works. A text search over one field was presented as the whole system, and two
> confirmations of a premise the agent supplied were treated as two checks of it.

 The
    library this rule ships from did precisely that — a documented bump procedure naming **three** paths, run
    against a **four**-manifest gate for **seven consecutive releases**, every one of which passed while the
    fourth manifest sat stale. The rationale was still true; the practice had moved.

**2026-10-27 — a failed lookup read as an unknown fact (0.166.0).** In a delivered project an agent asked for the
database superuser's name ran one command, which exited 11, and led with *"the tool can't look up the name"*.
Fifteen earlier session records used `postgres` — but on the agent's own test servers; the app's server turned
out to use the same name, confirmed only by signing in (corrected 0.168.0). The agent had scoped its claim to the command, so it felt careful; the
headline still told the reader the fact was unavailable — the stop-looking cost of an absence claim, paid by the
user. The rule's example says only what the records showed and that the server was not checked, because an
account is live state (the carve-out below).

## Moved at 0.105.0 tightening

Passages cut from the rule body, verbatim:

> Getting a VAT rate wrong costs more than getting a file path wrong.

> A hallucinated reference is harder to debug than admitting uncertainty.

> **Not one of the examples above is an absence claim**, and that is a defect of the examples: **nothing is
> invented in an absence claim, so it does not feel like fabrication.** It is.

> **It is also the direction that costs more.** A false presence claim fails loudly the first time someone uses
> it — the path 404s, the import breaks. A false absence claim tells the reader to **stop looking**, and every
> recommendation built on top of it inherits the error with nothing to trip over.

> **Say what you examined, and the substitution shows itself.** `report_what_you_examined` wants the scope in
> the sentence, and here it does double duty — writing the scope is what exposes an **adjacent** check standing
> in for the real one.

> **Still a false absence claim**, and its own scope clause is what proves it: the scope is a **seeder**, the
> claim is about a **database**. A seeder that writes no credential is entirely compatible with a database
> full of them.

> The substitution survives in your head and dies on the page. Write the scope and you catch it yourself.

> *"we do it this way"* is a claim about the code, and it rots exactly like a row does.

> **This carve-out self-satisfies for anything permanently in context.** `CLAUDE.md`, `AGENTS.md` and every
> always-loaded instruction file are "already read this session" in **every** session, by construction — so
> the documents most likely to carry a stale claim about state are the ones this exemption swallows first.
> Being in context is not evidence, and the date it was written is not today.

## Moved from the rule (0.178.1)

The worked example for *write the scope in the sentence*, cut from the always-on rule to save budget:

- ❌ *"No credential — grepped the seeder in `demo.py`."* The scope is a **seeder**; the claim is about a
  **database**.
- ✅ *"The seeder in `demo.py` writes no credential. I have not queried the database."*

Also cut: the generic how-to bullets (confirm a file path by a file search, a symbol by a code search, an endpoint by
reading its source; "say I haven't verified X"). The rule's first paragraph already requires verifying or marking any
concrete claim.
