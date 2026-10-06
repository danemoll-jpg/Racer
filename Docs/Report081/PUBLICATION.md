# 0.81.0-review1 — delivery evidence

## Source

- Completion commit `badc4857` (`badc48577f94a2840ee28e641cb2a9ccfe9e8031`) pushed and verified equal to origin/main. It
  contains the implementation, evidence, TODO and project version 0.81.0-review1. **This is the playable source.**
- Safety checkpoint: `c78abde8`; Part 0 work commit `15c9d8a9`.
- No temporary editor tool copies were committed: `Assets/Editor/Report081Temp` was removed before the commit and after
  the build. `VehicleGlazing.mat`, written by the play-mode checks, was reverted.

## Build

- Fresh Unity 6000.6.1f1 Windows build from `badc4857`: Succeeded, 0 errors, 2 warnings, 2m43s, GUID
  `4058c6f8182841adb9c1eeab29d2f180` ([build-release.txt](build-release.txt), [build-done.txt](build-done.txt)).
- A scratch test player was built from the working tree only for the 4K bench; not published, deleted.

## Staging

- `Builds/Latest` root and `Builds/Latest/versions/81000` hold all 234 signed manifest files
  ([runtime-identity.json](runtime-identity.json)); 26 runtime files changed against 80000 (the two VALIDATION.md copies
  are no longer shipped). The named launcher is preserved.

## Release

- Published [game-81000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-81000) at
  2026-10-06T01:42:41Z: `game-manifest.json` 47,181 bytes, `game.zip` 400,946,263 bytes, `update-catalog.json` 993 bytes.
- The first attempt hit the known draft-lookup miss after creating the draft. The empty draft (id 404225753, 0 assets)
  was inspected read-only, then `--resume-draft` uploaded the three assets and published. Previous releases are retained.

## Public verification ([hosted/result.json](hosted/result.json))

- The public catalog resolves to the 81000 manifest, and the pinned signature verified.
- All 234 Latest files match the public manifest. Public download, install and startup pass (exit 0).

## Activation and launch

- Production updater: nothing added, changed or removed, nothing pending
  ([launcher-catalog-check.json](launcher-catalog-check.json)). 81000 (current) and 80000 (previous) are kept.
- **Play-Racer.cmd (unchanged script):** launched a responsive `Builds/Latest/versions/81000/Racer.exe` (0.81.0-review1,
  build 81000) ([play-racer-launch.json](play-racer-launch.json)), muted, settings restored byte-for-byte
  ([settings-preserved.json](settings-preserved.json)). No game or launcher process was left running.

## Cleanup ([cleanup.json](cleanup.json))

- **Builds:** 10,092,961,132 → 7,966,638,546 bytes (2,126,322,586 recovered): the build output folder and the duplicate
  `game.zip`; plus the 1,727,732,235-byte hosted verification install in the project `Temp`.
- **Outside the project:** removed 1,873,610,321 bytes from `%LOCALAPPDATA%\Temp\report081` (check output, bench player,
  renders); the evidence was copied into this folder first.
- **Retained:** the Latest root, managed 81000 + 80000, the `LauncherRelease-81000` signed manifest and catalog, music,
  publisher tools and keys, launcher, metadata, source, evidence, saves. Debug report history is untouched.
- **C: free:** 251,708,329,984 bytes before cleanup; 257,447,268,352 bytes final.
