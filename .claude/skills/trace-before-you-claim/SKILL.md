---
name: trace-before-you-claim
description: Use before a reply, mail or document for someone outside the team says what the system does or what a fix will achieve, or before giving the user a root cause to act on, or a design or options built on what the system does — "you get error X", "the cause is Y, so here are the options". The trace procedure.
---

# Trace a Behaviour Claim Before Anyone Relies on It

*The always-on `trace_before_you_claim` rule carries the trigger and the decision. This is its full procedure
(0.151.0).*

## When it fires

Before you write a sentence that says what the system does, will do, or what a change will achieve, in text that
leaves the team:
- a client or vendor mail, or a client document;
- a reply you draft for the user to send onward;
- a spec others outside the team build from.

**Within that text, the trigger is the kind of sentence, not whether a client is named.** It fires on *"X
happens"*, *"you get error Y"*, *"the fix makes Z work"*, *"nothing changes for customers with W"*, *"so it does
not take V"*.

**It also fires before you give the user a root cause to act on, a code-review finding included, or a design or
options built on a claim about what the system does**, whoever owns the code on the path, the framework or a vendor
included (`knowledge_before_framework_code` still governs how that behaviour is sourced). Options that rest on no such
claim, such as a choice of name or of what to do first, do not fire it.

**It fires the same way before you tell the user that a plan or a brief will work**, where that rests on what the
system does. A yes about the brief's text is allowed (*"yes, both are in the brief"*); when the user is deciding
whether it will work, add what is not traced. Handing a brief to an agent is not gated.

- **Not allowed:** *"Both are in the brief, so that is covered."* — with the code on that path unread.
- **Allowed:** *"The brief asks for both. Whether the existing code allows the second is not traced."*

## What a trace is

1. **Where a setting, version, flag or dispatch selects the path, name it first**, with its value and where you read
   it. Two versions of a feature are two paths: a trace on the wrong one proves nothing about the one that runs. If
   you cannot read the value, trace the branches you can and say which you did not. A path nothing selects skips
   this step.
2. **Start where the user or the system starts** — the action clicked, the job that runs, the event — and follow
   every procedure, lookup, filter and gate to the effect the sentence claims. **Reading only the procedure that
   holds the defect is not a trace:** the step that decides the outcome is often before it (a gate that stops the
   call) or after it.
3. **Mark each step:**
   - **read** — you read it, with a source you can point to: `file:line` in code; otherwise the document
     section, screen, record or test run;
   - **setup-dependent** — the path depends on data: which row a lookup finds, which handler a setup names;
   - **not traced** — including a step you reasoned past and tagged `unverified`: the tag does not make it read.
4. **Re-test every finding you set aside earlier as "separate".** Putting it out of scope answered *"do we fix
   it?"*, not *"does it change what I am about to claim?"*. Ask whether it sits on this path.
5. **Trace each case a "who is affected" or "when does it show" claim names, and keep the exact case.** *"Has a
   row X"* and *"has only a row X"* are different claims; so are *"customers with W"* and *"customers with only
   W"*.
6. **Trace a described fix after the fix too.** Would the outcome the text promises actually happen with only that
   change? A second gate with the same fault means the fix needs both.

**Any step not traced, or setup-dependent → the text states the claim as a condition, or the claim is cut.** A
sentence that says nothing about the effect is always available: *"we confirm the exact effect after checking the
setup."*

**A conclusion carries the mark of its weakest link.** A cause resting on one setup-dependent step is setup-dependent,
and so is the design built on it. Tagging the one sentence does not settle what follows from it.

**A hypothesis is not a claim.** *"My best guess is X; reading Y would confirm it"* is allowed, and while diagnosing it
is the normal state.

**What rests on a conditional or a hypothesis waits.** Labelling a cause does not release the design built on it: run
the check that would settle it instead, or offer it if you cannot. If the user asks for the design or options
anyway, give them and say what they rest on.

## Test

> For each sentence that says what the system does: can I point to its path, start to end, with a source for each
> step — and is every case it names the case I traced? For a root cause or design: did I name what selects this path,
> and does the conclusion carry its weakest link's mark?

## Worked example — a gate earlier on the path

A client asks what a fix in the document-sending code means. The defect: a builder procedure looks up a setup row
by comparing two different option lists by position. The first draft said *"a customer with only a 'Retail' row
gets the error 'setup not found'; the fix is one line."* Every symbol existed and each claim had `file:line`.

Traced from the button, the path ran through a **gate earlier on it**, in another unit, that does its own lookup
with the same fault. A customer with only a 'Retail' row never reaches the builder: **nothing is sent, silently**,
not the promised error, and **the one-line fix alone would change nothing.** The gate had been read earlier in the
same session and set aside as a separate issue. A later holding sentence, *"nothing changes for customers with
an 'All' row"*, was wrong again for a customer with an 'All' row **and** a credit-memo row: the case written was
wider than the case traced.

**A language packet may add its own example** of a gate earlier on the path.

## Worked example — the path nobody checked runs

An agent traced a bug through the code that looked right and gave the user a root cause, a design and options. It had
not checked which path runs: one setting selected Version 1.0 or Version 2.0, the default was 2.0, and the trace was on
1.0. On 2.0 the proposed fix would do nothing. Some sentences carried an accurate *unverified* tag, and the
conclusion was still built on them.

- **Allowed:** *"If the setting is Version 1.0, the cause is X. If it is Version 2.0, I have not traced it."*
- **Not allowed:** *"The cause is X"*, followed by a design, with the setting unchecked.

## Not in scope

- **A commitment that claims no cause or effect** — what we will build, what we propose. A sentence about what that
  change *achieves* (*"so the job never takes an unfinished transaction"*) is a behaviour claim, and it is in scope.
  So is telling the user that a plan or a brief will work (*When it fires*).
- **Repeating what the reader said.**
- **A hypothesis labelled as one**, with what would confirm it.
- **Text that stays in the team and brings the user no new conclusion to act on** — a status report, a PR description,
  an internal note. `no_fabrication` and `report_what_you_examined` govern those. A new root cause, or an assurance that a plan
  will work, first stated in one of these is in scope.

## Neighbours

`no_fabrication` checks that a thing exists. `report_what_you_examined` checks that a result carries its scope.
`self_review` reviews work before it is reported done. `knowledge_before_framework_code` covers how external code
behaves. A behaviour claim can pass all four — every symbol real, every step cited, the
draft re-read — and still be wrong about the outcome. This one asks whether the path was traced end to end.

*Rationale and neighbouring rules: `docs/rules-rationale/trace_before_you_claim.md`.*
