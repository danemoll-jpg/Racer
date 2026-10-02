# 0.68.0-review1 — validation

**Source session:** Dan's debug session `2026-10-02_13-02-35-687_63de1f` (BUG-003 to BUG-009). BUG-001 and BUG-002 were captured on 0.66 and are excluded, as the TODO directs.

**Method:**
- Every comment was read and every screenshot inspected.
- Geometry was read with probes in `Tools/Report068/`.
- Changes were authored by `Tools/Report068/Report068Author.cs` and `Report068Arrows.cs`. The authoring log is `author-notes.txt` (chronological: later runs supersede earlier lines).
- Checks use the muted play-mode harnesses `Assets/Scripts/Report068ResetChecks.cs` and the 0.67 `Report067DriveChecks.cs`. Raw results are in `checks/`.

## Part A — reset rule rewrite and fall-through safety

### What was wrong in 0.67 (`VehicleRespawn.cs`, confirmed by reading and by reproduction)

- The player reset only tried the last *earned* anchor and older samples behind it.
- Anchors stop advancing during a jump, and samples on a run-up are rejected by the jump exclusions (`Jump run-up / no recovery checkpoint` covers the whole Homeward run-up). After a missed jump, therefore, every candidate is either far back or rejected.
- With every candidate rejected, the code showed "Waiting for clear course support" and retried the same list every 0.5 s with no fallback. While waiting, a vehicle could keep falling.

### Reproduction on 0.67 code (`checks/reset-before*.txt`)

- **Off-track landing after the Homeward run-up (Dan's BUG-007 case):** the reset jumped to the **start marker**, 93–109 m from the vehicle. No anchor survives the run-up exclusion.
- **Reverse South Face undershoot (missed jump):** reset to the **start line**, a **1,120 m** setback.
- **BUG-008 overshoots at 30–46 m/s:** reset 15–108 m **back**, or forward to the start marker.
- **Off the side of an elevated road with no fresh anchor:** reset to the start, 1,138 m and 581 m away.
- **Free Roam start / leaving a race (BUG-009): reproduced.** The bike dropped 20–30 m through the world at the start marker.
- **Exact "waiting" loop:** the harness could not reproduce the exact loop (its runs never had an anchor older than the run-up). The cause is in the source: every candidate rejected, no fallback.

### New rule (Dan, 2026-10-02) — `VehicleRespawn.RecoverNearest`

- **Player target:** the nearest route point to the vehicle, on the route being raced (the branch first if on one, then main). It is searched around tracked progress, using horizontal distance plus clamped height, so a multi-level mountain cannot snap to another part of the lap. The vehicle faces the route's forward direction.
- **Unusable spot:**
  - The search steps outward 2 m at a time in both directions, forward first on a tie.
  - Stepping backward never crosses a ramp or flight exclusion that it reached from outside, so a missed jump restores at or after the landing.
  - The search never crosses START/FINISH in either direction, so a reset cannot award or undo a lap.
  - Checkpoint and penalty logic is unchanged.
- **Guaranteed result:** the search is a bounded, synchronous scan of the route (≤ 1,500 stations). Last resort: the nearest usable point anywhere on the route, then the start position. The waiting message can no longer persist.
- **Unchanged:** the 0.8 s delay, the support/clearance/occupancy validation, wipeout detection, physics and checkpoints.
- **AI:** its earned-sample selection is unchanged. After 3 s of rejected candidates, it takes the same guaranteed nearest-point placement.
- **Free Roam:** the recent-road behaviour is unchanged. If that fails, the nearest road or trail is used. It cannot hang.
- **Failsafe (all scenes, player and AI):** below `fallResetHeight` (-15), the vehicle is placed at once (every 0.25 s while below) from its last position above the world.
- **Leaving a race:** `RaceFlow.QuitRace` seats the stopped vehicle on the nearest validated, clear course point before locking it.
- **Starting Free Roam / solo race:** `RestartAtStart` seats the start on its supporting surface.
- **Scene data:** the Mountain Forward start marker was raised from 1.49 m below the pavement to 0.7 m above it.

### Results on 0.68 code (`checks/reset-after-*.txt`)

All cases restored within the normal delay (0.80–0.83 s). None showed the waiting message, all faced the race direction (dot 1.00), and all stayed upright and held their position.

| Case | Before (0.67) | After (0.68) |
|---|---|---|
| Overshoot BUG-008 jump, 40 m/s takeoff, ended 5.8 m off the road | 107.7 m back | **2.4 m** (nearest s 2565) |
| Overshoot, 44 m/s takeoff | jumped to the start marker | nearest s 2557, 12 m |
| Dan's off-track landing after the run-up (690, 87, 75), fell to y 67.7 | start marker, 93 m | nearest s 2520.9 (on the bend), 28 m |
| Missed Reverse jump (South Face undershoot) | start line, 1,120 m back | nearest usable s 1183, **44 m forward**: the steep landing face is not a valid footprint |
| Off the side of an elevated road, Forward s 1500 (34 m below / beside) | 6 m forward to an earned anchor | nearest s 1490, 2.1 m back along the road |
| Upside down on the road, Forward s 1000 | 0.1 m | 0.1 m |
| Off track at Backyard ramp (0.67 BUG-020), s 724 | 12.7 m | 1.6 m |
| Upside down, Backyard s 600 | 17.5 m back | 18 m forward (s 600–616 is the ramp run-up exclusion) |
| Failsafe below the world, race | 0.81 s, start-station anchor | **0.02 s**, nearest point 0.2 m |
| Failsafe below the world, Free Roam | 0.81 s | **0.02 s** |
| Quit race at BUG-009 (728.2, 79.2, -10.4) | left under the pavement; Free Roam then fell 20 m | seated on the pavement (floor 0.53 m below); Free Roam holds (drop 0.04 m) |
| Quit race on an elevated road (1013, 160, 96) | left in mid-air; fell 20 m | seated on the pavement; Free Roam holds |
| Free Roam start, Mountain Forward | fell 29.6 m through the world | holds, drop 0.01 m |

## Fall-through holes (BUG-007, BUG-009)

- **Rule-5 history check (`checks/compare-066-067-*.txt`):** every 0.67-modified ground mesh was compared with its 0.66 version along every Mountain route corridor (39,184 + 43,951 samples).
  - 0.67 created **no** new void.
  - It removed a single buried duplicate ground layer at 42 Forward samples near s 2624–2678; the top surface was kept.
- **0.67 seam covers over holes:**
  - The audit (`checks/*-seam-audit.txt`) found that 538 of 6,488 (Forward) and 2,050 of 11,756 (Reverse) visual-only seam-cover triangles had **no collider at all** beneath them, or the ground more than 2.5 m down.
  - These are narrow slots up to ~1.1 m wide, enough for a motorcycle wheel.
  - A cluster lies right beside the Homeward landing descent and bend, where Dan overshot (x 720–810, z 20–90).
  - Each such triangle now has a matching collider (`Ground_Report068 seam support`, collider only). The visible surface is now solid.
  - Lists: `seam-support-*.txt`.
- **Mountain Forward start marker:** it was 1.49 m below the pavement with nothing under it. This is BUG-009's location (s 2635) and it pre-dates 0.67 (same height since CR-112). Fixed (see above).
- **Gentle drops onto the seam slots and onto Dan's fall site (`checks/reset-before3/4`):** none passed through on 0.67. The exact pass-through in BUG-007 was not reproduced. The holes are closed regardless, and the failsafe now recovers any fall immediately.
- **Discovery, not changed:**
  - Mountain Reverse has no ground north of z ≈ 380 (60 m beyond the northernmost road, x 800–1400).
  - It is outside every route. A vehicle reaching it is now recovered by the failsafe.

## Part B — Mountain Loop Reverse

**Before/after images:** `views/*-before-after.jpg`.

### BUG-003 (814.16, 95.49, -171.39) — PASS

- **Green sheets across the pavement:** these were a 0.67 edge-shoulder sheet built from the Downhill Ridge Cut edge, lying 1–7 cm *on top of* the main pavement (x 807–814).
  - 69 shoulder vertices that lay on the pavement were tucked 1.2 m under it, the same rule 0.67 used for points under the pavement.
  - The same check in Forward found none.
- **Torn edge:** this is the junction's jagged pavement outline beside the gold Downhill Ridge Cut arrow, already a known limitation. Its terrain sits at road level with no gap; unchanged.
- **Blank dark board:** this is the **back** of "<<< SHORTCUT 2 / DOWNHILL RIDGE CUT" (830.0, 98.1, -207.4).
  - Its text already faces approaching Reverse traffic. Race direction here is heading ≈333°; Dan's capture faced 176°, backwards.
  - It is useful (it names the shortcut at CP3), so it was **kept and not turned**. If Dan meant something else, his correction wins.

### BUG-004 (810.26, 95.43, -159.19) — PASS

The "OLD CUT CLOSED / MAIN ROUTE >>>" sign and post at (798.0, 96.4, -145.0) were removed.

### BUG-005 (744.70, 85.84, -113.38) — PASS

- **Cause:** a 0.3–0.6 m step in the pavement mesh along z ≈ -115 joined the climb to a flatter plateau.
- **First fix:** a 5 m blend. The full-throttle check still launched a bike at 35 m/s.
- **Final fix:** the pavement is set on a smooth 20 m vertical curve (cubic, z -126 to -106; 42 vertices of `reverse-branch-join.asset`). Profiles are in `bug005-crease.txt`.
- The cut arrow ("Main teal / turn") is reseated whole and flat.
- **Checks:**
  - AI moto now crosses with roll 1.4° (0.68 round 1 with the 5 m blend: roll 81°).
  - Full-throttle moto crosses with ≤ 0.1 s airtime.
  - At 35 m/s the full-throttle line then runs 11 m wide on the following curve and leaves the outside edge (x ≈ 757). That is the speed, not the crease; no tuning (rule 12).
- **Dependents:** no arrow or sign other than the arrow above lies in the regraded band; the edges meet the existing banks.

### BUG-006 (1005.11, 162.65, 121.37) — PASS for the gap and shards; the slab is identified and kept

- **Gap:** the pavement edge stood 0.5–1.8 m above the MountainCut cap.
  - A collidable earth shoulder now runs from each exposed edge (main s 428–488, both sides) down to the cap.
  - It skips the Summit Traverse entry corridor.
  - 459 portal-outcrop/cap vertices that poked up to 1.5 m above the new shoulder were lowered under it.
- **Protected cut:** the edited MountainCut meshes are Reverse-only. The South Face run-up passes through that cut, and drives from s 800 and s 900 through it are identical to 0.67 (same airtime and roll).
- **Large dark slab in the sky:** this is the **underside/back of the "OPTIONAL SHORTCUT / SUMMIT TRAVERSE" sign** (1002.1, 166.9, 113.0). It is not stray geometry; it is a named optional-route sign, so it was kept. The chase camera passes beside its board.

## Part C — BUG-008 landing (Mountain Forward, Homeward Summit Flight)

**Measured (`jumps-MountainLoop.txt`, `checks/reset-before2`):**
- The lip at s 2205 is a 32° ramp. Only riders taking off at ~33–36 m/s land on the 17 m flat deck (s 2316–2333). Slower riders drop into the valley (solid ground); faster ones fly over the deck.
- Full-throttle moto and ATV (40–44 m/s at takeoff) all land **on** the landing descent (s 2416–2473, within 0.9 m of the centre line).
- That descent is a 167 m paved slope already 9+ m wide each side. Widening it found nothing to add (`partC-landing.txt`).
- The problem is that it ends in a 68° left bend (s 2483–2563). Riders arrive too fast and leave the outside of the bend, toward the plateau edge, where Dan fell (684, 72).

**Change** (approach, ramp, lip, deck and descent unchanged):
- The landing zone continues through the bend as a **runout up to 15 m from the centre line** on the outside, s 2474–2545. It is flush with the pavement edge, falls 3%, and sits on a supported earth skirt (`Ground_Report068 landing zone`).
- It is bounded by the Part E berm.

**Checks (`checks/berms3-reset-MountainLoop.txt`):**
- Full-throttle clean line s 2470–2580, moto and ATV: complete, berm contacts 0.
- Overshoot with no steering at 32 m/s:
  - ATV: kept against the berm, upright.
  - Moto: glances over it.
- At 38 m/s both arrive airborne off the descent and clear the berm (2.4 m). A rider who does so is restored by the new reset at the nearest bend point.
- Dan judges the feel. A ramp reduction was not proposed: the landing already receives full-throttle riders.

## Part D — pause menu

The in-race option now reads **END RACE / RETURN TO MENU**, and its confirmation is "END RACE AND RETURN TO MENU?". Free Roam keeps "RETURN TO MENU".
- Text only: the existing row width fits it (`checks/pause-menu.png`).
- Controller and keyboard navigation are unchanged (same row and action).

## Part E — barriers

See `BARRIERS.md` for the jump list and suggestions.

**Built:** three earth berms (natural formation, near-vertical 85° rock inner face, grounded, collider matching the visible mesh, 8 m tapered ends).

| Site | Clean line (moto, ATV) | Overshoot, no steering |
|---|---|---|
| Forward Homeward bend, main s 2476–2545, 2.4 m | complete, 0 contacts | 32 m/s: ATV kept, moto over. 38 m/s: both airborne over it (see Part C) |
| Reverse South Face bend, main s 1279–1350, 1.8 m | complete, 0 contacts | 32 m/s: both kept and deflected along (lateral 6.5 / 8.6 m, upright 1.00 / 0.52) |
| Reverse Downhill Ridge Cut, branch s 112–192, 1.8 m, toe 1.4 m off the pavement | complete. The full-throttle line runs wide on this narrow 86° turn and brushes the berm (moto 1, ATV 2) | 24 m/s: both kept and deflected along (lateral 4.5 / 6.9 m) |

**Built and then removed:**
- The High Ridge Drop berm (Forward Summit Traverse s 8–60) is at the shortcut entrance.
- In verification it touched the full-throttle line and stopped an ATV, so it was removed and is listed as a suggestion.
- The first berm profile (65°, 1.5 m) let an ATV climb and vault it, so the profile was steepened.

## Part F — arrows

See `ARROWS.md`.
- 538 → 302 arrows on the eight courses. 236 were removed: 146 inherited Street Loop / off-course, 10 duplicates, 80 repeated on straights.
- 7 kept arrows were reseated flat.
- One pass per changed course and direction (`arrows/arrows-*.csv`): every turn, fork and shortcut entry keeps an arrow (fork windows 40 m before to 80 m after; turns over 20° within 40 m).

## 5A.6 neighbour checks (moto + ATV; `checks/drives-*.txt`)

**Same as 0.67:**
- Lower main route s 1460–1830 (AI moto/ATV, complete, 0 resets).
- South Face jump, throttle from s 800, moto/ATV complete. The known AI undershoot stop at s 1180.9 is unchanged.
- Summit Traverse, both scenes (AI moto/ATV, rejoin).
- Forward s 740–860 and s 2560–2740 (start area).
- Reverse s 100–200.
- BUG-006 corridor s 400–520 (AI moto, throttle ATV).
- Climbing Ridge Cut throttle: complete. ATV landing roll 87° vs 71°, upright again, no reset.
- Downhill Ridge Cut throttle: stops at s 117.4–117.8, identical to 0.67 (pre-existing crash at its second jump).

**Pre-existing, confirmed on the 0.67 scene (`checks/drives-baseline067-*.txt`):**
- The production AI stops at the Homeward Summit deck (s 2316–2317) in both versions. This is the same class of limitation as the known South Face AI undershoot.

## Remaining limitations

- The exact BUG-007 pass-through was not reproduced (holes closed and failsafe added anyway).
- Airborne overshoots of the Homeward jump at 38 m/s or more clear the bend berm.
- Downhill Ridge Cut: the full-throttle line brushes its berm.
- Reverse world edge at z > 380.
- Known from 0.67:
  - 74 Reverse intrusion samples;
  - jagged pavement outlines;
  - Reverse s 1583 bump awaiting Dan's recheck;
  - AI flight undershoots at the South Face and Homeward decks.
