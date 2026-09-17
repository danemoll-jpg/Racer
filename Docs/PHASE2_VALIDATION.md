# Phase 2 validation and map notes — 2026-09-17

Open `Assets/Scenes/StreetLoopGreybox.unity`. The saved scene contains the complete environment; it does not generate itself at runtime. `PrototypeTrack`, `PrototypeCar.prefab`, and all four Phase 1 runtime scripts are unchanged.

## Reference interpretation

The annotated reference is primary; plain and satellite references were also inspected. North is +Z. The trace follows the northern main road, eastern neighborhood road, southern hairpin, winding lower return, and western connecting road. No invented closure road or modern subdivision streets were added. Main-road junctions are simplified into continuous rounded bends; road extensions beyond the requested loop are omitted.

Approximate scale is 1.1 metres per pixel of the annotated image displayed at 1311 x 1888, producing a 4647.3 m loop. There is no supplied scale bar or survey elevation data. The road is uniformly 9 m wide with 4.5 m shoulders on each side, widened/simplified for approachable greybox driving. A cubic curve smooths the hand trace with roughly 2 m mesh segments; small offsets from the red pen line are expected. No banking, curbs, road markings, junction traffic, driveways, or road signage yet.

## Elevation and landmarks

Northern road is about 8 m high. The neighborhood entrance climbs to about 50 m over roughly 350 m, then rolls between approximately 50–59 m. The second yellow-circle section drops from about 57 m to 28 m; peak short grade is 24.4%. The southern hairpin and lower bends retain moderate rolling terrain; the last yellow-circle return eases from about 17 m to 6 m. All heights are interpretive and need Dan's review, especially the steep drop. Broad surrounding ground is interpolated from road heights with small hills; no imported geographic terrain.

Dan's house (blue X), original houses 1/2/3, friend's house east/across the road, and the house behind the southern hairpin are named scene objects and blue-grey placeholder masses. Sizes, setbacks, orientations, and the exact friend's-house position within its large circle are estimates. The unrelated lower blue circle and storage facility are omitted. Nine additional houses and five north-side businesses are approximate. Simple scattered trees and open ground replace marked later development, especially west of Dan's house. Tree placement is deterministic but not surveyed. Canopies are visual only; trunks/buildings have colliders. Density is a greybox suggestion of woods, not final forest art.

## Executed checks

- Compiled both new editor scripts successfully, no compiler errors.
- Saved, closed/reopened the scene, entered Play mode successfully with the existing car and chase camera.
- Tested actual road colliders at every route sample across a 6 m corridor: zero missing support samples, zero blocking overlaps.
- Ran unchanged Phase 1 suspension/drive/steering forces and PhysX at 0.02 s steps around the full loop in both directions. No position/velocity correction or teleport during traversal. The editor-only test driver supplies throttle, brake and steering; it is not a gameplay system or component in the scene.
- Final forward drive: 559.7 simulated seconds; maximum centre error 2.84 m; minimum upright dot 0.971.
- Final reverse drive: 560.2 simulated seconds; maximum centre error 2.82 m; minimum upright dot 0.971.
- One initial suspension-settling step below two grounded wheels forward; zero reverse. No rollover or fall reset.
- An earlier faster test driver completed both directions but cut a tight forward bend onto the shoulder (4.79 m centre error). Shorter lookahead and lower corner speed fixed the test driver; the car and geometry were not altered to force that pass.
- Overhead and chase-camera renders inspected. Abrupt distant terrain interpolation ridges found and smoothed before final validation.
- Unity Pipeline's five-second command transport deadline logged timeouts on the long authoring/test calls even though Unity completed them. Completion was independently confirmed through saved assets and test output. Old Phase 1/tooling logs were cleared before a fresh scene reload/Play session and final console check.

Detailed machine test output: `PHASE2_TEST_RESULTS.txt`. Images: `Phase2_Overview.png`, `Phase2_Driving.png`.

## Known limitations and manual review

No unresolved compile/runtime or road-blocking problem in tested runs. The 6 m corridor and conservative automated drives do not certify maximum-speed cornering or every off-road collision. Slow for tight bends. Wide shoulders transition to coarse ground; roadside/off-road recovery deserves hands-on review. Reset R/Y remains the original fixed-spawn behavior and returns to the northern hill entrance, not the nearest road position. Physical controller hardware remains unverified.

Dan should drive a complete loop in both directions, review the four-house sequence and friend's house, judge the initial climb/rolling hills/steep drop/gentle final descent, and compare the southern hairpin and overall proportions to memory. Test braking before the hairpin, camera behavior over crests, shoulder re-entry, R/Y reset, and physical Xbox controls. Supply corrections before approving Phase 2. Phase 3 is not started.

Authoring tools: `Racer > Build Phase 2 Street Loop (new scene only)` refuses to overwrite an existing scene. `Racer > Validate Phase 2 (Play mode)` runs the test while playing this scene and resets the car afterwards. Keep reports outside Assets.

Final live Console check after fresh scene load and Play: 0 errors, 0 warnings, compilationFailed=false (2026-09-17 06:37:44 UTC). Pipeline historical counters retain earlier resolved/tooling entries; the live Console ground-truth counts are zero. Editor left stopped with StreetLoopGreybox open.
