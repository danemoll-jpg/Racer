# 0.78.0-review1 — delivery evidence

## Source

- Completion commit `1f2de343` (implementation, evidence, TODO, project version 0.78.0-review1) pushed and verified equal to origin/main. **This is the playable source.**
- Safety checkpoint: `a696e27a`.
- No temporary editor tool copies were committed: `Assets/Editor/Report078Temp` was removed before the commit and after the build. `VehicleGlazing.mat`, written by the play-mode checks, was reverted.

## Build

- Fresh Unity 6000.6.1f1 Windows build from `1f2de343`: Succeeded, 0 errors, 2 warnings, 2m42s, GUID `e40f71640a2f4af780c564b0d28853f1` ([build-release.txt](build-release.txt), [build-done.txt](build-done.txt)).
- Scratch test players were built from the working tree only for the 4K bench; not published, deleted.

## Staging

- `Builds/Latest` root and `Builds/Latest/versions/78000` hold all 236 signed manifest files ([runtime-identity.json](runtime-identity.json)); 29 runtime files changed against 77000. `Racer.exe` (the Unity player) is byte-identical to 0.77; the game is in `Racer_Data`. The named launcher is preserved.

## Release

- Published [game-78000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-78000): `game-manifest.json` 47,465 bytes, `game.zip` 398,792,984 bytes, `update-catalog.json` 993 bytes.
- The first attempt hit the known draft-lookup miss after creating the draft. The empty draft (id 403363714, 0 assets) was inspected read-only, then `--resume-draft` uploaded the three assets and published. Latest release, not a draft; previous releases retained.

## Public verification ([hosted/result.json](hosted/result.json))

- The public catalog resolves to the 78000 manifest; pinned signature verified; all 236 Latest files match the public manifest; public download, install and startup pass (exit 0).

## Activation and launch

- Production updater: nothing added, changed or removed, nothing pending ([launcher-catalog-check.json](launcher-catalog-check.json)); it retired 76000; 78000 (current) and 77000 (previous) kept.
- **Play-Racer.cmd (unchanged script):** launched a responsive `Builds/Latest/versions/78000/Racer.exe` (0.78.0-review1, build 78000) ([play-racer-launch.json](play-racer-launch.json)), muted, settings restored byte-for-byte ([settings-preserved.json](settings-preserved.json)); no game or launcher process left running.

## Cleanup ([cleanup.json](cleanup.json))

- **Builds:** 10,066,636,651 → 7,949,978,005 bytes (2,116,658,646 recovered): build output folder and the duplicate `game.zip`; plus the 1,720,221,964-byte hosted verification install in the project `Temp`.
- **Outside the project:** removed 2,028,384,851 bytes (`%LOCALAPPDATA%\Temp\report078`: test players, play-check output, renders); evidence copied into this folder. One screenshot-key test capture removed from the player's `Screenshots` folder.
- **Retained:** the Latest root and managed 78000 + 77000; the `LauncherRelease-78000` signed manifest and catalog; music, publisher tools and keys, launcher, metadata; source, evidence, saves. Debug report history untouched.
- **C: free:** 272,793,202,688 bytes before the build; 274,774,908,928 bytes final.
