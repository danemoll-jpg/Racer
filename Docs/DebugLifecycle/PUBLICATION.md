# 0.65.0-review1 / game-65000 delivery

Safety checkpoint: clean main `23e383f95dc7ec1cf4a0570699fbade6531a37ed`. Completion source `a888e6a5b9e5fbd4377166e7a943de87cd6ed34d` was pushed and verified on origin/main before building. Subsequent delivery documentation does not change playable source.

Fresh Windows build succeeded in 4m52s with zero errors and 20 warnings; GUID `afe990d488284fef8a8c2e2ca319abc9`. See build-done.txt and build-release.txt. Complete runtime staged at Builds/Latest and managed versions/65000. runtime-identity.json verifies all 232 signed files against the fresh build and package, including the changed Assembly-CSharp and runtime data. Unity's platform bootstrap executable hash is unchanged; the complete newly built data/runtime is delivered.

Published [game-65000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-65000) using the existing signing identity and publisher. Assets: game.zip (377,191,909 bytes), game-manifest.json (46,693 bytes), update-catalog.json (993 bytes). The existing publisher's release-list lookup failed after draft creation; the inspected empty unpublished draft was resumed. Uploaded sizes/digests were verified before publication; the public latest catalog was fetched afterward. Previous releases remain intact.

github-release.json confirms published assets. hosted/result.json records actual public download, pinned signature validation, installation, all 232 Latest file matches, startup readiness and exit 0. Production updater activation preserved soundtrack state; launcher-catalog-check.json reports no pending game/music update.

Unchanged Play-Racer.cmd launched the existing signed launcher and responsive `C:\Users\danmo\Racer\Builds\Latest\versions\65000\Racer.exe`. See play-racer-launch.json. Original player settings were restored byte-for-byte after temporary mute; settings-preserved.json records the hash.

Cleanup followed all delivery gates and checked absolute target boundaries. Builds: **9,683,288,517 -> 7,678,725,701 bytes**. Total disposable output removed: **3,634,290,858 bytes**, including the duplicate build/package and isolated public install/save. C: free: **304,751,865,856 -> 308,386,238,464 bytes**. See cleanup.json.

Retained complete Latest root runtime, current managed 65000, previous 64000, source, evidence, music, saves/settings, signing keys, publisher tools, launcher and active signed metadata. All Debug session folders and ZIP history are preserved, including the three lifecycle test sessions, their export, the previous verified export and Dan's original 25-report ZIP. No DebugReports cleanup was performed.

All 28 targeted lifecycle/input checks passed; [VALIDATION.md](VALIDATION.md) contains the acceptance evidence. Delivery complete. Stop for Dan's next debug run.
