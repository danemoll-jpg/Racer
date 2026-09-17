# Phase 4: connector jump and driving refinement

Implemented for Dan's review, 2026-09-17. **Phases 2 and 3 remain accepted. Phase 4, CR-010 feel, and CR-011 speed/acceleration await Dan's approval. No Phase 5 work.**

Safety checkpoint: `391168140bcbe4604b46c6a3cff28b9946c5a088`. The first attempt was blocked by Git index permissions; the permitted retry succeeded before any implementation edits. The completion commit contains this report; its ID is in the task's final response.

## Play and find the jump

Open `Assets/Scenes/StreetLoopGreybox.unity`. Follow the normal numbered race gates. The single orange jump is on the long northbound western connector, **after CP 13 and before CP 14**, around world **(-628, 8, -110)**. Hierarchy root: `Phase 4 - Connector Jump`.

Use **110–120 km/h (about 31–33 m/s)** initially, line up straight on the ramp, and let the car settle after landing. The HUD now shows km/h beside the lap count. The 24 m long, 6 m wide takeoff rises 3 m with a smooth increasing slope to approximately 14 degrees. It occupies the left 6 m of the existing road; a 3 m ordinary-road lane remains on the right. There is no gap in the underlying road. This lane follows the same road and gates, not a shortcut.

Orange edge markings define a roughly 108 m long, 15 m wide landing corridor, from approximately Z=-74 to +34. This reuses the accepted road and already blended, collidable shoulders instead of introducing a raised landing slab. CP 14 is at Z=70.83. The long straight gives space to approach, fly, land, and align for that gate without changing its dimensions or order. No buildings, trees, landmarks, terrain, or road assets were moved.

Controls remain RT / W / Up for acceleration; LT / S / Down for braking then reverse; left stick / A-D / arrows for steering; Y / R for fixed-spawn vehicle reset; Start / Enter for race restart. Click Game view for keyboard focus.

## Exact tuning

Changes are scene-instance overrides. `PrototypeCar.prefab`, `PrototypeTrack`, and the vehicle physics implementation remain unchanged.

| Value | Phase 3 baseline | Phase 4 |
|---|---:|---:|
| Forward speed parameter | 38 m/s | 44 m/s (+15.8%) |
| Acceleration parameter | 12 m/s² | 13.5 m/s² (+12.5%) |
| Steering input response | 8/s | 8/s |
| Low/high steering angles | 33° / 10° | 33° / 10° |
| Yaw response | 8/s | 9/s |
| Lateral grip response | 7/s | 8.5/s |
| Lateral acceleration cap | 22 m/s² | 25 m/s² |
| Braking / reverse acceleration | 24 / 7 m/s² | unchanged |
| Reverse speed / coasting drag | 11 m/s / 0.45/s | unchanged |
| Suspension length | 0.8 m | unchanged |
| Spring / damping / lift cap | 65 / 8 / 45 | unchanged |
| Upright strength / damping | 12 / 4 | unchanged |
| Air stability | 0.18 | unchanged |
| Centre of mass | (0, -0.35, 0) m | unchanged |
| Yaw cap / body mass | 1.6 rad/s / 1200 kg | unchanged |

There are **no jump-specific vehicle overrides**, new air controls, landing impulses, or automatic landing assistance. Existing suspension and attitude stabilization handled tested landings. The forward-speed parameter is a propulsion taper, not a hard velocity clamp: drag lowers naturally attained flat-road speed, and an injected or downhill velocity can exceed it. Increasing it also shifts the existing speed-dependent steering-angle curve, even though angle endpoints remain unchanged.

For a reversible ordinary-road comparison, change the five altered instance fields back to the baseline column. `Phase4Setup.Tune(car, false)` applies the Phase 3 baseline in tests; `true` applies the revised tune. The earlier Phase 3 7→8 steering response and 32→33 low-speed angle changes are historical, not changes made here.

## Handling diagnosis and paired tests

`PHASE4_ROADS.txt` records both tunes on the **same original road geometry before adding the ramp**, with identical placement, initial velocity, target speed, and 0.02 s PhysX steps. The follower supplies steering/throttle/brake commands; it does not overwrite motion during driving. It compensates for steering changes, so it cannot establish human comfort. Fixed-input probes complement that test.

All twelve paired road cases passed: connector at 35 m/s for 6 s, main straight at 38 m/s for 8 s, ordinary bends at 20 m/s for 10 s, hairpin at 9 m/s for 15 s, hills at 20 m/s for 12 s, and reverse at -8 m/s for 8 s. These cases inject their stated starting velocity. No airborne samples or flips occurred; minimum upright was 0.964 on the hills.

Peak sideways motion fell from **0.51→0.41 m/s** on ordinary bends, **1.23→1.02 m/s** at the hairpin, and **1.63→1.32 m/s** on hills. A 4 m/s injected sideways disturbance at 25 m/s forward speed left **0.437→0.266 m/s** slip after 0.3 s. A fixed 0.3 steering step at 25 m/s produced **0.487→0.590 rad/s** yaw after 0.1 s. An 8° roll disturbance settled to upright dot 0.9998 after 0.3 s for both tunes. This supports a modest yaw/grip refinement rather than a suspension change; it does not conclusively identify the cause of Dan's subjective floatiness.

From rest on the same approximately 220 m connector segment, peak speed increased **33.93→38.41 m/s**. Full braking from an injected 35 m/s reached 0.85 m/s in **1.40 s / 24.64 m**, unchanged between tunes. Normal braking into bends is also exercised by the speed-planned full laps below.

## Racing-speed and jump evidence

`PHASE4_LAPS.txt`: three continuous real-PhysX laps through actual gate geometry, starting at rest with no teleport or velocity injection during traversal. The follower plans speeds from road curvature and brakes ahead of corners; target ceiling is 40 m/s on straights, 22 m/s on elevated roads, 32 m/s near the jump, with slower tight turns. The test does not demand full throttle through every turn.

- Three valid laps completed in **552.36 simulated seconds** of traversal, mean **25.47 m/s**, peak **39.71 m/s**. Last lap approximately **183.30 s**.
- **179.94 s at or above 25 m/s**; maximum sampled centre error **3.21 m**, minimum upright **0.880**; total airborne time **5.26 s**, encompassing the three jump crossings.
- All 19 existing checkpoints remained required and ordered. No gate relocation, enlargement, or recovery exemption.

`PHASE4_LIMITS.txt`: full throttle **from rest** on approximately 600 m of the main road reached **41.183 m/s / 148.3 km/h**, with **6.90 s above 40 m/s**, upright dot 1.000. This is naturally achieved speed, unlike the deliberately injected stress-test velocities.

`PHASE4_JUMP.txt`: 12 runs, crossing the physical ramp with injected approach speeds 12, 32, 42, and 46 m/s at headings 0° and ±3° relative to the road. Heading is held during approach/flight, then the follower corrects after landing. All landed upright; ten stayed within the test's 8 m lateral envelope, while two fast left-angle runs exceeded it.

| Approach target / initial speed | Actual centered takeoff | Centered landing Z | Air time | Apex above road |
|---|---:|---:|---:|---:|
| 12 m/s | 12.10 m/s | -73.29 m | 0.86 s | 3.92 m |
| 32 m/s | 31.59 m/s | -29.37 m | 1.72 s | 6.31 m |
| 42 m/s | 40.33 m/s | -1.87 m | 2.04 s | 7.83 m |
| 46 m/s (injected above tune) | 43.52 m/s | +10.45 m | 2.20 s | 8.56 m |

At the intended 32 m/s, ±3° approaches landed at Z≈-31 m, with minimum upright ≥0.906 and maximum lateral offset 7.13 m. At 42/46 m/s and -3°, offsets reached **8.46/9.33 m**, outside the marked corridor even though the car remained upright on existing support. Do not interpret this as approval for fast off-angle jumps.

Additional limit cases: a **6 m/s** crawl undershot, landing at Z=-80.53 on the continuous road (upright 0.919); an artificial **60 m/s** centered approach took off at 56.75 m/s and overshot the marked area to Z=68.64 (upright 0.973). An **-8° approach at 42 m/s missed the ramp**, travelled far off-road, and was recovered; this is a missed-takeoff test, not a successful jump. No claim of an exhaustive collision or rollover matrix.

The right-hand ordinary-road lane was traversed fully at 6 and 30 m/s with no airborne samples. Landing shoulder re-entry from both sides at 12 m/s remained upright (0.998) with positive clearance ≥0.644 m. Re-entry from both shoulders at Z=-145 continued over the ramp at 12 m/s, minimum upright 0.976 and positive clearance 0.224 m. Landing support ray samples across ±8 m and Z=-76…40 found no missing collision surfaces.

## Recovery, input, HUD, and reliability

Existing Y/R recovery resets to the original north-entrance spawn, clears velocity and steering, and abandons current-lap checkpoint credit. Previously completed laps and race time remain; Enter/Start clears the entire race. A staged tilted/fast failed-landing state was reset upright with zero speed. Undershoot, overshoot, and missed-takeoff endpoint recovery followed by attempts to cross only CP14–19 and finish awarded **zero laps**. No new checkpoint credit or nearest-road respawn was introduced. Falling below Y=-15 still automatically resets; a stuck car above that threshold needs Y/R.

`PHASE4_RACE_REGRESSION.txt`: **37 passes, zero failures**, reusing the existing Phase 3 rules/input/HUD/camera/reset checks and conservative 4.5/8 m/s laps as supplemental regression coverage. The first run had ten virtual-input failures caused by test focus routing; `PHASE4_RACE_REGRESSION_INITIAL.txt` retains it. The wrapper temporarily routes virtual devices to the Game view and ignores background focus, then restores all settings and the historical Phase 3 report. No production input changes. Long tests can exceed Pipeline's 5-second response deadline while finishing in the Editor; completed report contents, not transport success, establish results.

Scene saved, reloaded, and played; final compilation succeeded and Console ground-truth counters show **zero errors and zero warnings** (`PHASE4_CONSOLE.json`). Historical Pipeline counters include earlier resolved errors and command timeouts. Screenshots inspect the actual scene and speed HUD; an oversized sign was corrected after visual review. `PHASE4_HUD.txt` confirms 0/32/-8/41.183 m/s display as 0/115/29/148 km/h. HUD screenshot uses a paused, staged 32 m/s state and temporary Screen Space Camera canvas to include the overlay in capture; it is **not** evidence of real-time driving. The saved canvas remains Screen Space Overlay, and camera behavior is unchanged.

Preservation comparison: all **20,627** baseline serialized scene records retained. Only the vehicle prefab-instance override record and scene root list changed among old records. The new jump adds 99 records. Original road/terrain meshes, landmarks, gates, input, vehicle physics, reset, and chase-camera source are unchanged. RaceHud adds only the speed readout.

## Dan's review checklist and limits

- Compare acceleration and sustained faster driving on the connector/main straight. Brake before corners; do not judge the hairpin at full throttle.
- Check that turn-in and sideways settling feel noticeably less floaty without twitchiness, unwanted oversteer, or extra flipping.
- Try the jump straight at 110–120 km/h, then modestly slower and slightly angled. Confirm landing and camera comfort. Keep fast angled approaches exploratory.
- Try the right-hand road lane, shoulder re-entry, a slow undershoot, Y/R recovery, and Enter/Start restart.
- Complete valid laps; verify HUD speed/timers and gate sequence, and that reset or skipped gates cannot award a lap. Confirm physical controller behavior explicitly.

Physical-controller testing, subjective feel, every collision/rollover case, a standalone build, and a new ordinary-frame real-time driving run were **not performed**. These tests use virtual inputs/motor commands and manually stepped PhysX. Gate rules enforce sequence rather than continuous road boundaries, as before. Phase 4 remains awaiting Dan; do not begin Phase 5 or add more jumps without approval.
