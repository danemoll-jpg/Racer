# Phase 6 remaining environment delivery

Status: **implemented, awaiting Dan's review**. This does not accept Phase 6 or start Phase 7. Dan's combined CR-016/fences/CR-017 approval remains recorded: CR-016 accepted for now, CR-017 closed, CR-013 and CR-018 optional/deferred. Every house and yard remains fixed.

Safety checkpoint: `442d7af0047a73625c7a4b4a29cff2d6bd74f74c`. Initial Git staging failed creating `.git/index.lock`; the authorized elevated retry succeeded before modifications. Completion commit is reported in the task response.

Open `Assets/Scenes/StreetLoopGreybox.unity`. Visible Windows review player: `Builds/Phase6Environment/Racer.exe` (generated locally, ignored by Git).

## Integrated changes

- **Road markings:** terrain-surface shader with anti-aliased three-metre muted yellow dashes at 18-metre spacing. Main commercial road also gets restrained off-white edge lines at ±3.65 m. Hilly neighborhood has center dashes only; western and southern connecting woodland roads retain unmarked rural shoulders. Paint uses the existing surface triangles and interpolated route coordinates, not floating decals or collision geometry. 22 existing terrain tiles receive paint coordinates and the shared material. No additional road meshes or renderers.
- **Selective roadside treatment:** ten central commercial frontages (approximately X −260 to +80) get short flush paving strips beside their entrances and softly blended gravel edges. These are colored into the supporting terrain, not raised slabs. Raised curbs and continuous residential sidewalks are deliberately unnecessary: they would constrain shoulders/driveways and add urban character. No driveway, shortcut, bypass, jump or forest barrier is added.
- **Transitions:** 737 local frontage color entries blend paving, gravel and grass; 176 lie in paving cores. Road/hill/valley geometry remains intact. Foundation and terrain-boundary checks determine whether any physical repair is needed; no speculative resculpting or house adjustment.
- **Lighting:** one existing directional sun, intensity 1.7→1.25, warm-neutral color, elevation 50→58°, yaw −35→−28°. Flat ambient changes to sky/equator/ground ambient. The previously fixed-brightness terrain/forest shader now responds to ambient and sun color/direction, improving consistency with buildings. Existing far clip, camera behavior, 2× MSAA, 40 m shadow setting, render scale and driving systems stay unchanged. No day/night system, extra lights, bloom or new effects. The lightweight custom ground/forest shader still does not receive/cast real-time shadows; this is a deliberate retained limitation.
- **Audio:** one 31-second filtered-leaf/wind loop and three 1.7-second synthetic bird phrases. Two runtime sources for the whole scene; bird calls 12–26 seconds apart with pitch variation and spatial placement. Broad commercial/woodland volume transitions are smoothed over time. Restarts and vehicle resets preserve playback. Original deterministic synthesis, no downloads or third-party samples; generated WAVs dedicated under CC0-1.0. Source: `Assets/Environment/Phase6/AUDIO_SOURCES.md`; generator: `Phase6Environment.GenerateAudio`.

## Matched views

Static 1440×900 camera renders use the same positions, orientations and FOV. They show environment changes; they do not independently prove driving or performance.

| View | Before | After |
|---|---|---|
| Main commercial road | [Before](before_commercial_drive.png) | [After](after_commercial_drive.png) |
| Wooded neighborhood hill | [Before](before_neighborhood_drive.png) | [After](after_neighborhood_drive.png) |
| Commercial frontage | [Before](before_business.png) | [After](after_business.png) |
| House and compact yard | [Before](before_dan.png) | [After](after_dan.png) |
| Yard overview | [Before](before_yard.png) | [After](after_yard.png) |

Additional after captures show ordinary-frame driving. The standalone commercial chase view was visually inspected using the Windows Computer Use tool.

## Preservation and gameplay

[Geometry fingerprints](preservation.txt): all 100 terrain meshes retain identical positions, normals and indices; all 12,426 collider snapshots match before/after, including bounds, transforms, enabled/trigger state. No new collision surfaces. The existing 11,995 trees, 18 breakable assemblies and current house placements are retained. Only local surface colors/paint attributes and lighting/audio authoring change.

[Mixed laps](MIXED_LAPS.txt): shortcut/normal/shortcut, three completed laps, 551.52 s simulated, mean 25.42 m/s, peak 39.71, max path error 3.23 m, minimum upright 0.880. [Race regression](RACE_REGRESSION.txt): zero failures, including checkpoint validity, timing, HUD, virtual input, restart/reset and three additional slow PhysX laps. [Shortcut recovery](RECOVERY.txt): four failed-route resets and skip/repeat/wrong-way cases pass. These use virtual commands and manually stepped PhysX; they are not physical-controller or ordinary-frame performance measurements.

[Jump/bypass/shoulders](JUMP_TESTS.txt): accepted cases pass; bypass at 6/30 m/s stays grounded and upright, four shoulder traversals remain supported. Previous 42/46 m/s negative-angle approaches retain LIMIT results (8.46/9.33 m lateral deviation); this batch does not retune them.

[Prop impacts](impacts.txt): all 20 ordinary-frame virtual-Gamepad cases at 3/30 m/s, centered/glancing across five prop types pass. Consecutive fences, eight contact bursts, cleanup, five race restarts, ordinary vehicle reset and overlap-deferred restoration pass. Initial pose/velocity injected for impact approaches. No physical controller tested. Terrain contributes vertical motion at sloped approaches; trigger-only props add no collision solver impulse.

[Audio](audio.txt): no clipping in source clips, wind loops observed, race restart and vehicle reset preserve sample position, exactly two sources remain. Measured wind-source output peak 0.0331; largest observed per-frame volume change 0.00182. Two bird starts observed during the 38-second test. Waveform seam is smaller than normal adjacent-sample variation. This is objective playback/level testing, not speaker/headphone listening approval.

## Rendering/build limitations

The dormant SSAO reference is permanently removed from the saved PC renderer. Previous builds temporarily removed it because installed URP 17.6 could fail initializing stripped resources. This delivery uses the same renderer feature list in Editor and player; no hidden build-only feature switch. Build-generated URP shader-prefilter metadata is retained and documented as a cache update, not a quality change.

The development build succeeds. Its reported error entries are Pipeline five-second command timeouts during the long build, not compilation or player failures. Warnings concern future mesh collision prebaking and the absent optional RuntimePipelineConfig. Player logs still warn about stripped, unused depth-of-field/Panini postprocess shaders. No such effects are enabled by this batch. Actual visible rendering of road paint/lighting was verified; shader warnings remain a packaging limitation.

## Dan's review

- Drive a normal lap at usual display/controller settings; judge hills, bends, paint readability and daytime visibility.
- Visit the commercial frontages, cross shoulders, enter woodland and the unchanged house access.
- Try shortcut, jump and bypass; hit a mailbox/sign/fence, restart the race and reset the car.
- Listen for restrained wind/birds, smooth transitions and comfortable levels; report any stalls or visual defects.

## Additional ordinary-frame driving

[Local drives](local-driving.csv): reforested woodland 120.0 m, minimum upright 0.991; house access out/in 47.3/48.3 m, minimum upright 0.980/0.979. Peak speeds 3.98/4.54/4.51 m/s. The automatic follower overshot stopping endpoints by up to 7.49/6.38 m. These are route/access checks; their variable Editor timings include evidence work and are not standalone performance claims. [Moving woodland view](moving-woodland.png).

[Ordinary-frame jump](jump-ordinary-result.txt): takeoff 31.76 m/s, 1.72 s airborne, landed upright (minimum 0.981), starting from rest before the ramp. Virtual Gamepad; no injected velocity or manual physics stepping.

The longer western forest-to-road follower test did **not** complete: its first timed run covered 70.9 m and the continuation stalled against a retained `Tree trunk` near (−597,7,−317). [Obstruction record](reentry-obstruction.txt). All collider fingerprints are unchanged; this is an automated-route limitation through existing trees, not a claim that every forest gap is passable. A separate clear shoulder segment was tested independently. No trees or accepted corridors were altered to make the test pass.

## Matched standalone performance

Unity 6000.6.1f1 development players, Windows D3D11, i7-9700, GTX 1660 Ti, 1440x900 windowed, PC quality, 2x MSAA, render scale 1, vSync 0, unlimited target, 0.02 s physics. Same route data/chase camera and 18 m/s target; 3 s warmup + 21 s measured per section, two repetitions. Editor stays outside Play mode during standalone profiling. Baseline player is the prior CR-016/017 build (current safety checkpoint only added documentation); the after player contains this batch. No new runtime changes were made after the tested build.

The repeat pair runs sequentially with no screen capture, scene authoring, compilation or active Editor gameplay tests. Documentation/file reads continued; host background load is not controlled. Nonzero triangle counters confirm rendering. Draw-call counter is zero/unreliable; render-thread/GPU measurements unavailable. CPU markers include waits.

| Scenario | Before median ms (runs 1 / 2) | After median ms (1 / 2) | Before p95 ms | After p95 ms |
|---|---:|---:|---:|---:|
| Wooded road | 4.159 / 6.638 | 4.242 / 5.904 | 10.787 / 12.715 | 10.878 / 11.680 |
| Commercial | 6.506 / 6.676 | 6.793 / 4.865 | 11.763 / 12.677 | 12.580 / 11.697 |

[Repeat baseline](player-before-repeat.csv), [repeat after](player-after-repeat.csv). Maximum repeat frame: baseline 54.699 ms, after 33.470 ms. Frames over 33.33 ms: baseline 2, after 1, across all four sections. Loaded allocated memory approximately 292.5 to 295.7 MiB. Wooded drives covered 415–423 m with minimum upright 0.880; commercial about 424 m, upright 1.000. These results show no clear sustained regression within observed variability; they do not establish a speedup, controlled parity or guaranteed smoothness.

The [initial baseline](player-before.csv) and [initial after](player-after.csv) remain as raw evidence. The initial after run overlapped Windows screen-capture/activation work and had maxima up to 421.368 ms and wooded p95 66.113 ms. It is explicitly a disturbed run and is not silently omitted. Editor local drives also recorded stalls up to 576.7 ms and are not directly comparable to standalone timings.

The independent [clear shoulder crossing](shoulder.csv) covered 20.8 m across the road, reached final route index 13/13, minimum upright 0.998, maximum path error 1.24 m. This does not turn the obstructed longer forest route into a pass.

## Final status and remaining issues

All required Phase 6 implementation categories are addressed, **awaiting Dan's review**. No Phase 7. Accepted houses/yards, gameplay and props remain preserved. Open limitations: physical-controller testing; subjective lighting/audio/visual approval; retained fast angled-jump deviations; tree-obstructed automated western forest route; automatic access stopping overshoot; variable host/editor stalls; unused stripped postprocess warnings and future collision-prebaking warning. No unresolved new compiler or gameplay exception is established by these tests.

Focused authoring is guarded against cumulative reapplication. An intermediate frontage blend calculation was corrected from checkpoint color copies before final captures; temporary copies were removed. Some long CLI calls exceeded the command bridge's five-second limit while continuing successfully; completion files and live state were checked. A renderer inspection initially assumed an additional-camera component existed; the corrected read confirms postprocessing is disabled and there are no scene Volumes. Diagnostic history is retained in console-history.json, separately from the final clean Console snapshot.

Final [saved-scene reload](save-reload.json): correct scene, not dirty, outside Play mode, compilation complete without failure, 11,995 trees, 18 props, one ambience component with all four clips, zero test benchmarks. Final [live Console](console-final.json): 0 errors / 0 warnings after archiving diagnostic history. Existing authoring code emits obsolete-API warnings when compiled; those remain in the archive.

