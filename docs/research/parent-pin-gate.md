# Research brief: a PIN for Znalica's parent gate

Researched 2026-11-18. Verdicts: ✅ verified (cited) · 🟠 needs care · ❓ to-confirm.

## Overview

Znalica's parent area (add, rename and delete profiles; voice volume; progress; the school-like check) is reached
through a **parent gate**: a multiplication of two factors 3–9 written in words, not spoken, with 3 tries and then a
30-second wait (`Assets/Znalica/Domain/ParentGate.cs`; tech spec *Parent gate*, and Step 4 finding (2)). The product
owner wants it "like a PIN". This brief answers: what a parental gate is for and what guidance exists; how a PIN
should work here; what happens when it is forgotten; how to store it; and what the change would touch.

**Scope, as read from the brief (no user to ask):** a decision brief of about 2–3 pages for the product owner and the
architect, for the Windows desktop build only (Unity 6000.3, offline, no accounts, data under the user's
`AppData\LocalLow`). The threat is a curious child of 6–7, or a sibling, at the family PC, not an attacker. App store
rules are used as published guidance, not as rules Znalica must meet (it ships on no store). No real PIN appears
anywhere in this brief.

**The decision in one line:** a 4-digit PIN set by the parent at first start is a reasonable change, *provided* the
product owner also decides how a forgotten PIN is recovered, because the recovery path, not the PIN, sets how strong
the gate is (C1). Two spec texts change: the business rule *"No password to forget"* (FS:108–109) and *"The parent
gate has no setting in this version"* (FS:78, Q1).

## Findings

### A. Purpose of a parental gate, and the guidance

- ✅ **Apple defines a parental gate as an adult-level task** that "must be completed in order to continue", aimed at
  keeping "kids from engaging in certain activities without their parent's knowledge" (purchases, links out); for
  pre-literate children it suggests a voice prompt telling them to fetch a parent. Its example images are a maths task
  and a question; it names no PIN and sets no strength ([Apple, Kids apps](https://developer.apple.com/app-store/kids-apps/)).
  Guideline 1.3 requires links out, purchases and "other distractions" to sit "behind a parental gate"
  ([Guideline 1.3](https://developer.apple.com/app-store/review/guidelines/#kids-category)).
- ✅ **A parental gate is not parental consent.** Guideline 5.1.4(b): the gate requirement "is generally not the same
  as securing parental consent to collect personal data under these privacy statutes"
  ([Guideline 5.1.4](https://developer.apple.com/app-store/review/guidelines/#kids)). Znalica's gate protects settings
  and profile deletion, which fits the purpose; it is not a consent mechanism and need not become one.
- ✅ **Google Play's Families policy does not use the term "parental gate".** Its closest text describes a way "to
  verify that the user is not a child", with examples "an adult PIN, password, birthdate, email verification, photo
  ID, credit card, or SSN", tied to social features; a *neutral age screen* is a different thing (an age question that
  must not nudge a child to lie, used to route ads and SDKs)
  ([Google Play Families policy](https://support.google.com/googleplay/android-developer/answer/9893335?hl=en)).
  So a PIN is a recognised "adult" check; neither store says one is better than a maths task.
- ❓ **No study was found that measures how often children of 6–7 pass a maths gate or copy a parent's PIN.** Two web
  searches (gate effectiveness; children observing passcodes) returned only adjacent work. The comparison below is
  reasoning, not evidence.
- 🟠 **Maths gate versus PIN, for this child (reasoning):**
  - The current gate's strength rests on the child not reading the words and not knowing multiplication, which the
    tech spec calls grade 2 content. It **weakens as the child grows**, and an older sibling passes it by design (tech
    spec *Parent gate*: "does not stop an older sibling who can read and multiply, and is not meant to"). A child who
    copies the question to a parent gets in with the parent's help, which is the gate working.
  - A PIN does not weaken with the child's maths, and stops a reading sibling. Its weakness is **observation**: the
    child sits beside the parent at the same PC. ✅ In a survey of 174 shoulder-surfing stories, 15 (9 %) involved
    login data, mostly a PIN or unlock pattern; observers were family in only 1.7 % of stories, but the authors cite
    earlier work that "insiders (e.g., friends and family) pose a higher threat" ([Eiband et al., CHI 2017](https://www.medien.ifi.lmu.de/pubdb/publications/pub/eiband2017chi/eiband2017chi.pdf)).
    Showing dots hides the screen, not the hand on the keys.
  - Net: a PIN is stronger against a reading sibling and a growing child, weaker against a watching child, and adds
    something to forget, which the current rule deliberately avoided.

### B. PIN design

- ✅ **4 digits versus 6.** Against an attacker limited to 10, 30 or 100 guesses, "using 6-digit PINs instead of
  4-digit PINs provides little to no increase in security" ([Markert et al., IEEE S&P 2020, arXiv](https://arxiv.org/abs/2003.04868v3)).
  Znalica's gate is such a throttled setting, so **4 digits** is supported, and easier for a parent to remember.
- ✅ **Refuse trivial PINs, with a sizeable list.** Small blocklists (iOS: 274 four-digit PINs) "offer little or no
  benefit"; "a blocklist at about 10% of the PIN space may provide the best balance" (Markert et al., above). About a
  quarter of bank customers chose a date as PIN, nearly a third of those their own birthday
  ([Bonneau, Preibusch, Anderson, FC 2012](https://www.cl.cam.ac.uk/%7Erja14/Papers/BPA12-FC-banking_pin_security.pdf),
  figures from the abstract and its search summary; the body was not read 🟠). NIST requires checking a new secret
  against "commonly used, expected, or compromised" values ([NIST SP 800-63B-4, 3.1.1.2](https://pages.nist.gov/800-63-4/sp800-63b.html)).
  🟠 Proposed rule (design, not sourced): refuse four equal digits, runs up or down (`0123`, `9876`, wrapping
  included), two repeated pairs (`abab`, `aabb`), and years 1950–2030 (81 values). That is roughly 290 of 10,000 (about 3 %),
  far below 10 %; a larger list is the product owner's call (open question 4).
- 🟠 **Who sets it, and when.** The first-start flow already runs *gate, then first profile* (Step 4 finding (5)). With
  a PIN there is nothing to check on first start, so **whoever starts Znalica first sets the PIN**. Keeping the
  arithmetic question in front of *set PIN* on first start stops a child doing it (open question 2).
- 🟠 **Typed twice, shown as dots.** Standard practice; no source was read for it. A mismatch empties both boxes and
  says so; a refused PIN says why in plain words ("too easy to guess"), never shows the digits.
- 🟠 **Tries and lock-out.** NIST's ceiling is 100 failed attempts before disabling ([SP 800-63B-4, 3.2.2](https://pages.nist.gov/800-63-4/sp800-63b.html)),
  with lower limits allowed. Keeping **3 tries, then 30 s** gives about 6 guesses a minute: on average 5,000 guesses,
  about **14 hours** of steady guessing for a uniformly chosen 4-digit PIN (derived). 🟠 **The wait does not survive a
  restart:** `AppRoot` keeps one gate per run, and its counters live in memory (`ParentGate._wrongInARow`,
  `_waitEnds`), so closing and reopening Znalica skips the wait. A 6-year-old is unlikely to find that; a sibling may.
  Persisting the wait end, and growing it (30 s, 1 min, 5 min…), are options with a cost: a locked-out parent waits
  too.
- 🟠 **Entering it with the number pad and the keyboard.** `NumberPad` (pad and digit keys) is already shared by the
  arithmetic screen and the gate. But `ParentGate` types into an `AnswerEntry`, which holds an `int` up to a maximum
  (81): **a PIN must be a fixed-length digit string**, or `0123` becomes `123`. The PIN entry needs its own 4-slot
  buffer; the pad's *Confirm* can fire automatically at the fourth digit or stay a press (product choice).

### C. A forgotten PIN on an offline app with no accounts

- 🟠 **C1. The weakest way in sets the gate's strength.** Whatever resets the PIN is itself a gate: if it is the old
  maths question, the PIN is no stronger than today's gate against a child who can multiply (reasoning).
- 🟠 **Options (reasoning; none sourced):**

  | Option | Parent effort | Child can use it? | Notes |
  |---|---|---|---|
  | **Delete the PIN file**, with a written how-to (where the data folder is, which file) | Medium: file explorer | A 6–7-year-old, no; an older sibling who reads the how-to, yes | Keeps profiles. Exists anyway: anyone with the Windows account can edit the files today. Cheapest. |
  | **Reset behind the arithmetic gate** (*"Zaboravljen PIN?"*) | Low | A child who can read and multiply, yes | Caps the PIN at today's strength (C1). |
  | **Recovery code** shown once at setup, to write down | Low, if kept | Only if the paper is found | Another secret to keep; per install, never one code for all. |
  | **Recovery question** (pet's name…) | Low | Family members know the answers | Stores personal facts, against the spec's no-surname, no-birth-date line (FS:72). Not recommended. |
  | **Reset that clears the PIN with no check** | None | Yes | Defeats the gate. Not an option. |

  Recommended for the product owner to weigh: **delete-the-file how-to**, optionally with the arithmetic question in
  front of *set new PIN*. When the PIN is missing but profiles exist, Znalica asks for a new PIN; profiles and progress
  stay.
- ❓ Whether uninstalling Znalica removes `AppData\LocalLow\Znalica\Znalica` (and so the PIN) was not checked.

### D. Storing the PIN

- ✅ **Never in plain text.** NIST: secrets "SHALL be salted and hashed using a suitable password hashing scheme";
  salt "at least 32 bits"; cost "as high as practical" ([SP 800-63B-4, 3.1.1.2](https://pages.nist.gov/800-63-4/sp800-63b.html)).
  OWASP: PBKDF2-HMAC-SHA256 at 600,000 iterations, a unique random salt per secret
  ([OWASP Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)).
- 🟠 **Said plainly: a hash does not protect a 4-digit PIN from someone who reads the file.** There are 10,000 PINs;
  anyone who can read the salt and hash can try all of them offline, at 600,000 iterations each, in minutes to hours
  on one PC (derived, not measured). Hashing stops a child, or anyone glancing at `Settings.json`, from **reading the
  PIN by eye**. It is proportionate to the threat (a curious child), and nothing stronger is available without a
  secret the child's Windows account cannot read. The same account can also simply delete the file (C).
- ✅ **The .NET API.** `Rfc2898DeriveBytes(string password, byte[] salt, int iterations, HashAlgorithmName)` exists in
  .NET Standard 2.1 (its *Applies to* lists `netstandard-2.1`); the salt "must be 8 bytes or larger"; the overloads
  without a hash name default to SHA-1 and 1,000 iterations and are obsolete as insecure
  ([Rfc2898DeriveBytes constructors, netstandard-2.1](https://learn.microsoft.com/dotnet/api/system.security.cryptography.rfc2898derivebytes.-ctor?view=netstandard-2.1);
  [SYSLIB0041](https://learn.microsoft.com/dotnet/fundamentals/syslib-diagnostics/syslib0041)). The static
  `Rfc2898DeriveBytes.Pbkdf2` is the modern form, and the constructors are obsolete from .NET 10
  ([Pbkdf2](https://learn.microsoft.com/dotnet/api/system.security.cryptography.rfc2898derivebytes.pbkdf2);
  [SYSLIB0060](https://learn.microsoft.com/dotnet/fundamentals/syslib-diagnostics/syslib0060)); its availability on
  .NET Standard 2.1 was not checked, so plan on the constructor. Salt from `RandomNumberGenerator.Create()` +
  `GetBytes(byte[])` ([RandomNumberGenerator](https://learn.microsoft.com/dotnet/api/system.security.cryptography.randomnumbergenerator)).
  Pass `HashAlgorithmName.SHA256` explicitly ([CA5379](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5379)).
- ✅/🟠 **Unity's profile.** Unity's default API compatibility level is .NET Standard 2.1; `NET_Standard` "targets .NET
  Standard 2.1" ([Unity, .NET profile support](https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/overview-of-dot-net-in-unity/dotnet-profile-support);
  [ApiCompatibilityLevel](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/apicompatibilitylevel)).
  `ProjectSettings.asset` has `apiCompatibilityLevel: 6`; that 6 means `NET_Standard` is assumed, not read from Unity's
  docs (they show no numbers). **That Mono implements the SHA-256 PBKDF2 overload correctly, and how fast, is
  unverified:** a test against a published PBKDF2-HMAC-SHA256 vector settles the first; a timing in the editor and the
  player sets the iteration count (aim: a check under about half a second). Unity "makes no performance … guarantees
  for the .NET System libraries" (profile page above).
- 🟠 **SHA-256 with a salt** would be just as adequate for this threat model (the offline brute force above is quick
  either way); PBKDF2 costs a line more and follows the named guidance. Constant-time comparison is not needed here
  (no remote timing), but harmless.
- 🟠 **Code finding: saving the volume would wipe a PIN kept in `Settings.json`.** `SettingsStore.Save` writes a new
  `Settings` with `VoiceVolume` only (`Assets/Znalica/Storage/SettingsStore.cs`, `Save`), and `AppRoot` saves
  `new Settings { VoiceVolume = volume }`. And an unreadable `Settings.json` gives defaults, which would silently mean
  "no PIN". Either keep the PIN in **its own file** (for example `ParentPin.json`: salt, hash, iterations, algorithm
  name, base64) or change both. A separate file also makes the C how-to simple: "delete this one file".

### E. Proposed change

- **Functional spec:** replace the parent-gate business rule (FS:108–109) and FS:78's "no setting"; add the PIN
  screens (set, confirm, change) to *Screens*; amend AC8; state the recovery path; add a PIN to the first-start rule
  (FS:112).
- **Domain:** `ParentGate` keeps the tries-and-wait logic but checks against an injected verifier instead of
  `GateChallenge` (the maths question stays as a class only if kept for first start or recovery). New: PIN rules
  (length 4, digits only, blocklist), a fixed-length entry buffer, a hasher/verifier (salt, PBKDF2, compare).
- **Storage:** a PIN store (own file, atomic write through `AtomicFileWriter`, unreadable file reported through
  `IProblemLog` and treated as defined by the product owner).
- **Presentation:** *set PIN* (twice) in the first-start flow before the first profile; *change PIN* in the parent area
  (asks the current PIN first); the gate screen shows four dots instead of the question; new `PhraseKey`s (screen
  only).
- **Tests (EditMode):** rules accept a valid PIN and refuse each blocklist family; `0123` round-trips (leading zero);
  hash of the same PIN and salt is stable, different salt differs, wrong PIN fails; a published PBKDF2-SHA256 test
  vector; 3 wrong then locked 30 s, right PIN refused while locked; confirm mismatch refused; saving the volume keeps
  the PIN; missing PIN with profiles leads to *set PIN*; first start leads to *set PIN* then the first profile;
  `Settings.json`/PIN file holds no digits of the PIN (search the file text for the PIN string).
- **Manual:** digit keys and pad both reach the PIN boxes; no digit is ever shown; the how-to path is correct on a
  real install.

## Open questions

For the product owner:

1. **Recovery** (C): delete-the-file how-to, reset behind the maths question, a recovery code, or another? This decides
   the gate's real strength.
2. **First start:** keep the maths question in front of *set PIN*, so a child starting Znalica first cannot set it?
3. **Keep the maths gate at all** (first start, recovery), or remove it and its eight phrase keys?
4. **Blocklist size:** the ~290-value rule above, or larger (toward Markert's ~10 %)?
5. **Lock-out:** keep 3 tries / 30 s per run, persist the wait across restarts, or grow it?
6. **Length:** 4 digits (recommended by the evidence above) or 6?
7. **Change PIN:** asks the old PIN first (assumed), and does a successful change need anything else?

To confirm (technical): Mono's PBKDF2-SHA256 against a test vector, and its timing in the player; that
`apiCompatibilityLevel: 6` is `NET_Standard`; what an uninstall leaves in `AppData\LocalLow`.

## Sources

- Apple, [App Review Guidelines](https://developer.apple.com/app-store/review/guidelines/) (1.3, 5.1.4)
- Apple, [Kids apps — parental gates](https://developer.apple.com/app-store/kids-apps/)
- Google, [Play Families policy](https://support.google.com/googleplay/android-developer/answer/9893335?hl=en)
- Markert, Bailey, Golla, Dürmuth, Aviv, [This PIN Can Be Easily Guessed (IEEE S&P 2020), arXiv 2003.04868](https://arxiv.org/abs/2003.04868v3)
- Bonneau, Preibusch, Anderson, [A Birthday Present Every Eleven Wallets? (FC 2012)](https://www.cl.cam.ac.uk/%7Erja14/Papers/BPA12-FC-banking_pin_security.pdf)
- Eiband, Khamis, von Zezschwitz, Hussmann, Alt, [Understanding Shoulder Surfing in the Wild (CHI 2017)](https://www.medien.ifi.lmu.de/pubdb/publications/pub/eiband2017chi/eiband2017chi.pdf)
- NIST, [SP 800-63B-4](https://pages.nist.gov/800-63-4/sp800-63b.html) (3.1.1.2, 3.2.2)
- OWASP, [Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)
- Microsoft Learn: [Rfc2898DeriveBytes constructors (netstandard-2.1)](https://learn.microsoft.com/dotnet/api/system.security.cryptography.rfc2898derivebytes.-ctor?view=netstandard-2.1),
  [Rfc2898DeriveBytes.Pbkdf2](https://learn.microsoft.com/dotnet/api/system.security.cryptography.rfc2898derivebytes.pbkdf2),
  [SYSLIB0041](https://learn.microsoft.com/dotnet/fundamentals/syslib-diagnostics/syslib0041),
  [SYSLIB0060](https://learn.microsoft.com/dotnet/fundamentals/syslib-diagnostics/syslib0060),
  [CA5379](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca5379),
  [RandomNumberGenerator](https://learn.microsoft.com/dotnet/api/system.security.cryptography.randomnumbergenerator)
- Unity 6000.3: [.NET profile support](https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/overview-of-dot-net-in-unity/dotnet-profile-support),
  [ApiCompatibilityLevel](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/apicompatibilitylevel)
- Project: `docs/specs/2026-11-16-grade-1-maths-first-version.md` (FS:72, 78, 108–112),
  `docs/specs/2026-11-16-grade-1-maths-first-version-tech.md` (*Parent gate*, Step 4 findings (2), (5)),
  `Assets/Znalica/Domain/ParentGate.cs`, `Assets/Znalica/Storage/SettingsStore.cs`, `Assets/Znalica/Presentation/AppRoot.cs`
- Knowledge store: searched (`parental gate PIN children app`; `PBKDF2 Rfc2898DeriveBytes password hash salt`), no
  article on either topic.
