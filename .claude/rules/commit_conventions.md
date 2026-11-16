---
paths:
  - "**"
---

# Commit Conventions

## Rule

**Applies when this project uses git.** If it doesn't, this rule is simply inert — skip it.

**Look first.** If `AGENTS.md` or `CONTRIBUTING.md` names a commit format, follow it. Otherwise, before
your first commit, read the recent subjects (`git log -20 --format=%s`). If they show a consistent style
other than the one below (a ticket prefix, `[Area] Subject`), **ask** which to use; never switch it
silently. Follow the answer, and offer to record it in `AGENTS.md` so it is asked once. Mixed or no
history → the format below.

Write commit messages in the **Conventional Commits** format: `<type>(<scope>): <subject>`.

- **type** — one of `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `perf`, `build`, `ci`.
- **scope** — optional; the area touched (e.g. `auth`, `api`, `parser`).
- **subject** — imperative mood, lower case, no trailing period, ≤ ~72 characters.

Add a body (after a blank line) when the change needs a *why*. Flag a breaking change with `!` after the
type/scope (`feat(api)!: …`) or a `BREAKING CHANGE:` footer.
