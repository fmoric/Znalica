---
paths:
  - "**"
---

# Look Before You Change State Shared Beyond This Repository

**The trigger:** *am I about to change state that is shared beyond this repository?* — software outside the
repo (installing, upgrading, uninstalling, reinstalling); a registration or config that lives outside it (a
user-scope MCP server, a global `git config`, `~/.claude/settings.json`, an editor extension); credentials or
identity (`gh auth login`, `az login`, `docker login`, switching an account); something running or shared (a
process or service, a shared database, cluster or cloud environment, permissions on a path other people use).
**The test: could something outside this repository behave differently afterwards, for someone I cannot see?**

**When:** immediately before you run the command, once you know what it is. **Writing it into a `postCreate`
script, a Dockerfile, a Makefile target, an npm `postinstall` or a CI step counts as running it.**

**Any MCP server change — the project's own `.mcp.json` too — runs the `mcp-server-setup` skill first**;
registering is not installing.

**If yes, do these four things first. None of them changes anything.**

1. **Consult the curated source for that tool** — the knowledge MCP if one is connected, otherwise the
   authoritative docs — before you compose the command. Reading about the tool earlier does not cover how this
   operation behaves.
2. **Inspect the current state** with the tool's own list, status or `--version` command, matched to the tool
   you are actually running. **If the thing is already there, the install step is already done.**
3. **Check who else is using it** — other agent sessions, other projects, other people's terminals, processes
   holding its files or its port. A per-user install is shared by every session on the machine.
4. **Stop and ask, and wait,** if something already there would be overwritten, repointed, removed or
   interrupted — **or** step 3 found another live consumer. Say what exists now, what the command would leave
   behind, and why the change is needed. **Not keyed on a version number:** a same-version reinstall, a
   `--force`, a config overwrite, a re-registration and an uninstall all replace something.

A genuinely first-time install of something absent, with no other live consumer found, needs no answer — steps
1–3 still apply.

**Read `.claude/skills/external-tool-state-change/SKILL.md` (Copilot: `.github/skills/external-tool-state-change/SKILL.md`)** for what does not fire
and the limits of each exemption. **No exemption covers a production-signalled target** — `production_is_read_only` governs that write.
