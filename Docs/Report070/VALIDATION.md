# 0.70.0-review1 — validation

**Source session:** Dan's debug session `2026-10-02_20-28-25-546_11ee21` (BUG-001 to BUG-009, all captured on 0.69.0-review1, build `dd3589a4`) plus his written request for a quit confirmation.

**Method:**
- Every comment was read and every screenshot inspected.
- Geometry was read with the read-only probes in `Tools/Report070/` (`Report070Probe/Line/List/Near/Area/Layers/Fine`).
- Changes were authored by `Tools/Report070/Report070Author.cs`; its log is [author-notes.txt](author-notes.txt). It also contains the discarded first attempts, which were reverted before the final runs.
- Play-mode checks (muted, isolated save) are in `Assets/Scripts/Report070Checks.cs`, `Report070QuitChecks.cs` and `Report070CampChecks.cs`. The 0.69 sets were re-run with `Report069Checks.cs` / `Report067DriveChecks.cs`. Raw results are in [checks/](checks/).
- Before/after views are in [views/](views/): the 0.69 scene on the left, 0.70 on the right, framed like the chase camera at the reported coordinate and heading.

## Dispositions

| Bug | Where | Disposition |
|---|---|---|
| Quit (written request) | Main menu QUIT GAME, pause menu Quit Game | **PASS.** See Part A. |
| BUG-001 | Street Loop Free Roam (895.8, 151.1, 154.3) | **Not changed — stopped per the TODO (Part B item 3).** The obstruction is a Free Roam activity feature; see Part B. |
| BUG-002 | Street Loop Free Roam (761.9, 109.3, 80.5) | **Not changed — same object as BUG-001.** |
| BUG-003 | Street Loop Race (226.3, 9.7, 544.5) s 4574 | **PASS.** The sawtooth cliff is now a smooth slope; trees re-grounded. See Part F. |
| BUG-004 | Street Loop Race (264.5, 8.8, 562.2) s 4614 | **PASS.** The verge meets the highway flush and falls away smoothly. See Part F. |
| BUG-005 | Mtn Reverse (1008.3, 163.1, 129.7) s 463 | **PASS.** 12 m rock outcrop sized from measured full-throttle flights; see [BARRIERS.md](BARRIERS.md). |
| BUG-006 | Mtn Reverse (994.3, 172.4, 122.1) s 449, airborne at 32 m/s | **PASS.** Reproduced, then contained (same outcrop). |
| BUG-007 | Mtn Forward (1013.8, 160.9, 81.0) s 1572 | **PASS.** Camp seated on the ground in both Mountain scenes; Free Roam camp unmoved; "Campsite" landmark added. See Part D. |
| BUG-008 | Mtn Forward, Climbing Ridge Cut 3.9 m (750.4, 87.1, −119.0) | **PASS.** Cause found: hidden faces just under the junction surface. Removed locally. See Part E. |
| BUG-009 | Mtn Forward (1015.0, 139.9, −54.6) s 1407 | **PASS.** Both gores at the Climbing Ridge Cut rejoin are now one smooth, flush surface each. See Part F. |

## Part A — Quit confirmation

- Both quit-to-desktop controls now open the existing confirmation dialog: main menu **QUIT GAME** and pause menu **Quit Game**. No other control quits to desktop.
- The dialog reads **QUIT WOODSTOCK RUSH?** with buttons **[CANCEL] [QUIT]**. Focus starts on CANCEL; B / Esc cancel.
- Change: `RaceMenus.Core.cs` (`ConfirmQuit`) and `RaceMenus.Shell.cs` (`Confirm` takes an optional confirm label, default "CONFIRM").
- END RACE / RETURN TO MENU and every other confirmation are unchanged. Alt+F4 / closing the window is not intercepted.
- **Checks** (`checks/quit-checks.txt`): 16/16 PASS.
  - Main menu: keyboard (Space opens, Esc cancels), controller (A opens, A on the default CANCEL returns, B cancels) and mouse (click, click CANCEL).
  - Pause menu: controller A / B, mouse, keyboard Esc. B and Esc return to the paused race, not to driving.
  - Focus is on CANCEL every time it opens.
  - END RACE dialog unchanged.
  - QUIT ends the game: in the editor, `RaceFlow.Quit` stops play mode; in a build it calls `Application.Quit`.

## Part B — Free Roam mountain path (BUG-001 / BUG-002): stopped, not changed

- **What the obstruction is:** `CR094 summit launch / Ground_CR103 smooth landing` (`Assets/Track/Discovery/cr103-catch-landing.asset`) and the terrain shaped under it (`StreetLoopGreybox-cr103-catch-landing-Ground_Mountain_0_4`).
  - This is the **catch landing of the Summit Homeward Flight** giant jump (CR-094 / CR-103), a Free Roam activity jump Dan asked for.
  - He approved the physical summit jump on 2026-09-21.
  - It slopes down toward the houses from 188 m at x ≈ 867 to 108 m at x ≈ 762. Its high end stands 20–47 m above the mountainside, which is the steep wall Dan rides into from both sides.
- **It is not race geometry:** it is already **inactive in both Mountain race scenes**, and active only in the Street Loop world (Free Roam and Street Loop races).
- **History (rule 5):** introduced in `752ca2f1` (2026-09-21, 0.18, "Correct CR-101-104 … summit flight"). Its mesh, the terrain under it and its active state are unchanged since then. This is not a 0.67–0.69 regression.
- **Disposition:** the TODO says that if this is a Free Roam activity feature Dan asked for, do not delete it, report and stop. Nothing was changed.
- **Options for Dan, if wanted:**
  1. cut a rideable notch/ramp through the landing's high end;
  2. lower the landing's top section (this changes the jump's catch area);
  3. remove the Summit Homeward Flight from Free Roam.
- Views: [BUG-001](views/BUG-001-unchanged.jpg), [overview](views/BUG-001-over-unchanged.jpg), [BUG-002](views/BUG-002-unchanged.jpg).

## Part C — Reverse summit crest barrier (BUG-005 / BUG-006)

See [BARRIERS.md](BARRIERS.md).

- **Reproduced:** 8 full-throttle runs left the crest at 31–35 m/s and crossed the outside of the bend 4–9.5 m up; all 8 landed 50–88 m off the track.
- **Built:** one 12 m rock outcrop filling the gore. Its cliff face is 7 m off the main edge (behind the sign, which is not moved) and 1 m off the Summit Traverse edge. The 0.69 berm here was absorbed.
- **After:** all 8 runs (4 per vehicle) are held on the summit plateau. Both clean lines and the Summit Traverse entrance complete.

## Part D — Campsite (BUG-007)

- **Cause:** the CR-094 camp (tent, two seated figures with log seats, campfire, 8 fire-ring stones; no colliders) is positioned for the Street Loop world's ground (≈ 164). The Mountain scenes have their own ground there:
  - **Forward:** ≈ 145, a 35° slope, so the camp floated 16–19 m up.
  - **Reverse:** ≈ 164–166, so the pieces were 0.3–1.7 m sunk.
- **Race scenes (both Mountain scenes):** each piece is seated on that scene's own terrain under its centre, 5 cm sunk. The tent and each figure-with-log move as one unit; the campfire and each stone move individually. Terrain is not changed.
  - Forward: lowered 15.9–19.1 m. There is no flatter ground within 35 m, so the tent sits across the slope (ground under its footprint varies 2.9 m).
  - Reverse: raised 0–1.8 m. It sits under the elevated South Face run-up ramp, not visible from the road.
- **Free Roam:** the camp is not moved. The Street Loop world copy is unchanged at (989.00, 163.81, 75.00). In the Mountain scenes Free Roam uses the same seated camp, since the old position floats there.
- **Landmark "Campsite":** an ordinary `ExplorationMap` destination (id `campsite`) at (982, 164.2, 76), facing the camp. It is added to all 8 course scenes so the shared world map stays identical. It gets a map label, 45 m discovery and free-roam fast travel; no new system.
- **Checks** (`checks/camp-checks.txt`, isolated map save, the player's map never touched): 5/5 PASS.
  - Destination present; Free Roam camp unmoved; not discovered before visiting; discovered within 45 m.
  - Fast travel arrives at (981.7, 164.7, 75.9), upright, beside the camp.
- Views: [Forward close](views/BUG-007-camp-before-after.jpg), [Dan's view](views/BUG-007-before-after.jpg), [Reverse](views/BUG-007-reverse-after.jpg).

## Part E — Climbing Ridge Cut entrance flips (BUG-008)

- **Reproduced** (`checks/enter-*`, harness case `enter`). 12 entries from the main road, moto and ATV, 8–32 m/s, lines −2.5 … +2.5 m:
  - At 8 m/s nothing happens.
  - At every speed of 14 m/s or more, the body hits `Ground_Report068 seam support` at branch s 9–11 (impulse 450–1,500), loses 2–5 m/s instantly and is thrown airborne.
  - Rolls of 38–86° followed (one 180° roll-over). The worst case was 32 m/s with a 43° roll and 7.7 rad/s.
- **Cause:** hidden faces just under the junction surface.
  1. The 0.68 collidable seam cover here is a fan of long slivers spanning the whole entry 5–11 cm under the pavement, with faces tilted up to 39°, some facing against the direction of travel. Only its left ends close a real 0.1–0.3 m slot at the left edge. *(from 0.68)*
  2. Beside the right edge, the 0.69 BUG-007 patch lies only 3–20 cm above crumpled `CR133 earth banks` / `MountainPolish` shoulder faces (normals down to 0.26). In 0.69 only terrain standing *above* the new patch was lowered. *(0.69 regression)*
  - Not the cause: the 0.69 pavement edits (none at this junction) and the 0.69 patch surface itself (never hit).
- **Fix (local, junction box x 740–776, z −126…−104):**
  - The 34 seam-cover slivers were subdivided 8×8. Points under the pavement went to 0.6 m below it; points within 0.4 m of any pavement edge were left alone, so the slot stays closed.
  - Earth-bank / older-shoulder vertices lying under the pavement or the 0.69 patch went to 0.6 m below it **only where every triangle touching them lies fully under that cover**: 127 vertices.
    - A first, wider pass (239 vertices) left a visible shelf and notch beside the right edge, because some of those triangles reach out beyond the cover. Those 112 vertices were put back to their committed heights.
    - A further tried step (subdividing the partly covered earth-bank triangles) also exposed slopes, and was undone exactly.
    - The final view matches 0.69.
  - The pavement and the 0.69 patch were not changed. The Climbing Ridge Cut jump system is untouched.
- **After** (`checks/partE-enter-after.txt`, final geometry):

  | Case | Before | After |
  |---|---|---|
  | 8–26 m/s, all lines (−2.5 … +2.5 m), moto and ATV | rolls 38–86° (one 180°) | all clean: min up ≥ 0.95, roll ≤ 8°, no hidden-face contact |
  | 32 m/s (72 mph), moto centre / moto +3 m / ATV centre | 43° roll at s 10 (moto centre) | still roll: 105° at s 36, 180° at s 9, 71° at s 11 |

  - At 72 mph the remaining crumpled earth-bank faces under the 0.69 patch (beside the right edge) and the cut's side bank still catch the vehicle. Lowering those faces further exposes visible notches, so they were left.
  - Production AI with the shortcut planned: enters and rejoins, moto and ATV (`checks/partE-ai-entry.txt`, best 293.8–294.1 / 298.6, same as 0.69).
  - Climbing Ridge Cut clean line s 240–296 and the s 268 overshoot are unchanged from 0.69.
- Views: [BUG-008](views/BUG-008-before-after.jpg) (no visible change intended), [wide](views/BUG-008-wide-before-after.jpg).

## Part F — Smoothing

### BUG-003 — Street Loop, beside the highway at s 4574

- **What it was:** not a trench but a 3–6 m near-vertical sawtooth step in the terrain, 10–50 m off the road among the trees. It runs from (225, 462) to (290, 530), then fades east along z ≈ 530. The road side of it is low (≈ 8 m) and the far side high (up to 22 m).
- **Fix:** Laplacian smoothing of the terrain vertices within 12 m of that line (`SmoothBand`).
  - The rim is pinned to the original ground; the road is not touched.
  - Every tree standing in the band was re-grounded by the ground change under its trunk: its trunk collider and its pieces of the batched tree mesh moved together, by −2.5 … +1.6 m.
    - Street Loop Forward: 11 trees.
    - Street Loop Reverse: 7 trees.
    - Lake Woods: 11 trees.
    - Forest Reverse: 9 trees.
    - Dan's Backyard Forward / Reverse: 11 / 11 trees.
    - Mountain Loop Forward / Reverse: 7 / 7 trees.
- **Result:** an even slope of about 13° (e.g. z = 500: 11.0 → 17.8 m over ≈ 27 m). No 1 m cell in the area rises more than 0.49 m, against 5.8 m before.
- **Scope:** every course scene carries this part of the shared world, so it was applied in all 8 (Street Loop Forward / Reverse, Lake Woods, Forest Reverse, Dan's Backyard Forward / Reverse, Mountain Loop Forward / Reverse). Where two scenes shared a mesh, each got its own copy. It is far from every Mountain route.

### BUG-004 — Street Loop highway right verge at s 4614

- **What it was:** a 0.25–0.45 m lump of terrain right where the highway's driving strip ends (z ≈ 564), falling away behind it. There is no asphalt under it.
- **Fix:** the same smoothing in a 6 m band from x 205 to 282.
  - Terrain nodes on or within 1.2 m of the strip are pinned 3 cm under it.
  - Buildings, the highway, its markings and the strip are unchanged.
- **Result:** the verge meets the strip within ±0.05 m along the whole stretch (one cell is 0.07 m), then falls away smoothly. All 8 scenes.

### BUG-009 — Mountain Forward, Climbing Ridge Cut rejoin at s 1407

- **What it was:** both gores at the rejoin (right: between the Climbing Ridge Cut end and the main road; left: between it and the main road from the south-west) were a patchwork:
  - CR133 earth banks 0.1–0.4 m above the pavement edge;
  - the 0.69 flush shoulders at a different height;
  - older MountainPolish shoulders;
  - a step at the left gore's far end.
- **Fix:** each gore got the 0.69 junction surface: one smooth collidable surface, pinned 3 cm under the pavement edges and to the ground at its rim.
  - Right gore: r 7.5 m around (1011, −49.5).
  - Left gore: r 9.5 m around (1006.5, −63).
  - Terrain left under them goes 0.4 m below (not 0.1 m), so there are no hidden faces (the Part E lesson).
  - The 0.69 berm (a barrier) is excluded and keeps working; the pavement is unchanged.
- Views: [BUG-009](views/BUG-009-before-after.jpg), [left gore from above](views/BUG-009-left-gore-after.jpg).

## 5A.6 neighbour checks

The 0.69 check and drive sets (the same as 0.68's) were re-run unchanged with `Tools/Report069/Run-Checks.ps1` and `Run-Drives.ps1`, muted. Raw results: `checks/neighbour-*.txt`.

### Same as 0.69

- **Forward checks, 20 of 20 the same (18 byte-identical; the two resets differ by 0.01 s / 0.1 m):**
  - the 0.69 Climbing Ridge Cut berm overshoots and clean lines (main s 1360–1460, beside BUG-009, and the branch s 240–296);
  - Homeward landing, runout and berm;
  - edge off-and-on;
  - resets.
- **Forward drives, 9 of 10 identical:**
  - Homeward Summit Flight AI and throttle;
  - start area;
  - main s 740–860;
  - Summit Traverse, AI and throttle.
- **Reverse checks:**
  - Summit Traverse entrance and off-and-ons;
  - main s 254/273/1665 edges and resets;
  - South Face berm line and overshoots;
  - Downhill Ridge Cut overshoots.

  All are the same, or within ±0.1 m / ±0.02 min up. Those small differences come from the changed collider set.
- **Reverse drives, all same as 0.69 except one:**
  - South Face jump (AI stop at 1183, throttle at 1144, both pre-existing);
  - lower main route s 1460–1830;
  - s 2900–3010;
  - Downhill Ridge Cut;
  - Summit Traverse;
  - start area.

### Differences, explained

- **Forward, production AI into the Climbing Ridge Cut:** in this run it did not enter.
  - **Cause (harness):** the drive places the AI 40 m before the branch entry. For this branch (entry at main s ≈ 17) that wraps back across the start line to s ≈ 2735, where `RoadDriver` drops the planned branch as "past its exit". Entry then depends on the AI's random once-per-lap shortcut roll. 0.69's roll said yes, this one said no.
  - **Checked directly:** with the plan held, starting just after the start line, the AI enters and rejoins (moto and ATV, as 0.69).
- **Reverse, crest clean line s 400–520 and ATV throttle drive:** these are the new outcrop at work.
  - The full-throttle line still launches off the crest (the crest is unchanged).
  - In the 0.69-harness session the moto landed rolled (min up −0.47) without touching the outcrop; in three repeats in the 0.70 harness it lands upright with one brush (min up 0.71). These marginal lines vary between sessions. 0.68 recorded the same −0.46 roll before the 0.69 berm.
  - The ATV throttle drive, which in 0.69 flew off and rolled 91°, is now held at s 487 by the outcrop (no reset).
- **Reverse, Downhill Ridge Cut full-throttle clean line (moto):**
  - It completes but rolls after brushing the 0.68 berm twice (0.69: one brush, upright). The result repeats 3/3 within a session.
  - Nothing near it changed: the Reverse scene diff is only the outcrop, the removed 0.69 berm, the camp pieces and the landmark. This is the known marginal line (0.68/0.69: "brushes the narrow Downhill Ridge Cut turn") diverging with the changed collider set.
