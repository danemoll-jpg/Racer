# 0.71.0-review1 — validation

**Source:** Dan's debug session `2026-10-03_00-37-35-649_a733ff` (BUG-001 to BUG-003, captured on 0.70.0-review1, build `25ce7af3`) and his written decision on 0.70 Part B (the Free Roam catch mound), plus the authorized lighting and atmosphere pass.

**Method:**
- Every comment was read and every screenshot inspected.
- Geometry was read with the read-only probes in `Tools/Report071/` (`Report071Probe/PartA/Layers/Area/Line/Near/List/Fine`).
- Changes were authored by `Tools/Report071/Report071Author.cs`; its log is [author-notes.txt](author-notes.txt). It also records the Part B attempts that were rolled back (git checkout of MountainLoop and its assets) before the final application.
- Play-mode checks (muted, isolated save): `Assets/Scripts/Report071Checks.cs`. Raw results are in [checks/](checks/).
- Before/after views are in [views/](views/): the 0.70 scene on the left, 0.71 on the right, framed like the chase camera at the reported coordinate and heading. The Free Roam "before" views are 0.70's own views of the same positions.
- The look (Part C) is measured and pictured in a release player at 3840×2160; see [LOOK.md](LOOK.md).

## Dispositions

| Item | Where | Disposition |
|---|---|---|
| Part A (0.70 BUG-001/002) | Free Roam, Summit Homeward Flight catch mound between (761.9, 109.3, 80.5) and (895.8, 151.1, 154.3) | **PASS.** Mound removed; open natural run-out; straight path rideable both ways; longer flights score. |
| BUG-001 | Mtn Forward, Climbing Ridge Cut 24 m (767.3, 88.2, −107.3) | **PASS.** Voxel-terrain slab beside the left edge lowered under one smooth flush surface; notch closed. |
| BUG-002 | Mtn Forward, Climbing Ridge Cut 6 m (753.7, 87.7, −120.9) | **PASS, one remainder.** Corner-cut rolls past 60°: 22/48 → 1/48 (80°, recovers). See Part B. |
| BUG-003 | Mtn Forward, CRC 278 m / main s 1393 (998.5, 137.8, −58.0) | **PASS.** The hollow in the gore is filled to a smooth grade flush with both road edges. |
| Part C | All courses, Free Roam, garage, menus | Implemented; see [LOOK.md](LOOK.md). |

## Part A — Free Roam: the Homeward catch mound becomes natural open ground

**What it was** (0.70 finding, confirmed): the catch of the Summit Homeward Flight (CR-094/CR-103, `752ca2f1`). The mountain terrain was lifted to the catch profile over the launch frame's s 320–540 (up to 27 m above the ground beside it, ending in a 33–47 m cliff at s 328), with a 48 m-wide collidable sheet `Ground_CR103 smooth landing` on top. It buried the `Mountain return trails` exploration trail, the `Lake shore connection` route, 4 acorn-clue cairns (up to 21 m deep) and ~30 trees.

**Scenes:** it is active in all six Street-Loop-world scenes (StreetLoopGreybox, StreetLoopReverse, LakeWoods, ForestLoopReverse, DansBackyardForward, DansBackyardReverse), so it was changed in all six; Free Roam runs in whichever course scene is loaded. **Both Mountain race scenes: the mound is inactive there and they are unchanged** — their scene files are untouched and no asset they reference was modified (checked by GUID).

**Change (per scene):**
- The landing sheet was removed (its mesh stays on disk).
- Every terrain vertex in the box s 316–545, |x| ≤ 50 m was replaced by the smooth (harmonic) surface spanning the untouched ground around the box: the surrounding terrain level, no mound, no cliff. It falls from 135 m at s 316 to 79 m at s 536, then the existing flat at ~80 m. The buried trails came out within 0.3–2 m of their original heights (independent confirmation of the natural level).
- Approach, run-up, lip and activity site (s ≤ 220) are untouched.
- **Run-out:** 59–71 trees standing on the flight line (|x| ≤ 22 m, s 300–690, i.e. up to ~45 m before the Anderson/Kyle frontage) were removed, trunk collider and batched pieces.
- **Grounding (rule 4):** 51–54 trees per scene re-seated (trunk + batched pieces; the ones the mound had buried were stood on the new ground), the 4 cairns uncovered and seated, the RIDGE RETURN sign re-seated (−17 m, wording kept: it describes the return path, and no sign describes the catch), the supported-return ribbon's vertices in the box re-seated 3.5 cm above the new ground, and the three route polylines in the box (safe return, exploration trail, lake shore) re-seated on it (Free Roam resets use them). Terrain under the middle of the 14 m ribbon was then lowered 15 cm under it so grass does not show through.
- **Scoring:** a Homeward/South Face giant flight now scores up to 2000 m and 20 s airtime (other jumps keep 250 m / 12 s). Dan confirmed he never wanted a cap.

**Results** (StreetLoopGreybox, Free Roam, from rest at run-up s 5, full throttle to the lip, no input after touchdown; [before](checks/partA-homeward-before.txt) / [after](checks/partA-homeward-after.txt)):

| | Flight distance (3 runs) | Landing | Run-out | Score |
|---|---|---|---|---|
| Moto 0.70 | 230.2 / 231.2 / 231.1 m | on the catch sheet, s 451–452, at 49–52 m/s | 120–138 m; 1 of 3 hits a tree (s 566) | 1 of 3 not scored (hard landing, min up 0.36); 231.8 m |
| Moto 0.71 | 242.5 / 243.5 / 243.3 m | natural ground, s 464, at 54 m/s, upright (min up 0.97) | 80–84 m, open, stops at s 542–547 | all 3 scored, 243.8–244.2 m (800 ft) GOLD |
| ATV 0.70 | 206.9 / 207.9 / 207.2 m | on the catch sheet, s 428–429 | 77–135 m; 2 of 3 hit trees (s 505, 516) | 207.5–207.8 m |
| ATV 0.71 | 223.5 / 224.0 / 223.8 m | natural ground, s 445, upright (min up 0.96) | 93–94 m, open, stops at s 537–538 | all 3 scored, 224.1–225.3 m GOLD |

Take-off speed is unchanged (moto 43.0–43.2 m/s, ATV 41.1–41.2 m/s).

**Straight path between the 0.70 positions** ([before](checks/partA-ride-before.txt) / [after](checks/partA-ride-after.txt), ~12 m/s along the straight line):
- **0.70:** downhill (from the 0.70 BUG-001 position) blocked after 28–29 m at the mound's end cliff (min up 0.25); uphill "completes" only by climbing the mound and going off its 33 m cliff.
- **0.71:** complete both ways, moto and ATV, no stop, no reset (min up 0.68–0.83).

Views: [0.70 BUG-001 position](views/FR-001-before-after.jpg), [0.70 BUG-002 position](views/FR-002-before-after.jpg), [the new landing from above the lip](views/LAND-over-after.jpg).

## Part B — Mountain Loop Forward, Climbing Ridge Cut

**Cause (all three):** the ground beside the trail here is several terrain sheets stacked within a few centimetres to metres of each other (CR133 earth banks, MountainPolish shoulders, 0.69 flush shoulders, the 0.69 junction patch, the 0.68 seam support, voxel tiles `Ground_720_240/320`), with steep faces where they cross. 0.70's entry tests already showed the 32 m/s flips coming from earth-bank faces just under the 0.69 patch and from the `Ground_720_320` slab.

**Change:** one smooth collidable surface per place, built on a 0.5 m grid over a set of circles: tucked 3 cm under every pavement edge (no slot), pinned 2 cm under the existing ground at its outline, flush against surfaces that must stay (the 0.70 gores, the 0.69 berm, rock barriers), harmonic in between. Every other terrain sheet under it was lowered to 0.8 m below it — only where the new surface or the pavement covers the point and 0.4 m around it, so nothing is left just under the surface and no slot opens. Pavement, berm and gores were not edited. Trees and the sign post moved with the ground. Mountain Reverse uses none of these meshes (checked by GUID) and is unchanged.
- **BUG-001/002 entrance:** circles (757, −117, r 15), (777, −101, r 10), (771, −111, r 6) — the inside wedge by the sign, the gore between the shortcut and the main road, the right shoulder at branch s ~24 and the left edge at s 20–40. New surface within −2.7…+0.5 m of the old top (the BUG-001 slab came down 2–3 m). 9,114 sheet vertices lowered. The OPTIONAL SHORTCUT sign was not part of the cause (never touched in any run); it was only re-seated −0.15 m onto the new ground.
- **BUG-003 rejoin:** circles (1012, −26, r 17), (1028, −37, r 14), (1000, −38, r 10), (1033, −16, r 12), (1034, −5, r 10) — the hollow north of the shortcut and west of the main road (floor 128–130 m, walls up to the road at 140–146 m), filled to a smooth grade from both road edges (up to +10.6 m). Only the main road and the shortcut pass this area (no lower route, 5A.4). 9 trees raised onto the fill with their batched pieces. The 0.69 berm and both 0.70 gores were left as they are.

**Results** ([before](checks/partB-cut-before.txt), [after](checks/partB-after.txt), [final re-check](checks/partB-final-recheck.txt)):
- **Corner-cut lines** (6 straight lines across the inside wedge and the gore, then along the shortcut; 14/20/26/32 m/s; moto and ATV; 48 runs): rolls past 60° **22 → 1** (past 90°: 17 → 0). By speed, 0.70 → 0.71: 14 m/s 1 → 0, 20 m/s 3 → 0, 26 m/s 10 → 0, 32 m/s 8 → 1.
- **The 0.70 entry set** (13 lines, 8–32 m/s): all 7–12° roll. In 0.70 the 32 m/s moto lines rolled 105° and 180° and the ATV line 87°.
- **Remainder:** two 32 m/s corner cuts (the widest line across the wedge by the sign) carry the vehicle over the far side of the trail at branch s 30–40 into the ridge cut's bank: ATV 80° (recovers, completes), moto 51°. That is running off the trail at full speed, not a hidden face; per rule 12, left for Dan.
- Views: [BUG-001](views/BUG-001-before-after.jpg), [BUG-002](views/BUG-002-before-after.jpg), [BUG-003](views/BUG-003-before-after.jpg), [BUG-003 overview](views/PIT-over-after.jpg).

**5A.6 neighbour checks** (results match 0.70 except where intentionally changed):
- Climbing Ridge Cut jump line, full throttle s 0–296: complete, max air 1.00 s (moto) / 0.92 s (ATV), min up 0.93, one berm contact — identical to the 0.70 scene ([before](checks/partB-jump-line-before.txt)).
- Production AI with the shortcut planned: enters and rejoins (moto 293.9, ATV 293.6 / 298.6), as 0.70.
- Rejoin: clean line s 240–296 complete (max air 0.54, min up 0.98), as 0.70. Overshoots from s 268 at 28 m/s and s 276 at 24 m/s: the 0.69 BUG-008 berm still holds them (contacts 1–3, no drop), as 0.70.
- Forward main s 1360–1460 full throttle, moto and ATV: complete, no berm contact, min up 0.99, as 0.70.

## Part C — lighting and atmosphere

See [LOOK.md](LOOK.md).
