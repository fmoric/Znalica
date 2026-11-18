# Znalica — TODO

## Open

- [ ] **Decide whether the knowledge server loads at session start.** *(T3. New 2026-11-16.)* Claude Code reads
  `knowledge` from `Documents\.mcp.json`, which has no `"alwaysLoad": true`, so the main session goes through a
  tool search first. Adding the key costs context in every session and up to 5 s of startup.
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
  2026-11-18; their manual player checks are still to do. **In progress:** steps 6 and 7 (Veće, manje, jednako and
  Brojevna crta; branch `feat/comparison-and-number-line`). Both are committed; their review and test gates are next.
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

## Done

- [x] **Send the hand-off-a-change proof-run fix to the agent-packet library.** *(T8. Done 2026-11-18.)* Commit
  `abb78a9` (merged in PR #7) told the developer to prove tests with runs selected by name, never the unselected
  suite, after the step 5 developer had run the unselected EditMode suite about 22 times. The library merged the same
  fix in its PR #411 as version 0.197.2 on 2026-11-18, so the next re-delivery brings the same text; the local copy
  stays until then.
- [x] **Trust the project folder in Copilot CLI.** *(T2. Done 2026-11-16.)* Added
  `C:\Users\filip.moric\Documents\GitHub\Znalica` to `~/.copilot/config.json` `trustedFolders`.
- [x] **~~Rename the project folder to `znalica`.~~** *(T1. Dropped 2026-11-16.)* The folder keeps `Znalica`,
  matching the GitHub repository `fmoric/Znalica`; names start with a capital letter.
