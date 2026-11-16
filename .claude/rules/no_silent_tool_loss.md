---
paths:
  - "**"
---

# Say So When a Configured Capability Is Missing — Never Degrade Silently

At **first reliance** on an MCP capability this project or agent promises (symbol navigation, knowledge
lookup, thinking server), not at session start, check **your own loaded tools**. Claude Code uses
`mcp__<server>__*`, Copilot CLI `<server>/*`; the other form's absence proves nothing. A status panel
saying "No MCP servers configured" is not evidence if tools are live. Committed `.mcp.json` **declares**
servers; it does not **install** them.

**First determine whether the capability was granted.** A delivered agent's frontmatter `tools:`
(`.claude/agents/<name>.md` or `.github/agents/<name>.agent.md`) is its allowlist. If not named, the server
is withheld deliberately: this rule does not fire; if that limits your answer, tag it via `task_assessment`,
never call it an installation failure. If named but absent from your loaded tools, this rule fires. A main
session has no agent definition and receives every enabled server.

**Say so plainly in your reply before continuing with any fallback:** identify the missing granted
capability, what you will use instead and what it cannot do; report any observed load error rather than
implying the original tool ran. If no viable fallback exists, say what cannot be completed. Continue where
possible: this is disclosure, not an automatic stop. Quietly substituting `grep` for absent symbol tools
is a breach; choosing text search while those tools *were* loaded is not this case (`task_assessment` still
tags answer limits).

For every missing granted MCP capability, **READ the delivered `diagnose-mcp` skill**
(`.claude/skills/diagnose-mcp/SKILL.md` or `.github/skills/diagnose-mcp/SKILL.md`) **before claiming a
cause or prescribing a remedy**. Follow its scope, personal-config and A/B/C checks; never default to
"install the binary." If you cannot read it or establish a cause, say diagnosis is incomplete and name
the next check rather than inventing a fix. **Do not hide a load error behind a successful fallback.**
Reporting a remedy is not performing it: `external_tool_state_change` governs installs, personal config
and other shared-state changes.
