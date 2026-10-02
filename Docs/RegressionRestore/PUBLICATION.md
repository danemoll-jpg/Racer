# 0.66.0-review1 / game-66000 delivery

Safety checkpoint: clean main `f588a2d4`. Completion source `eec4e91131e3b3853981a6146f37ee9e0d91da84` was pushed and verified on origin/main before building. Subsequent delivery documentation does not change playable source.

The main project was held open by an idle batch-mode Unity editor left over from an earlier session. This session was not permitted to stop it, so the fresh Windows build ran in an isolated git worktree checked out cleanly at `eec4e911`. That worktree had a copied Library and a clean script recompile. The build script lived only in its Editor folder (editor-only; not part of the player). The build succeeded in 5m04s with zero errors and 11 warnings; GUID `34ba59b9b198412590505861745fcd79`. See build-done.txt and build-release.txt. The worktree was removed afterwards.

The complete runtime is staged at Builds/Latest and managed versions/66000. runtime-identity.json verifies all 232 signed files against the fresh build and package (47 changed runtime files, including Mountain Reverse data). The Unity bootstrap exe hash is unchanged, as in prior releases.

Published [game-66000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-66000) using the existing signing identity and publisher with the bundled gh. Assets: game.zip (377,199,875 bytes), game-manifest.json (46,685), update-catalog.json (993). As in 0.65, the publisher's release-list lookup missed the newly created draft. The single empty unpublished draft was inspected and resumed with `--resume-draft`. Uploaded sizes/digests were verified before publication, and the public latest catalog fetch matched. Previous releases remain intact; latest = game-66000. See github-release.json.

hosted/result.json records the actual public download, pinned signature validation, installation, all 232 Latest file matches, startup readiness and exit 0. Production updater activation preserved soundtrack state. launcher-catalog-check.json reports no pending game/music update.

Unchanged Play-Racer.cmd launched the existing signed launcher and a responsive `C:\Users\danmo\Racer\Builds\Latest\versions\66000\Racer.exe` (play-racer-launch.json). The first scripted attempt blocked because the launcher inherited this tool host's console pipe. The launch was observed directly and the game closed, which made that attempt's final assertion fail. The verifier was then changed to start Play-Racer.cmd detached; the script itself is unchanged. It passed cleanly. Original player settings were restored byte-for-byte after the temporary mute (settings-preserved.json, SHA-256 9DC0A467…050D).

Cleanup ran after all delivery gates. Builds: **9,683,739,471 -> 7,679,034,176 bytes** (2,004,705,295 recovered by the cleanup script: duplicate build output, package game.zip, isolated public install). The temporary probe worktree (copied Library plus checkout) and temp signature folder were also removed. Final C: free **305,971,154,944 bytes**. Retained: complete Latest root runtime, managed 66000 and previous 65000, source, evidence, music, saves/settings, signing keys, publisher tools, launcher and active signed metadata. All DebugReports folders/ZIPs, including Dan's report for this task, are preserved.

Delivery complete. STOP for Dan's verification of BUG-001 and BUG-002.
