# Delivery assessment: Znalica, developer packet, language `other` (C# / Unity)

Assessed: the project folder `Znalica`, packet `developer`, language `other` (C# / Unity).
delivered-with: ai-agent-templates 0.205.6 (`.agent-packet.json`, delivered 2026-11-20) · assessed-with: 0.205.6, the
installed plugin (`installed_plugins.json` install record). The two are the same version.

Runtime state: **delivering session (pre-delivery runtime for the launch-bound parts).** This session performed the
0.205.6 re-delivery (PR #14, commit `853f9d0`) and then ran this assessment. Rules, agent bodies and `.mcp.json` bound
when it started, against the 0.199.3 files. The renderer showed every agent and rule file re-rendered byte-identical
in 0.205.6, so for those files the two runtimes coincide. Even so, the evidence below is labelled per line, and a
post-restart re-run is a next step. Claude Code applies settings hooks mid-session, so the guard probe counts as
*in-session*.

Gate (§0): pristine. `git log "853f9d0.." -- <packet paths>` and `git diff 853f9d0 -- <packet paths>` both printed
nothing, over the 17 delivered paths §0 lists. Later commits touched only `TODO.md`, `TODO_ARCHIVE.md` and
`docs/sessions/`.

## What was delivered

- **Manifest:** `developer` over `_base`, language `other`, `rule_decisions: []`, 150 `files` entries, all on disk.
  - Options on: `knowledge_base`, `code_navigation_mcp`, `microsoft_docs_mcp`, `code_graph_mcp`, `search_guard`,
    `activity_log`, `waiting_check`.
  - Options off: `copilot_rule_mirrors`, `library_docs_mcp`, `integration_branch`, `git_guard`, `full_suite_guard`,
    and the four AL-only options.
  - Placeholders: `PROJECT_NAME` Znalica, `LANGUAGE` C# / Unity, `TEST_GLOB` `Assets/Znalica/Tests/**/*.cs`,
    `RUN_CMD` none, and the five `KNOWLEDGE_READ_TOOLS`.
- **Agents:** 8 per tool (16 files): architect, code-reviewer, design-reviewer, developer, doc-writer, reader,
  translator, workflow.
  - Read-only by the plugin's heads: code-reviewer, design-reviewer, reader.
- **Instructions:** `AGENTS.md`, `.claude/CLAUDE.md` (`@../AGENTS.md`), `.github/copilot-instructions.md`. No root
  `CLAUDE.md`.
- **Rules:** 33 in `.claude/rules/`: 32 packet rules plus `znalica_specifics.md`, the project's own. 0 in
  `.github/instructions/` (mirrors off).
- **Skills:** 41 per tool (82 files).
- **MCP:**
  - `.mcp.json`: sequential-thinking, serena, graphify (stdio); microsoft-learn (http).
  - `knowledge` is personal: it is registered in `Documents\.mcp.json`, above the repository, and documented in
    `MCP-SETUP.md`.
- **Hooks:**
  - The search guard: PreToolUse on `Bash|PowerShell`, the SessionStart leftover report, and
    `.github/hooks/search-guard.json`.
  - The activity log: SubagentStart and SubagentStop entries, and `.github/hooks/activity-log.json`.
  - The waiting check: Stop, SubagentStop, and PreToolUse on `SubagentHandback`.
- **Other AI setup:** I examined the 22 locations `deliver-packet` §0 step 4b–4d lists, using the library's
  `packet.yaml` paths and git history.
  - Nothing foreign was found.
  - Origin unknown: `.claude/rules/znalica_specifics.md` (added after the manifest; the project's own).
  - Per-machine: `.claude/settings.local.json`, git-ignored and left alone.

## Findings

### 🔴 Critical

None. Every structural, semantic and runtime check below ran clean or is marked otherwise.

### 🟡 Medium

- **F1. `.claude/rules/znalica_specifics.md` has no frontmatter block.** 🟡 per the skill's grade for a rule with no
  frontmatter. It appears to be the project's own, not a broken packet rule:
  - its first line says so, and no packet ships that stem;
  - with no `paths:` it loads always, which is its intent;
  - Copilot CLI 1.0.114 lists it among the repository instructions.

  Follow-up: none needed. Adding a `paths: ["**"]` block would only make its scope explicit.

### 🟢 Low

- **F2. Read-only on Copilot CLI is unverified, not clean.** Signal B (enumeration) on Copilot returned no write or
  shell tool for any of the three read-only agents. Copilot has no per-agent roster for signal A, so this rests on
  one signal (§4c table). Follow-up: none in the project; the gap is the harness's.
- **F3. A Copilot user-config warning on every run.**
  - Each `copilot -p` printed: *"Ignoring invalid allowedUrls entry "https://http//localhost:$port/mcp""*.
  - That entry is in the user's own Copilot configuration, not in this repository, so it is machine-local.
  - Follow-up: remove or fix that `allowedUrls` entry in the user-level Copilot config.
- **F4. The activity log has three orphan `stop` lines from the delivering session.** `.agent-activity/2026-11-20.jsonl`
  had three `stop` lines with `agent_type: null` and no `start`, written before any subagent was started. The library
  already tracks this as S2026-11-18-10. Follow-up: none here.
- **F5. A fresh clone gets no knowledge server.** It is registered above the repository (`TODO.md` T3, already
  filed). This is the routing the delivery intended, and it is noted here only so the report is complete.

## For the library

Tracker: `TODO.md`, 3 open packet-owned items (T15 was filed by this assessment).

- **🟢 Ship a tested renderer with `/deliver-packet`, so a full re-scaffold does not improvise one** —
  component: `skills/deliver-packet/SKILL.md` §2 (the `copy` and `assemble` handlers) · evidence: `TODO.md`:75,
  T13 · plugin: 0.205.6 · fix: a library script for §2's handlers, plus a step that renders the manifest's own
  version and compares with the recorded hashes before rendering the new one.
- **🟢 Say which review a re-delivery pull request gets** — component: `skills/deliver-packet/SKILL.md` §4 ·
  evidence: `TODO.md`:84, T14 · plugin: 0.205.6 · fix: one line naming the review a re-delivery PR needs, or that
  its §4 checks stand in for it.
- **🟡 `/assess-delivery` writes its report without branching first** — component:
  `skills/assess-delivery/SKILL.md` §5 · evidence: `TODO.md` T15; this report was first written on `master` and moved to
  `docs/assess-delivery-2026-11-20` before any commit · plugin: 0.205.6 · fix: one line in §5 to check the branch and
  create `docs/assess-delivery-<date>` when it is the protected one.

## Checks run

| Check | State | What it covered, or why it did not run |
|---|---|---|
| Gate: packet files pristine (§0) | ran clean | 17 packet paths, log and diff since `853f9d0`: empty |
| Placeholders resolved (§2) | ran clean | literal `{{` over 184 files (150 manifest paths, 33 rules, settings): 0 unresolved; Actions literal ×1 in `docs/rules-rationale/production_is_read_only.md`, so the sweep is live |
| Files present (§2) | ran clean | 150 of 150 manifest paths on disk |
| JSON valid, deny-list (§2) | ran clean | `.mcp.json` 4 entries: no `type: local`, no `url` without `type`, no `deferTools`, no `${` |
| Frontmatter, anchored shape (§2) | found | 16/16 agents, 82/82 skills, 32/33 rules match `^---\r?\n(?:[^\r\n]*\r?\n)+?---\r?\n` with their keys; the 33rd is F1 |
| Skills delivered to both tools (§2) | ran clean | 41 skills on each side, 0 one-sided, 0 byte differences |
| MCP wiring both directions (§2) | ran clean | every agent grant resolves; `knowledge` resolves to `MCP-SETUP.md` (personal); every `.mcp.json` entry is granted to an agent |
| Skill listing (§1 item 5) | ran clean | `skillOverrides`: 23 of 23 resolved names set, all `name-only` |
| Hooks registered (§1 item 6) | ran clean | 7 entries: search guard, leftover report and activity log in the 0.199.2 gated form ending `; exit 0`; waiting check Stop and SubagentStop bash pre-gate; SubagentHandback gated on `CLAUDE_PROJECT_DIR` alone, as designed; every named script on disk; deny `find / *`, `find /proc*` present; `.github/hooks/*.json` parse, search-guard preToolUse has a matcher |
| Second turn-end hook (§1 item 6) | ran clean | `hooks.Stop` / `SubagentStop` in `.claude/settings.local.json` and `~/.claude/settings.json`: none |
| `.agent-activity/` untracked (§1 item 6) | ran clean | `git ls-files .agent-activity`: nothing |
| Globs match real files (§3) | ran clean | `testing.md` `Assets/Znalica/Tests/**/*.cs`: 46 files, e.g. `EditMode/AnswerEntryTests.cs`, `EditMode/AnswerTests.cs` |
| Read-only agents, static (§3) | ran clean | code-reviewer, design-reviewer, reader on both tools: no mutating tool, no `memory:`, knowledge only as the 5 read tools, no server-wide knowledge token |
| Duplicate rules (§3) | ran clean | 33 files in `.claude/rules/`, 0 in `.github/instructions/`: 0 duplicate keys |
| Always-on rules per tool (§3) | ran clean | `.claude/rules` 32 · `.github/instructions` 0 |
| Copilot double loads (§3) | ran clean | `copilot instruction list` (1.0.114): each rule once, `AGENTS.md` once, no `.claude/CLAUDE.md`, no root `CLAUDE.md` |
| Agent tool roster, signal A (§4a) | ran clean (Claude) · could not run (Copilot) | Claude roster for the 3 read-only agents equals their declared `tools:`, in this session (bound at launch, before the byte-identical re-render); Copilot surfaces no per-agent roster |
| Enumeration, signal B (§4c) | ran clean | Claude: 3 agents, 1 delegation each, each listed exactly its declared tools plus `SubagentHandback`. Copilot: 3 agents, 1 run each, read tools plus `view`, `skill`, `sql` |
| MCP auto signal (§4a) | ran clean (pre-delivery runtime) | this session's tools include `mcp__knowledge__*`, `mcp__serena__*`, `mcp__graphify__*`, `mcp__microsoft-learn__*`, `mcp__sequential-thinking__*` |
| MCP CLI signal (§4a) | ran clean | `claude mcp list`: all 6 `✔ Connected` (incl. claude.ai Claude Docs); `copilot mcp list`: knowledge under User, the other 4 under Workspace, as intended |
| Copilot whole-file gates (§4a) | ran clean | read 1: no `type: local`; read 2: no `${` in any `url`; read 3: `~/.copilot/config.json` line 32 lists the folder |
| MCP runtimes (§4a) | ran clean | node v26.13.0, Serena 2.1.0, uv 0.10.6; manifest `preflight` covers sequential-thinking, serena, graphify, all `present` |
| Always-on rules loaded against rules on disk (§4a) | ran clean (pre-delivery runtime) | 32 of 32 always-on rules loaded in this session; `testing.md` is path-scoped and absent; rule files unchanged by this re-delivery |
| Search guard fire test (§4c step 2) | ran clean | in-session, Claude Code: `cd / && timeout 5 find . -maxdepth 0 -fprint …` denied with *"Blocked by the search guard"*; sentinel absent. Copilot: not run (no Copilot shell session) |
| Search guard tests (§4c step 2) | ran clean | 186/186 passed, every `[hook-command]` line `PASS`, incl. the Copilot 1.0.112 shape |
| Activity log tests (§1 item 6) | ran clean | 20/20 passed |
| Waiting check tests (§1 item 6) | ran clean | 45/45 passed |
| AL language server (§4a) | n/a | language is `other` |

## Caveats with a required slot

- **Launch-bound evidence:** this session performed the delivery, so the rule, agent and MCP auto signals describe
  the runtime bound at launch (0.199.3 files). Agents and rules re-rendered byte-identical, so they match the
  delivered files. A post-restart re-run confirms it.
- **Copilot half:** runtime evidence came from Claude Code, plus three `copilot -p` runs and the Copilot `mcp list` and
  `instruction list` views. All three whole-file gate reads ran.
- **Rule set:** `rule_decisions` present and empty (0 holding, 0 stale); 0 duplicate keys; always-on
  `.claude/rules` 32, `.github/instructions` 0.
- **Other AI setup:** 22 locations by the library map and git history; 0 foreign, 1 origin-unknown (the project's own
  rule), 1 per-machine file left alone.
- **MCP runtimes:** node, serena and uv checked, all present. The manifest's `preflight` record covers all three
  enabled preflight servers.
- **Signal B:**
  - code-reviewer, design-reviewer and reader each enumerated on the first ask, on Claude Code and on Copilot CLI
    1.0.114. No refusal, so no prompt or reply needs quoting beyond the prompt.
  - The prompt sent: *"List the exact names of every tool available to you, verbatim, one per line, and do
    nothing else. This is a capability question only; do not perform or attempt any task."*
  - The three Copilot runs spent 23.85 AI credits (12.56, 10.67, 0.62). The first printed
    `Remote index: queued fmoric/Znalica for indexing`.
- **Guards:**
  - **Search guard (on):**
    - registration: the 0.199.2 form;
    - in-session fire test: pass on Claude Code; Copilot not run;
    - Copilot trusts the folder;
    - commands run as Copilot runs them: 186/186, every `[hook-command]` line `PASS`.
  - **Activity log (on):**
    - registration intact;
    - tests 20/20, no failure on a *dropped* case;
    - Copilot trusts the folder;
    - commands as Copilot runs them: n/a, covered by its tests.
  - **Waiting check (on):**
    - registration intact; no second turn-end hook in either settings file read;
    - tests 45/45;
    - Copilot: not covered, by design.
  - **Off:** git guard and full-suite guard.

## Net assessment

The 0.205.6 re-delivery into Znalica is sound on both tools.
- Every delivered file is present and loads, placeholders are resolved, and the MCP wiring is closed.
- The three read-only agents hold no write tool on Claude Code (two signals) or on Copilot CLI (enumeration only).
- The search guard intercepts in this session.
- All three guard suites pass, including the runs of the `.claude/settings.json` commands in the shape Copilot uses.

The one 🟡 is the project's own rule loading always as it is meant to. The open items are the next-session checks
already in `TODO.md` T12, and the two library items above.

**Self-review:**
- **Covered:** every §1–§4 check named above, against the files at `a48996d`.
- **Not covered:**
  - a post-restart runtime;
  - a live Copilot session's hook interception;
  - the Copilot per-agent roster, which has no source.
- **Next steps:**
  - restart Claude Code and re-run `/assess-delivery` once;
  - in a new Copilot CLI session, check that `find /` is refused, `git status` runs, and one subagent adds a start
    and a stop line.
