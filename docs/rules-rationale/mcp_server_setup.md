# `mcp_server_setup` — rationale

Moved verbatim out of the former `mcp_server_setup` rule (both mirrors) at 0.105.0, when that rule became the `mcp-server-setup` skill, to keep the instruction files every session loads under Claude Code's total limit. **The skill is what governs**; read this for why it exists and how it composes with the rules around it. *"This rule"* below means that former rule.

## Source

2026-07-31. Asked to **register** an MCP server against an existing Serena installation, an agent ran
`uv tool install --from git+…` instead, destroying a working pinned install that another live session was
using. The correct procedure — check `serena --version` first, reuse the existing install, and use the plain
`uv tool install serena-agent` only if one is genuinely needed — **was already written down and verified in
the packet that delivered this project's rules.** It lived in a definition file the delivery reads at
scaffold time and never copies. `MCP-SETUP.md` exists so that it is readable from inside the project; this
rule exists so that it is read.
