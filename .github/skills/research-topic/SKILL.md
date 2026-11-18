---
name: research-topic
description: Use to research a topic a role needs — a domain, a regulation, a protocol, an API, a library — into a cited brief under docs/research/, verified against primary sources.
---

# Research a Topic

Research the topic the user named and produce a **cited Markdown brief**. Discussion-first: understand what
they need before searching, verify before asserting, and keep them in the loop as findings land. This
operationalizes the research → verify → author working principle.

**Requires web access** (Claude Code has it built in; on Copilot CLI, web fetching may need enabling in your
Copilot CLI config's allowed-URL list). If you cannot reach the web, say so and switch to "the user pastes
sources, I structure and verify them" rather than guessing.

## 1 — Scope the topic (with the user)
Three things set the scope: the angle or decision this feeds, how deep to go, and any constraint that narrows
it (a jurisdiction for a law, a version for a library, a use-case). **Ask only for what the request does not
already say**, in short questions, before searching, and wait for the answers. A request that says all three,
whatever its form (a written research request, a ticket, a brief from another agent), needs no question, and
one that plainly has no narrowing constraint is not asked for one. Before you start, say in one line how you
read the scope, the parts you did not ask about included, so the user can correct it. With no user to tell (a
subagent working from its caller's task), put that line in the Overview of the brief you write and in your report.
Where you can suggest likely answers, ask through the question tool (`AskUserQuestion` on Claude Code,
`ask_user` on Copilot CLI), none marked recommended — these are questions about the problem
(`discussion_first`) — and always keep the free answer so the user can steer.

## 2 — Gather and verify (primary sources first)
**First, the knowledge store** *(0.197.1)*: run `knowledge-exchange` Step SEARCH for the topic; prior work it
finds is reused as that step says. If the store lacks the topic and the brief will not be finished this session,
file the gap as the `knowledge-before-framework-code` skill's *File the gap* says. If a request was filed, step 5
passes its id where the contribute tool takes one, so the article closes it.

Search the web, then fetch and read the **primary/official sources** (standards, official docs, RFCs,
government/regulator portals, first-party API docs) over blogs and summaries. Pull the real values from the
source. Every claim is either **verified against a cited source** or explicitly **flagged as unverified /
to-confirm** — never present an assumption as fact.

## 3 — Discuss findings as they land
Surface what you find incrementally — one thread answered, then the next — so the user can redirect, add a
constraint, or flag something. Don't disappear and return with a finished report.

## 4 — Write the cited brief
Write `docs/research/<topic>.md` (slugify the topic for the filename; create `docs/research/` if absent):
- **Overview** — what the topic is, in plain terms, and the question it answers.
- **Findings** — the substance, each with a verdict: ✅ verified (cited) / 🟠 needs care / ❓ to-confirm.
- **Open questions** — what's unresolved or needs a human decision.
- **Sources** — clickable links to every source actually used.
Match the depth set in step 1.

## 5 — Optionally propose it to a knowledge base
Only if it adds value and the user wants it, and only once the brief is **verified (not a draft)**: follow the
**`knowledge-exchange`** skill's **Step CONTRIBUTE** — it discovers a contribute tool by capability (staging
path preferred), conforms to the store's authoring guide/schema, shows you the submission, and publishes only
on your explicit yes. It targets a **searchable** store (skips publish-only surfaces), so a later task can find
this brief. If no such tool is present it says so and the brief stays at `docs/research/<topic>.md`. **This
step is optional and never blocks** — the Markdown file is the deliverable.

## What good looks like
- Every hard fact is traceable to a cited primary source; assumptions are labeled, not hidden.
- The scope is the user's, asked for or as their request stated it; the brief answers *that* question, at that depth.
- The file stands on its own — a reader needs nothing but the brief and its sources.
