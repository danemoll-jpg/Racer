# Woodstock Rush CR-091–096 — implemented review pass

Baseline: **0.15.0-review3**, not blanket accepted. Its implementation, raw evidence and unresolved limitations remain in `Docs/CR081-090` and `Docs/CR082-089`.

Safety checkpoint: `a89577e9b0d3dfb4bf61652dd6ea71bae39bcdd1`. The initial staging operation failed on sandbox `.git/index.lock` permissions; elevated staging and commit succeeded and the clean tree was verified before source/content edits.

## Source and content

- Separate finite swept finish-region passage, armed by ordered route/shortcut progress and witnessed final approach. Outside-opening laps receive one finish entry in the existing five-second ledger. Final laps complete and missed laps invalidate clean ghosts. Reverse wraparound cannot fabricate ordinary missed-gate progress.
- Whole-area coordinate map with local saved exploration, discovered landmarks, collected-item markers, regional collection counts, mouse/controller pan/zoom, waypoints and free-roam-only discovered safe travel. Travel checks suspension support and complete vehicle clearance, cancels activity attempts and clears sampling histories. Map data uses `exploration-map-woodstock-world-v2.json`; older/unrelated files are retained.
- Stable 24 acorn IDs relocated to pockets with stone cairns and supported access. Existing found IDs remain found. A two-step collection-only restart preserves map, records, ghosts and preferences.
- House 2 shifted 65m along the street (about 213ft); house fronts aligned to local street; continuous frontage with two Dan drive gates, individual terrain-following fence endpoints, rear wood fencing, lowered Kyle property and overhead House 3 entrance sign.
- Permanent two-person mountainside campsite; separate summit launch/landing/return and authored jump activity. No mountain race, multiplayer, asset purchase or upload.

The new categories are `street-v14-discovery`, `lake-v7-discovery`, `street-reverse-v5-discovery`, and `forest-reverse-v5-discovery`. Historical boards/ghosts remain in their prior categories. Vehicle handling and jump scoring thresholds are unchanged.

## First-playable evidence and corrections

`first-geometry/summit-centered`: four actual 32m/s centered vehicle runs cleared the footprint and reset successfully, but **zero valid jump awards**. Approximately 5.6–5.9 seconds airborne led to hard landings.

`first-geometry/summit-matrix`: 60 actual traversals, four vehicles, 24/32/40m/s, center/±6m/actual body-edge approaches. Eleven did not clear; nineteen dipped below upright Y .65; zero awarded; all local recoveries succeeded. The original pilot also computed yaw from the pitched forward vector; the corrected pilot uses planar heading. These failures remain evidence, not successful final validation.

The revision lowers terminal launch grade from 1.2 to .4, raises and widens the landing, and adds a gentler supported return. Raw first-geometry builds and evidence are retained.

`first-geometry/finish-StreetLoopGreybox` reproduced eight reverse/linger failures in ordinary gate-miss accounting. The road-progress frontier has since been tightened; final repeat pending.

World authoring stopped before scene delivery on (1) an invalid generated filename containing a colon, (2) a steep collectible candidate with no suitable pocket in the initial search range, and (3) accessing a transient mesh after replacement by an existing asset. Targeted corrections and retries completed the four-scene pass. These authoring failures do not represent player validation.

## Validation and delivery

Compiled systems/persistence/regression checks and the 372-case final summit matrix are complete. See VALIDATION.md for exact evidence and remaining edge/shortcut/AI limits. Packaging, performance and checkpoint metadata are recorded there. No human driving, physical controller or listening acceptance has been performed. All ordinary test launches use isolated `Temp/cr091-*` saves and temporary mute; player preferences and shipped audio remain unchanged.

## Inspection corrections (historical intermediate results)

The first combined inspection reached all 24 relocated acorns from authored access points with ordinary-frame physical driving and successful local resets. A cold player launch retained all IDs and the same fog on all four tracks. This does not prove every approach angle is safe.

The first combined summit inspection awarded all four centered 32m/s runs, but the original/ATV flipped farther down the runout. Trace inspection identified the return trail carving a low crossing through the landing. The return now remains at runway height across the runout, then descends south. Its failed inspection traces remain intact.

Rendered sign inspection exposed coincident legacy labels re-enabled by BreakableProp.RestoreRace. Restoration now preserves each renderer's initial visibility; exact duplicate overlapping lettering is disabled and metric guidance converted to mph. Names and primary lettering remain. A separate map canvas was omitted by the old screenshot helper; the helper now captures every active overlay canvas. These corrections require the next compiled inspection.

Review2 confirms the matching-elevation northern return fixes the motorcycle descent. Review3 raises the landing plateau to 154.5m and return crossing to 151.5m to bring advertised fast approaches below the existing hard-landing cutoff; launch geometry, handling and scoring thresholds stay unchanged. Every new geometry has physical-player evidence before its next refinement. The final 372-case matrix is documented in VALIDATION.md; 216/216 center/normal-offset runs score, while 16 actual-edge forward cases remain nonclean.
