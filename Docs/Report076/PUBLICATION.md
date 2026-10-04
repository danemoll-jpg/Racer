# 0.76.0-review1 — delivery evidence

- **Source:**
  - Completion commit `03696d00f91a3860845381fecb52d6f635b9eeec`, pushed and verified equal to origin/main. It is the playable source.
  - Safety checkpoint: `8f2e4f95`.
  - No temporary editor tool copies were committed (`Assets/Editor/Report076Temp` removed before the commit and after the build); `VehicleGlazing.mat`, written by play-mode checks, was reverted.
- **Build:** fresh Unity 6000.6.1f1 Windows build from `03696d00`: Succeeded, 0 errors, 39 warnings, 6m44s, GUID `cbe513e24eaa4d9aaf831b835c659986` ([build-release.txt](build-release.txt), [build-done.txt](build-done.txt)). Scenes: the 8 courses plus `FreeRoamWorld` (level8).
- **Staged:** `Builds/Latest` root and `Builds/Latest/versions/76000` hold all 235 signed manifest files; 29 runtime files changed versus 75000 (levels 0–8, shared assets, resources, globalgamemanagers, boot.config, `Assembly-CSharp.dll`, README/VERSION/VALIDATION) ([runtime-identity.json](runtime-identity.json)). Named launcher preserved.
- **Release:** published [game-76000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-76000): `game-manifest.json` (47,393 bytes), `game.zip` (398,534,386 bytes), `update-catalog.json` (993 bytes). The first attempt hit the known draft-lookup miss after creating the draft; the empty draft (id 403129190, 0 assets) was inspected read-only and `--resume-draft` uploaded the three assets and published; the latest catalog was fetched back. Previous releases retained.
- **Public verification** ([hosted/result.json](hosted/result.json)): the public catalog resolves to the 76000 manifest, pinned signature verified; all 235 Latest files match the public manifest; public download, install and startup check pass (exit 0).
- **Activation:** the production updater reports nothing added/changed/removed and nothing pending ([launcher-catalog-check.json](launcher-catalog-check.json)); it retired its oldest retained version (74000) itself.
- **Play-Racer.cmd (unchanged script):** the first muted check timed out: the launcher stayed on its own window without starting the game (it auto-starts after 3 s only if it sees no keyboard or controller input, and the PC was in use at that moment). Closed and run again: launched a responsive `Builds/Latest/versions/76000/Racer.exe` ([play-racer-launch.json](play-racer-launch.json)); the muted check restored the original settings bytes both times ([settings-preserved.json](settings-preserved.json)). No game or launcher process left running.
- **Cleanup** ([cleanup.json](cleanup.json)):
  - Builds 10,149,122,192 → 8,033,624,052 bytes (2,115,498,140 recovered): build output folder and duplicate `game.zip`; plus the 1,719,319,403-byte hosted verification install in the project `Temp`.
  - Also removed 5,716,667,502 bytes outside the project (`%LOCALAPPDATA%\Temp\report076`: test players, probe and check output, renders); the evidence is copied into this folder. The test players' links to `Builds/Latest/Music` were detached before deletion; the bundled music is intact (807,133,422 bytes).
  - Retained: Latest root, managed 76000 + previous 75000, `LauncherRelease-76000` signed manifest and catalog, music, publisher tools/keys, launcher, metadata, source, evidence, saves.
  - C: free 281,086,062,592 bytes at the start of cleanup → 290,636,931,072 bytes final.
- **Debug report history:** untouched.
