---
name: knowledge-exchange
description: Use before writing a spec, brief or pattern that may already exist (search a connected knowledge store), and after one passes review (contribute it, with consent). No-ops with no store.
---

# Knowledge Exchange

Close the reuse loop with whatever **searchable knowledge store** this environment exposes: **search it before
you author** so you don't redo work that exists, and **contribute reviewed work back** so the next task starts
ahead. Two independent steps — a caller uses one or both.

**Tool-agnostic by design.** Never assume a specific server (e.g. `knowledge`). Match a tool by its
**description** at call time — it works the same whether the store is an internal knowledge MCP, a company
wiki, Confluence, Notion, or none. If nothing matches, **say so and continue** — this step never blocks the
real deliverable (the `docs/` file).

## Step SEARCH — reuse before you author

Use at the **start** of authoring (a spec, a research brief, a design), to find prior work to build on.

1. **Find a search tool by capability** — among the tools available this session, pick one whose description
   is *searching/querying a knowledge base or store*. Match on purpose, not name.
2. **Query** it with the topic/problem terms. **Don't repeat a query** — the `tool_retry` rule caps that at 2
   attempts per tool and topic, and a rephrased query is the same topic; after both fail, say the store was not searched — not that nothing was found.
   **Keep the total to a handful of focused queries, not a sweep** — no rule enforces this one; it is your
   judgment. Surface the top hits — titles + links — to the user.
3. **Decide reuse, with the user** — if a prior spec/brief covers this, **start from it** (adapt, don't
   reinvent); if partial, note what's reusable and what's new. If nothing relevant, or no search tool exists,
   **say so and continue** — authoring proceeds normally.

## Step CONTRIBUTE — publish reviewed work back

Use at the **end**, once the work has met its quality bar (a spec reviewed to grade ≥ B; a research brief
verified, not a draft). The **caller** decides when that bar is met — this step just performs the contribution.

1. **Find a contribute tool by capability** — pick one whose description is *proposing/adding an article or
   note to a shared store*, **preferring a staging/review path** over one that indexes immediately.
   **Searchable-store-only:** the target must be a store the next task can *search back*. Operational test:
   accept it **only if a search-capable tool for the same store is also discoverable this session** (the
   SEARCH tool above). A propose/publish tool with **no** paired search tool is treated as **publish-only**
   (an Artifact-style page, a write-only note sink) → **skip it**. Nothing valid → say so, point at the
   `docs/` file, stop.
2. **Discover-and-conform to the store's format** — before formatting the submission:
   - If a tool exists that *lists or fetches how-to guides / authoring instructions*, call it; if a guide on
     *writing or contributing an article* exists, fetch it and follow it.
   - Read the contribute tool's **input schema** at call time (e.g. it may want `content_base64`, or a
     required `doc_type`/category — pick the fitting one).
   - If neither yields a template, fall back to a **sensible default article structure** (title · summary ·
     body · sources/links). Never block on a missing template.
3. **Show, then get explicit consent** — present exactly *what* you'll submit (title, doc_type, a short
   content preview) and *to which tool* — it writes to a **shared store**. Proceed only on an explicit **yes**.
   No → keep the `docs/` file, stop.
4. **Submit and report** — call the tool; report the outcome (staged for review / accepted / returned id).

## What good looks like

- Nothing is reinvented that the store already holds; nothing half-baked is published to it.
- The store target was **discovered by capability**, so this works in any delivery — or no-ops cleanly when no
  knowledge tool is connected.
- Every contribution was shown to the user and explicitly approved before it left the machine.
