---
name: initialize
description: Use at the start of a session (/initialize) — reports tracker, git and gate state, what is pending, and what it did not read. Fetches, then writes nothing; pairs with /shutdown.
---

# Initialize

A **start-of-session orientation**: where this project stands, what is open, and what the last session left
behind. It replaces the ad-hoc *"read some of the backlog and guess"* that opens most sessions.

**Writes nothing to the working tree, the index, local branches or any remote — by instruction, not by
structure.** This skill needs a shell to read `git` and to count, and a shell can write. Read-only is earned by
*omitting* the tool, and this skill cannot omit it. So the restraint is yours: **run only the commands named
here, and do not redirect output into any path.** **One named exception: Step 1 (0)'s `git fetch`**, which
updates remote-tracking refs and git's own object store, and changes nothing you work with. Without it, every "behind", "newest pointer" and
"pending branch" below is computed from refs that may be days old, and reads exactly as current.

**The command blocks are written for Git Bash**, which Git for Windows ships. A session with only PowerShell runs
each block unchanged through Git's own bash — `& (Join-Path (Split-Path (Split-Path (Get-Command git).Source))
'bin\bash.exe') -c '<block>'`, which counts as running the commands named here — never a bare `bash`, which on Windows can be the WSL launcher. With no Git Bash,
say which checks could not run; do not improvise them. **A value one block prints goes into a later block as text
you type**, shown in angle brackets: the shell keeps no variable from one call to the next.

**The hard part is not gathering. It is honesty about coverage.** A real tracker is usually far too large to
read in one pass, and reading a slice while reporting as though it were the whole is the failure this skill
exists to prevent. So: **count exactly, read narrowly, and state the gap with a number.**

Sibling: `/shutdown` — the other end, which writes the session record.

---

## Step 0 — Resolve the tracker FROM THE PROJECT

Everything below hangs off this. Get it wrong and the report is confident, numerate, and about the wrong
file.

**1. Anchor to the project root.**

```bash
git rev-parse --show-toplevel 2>/dev/null || pwd
```

Resolve every path below against that, and **say which of the two you used**. **Never resolve a path against
this skill file's own directory.** If this project also has the templates plugin installed, a copy of the
*template repository's own backlog* sits next to the plugin's copy of this skill — and a relative path
resolved from there reads that backlog instead of yours. It parses, it counts, and every number is somebody
else's.

**2. Probe for a tracker.** The command lists every candidate that exists; it exits non-zero whenever one is
missing, which is the usual case, so read its output and ignore its exit code.

```bash
ls TODO.md docs/TODO.md BACKLOG.md docs/BACKLOG.md ROADMAP.md 2>/dev/null
```

**3. Act on what came back — four branches, and only the first is the ordinary one:**

- **Exactly one** → that is the tracker. **Name it in the report**, with how it was found.
- **Two or more** → **name them all and ask which is authoritative.** Never pick silently; the runner knows
  and you do not.
- **None** → **do not refuse.** Skip Steps 2–4 (the counting and reading steps) but **still do Step 1** —
  the resume pointer lives with the session records and does not depend on the tracker, so a previous
  session's hand-off may well exist. Then Steps 5 and 6, and report that half, **saying which candidates you
  probed and that none resolved**. A git-and-gate orientation is genuinely useful on its own. **It is not
  "nothing pending"**, and must not be written in a way that reads like it: you did not check a backlog, you
  failed to find one.
- **Any resolved path containing a `plugins/cache` segment** → **stop and say so.** That means step 1 failed
  and you are looking at an installed template snapshot. Continuing would report another project's numbers
  as this project's.

**4. Note the sessions directory.** `docs/sessions/` is the convention this pair uses. If it is absent, say
so once — its absence is normal in a project where `/shutdown` has not run yet, and `/shutdown` will
**propose** creating it rather than creating it silently.

## Step 1 — Locate the newest resume pointer, then read it (cheap)

`docs/sessions/RESUME.md` is small by design and written by `/shutdown`. **The copy in the working tree is not
necessarily the newest one.** A checkout that is behind, or a pointer committed on a branch not merged here,
leaves an older pointer on disk, and the older pointer reads exactly as authoritative as the newer one. So a
fetch and four read-only checks, (a) to (d), come first.

**(0) Fetch.** Every check below reads remote-tracking refs, and those are only as new as the last fetch. A
caveat such as *"refs as of the last fetch"* does not help: it cannot say whether that was an hour ago or a
week, and a reader takes *"0 behind"* as a fact. So fetch first:

```bash
f=$(git rev-parse --git-path FETCH_HEAD)
if [ -s "$f" ]; then echo "last fetch: $(date -r "$f" '+%Y-%m-%d %H:%M')"
elif [ -e "$f" ]; then echo "last fetch: unknown (FETCH_HEAD empty since $(date -r "$f" '+%Y-%m-%d %H:%M'))"
else echo "last fetch: never"; fi
if [ -z "$(git remote)" ]; then echo "NO REMOTE"
else GIT_TERMINAL_PROMPT=0 GCM_INTERACTIVE=never git fetch --all --prune && echo "FETCHED"; fi
```

Run the fetch with a timeout of about a minute if your shell tool takes one. `GIT_TERMINAL_PROMPT=0` and
`GCM_INTERACTIVE=never` make a missing credential fail instead of waiting at a prompt nobody sees. They do not
cover an SSH passphrase or host-key prompt, so **a fetch that times out is a failure**, like any other. `--all`
fetches every remote, so a fork's `upstream` is as fresh as its `origin`. `--prune` removes remote-tracking refs
whose upstream branch was deleted; (d) below depends on it. The fetch changes no local branch, the working tree,
or the remote. The first line is read before the fetch: it is the age of the refs you would otherwise have
used. A failed fetch leaves `FETCH_HEAD` empty with a fresh timestamp, which is why an empty one reads as *unknown*, not as the
time of a fetch.

- **`FETCHED`** → the scope line says *"fetched just now"*.
- **`NO REMOTE`, or any failure** (no network, refused credentials, a timeout, or the fetch was denied) → **the
  report's first line, above the scope line, is: "NOT FETCHED (`<reason>`) — every ref below may be stale; last
  fetch `<time from the first line above>`."** With no remote, there are no remote-tracking refs to be stale:
  say so, and (a)–(d) report on local branches only. Otherwise continue: (a)–(d) still run on the old refs, and
  every result they give carries that qualifier. Do not retry, and do not soften it into a scope-line footnote.

**(a) Where this checkout stands.**

```bash
d=$(git symbolic-ref -q --short refs/remotes/origin/HEAD || for b in origin/main origin/master; do
      git rev-parse -q --verify "$b" >/dev/null && { echo "$b"; break; }; done)
echo "default: ${d:-not determined}"
[ -n "$d" ] && git rev-list --left-right --count "$d"...HEAD   # <behind> <ahead> vs the default branch
git rev-list --left-right --count @{u}...HEAD   # <behind> <ahead> vs upstream (errors if none)
```

**Note the name printed after `default:`** — it is `<default ref>` in every later command, typed in.
`origin/HEAD` is absent from many clones, which is why the fallback exists. **`default: not determined`** → say the
default branch was not determined; the first count is skipped by its guard (with an empty name it would print
`0 0`, which reads as "level"), and skip every command below that takes `<default ref>`. If the project names an
integration branch that work merges into before the default branch (`AGENTS.md`, or a
`docs/process/INTEGRATION_BRANCH_GATES.md`), run the same count against `origin/<that branch>` and report both.
**Behind → say so before reporting anything the pointer says.**

**(b) The newest pointer across the refs this clone has.**

```bash
git for-each-ref --format='%(refname:short)' refs/heads refs/remotes |
  while read r; do git log -1 --format="%ct %h $r" "$r" -- docs/sessions/RESUME.md; done |
  sort -rn | head -3
```

Compare the top line's commit with `git log -1 --format=%h HEAD -- docs/sessions/RESUME.md`. If they differ,
read the newer one with `git show <ref>:docs/sessions/RESUME.md`, and **say which copy you read and that the
working tree holds an older one.** Name that ref's branch and whether it is merged into the default branch
(`git merge-base --is-ancestor <ref> <default ref>`). A ref that is the integration branch named in (a), or is merged
into it, describes work **integrated but not yet on the default branch**: say that, not "in review". Any other
unmerged ref, such as the top of a stack of open pull requests, describes work still in review. **A
working-tree pointer with local modifications** (`git status --porcelain docs/sessions/RESUME.md` prints it) is newer than any commit: read that one, and say it is uncommitted.

**When the newest pointer is not the working tree's, that is the headline.** Put it first in *Where we stand*,
and treat the working-tree pointer as **superseded**: its next steps, PAUSED marks and branch list are history,
not pending work. Build pending rows from the newer pointer. If you could not read the newer one, say that the
pending list is unknown, rather than falling back to the older pointer's. **A checkout that is behind** (a) is
headline news too, even when no newer pointer exists: the pointer may still be current, but the code under it is
not.

**(c) The pointer's `Written at:` sha.**

```bash
git cat-file -e <sha>^{commit}         # fails → that commit is not in this clone
git merge-base --is-ancestor <sha> HEAD   # fails → this checkout does not contain it
```

The first failing means it was never pushed, or not fetched here: the last session's work is not visible from
this checkout. The second means it exists but this branch does not include it. **Either way, say it in the
scope line.** Any uncommitted paths the pointer lists belong to the checkout that wrote it and cannot be
verified from here. **No `Written at:` line** → the pointer predates the field: say so, and rely on (a) and
(b).

**(d) Which local branches are still open.**

```bash
git for-each-ref --format='%(refname:short) | %(if)%(upstream)%(then)%(upstream:short) %(upstream:track)%(else)no upstream%(end)' refs/heads
git merge-base --is-ancestor <branch> <default ref>; echo "exit=$?"   # repeat against origin/<integration branch>, if (a) found one
```

`exit=0` is contained and `exit=1` is not. **Any other exit, or a `fatal:` line, means the check did not run** (a
name that does not resolve): say *could not check*, never *not merged*.

**Judge "merged" against the remote-tracking refs, never against `HEAD` or a local branch.** The fetch moved
`origin/*`, not the local default branch, so a local `main` that is 31 behind makes every branch merged
upstream look unmerged. Step 5 reuses this result.

- **Contained in `<default ref>` or the integration branch** → **not a pending row**, even if the pointer lists it as
  open or unpushed. Name such branches in one sentence outside the list, as merged, and say the local copies
  can be cleaned up.
- **`[gone]` and not contained** → its upstream was deleted, usually after a squash merge, which
  `--is-ancestor` cannot see — but also when a pull request was abandoned or the branch was deleted by mistake.
  **Not a pending row, and never a cleanup suggestion:** name it in one sentence as *"deleted upstream, not
  contained in `<default>`: check before deleting the local copy."* In a single-branch clone every tracking
  branch shows `[gone]`; if all of them do, say so and draw nothing from it.
- **No upstream, not contained** → never pushed, or pushed without tracking. Still pending.

If (0) did not fetch, all of this reflects only what an earlier fetch saw: a branch merged or deleted upstream
since then still looks open, so carry the NOT FETCHED qualifier.

Then read the pointer.

**Everything in it is unconfirmed.** A previous session's "next steps" are a proposal that did not survive
into this session: present them as *"the last session proposed X"* and **never auto-proceed on them** (the one exception, the
user's own standing instruction to start ready work, belongs to `work_while_waiting`). If it
names **PAUSED** work, carry that word into the report — paused work must not be presented as available.

If the file is absent from the working tree **and** (b) found it on no ref, say so in the scope line. Absence is normal; it is not an error, and it is not
"nothing pending".

## Step 2 — Count. Do not estimate

**Self-test the pattern before you trust a zero.** This costs one command and it is the difference between
*"no open items"* and *"my counter is broken"*:

```bash
printf '  - [ ] x\n' | grep -cE "^[[:space:]]*- \[ \]"    # MUST print 1
```

If that prints `0`, the pattern is broken — **say that instead of reporting a count.** The fixture is
**indented on purpose**: the common regression is `^- \[ \]`, which drops every item nested one level and
returns a *plausible non-zero* total rather than an obvious zero. Against this fixture it prints `0` and
announces itself.

Then count, against the tracker Step 0 resolved:

```bash
grep -cE "^[[:space:]]*- \[ \]" <tracker>     # open
grep -cE "^[[:space:]]*- \[x\]" <tracker>     # done
```

**Two traps, pulling opposite ways — the pattern above is the only form that clears both.**

- **`^` with no `[[:space:]]*` undercounts**: genuine open items are often nested one level under a parent.
- **No anchor at all overcounts**: the literal string `` `- [ ]` `` appears mid-line in prose in any document
  that explains its own checkbox convention.

**If the project keeps a second tree of items** — per-theme or per-area files alongside the main tracker —
count those too and **sum them**, then report both numbers and the total:

```bash
grep -cE "^[[:space:]]*- \[ \]" <dir>/*.md | awk -F: '{s+=$2} END {print s}'
```

`grep -c` over several files prints one `path:count` line per file and **never a total**. Keep the per-file
lines — Step 3 may need them — but the number you carry forward is the computed sum.

## Step 3 — Cross-check any published counts (conditional)

**Run this only if the tracker publishes counts of its own** — an index table with an open-item column per
area, a summary line, a badge. Many projects have none.

Compare the published figures against what you counted and report any drift, naming the rows and both
numbers. **Suspect your own count before you accuse the table:** if a published row disagrees and the
disputed items are ones nested under a parent, your pattern dropped them — re-run with the Step 2 form
rather than filing a drift finding.

**A "no drift" verdict computed from zero counted items is a dead check, not a clean project.** Say that out
loud rather than emitting a pass.

**If the tracker publishes no counts, say the cross-check did not run and why.** A step you skipped is not a
step that passed.

## Step 4 — Targeted orientation read

Read narrowly and deliberately. Good candidates, if the tracker has them: a header or changelog narrative of
what shipped recently; an index or table of contents; a stated priority order or "next up" section.

**Measure what you actually read.** Compute both numbers — never carry them from a previous run:

```bash
wc -l < <tracker>                       # the denominator
grep -nE "^#{2,3} " <tracker> | head -40   # section bounds, to choose from
```

If you stop early, that is allowed — the scope line then says how far you got.

**Do not read every linked file by default.** Count them, link them, and let the runner direct you into one.

**Read the system map, if the project has one** — `docs/system-map.md`, or the map `AGENTS.md` names. It is
what saves this session from re-learning the structure by search. Note its built-at commit; if source
commits have landed since, or no `system-map.json` sits beside it, say the map may be stale and that
`/system-map` refreshes it. No map → say so in
the scope line; do not build one here (this skill writes no files).

**Read the changelog's newest entries, if the project keeps one** — the `## [Unreleased]` section and the
newest release (`CHANGELOG.md`, or the file `AGENTS.md` names). It is the shortest record of what changed
recently. If commits have landed past its `<!-- changelog: through <sha> -->` marker, say how many and that
`/changelog` brings it up to date. No changelog → say so in the scope line.

### The priority-narrative guard

A "priority order" or "what's next" section is usually **a narrative, not a queue** — it accretes
paragraphs about what took the top and when. **A paragraph may enter your report only if it resolves to a
concrete open item.** A sentence naming *"a release"*, *"a migration"* or *"a run"* **without naming which
one** is prose: report it as context or drop it, but **never number it as a task**. A reader who plans
against a lifted narrative sentence is planning against an item that does not exist.

## Step 5 — Git state

Current branch · `git status --porcelain` · local branches not merged · any open pull requests, if a CLI for
that is available. Ahead/behind was computed in Step 1 (a), and which local branches are still open in Step 1 (d),
against the remote-tracking default branch; carry both forward, do not compute them twice.

**If Step 1 (0) did not fetch, every result here inherits its NOT FETCHED line.** A stale ref prints the same
reassuring "up to date" as a fresh one.

**A branch with no upstream is the dangerous case**, because `git log @{u}..HEAD` *errors* rather than
printing anything — so a reader skimming for output concludes nothing is outstanding. Name which state you
are in: no upstream, ahead by `<n>`, or in sync.

## Step 6 — Gate and version state (conditional)

If the project has a health check, a linter gate, or a test command that is cheap to run, run it and report
**hard failures and advisories separately**. If it has a version manifest, report the version.

**If it has neither, say so** — that is a fact about the project, not a pass.

Anything you assert about **external** state — what a remote carries, what is installed, what is newest —
is re-verified now or hedged in the sentence. Such claims go stale in minutes.

## Step 7 — Report

Fixed shape. **Order matters — the scope line comes first**, so nothing below it can be read as broader than
it is. **The one line allowed above it is Step 1 (0)'s NOT FETCHED line**, when the fetch failed.

**1. Scope line.** What you read, what you did not, with numbers. Model:

> *Resolved the tracker as `<path>` (`<how>`); read its header and priority order — **`<lines-read>` of
> `<total>` lines**. Counted **`<n>` open items**; I did not read the bodies. The pattern self-test returned
> 1, so the count is live. Resume pointer read from `<ref>` (newest of `<n>` refs; the working tree's copy is
> `<older | the same>`), written at `<sha>`, which `<is | is not>` in this checkout; fetched `<just now |
> NOT — see the first line>`, this branch `<n>` behind `<default>`. — or: `docs/sessions/RESUME.md` is absent — no prior session record. System map:
> `docs/system-map.md`, built at `<commit>`, `<n>` source commits since. Changelog: `CHANGELOG.md`, `<n>`
> Unreleased entries, `<n>` commits past its marker.*

**Every `<…>` is a value you computed this run.** None of them is a constant to copy from this file.

**2. Where we stand.** Version · branch · behind/ahead from Step 1 (a) · tree state · gate result · anything
the resume pointer carried, marked **unconfirmed**, with where that pointer came from · whether the user last said
they are away or remote and has not said they are back (this session or loaded memory), with where that came from,
marked **unconfirmed** when it came from memory, so `work_while_waiting`'s away notification is live from the first
turn.

**3. Pending.** Numbered rows are **pending work only**. A source that came back **empty is not a pending
item** — it goes in **one sentence outside the list**. Writing *"1. Nothing uncommitted"* under a **Pending**
heading is a self-contradiction that costs the credibility of everything else in the message. Anything drawn
from part of a source carries its denominator: *"…the areas this touched; `<n>` open items overall, which I
have not read."*

**4. Hand control back.** Do not ask "what's next?" when something is pending — present the list with one
recommended next item, asked as a question (`list_pending_work`); the runner decides. An open "what's next?" is
only honest when **every** source was checked, exhausted, and genuinely empty.

### Prohibited in the report

- **A bare `✅` in a checklist row.** A tick is a verdict, and this failure concentrates in that form.
- **An unqualified "nothing pending"** — it reads identically whether you checked three sources or none.
- **Any denominator you recalled rather than computed.** A fabrication carrying a number convinces better
  than a vague claim, which makes it worse.
- **Any priority-narrative paragraph promoted to a task** without an item behind it (Step 4).

---

## Non-goals

- **Writing anything** beyond Step 1 (0)'s fetch. `/shutdown` is the writing half. Enforced by instruction,
  not tool omission — the restraint is yours to keep.
- **Reading the whole tracker.** Deliberate: counts plus a stated gap beat a partial read presented as
  complete.
- **Deciding what to work on.** It recommends; the runner decides.
