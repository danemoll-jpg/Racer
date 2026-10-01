# Backyard Reverse optional shortcuts — publication

- Safety checkpoint: clean main `771bb757ae1fc8383cd9b73a1b964c71ca3b4ca5`.
- Completion source: `36ad4ce8dc18ab9d249a3693d3f0b1e6e715bccd`, pushed and remotely verified on origin/main before the fresh Windows build.
- Target: 0.54.0-review1 / game-54000. See [validation](VALIDATION.md) and [atlas](ATLAS.html). Player-only branches; Dan reviews handling and actual time savings. Targeted testing stopped.
- Existing publisher/signing identity and launcher architecture retained. Delivery results follow when verified.

- Fresh Unity Windows build succeeded in 3m52.781s, GUID `3be61b9ddf1649b4a95585fe252f1575`, from completion source `36ad4ce8dc18ab9d249a3693d3f0b1e6e715bccd`. Free space before build: 326,379,061,248 bytes. Report records 21 warnings and one error: the error is the concurrent CLI status request timing out after 5000ms while Unity's main thread was building. Runtime compilation and player build succeeded. Warnings include existing obsolete APIs/collider-baking notices, disabled development Pipeline runtime, and pending editor-script changes after whitespace normalization; no functional postprocessor change was pending. Full unedited build report is retained.

- Published [game-54000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-54000). Existing publisher verified all three remote asset digests/sizes and fetched latest signed catalog. The initial immediate draft-list lookup failed; the empty draft was verified and explicitly resumed, preserving prior releases.
- All 235 files match fresh output, signed ZIP, complete root Latest and managed 54000 (`runtime-identity.json`). Newly compiled gameplay assembly and scene/resource data are included. The unchanged Unity bootstrap executable hash is expected. The signed manifest and catalog preserve soundtrack ownership.

- Actual public download, pinned signature, installation and isolated muted startup passed, exit 0 (`hosted/result.json`). Production updater preserved music and reports no game/music updates pending.
- Unchanged **Play-Racer.cmd** launched responsive `Builds/Latest/versions/54000/Racer.exe`; full root Latest matches the public signed inventory. Original player settings restored byte-for-byte after temporary mute (`settings-preserved.json`).
- Cleanup: Builds **9,165,787,189 -> 7,286,940,014 bytes**, recovering **1,878,847,175 bytes** there; total disposable artifacts recovered **3,401,700,207 bytes** including the temporary public installation. Final C: free **326,113,779,712 bytes**. Current 54000 and previous 53000 retained, along with complete root Latest, music, signing identity/tools, launcher, metadata, source/evidence and saves. The updater had already retired 52000 before cleanup.
- STOP for Dan's handling/time-saving review. No further gameplay tuning or unrelated work. Subsequent commit contains delivery evidence and settings-preserving launcher verification only.
