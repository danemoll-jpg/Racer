# Forest / House 3 water jump — work in progress

Safety checkpoint: `480a279fc23afd0d1bee0a3c218f4a0ac6ecb5c9` (clean tree).

The current Forest Reverse main route remains main. Two obsolete driveway fragments are removed, the straight drop is now a smooth local ramp, and the pool followed by the lake is crossed in one continuous flight. House 3 and Laurel are preserved. No new shortcut geometry, receiving platform, road network, global physics, AI or recovery system was added.

## Required clarification

The prior pass removed the Granite Saddle optional component while making its line the current main. Fern Gully remains optional. The former main detour south of House 3 remains physically present but has no optional component. Is that former main detour the existing alternate Dan wants registered as optional? The dotted grey line in the [detail map](House3-Forest-Laurel.png) identifies this candidate. Classification must not be guessed or swapped with the main route.

## Interim atlas

- [Overall](Overall.png)
- [Forest Reverse](ForestLoopReverse.png), [Forest Forward](LakeWoods.png)
- [Street Forward](StreetLoopGreybox.png), [Street Reverse](StreetLoopReverse.png)
- [Mountain Forward](MountainLoop.png), [Mountain Reverse](MountainLoopReverse.png)
- [House 3 / Forest / Laurel detail](House3-Forest-Laurel.png)
- `routes-current.json`: actual saved Forest data plus unchanged scene data for the other five directions. Shared world X/Z metre coordinates. Unresolved classification is marked explicitly.

The entire Laurel scene is unchanged, including assets used outside the map's illustrative corridor. The pool/lake are Forest-only, south of the physical Laurel flight. The map's pink metadata line and physical flight differ in the existing project; neither was edited.

## Targeted evidence

- `geometry-checks.txt`: 615 support probes; three geometric flights at 24/32/40 m/s.
- `vehicle-check.txt` and `.csv`: one actual motorcycle flight, normal throttle and zero steering; launch 33.91m/s, dry supported landing near `(309,42,-196)`.
- `progression-check.txt`: production progression through the recorded flight, zero missed gates/penalties and no optional entitlement.
- `preservation.txt`, `data-preservation.txt`: unchanged house, main X/Z, existing shortcut points, other scenes, original assets and runtime behavior.
- `geometry.json`: footprint coordinates, affected vertex counts and removed driveway triangles.
- `overview.png`, `approach.png`, `water.png`: final local views after water-footprint vegetation clearance.

An initial authoring query included distant route points sharing the ramp's longitudinal coordinate. It stopped on missing support after creating local geometry. The query was bounded laterally and the partial operation completed without discarding work. The checked final data is saved.

## Still required

Resolve alternate identity; restore its metadata/entitlement/guidance with unchanged physical geometry; verify that route; finalize atlas/index and course/version metadata; completion commit; build/package; push; new latest release and signed catalog; remote assets and Play-Racer.cmd verification; authorized artifact cleanup. No new release has been built or published. Existing public release remains game-30000.

Builds inventory at this pause: 4.862 GiB; C: free approximately 314 GiB. No cleanup was needed/performed yet. Resume from saved implementation; do not repeat the completed geometry or motorcycle run.
