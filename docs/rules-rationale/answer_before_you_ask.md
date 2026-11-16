# `answer_before_you_ask` — rationale

Moved verbatim out of the `answer_before_you_ask` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Why this fires at writing time and not at review time

A filter that runs over a **list** only ever sees the entries that were in the list when it ran. A question
invented while producing the output goes straight into the output, arriving after the filter and before
anybody reads it. So the check has to sit where the question is born.

**That placement is also what makes it cheap.** Firing here costs a few searches and nothing has left your
hands yet. Firing one step later costs a question already sitting in somebody's inbox, and the only repair
available is a second message withdrawing the first.

This is the same placement argument `branch_before_edit` makes about the write boundary, and the same
name-versus-property argument `pr_merge_approval` and `no_silent_tool_loss` both make: a list of cases is
correct only until a case appears that it does not name, and it fails **silently** — nothing announces that
the gate stopped covering something.

## Relationship to the neighbouring rules

- **`discussion_first` owns the questions this one does not touch**, and the seam is the one drawn above: it
  asks about the problem and the direction, this asks for a fact. Where one message carries both, ask the
  problem question and look the fact up.
- **`no_fabrication` governs what you write once you have looked.** Having read the call sites, state what
  they do and cite them; do not state what the component does beyond what you read. **It also outranks the
  temptation this rule creates** — *"I could not find it, so it probably does X"* is precisely that rule's
  failure, and the honest move is the narrowed question above.
- **`knowledge_before_framework_code` orders the sources.** Where a curated source covers the thing, it comes
  before reading the implementation — and both come before asking a person.
- **`report_what_you_examined` governs the sentence you send.** When you ask a narrowed question, say what you
  checked and how far it got you.
- **Where a packet ships `look_before_you_write`, that is the same discipline pointed at code**: find what
  exists before writing a second copy. This one points it at questions: find the answer before asking for it.

## Source

2026-09-21, on a delivered project. A standing project instruction was already in force — *analyse the
current code state, prove or disprove the client's claims, and do not ask them anything we should have known*
— and had just been applied to a client questionnaire, correctly removing four questions as answerable from
the code, with file-and-line evidence. In the same pass, **two new questions were written into that same
document**: whether a third-party EDI component supported a new statutory reporting call, and whether one
document could carry several billing references.

Both were readable, and reading them took four searches. A single object was the only one in the codebase
that touched the component; it called fourteen methods on one class with every argument visible, the
reporting call among them, already wired. And the writer behind the second question emitted one reference per
row inside a loop over the source rows, with another builder in the same codebase already filling several.
The first question shrank to one sentence about a single remaining step; the second dissolved entirely, and
**what replaced it was more useful than an answer would have been** — the pattern the new code should copy,
already in the codebase.

Two causes, and the rule above is shaped by both. The filter ran over the **inherited list**, so the two
questions written after it were never tested — hence a trigger at writing time. And the instruction's own
wording named *"a question for [the vendor], [the client] or [the authority]"* as legitimate, so a name in
the addressee field read as a category that exempts — hence *test the property, never the addressee*, and
hence every carve-out above is written as **what the question asks for**, never as who receives it.

**A postscript that makes the point sharper than the original telling.** The vendor named in that exemption
list turned out to be the **former name of the team's own company**. So the addressee test did not merely
fail to prove the two questions unanswerable — it named as an outside party an organisation that was not
outside at all, and nobody noticed, because a name in a list is read rather than checked. **A name in an
exemption list is not evidence about anything, including who the name refers to.** The property — *is the
answer readable?* — was the only thing that was ever going to settle it, and it settled it in four searches.

## Source — a fix handed to the user (0.159.0)

2026-10-23, on a delivered NAV project. `docker ps` returned *"Access is denied"*. The packet's container skill
mapped that to one cause, *not elevated*, and the agent told the user to restart Claude Code from an elevated
terminal. The project's own container script documented a route that needed no elevation, and earlier
sessions had used Docker without it. Two reads would have found that. The rule did not fire because nothing in
it was a question: *"restart elevated"* is an instruction. **But it rests on a factual claim about the cause,
exactly like a question does, and that claim was readable.** So the trigger now covers a fix handed to the user
for a failure, and the procedure is in the skill. It is scoped to fixes for a failure, not every manual step,
because that is where the unchecked cause hides: a routine step such as running a migration carries no
diagnosis.

It is written as a property, *a step handed over to get past a failure*, with the verbs as examples only, for
the reason given under *Why this fires at writing time*: a list of cases fails silently on the first case it
does not name.

How it sits beside its neighbours. `no_fabrication` already forbids stating an unchecked concrete claim, and a
cause is one. `trace_before_you_claim` has the same *state it as a condition* move for behaviour claims. Neither
fired, because the claim was never written as a claim: an imperative (*"restart elevated"*) carries its cause
unspoken. This rule names that moment. `no_fabrication`'s pushback clause (*"when the user says how their system
behaves and your trace says otherwise, your trace is the claim in doubt"*) still covers what happens after the
user objects. `agent_handoff` still governs how the hand-off ends (stop, and wait). This rule governs whether
the step should be handed over at all, and with what cause.

## Source — an answer that says a fact is unknown (0.166.0)

An answer can hand a question back as surely as a question can. *"It can't be looked up"* asks the reader to find
it. In a delivered project (2026-10-27) an agent wrote exactly that about a database superuser's name after one
command failed; fifteen earlier session transcripts for that project used a name — on the agent's own test servers, not the app's
server, which turned out to share it (corrected 0.168.0) — and the rule did not fire,
because the reply was neither a question nor a fix.

The two rules split the work: `no_fabrication` says the failed command is a claim about the command, not the fact;
this rule's third trigger says where to look before declaring the fact unknown. The procedure — this project's
transcripts and memory, never another project's — is in the skill, so the always-on text stays one paragraph.

## Field notes moved from the rule body

 The instruction
this rule grew out of named a vendor, a regulator and a client as examples of legitimate questions; an agent
read those names as a category that **exempts** and sent two questions it could have answered in four
searches. That is the documented failure, not a misreading — see *Source*.

## Moved at 0.105.0 tightening

Whatever
survives that is a real question. Whatever does not is work you handed to someone else.

Who would reply is not the test, and it is the wrong thing to reach for first.

Reading an addressee as
an exemption is the documented failure — see *Source* in `docs/rules-rationale/answer_before_you_ask.md`.

Asking them spends
someone else's attention on a lookup you could have done — and the answer that comes back is worse than the
one in the file, because it is remembered rather than read.

They
are named because a reader who has just been told *"look before you ask"* will otherwise apply it where it
does not belong, and two of the three are obligations under other rules.

Nothing here delays them, and a reading that made you
   exhaust the codebase before every clarifying question would produce exactly the
   solo-research-then-finished-plan failure that rule exists to prevent.

This rule is about a fact you could
   have looked up, and none of those is a fact.

It is
deliberately not *who it is addressed to*, because that is the failure this rule exists to stop.

The second version costs the reader one answer instead of an investigation, and it cannot come back as *"you
could have looked."*

A clarifying question in-session costs a
sentence and is answered in seconds.

## Adopted from a delivered project (0.178.4)

The passage *"Something I can open" is every source a loaded tool reaches* was written by a delivered project for its own copy of this rule (BE-HUB-System, 2026-11-03), and its user asked the library to take it. It was never in the packet before. It closes the case of an agent asking the person to go and look for a ready-made tool it could have searched for itself.
