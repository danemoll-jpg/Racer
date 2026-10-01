# 0.63.0-review1 / game-63000 delivery

Safety checkpoint: clean main `b9041b01a5598a3fa1ce05a44f7de9a7ab59237f`. Completion source `2b584b4598240c3109f93cb1c142a3a658b93947` was pushed and verified on origin/main before building. Subsequent delivery documentation does not change playable source.

Fresh Windows build succeeded in 3m57s, zero errors and 11 warnings, GUID `20b9efb0b70e467cb533c69d2954e13a`. See `build-done.txt` and `build-release.txt`. Complete runtime staged in Builds/Latest and managed versions/63000; `runtime-identity.json` records identity.

Published [game-63000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-63000) with the existing signing identity and publisher: game.zip (372,900,479 bytes), game-manifest.json (46,921 bytes), update-catalog.json (993 bytes). The initial empty draft encountered a transient listing failure; the inspected empty draft was safely resumed, all three assets uploaded, and the release published. No previous release overwritten.

`github-release.json` confirms published assets. `hosted/result.json` records actual public download, pinned signature validation, installation, all 233 Latest file matches, startup readiness and exit 0. Latest catalog identifies build 63000. Production updater activation preserved soundtrack state; `launcher-catalog-check.json` reports no pending game or music update.

Unchanged Play-Racer.cmd launched the existing signed launcher and responsive `C:\Users\danmo\Racer\Builds\Latest\versions\63000\Racer.exe`. See `play-racer-launch.json`. Original player settings were restored byte-for-byte after temporary verification mute (`settings-preserved.json`).

Cleanup followed all delivery gates and verified absolute target boundaries. Builds shrank from 9,688,401,947 to 7,682,834,457 bytes. Total disposable output removed: 3,647,352,311 bytes, including redundant build/ZIP, isolated public install and temporary test reports. C: free changed from 306,212,466,688 to 309,860,352,000 bytes. See `cleanup.json`.

Retained complete Latest root runtime, current managed 63000, immediately previous 62000, source, evidence/sample reports, music, saves/settings, signing keys, publisher tools, launcher and active signed metadata. Stop for Dan's clean-baseline debug run; local technical checks do not claim human gameplay acceptance.
