# Phase 6 — Buildings and CR-012

Status: implemented and technically checked; **AWAITING DAN'S VISUAL APPROVAL**. Phases 2–5 remain accepted. No Phase 7 or other environment category started.

Safety checkpoint: `cbe65fcf2c16ed7f4e1b0e1f59342fad210c8d65`. This captured Dan's saved PROJECT_TODO changes before implementation. Git required an elevated retry. The completion commit is reported in the task response and contains this report.

Scene: `Assets/Scenes/StreetLoopGreybox.unity`.

## Corrected arrangement

The annotated map's blue 1 is at reference coordinate (1028,951). StreetLoopRevision mapped that marker to `Original house 1`, world (415.800000,82.446440,-1.100000). The live hierarchy matched before removal. Its only children were Foundation, House mass and Roof mass, each with one dedicated collider. All four objects and all three colliders were removed; there were no separate house-specific props or access objects. Shared materials and unrelated residences were retained.

The 47 surviving site transforms, names and labels match baseline to six decimal places. In particular:

| Site | Unchanged world position |
|---|---|
| Dan — blue X | (352.000000,74.986400,105.600000) |
| Original house 2 | (438.900000,85.374370,-111.100000) |
| Original house 3 | (429.000000,32.754780,-220.000000) |
| Friend across street | (511.500000,84.865520,-35.200000) |
| Hairpin house | (484.000000,23.899020,-596.200000) |

House 3 remains farther from the road and down the steep hillside. The friend remains closer and slightly downhill across the street. No renumbering.

The yard is a continuous lawn connecting Dan's existing site to the former House 1 site. Only 15 tree trunks/colliders and their 30 corresponding combined crown/trunk cubes were removed, within 27m of that connecting segment. Grass color feathers out to 34m and protects the road/shoulder inside 16m. Three terrain meshes have 3,069 changed color entries total. Binary vertex-buffer comparisons confirm **zero position or normal changes**, identical index buffers and all other mesh fields. Terrain-road support and collision therefore remain the same. There is no replacement building, foundation, road or raised lawn overlay.

## Buildings and reusable assets

All 25 remaining residences and 22 existing main-road businesses received architecture. Four reusable residential variants use blue/cream/sage/brick walls, different roof pitches, a hip roof, offset entrance, porch roof and/or chimney. Houses have framed windows on front, back and sides, a door and grounded entrance steps. Four commercial variants use market canopy, diner parapet, pitched workshop roof/shutter bays and general-store frontage. Fictional signs are CORNER MARKET, HILLTOP DINER, AUTO SERVICE and GENERAL STORE. Original footprints, facing and setbacks remain; only small eaves, entrance canopies and steps extend beyond the former wall envelope.

These forms, dimensions, colors, windows and business identities are **architectural approximations**, not historically verified reconstructions. Supplied maps establish relationships, not facade details.

`Assets/Buildings/Phase6/` contains 8 prefabs, shared pitched-roof meshes, restrained materials, combined prefab meshes and 11 scene visual-batch meshes. The runtime scene uses 11 shared-material meshes (19,552 triangles) in `Phase 6 - architectural render batches`, plus separate shop text. Original prefab geometry renderers are disabled on scene instances; collision remains active and aligned. This reduces active renderers without changing shaders or adding runtime behavior. Batches have broad scene bounds and can submit off-screen building geometry; the geometry budget is small, but standalone profiling is still needed.

Foundation tops and building elevations stayed fixed. Two bottoms were extended downward to remove exposed corners: House 2 by 0.071m, and the approximate residence at (-61.60,33.50,-511.50) by 0.359m. Their primitive box colliders extend with the visible geometry. Other foundation geometry remains. Walls use box collision and roofs simple convex meshes; small window/trim details and commercial awnings are visual only. Entrance steps use solid ground-reaching boxes.

## Workflow and preservation

- `Phase6Buildings.Build()` is a local authoring operation. It identifies House 1 explicitly, applies only this batch, and refuses to repeat a completed batch.
- `Phase6Buildings.RefreshVisualBatches()` refreshes only building visuals after a deliberate prefab/site edit. Save the scene and assets afterward; the helper already does so. Disabled source renderers should not be re-enabled alongside the batches, which would double-render them.
- Both older generators omit House 1 and reserve the connecting yard from tree generation. The legacy whole-environment rebuild now refuses scenes with the accepted shortcut, Phase 4 root or Phase 6 architecture. **Do not run it on this scene.** It was not run during this task; the guard was inspected in source only.
- `Phase6Review` supplies repeatable snapshots, review captures and a fixed-camera performance sample. `Phase6Validation.Geometry()` supplies read-only geometry checks.
- No vehicle, input, chase-camera, reset, race, HUD or checkpoint source changes. No jump/shortcut authoring or tuning changes. Preserved scene records and mesh-buffer comparison: `PHASE6_PRESERVATION.txt`.
- Original Phase 3–5 validation reports were restored byte-for-byte after reuse. Current results have PHASE6 names.

## Actual validation

- Saved, reloaded and played the final scene; House 1 stayed absent and the architecture/batches persisted. Final scene is saved, outside Play mode.
- All 47 retained site transforms and labels match baseline. 1,579 vertical yard probes: no missing support or non-terrain obstacles; maximum sampled slope 18.34 degrees. Duplicate terrain tile boundaries have zero height mismatch; all terrain render/collision mesh references match.
- Final foundation corner maximum gap 0.002m. Initial 0.309m failure and the correction are retained in `PHASE6_GEOMETRY_INITIAL.txt` and `PHASE6_FOUNDATIONS.txt`. Minimum sampled building/step box-collider distance from road center is 18.56m, outside the 9m road/shoulder envelope. All 50 pitched-roof colliders are valid convex meshes.
- Drove the expanded yard from rest in both directions: 11.90/11.60s, peaks 10.32/10.34m/s, minimum upright 0.986, lateral errors 0.234/0.172m. Used virtual motor input and manually stepped PhysX; source in `PHASE6_YARD_DRIVE.cs` (run with eval_file in Play mode). No invisible obstacle encountered.
- Three continuous racing-speed laps, shortcut/normal/shortcut: 182.568/184.140/182.460s, valid finish, average 25.42m/s, peak 39.71m/s (143.0 km/h), maximum road-line error 3.23m, minimum upright 0.880. These traverse neighborhood and commercial roads, accepted jump and both normal/shortcut lines. Tight/hilly sections use the established slower curvature-based targets. Virtual motor input and manually stepped PhysX, no physical controller. `PHASE6_MIXED.txt`.
- Jump matrix: intended 32m/s approach reproduced 31.59m/s takeoff and 1.72s flight, landed upright. Slow, intended and fast trials plus bypass, shoulder re-entry and failed-landing reset exercised. Existing fast negative-yaw limits remain: 42/46m/s injected cases had 8.46/9.33m lateral offset. These are limitations, not newly approved driving speeds. `PHASE6_JUMP.txt`.
- 37 race/input/HUD/reset/restart checks passed unpaused, including virtual keyboard/gamepad, lap validity and three additional conservative PhysX laps. The first paused attempt produced 10 input failures; retained separately as `PHASE6_RACE_REGRESSION_PAUSED.txt`. Final report has zero failures. No runtime code was changed to pass the tests.
- Inspected overhead yard, unchanged chase camera within yard, Dan, House 2, House 3, friend, hairpin house, representative market and road-height commercial/neighborhood views. Initial House 2 close-up was occluded by an existing canopy; final after view uses a closer camera. Before/after pairs for Dan/yard/business retain matching viewpoints. No vegetation outside the yard was changed for visibility.
- Compilation completed with no errors. Final Console check: 0 errors, 0 warnings; not compiling, compilationFailed=false. Historical tool timeout and earlier test messages retained in `PHASE6_CONSOLE_HISTORY.json`. The authoring command exceeded the CLI response timeout while the Editor finished successfully; checked its report/save state before proceeding.
- An automatic approval review rejected the original geometry-test invocation because it called the legacy rebuild to test its guard. That invocation did not run. Removed the call and performed safe read-only checks instead.

## Performance

Same Editor, fixed commercial camera (-260,23,531) looking toward (-120,11,532), forest enabled, 734x293 Game view, vSync 0, target -1, background Play enabled, 45 warmup + 240 ordinary Play frames:

| Sample | Median editor interval | p95 |
|---|---:|---:|
| Before | 13.78ms | 21.30ms |
| First architecture, unbatched | 18.12ms | 31.28ms |
| Final, batched | 3.46ms | 4.48ms |

Enabled renderers: 505 before, 394 final. Total Renderer components are 820 because 426 prefab geometry renderers remain disabled for authoring. Collider count 6,363→6,514; building colliders 144→310, offset by removal of House 1 and 15 yard tree colliders. Timings are a short editor sample including host overhead; they do not establish standalone frame rate, worst-case performance, GPU cost or a universal speedup. No standalone build/performance capture, physical-controller test or exhaustive collision proof was performed.

## Review views

| View | Before | After |
|---|---|---|
| Dan's house | [Before](PHASE6_BEFORE_DAN.png) | [After](PHASE6_AFTER_DAN.png) |
| Yard from above | [Before](PHASE6_BEFORE_YARD.png) | [After](PHASE6_AFTER_YARD.png) |
| Main-road market | [Before](PHASE6_BEFORE_BUSINESS.png) | [After](PHASE6_AFTER_BUSINESS.png) |
| Commercial road height | [Before](PHASE6_BEFORE_COMMERCIAL_DRIVE.png) | [After](PHASE6_AFTER_COMMERCIAL_DRIVE.png) |
| Neighborhood road height | [Before](PHASE6_BEFORE_NEIGHBORHOOD_DRIVE.png) | [After](PHASE6_AFTER_NEIGHBORHOOD_DRIVE.png) |

[Actual unchanged chase camera within the yard](PHASE6_YARD_CHASE.png). Additional after close-ups: [House 2](PHASE6_AFTER_HOUSE2.png), [House 3](PHASE6_AFTER_HOUSE3.png), [Friend](PHASE6_AFTER_FRIEND.png), [Hairpin](PHASE6_AFTER_HAIRPIN.png). Road-height comparison captures use a placed camera at comparable driving height; the yard chase view uses the actual ChaseCamera component with the car posed for inspection. Neither is a claim of physical-controller footage.

## Dan's next review

- Confirm there are only Houses 2 and 3 beside your house, with their original labels and locations.
- Inspect the size, slope and natural forest edge of the expanded yard; drive across the former House 1 footprint.
- Judge whether the roofs, entrances and storefronts read clearly at racing speed and whether the restrained variations feel appropriate.
- Recheck House 3 down the hillside, your friend's closer downhill site, and the hairpin house.
- Drive a normal/shortcut lap including the jump; try the physical controller. Accept this focused batch or request local corrections before another Phase 6 category.

Remaining: Dan's visual approval and physical-controller confirmation; standalone/performance coverage as above. Existing forest blocks, occasional distant terrain edge speckling, and other deferred polish remain outside this batch. No new fences, destruction, vegetation pass, lighting, audio, road markings, additional shortcut/jump or car retuning.
