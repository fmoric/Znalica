---
name: external-tool-state-change
description: Use before installing, upgrading or removing software outside the repository, changing a user-level config or registration, logging in or switching accounts, or stopping a shared process. The full look-before-you-change procedure, its exemptions and their limits.
---

# Look Before You Change State Shared Beyond This Repository

*The always-on `external_tool_state_change` rule carries the trigger and the decision. This is its full text, moved here verbatim in 0.139.0 so it loads only when needed.*

## Rule

**The trigger:** *am I about to change state that is shared beyond this repository?* Four kinds —
illustration, not definition:

- **Software** — installing, upgrading, uninstalling, reinstalling a tool outside the repo.
- **Registrations and configs that live outside it** — a user-scope MCP server, a global `git config`,
  `~/.claude/settings.json`, an editor extension.
- **Credentials and identity** — `gh auth login`, `az login`, `docker login`, switching an account.
- **Something running, or somewhere shared** — killing a process or service, a shared dev database, a shared
  cluster or cloud environment, permissions on a path other people use.

**Any MCP server change — the project's own `.mcp.json` too, which does not otherwise fire this rule — runs
the `mcp-server-setup` skill first**; registering is not installing.

**The test: could something outside this repository behave differently afterwards, for someone I cannot
see?** If yes, do these four things first. **None of them changes anything.**

1. **Consult the curated source for that tool** — the knowledge MCP if one is connected, the authoritative
   docs otherwise — *before* you compose the command, not to ratify one you already wrote. **No session
   cache**: reading about the tool earlier does not cover how *this operation* behaves.
2. **Inspect the current state.** `uv tool list`, `winget list`, `npm ls -g`, `claude mcp list`,
   `copilot mcp list`, `gh auth status`, or the tool's own `--version` — **matched to the tool you are
   actually running**; the two CLIs are not interchangeable. **If the thing is already there, the install
   step is already done — do not do it again.**
3. **Check who else is using it.** Other agent sessions, other projects, other people's terminals, processes
   holding its files or its port. A per-user install is **shared by every session on the machine**, and none
   of them can see each other.
4. **Stop and ask if either is true**, and wait for an answer:
   - **something already there would be overwritten, repointed, removed, or interrupted** — *or* —
   - **step 3 found another live consumer**, whatever the command does.

   Say what exists now, what the command would leave behind, and why you believe the change is needed.

**Step 4 is not keyed on a version number.** A same-version reinstall, a `--force`, a config overwrite, a
re-registration under an existing key and an uninstall all replace something while changing no version. The
test is whether something that exists now would be different afterwards.

**What step 4 does not gate:** a genuinely first-time install of something absent, with **no other live
consumer found in step 3**. Steps 1–3 still apply.

## When this fires

**Immediately before you run the command, once you know what the command is** — not at the start of the task,
not after.

**Writing the command counts as running it.** Adding `uv tool install …` to a `postCreate` script, a
Dockerfile, a Makefile target, an npm `postinstall`, or a CI step is the same decision, deferred to machines
you will never see. Apply steps 1–4 when you write it.

## What this does not fire on

- **Project-scoped installs.** `npm install` filling `node_modules/`, a project `.venv`, `dotnet tool
  restore` against an in-repo manifest.
- **Filling a content-addressed cache** as a side effect of one — npm's, uv's, NuGet's. **A mutable shared
  name does fire**: `docker pull node:20` retags an image every other project resolves, and removing a shared
  volume destroys their data.
- **A disposable environment you own** — a container, an ephemeral CI runner, a throwaway VM with no other
  session in it. If step 3 genuinely finds no other consumer, step 4 has nothing to stop; do not wait for an
  answer nobody is there to give — say what you did.

**Provisioning repos — a property of the PROJECT, never of the task.** Where provisioning is what this
repository is *for* (infrastructure-as-code, a toolchain or environment-setup repo), **step 4 softens to
disclosure for the environments that repo provisions**: name what you are replacing, then proceed. **Quote
the line in `AGENTS.md` you are relying on**; if you cannot, or `AGENTS.md` says nothing about provisioning,
step 4 applies in full. Limits:

- It **never** covers the host you are running on — `winget upgrade`, a global tool install, an edit to
  `~/.claude/settings.json`.
- **Steps 1–3 always apply.**
- Do not reach for it because *the task at hand* is about tooling, such as registering an MCP server.

**Neither exemption covers a target carrying a production signal.** A throwaway container holding a
production connection string is a disposable *environment*, not a disposable *target*; a provisioning repo's
disclose-and-proceed covers a prod apply going out the released way, **not an apply of a change you just
wrote, from your working copy, to a production-signalled workspace.** `production_is_read_only` governs that
write, and it does not soften.

## What an upgrade costs even when it works

A successful upgrade still invalidates everything pinned to the old release — knowledge-base articles, notes,
a `verified:` date in a config — silently. Treat *"upgrade a tool this project's knowledge documents"* as a
decision to put to the user, not a maintenance detail to fold into a larger task.

*Rationale and neighbouring rules: `docs/rules-rationale/external_tool_state_change.md`.*
