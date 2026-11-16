# `trace_before_you_claim` — rationale

Added at 0.151.0. **The rule file is what governs**; read this for why it exists and how it composes with the rules
around it.

## Why

A delivered project (`developer`, C/AL-NAV, 0.146.0) drafted a reply for its client explaining a fix. The draft
said *"a customer with only a 'Retail' row gets the error 'setup not found'"* and *"the fix is one line"*, both
with `file:line` evidence. Both were wrong: a gate earlier on the same path, in another unit, made the same
faulty lookup, so nothing was sent at all, and the fix needed both lookups. The user caught it by asking *"are you
sure?"* — twice. No build broke and no diff was wrong; the defect was a confident, cited, false sentence in text
the user was about to send. The field report:
`docs/assessments/2026-10-19-packet-finding-trace-before-you-claim.md` in the library repo.

Three mechanisms, none specific to that project:
1. **Locality of reading.** The defect sits in one procedure; the outcome is decided by the path. The agent read
   where the defect was and generalised to the outcome.
2. **A set-aside finding is never re-tested.** The gate had been read in the same session and filed as a separate,
   out-of-scope issue. Out of scope answers *"do we fix it?"*, not *"does it change what I am claiming?"*.
3. **Case drift.** *"Has a row X"* was written where *"has only a row X"* was traced.

## The second incident (added 0.175.0)

Field report: issue #269 and `docs/assessments/2026-11-02-packet-finding-root-cause-on-untraced-path.md`, a delivered
project (`developer`, `al-bc`, 0.173.0). An agent gave its user a root cause, a design and options on a Business
Central bug without checking which path runs. A setting selected Version 1.0 or 2.0, the default was 2.0, and the
trace was on 1.0; on 2.0 the proposed fix would do nothing. The user asked why the agent was guessing so much.

The rule did not apply: nothing left the team, and it listed status reports to the user as out of scope. Two more
mechanisms, neither specific to that project:
4. **The selector was never named.** "Setup-dependent" could mark a step, but nothing sent the agent looking for what
   chooses the path before it started reading.
5. **A tag marks a sentence, not a conclusion.** The accurate `derived-from-source, unverified` tags did not stop a
   conclusion built on them, so a conclusion now carries its weakest link's mark.

The scope stays narrow on purpose: only a conclusion the user is asked to act on, because tracing every status line
or routine option list would stop ordinary reporting. A labelled hypothesis stays allowed, because diagnosing runs on
them, but what rests on it waits, or relabelling would reopen this incident. The issue's three new
link states were not taken: the skill already has three marks, and a second vocabulary would compete with them.

## The third incident (added 0.195.4)

On 2026-11-15 a session told its user "both are already in the brief": two things the user wanted, written into
a brief for the developer agent. The developer then found the brief could not work as written, because a setup
check on the path knew only one of the two launch types. The session had read "a commitment that claims no cause
or effect (what we will build)" as covering its sentence. It did not: the sentence said the plan would deliver
both, and that rested on what the system does. The rule now names that sentence among the things traced. A plain
yes about what a brief says stays allowed; what is asked is that an assurance the plan will work is traced, or
worded as not traced. The brief itself was left ungated on purpose (maintainer's decision, 2026-11-15): the agent
that takes a brief reads the code to do the work, and a trace before every brief would repeat that reading in
the main session. One occurrence.

## Why a rule of its own, and not a part of `self_review`

`self_review` fires before work is reported done, and a reply drafted in chat for the user to send is not always
seen as work to report. The trigger here is the sentence, so it fires at the moment of writing. The project that
found this had already written a local rule in the same form; this is that rule, made generic.

## Neighbours

- `no_fabrication` — a thing exists. A sentence can name only real things and still be false about the outcome.
- `report_what_you_examined` — a result carries its scope. Here the scope is the path, and it has to be complete.
- `self_review` — the finished artifact is re-read. Re-reading a draft does not trace the system it describes.
- `knowledge_before_framework_code` — where a claim about *external* code gets its source. This rule asks whether the
  whole path was traced, and that path may run through external code.
- `write_for_the_reader` — a draft the user sends onward is held to this rule (its obligation 2).
