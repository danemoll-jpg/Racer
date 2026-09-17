# Phase 3 — race systems and CR-010

Implemented 2026-09-17. **Awaiting Dan's review. Phase 2 remains accepted; Phase 4 is not started.**

Safety checkpoint: `1b6e7bcf816a3afdfe55a1ae4e42b9c07095f2ed`. The completion commit includes this document; obtain its ID from the task report or `git log` (it cannot contain its own hash).

## Play and rules

Open `Assets/Scenes/StreetLoopGreybox.unity`, press Play, and focus Game view. Drive forward from the existing north-entrance spawn through the checkered START/FINISH line. Complete **three laps**, passing **19 numbered checkpoints** in order before each finish crossing. Timing begins on the first forward start crossing; the total freezes at race completion. The car remains driveable after finishing.

| Action | Keyboard | Xbox-style controller |
|---|---|---|
| Accelerate | W / Up | RT |
| Brake, then reverse | S / Down | LT |
| Progressive steering | A/D or Left/Right | Left stick |
| Existing fixed-spawn vehicle reset | R | Y |
| Restart entire race | Enter | Start / Menu |

HUD shows lap, completed laps, race/current/last/best lap times, next gate, course status, and reset/restart controls. Vehicle reset clears the current lap's gate credit, retains completed laps and total race time, and requires another start crossing. Restart returns to the same accepted spawn and clears all laps, gate credit, last/best splits and timers. Falling below the existing reset threshold retains automatic recovery. A finished race remains complete until race restart.

Gate crossing uses the car centre sweeping through a bounded plane, including direction, forward-heading and height/width checks; it does not rely on trigger overlap or child collider counts. Wrong-way or out-of-order checkpoint crossings invalidate the current lap. A forward finish with missing/invalid checkpoint credit begins a fresh attempt **without awarding a lap**. Repeated finish crossings cannot accumulate laps. Position jumps exceeding 10 metres between physics samples invalidate the attempt; respawns explicitly reset the sample origin, so the respawn path cannot hit gates.

`RaceProgress` contains plain C# progression/timing rules; `RaceGate` provides geometry; `RaceDirector` samples movement, connects reset and race-restart input, and owns an ordered gate array. Only the required gate sequence is enforced: future intentional paths can connect the same gates without changing lap logic. No shortcuts were added. This is course validation, not continuous road-boundary enforcement or comprehensive anti-cheat. Ordinary corner cutting between required gates remains possible.

## Preservation and steering

Race content lives under the separate **Phase 3 - Race Systems** root. No environment rebuild was run. Serialized comparison against the checkpoint found all 20,054 pre-existing scene records present. Only two existing records changed: the car prefab-instance record acquired the two overrides below, and the scene-root list acquired the race root. Existing road/terrain/house/business/tree records remain identical after line-ending normalization. The environment assets, PrototypeTrack, PrototypeCar prefab, ArcadeVehicle, VehicleInput, VehicleRespawn, and ChaseCamera sources are unchanged.

| Parameter | Before | After |
|---|---:|---:|
| Steering input response | 7/s | 8/s |
| Low-speed steering angle | 32° | 33° |
| High-speed steering angle | 10° | 10° |
| Yaw response | 8/s | 8/s |
| Grip response / acceleration cap | 7/s / 22 m/s² | unchanged |
| Yaw rate cap | 1.6 rad/s | unchanged |
| Stick deadzone | 0.12–0.95 | unchanged |

Existing response uses MoveTowards, so neutral-to-full input takes approximately 143→125 ms before physics integration. Steering authority still interpolates with absolute speed from low angle to 10° at the existing 38 m/s speed target, including reverse. At 25 m/s, nominal full-lock angle changes only about 17.526→17.868°. Braking, suspension, mass/centre of mass, camera and reset tuning are unchanged. Revert the **StreetLoopGreybox car instance** to slowSteerAngle=32 and steeringResponse=7 to compare; do not apply overrides to the base prefab. PrototypeTrack retains the original values.

## Actual tests

`PHASE3_TEST_RESULTS.txt`: **37 checks passed, 0 failed** in the final run. Includes pure progression scenarios, geometric crossing tests, virtual input checks, restart/reset/camera/HUD assertions, and three complete laps through the actual gate geometry using real vehicle forces and PhysX at 0.02 seconds. Only initial placement is artificial; no teleport or body-velocity correction occurs during the three-lap drive. Lap times **563.772 / 563.780 / 563.780 seconds**; final total **1691.332 seconds**. Targets are 4.5 m/s on tight bends and 8 m/s elsewhere. Maximum road-centre error **2.845 m**, minimum upright dot **0.880**, one ungrounded sample at initialization. Full-loop results cover the hairpin, hills and ordinary bends at these conservative speeds.

The initial report is retained as `PHASE3_TEST_RESULTS_INITIAL.txt`: two test-harness failures were corrected (world/local float comparison tolerance and synchronizing the fall-test Transform after moving its Rigidbody). No vehicle behavior was changed to make them pass. An early test-helper name conflicted with the Key enum and was corrected. Long direct validation calls exceeded Pipeline's five-second response deadline while completing in Unity; the saved final report establishes completion, not the transport response.

`CR010_TEST_RESULTS.txt`: **18 checks passed, 0 failed**. Original/revised route-following comparisons: ordinary bend 10 m/s, hairpin 5 m/s, hills 10 m/s, fast connecting/main road sections 25 m/s, reverse 8 m/s; eight simulated seconds each. Minimum upright at least **0.966**, zero airborne samples. The automated path follower compensates for steering-angle changes, so its essentially identical route results demonstrate retention of control, not subjective feel improvement.

Additional identical full-stick steps show earlier yaw response:

| Initial speed | Original yaw at 100 ms | Revised yaw at 100 ms |
|---|---:|---:|
| 10 m/s | 0.4762 rad/s | 0.5648 rad/s |
| 25 m/s | 0.7155 rad/s | 0.7568 rad/s |
| -8 m/s | -0.3982 rad/s | -0.4730 rad/s |

All six 600 ms response tests stayed upright; reverse yaw sign remains correct. These short pulses do not certify prolonged full-lock driving at maximum speed or collision recovery.

`PHASE3_REALTIME_RESULTS.txt`: a separate virtual Gamepad RT=0.65 drive ran ordinary Update/FixedUpdate frames, started the race and advanced to CP2, with HUD timer 42.290 seconds and camera distance 8.32 m. It was an unsteered integration smoke test, not a complete controlled lap: the car eventually left the road on the bend and stopped. Temporary device and run-in-background setting were removed/restored, then race restarted.

Virtual Gamepad checks cover progressive stick/trigger values, brake/reverse, Y reset and Start restart. Virtual keyboard checks cover WASD, arrows, R and Enter. **No physical controller or physical keyboard driving was performed.** Comfort, twitchiness, camera feel, and final CR-010 approval belong to Dan.

Saved scene reloaded and entered Play successfully. Gate labels and HUD visually inspected; mirrored initial labels were corrected. `PHASE3_HUD.png` uses camera capture with the HUD temporarily rendered through the camera because the composited screen tool returned a partial frame. The saved HUD remains Screen Space Overlay. Temporary capture assets were removed. Final compilation/Console status is in `PHASE3_CONSOLE.json`; historical Pipeline diagnostic counters include resolved errors and timeouts, while current `groundTruth` is the relevant final state. No standalone build or physical hardware test was performed.

## Files and repeatability

- Runtime: `Assets/Scripts/RaceProgress.cs`, `RaceGate.cs`, `RaceDirector.cs`, `RaceHud.cs` (+ metadata).
- Authoring/testing: `Assets/Scripts/Editor/RaceSetup.cs`, `RaceValidation.cs`, `SteeringValidation.cs` (+ metadata).
- Scene: `Assets/Scenes/StreetLoopGreybox.unity` (race root, HUD and two car overrides).
- Materials: `Assets/Materials/RaceGate.mat`, `RaceWhite.mat`, `RaceBlack.mat` (+ metadata).
- Documentation: this report, test results/capture/audit/Console evidence, root TODO and README.

In Play mode use **Racer > Validate Phase 3 (Play mode)** and **Racer > Validate CR-010 (Play mode)**. Do not manually drive while tests run. They temporarily use scripted physics and virtual devices, restore settings, and reset the race. `Add Phase 3 Race Systems` is a one-time authoring command and refuses a scene with an existing director; edit the saved director's gate array and gate transforms for future course changes. Gates and marking meshes have no physical colliders.

## Dan's review checklist

1. Complete three laps in numbered order. Check lap splits, total, next gate and completion display.
2. Skip a gate and cross the finish; try a wrong-way gate and repeated finish crossings. Confirm no invalid lap credit and that a fresh start recovers the attempt.
3. Use R/Y partway through a lap; check fixed-spawn recovery and lost current-lap credit. Use Enter/Start and verify all progress/timers clear.
4. Compare the tighter steering through normal bends, hairpin, hills, fast sections and reverse. Check progressive stick response, keyboard control, braking, stability and camera comfort.
5. Test a physical controller separately. Report any uncomfortable behavior with speed/location/input and decide whether to accept Phase 3 and CR-010. Phase 4 remains out of scope.
