---
name: mcp-server-setup
description: Use before registering, enabling, installing or configuring an MCP server for this project (Claude Code or Copilot CLI) — registering is not installing; read MCP-SETUP.md before composing any command.
---

# Setting Up an MCP Server — Read the Packet's Procedure First

`external_tool_state_change` governs any command that changes state shared beyond this repository — consult
the curated source, inspect current state, check other consumers, stop before replacing. **All of it applies
here.** This skill adds the two things that are true of MCP servers specifically.

**1. Registering is not installing.** Pointing a configuration at a server binary that is already on the
machine is configuration work. It needs no install, no upgrade, and no version change. *"Set up an MCP
server for X"* is most often a registration request, and reading it as an install request is what turned a
one-line config edit into an uninstalled tool on 2026-07-31.

**2. Read `MCP-SETUP.md` in the project root before composing any command.** It is generated at delivery from
the packet's own server definitions and has a section per server the packet declares, carrying what the
packet has verified: a preflight check, whether to reuse an existing install rather than add a second, an
install command, a setup shortcut, a per-project config step.

- **Coverage varies by server** — some sections have all of that, some little more than a name. A section
  that names a server without giving a command is the normal case for several.
- **Match the section by what the server is, not by the key your `.mcp.json` happens to use.** Sections are
  headed by the packet's declared key; a project may run the same server under a renamed one.
- **It covers servers this project did not enable.** That is deliberate — enabling one by hand is exactly
  when you need the procedure.

**If `MCP-SETUP.md` is missing, has no section for your server, or its section lacks the step you need, say
so and ask.** A delivery made before that file existed will not have it. An absent step is a reason to ask
for it — never a reason to compose one.

## `MCP-SETUP.md` and `.mcp.json` answer different questions

`MCP-SETUP.md` says what the packet **knows**; `.mcp.json` says what this project **runs**. They can disagree
legitimately: a server `MCP-SETUP.md` marks *enabled* may have no `.mcp.json` entry because it is reached
through a personal, user-level config instead. Read both before concluding something is missing — and see
`no_silent_tool_loss` for the case where a declared server's tools never loaded.

*Rationale and neighbouring rules: `docs/rules-rationale/mcp_server_setup.md` (an always-on rule until 0.105.0).*
