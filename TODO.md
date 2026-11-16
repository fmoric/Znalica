# Znalica — TODO

## Open

- [ ] **Decide whether the knowledge server loads at session start.** *(T3. New 2026-11-16.)* Claude Code reads
  `knowledge` from `Documents\.mcp.json`, which has no `"alwaysLoad": true`, so the main session goes through a
  tool search first. Adding the key costs context in every session and up to 5 s of startup.
- [ ] **Write the first functional spec for Znalica.** *(T4. New 2026-11-16.)* Start from the curriculum brief in
  Knowledge-Documentation, `docs/research/2026-11-16-croatia-primary-curriculum-learning-games-brief.md`
  (uncommitted, branch `docs/research-croatian-primary-learning-games`): tie each level to an outcome code,
  teach in the feedback, short sessions, spoken instructions for grades 1–2.

## Done

- [x] **Trust the project folder in Copilot CLI.** *(T2. Done 2026-11-16.)* Added
  `C:\Users\filip.moric\Documents\GitHub\Znalica` to `~/.copilot/config.json` `trustedFolders`.
- [x] **~~Rename the project folder to `znalica`.~~** *(T1. Dropped 2026-11-16.)* The folder keeps `Znalica`,
  matching the GitHub repository `fmoric/Znalica`; names start with a capital letter.
