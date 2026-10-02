# 0.67.0-review1 — 20-report cleanup + Debug session persistence — validation

Source session `2026-10-02_03-04-39-449_547472` (20 reports, 0.66.0-review1). Every comment and screenshot was reviewed before any change.

All geometry work is local (CODEX_RULES 5A). `Tools/Author-SolidReportTerrain.cs` was not re-run. The authoring pass is [Tools/Report067/Report067Author.cs](../../Tools/Report067/Report067Author.cs); it is idempotent and its notes are in [author-notes.txt](author-notes.txt).

Each `views/BUG-xxx-before-after.jpg` shows the 0.66 state on the left and the final 0.67 state on the right. Both use the same camera: chase-camera framing along the reported heading, plus the extra angles listed below.

## Part A — Debug session persistence: PASS

**Cause confirmed.** `DeveloperLocationHud` held the session only in a static field and never reloaded it. After a relaunch, `EnsureSession()` therefore created a new folder and orphaned the open one.

**Fix:**
- `DebugReportSession.ResumeLatest(root)` reads the newest session folder only, and never writes. It resumes that folder when it is OPEN, has at least one report, and its `bugs.json` is readable with a matching ID.
- In every other case it returns nothing, and the next F4 starts a new session:
  - the newest folder is closed or empty;
  - its `bugs.json` is unreadable (that folder is left untouched).
- Older orphaned OPEN sessions are never resumed.
- `DeveloperLocationHud` resolves this once per launch, on first HUD/menu/capture use.
- A session ends only by Export or by the confirmed START NEW DEBUG SESSION. Scene changes and returning to the menu keep it.

**Checks.** [DebugPersistence/checks.txt](DebugPersistence/checks.txt) has 20/20 PASS:
- capture → relaunch → HUD shows the same ID, OPEN and count → next F4 is BUG-003 in the same folder, with no orphan folder created;
- scene reload keeps the session;
- export → relaunch → NEW → next F4 is BUG-001;
- confirmed new session → relaunch → NEW;
- corrupt `bugs.json` → NEW, with the corrupt folder byte-identical;
- the newest session is resumed, not an older orphan;
- all history is byte-identical after resume.

**Docs.** Updated in [DEBUG_MODE.md](../DebugReporting/DEBUG_MODE.md).

**Delivered session.** `..._547472` was closed through `DebugReportSession.Close()`, the close-without-export path. It now reads `closed:true, exported:false`, with all 20 reports and 20 screenshots kept ([delivered-session-closed.txt](delivered-session-closed.txt)).
- A newer OPEN session, `2026-10-02_13-02-35-687_63de1f` (2 reports, created by the installed 0.66 build at 13:02 during this round), was left untouched.
- The 0.67 build will resume that session on first use.

## Part B — dispositions

| Bug | Where | Cause found | Change | Result |
|---|---|---|---|---|
| BUG-001 | Street Loop Fwd (314.7, 9.1, 538.0) | The acorn-clue cairn `woodland-08` (a shared-world object) floated 1.63 m above lowered ground. | Seated the cairn and its collider in all 8 course scenes. Patch: vertex colours within 9 m blended to the surrounding ring median. | **PASS** (cairn). The faint flat patch is surface shading on a flat area, not a leftover pad. It remains faintly visible. |
| BUG-002 | Mtn Rev s 136 | The arrow has a renderer only (no collider, no raised geometry). The real cause is the vehicle body. It dips ~0.5 m below the pavement under compression (wheels are raycasts) and struck `continuous solid 1` terrain lying 0.7 m under the pavement at s 134–138. Contact logs showed the impulses. | Body clearance: every terrain vertex with pavement above it within 1.2 m was lowered to 1.2 m under that pavement (barriers, outcrops and tunnel ceilings excluded). The same cause was found and fixed at s 1576. | **PASS.** AI s 100–200: max air 0.18 s, roll 1.5° (0.66: 3.36 s, 43°). |
| BUG-003 | Mtn Rev s 369 | The SUMMIT sign board was sunk below the adjacent road. | Board raised to 1.55 m above max(ground, road within 6 m); now 2.05 m in Reverse. Forward checked (already readable). | **PASS** |
| BUG-004 | Mtn Rev s 397 | — | Removed the "<<< MAIN ROUTE / LEFT TO FULL RUN-UP" sign and post. The SUMMIT TRAVERSE sign and gate are kept. | **PASS** |
| BUG-005 | Mtn Rev s 953 | The boulder is "natural edge" rock mesh 1/3/7, hanging 0.6–0.8 m over the downhill slope. | Removed the "SUMMIT CAMP" sign in every scene. Natural edge rocks seated by the median underside gap (moved 0.9–1.1 m). Cairn `woodland-19` grounded. | **PASS** |
| BUG-006 | Mtn Rev s 1535 | Legacy CR120/CR121 arrows floated 1–3.7 m above the later regraded pavement. They overlapped the Route Atlas arrows. "Intermittent": no runtime toggling or culling, but the Downhill Ridge Cut bypasses this stretch, so they are not seen on every lap. | s 1495–1632: overlapping legacy arrows hidden; all others draped onto the final pavement (new meshes in `Assets/Track/Report067`). | **PASS** |
| BUG-007 | Mtn Rev s 1567 | The "hole" is a see-through seam 0.25–0.5 m wide between the jagged pavement edge and an earth bank at road level, with terrain 1.3 m below. Further on (s 1576–1590), the jagged edge stepped 0.4–1.2 m straight down onto lower ground. The old support test counted that near-vertical face as an embankment. | (1) Visual seam cover 4–6 cm under the surrounding surfaces wherever an edge gap closes again at road level within 1 m. It has no collider, so driving is unchanged. (2) Shoulders now treat a near-vertical step under the edge as unsupported, and add a sloped shoulder. Extra views: `BUG-007-edge`, `BUG-007-ahead`, `BUG-007-top`. | **PASS.** No see-through gaps remain (top view). The pavement mesh's own jagged outline is unchanged: changing it would change the driving surface. |
| BUG-008 | Mtn Rev s 1589 | A 0.9 m flat shelf followed by a ~45°, 0.9 m step in the pavement at s 1614–1617. The navigation polyline is degenerate there: it climbs almost vertically. | Grade rebuilt over s 1602–1625 on horizontal distance, with a per-offset Hermite profile. Top pavement layer only; driving width only; branches excluded; max change 0.16 m this pass. The holes between the sawtooth teeth beside it (s 1596–1604) are filled by the BUG-007 seam cover (`BUG-008-left`, `BUG-007-top2`). | **PASS.** AI moto/ATV s 1540–1830: max air 0.02/0.00 s, roll 0° (0.66: 1.46 s, 110°, minUp −0.34). |
| BUG-009/010 | Mtn Rev s 1734–1739 | No ground under the 48 m South Face receiving deck: a void beside the lower route and curtain faces. | New connected hillside (Laplace-filled heightfield) around the protected lower-route clearance. Tunnel ceiling at floor +6.8 m with portal headwalls. Under-deck fill at deck −0.35 m. Extra views: `BUG-009-side`, `BUG-010-below`, `FILL-*`. | **PASS.** Lower route s 1600–1820 and the South Face jump are unchanged in results (below). |
| BUG-011 | Summit Traverse 110 m | Eight "Summit natural edge boulder" objects were plain grey spheres. | Replaced by faceted rock meshes (3 variants) with matching convex mesh colliders, seated on the ground in both scenes. | **PASS** |
| BUG-012 | Summit Traverse 219 m | Unsupported pavement edge. | Part C shoulder (below). | **PASS** |
| BUG-013 | Downhill Ridge Cut 34 m | — | Removed the "SHORTCUT JUMP / CLEAR THE MAIN ROAD" sign and post. Ramp and orange arrows unchanged. | **PASS** |
| BUG-014 | Downhill Ridge Cut 151 m | The sign stood on the driving surface. | Moved 7.5 m off the pavement and grounded; readable from the road. | **PASS** |
| BUG-015 | Mtn Fwd s 786 | Unsupported left edge. | Part C shoulder. Extra view: `BUG-015-left`. | **PASS (support).** The chase view looks almost unchanged because the new shoulder follows the existing slope; the side view shows it closed. |
| BUG-016 | Summit Traverse Fwd 295 m | The grey slab is the traverse's outer pavement ribbon, flush on its supporting slope. | Part C shoulder under the unsupported part. | **Explained.** Supported now; the grey colour is the existing pavement material and was left unchanged. |
| BUG-017 | Summit Traverse Fwd 48 m | The ribbon had nothing under it. | Part C shoulder. Extra view: `BUG-017-below`. | **PASS** |
| BUG-018 | Mtn Fwd s 2653 | 553 buried duplicate terrain triangles stacked within 17 m. | Layer dedupe: duplicate sheets removed; pavement, gates and lines untouched. | **PASS (mesh mess).** The remaining pale band is the trail's own "Summit packed earth" driving surface. |
| BUG-019 | Backyard Fwd s 0.87 | Arrow `StreetLoopGreybox-arrow-031` was inherited from the Street Loop scene and points along Street Loop's direction. | Removed. About 75 other inherited Street Loop arrows exist in this scene; they are listed in author-notes (not reported, left unchanged). | **PASS** |
| BUG-020 | Backyard Fwd s 738 | Shared cause: the route polyline curved 74→95° over a straight run-up. The arrow and the reset heading both use the route tangent. | Route points s 718–762 aligned to the run-up axis (heading 90° from s 738); downstream stations shift ~0.4 m. The arrow at s 740.5 was rotated to 90° and draped. Recovery tuning unchanged. Ramp geometry unchanged. | **PASS.** Tangent is 90.0° at s 738–762. |

## Part C — Mountain edge sweep (both directions, main + all branches)

- **Sweep.** Ground 0.8 m outside each pavement edge every 2 m, plus wall/back-face checks. Lists: [sweep/](sweep/).
- **Flagged stations:**

  | Scene | Before | After |
  |---|---|---|
  | Forward | 207 | 92 |
  | Reverse | 408 | 143 |

- **Remaining flags:**
  - 40 are inside protected jump/flight systems (FLIGHT).
  - 13 are multi-level routes over another route (OVER).
  - 22 are South Face tunnel/hillside faces seen from inside the clearance (BACK).
  - The rest are steep but continuous embankments under raised berm roads:
    - Forward s 1816–1970 (summit plateau)
    - Reverse s 416–492 and s 718–770
    - Reverse Summit Traverse

    These are not floating; see `views/SWEEP-remaining-embankments-after.jpg`.
- **Fixes**, with per-run coordinates in [partC-MountainLoop.txt](partC-MountainLoop.txt) and [partC-MountainLoopReverse.txt](partC-MountainLoopReverse.txt):

  | Fix | Forward | Reverse |
  |---|---|---|
  | Edge shoulders | 18 runs / 604 stations | 32 runs / 1,238 stations |
  | Seam covers | 170 runs | 467 runs |

  - Protected edges left unchanged: 846 (Forward) and 953 (Reverse). These are jump approach→landing, flight corridors, and edges above another route.
  - Shoulders stop before other routes' corridors and never put a face into one.
- Intrusion lists: [intrusions-MountainLoop.txt](intrusions-MountainLoop.txt), [intrusions-MountainLoopReverse.txt](intrusions-MountainLoopReverse.txt).

## 5A.6 neighbour checks (moto + ATV, muted)

Before = 0.66 geometry; after = final 0.67 geometry. Raw results: [drives/](drives/).

| Corridor | Before | After |
|---|---|---|
| Rev main s 1540–1830 (lower route, BUG-006/007/008, Fern Creek Leap approach), AI moto / AI ATV / throttle moto | complete; air 1.46 / 1.30 / 1.76 s; roll 110 / 112 / 101°; minUp −0.34 | complete, 0 resets; air 0.02 / 0.00 / 1.08 s; roll 0 / 0 / 1.1° |
| Rev South Face jump s 800–1300, throttle moto / ATV | 7.16 s / 0.7°, 6.64 s / 0.1° | identical |
| Rev South Face s 800–1300, AI moto | fails at 1180.9 (known AI undershoot) | same point, 1180.9 (unchanged limitation) |
| Rev Summit Traverse entry → rejoin, AI moto / ATV | rejoined; air 0.48 / 0.46 | identical |
| Rev main s 100–200 (BUG-002), AI moto | air 3.36 s, roll 43°, minUp 0.68 | complete; 0.18 s, 1.5°, minUp 0.92 |
| Rev Downhill Ridge Cut jump, throttle moto / ATV | stall 117.8 / 117.4 m (same wall) | identical trace and results |
| Fwd main s 740–860, AI moto / ATV | clean | clean |
| Fwd main s 2560–2740 (BUG-018), AI moto | air 0.00 | air 0.14 s, roll 9.7°, complete, 0 resets |
| Fwd Summit Traverse, AI moto / ATV | rejoined; air 0.18 / 0.12 | rejoined; air 0.22 / 0.00 |
| Fwd Climbing Ridge Cut, throttle moto / ATV | complete | identical |
| Backyard ramp s 690–800 (BUG-020), AI moto / AI ATV / throttle moto | moto 1 recovery (roll 87°); ATV roll 10.9°; throttle 1.0° | moto 0 recoveries (7.6°); throttle 3.8°; ATV completes with 0 resets but lands harder on the straighter line (roll 23–37°, minUp 0.46–0.79) |

Ridge Cuts are not AI-validated (AI never enters them), so they are checked with the full-throttle driver.

**Fern Creek Leap.** The activity site (880.2, 115.5, −84.9) sits at main s 1630, 14.5 m left of the centreline, and its geometry was not modified:
- BUG-008 grading is limited to the driven width (|offset| ≤ 7.5 m) and excludes branch corridors.
- The only change near the site is the collider-free seam cover on the road edge at s 1628–1629.
- The approach is covered by the s 1540–1830 drives above.

## Remaining limitations (not changed in this round)

- BUG-001: a faint flat-area shading patch remains near the cairn.
- BUG-016: grey pavement-material slab.
- BUG-018: pale "Summit packed earth" trail band.
- Jagged pavement mesh outlines (BUG-007/008) remain; the gaps behind them are closed.
- 74 Reverse intrusion samples remain; they are mostly barrier rocks and pavement-edge slivers ([intrusions-MountainLoopReverse.txt](intrusions-MountainLoopReverse.txt)).
- Production AI still undershoots the South Face receiving deck (since 0.63).
- About 75 inherited Street Loop arrows in Dan's Backyard Forward are listed for Dan's review.
- Backyard ramp: the ATV lands harder on the corrected straight AI line. It completes with no reset in every run.
