# Racer

A small single-player PC arcade racing game inspired by a childhood street.
`PROJECT_TODO.md` is the source of truth for scope, phases, and acceptance.

## Current scope

Phase 0 establishes the project only. No vehicle, track geometry, racing logic,
shortcuts, jumps, or gameplay systems are implemented. Phase 1 requires explicit
approval.

## Open

Use Unity **6000.6.1f1** (Unity 6). Add this repository root to Unity Hub,
open it, allow package restoration/import to finish, then open
`Assets/Scenes/PrototypeTrack.unity`. The scene intentionally has no GameObjects;
an empty Game view with no camera is expected.

Rendering uses Universal Render Pipeline (URP) 17.6.0 with a lightweight forward
renderer. Package versions are recorded in `Packages/manifest.json` and
`Packages/packages-lock.json`. No paid assets are used.

## Layout

All game assets live under `Assets/`:

- `Scenes`: Unity scenes, starting with `PrototypeTrack`.
- `Scripts`: future game code.
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

The local baseline commit is the Phase 0 recovery point. An existing GitHub
`origin` is configured; this session does not push any commits.
