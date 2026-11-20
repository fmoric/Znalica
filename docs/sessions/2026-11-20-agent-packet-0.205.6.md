# Session: agent packet 0.205.6

**Date**: 2026-11-20 · **Branch**: `master` (work on `chore/deliver-packet-0.205.6`, merged and deleted locally; still
on the remote) · **Version**: ai-agent-templates 0.205.6

## What was done

- **Re-delivered the developer packet, 0.199.3 → 0.205.6** (PR #14, merge `604da6c`, commit `853f9d0`).
  - Full re-scaffold with every recorded answer kept. The changed-paths shortcut was not open: the 0.199.4 and
    0.199.5 changelog entries carry no `Delivered paths:` line.
  - 23 files changed, none added or removed. They are 7 skills on both tools (`shutdown`, `pr-merge-approval`,
    `work-monitor`, `housekeeping`, `hand-off-a-change`, `retro`, `leftover-processes`), the three guard docs and
    their test scripts, `MCP-SETUP.md` (stamp and Serena note) and `.agent-packet.json`.
  - `.claude/settings.json` gained 8 exact-command `permissions.allow` rules (the three guard test suites and the
    leftover report, as `Bash(...)` and `PowerShell(...)`) and 2 `ask` rules. Its 7 hook entries and 23 skill
    listings were already exact.
  - `.mcp.json` is unchanged; the knowledge server stays in `Documents\.mcp.json`.
  - The other 159 packet files re-rendered byte-identical. Only the project's own rule, `.claude/rules/znalica_specifics.md`, was left as it was.
- **How it was rendered:** `/deliver-packet` ships no renderer, so the session wrote one in its scratchpad. Rendering
  the 0.199.3 library with it first reproduced all 180 delivered files byte for byte, after four fixes (T13). Only
  then was 0.205.6 rendered.
- **Checks at delivery** (183 files written):
  - 0 unresolved placeholders (the Actions literal found); frontmatter 130/130; no `\r\r\n`; `changelog: through` 6/6.
  - The `testing` rule's glob matches 46 files.
  - MCP wiring resolves; the read-only agents hold only knowledge's five read tools; `MCP-SETUP.md` has 10 server sections.
  - Guard suites 186/186, 20/20, 45/45, each run alone in the pre-allowed form, with no permission prompt.
  - Fire tests: search guard `deny`; waiting check `block` on Stop and SubagentStop; hand-back denied once, then passed.
  - Leftover report: none. Preflight: node v26.13.0, Serena 2.1.0 (latest), uv 0.10.6.
- **Pre-commit** over the 23 staged files: no secrets, no debug artifacts; build n/a (no C# staged); no linter
  configured. No code review ran (T14).
- **Two status pings from the coordinator session** were answered; nothing was changed because of them, and the
  merge waited for the user's word in this session.

## PAUSED

none.

## Findings filed

- T13, *Ship a tested renderer with `/deliver-packet`* (`TODO.md` → Open), packet-owned.
- T14, *Say which review a re-delivery pull request gets* (`TODO.md` → Open), packet-owned.
- A sub-bullet on T12, *Check the 0.199.3 hooks where no session has run them yet*: what the 0.205.6 re-delivery
  checked in-session, and what is still open.
- A user memory, outside the repository: write multi-line scripts with the Write tool, not a Bash heredoc.
- look back: work-monitor + retro, 3 candidates, 3 filed.

## Not done / left open

- The next-session checks: on Copilot CLI, `find /` refused and `git status` runs; one subagent per tool adds a start
  and a stop line to `.agent-activity/<date>.jsonl`; `code-reviewer` lists exactly the five knowledge read tools (T12).
- The remote branch `chore/deliver-packet-0.205.6` was not deleted.
- The activity log has three `stop` lines from this session with `agent_type: null` and no `start`. No subagent was
  started, so they are orphan stops; the library tracks this as S2026-11-18-10.
- Tidy-up check: 3 done items not archived; `TODO.md` 7,685 bytes before this run's appends, under the 256 KB ceiling;
  no system map, changelog or roadmap in this project.

## Scope caveat

Written from this session's own context, which was not compacted. It covers the re-delivery and its merge only, not
the coordinator session's work or anything on other branches.
