# 0.44.0-review1 / game-44000 — delivered

- Safety checkpoint: clean main `73b0399b260cbf1d400f55d73bbd2d9f2d605828`.
- Completion source: `6f6cd9c0e3543f3ed3e7494475712ddd1ad0f023`, pushed and verified on origin/main before building. Subsequent delivery commit contains documentation/evidence only.
- Fresh Unity 6000.6.1f1 Windows build: succeeded, zero errors, 12 warnings, 9m09.439s. GUID `d0da7840c7124a82b2111526bbd3c528`; details in `build-done.txt` and `build-release.txt`. No unrelated warning cleanup.
- Final standalone targeted fixture: 26/26 pass, exit 0; six geometry assertions pass. Actual menu, overlay, waiting button and results captures inspected. Testing stopped; limitations in [VALIDATION.md](VALIDATION.md).
- All 232 signed runtime files agree across the fresh build, package, complete root `Builds/Latest` and managed 44000. Runtime data and Assembly-CSharp changed; Unity's bootstrap executable hash itself is unchanged, as expected. See `runtime-identity.json`.
- [Published release game-44000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-44000): three required assets uploaded and remote digests/sizes verified. Published 2026-09-29T22:57:33Z. Initial draft lookup lag was resolved by resuming the same empty draft; no replacement release or authentication intervention.
- Public latest catalog and manifest verified with the existing pinned signing identity. Actual public download installed and reached startup successfully, exit 0 (`hosted/result.json`). No pending game/music updates after production activation (`launcher-catalog-check.json`); soundtrack state preserved.
- Unchanged `Play-Racer.cmd` launched responsive `Builds/Latest/versions/44000/Racer.exe` at 2026-09-29T19:02:59-04:00. Only verification-owned processes closed. Root Latest contains the complete matching runtime, and updater architecture remains intact (`play-racer-launch.json`).

## Cleanup

Removed the temporary fresh-build directory, duplicate upload ZIP and temporary public-download installation after all delivery gates passed. Updater retained current 44000 and previous 43000 only. Source, maps, evidence, music, saves, keys, publisher tools, launcher and signed metadata preserved.

| Measurement | Before | After |
|---|---:|---:|
| Builds bytes | 7,846,892,729 | 6,234,365,055 |
| Builds GiB | 7.308 | 5.806 |
| C: free bytes | 326,779,060,224 | 329,668,124,672 |
| C: free GiB | 304.337 | 307.027 |

Reclaimed 1,612,527,674 bytes inside Builds plus 1,277,651,117 bytes from the temporary public install. Measured total free-space gain: 2.691 GiB. Exact paths and measurements: `cleanup.json`.

No Backyard Reverse or broader menu cleanup. Racing minimap unchanged. Work stopped for Dan's review.
