# Racer

A small single-player PC arcade racing game inspired by a childhood street.
`PROJECT_TODO.md` is the source of truth for scope, phases, and acceptance.

## Current scope

Single-player Street and Forest races in both directions, plus free roam.
The CR-101�104 correction pass revises Dan's property layout, summit signs,
the homeward summit jump and the spoken title's mix. See the
[validation report](Docs/CR101-104/VALIDATION.md) and
[player guide](Docs/CR101-104/README-player.txt).
Only these reported issues were checked; other acceptance states remain unchanged.
Technical checks do not close human reports.

The earlier phase-specific tuning and reset descriptions below are historical.
Current local recovery uses R / Xbox-style Y and preserves race progress.
## Open

Use Unity **6000.6.1f1** (Unity 6). Add this repository root to Unity Hub,
open it, allow package restoration/import to finish, then open
`Assets/Scenes/StreetLoopGreybox.unity`. Press Play, then click the Game view for
keyboard focus. The Editor may suspend play while in the background.
The visible Windows review build is `Builds/Latest/Racer.exe` (local, excluded from Git).
Choose Start race and wait for GO; R/Y performs local recovery.

## Controls

| Action | Xbox-compatible gamepad | Keyboard |
|---|---|---|
| Accelerate | RT | W / Up |
| Brake, then reverse when held | LT | S / Down |
| Steer | Left stick | A/D / Left/Right |
| Reset locally | Y | R |
| Pause / resume | Start / Menu | Enter / Escape |
| Confirm menu selection | A | Space |
| Back from Settings / Pause | B | Escape |
| Restart whole race | Pause > Restart race | Pause > Restart race |
| Exploration map | View | M |

On the map: stick/WASD or mouse drag pans, triggers/wheel zoom, A/Space or
click places a waypoint, D-pad cycles destinations, and X or the Travel button
travels to a discovered safe destination in free roam. B/Esc closes the map.
The Exploration menu offers a separately confirmed acorn-only restart; existing
found IDs remain collected after relocation unless you choose that restart.

Menus also accept mouse clicks, D-pad/stick and arrow navigation. Restart restores
props and starts a new countdown; Y/R resets the vehicle locally while preserving race progress.

RT also brakes when moving backward. There is no separate handbrake in this phase.
Unity Input System actions support controller connection/disconnection and keyboard
fallback. Physical gamepad hardware still needs Dan's test.

## Historical Phase 3 vehicle tuning

The table below is historical. Current accepted Phase 4 tuning is in PROJECT_TODO.md
and the saved **PrototypeCar** in StreetLoopGreybox; Phase 7 does not alter it.
CR-010 is a scene-only override;
the base prefab and PrototypeTrack retain their original values.
Values are exposed on ArcadeVehicle unless noted. Speeds use metres/second.

| Parameter | Current value |
|---|---|
| Forward / reverse speed targets | 38 / 11 m/s (137 / 40 km/h) |
| Forward / reverse acceleration | 12 / 7 m/s² |
| Braking | 24 m/s² |
| Coasting drag | 0.45/s |
| Low / high speed steering angle | 33° / 10° (original 32° / 10°) |
| Steering / yaw response | 8 / 8 per second (original 7 / 8) |
| Wheelbase | 2.6 m |
| Lateral grip response / acceleration cap | 7/s / 22 m/s² |
| Suspension ray length | 0.8 m |
| Spring strength / damping | 65 / 8 |
| Suspension acceleration cap per probe before averaging | 45 m/s² |
| Upright strength / damping | 12 / 4 |
| Air stability fraction | 0.18 |
| Centre of mass offset | (0, -0.35, 0) m |
| Rigidbody mass / linear damping / angular damping | 1200 kg / 0.02 / 0.2 |
| Gamepad axis deadzone (VehicleInput code) | 0.12 to 0.95 |

Drive force tapers toward speed targets; these are not hard velocity clamps.
Four suspension probes support the rigidbody. Grounded drive and lateral grip act
at the centre of mass; damped upright assistance resists roll and follows slopes.
Airborne assistance is weaker. This is an arcade model, not tire simulation.
The car uses Ignore Raycast layer so ground/camera probes cannot hit its own body.

## Historical Phase 7 reset and camera

Reset returns the car upright to the existing **north-entrance spawn** in StreetLoopGreybox
(PrototypeTrack still uses Respawn Pad at (0, 1.1, -45)), clears velocity,
angular velocity and steering smoothing, and snaps the camera. Falling below y=-15
also resets automatically. A prefab without a scene spawn reference uses its initial
position and heading. Reset is fixed-pad recovery, not checkpoint recovery.
During a race it discards the current lap's checkpoint credit, retaining completed laps
and total time. Cross START to begin another attempt. Restart the entire race from Pause.

ChaseCamera follows in LateUpdate using position smoothing (0.16 s) and heading
smoothing (6/s), a (0, 3.6, -7.5) m offset, 3 m look-ahead and 65° field of view.
It keeps world-up rather than inheriting vehicle roll/pitch. A 0.3 m spherecast
shortens the camera arm around geometry and checks the smoothed position too.

## Test area and verification

StreetLoopGreybox uses three laps through the numbered gates. Timing starts at the
first forward START crossing. Wrong-way or skipped gates invalidate the current lap;
finish crossings cannot award a lap without all checkpoints in order. Required gates
allow the accepted shortcut and normal route between them.

Use `Docs/Phase7/VALIDATION.md` for current tests. Older Phase 3/4/5 full-race harnesses
predate countdown and changed bindings; do not use them as Phase 7 acceptance tests.
Phase7Validation runs with isolated records in a development player using
`-racerValidate <absolute-output-directory>`. It creates virtual input devices and
synthetic gate results; it is not a physical-controller or human driving test.
The original PrototypeTrack remains available for the Phase 1 tests described below.

The pad is 220 x 260 m with two turn islands and five slalom blocks. Facing forward
from spawn, the left ramp is 1.8 m high over 14 m; the right ramp is 5 m high over
18 m. These are temporary vehicle-test obstacles, not the final street or stunts.

In Play mode, **Racer > Validate Phase 1 (Play mode)** runs 20 checks and writes
`Logs/Phase1Validation.txt`. It temporarily uses scripted physics and virtual input
devices, then restores physics and removes those devices. Run while not manually
driving. See `Docs/PHASE1_VALIDATION.md` for recorded results and remaining tests.

Placeholder wheels do not animate. Hard impacts, ramp edges and awkward landings
can still flip the car; use reset. Enjoyment, camera comfort and physical controller
operation require hands-on acceptance. Phase 7 includes visible standalone validation.

Rendering uses Universal Render Pipeline (URP) 17.6.0 with a lightweight forward
renderer. Package versions are recorded in `Packages/manifest.json` and
`Packages/packages-lock.json`. No paid assets are used.

## Layout

All game assets live under `Assets/`:

- `Scenes`: Unity scenes, starting with `PrototypeTrack`.
- `Scripts`: modular input, vehicle physics, respawn and camera; `Editor` holds explicit authoring/validation tools.
- `Prefabs`: reusable objects.
- `Materials`, `Models`, `Audio`, `UI`: content by type.
- `Track`, `Vehicles`: content specific to those domains.
- `Tests`: reserved for meaningful tests as systems are introduced.
- `Settings`: rendering configuration.

## Version control

Commit `Assets` (including `.meta` files), `Packages`, `ProjectSettings`, and
project documentation. Unity uses visible metadata and text serialization.
Generated `Library`, `Temp`, `Logs`, builds, and local IDE settings are ignored.
Empty content folders have `.gitkeep` files so a fresh clone retains the layout.

Accepted Phase 0 is preserved in commit `12b1dff`, with the original foundation
at `c7dc529`. Phase 1 has its own local implementation commit. An existing GitHub
`origin` is configured; no commits were pushed.

Latest review: **0.11.0-review1 / CR-067–069**, awaiting Dan. Launch `Play-Racer.cmd` or the complete `Builds/Latest/Racer.exe` runtime. The southern hairpin/property correction, persisted household variety and delayed wrong-way guidance are documented in [validation and review checklist](Docs/CR067-069/VALIDATION.md). Current reset is local: keyboard R / Xbox-style controller Y. Earlier phase descriptions above are historical.

For portable music, put deliberately chosen songs in BundleMusic and run Package-Racer.cmd. Packaging preserves the previous complete Latest and verifies runtime/ZIP hashes. No external upload. The original household locations remain beside the neighborhood street in both scenes; the Forest racing line does not pass them.
