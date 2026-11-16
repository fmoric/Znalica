# Znalica — Copilot Instructions

See `AGENTS.md` for the project's standards and working principles (shared with Claude Code; the agent
roster is defined there too).

## Copilot specifics

- Custom agents live in `.github/agents/*.agent.md` — run one with `copilot --agent <name>`. That file's
  frontmatter is where an agent's `tools:` list is read, which `AGENTS.md` → *Delegating to an agent* asks
  you to name when you hand one a task.
- Path- and topic-scoped rules live in `.claude/rules/` (Copilot CLI ≥ 1.0.89 and VS Code read it), and in
  `.github/instructions/*.instructions.md` when that copy was delivered.
