# `list_pending_work` — rationale

Moved verbatim out of the `list_pending_work` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Field notes moved from the rule body

Observed failure this exists to prevent — a report after a merge:

> **Pending:**
> 1. Nothing uncommitted or unpushed — tree clean, `main` in sync with `origin/main`
> 2. No failing tests reported this session
> 3. `docs/TODO.md` §0 — gates 2 and 3, the installer, the deferred connectors

Only 3 was pending. Items 1 and 2 were checks that came back **empty** — the opposite of pending — presented
as numbered entries under the heading **Pending**. Correct form:

## Moved at 0.105.0 tightening

Passages cut from the rule body when it was tightened; verbatim from the pre-tightening text.

**From *An item the source marks as someone else's*:** **The cross-check above does not catch this, which is the whole reason this section exists.** It asks whether
the item is still in the file — and it is. The note is right there, saying exactly what it always said.
**What changed is your paraphrase of it**, and nothing in the file will ever contradict you.

**Same section:** This is the mechanism,
and it is the half worth fixing: a qualifier lost in the **first** paraphrase is inherited by every
restatement after it, and no later step re-reads the note. Each list is generated from the last message
rather than from the tracker, so the loss compounds while every individual step looks careful.

**Same section:** putting it there makes it look like
something still awaiting a decision, when the decision was already made and recorded.

**Same section:** A list of kinds is correct
only until the next kind appears, and it fails silently.

**Same section:** *"Nothing pending"* on its own reads identically whether
you excluded three items on the evidence or never looked.

**From *The usual case*:** **Why this matters more than formatting.** The user directs from this list. A list they must first filter
fakes out of is worse than a shorter true one, and a visible self-contradiction — "pending: nothing" — costs
the credibility of everything else in the message.

**From *The other middle*:** **The ranking is not the problem; the missing denominator is.** Three items chosen out of a hundred is more
useful than a hundred dumped unranked. But three presented without the hundred reads as *"this is what is
left"*, and the reader plans against a backlog that does not exist.

**Same section:** **A denominator you
recalled is worse than none** — it is a fabrication carrying a number, and a number convinces better than
the vague version, which makes it worse.

**From *Exception*:** Here the check ran and every source was genuinely
empty, so there is nothing to direct from and the question is the honest ending.

**Same section:** This is the most dangerous place for a partial look, because
here the missing coverage produces an *all-clear* rather than a short list.

## Added at 0.141.0 — *A quick question is not a full report*

Field report: `docs/assessments/HANDOFF_2026-10-19_templates-quick-question-overread.md` (knowledge-mcp-server,
`developer` 0.135.0). Mid-session the user asked *"anything else for this pc to fix?"*. The agent had already read
the resume pointer and the last session record, which held the complete answer. It went on to read a 1,745-line
tracker section by section, and the user stopped it. The answer it then gave from what it already held was the
same answer. Nothing in the rule limited *every source to its end* or the denominator count to full reports, so
the agent applied them to a status question.

The section must decide two cases:
- **Allow:** a mid-session "anything else to fix?" answered from the resume pointer, which names it as the
  source and offers to scan the tracker.
- **Still forbid:** an end-of-work "nothing pending" after half the tracker was read without saying so. That is a
  full report, so the section does not reach it.

`report_what_you_examined` and `no_fabrication` are unchanged. They require the scope to be **named**, not
exhausted, and a quick answer that names its sources meets both.
