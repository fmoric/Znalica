# Znalica — First Version: Grade 1 Mathematics (Technical Spec)

**Status:** draft for design review · **Date:** 2026-11-16 · **Functional spec:**
[`2026-11-16-grade-1-maths-first-version.md`](2026-11-16-grade-1-maths-first-version.md) · **Next:** design-reviewer
grade (B or better to build)

Line references of the form *FS:n* point to lines of the functional spec.

## Intake Assessment

**Verdict: at bar — designed.** The functional spec has a clear goal (FS:7–12), thirteen testable acceptance criteria
(FS:51–67), a data list (FS:71–80), screens (FS:84–96) and business rules (FS:100–113). Its seven open questions
(FS:126–137) either carry a proposal this design adopts (round of 10, session of about 10 minutes, FS:133) or do not
change the design (an outcome code's label, voice talent, art style, later testing with children, publication).

One small gap, carried to *Risks and Open Questions* as Q1: the data list names "the parent gate setting" (FS:78), but
no screen (FS:94) and no rule (FS:108–109) says what a parent sets. **Resolved:** the product owner dropped it, and
the functional spec now says the gate has no setting in this version (FS:78, commit `057e3c6`).

Product-owner constraints taken as given: Windows desktop app; grade 1 maths, domains A/B; pupil plus a parent view
behind a simple parent gate; progress only on this PC; spoken instructions from pre-made clips, placeholders for
now, with the voice method (recorded human or generated) left open until release (FS:134); every name starts with a
capital letter (applied below to types, assemblies, folders, files, enum values and text keys).

## Goal

Build, from an empty repository, a Unity application for **Windows 64-bit standalone** that runs three practice
activities on the numbers 0–20 (number line, comparison, addition and subtraction), records every answer to local files
as it is given, schedules missed items to return sooner, speaks every instruction and feedback from pre-made voice clips with
the same text on screen, and offers a parent area behind a gate with progress, a school-like check and profile
management. Nothing is sent off the PC.

## Starting Point

What was examined, and what it showed:

- **No code exists.** `git ls-files` lists 0 `.cs` files; there is no `Assets/`, `ProjectSettings/` or `Packages/`
  folder at the repository root. The symbol server (`serena`, project active at the repository root, C# language server
  running) finds no `BuildScript` symbol — the class the `AGENTS.md` build command calls does not exist yet.
- **The code graph holds only the agent packet.** `graphify-out/graph.json` (1194 nodes) has PowerShell guard scripts
  and documentation as its most connected nodes (`Invoke-GuardSimple`, `Get-GuardPosixCommands`, …); no game code.
- **The build command targets the wrong platform.** `AGENTS.md` → *Commands* builds `-buildTarget WebGL`; the product
  owner chose Windows desktop. `TODO.md` (item T4) records the same mismatch. Step 1 fixes it.
- **`.gitignore` is already Unity-ready** (`Library/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, `Logs/`, `UserSettings/`,
  `*.csproj`, `*.sln`, `TestResults.xml`).
- **No Unity editor at the default Hub path** on this machine (`C:\Program Files\Unity\Hub\Editor` and
  `C:\Program Files\Unity Hub` absent; nothing else searched). See R1.
- **No prior design to reuse.** Knowledge store searched for a prior game or spaced-repetition design (`technical spec
  design educational game spaced repetition item schedule`) and for Unity local persistence (`Unity persistentDataPath
  save data local file JSON`): no Unity or game-design article; the two articles the functional spec cites were read
  and their *Design Rules That Follow* section shapes the approach below.

So every component below is new. There are no existing patterns to follow or bypass; the design sets them.

## Affected Components

All paths are new. Assembly boundaries are Unity assembly definitions (`.asmdef`); the arrows are the only allowed
references.

```
Znalica.Presentation ──► Znalica.Storage ──► Znalica.Domain
        │                                        ▲
        └────────────────────────────────────────┘
Znalica.Editor (editor only) ──► Znalica.Presentation, Znalica.Storage, Znalica.Domain
Znalica.Tests.EditMode ──► Znalica.Domain, Znalica.Storage
```

Everything a test must reach sits in `Znalica.Domain` or `Znalica.Storage`, including the phrase catalog's model and
its completeness check (*Text and voice*). `Znalica.Presentation` holds only what plays, draws or reads input, so the
test assembly needs no reference to it.

| Assembly | Folder | Holds | Unity engine reference |
|---|---|---|---|
| `Znalica.Domain` | `Assets/Znalica/Domain/` | Numbers and items, item keys, item sets, round engine, scheduler, explanations, parent gate, progress and check calculations, `PhraseKey` and the phrase catalog model with its completeness check, the `IAnswerLog`, `IClock`, `IRandom` (seedable, so tests are repeatable) and `IProblemLog` contracts | **None** (`noEngineReferences: true`) — plain C#, fully testable in EditMode |
| `Znalica.Storage` | `Assets/Znalica/Storage/` | File answer log (JSON Lines), profile store, settings store, `AtomicFileWriter`, data-folder layout, `PhraseCatalogFile` (reads `Hr.json`) | Yes — `JsonUtility` and `Application.persistentDataPath` |
| `Znalica.Presentation` | `Assets/Znalica/Presentation/` | `AppRoot` (composition root), `ScreenNavigator`, one view per screen, `VoicePlayer`, `VoiceLibrary`, input | Yes |
| `Znalica.Editor` | `Assets/Znalica/Editor/` | `BuildScript.Build` (global namespace, to match the `AGENTS.md` command); `VoiceLibraryBuilder` | Editor only |
| `Znalica.Tests.EditMode` | `Assets/Znalica/Tests/EditMode/` | NUnit tests via the Unity Test Framework | Editor test assembly |

Content outside the code:

| Content | Location | Why there |
|---|---|---|
| All Croatian text (child and parent) | `Assets/StreamingAssets/Text/Hr.json` | FS:96 — correctable without a rebuild; StreamingAssets ships as loose files beside the player (unverified, R2) |
| Voice clips (placeholders now; recorded or generated later) | `Assets/Znalica/Voice/<ClipName>.ogg`, indexed by name in a `VoiceLibrary` ScriptableObject that `VoiceLibraryBuilder` regenerates from the folder | Imported assets: no runtime file loading; the clip source changes by replacing files, not code (*Approach* §8) |
| Profile pictures | `Assets/Znalica/Art/ProfilePictures/` | Bundled set; art style is the functional spec's open question 6 |
| Main scene | `Assets/Znalica/Scenes/Main.unity` | One scene; screens are panels |

`AGENTS.md` → *Commands* changes in step 1 (build target). No other existing file changes.

## Data and Interfaces

### Numbers and items (`Znalica.Domain`)

- `NumberRange` — `public const int Min = 0; public const int Max = 20;` and `bool Contains(int)`. A constant by
  `look_before_you_write` Test 1: it is the curriculum range, part of the code's meaning. Every generator and every
  answer input checks against it (AC3).
- `Activity` enum — `NumberLine`, `Comparison`, `Arithmetic`. Croatian names come from the text catalog
  (`ActivityNameNumberLine` → *Brojevna crta*, and so on).
- `Difficulty` enum, ordered — `WithinTen`, `BeyondTenWithoutCrossing`, `CrossingTen`.
  - Arithmetic: `WithinTen` when every number in the fact is ≤ 10; `CrossingTen` when the fact crosses 10
    (`8 + 5`, `13 − 6`); otherwise `BeyondTenWithoutCrossing` (`12 + 3`, `17 − 4`). FS:103 orders "to 10" before
    "crossing 10" and says nothing of the middle group; placing it between them is this design's choice.
  - Number line and comparison: `WithinTen` when every number is ≤ 10, otherwise `BeyondTenWithoutCrossing`. The
    functional spec states easy-first only for arithmetic; extending it is this design's choice.
- `Item` — abstract class with `ItemKey Key`, `Activity Activity`, `Difficulty Difficulty`, `Explanation Explain()`
  and `bool IsCorrect(Answer)`. Three subclasses:
  - `ArithmeticItem` — `int First`, `Operation Operation` (`Addition`, `Subtraction`), `int Second`, `int Result`,
    `Unknown Unknown` (`Result`, `First`, `Second`). Covers `8 + 5 = ?`, `8 + ? = 13`, `? + 5 = 13` and the three
    subtraction forms.
  - `ComparisonItem` — `ComparisonTask Task` (`ChooseSign`, `PutInOrder`), `Representation Representation`
    (`Numbers`, `Groups`), `int[] Numbers` (two for `ChooseSign`, three to five for `PutInOrder`).
  - `NumberLineItem` — `NumberLineTask Task` (`Place`, `Before`, `After`, `Jump`), `int Target`, `int Step` (signed;
    used by `Jump` only).
- `Answer` — a small value type: `int? Number`, `Sign? Sign` (`Less`, `Greater`, `Equal`), `int[] Order`. Exactly one
  is set, by activity.
- `ItemKey` — readonly struct wrapping a canonical string; `ToString()` and `static bool TryParse(string, out ItemKey)`
  round-trip. It is the item's identity in the answer log, the schedule and the progress view. Formats (ASCII `-` in
  keys; the screen shows `−`, U+2212):

  | Item | Key |
  |---|---|
  | `8 + 5 = ?` · `8 + ? = 13` · `? + 5 = 13` | `A:8+5=?` · `A:8+?=13` · `A:?+5=13` |
  | `13 − 6 = ?` · `13 − ? = 7` · `? − 6 = 7` | `A:13-6=?` · `A:13-?=7` · `A:?-6=7` |
  | Sign between 7 and 12, numbers / groups | `C:7?12` · `C:G7?12` |
  | Put 14, 3, 9 in order | `O:3,9,14` (sorted, so one set has one key) |
  | Place 14 · before 14 · after 14 · jump from 6 by +3 | `L:Place:14` · `L:Before:14` · `L:After:14` · `L:Jump:6+3` |

- `ItemFormatter` — the one place that turns an item or key into display text (`8 + 5`), used by activity screens,
  the progress view and the check (three callers, one formatter).

### Item sets (`Znalica.Domain`)

`IItemSet` — `Activity Activity`, `IReadOnlyList<Item> All` (deterministic order: by `Difficulty`, then by key) and
`bool TryFind(ItemKey, out Item)`. `TryFind` **builds the item from the key**, not by looking it up in `All`: the
planner turns due keys from the schedule back into items this way, and `PutInOrder` items are never in `All`. It
returns false (a log line written by a later or broken version; the planner then skips that key) for a key that:

- does not parse, or belongs to another activity;
- holds a number outside 0–20;
- is inconsistent: a stated result that is not the operation's result (`A:8+5=14`), or a missing number whose
  solution falls outside 0–20 (`A:3-?=7` would need −4);
- names a number-line item that cannot exist: `L:Before:0`, `L:After:20`, a `Jump` whose landing is outside 0–20
  (`L:Jump:18+5`) or whose `|Step|` is outside 1–10.

Three implementations:

- `ArithmeticItemSet` — every `a + b = c` and `c − a = b` with all numbers in 0–20, in all three unknown positions.
  FS:101–102's four forms of one fact are the two operand orders times the two operations, so they fall out of the
  enumeration. The generator asserts `NumberRange.Contains` on every number it produces.
- `ComparisonItemSet` — every `ChooseSign` pair in 0–20 in both representations; `PutInOrder` sets are drawn from a
  seeded random source when a round is planned (the space of 3–5-element sets is too large to enumerate usefully),
  and become schedule entries like any other key once answered. `TryFind` rebuilds one from its `O:` key (three to
  five numbers, ascending, distinct, each in 0–20; anything else returns false).
- `NumberLineItemSet` — `Place` 0–20, `Before` 1–20, `After` 0–19, `Jump` with start and landing both in 0–20 and
  `|Step|` 1–10.

### Answer record and the answer log

`AnswerRecord` (domain type; one JSON object per line on disk):

| Field | Type | Meaning |
|---|---|---|
| `Activity` | `Activity` | Which activity |
| `ItemKey` | string | The item (FS:74 "item") |
| `Answer` | string | The answer given, canonical (`13`, `Less`, `3,9,14`) |
| `IsCorrect` | bool | Right or wrong |
| `Attempt` | `AttemptKind` — `First`, `Retry`, `Return` | `First`: first time the item is asked this round; `Retry`: straight after an explanation; `Return`: later in the same round. *Right first time* = `Attempt == First && IsCorrect` |
| `ResponseMilliseconds` | int | Time to answer; recorded, never shown to the child (AC7) |
| `AnsweredAt` | string, ISO 8601 with offset (`DateTimeOffset.ToString("o")`) | Date and time |
| `SessionNumber` | int | Per-profile session counter (see scheduler) |
| `RoundId` | string (GUID) | Which round |

`IAnswerLog` — `void Append(AnswerRecord)` and `IReadOnlyList<AnswerRecord> ReadAll()`. Two implementations, both with
callers: `FileAnswerLog` (`Znalica.Storage`, used by the app) and `InMemoryAnswerLog` (tests). `Append` returns only
after the line is on disk (AC11).

### Item schedule (derived, not stored)

`ItemSchedule` — per profile, `ItemProgress Get(ItemKey)` with `ItemProgress { int Box; int DueSession; }`.
`ItemSchedule.Build(IEnumerable<AnswerRecord>)` replays the log; `Apply(AnswerRecord)` updates it as each answer is
recorded. FS:76's "item schedule" is satisfied as this derived structure — see *Approach* §4 for why it is not a
second file.

Rules (only `First` attempts move an item; `Retry` and `Return` do not):

- First attempt wrong → `Box = 0`, `DueSession = SessionNumber + 1`.
- First attempt right → `Box = min(Box + 1, 4)`, `DueSession = SessionNumber + Interval[Box]`, with
  `Interval = { 1, 1, 2, 4, 8 }` sessions.
- An item never answered is *new*.

The intervals are a proposal in the same sense as FS:133's round size: one named constant array, changed in one
place.

### Profile, settings and check result (`Znalica.Storage`)

Data folder: `Application.persistentDataPath` (on Windows, a per-user folder under `AppData\LocalLow\<Company>\<Product>`
— unverified, R2). Layout:

```
<data folder>/
  Settings.json
  Profiles/
    <ProfileId>/
      Profile.json
      Answers.jsonl
      Checks.jsonl
```

- `Profile` — `ProfileId` (GUID string; also the folder name), `DisplayName` (1–20 characters after trimming;
  letters, spaces and hyphens), `PictureId` (name of a bundled picture, e.g. `Fox`), `CreatedAt` (ISO 8601). No
  surname or birth date field exists (FS:73).
- `Settings` — `VoiceVolume` (0.0–1.0, default 1.0). No gate setting (FS:78).
- `Checks.jsonl` — one line per check answer, `CheckId`, `ItemKey`, `Answer`, `IsCorrect`, `AnsweredAt`, and one
  closing line per finished check, `CheckId`, `CompletedAt`, `Score`, `ItemCount`. `CheckResult` (FS:77: date, items,
  answers, score) is read back by grouping lines by `CheckId`; a check with no closing line is listed as *not
  finished* and gets no score.
- `ProfileStore` — `List()`, `Create(DisplayName, PictureId)`, `Rename(ProfileId, DisplayName)`, `Delete(ProfileId)`
  (deletes the profile folder recursively: profile, answers and checks together, AC13).
- `AtomicFileWriter.Write(path, text)` — writes `path.tmp`, then `File.Replace(tmp, path, null)` when `path` exists,
  else `File.Move(tmp, path)`. `File.Replace` throws `FileNotFoundException` when the destination is missing
  ([File.Replace](https://learn.microsoft.com/dotnet/api/system.io.file.replace)), hence the two branches. Two callers:
  `Profile.json` and `Settings.json`.
- `FileAnswerLog.Append` opens the file for append, writes one UTF-8 line, and calls `FileStream.Flush(true)`, which
  also clears the operating system's intermediate buffers
  ([FileStream.Flush(Boolean)](https://learn.microsoft.com/dotnet/api/system.io.filestream.flush)). `ReadAll` skips a
  line that does not parse (the one a power cut can leave half-written) and logs a warning; it never rewrites the
  file.
- Serialisation: `JsonUtility` over `[Serializable]` DTO classes with public fields; dates and enums as strings. No
  JSON package is added. The DTOs live in `Znalica.Storage` and map to domain types, so the domain keeps no engine
  reference.

### Round engine (`Znalica.Domain`)

`RoundPlanner.Plan(IItemSet, ItemSchedule, int sessionNumber, IRandom) → IReadOnlyList<Item>` — ten items:

1. Due items (`DueSession ≤ sessionNumber`), lowest box first (so box 0, the items missed last time, lead), then
   oldest due — **at most 6**.
2. New items, easiest `Difficulty` first — **at most 4**.
3. Remaining slots: not-yet-due items, lowest box first.
4. Shuffled, with no item twice.

With box 0 first and 6 due slots a round, up to 12 items missed in one session all come back within the first two
rounds of the next session; more take a further round each — AC6's "within the first rounds of the next session".
Missing more than 12 distinct items in one session of about 10 minutes (about three rounds) is possible but rare.

`RoundSession` — a plain C# state machine, one per round; views render its state and call it:

| State | Enters when | Leaves by |
|---|---|---|
| `AwaitingAnswer` | Round starts; after a right answer; after `Continue()` | `Submit(Answer, int responseMilliseconds)` |
| `ShowingExplanation` | Wrong answer | `Submit` again (retry of the same item) or `Continue()` after a second wrong answer |
| `Ended` | No items left | — |

- `Submit` builds the `AnswerRecord`, calls `IAnswerLog.Append` and `ItemSchedule.Apply` **before** it returns the
  `AttemptOutcome` (`IsCorrect`, `Explanation`, `PraisePhrase`).
- Right → the next item is current at once; praise plays over it (AC5: nothing waits).
- Wrong on `First` → `ShowingExplanation`; the same item is asked again (`Retry`), and the item is inserted for a
  `Return` three places later, or at the end if fewer remain — at most one `Return` per item per round (FS:43, AC6).
- Wrong on `Retry` → the explanation now shows and says the right answer; `Continue()` moves on. The functional spec
  does not say what happens on a second wrong answer; this design ends the item there so a child cannot get stuck.
- `FirstTimeRightCount` and `PlannedCount` (10) feed the round-end screen (FS:44). `Return` and `Retry` attempts never
  count toward it.

`Explanation` — `ExplanationKind` plus the numbers it needs, and the `PhraseKey` that speaks it:

| Kind | Used by | Shows |
|---|---|---|
| `NumberLineJump` (`Start`, `Change`, `End`) | Arithmetic, `L:Jump` | Jumps on the 0–20 line: 8, then 5 more, 13 (FS:42) |
| `SideBySide` (`Left`, `Right`, `Representation`) | `ChooseSign` | Both quantities side by side, in ten-frames for groups |
| `OrderedSequence` (`Numbers`) | `PutInOrder` | The numbers in order on the line |
| `PositionOnLine` (`Target`, `Given`) | `Place`, `Before`, `After` | The child's mark and the right place |

Every wrong answer produces an `Explanation`; there is no "wrong" outcome without one (FS:105, AC5).

### Session and break

`SessionNumber` = 1 + the highest `SessionNumber` in the profile's log, fixed when the profile is picked. A session
lasts until Znalica closes or another profile is picked. `SessionClock` (domain, over `IClock`) starts at the pick;
at round end, if 10 minutes or more have passed, the round-end screen adds the spoken break suggestion and keeps both
buttons (FS:45, no lock). Closing mid-round needs no special handling: the answers are in the log, the next start
shows the profile pick and then the activity menu (FS:110).

### Parent gate (`Znalica.Domain`)

`ParentGate` — `GateChallenge Next(IRandom)` returns a multiplication of two factors 3–9, **written in words, not
digits, and not spoken** (*"Koliko je šest puta sedam?"*); `bool Check(GateChallenge, int)`. A wrong answer draws a new
challenge; three wrong in a row make the gate wait 30 seconds before the next one. Multiplication is grade 2 content,
reading the words is beyond most grade 1 pupils, and an adult answers in seconds (FS:108–109, AC8). No password.

- **Text:** `PhraseKey.GateQuestion` (*"Koliko je {First} puta {Second}?"*) with the factors filled from the text-only
  keys `NumberWord3`–`NumberWord9` (*tri* … *devet*; these forms do not change with gender). All eight have `Text` and
  an empty `Voice`, so the completeness check accepts them without clips.
- **Guessing limit:** there are 49 factor pairs and the answer lies between 9 and 81; a random two-digit guess hits about 1 time in
  90. With three tries, then a 30-second wait, a child pressing keys at random gets about 6 tries a minute, so passing
  by chance takes on average about a quarter of an hour of steady guessing. The gate stops a grade 1 pupil "clicking
  around" (AC8); it does not stop an older sibling who can read and multiply, and is not meant to.

### Progress (`Znalica.Domain`)

`ProgressCalculator.For(IReadOnlyList<AnswerRecord>, ItemSchedule, DateTimeOffset now) → ProgressReport`, per
`Activity` (AC9):

- `RoundsPlayed` — distinct `RoundId`s.
- `FirstTimeRightShare` — right `First` attempts ÷ all `First` attempts.
- `StillMissed` — up to 5 item keys with the most wrong `First` attempts in the last 28 days whose current `Box` is
  0 or 1, shown through `ItemFormatter`.
- `WeeklyPractice` — rounds and answers for each of the last 6 calendar weeks (Monday-based).

### School-like check (`Znalica.Domain`)

`SchoolCheckPlanner.Plan(IRandom) → IReadOnlyList<Item>` — 20 items drawn evenly across difficulties, ignoring the
schedule: 6 arithmetic with the result unknown, 4 with a missing number, 4 `ChooseSign` with numbers, and 6 existing
`NumberLineItem`s with task `Before` or `After` (keys `L:Before:14`, `L:After:12`), shown as a worksheet strip — `_, 14`
or `12, _` — with the answer typed, not clicked on a line. No new task or key form. Two pages of ten, one answer box
or sign box per item, plain style; one spoken instruction per section and nothing else — no hints, no feedback, no explanation (AC10). Answers go
to `Checks.jsonl`, never to `Answers.jsonl`, so the check neither moves the schedule nor mixes with the game score.

### Text and voice

**Model and check — `Znalica.Domain`:**

- `PhraseKey` enum — every instruction, praise, explanation, break suggestion, spoken activity name, and the
  screen-only texts (buttons, parent area, gate).
- `PhraseCatalog` — `Phrase Get(PhraseKey)` with `Phrase { string Text; string[] Voice; }` and
  `string Text(PhraseKey, args)`; a missing key gives the key name and reports it to `IProblemLog`.
- `IProblemLog` — `void Report(string message)`, injected like `IClock`, because the domain has no engine reference
  and so no `Debug.Log`. `AppRoot` passes an implementation that writes `Debug.LogError`; tests pass one that
  collects the messages, so a test can assert that a missing key was reported.
- `PhraseCatalogCheck.Missing(PhraseCatalog, IEnumerable<string> clipNames)` — every `PhraseKey` has a text, and
  every clip name a phrase's `Voice` uses (number slots expanded to `Number00`–`Number20`) is in `clipNames`. The
  test passes the file names it lists in `Assets/Znalica/Voice/` with `System.IO` — no Unity object needed.

**File — `Znalica.Storage`:** `PhraseCatalogFile.Read(path)` parses `Hr.json` with `JsonUtility` into the domain
model. The file is an **object wrapping the list**, because `JsonUtility` is believed not to parse a top-level array
(unverified, R2 (e)):

```json
{ "Entries": [
  { "Key": "PraiseRight", "Text": "Bravo!", "Voice": ["PraiseRight"] },
  { "Key": "ExplainJump", "Text": "{Start}, pa još {Change}, to je {End}.",
    "Voice": ["{Start}", "ThenMore", "{Change}", "Is", "{End}"] },
  { "Key": "ButtonNext", "Text": "Dalje", "Voice": [] }
] }
```

**Spoken forms (Croatian *jedan/jedna/jedno*, *dva/dvije*):** a number before a noun changes with the noun's gender,
so a joined clip would be wrong there. Two rules keep the joined clips correct:

1. **Every phrase without a number is one whole-sentence clip** named after its key (`PraiseRight.ogg`,
   `InstructionArithmetic.ogg`, …).
2. **A phrase with numbers joins clips only where the number stands alone as a bare numeral** — in a sum, a count
   along the line, a comparison of two numbers (*"osam, pa još pet, to je trinaest"*) — never before a noun. Group
   comparisons say *"ovdje sedam, ondje dvanaest"*, not *"sedam jabuka"*. A phrase that needs a number before a noun
   gets its own whole-sentence clips, one per number, instead of a slot.

`Number00`–`Number20` are recorded in the bare counting form. A Croatian speaker the product owner names checks the
template texts against rule 2 when the catalog is written (step 2); a wrong form is a text and clip fix, not a code
fix.

**Residual risk, stated plainly:** FS:134 prefers whole spoken sentences; this design still joins bare-numeral clips
in sums, number-line counts and number comparisons, because one clip per sentence would mean one per fact and form
(hundreds). Rule 2 rests on that speaker's check alone: `PhraseCatalogCheck` sees clip names, not grammar, and
cannot tell a bare numeral from one before a noun. If the voice method chosen for release is generated speech, whole
sentences per item become cheap; using them would need a per-item clip lookup this design does not build — a design
change to make then, not now.

**Playback — `Znalica.Presentation`:**

- `VoiceLibrary` — ScriptableObject mapping clip name → `AudioClip`. It is **generated**, not hand-edited:
  `VoiceLibraryBuilder` (`Znalica.Editor`) scans `Assets/Znalica/Voice/` and maps each file by its name, and
  `BuildScript.Build` runs it before every build.
- **The asset is committed** (`Assets/Znalica/Voice/VoiceLibrary.asset`, with `VoiceScript.csv`). An editor session
  plays from the committed asset; after adding or renaming clips the developer runs the menu command
  `Znalica/Rebuild Voice Library` (`VoiceLibraryBuilder`) and commits the result. A stale asset in the editor costs
  only sound — a clip it lacks leaves the text on screen — and never ships, since every build regenerates it. Tests
  do not use the asset at all: the completeness test reads the folder's file names (above).
- **Changing the clip source needs no code change.** Placeholders, a human recording or clips generated with a speech
  service (the functional spec's open question 5) are all the same: files with the names the catalog lists, dropped
  into the folder. `VoiceLibraryBuilder` also writes `Assets/Znalica/Voice/VoiceScript.csv` (clip name, the Croatian
  sentence to say) as the script for whoever or whatever produces the clips.
- `VoicePlayer` — plays a phrase as a queue of clips on one `AudioSource` at `Settings.VoiceVolume`; `ReplayLast()`
  backs the speaker button (FS:39, AC2). A missing clip, no audio device or volume 0 leaves the text on screen and the
  game playable (FS:111).

### Input

Mouse for everything (AC1). Number entry: an on-screen pad 0–9, *Obriši*, *U redu*; keys 0–9 (top row and numpad),
Backspace, Enter through the Input System package. The answer box refuses any keystroke that would make its value
above 20 (after `2`, only `0`) — no answer outside 0–20 can be entered (AC3). That rule is `AnswerEntry`
(`Znalica.Domain`: `bool TryType(int digit)`, `Erase()`, `int? Value`), so it is tested in EditMode; the pad and the
keys only call it. Number-line answers are a click on the
line, snapped to the nearest whole number; ordering answers are clicks in order (no drag).

## Approach

1. **Plain C# core, thin Unity shell.** All rules the acceptance criteria test — ranges, forms, retry and return,
   scheduling, progress, gate, check — live in `Znalica.Domain` with no engine reference, so EditMode tests cover them
   without a scene. Views hold no rules. *Rejected:* logic in MonoBehaviours — untestable without PlayMode and scenes.
2. **One round engine for three activities.** The three activities share the round, retry, return, scoring and
   logging behaviour; only the item set, the answer type and the explanation differ. The spec names all three callers,
   so the shared engine is built with the first activity and the other two plug in an `IItemSet`.
3. **Append-only answer log, written per answer.** One JSON line per answer, flushed to disk before the engine moves
   on (AC11). *Rejected:* SQLite — a native plugin for a few thousand rows a year; one JSON file rewritten per answer —
   a crash mid-write loses everything, where a half-written last line loses one answer.
4. **The schedule is derived from the log, not stored.** Replaying the log on profile pick gives the schedule, so
   there is one source of truth (`look_before_you_write`) and no second file to keep in step with the first, or to
   write atomically after every answer. Cost: a replay over the whole log at each pick — linear, and small at about
   100 answers a session. *Rejected:* a stored `Schedule.json` — two records of one fact that can disagree after a
   crash.
5. **One folder per profile.** Progress never mixes (FS:113), and deleting a profile is deleting one folder (AC13).
6. **uGUI with TextMeshPro, one scene.** `ScreenNavigator` shows one panel at a time; feedback is drawn inside the
   activity panel (FS:92). *Considered:* UI Toolkit — fine for menus and the pad, less direct for the number-line
   jump; one UI system for everything is simpler. The jump is drawn static, at once, with no animation (step 3b). A Canvas Scaler set to scale with screen size at a
   1920×1080 reference keeps targets large; buttons start at the 2 cm the touch guidance gives for under-nines, applied
   to the mouse by analogy (the functional spec's open question 2).
7. **Easy first, few new items per round.** At most 4 new items per round keeps new load small; the difficulty order
   decides which.
8. **Voice as imported assets; text as a loose file.** FS:96 requires text to be correctable without a rebuild; voice
   is not so required, and a new phrase needs a new clip anyway. Imported clips avoid loading audio from disk at
   runtime. Clips are found by file name through a generated `VoiceLibrary`, so the voice method (open until release,
   the functional spec's open question 5) changes by replacing files. *Trade-off:* a replaced clip needs a rebuild.
9. **No network code at all.** No `UnityWebRequest` to any URL, no Unity Services; the analytics module is removed
   from the package manifest and Unity Cloud Diagnostics is off. Step 10 checks it on the built player (AC12, R3).
10. **No hours or effort in this spec** — estimating is the `estimate-effort` skill's separate step.

## Work Sequence

Each step is a vertical slice for one session, names its check, and lists what blocks it. "EditMode tests" means the
`AGENTS.md` test command, filtered to the named tests where a full run is not the gate.

1. **Project skeleton that builds for Windows.** Create the Unity project (pinned version, R1), the five assembly
   definitions and their references, `Main.unity` with `AppRoot` and an empty `ScreenNavigator`, `BuildScript.Build`
   building `StandaloneWindows64` to `Builds/Windows/Znalica.exe`, one EditMode smoke test, the Input System package,
   the analytics module removed. Change the `AGENTS.md` build command to the Windows 64-bit target (`-buildTarget
   Win64` — the command-line name is unverified, R2; confirm it in step 1). Confirm R2's platform behaviours in the
   built player and record them in this spec.
   *Check:* the batch build exits 0 and `Znalica.exe` starts; the smoke test passes.
   *Blocked by:* none — can start now (needs a Unity install, R1).
2. **Text and voice catalog, and the activity menu that speaks.** `PhraseKey`, `PhraseCatalog`,
   `PhraseCatalogCheck`, `PhraseCatalogFile`, `Hr.json` (wrapped object), `VoiceLibraryBuilder` with `VoiceScript.csv`,
   `VoicePlayer` with replay and volume; the activity menu with three pictures, each speaking its name; placeholder
   clips under the final names (Q3). A Croatian speaker reads the template texts against the spoken-forms rule.
   *Check:* the completeness test (`PhraseCatalogCheck` over the voice folder's file names); a `PhraseCatalogFile`
   test reading a small wrapped `Hr.json`; clicking each picture plays its name and shows it; with the audio device
   disabled, the text still shows; replacing one placeholder file by another with the same name changes the voice
   after a rebuild, with no code change.
   *Blocked by:* 1.
3a. **Arithmetic items and the round engine (domain only).** **The riskiest step:** the round engine is the design's
   core, and the three activities, the scheduler and the progress view all build on its records. `NumberRange`,
   `ItemKey`, `Difficulty`, `ArithmeticItemSet`, `Explanation`, `RoundPlanner` (new items only — no schedule yet),
   `RoundSession` over `InMemoryAnswerLog`, `AnswerEntry` (the answer box's rule: refuse a keystroke that would pass
   20). Tests: every generated number in 0–20; the four forms of a fact present; `ItemKey` round-trip; `TryFind`
   rebuilds an item from its key; the answer-box rule; wrong → explanation → retry → return later; a second wrong
   ends the item; right first time counted only on `First`; `TryFind` returns false for `A:8+5=14` and `A:3-?=7`.
   Domain-only, so it is checked by tests alone; 3b is where it is first seen playing.
   *Check:* the tests above.
   *Blocked by:* 2 — `Explanation` and `AttemptOutcome.PraisePhrase` carry a `PhraseKey`, which step 2 creates; 3a
   adds its own entries to that enum.
   **Step 3a findings (2026-11-17; by EditMode tests in the editor).** Built as specified, with these points the spec left open and 3a settled by the smallest choice, each open to the product owner or the next step: (1) **Subtraction needs its own explanation phrase.** `ExplainJump` says *pa još* (then more), wrong for `13 − 6`; 3a appends `ExplainJumpBack` and `ThenLess` (placeholder text *pa manje za*, a copy of `Is.ogg` as clip) for the Croatian speaker to replace. `Explanation.Change` is signed; the spoken `{Change}` is its size. (2) **`Difficulty` of a fact is defined on the fact, not the form:** `CrossingTen` when both parts are below 10 and the whole is above it, so the four forms and the three unknowns of one fact share a difficulty; facts that start from or land on 10 (`10 + 5`, `13 − 3`) are `BeyondTenWithoutCrossing`. (3) **`RoundPlanner` in 3a has no schedule, so every item is new and the cap of 4 new items cannot apply:** it fills the 10 slots from the easiest `Difficulty` first, drawing at random inside a level and moving to the next level only when one is used up, then shuffles the whole round. With no schedule, play therefore stays `WithinTen` (396 items); step 5 must add the cap, the due items and the fill, and Q4 below must be settled first. (3a) **Return distance, exactly:** a missed item comes back **after three other items**, or last if fewer than three other items remain in the round (`RoundSession.ReturnDistance`); tests cover exactly three, exactly two and one remaining. (4) **A wrong answer on a `Return` ask** is not covered by the spec: 3a gives it a `Retry`, then `Continue()` after a second wrong answer, and never a second `Return` (at most one per item per round). (5) `A:8+5=14` is refused because a key must have exactly one `?`; the inconsistent-result rule is thereby subsumed. (6) `Answer` holds a number only; the sign and the ordering arrive with steps 6 and 7. `IClock`, `IRandom` (with `SeededRandom`) and `AttemptOutcome` (`PraisePhrase` is `PhraseKey?`, `PraiseRight` on a right answer) were added because `RoundSession` and `RoundPlanner` need them. `RoundSession` does not call `ItemSchedule.Apply` yet: it does not exist until step 5, and the records it needs are in the log.
3b. **Računanje do 20 screen, playable end to end in memory.** The arithmetic screen over `RoundSession`: number pad,
   keys, in-place feedback with the number-line jump, praise that does not hold the next item; the round-end screen.
   *Check:* a round played with the mouse only and one with the keys only; a wrong answer shows and speaks the jump
   before the retry.
   *Blocked by:* 2, 3a.
   **Step 3b findings (2026-11-17; by EditMode tests, a batch build and a throwaway editor probe that drove the screen's
   code; the screen was not seen, so its manual checks are open).** Built as specified, with these choices where the spec
   was silent, each open to the product owner or the next step: (1) **Where the rules sit.** `ArithmeticPlay`, with
   `AnswerKey` (pad button or key) and `Feedback`, in `Znalica.Domain`: what a key does to the answer box, when an answer
   is given, the feedback that follows, the response time (from the moment the item was last put in front of the child),
   and the round-end numbers. `ArithmeticScreen` (Presentation) only draws it and maps buttons and keys to `AnswerKey`.
   `ItemFormatter` is in `Znalica.Domain` and returns tokens (`8`, `+`, `5`, `=`, the unknown), so the screen can draw
   the unknown as a box; it formats arithmetic items only and shows `−` (U+2212). The progress view's use of keys is not
   built, since it has no caller yet. `SystemClock` is added to Domain (the real `IClock`). (2) **Keys and the second
   wrong answer.** Enter (also the numpad's) confirms; after a second wrong answer Enter goes on, as the *Dalje* button does,
   so a round can be played from the keyboard alone. **The pad's *U redu* does not continue** (it does nothing then, and
   the pad is not interactable), so a double-click on it cannot skip the second explanation unseen; at most one Confirm
   is handled per frame. Going on stops the explanation's voice. The round-end buttons are mouse only. (3) **Feedback has no timer.**
   Praise stays beside the next item until the first key press of the next answer clears it; an explanation stays until the retry is answered or *Dalje*
   is pressed. The knowledge article on 6–7-year-olds gives no duration and suggests keeping it until the child moves on
   (an inference there, not a finding). (4) **The jump is drawn at once, not animated** (the product owner's decision; *Approach* 6 now says so): the 0–20
   line with the start and the landing marked and a bar with the size of the jump; the spoken phrase gives the order.
   An animation is a later addition if the manual check finds the picture hard to follow. (5) **Nothing
   is said that the spec gives no words for:** no spoken instruction (`InstructionArithmetic`), no spoken item, no spoken
   round-end. Clicking the picture speaks the activity's name, as in step 2, then opens the screen. New keys, screen only
   and appended at the end of `PhraseKey`: `ButtonErase` (*Obriši*) and `ButtonOk` (*U redu*), the spec's words;
   `ButtonPlayAgain` (*Igraj opet*), `ButtonStop` (*Završi*) and `RoundEndScore` (`{Right} / {Total}`) are placeholders for
   the Croatian review. **Not spoken yet, waiting for the voice method (Q3):** the spoken instruction and the spoken
   round-end (functional spec, line ~37 and AC2). **No back-to-menu button during a round** (a choice): a round is left
   by finishing it and pressing *Završi*. **Response times include speech:** a retry's response time includes the
   playback of the explanation, and the first item's includes the spoken activity name. (6) **Stand-ins until step 4:** every round is session 1, the answers go to one `InMemoryAnswerLog`
   for as long as the app runs, and the planner is seeded from `Environment.TickCount`. (7) **`ScreenNavigator` now
   works:** it is the component already in `Main.unity`, found with `FindFirstObjectByType`, with `Add` and `Show`; no
   scene change. (8) **Unity behaviour relied on, unverified in a player:** a uGUI `Button` clicked with the mouse holds the
   selection, and the Input System UI module would then also "click" it on Enter; every button therefore takes no
   keyboard navigation and clears the selection after a click (`UiBuilder.NewButton`), and the navigator clears it on a
   screen change. That the fallback font draws `−` and the layout at 1920×1080 are for the manual check, and so is this: *Ponovi* after
   *Dalje* replays the previous item's explanation (the last phrase spoken), and on entering the screen it replays the
   activity's name.
4. **Profiles, the parent gate, and answers saved to disk.** `ProfileStore`, `SettingsStore`, `AtomicFileWriter`,
   `FileAnswerLog`; `ParentGate`; screens: start/profile pick, gate, first-start flow (gate then first profile),
   profiles and settings (add, rename, delete, volume); `SessionNumber`. Tests: append survives reopening; a
   truncated last line is skipped; delete removes the folder; two profiles never share records; gate check and the
   30-second wait.
   *Check:* the tests; kill `Znalica.exe` from Task Manager mid-round, restart: the answers given are in
   `Answers.jsonl` and the app opens at the profile pick, then the menu.
   *Blocked by:* 3b (its screens and round to kill mid-way).
   **Step 4 findings (2026-11-18; by EditMode tests, each shown to fail with the code it guards broken, a batch build, and a 15-second headless run of the built player that logged no exception; the screens were not seen, so their manual checks and the kill-mid-round check are open).** Built as specified, with these choices where the spec was silent, each open to the product owner or the next step: (1) **Where the pieces sit.** `Znalica.Domain`: `ProfileName` (1-20 characters after trimming, letters, spaces and hyphens: no field and no character that lets a surname or a date in), `ProfilePictures` (six names, `Fox` to `Frog`), `SessionNumber.Next` (1 + the highest number in the log), `ParentGate`, `GateChallenge`, `GateResult`; `AnswerEntry` got a constructor with a higher limit (81 for the gate) and `Clear()`. `Znalica.Storage`: `DataFolder` (the one place that builds the paths), `AtomicFileWriter`, `FileAnswerLog` with its DTO, `Profile`, `ProfileStore`, `Settings`, `SettingsStore`. (2) **`ParentGate` is an object, not two static functions.** The spec's `Next(IRandom)` and `Check(challenge, int)` exist (static), but the three tries and the wait need state, so the gate is built over `IClock` and `IRandom` and takes `Press(AnswerKey)`, returning `GateResult` (`Typing`, `Passed`, `Wrong`, `Locked`). During the 30 seconds it takes no answer, the right one included, and hides the question. `AppRoot` keeps **one gate for the whole run**: a gate made new at each visit would let a child skip the wait by leaving and coming back. The question is built from `GateQuestion` and `NumberWord3`-`NumberWord9` (`GateChallenge.Text`), shown, never spoken. (3) **`FileAnswerLog` keeps a cut last line from spoiling the next answer.** If the file does not end with a line break (a half-written last record), `Append` writes a break first, so the new record is not glued to the fragment. `ReadAll` skips any line that is not `{...}`, does not parse, or has an unknown activity or attempt, a missing item key, answer or round id, or an unreadable date, and reports each through `IProblemLog` (one severity there, so the player log shows it as an error, not a warning). It never rewrites the file. A test cuts the second line at every position. The whole record goes to disk in one write, then `FileStream.Flush(true)`, which the .NET page says also clears the operating system's buffers ([FileStream.Flush](https://learn.microsoft.com/dotnet/api/system.io.filestream.flush)); that this reaches the disk on Unity's Mono runtime is unverified, and no test can show it (a killed process, unlike a power cut, loses nothing already handed to the operating system). A failed write (disk full, file locked) is not handled: the `IOException` leaves `Submit` and the screen. **After the code review (2026-11-18), a failed write is handled at the screens:** `DiskWrite.TryRun` (`Znalica.Domain`) catches `IOException` and `UnauthorizedAccessException`, the exceptions the .NET pages list for file and directory operations (for example [Directory.Delete](https://learn.microsoft.com/dotnet/api/system.io.directory.delete); `FileStream` write failures are `IOException` and were not looked up separately), writes the failure to the problem log and returns false; the arithmetic screen (the answer), the profiles screen (create, rename, delete, and the volume's save) show the new `PhraseKey.SaveFailed` (*Nije uspjelo spremanje. Pokušaj ponovno.*, placeholder Croatian, screen only, appended at the end) and keep running. `Append` throws before `Apply` and before the answer box is emptied, so the child presses U redu again; a test with a log that fails once shows the schedule, the round and the box unchanged. A failed create can leave an empty profile folder, which `List` reports and skips. Other exceptions are not caught. Also: `GateScreen.Begin` now empties digits typed before the child left the gate (`ParentGate.ClearEntry`, which keeps the wrong tries and the wait), and `InMemoryAnswerLog` moved to the EditMode test assembly. (4) **A profile id is checked to be a GUID before it is part of a path** (`DataFolder.Canonical`), so an id read from a file or passed in cannot make `Delete` reach outside `Profiles/`; `Delete` of a profile that is not there returns false. `List` leaves out a folder whose `Profile.json` is missing, unreadable, names another id or holds a name or picture the rules refuse, and reports it. Profiles are listed oldest first. `Rename` changes the name only. `Settings.json` is written when the volume changes, not before; a missing or unreadable file gives volume 1.0. `AtomicFileWriter` writes UTF-8 without a byte order mark and flushes the temporary file to disk before the replace; `File.Replace` and `File.Move` work as the spec says on the editor's Mono (tests), and are unverified in the player. (5) **The flow.** The app opens at `ShowStart` in `AppRoot`: with no profile it shows the gate, and a passed gate opens the profiles and settings screen with its add form (the first-start flow); with profiles it shows the profile pick, and *Za roditelje* there leads through the gate to the same screen. *Natrag* on the gate or the profiles screen returns to `ShowStart`, which asks for the gate again while no profile exists. Picking a profile opens its `FileAnswerLog`, fixes `SessionNumber.Next(log.ReadAll())` and opens the activity menu; the menu got a small *Promijeni profil* button (spec: a session lasts "until another profile is picked", but no screen listed the way to it) that returns to the profile pick and drops the session. *Igraj opet* stays in the same session. (6) **Delete asks once more** (*Da* / *Ne*, with the child's name) before it removes a profile and all its progress; the spec gives no confirmation. Volume is changed with - and + buttons in steps of 10 %, applied and saved at each press (a slider is more code in a screen built in code). The name is typed into a TextMeshPro input field built in code. (7) **Profile pictures are placeholders:** a square in a colour of its own with the picture's name on it, no sprites, until the art style is chosen (functional spec open question 6). (8) **Step 3b's stand-ins are gone:** `ArithmeticScreen.Create` no longer takes a log, `Begin(log, sessionNumber)` does, and the planner seed is unchanged. The number pad and the key map were moved out of `ArithmeticScreen` into `NumberPad` because the gate is a second caller; behaviour is unchanged. `InMemoryAnswerLog` stays for tests only. (9) **New `PhraseKey`s, appended at the end, all screen only, with placeholder Croatian for the review and no clips** (nothing here is spoken, so no clip name exists to keep): `ButtonParentArea`, `ButtonBack`, `ButtonChangeProfile`, `ProfilePickTitle`, `GateWrong`, `GateWait`, `ProfilesTitle`, `ProfileNamePlaceholder`, `ProfileNameInvalid`, `ButtonSave`, `ButtonRename`, `ButtonDelete`, `ConfirmDeleteProfile`, `ButtonYes`, `ButtonNo`, `VolumeLabel`, `VolumeValue`. (10) **Unverified, for the manual check in the player:** that typing, including č, ć, đ, š, ž, reaches the code-built input field with the Input System as the only input handler (`activeInputHandler` is 1); that Backspace in the field does not also act elsewhere; the layout of the three new screens at 1920x1080; the wait count-down; and, as the spec's check says, killing `Znalica.exe` mid-round and restarting. **Not built, by the task's scope:** `SessionClock` and `Checks.jsonl` with its path (steps 10 and 9), `ItemSchedule` and the full `RoundPlanner` (step 5); Q4 is untouched.
5. **Missed items return next session.** `ItemSchedule` (build by replay, apply per answer) and the full
   `RoundPlanner` (due, new, fill). Tests: a first-time wrong item is due next session and is planned within the
   first two rounds; boxes rise on right answers and intervals lengthen; `Retry`/`Return` do not move an item.
   *Check:* the tests; miss an item, restart, see it again in the first rounds.
   *Blocked by:* 4.
   **Step 5 findings (2026-11-18; by EditMode tests, each shown to fail with the code it guards broken; the screen was not seen, so the manual check, miss an item, restart, see it again in the first rounds, is open).** Built as specified, with these choices where the spec was silent, each open to the product owner or the next step: (1) **Where the pieces sit.** `ItemSchedule` and `ItemProgress` in `Znalica.Domain`. `ItemSchedule.Get(ItemKey)` returns `ItemProgress?`, null for a new item (the spec's `Get` gives no way to say "new"), and `Entries` lists what has been answered. `RoundSession` takes the schedule as a constructor argument and applies each record after appending it, before `Submit` returns, as the spec says; `ArithmeticScreen.Begin` builds the schedule from `log.ReadAll()` once, and the same object is applied to and planned from for every round of the session, so *Igraj opet* sees the answers just given. `RoundPlanner.Plan(set, random)` is replaced by `Plan(set, schedule, sessionNumber, random)`. (2) **A new item starts as box 0.** The first right answer to a new item gives box 1, due after `Interval[1]` = 1 session; box 3 therefore takes three right first answers in three different sessions (due after 1, then 2 sessions), and box 4 a fourth. The spec's formula `min(Box + 1, 4)` is silent on the box of an item never answered. The intervals, `{ 1, 1, 2, 4, 8 }`, are one private constant array in `ItemSchedule`; `DueCap` (6), `NewCap` (4), `KnownBox` (3) and `PromotionPercent` (80) are named constants in `RoundPlanner`. (3) **Q4, "a fact is in box 3 or higher" means: at least one item of the fact is.** A fact is the unordered pair of parts and the whole (`Item.FactId`, `5+8=13` for all twelve items of 8 + 5 and its forms; an item of another activity is its own fact until steps 6 and 7 say otherwise). Counted as the spec's words read: the facts of a level are the distinct fact ids of its items, the known ones those with an item in box 3 or higher, and the level moves on when `known * 100 >= facts * 80` (exactly 80 percent moves; `CurrentLevel` is public so the progress view can show it). *Why "any":* "all items" or "80% of items" needs about 80% of the 396 within-ten items in box 3, which at 4 new items a round is the 100 rounds Q4 was raised to avoid; "any" needs 80% of the 36 within-ten facts, so a child reaches the next level after some 6 to 8 sessions (an estimate from the intervals, not measured). *Consequence, for the product owner:* a fact counts as known after one right form, so a child can be promoted with other forms of the same facts never asked. (4) **New items come from the current level only.** The current level is the easiest level below 80%, or the hardest if every level is at 80%. The unseen items of a lower, promoted level do not come back as new (the spec says new items come from the next level, and an "easiest first" over all levels would hold the promotion back behind the leftovers). They are then never asked: nothing in the spec brings them back. Drawing inside the level is random, as in step 3a. (5) **A round is topped up past the cap of 4 new items.** The spec's order is due (at most 6), new (at most 4), fill with items not yet due. With a new profile there is nothing to fill with, so the cap alone would give a round of 4; after the fill the planner adds more new items from the current level up to 10 (FS:39 says a round has ten). With enough scheduled items the cap holds: 4 new. A level with too few unseen items gives a shorter round (a set of three items gives three), and step 3a's rule of filling from the next level is gone. (6) **The fill is items not yet due, lowest box first; a due item over the cap of 6 is not fill** (it stays for the next round, as the spec's 12-items-in-two-rounds count relies on). Ties in box, then in due session, are broken at random with the seeded generator. A scheduled key the item set cannot build (`TryFind` false) is skipped. (7) **Existing tests that changed on purpose.** `RoundPlannerTests` pinned "no schedule" and are rewritten: `Plan_WhenTheEasiestLevelHasTooFew_FillsFromTheNextLevel` became `Plan_NewItemsStayInTheCurrentLevel_EvenWhenItHasTooFew` (a round never reaches into the next level now), and `Plan_ShufflesAcrossLevels_SoTheEasyItemsAreNotAlwaysFirst` became `Plan_ShufflesDueAndNewTogether_SoTheDueItemsAreNotAlwaysFirst` (its premise, new items of two levels in one round, no longer happens); the other planner tests take an empty schedule and a session number. `RoundSessionTests`, `ArithmeticPlayTests` and `ArithmeticScreenTextTests` only gained the schedule argument. (8) **Not built:** the progress view's use of the box (step 8); how the check, the comparison and the number line plug into the planner (steps 6, 7, 9). **The product owner
   confirmed (3) "any item" and (5) the top-up on 2026-11-18.** **After the code review (2026-11-18):** (9) **An item is asked at most once per session, so it moves at most once (finding 2's claim now holds).** Without this, a right first answer raised the box in round 1 and the fill of round 2 of the same session took the item back (not yet due) and raised it again. `ItemProgress` gained `LastSession`, the session of the answer that last moved the item, set by `ItemSchedule.Apply`, whose rule is unchanged. `RoundPlanner.Plan` leaves out of every scheduled pick (due, and fill) an item whose `LastSession` is the planning session; a new item has no entry, so once answered it is no longer new either, and a top-up draws only from items with no entry. Consequences: an item missed in round 1 does not come back in round 2 of the same session (it is due in the next); a box 1 item is due the session after its answer, so it is never fill, and fill is items of box 2 or more answered in an earlier session (the planner tests that built fill from box 1 or from a miss in the planning session were changed to build it from box 2 in the session before, and a new test plays two rounds in one session). With a small set, the second round may be shorter than ten. (10) **Finding 4, decided by the product owner:** "The product owner accepted on 2026-11-18 that unseen items of a promoted lower level are not asked again in this version."
6. **Veće, manje, jednako.** `ComparisonItemSet` (signs with numbers and groups, ordering 3–5), the comparison screen
   with three sign buttons, groups in ten-frames, click-to-order, `SideBySide` and `OrderedSequence` explanations.
   Tests: all numbers in 0–20; `O:` keys sorted; correctness of each sign; `TryFind` rebuilds a `PutInOrder` item
   from `O:3,9,14` with those numbers, and returns false for `O:3,3,9`, `O:9,3`, `O:3,9,21` and six numbers.
   *Check:* the tests; a round played to the end with the mouse.
   *Blocked by:* 3b (5 for scheduling, which then applies without change).
   **Step 6 findings (2026-11-18; by EditMode tests, each shown to fail with the code it guards broken, and a batch build; the screen was not seen, so its manual check, a round played to the end with the mouse, is open).** Built as specified, with these choices where the spec was silent, each open to the product owner or the next step: (1) **Where the pieces sit.** `Znalica.Domain`: `ComparisonItem` (one class with `Task`, `Representation` and `Numbers`, built by `ChooseSign` and `PutInOrder`, and by `TryParse` from a key), `ComparisonItemSet`, `ComparisonRoundPlanner`, `ComparisonPlay`, and the enums `Sign`, `ComparisonTask`, `Representation`. `Answer` gained `Sign` and `Order` (log forms `Less`, `3,9,14`); `Explanation` gained the kinds `SideBySide` (`Left`, `Right`, `Representation`) and `OrderedSequence` (`Numbers`). `ItemFormatter.SignText` gives `<`, `>`, `=`. (2) **`FactId` and `Difficulty` of a comparison item.** A sign item's fact is the unordered pair of its two numbers (`7?12` for `C:7?12`, `C:12?7`, `C:G7?12` and `C:G12?7`), so Q4's "any item of the fact in box 3" makes a pair known after one right sign in either order or representation, as for arithmetic; the within-ten level then holds 66 facts (242 items). An ordering item is its own fact (its key). `Difficulty` is as the spec has it: `WithinTen` when every number is 10 or less, otherwise `BeyondTenWithoutCrossing`; comparison has no third level, and `CurrentLevel` copes with that. (3) **Ordering items and the planner.** `O:` items are not in `All` (as the spec says), so `RoundPlanner` alone would never offer a new one. `ComparisonRoundPlanner.Plan` takes the `RoundPlanner` round (a missed ordering item comes back through the schedule, because `TryFind` builds it from its key) and swaps new sign items, from the end of the shuffled round, for freshly drawn ordering items until the round holds `OrderingPerRound` (2, a proposal like the other caps); an item that is already scheduled is never swapped, so a round of due items keeps them all, and a drawn key already in the round or the schedule is drawn again. A draw is three to five different numbers, 0 to 10 while the current level is `WithinTen`, otherwise 0 to 20 (the size is drawn too). Drawn items count in the cap of 4 new items. (4) **What the child sees.** Ordering items are shown in a fixed shuffle (`ShownNumbers`, never already in order), so a rebuilt item looks the same. A group is drawn in ten-frames (two rows of five, 11 to 20 in a second frame, 0 in an empty one) as placeholder squares; the task shows no count, the explanation shows each count under its frame. The task draws the two quantities with an empty box between them and the three sign buttons below; the right sign goes in no box. Ordering is by click: the numbers are tiles, each click puts one in the next empty place, **the last click is the answer** (no U redu), and *Obriši* takes the last click back before that; a tile clicked twice, or a number not in the item, does nothing. A save failure leaves the earlier clicks as they were, as for the arithmetic box. The comparison screen has no keyboard input (the spec's *Input* gives keys for number entry only) and, as in 3b, no back-to-menu button during a round; after a second wrong answer only *Dalje* goes on. (5) **Explanations.** `SideBySide` draws both quantities with the right sign between them: numbers as large digits, groups in ten-frames with their counts. `OrderedSequence` marks the numbers on the 0-20 line, drawn at once, not animated (3b's decision, applied as the brief says). Phrases, all new at the end of `PhraseKey`, placeholder Croatian for the review: `ExplainLess`, `ExplainMore`, `ExplainSame` (*{Left} je manje od {Right}.* and so on, numbers only), `ExplainGroups` (*Ovdje {Left}, ondje {Right}.*, from the spec's spoken-forms rule 2, so a group explanation names the two counts and not the relation; the picture shows it), `ExplainOrder3` to `ExplainOrder5` (*Redom: {First}, {Second}, ...*, one key per count since a phrase has a fixed number of slots), and `InstructionOrder` (screen only, *Klikni brojeve od najmanjeg do najvećeg.*; no spoken instruction, Q5). The explanations are spoken (a wrong answer always is), so the six words they join have keys of their own, as `ThenMore` and `Is` do, and placeholder clips under their final names, copies of `Is.ogg`: `LessThan`, `MoreThan`, `SameAs`, `Here`, `There`, `InOrder`; the voice library and `VoiceScript.csv` were rebuilt. (6) **Reused and extracted at the second caller.** `RoundPlay` (domain: the round, the current feedback, the response time, going on after a second wrong answer, clearing praise) now sits under both `ArithmeticPlay` and `ComparisonPlay`, whose public behaviour is unchanged (the existing tests pass as they were). In Presentation: `RoundEndPanel`, `UiBuilder.TopRow`, `CentreTop`, `Clear` and `CentreRow` (from `ArithmeticScreen`) and `NumberLineDrawing` (the 0-20 line, from `ArithmeticScreen`'s jump, which now calls it and adds only its bar; step 7 is its third caller). `TenFrameDrawing` is new. Scheduling needed no change: `ComparisonScreen.Begin(log, sessionNumber)` builds the schedule from the profile's log and plans, plays and applies as `ArithmeticScreen` does. (7) **Unverified, for the manual check in the player:** the layout at 1920x1080 (rows at fixed offsets, the explanation picture above the *Ponovi* and *Dalje* buttons), that a tile destroyed by the redraw during its own click causes no error, that the ten-frames and the sign between the quantities read clearly to a child, and the placeholder clips' order against the texts.
7. **Brojevna crta.** `NumberLineItemSet` (place, before, after, jump), the number-line screen with click-to-place,
   `PositionOnLine` and `NumberLineJump` explanations. Tests: all numbers in 0–20; before/after bounds; `TryFind` false for `L:Before:0`, `L:After:20`, `L:Jump:18+5`
   and `L:Jump:6+11`; jumps land in
   range.
   *Check:* the tests; a round played to the end with the mouse.
   *Blocked by:* 3b.
   **Step 7 findings (2026-11-18; by EditMode tests, each shown to fail with the code it guards broken, and a batch build; the screen was not seen, so its manual check, a round played to the end with the mouse, is open).** Built as specified, with these choices where the spec was silent, each open to the product owner or the next step: (1) **Where the pieces sit.** `Znalica.Domain`: `NumberLineItem` (one class: `Task`, `Target`, `Step`, and `Solution`, the number the child gives), `NumberLineTask`, `NumberLineItemSet`, `NumberLinePlay`; `Explanation` gained the kind `PositionOnLine` (`Target`, `Given`). `Target` is the number the task names (the one to place, the one to find the neighbour of, the start of a jump); the answer of every task is one number (where it lies, the neighbour, where the jump lands). The set holds 371 items (21 `Place`, 20 `Before`, 20 `After`, 310 `Jump`), 141 of them within ten. `TryFind` is false for every key the spec lists and also for a jump of 0, a start above 20, a task written in lower case or as a number, and a number with a sign or a leading zero. (2) **`FactId` and `Difficulty` of a number-line item.** `Difficulty` is `WithinTen` when the number named and the answer are both 10 or less, otherwise `BeyondTenWithoutCrossing` (so `After:10`, whose answer is 11, is beyond ten); like comparison it has no third level. A fact is the pair of numbers the item is about, so Q4's "any item of the fact in box 3" counts a pair as known after one right form: `Before:14` and `After:13` are one fact (the neighbours 13 and 14), a jump and the one that undoes it (`6+3`, `9-3`) are one fact (the jump between 6 and 9), and `Place:n` is its own fact. The within-ten level then holds 76 facts (11 + 10 + 55) in 141 items, so 80 % is 61 facts, where each item as its own fact would need 113 of 141. Items of one fact always share a difficulty (tested). (3) **No planner of its own.** Every number-line item is in `All`, so `RoundPlanner.Plan` over `NumberLineItemSet` is the round (a test plans one and brings a missed jump back through the schedule); step 6's `ComparisonRoundPlanner` exists only because ordering items are not in `All`, so generalising it would have been a near-copy with nothing to swap. A round mixes the four tasks as it mixes the forms of a fact in arithmetic. (4) **What the child sees and does.** *Place:* the number in large digits, the 0-20 line with a tick for every number and a label only at 0, 10 and 20 (a label on every number would turn placing into reading), and the instruction *Klikni na crti gdje je ovaj broj.* A click anywhere on the line, between ticks included, is snapped to the nearest whole number (`NumberLinePlay.NearestNumber`, half way rounds up) and is the answer; it is right only when that number is the one asked for, with no tolerance beyond the snap, and a click past an end counts as that end. The line then has the whole width and the pad is hidden. *Before, After, Jump:* typed on the pad (`ArithmeticPlay`'s answer box, so 0-20 only), shown by `ItemFormatter.Tokens` as `? , 14`, `14 , ?` and `6 +3 = ?` (`6 −3 = ?` back); no line is drawn until a wrong answer, since a labelled line beside `? , 14` would show the answer. A click on the line does nothing for them, and a digit or U redu does nothing for a placing item; after a second wrong answer only *Dalje* goes on, as in step 6. No back-to-menu button, no spoken instruction (Q5), as before. (5) **Explanations.** A jump uses the existing `NumberLineJump` kind and `ExplainJump` / `ExplainJumpBack`, so nothing new is said for it. `PositionOnLine` draws the 0-20 line with every number labelled, the right place in the end colour and the number the task named in the start colour (for placing they are one); for placing, the child's own click is marked too in the start colour, from `NumberLinePlay.LastPlaced`, because `Item.Explain()` takes no answer. The picture is drawn at once. New `PhraseKey`s, appended at the end, placeholder Croatian for the review: `ExplainPlace` (*Ovdje {Target}.*), `ExplainBefore` (*Ispred {Given}, to je {Target}.*), `ExplainAfter` (*Iza {Given}, to je {Target}.*), all spoken; `InFrontOf` and `Behind`, single words with placeholder clips of their own names, copies of `Is.ogg`; and `InstructionPlace`, `InstructionBefore`, `InstructionAfter`, `InstructionJump`, screen only. The voice library and `VoiceScript.csv` were rebuilt. (6) **Reused, extended and extracted.** `RoundPlay` sits under `ArithmeticPlay`, which already served any item answered with a typed number; it gained `Give(number)` (an answer without the box, for a click), and `NumberLinePlay` wraps it and adds only what depends on the task. `NumberLineDrawing` (third caller) gained the optional `labelled` predicate, `DrawJump` (moved from `ArithmeticScreen`, which now calls it), `DrawPosition` and `NumberAt`, the inverse of `X`. Extracted at the second or third caller: `EquationDrawing` (the token row of `ArithmeticScreen`), `AnswerGiving` (the write-and-speak step with its save failure, third copy, now called by the three screens), `NumberPad.HandleKeys` (the key loop of `ArithmeticScreen`), `ItemOrder.SortEasiestFirst` (the sort of the three item sets) and `ItemKey.TryReadNumber` (from `ComparisonItem`). Behaviour of arithmetic and comparison is unchanged and their tests pass as they were. `LineClickArea` is new. `AppRoot` opens the screen from the menu's *Brojevna crta* picture, which already existed, with the profile's log and session number as for the other two. (7) **Unverified, for the manual check in the player:** that a click on a tick or label reaches the line's click handler (the event goes up to the nearest parent with a handler); that the click position is read correctly on the overlay canvas (`NumberAt` and `LineClickArea` are Presentation and have no test: a click on 14 must be right and one a third of the way to 15 must still be); the layout at 1920x1080; whether the ticks, 52 reference pixels apart, are easy enough to hit (the knowledge article on 6-7-year-olds gives a floor of 1 cm per click target, which this meets on a screen of about 24 inches and up, an inference, and it suggests offering a drag as well as a click, which the spec does not ask for and this does not build); that the labels at 0, 10 and 20 only are enough to place 11-19 (the evidence for the line covers 1-10, 11-20 is inference, as the functional spec says); and the placeholder clips' order against the texts.
8. **Parent area — progress.** `ProgressCalculator` and the progress screen, per profile and activity. Tests over a
   fixed log: rounds played, first-time share, still-missed list, weekly practice across a week boundary.
   *Check:* the tests; the screen for a profile with a few rounds in each activity.
   *Blocked by:* 4, 5 (`StillMissed` reads the box); 6 and 7 for their activities to show data.
   **Step 8 findings (2026-11-18; by EditMode tests, each shown to fail with the code it guards broken, and a batch build; the screen was not seen, so its manual check, a profile with a few rounds in each activity, is open).** Built as specified, with these choices where the spec was silent, each open to the product owner or the next step: (1) **Where the pieces sit.** `Znalica.Domain`: `ProgressCalculator` (static), `ProgressReport`, `WeekPractice`, and `ItemFormatter.Text(Item)`. `Znalica.Presentation`: `ProgressView`. The spec's `For(log, schedule, now) → ProgressReport "per Activity"` returns `IReadOnlyList<ProgressReport>`, one for each of `ProgressCalculator.Activities` (Arithmetic, Comparison, NumberLine, the menu's order), an activity with no answers getting an empty report, so the screen always has three columns. The schedule is the profile's one `ItemSchedule`, built from the same log; keys of the three activities do not collide (`A:`, `C:`/`O:`, `L:`). (2) **`FirstTimeRightShare` is `double?`**, null when the log holds no `First` attempt of the activity, so "no data" is not shown as 0 %; the screen shows whole percent, half away from zero, and for an activity with no rounds shows *Još nema odgovora.* instead of the three figures and the weeks. `RoundsPlayed` counts distinct `RoundId`s over the whole log, not only the last 28 days. (3) **`StillMissed`.** Candidates are items with a wrong `First` attempt at or after `now − 28 days` (the boundary instant included); a `Retry` or `Return` is not counted. The item must have a schedule entry with box 0 or 1 now, so an item missed a month ago and since answered right three times in a row is gone from the list even though its wrong answers are in the window. Order: most wrong first attempts, then the key's text in ordinal order (so the same log always gives the same list); at most 5. A record whose date cannot be read is left out of the window (it still counts in rounds and share). (4) **`WeeklyPractice`.** Six entries, oldest first and the current week last, each with its Monday; weeks with no practice are in the list with zeros. A week is Monday 00:00 to Sunday 24:00 of the calendar, computed in the offset of `now` (each record's `AnsweredAt` is converted to it), so a week is the parent's own week on the PC's clock; the calculator takes no time zone of its own. `Rounds` is the distinct round ids with an answer in that week, so a round that runs over midnight on Sunday counts once in each week; `Answers` is every record, `Retry` and `Return` included (the spec says "answers"). A record before the oldest week, or after the current one (a clock set back), is not counted; an unreadable date is not counted. Tested with `now` on a Wednesday, a Monday (00:30) and a Sunday (23:30), at the Sunday 23:59:59 / Monday 00:00 boundary, and with a date in another offset. (5) **`ItemFormatter.Text`** was added because `Tokens` only knows arithmetic and number-line items and the still-missed list holds comparison items too: arithmetic and number-line as their tokens joined by spaces (`8 + 5 = ?`, `6 +3 = ?`, `? , 14`, a placing item as its number); a sign item as `7 ? 12`, with brackets for groups (`(7) ? (12)`); an ordering item as its numbers in the order the child was shown them. `ProgressView` turns a key into an item through the activity's item set (`TryFind`) and shows the key's own text for one the set cannot build (a log line of another version). (6) **The screen** is reached from the profiles screen, so it is behind the parent gate with no gate of its own. Each profile row there got a *Napredak* button, left of *Preimenuj* and *Obriši*; to fit three buttons the name is on the upper line across the row and the buttons, 60 px high, on the lower line, in a row of 124 px (130 before), so five rows still fit the unscrolled list (the code review's warning against a first try at 190, which left room for three). *Natrag* on the progress screen returns to the profiles screen without redrawing it. It shows three columns, one per activity, each with the activity's name, *Odigrano rundi*, *Točno iz prvog pokušaja*, the still-missed items (or *Nijedan zadatak.*) and the six weeks as text lines (*Tjedan od 16.11.: rundi 3, odgovora 24*, the date being the Monday). No bars, colours or comparison between children or with a target: a parent reads plain figures. The log is read, and the numbers worked out, each time the screen opens; nothing is cached. (7) **New `PhraseKey`s**, appended at the end, all screen only, placeholder Croatian for the review, no clips: `ButtonProgress`, `ProgressTitle`, `ProgressNoAnswers`, `ProgressRounds`, `ProgressFirstTimeRight`, `ProgressStillMissedTitle`, `ProgressNoneMissed`, `ProgressWeeksTitle`, `ProgressWeekRow`. The existing completeness test fails when one lacks its text (shown with `ProgressWeekRow` taken out of `Hr.json`). (8) **Evidence for the layout.** The knowledge base holds nothing on how parents read progress for 6-7-year-olds (one search, nothing on the topic; the article on learning games says adult support changes the result, which supports plain figures a parent can talk through, an inference); the screen is the simplest reading of AC9. (9) **Unverified, for the manual check in the player:** that three columns of up to 15 lines each fit above *Natrag* at 1920x1080 (the heights are estimated from the font sizes, not seen), that a long profile name fits on its line, that the three buttons of a row fit its 900 pixels, that `dd.MM.` prints as *16.11.* on the player, and that a profile with a log of thousands of lines opens the screen without a pause (the log is read in full). Presentation has no EditMode tests, as decided, so nothing here tests the screen; the numbers it shows are the calculator's, which is tested. **Not built, by the task's scope:** the school-like check's results in the list (step 9), `SessionClock` (step 10).
9. **Parent area — school-like check.** `SchoolCheckPlanner`, the worksheet screens (no feedback), `Checks.jsonl`,
   past results listed with unfinished checks marked. Tests: 20 items in the stated mix, all in range; the 6
   number-line items are `Before`/`After` and scored from a typed number; check answers
   never reach `Answers.jsonl`; an unfinished check has no score.
   *Check:* the tests; one check run through, its score in the list, the game progress unchanged.
   *Blocked by:* 4.
   **Step 9 findings (2026-11-18; by EditMode tests, each shown to fail with the code it guards broken, and a batch build; the screens were not seen, so their manual check, one check run through with its score in the list and the game progress unchanged, is open).** Built as specified, with these choices where the spec was silent, each open to the product owner or the next step: (1) **Where the pieces sit.** `Znalica.Domain`: `SchoolCheckPlanner` (`Plan(IRandom)` and `SectionOf(Item)`), `CheckSection` (`Result`, `MissingNumber`, `Sign`, `Neighbour`), `SchoolCheckSession` (the worksheet's rules, one per check), `ICheckLog`, `CheckLine`, `CheckHistory` and `CheckResult`. `Znalica.Storage`: `FileCheckLog`, its DTO `CheckLineDto` and `JsonLinesFile`; `DataFolder.ChecksPath` and `ProfileStore.CheckLog` name `Checks.jsonl` in the profile's folder. `Znalica.Presentation`: `SchoolCheckScreen` (the worksheet) and `CheckResultsView` (the list), reached from a new *Provjera* button on the progress screen (the progress screen is otherwise as step 8 left it). Both screens, like the progress screen, are behind the parent gate with no gate of their own. (2) **The draw.** The 20 items come out section by section (6 `Result`, 4 `MissingNumber`, 4 `Sign`, 6 `Neighbour`), so page one, the first ten, is all arithmetic and page two is the four sign items and the six number-line strips, and a section's items stand together under their instruction; within a section the order is shuffled with the seeded generator. "Evenly across difficulties" is read per kind, over the difficulties that kind has: `Result` 2, 2 and 2 over within ten, beyond ten and crossing ten; `Sign` 2 and 2 and `Neighbour` 3 and 3 over within ten and beyond ten (comparison and number line have no third level); `MissingNumber` has four over three levels, so one each and the extra one goes to a level drawn at random (a test shows all three levels get it over 60 seeds). Within a level the draw is random and no fact occurs twice on a worksheet (`Item.FactId`, so `8 + 5 = ?` and `13 - 8 = ?`, or `L:Before:14` and `L:After:13`, never share one). The sign items are `ChooseSign` with numbers (no groups, which need ten-frames, not plain style) and include equal pairs; the number-line items are `Before` and `After` only, as the spec says (`Place` and `Jump` are not drawn, and `SectionOf` refuses them). The three item sets are built inside `Plan`, each time a check starts. (3) **The worksheet.** Two pages of ten in two columns, a column for each section: its instruction (screen text only) above its rows. An arithmetic item is drawn as its tokens with the unknown a white box (`8 + _ = 13`); a strip as the box, a comma and the number, `_ , 14` or `12 , _`, with the answer typed; a sign item as the two numbers with a box between them and three buttons *<*, *>* and *=* beside it that fill the box. One number box at a time is the one typed into (light blue; the first number box of the page at first, another by clicking it), the pad and the keys (digits, Backspace) fill it by `AnswerEntry`'s rule (no number above 20), and *U redu* or Enter moves to the next number box and records nothing. No sound, no picture, no hint, no praise, no right or wrong shown, and no explanation at any point. **The spec's "one spoken instruction per section" is not built: Q5 (spoken instructions) waits for the voice method, so the instruction of each section is screen text only, as in steps 3b to 7**; the four texts are `CheckInstructionResult`, `CheckInstructionMissing`, `CheckInstructionSign` and `CheckInstructionNeighbour`, and the sign one is written in words (*manje, veće ili jednako*) because a bare `<` could be read as the start of a text mesh tag. (4) **When an answer is recorded.** The game records an answer when it is given; a worksheet box can be changed, so the answers of a page are recorded when the parent presses *Dalje* (page one) or *Završi* (page two), one line for each of the ten items, an item left empty as an empty answer marked wrong. There is no way back to page one. The last page's finish also appends the closing line with the score. **A check that is left, by *Natrag* or by closing the app, therefore keeps the pages already finished and loses the page being filled in**, and it has no closing line, so it is listed as not finished with no score. A check left before page one is finished leaves no line at all, so it is not listed either. A failed write (`DiskWrite.TryRun`, as in step 4) shows `SaveFailed` and keeps the page; pressing the button again writes only the lines not yet written, so no line is recorded twice (tested with a log that fails at the fourth line and at the closing line). (5) **`Checks.jsonl`.** The spec's lines are one JSON object each with a `Line` field added that says which kind it is: `Answer` (`CheckId`, `ItemKey`, `Answer`, `IsCorrect`, `AnsweredAt`) or `Closing` (`CheckId`, `CompletedAt`, `Score`, `ItemCount`); the fields a kind does not use are written empty. `CheckId` is a GUID made when the check starts. `FileCheckLog` is the second caller of what `FileAnswerLog` did, so that moved into `JsonLinesFile` (append with the cut-last-line guard and `Flush(true)`; read that skips, reports and never rewrites a line that does not read back), and `FileAnswerLog` now calls it with its own DTO; its behaviour and its tests are unchanged (they pass as they were). A line that is cut, of an unknown kind, or whose score is above its item count is skipped and reported. A check's date is the time of its first answer, shown as `dd.MM.yyyy. HH:mm` in the offset it was stored with; the score is the number of right answers and the closing line's count is the number of items, shown as *n od 20*. `CheckHistory` lists checks in the order their first line appears (the order taken); a second closing line for one check is ignored. A check never touches `Answers.jsonl` or the schedule: a test runs a whole check through `ProfileStore` and finds no answers file, no answer records and an empty schedule, and another that a check taken after a game leaves the answers file byte for byte as it was. Deleting a profile removes `Checks.jsonl` with the folder (tested). (6) **The list.** `CheckResultsView`: the newest check first, at most 7 rows because the screen does not scroll (older checks stay in the log and are not listed; a later scrolling list is a change to this screen only), each row `{date}: {score} od {total}` or `{date}: nije završena`; *Nova provjera* starts a worksheet, *Natrag* returns to the progress screen without redrawing it, and a worksheet ends at the list, redrawn. Nothing compares a check with the game's score, as `AnswerRecord`'s comment foresaw; that is not built, since step 9 does not list it. (7) **Reused and extracted.** `AnswerEntry`, `NumberPad` (pad and keys), `ItemFormatter.Tokens`, `ItemFormatter.SignText`, `DiskWrite`, `ItemOrder`-sorted item sets, `FactId`, the plan's seeded random (`SeededRandom(Environment.TickCount)`, as the round screens), and `RoundPlanner.Shuffle` (was private, now `internal`, for the draw). Extracted at the second caller: `JsonLinesFile` (above), `UiBuilder.PlaceTopLeft` (from `ProfilesView`, whose eight calls now use it; no other change to that screen) and `UiBuilder.PlaceBottom` (the worksheet and the list). Not merged, by the task's scope: the three activity screens' round code (tracker T9). (8) **New `PhraseKey`s**, appended at the end, all screen only, placeholder Croatian for the review, no clips: `ButtonCheck`, `CheckTitle`, `ButtonNewCheck`, `CheckNoResults`, `CheckResultFinished`, `CheckResultUnfinished`, `CheckInstructionResult`, `CheckInstructionMissing`, `CheckInstructionSign`, `CheckInstructionNeighbour`, `CheckPage`; *Dalje*, *Završi*, *Natrag*, *Obriši*, *U redu* and `SaveFailed` are reused. (9) **Evidence.** The knowledge base holds nothing on worksheet-style checks for 6-7-year-olds (one search); the layout is the simplest reading of AC10 and of the pad rules already in use, and the 8 rows, the date format and the two-column layout are choices, not findings. (10) **Unverified, for the manual check in the player:** that two columns of up to six rows (96 reference pixels each, 6 px apart, 580 wide) and the pad on the right fit at 1920x1080 above the *Natrag* and *Dalje* buttons and that a row of a sign item (two numbers, a box and three buttons) fits its column; that the light blue marks the active box clearly enough; that a click on a box and the pad then fill that box and no other; that a digit, Backspace and Enter reach the screen with the Input System as the only input handler, and that Enter does not also press the last button; that a sign button and a number box are big enough for a 6-7-year-old with a mouse (the boxes are 110 by 84 and the sign buttons 75 by 84 reference pixels, more than the 52 pixels step 7 judged enough for the knowledge article's 1 cm floor on a screen of about 24 inches and up, an inference, not measured); that `dd.MM.yyyy. HH:mm` prints as *18.11.2026. 15:00*; that *Natrag* mid-check and then the list show the check as not finished; and, for a full run, that after a check the progress screen's numbers have not moved. **Not built, by the task's scope:** step 10 (`SessionClock`, the break suggestion, the release checks).
10. **Break suggestion and release checks.** `SessionClock` and the round-end break suggestion; a pass over AC1–AC13
    on the built player: mouse-only play of each activity; network check — run `Znalica.exe` with an outbound
    Windows Firewall block rule and confirm it plays, then without the rule watch the process's connections (Resource
    Monitor) through a session and confirm none; no timer, ranking or leaderboard anywhere. Update `docs/system-map.md`
    (new — the project then has structure to map).
    *Check:* a filled AC1–AC13 checklist with what was run for each; the break-suggestion test over a fake `IClock`.
    *Blocked by:* 5, 6, 7, 8, 9.

## PR Grouping

| PR | Steps | Why together, or alone |
|---|---|---|
| 1 | 1, 2 | Skeleton and catalog: infrastructure every later step stands on; small and reviewed together |
| 2 | 3a | The round engine — the core design and the riskiest step; its review needs full attention on its own |
| 3 | 3b | The first playable screen; checked by hand, kept apart so PR 2's review stays on the rules |
| 4 | 4, 5 | The on-disk format and the schedule derived from it — one data decision, reviewed as one |
| 5 | 6, 7 | Two activities on the finished engine; similar shape, cheap to review together |
| 6 | 8, 9 | The parent area's two screens, both read-only over the stored data |
| 7 | 10 | Release checks on the built player; mostly verification |

PR 2 starts after PR 1, since step 3a needs step 2's `PhraseKey`. PR 5 can be built beside PR 4 (steps 6 and 7
need only step 3b). Parallel work goes in a separate worktree, as `AGENTS.md` → *Overlap is allowed* says.

## Risks and Open Questions

### Open questions

- **Q1 — "The parent gate setting" (FS:78). Resolved:** the product owner dropped it; the functional spec now says the
  gate has no setting in this version (commit `057e3c6`). `Settings` holds `VoiceVolume` only.
- **Q2 — Unity version. Open, to settle at step 1.** *Recommended:* the newest Unity 6 LTS release available when
  step 1 starts, pinned in `ProjectSettings/ProjectVersion.txt`. Which release that is was not checked here (no Unity
  source reachable).
- **Q3 — Voice method (the functional spec's open question 5). Open until release, by the product owner's choice:**
  recorded human voice or clips generated with a speech service (the functional spec names two Croatian voices).
  The design does not depend on the answer: steps 2–10 run on placeholder clips under the final names, and the final
  clips replace them as files, with no code change (*Text and voice*). `VoiceScript.csv` (step 2) is the script for
  either method.

- **Q5 — Spoken instruction and spoken round-end. Open, waits for Q3.** The activity instruction (functional spec, line ~37)
  and the round-end (AC2) are not spoken by step 3b: `InstructionArithmetic` and a round-end phrase have no clips yet. They
  are added once the voice method is chosen.

- **Q4 — Pacing of new items. Resolved 2026-11-18 by the product owner: option (a).** New items come from the next
  `Difficulty` level once at least 80% of the current level's facts are in box 3 or higher; until then they come from
  the current level, easiest first, as before. The 80% is one named constant, a proposal like the R6 constants; (b) was
  not chosen. Step 5 builds it. The reasoning that led there: with "at most 4 new items per round, easiest
  first" (*Approach* 7), the `WithinTen` pool is 66 ordered pairs with a sum of 10 or less, times 2 operations times 3
  unknown positions = 396 items, so at 4 new items a round it takes about 100 rounds before the first
  `BeyondTenWithoutCrossing` or `CrossingTen` item can be new. Until step 5, `RoundPlanner` never leaves `WithinTen`, so step 3b's
  screen shows nothing beyond 10. The options the code review named: (a) promote a level once most of its
  facts are in box 3 or higher; (b) cap new items per unknown form so a round does not spend its new slots on one form.
  The count of 396 is the review's; it matches 66 x 6 from the enumeration in `ArithmeticItemSet` and is pinned by `ArithmeticItemSetTests.All_HoldsThreeHundredNinetySixWithinTenItems`.

### Risks

- **R1 — No Unity editor found on this machine** at the default Hub path (the only place looked). Without one, no
  step can build or run tests. The user approved installing Unity Hub; the editor version is Q2. Until an editor is
  installed, step 1 cannot start.
- **R2 — Unity behaviours this design relies on are unverified.** No curated Unity source was reachable: the knowledge
  store has no Unity article (two searches, above), `microsoft-learn` does not document the Unity runtime, and no
  library-documentation server is connected. **Step 1 findings are at the end of this entry.** Derived from general knowledge, unverified: (a) StreamingAssets ships as
  loose, editable files beside a Windows player; (b) `Application.persistentDataPath` on Windows is a per-user folder
  under `AppData\LocalLow`; (c) `JsonUtility` serialises `[Serializable]` classes with public fields but not
  dictionaries or `DateTime`; (d) the command-line build target for Windows 64-bit is `Win64`; (e) `JsonUtility`
  cannot parse a JSON document whose top level is an array — the reviewer's belief too, also unverified — which is
  why `Hr.json` wraps its list in `{ "Entries": [...] }`; the wrapped form works either way. Step 1 confirms all five
  before step 2 relies on them; knowledge-store research request 67 asks for a cited Unity article covering (a)–(d)
  and R3.
  **Step 1 findings (2026-11-16, Unity 6000.3.28f1, Mono, Windows 64-bit player built with
  `BuildScript.Build`; by observation of the built player, not from a cited source).** A throwaway probe script,
  run in the built player with `-batchmode -nographics` and then removed, wrote what it saw to a file:
  (a) **Confirmed.** A file under `Assets/StreamingAssets/Text/` arrived as the loose file
  `Znalica_Data/StreamingAssets/Text/<name>`; after editing it on disk without a rebuild, the player read the edited
  text. (b) **Confirmed.** `Application.persistentDataPath` was `C:/Users/<user>/AppData/LocalLow/Znalica/Znalica`
  (company name / product name from `ProjectSettings`). (c) **Confirmed.** `JsonUtility.ToJson` wrote public fields,
  strings with `č` and a `List<int>`, and dropped a `Dictionary<string,int>` and a `DateTime` field silently
  (`{"Kept":5}` for a class that had both). (d) **Confirmed.** `-buildTarget Win64` is accepted: the build succeeded
  (exit 0, `Builds/Windows/Znalica.exe`); an invalid name exits 21 and the editor lists the valid names, among them
  `Win` and `Win64`. (e) **Confirmed.** `JsonUtility.FromJson<T>("[1,2,3]")` throws `ArgumentException: JSON must
  represent an object type.` (also for `List<int>`); the wrapped `{"Entries":[1,2,3]}` parsed into `int[] Entries`.
  Not covered: an installed player (rather than the build folder), other Windows users, a user name with non-ASCII
  characters. R3 was not tested here (step 10).
  **Step 2 findings (2026-11-17, same editor; by observation in the editor and a headless run of the built player,
  not from a cited source).** (f) The project had no `com.unity.ugui` entry in `Packages/manifest.json`, so uGUI and
  TextMeshPro did not exist for the asmdefs; step 2 added `"com.unity.ugui": "2.0.0"` (the copy shipped inside the
  editor install) and imported TextMeshPro's *TMP Essential Resources* into `Assets/TextMesh Pro/`, without which no
  `TextMeshProUGUI` has a font. (g) `activeInputHandler` is 1 (Input System only), so the menu's event system uses
  `InputSystemUIInputModule`, and `Znalica.Presentation` references `Unity.InputSystem`. (h) A built player started
  with `-batchmode -nographics` for 12 s built the menu, read `Hr.json` from `Znalica_Data/StreamingAssets/Text/` and
  wrote no error line; what it draws and plays was not seen (manual checks, step 2).
- **R3 — "Sends no data off the PC" depends on the engine, not only our code.** Our code has no network calls; whether
  a Unity player contacts any Unity service by default is unverified (R2). Step 10's firewall and connection check is
  the test.
- **R4 — Croatian glyphs.** The TextMeshPro font asset must include č, ć, đ, š, ž, their capitals and `−` (U+2212);
  a missing glyph shows as a box. Step 2's catalog screen exposes it.
  **Step 2 finding:** the default font asset *LiberationSans SDF* is static with 250 characters and holds none of
  those eleven (`HasCharacter` without fallbacks: false for all). With fallbacks searched, all eleven are found, in
  the dynamic fallback asset *LiberationSans SDF - Fallback* that the essentials import adds. So the menu should show
  them; the first look at the built player (step 2's manual checks) confirms it, and a box in place of a letter there
  means the fallback did not reach the label.
- **R5 — The number line 11–20 and addition to 20 rest on inference** (FS:30, FS:32). The design does not depend on
  it; testing with children is the functional spec's open question 3.
- **R6 — Scheduler constants are proposals** (6 due, 4 new, intervals 1-1-2-4-8). Each is one named constant.

## Out of Scope

From the functional spec (FS:117–122), carried over:

- Grade 1 outcomes `MAT OŠ A.1.3`, `A.1.5`, `B.1.2`, and domains C, D, E.
- Other grades and subjects.
- Online accounts, sync, a teacher view, multiplayer, leaderboards, payments.
- Browser and tablet builds. The domain assembly has no engine reference, which keeps that door open; nothing else
  is done for it.
- Adaptive difficulty beyond "missed items return sooner".

Technical exclusions in this design:

- **An installer and code signing** — the build is a folder with `Znalica.exe`; distribution beyond the family is the
  functional spec's open question 7.
- **Exporting or backing up progress** — not asked for; progress lives in the data folder only.
- **PlayMode and UI automation tests** — the rules are tested in EditMode; screens are checked by hand in each step's
  check.
- **Localisation beyond Croatian** — one `Hr.json`; the catalog's shape would take a second file, but none is built.
- **A gate setting** — dropped by the product owner (Q1; FS:78).

## Coverage Check

| Functional point | Where in this design |
|---|---|
| AC1 mouse alone; keys for add/subtract | *Input*; steps 3b, 6, 7, 10 |
| AC2 voice and text for every instruction and feedback; replay | *Text and voice*; completeness test, step 2 |
| AC3 no number below 0 or above 20 | `NumberRange`, item sets, answer box; steps 3a, 6, 7, 9 |
| AC4 answer produced, never chosen | Number pad and keys, no offered answers; step 3b |
| AC5 explanation before the next item; right never waits | `RoundSession`, `Explanation`; steps 3a, 3b |
| AC6 wrong item returns in round and early next session | `Return` insertion; scheduler and planner; steps 3a, 5 |
| AC7 no leaderboard, ranking or timer pressure | Nothing built; response time never shown; step 10 check |
| AC8 child cannot enter the parent area | `ParentGate`; step 4 |
| AC9 progress per profile and activity | `ProgressCalculator`; step 8 |
| AC10 school-like check, no hints, separate score | `SchoolCheckPlanner`, `Checks.jsonl`; step 9 |
| AC11 every answer saved as given; survives a close mid-round | `FileAnswerLog.Append` with `Flush(true)`; step 4 |
| AC12 offline, no data sent | No network code; step 10 check; R3 |
| AC13 delete a profile and all its progress | `ProfileStore.Delete`; step 4 |
| FS:36 profile pick by picture · FS:84 add only via parent area | Start screen and profiles screen; step 4 |
| FS:38–39 picture menu, spoken instructions, speaker button | Step 2 |
| FS:39 round of 10 · FS:44 round-end screen | `RoundPlanner`, round-end screen; steps 3a, 3b |
| FS:45 about 10 minutes, break suggested, no lock | `SessionClock`; step 10 |
| FS:71–80 data | *Data and Interfaces*; schedule derived (§4); no gate setting (FS:78, Q1 resolved) |
| FS:96 text outside the code | `Hr.json` in StreamingAssets; R2(a) |
| FS:100–113 business rules | Range, fact set and difficulty, scheduler, explanations, praise only, gate, closed mid-round, no sound, first start, several children — each in *Data and Interfaces* |
| FS:117–122 out of scope | *Out of Scope* |
