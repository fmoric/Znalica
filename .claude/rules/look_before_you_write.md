---
paths:
  - "**"
---

# Look Before You Write

## Rule

Before you write a **literal** or a **block of code**, find what already reads or does it — and use that
instead. When there is genuinely nothing to reuse, write **one** copy — not the first of several.

Three applications of one discipline — **one source of truth for one thing**:

1. **A literal that varies** → find the existing reader. Do not add a second source of truth.
2. **A block that already exists** → find it and call it. Do not write a near-copy.
3. **A block that does not exist yet** → write it once. The moment a second caller appears it becomes one
   thing in one place, not two.

## Test 1 — is this literal a constant, config, or per-install?

Every literal you are about to write gets one question:

> **If two installs existed on one machine, or this value changed, what breaks?**

The answer sorts it into exactly one of three:

- **Correctly a constant** — two installs want the same value forever: write the literal.
- **Should read config** — it varies by environment, but installs do not fight over it: **call the reader that
  already reads it, never re-derive it**; if none does, create the one read here, not the literal.
- **Must be per-install** — a key in a namespace outside this process (an OS service registry, a shared config
  file's top-level key, a path a second install would claim), so two installs **collide**: derive it per install.
  That is a design change — **stop and discuss** first (`discussion_first`).

**Bucket 2 covers the first write too** — "nothing reads it yet" is not a reason to hardcode; it is the moment
the reader gets created.

## A path is a literal too

A path that walks **up** and back **down** into a hardcoded folder name breaks when that folder is renamed.
The folder name is a literal — Test 1 applies; prefer the path relative to the file's own location.

## Test 2 — does this code already exist?

Before writing a function, helper, or block: **search the codebase for it.** If it exists, call it. If it half
exists, extend it. Write a new one only when you have looked and it is genuinely absent — two near-copies are
one unit of work and one future bug. **A document for the user counts too:** find the project's document template,
converter or header convention and write through it.

**Search your own change, not only the repo.** The block you wrote earlier in this same task is not in the
committed code, and **the near-copy you are most likely to create is your own** — a second parser, retry loop
or date formatter. Re-read what this change has already added before you add to it.

**Before you propose building a tool, a component or a test double, search for a ready-made one** as
`answer_before_you_ask` says (the whole repository, then package indexes and the web) and say where.
*"Nothing in `src/`"* is not *"nothing exists"*: build only what that leaves absent.

## Test 3 — nothing to reuse, so write the ONE copy

When there is genuinely nothing to reuse, what you write is a **counting** question, never a feeling:

**One caller** — write it plainly where it is used, with no abstraction for a caller that does not exist. **Two**
(a second call site, *or a second copy inside your own change*) — one thing in one place, which the second caller
calls. **Three or more** — you are already late: extract now.

**Extract at two** — both callers are in front of you and the shape is known. *"This might be needed
elsewhere"* is a prediction, not a caller; wait for the second one.

Name the extracted unit from **what it represents**, not its first caller (`name_from_concept`).

## Scope

Applies to any file you write or edit. Where the value **is** a constant by Test 1, a named constant is correct
and preferred — see `AGENTS.md` §Standards.
