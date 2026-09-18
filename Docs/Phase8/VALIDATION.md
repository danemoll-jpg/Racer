# Phase 8 combined visual/performance delivery

Status: implemented, awaiting Dan's review; not accepted. CR-019 is accepted/closed following Dan's report. CR-013 photo-based homes and CR-018 exact placement remain deferred. No Phase 9 work.

Safety checkpoint: `c17fd0afa2c77512184066db036702250d7450a5`. Initial staging failed to create `.git/index.lock` with permission denied. The authorized elevated staging/commit retry succeeded before modifications.

Scene: `Assets/Scenes/StreetLoopGreybox.unity`. Fresh Windows development review build: `Builds/Phase8/Racer.exe`. Separate current-scene baseline: `Builds/Phase8Baseline/Racer.exe`; includes accepted CR-019 and identical benchmark bootstrap. Builds are local, Git-ignored artifacts.

## Changes and scope decisions

- Replace the constantly visible block car body/cabin with original low-poly chamfered bodywork, tapered smoked glazing, bumpers, grille, lamps and plate. Same car, wheels, main collision envelope, suspension, tuning, camera, controls and audio. Five small material-group meshes; no additional vehicle or gameplay feature. Lamps are decorative, not new realtime lights. Wheel animation remains outside this pass.
- Both terrain shaders share world-space asphalt grain, filtered out as it becomes subpixel, and a restrained darker road value. Existing shoulder gradients, markings, terrain vertices/normals/colors, hills and access remain intact. Shared world coordinates avoid texture seams at tile boundaries.
- Ground now receives the existing sun's soft shadows, grounding car and architecture. The light and saved URP settings remain unchanged: 40 m shadow distance, 2048 shadow map, four cascades, soft shadows, 2x MSAA, render scale 1. Shadow attenuation fades at distance. No added forest shadow casters; the dense forest uses its existing cheap illumination with slightly stronger directional crown shading and subdued undersides. Forest shaders bypass road grain/shadow sampling.
- World TextMesh lettering now uses depth-tested URP font rendering. This removes distant text fragments visible through hills in baseline road/forest images. Direction arrows are aligned to sampled terrain normals with a 4.5 cm visual clearance. Gate roots, course validation, storefronts and prop colliders are unchanged. Physical occlusion of text by geometry is now respected.
- Existing Phase 6 vegetation already has three patch-distributed crown forms and individual height, width, rotation and tint variation. Preserve that work and every remaining tree. No new LOD/culling threshold, collision culling, forest thinning, house/yard placement or commercial rearrangement.
- Existing daytime light is retained. Optional post-processing is deferred: surface/text issues have direct fixes, the approved clear daytime presentation is retained, and the player still has legacy unused post-process shader warnings. No day/night system or cinematic stack.
- Homes, storefront architecture and roads reuse accepted work. No broad asset replacement, legacy environment rebuild, new shortcut/jump, stunt scoring or extra track.

Focused authoring: `Racer > Phase 8 > Apply focused visual polish` (`Phase8Polish`). Mesh/material paths are reused on reapply. `Phase8Review` handles deterministic camera captures, route data and review builds. `Phase8BenchmarkBoot` is development-only and activated solely by `-phase8Benchmark`; it uses production countdown and isolated storage. No benchmark components are saved in the scene.

## Matched images

Identical car/camera positions, FOV 65, both 1920x1080 and 1280x720. Camera renders establish visual differences, not standalone or moving-camera performance.

| View | Before | After |
|---|---|---|
| Neighborhood road | [Before](before-road-1920.png) | [After](after-road-1920.png) |
| Commercial street | [Before](before-commercial-1920.png) | [After](after-commercial-1920.png) |
| Wooded road | [Before](before-forest-1920.png) | [After](after-forest-1920.png) |
| Shortcut/rejoin | [Before](before-shortcut-1920.png) | [After](after-shortcut-1920.png) |
| Jump approach | [Before](before-jump-1920.png) | [After](after-jump-1920.png) |

The matching `*-1280.png` files retain the 720p comparisons. Initial car lamp positions were corrected after visual inspection; delivered captures show exposed lamps and bumpers.

## Preservation and integrated validation

[Preservation](preservation.txt): exact before/after match for all 100 terrain shape/normal/index/color fingerprints, all 12,418 collider snapshots, and all 47 building site positions/rotations. All 11,986 trees remain. No House #1; compact yard and 18 breakable props retained. [Geometry](geometry.txt) checks terrain height/normal/color/marking seams, visible/collision mesh agreement, grounding, and renderer feature consistency. The first geometry comparison falsely failed because the baseline was sorted and the current inventory was not; corrected comparison sorts both inventories. No buildings were moved to satisfy the test.

[Mixed full race](mixed-laps.txt): three valid laps via shortcut / normal / shortcut, 551.52 simulated seconds, mean 25.42 m/s, peak 39.71 m/s, maximum path error 3.23 m, minimum upright .880. Existing follower, manually stepped PhysX and virtual motor commands. It traverses the established normal/jump/shortcut course; it is not an ordinary-frame or human-driven full race.

[Ordinary-frame jump](jump-ordinary-result.txt): started at rest 240 m before the ramp; 31.77 m/s takeoff, 1.73 s airborne, landed, minimum upright .981. No injected velocity or manual physics stepping. [Raw traversal](jump-realtime.csv).

[Ordinary-frame impact](impact.txt): actual virtual-Gamepad mailbox contact at an initial 6 m/s; broke once, remained upright (.981 minimum), restored through production race restart. Wider previously accepted prop stress matrices were not repeated. Fresh standalone flow validation also covers contact dispatch, overlap-deferred restoration and restart.

[Forest-edge shoulder crossing](shoulder.csv): 20.8 m traversal, final route index 13/13, 3.88 m/s peak, .998 minimum upright, 1.26 m maximum path error. This verifies the retained clear crossing, not every forest gap; the historical obstructed western-forest follower route is not reclassified as a pass.

All input is virtual. No physical-controller or subjective listening coverage is inferred. Legitimate records/settings are not overwritten: player invocations use `-racerTestSave`; harnesses select isolated directories. Mixed-lap testing uses `Docs/Phase7/Phase8Isolated` because the existing guard explicitly requires storage beneath Phase7. Historical mixed-lap reports are restored after copying this run's result.

## Performance method and limitations

Unity 6000.6.1f1 development players, D3D11, Intel i7-9700, GTX 1660 Ti, 65,341 MB system RAM. PC quality, 1920x1080 windowed, 2x MSAA, scale 1, VSync 0, unlimited target, fixed timestep .02 s. Same `routes.json`, chase camera and target speeds: road/commercial/forest 18 m/s, shortcut 14 m/s. Each route has 3 s warmup + 21 s sampling, repeated twice. Road sections cover about 423 m; short shortcut reaches its endpoint, so its tail includes stationary frames facing dense woodland. Route-by-route distributions must not be interpreted as all-moving forest timings.

Main-thread recorder is valid but includes waits; render-thread recorder is unavailable. Physics is small in the sampled views. Draw-call recorder returns zero and is not used as evidence; nonzero triangle counts and visible player inspection establish rendering. Renderer counts alone are not an optimization verdict. No reliable GPU timing or universal frame-rate promise.

The first `before.csv` run was obscured by a Windows Firewall prompt and overlapped inspection work. It is retained but excluded from comparison. `before-visible.csv` is the visibly verified replacement. Its first road section includes a visibility screenshot; subsequent sections/repetition do not. Host background applications remain uncontrolled. Editor traversal measurements include Editor/tool overhead and are not standalone performance claims.

### Comparable results

Clean repeats use the same routes/settings with 3 s warmup + 12 s measured per section, two repetitions (routes-repeat.json). Both players were foregrounded; no screenshots or heavy Editor validation ran during these repeats. Each cell lists repetition 1 / 2, milliseconds.

| View | Before median | After median | Before p95 | After p95 |
|---|---:|---:|---:|---:|
| Road | 1.166 / 1.178 | 3.356 / 3.449 | 6.949 / 6.730 | 8.450 / 8.670 |
| Commercial | 1.491 / 1.185 | 3.504 / 3.386 | 7.595 / 6.047 | 8.739 / 8.343 |
| Forest | 1.305 / 1.246 | 3.338 / 3.235 | 6.800 / 6.335 | 8.593 / 8.117 |
| Shortcut | 1.439 / 1.213 | 3.488 / 3.264 | 8.279 / 6.106 | 8.795 / 8.290 |

[Before raw distributions](before-repeat.csv), [after raw distributions](after-repeat.csv). Before p99 spans 9.680–12.750 ms, maximum 92.713 ms, 16 frames over 33 ms / 48,998 sampled frames. After p99 spans 9.664–10.112 ms, maximum 13.288 ms, zero frames over 33 ms / 24,057 sampled frames. Different sample counts follow different uncapped frame rates, with equal measured time.

The clean repeat shows roughly 2 ms higher typical frame time and generally higher p95 after polish. It does **not** establish a speedup. Ground shadow reception and surface shading add work; their isolated GPU cost was not measured. Final player allocation settles at about 299.6 MB versus 299.1 MB before. Physics p95 is approximately .2 ms, too small to explain the broad frame-time shifts.

The longer first visible pair ([before](before-visible.csv), [after](after-visible.csv)) shows the opposite typical-frame trend: baseline medians 3.031–3.417 ms, versus about 1–1.4 ms after. Before maximum 14.130 ms / zero >33 ms frames across 46,027 samples; after maximum 211.745 ms / 20 >33 ms frames across 92,505 samples. Visibility captures occurred in road sections of these runs. This contradictory pacing and spikes in either build across runs show substantial host/presentation variability; they do not prove the shader change caused or cured intermittent stalls.

No defensible render/GPU bottleneck was isolated with available counters. Retain accepted batching, coverage, shadow settings and collision availability; adding LOD/culling or reducing forest density would introduce risk without measured justification. Dan should assess smoothness on his normal driving setup. No stable parity or universal FPS guarantee is claimed.

### Standalone and final state

The actual fresh Windows player was visibly inspected during countdown and ordinary moving-camera driving at 1920x1080. Road detail, car bodywork, shadows and world text render correctly. The same build also produced inspected 1280x720 HUD/menu captures.

- [1080p clean run](validation-launch1.txt): 37 checks passed, zero failures.
- [1080p independent second launch](Player1080Clean/validation.txt): 37 checks passed, zero failures; loaded previous isolated best lap 40, race 120 and master volume .7.
- [720p run](Player720/validation.txt): 37 checks passed, zero failures.
- Checks cover start/countdown, ordinary virtual driving, pause/resume, reset/restart, ordered synthetic finish/results, invalid race events, settings/navigation, isolated persistence/corrupt-save defaults, prop restoration and audio-source stability. Synthetic finishes are distinct from the separately recorded full mixed race.
- [Saved scene reload](save-reload.json): reopened in Edit mode, not dirty, 11,986 trees, no saved benchmark component.
- [Final Console](console-final.json): current ground-truth counts zero errors/warnings, compilation not failed or active. The bridge retains historical entries after the Editor Console is cleared; these include documented long-operation timeouts, not new compiler failures. History is retained in console-history.json.

## Build/runtime warnings versus tooling

`build-before.txt`: Succeeded, 0 errors / 2 warnings. `build-after.txt`: Succeeded, one bridge timeout counted as an error / 2 warnings. The actual warnings are future collision-prebaking requirements on 103 meshes and the absent optional RuntimePipelineConfig. No runtime Pipeline bridge is needed by the game.

Player logs retain unused stripped Gaussian/Bokeh depth-of-field and Panini shader messages. No such effects are enabled. Compilation succeeded. Long capture, inventory, test and build calls can exceed the Pipeline server's five-second limit while continuing; evidence files and actual state are checked separately. Deferred build scheduling was not treated as completion; a direct build call produced the final report.

The first standalone flow attempt (`Player1080`) records one settings-change/focus assertion failure while the Firewall prompt was present. Retain that result; subsequent clean 1080p runs and the 720p run pass all 37 checks. The prompt is handled manually by Dan, not by changing security settings.
Final diff check flags only Unity-serialized empty prefab override values (trailing spaces in the scene). These generated YAML values are retained as saved by Unity; no hand-edit of the scene was used to silence whitespace checks.
