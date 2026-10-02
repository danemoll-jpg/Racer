# Mountain Reverse regression restoration - 0.66.0-review1

Input: Dan's debug session `2026-10-02_00-57-44-616_93acc0_4d42d5ed.zip` (BUG-001 "really? Main route blocked", BUG-002 "this jump is now broken"); metadata and both screenshots reviewed. Safety checkpoint: clean main `f588a2d4` (playable source `a888e6a5`, 0.65.0-review1).

Scope: exactly BUG-001, BUG-002 and the permanent CODEX_RULES section 5A. Nothing else changed: no road, ramp, landing, navigation, checkpoint, sign, barrier, prop, physics, AI or other-track edits. The scene file is unchanged; only two 0.64 MountainLoopReverse-only terrain mesh assets were locally edited.

## Method: known-good history comparison

- Known-good: `2b584b45` (0.63.0-review1, game-63000), the last commit before the 0.64 cleanup. Its MountainLoopReverse scene was loaded beside the current scene in an isolated project copy (all referenced meshes still exist) and probed with identical raycast/overlap/drive checks.
- Regressing commit: `6af2c67d` (0.64.0-review1, "Fix 25 Mountain reports"). `Tools/Author-SolidReportTerrain.cs` deleted/clipped the existing Mountain ground tiles across x 830-1100 / z -144..356 and regenerated them as voxel "continuous solid" meshes. Its clearance carve list covered only main s 798-964, Summit Traverse and the two flight corridors. It did not protect the lower main route at s≈1700-1770 or the space under the South Face ramp. 0.64's own drive spans stopped at s 955 (before this ramp) and s 1690 (before the BUG-001 blockage), so neither regression was tested.

## BUG-001 - lower main route blocked (948.26, 127.27, -101.06)

- Root cause: `continuous solid 2` left two thin terrain sheets standing across the lower main route at s≈1713-1714 (x≈964) and s≈1765-1767 (x≈1015). These are the faces at the edges of the 0.64 South Face flight-corridor carve (x = 990 ± 25), where the lower route passes under the receiving road. In 0.63 the same volume was completely open (forward/overhead rays clear at every lateral offset).
- Restoration: removed only the 394 sheet triangles facing along the road inside the lower-route clearance volume (+0.15 m to +6 m above pavement, lateral < 14 m, s 1709-1719 and 1760-1772). Floor, canyon walls and the receiving-road support above (≥6.2 m) are retained. Route trajectory, pavement and checkpoints are unchanged.
- Static: zero forward blockers along s 1640-1800 at offsets -4..+4 (1.0 m and 2.5 m heights). The only overhead face is a retained 0.64 face 5.5 m above the left road edge at s 1767, above rider/camera height and not blocking.
- Drive (production RoadDriver AI and full-throttle line, s 1600-1820): 0.65 broken: moto AI stopped at 1710.8 (reset/recovery), moto throttle stopped at 1713.0, ATV AI stopped at 1710.6. 0.66 restored: moto AI, moto throttle and ATV AI complete to 1820 with zero resets/recoveries. 0.63 baseline: same three complete.

## BUG-002 - South Face Summit jump launch (983.14, 176.29, 68.85)

- Ramp, lip, landing and pavement collision are identical in 0.63 and 0.65 (±10 m lateral, s 950-982).
- Root cause: 0.64's heightfield rule (`floor-0.10` under any driving surface) filled `continuous solid 0` up to 0.1-0.5 m beneath the ramp pavement at s 977-983, with steep (normal.y 0.35-0.53) faces at the lip. In 0.63 the ramp stood free, with no geometry beneath it for over 12 m. The vehicle body collided with these faces while its wheels were still on the pavement (logged contacts at s 975.9-978.5, impulses 335-652), pitching the bike up 6-10° and rolling it. The same faces also tripped the AI obstacle sphere, so rivals braked from 46 to 16 m/s on the ramp.
- Restoration: lowered only the 884 `continuous solid 0` vertices beneath the ramp/lip (main s 966-986, lateral ≤ 13 m with a 6 m fade) to 4 m below the pavement. That matches 0.64's own 4 m post-lip flight clearance and restores 0.63's free-standing ramp relationship. Vertices within 12 m of any branch route were excluded. No holes: vertices are displaced, not removed.
- Neighbor check caught and corrected: the first, wider attempt (s 940-986) also deepened support under the Summit Traverse entry (branch s 16-30). It was rejected and restricted. Final neighbor diff across every main/branch station in both repair regions shows only the intended stations changed (main s 978-984, 1713-1714, 1765-1767). Summit Traverse is identical.
- Drive, full throttle from s 800 at offsets -5/0/+5 (moto) and 0 (ATV), compared with 0.63 from s 700:

| Run | 0.63 known-good | 0.65 broken | 0.66 restored |
|---|---|---|---|
| Moto 0 m, max flight roll | 0.6° | 28.5-58.4° | 0.7° |
| Moto +5 m, max flight roll | 0.5° | 25.1-93.6° | 0.5° |
| Moto -5 m, max flight roll | 0.8° | 0.7° (this line missed the worst faces) | 0.9° |
| ATV 0 m, max flight roll | 0.2° | 20.4-154° (one roll-over) | 0.1° |
| Solid-0 body contacts on ramp | none | 115-162 (impulse up to 652) | none on ramp* |
| AI takeoff speed (moto/ATV) | 35.9 / 34.6 m/s | 19.4-19.9 / 16.4-16.5 m/s | 35.7 / 33.9 m/s |

All restored throttle runs take off at 39.8-41.6 m/s, land on the receiving deck (s≈1182-1209) and complete to s 1300 with no reset. *The only remaining solid-0 contacts are zero-impulse resting contacts on the flat run-up at s 880-929 (normal.y 1.00), outside the changed area.

- Retained pre-existing AI limitation (unchanged from 0.63): at AI takeoff speed (~35.8 m/s) the production AI flight is level, then undershoots the receiving deck at s≈1137 in both versions. In 0.63 it passed under the unsupported deck; now it meets 0.64's accepted receiving support. Both fail after the flight. The landing was not moved or altered.

## Evidence

[restore-notes.txt](restore-notes.txt), [neighbor-diff.txt](neighbor-diff.txt), [drive/](drive/), before/after/known-good views `BUG-00N-*.png`. Tools: `Tools/Restore-ReverseRegressions.cs`, checks in `Tools/RegressionRestore/`. All checks are automated technical evidence; Dan's gameplay verification is pending.
