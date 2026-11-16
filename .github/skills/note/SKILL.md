---
name: note
description: Quick-capture a thought, idea, or pending discussion topic without interrupting the current session.
---

# Note Capture

Capture the note the user gave — the text after the skill's name in their message — to the project's pending-notes
file. Fast — do not start a
discussion or ask follow-up questions. Just capture and confirm.

## Step 1 — Determine note text

If the user's message has text after the skill's name (`/note cache the lookup table`), use it verbatim as the
note text — **except a real person's identifying values**
(`no_personal_data_recorded`: the notes file persists and every later session reads it). Write the record's
key instead, in `compaction_stop`'s `subject` form — *"call back the customer on ticket #4411"*, never a name,
an email, or a key this workspace itself maps to the person; with no key, *"(the customer the user pasted; ask
again)"*. **People working on this project stay as written** — a teammate the user names, an `@mention`: like
the rule's collaboration identities, they take part in the work. Only customers' and other outsiders' data is
replaced.

If there is no such text, ask the user in one sentence: "What would you like to note?" Wait for their
response, then use that as the note text, under the same exception.

## Step 2 — Classify (one word, inferred)

Pick the most fitting label from: `discuss` · `research` · `idea` · `bug` · `reminder` · `pending`

Infer it from the text — do not ask the user. When in doubt, use `pending`.

## Step 3 — Append to `docs/notes/PENDING_NOTES.md`

**On a protected branch, hold the note instead** (0.190.23). A protected branch is one `branch_before_edit` would
make you leave before writing: the repository's default or declared primary branch (`main`, `master`, or the
branches a git-guard config lists as protected), and a detached `HEAD` counts too. Write the entry, in the same
format, to `<git dir>/pending-notes.md`, where `<git dir>` is what
`git rev-parse --path-format=absolute --git-common-dir` prints from the project root. That file is in the git directory, not the working tree, so nothing is committed, nothing rides along onto
the next branch, and no pull is blocked; `branch_before_edit` names it as its exception, and `shutdown` moves held
entries into `docs/notes/PENDING_NOTES.md` in its record commit. Create it with the header below if it is absent.
Everywhere else (a feature branch, or not a git repository) write `docs/notes/PENDING_NOTES.md` as below. **If the
write to the held file is refused** (a permission prompt declined, a tool that will not write under `.git/`), write
nothing else: reply with the entry as you would have written it and say it was not saved, so the user can keep it.

Determine today's date (format `YYYY-MM-DD`).

If the file you are writing to does not exist, create it (for `docs/notes/PENDING_NOTES.md`, with the
`docs/notes/` directory) with this header:

```markdown
# Pending Notes

Quick captures from sessions. Review before wrapping up or when starting a new discussion.

---

```

Append the note in this format (add a blank line before the entry if the file already has content; if the file
does not end with a newline, add one first, and end the entry with one):

```
### <YYYY-MM-DD> [<label>]: <note text>
```

The square brackets around the label are written; the angle brackets mark what you fill in.

If Step 1 replaced anything, end the line with `(identifying details replaced)`.

Example: `### 2026-01-15 [idea]: cache the lookup table to cut the repeated dashboard query`

## Step 4 — Confirm

Reply with exactly one sentence: "Noted — [label]: [note text]", the label and text read back from the line as it
now stands in the file. On a protected branch add *"(held until /shutdown)"*. If Step 1 replaced anything, end that sentence
with what it replaced — "(name replaced with the ticket number)" — so the user knows the note is not their exact
words.

Do not summarize, do not suggest follow-up, do not ask what to do next. The session continues. This
confirmation is a capture, not a work report: `write_for_the_reader`'s next step does not apply, because the work
in progress continues. Appending one entry to a capture log does not change what the file means, so `self_review` asks no
review of the file here; read the entry back before replying, and fix it if it came out different from what you
meant.
