# Racer

A small single-player PC arcade racing game inspired by a childhood street.
`PROJECT_TODO.md` is the source of truth for scope, phases, and acceptance.

## Current scope

Phase 0 is accepted. Phase 1 provides one arcade-driving prototype in a grey-box
test area. Driving feel and physical controller acceptance await Dan's review.
Phase 2 has not begun. No street, race rules, menus, AI, or final stunts are included.

## Open

Use Unity **6000.6.1f1** (Unity 6). Add this repository root to Unity Hub,
open it, allow package restoration/import to finish, then open
`Assets/Scenes/PrototypeTrack.unity`. Press Play, then click the Game view for
keyboard focus. The Editor may suspend play while in the background.

## Controls

| Action | Xbox-compatible gamepad | Keyboard |
|---|---|---|
| Accelerate | RT | W / Up |
| Brake, then reverse when held | LT | S / Down |
| Steer | Left stick | A/D / Left/Right |
| Reset to starting pad | Y | R |

RT also brakes when moving backward. There is no separate handbrake in this phase.
Unity Input System actions support controller connection/disconnection and keyboard
fallback. Physical gamepad hardware still needs Dan's test.

## Vehicle tuning

Select **Prototype Car** in the scene (or edit `Assets/Prefabs/PrototypeCar.prefab`).
Values are exposed on ArcadeVehicle unless noted. Speeds use metres/second.

| Parameter | Current value |
|---|---|
| Forward / reverse speed targets | 38 / 11 m/s (137 / 40 km/h) |
| Forward / reverse acceleration | 12 / 7 m/s² |
| Braking | 24 m/s² |
| Coasting drag | 0.45/s |
| Low / high speed steering angle | 32° / 10° |
| Steering / yaw response | 7 / 8 per second |
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

## Reset and camera

Reset returns the car upright to **Respawn Pad** at (0, 1.1, -45), clears velocity,
angular velocity and steering smoothing, and snaps the camera. Falling below y=-15
also resets automatically. A prefab without a scene spawn reference uses its initial
position and heading. Reset is fixed-pad recovery, not checkpoint recovery.

ChaseCamera follows in LateUpdate using position smoothing (0.16 s) and heading
smoothing (6/s), a (0, 3.6, -7.5) m offset, 3 m look-ahead and 65° field of view.
It keeps world-up rather than inheriting vehicle roll/pitch. A 0.3 m spherecast
shortens the camera arm around geometry and checks the smoothed position too.

## Test area and verification

The pad is 220 x 260 m with two turn islands and five slalom blocks. Facing forward
from spawn, the left ramp is 1.8 m high over 14 m; the right ramp is 5 m high over
18 m. These are temporary vehicle-test obstacles, not the final street or stunts.

In Play mode, **Racer > Validate Phase 1 (Play mode)** runs 20 checks and writes
`Logs/Phase1Validation.txt`. It temporarily uses scripted physics and virtual input
devices, then restores physics and removes those devices. Run while not manually
driving. See `Docs/PHASE1_VALIDATION.md` for recorded results and remaining tests.

Placeholder wheels do not animate. Hard impacts, ramp edges and awkward landings
can still flip the car; use reset. Enjoyment, camera comfort and physical controller
operation require hands-on acceptance. No standalone player build was tested.

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
