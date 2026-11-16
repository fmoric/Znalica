# `no_personal_data_recorded` — rationale

Moved out of the `no_personal_data_recorded` rule (both mirrors) at 0.105.0 (later sections name the version that added them), to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Why

Two reasons, and either alone is enough.

**The legal one is not negotiable and is not yours to trade.** Personal data may be processed for a stated
purpose, kept no longer than that purpose needs, and no more of it than that purpose needs. A copy of a
customer record living in a repository as a test fixture has no stated purpose, no retention, no erasure path
and nobody accountable for it. Nobody in the conversation has the standing to authorise it, which is why this
rule takes no override.

**The practical one is what actually bites.** Personal data in a repository is not one problem, it is a
permanent one: it survives the branch, the fix, the release and the person who added it, and it turns every
later routine act — publishing the repo, onboarding a contractor, restoring a backup, answering an erasure
request — into an incident. And **you never need it.** The value's shape is what the test needs, and the
shape is free.

## Source

Established at the packet library maintainer's instruction as a standing obligation, rather than from a
single failure in that library's record — the source is external and it does not expire. **Do not go
looking for an incident to justify it**; the obligation stands without one.

The nearest field evidence is indirect, and worth naming for its shape rather than its subject: TM #114 — the
incident behind `production_is_read_only` — is a case of a **working database full of real records** being
written to by a test its author believed was safe. It is the same database whose rows become a fixture the
next afternoon, and the same asymmetry: a moment of convenience against a consequence with no undo.

## Relationship to the neighbouring rules

- **`production_is_read_only` is the sibling and the seam is exact.** That rule refuses going to production
  for development, reads and writes alike. **This rule governs what may be kept out of what you read.** They are
  independent in both directions: a customer's email pasted into a bug report fires this one and never goes
  near production, and a migration run against the production database fires that one and touches no personal
  data. Where both fire, both apply — neither defers.
- **This rule blocks one act that `external_tool_state_change` would otherwise merely ask about: writing
  personal data into shared state.** A memory file, a knowledge-base contribution, an issue on a tracker
  other people read. Named by the act and not only by the rule: **that rule's step 4 is a question a yes
  satisfies, and there is no yes here.** Its steps 1–3 still run; the write does not.
- **`commit_conventions` owns the shape of the commit message and this rule owns what may not be in it** — a
  customer name in a subject line, an email in a body, a ticket paste in a trailer. The two never disagree,
  because nothing in a well-shaped message needs a real person in it.
- **`no_fabrication` governs the substitution.** Synthetic values are correct; **presenting them as captured
  is not.** Say which they are, in the artifact, where the next reader will see it.
- **`discussion_first` does not reach this.** Its residual clause ends in a confirmation. This one ends in a
  substitution, and the substitution is not a decision the user is being asked to make.
- **`compaction_stop` mandates a write, and this rule would have made it useless** (review O4, settled
  0.117.0). For halt state the person *is* the content, so substitution does not transfer: a synthetic name
  in a `PAUSED:` line points at nobody. The settlement is a different pointer rather than a precedence order
  — the pause note's `subject` carries the **record's key** (ticket, order, invoice), which resumes the task
  without holding the person.
- **`client_confidentiality` (accountant) needs client data written, because that is the deliverable**
  (review O5, settled 0.117.0). An accountant workspace can be a neutral research drop box or a client
  engagement, and only the delivery knows which. So the exemption is a **home the delivery names**
  (`CLIENT_FILES_PATH`), not a permission anyone in the conversation grants — which is why *"takes no
  override"* still holds. **The exemption is the working copy only** — the delivery gitignores the folder,
  because a commit or push is a destination of its own, and handing that to a conversational yes would
  reopen the collision one level down. A pattern for any future packet whose deliverable is personal data:
  name the home at delivery, keep it out of version control, and keep every other destination under this rule.

## Settled at 0.118.0 — one trigger (review O7)

**The trigger picks what to look at; the list picks what to remove.** The rule used to state its trigger two
ways. *"Both true: do not write it"* read as pure provenance: nothing imported may be written, so a production
stack trace could not go in a bug report, which blocks routine debugging. *"What counts as identifying, once
provenance says to look"* read as classifying values, where the design avoids classifying anything you did
not import (see *Half 2 asks about provenance* below). The settlement keeps both steps and orders them.
Provenance still bounds the work, so nothing you did not import is ever classified. Within an import, the
identifying list decides what comes out, and the rest may be written. The rule names where identifying values hide in a trace or capture (messages, paths,
URLs, headers, tokens, free text), because an explicit list of what may be written would otherwise suppress
the noticing. An import that cannot be scrubbed reliably, such as an image, is described rather than written.
**A value you are unsure about counts as identifying**, so a doubtful call costs a substitution rather than a
leak. A confident misjudgement still leaks, which is why the list is explicit.

The combination clause now says **what the destination, or anyone who can read it, already holds**. Read as
*"what you already hold"*, it covered the whole import, name included, so every frame identified someone and
the permission to write the rest meant nothing.

## Field notes moved from the rule body

**The blast radius, and it is why this fires before rather than after.** Nothing has persisted yet, so a
false stop costs one substitution. A miss is mostly irreversible, and asymmetric in a way that does not feel
irreversible at the time:

- **A commit that removes an email does not remove the commit that added it.** History is the artifact.
- **A pushed commit is not recalled by a rewrite** — clones, forks and mirrors already have it.
- **A log line is at the collector before you finish the sentence**, and its retention is not yours to set.
- **A memory or notes file is read by every later session**, including sessions run by people who were never
  part of this one, for as long as the file exists.

## Moved at 0.105.0 tightening

These explain lines that still stand in the rule. The move left them as sentence fragments; 0.118.0 restored
each one to the point it explains, wording kept where it survived.

**Half 2 asks about provenance, not sensitivity.** *"Is this personal data?"* is a judgement you have to make
about a string, and a judgement is exactly what gets skipped under time pressure. *"Where did this string
come from?"* is something you already know, because you were holding it a second ago.

**Files you did not author.** The same test applies to a file **you did not author** that you are about to
commit or push. It catches regressions: a fixture that was synthetic when it was written gets regenerated from
a real export six months later by someone who was not thinking about this.

**The collaboration-identity carve-out** is stated as a category, not as a tolerance. Without it the rule fires
on the first commit and gets switched off.

**The conversation is not a recording destination.** If it were, this rule would forbid discussing what a user
volunteers in a bug report, which nothing else refuses.

**Not telling the user off.** A rule that lectures on every message gets muted before the message that
mattered.

**Substitute, preserving shape.** The fixture has to keep testing what it tested, so the synthetic value keeps
the real value's shape. Finding that shape is the work.

**Say which values are synthetic.** `no_fabrication` governs this: a synthesized trace presented as a captured
one is worse than either.

**If it already landed, enumerate.** Enumeration is the whole value of the disclosure. Whoever has to erase
the data can only reach what you name, and you are the only participant who knows where it went.

**A notes or memory file is a destination with no natural prompt.** Nothing about writing one feels like
publishing, no diff shows it, and no review reads it — and it is read by every future session.
