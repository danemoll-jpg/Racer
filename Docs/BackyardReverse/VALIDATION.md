# Targeted validation — Backyard Reverse and shared-world corrections

## Result

Requested technical checks pass. Gameplay testing stopped after the one completed Reverse event. New Reverse remains for Dan's subjective gameplay review; approved Forward remains protected.

## Driving — ordinary motorcycle and ATV physics

- North gully: both pass, 2.48 / 2.50 seconds airborne; grounded touchdown near (50, 43, 80).
- South gully: both pass, 2.72 / 2.76 seconds airborne; grounded touchdown near (151, 45, -95).
- Pool-house flight: both pass, 2.24 seconds airborne; grounded touchdown near (457, 80, 12), beyond the pool house into the existing property return. Launch speed stayed above 30.8 m/s in these runs.
- The six feature drives use the actual ArcadeVehicle motor and Unity collision simulation, with a scripted pedal/steering pilot and a 28 m/s approach. They are technical checks, not a claim that a human drove the laps. No vehicle physics were altered.
- First-candidate gully/pool approach failures are retained in first-candidate/. The second candidate fixed the south-gully alignment; the pool-house pre-ramp crest still caused premature flight. A continuous local ramp profile corrected that issue; only the two pool-house drives were repeated. See feature-drive.txt, pool-feature-drive.txt and their pace CSVs.

## One completed normal-difficulty Reverse event

Menu selection, natural northbound grid, no optional shortcut entitlement, existing racing minimap, blue intermediate checkpoints and non-solid gates: all pass.

All four racers physically completed a full lap in approximately 105 seconds of event time, with no estimates or DNFs:

| Racer | Vehicle / control | Missed-gate events | Existing recoveries |
|---|---|---:|---:|
| YOU reference pilot | Motorcycle, standard RoadDriver | 1 | 2 |
| EMBER | Motorcycle AI | 1 | 2 |
| GOLD | ATV AI | 0 | 0 |
| BLUE | Motorcycle AI | 2 | 2 |

The event confirms traversal of both gully crossings, wooded dump bypass, pool-house ramp and finish under the existing AI system. Occasional collisions, missed gates and local recovery remain; there was no common permanent trap or systematic failure to finish. GOLD completed without recovery. Global difficulty, driver variation, shortcut probabilities, recovery and physics remain unchanged. Evidence: drive/done.txt and drive/pace.csv. Headless Editor ScreenCapture did not produce the requested finish PNG; no finish screenshot is claimed. Actual saved-geometry renders supply the visual evidence instead.

Editor harness attempts before this event did not complete a measurable race: exact button text missed the existing difficulty suffix, then Unity's temporary play-scene backup recreated an edit-time test object on reload. The corrected harness starts once after entering Play from the saved scene. These harness corrections are documented separately.

## Shared world and complete-tree dependencies

- 65 scoped scene checks pass across all eight applicable scenes: accepted McFadden pool/lake present; eight rim/shell colliders; locally supported coping; one centered original Roger house with supported foundation corners; trees around Dan's property on final terrain; physical lettering retained.
- Roger is at (440, 86.96748, -82), with local footprint support and matching landmark/collection approach. No roads, property boundaries, McFadden house or ownership changed.
- McFadden accepted geometry was found in ForestLoopReverse. Its pool at (416, 34.65, -197) and lake at (384, 33.35, -194.1) were reused rather than redesigned. Course-specific Forest jump logic was not copied.
- Largest checked pool coping-to-ground gap: 0.065 m. Roger foundation corner gap: less than 0.00034 m. Regional tree-foot error: at most 0.00299 m.
- Street Reverse Laurel approach/ramp/flight/runout has no conflicting added woodland trunks; ramp geometry is unchanged.
- Visual inspection found multi-piece crown remnants that the initial collider checks missed. Complete render batches were reconciled against preserved pre-task mesh geometry and original trunk identities, then existing orphan pieces were removed only in affected water/property/ramp regions. No floating canopy was accepted as a trunk-only fix. See complete-tree-repair.txt, orphan-piece-cleanup.txt and actual-geometry atlas.
- Map capture additionally checks the established added woodland against final terrain in each scene; map-grounding-checks.txt records counts and errors.

## Physical sign rendering

All physical TextMesh lettering uses the existing depth-tested, back-face-culled Racer/SummitText shader. WorldText also culls backs. Wording, transforms, direction logic and breakable components remain.

Representative actual renders: front lettering changes 1,614 pixels; back and opaque-blocked views change zero pixels when lettering is toggled. Direct BreakableProp lifecycle checks pass for attached lettering, debris hiding and restoration. This was a component lifecycle fixture, not a vehicle impact test. See sign-checks.txt and sign-front/back/occluded.png.

## Protection and maps

The preservation check compares the pre-task checkpoint: 23 Forward route/gate/shortcut/flight/undergrowth serialized components, approved Forward/shortcut mesh assets and 11 global runtime systems remain unchanged. Shared physical corrections are the intentional Forward changes.

The separately selectable actual-geometry atlas is ATLAS.html. Exact constructed coordinates and gate transforms are in ROUTE.md / geometry.json; original Forward anchor history remains. Full menu cartography is regenerated from the final eight saved scenes, keeps the persistent physical network and adds the actual Reverse overlay. The racing minimap architecture is unchanged.

Regional scene variants still contain their established region-specific terrain revisions, so the menu map remains an explicitly registered Forest/Backyard/Mountain composite. Permanent McFadden water exists in every current scene representing that property. Reverse adds no optional shortcut routes.

No all-vehicle/all-track regression matrix was run. Remaining evaluation is Dan's review of jump feel, forest readability and race pacing.
