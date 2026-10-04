# 0.75.0-review1 — delivery evidence

- **Source:**
  - Completion commit `e1182daf671796f6f197d1b4fe9fb5aa3df82cd3`, pushed and verified equal to origin/main. It is the playable source.
  - Safety checkpoint: `58d7df9d`.
  - No temporary editor tool copies were committed (probe and build-script copies in `Assets/Editor` removed before the commit and after the build). `VehicleGlazing.mat`, written by play-mode checks, was reverted.
- **Build:** fresh Unity 6000.6.1f1 Windows build from `e1182daf`: Succeeded, 0 errors, 2 warnings, 2m35s, GUID `e0b46f720c1f4d6abea0de640f348a05` ([build-release.txt](build-release.txt), [build-done.txt](build-done.txt)).
- **Staged:** `Builds/Latest` root and `Builds/Latest/versions/75000` hold all 232 signed manifest files; 26 runtime files changed versus 74000 (scene data level0–7, shared assets, resources, globalgamemanagers, boot.config, `Assembly-CSharp.dll`, README/VERSION/VALIDATION) ([runtime-identity.json](runtime-identity.json)). Named launcher preserved.
- **Release:** published [game-75000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-75000): `game-manifest.json` (46,869 bytes), `game.zip` (419,190,499 bytes), `update-catalog.json` (993 bytes). The first attempt hit the known draft-lookup miss after creating the draft; the empty draft (id 403008556, 0 assets) was inspected read-only and `--resume-draft` uploaded the three assets and published; the latest catalog was fetched back. Previous releases retained.
- **Public verification** ([hosted/result.json](hosted/result.json)): the public catalog resolves to the 75000 manifest, pinned signature verified; all 232 Latest files match the public manifest; public download, install and startup check pass (exit 0).
- **Activation:** the production updater reports nothing added/changed/removed and nothing pending ([launcher-catalog-check.json](launcher-catalog-check.json)).
- **Play-Racer.cmd (unchanged script):** launched a responsive `Builds/Latest/versions/75000/Racer.exe` ([play-racer-launch.json](play-racer-launch.json)); the muted check restored the original settings bytes ([settings-preserved.json](settings-preserved.json)). No game process left running.
- **Cleanup** ([cleanup.json](cleanup.json)):
  - Builds 10,422,915,497 → 8,201,176,166 bytes (2,221,739,331 recovered): build output folder and duplicate `game.zip`; plus the 1.80 GB hosted verification install in the project `Temp`.
  - Also removed 1,953,310,567 bytes outside the project (`%LOCALAPPDATA%\Temp\report075`: probe output, the 4K test player and bench output; render scratch); the evidence is copied into this folder.
  - Retained: Latest root, managed 75000 + previous 74000 (73000 was already gone), `LauncherRelease-75000` signed manifest and catalog, music, publisher tools/keys, launcher, metadata, source, evidence, saves.
  - C: free 289,423,347,712 bytes at the start of cleanup → 295,396,601,856 bytes final.
- **Debug report history:** untouched.
