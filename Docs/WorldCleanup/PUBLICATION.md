# World cleanup delivery — 0.45.0-review1 / game-45000

- Safety checkpoint: clean main `132bc5719436a5cebdcc76c3c16d1816e088ee13`.
- Implementation source `6bb604b7d12d118a7d73dc9ab0a9654bfbe1d339`; final completion source `a6d467a4834b35a0c9f0f414433121fbae2dc17b`, pushed and verified on origin/main before the final build.
- Fresh Unity 6000.6.1f1 Windows build: zero errors, 11 warnings, 10m28.624s; GUID `9f9d41f463244172a25d4537e732b35a`. Build started 2026-09-30 01:07:31 UTC. Full report: [build-release.txt](build-release.txt).
- Targeted checks: first candidate 40/41; only Anderson arrival failed. Corrected house-front arrival passes Editor and final Windows player (exit 0). Initial failed evidence retained. [Validation and limits](VALIDATION.md). Testing stopped.
- [Published release](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-45000): all three assets uploaded through existing publisher, latest signed catalog fetched and verified. Previous releases retained.
- All 232 signed package/runtime/root Latest/managed files match. Unity's platform bootstrap Racer.exe hash is unchanged from the previous version; fresh scene data, resources and Assembly-CSharp.dll have changed and match the new manifest. This is a complete freshly built runtime, not old data with new metadata. [Identity](runtime-identity.json).
- Actual public updater download, pinned signatures, extraction and startup passed, exit 0; all Latest files match public manifest. [Hosted result](hosted/result.json).
- Existing updater activated build 45000 and preserved soundtrack state. Public catalog reports no game/music update pending. [Catalog check](launcher-catalog-check.json).
- Unchanged `Play-Racer.cmd` launched responsive `Builds/Latest/versions/45000/Racer.exe`. Complete root `Builds/Latest/Racer.exe` runtime matches. [Launch evidence](play-racer-launch.json).
- Cleanup: Builds 9,806,284,467 -> 6,776,167,225 bytes, recovering 3,030,117,242 bytes. Separate temporary public install removed: 1,354,979,412 bytes. Total measured deleted artifacts: 4,385,096,654 bytes. C: free 323,676,155,904 -> 328,061,792,256 bytes (305.531 GiB final). [Cleanup](cleanup.json).
- Retained managed 45000 and immediately previous 44000, full root runtime, required evidence/maps/source, music, keys, publisher tools, launcher metadata and saves. Production updater had already retired managed 43000; cleanup did not remove any required rollback runtime.

Backyard Forward and accepted shortcuts remain protected. Reverse is upcoming and was not built. STOP for Dan's review.
