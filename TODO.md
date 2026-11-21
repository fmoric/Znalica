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
  - **2026-11-21:** step 10's code (the break suggestion), the system map and the AC1–AC13 checklist are in draft
    PR #17 (`feat/break-suggestion`); review passed in 2 rounds and the full suite passed 571/571 on `3ba89db`. The
    product owner's player checks are open, step by step in
    `docs/specs/2026-11-21-grade-1-maths-release-checks-how-to.md` (on that branch).
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
  - **2026-11-21:** fixed by a `.gitattributes` in PR #18 (`fix/unity-asset-line-endings`, `73f9876`): review
    passed, full suite 563/563; not merged.
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
  - **Again at 0.206.6 (2026-11-21).** The changed-paths re-delivery built the renderer a second time. Rendering
    0.205.6 first and comparing with the manifest's hashes caught three misses: a `description:` holding `"` was not
    quoted, the `Run: none` line came back, and an apostrophe alone was quoted when the delivery does not. `MCP-SETUP.md`
    was again patched by hand.
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
- [ ] **Run `/assess-delivery`'s Copilot tool-list check on a cheap model.** *(T16. New 2026-11-20. packet-owned.)*
  §4c signal B runs `copilot -p … --agent <name>` at each agent's pinned model, though listing tools needs no
  judgment. The 2026-11-20 assessment spent 23.85 AI credits on three runs: code-reviewer 12.56 and design-reviewer
  10.67 (Sonnet-pinned), reader 0.62 (Haiku-pinned).
  - **Fix direction:** pass `--model <a low-cost model>` to signal B's runs; the tool list does not depend on the model.
- [ ] **Build the roadmap the user asked for in PR #17.** *(T17. New 2026-11-21.)* Two doc-writer runs were stopped by
  API safeguards (`[reasoning_extraction]`): on Sonnet 5.5 (`req_011ChFrXyNhqA1kfcM8B4rhY`) and on Opus 5.5
  (`req_011ChFs8PN3VgkomD2kus8oa`). The second left `docs/roadmap.md`, `.json` and `.html` untracked on the
  `feat/break-suggestion` checkout; the HTML is probably unfinished. The product owner kept them to read. **PAUSED**
  until the product owner decides on them; do not resume without explicit instruction.
  - **Fix direction:** after the read, keep and finish the files, or rebuild the roadmap in a later session.
- [ ] **Make the break suggestion speak even when the last answer's clips are missing.** *(T18. New 2026-11-21.)* The
  step 10 review's second round (PR #17) found that a `VoicePlayer.Play` whose clips are all missing finishes inside
  `StartCoroutine`, which then assigns the finished coroutine to `_playing`, so `_playing` stays non-null; a later
  `PlayAfterCurrent` only queues, nothing plays, and the suggestion is silent (`VoicePlayer.cs:74`, `122-126`). It
  needs the feedback clips to be missing, which the game already reports as a problem. Left undone in PR #17.
  - **Fix direction:** let `PlayQueue` clear a handle the caller checks, or also test `_source.isPlaying` in
    `PlayAfterCurrent`.
- [ ] **Tell the developer agent never to leave a background command running when it reports.** *(T19. New
  2026-11-21. packet-owned.)* In the step 10 fix round the developer's hand-back was refused twice ("waiting on its
  own background work"), though its brief said to keep every command in the foreground, so its report never reached
  the main session, which rebuilt the results from the branch and the run logs.
  - **Fix direction:** one line in `.claude/agents/developer.md`: start no background command, and wait for any
    started one to end before reporting.
- [ ] **Run the EditMode suite in CI.** *(T20. New 2026-11-21.)* The repository has no `.github/workflows`, so a local
  full run is the only full run a pull request gets (PRs #17 and #18 on 2026-11-21).
  - **Decisions first:** a Unity licence for CI (a secret in the repository), and which runner.
  - **Fix direction:** a GitHub Actions workflow running the EditMode tests on each pull request (the `workflow`
    agent).
- [ ] **Make the developer brief say to open `PR_GATES_PRECHECK.md` before selecting a pre-check.** *(T21. New
  2026-11-21. packet-owned.)* In step 10 the developer selected and budgeted its pre-check without opening the file,
  and said so; the budget it used happened to match.
  - **Fix direction:** one line in `hand-off-a-change`'s developer brief (*Gate*): open
    `docs/process/PR_GATES_PRECHECK.md` before you select.
- [ ] **Find out why the activity log records stops with no start and no agent type.** *(T22. New 2026-11-21.
  packet-owned.)* The 2026-11-21 re-delivery session started no subagent, yet `.agent-activity/2026-11-21.jsonl` holds
  four `stop` lines for it (19:40–19:45 UTC) with `agent_type: null` and no matching `start`. The likely source is the
  harness's own background agents firing `SubagentStop`; not traced.
  - **Fix direction:** once the cause is confirmed, have `activity_log.ps1` skip or label a stop whose agent type is
    empty, so `work-monitor` does not count it.

## Done

_Completed items move to [TODO_ARCHIVE.md](TODO_ARCHIVE.md)._
