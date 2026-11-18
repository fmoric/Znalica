# Delivery assessment: Znalica, developer packet, language `other` (C# / Unity)

Assessed: the project folder `Znalica`, packet `developer`, language `other` (C# / Unity).
delivered-with: ai-agent-templates 0.199.3 (`.agent-packet.json`, delivered 2026-11-18) · assessed-with: 0.199.3, the
installed plugin (`installed_plugins.json` install record). The two are the same version.

Runtime state: **post-restart.** The delivery ran in the previous session; this session started after it. Its hooks
are live: the search guard refused a `find /` from this session's own shell, and the activity log wrote a start
and a stop line for a subagent of this session.

## What was delivered

- **Manifest:** packet `developer`, language `other`. Options on: `knowledge_base`, `code_navigation_mcp`,
  `microsoft_docs_mcp`, `code_graph_mcp`, `search_guard`, `waiting_check`, `activity_log`. Off: `copilot_rule_mirrors`,
  `library_docs_mcp`, `integration_branch`, `git_guard`, `full_suite_guard`, and the four AL-only options.
  `rule_decisions: []`. `files`: 150 paths, none `merged`. `preflight`: 3 servers recorded.
- **Agents:** 8 on each tool (architect, code-reviewer, design-reviewer, developer, doc-writer, reader, translator,
  workflow).
- **Instructions:** `AGENTS.md`, `.claude/CLAUDE.md` (`@../AGENTS.md`), `.github/copilot-instructions.md`. No root
  `CLAUDE.md`.
- **Rules:** 33 in `.claude/rules/`: 32 packet rules, plus `znalica_specifics.md`, the project's own (added after the
  first delivery). `.github/instructions/` is absent, as mirrors are off.
- **Skills:** 41, each in `.claude/skills/` and `.github/skills/`.
- **MCP:** `.mcp.json` has `sequential-thinking`, `serena`, `microsoft-learn` and `graphify`. `knowledge` is
  personal. Claude Code reads it from a `.mcp.json` in a parent folder of the project, and Copilot CLI lists it as a
  user server. `MCP-SETUP.md` documents all 10 resolved servers.
- **Hooks:** the search guard, the waiting check and the activity log, each with its scripts, tests and its
  `.claude/settings.json` entries. The search guard and activity log also have `.github/hooks/*.json` files.
- **Another AI setup:** I checked 130 items in 15 locations against the library's 275 packet paths. 0 are foreign.
  `znalica_specifics.md` and the untracked, per-machine `.claude/settings.local.json` are the project's own. The
  evidence was the library map plus git history.

## Findings

### 🔴 Critical

**F1 — The test-file glob matches no file, so the `testing` rule never loads.**
- **Why it fails:** `TEST_GLOB` is recorded as `Assets/Tests/**/*.cs`, and `.claude/rules/testing.md` is scoped to
  `paths: ["Assets/Tests/**/*.cs"]`. That glob matches 0 of the project's 153 `.cs` files under `Assets/`. The 37
  files holding `[Test]` or `[UnityTest]` live in `Assets/Znalica/Tests/EditMode/`, for example `AnswerEntryTests.cs`
  and `AnswerTests.cs`.
- **Consequence:** the rule is the one scoped to test files. With a glob that matches nothing it never enters a
  session, on either tool, and nothing reports that.
- **Since when:** the value arrived in the first delivery (ea3d658, 0.195.10, 2026-11-16). Both re-deliveries
  offered it back as a recorded answer, and today's run kept it without running the glob.
- **Fix:** re-deliver with `TEST_GLOB` = `Assets/Znalica/Tests/**/*.cs`. Hand-editing `testing.md` would be written
  over by the next delivery, and the manifest would still record the wrong value.

### 🟡 Medium

None.

### 🟢 Low

**F2 — The knowledge server is registered outside the repository and stays deferred.**
- **What is there:** its Claude Code entry is in a `.mcp.json` two folders above the project, which `claude mcp get
  knowledge` reports as *Project config (shared via .mcp.json)*. That entry has no `alwaysLoad`, although the packet
  declares `always_load: true` for this server. Copilot CLI 1.0.114 lists it under `User servers:`.
- **Why it is low:** it works. `claude mcp list` showed it `✔ Connected`, and its five read tools were live in this
  session and in the read-only agents. Its tools simply load on demand, not at session start.
- **Consequence:** a teammate who clones only Znalica gets no knowledge server.
- **Fix:** if that is unwanted, register it per user (`claude mcp add-json --scope user`, with `"alwaysLoad": true`),
  as `MCP-SETUP.md`'s knowledge section describes.

**F3 — The project's own always-on rule loads beside the packet's.**
- `.claude/rules/znalica_specifics.md` matches no packet rule by name or heading, and is a separate project rule by
  design. No finding beyond inventory.

## Checks run

| Check | State | What it covered, or why it did not run |
|---|---|---|
| §0 gate: packet files unchanged since delivery | ran clean | `git log "$M.."` and `git diff "$M"` over the 17 delivered paths, `M` = b128a35: both empty |
| Placeholders resolved (§2) | ran clean | literal `{{` over 194 files in `.claude/`, `.github/`, `docs/` and the root files: 0 unresolved. The 1 hit is the Actions literal in `docs/rules-rationale/production_is_read_only.md`, which shows the sweep is live |
| JSON valid (§2) | ran clean | `.mcp.json`, `.claude/settings.json` and both `.github/hooks/*.json` parse. The 3 stdio entries have no `type`, `tools`, `timeout` or `deferTools`. `microsoft-learn` is `type: http` with a literal `url` |
| Frontmatter shape (§2) | ran clean | 130 of 130 agents, skills and rules match `^---\r?\n(?:[^\r\n]*\r?\n)+?---\r?\n`, with `name` + `description` or `paths:` present (delivery §4, same tree, unchanged since) |
| Files present (§2) | ran clean | 184 of 184 paths: the rendered map plus the generated and merged files |
| Skills delivered to both tools (§2) | ran clean | 41 of 41 pairs present and byte-identical (line endings ignored) |
| MCP wiring (§2) | ran clean | agents grant 5 keys, and all resolve: 4 in `.mcp.json`, `knowledge` in `MCP-SETUP.md` (personal). Every `.mcp.json` server is referenced by at least one agent |
| Globs match real files (§3) | found | F1: `Assets/Tests/**/*.cs` matches 0 of 153 `.cs` files |
| Read-only agents, static (§3) | ran clean | code-reviewer, design-reviewer and reader, on both tools: no `Write`/`Edit`/`Bash`/`Agent` and no `edit`/`execute`/`agent`/`write`; no `memory:`; `knowledge` granted per tool (5 tools), never server-wide; no `serena` |
| Mirror parity (§3) | n/a | `copilot_rule_mirrors: false`, and `.github/instructions/` is absent |
| Duplicate rules (§3) | ran clean | 33 files in `.claude/rules/` and 0 in `.github/instructions/`: 0 duplicate keys |
| Always-on rules per tool (§3) | ran clean | `.claude/rules` 32 always-on, plus 1 scoped (`testing.md`) · `.github/instructions` 0 |
| Copilot double loads (§3) | ran clean | no earlier rule copies; no root `CLAUDE.md`; `.github/hooks/search-guard.json` has the matcher `bash\|powershell\|shell`; the activity log's `subagentStart` and `subagentStop` are not tool events and take no matcher. `copilot instruction list` not run |
| Another AI setup (§3) | ran clean | 0 foreign, 1 project rule, 1 per-machine file |
| Skill listing (§1.5) | ran clean | `skillOverrides`: 23 of 23 resolved names set, all `name-only` |
| Agent tool roster, signal A (§4a) | ran clean | the roster for this session lists each read-only agent's tools as its file declares them, the per-tool `mcp__knowledge__*` and `mcp__graphify__*` grants included |
| Always-on rules loaded against rules on disk (§4a) | ran clean | 32 of 32 always-on rules are in this session's loaded instructions. The 33rd, `testing.md`, is path-scoped and correctly absent; its glob matches nothing (F1) |
| MCP signal 1, in-session tools (§4a) | ran clean | `mcp__knowledge__*`, `mcp__sequential-thinking__*`, `mcp__serena__*`, `mcp__microsoft-learn__*` and `mcp__graphify__*` are present |
| MCP signal 2, `claude mcp list` (§4a) | ran clean | all 5 project-relevant servers `✔ Connected`, with no warning lines. Agrees with signal 1 |
| `copilot mcp list` (§4a) | ran clean | configuration only: `knowledge` under user servers; the 4 others under workspace servers. Matches the intended routing |
| Copilot whole-file gates (§4a) | ran clean | read 1: no `type: local`. Read 2: no `${…}` in any `url`. Read 3: the project folder is listed in `trustedFolders`, an exact match, 1 of 23 entries |
| MCP runtimes (§4a) | ran clean | `node --version` v26.13.0, `uv --version` 0.10.6, `serena --version` 2.1.0. The manifest's `preflight` covers all 3 enabled preflight servers |
| Grant probe, signal B, Claude (§4c) | ran clean | see the read-only agents block below |
| Grant probe, signal B, Copilot (§4c) | not run | declined by the runner: the prompts would leave the machine |
| Search guard fire test, Claude (§4c) | ran clean | hook pass, see the guards block below |
| Search guard fire test, Copilot (§4c) | not run | no Copilot session was driven |
| AL language server (§4a) | n/a | the language is not `al-bc` |

## Read-only agents: signal B

All three agents took 1 delegation each, in Claude Code, and each was asked: *"This is a capability check for a
delivery assessment, not a review. Do not call any tool and do not perform any task. Reply with the exact names of
every tool available to you, verbatim, comma-separated, and nothing else."* (For `reader`: *"… not a search."*)

| Agent | Replied, verbatim |
|---|---|
| code-reviewer | Read, Grep, Glob, mcp__knowledge__search_knowledge, mcp__knowledge__get_article, mcp__knowledge__list_articles, mcp__knowledge__get_research, mcp__knowledge__get_teachable_moment, mcp__sequential-thinking__sequentialthinking, mcp__graphify__query_graph, mcp__graphify__get_node, mcp__graphify__get_neighbors, mcp__graphify__get_community, mcp__graphify__god_nodes, mcp__graphify__graph_stats, mcp__graphify__shortest_path, SubagentHandback |
| design-reviewer | the same 17 names |
| reader | the same, without `mcp__sequential-thinking__sequentialthinking`, which matches its file |

- **Grade:** 🟢 for all three. Both signals are negative for mutating tools. Enumeration cannot prove absence, so
  this is not a proof.
- **`SubagentHandback`:** the harness's own tool for returning a report. It writes no file.
- **Copilot:** not run, declined by the runner.

## The guards

**Search guard (`search_guard: true`)**
- **§1 item 6:** registered. The `PreToolUse` entry has matcher `Bash|PowerShell` and names `search_guard.ps1`; the
  `SessionStart` entry is `startup`. Both commands are the 0.199.2 form, gated on `CLAUDE_PROJECT_DIR` and `-not
  $env:COPILOT_HOOK_DEPTH` and ending in `; exit 0`. Both `deny` rules are present.
- **Claude Code:** hook pass. A `developer` subagent ran `cd / && timeout 5 find . -maxdepth 0 -fprint
  "<project>/.assess-probe-sentinel"` once, got *"Blocked by the search guard: 'find' from ."*, and no sentinel
  exists on disk. This session's own `timeout 5 find / -name …` was refused too.
- **Copilot CLI:** not run. No Copilot session was driven.
- **Copilot trusts the folder:** yes (gate read 3).
- **Run the way Copilot runs them:** `test_search_guard.ps1` 186/186 passed, with no `FAIL` or `SKIP` lines,
  `[hook-command]` lines included. It was run in this session and in the delivering one.

**Waiting check (`waiting_check: true`)**
- **§1 item 6:** registered. The `Stop` and `SubagentStop` entries are the bash pre-gate form. The `PreToolUse`
  `SubagentHandback` entry is gated on `CLAUDE_PROJECT_DIR`, which is the intended form for this hook.
- **Other turn-end hooks:** none. `.claude/settings.local.json` and `~/.claude/settings.json` hold no `Stop` or
  `SubagentStop` entries.
- **Tests:** `test_waiting_check.ps1` 45/45 passed.
- **Live check:** a hand-back from this session's subagents went through the `SubagentHandback` hook without being
  denied, as expected for reports with no `WAITING:` line. A turn ending on a bare `WAITING:` line was not tried in
  this session.
- **Copilot trusts the folder:** yes. Copilot is not covered by this hook anyway.
- **Run the way Copilot runs them:** n/a. Its bash entries are skipped there by design.

**Activity log (`activity_log: true`)**
- **§1 item 6:** registered. One `SubagentStart` and one `SubagentStop` entry name `activity_log.ps1`, both in the
  0.199.2 form with `async: true`. `.github/hooks/activity-log.json` parses and holds `subagentStart` and
  `subagentStop`. `.agent-activity/` is not tracked by git.
- **Tests:** `test_activity_log.ps1` 20/20 passed, with no failure on a dropped-content case.
- **Live check, Claude Code:** each of this session's subagents left one `start` and one `stop` line, with
  `tool: claude` and the agent type, and no content.
- **Live check, Copilot:** not run.
- **Copilot trusts the folder:** yes.

**Off:** git guard, full-suite guard.

## For the library

- **🟡 A recorded `TEST_GLOB` is offered back without running it, and no delivery step counts its matches.**
  - Component: `skills/deliver-packet/redelivery.md`, *Recorded answers in the interview*; `SKILL.md` §4.
  - Evidence: Znalica's `Assets/Tests/**/*.cs` matches 0 of 153 `.cs` files, and was carried through 3 deliveries
    (0.195.10, 0.195.16 and 0.199.3) without either check noticing. F1 above.
  - Plugin: 0.199.3.
  - Fix: before offering a recorded glob back, run it against the tree and show the count. Add a §4 check that
    every `paths:`/`applyTo:` glob this run wrote matches at least one file, reporting rather than failing, the way
    `/assess-delivery` §3 already grades it.
- **🟢 *Edited since the last delivery* flags a file whose only difference from the new render is line wrapping.**
  - Component: `skills/deliver-packet/redelivery.md`, *Edited since the last delivery*.
  - Evidence: `hand-off-a-change/SKILL.md` (both copies) was flagged *will be written over; back it up now*. The
    project's text had been taken upstream in 0.197.2 and re-wrapped. A diff of the new render against disk showed
    two wrapping changes plus the new 0.199.1 lines.
  - Plugin: 0.199.3.
  - Fix: compare against the new render with whitespace ignored, and say *"differs only in wrapping"* in the plan
    line.
- **🟢 No delivery step covers a server Claude Code loads from a `.mcp.json` in a parent folder.**
  - Component: `skills/deliver-packet/SKILL.md` §1 step 2, *An opt-in whose server the project already runs*;
    `mcp.md`, *Opt-in servers*.
  - Evidence: `knowledge` is absent from Znalica's `.mcp.json`, and `claude mcp get knowledge` reports it as
    *Project config (shared via .mcp.json)* from a folder above the repository. F2.
  - Plugin: 0.199.3.
  - Fix: when an enabled server is absent from the project's `.mcp.json` but `claude mcp get <key>` finds it, name
    its scope in the plan, and treat a parent-folder file as personal routing.

## Net assessment

The 0.199.3 delivery is structurally and semantically sound, and its runtime matches its files in Claude Code:
- every rule that should load did;
- all five MCP servers connect;
- the three read-only agents hold only read tools;
- the search guard and the activity log were observed working from a session started after the delivery.

The one real defect is F1, the test glob, and it predates this delivery. It is a wrong recorded answer, fixed by
re-delivering with `Assets/Znalica/Tests/**/*.cs`. The Copilot side is verified only by file reads and the guards'
test suites; no Copilot session ran.

## Self-review

- **Covered:** §0 to §4c in Claude Code, in a post-restart session. MCP health was checked with `claude mcp list`
  (connects to every server) and `copilot mcp list` (configuration only).
- **Not covered:**
  - The Copilot signal B and Copilot prompt runs, declined.
  - A Copilot-side fire test of the search guard and a Copilot activity-log line.
  - A live waiting-check block on a bare `WAITING:` line.
  - `copilot instruction list`.
- **§4e:** no finding about agent behaviour was made, so no delegation prompt needed grading. The probe prompts
  above named no MCP server.
