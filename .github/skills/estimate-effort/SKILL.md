---
name: estimate-effort
description: Use for an effort estimate in hours, a quote or a procjena for a ticket.
---

# Estimate Developer Effort

Size the work an experienced developer would bill, not the work a newcomer would need. A padded estimate is
not a safe one: it is a wrong figure that someone must argue down. Every rule below answers a way real estimates
have been padded to two or three times the work.

## 1 — Read before you size

Read the objects the work touches, and the technical spec if one exists. Many costs are facts only the code
or the design shows: whether a translation file for that language exists, whether a field or hook the spec
needs is already there, whether the design computes a value live instead of storing it.

**Standard features and existing code first.** Before you size custom code for a need, check what the
platform's standard features and this app's existing code already do for it — setup, standard events, a
subscriber already on the same event — and size only the gap. A spec's statement that the standard cannot do
something is a claim, not a fact: check it against the curated source first and the platform's own code only
after, as the `knowledge-before-framework-code` skill describes, and name what you checked.

If you size without that reading, say so on the figure itself — **"before code read"** — and name what you did
not read. Never let that figure stand as the estimate once the code has been read.

## 2 — List the artifacts first

Before any hours, list what will be built, one line per artifact: a field, a key, a procedure, a page column,
an event subscriber, a test step. **Each line names the spec point it serves** — a point of the functional or
technical spec, or, where there is no spec, the sentence of the ticket that asks for it.

- **No line without a spec point.** Work a spec point cannot work without — a backfill so existing records
  pass a new mandatory field — is a line under that point. A backfill, migration or clean-up nothing in the
  spec needs is not in the estimate, and is not raised as a question either (§3).
- **No line for work the code or design makes unnecessary** — no translations when no target-language file
  exists, no clearing of a value the design computes live.

## 3 — Size each line

- Size for an **experienced C# / Unity developer who knows this codebase**: no learning margin, no
  "figure it out" time. Not knowing the language is not billed.
- **Never price or ask about a requirement the spec is silent on.** A feature, option or behaviour the spec
  does not mention does not happen: no line, no `+N h if …` add-on, no question to the client about it.
- **How to build what the spec does ask for is a different thing.** A technical unknown — which event to
  subscribe to, which send path to use — is the developer's to settle. Size the time to settle it once, in the
  analysis phase (if the project's phases have none, as its own line, `settle: <unknown>`), and size the build
  on the branch the code reading makes likely, not the cheapest one. Never fold the unknown into a step's
  hours, and never show it to the client as an add-on or a question.
- **An add-on line only for an option the spec itself names**, sized by the same method — an option is not a
  place to put back the margin removed from the base.

## 4 — Your figure, not one you were handed

A figure in the request, in an earlier message or in another agent's output is not evidence. You may compare
your result against it and say why they differ; never adopt it, and never "confirm" it without sizing.

**Never put a figure in a prompt you delegate** to another agent — not "current estimate N h, confirm or
correct". Hand over the spec and let the work be sized.

## 5 — Group into billing phases

Group the lines into the phases this project bills by, as given at delivery:

Design (technical analysis and design, including settling technical unknowns) · Build (development, including the developer's own testing) · Test (playtesting and fixes)

Work that is not billed stays out of the total; name it once below the table if the reader would expect it.

## 6 — Check before you report

- **Any single-artifact line over 1 h states why** — what makes this artifact more than an hour for an
  experienced developer. If you cannot name one, the hours did not come from the artifact: re-size the line
  to what that artifact needs, or, if it really covers several, split it and size each.
- **A bundled line** (the developer's own testing, a deployment) **names what it contains** — e.g. "phone and
  camera run, two-user test under the debugger".
- Every line still names its spec point, and every add-on line names the spec option it prices.

Report the table (artifact · spec point · hours), any add-on lines, the phase totals, and what you read.

## When the figure changes

- **The technical spec lands** → re-size from the artifact list against the design. Do not carry the earlier
  figure over: the design often makes a line trivial or removes it.
- **Someone says "that seems a lot"** → recompute from the list, line by line. With no new fact, a line moves
  only where the recompute finds an error in it — name the error, and correct it. Never shave the total: the
  challenge itself moves nothing, only a named error does.
- **A new fact arrives** (a field turns out to exist, the scope changes) → re-size the lines it touches, as
  many times as facts arrive.
