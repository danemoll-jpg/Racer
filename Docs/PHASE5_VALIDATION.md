# Phase 5 — one southwest woodland shortcut

Status: implemented and technically tested; **awaiting Dan's approval**. Phases 2–4 remain accepted. No Phase 6 work, second shortcut or additional jump.

Safety checkpoint: `64c122016a6efa7c142438835c88ff47770d38ed`. The working tree was clean before modifications. The completion commit includes this report and is identified in the task's final response.

## Find and drive it

Open `Assets/Scenes/StreetLoopGreybox.unity`. Follow the gates to **CP11**, approaching the southwest right-angle bend westbound. A yellow **WOODLAND CUT > / NARROW DIRT PATH / 80–95 km/h** sign appears just before CP11. Pass CP11, then bear right onto the brown dirt path across the inside of the bend. Rejoin the northbound road before CP12. The accepted orange jump is farther ahead, between CP13 and CP14.

- Entrance: **(-560.08, 6.36, -551.35)**, original road sample 1292.
- Rejoin: **(-620.04, 6.39, -449.38)**, original road sample 1376.
- Hierarchy: `Phase 5 - Southwest woodland shortcut`.
- Start around **80–90 km/h (22–25 m/s)**. Follow the curve smoothly, stay between the yellow edge markers, then straighten onto the road. 95 km/h is the upper end of the sign's suggestion, with less margin.
- Missing the entrance is harmless: stay on the normal road. For an excursion, lift/brake and return gradually. R/Y remains the established fixed-grid reset and abandons current-lap credit.

The inside of this relatively flat bend offers a shorter, smoother line without bypassing a required gate or touching the remembered key houses or jump. The normal road is approximately 9 m wide; the shortcut has a 5.2 m dirt core, feathered to 7.6 m. The risk is overspeed/poor alignment carrying the car onto the surrounding shoulder and woodland, requiring correction or reset. It is deliberately forgiving; the dirt has no added grip penalty and leaving its paint does not itself invalidate a lap. Dan should judge whether that precision demand is enough.

## Local implementation and preservation

One continuous, existing terrain/collision tile, `Ground_80_80.asset`, is locally reshaped and recolored along the passage: 548 vertices visited with nonzero surface/color influence; height blend extends to 12 m from the centerline. Existing road center/inner shoulder heights are protected by the road-distance mask. Visible and physical ground use the same mesh, without an overlaid slab, lips or a new jump. Original central-difference terrain shading is retained, including tile edges.

One tree's trunk collider and its two visual cubes (crown and trunk) were removed from `Forest_-4_-4.asset` and the scene. Other woods, buildings and landmarks remain. Two non-colliding signs and twelve small non-colliding edge markers communicate the passage. No barrier or destruction system was necessary. No new runtime component, off-road behavior, checkpoint logic, gate dimensions, gate positions, race progression, reset rule, car tuning or camera/input source changed.

Scene record comparison against the checkpoint: 20,726 original records; only the forest parent's child list and scene root list changed among retained records. Three records for the one tree collider object were removed; 66 shortcut records were added. Car prefab overrides, gates, jump, camera and landmark records are identical. The authoring tool refuses to overwrite an existing shortcut. Do not run the older Phase 2 environment rebuild on this scene: it regenerates local terrain/forest assets and is not an incremental Phase 5 rebuild.

## Matched timing trials

Manual PhysX at 0.02 s, virtual throttle/brake/steering commands, same approved car and fixed initial conditions. Every pair starts at **(-516.01, 6.81, -549.64)** before the divergence/CP11 and stops at the same interpolated exit plane through **(-621.28, 6.69, -394.53)** after the rejoin, just before CP12. Thus approach and re-entry are included. Initial speed is injected equally for each pair after identical 1.5 s settling, excluded from the clock. A common curvature/braking policy follows each path, with a local 26 m/s cap; it slows more at the normal road corner and also conservatively brakes at the shortcut's entrance.

| Initial speed | Normal route | Shortcut | Saved | Result |
|---|---:|---:|---:|---|
| 22 m/s / 79.2 km/h | 11.506 s | 9.528 s | 1.978 s | Both clean |
| 24 m/s / 86.4 km/h | 11.445 s | 9.487 s | 1.958 s | Both clean |
| 26 m/s / 93.6 km/h | 11.385 s | 9.459 s | 1.926 s | Both clean |
| Mean | **11.445 s** | **9.491 s** | **1.954 s / 17.1%** | 0/3 failures on each route |

Normal mean speeds were 21.83–22.06 m/s; shortcut mean speeds 23.78–23.94 m/s. Both reached about 32.57 m/s after the rejoin. The conservative shortcut policy measured 17.02–17.04 m/s at divergence; separate sustained 24–26 m/s tests establish that the intended faster entry is usable. These are deterministic controller trials, not a statistical prediction of human success or optimal race times. Three pairs vary the common starting speed rather than claiming three independent human attempts.

## Stress, recovery and race validity

Raw evidence: `PHASE5_TIMING.txt`, `PHASE5_RECOVERY.txt`, `PHASE5_NORMAL.txt`, `PHASE5_SHORTCUT.txt`, `PHASE5_MIXED.txt`.

- **Slow**: two 6 m/s trials with ±4° initial headings completed in 36.06 s, no airborne steps, maximum error 1.29 m.
- **Intended speed**: two sustained 24 m/s trials with ±4° initial headings completed in 9.35–9.36 s, maximum error 2.00 m. Two 26 m/s trials with ±2 m starting offsets completed in 8.65–8.66 s, maximum error 2.59 m, close to the core edge.
- **Faster/poor entries**: all eight deliberately harder trials were classified as failures of the clean dirt-core criterion: 34/42 m/s with ±4°, 34 m/s with ±10°, and 26 m/s with ±5 m starting offsets. All eventually reached the common exit upright, but ran outside the intended core. The 42 m/s tests reached 9.22/11.48 m maximum error and slowed during recovery. Their 6.39–8.68 s endpoint times are **not clean shortcut timing claims**. The ±5 m trials start outside the clean corridor and measure recovery, not an unexpected loss of control.
- Overall stress matrix: **6 clean / 14 attempts, 8 failed clean-route attempts**. Failure is not necessarily a crash. No physical barrier or artificial race penalty is imposed at the dirt edge.
- More targeted **±5° heading errors immediately at entrance and re-entry**, injected 24 m/s: entrance maximum errors 2.23/1.99 m; re-entry 1.65/1.64 m. Upright 1.000 and positive clearance in all four.
- **Four off-road recoveries** at both transitions, ±8 m off the original road, 18 m/s: ended 0.35–1.03 m from road center, upright ≥0.999, minimum body-center ground clearance ≥0.632 m. Normal-route timing/laps also cover taking the road after missing/declining the shortcut entrance.
- **273 ground samples** across center and ±3 m: zero missing surfaces, zero non-terrain obstructions; maximum neighboring height difference 0.029 m. These samples complement actual driving; they are not an exhaustive collision proof.
- **Nine continuous racing-speed laps**, no teleport during traversal: three normal, three shortcut, and shortcut/normal/shortcut. Every race finished with all required gates valid, including the existing jump. Normal splits 184.248 / 184.140 / 184.140 s; shortcut 182.568 / 182.440 / 182.460 s; mixed 182.568 / 184.140 / 182.460 s. Peak **39.71 m/s (143.0 km/h)**; average 25.35–25.45 m/s; minimum upright 0.880 over the whole hilly course. Full-lap savings differ from the injected-speed segment trials because arrival conditions differ.
- **Reset exploit probes** at four shortcut/rejoin failure positions: establish credit through CP11, overturn/displace the car, invoke the actual reset, then try remaining gates and finish. All awarded zero laps; motion cleared, car upright, current-lap progress abandoned. Synthetic CP7 skipping, repeated CP11 and wrong-way CP11 likewise awarded no lap. These four reset/progress probes seed gate state directly; they do not claim four continuously driven crashes.
- Existing physical gate-sweep tests reject out-of-bounds, wrong-way, rear-first and teleport crossings. All **37 race/input/HUD/reset regression checks passed** on the final unpaused run, including virtual keyboard/gamepad controls and restart. No gate was removed, expanded or exempted. Existing rules still permit small cuts between gates; this phase does not add a global track-boundary system.

## Accepted systems and ordinary-frame test

`PHASE5_JUMP_REGRESSION.txt`: existing 12/32/42/46 m/s × 0/±3° jump trials retain previous results. Intended centered 32 m/s: takeoff 31.59 m/s, about 1.72 s airborne, landing Z=-29.37, upright ≥0.980. The same two fast angled tests exceed the landing corridor; these are accepted, documented limits. The normal road lane beside the ramp and jump shoulder recovery tests also ran.

`PHASE5_BRAKING.txt`: injected 35 m/s braking reached 0.85 m/s in **1.40 s / 24.64 m**, upright 1.000. Four further seconds of brake/reverse reached -9.92 m/s upright. Handling on bends, hairpin and hills was exercised by the racing laps; no retuning.

`PHASE5_REALTIME.txt`: one **ordinary-frame Input System virtual Gamepad** drive, normal Update/FixedUpdate and chase-camera updates, rather than manual Physics.Simulate. Injected initial speed 24 m/s; reached exit in 9.468 game seconds / 9.364 wall seconds; peak 24.32 m/s; upright 1.000; maximum line error 2.86 m (into the feathered dirt edge, within 3.8 m); maximum camera-to-car distance 12.36 m; HUD showed 87 km/h. This standalone segment did not start a lap and correctly displayed START/FINISH rather than awarding progress. Camera comfort still requires a person.

Initial harness results are retained: `PHASE5_RECOVERY_INITIAL.txt` mislabeled every terrain hit as an obstruction by checking the wrong object name; corrected check verifies terrain ancestry. `PHASE5_RACE_REGRESSION_INITIAL.txt` records ten virtual-input failures with paused Play mode; the final unpaused run passed all checks. `PHASE5_REALTIME_INITIAL.txt` records a 4.01 m wide run before the test inverted the existing axis deadzone to generate its intended steering. The car's input/physics code was not changed to pass these tests.

The scene was saved, reloaded, compiled and played; overview and chase-camera entrance captures were inspected. Unity reported no compilation failure. Pipeline's five-second command timeouts and rejected scratch-evaluation syntax were tooling issues, recorded separately from gameplay; longer validation calls completed and produced reports. No physical controller, standalone build, exhaustive collision testing or human subjective handling test was performed.

## Dan's review

1. Follow CP11, recognize the dirt entrance on the right, and try it around 80–90 km/h. Is it discoverable without feeling mandatory?
2. Compare it with the normal bend. Does a clean run feel usefully faster, and is the narrower line sufficiently demanding?
3. Try a slightly wide entry, late correction and re-entry. Check camera comfort, clear ground support and predictable braking.
4. Miss the entrance and stay on the road; then try an off-road recovery and R/Y reset. Confirm the reset abandons the current lap as before.
5. Complete normal, shortcut and mixed laps, including the existing jump. Check CP11→CP12 progression, no credit after skipping another gate, and Start/Enter restart.

Leave Phase 5 awaiting approval. Do not begin Phase 6 or add another shortcut/jump.
