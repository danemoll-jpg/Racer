# Forest water jump release — 0.31.0-review1

Published latest: [game-31000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-31000).

Completion source commit: `08b625a14a789cf6d06fe56d89def8ddc3661884`, pushed to source main before building. Later delivery-documentation commits do not alter the game source represented by this release.

## Fresh build and installed identity

- Unity 6000.6.1f1, clean build cache, Windows x64, all six scenes. Build started 2026-09-28 07:07:55 UTC, succeeded in 4m55s.
- Unity build GUID: `8611571668674d029ff89e8042f1c986`.
- Complete current root runtime: `C:\Users\danmo\Racer\Builds\Latest\Racer.exe` and its associated data/support files.
- Normal launcher destination: `C:\Users\danmo\Racer\Builds\Latest\versions\31000\Racer.exe` and its matching complete runtime.
- Version/build: `0.31.0-review1` / `31000`. VERSION.txt contains the full source commit and build GUID.
- EXE SHA256: `07534cc5839d2c89c4fe45ec272c91b046ff3d64f14ab3f0a32221121e0ffff2`.
- Unity's bootstrap EXE hash is unchanged from 30000; this is not evidence of old game data. The fresh clean build produced changed scene files, shared assets, global managers and `Assembly-CSharp.dll`. All 220 runtime files were compared between fresh build output, packaged ZIP, both Latest locations and public signed inventory. The public updater independently downloaded and validated the package.
- Root Racer.exe previously duplicated the launcher; it now contains the actual Unity player from this build. `WoodstockRushLauncher.exe` remains intact.

## Publication and launcher checks

All three remote assets matched local SHA256 and sizes before publication; latest signed catalog was fetched afterward. Package ZIP: 240,053,261 bytes, SHA256 `05905443f7f68cfa6206286cf31c921c498a43bb55a819e4cd2b447fc974e52b`.

An initial GitHub list response lagged behind draft creation. The empty matching draft was inspected and resumed without overwriting any release. Existing publisher key and soundtrack catalog pointer were retained.

One genuine public download installed through the production updater into an isolated directory, verified its pinned signature/inventory and passed startup with no NullReferenceException or MissingReferenceException. The normal installation was then activated through that same updater, preserving soundtrack state and build 30000 for rollback. The catalog reports no game/music update pending.

`Play-Racer.cmd` was run on 2026-09-28 at 03:19 EDT. Its actual responsive child process was `Builds/Latest/versions/31000/Racer.exe`. The test closed only the processes it launched. Existing saves were not replaced.

Evidence: `runtime-identity.json`, `remote-release.json`, `package-verification.json`, `hosted/result.json`, `hosted/startup.json`, `hosted/game.log`, `launcher-catalog-check.json`, `play-racer-launch.json`, `build-release.txt` and `build-done.txt`.

## Validation and limits

Current Forest main is preserved; the existing southern House 3 Detour is optional with CP1 bypass, unchanged physical geometry. One actual normal-physics motorcycle pass cleared pool and lake in a continuous flight. Production main and optional progression checks passed without penalties; six-direction arrow checks found no colliders. House 3, the full Laurel scene, original shared assets and global vehicle/AI/recovery code remain unchanged. See [atlas](ATLAS.md) and targeted check files in this directory.

BuildResult was Succeeded. Its report counts one error diagnostic from the CLI's five-second request timeout while the build continued successfully, plus five warnings (mesh pre-bake, missing optional Pipeline runtime configuration and obsolete API calls). These are recorded rather than described as a warning-free build. Detailed human playtesting remains pending; no repeated subjective tuning was performed.

## Cleanup

Builds shrank from 7,883,858,282 to 5,227,358,098 bytes (4.868 GiB). Removed obsolete 0.29 runtime, duplicate fresh output directory, upload ZIP and temporary public test install after successful verification. All 190 old Music files had verified preserved copies; the package music README was archived here. Retained current root runtime, managed 31000 and previous 30000, music, saves, publisher identity/tools, launcher and signed metadata. Final free disk space: 330,407,596,032 bytes. Exact targets: `cleanup.json`.

The new shortcut elsewhere remains deferred until Dan reviews the atlas.
