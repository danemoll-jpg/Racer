# Phase 6 initial vegetation refresh — historical validation

CR-014 supersedes this initial shape-only pass with expanded woodland. See [CR-014 validation](CR014/VALIDATION.md). Vegetation approval remains pending Dan's review; the earlier phase and building approvals are unchanged.

Safety checkpoint: `c20f244a36f0779d1b9e1e7ed8713e1b10ebc8d9`. The first staging attempt failed creating `.git/index.lock`; the authorized elevated retry committed the existing approval/backlog documentation. No project content changed before that checkpoint. Completion commit is reported in the task response.

Open `Assets/Scenes/StreetLoopGreybox.unity`. Phases 2–5 and the building batch remain accepted. CR-012 is closed; CR-013 remains optional backlog. Phase 6 remains incomplete; Phase 7 was not started.

## Changes and authoring

- Retained all **6,102 tree positions and colliders**, preserving the established forest coverage and sparse settlement. No new trees, ground vegetation or understory.
- Replaced repeated cube crowns with three reusable low-poly meshes: broad, irregular three-lobed, and upright. Deterministic variation in crown height, width and rotation; broad spatial patches vary crown family and muted green. Existing placements preserve clusters and gaps; narrower, uneven crowns soften forest edges. Brown trunks use the exact existing collider transforms.
- `Assets/Vegetation/Phase6/`: three crown meshes, one trunk mesh, one shared vertex-color material using the existing `Racer/GreyboxGround` shader, and **90 spatial batches** (160 m cells). Shared indexed crown vertices reduce bandwidth. No individual visible tree objects, runtime vegetation scripts, new shaders or duplicate renderers. Original unused cube mesh assets remain available for historical comparison.
- `Phase6Vegetation.Refresh()` / **Racer > Refresh Phase 6 Vegetation Visuals** rebuilds only forest visual batches from existing trunk colliders. It requires the saved scene outside Play mode. It is repeatable and preserves collider authoring. Do not invoke the older whole-environment rebuild.
- Crown radius respects the 27 m Dan/former-House-1 yard capsule, 16 m road-center corridor (including jump/bypass/landing), 7.6 m shortcut-center corridor and 16 m building-site radius. Final radius range 0.68–6.11 m; no leaves have collision. These are horizontal clearances, not a guarantee about every oblique sightline.
- Both older vegetation placement generators now use `Reserved()`: 28 m yard capsule, existing 23 m road setback and 10 m shortcut setback. Their existing site/sightline exclusions remain. The accepted-work guard on the revision rebuild remains; neither legacy builder was run.
- `VegetationReview` provides repeatable captures, collider snapshots and clearance probes. `Docs/VegetationTools/` contains C# snippets for Unity CLI `eval_file`: performance A/B, baked-trunk verification and existing driving tests. Run the profile/driving snippets in unpaused Play mode with final vegetation loaded; the trunk check runs in either mode. Test reports preserve previous phase reports.

## Preservation and geometry

Saved-scene record comparison against the checkpoint found **one retained record changed**: the forest parent's child list. The 360 removed records belong exclusively to the 90 original forest renderer objects; the 360 added records belong exclusively to their replacements. All other retained scene records are unchanged, including tree colliders, buildings/labels/elevations, road support, jump, shortcut, car, camera, controls and race objects. No terrain, building, vehicle, runtime gameplay, lighting or package assets changed.

- All 6,102 collider position/rotation/scale/center/size snapshots match before/after, including after final save/reload.
- All **48,816 unique visible trunk corners** match collider corners at 1 mm rounding; 146,448 baked bark vertices checked. Zero missing/extra corners.
- Expanded yard: zero tree trunks. Existing building validation also passed 1,579 yard support/obstacle probes and all 47 retained site transforms, with zero failures.
- Minimum conservative trunk surface clearance: **22.40 m from road center**, **16.72 m from shortcut center**.
- **1,553 vehicle-volume probes** across roads, shoulders, shortcut and re-entry found zero forest obstacles.
- Only combined forest geometry renders; forest renderers stay **90**, total enabled scene renderers stay **394**. Leaves add no colliders.

Evidence: [geometry](VEGETATION_GEOMETRY.txt), [trunk alignment](VEGETATION_TRUNK_ALIGNMENT.txt), [scene preservation](VEGETATION_PRESERVATION.txt), [build](VEGETATION_BUILD.txt), [before counts](VEGETATION_BEFORE_COUNTS.txt), [after counts](VEGETATION_AFTER_COUNTS.txt). Sampling is not an exhaustive collision proof.

## Driving and race validation

- **Three continuous mixed racing-speed laps passed** (shortcut / normal / shortcut): 551.52 s traversal, 25.42 m/s mean, **39.71 m/s peak**, maximum path error 3.23 m, minimum upright 0.880; valid finish. Existing approved vehicle tuning was retained. [Report](VEGETATION_MIXED_LAPS.txt)
- Shortcut support: **273 probes**, no missing support or obstacles. Four shoulder recoveries at 18 m/s stayed upright at least 0.999 and returned within 1.03 m of the road path. Four angled entrance/re-entry runs at 24 m/s stayed upright; maximum path error 2.23 m. Reset/skip/repeat/wrong-way checks passed. [Report](VEGETATION_RECOVERY.txt)
- Jump matrix reproduced the intended centered case: **31.59 m/s takeoff, 1.72 s flight**, landing upright. Bypass at 6/30 m/s, shoulder and reset cases passed. Existing high-speed negative-angle limits remain: 42/46 m/s target runs at −3° drift beyond the marked landing corridor (8.46/9.33 m maximum lateral offset). These are retained limits, not newly fixed behavior. [Report](VEGETATION_JUMP_TESTS.txt)
- **All 37 race/input/HUD/reset checks passed** using virtual input and manually stepped PhysX. [Report](VEGETATION_RACE_REGRESSION.txt)
- With the final optimized forest, an **ordinary-frame virtual Gamepad** shortcut run passed: 9.466 game seconds, **24.32 m/s peak**, 2.90 m maximum path error, upright 1.000. Camera followed (maximum camera-to-car distance 12.24 m) and HUD showed speed. Initial speed was injected; subsequent movement used ordinary Update/FixedUpdate and virtual controls. This run begins before starting a race, so race validity is established by the separate mixed laps. [Report](VEGETATION_REALTIME.txt)

The manually stepped driving suite ran before the final render-only indexing optimization; collider snapshots remained identical afterward, and ordinary-frame input/camera testing used the optimized final meshes. No physical controller, standalone build or human driving test was performed.

## Performance

Same commercial camera `(-260,23,531)` toward `(-120,11,532)`, 734×293 Game view, vSync 0, target −1, forest enabled. Values are **Editor frame intervals**, including Editor/host overhead, not GPU timings or a standalone benchmark.

The initial 240-frame baseline measured 5.97/10.82 ms median/p95. A high-detail crown iteration showed 24.99/36.05 ms; longer A/B sampling also showed added cost. It was optimized before completion: forest triangles **837,064 → 416,952**, serialized vegetation mesh assets about **372 MB → 59 MB**. The original cube forest had 146,448 triangles. Renderer count is unchanged. The final short sample measured 3.54/4.68 ms, but host variability makes that alone unsuitable as a speedup claim.

Final same-session alternating comparison, **90 warmup + 600 ordinary frames per run**, swapping original/final mesh references only in Play mode:

| Run | Forest | Median | p95 |
|---|---|---:|---:|
| 1 | Original | 3.72 ms | 5.38 ms |
| 2 | Final | 5.11 ms | 19.34 ms |
| 3 | Original | 14.59 ms | 25.21 ms |
| 4 | Final | 15.36 ms | 26.03 ms |

Final medians were approximately **0.77–1.39 ms higher** than preceding original samples. Both versions drifted substantially during sampling; the first final run had a higher tail. The batch has a measured rendering cost and **no claim of unchanged frame rate or universal speedup**. Review on Dan's normal play setup remains appropriate. Samples restored final visuals and the chase camera; Play changes were not saved.

[Final alternating data](VEGETATION_ALTERNATING_PERFORMANCE.txt), [discarded high-detail iteration](VEGETATION_HIGH_DETAIL_PERFORMANCE.txt), [initial baseline](PHASE6_VEG_BEFORE_PERFORMANCE.txt), [final short sample](PHASE6_VEG_OPTIMIZED_PERFORMANCE.txt). Earlier short samples are retained as `PHASE6_VEG_*_PERFORMANCE.txt`.

## Visual comparison

Each pair uses matching 1440×900 camera placement, FOV and scene lighting. Driving comparisons use the existing chase camera's offset/aim with the car placed at the same route point; they are static comparison renders, not claims of physical driving. Final captures were visually inspected, alongside the overview. The comparison pairs were recaptured with original mesh/material references temporarily restored in Play mode to improve the entrance camera; final references were restored afterward and no Play changes were saved. No lighting changes were made.

| View | Before | After |
|---|---|---|
| Childhood house and expanded yard overview | [Before](VEGETATION_BEFORE_YARD.png) | [After](VEGETATION_AFTER_YARD.png) |
| Forest edge near house | [Before](VEGETATION_BEFORE_EDGE.png) | [After](VEGETATION_AFTER_EDGE.png) |
| Childhood-house driving camera | [Before](VEGETATION_BEFORE_HOUSE_DRIVE.png) | [After](VEGETATION_AFTER_HOUSE_DRIVE.png) |
| Wooded road | [Before](VEGETATION_BEFORE_WOODED_ROAD.png) | [After](VEGETATION_AFTER_WOODED_ROAD.png) |
| Shortcut entrance | [Before](VEGETATION_BEFORE_SHORTCUT_ENTRY.png) | [After](VEGETATION_AFTER_SHORTCUT_ENTRY.png) |
| Shortcut interior | [Before](VEGETATION_BEFORE_SHORTCUT.png) | [After](VEGETATION_AFTER_SHORTCUT.png) |
| Re-entry | [Before](VEGETATION_BEFORE_REENTRY.png) | [After](VEGETATION_AFTER_REENTRY.png) |
| Jump approach | [Before](VEGETATION_BEFORE_JUMP.png) | [After](VEGETATION_AFTER_JUMP.png) |

## Save, compilation and limitations

Final scene saved/reloaded, outside Play mode, not dirty. Compilation completed successfully; final check up to date with no compilation failure. Final live Console snapshot reported **1 tooling timeout error / 0 warnings**, from the final comparison capture; no compilation or game runtime errors were found. During authoring, Pipeline's 5-second main-thread request limit produced timeout errors even though captures, refreshes and tests completed; completion was verified by reports and scene state. Existing obsolete `FindObjectsSortMode` warnings appeared during recompilation in older editor tools; the new review helper uses the current overload. Historical tool errors are not presented as game runtime failures.

Trees remain intentionally stylized and do not model particular local species. Ground remains sparse beneath the canopy; no understory was necessary for this focused pass. Existing distant terrain-edge speckling remains outside scope. All driving clearances were preserved, but exhaustive forest collision coverage and camera comfort need human review. Performance remains an Editor sample with the limitations above.

Dan's review:

- [ ] Check crown shapes, greens and forest-edge density at normal racing speed.
- [ ] Confirm the enlarged yard/former House 1 site stays open and Houses 2/3 read correctly.
- [ ] Drive wooded bends, shortcut entry/re-entry, jump and bypass; check sightlines and shoulders.
- [ ] Try the physical controller and watch frame smoothness on the normal play setup.
- [ ] Approve the vegetation batch or request local adjustments. Phase 6 overall stays incomplete.
