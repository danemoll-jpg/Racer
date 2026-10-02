# 0.64.0-review1 / game-64000 delivery

Safety checkpoint: clean main `f034febc94ba52f61b3bba2acce0a620a0e2b993`. Completion source `6af2c67dcfa4aaf14a167604bf535d6514be857e` was pushed and verified on origin/main before building. Subsequent delivery documentation does not change playable source.

Fresh Windows build succeeded in 4m11s with zero errors and 20 warnings; GUID `d7e138747522466dad964bbae2b9643c`. See build-done.txt and build-release.txt. Complete runtime staged at Builds/Latest and managed versions/64000. runtime-identity.json verifies all 232 signed runtime files against the fresh build and package; gameplay data and Assembly-CSharp changed. Unity's platform bootstrap executable hash is unchanged, as expected; the complete newly built data/runtime is delivered.

Published [game-64000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-64000) using the existing signing identity and publisher. Assets: game.zip (377,196,217 bytes), game-manifest.json (46,677 bytes), update-catalog.json (993 bytes). A transient release-list lookup failed after draft creation; the inspected empty unpublished draft was resumed safely. All uploaded sizes/digests were verified before publication, and the public latest catalog was fetched afterward. No previous release overwritten.

github-release.json confirms published assets. hosted/result.json records actual public download, pinned signature validation, installation, all 232 Latest file matches, startup readiness and exit 0. Production updater activation preserved soundtrack state; launcher-catalog-check.json reports no pending game or music update.

Unchanged Play-Racer.cmd launched the existing signed launcher and responsive `C:\Users\danmo\Racer\Builds\Latest\versions\64000\Racer.exe`. See play-racer-launch.json. Original player settings restored byte-for-byte after temporary mute; settings-preserved.json records the matching hash.

Cleanup followed all delivery gates with absolute target-boundary checks. Builds: **9,845,981,594 -> 7,683,982,559 bytes**. Total disposable output removed: **3,791,740,033 bytes**, including the duplicate build/package, copied private report workspace and isolated public install. C: free: **305,078,202,368 -> 308,871,303,168 bytes**. See cleanup.json. Earlier authoring cleanup also removed 1,824,052,411 bytes of this task's unreferenced mesh intermediates while preserving all production scene dependencies.

Retained complete Latest root runtime, current managed 64000, previous 63000, source, evidence/verified-menu-export.zip, original user report outside the project, music, saves/settings, signing keys, publisher tools, launcher and active signed metadata. The original generated test-export.txt path points to the now-disposable test workspace; the identical verified ZIP remains at `C:\Users\danmo\Racer\Docs\ReportCleanup\verified-menu-export.zip`.

All 25 report dispositions and targeted check limitations are in [VALIDATION.md](VALIDATION.md). Delivery complete. Stop for Dan's next debug run; no further gameplay tuning.
