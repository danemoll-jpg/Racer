# 0.22.0-review1: screenshot repairs

Rollback checkpoint: `38cec110bb934925e252dbd18afb6c06445be3d9`.

The nine user screenshots were compared with the authored geometry and collider
inventory. Written requirements take precedence over image names/labels.

- Issues 1/3/4/7: replace intersecting legacy mountain driving ribbons with
  continuous support following the retained route profile. Only abrupt slope
  joins receive local eight-metre tangent blends. Branch mouths are clipped to
  the main surface; terrain intruding into the driving ribbon is lowered locally.
- Issue 2: place both forward big-flight warnings on the approach shoulder using
  actual roadside ground height, with posts rooted at that height.
- Issue 5: visible driving mesh and MeshCollider share one asset; obsolete landing
  and return ribbons are retired, independently of recovery changes.
- Issue 6: Reverse Ridge Cut has a curved 45m run-up, 6.5m rise and 34m airborne
  crossing. The far-side landing is separated from the usable main road by timber.
- Issue 8: require one second of grounded, upright, low-angular-velocity support;
  reject jump approaches/ramps; use established occupied samples instead of
  arbitrary backwards candidate offsets. Normal intentional airtime is not a
  new reset trigger.
- Issue 9: local Laurel takeoff and catch restore support above the valley
  driveway. The existing house, driveway and valley layout remain.

No vehicle speed, acceleration, gravity, suspension or jump boost changes.

Validation is intentionally limited to Unity compilation, a non-development
Windows build, and structural mesh/collider checks. No lap runs or repeated
driving/tuning passes. Gameplay difficulty, landing feel and recovery behavior
remain for Dan's hands-on acceptance. See the generated build and sanity files.

Packaging uses the existing portable Windows format and preserves previous
Latest/runtime packages. Publishing uses the existing signed game-component
update and preserves the soundtrack URL. Play-Racer.cmd retains its normal
Builds/Latest/Racer.exe entry point; packaging installs the same new runtime.

## Completed release

- Implementation commit: `b97f83b55f4bf03cc5b605e2e1f4533ca9b2f693`.
- Final build source, including corrected crossing markers:
  `8857f1bfe40c5585b0f33e932c9cd95095fe41fe`.
- Final Windows build succeeded: **0 errors, 2 warnings**.
- Static support: Forward 208 samples, Reverse 230, Laurel 11; zero misses.
  These are bounded mesh support checks, not driving acceptance tests.
- Portable packaging verified all 451 files in the runtime, Latest and extracted
  ZIP; 187 playable songs and two staged originals retained.
- Published latest release: [game-22000 / 0.22.0-review1](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-22000).
  Three signed-update assets uploaded and remotely verified; previous release
  and soundtrack pointer retained. The publisher fetched the new latest catalog.
- Play-Racer.cmd resolves to Builds/Latest/Racer.exe. All **261 game files** there
  match the published signed game manifest by size and SHA256. No game launch
  or additional playtesting was required for this launcher-resolution check.

The PNGs and route traces in this folder are pre-repair location references.
Detailed gameplay acceptance remains with Dan; no requested issue was omitted.
