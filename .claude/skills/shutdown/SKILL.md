---
name: shutdown
description: Use only when the user ends the session (/shutdown [full]) — never at the end of a task, PR or branch, and never offered unprompted. Reconciles git, files findings, writes the session record, then commits and pushes that record alone to main.
---

# Shutdown

An **end-of-session hand-off**: write down what happened, file what was discovered, and leave a pointer the
next session can pick up. It closes the loop `/initialize` opens.

**It exists because findings evaporate.** A finding that reaches the screen and not the tracker gets
re-discovered later at full cost, with a person as the transport every time.

**It commits its own files and nothing else** (0.190.0). A session record is not a code change, so it lands on the
default branch, where the next session looks first: Step 4 writes in a temporary worktree on a session branch cut from
it, and Step 6 commits and pushes **only the files this skill wrote**. The session's own branch, its commits and its
uncommitted work are never touched. **Never force-push, never merge another branch, never open or merge a pull request,
never commit a file this skill did not write.** That limit is an instruction, not a structural guarantee: the shell
this skill needs for `git log` can do all of it.

**The command blocks are written for Git Bash**, which Git for Windows ships. Each block stands alone: a value one
block prints (the default branch, the worktree path) goes into the next as **literal text you type**, shown as
`<default>` and `<worktree>` — the shell keeps no variable from one call to the next, and an empty one makes git act
on the session's own checkout. A session with only PowerShell runs a block through Git's own bash,
`& (Join-Path (Split-Path (Split-Path (Get-Command git).Source)) 'bin\bash.exe') -c '<block>'` — never a bare `bash`,
which on Windows can be the WSL launcher. With no Git Bash, say which step could not run; do not improvise it.

## Modes — light and full (0.205.0)

Read the text after the skill's name for a mode word: `/shutdown full` → **full**. Nothing, or anything else →
**light**, the run described below. A request in plain words (*"full shutdown"*, *"shut down and tidy up"*) is the
word; a missing one never is.

| invocation | Steps 0–6 (the record) | Step 1c (tidy-up check) | Step 7 (tidy-up) |
|---|---|---|---|
| `/shutdown` | run | run, read-only | **not run**: Step 6's report names what it would do |
| `/shutdown full` | run | run, read-only | run, each part on the user's yes |

Light is the default because Step 7 can take long: a changelog or map rebuilt from history, a triage asked verdict by
verdict. **Full changes nothing in Steps 0–6**: the record is written and landed exactly as in a light run, and
Step 7 starts only after it has landed.

Sibling: `/initialize`.

---

## When this runs — and when it does not

**Once per session, when the user ends the session.** Not at the end of a task, not on a merged PR, not on a
closed branch — those are the `list_pending_work` rule's trigger: list what is pending and carry on.

**The discriminator has to be answerable at the moment it fires, which rules out the obvious one.** *"Am I
stopping work here?"* is not answerable then: at the end of the first of four things the user came to do,
the honest answer is *I do not know — they have not said*, and that is exactly the moment this skill
misfires. The evidence you
actually hold is whether **the user said so**. The boundary is theirs, not yours — **the session ends when
the user ends it.**

**"The user said so" still needs one distinction, and it is the one that misfires:** *"done with this"* is
not *"done for today"*. *"Ok, we're finished — wrap it up"* after the first of four things satisfies "the
user said so" completely and is still the wrong trigger. If what they ended is a **task** and there is more
on the table, that is `list_pending_work` again. **Genuinely ambiguous → ask, in one line, before writing
anything** — never guess toward the branch that writes files.

The corollary, in these words: **if you are wondering whether to run `/shutdown`, the answer is no.** It is
**invoked, not proposed** — do not offer it at the end of a unit of work.

**And if you are already reading this on a task boundary, STOP HERE — do not continue to Step 0.** Write
nothing: no session record, no tracker append, no `RESUME.md`. Say in one line that `/shutdown` was reached
at the end of a unit of work rather than a session, give `list_pending_work`'s answer instead — what is
pending — and hand back. A section that only *describes* the boundary cannot enforce it; this sentence is
the enforcement.

**What an early run costs**, since the failure is quiet: another dated record under `docs/sessions/`, a
second append into the tracker, and `RESUME.md` overwritten with a **narrower** slice of the session than the
previous run held — so the next `/initialize` picks up the smaller hand-off.

**Step 4a's same-day `-2` suffix is not a licence for this, and the distinction is per *session*, not per
day.** Two genuine sessions in one day is normal — that is exactly what Step 4a exists for. Two records for
**one** session is the misfire, and the suffix only keeps them from colliding on disk; it never made the
second one correct.

---

## Step 0 — Resolve the tracker FROM THE PROJECT

**The same resolution as `/initialize` Step 0, stated here in full: that file need not be opened for it**
(0.190.12). Anchor to the project root (`git rev-parse --show-toplevel`; never this skill file's own folder), then
probe `TODO.md`, `docs/TODO.md`, `BACKLOG.md`, `docs/BACKLOG.md`, `ROADMAP.md` there:

- **exactly one exists** → that is the tracker; name it;
- **two or more** → name them all and ask which is authoritative; never pick;
- **none** → the branch below;
- **a resolved path containing a `plugins/cache` segment** → stop and say so: that is an installed template's
  snapshot, not this project.

It matters more here, because **this skill writes**: a tracker resolved from an installed plugin's snapshot
instead of from this project would append your findings into somebody else's backlog.

One branch differs, because writing is not reading:

- **No tracker resolved → do not invent one.** You may still write the session record (Step 4a) and the
  resume pointer (Step 4c) if a sessions directory exists or the runner approves creating one. **Propose
  where a tracker should live; never create one silently.** Findings with nowhere to go are reported in
  Step 6 as unfiled, which is honest and keeps them visible.

## Step 1 — Reconstruct the session, against git rather than memory

What you remember of the session is **low-trust** — a summary reads as authoritative while being partial.
Cross-check before writing anything down.

**First find the default branch, once, and note its name.** Every later step uses it as the literal `<default>`:

```bash
GIT_TERMINAL_PROMPT=0 git fetch origin
git symbolic-ref -q --short refs/remotes/origin/HEAD || git branch -r --list origin/main origin/master
```

Take the first name printed, without `origin/`. **Nothing printed** (no remote, or a remote with neither name) →
there is no `<default>`: say so, skip every command that takes it, and Step 4 takes its no-remote case. **The fetch
failed** (offline, credentials) → say so; `origin/<default>` is then as old as the last fetch, and Step 6 will not
be able to push.

```bash
git status -sb                  # uncommitted work AND ahead/behind in one line
git rev-parse --abbrev-ref HEAD # branch
git log --oneline @{u}..HEAD    # commits that exist ONLY here (errors if no upstream)
git merge-base HEAD origin/<default>   # <base>, on a feature branch
```

`<base>` is where this session's work started. On a feature branch it is the last line's output. On the default
branch itself, or with no `<default>`, it is the commit the session started at: the `Written at:` sha in
`docs/sessions/RESUME.md` if it is an ancestor of `HEAD`, else ask. Then, with the sha typed in:

```bash
git log --oneline <base>..HEAD  # what actually landed
git diff --stat <base>..HEAD    # what actually changed, and how much
```

**No `<base>` you can name → say so, and show `git log --oneline -20` instead.** Never report an empty range as
"nothing landed".

Where the conversation and git disagree, **git wins, and the disagreement is worth a line in the record.**

### Flag unpushed work explicitly — this is the point of running at session close

**A branch with no upstream is the dangerous case, not an edge case.** `git log @{u}..HEAD` **fails** with
*"no upstream configured"* rather than printing anything, so a reader skimming for output sees nothing and
concludes nothing is outstanding. Say which of the three states you are in:

- **no upstream** — *"`<branch>` has no upstream; `<n>` commits exist only in the checkout that ran this
  shutdown."* Not *"this working copy"*: the line is copied into `RESUME.md`, and the next reader may be on
  another machine, where "this" is theirs.
- **ahead** — *"`<n>` commits unpushed."*
- **in sync** — *"pushed through `<sha>`."*

Session close is the only place this prevents loss.

**Held notes.** Run `git rev-parse --path-format=absolute --git-common-dir` from the project root and note the path; if `<git dir>/pending-notes.md` holds any
`### ` entry, `/note` wrote it on a protected branch (0.190.23). Report *"<n> held notes: Step 4 (d) carries them"*.

## Step 1b — Offer a look back: `work-monitor`, then `retro`

**Only where both skills are delivered** (0.201.0): `work-monitor` and `retro` each have a `SKILL.md` under
`.claude/skills/<name>/` or `.github/skills/<name>/` at the project root. Either one missing → skip this step without
a word; Step 4a's record says *"look back: not delivered"*.

**Offer it once, as a question, before anything else is harvested:** *"Before I write the record: run `work-monitor`
on this session, then `retro`? Both, retro only, or skip."* Skip is a whole answer: ask nothing more about it. This
is an offer inside a `/shutdown` the user already started, not `/shutdown` offering itself.

**On *both*, run them in that order, each by its own steps except where this list says otherwise.** On *retro only*,
skip the first bullet: `retro` reads the session alone.

- **`work-monitor`**, with the window *this session*: since the commit it started at where Step 1 named one on the
  default branch, else *today*. Not a feature branch's merge-base, which can span earlier sessions whose rework is
  not this one's to file. Its report is the evidence: runs over budget, stalls, spend and rework. **Give its report
  without its step 3 item 3, the next steps.** `retro` runs next anyway, and stopping a process is not part of `/shutdown`.
- **`retro`** on this session, with that report beside the session as evidence for its step 1.
- Neither writes anything, and neither changes how this skill ends. When `retro` asks which candidates to act on,
  the question here is **which to file**. **Apply none during `/shutdown`**: a fix the user asks for is new work,
  started after this skill ends, and this skill commits only the files it writes.
- **An answer the tool chose** (Copilot autopilot answering the offer or the pick) is no choice: take it as *skip*,
  file nothing from `retro`, and say so (`write_for_the_reader` obligation 6).

**What the picks become.** Each `retro` candidate the user picks is a Step 2 candidate: it gets the duplicate search,
Step 3's confirmation and Step 4 (b)'s append like any other finding. A candidate `retro` marked **packet-owned** is
filed with the word `packet-owned` in its body. The next `/assess-delivery` lists every open item that says so in
its *For the library* section, which is how it reaches the packet's maintainers. A
candidate not picked is dropped, as a finding refused in Step 3 is.

## Step 1c — The tidy-up check (both modes, read-only)

Run `housekeeping` Step 4c's signals **1** (done but not archived), **2** (the tracker's size), **4** (a stale system
map), **5** (a changelog behind) and **6** (a stale roadmap), by that skill's own commands, in its `SKILL.md` beside
this one. This step does not restate them. `housekeeping` not delivered → *"tidy-up check: not run, housekeeping not
delivered"*. Each signal gets one state: *stale* or *over*, with its count; *current*; *none in this project*; or
*not run*, with why. **Nothing is refreshed or edited here.** The result goes in the record (Step 4 (a), *Not done /
left open*) and in Step 6's report, and in a full run it is what Step 7 offers.

## Step 2 — Harvest findings

Go through the session for anything worth keeping that is **not already filed**: a defect found, a claim
falsified, a document that went stale, a check that missed something, a decision that needs recording.

For each candidate, **search the tracker before proposing it**:

```bash
grep -rn "<distinctive phrase>" <tracker> <any second tree>
```

**Search the default branch's copy too** (`git show origin/<default>:<tracker> | grep -n "<phrase>"`, with Step 1's
`<default>`): Step 4 appends there, and a session on a feature branch may hold an older or newer copy than the one
the append lands in.

**Search in the existing item's vocabulary, not only your own.** A near-duplicate written in different words
is the one this step misses, and appending a second copy of an existing item is the main way this skill does
damage. A near-duplicate belongs as a sub-bullet on the existing item — which is an **edit**, so it goes to
Step 3 for confirmation like everything else.

**Not every observation is a finding.** Something already fixed this session is history for the session
record, not a new open item.

**Step 1b's picks are candidates here too**, searched the same way.

## Step 3 — Discuss before writing

Present, in plain conversation:

- the proposed session record, in outline;
- **each proposed tracker append, in full**, with the section it would land in, and *from `retro`* on each that came
  from Step 1b;
- anything you judged a duplicate and dropped, with the item it duplicates;
- **held notes** (Step 1), when there are any: they go into `docs/notes/PENDING_NOTES.md`, which Step 6 then
  commits with the record;
- **where it lands**, in one line: *"Step 6 commits these files, and only these, and pushes them to `<default>`"* —
  or to a `docs/session-<date>-<slug>` branch where Step 6 says so, pushed nowhere where there is no remote.

Get confirmation. The tracker is usually the highest-blast-radius file in the project. **A finding the runner
rejects is dropped — not softened and filed anyway.**

## Step 4 — Write the artifacts

### Where to write — decide first, then create a temporary worktree

**Do not write in the session's checkout.** Pick the case before running anything:

- **Not a git repository** → write in the project folder. Nothing is committed, and Step 6's landing is skipped.
- **A git repository** → the files go into a temporary worktree on a new branch, `docs/session-<date>-<slug>`.
  It starts from `origin/<default>`; **with no `<default>`** (Step 1: no remote) it starts from the local `main` or
  `master`, or from `HEAD` if the repository has neither.

**Pick the slug first, and check it is free — writing overwrites, and these records are history.** Two sessions in
one day is normal, and a related slug collides easily. Look in both copies, with the start point typed in:

```bash
ls docs/sessions/$(date +%Y-%m-%d)-* 2>/dev/null
git ls-tree -r --name-only <start point> -- docs/sessions/ | grep "$(date +%Y-%m-%d)-"
git branch -a --list "*docs/session-$(date +%Y-%m-%d)-*"
```

If the path or the branch exists, **pick a distinct slug** (or suffix `-2`). Never overwrite a session record: it is
the only copy of that session's narrative and its PAUSED block, and the resume pointer is overwritten too.

Then create the worktree, in **one** block, and note what it prints. The path is `<worktree>` from here on:

```bash
t=$(mktemp -d)/session; t=$(cygpath -m "$t" 2>/dev/null || echo "$t")     # outside the project; C:/… on Windows
[ "$(git config --bool --get extensions.worktreeConfig)" = true ] || echo "worktreeConfig: was unset"
git worktree add --no-checkout -b docs/session-<date>-<slug> "$t" <start point> && echo "worktree: $t" \
  && git -C "$t" sparse-checkout set --cone docs/sessions docs/notes \
  && git -C "$t" read-tree -mu HEAD && echo "sparse: ok"
```

**The worktree is sparse** (0.190.27): only the root's files, `docs/`'s own files, `docs/sessions/` and `docs/notes/`
are on disk: the session record, the resume pointer, the held notes and all five trackers Step 0 probes. A full
checkout of a repository with thousands of files costs minutes: in a 20 000-file test repository the full one took
46 s and the sparse one under 2 s. `read-tree` is not optional: `--no-checkout` leaves the index empty, and without
it every file reads as deleted.

- **Before reading or writing any other path in `<worktree>`** (a second tree Step 2 searched, a tracker location
  the runner approved under Step 0) → `git -C "<worktree>" sparse-checkout add <its folder>` **first**. A tracked
  file outside the sparse checkout is **absent, not empty**: an append to it creates a file holding only the new
  lines, and committing that replaces the whole file. `git -C "<worktree>" ls-files -t -- <path>` printing `S`
  means it is outside. If you already wrote such a file, delete it, add the folder, and write again.
- **It printed `worktree:` but not `sparse: ok`** (most often a Git too old for `sparse-checkout set --cone`; give
  git's own message) →
  `git worktree remove --force "<worktree>"` and `git branch -D docs/session-<date>-<slug>`, then the full checkout
  once, which takes the new `<worktree>`. Say that it was a full checkout, and why:

  ```bash
  t=$(mktemp -d)/session; t=$(cygpath -m "$t" 2>/dev/null || echo "$t")
  git worktree add -b docs/session-<date>-<slug> "$t" <start point> && echo "worktree: $t"
  ```
- **It printed `worktreeConfig: was unset`** → remember that for Step 6: the sparse settings switch that
  repository-wide setting on, and Step 6 switches it back off.
- **The git guard is on** → it asks the user to approve the branch name, because `docs/session-…` is not one of the
  project's branch types. That prompt is expected; say so beforehand.
- **`git worktree add` fails** → do not retry another way. On a branch that is not protected, write in the session's
  checkout and commit nothing. **On a protected branch, write the files into a new folder outside the repository**
  (`mktemp -d`) and give its path in Step 6: `branch_before_edit` holds there. Say why in Step 6 either way.

Every write in (a) to (d) goes into `<worktree>`, at the same paths relative to the project root, so the tracker
append lands in the default branch's copy. `Written at:` in (c) still names the **session's** checkout and sha.

### (a) `docs/sessions/YYYY-MM-DD-<slug>.md`

**If `docs/sessions/` does not exist, propose creating it** — say what it is for and let the runner decide.
Do not create it silently, and do not skip the record without saying so.

Sections:

```markdown
# Session: <what it was about>

**Date**: YYYY-MM-DD · **Branch**: <branch> · **Version**: <if the project has one>

## What was done
## PAUSED
## Findings filed
## Not done / left open
## Scope caveat
```

- **PAUSED** uses this literal form, verbatim, because `/initialize` and future readers match on it:

  ```
  PAUSED: <task> — subject: <record key> — <why it is stopped; what would unblock it>.
  Do NOT resume without explicit user instruction.
  ```

  `subject` follows `compaction_stop`: the record's key, never a person's name or email, nor a key this
  workspace maps to them; drop it when there is no record. A person with no usable key gets where to find
  them: `subject: (the customer the user pasted; ask again)`.

- **Findings filed** links each item to the section it landed in. If nothing was filed, say *"none — no
  finding worth keeping this session"*, which is a claim rather than a blank. Its last line is Step 1b's state:
  *"look back: work-monitor + retro, <n> candidates, <m> filed"* (or *retro only*), *"look back: skipped"*, or
  *"look back: not delivered"*.
- **Scope caveat** states what this record does **not** cover. A record written from a compacted context
  should say so.

### (b) Append findings to the tracker

Match the tracker's **existing** item shape — read a nearby item and follow it rather than importing a
format. A common one:

```markdown
- [ ] **<headline that is an action or a claim>.** *(<ID>. New <YYYY-MM-DD>.)* <body>
  - **<Bold lede.>** the evidence, measured
  - **Fix direction:** <what would close it>
```

**If the tracker uses IDs, never guess the next number — look it up.** Closed IDs stay in the file and are
cross-referenced, so reusing one makes every existing reference to it ambiguous, and Step 2's search will not
catch the collision because it is in the ID rather than the wording.

Rules — each is a tracker convention, not a preference:

- **Append only.** Do not edit or delete existing items; superseded reasoning is struck through and kept, so
  items only grow. The one exception is the near-duplicate sub-bullet confirmed in Step 3.
- **Land it in an existing section.** If none fits, **propose** a new one — never invent one silently.
- **If the tracker publishes counts** (an index table with an open-item column), and your append changes one,
  **update it in the same edit.** `/initialize` Step 3 diffs that column against live counts and reports
  mismatches as staleness — leave it stale and the next session investigates drift this skill created minutes
  earlier. The two skills form a loop where one manufactures the signal the other reports.

### (c) Overwrite `docs/sessions/RESUME.md`

Keep it **small** — `/initialize` reads it first and its cheapness is the point. Last session date and a link
to its record · a **`Written at:`** line · the push state of the session's work · **PAUSED** items · the 1–3
things the next session might pick up, explicitly marked **unconfirmed**. Overwrite; the dated records are the
history.

```markdown
**Written at:** `<branch>` @ `<sha>` (`git rev-parse --short HEAD` when this file was written)
```

`/initialize` Step 1 checks that sha against the checkout it runs in, which is how a reader on another
machine, or on a checkout that is behind, learns that this pointer describes work it cannot see.

**Never write the commit or push state of this shutdown's own files** — the session record, the tracker
appends, this pointer. Step 6 commits them right after, and the sentence is false the moment it does: *"these files are uncommitted"* gets committed and pushed unchanged, and the pointer then misstates
its own state to every later reader. Those files are listed in the Step 6 hand-off, not here. Push state
means the **session's work**, from Step 1, stated against the `Written at:` sha. Uncommitted work that is
**not** this shutdown's output is listed by path, as existing only in the checkout that wrote the pointer.

### (d) Held notes into `docs/notes/PENDING_NOTES.md` (0.190.23)

Only when Step 1 found held notes. Append each `### ` entry of `<git dir>/pending-notes.md`, in order, to
`docs/notes/PENDING_NOTES.md` in `<worktree>` (or in the folder Step 4 wrote to instead), skipping an entry already
there word for word. With no copy there, create it with `/note`'s header first. It is now a file Step 4 wrote.
Note which entries you carried and which you skipped as already there. Leave `<git dir>/pending-notes.md` as it is:
Step 6 removes entries from it only once they are on `<default>`.

## Step 5 — Verify, both sides

The open-item counts, the file sizes and the re-read are of the files as written: in `<worktree>` when Step 4 used
one. The instruction-file measurement is of the session's checkout, which is what its sessions load.

- **Count open items before and after the append, and report both numbers.** The delta must equal the number
  of items you added. Use `/initialize` Step 2's pattern **including its self-test** — a miscount here is
  written to disk rather than merely reported. Both are here, so that file need not be opened: the count is
  `grep -cE "^[[:space:]]*- \[ \]" <file>`, and first `printf '  - [ ] x\n' | grep -cE "^[[:space:]]*- \[ \]"`
  **must print 1**; if it prints 0 the pattern is broken, and you say that instead of reporting a count.

  **Held notes are not `- [ ]` items:** for `docs/notes/PENDING_NOTES.md`, count `### ` lines before and after
  (`grep -c "^### " <file>`), and the delta must equal the entries carried, less any skipped as already there.

  **Count every file you wrote to.** An append routed into a second tree does not move the main tracker's
  count at all, so checking only the main file returns a delta of 0 and reads as *"the append failed."*

  **Use `-E` and escape the brackets.** `grep -c "^- [ ]" <tracker>` is a different command: `[ ]` is a
  character class matching one space, so it matches **nothing** and returns a silent `0` — the dead-check
  shape this pair exists to prevent, sitting inside the check itself.
- **Report the size of every file you appended to — every run, not only when it is over** (`wc -c <file>`).
  A run that prints no size reads exactly like a run where nothing was measured.

  **Flag it explicitly past the ceiling `/housekeeping` §4c sets** — ~256 KB today — and name the
  consequence, not the number alone: beyond it a session cannot read the tracker in one pass, so every later
  reader works from a slice it may mistake for the whole.

  **§4c owns that threshold and this is a second copy of it, deliberately** — a flag without its consequence
  is unactionable at the point of use. The cost of the copy is that the two must move together: change the
  ceiling here and change it in `/housekeeping` §4c in the same edit. A project set up for both tools holds
  each skill twice, under `.claude/skills/` and `.github/skills/`, so that is up to four places.

  **This reports; it never blocks.** Append anyway. This skill exists because findings evaporate, and
  refusing to write over a size threshold loses the very finding it was built to keep. The remedy — moving
  **history** out into a dated companion file — is the user's decision and belongs to `/housekeeping` §6e.

  **There are three states here and only one of them is silent by accident — so name all three.** Silence is
  indistinguishable from having measured a small file, and two of these look like nothing happened:

  | state | what to print |
  |---|---|
  | appended to a tracker | `<path>, <n> bytes` + the flag if it fired |
  | **tracker resolved, nothing appended** — Step 3's rejections, or no finding worth keeping | `<path>, <n> bytes, nothing appended this run` |
  | no tracker resolved (Step 0's branch) | `no tracker resolved — no size to report` |

  **The middle row is the one a size-report bullet forgets**, because "report the size of what you appended
  to" is vacuously satisfied by an empty set.

  **Bytes, deliberately.** This ceiling is about how much a session can read in one pass, so `wc -c` is the
  right tool and there is no locale trap here. Do not try to reconcile it with `/housekeeping` §4d's
  instruction-file figures — those are **characters**, against a different limit.
- **Measure the instruction files every session loads — in characters, every run.** The tracker figure
  above is about reading; this one is about what the platform loads into **every** session. Claude Code
  warns past **40 000 chars per file** and past a **120 000-char total** (the floors — both limits scale up
  on a 1M-context session, so flag at the floor, never at the figure you happened to see). The total is how
  a project's `CLAUDE.md` grows past the limit **one session at a time**, with every single write looking
  small. Report three things:

  1. **The instruction total** and the three largest files. The set is `CLAUDE.md`, `.claude/CLAUDE.md`,
     `CLAUDE.local.md`, `AGENTS.md` **when either `CLAUDE.md` imports it**, and **every** `.claude/rules/**/*.md`.
     A rule with no `paths:` loads at launch; a rule **with** `paths:` loads as soon as the session reads a
     matching file and counts from then on — so a rule scoped to source code counts in almost every
     working session. Narrowing `paths:` is not a way to get under the limit.
  2. **Every one of those files this session grew**, with the growth in characters — compare against the
     session's base from Step 1 (`git show <base>:<file>`), not against memory.
  3. **Whether this run itself wrote to one of them.** It should not: Step 4 writes the session record, the
     tracker and `RESUME.md`, and none of those is loaded every session. If a write landed in an instruction
     file anyway, name it — that is how a session log turns into a per-session cost.

  ```bash
  python - <<'EOF'
  import glob, os
  text = lambda f: open(f, 'rb').read().decode('utf-8', 'replace')    # bytes -> chars, CRLF kept
  fs = [f for f in ('CLAUDE.md', '.claude/CLAUDE.md', 'CLAUDE.local.md', 'AGENTS.md') if os.path.exists(f)]
  fs += glob.glob('.claude/rules/**/*.md', recursive=True)   # every rule: a scoped one counts once loaded
  n = {f: len(text(f)) for f in fs}
  print(len(n), 'files,', sum(n.values()), 'chars')
  for f, c in sorted(n.items(), key=lambda x: -x[1])[:3]:
      print(c, f)
  EOF
  ```

  **Decode from bytes, as above — never `open(f).read()`.** Python's default text mode turns every CRLF into
  one character, and the platform counts both: a delivered `CLAUDE.md` it reported at *112.4k* reads 111.2k
  that way. Drop `AGENTS.md` from the set if neither `CLAUDE.md` nor `.claude/CLAUDE.md` `@import`s it. **The platform's total also
  includes the user's own `~/.claude/CLAUDE.md`**, which this figure does not — say so beside the number.

  **Flag, never block — the same as the tracker bullet above.** Past either floor, name the file, the figure
  and the remedy: **`/housekeeping`** §4d measures and names the remedies — rationale out of a rule, a
  procedure into a skill, history out of `CLAUDE.md` into a session record or `docs/`. Narrowing a rule's
  `paths:` does not reduce the total. Silence here reads exactly like a small file, so print the line
  even when everything is under.
- **Re-read the finished session record WHOLE — not the diff.** Contradictions between a new paragraph and an
  older one are invisible in a diff and obvious in the finished file, because what they contradict has not
  changed.
- If the project has a health check or gate and this session touched anything it covers, run it **in the
  session's checkout** and report the result. The sparse `<worktree>` is missing most of the repository, so a
  check run there reports failures that are not real.

## Step 6 — Land the record, then hand off

**Land only the files Step 4 wrote, from `<worktree>`.** Skip this part where Step 4 wrote no worktree (not a git
repository, or `git worktree add` failed): nothing is committed there. **Type the worktree path into every command;
an empty path makes git act on the session's checkout.**

**1. Stage and check.**

```bash
git -C "<worktree>" add -- <each file Step 4 wrote, by path>
git -C "<worktree>" diff --cached --name-only    # exactly that list (a file that came out unchanged is absent)
```

- **Any other path in the list** → do not commit. Leave the worktree as it is, and report its path and the paths.
- **The list is empty** (every file came out identical to the start point's copy) → nothing to land; say so, then
  `git worktree remove "<worktree>"` and `git branch -D docs/session-<date>-<slug>`, then part 4's
  `worktreeConfig` step.

**2. Commit, on the session branch.**

```bash
git -C "<worktree>" commit -m "docs(sessions): <date> <slug>"
```

Use the project's own commit format where `AGENTS.md` or `CONTRIBUTING.md` names one (`commit_conventions`).
**The commit fails** (a hook, no `user.name`, signing) → stop: leave the worktree, report its path and git's message.
From here on the record is safe whatever the push does: the commit is on a branch.

**3. Push.** One of three, decided before pushing:

- **No remote** → push nothing. The record is on the local branch `docs/session-<date>-<slug>`; say so.
- **The git guard is on** (`.claude/scripts/git_scope_guard.config.json` exists, or Step 4's branch name drew its
  prompt) → it denies a push to a protected branch by design, so do not try one:
  `git -C "<worktree>" push -u origin docs/session-<date>-<slug>`, and report that branch for the user to land.
- **Otherwise** → `git -C "<worktree>" push origin HEAD:<default>`.
  - **Rejected as non-fast-forward** (the default branch moved since the fetch) →
    `git -C "<worktree>" fetch origin && git -C "<worktree>" rebase origin/<default>`, then push **once** more. A
    rebase that stops on a conflict → `git -C "<worktree>" rebase --abort`. A conflict or a second rejection → the
    session-branch push below.
  - **Refused for another reason** (branch protection on the remote, a server hook, a guard this skill did not
    detect) → `git -C "<worktree>" push -u origin docs/session-<date>-<slug>`, and report that branch for the user
    to land. Open no pull request.
- **A session-branch push fails too** (offline, credentials, the name already taken on the remote) → nothing left
  the machine. Do not retry. Report that the record is committed on the **local** branch
  `docs/session-<date>-<slug>`, with the sha and git's message, for the user to push.
- **Never** retry with `--force`, push another branch, or merge. **Name a remote branch as the record's home only
  after the push to it succeeded.**

**4. Clean up.** **Held notes (Step 4 (d))** → remove from `<git dir>/pending-notes.md` exactly the entries that
are now on `<default>`, and no other line: those skipped as already there (they were in the start point's copy), and
those carried, **only when the record landed on `<default>`**. Another session may have added an entry since Step 4,
so never rewrite the whole file. A record left on a session branch, local or remote, or not committed → keep the
carried entries; the next `/shutdown` skips them once that branch is merged and removes them then. This applies also
when the staged list came out empty. The file is outside the working tree, so no branch rule applies.

The commit is on a branch, so the worktree can go:

```bash
git worktree remove "<worktree>"
```

- **It refuses** (files in it that were never committed) → leave it, and report its path. Never add `--force`.
- **It went, and Step 4 printed `worktreeConfig: was unset`** → switch the setting back off, unless a
  `config.worktree` file still needs it: another worktree's sparse settings, or, in a bare repository with
  worktrees, the `core.bare` setting git moved into the shared one. "In use, kept" is then correct, not a failure:

  ```bash
  c=$(git rev-parse --path-format=absolute --git-common-dir); ls "$c"/config.worktree "$c"/worktrees/*/config.worktree 2>/dev/null | grep -q . && echo "worktreeConfig: in use, kept" || { git config --unset extensions.worktreeConfig && echo "worktreeConfig: unset again"; }
  ```
- **The record landed on `<default>`** → the session branch has served: `git branch -D docs/session-<date>-<slug>`.
  **Landed anywhere else, or nowhere → keep the branch.** It is the only ref holding the record.

If the session's checkout is on `<default>` with a clean tree, `git pull --ff-only` brings the record in. Otherwise
say that the checkout gets it on its next pull from `<default>`. Do not switch the session's branch.

Report:

- the files written, by path;
- the before/after open-item counts from Step 5;
- **the look-back's state** (Step 1b), as the record's *Findings filed* gives it;
- **the size of each file appended to**, with the over-ceiling flag if it fired (Step 5) — or the
  `no tracker resolved` line, so the reader can tell a small tracker from an unmeasured one;
- **the always-loaded instruction total** (Step 5), in characters against the 120 000 floor, the files this
  session grew and by how much, and any file over 40 000 — flagged, with `/housekeeping` named as the remedy;
- **the push state from Step 1** — unpushed commits, or no upstream, named as such;
- **any finding that had nowhere to go** (Step 0's no-tracker branch), so it is not silently lost;
- **held notes** (Step 4 (d)): how many were carried, skipped and removed from the held file, and how many are
  still held, and why;
- **where the record landed:** the commit and the branch it is on (`<sha>` on `<default>`; or on
  `docs/session-<date>-<slug>`, saying whether that branch is on the remote or only local, and why), or
  **explicitly, that nothing was committed**, with why and where the files are (a worktree or folder left in place
  is named by its full path) for the runner to commit;
- **a worktree left in place after Step 4 printed `worktreeConfig: was unset`:** that `extensions.worktreeConfig` is
  still on, and that once the worktree is removed, `git config --unset extensions.worktreeConfig` switches it off;
- **the tidy-up check** (Step 1c), one line per signal. **In a light run with anything stale or done items to
  archive, end the report with it:** *"`/shutdown full` would refresh the changelog and the system map, and offer
  triage of 12 done items."* That names what a full run does; it is not an offer to run it now.

Then **stop**, or in a full run go on to Step 7. Do not commit anything else, do not open a pull request, do not
offer to. A question about committing the session's work is not authorization to commit it.

---

## Step 7 — Full run only: the tidy-up, after the record has landed

**Only in a full run, and only after Step 6.** Typing `full` asked for this work, so it is not this skill's
*its own files only* limit: **each tool runs by its own steps and commits by its own rules** (`branch_before_edit`,
its own branch, never a merge, never a pull request opened). This skill still commits nothing here.

1. **The documents, in one question.** List each document Step 1c found stale (multi-select): *"Refresh which?
   changelog (14 commits behind), system map (3 commits under its roots), roadmap (5 since its build)"*. Run
   `/changelog`, `/system-map` or `/roadmap` for each one picked, in that order, each by its own `SKILL.md`. Nothing
   stale → say so in one line and skip to item 2.
   **A refresh needs the whole repository.** A generator reads sources from anywhere: its notes, and the files its
   entries link to. Run it in the session's checkout or in a **full** worktree, never a sparse one like Step 4's. A
   sparse checkout makes a missing source look like a broken link: a roadmap refresh run with only `scripts/` and
   its output folder gave 15 false warnings (BE-terna-Retail, 2026-11-20). Where its branch is not the session's
   checkout, make the worktree with no `sparse-checkout` (`git worktree add -b <branch> <path> <default>`), and
   remove it after its commit as Step 6 removes its own, never with `--force`.
2. **The tracker, as an offer.** Only where Step 1c found done items or a tracker over its ceiling: *"Run
   `/housekeeping archive` now? It triages with your yes on each verdict, then archives what that closed."* Offer it only
   when Step 6 landed the record on `<default>` **and** the session's checkout is on `<default>` with a clean tree,
   so Step 6's `git pull --ff-only` brought the record in and the tracker triage reads holds this session's findings.
   Otherwise do not offer it, and say why. A record on another branch, or not committed, would mean triage editing
   the tracker that record also edits, and this skill never switches the session's branch to get there.
3. **Report:** for each tool, what it wrote, its branch and its commit, or *not committed* with why. Then stop.

An answer the tool chose (Copilot autopilot) is no choice: take it as *skip*, for both items.

## Non-goals

- **Firing at the end of a task** — see *When this runs*. One run per session, on the user's word; a merged
  PR or a finished task is `list_pending_work`'s trigger, not this skill's.
- **Committing, pushing or merging the session's work** — Step 6 lands the record and nothing else.
- **Deciding alone what is worth filing** — Step 3 confirms first.
- **Rewriting tracker history** — append only.
- **Archive sweeps and triage in Steps 0–6** — re-adjudicating items and moving completed ones out is a separate,
  deliberate operation, and it has a home: `/housekeeping triage` and `/housekeeping archive`, which a full run's
  Step 7 offers after the record has landed. This skill
  appends — plus the one confirmed near-duplicate sub-bullet Step 4b allows — and **removes nothing, ever** (taking landed entries out of the held-notes file outside the working tree is not a
  removal: Step 6).
