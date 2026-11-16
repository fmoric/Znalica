# Znalica — First Version: Grade 1 Mathematics (Functional Spec)

**Status:** draft for review · **Date:** 2026-11-16 · **Next:** technical spec (`write-tech-spec`)

## Goal

A pupil in grade 1 of a Croatian primary school practises, at home on a Windows PC, the core of grade 1
mathematics: **numbers 0–20** and **addition and subtraction to 20**. A parent can see what the child has practised,
what is secure and what still goes wrong.

The game teaches through its feedback, not through points: every mistake is followed by a short spoken explanation,
and the facts a child misses come back sooner. Success is measured on school-like tasks, not on the game score.

Decisions taken with the product owner (2026-11-16): Windows desktop app; grade 1 maths, domains A/B only; pupil plus
a parent progress view behind a simple parent gate; progress kept only on this PC; instructions in a recorded human
Croatian voice.

## Users

- **Pupil** — about 6–7 years old, still learning to read, plays alone at home. Uses a mouse and the number keys.
- **Parent** — opens the progress view, runs the school-like check, manages profiles. Reads Croatian.

## Activities and the outcomes they practise

Each activity names the curriculum outcome it practises (Kurikulum nastavnog predmeta Matematika, NN 7/2019, grade 1).
Each format carries the strength of its evidence, from the knowledge-store article on learning-game evidence.

| Activity | What the child does | Outcome code | Format and its evidence |
|---|---|---|---|
| **Brojevna crta** (number line) | Places a spoken or shown number on a 0–20 line; finds the number before and after; jumps along the line | `MAT OŠ A.1.1` — *Opisuje i prikazuje količine prirodnim brojevima i nulom* | Number-line game. Evidence for 1–10 (Siegler 2009); **11–20 is inference — to be checked with children** |
| **Veće, manje, jednako** (compare) | Chooses `<`, `>` or `=` between two numbers or two groups of objects; puts 3–5 numbers in order | `MAT OŠ A.1.2` — *Uspoređuje prirodne brojeve do 20 i nulu* | Sorting and choosing with an explanation after each error. Inference from the scaffolding result |
| **Računanje do 20** (add and subtract) | Short recall rounds: answers `8 + 5`, `13 − 6`, and missing-number forms `8 + ? = 13`; types or clicks the answer | `MAT OŠ A.1.4` (assumed, see Open questions) and `MAT OŠ B.1.1` — *Zbraja i oduzima u skupu brojeva do 20* | Recall with the answer produced, missed items return sooner. Evidence for times tables (retrieval, spacing); **for addition to 20 it is inference** |

## Process

1. The child starts Znalica. If there is more than one profile, they pick their own by its picture.
2. The child picks one of the three activities from a picture menu. Each activity's instruction plays in the recorded
   voice; a speaker button repeats it.
3. A **round** is a short set of items (proposed: 10). After each answer the child sees and hears whether it was right.
   - **Right:** brief praise, next item at once. No animation stands between the child and the next item.
   - **Wrong:** a short explanation, spoken and shown. For a sum, the jump on the number line (8, then 5 more, 13);
     for a comparison, the two quantities side by side. The child then answers the same item again. The item returns
     later in the round and early in the next session.
4. At the end of a round the child sees how many items they got right first time, and can play another round or stop.
   A session is meant to be short (proposed: about 10 minutes); after that Znalica suggests a break but does not lock.
5. The parent opens the **parent area** through the parent gate (see Business rules), and sees progress, runs the
   school-like check, or manages profiles.

## Acceptance criteria

1. Each of the three activities can be played start to finish with the mouse alone; the add/subtract answer can also
   be typed with the number keys.
2. Every instruction and every feedback message plays in the recorded voice and is also shown as text; the speaker
   button replays it.
3. No number shown, asked or answered is below 0 or above 20, in any activity.
4. In **Računanje do 20**, the child produces the answer (number pad or keys); it is never a choice from offered answers.
5. A wrong answer always gets an explanation before the next item; a right answer never waits on an animation.
6. An item answered wrong appears again later in the same round and within the first rounds of the next session.
7. The game shows no leaderboard, no ranking against other children and no timer pressure on the child.
8. A child cannot enter the parent area by clicking around: the parent gate stops a grade 1 pupil.
9. The parent area shows, per profile and activity: rounds played, share right first time, the facts or numbers still
   missed most, and practice over the last weeks.
10. The parent can run a **school-like check**: a plain set of items in the format of a school worksheet, with no
    hints and no feedback during it, scored and shown separately from the game score.
11. Progress survives closing Znalica in the middle of a round; every answer is saved as it is given.
12. Znalica works with no network connection, and sends no data off the PC.
13. A parent can delete a profile and all its progress.

## Data

Kept only on this PC, per profile:

- **Profile:** a display name or nickname chosen by the parent (no surname, no birth date), a picture, created date.
- **Answer record:** activity, item (for example `8 + 5`), the answer given, right or wrong, right first time or not,
  time to answer, date and time.
- **Item schedule:** for each item, when it is next due (used by the "missed items return sooner" rule).
- **Check result:** date, items, answers, score.
- **Settings:** voice volume, the parent gate setting.

No account, no e-mail, no online service.

## Screens

1. **Start / profile pick** — profile pictures; "add profile" only through the parent area.
2. **Activity menu** — three large pictures, one per activity, each with a spoken name.
3. **Brojevna crta** — the 0–20 line, the number to place, the child's marker.
4. **Veće, manje, jednako** — two numbers or two groups of objects, the three signs as large buttons.
5. **Računanje do 20** — the task, a large on-screen number pad, the answer box.
6. **Feedback** — the explanation shown in place on the activity screen, not as a separate page.
7. **Round end** — right first time out of the round, "play again" and "stop".
8. **Parent gate.**
9. **Parent area — progress** — per profile, per activity (acceptance criterion 9).
10. **Parent area — school-like check** — runs the check and shows past results.
11. **Parent area — profiles and settings** — add, rename, delete profile; voice volume.

All text the child sees is Croatian, kept outside the code so it can be corrected without a rebuild.

## Business rules and edge cases

- **Number range:** 0–20 everywhere. A sum never exceeds 20; a difference is never below 0.
- **Fact set for Računanje do 20:** every addition and subtraction fact within 0–20, in the four forms the curriculum
  lists for one fact (`3 + 7 = 10`, `7 + 3 = 10`, `10 − 3 = 7`, `10 − 7 = 3`) plus missing-number forms.
  Easier facts (to 10) come before harder ones (crossing 10).
- **Missed items return sooner:** a missed item is due again in the same round and early next session; an item
  answered right several times in a row is asked less often.
- **Feedback teaches:** no feedback is "wrong" alone; it always shows or says why.
- **Praise is short;** no points shop, no unlockables, no story between items.
- **Parent gate:** a step a grade 1 pupil cannot pass but an adult passes in seconds (for example a sum beyond grade
  1, or holding a button for several seconds while a word is read). No password to forget.
- **Closed mid-round:** the next start resumes at the activity menu; answers already given are kept.
- **No sound device or muted:** text is still shown and the game remains playable.
- **First start, no profile:** Znalica asks for the parent gate and the first profile.
- **Several children on one PC:** each has a profile; progress never mixes.

## Out of scope for this version

- Other grade 1 maths outcomes: ordinal numbers (`MAT OŠ A.1.3`), word problems (`MAT OŠ A.1.5`), patterns and
  sequences (`MAT OŠ B.1.2`), and domains C (shapes), D (measuring, money) and E (pictograms).
- Other grades and subjects.
- Online accounts, sync, a teacher view, multiplayer, leaderboards, payments.
- Browser and tablet versions (the game may later be built for them; nothing here depends on that).
- Adaptive difficulty beyond the "missed items return sooner" rule.

## Open questions

1. **`MAT OŠ A.1.4` text.** On the NN 7/2019 web page the code `A.1.4` has no text under it, and the sentence
   *Zbraja i oduzima u skupu brojeva do 20* follows the code `B.1.1`, with a note that `B.1.1` marks domain-B content.
   This spec assumes the sentence is `A.1.4`'s outcome. To confirm from the printed (PDF) edition.
2. **Mouse and screen sizes for six-year-olds.** The evidence article gives touch-target sizes only; nothing was found
   for mouse use at this age. Proposed: test the first build with a few children.
3. **Number line 11–20.** The evidence covers 1–10; whether the same game helps for 11–20 is to be checked with
   children.
4. **Round and session length** (10 items, about 10 minutes) are proposals, not research results.
5. **Voice recording.** Who records it, and how new phrases are added later.
6. **The app's shown name and art style** (simple, clear, cartoon or schematic, per the evidence).
7. **Distribution beyond the family.** Rules for children's apps were not researched; if Znalica is published, they
   need research first.

## Sources

- Kurikulum nastavnog predmeta Matematika, NN 7/2019, grade 1, domains A and B:
  <https://narodne-novine.nn.hr/clanci/sluzbeni/2019_01_7_146.html> (read 2026-11-16).
- Knowledge store: `docs/articles/tools/CROATIA_PRIMARY_SCHOOL_CURRICULUM_SUBJECTS_PER_GRADE_AND_OUTCOME_CODES.md`.
- Knowledge store: `docs/articles/LEARNING_GAME_EVIDENCE_AND_GAME_FORMAT_PER_CONTENT_TYPE.md`.
- Research brief behind both: Knowledge-Documentation,
  `docs/research/2026-11-16-croatia-primary-curriculum-learning-games-brief.md`.
