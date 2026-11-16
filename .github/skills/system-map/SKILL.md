---
name: system-map
description: Use when the system map is missing or stale, or after a change that moves structure — maps components or business processes into Markdown, system-map.json and an HTML page.
---

# System Map

A map an agent reads at the start of a session saves it re-learning the system every time; a page a person
can click through shows them the same thing; a JSON copy lets an agent or a tool ask "what depends on X?"
exactly. **All three are one map, written once** — the Markdown is the source, the JSON and the HTML are
drawn from it, and all three say which commit they describe. **A map that is wrong is worse
than none**, so it states what it could not see, and it never claims a link it did not read.

## 1 — Find what already exists

- **A pointer in `AGENTS.md`** to a map or a map generator — follow it, and keep its paths.
- **A map file** — `docs/**/system-map*`, `docs/**/SYSTEM_MAP*`. Read its built-at commit.
- **A generator** — a script that writes the map (e.g. `scripts/*system_map*`), usually with notes written
  by hand beside it (purpose lines, flows).

**A generator exists** → run its staleness check if it has one (a `--check` flag, or what `AGENTS.md`
says), and regenerate if it reports stale. Its output is the map; do not hand-edit generated files — edit
the hand-written notes it reads, then regenerate. **If it writes no `system-map.json`** (generators built
before the JSON existed do not), offer to extend it so it writes one to §4's core, covered by its `--check`,
and do it on a yes. Until then, report the JSON as missing; never write it by hand beside generated files,
where the next regeneration leaves it behind. Report and stop.

**Only a hand-written map exists** → refresh it in place (§2–§4), keeping its path and structure. **A map
with no `system-map.json` beside it is stale** however recent its commit: write the JSON in the same
refresh, beside the Markdown.

**Nothing exists** → build it (§2–§4) at `docs/system-map.md`, `docs/system-map.json` and
`docs/system-map.html`.

## 2 — Decide what this repository maps

- **Code** → its components (modules, packages, apps, services — whatever unit the code is organized by).
- **Specs and processes** (a consultant-style repository: functional specs, process descriptions, no code)
  → the business processes, the specs that cover each one, and how they hand off to each other.
- **Both** → map the code, and link each component to the specs it implements where the specs say so.

## 3 — Read, structure first

For code, use the structural tools this session has before text search — a code-navigation server for
symbols and references, a language server, a call-graph tool — matched by what they do, not by name. Text
search finds names, not which object a reference resolves to. A dependency you could not resolve is listed
under *what this map cannot see*, not guessed.

For specs and processes, read the specs and the domain reference; a link between two processes is one a
spec states, never one inferred.

## 4 — Write the map

**`docs/system-map.md`** (or the existing map's path), in this order:

1. **Header** — what was mapped (**the source roots, as paths**), counts (components, files read), and
   **built at `<commit>` on `<date>`**. The roots and the commit together are what staleness is judged by:
   source commits under those roots since that commit.
2. **How to use this map** — three or four lines: find the component, open what it depends on, run what
   tests it.
3. **What this map cannot see** — mandatory. Runtime dispatch, events raised by a platform, reflection or
   calls by name or number, anything outside the repository, names that matched more than one thing.
4. **Components** (or **Processes**) — one table: purpose (one line, plain language), size, **depends on**
   and **used by** with reference counts, **tested by** (or, for processes: specs, and each spec's status).
   If the project enforces a dependency direction (an import linter, layer rules), list those rules next to
   the table, so the allowed direction sits beside the actual one.
5. **Interfaces and their implementers** — each interface (or abstract contract) and what implements it.
   **An interface with no implementer is a finding**, not a blank row. Omit the section where the language
   has none.
6. **Runtime view — what runs where.** **Required when the code runs in more than one process, thread or
   device**; otherwise one line: *single process — no runtime view*. A dependency graph shows what imports
   what; it cannot show that one thread writes the database while another talks to an outside service,
   which is what a reader needs before changing anything.
   - One table per place (in the program · on the machine · outside it): each part — thread, process,
     store, device, outside service — with what it does, its code, its **status (built · not built yet)**
     and the spec that decides it, where one does.
   - A *what talks to what* table: from, to, how, status.
   - This view is written by hand, so **check every code name in it against the source**; a row naming
     code that does not exist is a finding, not a row.
   - **A planned part or link is shown, marked planned, and never counted as a dependency.**
7. **Main flows** — the few paths that matter most, each step naming a real component (and, where it
   matters, the thread it runs on). **Where a spec fixes a flow's order, cite it and say the order is
   binding.**
8. **Entry points and integrations** — what calls in from outside, and what this calls out to.

**`docs/system-map.json`** — the components and their links, for an agent's exact query or another tool.
**This core is fixed by the template**, as `roadmap.json`'s is, so any two delivered projects can read each
other; a project may add fields, never change the meaning of these:

- top level: `schema` (integer; this is `1`), `project`, `commit` (the same built-at commit as the
  Markdown's header), `generated` (date), `roots` (the mapped source roots, as paths), `read_with` (the
  structural tools the reading used — a count from text search alone is a weaker claim, and the reader
  must be able to tell), `cannot_see` (the Markdown's *what this map cannot see*), `components`; optionally `interfaces`;
- each component (or process): `id`, `title`, `kind` (`component` or `process`), `purpose` (the one line),
  `paths` (where it lives; for a process, the specs that describe it), `depends_on` (a list of
  `{id, refs}`: `refs` is the reference count, or for a process the number of specs that state the
  hand-off); optionally `parent` (an `id`, for a component inside another),
  `tested_by` (paths), `specs` (a list of `{path, status}`), `implements` (interface `id`s);
- each interface: `id`, `path`. An interface no component `implements` stays listed — the Markdown's
  *interface with no implementer* finding must survive the copy.

**`used_by` is not stored**: it is `depends_on` read backwards, and a file that holds both can disagree
with itself. The runtime view and the main flows are not in the core; a project may add them. A planned
part is never a component here, as it is never a dependency in the Markdown. A change to the core bumps
`schema`; tell anyone known to read the file before it lands.

**`docs/system-map.html`** — the same content as a page a person clicks through:

- **Self-contained**: inline SVG and script, no external fonts, scripts or stylesheets, so it opens
  offline from a checkout and shows exactly what the Markdown says.
- A component graph where selecting a component highlights what it depends on and what uses it, and shows
  its row from the table; the flows as a list beside it; the built-at line and *what this map cannot see*
  visible without scrolling.
- The runtime view, if there is one, drawn with planned parts and links dashed and greyed.
- Readable in light and dark mode.

Regenerate all three together. **One without the others is a stale map with a fresh-looking twin.**

## 5 — Offer a generator (ask; never set one up unasked)

Reading by hand works on day one and in any language, but nothing tells you when it has gone stale. When
the code is large, or the user wants staleness caught automatically, **offer** to write a generator for
this project:

- a script that reads the source and writes all three files;
- the parts only a person knows — purpose lines, the main flows — in a small hand-written notes file the
  script reads, with each object a flow names checked against the source;
- a line in `AGENTS.md`: read the map first, and regenerate it in any change that moves structure.

**How a generator is held to its output** (the same in the `roadmap` skill):

- `--check` writes nothing and exits non-zero naming each stale file. **A source it could not read exits
  with a different code** — "current" is never claimed over a source nobody read.
- A test in the project's suite runs `--check`, so CI fails on a stale output — not only a check someone
  must remember at pull-request time.
- A self-test runs the generator on small invented inputs against a known-good output.
- `AGENTS.md` names the command and the kind of change that requires it.
- Optionally, a `--fragment PATH` mode writes the page without its document skeleton, for publishing it
  inside another page.
- When the project has both a map and a roadmap generator, they share the page code; write it once.

Write it only on a yes. The library cannot ship one: what a "component" and a "reference" are depends on
the language.

## Report

Say where the map was written, the commit it describes, the counts (components, files read, links
unresolved), which structural tools the reading used, and whether the JSON and the HTML were regenerated
with it — the JSON's `commit` equal to the Markdown's, or why there is no JSON (a generator not yet
extended). Say whether a runtime view was required, and how many of its code names were checked against
the source. **A count is only as good as the tool behind it:** "0 links unresolved" from text search alone
is not the same claim as from a language server — say which it was. If the map was **not** refreshed — a generator
failed, a structural tool was missing — say which, rather than reporting the old map as current.
