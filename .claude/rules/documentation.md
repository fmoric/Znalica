---
paths:
  - "**"
---

# Documentation Standards

## Where docs live

**This governs where documentation *you* write goes. It is not an audit of what is already on disk.**

- Technical documentation you write belongs in `docs/`.
- User manuals belong in `docs/manuals/`.
- Don't add **new** documentation to the project root (except `README.md`).

**A root file that something DEPENDS ON being there is not yours to relocate**, and updating or regenerating
one in place is not "creating documentation in the root" — other instructions and tooling read it from that
fixed path. Example: `MCP-SETUP.md`, which the `mcp-server-setup` skill reads at the root; moving it silently
disables that skill.

**Presence is not permission.** A root document nothing points at — even one an earlier session put there —
gets no protection from having survived.

**Test — ask what breaks if the file moves.** If something does, leave it; if nothing does, and you are
creating or tidying, it goes in `docs/`. `MCP-SETUP.md` is an illustration, not a list.

## Keeping docs correct

- Update docstrings, comments, and type hints for changed code.
- Keep docs consistent with the implementation: update docs, comments, and examples in the **same** pass as
  the change (tests: the testing rule).
