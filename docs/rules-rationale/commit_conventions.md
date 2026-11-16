# `commit_conventions` — rationale

Moved verbatim out of the `commit_conventions` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Why

A consistent, machine-parseable history enables changelog generation, semantic-version inference, and fast
`git log` scanning. The subject answers *what changed*; the body answers *why*.

## Why look first (0.132.0)

A team's existing commit style is a decision someone made, and an agent's first commit is the wrong place
to overturn it. So the rule reads the recent history and **asks** when it finds a consistent different
style, rather than switching the format silently; recording the answer in `AGENTS.md` keeps it from being
asked again. Keeping the team's style costs one thing: `/changelog` sorts commits by their Conventional
Commits type, so a project that keeps its own style gets its commits under *Unsorted* — the skill's
"most of the range is Unsorted" path handles that, and says so.
