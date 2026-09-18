# CR-019 commercial street — Phase 8 review batch

Status: implemented and technically checked; **awaiting Dan's visual review**. Phase 8 is not complete. Phase 7 is accepted because Dan answered yes to every checklist item; his input device was not specified.

Safety checkpoint: `f94c8f864fb7480a31a4d08ef7e4551d3fa9c1d5`. Git staging and commit initially failed to create `.git/index.lock`; elevated retries succeeded before project edits. The completion commit is reported in the task response / Git history.

Open `Assets/Scenes/StreetLoopGreybox.unity`. The scene is saved and left in Edit mode. Existing Phase 7 player builds are older and do not contain these scene changes; no new standalone build was made in this batch.

## Layout and coordinated sites

All **22 existing businesses remain**, 11 per side; the existing prefab instances, widths, depths, shop identities and signs are retained. Before, both rows were M-D-A-G-M-D-A-G-M-D-A, at approximately 73–75 m intervals, with all 11 centers aligned across the road.

West to east, M = Corner Market, D = Hilltop Diner, A = Auto Service, G = General Store:

| Row | New order | Measured center X intervals (m) |
|---|---|---|
| South | M-A-D-M-G-A-D-M-G-A-D | 47.30, 100.49, 59.06, 49.26, 102.65, 54.12, 56.88, 121.05, 62.90, 90.16 |
| North | D-M-G-A-D-M-G-A-D-M-A | 80.97, 55.18, 104.31, 51.67, 64.81, 117.11, 50.44, 63.34, 80.07, 68.29 |

Road-normal center setbacks are authored at 29–34 m. The sequences intentionally retain recurring assets while interrupting the order and spacing at different points. X intervals differ slightly from authored road-station intervals because sites follow the road normal through bends. Exact transforms, sizes and identities are in `before-sites.csv` / `after-sites.csv`.

The whole site root moves: architecture, prefab colliders, foundation and sign. Foundations are fitted to the existing terrain under nine footprint samples; entrance steps are regenerated locally. Repeated plaster storefronts receive restrained sage, cream or slate-blue material overrides from the existing palette. No paid assets, invented personal details, traffic or destruction systems.

Existing central frontage color patches were removed from their old positions. All 22 new sites receive flush paving and narrow gravel access, using terrain vertex color only. Road positions, collision, normals, indices and marking UVs remain unchanged; painting stops outside the 10 m center corridor. No terrain height adjustment was needed.

Nine interfering trunks were removed, listed in `local-tree-clearance.txt`; **11,986 remain**. Surviving trunk positions/rotations/scales are unchanged. Crown clearance follows the new sites. Mesh geometry/color auditing found changes in only six forest cells adjacent to commercial sites. Unrelated serialization noise from the existing vegetation refresher was restored through Unity asset APIs after exact geometry/color checks. The refresher now skips semantically unchanged meshes and clears unused vertex channels when a changed mesh is saved.

## Authoring and reproducibility

- `CR019Commercial.South` / `North` are explicit road-station, setback and original-site-index tables. Stable `CR019 S01..S11` / `N01..N11` identities preserve the original assets.
- `Racer > Phase 8 > CR019 Apply authored commercial sites` applies these tables in the saved scene, fits foundations/steps, repaints access, performs local trunk clearance, refreshes building and vegetation visuals, and saves. It runs only in Edit mode; there is no load-time randomization.
- `Phase6Buildings.RefreshVisualBatches` also refreshes frontage at current authored transforms. `original-frontages.json` and `last-frontages.json` retain paint footprints, removing previous patches on deliberate edits. Keep these tracked authoring files.
- The old generator now references the same independent slot tables and stable prefab identities. Its accepted-work guard remains in place. **The legacy whole-environment rebuild was not run and must not be used on this scene.**
- A second complete focused apply produced identical site inventory; frontage refresh changed **zero** color entries. Save/reload and repeat checks both passed with zero failures. Batched world vertices exactly match source transforms for every architectural material.

## Evidence

[Annotated overview](annotated-overview.svg) · [Annotated road comparison](annotated-road.svg) · [Full comparison gallery](review.html).

Actual Unity camera renders, 1920×1080, identical before/after camera transforms: full overview; three overlapping overhead sectors; six road-level views (west, center and east, each facing both directions). Overhead north is up. Labels in the annotated overview identify prefab types, rather than claiming new businesses. All after views were visually inspected. Raw PNGs remain alongside the annotations.

## Actual validation

Unity 6000.6.1f1, Windows Editor, unchanged vehicle settings, 0.02 s fixed step. `after-validation.txt`, `reload-validation.txt` and `repeat-validation.txt` each report **zero failures**:

- 22 businesses, 11 on each side; nearest opposing center stagger 14.35 m.
- Matching opposite storefront footprint intervals have at least **58.60 m longitudinal separation**; no matching stores directly opposite.
- Minimum same-row building footprint gap **21.40 m**; no foundation overlaps. Frontage widths plus transition margins fit within these gaps.
- Foundation bottoms have no positive sampled ground gap; minimum foundation-to-road-center distance **19.56 m**.
- No solid obstacles in sampled central driveway corridors. Signs and dedicated prefab colliders remain under their sites; source renderers are disabled and all 11 architectural batches match their world vertices.
- House hierarchies, terrain shape/marking data and all 18 breakable placements match the baseline; only nine local tree removals differ. No nearby breakable was moved or affected, so the prior impact/restoration stress suite was not repeated. Race restart was exercised in the smoke test.

Ordinary-frame **virtual Gamepad** pursuit drove the full commercial road in both directions. Vehicle pose and velocity were set only at the start of each run (30 m/s initial speed); no per-frame correction or manual physics stepping. Target cruise 32 m/s; braking region target 13 m/s; an excursion onto the south shoulder followed by re-entry in each direction. Road bends were followed naturally. This is automated driving, not a human/physical-controller claim.

| Result | Eastbound | Westbound |
|---|---:|---:|
| Complete stretch | PASS, 30.56 s | PASS, 30.50 s |
| Peak speed | 32.00 m/s | 32.01 m/s |
| Braking minimum | 12.66 m/s | 12.67 m/s |
| Minimum upright dot | 0.998 | 0.998 |
| Maximum shoulder offset | 11.29 m | 11.32 m |
| Final road-center error | 1.83 m | 1.67 m |
| Non-terrain contacts | 0 | 0 |

`driving.txt` contains exact settings and endpoints. Six final flow smoke checks passed: restart/locked countdown, countdown pause, frozen countdown, resumed countdown reaching unlocked Racing, racing pause, racing resume. The first follow-up countdown sample failed because it did not wait for simulation advancement in the unfocused Editor; that attempt remains in `flow-smoke.txt`. The corrected test explicitly enabled background simulation and waited for Racing (`flow-smoke-final.txt`). No production game code was changed to pass it.

All test activity used isolated `Docs/CR019/test-save`; no player records or settings were written. The scene's production save paths and accepted race/UI/input/audio/vehicle code remain unchanged. No full-race, results, persistence or physical-controller suite was repeated because those systems were not modified.

## Performance and limitations

Same fixed commercial camera, forest enabled, 240 ordinary Editor frames after 45 warmup, **734×293, VSync 1, 60 fps cap**:

| | Before | After |
|---|---:|---:|
| Median Editor interval | 16.74 ms | 16.74 ms |
| p95 | 20.21 ms | 20.83 ms |
| Scene renderers | 885 | 886 |

Driving intervals: east median/p95 16.65/21.62 ms (1,753 samples), west 16.70/20.73 ms (1,746 samples). The low-resolution Editor measurements include host/editor overhead and a frame cap; they do **not** establish standalone/GPU parity or performance at 1080p. The +1 renderer is one additional local entrance-step source; sources are disabled after batching. Architectural material batch count remains 11.

Compilation succeeded. Existing editor code emits obsolete `FindObjectsSortMode` warnings on recompilation; new validation uses the current overload. Final saved reload: not compiling, no compilation failure, **Console 0 errors / 0 warnings** (`console-final.json`). Earlier Pipeline connection/timeouts, the corrected row-naming preflight exception, and prior historical messages are archived in `console-history.json` / `console-smoke.json`; Console was cleared after archiving to check the final state. Several long commands exceeded the bridge's 5 s response limit while their file reports completed; reports and saved scene state were checked before retrying.

`git diff --check` reports Unity-serialized empty prefab `value:` lines and intentional Markdown hard-break spaces. Scene YAML was not hand-edited to remove Unity's formatting. The complete changed-file manifest is `FILES_CHANGED.txt`.

## Dan's review checklist

- Drive both directions: approve the independent storefront order, clusters and wider gaps.
- Check signs, foundations, paving and driveway/shoulder access at the moved sites.
- Approve the restrained colors and preserved woodland character.

Leave CR-019 and this Phase 8 batch awaiting that review. CR-013 and CR-018 remain optional deferred backlog.
