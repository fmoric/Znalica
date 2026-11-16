# `production_is_read_only` — rationale

Moved verbatim out of the `production_is_read_only` rule (both mirrors) at 0.105.0 (the *0.114.0* section is
newer, written for that release), to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Why

Nothing else is positioned to catch this. The repository is unchanged, so no diff, test or review sees
anything. The change is often correct — the migration works, the backfill is right — and the damage is
entirely in *where it ran*. And the person who asked described an outcome, not a target: they said *"apply
the migration"*, and which connection string that resolves to is a fact only you are holding at the moment it
matters.

The asymmetry is the whole argument. Sandbox data can be reseeded in a minute. Production rows that carried
no prior value cannot be reconstructed at any price, and you find out which kind you had **after** the write.

## The second incident (0.171.0)

Until 0.171.0 this rule said reading production was allowed, and it fired on writes only. In a delivered project, for
two design questions, an agent wrote and merged a raw SQL script and asked who should run it on a client's production
server, although the user had said it would get no database access. Nothing was run and no client data was read. Of its
18 checks, 5 clearly changed the work, 5 were information only and 8 were never traced. The user: *"Access to production
data for development purpose is not permitted and maybe against the law especially the client data. We develop we can
imagine any data we need"*.

Three rules looked at it and none fired: this one keyed on writes, `answer_before_you_ask` recommended a named extract,
and `no_personal_data_recorded` governs only what is written down. So the rule now covers taking data out, by the agent
or through a person, and writing or proposing the script counts. It has no incident exception, read or write. The
user also decided that a known copy of client data counts as production, that work done for a client (consultant,
accountant, ticket analyst) may use and ask for what it needs but never by SQL on production, and that with no documented
release path a fix is a reviewed change the user ships. It is not a purge: *"Not all
production data needs to be purged, but like asking to run data with an SQL script on production, newer in a milion
years"*.

## Source

TM #114, in the packet library's field record. While adding an auth guard to an unauthenticated route whose
`UPDATE` had no `WHERE` clause, an end-to-end script called that route anonymously and asserted it would
return 401. It returned **200**: the fix was on disk but not in the running process, so the call executed
against the live database and set `verified_date` to today for **259 documents**, destroying the counters
that drove a review queue. No backup, no retained prior value, unrecoverable.

The script's own docstring said in capitals that it deliberately did not click the "Verify All" button,
because doing so against a real database would destroy the review queue — and called the identical endpoint
over HTTP three lines later. The browser path was guarded; the wire path, which is the same write, was not.

Its own verdict, and this rule's: **the failure is not "forgot to restart" — it is using a live destructive
call as the assertion.** Note what fired the rule here, because it is the general lesson: **the record names
no host, no profile and no environment variable** — nothing about the target announced itself as production,
and a signal catalogue that stopped at connection strings would have been silent. **The project's own words
did**, in the file the agent was editing.

## Relationship to the neighbouring rules

- **`external_tool_state_change` and this rule meet on one act and part on the answer.** That rule's trigger
  is *"could something outside this repository behave differently afterwards, for someone I cannot see?"*,
  and its step 4 is **stop and ask** — a question a yes satisfies. **This rule owns one act inside that
  class: going to a system of record or a running environment that names itself as production, to write or for data, for work
  that has not been released.** Named by the act and not only by the rule, deliberately: a hand-off that
  names only a rule reopens silently the moment that rule acquires a condition under which it does not apply.
  Where both reach a command, **steps 1–3 there still run and this rule decides the write** — and this one
  has no yes. Everything else stays with that rule: a **shared dev database**, a **staging** environment, a
  shared cluster that is nobody's production. Those are stop-and-ask, and they always were, unless the target is known to hold real client data, which is this
  rule's (0.171.0).
- **Two exemptions in that rule stop at this boundary, and both are named there as well as here.** A
  **disposable environment you own** is a disposable *environment*, not a disposable *target* — a throwaway
  container holding a production connection string writes to production. And where **provisioning is what the
  repository is for**, its softening of step 4 to disclosure covers the environments that repo provisions and
  a prod apply going out the released way; **it does not cover applying a change you just wrote, from your
  working copy, to a production-signalled workspace, to see what it does.** That one is this rule's, and it
  does not soften.
- **`discussion_first` names the residual class and this rule takes a slice of it.** That rule's second
  trigger covers whatever is outward-facing or hard to reverse and no specific rule names. **The slice taken
  here is development, test and experimental access to a production-signalled target** — and the difference
  is not procedural, it is the answer: that clause ends in a confirmation, and this one ends in a refusal.
- **`no_personal_data_recorded` is the sibling and the seam is exact.** This rule refuses going to production for
  data. **That rule governs what may be kept out of what you do hold** — a row in a fixture, an email in a log, a name
  in a commit message.
- **`cross_repo_guard` and `branch_before_edit` cannot see this at all**, and that is why this rule exists.
  One resolves filesystem paths against the git root — a connection string is not a path. The other asks
  whether a call changes a file on disk — a `DELETE` changes no file. Both are correct and neither is looking
  at the database.

## 0.114.0 — a target the tool resolves for itself

Until 0.114.0 the one permitted look was *"the connection string or profile you are composing the command
from"*, justified by *"you are reading it anyway"* (kept below). That is false for a tool that resolves its
own target: `prisma migrate reset` with `DATABASE_URL` exported by `direnv`, `dotnet ef database update` with
no `--connection`, `kubectl` against the current context, an MCP server holding its own credentials. The
command names nothing, so there was nothing to quote, and *proceed silently* let the write through — the
very case the rule listed as one that *"actually happens"* (review of 2026-08-08, O1, filed as
S2026-08-08-7 in the packet library's tracker).

- **The look moved; the bound did not.** For such a tool, the place the target is written **is** what the
  tool will act on — the variable it reads, its read-only status command, its `.mcp.json` entry. One read,
  before the operation only, without connecting, and the tool's own local status command is preferred because it reports what the tool
  resolves — `prisma` loads `.env` itself, so the shell's `$DATABASE_URL` can be empty while the tool
  connects to production. Not the infrastructure repo, not deployment history, not a console: those are still
  hunting, and so is anything past the tool's config or status command.
- **Only the host is printed.** `echo $DATABASE_URL` prints the password; the rule names the host-only form.
- **A command written for another run looks where that run gets its target** — a workflow's `env:` and
  `environment:` — never the agent's own shell, which would describe a different target.
- **"No signal" now says the rule does not fire, not that the target is safe:** a shared, non-local target
  is still `external_tool_state_change`'s stop-and-ask.
- **Silence kept its purpose and lost its ambiguity.** The rule still never asks *"is this production?"*.
  But *"I did not look"* and *"I looked and it was clean"* used to produce the same report, which is what
  `report_what_you_examined` forbids. A look that named nothing is now one line of scope in the report.
- **Not covered:** a target bound late by CI (`${{ secrets.DB_URL }}` in a workflow) has nothing to read
  when the command is written. The report line now says so (*"not resolvable when written"*) rather than
  *no signal*, but nothing refuses it; that half of S2026-08-08-7 stays open.
- **Process-mode variables demoted (O3, S2026-08-08-8, same tracker).** `NODE_ENV=production npm start` against a local
  database, then a reseed to reproduce a production-only bug, used to be refused: the variable was
  catalogued as a signal. It describes how a process configures itself, not which store it writes to, so it
  now counts only through the target that file or connection string names. The filename bullet gained
  *"unless the target it holds names a sandbox"* in the same change, or `NODE_ENV=production` would have
  come straight back through `.env.production`.

## Moved at 0.105.0 tightening

Passages cut from the rule body because they explain *why* rather than instruct. Verbatim, grouped by the
section they came from.

**From *The signals* — the project's own words:**

> **This is the signal that fires on the incident in the rationale doc**, where nothing about the target named it and the danger was written down
> only in the project's own text.

**From *The signals* — one look, no further:**

> You are reading the connection
> string or the profile anyway, in order to compose the command; read what it says. [...] A value you are
> about to use is not an investigation.

**From *When this fires*:**

> That is also this guard's blast radius: **it fires before anything has changed**, so a false
> stop costs one message and a miss is unrecoverable by construction.

> **A passing test costs nothing and a failing one costs the data**; that asymmetry
> is designed around, not hoped past.

> A named button and a raw HTTP call to the same route are one write. The incident in the rationale doc guarded
> the first and left the second three lines away.

**From *What this does not fire on*:**

> Those write to live services and change nobody's records; `pr_merge_approval` and
> `external_tool_state_change` own them, and they are not development on production.

> If a target later turns out to have been production and nothing in the work in front of you said so, that is
> a labelling failure in the environment, not a check you skipped.
> *(Superseded in part at 0.114.0 — for a self-resolving tool the look can now be skipped. See above.)*

> **Asking anyway is the failure this paragraph exists to prevent.** An agent that asks about every target
> teaches the person answering to wave it through unread, and the one time it mattered it was waved through
> with the rest. This rule earns its authority by being silent when it has nothing to show.
> *(Superseded in part at 0.114.0 — it still never asks, but a look that found nothing is now one line of
> scope. See above.)*

**From *How to apply*:**

> Quote the signal — that is what makes this a finding rather than a mood

> Until 0.171.0: *what someone does with their own credentials is theirs*. No longer: asking them to run it is refused too.

> If the signal is gone the rule does not fire, and it never did: you
> were wrong about the target, not about the policy.

> There is no form of words that turns this into a
> yes, including one from someone who genuinely has the authority to do it with their own hands.

> It does not
> release this rule, because what is blocking here is not a question. **The same three words are a safe word
> against one gate and a waiver attempt against this one**, so say which you are answering rather than
> treating the phrase as universal.
