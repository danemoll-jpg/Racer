# Delivery — 0.56.0-review1 / game-56000

Published 2026-10-01: [game-56000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-56000).

- Safety checkpoint: clean `main`, `8ab1294278a0708a6e7ab2e4ab5490d5c442eba9`.
- Completion source: `d31f58db8ad1e0f85c247c0f9ea6452c688444d0`, pushed and verified on origin/main before building. Git staging's sandbox index-lock denial was retried successfully with supported elevation.
- Fresh Unity 6000.6.1f1 Windows build: succeeded, **0 errors / 20 warnings**, 5m15.91s. Started 2026-10-01T04:24:24.7855192Z; build GUID `d33370c4af3248f9a190737076e9a5a2`. Warnings include existing pre-bake/import and obsolete API advisories; no unrelated warning-suppression changes.
- Complete 234-file runtime staged at root `Builds/Latest` and managed `Builds/Latest/versions/56000`. New compiled gameplay assembly, scene data and assets are inventoried in `runtime-identity.json`; the generic Unity bootstrap executable hash being unchanged does not represent an old-runtime reuse.
- Existing publisher/signing identity used. Three remote asset sizes/SHA-256 verified: 363,020,591-byte game ZIP, signed game manifest and signed catalog. The publisher's immediate draft-list read lagged; the exact empty unpublished draft was inspected and resumed through the supported option. Previous releases were not overwritten.
- Actual public catalog/download, pinned signature verification, extraction/install and isolated muted startup passed. Every complete Latest runtime file matches the public manifest. See `hosted/result.json` and `remote-release.json`.
- Production updater activated 56000, retained 55000, preserved music, and reported no pending game/music updates. **Unchanged `Play-Racer.cmd` launched responsive `C:\Users\danmo\Racer\Builds\Latest\versions\56000\Racer.exe`.** Settings were temporarily muted and restored byte-for-byte, with SHA-256 recorded in `settings-preserved.json`.
- Cleanup ran after those gates: Builds **9,350,785,533 → 7,440,086,743 bytes**. Removed disposable build output, duplicate ZIP and isolated public-download install: **3,460,731,828 bytes total**. C: free **320,898,101,248 → 324,359,753,728 bytes**. Current/previous managed runtimes, root Latest, music, publisher keys/tools, launcher, signed metadata, source and evidence remain.

[Targeted results and limitations](VALIDATION.md) remain the gameplay evidence. No additional driving tests or subjective tuning after the requested checks. A subsequent documentation-only commit records this delivery; the playable source remains `d31f58db`. Stop and await Dan's gameplay review.
