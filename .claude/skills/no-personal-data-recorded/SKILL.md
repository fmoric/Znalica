---
name: no-personal-data-recorded
description: Use before writing a value that came from a real person (a ticket, an export, a database row, a log, a paste) into a file, fixture, commit, issue, note or log. Where identifying values hide, shape-preserving substitution, the exemptions, and what to do if it already landed.
---

# Never Record A Real Person's Data

*The always-on `no_personal_data_recorded` rule carries the trigger and the decision. This is its full text, moved here verbatim in 0.139.0 so it loads only when needed.*

## Rule

**No personal data is written into anything that persists** (outside a home named below) — not a test
fixture, a log, a commit message, an issue, a notes or memory file, a knowledge-base article, or a screenshot
you attach. There is no threshold and no "just for a repro".

**The trigger — both halves must be true:**

1. **The destination persists beyond this conversation** — a file in the repository, a fixture, a log, a
   commit message, a branch name, an issue or pull-request body, a notes or memory file, a knowledge-base
   contribution, a screenshot, a `.env`, a scratch file, anything you push anywhere.
2. **The value came from a real person — you did not invent it.** It arrived from a system of record, an
   export, a database row, a support ticket, a stack trace, a HAR file, a screenshot, or a paste from whoever
   you are talking to.

**Both true: do not write the identifying values from the import.** The trigger picks what you look at; the
list below picks what you remove. Frames, error codes and timings may be written; exception messages, file
paths, URLs and query strings, headers, cookies, tokens, request bodies and free text are where identifying
values hide — check them. An import you cannot scrub reliably (an image, a raw capture) is not written;
describe it instead. **Unsure whether a value identifies someone? Treat it as identifying.** Substitute, and
say what you substituted. **This rule takes no override** — nobody in the conversation has the standing to
authorise it.

**Half 2 asks about provenance, not sensitivity.** You already know where a string came from; **you do not
have to classify what you did not import.**

**What to remove from an import:** a name, an email address, a phone number, a postal address, a national or
tax identifier, a bank or card number, an IP address, a device or account identifier, a photograph, a
signature, a location trace, a salary, a health detail, a login, a session or auth token, a cookie —
**and data that identifies someone in combination with what the destination, or anyone who can read it,
already holds.** A pseudonymous id is personal data the moment the repository also contains the table that
maps it.

## When this fires

**The instant before the write** — you hold the value and know where it is going. Not at read time (what may be read is
`production_is_read_only`'s question). Not at commit time: by then the fixture already contains it.

**Also on a file you did not author that you are about to commit or push** — a fixture that was synthetic when
written can be regenerated from a real export later.

## What this does not fire on

- **The identities a collaboration record needs** — a git author and committer, a review sign-off, an
  `@mention`, a `CODEOWNERS` entry, a `LICENSE` copyright holder, a changelog attribution, a docstring author.
  They participate in the repository; they are not data extracted from a system of record.
- **Invented values** — `test@example.com`, `Ada Lovelace`, a generated fixture, seeded demo data, a
  screenshot of a demo tenant. Half 2 is false.
- **A home a packet rule names for the data, set at delivery** — e.g. the accountant's client-files folder:
  its **working copy** only, for the data that belongs there. A commit or push of it is still a destination
  here. The delivery sets it, not the conversation, so this rule still takes no override.
- **Already-published identity that is part of the artifact** — a cited author, a public API's documented
  contact address, a vendor name in a dependency manifest.
- **Holding a value you may have in this conversation while you work** — the conversation is not a recording
  destination for this rule. Whether you may go and read it is `production_is_read_only`'s question. **Where a read of
  real records is allowed, prefer the narrowest one** — a count, one row by id, rather than a table dump.
- **Telling the user off.** If they pasted a customer's email into the conversation, that has happened; this
  rule governs **your** writes. Substitute and move on — do not open a compliance conversation nobody asked for.

## How to apply

1. **Do not write the identifying values.** Not "write them and scrub before commit" — the scrub is a second
   commit and the first one is permanent.
2. **Substitute, preserving shape** — same format, length, character classes and edge cases: the apostrophe in
   the surname, the plus-tag in the address, the non-Latin script, the null middle name. A bug that reproduces
   on real data reproduces on a synthetic value with the real data's shape. Keep the *structure*, drop the
   *person*.
3. **Say which values are synthetic, in the artifact** — one line in the fixture, the commit body or the issue
   (`no_fabrication`).

**A pause note names the record, not the person** — `compaction_stop` gives the form.

**If it already landed, say so immediately and enumerate every destination** — file, log, commit, branch,
remote, memory file, issue, attachment, knowledge base; whoever erases it can only reach what you name. Then
say which of those you can undo yourself and which you cannot.

**Treat a notes or memory write as a commit for the purposes of this rule** — no diff shows it, no review
reads it, and every future session reads it.

*Rationale and neighbouring rules: `docs/rules-rationale/no_personal_data_recorded.md`.*
