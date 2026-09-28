# Delivery — 0.33.0-review1 / game-33000

Delivered: https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-33000

Safety checkpoint: clean main `bb4f0c301bd9bf1a73999797c858e894acfddc35`. Completion gameplay source: `7194b82b317f1f9a783ccc17d37984ba124f0efd`, pushed and verified on origin/main before building. Final documentation/evidence commit follows delivery; it changes no gameplay.

Fresh Unity 6000.6.1f1 Windows build: succeeded, zero errors/five warnings, 4m19s, GUID `15c6d99d52954a25a162e0a3daeb7ede`. Free disk before build: 329345970176 bytes. The new gameplay assembly SHA256 is `46f21c5104e852cc23dc43bace7f36223b05731ecbec228ed2bd99cc23e8708e`, different from 32000. Unity emits the same bootstrap EXE bytes; the complete freshly generated runtime contains the changed assembly, scenes, audio and data. This is not metadata wrapped around an old game.

All 220 files match fresh output, signed ZIP, complete root `Builds/Latest` runtime and managed `Builds/Latest/versions/33000`. Existing named launcher retained. Published three immutable assets; remote size/SHA256 verified, signed latest catalog fetched, existing signing identity and soundtrack pointer preserved. GitHub's initial listing omitted its just-created empty draft; the confirmed draft was resumed without duplication or overwrite.

One genuine public updater download passed pinned signatures, all-file inventory and startup. Production updater activated 33000 and retained previous 32000. Catalog reports no game/music update. Unmodified `Play-Racer.cmd` launched responsive `C:/Users/danmo/Racer/Builds/Latest/versions/33000/Racer.exe` at 12:00 EDT; only verification-owned processes were closed. Root Latest is the matching complete new runtime. Existing player saves/settings/music preserved.

Targeted results: `exit-driving-final-scenes.txt` passes Forward exit/immediate turn and blocking entry in the actual Street Reverse scene. Forest route/gate invariants pass. Fresh compiled player `runtime-visible/checks.txt` passes all 25 map/sign/audio assertions on one Forward and one Reverse course, three headings each. Final 1280x720 HUD captures in runtime-visible were inspected and are readable; main/optional and racer markers are distinct. Hidden non-batch startup did not advance; batch assertions passed but hidden screenshots were black; one visible capture resolved presentation verification using the same binary. These fixture limitations are not gameplay defects. No broad regression or subjective human-listening claim. Dan judges coyote sound and driving feel. Implementation/source checks: VALIDATION.md.

Mandatory cleanup after publication/public verification/launcher success:
- Builds: 6,284,513,352 → 5,229,738,140 bytes (5.853 → 4.871 GiB).
- Removed from Builds: 1,054,775,212 bytes (0.982 GiB).
- C: free: 326,830,411,776 → 328,832,700,416 bytes; final 306.249 GiB.
- Approximate total disk recovered: 1.865 GiB, including the public verification install and temporary plotting dependency.
- Retained current complete root runtime, managed 33000 and previous 32000, music, source/Git, atlas/docs/evidence, publisher tools/key, launcher/SDK and signed metadata. Exact checked deletion targets: cleanup.json.

All five requested changes are delivered. AI probabilities/recovery, Laurel, Roger's property, McFadden geometry and future trail construction remain untouched. No further tuning or testing is planned in this pass.
