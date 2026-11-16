---
paths:
  - "**"
---

# Name a New Artifact From the Concept, Not the Identifier That Spawned It

## Rule

When you create a **new named artifact that outlives the code around it** — a database field or column, an
enum value, a config key, a class/interface/type, a UI label, a permission set — **derive its name from what
it represents in the domain**, not from the procedure, variable, or call site that happens to spawn it.

**Trigger:** *does the name I'm reaching for carry a word from the identifier I was just editing?* That is a
signal to **stop and check**, not a verdict (`Customer Email` from `customerEmail` is fine): say in one
sentence what the thing **is** to someone who never sees the code, and name it from that.
**Test:** *"If I only knew what this stores — not the code that spawned it — would I still choose this
name?"* A carried word is an accident when it describes a **neighbouring action** rather than the thing
itself.

## When this fires

When you **improvise a name during implementation**, with no design pass that already named it. When a spec
names the thing, implement that name — do not invent one. A new domain artifact with no design decision
behind it signals skipped design: where the packet has a design step (an architect or technical spec), prefer
routing it back there.

Reuse first (whether to create at all — `look_before_you_write`, where shipped); then name the new from the
concept.
