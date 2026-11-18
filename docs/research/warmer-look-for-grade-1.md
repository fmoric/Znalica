# A warmer look for grade 1 — research brief

*Written 2026-11-18. Feeds the look work planned after step 10 of the first-version plan.*

## Overview

After the first play-test (2026-11-17) the product owner found Znalica "simple and not fun at all, so cold", and named
four causes: flat colours and backgrounds; no characters (profile pictures are coloured squares with a name); abstract
objects (ten-frames, counters and the 0–20 number line are plain shapes); no life or reward on a right answer or at round
end. This brief says what the evidence allows, what a warm look means for 6–7-year-olds, how art drawn in Claude
desktop gets into Unity 6.3, how to add motion without breaking AC5, and how to slice the work.

**Scope, as read from the brief (no user to ask):** the decision this feeds is *what the look work contains and how it
is built*. Depth is a decision brief (2–4 pages), not a literature review. The constraints: grade 1 (ages 6–7), mouse
play on Windows; Unity 6000.3.28f1 with uGUI and TextMeshPro, screens built in code; the art drawn by the product owner
in Claude desktop; and the functional spec's rules (AC5, AC7, open question 6). Not covered: sound effects and music,
grades above 1, and children's-app publishing rules.

**Recommendation in one paragraph.** Make the game warm *around* the task and keep the task area plain. Use a soft
palette, rounded shapes, one mascot that delivers the feedback, and animals for the profile pictures. Counters become
simple drawn objects, all identical, with no detail inside them. A right answer gets a short bounce on something that
stays on screen (a progress row, the mascot). It never delays the next item. Round end shows a brief celebration that
tells the child how they did, with no currency, shop or collection. The owner draws SVG in Claude desktop. The SVG is
turned into PNG at a fixed size and imported as sprites, as the activity pictures already are. Motion is one small
coroutine helper, with no tween library.

Marks: ✅ verified against the cited source, read this session · 🟠 needs care (read through a summary or abstract
only, a different age group, or our own inference from a source) · ❓ to confirm.

## Findings

### A. Learning design: what decoration, characters, motion and rewards do at 6–7

**What the project already holds**

- ✅ The functional spec says a right answer gets "brief praise, next item at once. No animation stands between the
  child and the next item" (`docs/specs/2026-11-16-grade-1-maths-first-version.md:40`). AC5 says "a right answer never
  waits on an animation" (same file, line 57). Open question 6 leaves the art style open: "simple, clear, cartoon or
  schematic, per the evidence" (line 135). AC7 rules out leaderboards and timer pressure (line 59).
- ✅ The knowledge-store evidence article (`LEARNING_GAME_EVIDENCE_AND_GAME_FORMAT_PER_CONTENT_TYPE.md`, *Design Rules*)
  says: "Spend the graphics budget on clarity". Effects and rewards are the main distractions in children's apps, and
  animation should not sit between a child and the next question. Its sources are Clark 2016 for visual realism
  (schematic g = 0.48, cartoon 0.32, realistic −0.01, a 🟠 moderator lead) and Meyer 2021 for distraction (58% of 124
  top children's apps scored low, with rewards and visual effects the main distractions, 🟠). The same article reports
  Brom 2022: the richer game version got more children through a first level, but the plain quiz got more correct
  answers. **Appeal brings a child back to voluntary play at home, and it can also cost practice.**

**What the wider research adds**

- ✅ **Irrelevant pictures inside a maths representation hurt this exact age.** Kaminski and Sloutsky (2013) trained
  122 children in kindergarten to grade 2 to read bar graphs. Children trained on bars filled with pictures often
  counted the pictures instead of reading the bar height (90% of kindergartners and 72% of first graders on one test).
  Children trained on plain bars read the graphs accurately. The lead author: "Any unnecessary visual information may
  distract children from the very procedures we want them to learn." ([OSU news](https://news.osu.edu/look-something-shiny-how-some-textbook-visuals-can-hurt-learning/))
- ✅ **A busy surrounding costs attention.** Fisher, Godwin and Seltman (2014) ran kindergartners through science
  lessons in a sparse and a decorated lab classroom. Off-task time was 38.6% in the decorated room against 28.4% in the
  sparse one, and test accuracy was 42% against 55%. This was a classroom wall, not a screen, and 24 children; the
  authors warn against over-generalising, so applying it to a game background is 🟠.
  ([CMU news](https://www.cmu.edu/news/stories/archives/2014/may/may27_decoratedclassrooms.html))
- 🟠 **Seductive details: a small but real cost.** A meta-analysis found that interesting but irrelevant additions
  lowered learning outcomes, and that the cost was larger when the detail sat next to the relevant picture or stayed on
  screen. Sundararajan and Adesope 2020; read through the university press release, mostly older learners.
  ([WSU news](https://news.wsu.edu/press-release/2020/03/19/seductive-details-inhibit-learning))
- 🟠 **Concrete objects: rich pictures help only with unfamiliar objects.** Petersen and McNeil (2013, 133 preschoolers)
  found that perceptually rich objects helped counting when children had little established knowledge of the objects,
  and hindered it when they had a lot. Abstract only, ages 3–5. ([doi:10.1111/cdev.12028](https://doi.org/10.1111/cdev.12028))
  Carbonneau, Marley and Selig (2013, 55 studies, kindergarten to college) found small to moderate gains from
  manipulatives, depending on how they are taught. ([abstract](https://asu.elsevierpure.com/en/publications/a-meta-analysis-of-the-efficacy-of-teaching-mathematics-with-conc/))
  *Inference:* a simple drawn counter (a ball, a button) is safe; a detailed toy or animal as the counted thing is not.
- 🟠 **Expected rewards that depend on performance can lower interest; positive feedback raises it.** In Deci, Koestner
  and Ryan's (1999) meta-analysis of 128 experiments, expected tangible rewards that depended on engagement, completion
  or performance lowered free-choice intrinsic motivation. Positive feedback raised it, and tangible rewards did more
  harm to children than to college students. Read as an abstract summary
  ([Psychological Bulletin 125, 627–668](https://doi.org/10.1037/0033-2909.125.6.627)). Whether on-screen stars count
  as a "tangible" reward is our judgement. The classic studies used certificates, so treat stars that are earned and
  collected as being in the risk zone.
- 🟠 **Praise the work, not the child.** Mueller and Dweck (1998) found that praising intelligence hurt persistence
  after failure, and praising effort did not ([doi:10.1037/0022-3514.75.1.33](https://doi.org/10.1037/0022-3514.75.1.33)).
  The children were about 10–12, and a close replication with 9–13-year-olds found no effect (Li and Bates 2019,
  [repository](https://westminsterresearch.westminster.ac.uk/item/v62v1/you-can-t-change-your-basic-ability-but-you-work-at-things-and-that-s-how-we-get-hard-things-done-testing-the-role-of-growth-mindset-on-response-to-setbacks-educational-attainment-and-cognitive)).
  Low cost either way, so the wording rule below stands; the evidence for it is weak.
- 🟠 **A pedagogical agent (mascot) helps a little, and more in school-age learners.** Schroeder, Adesope and Gilbert
  (2013, 43 studies, 3,088 participants): "a small but significant effect on learning", larger for K-12 than for
  post-secondary learners. ([abstract](https://rex.libraries.wsu.edu/esploro/outputs/journalArticle/How-Effective-are-Pedagogical-Agents-for/99900601056901842))
  The studies tested agents that instruct. A mascot that only decorates is a seductive detail.
- ✅ NN/g found that at 6, children "began to understand the feedback expressed by subtle facial and sound cues", and
  that animated instructions worked for 6-year-olds. ([NN/g, Kids' cognition, 2018](https://www.nngroup.com/articles/kids-cognition/))
  *Inference:* a mascot's face can carry right and wrong at this age, but it should be plain, not subtle.

**Do / don't rules that fit AC5**

| Do | Don't |
|---|---|
| Keep the task area (numbers, counters, number line) plain, on a calm panel | Put scenery, patterns or moving things inside or right beside the task area |
| Make all counters identical, simple and flat, one colour per group as now | Use counters with inner detail (faces, stripes, dots) a child may count instead |
| Let the mascot *deliver* the feedback: its pose plus the spoken phrase | Have a mascot that moves, talks or plays during the task with no teaching role |
| Show a right answer as a bounce on something that **stays** (progress row, mascot), with the next item on screen in the same frame | Disable input, or hold the next item, for any animation |
| At round end, celebrate in a way that tells the child how they did ("8 right first time") | Use stars as currency, a shop, unlockables, streaks or comparison (AC7) |
| Praise the work ("You found it!", "You tried again and got it") | Praise the child ("You're so clever") |
| Keep wrong-answer explanations static and clear, as the tech spec does (`…-tech.md:414`) | Animate the explanation unless a test shows the still picture is hard to follow |

### B. Visual design for ages 6–7

- ✅ **Contrast:** WCAG 2.2 SC 1.4.3 asks for 4.5:1 for normal text and 3:1 for large text (18 pt, or 14 pt bold).
  ([W3C](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html)) The current text colour (0.12 grey on
  cream, `UiBuilder.cs:13-14`) is far above that. A warmer palette must keep **text and numbers** dark on light.
  Colour goes in the surroundings, not behind digits.
- 🟠 **Larger type for early readers.** In Hughes and Wilkins (2000, 120 children), 5–7-year-olds read more slowly as
  type size fell and made more errors on smaller text; 8–11-year-olds were unaffected. Read through a search summary.
  ([doi:10.1111/1467-9817.00126](https://doi.org/10.1111/1467-9817.00126)) Today's sizes (56–80 reference px for
  labels, 200 for the round-end score: `ProfilePickView.cs:19,23`, `RoundEndPanel.cs:16`) are already large. Keep
  them; do not shrink them for decoration.
- ✅ **A typeface made for beginning readers exists under an open licence:** SIL's Andika is "designed and optimized
  especially for use as a literacy font", under the SIL Open Font License. ([SIL](https://software.sil.org/andika/))
  ❓ Its support for č, ć, đ, š and ž is likely, since the page claims near-complete Latin coverage, but it is not
  confirmed; check it when building the TextMeshPro font asset.
- ✅ **Target size:** at least 1 cm for mouse clicks at this age (knowledge store,
  `CHILDREN_AGED_6_TO_7_MOUSE_AND_KEYBOARD_…md`, Donker and Reitsma 2007). Rounded art must not shrink the
  clickable area. The art goes inside the existing hit rectangles (number pad cells 190×160, round-end buttons 480×170
  reference px: `NumberPad.cs:18-19`, `RoundEndPanel.cs:18-19`).
- 🟠 **What "warm and playful but not cluttered" means in practice** (our synthesis of the sources above, not a
  finding):
  - **Palette:** 1 warm background family (cream, peach, soft sky), 2 object colours that differ for colour-blind
    children too (the current blue and orange pair, `ComparisonScreen.cs:66-67`, is a good base), 1 "right" accent
    and 1 neutral dark for text. About 6–8 colours in all, fixed as hex values.
  - **Shapes:** rounded corners everywhere, thick even outlines, flat fills with at most one soft shadow. No textures
    or photographs.
  - **Layout:** one calm focal panel for the task, decoration only at the screen edges (a hill, a cloud), and nothing
    moving while the child thinks.
- 🟠 **Well-regarded styles, described only (do not copy).** DragonBox Numbers (ages 4–8) turns the quantities
  themselves into simple block characters whose height is the number. The character *is* the maths, not a decoration
  beside it. ([DragonBox](https://dragonbox.com/numbers)) This is the strongest pattern for Znalica's mascot: let it
  point at, hold or react to the quantity, never compete with it. The onebillion apps, which have a positive EEF trial
  (knowledge store), likewise keep plain, high-contrast task screens. That onebillion description is ❓, from memory.

### C. The art pipeline from Claude desktop into Unity

- ✅ **What Claude desktop can output.** Anthropic's help page says artifacts can be "documents, code snippets,
  single-page websites, images, diagrams and flowcharts", and that legacy artifacts can be copied or downloaded; the
  Design template exports .zip, PDF, PowerPoint or HTML. ([Claude Help: artifacts](https://support.claude.com/en/articles/9487310-what-are-artifacts-and-how-do-i-use-them))
  With *Create and edit files* turned on (available in Claude Desktop), Claude runs code in a sandbox and "can create
  data visualizations in image files like PNG", and with network access it can install packages from PyPI or npm.
  ([Claude Help: files](https://support.claude.com/en/articles/12111783-create-and-edit-files-with-claude))
  - 🟠 **No native text-to-image generation was found** on either Anthropic page. Its "images" are drawn as code
    (SVG, HTML/CSS). Plan for **SVG** as the art format.
  - ❓ Whether Claude's sandbox can render an SVG to a PNG at a set size (it needs a Python rasteriser package) was
    not tested.
- ✅ **SVG in Unity 6.3.** Unity 6.3 has a built-in Vector Graphics module (the project's manifest already lists
  `com.unity.modules.vectorgraphics`). It imports SVG as **Texture2D** or as a UI Toolkit Vector Image. **Sprite and
  uGUI import need the `com.unity.vectorgraphics` 3.0.0-preview package.**
  ([package manual](https://docs.unity3d.com/Packages/com.unity.vectorgraphics@3.0/manual/index.html)) The importer
  has `TextureWidth` / `TextureHeight` for texture import
  ([SVGImporter, 6000.3](https://docs.unity.com/en-us/engine/6000.3/script-reference/unity/vectorgraphics/editor/svgimporter)).
  An open issue reports SVGs imported as Vector Sprite though set to Texture2D (UUM-133203, per search summary, 🟠).
- **The three ways in, compared** (🟠: derived from the docs above and the project's code):

  | Way in | Fits uGUI `Image` (9-slice, atlas) | Cost |
  |---|---|---|
  | **SVG to PNG at a fixed size, imported as Sprite** (recommended) | Yes | A rasterising step per asset. It matches the existing activity pictures: 512×512 PNG sprites (`Art/ActivityPictures/*.png.meta`: `textureType: 8`, `spriteMode: 1`), wired as `Sprite` fields on `AppRoot` (`AppRoot.cs:19`, `ActivityMenuView.cs:15`). |
  | SVG imported as Texture2D by the built-in module, shown with `RawImage` | No 9-slice, no atlas | No extra step and no preview package; needs a second display path beside `Image` |
  | SVG through the 3.0.0-preview package (`UISVGImage`) | Partly; tessellated meshes | A preview package plus a known import issue |

  For the rasterising step, either ask Claude to render the PNG in its sandbox (❓ untested) or have the developer run
  a free command-line rasteriser such as Inkscape on the committed SVGs (❓ not checked this session). The second is
  repeatable and keeps the SVG as the one source.
- ✅ **9-slice for buttons and panels:** set the sprite's borders in the Sprite Editor and use `Image.Type.Sliced`; a
  Tight mesh type can break 9-slicing. ([Unity 6 manual, 9-slicing](https://docs.unity3d.com/Manual/sprite/9-slice/9-slicing.html))
  ✅ **Atlases:** a Sprite Atlas packs many sprites into one texture so they draw in one call
  ([Unity manual, Sprite Atlas](https://docs.unity3d.com/Manual/sprite/atlas/atlas-introduction.html)).
- 🟠 **Sizes.** The canvas scales from a 1920×1080 reference (`AppRoot.cs:15-16,167`). On a 4K screen, 1 reference px
  becomes 2 screen px, so export each picture at about **2× its largest on-screen reference size**, in multiples of 4.
  - Profile picture: 330 reference px (`ProfilePickView.cs:20`), so about 660 px.
  - Activity picture: 440 reference px (`ActivityMenuView.cs`), so about 880 px; today's 512 px PNGs are slightly
    soft on 4K.
  - Counter: 56 reference px (`ComparisonScreen.cs:36`), so about 128 px.
  - Mascot pose: about 1024 px.
  - Buttons: one small 9-slice sprite, about 128 px.
- 🟠 **Folder layout** (our proposal). Keep the source SVGs **outside `Assets/`** (for example `ArtSource/`), so the
  built-in SVG importer does not import them a second time:

  ```
  Assets/Znalica/Art/
    ActivityPictures/   (exists)
    ProfilePictures/    (named in the tech spec, …-tech.md:86): Fox, Bear, Owl, Cat, Rabbit, Frog
    Mascot/             one PNG per pose
    Counters/           counter, ten-frame cell, number-line marker
    Ui/                 button and panel 9-slices, background
    Celebration/        round-end stars or confetti pieces
  ```

  The six profile picture ids already exist in code (`Assets/Znalica/Domain/ProfilePictures.cs:12`), so the animals
  need no new names.
- 🟠 **Keeping one style across many assets** (practice, not research). The owner keeps one **style card** and starts
  each Claude chat by pasting it, together with one or two approved SVGs as references. The card holds:
  - the fixed hex palette and a fixed `viewBox` (for example `0 0 512 512`);
  - one outline width, one corner radius, and a rule of flat fills with at most one shadow shape;
  - no text inside the art (all words come from `Hr.json` through TextMeshPro);
  - a transparent background, with the subject centred and a margin of about 8%.

  New art is checked side by side with the approved set before it is committed.

### D. Life without slowing the child

- ✅ **uGUI cost:** a change to one element "dirties the whole Canvas". Unity advises putting changing elements on a
  small sub-canvas, and not using Animators for things that change only on events ("Animators will dirty their UI
  Elements on every frame"); custom code or a tween is the alternative.
  ([Unity UI optimization tips](https://unity.com/how-to/unity-ui-optimization-tips))
- **Library or coroutine.**
  - ✅ DOTween (free) may be used commercially, with no redistribution of modified copies
    ([licence](https://dotween.demigiant.com/license.php)).
  - 🟠 LitMotion is listed as MIT; PrimeTween's licence allows free and commercial use but forbids reselling it as a
    library (search summaries only).
  - 🟠 **Recommendation: no library.** Znalica needs about three motions: a scale pulse, a pop-in and a slow idle bob.
    One small coroutine helper in `Znalica.Presentation` covers them. It uses unscaled time, sets `localScale` only and
    never touches input. Coroutines already run there (`VoicePlayer.cs:63`), and the screens are MonoBehaviours.
- 🟠 **How it fits AC5** (our design):
  1. On a right answer, the next item is built **in the same frame**, as now.
  2. The pulse (about 0.2 s, scale 1 → 1.15 → 1) plays on an element that **persists across items**: a progress row
     of ten slots filling in, or the mascot's happy pose.
  3. No animation ever sets `interactable = false` or gates `Submit`. A test can assert that the next item is shown
     and accepts input with the pulse still running.
  4. Round end may show a short pop-in of stars or confetti (under about 1 s). The *Igraj opet* and *Stani* buttons are
     clickable from the first frame.
  5. The mascot's idle motion is slow and small, and it stops while an item waits for an answer (the decorated-room
     finding).
  6. A "less motion" setting is optional; not researched.

### E. Proposed scope, in small slices

Each slice can ship alone; the order puts the cheapest warmth first. "Code" is the developer's part.

| # | Slice | The owner draws | The code changes |
|---|---|---|---|
| 1 | **Palette, background, buttons, font** | Style card; background (edges only, calm centre); rounded button and panel 9-slices | The colours in `UiBuilder.cs:13-14` and the other screens' colour constants move to one palette; `NewButton` gets a sliced sprite; one TMP font asset (Andika, if č ć đ š ž check out) |
| 2 | **Profile pictures as animals** | 6 animal heads (Fox, Bear, Owl, Cat, Rabbit, Frog) | The HSV square at `UiBuilder.cs:123` becomes a sprite looked up by picture id; a `Sprite[]` on `AppRoot`, as `_activityPictures` already is |
| 3 | **Concrete counters, ten-frame, number line** | One plain counter in the two group colours; a rounded ten-frame cell; a number-line track and marker | `TenFrameDrawing.cs:31-41` and `NumberLineDrawing.cs` set sprites instead of flat colours. Layout and sizes stay; no inner detail on counters (finding A) |
| 4 | **Mascot in 4 poses** | Neutral, happy (right), encouraging (wrong), cheering (round end) | A mascot `Image` beside the task area on each activity screen; the pose swaps with the feedback that is already spoken; no new flow |
| 5 | **Right-answer pulse and round-end celebration** | Star or confetti pieces; a progress-row slot (empty and filled) | One coroutine helper; a 10-slot progress row; `RoundEndPanel` shows the stars with a pop-in and the score as now; tests that input is never held (AC5) |

Slices 1–3 change only how things look. Slices 4–5 add elements, so they need a short functional-spec note, because
they touch AC5 and AC7.

## Open questions

Decisions for the product owner:

1. **Mascot or not, and who.** It is recommended only if it delivers the feedback (finding A). An animal from the
   profile set would tie in; an original character is equally possible. Name and look are yours.
2. **What the round-end stars mean.** (a) A fixed celebration for finishing, plus "N right first time" (lowest risk
   by Deci 1999). (b) 1–3 stars by score: more motivating to some children, but a performance-contingent reward (🟠).
   Neither collects or buys anything.
3. **Who rasterises.** Claude's sandbox, untested, or a command-line tool the developer runs on committed SVGs. The
   second is recommended for repeatability.
4. **Where the SVG sources live:** `ArtSource/` outside `Assets/` (recommended), or not committed at all.
5. **Font:** Andika or another literacy font, once the Croatian letters are checked.
6. **Test with children:** the look itself (warm or busy?) is best checked at the next play-test, against the same
   complaint.

To confirm:

- ❓ Claude sandbox SVG-to-PNG; Andika's č ć đ š ž; the Inkscape route; the onebillion style description.
- 🟠 Several findings rest on abstracts or press releases, and some on other age groups (Petersen: 3–5;
  Mueller/Dweck: about 10–12; Schroeder: mixed ages).

## Sources

Project:
- `docs/specs/2026-11-16-grade-1-maths-first-version.md` (lines 40, 57, 59, 135) and `…-tech.md` (lines 86, 414)
- `Assets/Znalica/Presentation/` (`UiBuilder.cs`, `TenFrameDrawing.cs`, `NumberLineDrawing.cs`, `ProfilePickView.cs`,
  `RoundEndPanel.cs`, `ComparisonScreen.cs`, `NumberPad.cs`, `ActivityMenuView.cs`, `AppRoot.cs`, `VoicePlayer.cs`),
  `Assets/Znalica/Domain/ProfilePictures.cs`, `Assets/Znalica/Art/ActivityPictures/`, `Packages/manifest.json`

Knowledge store:
- `docs/articles/LEARNING_GAME_EVIDENCE_AND_GAME_FORMAT_PER_CONTENT_TYPE.md`
- `docs/articles/CHILDREN_AGED_6_TO_7_MOUSE_AND_KEYBOARD_UI_TARGET_SIZE_DRAG_VS_CLICK_AND_FEEDBACK.md`

Research:
- Kaminski and Sloutsky 2013: [OSU news](https://news.osu.edu/look-something-shiny-how-some-textbook-visuals-can-hurt-learning/) · [doi:10.1037/a0031040](https://doi.org/10.1037/a0031040)
- Fisher, Godwin and Seltman 2014: [CMU news](https://www.cmu.edu/news/stories/archives/2014/may/may27_decoratedclassrooms.html) · [doi:10.1177/0956797614533801](https://doi.org/10.1177/0956797614533801)
- Sundararajan and Adesope 2020: [WSU news](https://news.wsu.edu/press-release/2020/03/19/seductive-details-inhibit-learning) · [doi:10.1007/s10648-020-09522-4](https://doi.org/10.1007/s10648-020-09522-4)
- Petersen and McNeil 2013: [doi:10.1111/cdev.12028](https://doi.org/10.1111/cdev.12028)
- Carbonneau, Marley and Selig 2013: [abstract](https://asu.elsevierpure.com/en/publications/a-meta-analysis-of-the-efficacy-of-teaching-mathematics-with-conc/)
- Deci, Koestner and Ryan 1999: [doi:10.1037/0033-2909.125.6.627](https://doi.org/10.1037/0033-2909.125.6.627)
- Mueller and Dweck 1998: [doi:10.1037/0022-3514.75.1.33](https://doi.org/10.1037/0022-3514.75.1.33) · Li and Bates 2019: [repository](https://westminsterresearch.westminster.ac.uk/item/v62v1/you-can-t-change-your-basic-ability-but-you-work-at-things-and-that-s-how-we-get-hard-things-done-testing-the-role-of-growth-mindset-on-response-to-setbacks-educational-attainment-and-cognitive)
- Schroeder, Adesope and Gilbert 2013: [abstract](https://rex.libraries.wsu.edu/esploro/outputs/journalArticle/How-Effective-are-Pedagogical-Agents-for/99900601056901842)
- Hughes and Wilkins 2000: [doi:10.1111/1467-9817.00126](https://doi.org/10.1111/1467-9817.00126)
- NN/g: [Kids' cognition](https://www.nngroup.com/articles/kids-cognition/) · [Physical development](https://www.nngroup.com/articles/children-ux-physical-development/)

Guidelines and products:
- [WCAG 2.2 SC 1.4.3](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html)
- [SIL Andika](https://software.sil.org/andika/)
- [DragonBox Numbers](https://dragonbox.com/numbers)

Tools:
- Claude Help: [artifacts](https://support.claude.com/en/articles/9487310-what-are-artifacts-and-how-do-i-use-them) · [create and edit files](https://support.claude.com/en/articles/12111783-create-and-edit-files-with-claude)
- Unity: [Vector Graphics 3.0 manual](https://docs.unity3d.com/Packages/com.unity.vectorgraphics@3.0/manual/index.html) · [SVGImporter 6000.3](https://docs.unity.com/en-us/engine/6000.3/script-reference/unity/vectorgraphics/editor/svgimporter) · [SVGType](https://docs.unity.com/en-us/engine/current/script-reference/unity/vectorgraphics/editor/svgtype) · [9-slicing](https://docs.unity3d.com/Manual/sprite/9-slice/9-slicing.html) · [Sprite Atlas](https://docs.unity3d.com/Manual/sprite/atlas/atlas-introduction.html) · [UI optimization tips](https://unity.com/how-to/unity-ui-optimization-tips)
- [DOTween licence](https://dotween.demigiant.com/license.php) · [LitMotion](https://annulusgames.github.io/LitMotion) · [PrimeTween](https://github.com/KyryloKuzyk/PrimeTween)
