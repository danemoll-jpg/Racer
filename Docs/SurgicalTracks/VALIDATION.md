# Surgical track correction — 0.26.0-review1

Rollback checkpoint: `8dea14cb2979c2d1d51df5cdf78daf408c688590`.

## Laurel Pass / Street Loop

Git history identifies the CR122/CR133 shortcut replacements and the later
`398f1ace` / `84220a5b` local replacement as the regression chain. The latter
cut terrain beneath a large shortcut embankment and replaced the landing-road
footprint with an apron. `573c0432` further widened that landing shoulder.
The embankment occupied the normal road, with several metres of displacement
and a missing support sample around reverse station 3883.

The seven affected terrain references were restored from `489b220b`, before
those shortcut repairs. Those baseline assets are unchanged in Git. The wide
replacement road, landing apron and embankment were removed. The main road
keeps its original centerline, elevation and width. Only the separate shortcut
footprint is cut from surrounding terrain; it does not replace the main road.

The inherited approach was up to 38m above the valley. Its replacement follows
the restored terrain, with a local transition into a straight 10m run-up and
25m launch ramp. The ramp is 5m wide, rises 0.7m and has a 0.03 terminal grade.
It targets the existing main road, without a catch road or landing platform.

Street Forward's scene and referenced geometry were unchanged by the recent
Laurel commits. It was inspected rather than unnecessarily rebuilt. Both
directions now pass 6,805 support samples over the local normal route, including
lanes 4.4m either side of center: zero missing samples, maximum centerline height
difference about 6.5cm (existing road/driveway surface detail).

The shortcut passes 4,944 cross-lane samples with no stacked surface colliders;
maximum route/surface difference is 1.95cm. The ramp entry and straight segment
have a maximum adjacent normal change of 1.234 degrees. Unboosted geometric
trajectories at 18, 24, 32, 38, 49, 52, 56 and 61m/s contact the restored main
road. These are structural/ballistic checks, not vehicle gameplay guarantees.

## Mountain Reverse early arrows

There is a marker at the grid (station 0), then early arrows near stations 70
and 140. All three have zero child colliders. The first ahead-of-grid arrow's
existing correction remains unchanged, with maximum adjacent face change of
1.686 degrees in the inspected region.

At the second ahead-of-grid arrow (station 140), the actual road collision mesh
contained alternating face slopes: adjacent changes near 19 degrees under the
marker. A continuous local cubic height field plus finer collision triangles
removes that faceting. The core is stations 118–160, feathering to unchanged
surfaces at stations 100 and 180; no route or flight metadata is altered.
The core now has maximum adjacent normal change 0.636 degrees and no gaps.
The full feathered region has maximum adjacent change 3.722 degrees. Visible
and collision surfaces share the same mesh; the visual arrow is re-seated.

This identifies and removes a physical discontinuity; the rider-reported event
has not been reproduced in a full race. Gameplay testing is left to the user.

## Preservation and release

`VehicleRespawn`, `JumpRecoveryExclusion`, `ArcadeVehicle`, and `VehicleProfile`
are unchanged. Every serialized recovery-exclusion component in both edited
scenes matches the checkpoint. Race AI and other track scenes are unchanged.
See `preservation.txt`, `verification.txt`, and the captured local views.

The existing Windows build, package verifier, signing key, component publisher,
public updater check and Play-Racer startup check are used for this release.
Publication results will be recorded after successful upload and verification.
