---
name: roadmap
description: Use when the roadmap is missing or stale, or after planning changes — builds Markdown, roadmap.json and an HTML page from the tracker, specs and git; never invents a date or state.
---

# Roadmap

A roadmap answers two questions for someone who was not in the room: **where is this going, and where does
each piece stand?** It is only worth having if every line can be checked, so **every item links the source
that says so**, and anything no source says is asked, not written.

## 1 — Find what already exists

- **A pointer in `AGENTS.md`** to a roadmap or a roadmap generator — follow it, and keep its paths.
- **A roadmap file** — `docs/**/roadmap*`, `docs/**/ROADMAP*`.
- **A generator** — a script that writes the roadmap, usually from hand-written notes beside it (themes,
  phases, milestones). **A generator exists** → run its staleness check if it has one, regenerate, report,
  stop; edit its notes, never its output.

Otherwise write `docs/roadmap.md`, `docs/roadmap.json` and `docs/roadmap.html`. **Not `ROADMAP.md` at the
repository root**: the `initialize` and `housekeeping` skills probe that name for the project's tracker, and
a roadmap there would be read as the backlog.

## 2 — Gather the sources

- **The tracker** — resolve it exactly as the `initialize` skill's Step 0 does (same candidates; exactly one
  that exists is the tracker, two or more → ask which is authoritative). Open items, and their markers for state.
- **Specs on disk** — functional and technical specs, and any status or grade they record.
- **Git** — what has landed: merged branches and the commits that closed items.
- **The system map**, if there is one — to group items by the component they touch.
- **A hand-kept plan file**, if there is one (e.g. `docs/roadmap-plan.toml`) — target dates and release
  conditions (*X ships when A and B are done*). Dates come from here or from nowhere.
- **Another project's `roadmap.json`**, when this roadmap covers more than one system (§4).

Count what each source gave you; a roadmap built from a tracker you could not find says so at the top.

## 3 — Shape it to the repository

- **Code** → **themes** (what the work is for) → **phases or milestones** (in order) → **items**.
- **Specs and processes** (a consultant-style repository) → the **requirements pipeline**: each spec and
  where it stands — *draft · graded (grade) · handed to the developer · built* — grouped by business
  process. A grade is often given in conversation and recorded nowhere, and *built* usually happens in
  another repository: take either from a source that says so (a grade recorded in the spec, the user's
  answer with its date), and otherwise **ask** — a spec with no recorded grade is *draft* only if the user
  confirms it.
- **Both** → the pipeline for requirements, the themes for the code, and links between the two where a spec
  names its implementation.

**States are a closed list, written at the top of the file.** For code items:

| State | Means |
|---|---|
| `exploring` | the problem is real; the shape is not known |
| `planned` | agreed; not started |
| `designed` | its spec is graded; nothing built yet |
| `in progress` | someone is working on it — named |
| `blocked` | waiting on something — named |
| `done` | landed — linked to the commit or changelog entry |
| `deferred` | not now; may return |
| `rejected` | decided against — linked to the decision |
| `superseded` | replaced — by what, linked |
| `unknown` | **no rule could classify it** — listed, never guessed |

`deferred` and `rejected` matter most: without them a dropped item is deleted, and the next person
proposes it again. Use only the states this repository needs, and write that list at the top.

**How a state is decided.** Each state comes from a rule over the sources (*a checked box in the tracker →
`done`*; *a graded spec with no landed commit → `designed`*), first match wins, and an item no rule matches
is `unknown`. A state that the rules get wrong is corrected by an override in the notes file, never by
editing the output.

**An item that only groups others** (a phase that is just its steps) is marked as a container and **not
counted** in any total.

## 4 — Write it

**`docs/roadmap.md`**: a header with the date, the commit, the sources read (with counts) and the state
list; a summary table of items per state; then the themes or processes with their items. Each item: one
line, a stable ID (never reused), its state, and **a link to its source** (tracker line, spec file,
commit). Then:

- **Dates** — a phase's started and finished dates are the earliest and latest dates of its items; target
  dates and release conditions come from the plan file. With no plan file: *no target date yet*.
- **Decided** — decisions, each with the source that made it.
- **Rules** — each rule, what it means, and how many items it matched, so a reader can see why an item has
  its state.
- **Warnings** — every notes entry that matched no item, and every `unknown` item.

**`docs/roadmap.json`** — the same items for another tool or project to read. **This core is fixed by the
template**, so any two delivered projects can read each other; a project may add fields, never change the
meaning of these:

- top level: `schema` (integer; this is `1`), `project`, `commit`, `generated` (date), `states` (the list in
  use), `items`;
- each item: `id`, `title`, `state` (one of `states`), `source` (link or path); optionally `parent` (an
  `id`), `container` (true for an item that only groups others), `requires` (a list of `id`s).

A change to the core bumps `schema`; tell anyone known to read the file before it lands.

**Combining roadmaps.** A roadmap may include another project's `roadmap.json` — read from a pinned
commit, and say which. **That project's states are its own: show them, never re-derive them.** Refer to
its items as `<project>:<id>`.

**`docs/roadmap.html`** — the same content as a page: self-contained (inline SVG and script, nothing loaded
from outside), phases or the pipeline drawn left to right, items filterable by state, each item's source
one click away, readable in light and dark mode. Regenerate all three together.

## Never invent

- **No date** that no source gives. "Planned" is a state; a quarter is a claim. A date in the plan file is
  a source; a date you estimated is not.
- **No state** that no source gives. An item the tracker marks open is not "in progress" because a branch
  exists — say both facts instead. An item no rule classifies is `unknown`, not your best guess.
- **No decision.** A direction nobody wrote down is a question for the user: ask it, and write the answer
  with its date and who gave it.

## 5 — Offer a generator (ask; never set one up unasked)

When the roadmap will be refreshed often, offer a generator: a script that reads the tracker and specs and
writes all three files, with the hand-written parts (themes, phase order, milestones, overrides) in a small
notes file.

**How a generator is held to its output** (the same in the `system-map` skill):

- `--check` writes nothing and exits non-zero naming each stale file. **A source it could not read exits
  with a different code** — "current" is never claimed over a source nobody read.
- A test in the project's suite runs `--check`, so CI fails on a stale output — not only a check someone
  must remember at pull-request time.
- A self-test runs the generator on small invented inputs against a known-good output.
- `AGENTS.md` names the command and the kind of change that requires it.
- Optionally, a `--fragment PATH` mode writes the page without its document skeleton, for publishing it
  inside another page.
- When the project has both a map and a roadmap generator, they share the page code; write it once.

Write it only on a yes.

## Report

Say where the roadmap was written, the sources read with their counts, the items per state (containers not
counted), how many items are `unknown`, and every question you had to leave open instead of answering.
