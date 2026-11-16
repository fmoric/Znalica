# `task_assessment` — rationale

Moved verbatim out of the `task_assessment` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Relationship to the plan-quality gate

This gate fires **first** — at task start, before any plan exists (it decides which *tool*). The
`plan_quality_gate` fires **after** — once a plan is formed, it decides whether the plan is ready to build (it
grades the *plan*). Sequential, not alternatives.

## Why

Built-in tools have zero friction; a specialized tool requires a conscious choice that is easy to skip. The
fix is tool-agnostic — read the descriptions of what's available. Descriptions are the routing table.

## Field notes moved from the rule body

### A tool you expected and do not have — find out WHICH kind of absent before you prescribe anything

**Nothing below applies to a tool you HAD.** If it was loaded and you did not use it, none of the three
branches fits: nothing is broken, there is no remedy to name, and the tag itself is the whole obligation.

**Check your own agent definition first, `.mcp.json` second.** That order matters and is not the obvious one.
Your `tools:` list is the authoritative statement of what you were *granted*; `.mcp.json` only explains where
a granted server was supposed to come from — which is what decides the remedy. Reading them the other way
around produces a confident wrong answer, and prescribing the wrong remedy is worse than saying nothing.

- **Not named in your own agent definition's `tools:`** → **it was withheld from you on purpose.** Nothing is
  broken and there is nothing to install; whether `.mcp.json` mentions it or not is irrelevant here. Say what
  you cannot do and why, and **do not name a remedy** — the fix, if one is wanted, is a scoping decision for
  whoever maintains the packet, not a command for the user to run:
  > *"No callers found — **text search only**. `serena` is configured for this project but deliberately not
  > granted to this agent, so I cannot follow a reference that isn't a literal string match."*
- **Named in your `tools:`, present in `.mcp.json`, and still not loaded** → a **team** server that did not
  come up on this machine. **Add the remedy to the tag**, naming the server key and what to run:
  > *"No callers found — **text search only**; event subscribers bound by a string key are invisible to it.
  > `serena` is configured in `.mcp.json` and granted to this agent but not responding on this machine —
  > likely not installed here. `uv tool install serena-agent`, then restart the session (on Copilot CLI,
  > `/mcp reload` or a restart)."*
- **Named in your `tools:`, absent from `.mcp.json`** → a **personal** server. This is a real configuration,
  not a mistake: the delivery interview offers team-vs-personal routing, and a server the user already ran is
  deliberately reused from their **user-level** config instead of being duplicated into the repo. So the
  remedy is user-level too, and **`.mcp.json` will never mention it no matter how correctly it is set up.**
  Say that, rather than "this project never had it":
  > *"`serena` is granted to this agent but not registered on this machine. It is a personal server — it
  > lives in your user-level MCP config, not the committed `.mcp.json`, so nothing in the repo will add it.
  > `MCP-SETUP.md` has the procedure."*

**Naming the remedy is this rule's job; running it is not.** Both branches above hand the user something to
do — an install command, or a procedure to follow. Before you run one **yourself**,
`external_tool_state_change` applies: the tool may already be present under another name or path, another
session may be using it, and an install is a machine-global change. **Announcing a missing capability must
not become the thing that removes one for somebody else.**

**Where to look:** a delivered agent's definition is `.claude/agents/<name>.md` or
`.github/agents/<name>.agent.md`, and its frontmatter `tools:` **is** the allowlist — the delivery writes
each granted server into it on both tools. **This discriminator is for a sub-agent running under a defined
tool list.** A main session has no agent definition and receives every enabled server, so the first branch
does not arise for it.

**Why the withheld case is common, and why it does not look like one.** Per-agent scoping is how this packet
constrains what an agent can do — a read-only reviewer is read-only *because* the write-capable servers were
kept out of its list. So the config naming a server the agent does not have is the design working, not
failing. From the inside, that is indistinguishable from a broken install unless you look at your own
definition.

**Why the team case is common enough to name.** `.mcp.json` is **committed**, so it travels with the repo —
but the install does not. Whoever ran the original setup was asked for consent and installed the runtime;
**every teammate who clones afterwards gets the configuration and none of the software.** Nothing re-runs
that setup for them, and every static check passes, because the config is correct — it is the machine that is
missing.

**Say what makes it attach.** Claude Code binds MCP servers **at launch**; installing mid-session does not
attach the server to the session you are in, so say "restart the session." On Copilot CLI say "run `/mcp
reload`, or restart the session." Copilot 1.0.93 or later applies an MCP *configuration* change between turns
without a restart (its changelog), but the install remedy changes no configuration — the config was already
correct — so that entry does not cover it. Without that sentence the reader installs it, sees the tool
still absent, and reasonably concludes the fix did not work. *(Until 0.148.0 this said both CLIs bind at
launch.)*

**Why the tag and not just the notice:** a notice at the start scrolls away, and the wrong answer arrives
later looking like a fact. The tag travels with the finding, so it is present at the moment someone acts on
it. It also needs no memory of what you already said — which is why it is the part that must not be skipped.

## Moved at 0.105.0 tightening

> **That last one is the one with no error trail.** A missing server surfaces somewhere — an empty tool list, a
> failed call. A specialized tool you simply did not reach for leaves nothing behind at all: the reader sees
> *"no callers found"* with no way to tell it came from `grep`.

> **`report_what_you_examined` is this rule's other half**, and the pair is one obligation: a report line
> carries its scope. This one names what you could **not** see; that one names what you **did** — the count,
> the path, the terms. Most degenerate results want both clauses in the same sentence, so write both rather
> than deciding which rule you are in.

> ✅ *"No callers found — **text search only**; `serena` was loaded and I did not use it, so a reference
> bound by anything other than a literal string match is invisible here."* — the present-and-unused case,
> and it reads the same because the blind spot is the same.

> **Naming the blind spot is the whole point.** *"Degraded"* tells a reader to be vaguely uneasy. *"Text search
> cannot see event subscribers bound by a string key"* tells them exactly which conclusion is unsafe and what
> would settle it. If you cannot name what the fallback misses, say that too — *"I don't know what this misses"*
> is honest and still useful.

> **A stricter local rule wins.** If this project sets a rule that says **stop and wait** for a specific
> capability, that rule governs — it was written by someone who knows what that gap costs here.
