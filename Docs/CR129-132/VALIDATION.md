# 0.23.0-review1 road follow-up

Rollback checkpoint: 40f5700f (clean repository checkpoint before changes).
Screenshots read from Downloads by the four supplied filenames.

- Issues 1/2: the CR113 Highway 92 ribbon raised the junction above the
  original race line; its terrain adjustment extended into the player grid.
  Correct the local highway profile and blend the street approach into it.
  Clip the underlying terrain at the pavement boundary, preserving terrain
  vertex colors. Re-seat navigation heights on the actual colliders, preserving
  route x/z and the intended grid. Apply this shared local junction correction
  to the six scene copies so direction/free-roam changes remain consistent.
- Issue 3: blend the Forward Mountain rejoin over 50m, including horizontal
  tangents and elevation; smooth the folded summit junction locally.
- Issue 4: rebuild both Forward main jump final approaches as straight run-ups
  followed by tangent-continuous 60m ramps. Keep lip positions, elevations,
  flight gaps and destinations. Remove the surviving duplicate summit launch
  mesh/collider. Re-seat forward branch mouths and guidance to the repaired
  main surface and lower only intruding local subgrade.

No vehicle physics or recovery code changed. Reverse shortcut geometry and
unrelated route layouts remain unchanged. The shared Highway 92 junction is
the only geometry correction applied to other scenes.

Verification is limited to compilation, a non-development Windows build and
static mesh/collider/grid checks. No gameplay or automated tuning passes.
Detailed gameplay acceptance remains with Dan.

Distribution uses the existing portable Windows package and signed game.zip,
game-manifest.json and update-catalog.json component update. Soundtrack and
prior releases are retained. Play-Racer.cmd retains Builds/Latest/Racer.exe.

Static checks passed: four of four player suspension probes supported; 161
junction samples supported; 336 runway samples each hit exactly one collider,
with maximum analytic-profile error 0.0003m. No race/jump playthroughs run.
