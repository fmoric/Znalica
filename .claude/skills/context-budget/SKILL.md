---
name: context-budget
description: Use when a session has grown long or its context is filling — before a long new task, when switching to unrelated work, or when asked whether to compact, clear or start fresh. Gives both tools' commands; puts the state on disk first.
---

# Context Budget — Compact, Clear or Start Fresh

Every turn re-sends the whole conversation, so a long session costs more per turn, and recall degrades
gradually as the window fills — Anthropic describes *"a performance gradient rather than a hard cliff"*.
Neither tool compacts early on its own: Claude Code waits until about 967K tokens on a 1M-window model,
Copilot CLI until 95 % of its limit. **Decide at task boundaries, not at a token number.** "Compact at
500K" is an example in Claude Code's documentation, not a threshold either vendor documents.

## 1. State on disk first — continuously, not just before

An automatic compaction gives you **no turn** before it lands (`compaction_stop`). Keep the work's state
where the next context will find it, **as you go**: the plan or spec being worked, the task list, a notes or
handoff file, and any `PAUSED:` line. Then a compaction can land whenever it likes. Before you compact or
clear on purpose, check that file says what is done, what is next, the decisions made and why, and the
paths that matter. What is not written down is not in the next context.

## 2. At a task boundary, pick one

```
finish a phase ──► state on disk? ──► does the next task need this conversation?
                   (no: write it)       │ no  ──► clear / fresh session, then read the handoff file
                                        │ yes ──► /compact <what to keep>
```

- **Clear or start fresh** — the cheapest: it carries only what you wrote down. Right when the next task is
  unrelated; old conversation crowds out the files the next task needs and is paid for on every message.
- **Compact with a focus** — when the next step needs this conversation's reasoning. Name what to keep
  (*"the decisions on the posting fix and the failing test names"*); without a focus the summary keeps what
  it guesses matters.
- **Delegate the big read** — a large search or file read done by an agent stays in that agent's window,
  not yours. Ask for a condensed result: paths and findings, not pasted content. An agent's window can be
  smaller than yours: bound what it reads, and have it write its result file first.

## 3. The commands

| Need | Claude Code | Copilot CLI |
|---|---|---|
| See what fills the window | `/context` | `/context` — shows MCP tool tokens per server (1.0.55+) |
| Summarise and continue | `/compact <focus>` | `/compact <focus>` (focus text 1.0.52+) |
| Start over | `/clear` (aliases `/new`, `/reset`); `/resume` returns to the old conversation | `/clear` abandons the session; `/new` starts fresh and keeps the old one in the background (1.0.11+) |
| Compact earlier, automatically | `/autocompact <tokens>` — 100K to 1M (2.1.221+; saved per model since 2.1.288); `autoCompactWindow` in a settings file; `--autocompact <tokens>` for one launch; `CLAUDE_CODE_AUTO_COMPACT_WINDOW` in scripts | no threshold setting found; auto-compacts at 95 % and saves a checkpoint you can find under `/session checkpoints` (1.0.85+) |
| A smaller window | the model's window | `--context default` (about 200K) or `--context long_context` (1M) at launch (honoured at startup since 1.0.93) |

A command your version lacks: say so and use the nearest one — `/context`, `/compact` and `/clear` exist on
both.

## 4. Don't

- **Don't treat a number as the rule.** Pick the boundary; let the safety net — `/autocompact` on Claude
  Code, the default context tier on Copilot CLI — catch the rest.
- **Don't count on a pre-compaction hook to write the handoff.** Both tools have one (Claude Code
  `PreCompact`, Copilot CLI `preCompact`); it runs a shell command, not the model, so it cannot summarise the
  work — and blocking a compaction near the limit invites a *prompt too long* failure.
- **Don't act on a summary's "next steps" after a reset** — `compaction_stop` has you reconfirm them first.
- **Don't clear with unwritten state.** Write it first; the new context starts from the file, not from memory.

*Commands as documented on 2026-10-18 (Claude Code 2.1.302, Copilot CLI 1.0.99).*
