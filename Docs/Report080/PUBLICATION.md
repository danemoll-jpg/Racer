# 0.80.0-review1 — delivery evidence

## Source

- Completion commit `1397143a` pushed and verified equal to origin/main. It contains the implementation, evidence, TODO and
  project version 0.80.0-review1. **This is the playable source.**
- Safety checkpoint: `88f3a761`.
- No temporary editor tool copies were committed: `Assets/Editor/Report080Temp` was removed before the commit and after
  the build. `VehicleGlazing.mat`, written by the play-mode checks, was reverted.

## Build

- Fresh Unity 6000.6.1f1 Windows build from `1397143a`: Succeeded, 0 errors, 2 warnings, 2m24s, GUID
  `64ae76dc0fcd4d619b5c3d8889441a1b` ([build-release.txt](build-release.txt), [build-done.txt](build-done.txt)).
- Scratch test players were built from the working tree only for the 4K bench. They were not published, and were deleted.

## Staging

- `Builds/Latest` root and `Builds/Latest/versions/80000` hold all 236 signed manifest files
  ([runtime-identity.json](runtime-identity.json)); 29 runtime files changed against 79000. The named launcher is preserved.

## Release

- Published [game-80000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-80000) at
  2026-10-05T23:39:30Z: `game-manifest.json` 47,429 bytes, `game.zip` 399,010,433 bytes, `update-catalog.json` 993 bytes.
- The first attempt hit the known draft-lookup miss after creating the draft. The empty draft (id 404173047, 0 assets)
  was inspected read-only, then `--resume-draft` uploaded the three assets and published. Previous releases are retained.

## Public verification ([hosted/result.json](hosted/result.json))

- The public catalog resolves to the 80000 manifest, and the pinned signature verified.
- All 236 Latest files match the public manifest.
- Public download, install and startup pass (exit 0).

## Activation and launch

- Production updater: nothing added, changed or removed, nothing pending
  ([launcher-catalog-check.json](launcher-catalog-check.json)). 80000 (current) and 79000 (previous) are kept.
- **Play-Racer.cmd (unchanged script):** launched a responsive `Builds/Latest/versions/80000/Racer.exe` (0.80.0-review1,
  build 80000) ([play-racer-launch.json](play-racer-launch.json)).
  - The check ran muted, and the settings were restored byte-for-byte ([settings-preserved.json](settings-preserved.json)).
  - No game or launcher process was left running.

## Cleanup ([cleanup.json](cleanup.json))

- **Builds:** 10,069,775,295 → 7,952,276,087 bytes (2,117,499,208 recovered): the build output folder and the duplicate
  `game.zip`; plus the 1,720,845,086-byte hosted verification install in the project `Temp`.
- **Outside the project:** removed 1,876,158,718 bytes from `%LOCALAPPDATA%\Temp\report080` (check output, bench
  players, shots); the evidence was copied into this folder first.
- **Retained:** the Latest root, and managed 80000 + 79000; the `LauncherRelease-80000` signed manifest and catalog;
  music, publisher tools and keys, launcher, metadata; source, evidence and saves. Debug report history is untouched.
- **C: free:** 252,406,616,064 bytes before cleanup; 258,123,685,888 bytes final.
