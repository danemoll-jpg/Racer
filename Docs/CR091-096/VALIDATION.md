# CR-091–096 validation

Delivery: **0.16.0-review3**, awaiting Dan review. Safety checkpoint: `a89577e9b0d3dfb4bf61652dd6ea71bae39bcdd1`.

The baseline is 0.15.0-review3. Its evidence and unresolved limitations remain in Docs/CR081-090 and Docs/CR082-089; this pass does not retroactively accept it. Intermediate failures and corrections are preserved in INTERMEDIATE-WORKLOG.md and the raw candidate folders.

## Evidence boundaries

- `first-geometry`, `inspection`, `inspection2`, `inspection3`: development players and first-playable evidence.
- `final`, `final-regression`: 0.16.0-review1, despite the historical folder names.
- `verified`, `verified-regression`: 0.16.0-review2, including the interrupted summit matrix and failed Street acorn-24 approach.
- `release3`: delivered-candidate Windows player. Each process.json identifies its runtime, assembly hash, isolated save, arguments, exit and timeout. Scene geometry differs between review1/2 even though the gameplay assembly hash is unchanged.

All ordinary tests use isolated Temp/cr091-* saves with temporary master mute. No player preferences are replaced. Captures explicitly render the game camera and every overlay canvas. Virtual mouse/gamepad checks are automated input, not physical-controller or human acceptance.

## Final player systems

- Finish: 166/166 explicit checks across four variants, including exactly one visible +5s final-lap penalty.
- Map: 168/168 course checks, including 60 supported destination/eligible-vehicle combinations, race/unknown-destination rejection, virtual mouse/controller input, cancelled attempts and no teleport-path awards, collection or fog reveal.
- Collectibles: 24/24 physically reached and locally recoverable in every variant (96 approaches); 22 placements are away from ordinary race roads. Eight cold/track-reload assertions retain all IDs and identical fog. Confirmed-restart checks preserve fog/settings; stable IDs and unrelated save files remain.
- Final return: all four Street profiles physically drive the summit return and Kyle driveway in both directions; 16 assertions pass including supported recovery. Minimum upright on the return is recorded in release3/return-driveway/checks.txt.
- Final one-lap race smoke: all four variants, all four racers finish in each smoke run; no gate misses. This is separate from the twelve three-lap regression combinations below.
- Isolated Classic/tourer clean runs save their ghosts without recovery; replay pause and non-collision checks pass. Together with the directly inspected ten regression saves, all twelve categories have real clean-lap files.
- Rendered views and destruction/restoration checks cover 50/47/55/58 enabled physical signs for Street/Forest/Street Reverse/Forest Reverse. The final summit return sign was explicitly regrounded after the raised landing. Property, camp, map and physical lettering captures are retained.
- Real scored summit attempts and their exact record categories survive a cold player relaunch.
- Household rotation: 474 assertions pass; radio/folder behavior: 41 pass. These are automated behavior checks with muted output, not listening acceptance.

Final summit matrix: **372 physical cases**. Of 360 forward race/free-roam runs, **344 scored**. All **216 center/±6m cases scored**, with 2.40–3.26 seconds airborne and minimum upright .934. Of 144 actual body-edge cases, 16 did not score: the southern motorcycle edge at 32m/s and southern ATV edge at 40m/s, repeated across the four variants and both modes. Twelve dipped below upright .65 (eight motorcycle cases and four Forest ATV cases); the four Street ATV cases remained upright but hit the hard-landing scoring rejection. All local recoveries succeeded.

The harness completion flag means passing 200m and eventually maintaining supported upright motion for 20 fixed frames. It does **not** mean an uninterrupted clean landing inside the intended footprint; the rolled edge cases can travel beyond it before settling. Those are residual limits, not passes. The 12 reverse-face probes completed and recovered without scoring, as intended for the explicitly signed wrong approach. The separate northern return passed all four Street profiles.

Serial ordinary-rate performance: each area ran alone for 75 seconds with a 10-second warmup, timeScale=1, explicit 1280×720 rendering, traffic enabled (20 drivers), ambient people enabled and bundled radio playing under temporary master mute. Neighborhood median/p95/p99: **8.334 / 11.949 / 17.042 ms**; mountain: **8.334 / 11.929 / 17.653 ms**. Peak process memory is in each process.json. Background editors remained open; no other test player ran concurrently. These observed frame times do not close earlier neighborhood spikes or promise target-machine FPS. `release3/summary.json` supplies the complete check and matrix counts. First-playable Street geometry was tested before refinement; initial all-variant coverage was completed later, not retroactively claimed before the first polish.

The physical summit matrix uses the actual authored launch/terrain, existing vehicle physics and ordinary fixed updates, at 24/32/40 m/s (approximately 54/72/89 mph), center, ±6m and the actual vehicle-body side edges. Each trace includes input, position, speed, vertical velocity, wheel support, suspension, alignment, upright orientation and collision normals. Forward race/free-roam runs are separate from the reverse-face risk checks and the deliberately designed return. The summit is optional exploration content, not an AI race route or mountain race.

Finish fixtures seed earlier ordered progress, then sample actual swept passage and legal branch travel. They are separate from full physical races. They cover initial/reverse/duplicate/teleport rejection, airborne and side misses, exactly one five-second HUD notice, final-lap completion and legal-shortcut entitlement.

Collection checks drive from each inventory access point with ordinary physics and rotate through eligible profiles. Successful supported local recovery supplies the verified escape option; this does not assert every possible off-road approach is safe. The confirmed restart only removes collected IDs. Previously found relocated IDs remain found by default.

## Complete regression before final landing/access refinements

The review1 Street and review2 Forest regression players completed all twelve eligible player/course three-lap combinations, with zero player gate misses. One original-car AI opponent in the forward Street tourer race finished only two of three laps before the existing grace deadline. Safe route/branch recovery, free-roam recovery, rules, speed activities, wrong-way guidance and repeated-obstruction recovery checks passed. The earlier tourer obstruction timing failure passed after the targeted repeated-stall retry change (33.23 seconds, two recoveries).

Ten clean-ghost category files were verified directly; the two forward Street car traffic runs reset during every lap and correctly produced no clean ghost. Final isolated clean runs cover those categories separately. A passing solo ghost does not erase a traffic/AI limitation. Ghost files retain versioned course/direction/vehicle/handling categories, and old categories remain stored.

Actual shortcut-driving regression retains three forward Creek Leap clean-run failures (Classic, tourer, ATV). Their runs finish but require recovery. The other cases in these four course-specific shortcut harnesses passed. This is not blanket closure of earlier Granite Saddle or CR-087 reports. The folders named shortcuts in inspection2 were guidance tests, not physical shortcut proof.

The unchanged race-jump fixture completed 36 traversals: 24 Street/Reverse Street runs scored, while the 12 Forest/Reverse Forest runs completed stably but did not award an authored activity. Those zero-award results remain a limit; fixture completion is not a claim that every existing ramp works.

## Retained baseline limits and unperformed acceptance

The baseline's 1,500 ramp traversals remain intact: 173/744 Trickum and 316/720 mountain cases were nonclean. Earlier Creek Leap/Granite Saddle, AI grace, AI timing and neighborhood frame-time failures are retained alongside the new repeats. No global handling or scoring-threshold relaxation was used to hide them.

No human driving, physical-controller ergonomics, listening, target-machine FPS guarantee or blanket architectural acceptance is claimed. Tests at accelerated simulation speed are not performance measurements. Serial ordinary-rate home/mountain measurements are reported separately. Dan's new review checklist and controls are in README-player.txt.

## Packaging and checkpoints

Final matrix and serial performance are complete. The full runtime, extracted ZIP and Latest are SHA256-verified; package-final-verification.json records the final archive, preserved previous runtime and music counts. Completion commit: `87d53b7d609b77f1a5e24106845fb183a57a9d1d`. Final metadata/report updates are a subsequent commit; they do not change the tested player code or geometry. Earlier versioned builds and staged music are preserved through Tools/Package-Racer.ps1. Build succeeded with zero errors and four warnings. Source/Tools whitespace checks pass; Unity-authored scene serialization retains its normal empty-field whitespace.

Final systems total: 1,246 explicit assertions passed, zero process timeouts or nonzero final-player exits across 45 launches. Matrix instability/unscored cases are reported separately above and are not erased by that assertion count. No final logs contain NullReferenceException, MissingReferenceException, unhandled exceptions or C# compile errors.
