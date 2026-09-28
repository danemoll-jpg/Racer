# Release 0.35.0-review1 / game-35000

Safety checkpoint: clean main `eb65014b114f3e86fe5126b3fcb26b2f6b0f3110`. Completion source `d50142fab4a364970f1f1dfd444cdbb79e942d4e`, pushed and verified on origin/main before the build. Git staging permission denial was resolved through the supported elevated retry.

Fresh Unity 6000.6.1f1 Windows build: succeeded, zero errors/five existing warnings, 6m03s; GUID `43f3ed5cf2354018b03e6ee3518008a7`. Started 2026-09-28 23:25:02 UTC with 321,734,144,000 free bytes. Source identity is embedded in VERSION.txt and signed release notes.

All 220 fresh runtime files match the signed ZIP, complete root Builds/Latest and managed versions/35000. All six scene data files changed. Unity freshly emitted identical bootstrap EXE bytes; the corrected world is in the new scene/runtime data. No gameplay system code changed. The named launcher and soundtrack were preserved.

[Published release](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-35000). All three assets verified remotely by SHA256/size; published latest, not draft/prerelease. Existing signing identity used; signed catalog points to game-35000 and retains the soundtrack pointer. Real public updater download passed pinned signatures, complete inventory and isolated startup (exit 0), with no NullReferenceException/MissingReferenceException. See remote-assets.json, runtime-identity.json and hosted/result.json.

Production activation initially stopped safely because the existing 34000 Racer game and launcher held the installation lock. Dan closed them, then activation resumed using the same published runtime without rebuilding. Unmodified Play-Racer.cmd launched responsive `Builds/Latest/versions/35000/Racer.exe` at 19:37:55 EDT. Public catalog reports no pending game/music update. Current 35000 and previous 34000 are retained; soundtrack pointer unchanged. Only verification-owned processes were closed afterward.

Mandatory cleanup completed after publication, public startup and production launcher verification:
- Builds: **6,298,202,813 → 5,240,041,973 bytes (5.866 → 4.880 GiB)**.
- Builds space recovered: **1,058,160,840 bytes (0.985 GiB)**.
- Total free-space gain during cleanup, including temporary public install: **1,876,000,768 bytes (1.747 GiB)**.
- Final C: free: **321,237,143,552 bytes (299.175 GiB)**.
- Removed duplicate fresh build output, uploaded ZIP and temporary public install. Updater already retired obsolete managed 33000; only 34000/35000 remain. Source/Git, current complete root runtime, both required managed runtimes, signing identity/tools, launcher/SDK, music, catalogs/manifests, atlas and evidence preserved. See cleanup.json.

Implementation and targeted testing are complete. PROJECT_TODO.md records the current entrance and accepted previous systems. This final documentation commit changes no playable source; the release remains built from `d50142fab4a364970f1f1dfd444cdbb79e942d4e`.
