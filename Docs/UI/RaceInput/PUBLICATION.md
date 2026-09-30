# In-race map/menu input repair — 0.53.0-review1

- Safety checkpoint: clean main `6a06d9ef9a31be08793ce1379f1ba1e71ac23042`.
- Completion source: `571976e32fc3362af503777e8d337ca828261960`, pushed and verified on origin/main before the fresh build. Only five runtime input/callback files plus release version change; world/geometry, driving controls/physics, AI, records, track selection and title behavior preserved.
- Pre-fix held-throttle failure reproduced. Final 34 unique targeted assertions pass, including repeated controller/keyboard menu/map use while driving controls remain held, countdown, held UI actions, disconnect recovery and effective trigger binding. Initial fixture failures and input-buffer/event-delivery diagnostics retained in [validation](VALIDATION.md). No physical-controller/Deck acceptance claimed. Feature testing stopped.
- Delivery results are recorded below. Existing signing identity, publisher, launcher architecture and soundtrack ownership are retained.

- Fresh Unity 6000.6.1f1 Windows build succeeded: **0 errors, 11 existing warnings, 4m16.915s**. Started 2026-09-30 21:59:59 UTC; source `571976e32fc3362af503777e8d337ca828261960`; GUID `9d07d9ab670f40558d657254497dbeff`. Free space before build: 327,538,237,440 bytes. Full warnings in `build-release.txt`.

- [Published game-53000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-53000). All three asset sizes/digests and fetched latest signed catalog verified by the existing publisher. Its immediate post-create draft-list lookup failed; verified the empty draft and resumed it without creating a duplicate or overwriting any prior release.
- All **230 signed runtime files** match fresh output, signed ZIP, complete root Latest and managed 53000 (`runtime-identity.json`). The gameplay assembly is newly built from the completion source; unchanged Unity bootstrap executable bytes are expected.
- Actual public download, pinned signature, installation and muted isolated startup passed, exit 0 (`hosted/result.json`). Published state and all three assets recorded in `remote-release.json`.
- Production updater preserved soundtrack state and reports no pending game/music updates. Unchanged **Play-Racer.cmd** launched responsive `Builds/Latest/versions/53000/Racer.exe`; complete root Latest matches. Startup was temporarily muted and original player settings restored byte-for-byte with hash verification.
- Cleanup: Builds **8,944,329,819 → 7,145,655,098 bytes**; **1,798,674,721 bytes** recovered there plus **1,452,235,007 bytes** from the temporary public installation, **3,250,909,728 bytes total**. Final C: free **327,476,436,992 bytes** (`cleanup.json`). Current 53000, previous 52000, complete root Latest and all protected source/music/signing/tools/launcher/metadata/saves retained.
- STOP for Dan's review. No further feature tests, SCS or unrelated changes.
