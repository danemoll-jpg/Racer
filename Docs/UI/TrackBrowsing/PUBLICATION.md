# Delivered — track browsing / Race Setup preview — 0.50.0-review1

- Safety checkpoint: clean main `43c8ef0d9a6e0ad85da1929304fdc66474ee1e04`.
- Completion source **`d5fe1e3d49fb8a620984c4453f13b3427db0c608`** pushed and verified on origin/main before building. Subsequent changes contain delivery documentation/evidence only.
- Fresh Unity 6000.6.1f1 Windows build: **0 errors, 11 existing warnings, 3m23.192s**, started 2026-09-30 15:34:59 UTC; GUID `5f3b42596899448fa8e82f20cb3c5d15`. C: free before build 330,399,698,944 bytes. Warnings retained in `build-release.txt`.
- [Published game-50000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-50000). All three remote asset sizes/SHA-256 digests match the approved inventory. Latest signed catalog verified. Initial draft listing missed the new draft; inspecting the empty unpublished draft and using supported `--resume-draft` completed publication without overwriting assets. Previous releases and signing identity retained.
- **All 230 signed files match** the fresh output, ZIP, complete root Latest and managed 50000. Game assembly and data were freshly built. Unity bootstrap executable bytes match the previous version; this is a complete new runtime (`runtime-identity.json`).
- Public download, pinned signature validation, extraction and muted isolated startup passed, exit 0. All Latest files match the public manifest (`hosted/result.json`).
- Existing production updater activated 50000 and preserved music state. No game/music update pending (`launcher-catalog-check.json`).
- **Unchanged Play-Racer.cmd** launched responsive `Builds/Latest/versions/50000/Racer.exe` at 11:45 EDT. Root `Builds/Latest/Racer.exe` represents the same release. Verification-created processes were closed (`play-racer-launch.json`).
- Player record archive SHA-256 **remained identical before and after delivery**. All 219 existing board entries remain present; no record deletion/reset/restoration was performed (`player-records-readonly.json`, `player-records-after.json`).
- Cleanup: Builds **8,939,180,233 → 7,140,513,029 bytes**, recovering **1,798,667,204 bytes** there. Including the temporary public install, **3,250,896,098 bytes** disposable artifacts removed. C: free **327,123,177,472 → 330,375,139,328 bytes** (`cleanup.json`).
- Retained current **50000**, previous **49000**, complete root Latest, source/evidence, music, publisher keys/tools, launcher metadata and user saves. Updater retired managed 48000 before manual cleanup.

[Validation](VALIDATION.md): 20/20 targeted checks passed, screenshots inspected. Physical-controller/Deck and subjective acceptance remain for Dan. No broad gameplay matrix or further tuning.
