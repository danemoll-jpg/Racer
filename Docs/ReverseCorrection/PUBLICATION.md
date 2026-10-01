# Delivery — Backyard Reverse correction — 0.55.0-review1

Delivered 2026-09-30 local / 2026-10-01 UTC. [Published game-55000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-55000). [Gameplay evidence and limitations](VALIDATION.md), [route atlas](ATLAS.html).

## Source and build

- Safety checkpoint: `cad2c3f5c2885bb46c91adc9f7d8b6c6885701b1`.
- Completion source: `35a9dec0bc519d1422de4f877252afcd46e4964d`, pushed and verified on `origin/main` before building. [Push identity](source-push.txt).
- Fresh Unity 6000.6.1f1 Windows build: **Succeeded, 0 errors, 11 warnings, 4m27.62s**. Build 55000 / version 0.55.0-review1; GUID `9ee65f590e7b4b498ba823bbe7ccb346`. Build began `2026-10-01T02:55:08.5184449Z`. [Full report](build-release.txt).
- The warnings include existing mesh collision pre-bake/import advisories; no unrelated settings were changed to suppress them.
- Unity's generic executable hash is the same as the previous Unity-player bootstrap. This was a fresh build, not metadata substitution: new compiled Assembly-CSharp, eight scene data files and runtime assets were generated. [Exact changed inventory and source identity](runtime-identity.json).

## Signed publication and installation

- Existing signing identity and publisher used; previous releases retained and music pointer preserved. All **3 remote assets** have matching size/SHA-256: game ZIP (365,274,900 bytes), signed manifest and signed catalog. [Remote read-back](remote-release.json).
- Publisher initially created the draft but its immediate GitHub release-list read lagged. Read-only inspection confirmed the exact empty unpublished draft; the supported resume option completed upload and publication. No existing release was overwritten.
- The real public latest catalog and game were downloaded. Pinned signature verification, extraction/install and isolated muted startup all passed. All **235 files** in complete root `Builds/Latest` match the public signed manifest. [Hosted result](hosted/result.json).
- Production updater activated managed **55000**, retained **54000**, preserved music state, and reported no game/music updates pending. [Catalog check](launcher-catalog-check.json).
- **Unchanged `Play-Racer.cmd`** launched a responsive process at `C:\Users\danmo\Racer\Builds\Latest\versions\55000\Racer.exe`. [Actual launcher result](play-racer-launch.json). Player settings were temporarily muted and restored byte-for-byte with matching SHA-256. [Settings restoration](settings-preserved.json).
- Complete root runtime remains at `C:\Users\danmo\Racer\Builds\Latest\Racer.exe`; the managed updater path above is the normal launch destination. Future signed updating remains intact.

## Cleanup

[Measured cleanup](cleanup.json) ran only after all release/startup/launcher gates passed. Removed the disposable build output, duplicate game ZIP and isolated public-download installation.

- Builds: **9,327,589,508 → 7,413,746,854 bytes** (1,913,842,654 bytes recovered there).
- Total removed including temporary verification install: **3,464,765,623 bytes** (~3.46 GB).
- C: free: **321,461,665,792 → 324,927,385,600 bytes** (~324.93 GB free after cleanup).
- Retained complete Latest, current managed 55000, previous managed 54000, music, publisher/signing identity/tools, launcher, signed metadata, source, evidence and saves.

## Handoff

Targeted technical checks passed; no further tuning or broad testing. Dan's subjective handling/time-saving review remains. Initial failures and fixture limitations are retained. A subsequent documentation-only commit records this delivery; the playable build's completion source remains **35a9dec0**.
