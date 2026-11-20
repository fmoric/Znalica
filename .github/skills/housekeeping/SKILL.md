---
name: housekeeping
description: Use when asked for housekeeping, a cleanup, or a workspace or tracker health check — audits git state, worktrees, to-do lists and instruction-file sizes; reports unless asked to act.
---

# Workspace Housekeeping

Run a health check on the project and fix what is safe to fix. Work through the steps in order.

> **Scope note:** this skill does tool-agnostic git + list hygiene only. If the project adds its own
> session-log, spec-index, or changelog/version files, extend the relevant step to cover them — **and when
> you do, self-test the check you added against something it should catch.** Two downstream projects
> extended this skill with steps whose "nothing found" output was byte-identical to "the check is broken",
> and both reported a clean pass for an unknown number of runs. See the `report_what_you_examined` rule.

**The command blocks are written for Git Bash**, which Git for Windows ships. A session with only PowerShell runs
each block unchanged through Git's own bash — `& (Join-Path (Split-Path (Split-Path (Get-Command git).Source))
'bin\bash.exe') -c '<block>'` — never a bare `bash`, which on Windows can be the WSL launcher. With no Git Bash,
say which checks could not run; do not improvise them. **A value one block prints goes into a later block as text
you type**, shown in angle brackets: the shell keeps no variable from one call to the next.

## Modes — what a bare run does, and what it does NOT do

Read the text after the skill's name in the user's message for a mode word (`/housekeeping archive` → `archive`).
**If there is none, you are in the default mode** — the destructive work is opt-in, and a missing argument must never be read as
permission to sweep.

| invocation | Steps 1–4 | Step 5 (triage, incl. 5f) | Step 6 (archive) |
|---|---|---|---|
| `/housekeeping` | run | **report candidates only** | **report candidates only** |
| `/housekeeping triage` | run | run | report candidates only |
| `/housekeeping archive` | run | run | run |

**Step 4 always runs in full, 4d included** — it is read-only throughout, and it is what Steps 5 and 6 work
from. Only the adjudicating steps are opt-in.

A request in plain words — *"housekeeping, and archive the finished items"* — is the argument. The mode
words are a convenience, not a grammar.

**Steps 5 and 6 edit the project's highest-blast-radius file, and Step 6 moves history.** That is why they
are not in the routine run: a gate that fires on every tidy-up is a gate that gets clicked through.
**5f raises that ceiling again** — it edits the *rules themselves*, and a wrongly narrowed rule stops
loading with no error anywhere. Same reasoning, one step further: it is opt-in, and it is confirmed
per-rule rather than applied in a batch.

**Branch first, and the check belongs HERE — not at Step 7.** `branch_before_edit` fires at the **first file
change**, and in a triage or archive run that is Step 5, not the commit step. Before the first edit in either
step, verify the current branch is not the protected/primary one and create `housekeeping-YYYY-MM-DD` if it
is. Reaching Step 7 and branching there would be a gate that fires after the act it guards.

## Step 1 — Git state (local + remote)

```bash
GIT_TERMINAL_PROMPT=0 git fetch origin   # confirm this SUCCEEDED before trusting anything below; the variable makes
                                         # a missing credential fail instead of waiting at a prompt nobody sees
git status
git log --oneline -5
```

Report:
- Current branch
- Any uncommitted changes (modified, deleted, untracked)
- **Sync state vs. remote (ahead/behind) — or `unknown (fetch failed: <reason>)`.**

**Confirm the fetch succeeded before reporting any sync number.** A failed `git fetch` — offline, expired
auth, no remote — leaves the local refs stale, and `git status` then reports "up to date with origin/main"
off refs nobody refreshed. "In sync" would mean either *genuinely current* or *never checked*, and the two
print the same sentence. If the repo has no remote at all, say "no remote configured", not "in sync".

## Step 2 — Classify uncommitted changes

For each file in `git status`:

| Change type | Action |
|-------------|--------|
| Source/content file — cosmetic/formatting only | Report it; **on the runner's yes**, stage and commit on a `housekeeping-*` branch |
| Source/content file — substantive change | Investigate: accidental edit or intended revision? Ask if unclear. |
| Config or instruction/rule file (agent rules, skills, CI) | Do NOT commit without understanding why it changed |
| Persisted memory / knowledge file | Verify the fact is still correct before committing |
| Deleted file superseded/renamed in a prior commit | Report it; **on the runner's yes**, stage the deletion and commit — the prior commit missed it |
| Untracked file under an agent-worktree directory | Orphaned worktree — see Step 3 |

Never use `git add -A` or `git add .` — add files explicitly. **Nothing in this table commits on a bare run
without that yes:** uncommitted files are usually someone's work in progress, and this skill reports unless asked to
act. Where the git guard is on, a `housekeeping-*` branch is not one of the project's branch types and the guard
asks the user to approve the name; say so before creating it.

## Step 3 — Orphaned agent-worktree cleanup

Agent-isolation runs can leave a dead linked-worktree folder whose `.git` gitfile points at a
deleted admin dir. If such a folder is tracked, it can break `git status` entirely (a gitlink to a
nonexistent gitdir).

```bash
git worktree list
git worktree prune
```

- **Report how many trees `git worktree list` named** — Step 8's `Worktrees` row asks for that count, and
  `clean, 1 listed` is a different claim from `clean`.
- An agent-worktree directory should be git-ignored; any leftover folder there is safe to delete
  after confirming `git worktree list` shows only the main tree.
- If a dead gitlink is already staged/tracked, unstage and remove it, then confirm `git status`
  runs cleanly.

## Step 4 — Resolve the project's to-do lists, then take a census

**Always runs. Read-only.** It produces the numbers Steps 5 and 6 work from, and on a bare run it is the
whole of the list handling.

### 4a. Resolve the lists FROM THE PROJECT

**The same resolution as `/initialize` Step 0, stated here in full: that file need not be opened for it**
(0.190.12; it is 21,000 characters, and this is the part needed). Anchor to the project root
with `git rev-parse --show-toplevel`, probe the ordered candidate list (`TODO.md`, `docs/TODO.md`,
`BACKLOG.md`, `docs/BACKLOG.md`, `ROADMAP.md`): exactly one that exists is the tracker, **two or more → name them
and ask rather than picking**, **none → say which candidates you probed and that none resolved** (never "nothing
pending"), and **refuse any path containing a `plugins/cache` segment**. Never resolve a path against this skill
file's own folder.

**That last backstop matters more here than it does there, because this skill writes.** A tracker resolved
against an installed plugin's snapshot instead of against this project is somebody else's backlog — and
Step 5 would then propose closing their items and Step 6 would move them.

Four kinds of list, all resolved the same way:

| list | path | what it is |
|---|---|---|
| **tracker** | resolved above | the backlog |
| **pending notes** | `docs/notes/PENDING_NOTES.md` | what the `note` skill appends to |
| **held notes** | `<git dir>/pending-notes.md` (`git rev-parse --git-common-dir`) | notes `/note` took on a protected branch, until `/shutdown` commits them (0.190.23) |
| **resume pointer** | `docs/sessions/RESUME.md` | what `/shutdown` leaves for the next session |

**Each absent file is reported by path** — *"no pending-notes file at `docs/notes/PENDING_NOTES.md`"*, not
"no pending notes". The first states what you checked; the second claims an audit you never ran, and is
exactly what gets read past when a project keeps its notes somewhere else.

**No tracker resolved → degrade, do not refuse** (`/initialize`'s branch, and for the same reason). Steps 5
and 6 then have no tracker to work on and say so **naming the candidates probed**; the notes file and the
resume pointer are still censused and still triaged. *"No tracker found among `<candidates>`"* is a
different sentence from *"nothing pending"*, and only the first one is true.

### 4b. Count — do not estimate

**Self-test the pattern before you trust a zero.** One command, and it is the difference between "no open
items" and "my counter is broken":

```bash
printf '  - [ ] x\n' | grep -cE "^[[:space:]]*- \[ \]"    # MUST print 1
```

If that prints `0`, **say the counter is broken instead of reporting a count.** The fixture is indented on
purpose: the common regression is `^- \[ \]`, which silently drops every item nested one level.

```bash
grep -cE "^[[:space:]]*- \[ \]" <tracker>     # open
grep -cE "^[[:space:]]*- \[x\]" <tracker>     # done
wc -c < <tracker>                              # size, in bytes
```

If the project keeps a **second tree** of items (per-area or per-theme files beside the tracker), count
those too and **sum them** — `grep -c` over several files prints one `path:count` line each and never a
total.

### 4c. Signals worth reporting even when no sweep was asked for

1. **Done-but-unarchived** — the `- [x]` count. This is the archive candidate count, not a verdict.
2. **Size against the read-in-one-pass ceiling.** Past roughly **256 KB** a session cannot read the tracker
   in one pass, so every future reader works from slices. **Report the number every run and flag it
   explicitly when it is over** — this failure has no symptom until someone plans against a slice they
   mistook for the whole. *(This threshold is the authority for the same report in `/shutdown` Step 5, which
   states it at write time on the file it just grew. **Step 5 carries a second copy of the number and its
   consequence on purpose** — a flag without its consequence is unactionable there — so changing the ceiling
   here means changing it there in the same edit. The other copy is in the `shutdown` skill, in the folder
   beside this one. A project set up for both tools holds each skill twice, under `.claude/skills/` and
   `.github/skills/`, so that is up to four places.)*
3. **A stale resume pointer.** If the newest file in `docs/sessions/` is older than the newest commit, the
   pointer describes a session that is no longer the last one, and `/initialize` will present it as the
   hand-off. Compare two commit times: `git log -1 --format=%ci -- docs/sessions/` (the newest commit that touched a
   session file) against `git log -1 --format=%ci` (the newest commit). A session file that was never committed
   has no time to compare: say so.
4. **A stale system map.** If the project has one (`docs/system-map.md`, or the map `AGENTS.md` names), read
   its built-at commit and count the commits since that touch the sources it maps:
   `git log --oneline <built-at>..HEAD -- <mapped roots> | wc -l` (the roots its header lists; a map that
   lists none is measured against the whole tree, and say so). Non-zero is stale — the map describes
   structure that may have moved. A map with a generator: run its `--check` instead, which answers exactly.
   **No `system-map.json` beside the map is stale too**, whatever the count or `--check` says: a generator
   older than the JSON passes its own check without writing one. No map is a fact to report, not a
   staleness. Refresh with `/system-map`.
5. **A changelog behind.** If the project keeps one (`CHANGELOG.md`, or the file `AGENTS.md` names), take its
   `<!-- changelog: through <sha> -->` marker, else the latest version tag, and report **two** counts since
   it: every commit (`git log --no-merges --oneline <start>..HEAD | wc -l`), and those the `/changelog`
   skill would list:
   `git log --no-merges --format=%s <start>..HEAD | grep -cE '^(feat|fix|perf)(\(.*\))?!?:|^[a-z]+(\(.*\))?!:'`.
   **Self-test it first:** the same `grep -cE` on the line `feat: x` must print `1`. Either count non-zero
   is behind. **Never report "current" from the second count alone**: in a history that does not use
   Conventional Commits it is 0 however far behind the file is.
   Release tooling that writes the changelog (`release-please`, `semantic-release`, `changesets`,
   `git-cliff`) answers this itself — report its preview instead. No changelog is a fact to report, not a
   lag. Refresh with `/changelog`.
6. **A stale roadmap** (0.205.0). If the project has one (`docs/roadmap.md`, or the file `AGENTS.md` names), read the
   commit its header was built at and the sources it lists, and count the commits since that touch them:
   `git log --oneline <built-at>..HEAD -- <its sources> | wc -l`. Non-zero is stale: an item may have moved state
   since. A header that names no sources is measured against the whole tree, and say so. No roadmap is a fact to
   report, not a staleness. Refresh with `/roadmap`.

**A census is not a sweep, and must not read like one.** *"12 items carry `- [x]`"* is a count;
*"12 items are closeable"* is an adjudication you did not perform. On a bare run, say which one you did:

> *"Census only — no item was adjudicated and nothing was edited. Run `/housekeeping triage` for that."*

### 4d. The instruction files — per-file limit AND always-loaded total

**Always runs. Read-only.** The lists above are what the project *plans* from; these are what every session
in it *starts* with. Different blast radius, not a bigger one — a bad tracker misdirects the work, a bad
instruction set misdirects every session before the work begins. Nothing else in this skill looks at them.

Resolve them from the project exactly as 4a does — anchor with `git rev-parse --show-toplevel`, **report
each absent file by path** (*"no file at `.github/copilot-instructions.md`"*, never "none"), and **refuse any
path containing a `plugins/cache` segment**.

| tool | files — each one is counted separately |
|---|---|
| **Claude** | `CLAUDE.md` · `.claude/CLAUDE.md` · `CLAUDE.local.md` · `.claude/rules/**/*.md` (**recursive**) · every file `@import`ed from those, to a depth of 4 |
| **Copilot** | `AGENTS.md` · `.github/copilot-instructions.md` · `.github/instructions/**/*.instructions.md` · `.claude/rules/**/*.md` (CLI ≥ 1.0.89 — a rule in both folders counts twice) · a root `CLAUDE.md` and every file it `@import`s (CLI ≥ 1.0.66 — an imported `AGENTS.md` counts twice) · `.claude/CLAUDE.md` and its imports **only when `copilot instruction list` shows it** (1.0.99-2 did not read it; GitHub's docs say Copilot does) |

**An `@import` is not a way to hide size, and this is the part that surprises people.** Claude Code loads
each imported file as its **own** memory file under its own name, so a thin `CLAUDE.md` that imports a huge
`AGENTS.md` produces a warning naming **`AGENTS.md`** — the importer looks innocent. Count the files, never
the bundle.

#### Two numbers, because they answer different questions

**1 — Per file, against the platform's limit.** Claude Code warns per memory file:
`<path> is over the <N>-char limit (<M> chars)`.

> **Flag at 40 000 characters.** The limit is **computed per session, not fixed** — roughly 5% of the
> session's context window converted to characters, with a floor of 40 000. A 1M-context session gets about
> **150 000**; an ordinary 200k-context session gets the **40 000 floor**. Flagging at the number you
> personally saw is the trap: **a file that passes on a 1M session warns for everyone else.**

State the formula once in the report, not on every row.

**2 — The always-loaded total, against the platform's TOTAL limit.** Since Claude Code 2.1.281 the platform
warns on this too: `<N> instruction files add up to <X> chars, over the <Y>-char total limit`.

> **Flag at 120 000 characters.** The total limit is `max(120 000, per-file limit)` — **120 000** on a 200k
> session, ~150 000 on a 1M one — so the same floor argument applies. Files already over the per-file limit
> are left out of the platform's sum; report them under number 1 instead of counting them twice.
> **The platform's sum also includes the user's own `~/.claude/CLAUDE.md` and rules**, which this project
> cannot see. Report the project's figure and say that it excludes those.

*For most projects this is the finding.* Per-file limits can all pass while the set costs an enormous share
of every session's context. Three cases, and **the first is the one that gets missed**:

| the rule's frontmatter | when it loads | counts toward the total? |
|---|---|---|
| **no `paths:` key at all** | **unconditionally, at launch** — this is the strictest case | **yes** |
| `paths:` is `**` | whenever a file is read, i.e. in practice always | **yes** |
| `paths:` is a real glob | as soon as the session reads a matching file | **yes, once loaded** — count it, and say how many are scoped |

**A rule with no `paths:` key is the genuinely always-loaded one**, and a criterion written only around `**`
drops it silently — the low-number failure this step is most prone to. Rules delivered by the packet library all
carry `paths:`, but hand-written rules in a project routinely omit the key.

**Copilot has no known limit, which is not the same as no limit.** Report the sizes and say so in those
words — *"no Copilot instruction-file limit is documented, and I have not measured one"*. A sweep of the
Copilot CLI changelog found none; that is absence of evidence, and *"no limit"* would be a claim nobody has
checked.

#### Count CHARACTERS, not bytes — and prove it with a non-ASCII fixture

**The limit is measured in characters; `wc -c` returns bytes, and on prose they are not the same number.**
*(4c's tracker size is deliberately in **bytes** — that ceiling is about how much a session can read in one
pass, not a character limit. The two figures measure different things and should not be made to match.)*
Every non-ASCII character — an em dash, a curly quote, an accent, and every CJK or emoji character — is 2–4
bytes. Byte-counting therefore **over-reports**, and it over-reports worst exactly where the file is most
likely to be near a limit: a CJK-heavy rule set can read **~3× its real character count**, flagging a
39 000-char file as over the 40 000 floor when the platform would never have warned.

**The self-test must contain a non-ASCII character, or it cannot detect the failure it exists for:**

```bash
printf 'a—b' | wc -m        # MUST print 3. If it prints 5, you are counting BYTES
```

**5 is the likely answer, not a hypothetical one** — `wc -m` decodes only under a UTF-8 locale, so with
`LC_ALL` unset it silently counts bytes and the ASCII fixture `printf 'abc' | wc -c` passes either way.
If the fixture prints 5, either set a UTF-8 locale (`LC_ALL=C.UTF-8 wc -m`) or count with something that
decodes — e.g. `python -c "import sys; print(len(open(sys.argv[1],'rb').read().decode('utf-8','replace')))" <file>`
(from bytes: Python's text mode counts each CRLF as one character, and the platform counts two). **If you cannot get a
character-accurate count, report the numbers as bytes and say so in the row** — a byte count labelled
"chars" is a fabrication carrying a number, and an over-report sends someone editing a healthy file.

**The finder is the one a fixture cannot cover, and it is the likelier failure.** A recursive glob that
misses `.claude/rules/` **subdirectories** prints exactly like a project that has no rules. So: **if
`.claude/rules/` exists and your listing returned zero files under it, say the finder is broken rather than
reporting `0`** — and when you do list files, confirm the listing reaches a nested one before trusting a
small total. A low number is this step's failure mode; a high one is self-announcing.

#### Remedies — name them, do not perform them

**For the total, in the order to try:** **move rationale out** — a rule's *Why*, *Source* and
neighbouring-rules sections go to a plain doc nothing auto-loads (`docs/rules-rationale/<rule>.md`, the shape
the packet library's own rules use since 0.105.0), with one line naming that folder in the instruction file every session loads (`AGENTS.md`, or `CLAUDE.md` where
there is none) · **move a multi-step procedure into a
skill**, which loads on demand · **move history out of `CLAUDE.md`** into session records or `docs/` ·
**`/doctor`**, which proposes trims for a checked-in `CLAUDE.md` (Claude Code v2.1.206+).

**Narrowing a rule's `paths:` (Step 5f) is NOT a remedy for the total.** A scoped rule loads the moment the
session reads a matching file and counts from then on, so a rule scoped to source code costs the same in any
session that touches code. 5f is for a rule that genuinely governs only some files — a correctness question,
not a size one — and it helps size only in sessions that never open those files.

## Step 5 — Triage the lists

**Conditional: full run only when the invocation asked for it (see Modes). Otherwise report the candidate
counts from Step 4 and move on — the remaining steps still run.**

Triage is *re-adjudication*: for each item, decide whether it still is what it says it is. It runs before
archiving because archiving moves what triage closed.

### 5a. Six verdicts

| verdict | what it needs before you may assign it |
|---|---|
| **done — closeable** | **a citation, verified now**: a commit SHA, a file path that exists, a shipped version, a released artifact |
| **stale external-state claim** | re-verification, or a hedge written into the item — anything asserting what a remote carries, what is installed, what is newest |
| **superseded** | the later item that covers it, named |
| **duplicate** | the surviving item, named |
| **unactionable** | say why — a narrative sentence with no concrete artifact behind it is not a task |
| **still open** | nothing; this is the default |

### 5b. Closing is the only destructive direction

**Evidence to close; none needed to keep.** A wrongly-kept item costs a line in a list. A wrongly-closed
item leaves the backlog permanently and silently — nobody re-reads the archive looking for live work, and
the file it lands in usually opens with *"nothing here is actionable"*.

So: **no citation, no close.** *"This looks finished"* is not a citation. If the item's own wording is too
vague to verify — a frequent and legitimate outcome — the verdict is **unactionable**, which asks the user
to rewrite it, not **done**.

### 5c. The stale-external-state row is the one that gets skipped

Items that assert external state — *"the plugin is installed at 0.62.0"*, *"`origin` carries the fix"*,
*"the dependency is on 3.x"* — read as settled fact and rot without any edit to the file. They are **the
most common defect a triage finds**, and one has been observed going stale **13 minutes** after the commit
that wrote it. Either re-verify it now and update the number, or write the hedge into the item
(*"as of <date>"*). Do not leave it bare.

### 5d. Never edit alone

Present the triage as a table — item, verdict, evidence — **and get confirmation before writing anything.**
Then apply only what was confirmed. A verdict the user rejects is dropped, not softened and applied anyway.

Two conventions that survive the confirmation:

- **Search in the existing item's vocabulary before calling something a duplicate.** A near-duplicate
  written in different words is the one this step mis-files.
- **Do not delete superseded reasoning.** Most trackers keep it struck through, because the post-mortem in
  an item is often worth more than the item. Follow the file's existing convention rather than importing
  one.
- **If the tracker publishes counts of its own** — an index table with an open-item column, a summary line,
  a badge — **update them in the same edit**, for both this step and Step 6. `/initialize` Step 3 diffs
  those figures against a live count and reports any gap as staleness: leave them and the next session
  investigates drift **this skill created minutes earlier**. `/shutdown` Step 4b carries the same obligation
  for appends; closing and moving items changes the same numbers in the other direction.

### 5e. Pending notes use the same table

`docs/notes/PENDING_NOTES.md` entries carry a label from the `note` skill's vocabulary — `discuss` ·
`research` · `idea` · `bug` · `reminder` · `pending` — and a date. Triage them exactly as above: an entry
whose work has landed is **done — closeable** *with a citation*, and gets removed. **Held notes are listed and
triaged in the report but never edited here**: `/shutdown` moves them into the committed file, and only then can
they be closed.

**Age is a flag, never a verdict.** An entry older than **30 days** is counted in Step 8's `Lists` row —
**not** in the `Triage` row, whose "stale" means the external-state verdict of 5c and nothing else — and is
still triaged on evidence like everything else. Nothing is deleted for being old.

### 5f. The instruction files use the same table — but the destructive direction is reversed

Adjudicate what 4d counted. One row per rule that is loaded unconditionally.

**Confirm each narrowing individually — this step does NOT take 5d's single table, single yes.** That
convention is right for closing tracker items, where the table's rows are independent and a mistake costs a
line. Here every row edits a file that shapes future sessions, and the damage is silent (see below), so a
blanket yes over a dozen rules is exactly the shape that should not be available. Present the table for
discussion, then **take the scopes one at a time.** Everything else in 5d still applies: apply only what was
confirmed, and a verdict the user rejects is dropped, never softened and applied anyway.

| verdict | what it needs before you may assign it |
|---|---|
| **always-loaded, justified** | the rule genuinely governs every file — a conduct rule like `no_fabrication` or `report_what_you_examined` has no narrower scope. Keep `**` |
| **narrowable** | **the file-set it governs, named as concrete globs.** A rule about test conventions is `tests/**`; a rule about migrations is the migrations path |
| **belongs in a skill** | it is a multi-step *procedure*, not a standing constraint — it should load on demand, not every session |
| **keep** | nothing; this is the default |

**Read 5b backwards here, and note that deliberately.** Everywhere else in this skill, *closing* is the
destructive direction. Here it is **narrowing**: a rule wrongly left at `**` costs context and nothing else,
while a rule wrongly narrowed **stops loading in the sessions that needed it and fails silently** — no error,
no missing file, just an agent that no longer knows the rule exists. So: **evidence to narrow, none to
keep.** *"This looks specific"* is not evidence.

> ⚠️ **The mirror pair moves together, and NOTHING WILL TELL YOU IF IT DOES NOT.** Where the project has
> mirrors (delivered only with `copilot_rule_mirrors` on — check `.github/instructions/`), every
> `.claude/rules/<name>.md` (`paths:`) has a twin `.github/instructions/<name>.instructions.md`
> (`applyTo:`) holding the same body. **Narrowing one without the other desynchronises the two tools** —
> the rule keeps firing on Copilot and stops on Claude, or the reverse — and **no check in your project
> catches it.** Do not wait for one: the library's own parity gate (`health_check.py` Check 1) is a
> **maintainer tool that is never delivered**, and it reads the library's `packets/**` sources, not your
> `.claude/rules/`. **So the verification is manual and it is yours:** after editing, re-read the two files
> and confirm the scopes match, then say in the report that you did — naming both paths.

**Never let this step touch `CLAUDE.md`'s `@import` lines.** Removing an import is not a size fix — it
silently unloads whatever it pointed at, which on the delivered shape is the whole of `AGENTS.md`.

## Step 6 — Archive what triage closed

**Conditional: full run only when the invocation asked for it (see Modes). Otherwise report the
done-but-unarchived count from Step 4 and move on — Steps 7 and 8 still run.**

### 6a. Destination

`<TRACKER-STEM>_ARCHIVE.md`, beside the tracker — `TODO.md` → `TODO_ARCHIVE.md`. If it does not exist,
**propose creating it** with a header that states where the items came from, that **nothing in it is
actionable**, and a link back to the tracker. Do not create it silently.

### 6b. Move verbatim. Never summarise, never delete

Completed items are moved **byte-for-byte**, grouped under their original section. The reasoning attached
to a finished item — why it was wrong twice, what the fix cost — is the reason to keep the file rather than
delete the entries, and a summary destroys exactly that.

### 6c. Count both sides, before and after

```
open_before  ==  open_after_in_tracker  +  open_items_moved
```

Both numbers get reported. **The failure this catches is a live `- [ ]` nested under a completed parent**,
which rides along invisibly in a block move and lands in a file whose first line says nothing there is
actionable. It has happened; it was caught by the counts disagreeing by one, not by reading.

**Never move a block without counting its open children first.**

### 6d. Repoint the anchors the move breaks

A link to a **file** survives a move. A link to a **`#heading`** does not, and it breaks **silently** —
nothing renders an error. After moving, grep both files (and any doc that links into the tracker) for
anchors pointing at the headings you moved, repoint them, and **report how many you fixed**.

Expect to find some that were **already** stale from an earlier rewording. Fix those in the same pass and
say so — they are evidence that nothing in the project is watching this.

### 6e. "Nothing to archive" is not "the list is healthy"

If few items are complete but Step 4c flagged the size, **say both**. A tracker can grow past the ceiling
purely through prose accreting onto *live* items, in which case an archive sweep is the wrong tool and
would report a clean pass on the file most in trouble. The remedy there is moving **history** out — older
narrative sections into a dated companion file — which is a different operation and needs the user's
decision. Name it; do not perform it unasked.

## Step 7 — Commit (only if changes were made)

1. Verify the current branch is NOT the protected/primary branch. If it is, create
   `housekeeping-YYYY-MM-DD` first (per the `branch_before_edit` rule). **In a triage or archive run this
   was already done before Step 5's first edit** — re-confirm rather than re-create.
2. Stage only the files identified above (explicitly, never `git add .`).
3. Commit with: `docs: housekeeping — [one-line summary of what was fixed]`
4. **Do not push and do not open a PR unless the user asks** — per `discussion_first`'s outward-facing
   trigger, which covers anything that leaves the machine. *(Not `pr_merge_approval`: that rule gates work
   landing **onto** the protected branch and explicitly declines pushing a feature branch. It hands this
   case to `discussion_first` by name.)* Report the local commit and stop.

## Step 8 — Report

```
## Housekeeping Report

**Mode**        : [census only / triage / archive] — from the user's message: [the words after the skill's name, or none]
**Git state**   : [clean / N files fixed] — branch [name], [ahead/behind origin / sync unknown — fetch failed: reason]
**Uncommitted** : [N files reviewed, all clean / list classified changes + action taken]
**Worktrees**   : [clean, N listed / pruned N / removed orphan folder / worktree check failed]
**Lists**       : tracker [path, how resolved / none — candidates probed: ...] — [N open, N done, N bytes, over/under the one-pass ceiling]; notes [N entries, N over 30 days / no file at <path>]; resume pointer [current / stale: last record <date> vs last commit <date> / no file at <path>]; system map [current / stale: built at <commit>, N source commits since / no map]; changelog [current / behind: N listable commits since <marker or tag> / owned by <tool> / no changelog]
**Memory**      : claude [N files read; always-loaded total N chars vs the 120 000-char total floor (excludes ~/.claude); largest <path> N chars vs the 40 000-char floor / no file at <path>]; copilot [N files, N chars — no limit documented or measured]
**Triage**      : [not run — census only, N closeable candidates / N items adjudicated: N closed, N stale claims, N superseded, N duplicate, N unactionable — each with its evidence]
**Scoping**     : [not run — census only, N rules loaded unconditionally / N adjudicated: N narrowed, N justified, N moved to a skill — mirror twins updated in the same change]
**Archive**     : [not run — N done items unarchived / N items moved to <path>, open count N → N (+N moved = N), N anchors repointed]
**Review**      : [re-read the finished edits whole — what that pass found, or "no edits made"]
**Committed**   : [nothing / commit SHA + message] (not pushed — awaiting user)
```

**Every row states what was examined, not only what was found.** A bare `clean` or `none` reads the same
whether the step ran or not — say `no pending-notes file at <path>` rather than `none`, and `clean, 1 listed`
rather than `clean`. If a step could not run, say so in its row instead of leaving it blank or optimistic.

**The counting rows carry their self-test.** A count reported without the Step 4b fixture having returned
`1` — or, for the `Memory` row, without 4d's — is a number of unknown provenance; say the fixture passed, or
say you are reporting no count. **The `Memory` row's own failure mode is a small number, not a big one:** a
finder that reached no rules and one that found none print the same total.

**The `Review` row is not optional when anything was written.** Steps 5, 5f and 6 edit the project's memory of
its own work. Re-read the **finished files whole — not the diff** (per the `self_review` rule): a moved
block that now contradicts a paragraph above it is invisible in a diff and obvious in the file, because
what it contradicts did not change.

## Related

Surfaces the same pending/uncommitted state the `list_pending_work` rule expects you to report instead of
asking "what's next?", audits the file the `note` skill writes to, and performs the **archive sweep** that
`/shutdown` names as out of its own scope. `/initialize` counts these same lists read-only at session
start; this skill is the only one that removes anything from them.

**§4d and §5f are the only steps that ACT on the instruction files.** `/shutdown` Step 5 and a delivery's
Verify step now **measure** the same two numbers and point here, but neither narrows a scope or trims a file
— so if a measurement turns up an overrun, this is where it gets resolved. That is also why §4d
runs on a bare invocation: the other measurements only report, so an opt-in step here would leave every
overrun reported and none resolved.
