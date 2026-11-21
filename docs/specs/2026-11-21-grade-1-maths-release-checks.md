# Znalica — First Version: Release Checks (AC1–AC13)

Step 10 of [the technical spec](2026-11-16-grade-1-maths-first-version-tech.md) asks for a filled AC1–AC13 checklist,
saying for each check what was run. The criteria are listed in that spec's *Coverage Check*.

- **Player:** `Builds/Windows/Znalica.exe`, built by `BuildScript.Build` on 2026-11-21 at commit `3ba89db`
  (branch `feat/break-suggestion`). Build log: `Logs/test-runs/step10-release-build.log`, ending `Build Finished,
  Result: Success.`, with 0 `error CS` lines.
- **Tests on the same commit:** the full EditMode suite, 571 of 571 passed (`Logs/test-runs/step10-full-1.xml`).
- **States:** *checked in code* means read in the code or the tests, not seen in the player. *Open* means the
  product owner's check in the player is still to do. Each player result is added when it is reported.

| AC | Criterion | Checked in code (what was run) | Player check (what to do) | Result |
|---|---|---|---|---|
| AC1 | Mouse alone; keys for add/subtract | No test covers this: the Presentation layer has no EditMode tests, by decision. In code, every answer has an on-screen pad, line or button, and the keys only add to the pad (`NumberPad.HandleKeys`). | Play one round of each activity (*Računanje do 20*, *Veće, manje, jednako*, *Brojevna crta*) and one school-like check with the mouse only. Then type answers with the keys in *Računanje do 20*. | Open |
| AC2 | Voice and text for every instruction and feedback; replay | The completeness test (`PhraseCatalogContentTests`) fails when a key lacks its text or clip. It passed. | In each activity, press *Ponovi* (replay) after feedback and after the break suggestion. The clips are placeholders, so check that something plays, not the wording. | Open |
| AC3 | No number below 0 or above 20 | Item set tests (`NumberRange`, `TryFind` bounds) and the `AnswerEntry` tests passed in the full run. | Try to type 21 or 25 in the answer box: it must refuse. | Open |
| AC4 | Answer produced, never chosen | Arithmetic answers, and the number line's before, after and jump answers, are typed on the pad. Placing is a click on the line, and ordering a click on each tile. *Veće, manje, jednako* offers its three sign buttons (*<*, *>*, *=*), as the step 6 design has it; no screen offers a list of number answers. | Confirm that no screen shows number answers to pick from. | Open |
| AC5 | Explanation before the next item; a right answer never waits | `RoundSession` tests passed. | Answer wrong twice: the explanation shows, and only *Dalje* goes on. Answer right: the next item comes at once. | Open |
| AC6 | A wrong item returns in the round and early next session | `Return` insertion and scheduler tests passed. | Miss one item on purpose. It must come back later in the round, and again early in the next session (close and restart). | Open |
| AC7 | No leaderboard, ranking or timer pressure | A search of `Hr.json` and `Assets/Znalica/Presentation/*.cs` for *leaderboard, rang, ljestvic, timer, vrijeme, sekund, brzin, rank*. The only hits are the progress screen's first-try share (a parent's figure), the break suggestion's *Vrijeme je za…*, and a code comment saying the progress screen ranks nothing. The response time is never shown. | Look at every screen: no clock, countdown, score table or comparison between children. | Open |
| AC8 | A child cannot enter the parent area | `ParentGate` tests passed. | Try the gate with a wrong answer and wait out the lock. | Open |
| AC9 | Progress per profile and activity | `ProgressCalculator` tests passed. | Open *Napredak* for a profile with a few rounds in each activity. | Open |
| AC10 | School-like check, no hints, separate score | `SchoolCheckPlanner` and `SchoolCheckSession` tests passed. A check never writes `Answers.jsonl` (tested). | Run one check through. Its score must be in the list, and the progress figures must be unchanged. | Open |
| AC11 | Every answer saved as given; survives a close mid-round | `FileAnswerLog` tests passed. That `Flush(true)` reaches the disk in the player is unverified (derived from source). | Answer 3 items, close Znalica with the window's X mid-round, and restart. The answers must be in progress. | Open |
| AC12 | Offline, no data sent | A search of the 155 `.cs` files under `Assets/Znalica` for `UnityWebRequest`, `HttpClient`, `WebClient`, `System.Net`, `Socket`, `TcpClient`, `UdpClient`, `WWW`, `Analytics`, `Application.OpenURL` found 0 hits; the pattern matches a test line, so the search works. `Packages/manifest.json` has no analytics, services, ads, purchasing or cloud package. `ProjectSettings/UnityConnectSettings.asset` has every `m_Enabled: 0`, crash reporting included. Whether the Unity player itself contacts a Unity service is unverified (R2/R3). | (1) Add an outbound Windows Firewall block rule for `Builds\Windows\Znalica.exe` (needs admin) and play a round: it must play. (2) Remove the rule, open Resource Monitor → Network → *Network Activity* and *TCP Connections*, and play a session. No `Znalica.exe` line may appear. (3) Remove any rule you added. | Open |
| AC13 | Delete a profile and all its progress | `ProfileStore.Delete` tests passed, removing the folder with `Checks.jsonl`. | Delete a test profile in the parent area. It must be gone from the pick screen, and its folder must be gone. | Open |

**Also in step 10, FS:45 (break after about 10 minutes, no lock).** `SessionClockTests` passed, 7 tests over a fake
`IClock`. In the player: play for 10 minutes. From then on, every round end must speak and show the break suggestion
after the last answer's feedback, with both buttons still there.
