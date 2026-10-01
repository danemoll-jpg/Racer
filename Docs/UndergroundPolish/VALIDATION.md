# Underground / shortcut polish and recovery — 0.56.0-review1

Safety checkpoint: clean `main` at `8ab1294278a0708a6e7ab2e4ab5490d5c442eba9`. This is a focused polish pass; the accepted routes remain intact.

## Backyard Reverse

Removed the off-route `Main teal / reassurance` atlas arrow beside the supplied driveway-exit position. Its mesh is stored at world origin, with bounds centred at `(472.79,82.41,-5.80)`. No replacement or global arrow regeneration. The adjacent driveway/main-route arrows remain and support the left turn; see the local overhead image.

The Storm Drain entrance ceiling and lintel rise 0.9m, tapering into the existing ceiling over 28m. The two entrance piers extend upward with their bases retained. The continuous floor is unchanged. New local surface shading gives a darker mottled concrete interior with daylight at the portals and occasional subdued maintenance-light pockets. Existing shallow water, channels, pipe inlets and joints remain; substantial wall-edge boards, sediment, litter and pipe fragments add visible clutter without collision.

Four fixed primitive rats scurry towards wall recesses on approach, accompanied by a brief generated spatial squeak/scratch clip. They have no colliders. The event only rearms after the player is over 65m away for 20 seconds; it does not continually spawn. Audio uses the existing ambience setting, low gain and distance attenuation. No external assets or audio dependencies.

`LOGGING RIDGE` uses the established one-sided, depth-tested text shader on a grounded timber sign on the clear shortcut shoulder. The prior rebuild had removed the shortcut label. The new board is 6m off the shortcut centre and 10.97m off the main route; it is outside both driving corridors and has no collision.

## Recovery correction

The existing architecture already records recent stable progress, but its validators could prevent those samples from advancing:

- Every shallow water skin was rejected, including the drain's harmless water.
- A 14m-high support cast could choose an overhead road instead of the underground route.
- Placing the chassis above the highest uphill wheel inflated recovery height on slopes, rejecting otherwise usable recent poses.
- Legacy ramp-name checks did not recognize the new curved `Ground_` drain takeoff. The prior player-margin change reduced run-up exclusion to about one vehicle length.

Support now uses a bounded cast around route height, fits the chassis-centre support plane, rejects severe slopes/discontinuous footprints, and permits water no deeper than 0.18m. Full body/rider clearance remains required. Recovery consumes the shortcut's existing flight metadata and checks each lateral candidate; player approach margins allow acceleration. Airborne travel still earns no safe anchor, and a successful stable landing replaces pre-jump history. Delay/penalty, checkpoint credit, AI strategy and race architecture are preserved. See [the standing technical contract](../RECOVERY_TECHNICAL.md).

Representative saved-geometry fixtures pass normal off-road recovery, Logging Ridge, shallow drain below surface roads, failed exit jump to its safe approach, successful landing followed by a crash, cave recovery and Granite Creek recovery. Each usable established case selected its newest safe station without extra rollback. Drain ramp/crest/landing exclusions and a separate existing Forest main-jump approach/face check pass. These fixtures invoke the real record/selection methods with controlled grounded poses; they are not human crash tests or proof of every automatic-respawn scenario.

## Forest Reverse cave

The existing Fern Gully shortcut crosses the older longitudinal cave through the Fern Grotto. New detail is confined to its measured roof-covered span, stations 281.5–294.0: darker damp earth/stone, shoulder outcrops, ceiling formations, roots, gravel/puddles and one quiet localized drip source. Daylight remains visible at the exit. New detail has no collision. No cave route or surrounding Forest terrain was redesigned.

## Targeted results and limits

- Four Backyard production-driver traversals: motorcycle and ATV complete Storm Drain and Logging Ridge with zero resets and zero AI recoveries. Rat visual event/audio trigger counters pass. Full traces retain lateral deviations during jump/rejoin; no claim of perfect centre-line driving.
- Motorcycle and ATV traverse the local Forest cave segment, stations 268–310, with zero resets/recoveries and maximum lateral error about 0.04m. The existing production AI driver controls steering/throttle. This fixture seeds an already-selected branch; its inherited attempt to assert a new probabilistic choice reports failure because that choice is not applicable at this stage. The successful traversal results are separate. No AI tuning was made.
- Generated cave audio has nonzero PCM peak 0.2775, spatial blend 1, gain 0.11. Rat audio was triggered in live play. Ordinary checks were muted; no human/OS-endpoint listening claim.
- The first unrelated-ramp probes used an old shared Gully collider, including its bounds centre and surfaces without established route direction; they did not prove a valid takeoff exclusion and remain recorded. The representative additional ramp check uses the authored Forest main-jump metadata and passes. No coordinate exception was added to satisfy the probes.
- Visual inspection caught sign occlusion and overly regular surface shading; both were corrected. Eighteen cave stones were seated by their renderer bases; entrance pier bases remain unchanged. Trees, terrain and existing grounded dependencies were not moved.
- Preservation audit: six unrelated scenes byte-identical; 35 Forest and 23 Backyard original route/gate/flight components unchanged, allowing only newly serialized existing default values in Forest branch components. Vehicle physics, race/scoring, AI driver/strategy, UI flow, minimap and radio sources unchanged. Exactly one original scene object removed: the stray arrow.

Evidence: `backyard-driving`, `cave-driving`, `recovery-checks.txt`, `forest-ramp-check.txt`, `structure-checks.txt`, `grounding.txt`, `preservation.json`, and the local review PNGs. Initial failures are retained. Passing these targeted checks is the stop condition; Dan handles gameplay/atmosphere acceptance. Publication evidence is recorded separately after delivery.

