# `look_before_you_write` — rationale

Moved verbatim out of the `look_before_you_write` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Source

Established 2026-07-15 from a hardcoded-values audit of a real project, where three separate incidents in
one month were the same bug in different clothes — each found by tripping over it. The three-bucket test is
that audit's, generalized.

**Test 3 added 2026-08-09.** The packet had a reuse-first check and nothing on the other side of it: Tests 1
and 2 tell you to find what exists, and no rule, agent body or skill said how to write what does not — so
the first copy was written with no thought to the second, and Test 2's own search could not see a duplicate
created inside the same uncommitted change. The count-based trigger is deliberate: *"write reusable code"*
as an aspiration produces speculative generality, which the architect's *"design for the requirement as
written"* already forbids. One caller is not a reason to abstract; two is.

## Field notes moved from the rule body

## The failure Test 1 exists to prevent

**One component reads config, another hardcodes the same value.** This is the shape, and it recurs:

- A server reads its port from `.env` while five scripts hardcode it. The install wizard's port prompt
  becomes a **trap**: choose a non-default port and the server moves, the firewall rule doesn't, start
  waits forever, stop can't find the process. Every script "worked" in isolation.
- The same codebase, one service over: the server honours an `OLLAMA_HOST` env var while the health-check
  script hardcodes `localhost`. Point it at a remote host and the server obeys while the check reports it
  down.
- A dashboard page hardcodes the port in the copy-paste config snippets it hands out — so on a non-default
  port it dispenses config for a port nothing listens on.

And the worst variant — a **machine-global** name treated as a constant: a service name hardcoded in three
files means a second install never registers a service, and uninstalling the second **deletes the first's**.
That is bucket 3.

**Note what the first three have in common:** the right value was **already available**. A generic env
helper already existed. The dashboard page was served *by the very server the browser was talking to*, so
the port was already in `location.port` — no config read needed at all. Nothing needed inventing. Someone
needed to **look**.

**Bucket 3 is the exception, and that is why it says stop and discuss:** a colliding name has no existing
right answer to go find. It needs a scheme designed and existing installs migrated. Looking is not enough.

## Moved at 0.105.0 tightening

**Bucket 2 covers the first-write case too.** "Nothing reads it yet" is not a reason to hardcode — it is the
moment the reader gets created, and it is cheapest here. The alternative is a literal that a later change
has to hunt down across files.

**Why the test and not just "don't hardcode":** everyone already believes "don't hardcode," and writes
`9130` anyway — because at the moment you type it, it *looks* like a constant. The question above is what
makes the difference visible **before** the literal is written.

The inconsistency, where one file
survives a rename and the file beside it does not, is miserable to debug.

Two near-copies are not two units of work — they are one unit of work and one future bug, because the next
change lands in one of them.

The search above reads what is already committed; the block
you wrote twenty minutes ago in this same task is not in it. **The near-copy you are most likely to create
is your own** — a second parser, a second retry loop, a second date formatter, written because the first
one had scrolled out of sight.

**Extract at two, because two is when it is cheap.** Both callers are in front of you and the shape is
known. At four it is a cross-file refactor nobody schedules, and by then the copies have drifted — which is
the future bug Test 2 names, arriving on time.

**"Reusable" is not something you can feel, which is exactly why the trigger is a count.** *"This might be
needed elsewhere"* is a prediction, and building for it produces machinery for callers that never arrive.
Wait for the second one. It is a fact, and it comes soon enough.

Both send you to look at the repo, and on the same edit both can fire — the mechanical act ("read the
relevant source file") is shared. They differ in what they do with the answer, and each owns its half:

- **`no_fabrication`** — do not *claim* a path, symbol, or config key exists unless you verified it.
  Guards against citing what **is not** there.
- **This rule** — before writing a literal or a block, find what **is** there and use it; where nothing is,
  write one copy rather than several. Guards against re-creating what already exists.

## Adopted from a delivered project (0.178.4)

The passage *Before you propose building a tool, a component or a test double, search for a ready-made one* (adapted; the project's version listed the sources itself, which `answer_before_you_ask` now defines once) was written by a delivered project for its own copy of this rule (BE-HUB-System, 2026-11-03), and its user asked the library to take it. It was never in the packet before. It applies the reuse test to the decision to build at all: a search of `src/` alone reads the same whether a ready-made tool exists or not.
