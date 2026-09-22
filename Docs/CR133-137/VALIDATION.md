# 0.24.0-review1 structural roads and earned recovery

Rollback checkpoint: `46e689e9119395ce2eb26f163b685279d1b7f90a`.

The three supplied screenshots were read from Downloads. Written requirements
were authoritative. Only MountainLoop, MountainLoopReverse and the Laurel
shortcut in StreetLoopReverse are authored by this correction.

## Root causes and changes

- Laurel's previous takeoff followed the curved shortcut centerline. Replace
  the approach with a tangent-continuous normal approach, a 72m straight run-up
  and a 28m straight quadratic ramp. Retain the launch/landing positions and
  original 3.8m added ramp rise. The full replacement uses shared visible and
  collision meshes; terrain is clipped beneath the new driving surface.
- Mountain branch clipping previously used a fixed seven-metre centerline
  distance, including elevation in that distance. It did not represent the
  actual wider road edges. Build a geometric union of actual road triangles,
  joining same-level intersections and preserving separate crossover levels.
  Grade the full junction cross-section, not only the branch centerline.
- Forward Summit Traverse's entrance was over 22m from the current main
  centerline, and its route crossed underneath the high landing road. Connect
  it to the supported regular approach and reshape the shortcut south of the
  flight corridor, retaining its original lower-route exit.
- The previous mountain subgrade operation only lowered terrain. Add visible,
  collidable earth banks that start at ordinary road edges and slope into
  locally sampled terrain. Trim terrain/road intersections and remove obsolete
  road colliders. Preserve the main flight gaps and reverse shortcut crossing;
  banks are excluded from deliberate flight corridors. No blanket terrain lift
  or invisible containment walls are used.
- Recovery's fixed 75m projection window could fail to follow a long flight.
  Expand it by observed travel while off the route. Safe anchors still require
  grounded, upright, low-angular-velocity support, a valid suspension footprint,
  one second of stability and two metres of forward progress. Completing that
  landing retires pre-jump fallback history. Free-roam recovery also uses
  established occupied samples rather than guessed backward offsets. Narrow
  exclusion volumes vertically so they do not reject unrelated road levels.

No speed, torque, gravity, suspension, handling or global physics settings are
changed. Highway 92 geometry and other scenes remain untouched. Clearance of
whole trees is limited to the rerouted shortcut corridors.

## Verification scope

Tools/Verify-StructuralRoads.cs checks actual route and lane support against
colliders, detects duplicate near-coplanar support, checks intentional gap
intervals and samples mesh triangle centroids. Tools/Check-EarnedRecovery.cs
uses deterministic grounded/airborne samples with real scene support queries;
it does not run a race or tune vehicle physics.

Per-scene checks, recovery results and build results are recorded beside this
file. Human gameplay acceptance remains open. Distribution uses the existing
Windows release scene list, portable packaging, signed game.zip,
game-manifest.json and update-catalog.json. Publication and launcher verification
are recorded after uploading the release.

## Final targeted results

- Forward Mountain: 4,845 route/lane samples and 4,641 mesh/collider samples;
  zero failures after the final fork grading.
- Reverse Mountain: 6,903 route/lane samples and 4,004 mesh/collider samples;
  zero failures.
- Laurel: 1,170 route/lane samples and 80 mesh/collider samples; zero failures.
  An additional 1,203 highest-surface raycasts across the straight runway found
  maximum profile error 0.34mm and maximum adjacent normal change 0.28 degrees.
- Recovery fixtures passed Eastbound Gully Flight and Homeward Summit Flight:
  no airborne/ramp anchor, earned post-landing progress, and pre-jump fallback
  history retired. No full race playthroughs were performed.
- Static driver and junction images were inspected. The final forward fork
  grades the entire physical intersection onto a common plane with a gradual
  blend to the existing approaches; its navigation heights follow that surface.

Intermediate mesh assets are retained. Only scene-referenced meshes are part
of the built tracks; no broad deletion or unrelated asset cleanup was done.

## Published release and launcher

- Windows build succeeded with zero errors and five warnings (mesh pre-baking
  guidance, player Pipeline configuration and existing deprecated API calls).
- Implementation commit `32f9220b06372496d6830967d2bd51946a72f616`,
  "Fix continuous mountain roads, straight Laurel runway, and earned post-jump
  recovery", pushed to `danemoll-jpg/Racer` main.
- Published `game-24000` / `0.24.0-review1` as the latest non-draft release:
  https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-24000
- `game.zip`, `game-manifest.json` and `update-catalog.json` uploaded and their
  GitHub SHA-256 digests matched the prepared inventory. Existing soundtrack
  pointer preserved. Public latest catalog fetched and signature verified.
- The native updater downloaded and activated the public game package in an
  isolated installation. One muted startup handshake passed with exit code 0.
  All 255 files in the public game manifest match `Builds/Latest` exactly.
- The unchanged `C:\Users\danmo\Racer\Play-Racer.cmd` was actually invoked;
  it started `C:\Users\danmo\Racer\Builds\Latest\Racer.exe`, version
  `0.24.0-review1`, with a responsive Racer window. Only that verification
  process was closed afterward. No temporary development path was substituted.
- Publisher signing required the user to run the existing preparation script
  outside Codex because Windows denied key access. Work resumed from those
  prepared assets; no rebuild was performed after signing.

See `public-release.json`, `play-racer-launch.json` and `hosted/result.json`
for publication and startup evidence. Detailed driving/jump acceptance remains
the user's gameplay test; this pass did not run full races.

