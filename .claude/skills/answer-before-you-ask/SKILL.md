---
name: answer-before-you-ask
description: Use before asking anyone for a fact (in a questionnaire, an e-mail, a spec's open questions, a report or a message to the user), before handing the user any step to get past a failure (elevate, restart, reinstall, or any other), or before answering that a fact can't be found, to answer first what you can read. Readable-versus-external examples, and how to ask the questions that survive.
---

# Answer It Yourself Before You Ask Anyone

*The always-on `answer_before_you_ask` rule carries the trigger and the decision. This is its full text, moved here verbatim in 0.139.0 so it loads only when needed. 0.159.0 added the section "A fix you hand the user" and its trigger sentence under Rule, and narrowed the boundary line under "What this does NOT fire on" to questions. 0.166.0 added the section "An answer that says a fact is unknown" and its trigger sentence under Rule; 0.168.0 added its same-system check.*

## Rule

**Before you ask a person for a fact, answer it from what you can already read** — the repository, the source
or object export, the specification, the connected knowledge base, the tool output in front of you, and,
with a search tool loaded or loadable, the package indexes and the web (the rule says when a query stays a read). Only what
survives that is a real question.

**The trigger is writing the question, not reviewing a list of them.** Any sentence that asks somebody for
information — in a questionnaire, a vendor e-mail, a spec's *Open questions* section, a status report, a
meeting agenda, **or a message to the person you are working with right now** — fires this, whether you
invented the question thirty seconds ago or inherited it. **A second trigger is handing the user a step to get
past a failure**, whose cause is a fact in the same way (*A fix you hand the user*, below). **A third trigger is
an answer that says or implies a fact is unknown**, which hands the question back to the reader (*An answer that
says a fact is unknown*, below).

**The one test:**

> **Is any part of this answerable from something I can open?**

## Test the property, never the addressee

*"It is a question for the vendor"* · *"it is about a third-party component"* · *"only the client knows their
own setup"* — each names **who would answer**, not whether the answer is in front of you. An addressee is never
an exemption.

A third-party DLL, service or API catches people most, and has the clearest tell: **its call sites are in our
code, and the arguments passed across the boundary are the contract.** Every method the codebase invokes,
every value it sends and reads back, can be enumerated without anyone replying.

| Looks external | Actually readable |
|---|---|
| "Does the vendor's library support X?" | Every method the code calls on it, with its full argument list |
| "Can the interface carry several of Y?" | The writer that emits Y — does it loop, or write one? |
| "What does the integration send?" | The exact fields, in order, at the call site |
| "Which version are we on?" | The assembly reference, the lock file, the manifest |

**The same holds for the person you are talking to.** *"Which framework version is this project on?"* and
*"does the importer already handle duplicates?"* are readable — and the file's answer beats a remembered one.

## A fix you hand the user

An instruction can carry a factual claim as surely as a question does. *"Restart the session as administrator"*
says *"this fails because the session is not elevated"*. When you hand the user any step to get past a failure
(elevate, restart, reinstall, log in again, re-run a setup step, change a setting, or any other), **the cause is
the fact, and the same test applies to it**. A *fix* here means any such step, whether or not it is sure to
work. Handing the step over feels like the careful move, because you did
not act yourself. But an unchecked cause only moves the cost onto the user.

Where to look before you name the cause:

- **The project's own scripts and docs** for this step: a setup or container script, a README section, a
  troubleshooting note. They often say which route this project uses.
- **The session records and the git history of the setup.** If an earlier session did the same step without
  the fix you are about to propose, the real question is what changed since then.
- **The error itself.** Read all of it, not only the line that matches a remedy you know. A table or comment
  that maps a symptom to one cause is a shortcut for the common case, not a diagnosis.

When you have checked it, **name the cause with what showed it, and the step's own costs**:

- ❌ *"Blocked: this session is not elevated. Restart Claude Code from an elevated terminal."*
- ✅ *"`docker ps` is refused. The project's container script says this account reaches Docker without an
  elevated session once its pipe-access step has run, and earlier sessions did. So the access was most likely
  lost, for example after a reboot. Re-running that step should fix it; it asks for elevation itself, through
  its own prompts. Restarting this whole session elevated isn't needed."*

The case is from a delivered NAV project. The ❌ was sent, and the user had to point at the script. The ✅ is
what reading that script and the session records would have produced.

When you cannot check it, **state the step as a condition, never as the cause**:

- ✅ *"`docker ps` is refused, and nothing I can open says how this account normally reaches Docker. If it
  usually needs an elevated session, restart Claude Code elevated. If it has pipe access that was lost, re-run
  the step that grants it. Do you know which this machine uses?"*

## An answer that says a fact is unknown

*"It can't be looked up"*, *"that isn't available"*, *"there's no way to tell"* — each is a question handed back to
the reader, and usually it follows one failed command. The failure is a fact about that command
(`no_fabrication`); the question is still open. Before you write it, look where this project has recorded the
fact before:

- **The project's files and docs** — config, scripts, README, `docs/`, and its session notes (`docs/sessions/`)
  if it keeps them.
- **The tool's own records for this project** — Claude Code keeps each project's session transcripts (`.jsonl`
  files) and its `memory/` folder under `~/.claude/projects/<this project's folder>/`; Copilot CLI keeps its own
  session history. **Search them for the fact** — the command that used it, the name, the setting — rather than
  reading them whole.
- **Never another project's transcripts or memory.** They are other work, often another client's, and
  `no_personal_data_recorded` applies to anything you copy out of them.

A fact found in a record is what was true then. Say where it came from and when, and check the live system when
the fact can change — an account, a credential, an installed version (`no_fabrication`) — unless it is production
(`production_is_read_only`): then say the fact is unverified. **Check that the record is
about the same system** — the same host and port, database or instance — not only the same kind of thing: a test
server's account says nothing about the one the app uses. A record that names no host or port (a default, an
environment variable) has not said which system it was; treat it as unknown until you know what those resolved to.
If nothing held it, say what you searched; that answer is honest and useful, and *"it can't be looked up"* is
neither. Either way, name the check that would settle it.

- ❌ *"The tool can't look up the superuser name."*
- ✅ *"Fifteen earlier session transcripts for this project connect with `-U postgres`, but all to throwaway test
  servers on their own ports, not the app's server on 5432 — so they don't settle it. Signing in to the app's
  server would, if it is not production: shall I try `postgres` there? The lookup command itself failed (exit 11); that is a separate
  problem worth fixing."*

The case is from a delivered project (2026-10-27). The ❌ was sent, with the agent's fallback value after it, and
the user had to point at the earlier sessions. Those sessions had used `postgres` on the agent's own test servers;
the app's server turned out to use the same name, which was confirmed only by signing in to it.

## What this does NOT fire on

Three kinds of question are not requests for a fact, so the test never reaches them:

1. **The problem or the direction** — *"What are you trying to solve?"* · *"Which of these two shapes do you
   want?"* `discussion_first` asks these **first, before you research anything**; nothing here delays them.
2. **Permission** — a merge approval (`pr_merge_approval`), a change to state shared beyond this repository
   (`external_tool_state_change`), a production-signalled target (`production_is_read_only`, which takes no yes),
   anything outward-facing or hard to reverse (`discussion_first`'s outward-facing clause). **Reading the code never
   substitutes for a confirmation.** A step you hand the user to get past a failure is not a request for
   permission: checking its cause is this rule's job, and any confirmation the step needs is still asked for.
3. **A decision, a preference or a priority only that person can make** — what the business wants, what it
   will pay for, when it wants it. No source predicts it.

**For a question, the boundary is what it asks FOR — a fact, versus a decision, a preference or permission** —
never who it is addressed to.

## Two more that survive, and how to ask them

Fact requests where you looked and nothing you can open holds the answer:

- **Data in their system that you cannot reach** — setup rows, configuration, master data (an export contains
  no rows). **First ask what the answer would change;** if nothing, drop the question. **Production or client data for
  development, design, testing or diagnosis is never requested** (`production_is_read_only`): invent the data, design
  so it does not matter, or let the user decide. Work done for that client asks the client, never by SQL on production. From a non-production source you can reach, run the query yourself;
  otherwise ask for a named extract — *"these five columns of that table"* — not an open question.
  - A value fixed by law or a technical specification is **not theirs to confirm** — look it up. A setting you
    need only to **test** is test preparation — set it yourself and say what you set.
- **Behaviour or intent that nothing we hold records** — what a regulator's validator actually accepts, what a
  vendor plans to ship next quarter, an error string nobody has captured.

Anything else is ours.

## Split the question before you ask it

Most questions that feel unanswerable are **partly** answerable — usually the larger part. Answer that part,
ask only the remainder, and **say what you established**.

- ❌ *"Does the vendor interface support the new reporting calls?"*
- ✅ *"The collection call exists and we already invoke it — here is the method and its arguments. What we
  cannot see is whether the submit call also performs the other record-keeping step. That is the question."*

**Apply it hardest where the question leaves the team** — a document that reaches a client, a vendor or a
regulator costs them an investigation, arrives days later, and cannot be taken back once sent.

*Rationale and neighbouring rules: `docs/rules-rationale/answer_before_you_ask.md`.*
