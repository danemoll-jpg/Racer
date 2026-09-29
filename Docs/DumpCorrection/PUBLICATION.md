# Dump correction — 0.39.0-review1 / game-39000

Delivery complete. [Published release](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-39000). Wait for Dan to inspect the dump; no Reverse or optional shortcuts.

- Safety checkpoint: clean main `c27b59c777bb1fa7e01c225b0e1a13cab0039201`.
- Completion source: **e2d409a0ca674648bd3ea841a832401f3e79655b**, pushed and verified on origin/main before the fresh Windows build. Git's initial sandbox index-lock permission failure was retried successfully through supported elevation.
- [Sixteen targeted driving/geometry assertions pass](VALIDATION.md). Gameplay testing stopped there; no full lap, additional vehicle or course matrix. Dan's overall Forward acceptance is recorded, with this corrected dump awaiting his inspection.
- Fresh clean-cache Windows build: **Succeeded, zero errors, five existing warnings, 8m41.605s**; GUID `27f9d7e391984b91934e68bfa8d202e3`. [Build report](build-release.txt). Warnings concern future collision prebaking, intentionally absent runtime Pipeline configuration and existing obsolete API uses. Unity automatically bakes collision in this version.
- All **232** runtime files match the signed ZIP, complete root Latest and managed 39000. Gameplay assembly and scene data are freshly built from the completion source; Unity's bootstrap Racer.exe retains the same bytes as the previous build. [Runtime identity](runtime-identity.json).
- Published **2026-09-29T11:57:54Z**. Existing publisher verified SHA256/size for all three uploaded assets before publication, then fetched the new latest signed catalog. Previous releases and signing identity retained. [Remote release](remote-release.json).
- Actual public updater download, pinned signatures, inventory, extraction, activation and muted startup passed, exit 0, without NullReferenceException/MissingReferenceException. [Public download/startup](hosted/result.json).
- Production updater activated 39000, retained 38000, preserved soundtrack state and reports no game/music update pending. [Catalog result](launcher-catalog-check.json).
- Unmodified **Play-Racer.cmd** launched responsive `C:\Users\danmo\Racer\Builds\Latest\versions\39000\Racer.exe` at **2026-09-29T08:00:46-04:00**. Complete root `Builds\Latest\Racer.exe` is the same published runtime. Only verification-owned processes were closed. [Launch record](play-racer-launch.json).
- Cleanup removed the duplicate fresh runtime, upload ZIP and public test install. Only managed 39000 and previous 38000 remain; the updater already removed superseded 37000. Source, documentation/atlas, music, saves, signing keys/tools, launcher and metadata retained. Builds: **6.102 → 5.044 GiB**, **1.057 GiB** removed there. Total measured free-space recovery **1.874 GiB**; final C: free **299.563 GiB (321,653,518,336 bytes)**. [Cleanup paths/measurements](cleanup.json).

The final documentation/evidence commit is separate from the playable source commit above. No gameplay changes were made after that source commit.
