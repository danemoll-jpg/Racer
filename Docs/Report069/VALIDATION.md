# 0.69.0-review1 — validation

**Source session:** Dan's debug session `2026-10-02_17-29-13-319_325918` (BUG-001 to BUG-008, all captured on 0.68.0-review1).

**Method:**
- Every comment was read and every screenshot inspected.
- Geometry was read with probes in `Tools/Report069/`.
- Changes were authored by `Tools/Report069/Report069Author.cs`; the log is [author-notes.txt](author-notes.txt).
- Checks use the muted play-mode harness `Assets/Scripts/Report069Checks.cs` and the 0.67 `Report067DriveChecks.cs`. Raw results are in [checks/](checks/).
- The before/after views are in [views/](views/). Each image shows the original 0.68 geometry on the left and 0.69 on the right, framed like the chase camera at the reported coordinate and heading.

## Dispositions

| Bug | Where | Disposition |
|---|---|---|
| BUG-001 | Street Loop Fwd (323.6, 8.7, 542.5) | **PASS — regression restored.** See Part C. |
| BUG-002 | Street Loop Fwd (471.2, 85.7, -20.8) | **PASS.** Both signs and their posts removed in all 8 course scenes. |
| BUG-003 | Street Loop Fwd (539.0, 81.0, -83.8) | **PASS.** LAKE SHORE / BOTH TRAILS RETURN HOME sign reseated in all 8 scenes. |
| BUG-004 | Mtn Reverse (827.3, 124.6, 108.9) s 254 | **PASS.** Sawtooth gone and the shoulder is flush; see Part A. |
| BUG-005 | Mtn Reverse (995.8, 163.8, 121.1) s 450 | **PASS, with limitations.** Berm built; see [BARRIERS.md](BARRIERS.md). |
| BUG-006 | Mtn Reverse (730.4, 99.6, -294.4) s 2740 | **PASS.** Smooth, collidable, flush surface over the torn gore. |
| BUG-007 | Mtn Forward (759.4, 87.8, -113.4), Climbing Ridge Cut 14 m | **PASS.** One smooth surface flush with the trail; see Part C. |
| BUG-008 | Mtn Forward (1021.3, 139.3, -66.9) s 1403 | **PASS.** Berm built; see [BARRIERS.md](BARRIERS.md). |

## Part A — road edges (Mountain Loop, both scenes)

### Cause

There are two separate things here. Neither is the voxel terrain, which the TODO suspected.

**1. Sawtooth outline (BUG-004).** The Mountain pavement is a world-axis 0.5 m grid in places (`reverse-branch-join.asset`, Reverse). Where the road runs diagonally to that grid, the pavement outline steps in 0.5 m stairs. The 0.67 shoulder lies 0.12 m under the edge, so it shows green in every notch.

**2. Step at the edge.** Ground just outside the pavement edge is not flush along most edges:

| Ground 0.3 m outside the edge | Before |
|---|---|
| `Ground_Report067 edge shoulders` (Reverse) | median −0.47 m; only 117 of 3,191 stations flush |
| `Ground_Report067 edge shoulders` (Forward) | median −0.38 m; 411 of 1,997 flush |
| CR133 earth banks | already flush (median −0.02 m) |

The 0.67 shoulders start 0.12 m under the edge and fall at 37°.

### Fix

**A1 — outline (source fix).** On each road side, every pavement boundary vertex moves sideways onto the local edge line:
- The edge line is a least-squares fit over ±1.25 m along the road, iterated 3 times.
- Heights follow the cross slope.
- Moves never flip a triangle.
- A "do no harm" pass measures edge straightness before and after at every 0.5 m station. Moves within 1.5 m of any station that got worse are undone.
- Lips, junction mouths, flaring edges, protected systems and multi-level/covered roads are left as they are.

Vertices moved:

| Scene | Mesh | Moved | Largest move |
|---|---|---|---|
| Reverse | `reverse-branch-join.asset` | 438 | ≤ 0.40 m |
| Forward | `forward-bend-final.asset` (mostly ribbons, few stairs) | 8 | — |

Centre line, width, grade and banking are unchanged.

**A2 — flush shoulders.** Where the ground beside an open edge is not flush, a collidable earth shoulder is built (`Ground_Report069 flush shoulders`):
- it starts 0.3 m under the pavement (tucked 0.1 m below it) and meets the edge flush;
- it runs 1 m out at −4%;
- it then falls 1:2 for 3 m, 1:1.33 to 8 m and 1:0.7 beyond, until it meets the existing ground, or rises 1:2 to meet a bank;
- terrain standing above the new 1 m verge is lowered 0.1 m under it, never toward a road below.

Exclusions:
- Where a tree trunk would end up more than 1 m inside the shoulder, that station falls 1:0.7 straight after the verge, or is left as it was (60 Forward and 47 Reverse stations). No trunk is buried more than 1 m.
- Posts and signs in the footprint were raised onto it. See author-notes.txt.

Built: Forward 1,909 stations (43 runs); Reverse 4,332 stations (106 runs). Runs are listed in shoulders-*.txt.

**Protected — not changed** (5A; the protection rule is the same in the author and the sweep):
- game jump-exclusion zones;
- branch flight windows;
- every flight system from its approach (approach station −40 m) to its end;
- 40 m around every activity jump (Fern Creek Leap, High Ridge Drop, Summit flights);
- roads with another route 1.5–40 m above or below (bridges, lower routes);
- covered roads (tunnel or cut roof overhead);
- the 0.68 berms;
- junction mouths;
- natural rock barriers;
- other routes' corridors and flight corridors.

### Counts

0.5 m edge stations measured in both sweeps, with the same protection rule for both ([edge-counts.md](edge-counts.md)).
- **Sawtooth:** the edge position deviates more than 0.08 m from the local straight edge line.
- **Step:** terrain adjoins the edge and the ground 0.3 m out is more than 0.15 m above or below the edge.

| Scene | Open stations | Sawtooth before → after | Step before → after | Protected (unchanged) |
|---|---|---|---|---|
| Mountain Loop (Forward) | 7,720 | 125 → 130 | 1,345 → **228** | 5,070 |
| Mountain Loop Reverse | 7,929 | 283 → **193** | 3,776 → **520** | 5,087 |

What remains, with coordinates, is in [partA-remaining.txt](partA-remaining.txt):
- **Forward sawtooth:** mostly junction mouths (Climbing Ridge Cut entry s 0–23 and rejoin s 1403 / branch 293; Summit Traverse rejoin s 337–345; lake junction s 2659–2688). The few stations added are where a junction edge was left as it was.
- **Reverse sawtooth:** junction and deck edges and grid stretches whose straightening was undone by the do-no-harm pass.
- **Remaining steps:** natural rock barriers, banks steeper than a 1.5 m wall, junctions, other routes' corridors and tree stations.
- **Shoulder overlaps:** at 7 single stations the new shoulder overlaps a pavement edge by 2–11 cm (Forward s 1330–1335, Reverse s 1566 and 1594). Nothing stands inside any driving corridor ([checks/intrude-*.csv](checks/)).

### Ride off and back on

Harness `offon`, 12 m/s: steer 3 m beyond the edge over 10 m, hold 10 m, return to the centre.

| Sample (worst stations before the fix) | Before | After |
|---|---|---|
| **BUG-004** Reverse s 254 L, moto / ATV | back on (minUp 0.70 / 0.44) | back on (minUp 0.85 / 0.54) |
| Reverse s 273 L, moto / ATV | back on, 14.2 / 13.4 m out | back on, 12.0 / 9.0 m out |
| Reverse Summit Traverse s 300.5 R, moto / ATV | back on, 12.6 / 12.8 m out | back on, 11.6 / 9.5 m out |
| Reverse s 1665 R, moto / ATV | cannot leave the road | same: a natural rock barrier lines this edge (left as it is) |
| Reverse Summit Traverse s 452.5 R, moto / ATV | moto not back (fell); ATV back on | both not back (ATV run differs): inside the protected Fern Creek Leap zone, geometry unchanged |
| Forward s 747 L, moto / ATV | back on | back on (same) |
| Forward s 1825.5 L, moto / ATV | back on | back on (same) |
| Forward s 2665 R, moto / ATV | back on | back on (same) |
| Forward Summit Traverse s 170.5 L, moto / ATV | back on | back on |

### Reset beside a corrected edge

All resets were restored in 0.82–0.83 s at the nearest point, with no waiting message, facing forward (dot 1.00), upright:

| Scene | Case | Result |
|---|---|---|
| Reverse | s 254 (moto) | 1.1 m along the road |
| Reverse | s 1665 (ATV) | 9.9 m along the road |
| Forward | s 1825.5 (moto) | 4.1 m |
| Forward | s 747 (ATV) | 1.8 m |

## Part B — barriers

See [BARRIERS.md](BARRIERS.md).
- **BUG-008:** Climbing Ridge Cut overshoots that fell about 40 m are now kept at the rejoin.
- **BUG-005:** 3 of 4 overshoots that fell 24–43 m now stop within 2–7 m. One ATV run follows the berm off its far end.
- Clean lines complete. The full-throttle Reverse crest line brushes the BUG-005 berm, and it no longer rolls over.
- The Summit Traverse entrance is open.

## Part C — terrain on the road

### BUG-001

**Regression from 0.67.** The 0.67 "pale patch" blend (`Report067Author.PalePatch`) recoloured every vertex within 9 m of the cairn to grass in `StreetLoopGreybox-CR129-junction-Ground_480_640.asset`. The Street Loop road there is vertex colour on that ground mesh, so 276 vertices changed:
- the west half of the asphalt (x 316–320, z 530–544);
- its pale shoulder.

Positions were unchanged. The mesh was restored byte-for-byte to its 0.66 version (`eec4e911`).

That asset is used by StreetLoopGreybox and both Backyard scenes. The other scenes have their own copies, which 0.67 did not change. The cairn keeps its 0.67 grounding (it is a separate object).

The "pale flat rectangle" is **not a pad**:
- its vertex colours are ordinary grass;
- it is flat ground at street level beside the cross street, below a 2 m slope break;
- it reads lighter only because flat ground is lit differently.

Left as it is.

### BUG-006

The green mound is the gore between two pavement arms at the end of the landing deck. Its tip was torn: ragged sheets with a trough showing through.

It was replaced by a smooth collidable surface, r 9 m:
- pinned 3 cm under the pavement edge (flush) and 2 cm under the ground at the rim;
- harmonic in between;
- 307 lumpy or torn terrain vertices inside lowered under it.

### BUG-007

The lumpy earth-bank wedge in the junction between the Climbing Ridge Cut and the main road sat 0.1–0.3 m above the trail. It had a crumpled, ribbed edge.

It was replaced by the same kind of surface, r 8.5 m: flush with both trails, joining the bank at the rim, with 281 vertices lowered.
- The Climbing Ridge Cut jump is not in this area and is unchanged.
- The production AI now enters and rejoins the Climbing Ridge Cut. In 0.68 it was reset before entering.
- A faint stepped rim remains on the far side of the patch (visual only). It was left there per rule 12.

## Part D — signs

Removed from all 8 course scenes, as `PhysicalSign` objects with their posts:
- FENCE LINE SMASH / 3 / 6 / 10 PROPS IN 8 s (CR079 arcade sites);
- ANDERSON'S / LAKE / MOUNTAIN TRAILS (CR081).

The Fence Line Smash activity, its props and the fences are separate objects and are untouched.

The LAKE SHORE / BOTH TRAILS RETURN HOME board was raised 3.0 m in all 8 scenes. Its bottom is now 0.4 m above the highest ground under the whole board width. The post runs from 0.3 m under the ground to the board.

## 5A.6 neighbour checks (same sets as 0.68; [checks/drives-after-*.txt](checks/))

**First run:** A1 had moved 3 pavement edge vertices on the South Face run-up (s≈950). That run showed:
- AI moto/ATV stopping at s 1092 instead of 1182–1184;
- the Forward Summit Traverse throttle line rolling.

Restoring the original pavement gave the 0.68 numbers back ([checks/southface-original-pavement-test.txt](checks/southface-original-pavement-test.txt)). Whole flight systems from their approach, and activity jumps, were then added to the protection rule and everything was rebuilt from scratch.

**Final run against 0.68:**

| Route | 0.69 result | Compared with 0.68 |
|---|---|---|
| South Face jump AI, moto / ATV | 1183.3 / 1181.6 | known AI stop, as 0.68 1183.9 / 1182.4 |
| South Face jump throttle, moto / ATV | 1143.4 / 1144.5 | 0.68: 1144.1 / 1144.3 |
| Lower main route s 1460–1830, moto / ATV AI | complete, 0 resets | identical |
| Summit Traverse, Forward and Reverse | AI entered and rejoined; throttle 341.0 | identical |
| Downhill Ridge Cut | AI / throttle | identical to 0.68 |
| Homeward landing / runout, s 2100–2650 | AI stops at the deck s 2316 (pre-existing); throttle complete | identical |
| Reverse s 100–200, 400–520, 2900–3010 | | as 0.68 (the throttle line is variable at s 2900–3010, as in 0.68) |
| Climbing Ridge Cut AI | entered and rejoined | improved; 0.68 reset before entering |

**0.68 berms:**
- **Homeward:** clean line complete with no contact; overshoot berm contact, rider ends 6.7–10.5 m off the line.
- **South Face:** clean line no contact; overshoot stopped 6.4–6.5 m out.
- **Downhill Ridge Cut:** the line brushes the berm, as in 0.68.
