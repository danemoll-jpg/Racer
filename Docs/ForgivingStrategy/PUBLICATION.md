# 0.32.0-review1 / game-32000 delivery

Completion source: `019219f64e27d99a41bfec817d50ebbe24394c53`, pushed and remotely verified on origin/main before the fresh Unity Windows build. Gameplay safety checkpoint: `18589a4133ec5fb907b64625541fbe27236058eb`; original clean checkpoint: `ca84ef9e62d21b4031e0d18e4f2f08241aca7933`.

Published latest: [game-32000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-32000). Implementation, targeted verification, source push, fresh build, publication, normal launcher delivery and cleanup are complete.

Targeted implementation evidence: [VALIDATION.md](VALIDATION.md). No physical track geometry changed. Granite remains Forest main; House 3 Detour remains optional; another Forest shortcut is canceled, not deferred. Standing rules, project-status pointer and automatic AGENTS.md discovery are already committed/pushed.

## Fresh runtime

- Unity 6000.6.1f1 clean-cache Windows x64 build, six existing scenes, new output directory. Started 2026-09-28 09:49:19 UTC; succeeded in 4m15s with zero errors and five existing warnings (mesh collision pre-bake, absent optional Pipeline runtime configuration, obsolete APIs).
- Build GUID: `6391ebac8c4e481f869b4fd99d6216ba`.
- Gameplay DLL SHA256: `9b1ee4e31e7fa5641aa935aa96a587cf4ef17df3e88beefe944bde3fa5d059b0`; prior 31000: `ae2b83cd24dbe1df99cd7161d36f4a29f9fa27f8214df80d878ec9d8aeac7cd6`.
- Fresh output Racer.exe written 09:52:45 UTC. Unity's bootstrap EXE hash remains `07534cc5839d2c89c4fe45ec272c91b046ff3d64f14ab3f0a32221121e0ffff2`; it was emitted by the new build. The gameplay assembly and runtime data changed. No old runtime was repackaged with new metadata.
- All 220 signed runtime files compared between fresh output, ZIP, complete `Builds/Latest` and `Builds/Latest/versions/32000`. Named launcher preserved. VERSION.txt identifies source, version and GUID.

## Publication and launcher

All three remote assets match signed publisher inventory by size and SHA256. ZIP size: 240,055,581 bytes. Signed catalog fetched from public latest; existing key identity and soundtrack pointer preserved. GitHub briefly omitted the created draft from its list; the confirmed empty draft was resumed without duplication or overwriting a release.

A genuine public download used the production updater to verify pinned signatures, download, extract and activate the package in an isolated test installation. Startup succeeded with no NullReferenceException or MissingReferenceException; public manifest matched all installed Latest files. This disposable installation was subsequently removed.

Production updater activated 32000 with previous 31000 retained; catalog check reports no game/music updates pending. At 05:58 EDT, unmodified `Play-Racer.cmd` launched responsive `C:\Users\danmo\Racer\Builds\Latest\versions\32000\Racer.exe`. The root complete `Builds\Latest\Racer.exe` runtime also matches. Verification closed only its own launched game/launcher. Saves and soundtrack state preserved.

Evidence: build-release.txt, build-done.txt, fresh-output.json, runtime-identity.json, remote-release.json, hosted/result.json, hosted/startup.json, hosted/game.log, launcher-catalog-check.json and play-racer-launch.json.

## Cleanup

Builds: 6,268,971,529 → 5,220,883,238 bytes (**5.838 → 4.862 GiB**), recovering 1,048,088,291 bytes (**0.976 GiB**). C: free: 327,953,772,544 → 329,813,315,584 bytes (**307.163 GiB final**), approximately **1.732 GiB total recovered** including the temporary hosted install.

Deleted only verified disposable fresh build output, release upload ZIP and public test installation after successful normal launcher delivery. Updater already removed obsolete managed 30000. Retained complete current root runtime, managed current 32000 and previous 31000, music, signing identity, publisher tools, launcher/SDK, saves, source/Git and release evidence. Other old release directories contain only small metadata/evidence; Preserved is empty. Exact paths/measurements: cleanup.json.

No additional gameplay testing or tuning after targeted acceptance. Dan evaluates gameplay feel.
