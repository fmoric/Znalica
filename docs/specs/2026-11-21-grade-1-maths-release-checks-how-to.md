# Znalica — First Version: How to Run the Player Checks

The steps for the *Player check* column of [the release checklist](2026-11-21-grade-1-maths-release-checks.md),
in the order to run them. They take about 30 minutes.

- **Player:** `Builds\Windows\Znalica.exe` in this repository's folder (on the product owner's PC,
  `%USERPROFILE%\Documents\GitHub\Znalica\Builds\Windows\Znalica.exe`), built on 2026-11-21 at
  commit `3ba89db`. The exe file itself is dated 16.11., because Unity does not rewrite this start-up program when it
  is unchanged. The game code beside it, in `Znalica_Data\Managed`, is from 2026-11-21.
- **Game data:** `%USERPROFILE%\AppData\LocalLow\Znalica\Znalica`, which held one profile on 2026-11-21.
- **Reporting:** reply with each step's number and "ok", or with what you saw instead, for example "1 ok, 2 ok, 3: the
  box took 25". Anything that looks wrong or ugly counts too, such as text running off the screen or a button too
  small to hit.

## Part A — offline test (AC12), first, about 2 minutes

1. Open **PowerShell as Administrator** (Start menu, type `PowerShell`, right-click, *Run as administrator*) and block
   the game from the internet:

   ```powershell
   New-NetFirewallRule -DisplayName "Znalica block test" -Direction Outbound -Action Block -Program "$env:USERPROFILE\Documents\GitHub\Znalica\Builds\Windows\Znalica.exe"
   ```

2. Start `Znalica.exe`, pick a profile and play one round of any activity. **It must play normally.** Close the game.
3. Remove the block, in the same Administrator PowerShell:

   ```powershell
   Remove-NetFirewallRule -DisplayName "Znalica block test"
   ```

## Part B — one long session, with Resource Monitor watching (AC12, then the rest)

4. Press Win+R, type `resmon`, press Enter, and open the **Network** tab.
5. Start `Znalica.exe` again and **note the time**, because the break suggestion needs 10 minutes from the profile pick.
   Check now and then during steps 6–12: **`Znalica.exe` must never appear** in *Processes with Network Activity* or
   *TCP Connections*.
6. **Mouse only (AC1, AC4):** pick the profile, then play one full round each of *Računanje do 20*, *Veće, manje,
   jednako* and *Brojevna crta*. Don't touch the keyboard. No screen should offer number answers to pick from; the
   three sign buttons in *Veće, manje, jednako* are fine.
7. **Keyboard (AC1, AC3):** in *Računanje do 20*, type answers with the number keys and Enter. Type `2` then `5`: the
   box **must not accept 25**.
8. **Replay (AC2):** after any feedback, press *Ponovi*. Something must play. The clips are placeholders, so the
   wording doesn't matter.
9. **Wrong and right answers (AC5, AC6):** answer one item wrong twice. The explanation must show, and only *Dalje*
   should go on. Remember that item: it **must come back later in the same round**. Answer right, and the next item
   must come at once.
10. **No pressure (AC7):** on every screen, there must be no clock, countdown, score table or comparison with other
    children.
11. **Close mid-round (AC11):** answer 3 items in a new round, then close the window with the **X**. Start the game
    again. In the parent area, *Napredak* must count those answers.
12. **Item comes back (AC6, second half):** in this new session, the item you missed in step 9 should come up early.
13. **Break (FS:45):** keep playing until **10 minutes** have passed since the profile pick in step 11, which started
    the clock again. From then on, **every round end must show and say "Igraš već dugo. Vrijeme je za kratku
    stanku."**, after the last answer's sound finishes, with both buttons still there.

## Part C — the parent area

14. **Gate (AC8):** open the parent area and give a wrong answer. You must not get in, and a wait must follow. Then get
    in with the right answer.
15. **Progress (AC9):** open *Napredak* for the profile. It must show figures for each of the three activities.
16. **School-like check (AC10):** press *Provjera*, then *Nova provjera*. Fill both pages and finish. The score must
    appear in the list as *n od 20*. *Napredak*'s figures must be **unchanged** by the check.
17. **Delete (AC13):** add a throwaway profile (for example "Test"), play one answer with it, then delete it in the
    parent area. It must be gone from the pick screen, and its folder must be gone from
    `%USERPROFILE%\AppData\LocalLow\Znalica\Znalica\Profiles`. There should be one folder fewer than before.

Close Resource Monitor when you are done.
