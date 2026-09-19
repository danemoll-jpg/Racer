# CR-057–060 combined review

Status: implemented, awaiting Dan's review. Windows version: **0.9.0-review1**.

Safety checkpoint: `8fee1f5475e6c2b9ee0a9eb646cd2e61e40c4664`. The saved TODO changes were committed before implementation. Staging and commit each needed a successful elevated retry after a sandbox index-lock denial. No changes were discarded and no hooks, signing or history were bypassed.

## Water and preservation

The original lake surface stood about 3.5 m above its bed. The new terrain supports a nominal 0.65 m water depth and rises through the rendered shoreline. Inspection also found a nearly four-metre vertical transition in one metre beside the grid; a continuous bank now meets the preserved trail. Forest's legacy street-closure boundary formerly projected a widening plane through the lake. In Forest only, that boundary now protects the actual closed street corridor; the Street Loop boundary behavior is unchanged.

The shared motor samples the water surface against the vehicle's lower extent, with a vertical-depth limit and actual surface footprint. Fully immersed propulsion is 48% of normal, with 0.95/s horizontal resistance. Steering and traction remain available. Grounded water samples in `probe3/water.csv` average 5.33 m/s during full-throttle drive-out fixtures (maximum 7.03 m/s); this is a fixture measurement, not a universal speed cap. Dry controls are unchanged, and overflying the surface does not acquire resistance. Six pooled ripples per vehicle and brief synthetic entry/exit sounds provide feedback. Recovery clears their state immediately. AI uses the same motor and water feedback.

`probe3/checks.txt` records **86/86** checks: two vehicle profiles, four approaches at each of three water surfaces, reverse and drive-out, airborne samples, recovery, household occupancy, labels and bat trigger checks. These are scripted physical fixtures, not human driving. The first two probe directories preserve failed candidates: the infinite closure plane, the west-shore cliff, and tests that incorrectly judged much later off-road travel instead of the actual shore exit. Those candidates are not passing evidence.

The later `lifecycle-final/checks.txt` passes **68/68** additional checks: actual dry-shore entry, immersed steering, stopping/reversing, steering while reversing out, entry/exit sound dispatch, AI immersion/feedback, recovery cleanup, stable occupancy through pause/recovery, AI not triggering bats, cooldown/leave-to-rearm, and restart/scene-change cleanup. `lifecycle/` preserves its failed predecessor: vehicle-profile artwork hiding the water pool, a kinematic AI fixture, and an exit test that reversed tangentially without steering back toward shore. The pool now stays separate from profile artwork and engine-audio cleanup. Sound-dispatch/sample checks are not subjective listening.

`regression-forest/nominal/summary.txt` passes **12/12** existing motorcycle/ATV jump probes. Takeoff/landing speeds match the prior nominal results to their reported precision; water does not make these airborne jumps sticky. `records/done.txt` records **29/29** storage/history checks. `aborted-legacy-probe/` preserves an interrupted invocation of the old hardcoded evidence path; the old CR-056 evidence was restored byte-for-byte, and the probe now accepts `-forestEvidence` for a separate output directory. No prior-release evidence is intentionally replaced.

`pose-preservation.json` confirms **14,236 Street and 14,303 Forest retained transforms** have unchanged position, rotation and scale. `route-preservation.json` confirms unchanged Forest road, cave, jump-layout and gate component data. Street gate data is identical; road/branch serialization only added already-existing default fields (`forestTrail=0`, `entryInset=0`, `entryMargin=7`). Houses, fences, garage, vehicle profiles, paint, radio logic and ordinary penalty rules were not replaced. Stunts and ghosts remain unimplemented.

## Labels, people and bats

The first authoring pass removed **57 Street and 77 Forest** floating TextMesh objects. Mounted storefront lettering remains. Checkpoint geometry/detection, route entitlement and compact screen UI remain. No replacement floating labels or debug identifiers are rendered. No new cave sign was needed.

Dan's property independently selects football (42%), coffee (31%) or empty (27%). The friend selects two smoking/chatting men (52%) or empty (48%). The pool contains 35 figures total; mutually exclusive household selections cap active figures at 33. There are 24 highway frontage slots versus four sparse residential slots, with 70% versus 30% occupancy. Figures have no colliders, and no damage, impacts or penalties are attached to them. Walkers use short supported paths; other residents gesture in place. Grounded anchors preserve the accepted buildings and fences. Animation work is limited to 30 Hz within 220 m, with small-object LOD culling. Selection is stable until a race/world setup; no passing-player respawn or in-view refresh is used.

`probe3/occupancy.csv` demonstrates all six household combinations across 30 forced seeds. Seeds **1** (football/smokers), **3** (football/empty), **9** (coffee/smokers), **13** (coffee/empty), **2** (empty/smokers), and **4** (both empty) are useful review fixtures. `probe3/natural-occupancy.csv` records ordinary clock-seeded choices, including all three Dan states and both friend states. Closely adjacent diagnostic calls can share a millisecond seed; ordinary race restarts are much farther apart.

Eight pooled bats leave a cave roost, approach above the rider and scatter to alternating sides over three seconds. Scalloped wing meshes, ears, bodies and individual wing phases form the silhouettes. Only the player can trigger them; the approach must have forward motion. Rearming requires both a 45-second cooldown and 12 seconds outside 65 m. There is no collision, damage, force, camera shake or steering effect. Flutter/chirps are synthesized and modest; no subjective listening claim is made.

Close-up visual review corrected the initial household anchors: the accepted fence is rectangular and larger than the historical circular yard-clearance area. Coffee now sits at 8.5 m from the street centerline, outside its 11 m front fence; the football triangle is inset from the side fence. Houses/fences are not moved. Earlier `probe3` and `lifecycle-final/coffee.png` images show the superseded anchors; final scene captures identify the corrected placement separately.

The final player passes **185/185 Forest and 157/157 Street scene checks** in `scenes-forest/` and `scenes-street/`: all six forced household combinations, supported feet, in-yard football endpoints/flights, coffee outside the front fence, pedestrian traffic/trail clearance, and absence of floating text. Their PNGs show the final corrected placements, smokers, empty states and preserved start menus. These scripted captures and geometry checks do not prove subjective animation quality or all possible passing-camera pop-in behavior.

To review a fixed household arrangement without altering normal saves, launch `Racer.exe -racerTestSave Temp/life-review -lifeSeed 9` from the project folder, then select either track normally. Substitute the seeds above. Omit `-lifeSeed` for ordinary variation. Automated commands additionally use `-livingTest scenes|probe|lifecycle|race`, `-course LakeWoods|StreetLoopGreybox` and a distinct `-evidence` directory. Race comparisons accept `-racerDifficulty 0|1|2` and `-errorsDisabled`; all diagnostic overrides require `-racerTestSave`.

Art limitations: simple procedural capsules/spheres, limited shoulder/leg gestures, short walking loops, small props, stylized smoke and synthetic sound. There is no facial animation, full hand/foot IK, cloth, dialogue or wildlife simulation. Bat readability and the recognizability of the football/coffee/smoking gestures still need Dan's driving-view review.

Highway slots include pairs 2.4 m apart so independent occupancy produces occasional small idle groups as well as solitary walkers. The same 24-slot population limit applies. This final placement adjustment does not add colliders or change driving inputs.

## AI comparison method

The driver varies corner line, braking anticipation or pre-jump alignment through its existing targets and motor inputs. Maximum easy line variation is 0.65 m on a corner or 0.28 m before a jump, with smaller Normal/Hard amplitudes. Cooldowns start at 18/30/48 seconds plus per-driver jitter. The cave, bypasses, launch/landing zones, immersion and already-unstable driving suppress new mistakes. Base speed, grip and brake capability constants remain unchanged. No player-position trigger, injected pause, velocity override, forced crash or teleport was introduced for mistakes.

The matrix uses seeds 101–103, two laps, three AI opponents plus a reference driving pilot, traffic enabled, both tracks, all difficulties, and matching errors-disabled runs. Accelerated headless runs measure physical completion and driving inputs; their frame times are **not rendering benchmarks**. AI event starts are in `events.txt`/player logs; traces include line offset, anticipation factor, pedals, immersion, support and recovery. Whole-race paired time differences include traffic interactions and existing recovery behavior, so they are not isolated causal costs for a single mistake.

The initial candidate accidentally clamped the pre-existing civilian bypass lane. That candidate's Street traces are retained separately and are not final completion evidence. The correction only clamps a line when a real judgment offset is active.

### Completed matrix

All **108/108 AI entries** finished their two-lap races: 54 with errors, 54 without. All Forest AI gate counts were clean. One Normal Street AI run missed one ordinary gate; no new penalty mechanism was added. **35/36 reference-pilot entries** finished; the Easy Street seed-103 reference pilot was stuck in traffic and DNF'd, while all three opponents finished. This is retained as a limitation, not counted as a pass. Overall: 143/144 riders completed across 36 races.

Each table row compares nine AI results (three opponents × three seeds). Times are raw race seconds, not penalty-adjusted totals. Positive delta means slower with mistakes; negative means faster.

| Track | Difficulty | Errors / baseline mean seconds | Delta | Judgment events | AI recoveries, on / off |
|---|---|---:|---:|---:|---:|
| Forest | Easy | 159.509 / 155.278 | 4.231 | 41 | 1 / 0 |
| Forest | Normal | 159.577 / 156.737 | 2.84 | 32 | 1 / 4 |
| Forest | Hard | 154.937 / 160.482 | -5.545 | 22 | 1 / 0 |
| Street | Easy | 323.608 / 319.817 | 3.791 | 84 | 4 / 2 |
| Street | Normal | 279.744 / 281.015 | -1.271 | 54 | 0 / 0 |
| Street | Hard | 282.015 / 273.497 | 8.518 | 39 | 1 / 0 |

`race-summary.json` and the `final-<track>-d<difficulty>-<disabled>` directories are the authoritative raw results. Events comprise **131 wide-line, 121 early-braking and 20 jump-approach** deviations. These are actual activated input/target variations, not crash counts or a claim that every deviation was obvious to a human observer. The negative timing deltas show why these whole-race comparisons cannot assign a fixed time penalty to each error: traffic spacing, line choice and ordinary recovery interact. The implementation imposes no such penalty. Base pace remains competitive, but the readable severity/frequency and imperfect-landing presentation still require Dan's review.

## Rendered performance and packaging

Sequential Windows D3D11 tests explicitly rendered a 1280×720 camera target at normal simulation speed with a 120 fps cap. Forest measured median **8.334 ms**, p95 **8.404 ms** over 19,826 frames; all four riders finished. Street measured median **8.334 ms**, p95 **10.488 ms** over 20,556 frames in a 180-second performance window. That Street window intentionally stopped before two-lap completion; the separate full-race matrix supplies completion evidence. Both selected 26 people and played the Groove radio channel (34 songs in that channel; 187 songs bundled overall). Normal track traffic populations were four Forest vehicles and 20 Street vehicles. Host-sampled peak working sets were approximately 752.4 MiB Forest and 492.8 MiB Street; the in-player process API returned zero, so its `peakMiB=0` field is unavailable data, not zero memory use.

These are offscreen rendered, capped local measurements with the editor open, not foreground presentation measurements or worst-case guarantees. The final group-anchor pairing keeps the same count/animation budget; these two race measurements precede that placement-only adjustment. Neither reference route triggered bats, so a separate final cave-effect fixture is reported below. Human driving, subjective listening and physical-controller testing remain pending.

The final `performance-bats/` fixture deliberately positions the rider on the cave approach, then lets the normal trigger run. All eight bats activate once alongside 26 selected people, four Forest traffic vehicles and Groove playback. Its 2,376 rendered frames measure median **8.334 ms**, p95 **8.367 ms**. This short scripted fixture tests simultaneous systems, not a completed race or human cave approach. `scenes-forest/` and `scenes-street/` were rerun after the group pairing and retain their 185/185 and 157/157 passing checks. Final build: zero errors, one existing optional Runtime Pipeline configuration warning. A preceding build included a CLI request timeout in its log; the clean final build followed after that request finished.

The local release uses `Builds/Racer-0.9.0-review1-Windows/`, its matching ZIP, and the complete `Builds/Latest/` runtime. `Play-Racer.cmd` remains unchanged. `SOURCE-SHA256.txt` records 2,458 Assets/Packages/ProjectSettings files. The existing packaging workflow hashes staged music, runtime files and every extracted ZIP file, preserves previous complete runtimes/ZIPs, and copies the full result into Latest. The committed package verification snapshot is `package-final.json`; after the completion commit, the same workflow stamps that commit into VERSION.txt and verifies again in `Builds/PACKAGE-LATEST.json`. Bundled music remains local and no package is uploaded.

## Dan's combined playtest checklist

- [ ] Ride motorcycle and ATV into the lake from several shores. Stop, turn, reverse, climb out, and recover locally. Confirm the vehicle looks partially submerged and loses only temporary speed. Repeat accessible creeks; overfly water and use dry ramps.
- [ ] Drive both tracks with no floating route/location/development text. Confirm screen HUD, countdown/results, gate navigation and radio information remain readable.
- [ ] Race each difficulty. Watch for occasional modest errors and natural recovery; judge competition, cave reliability and whether Hard is still imperfect without feeling artificially slow.
- [ ] Approach the cave on motorcycle and ATV. Judge silhouettes, sound and visibility of the turn/jump. Linger, reverse and recover nearby; swarms must not spam or interfere with control.
- [ ] Review football, coffee, smokers and empty visits. Confirm the football stays inside the yard, feet/props look grounded and gestures are recognizable. Check denser highway frontage and sparse South Cherokee occupancy.
- [ ] Restart, pause/resume and change tracks. Check stable occupancy during a visit, no visible pop-in, no vehicle snags or gate interference, preserved garage/colors and historical boards.
- [ ] Play with traffic and bundled radio music. Judge volume balance, performance, driving feel and physical-controller behavior on Dan's hardware.

New categories are `street-v9-life` and `lake-v3-shallows`. Existing record boards and save files remain historical and are not rewritten. Human driving, subjective listening and a physical-controller test remain outstanding; automated tests do not close them.
