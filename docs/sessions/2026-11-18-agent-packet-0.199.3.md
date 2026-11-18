# Session: agent packet 0.199.3, its assessment, and the testing-rule glob

**Date**: 2026-11-18 · **Branch**: `master` (work on `chore/deliver-packet-0.199.3`, `docs/assess-delivery-2026-11-18`,
`chore/fix-test-glob`; all merged and deleted) · **Version**: ai-agent-templates 0.199.3

## What was done

- **Re-delivered the developer packet, 0.195.16 → 0.199.3** (PR #11, merge `fc534dd`, commit `b128a35`).
  - Full re-scaffold with the recorded answers, plus the new `activity_log` option, on.
  - 39 files changed and 10 are new: the `list-pending-work`, `work-while-waiting` and `work-monitor` skills, and the
    activity log's 4 files.
  - `.claude/settings.json` gained the activity log's two hook entries and three name-only skill listings. The search
    guard's two commands now carry the 0.199.2 `COPILOT_HOOK_DEPTH` gate.
  - `.mcp.json` is unchanged. `MCP-SETUP.md` was restamped.
  - `hand-off-a-change` was written over. Its local edit (PR #7's proof-runs fix) is upstream since 0.197.2; the two
    differed only in line wrapping.
  - Verified at delivery: hook tests 186/186, 20/20 and 45/45; search-guard fire test `deny`; frontmatter 130/130; 0
    unresolved placeholders over 176 files.
- **Next-session checks, Claude Code, after the restart:**
  - the search guard refused `find /`;
  - the activity log wrote one start and one stop line per subagent;
  - `code-reviewer` sees exactly the 5 knowledge read tools.
- **Ran `/assess-delivery`** (PR #12, merge `83c1756`): report `2026-11-18-deliver-packet-other-assess.md` at the
  project root. Result: 1 🔴, 0 🟡, 2 🟢, and 3 library findings, sent to the ai-agent-templates session.
- **Fixed its 🔴 F1** (PR #13, merge `d90edc4`, commit `b72e3ef`). `TEST_GLOB` `Assets/Tests/**/*.cs` matched 0 of 153
  `.cs` files, so `.claude/rules/testing.md` never loaded. It is now `Assets/Znalica/Tests/**/*.cs` (46 files), in the
  rule and in `.agent-packet.json`. Made as a re-render with that one answer changed, not a full `/deliver-packet` run.
- **Corrected a claim during the assessment:** a text search first reported this folder as missing from Copilot's
  `trustedFolders`. A JSON read showed it listed (an exact match); the report carries the corrected result.

## PAUSED

None.

## Findings filed

- **T12** (`TODO.md` → *Open*): check the 0.199.3 hooks on Copilot CLI, and the waiting check live in Claude Code.
- **T3** (`TODO.md` → *Open*), sub-bullet: a fresh clone gets no knowledge server, because its entry sits in a
  `.mcp.json` above the repository.
- **Not filed here:** the assessment's three library findings, which belong to ai-agent-templates and went to its
  session. They are listed in the report's *For the library* section:
  - a recorded glob is offered back without being run;
  - a re-wrapped file is flagged as edited;
  - no step covers a parent-folder `.mcp.json`.

## Not done / left open

- T12's checks, which need a Copilot CLI session and a live Claude Code turn.
- The Copilot side of the read-only agents' tool probe: the runner declined the Copilot prompts.

## Scope caveat

- Written from this session's own context. It covers the delivery, the assessment and the glob fix, and no game code.
- The session spanned one restart (the user restarted Claude Code to load the new hooks). The checks after it were
  made in Claude Code only.
