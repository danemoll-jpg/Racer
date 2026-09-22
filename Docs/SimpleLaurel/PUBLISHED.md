# Publication — 0.27.0-review1

- Implementation commit: aba9e15bda72cb4cf8e99bcb3d7a21093de1b137 — Replace Laurel shortcut overengineering with one straight ground launch ramp.
- Source pushed to origin/main before publication.
- Existing Windows release build succeeded with zero errors and two warnings (recorded in build-release.txt).
- Packaging resumed from that completed build after the user cleared disk space; no rebuild or further geometry changes were made. All 451 packaged files passed SHA-256 verification. Previous Latest was preserved by the existing packager.
- Published latest release: https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-27000
- All required assets uploaded and remote hashes verified: game.zip (239,153,149 bytes), game-manifest.json (53,849 bytes), update-catalog.json (993 bytes).
- Existing publisher identity/key and project-local GitHub CLI used. Soundtrack catalog pointer preserved.
- Publisher's immediate draft-list lookup missed the newly created empty draft; inspection confirmed it, then the existing resume-draft option completed publication successfully.
- Public signed catalog and manifest verified against the pinned publisher identity. All 261 installed Latest game files match the public manifest.
- Existing updater downloaded and installed the public release successfully. Native launcher startup check passed with isolated test saves; see hosted/result.json.
- Unchanged Play-Racer.cmd launched Builds/Latest/Racer.exe at version 0.27.0-review1 with a responsive game window; see play-racer-launch.json. Only the verification-launched game process was closed.
- Detailed gameplay testing remains with the user. No extra improvements were made.
