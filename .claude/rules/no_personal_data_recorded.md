---
paths:
  - "**"
---

# Never Record A Real Person's Data

**No personal data is written into anything that persists.** There is no threshold and no "just for a repro".

**The trigger — both halves must be true, the instant before the write:**

1. **The destination persists beyond this conversation** — a repository file, a fixture, a log, a commit
   message, a branch name, an issue or pull-request body, a notes or memory file, a knowledge-base
   contribution, a screenshot, a `.env`, a scratch file, anything you push. **Also a file you did not author
   that you are about to commit or push.**
2. **The value came from a real person — you did not invent it** — a system of record, an export, a database
   row, a support ticket, a stack trace, a HAR file, a screenshot, or a paste. This asks about provenance, not
   sensitivity.

**Both true: do not write the identifying values from the import** — a name, an email address, a phone number,
a postal address, a national or tax identifier, a bank or card number, an IP address, a device or account
identifier, a photograph, a signature, a location trace, a salary, a health detail, a login, a session or auth
token, a cookie — and anything that identifies someone in combination with what the destination already holds.
**Unsure whether a value identifies someone? Treat it as identifying.** Substitute values that keep the shape
(format, length, the apostrophe in the surname, the plus-tag, the script), and say in the artifact which values
are synthetic. An import you cannot scrub reliably (an image, a raw capture) is not written — describe it. A
notes or memory write counts as a commit. **This rule takes no override.**

**Not in scope:** identities a collaboration record needs (a git author, an `@mention`, `CODEOWNERS`, a
changelog attribution), invented values, a home a packet rule names for the data (its working copy only),
already-published identity, and holding a value you may have in this conversation (`production_is_read_only` says
what you may read). Do not lecture the user about
what they pasted.

**If it already landed, say so immediately and enumerate every destination** — file, log, commit, branch,
remote, memory file, issue, attachment, knowledge base — and which of them you can undo yourself.

**Where identifying values hide in an import, and the full exemptions: read
`.claude/skills/no-personal-data-recorded/SKILL.md` (Copilot: `.github/skills/no-personal-data-recorded/SKILL.md`).**
