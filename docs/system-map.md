# Znalica system map

**Mapped:** the source roots `Assets/Znalica/` (Domain, Storage, Presentation, Editor, Voice, Tests, Scenes, Art) and
`Assets/StreamingAssets/Text/`. 7 components; 155 `.cs` files counted (Domain 71, Storage 12, Presentation 23, Editor 2,
EditMode tests 47), the 5 `.asmdef` files, `Scenes/Main.unity` (searched for two asset links only), `Voice/VoiceScript.csv`
and `Text/Hr.json` (entry count only). Not mapped: `Art/`'s pictures, `Library/`, `Builds/`, the Unity packages.

**Built at `3ba89db` on 2026-11-21** (branch `feat/break-suggestion`; includes `SessionClock` and `VoicePlayer.PlayAfterCurrent`).
Staleness is judged by commits under the roots above since that commit.

Companions, one map in three forms: [`system-map.json`](system-map.json) (for tools; the same commit) and
[`system-map.html`](system-map.html) (click-through page, opens offline). The Markdown is the source; edit it first.

## How to use this map

- Find the component in the table below, then open its `paths`. Everything the game decides is in **Domain**; everything
  drawn or clicked is in **Presentation**; every file on disk is written by **Storage**.
- To see what a change reaches, read the **used by** column and the *Main flows* section. The direction of dependency is
  fixed by the assembly definitions (below), so a compile error is the first warning of a wrong direction.
- Run the tests of a component with the test command in `AGENTS.md`, selecting by class (`-testFilter`, see
  `.claude/rules/znalica_specifics.md`). Presentation and the build script have no tests.
- Question "is X the only caller?": this map's counts are text matches, not a language-server result; ask serena or
  graphify for the exact answer.

## What this map cannot see

- **How the counts were made.** The `refs` below are source lines, outside comments, that name a type declared in the other
  component (a text scan of type names declared in exactly one component, `refs.py`, a throwaway script not kept in the
  repository). It is a weaker claim than a language server's references. The *direction* of each link is checked against
  the `.asmdef` files, which the compiler enforces; the counts are only a size. Serena resolved no references for this map;
  its type and caller checks were done by reading the files. The graphify graph was built at `060fc64`, older than this
  map's commit, and was used only to read its statistics.
- **Scene wiring.** `Scenes/Main.unity` links the `AppRoot` script and the `VoiceLibrary.asset` asset by GUID (found by a
  text search of the scene for each GUID: one hit each). What else the scene holds, and the `ActivityPicture[]` pictures
  `AppRoot` receives, were not read.
- **Unity at runtime.** `MonoBehaviour` lifecycle (`AppRoot.Awake`), button callbacks, coroutines and the audio device run
  inside Unity; this map cannot trace them as calls. Screen buttons reach the next screen through the lambdas in
  `AppRoot.Awake`, which were read, not executed.
- **The built player.** Nothing here was run. Whether `FileStream.Flush(true)` and `File.Replace` reach the disk in an
  installed player is unverified (`.claude/rules/znalica_specifics.md`).
- **Content.** Whether every `PhraseKey` has text and every clip exists is the job of the `PhraseCatalogContentTests`
  completeness test, not of this map.
- **Outside the repository.** The Unity editor, the packages (`UnityEngine.UI`, `Unity.TextMeshPro`, `Unity.InputSystem`),
  and the player's data folder on the pupil's PC. There is no CI (`.claude/rules/znalica_specifics.md`).

## Components

The project is one Unity player (single process). Rules, enforced by the `.asmdef` files:

| Assembly | May reference | Notes |
|---|---|---|
| `Znalica.Domain` | nothing | `noEngineReferences: true`: no `UnityEngine` in it |
| `Znalica.Storage` | Domain | uses `UnityEngine` (`JsonUtility`) |
| `Znalica.Presentation` | Domain, Storage, `UnityEngine.UI`, `Unity.TextMeshPro`, `Unity.InputSystem` | |
| `Znalica.Editor` | Presentation, Storage, Domain | editor platform only |
| `Znalica.Tests.EditMode` | Domain, Storage (and the test runner) | does **not** reference Presentation or Editor |

| Component | Purpose | Size | Depends on (refs) | Used by (refs) | Tested by |
|---|---|---|---|---|---|
| **domain** `Assets/Znalica/Domain` | The rules of the game with no Unity in them: items and item sets of the three activities, rounds and answers, the schedule that decides what is asked next, the progress and school-check calculations, the parent gate, the phrase catalog and its keys | 71 files, 4108 lines | nothing | storage 76, presentation 380, editor 10, tests 1788 | `Tests/EditMode` (45 of its 47 files name Domain types) |
| **storage** `Assets/Znalica/Storage` | Everything written to or read from disk: profiles, settings, the answer log, the check log, the phrase file | 12 files, 779 lines | domain 76; text 1 (data) | presentation 30, editor 2, tests 76 | `ProfileStoreTests`, `SettingsStoreTests`, `FileAnswerLogTests`, `FileCheckLogTests`, `AtomicFileWriterTests`, `PhraseCatalogFileTests` |
| **presentation** `Assets/Znalica/Presentation` | The screens, drawing, buttons, number pad and the voice player; `AppRoot` wires the app together and decides which screen leads to which | 23 files, 3438 lines | domain 380, storage 30; voice 1 (scene link) | editor 8 | none |
| **voice** `Assets/Znalica/Voice` | The spoken clips (`*.ogg`, one per phrase or number), the generated `VoiceLibrary.asset` that maps a clip name to its clip, and `VoiceScript.csv`, the script for whoever records the clips. Its code is elsewhere: `VoiceLibrary`, `VoicePlayer` (presentation), `VoiceLibraryBuilder` (editor), `VoiceClipFiles`, `VoiceVolume` (domain) | 37 `.ogg` clips, 1 asset, 1 csv (36 rows) | nothing (assets) | presentation 1, editor 1, tests 1 (data) | `PhraseCatalogContentTests` (clip names against the folder), `VoiceVolumeTests` |
| **text** `Assets/StreamingAssets/Text` | `Hr.json`, the Croatian text and spoken clip names of every phrase (107 entries), loose beside the player so it can be edited without a rebuild | 1 file | nothing (data) | storage 1, editor 1, tests 6 files (data) | `PhraseCatalogFileTests`, `PhraseCatalogContentTests`, `ArithmeticScreenTextTests` and the three Explanation tests |
| **editor** `Assets/Znalica/Editor` | `BuildScript.Build` (batch build to `Builds/Windows/Znalica.exe`) and `VoiceLibraryBuilder.Rebuild` (menu `Znalica/Rebuild Voice Library`, also run by the build) | 2 files, 138 lines | presentation 8, domain 10, storage 2; voice, text (data) | nothing | none |
| **tests-editmode** `Assets/Znalica/Tests/EditMode` | NUnit EditMode tests of Domain and Storage, with in-memory test doubles | 47 files | domain 1788, storage 76; text, voice (data) | nothing | n/a |

Counts for code links are the lines described under *What this map cannot see*; `1` for a data link means one file reads or
links the asset. Domain has no reverse reference to anything (checked: no `using Znalica.Storage`, `Znalica.Presentation`, `Znalica.Editor` or `UnityEngine` in
`Assets/Znalica/Domain`, by text search, 0 hits).

### What Domain holds, by group

- **Items and item sets:** `Item`, `ItemKey`, `ItemToken`, `ItemFormatter`, `ItemOrder`, `Difficulty`, `NumberRange`;
  per activity `ArithmeticItem`/`ArithmeticItemSet`, `ComparisonItem`/`ComparisonItemSet`, `NumberLineItem`/`NumberLineItemSet`.
- **Rounds:** `RoundPlanner`, `ComparisonRoundPlanner`, `RoundSession`, `RoundPlay` and per-activity `ArithmeticPlay`,
  `ComparisonPlay`, `NumberLinePlay`; `Answer`, `AnswerEntry`, `AnswerKey`, `Feedback`, `Explanation`, `AttemptOutcome`.
- **Memory of what was asked:** `AnswerRecord`, `ItemSchedule`, `ItemProgress`, `SessionNumber`, `SessionClock`,
  `ProgressCalculator`, `ProgressReport`, `WeekPractice`.
- **School-like check:** `SchoolCheckPlanner`, `SchoolCheckSession`, `CheckLine`, `CheckHistory`, `CheckResult`, `CheckSection`.
- **Parent area:** `ParentGate`, `GateChallenge`, `GateResult`, `ProfileName`, `ProfilePictures`.
- **Text and voice:** `PhraseKey`, `Phrase`, `PhraseCatalog`, `PhraseCatalogCheck`, `PhraseSlot`, `VoiceClipFiles`, `VoiceVolume`.
- **Ports and plumbing:** the six interfaces below, `SystemClock`, `SeededRandom`, `DiskWrite`.

### Presentation: the files

`AppRoot` (composition root), `ScreenNavigator` (one panel visible at a time), the screens (`ProfilePickView`, `GateScreen`,
`ProfilesView`, `ProgressView`, `CheckResultsView`, `SchoolCheckScreen`, `ActivityMenuView`, `ArithmeticScreen`,
`ComparisonScreen`, `NumberLineScreen`), shared parts (`RoundEndPanel`, `AnswerGiving`, `NumberPad`, `UiBuilder`,
`LineClickArea`), drawing (`EquationDrawing`, `NumberLineDrawing`, `TenFrameDrawing`), and the voice (`VoicePlayer`,
`VoiceLibrary`) and logging (`UnityProblemLog`) adapters.

## Interfaces and their implementers

All six interfaces are in Domain. Every one has an implementer; none is a finding.

| Interface | Implemented in the game by | Test doubles |
|---|---|---|
| `IAnswerLog` | `FileAnswerLog` (storage) | `InMemoryAnswerLog`, `FailingAnswerLog` |
| `ICheckLog` | `FileCheckLog` (storage) | `InMemoryCheckLog` |
| `IClock` | `SystemClock` (domain) | `FixedClock` |
| `IItemSet` | `ArithmeticItemSet`, `ComparisonItemSet`, `NumberLineItemSet` (domain) | `ListItemSet` (inside `RoundPlannerTests`) |
| `IProblemLog` | `UnityProblemLog` (presentation) | `CollectingProblemLog` |
| `IRandom` | `SeededRandom` (domain); no other implementer found | none found (no test file names `IRandom`) |

## Runtime view

**Single process, single thread of game logic.** One Unity player (Windows 64-bit). The only asynchronous part found in the
three runtime assemblies is `VoicePlayer`'s Unity coroutine (`_playing`), which steps through the clip queue on the main
thread. No `Thread`, `Task` or `async` use was found by text search of Domain, Storage and Presentation. Storage's writes
are synchronous: a call returns after the data is flushed.

What lives where on the machine:

| Place | Part | Status |
|---|---|---|
| In the program | `AppRoot` and the screens; `VoicePlayer` (one `AudioSource`); `PhraseCatalog` read once in `AppRoot.Awake` | built |
| On the machine | `Text/Hr.json` under `Application.streamingAssetsPath` (read at start) | built |
| On the machine | The data folder `Application.persistentDataPath` (files below) | built |
| Outside it | nothing: no network, no service | none in the code |

### The data folder

Built only by `DataFolder`, the one place that makes these paths:

```
<Application.persistentDataPath>/
  Settings.json                      voice volume only {"VoiceVolume": 0.0-1.0}; written by SettingsStore (atomic replace)
  Profiles/<profile GUID>/
    Profile.json                     ProfileId, DisplayName, PictureId, CreatedAt (ISO 8601); atomic replace
    Answers.jsonl                    one JSON line per answer in the three activities; append-only
    Checks.jsonl                     one JSON line per school-check answer, plus a closing line per finished check; append-only
```

- `Answers.jsonl` line fields (`AnswerRecordDto`): `Activity`, `ItemKey`, `Answer`, `IsCorrect`, `Attempt`,
  `ResponseMilliseconds`, `AnsweredAt`, `SessionNumber`, `RoundId`.
- `Checks.jsonl` line fields (`CheckLineDto`): `Line` (an answer or `Closing`), `CheckId`, `ItemKey`, `Answer`, `IsCorrect`,
  `AnsweredAt`, `CompletedAt`, `Score`, `ItemCount`. Checks never go in `Answers.jsonl` (`FileCheckLog`).
- **Appending** (`JsonLinesFile.Append`): the line is written and `Flush(true)`d before the call returns; a file that ends
  mid-line gets a newline first, so a half-written last line does not swallow the next. **Reading** skips a line that does not
  parse and reports it (`Skipped an unreadable line N of <path>.`); it never rewrites the file.
- **Whole-file writes** (`AtomicFileWriter`): text goes to `<path>.tmp`, is flushed, then replaces the file.
- A profile id is checked to be a GUID before it becomes part of a path (`DataFolder.Canonical`); deleting a profile deletes
  its whole folder (`ProfileStore.Delete`).
- A missing or unreadable `Settings.json` gives the defaults and a reported problem; the game goes on.

| From | To | How | Status |
|---|---|---|---|
| `AppRoot` | `PhraseCatalogFile` | reads `Hr.json` once at start | built |
| `AppRoot` | `VoicePlayer` | `Initialize(VoiceLibrary, problems)`; the library asset is linked in `Main.unity` | built |
| Screens | `FileAnswerLog` / `FileCheckLog` | through `IAnswerLog` / `ICheckLog` from `ProfileStore` | built |
| `AnswerGiving` | `DiskWrite.TryRun` | a failed write is reported and the screen redraws (`AnswerGiving.cs`) | built |
| `VoiceLibraryBuilder` | `Voice/` folder, `Hr.json` | regenerates `VoiceLibrary.asset` and `VoiceScript.csv`; `BuildScript.Build` runs it first | built |

No planned part is shown: work that is only planned (a parent PIN, tracker T11, `docs/research/parent-pin-gate.md`) has no
code here and is not mapped.

## Main flows

**Screen navigation from `AppRoot`** (`AppRoot.Awake` builds every screen once, adds each panel to `ScreenNavigator`,
which shows exactly one at a time, then calls `ShowStart()`). Each arrow is a callback passed in `AppRoot.Awake`:

```
ShowStart
 ├─ no profile yet ─────────────► GateScreen ─(passed)─► ProfilesView
 └─ profiles exist ─► ProfilePickView
        ├─ child picked (starts SessionClock, reads the session number) ─► ActivityMenuView
        │        ├─ Arithmetic ─► ArithmeticScreen ─(back)─► ActivityMenuView
        │        ├─ Comparison ─► ComparisonScreen ─(back)─► ActivityMenuView
        │        ├─ NumberLine ─► NumberLineScreen ─(back)─► ActivityMenuView
        │        └─ (back) ─► ShowStart
        └─ parent area ─► GateScreen ─(back)─► ShowStart
                          └─(passed)─► ProfilesView ─(done)─► ShowStart
                                  └─ a profile's progress ─► ProgressView ─(back)─► ProfilesView
                                          └─ school checks ─► CheckResultsView ─(back)─► ProgressView
                                                  └─ start a check ─► SchoolCheckScreen ─(ends, finished or left)─► CheckResultsView
```

The parent area is reached through `GateScreen` only; one `ParentGate` object is shared for the whole run, so its 30-second
wait cannot be escaped by leaving the screen and coming back (comment in `AppRoot.cs`). Source: `Presentation/AppRoot.cs`.

**Play one round** (`ArithmeticScreen`; `ComparisonScreen` and `NumberLineScreen` do the same with their own item sets):

1. `Begin(activeLog, sessionNumber)` reads the log and builds `ItemSchedule.Build(log.ReadAll())`.
2. `RoundPlanner.Plan(itemSet, schedule, sessionNumber, random)` picks the round's items (`ComparisonRoundPlanner` for comparison).
3. A `RoundSession` is created over those items, the log, the schedule and the clock.
4. Each answer goes through `AnswerGiving`, then `RoundSession.Submit`, which calls `IAnswerLog.Append`; a failed write is
   handled by `DiskWrite.TryRun`.
5. At the round's end, `RoundEndPanel.SuggestBreak(SessionClock.BreakDue)` is called; `BreakDue` is true once the picked
   profile has played 10 minutes (`SessionClock.BreakAfter`). The suggestion shows `PhraseKey.BreakSuggestion` and speaks it
   with `VoicePlayer.PlayAfterCurrent`, so it waits for the last answer's feedback. Nothing is locked.
   Sources: `ArithmeticScreen.cs:173`, `ComparisonScreen.cs:215`, `NumberLineScreen.cs:201`, `RoundEndPanel.cs:86-87`.

**Build** (`BuildScript.Build`): `VoiceLibraryBuilder.Rebuild()`, then `BuildPipeline.BuildPlayer` for the scene
`Assets/Znalica/Scenes/Main.unity` to `Builds/Windows/Znalica.exe` (the editor's own build-scene list is empty); a
result other than `Succeeded` throws. A broken `Hr.json` or a non-`.ogg` clip is logged but does not fail the build; the
completeness test is the guard.

No binding order is fixed by a spec for these flows that this map checked; the technical spec
`docs/specs/2026-11-16-grade-1-maths-first-version-tech.md` was not read for flow order beyond its step 10.

## Entry points and integrations

- **In:** `AppRoot.Awake` (a `MonoBehaviour` placed in `Main.unity`); `BuildScript.Build` (batch `-executeMethod`);
  the menu item `Znalica/Rebuild Voice Library`; NUnit EditMode tests.
- **Out:** files in the data folder and `Builds/Windows/Znalica.exe`. No network or outside service found in the three runtime
  assemblies (text search of Domain, Storage and Presentation for `UnityWebRequest`, `System.Net`, `HttpClient`, `Socket`: 0 hits).
