# Publication — 0.61.0-review1 / game-61000

- Safety checkpoint: clean `b29330bc4ceecf8eda2dba77697583f538db002e`.
- Completion source: `56e63e4797d912f5e5822dee9f39eb3afc47919a`, pushed and remotely verified on origin/main before building. Delivery evidence is committed separately afterward; no gameplay changes after that source commit.
- Fresh Unity Windows build: success, zero errors, 21 warnings, 3m33s. Warnings include future mesh-collision pre-bake requirements and existing obsolete API calls. See build-release.txt for the full record.
- Published [game-61000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-61000). Signed game manifest, complete game ZIP and latest signed catalog match their remote hashes. GitHub initially lagged in listing the newly created empty draft; verified it directly and resumed that same draft without replacing any release.
- Complete 233-file runtime verified at Builds/Latest and managed versions/61000. Runtime data and Assembly-CSharp.dll changed from the previous build; Racer.exe is Unity's unchanged bootstrap executable, not evidence of an old game. VERSION.txt identifies the new source and build GUID.
- Public catalog fetch, pinned signature verification, actual public download/install, and isolated launcher startup all passed. Every installed Latest runtime file matches the public signed manifest.
- Existing updater activated game 61000. Soundtrack state unchanged; final catalog check has no pending game/music update.
- Unchanged Play-Racer.cmd launched responsive `Builds/Latest/versions/61000/Racer.exe`. Temporary verification mute was removed; original player settings restored byte-for-byte.

## Cleanup

- Builds: **9,565,965,293 → 7,593,020,763 bytes**.
- Builds reduction: 1,972,944,530 bytes; total disposable files removed including temporary public install: **3,578,626,950 bytes**.
- C: free: **290,770,419,712 → 294,350,192,640 bytes**.
- Retained complete Latest, current managed 61000 and immediately previous managed 60000, source/evidence, music, saves, publisher keys/tools, launcher and active signed metadata.

Technical checks and delivery are complete. STOP for Dan's gameplay review; CR-118 and physical Steam Deck review remain open/deferred as recorded in PROJECT_TODO.md.
