---
name: retro
description: Use for a retro or retrospective on a session — what to change in hooks, rules, docs.
---

# Retro — Improve the Environment, Not the Session

A retro looks back at one session and asks what in the **environment** — the files, checks, rules, skills and
tools the agent works with — would make the next session go better. The session itself is over; its mistakes
are evidence. This skill **suggests and changes nothing**: every candidate goes to the user, who decides.

## 1 — Read the session

Default: the current session. An earlier one only when the user names it (a transcript file, a session log, a
pull request). Read the primary record, not a summary of it. If part of the session was compacted, the summary
is all that is left of that part: say which part, and that its candidates rest on a summary — or ask the user
for the transcript file. Note each place where the agent:

- searched for the same file, fact or command more than once, or opened files that did not hold the answer
  before finding the one that did;
- made a mistake that someone or something caught later — the user, a reviewer, a failing build;
- ignored, misread or contradicted an instruction it had;
- ran a command in one form that failed, then in a form that worked — the spelling, a flag, the order of steps,
  a host or tool that is not reachable here;
- made a slow or expensive call (a huge output, a repeated search, a long-running command);
- lacked information it needed and guessed, asked, or stopped.

Done when every such place is listed with where it happened (the message, the command, the file).

## 2 — Turn each place into a candidate

Match each place to the cheapest fix that would have prevented it. Prefer a fix the agent cannot skip over one
it must remember:

| Category | Fits when | Typical fix |
|---|---|---|
| **Automated check** | The mistake has a fixed shape — a banned call, a file in the wrong place, a missing test, a format | A linter rule, a pre-commit hook, a CI step, a test. First look for an existing check that is unwired or broken: fixing it beats adding one. No pre-commit hook and no CI running the build and tests is itself a finding |
| **Navigation** | Finding the right file or fact took long | One pointer line where the agent already looks (`AGENTS.md`, a README, a skill) naming the file and when to open it |
| **Instruction text** | An instruction was missed or misread, or says nothing the agent would not do anyway | Reword it to name the behaviour wanted; give a step a condition that says when it is done; delete a line that changes nothing; move a long procedure out of an always-loaded file into a skill it points to |
| **Review standard** | A judgement call slipped through review — consistency, fit with surrounding code | A line in the project's review standard, read at review time rather than loaded every session |
| **Tool economy** | A call was expensive for what it returned | A narrower command, a filter, a script that returns only the signal |
| **Information access** | The agent lacked data a person could have given it | Read access to non-production logs, a sandbox service or the documentation — an MCP server, a log file, a saved query (never production: `production_is_read_only`) |

**A fact about the environment is a candidate only when the session proved it** — the failing form and the
working one are both in it. Propose it as a **Navigation** fix: the working form and when it applies, not the
story of finding it. Its target is a project file (the README, the project's own docs) — the fact is this
project's, not the packet's — with a pointer from where the agent looks only if that place is project-owned too.
A fact the session only suggests is not proposed. Never propose a result as a fact — "it is
done", "the tests pass", "verified": those were true once, and a later session would skip the check on their word.

**Check who owns the file a fix would change.** A file the agent packet delivered — listed under `files` in
`.agent-packet.json` at the project root (`AGENTS.md`, the agents, rules and skills) — is written over at the next
re-delivery, so a local edit there is lost. Mark such a candidate **packet-owned** and propose it for the packet's
maintainers instead: `/assess-delivery` carries it to them in its *For the library* section. A project file
(its linter config, CI, README, own docs) is the project's to change.

Done when every place from step 1 has a candidate, or a stated reason it has none (a one-off, already fixed), and
every candidate's target is marked packet-owned or project-owned.

## 3 — Report

Candidates ranked by **cost of the failure** (a wrong result first, then lost time, then tokens). For each:

- what happened, with the evidence from the session;
- the fix, and the file or setting it would change — packet-owned or project-owned;
- what it costs: the lines added to an always-loaded file, the check to maintain.

State what you read — the session, its length, any part you skipped or knew only from a summary. Then ask
which candidates to act on, as a question, recommending the top one. Applying a fix is new work the user starts;
this skill ends at the list. An answer the tool chose for the user — Copilot autopilot answers `ask_user` with a
guess — is not that start: name the pick, start no fix (the list is this skill's end), and follow `write_for_the_reader` obligation 6's
autopilot bullet.

*Adapted from [Matt Pocock's retro skill](https://github.com/mattpocock/skills) (MIT); "no pre-commit hook and no CI
is itself a finding" is his.*
