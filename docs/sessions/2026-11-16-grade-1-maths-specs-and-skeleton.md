# Session: grade 1 maths specs, Unity setup and the step 1 skeleton

**Date**: 2026-11-16 · **Branch**: `feat/unity-project-skeleton` (session ended there) · **Version**: none yet

## What was done

- **PR #1 merged** (`224d33a`): T1, renaming the folder to `znalica`, was dropped, because the user wants every name to
  start with a capital letter. T2 is done: the project path is in Copilot CLI's `trustedFolders`
  (`~/.copilot/config.json`, which is outside the repository).
- **GitHub remote added by the user:** `origin` → `fmoric/Znalica`.
- **Functional spec** `docs/specs/2026-11-16-grade-1-maths-first-version.md`. Decided with the user:
  - Windows desktop app, grade 1 maths, numbers 0–20 and addition/subtraction to 20;
  - a pupil plus a parent progress view behind a simple parent gate, with no gate setting;
  - progress kept only on this PC;
  - the voice method left open, with placeholder clips meanwhile.

  Sources: the curriculum text (NN 7/2019, grade 1, domains A and B) and the two knowledge-store articles.
- **Technical spec** `docs/specs/2026-11-16-grade-1-maths-first-version-tech.md`, written by the architect. The
  design-reviewer graded it **B, ready to build** after two fix rounds (`d5a12b0`, `6233352`).
- **PR #2 merged** (`c419ade`): both specs.
- **Unity installed:**
  - Unity Hub 3.22.6 through winget.
  - Editors `6000.3.28f1` (6.3 LTS, the one the project pins) and `6000.6.9f1`. Nothing uses `6000.6.9f1`.
  - The Personal licence activated by the user. The first batch-mode run failed (exit 198, no licence) until it was.
- **Step 1 built** on `feat/unity-project-skeleton` by the developer:
  - Commits `9e6148d` (project skeleton), `ca33ce9` (`AGENTS.md` build command to `-buildTarget Win64`) and
    `90b260e` (R2 findings recorded in the technical spec, T4 in progress).
  - The batch build exits 0 and produces `Builds/Windows/Znalica.exe`.
  - The pre-check on `90b260e` ran the EditMode suite: 1 test, passed, 8 s.
  - All five R2 Unity behaviours were confirmed in a built player.
- **Knowledge-store research requests filed:** 67, cited Unity runtime behaviours (by the architect); 68, mouse and
  keyboard UI for children aged 6–7.
- **Mock-up:** a clickable web mock-up of *Računanje do 20*, private to the user. It is a throwaway, not game code:
  https://claude.ai/artifact/5dncGwLHzKJmKpu6L3xfvq

## PAUSED

None.

## Findings filed

- T5 in `TODO.md` → *Open*: the Unity commands in `AGENTS.md` assume `Unity.exe` is on PATH, and on this PC it is
  not.

## Not done / left open

- **Step 1 is not through its gates.**
  - No code review yet.
  - The full EditMode run needs the user's yes, asked after the review passes.
  - Then the push and the pull request.
- **`feat/unity-project-skeleton` has no upstream.** Its 3 commits exist only in the checkout that ran this shutdown.
- **Open in the specs:** the voice method (functional spec, open question 5), and `MAT OŠ A.1.4`'s outcome text
  (open question 1).
- **T3** (whether the knowledge server loads at session start) is still the user's call.
- `Znalica.Storage` has no scripts yet, so Unity logs a warning about it. The warning goes when step 4 adds code.

## Scope caveat

Written from this session's own context, checked against `git log 9d6e3b4..HEAD` (11 commits). It does not cover
what the subagents read beyond their reports. It also does not cover the state of `~/.copilot/config.json` after
any Copilot CLI session rewrote it.
