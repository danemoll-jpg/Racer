# 0.77.0-review1 — delivery evidence

## Source

- Completion commits pushed and verified equal to origin/main:
  - `dd0bab13`: implementation, evidence, TODO.
  - `952de374`: project version 0.77.0-review1. **This is the playable source.**
- Safety checkpoint: `9fe109d4`.
- No temporary editor tool copies were committed. `Assets/Editor/Report077Temp` was removed before the commit and after the build. `VehicleGlazing.mat`, written by the play-mode checks, was reverted.

## Build

- Fresh Unity 6000.6.1f1 Windows build from `952de374`: Succeeded, 0 errors, 2 warnings, 3m21s, GUID `9db22702f57446f9a009399cd593a2da` ([build-release.txt](build-release.txt), [build-done.txt](build-done.txt)).
- A separate scratch test player was built from the working tree before the commit, only for the 4K in-game probe. It is not published, and it was deleted.

## Staging

- `Builds/Latest` root and `Builds/Latest/versions/77000` hold all 236 signed manifest files ([runtime-identity.json](runtime-identity.json)).
- 29 runtime files changed against 76000. `Racer.exe` (the Unity player) is byte-identical to 0.76; the game code and data are in `Racer_Data`.
- The named launcher is preserved.

## Release

- Published [game-77000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-77000) with three assets:

  | Asset | Size |
  | --- | --- |
  | `game-manifest.json` | 47,465 bytes |
  | `game.zip` | 398,549,138 bytes |
  | `update-catalog.json` | 993 bytes |

- The first attempt hit the known draft-lookup miss after creating the draft. The empty draft (id 403229488, 0 assets) was inspected read-only, then `--resume-draft` uploaded the three assets and published.
- It is the latest release, not a draft or prerelease. Previous releases are retained.

## Public verification ([hosted/result.json](hosted/result.json))

- The public catalog resolves to the 77000 manifest, and its pinned signature is verified.
- All 236 Latest files match the public manifest.
- The public download, install and startup check pass (exit 0).

## Activation and launch

- **Activation:** the production updater reports nothing added, changed or removed, and nothing pending ([launcher-catalog-check.json](launcher-catalog-check.json)). It retired 75000 itself; 77000 (current) and 76000 (previous) are kept.
- **Play-Racer.cmd (unchanged script):**
  - It launched a responsive `Builds/Latest/versions/77000/Racer.exe` (0.77.0-review1, build 77000) ([play-racer-launch.json](play-racer-launch.json)).
  - The temporary master mute was removed and the settings restored byte-for-byte ([settings-preserved.json](settings-preserved.json)).
  - No game or launcher process was left running.

## Cleanup ([cleanup.json](cleanup.json))

- **Builds:** 10,063,705,343 → 7,948,158,290 bytes (2,115,547,053 recovered). Removed the build output folder and the duplicate `game.zip`, plus the 1,719,354,158-byte hosted verification install in the project `Temp`.
- **Outside the project:** removed 1,856,595,655 bytes (`%LOCALAPPDATA%\Temp\report077`: the test player, play-check output and renders). The evidence is copied into this folder.
- **Screenshots folder:** the five screenshots-key test captures were removed from the player's `Screenshots` folder, which is new this round, so it starts empty.
- **Retained:**
  - the Latest root and managed 77000 + 76000;
  - the `LauncherRelease-77000` signed manifest and catalog;
  - music, publisher tools and keys, the launcher and metadata;
  - source, evidence and saves.
- **C: free:** 282,636,931,072 bytes before the build; 284,221,530,112 bytes final.
- **Debug report history:** untouched.
