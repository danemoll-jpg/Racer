# Phase 7 — implemented, awaiting Dan's review

Open `Assets/Scenes/StreetLoopGreybox.unity` or the local Windows development player `Builds/Phase7/Racer.exe`. Safety checkpoint: `46f0e8b6fc19d5002f8260810ee0d0cddec61e11`. The first Git staging attempt failed creating `.git/index.lock`; the authorized elevated retry succeeded before project changes. Completion commit is reported in the task response.

## Delivery and preservation

Ready/start, three-second countdown, results with all valid lap splits/current best/new-record labels, persistent lap/race personal bests, distinct car reset feedback, pause/settings, keyboard/controller/mouse menus and five restrained synthesized audio cues are implemented together. Existing `RaceProgress`, `RaceDirector`, `VehicleRespawn`, speed HUD, checkpoint geometry and prop restoration are reused.

The scene diff adds only RaceFlow and adjusts the HUD panel dimensions/Canvas scaling. No road, terrain, house/yard, forest, gate, jump, shortcut, prop, vehicle or camera authoring data was changed. Vehicle motor/input/reset/camera source and tuning remain unchanged. WoodlandAmbience receives a volume multiplier while retaining its original clips, scheduling and two voices. Phase 6 remains accepted. CR-013 and CR-018 remain deferred; scoring, speed traps and Phase 8 remain unimplemented.

Controls, timing/reset rules, record eligibility, save paths, settings defaults, audio provenance and a manual checklist are in [RULES.md](RULES.md).

## Actual checks

- [Final 1280×720 standalone](Final16x9/validation.txt): **37 PASS, 0 FAIL**. Visible player, ordinary Update/FixedUpdate and virtual Gamepad/Keyboard/Mouse. Covers Ready; A start; held-throttle countdown lock; Start/Enter/Escape pause/resume during countdown; arrow/Space settings navigation; setting changes with retained focus; B back/focus restoration; actual Canvas mouse raycast/click; ordinary driving input; paused physics/timer/audio; menu reset suppression; Y reset feedback; skipped/repeated/wrong-way gates; repeated finishes; three valid synthetic gate laps and results; settings return; missing/malformed data; repeated restarts; prop contact/restoration/overlap deferral; ambience muting; exactly three audio sources after repeated transitions.
- [Final 1680×720 ultrawide](Ultrawide/validation.txt): **37 PASS, 0 FAIL**. Screenshots inspected with no clipping of menu controls/text. Final 16:9 screens likewise inspected. [1024×768 4:3 screenshots](Aspect4x3/settings.png) and navigation passed before the final HUD-behind-menu suppression; scaling and menu layout were unchanged by that cleanup.
- [Application launch 1](Standalone/launch1.txt) and [launch 2](Standalone/launch2.txt): independent visible player processes using the same isolated storage. Launch 1 loaded zero records/master .8; launch 2 loaded lap **40**, race **120**, master **.7**, and VSync off. Both runs passed all 37 checks. These are synthetic test times, not player records.
- [Valid lap from abandoned race](abandoned-lap.txt): one fully ordered synthetic lap saves immediately; restarting the incomplete race retains that lap record and leaves race record zero.
- [Mixed-route driving](mixed-laps.txt): existing PhysX follower reused with flow active and isolated storage. Shortcut / normal / shortcut, **3 valid laps**, **551.52 s** simulation, mean **25.42 m/s**, peak **39.71 m/s**, max center error **3.23 m**, minimum upright **0.880**. Uses manually stepped PhysX and virtual motor commands; not an ordinary-frame full-race or human-controller run.
- [Ordinary-frame jump probe](ordinary-jump.txt): injected 32 m/s approach, virtual Gamepad throttle, no manual PhysX stepping. Flew and landed, but **failed** the harness's upright >.8 criterion (minimum **.788**, landing Z **−82.88**, 12.01 s timeout). This uncorrected approach is an explicit remaining test limitation; no approved ramp/vehicle tuning was changed to conceal it. Script is archived in [ordinary-jump.cs.txt](ordinary-jump.cs.txt). An earlier duplicate Editor-callback timing measurement was rejected and replaced by the once-per-rendered-frame result.
- [Editor integrated test](Editor/validation.txt): earlier 28-check run passed after fixing initialization. [Initial failure history](INITIAL_FAILURES.txt) records the first UI action-reference error and its downstream failures. The submit action was moved into an InputActionAsset; final standalone runs exercise the corrected implementation.
- Build succeeded ([build.txt](build.txt)). The single reported error is the Pipeline command bridge's five-second timeout while the build continued and succeeded. The final build warning is the absent optional runtime Pipeline config. The first build also warned about future mesh collision prebaking. Player logs retain the pre-existing stripped unused DOF/Panini shader messages and a host Direct3D timestamp fallback; no gameplay exceptions observed. New scripts compile without compiler errors; obsolete API warnings introduced by the test harness were removed.
- Final saved scene reload and Console status are archived in `save-reload.json` and `console-final.json`. Diagnostic history, including initialization and bridge errors, is retained separately in `console-history.json`.

## Performance and limits

Final visible 1280×720 player: 180 ordinary idle frames at the selected 60 fps cap, median **16.67 ms**, p95 **16.73 ms**, max **17.14 ms**. Ultrawide: median **16.67 ms**, p95 **16.68 ms**, max **17.03 ms**. These brief frame-time samples verify responsive operation at the grid; they do not establish course-wide performance parity, CPU/GPU costs or freedom from host-dependent spikes. Prior forest-follower and frame-time limitations remain documented in Phase 6.

All device tests were **virtual**, including mouse/keyboard event injection; physical controller and speaker/headphone balance require Dan. Audio routing/muting/pause/source-count behavior was tested, not subjective listening. No human-driven ordinary-frame three-lap race was completed in this session. Full prop impact matrix and all jump edge/yaw stress cases were not rerun; existing environment/tuning were preserved. The focused ordinary jump probe's failed threshold is disclosed above. Test saves live only in isolated Docs directories; legitimate player records were not overwritten.

## Files changed

- Existing scene: `Assets/Scenes/StreetLoopGreybox.unity`.
- Reused systems: `Assets/Scripts/RaceDirector.cs`, `RaceProgress.cs`, `RaceHud.cs`, `WoodlandAmbience.cs`.
- New runtime flow/UI/storage: `Assets/Scripts/RaceFlow.cs`, `RaceMenus.cs`, `RacerSave.cs`, with Unity metadata.
- Validation/build: `Assets/Scripts/Phase7Validation.cs`, `Assets/Scripts/Editor/Phase7Build.cs`, and a focused mixed-route entry point in `Assets/Scripts/Editor/Phase5Validation.cs`.
- Documentation: `PROJECT_TODO.md`, `README.md`, and `Docs/Phase7/` reports, isolated fixtures and screenshots. Build output remains ignored by Git.

## Screenshots and Dan's review

[HUD](Final16x9/hud.png) · [Pause](Final16x9/pause.png) · [Settings](Final16x9/settings.png) · [Results](Final16x9/results.png). Results screenshots use clearly isolated synthetic gate results.

Start → pause during countdown → resume and drive → reset car → restart race → complete three valid laps using normal/shortcut/jump routes → review results → race again → change settings → quit/relaunch and confirm saved records. Check physical-controller focus, audio balance and the retained jump/landing behavior. Phase 7 awaits Dan's review; no approval is implied.
