# Delivered — UI Phases 3–5 / 0.49.0-review1 / game-49000

- One combined review build, retaining accepted Phase 1–2. Phases 3–5 await Dan's gameplay/UI acceptance.
- Safety checkpoint: clean main `69d8d2e44c77ef45e2e69d0b43f5d28b6acd7da8`.
- Completion source: **`c768b5d4622e4098e59736f1586b4619af255481`**, pushed and verified on origin/main before building. Later changes are delivery documentation/evidence only.
- Fresh Unity 6000.6.1f1 Windows build: **0 errors, 11 warnings, 7m20.764s**, started 2026-09-30 14:43:24 UTC; GUID `4de55612d2374e51a4280255d2862430`. C: free before build: 330,493,513,728 bytes. Existing collision-prebaking and optional RuntimePipelineConfig warnings are retained in `build-release.txt`.
- [Published game-49000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-49000). All three immutable remote asset sizes and SHA-256 digests match the prepared inventory; latest signed catalog fetched and verified. Existing signing identity and prior releases retained. The publisher's immediate post-create listing missed the new draft; inspection confirmed the empty unpublished draft, and its supported `--resume-draft` path completed publication without overwriting assets.
- **All 230 signed files match** the fresh output, ZIP, complete root Latest and managed 49000. Game assembly and runtime data changed; Unity's bootstrap executable bytes match the prior release. `runtime-identity.json` records the complete-runtime comparison.
- Public download, pinned-signature validation, extraction and muted isolated startup passed, exit 0. The startup log contains no errors or exceptions. All complete root Latest files match the public manifest. See `hosted/result.json` and `hosted/startup.json`.
- The existing production updater activated 49000, preserved music state and reports no pending game/music updates (`launcher-catalog-check.json`).
- **Unchanged Play-Racer.cmd** launched responsive `Builds/Latest/versions/49000/Racer.exe` at 11:02 EDT. Complete root `Builds/Latest/Racer.exe` represents the same release. Only verification-created processes were closed (`play-racer-launch.json`).
- Cleanup: Builds **8,923,478,965 → 7,130,480,199 bytes**, recovering **1,792,998,766 bytes** there. Including the temporary public installation, **3,240,249,343 bytes** of disposable artifacts removed. C: free **327,233,081,344 → 330,472,349,696 bytes** (`cleanup.json`).
- Retained complete root Latest, managed current **49000**, previous **48000**, music, publisher identity/tools, launcher metadata, user saves, source and evidence. The updater retired managed 47000 before manual cleanup.

Targeted implementation checks and limitations: [VALIDATION.md](VALIDATION.md). Physical-controller, Steam Deck and subjective UI acceptance remain for Dan; no further feature testing or tuning was performed.
