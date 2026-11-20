# Znalica — TODO

## Open

- [ ] **Decide whether the knowledge server loads at session start.** *(T3. New 2026-11-16.)* Claude Code reads
  `knowledge` from `Documents\.mcp.json`, which has no `"alwaysLoad": true`, so the main session goes through a
  tool search first. Adding the key costs context in every session and up to 5 s of startup.
  - **A fresh clone gets no knowledge server at all.** *(Added 2026-11-18, from the 0.199.3 delivery assessment, F2.)*
    The entry sits in a `.mcp.json` above the repository, so a teammate who clones only Znalica has none, and
    `MCP-SETUP.md`'s knowledge section is the only record of how to add it (per user, with `"alwaysLoad": true`).
- [ ] **Write the first functional spec for Znalica.** *(T4. New 2026-11-16.)* Draft:
  `docs/specs/2026-11-16-grade-1-maths-first-version.md` (grade 1 maths, numbers 0–20 and add/subtract to 20,
  Windows desktop). Sources: the knowledge-store articles
  `docs/articles/LEARNING_GAME_EVIDENCE_AND_GAME_FORMAT_PER_CONTENT_TYPE.md` and
  `docs/articles/tools/CROATIA_PRIMARY_SCHOOL_CURRICULUM_SUBJECTS_PER_GRADE_AND_OUTCOME_CODES.md`. Next: its
  technical spec (`write-tech-spec`; graded B), `docs/specs/2026-11-16-grade-1-maths-first-version-tech.md`. Steps 1
  and 2 (project skeleton for Windows; text and voice catalog and the activity menu) merged in PR #3 on 2026-11-17;
  their manual player checks and the Croatian reading of `Hr.json` are still to do. Step 3a (round engine, domain
  only) merged in PR #4 on 2026-11-17. Step 3b (the Računanje do 20 screen) merged in PR #5 on 2026-11-18; its
  manual player checks are still to do. Open question Q4 (pacing of new items) was settled on 2026-11-18. Steps 4
  and 5 (profiles, the parent gate, answers saved to disk; missed items return next session) merged in PR #6 on
  2026-11-18; their manual player checks are still to do. Steps 6 and 7 (Veće, manje,
  jednako and Brojevna crta) merged in PR #8 on 2026-11-18; their manual player checks are still to do. **In
  progress:** steps 8 and 9 (the parent area's progress screen and school-like check; branch
  `feat/parent-area-progress-and-check`).
  - **2026-11-18, end of day:** steps 8 and 9 merged in PR #9; the product owner's play-test of steps 2 to 9 in the
    built player passed ("all looks fine"). Next: step 10.
- [ ] **Make the Unity commands in `AGENTS.md` run on a machine where `Unity.exe` is not on PATH.** *(T5. New
  2026-11-16.)* The build and test commands call a bare `Unity.exe`; on this PC the editor is at
  `C:\Program Files\Unity\Hub\Editor\6000.3.28f1\Editor\Unity.exe` and not on PATH, so the developer had to call it
  by full path. The path is per-machine, so it must not be hardcoded: read it from an environment variable or from
  `ProjectSettings/ProjectVersion.txt` plus the Hub's install folder.
- [ ] **Decide whether "05" in the answer box should count as 5.** *(T6. New 2026-11-17.)* Step 3a's `AnswerEntry`
  takes "0" then "5" as 5, so the box shows "5" when the child typed 0 then 5. The step 3b code review raised it as
  pre-existing; it is the product owner's call whether a leading 0 is dropped, refused or counted wrong.
- [ ] **Stop the Unity build from rewriting two asset files with line-ending-only changes.** *(T7. New 2026-11-17.)*
  Each batch build rewrote `LiberationSans SDF - Fallback.asset` and `VoiceLibrary.asset` with only line-ending
  changes, and both were reverted by hand. The repository has no `.gitattributes`.
  - **Fix direction:** a `.gitattributes` that fixes the line endings of Unity's text assets, so a build leaves them
    unchanged.
- [ ] **Merge the round code the three activity screens copy.** *(T9. New 2026-11-18.)* `ArithmeticScreen`,
  `ComparisonScreen` and `NumberLineScreen` each carry near-identical `Begin`, `BeginRound`, save-failure handling,
  Next/Replay buttons and key loop; the steps 6+7 code review raised it, and the product owner deferred it on
  2026-11-18 so that PR did not rewrite untested Presentation code. Do it before a fourth activity screen adds a
  fourth copy; the Računanje do 20 manual check then needs repeating.
- [ ] **Give the game a warmer look.** *(T10. New 2026-11-18.)* The play-test on 2026-11-18 found everything working
  but the look "so simple and not fun at all, so cold": flat colours, no characters, abstract counters, no reaction or
  reward. Research brief: `docs/research/warmer-look-for-grade-1.md` (merged to `master` in PR #10). The product owner draws the art in Claude desktop (SVG, made into PNG
  sprites). After step 10, by the product owner's order.
  - **Decisions first:** a mascot and who it is; what the round-end stars mean; who converts SVG to PNG; where the SVG
    sources live; the font (Andika, if it has č ć đ š ž); a check at the next play-test.
  - **Fix direction:** settle the functional spec's open question 6, then a tech spec in the brief's 5 slices.
- [ ] **Replace the parent gate's maths question with a PIN.** *(T11. New 2026-11-18.)* The product owner's ask on
  2026-11-18. Research brief: `docs/research/parent-pin-gate.md` (same commit). It changes the functional spec's "no
  password to forget" (FS:108–109) and "no setting" (FS:78, Q1).
  - **Decisions first:** how a forgotten PIN is reset; whether the maths question guards setting the PIN the first
    time; whether the maths gate stays at all; 4 or 6 digits; whether the wait survives a restart; how "change PIN"
    works.
  - **Code facts:** the PIN cannot live in `Settings.json` (`SettingsStore.Save` writes only the volume,
    `SettingsStore.cs:48`); the 30-second wait is held in memory (`ParentGate.cs:25`), so a restart skips it;
    `AnswerEntry` holds an integer, so `0123` would become `123`.
- [ ] **Check the 0.199.3 hooks where no session has run them yet.** *(T12. New 2026-11-18.)* The delivery and its
  assessment (`2026-11-18-deliver-packet-other-assess.md`) checked the search guard and the activity log live in Claude
  Code only; on Copilot CLI only their test suites ran (search guard 186/186, activity log 20/20).
  - **Copilot CLI:** an agent's `find / -name x` must be refused with the guard's reason, `git status` must still run
    (*hook errored* from "repo settings" means a `.claude/settings.json` entry fails there), and one subagent must add a
    `start` and a `stop` line with `tool: copilot` to `.agent-activity/<date>.jsonl`.
  - **Claude Code:** end a turn on a bare `WAITING:` line (start a background `sleep 120` and stop); the waiting check
    must send the turn back with its reason.
  - **Fix direction:** a failure on either is a library finding for ai-agent-templates, reported with the command and output.
  - **The 0.205.6 re-delivery (2026-11-20) adds to these checks.** In-session, the guard scripts and entry commands were
    fired directly (search guard deny; waiting check block on Stop and SubagentStop, hand-back deny once), not through a
    live turn. Still open after it: the Copilot CLI checks above, and in a fresh session `code-reviewer` lists exactly
    the five knowledge read tools.
- [ ] **Ship a tested renderer with `/deliver-packet`, so a full re-scaffold does not improvise one.** *(T13. New
  2026-11-20. packet-owned.)* `/deliver-packet` is instruction-driven with no bundled runtime, so the 0.205.6
  re-delivery built its own renderer for the copy, assemble and comment-stripping rules. Rendering the 0.199.3 library
  with it first and comparing with the delivered files caught four defects before anything was written: a comment
  holding backticks emptied `testing.md`; read-only agents were not recognised, which granted them the whole knowledge
  server; doc-writer's quoted `description:` was lost; the no-op `Run: none` line came back. The first two would have
  shipped silently. `MCP-SETUP.md` had to be patched by hand, since it is prose generated from `servers.yaml`.
  - **Fix direction:** a library script for §2's `copy` and `assemble` handlers, plus a step that renders the
    manifest's own version and compares with the recorded hashes before rendering the new one.
- [ ] **Say which review a re-delivery pull request gets.** *(T14. New 2026-11-20. packet-owned.)* `self_review`
  sends code to the code-reviewer agent, and `/deliver-packet` §4 does not say whether a re-delivery's packet text
  counts. PR #14 (0.205.6) merged with no review; its check was the byte-for-byte reproduction of 0.199.3 and the §4
  checks.
  - **Fix direction:** one line in `/deliver-packet` §4 naming the review a re-delivery PR needs, or that its §4
    checks stand in for it.
- [ ] **Make `/assess-delivery` branch before it writes its report.** *(T15. New 2026-11-20. packet-owned.)* Its §5
  says where to write `<slug>.md` (`docs/assessments/`, else the project root) but has no branch step, so the
  2026-11-20 assessment wrote the report on `master`, against `branch_before_edit`; it was caught after the write and
  moved to `docs/assess-delivery-2026-11-20` before any commit. The 2026-11-18 assessment branched by itself.
  - **Fix direction:** one line in `/assess-delivery` §5: before writing, check the branch and create
    `docs/assess-delivery-<date>` when it is the protected one.

## Done

_Completed items move to [TODO_ARCHIVE.md](TODO_ARCHIVE.md)._
