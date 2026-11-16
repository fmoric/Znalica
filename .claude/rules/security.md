---
paths:
  - "**"
---

# Security Rules

## Security

- Sensitive or privileged methods start with a **permission check** (RBAC) before doing any work.
- Use **parameterized queries** for all database access — never concatenate user input into a query string.
- **Log** security-relevant operations (authentication, authorization, permission changes, sensitive data
  access) at an appropriate level.

## Credentials in the conversation

**A credential's value stays out of the conversation** — a password, a client secret, an API key, a token, a
connection string that holds one. An invented or placeholder value is not one.

- **Do not ask for it, and decline when the user offers to paste it**, whatever data is behind it: demo data
  does not make the credential a demo, and the conversation is stored. Ask the user to enter it themselves,
  outside this session, where it is read without being shown (a masked prompt in their own terminal, an
  environment variable, the project's secret store), and refer to it by name. Check that it is set, never its
  value. If no such place exists yet, say so and stop there; do not accept the value instead.
- **Do not print or quote one you come across** in a file or a command's output: read only the part you need
  (a host, a name).
- **One already pasted:** do not repeat it; say once that it is in the conversation, and suggest rotating it.

## Before committing

Follow the `pre-commit` skill on the staged changes: invoke it, or where you cannot, read
`.claude/skills/pre-commit/SKILL.md` (Copilot: `.github/skills/pre-commit/SKILL.md`) and do what it says.
It is the one definition of the commit checks; this rule lists none. Committing without it is the
exception: when you do, say the checks were not run, and why.
