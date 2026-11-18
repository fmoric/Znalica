---
paths:
  - "**"
---

# Consult Knowledge Before Reverse-Engineering a Framework

**The trigger:** you are about to **read an implementation you did not write — its source, its internals — to
learn how a library, platform API, format, regulation or pattern behaves.** It fires the same way on operating
an unfamiliar tool and on an unfamiliar failure or error — mid-task, per sub-question, not on a feeling of
unfamiliarity.

**Consult the curated source first, and read the relevant material in full** — a connected knowledge MCP (query
it and open the matching article; a hit list is not consultation), otherwise the authoritative docs or spec. A
search run to ratify a fix you already derived is not consultation.

**Before you assert how it behaves or edit against it, cite the curated source you read.** **The reply is the
second trigger:** before you send, check each sentence that says how an external library, platform, runtime,
API, format or regulation behaves. Each one carries a curated-source citation or a
`derived-from-source, unverified` tag, including a claim made from memory with no source read. **The usual
failure is a mixed answer:** citing your own code for one part of a chain verifies that part only, so mark where
the verified code ends. **A subagent's "cited" is a lead, not your citation:** open the source before you relay
it. A behaviour you reverse-engineered from source earlier this session is never "already confirmed".

**Silent is not unreachable.** Consulted and genuinely silent → reasoning from source is legitimate; tag the
answer (`task_assessment`). **If a connected tool's description says it files research requests or knowledge
gaps, file the gap once, as a generic request with no client, ticket or personal detail, and name it in your
reply** — `discussion_first` exempts it from a yes. No such tool, as in a read-only agent: name the gap and your
searches in your report, for your caller to file. Unreachable — the server absent or disconnected, the docs
will not load → name it, and try the authoritative docs before you reason from source.

Full procedure: `.claude/skills/knowledge-before-framework-code/SKILL.md` (Copilot: `.github/skills/knowledge-before-framework-code/SKILL.md`).
