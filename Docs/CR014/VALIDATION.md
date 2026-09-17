# CR-014 — expanded explorable woodland

**Awaiting Dan's review of coverage, driveability, and smoothness.** Phases 2–5 and the building batch remain accepted. CR-012 remains closed; CR-013 is optional backlog. Phase 6 is incomplete; Phase 7 has not started.

Safety checkpoint: `fcd82ea507732b35f5b025ae842d7fbd3a991636`. Initial staging failed with permission denied creating `.git/index.lock`; the authorized elevated retry succeeded. No project content changed before that checkpoint. The completion commit is reported in the task response.

Open `Assets/Scenes/StreetLoopGreybox.unity`.

## Coverage and authoring

- Retained every original **6,102** trunk. Added **5,795**, for **11,897** total. Stage 1 added 2,897 (8,999 total); stage 2 completed the layout.
- Expanded the broad central removed-subdivision land west of Dan's street, the woods east of the neighborhood, and woodland south of the connecting road/hairpin. The annotated reference and corrected memory notes guided these areas. Broad noise patches leave irregular glades; this is not uniform scattering across every yard and roadside.
- New mature trees are 15–20 m tall with crowns 1.35 times the existing radius rule, bounded by the same protected canopy clearances. Closest new-to-any-trunk spacing is at least **6.8 m**; new trunks are **0.75 m** wide. Their canopy overlaps overhead while the floor retains room to maneuver. Existing close original-tree clusters remain natural obstacles.
- Kept **90 spatial 160 m mesh batches**, one shared vertex-color material, three shared crown shapes and one trunk mesh. Forest triangles: **416,952 → 616,128 → 810,312**. Final serialized vegetation meshes total approximately **109 MiB**. No leaf colliders, understory clutter, distant collision deactivation, new shortcuts, jumps, or checkpoint exceptions.
- Fixed a discovered authoring bug: `EditorUtility.CopySerialized` could leave old mesh GPU buffers visible after a refresh. The mesh saver now explicitly replaces vertex/index/normal/color buffers and uploads them. Stage-1 captures before that fix and `editor-stage1.csv` are superseded by `stage1-verified-*` and `editor-stage1-verified.csv`.
- `CR014Woodland.Plan()` creates a deterministic saved placement plan before coverage changes. It refuses to overwrite a plan after CR-014 trunks exist. `Apply(1)`/`Apply(2)` only add that layout and refresh forest visuals. `Phase6Vegetation.Refresh()` remains the repeatable vegetation-only refresh. Never run the old whole-environment rebuild.
- Placement exclusions now reserve accepted building sites and House 3's downhill sightline as well as Dan's expanded yard, roads, shoulders, and shortcut. Existing whole-rebuild protection remains active. Test trajectories do **not** reserve or cut paths into the forest.

Projected tree footprints were sampled from actual mesh triangles on the same 2 m horizontal grid. These are approximate canopy-area measurements, not a claim that every point beneath a crown is traversable.

| Region | Before | Stage 1 | Final |
|---|---:|---:|---:|
| Central removed subdivisions | 10.77% | 27.99% | **43.76%** |
| Eastern woods | 8.97% | 28.31% | **45.55%** |
| Southern woods | 9.18% | 25.55% | **40.74%** |
| Entire supported 1,600 × 1,600 m terrain | 6.73% | 16.13% | **24.75%** |

The whole-terrain denominator includes roads, protected sites, and the large unwooded northern terrain margin. Raw measurements: [before](coverage-before.txt), [stage 1](coverage-stage1.txt), [final](coverage-after.txt).

## Matched views

Same camera positions, orientations, lighting, and 1440×900 capture target. These are static coverage comparisons, not screenshots proving a driving run.

| View | Before | After |
|---|---|---|
| Entire neighborhood | [Before](before-0.png) | [After](after-0.png) |
| Road near protected homes | [Before](before-1.png) | [After](after-1.png) |
| Central forest interior | [Before](before-2.png) | [After](after-2.png) |
| Southwestern forest | [Before](before-3.png) | [After](after-3.png) |
| Eastern forest | [Before](before-4.png) | [After](after-4.png) |
| Expanded yard and nearby homes | [Before](before-5.png) | [After](after-5.png) |

## Preservation and collision

- [Saved-scene comparison](scene-preservation.txt): only the forest parent's child list changed among retained records. The 360 removed records are the old 90 forest renderer objects. All other retained records match the safety checkpoint, including all original trunks, buildings/labels/transforms, terrain, road support, shortcut, jump, vehicle, camera, controls, reset, race, and HUD objects.
- [Trunk verification](trunk-alignment.txt): **95,176** unique collider corners and **285,528** baked bark vertices checked at 1 mm rounding; no missing or extra corners. Every trunk stays solid at every distance.
- [Clearance checks](clearances.txt): zero trunks in the expanded yard, zero new spacing violations, zero unsupported new tree bases, and zero new trees in reserved sites. Minimum conservative trunk-surface road/shortcut clearance: **22.30 / 16.72 m**. **1,458** vehicle-volume corridor probes found no forest obstacles.
- [Building/yard/terrain checks](building-yard-terrain.txt): all 47 surviving site names/transforms match accepted placements; 1,579 yard support/obstacle probes pass; terrain seams match and every terrain tile uses the same visible and collision mesh. No terrain positions were changed.

## Actual driving checks

All automated driving used the approved scene car and **virtual input**, not a physical controller.

- **Ordinary-frame exploration:** three separate 40-second drives through central, southwestern, and eastern woods covered **150.4 / 150.2 / 150.9 m**. Peak speeds **3.92 / 4.04 / 3.89 m/s**, maximum path error **0.89 / 0.98 / 1.05 m**, minimum upright **0.996 / 0.996 / 0.988**. These use normal Update/FixedUpdate and smoother test trajectories through existing gaps, with no scenery edits. [Results](exploration.csv), [trajectories](exploration-routes.json). The old CSV header says 12 measured seconds, but these runs used **40 total seconds, 3 warmup + 37 measured**; later tooling corrects the header.
- The original sharp-grid 7 m/s central profile trajectory struck a solid trunk after about 12 m; the eastern full-layout trajectory stopped after about 54 m. These are **not traversal passes** and their moving-view timings are not directly comparable with the uninterrupted baseline. The slower checks above resolve access along smoother routes; they do not prove every gap is navigable.
- **Tree contact, braking, reverse, turn and reset:** one new trunk contact registered; the car stopped, reversed to −8.67 m/s, moved 36.52 m away, turned, and reset using the virtual north/Y button. Minimum upright 0.981. HUD reported the reset and invalidated the active lap. [Detailed states](maneuvers.txt).
- **Forest/road crossing:** 118 m from western woodland across supported shoulders and the road to the outer shoulder; maximum path error 0.93 m, minimum upright 0.998. [Results](reentry.csv). Existing regression checks additionally verify normal shoulder recovery and return to the road path.
- **Ordinary-frame jump from rest:** 240 m acceleration approach, takeoff **31.79 m/s**, **1.74 s** flight, landed upright, minimum upright **0.981**; 647.9 m total road traversal, peak 32.24 m/s. No injected velocity or manual physics stepping. [Result](jump-ordinary-result.txt), [run](jump-realtime.csv).
- **Ordinary-frame shortcut:** reached re-entry in 9.52 game seconds, peak 24.31 m/s, maximum path error 2.88 m, upright 1.000; chase camera and HUD active. This test injects 24 m/s initial velocity, then uses normal frames and virtual Gamepad input. [Result](shortcut-ordinary.txt).
- **Manually stepped regression:** three valid mixed laps, **551.52 s**, mean **25.42 m/s**, peak **39.71 m/s**. Shortcut support/recovery, jump/bypass matrix, and **37 race/input/HUD/reset/restart checks** passed. [Laps](MIXED_LAPS.txt), [recovery](RECOVERY.txt), [jump](JUMP_TESTS.txt), [race](RACE_REGRESSION.txt). Existing −3° high-speed jump limits remain; they were not introduced or fixed by CR-014.

## Performance methodology and limitations

Unity **6000.6.1f1**, quality **PC**, GTX **1660 Ti** (5,966 MB VRAM reported by player), Intel **i7-9700 @ 3.00 GHz**, **65,341 MB** system RAM, Direct3D 11, driver **32.0.16.1062**. vSync **0**, target frame rate **−1**, fixed step **0.02 s**, quality shadow distance **40 m**. No accepted quality/tuning changes were made for the coverage comparison.

The current normal Editor Game view was **734×293**. Baseline, verified stage 1, and full coverage were each measured on the same five routes/views: 3 warmup + 12 measured seconds per section. A separate **1440×900 windowed development player** provides a representative larger render workload. Initial pose is set before each section; the road and forest motion then use normal physics frames and virtual Gamepad controls. Host load was not controlled, and CPU markers include waits. Render-thread and GPU timings were not available; the draw-call counter returned zero and is not used as evidence. Physics p95 is more informative than its median because many rendering frames contain no fixed physics step.

The first hidden-player sample suppressed rendering and is invalid. The first visible-player sample failed during URP initialization and is also invalid. Their CSVs are explicitly named `invalid-*` and excluded. The working benchmark build temporarily omits the **already-disabled SSAO feature** to avoid an installed URP 17.6 resource-stripping initialization fault, then restores the Editor renderer configuration. No active visual feature is removed. Stripped post-processing shader warnings remain a player limitation. Builds and logs are local ignored artifacts under `Builds/` and `Logs/`.

### Comparable results

Values are milliseconds. Each standalone cell contains **run 1 / run 2**, not a pooled percentile. The following scenarios retained comparable camera trajectories (road/southwestern driving) or the identical fixed camera (dense view):

| 1440×900 standalone scenario | Before median | Final median | Before p95 | Final p95 |
|---|---:|---:|---:|---:|
| Wooded road | 6.92 / 11.25 | 4.34 / 11.10 | 15.38 / 15.52 | 12.35 / 16.64 |
| Southwestern forest driving | 5.23 / 12.61 | 3.22 / 12.53 | 11.91 / 19.80 | 9.37 / 21.57 |
| Fixed dense-forest view | 3.33 / 8.83 | 13.87 / 7.80 | 7.68 / 19.68 | 27.58 / 14.41 |

Final p99 in those scenarios ranges **11.32–31.16 ms**. Maximum individual final frames include **83.75 ms** on the road and **80.77 ms** in the fixed view; the central stopped-route profile includes an **89.95 ms** spike. The worst original standalone frame was **50.84 ms**. Spike causes were not conclusively isolated. No frame-rate parity or speedup is claimed.

Raw repeated data: [standalone before](standalone-before.csv), [standalone final](standalone-after.csv). Central/eastern moving-view results remain in these files but are excluded from the comparison table because the 7 m/s automated routes hit trees and stopped at different places. The successful slower exploration tests are separate.

Normal **734×293 Editor** comparison (median / p95):

| Scenario | Before | Verified stage 1 | Full forest |
|---|---:|---:|---:|
| Wooded road | 9.30 / 35.60 | 5.51 / 15.89 | 14.61 / 27.36 |
| Southwestern forest driving | 17.19 / 30.80 | 7.91 / 15.24 | 18.68 / 30.03 |
| Fixed dense view | 18.30 / 31.69 | 8.97 / 15.87 | 19.57 / 32.96 |

[Editor before](editor-before.csv), [verified stage 1](editor-stage1-verified.csv), [full forest](editor-after.csv). These samples include substantial Editor/host overhead; the full-forest southwestern run had a **128.47 ms** maximum frame. The baseline road had a **158.59 ms** maximum. The stage samples alone do not establish a monotonic vegetation cost.

### Diagnosis and tradeoffs

A subsequent identical fixed-view **1440×900 standalone** comparison disabled **only forest renderers**, leaving all 11,897 colliders enabled:

| Condition | Median run 1 / 2 | p95 run 1 / 2 | Physics p95 run 1 / 2 |
|---|---:|---:|---:|
| Forest rendering off | 3.31 / 4.09 | 7.66 / 8.03 | 0.164 / 0.179 |
| Forest rendering restored | 4.26 / 4.48 | 8.38 / 8.56 | 0.161 / 0.161 |

The restored runs were **0.38–0.95 ms** higher in median than corresponding off samples, with **575,040** more submitted triangles in this view. This supports a modest forest-rendering cost in that comparison; it does not isolate GPU versus CPU submission time. Physics is not the demonstrated bottleneck. Runs were separate sequential player processes, so host variability still applies. [Off](standalone-render-off.csv), [restored](standalone-render-on.csv).

The *unchanged final scene* measured **13.87 / 7.80 ms**, then later **4.26 / 4.48 ms** at the same fixed camera. That is strong evidence of host/runtime-state variability rather than an optimization: **no forest geometry was reduced between those samples**. It does not prove every spike is host-caused.

Unity-reported allocated player memory rose approximately **253 → 292 MiB** (**+39 MiB**). Reserved memory reached **800 MiB** during the final multi-route run versus **544 MiB** before; the final fixed-view diagnostic reserved 544 MiB. Reserved memory is allocator capacity, not an exact foliage allocation or OS working set. Editor allocation rose approximately **1,529 → 1,645 MiB**; that includes authoring/test/editor state and is not a standalone memory budget.

Retained the existing shared indexed low-poly assets, one material, spatial batches and ordinary frustum culling. The forest shader has no ShadowCaster pass, so reducing forest shadow distance would not address this measured cost. No collision activation scheme is justified by the low measured physics cost. Full coverage stays in place. If Dan still finds dense areas unsmooth, next options are a controlled GPU capture, tested distant-canopy LOD, and a measured batch-size/instancing comparison; none is represented as an already-proven improvement. There is no promised frame-rate target.

## Save, compilation, authoring limitations and review

[Reload](save-reload.txt) confirms the saved scene outside Play mode, not dirty, with **11,897 enabled colliders and 90 enabled forest renderers**. Compilation succeeded. The [verified final build](build-verified.txt) succeeded with **0 errors / 1 warning** (Pipeline runtime configuration is absent, so remote Pipeline control is disabled in the player). The final live Console reported **0 errors / 1 warning**, with no compilation failure; see [verification audit](console-build-verification.json). Pipeline's five-second request limit produced earlier tooling timeouts even when operations completed; saved reports verified completion. A renderer JSON restoration attempt also raised editor serialization errors; the final build helper restores the original feature-reference list directly instead, and the verified build confirms that fix. Historical diagnostics are preserved rather than reported as game errors.

The build pipeline serialized a default Bloom filter (Bloom intensity remains zero) and the URP runtime-settings cache. These are recorded asset-normalization changes, not intentional lighting/quality edits. The PC render settings and accepted renderer feature list are restored. Standalone release packaging beyond these benchmark builds remains unvalidated.

Changed file groups: `Assets/Scenes/StreetLoopGreybox.unity`; forest meshes in `Assets/Vegetation/Phase6/`; `Assets/Scripts/Editor/Phase6Vegetation.cs`; new `CR014Woodland.cs`, `CR014Driving.cs`, and opt-in `Assets/Scripts/WoodlandBenchmark.cs` with Unity metadata; the two normalized URP settings assets above; `PROJECT_TODO.md`, the historical vegetation-report pointer, and evidence/tools in `Docs/CR014/`. No vehicle/race/runtime gameplay scripts, terrain meshes, buildings, packages, or accepted gameplay assets were edited.

Remaining limitations: no physical-controller testing; no exhaustive proof that every woodland gap or slope is driveable; stylized crowns and plain forest floor; retained distant terrain speckling; retained fast angled-jump limits; unresolved intermittent frame spikes. Dan's visual and smoothness review remains required.

Suggested exploration: central woods west of Dan's street (tested around **x −300, z 260**), southwestern interior (**x −430, z −200**), and eastern woods (**x 590, z 100**). Test coordinates are guidance, not named gameplay routes. Enter from ordinary shoulders and use speeds suited to visibility and trunk spacing.

Dan's checklist:

- [ ] Judge coverage from the roads and while driving under the canopy.
- [ ] Explore several directions, turn, brake/reverse around trunks, and rejoin roads.
- [ ] Check the expanded yard/former House 1 clearing and retained Houses 2/3.
- [ ] Drive a normal lap, shortcut, jump/bypass; verify reset/restart and camera/HUD.
- [ ] Try the physical controller and assess smoothness at your normal window settings.
- [ ] Approve CR-014/vegetation or request local changes. Earlier approvals remain intact; Phase 6 stays incomplete.
