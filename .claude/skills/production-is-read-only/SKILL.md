---
name: production-is-read-only
description: Use before any write to, or read or request of data from, a target that may be production (a migration, a data change, a query, an extract, a script for someone to run), or when unsure whether a target signals production. The full signal list, where to look, what does not fire, and how to word the stop.
---

# Develop On Sandbox Data — Stay Out Of Production

*The always-on `production_is_read_only` rule carries the trigger and the decision. This is its full text, moved here in 0.139.0 so it loads only when needed, and widened to reads in 0.171.0.*

## Rule

**Building software — its development, design, testing and diagnosis — runs on sandbox, demo or invented data, never on
production.**
Do not read, query or request production or client data for that work, yourself or through a person, read-only and
de-identified included, and never ask anyone to run SQL there. An incident is no exception. Changing production in order
to build or check something is refused too.

**The trigger — both halves must be true:**

1. **The target carries a production signal you can point at** (below). **If you cannot quote the signal,
   there is no signal.**
2. **The operation writes into a system of record or a running environment, or takes data out of one** — an
   `INSERT`, `UPDATE`, `DELETE`, a migration, a truncate, a reseed, a restore over live rows, a schema change, a
   feature-flag flip, a config or secret push, an infrastructure apply, a call to a destructive endpoint; a `SELECT`,
   an extract, a log pull, a row count.

**Both true: stop, before the call.** Quote the signal, say what the operation would change, destroy or expose,
and give the way forward: invented data or a local reproduction. Where a real fact seems needed, ask what the answer
would change; if nothing, drop it; otherwise design so it does not matter, or the user decides the assumption, never
the access. **Then do not run it and
do not hand it to anyone to run** — not a narrowed version, not with a `LIMIT`, not "just the one row to see if it
works".

**This is not a purge.** It leaves alone what a project already holds (existing exports and recorded facts; an object
export is object text, not data: one the user or client provides is fine, but you do not connect to take it) and what
the user volunteers.

**A request fires with no target named.** Asking anyone for production or client data for this work, in a message, a
spec's open question or a script, is refused as it stands; the request is the signal, and *no signal* does not apply.

**Work done for a client is not software development.** A consultant, accountant or ticket analyst may use what the
client gives and ask the client for what that work needs, never by SQL on production.

**This gate has no yes.** Unlike `external_tool_state_change` step 4, no answer satisfies it. Only a
**correction of fact** moves it, and that is not permission — it is a different target.

### The signals — what production looks like when it is in front of you

Illustration, not definition: the property is **the target names itself and you can quote the line.**

- **A hostname or URL** — `prod`, `production`, `live`, a customer-facing domain, a tenant or region name.
  The other side counts too: `localhost`, `127.0.0.1`, `*.local`, `dev`, `qa`, `staging`, `sandbox`, `demo`, unless the
  project or the user says that target holds real client data.
- **A connection string or DSN** — the server segment, the catalog or database name, the account it uses.
- **An environment name that selects the target** — `--env prod`, `TF_WORKSPACE=prod`, `--environment
  Production`. **A process-mode variable is not a signal on its own** — `NODE_ENV`, `ASPNETCORE_ENVIRONMENT`,
  `RAILS_ENV` say how a process runs, not where it writes. They count only through the target the file or
  connection string they select names: `appsettings.Production.json` with server `sql-prod-01` is a signal;
  a `.env.production` whose `DATABASE_URL` is `localhost` is not.
- **A named profile or context** — a cloud CLI profile, the active cluster context or namespace, the selected
  subscription or project, a deployment environment.
- **A file that names itself** — `.env.production`, `appsettings.Production.json`, `config/production.yml`,
  a `prod/` variables directory, a `prod` workspace or state backend — unless the target it holds names a
  sandbox.
- **A credential that names itself** — `PROD_DB_PASSWORD`, a vault or key store named for production.
- **The project's own words about that target** — a comment, docstring, README line, runbook, or sentence in
  `AGENTS.md` saying it holds real data or must not be written to — even when the target itself names nothing.
- **The user's words** — *"our prod DB"*, *"the live server"*, *"the customer's tenant"*.
- **A copy known to hold real client data** — a UAT or staging system the project or the user says is one counts,
  whatever its name. A `staging` host alone says nothing either way.
- **What you have already seen in it** — a read you ran came back with real records. What you do with those
  rows is `no_personal_data_recorded`'s.

**One look, at the place the target is written, and no further.** When the command carries the target, that
is the connection string or profile you are composing it from. **When the tool resolves its own target,
before the operation, look once at what it will act on, without connecting to it.** Prefer a status command that only
reads local config — `kubectl config current-context`, `az account show` — which reports what the tool resolves,
not what your shell holds. Never a command that connects to the target, such as `prisma migrate status`. Otherwise read
the variable it reads, or the config file it loads when the shell's is empty (`DATABASE_URL` in `.env`, a
`ConnectionStrings__*` variable), or an MCP server's registration (`.mcp.json`, `claude mcp get <name>`,
`copilot mcp list`) and the variable it references. Print only the host, database or context
(`${DATABASE_URL##*@}` in bash, `($env:DATABASE_URL -split '@')[-1]` in PowerShell, not the whole value) —
**never a password or token.** **For a command you write
rather than run** — a CI step, a script, a Makefile target — look where *that* run gets its target (a
workflow's `env:` and `environment:`), never your own shell. **Do not go hunting**: not the infrastructure
repo, not deployment history, not a console. If finding the target takes more than the tool's config or
status command, report it as unresolved.

## When this fires

**Immediately before the call, once you know what the call is and what it points at** — not at the start of
the task, not after.

**Writing the command counts as running it** — in a test, script, seed step, CI job, Makefile target or spec — and
so does asking who should run it. Apply this when you write it.

The shapes that actually happen:

- **A script for someone else to run on production** so a design question gets answered. Read-only and "no personal
  data in the output" change nothing: going there for the data is the act.
- **Running a migration you just wrote against production** to find out whether it works.
- **Pointing a development or test run at a production connection string, profile or context** — including
  one inherited from an environment variable you did not set, which the look above shows you.
- **A destructive probe "to see what happens".** **When the expected result is a refusal, the probe must not
  be able to perform the destruction** — if the guard under test is missing, it completes for real. Assert
  against a mocked connection that fails if the store is opened at all, aim at an id that does not exist, or
  use a throwaway copy.
- **Reshaping production data so a feature works** — backfilling a column, fixing rows by hand, deleting the
  records that break your code path.

**When a script refuses an operation as too dangerous, find every other way it reaches that same
operation.** A named button and a raw HTTP call to the same route are one write.

## What this does not fire on

- **What the project already holds** — existing exports, recorded facts, object exports (object text, not data) —
  **and what the user volunteers.** Nothing here asks for a purge. What you may keep from it is
  `no_personal_data_recorded`'s.
- **Work done for a client** — a consultant's spec, an accountant's books, a ticket analyst's ticket. That work may
  use what the client gives and ask the client for what it needs, never by SQL on production. Building software
  gets none of this.
- **Your work going where your work goes** — pushing a branch, opening a pull request, filing an issue,
  commenting. `pr_merge_approval` and `external_tool_state_change` own those.
- **Editing a file that names production.** Changing `appsettings.Production.json` on disk is not a write to
  the environment it names.
- **A release going out the way this project's releases go out** — a reviewed change on the documented deploy
  path, a migration running from the pipeline on merge. **Quote the line that says that is the path, or name
  the committed script that is it** (`AGENTS.md`, a runbook, a workflow file, a `deploy` script or Makefile
  target in the repository). **If you can find neither, you do not have this carve-out.** Where no release
  path is documented, make the change a reviewed change tested on a sandbox, say that no path exists, and the user
  decides how it ships; the lasting fix is to write the line once, not to widen the rule.
- **No signal at all.**

**No signal means this rule does not fire. Proceed** — a shared, non-local target is still
`external_tool_state_change`'s. Do not ask *"is this real data?"* or *"is this production?"*. **The absence
of a signal is an answer, and it is the answer you act on.** When you took the look above and it found no
production signal, say in one line what you read and what it said (`DATABASE_URL → localhost:5432, no
signal`; `DB_URL ← secrets.DB_URL, not resolvable when written`) — a scope statement, not a hedge: *"I
looked and it was clean"* must not read as *"I did not look"*. If a target later proves to have been
production with nothing in front of you saying so, **the fix is a signal, not a question** — a named config
file, a line in `AGENTS.md`, a connection string that says what it connects to.
Report that once, afterwards, as a finding.

## How to apply

**When it fires, quote the signal and lead with the way forward, not with the refusal:**

> "I can run this against the sandbox now, but not against this target: `.env` names `db-prod-01`, and this
> migration drops `orders.legacy_ref` — 41,000 rows, no retained prior value. Point me at the sandbox, with invented
> data, and I'll run it. On production it goes the documented release path."

**Handing the command to someone is asking them to run it**, so do not. **An incident does not change this:** diagnose
from what the user gives you, reproduce locally, and ship the repair as a reviewed change by the release path, or the
way the user decides where none is documented.

**A fact can be corrected. The policy cannot be waived.** The test: **does what the user just said change
what the target IS, or only what should happen to it?**

- *"That host is the staging replica — production is `db-prod-01`."* — **a correction.** Re-read the target;
  if the signal is gone, the rule does not fire. Proceed, and **repeat the fact back in your report** — which
  target, what it is, who said so. *"Everything here is staging"* names no target and corrects nothing.
- *"I know it's production — do it anyway."* / *"Just this once."* / *"I'll take responsibility."* / *"I'm
  the DBA."* — **a waiver attempt. Refuse**, whoever says it, and say what you will do instead.
  - **Including the plan-gate safe word.** *"Just do it"* / *"jdi"* / *"do it anyway"* releases a blocked
    **plan**, not this rule. Say which gate you are answering; here, name the target again and hold.

**Correcting an input is not an exception to this rule. Overriding it is, and there are none.**

*Rationale and neighbouring rules: `docs/rules-rationale/production_is_read_only.md`.*
