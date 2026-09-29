# Dense dump delivery — 0.40.0-review1 / game-40000

Delivery complete. [Published release game-40000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-40000). Stop and await Dan's gameplay review.

- Safety checkpoint: clean main `6396cb50de28a157ee1a3b3b34f79d6d7d6873a9`.
- Completion source: **c701d791f2a2648bca1bf72ab8ed69f07d735ccd**, pushed and verified on origin/main before the fresh Windows build.
- [21 final targeted assertions pass](VALIDATION.md). Gameplay tests stopped. No Reverse, shortcuts, AI races or broad matrix.
- Git staging initially failed on sandbox index-lock permissions, then succeeded through supported elevation. Automatic push review initially rejected the combined push/build command; read-only verification established the existing remote at the safety checkpoint and the retry supplied the user's mandated CODEX_RULES.md sections 7/14 explicitly authorizing push. The narrowly scoped push succeeded. No hooks/signing/history bypass.
- Fresh clean-cache Windows build **Succeeded: zero errors, seven warnings, 6m47.154s**, GUID `420a0a5060a34fd796c6157c529a459a`. [Build report](build-release.txt). Existing collision-prebake/runtime-Pipeline/obsolete API notices plus two deprecation notices for the new component's supported vehicle-discovery overload. No compiler/player-build failure.
- All **231 files** match the signed ZIP, fresh output, complete root Latest and managed 40000. [Identity](runtime-identity.json). Fresh gameplay assembly/data differ; Unity's bootstrap EXE remains byte-identical as expected. Named launcher unchanged.
- Known `VehicleVisual` Editor play-mode glazing mutation was saved when the build began (`_SrcBlend` 1 to 5); restored only that source property through Unity afterward. Existing runtime `VehicleVisual` always sets 5 for every vehicle, so this does not change delivered vehicle behavior. No unrelated source change retained.
- Publisher created draft game-40000 but its immediate release-list query missed it. Read-only lookup verified one empty draft (399223805); resumed with the existing publisher's `--resume-draft`, no duplicate/overwrite.
- Published **2026-09-29T14:22:14Z**, all three assets independently verified by remote size/SHA-256; latest signed catalog fetched and verified. Existing soundtrack pointer and signing identity preserved. [Remote release](remote-release.json).
- Actual public updater download, pinned signature/inventory/extraction/activation and isolated muted startup **PASS**, exit 0, no NullReferenceException/MissingReferenceException. All public manifest files match local Latest. [Public startup](hosted/result.json).
- Production updater activated 40000, retained previous 39000, removed superseded 38000 and preserved soundtrack state. No game/music update pending. [Catalog check](launcher-catalog-check.json).
- Unmodified **Play-Racer.cmd** launched responsive `C:/Users/danmo/Racer/Builds/Latest/versions/40000/Racer.exe` at 10:25:19 EDT. Complete root `Builds/Latest/Racer.exe` matches the same published inventory. Only verification-owned processes closed. [Launch record](play-racer-launch.json).
- Cleanup removed the duplicate fresh runtime, upload ZIP and temporary public-test install. Builds **6,568,958,975 → 5,425,821,786 bytes**, recovering **1,143,137,189 bytes** there. Total measured C: gain **2,023,194,624 bytes**; final free **320,103,231,488 bytes**. Current and previous managed runtimes, music, saves, keys/tools, launcher, metadata, source and evidence retained. [Exact targets and measurements](cleanup.json).

Only the existing publisher, signing identity and launcher are used. Soundtrack and user saves/settings are preserved. Final evidence/documentation commit will be separate from the playable source above.
