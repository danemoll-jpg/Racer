# 0.79.0-review1 — delivery evidence

## Source

- Completion commit `d00e017e` pushed and verified equal to origin/main. It contains the implementation, evidence, TODO and
  project version 0.79.0-review1. **This is the playable source.**
- Safety checkpoint: `4d7a941d`.
- No temporary editor tool copies were committed: `Assets/Editor/Report079Temp` was removed before the commit and after
  the build. `VehicleGlazing.mat`, written by the play-mode checks, was reverted.

## Build

- Fresh Unity 6000.6.1f1 Windows build from `d00e017e`: Succeeded, 0 errors, 2 warnings, 2m17s, GUID
  `fe1f9c8d3e0b4eed9a8254341cb284d1` ([build-release.txt](build-release.txt), [build-done.txt](build-done.txt)).
- A scratch test player was built from the working tree only for the 4K bench. It was not published, and was deleted.

## Staging

- `Builds/Latest` root and `Builds/Latest/versions/79000` hold all 236 signed manifest files
  ([runtime-identity.json](runtime-identity.json)); 28 runtime files changed against 78000. The named launcher is preserved.

## Release

- Published [game-79000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-79000):
  `game-manifest.json` 47,521 bytes, `game.zip` 398,826,186 bytes, `update-catalog.json` 993 bytes.
- The first attempt hit the known draft-lookup miss after creating the draft. The empty draft (id 403998510, 0 assets)
  was inspected read-only, then `--resume-draft` uploaded the three assets and published. Previous releases are retained.

## Public verification ([hosted/result.json](hosted/result.json))

- The public catalog resolves to the 79000 manifest, and the pinned signature verified.
- All 236 Latest files match the public manifest.
- Public download, install and startup pass (exit 0).

## Activation and launch

- Production updater: nothing added, changed or removed, nothing pending
  ([launcher-catalog-check.json](launcher-catalog-check.json)). 79000 (current) and 78000 (previous) are kept.
- **Play-Racer.cmd (unchanged script):** launched a responsive `Builds/Latest/versions/79000/Racer.exe` (0.79.0-review1,
  build 79000) ([play-racer-launch.json](play-racer-launch.json)).
  - The check ran muted, and the settings were restored byte-for-byte ([settings-preserved.json](settings-preserved.json)).
  - No game or launcher process was left running.

## Cleanup ([cleanup.json](cleanup.json))

- **Builds:** 10,067,841,381 → 7,951,064,918 bytes (2,116,776,463 recovered): the build output folder and the duplicate
  `game.zip`; plus the 1,720,306,749-byte hosted verification install in the project `Temp`.
- **Outside the project:**
  - removed 2,658,086,758 bytes from `%LOCALAPPDATA%\Temp\report079` (the bench player, play-check output, shots);
    the evidence was copied into this folder first;
  - removed nine screenshot-key test captures from the player's `Screenshots` folder.
- **Retained:**
  - the Latest root, and managed 79000 + 78000;
  - the `LauncherRelease-79000` signed manifest and catalog;
  - music, publisher tools and keys, launcher, metadata;
  - source, evidence and saves. Debug report history is untouched.
- **C: free:** 255,517,986,816 bytes before cleanup; 262,017,355,776 bytes final.
