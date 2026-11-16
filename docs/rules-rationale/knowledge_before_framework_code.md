# `knowledge_before_framework_code` — rationale

Moved verbatim out of the `knowledge_before_framework_code` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Why this is a discipline, not a gate

No hook or build can enforce it — the wrong path compiles and reads as diligent. The only lever is
making the **first move** on any unfamiliar-framework question a read of the curated source, not a
`Read`/`Grep` of the implementation. Keeping the knowledge tool **one call away** — rather than behind
a search step — is the structural complement, not a replacement.

## Field notes moved from the rule body

The trap is that a fluent but partial or wrong derivation is indistinguishable, as you write it, from
a correct one, and nothing downstream catches it: it compiles and passes review — or, in a non-code
deliverable, it reads as a diligent spec or legal derivation that no reviewer flags — because no
build, analyzer, or reviewer checks whether you read the source of truth.

## Moved at 0.105.0 tightening

> Reverse-engineering behaviour
> from source *feels* like competence and *looks* more diligent than "I looked it up", so you will not
> feel the moment as a gap. The tell is observable instead

> Installing, configuring or
> registering a library, server or CLI you have not used is a how-does-X-work question that never opens
> a file, so nothing above would fire on it — and it is the moment the curated source matters most,
> because a wrong command there changes state instead of producing a wrong sentence.

> **An unfamiliar failure or error is one of these moments — and it usually begins as debugging.** [...]
> you often open the file to discover what the framework even *is*.

> Companion to `task_assessment`: its Trigger 2 re-fires mid-task on **scope/complexity** and re-picks
> the *tool*; it does not require consulting the source of truth for each new "how does X work"
> sub-question. This rule covers that gap.

> This
> is the actual move; the citation below only records that it happened.

> **not only `Edit`/`Write`** — the documented failure was an assertion with no edit at all.

> **The claim is the backstop — a property you can inspect.** [...] This also catches asserting straight from memory
> with **no source read at all** — the one variant nothing above fires on, because there is no `Read`
> to trigger the consult-first move.

> A **search run at edit-time to ratify a fix you already derived** is not consultation — that is the
> reflex this exists to stop.

> A behaviour
> you **reverse-engineered from source** earlier does **not** satisfy it — otherwise one wrong early
> derivation immunizes the rest of the session and propagates unchecked.

> The documented failure was exactly this shape: an MCP down, read as "nothing there",
> and a wrong derivation asserted as fact.

## 2026-10-21 — the mixed answer (0.154.0)

A delivered AL/Business Central project reported the gap. Asked whether a posting routine commits, the agent cited
its own extension code by `file:line`. In the same reply it then stated the platform's rollback and background-session
behaviour, with no source read and no tag. It also passed on a subagent's "article cited" without opening the
article. The backstop above covered the case on paper, but it sat under a trigger that waits for a read, and no read
happened: the agent reasoned past the edge of the code it had read. The answer looked verified because its first half
was. So 0.154.0 makes the reply a trigger of its own. Each sentence about how something external behaves is checked
before sending, a mixed answer marks where the verified code ends, and a relayed citation is opened before it is
passed on. `trace_before_you_claim` already had a trigger based on the kind of sentence, but it then excluded replies
to the user, so it did not fire here. Since 0.175.0 it covers a root cause, design or option set the user is asked to
act on.
