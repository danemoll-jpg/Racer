# Delivered — Records filters — 0.51.0-review1

- Safety checkpoint: clean main `966fa46016227fbedeaf0a9f85bb13d39a26b7de`.
- Completion source **`a9ed31f8d7060dc311cbf94ba0902c989fd4a769`** pushed and verified on origin/main before building. Initial approval review rejection of push was resolved by verifying the configured destination and citing Dan's explicit standing delivery authorization. Subsequent commit contains only delivery evidence/documentation.
- Fresh Unity 6000.6.1f1 Windows build: **0 errors, 11 warnings, 5m37.295s**; started 2026-09-30 17:40:00 UTC; GUID `85042f2ecf7a4090800528e875c88f8c`. Free space before build: 330,326,953,984 bytes. Full warnings preserved in `build-release.txt`.
- [Published game-51000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-51000). All three remote assets match their staged SHA-256 and size; latest signed catalog verified. Previous releases and existing signing identity retained.
- **230 signed files match** fresh output, ZIP, complete root Latest and managed 51000 (`runtime-identity.json`). Assembly/data were freshly built; unchanged Unity bootstrap executable bytes are expected.
- Public download, pinned signature, installation and muted isolated startup passed with exit 0. Latest matches every public manifest file (`hosted/result.json`).
- Existing production updater activated 51000, preserved soundtrack state, and reports no pending game/music updates. **Unchanged Play-Racer.cmd** launched responsive `Builds/Latest/versions/51000/Racer.exe`. Root `Builds/Latest/Racer.exe` contains the same complete new runtime.
- After production launcher verification: **219 entries before and after (161 lap / 58 race)**, with all **62 original record-file hashes identical**, including 61 legacy best files. No destructive migration, restoration, deletion or reset (`player-records-before.json`, `player-records-after.json`).
- Cleanup: Builds **8,944,219,169 → 7,145,547,349 bytes**; **1,798,671,820 bytes** recovered there. Including temporary public installation, **3,250,904,385 bytes** disposable artifacts removed. C: free **326,997,729,280 → 330,249,216,000 bytes** (`cleanup.json`). Current 51000, previous 50000, complete root Latest, music, tools, keys, launcher metadata, evidence and saves retained.

See [validation](VALIDATION.md) and [comparability audit](COMPARABILITY.md). Initial compound fixture failure is retained; independent state checks pass. Physical controller/Deck and subjective UI acceptance remain for Dan. General menu polish remains deferred. STOP for review.
