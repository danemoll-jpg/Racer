# Dense garbage and forgiving traversal — 0.40.0-review1

Dan accepted the bowl/jump direction and rejected the 60-piece treatment as far too sparse. This changes trash density and failed-jump resistance only. Safety checkpoint: clean main `6396cb50de28a157ee1a3b3b34f79d6d7d6873a9`.

## Implementation and preservation

The same bowl now contains **2,100 overlapping pieces**: drums, boards, discarded containers, sheets, rubber wheels, dirty paper/plastic, cardboard and irregular refuse shapes. A broad irregular bed plus 19 concentrations covers the floor and lower slopes, including the former clean central lane. Each piece is seated against the actual existing terrain, with 3.5cm contact embed; maximum grounding calculation error is 0.000004m. Geometry is combined into **8 renderers / 63,740 triangles**, using existing materials and three inexpensive color variations. No per-piece rigidbodies or colliders.

`DumpRefuse` exists only in this scene. It adds smooth velocity-proportional horizontal resistance at the vehicle centre of mass while at least two wheels and a terrain probe establish support within the trash footprint. Resistance fades from the floor to zero at normalized bowl radius 0.86. It is zero at rest, allowing forward/reverse pull-away, and applies no upward force, torque, artificial timer penalty or reset. Overflying vehicles receive no resistance. Dense visual debris is non-blocking; it does not physically scatter. This deliberately prioritizes forgiving traversal and low cost over individual garbage-body simulation.

The [preservation check](preservation.json) confirms **49,341 existing scene blocks unchanged**. Only the old debris hierarchy is replaced and its parent child-list changes. Terrain geometry/colors/colliders, trees, launch, landing, route, checkpoints, AI, vehicle motor, recovery, and other courses remain unchanged. Atlas footprint is unchanged.

Inspected views: [launch](approach.png), [overhead](bowl.png), [floor](floor.png). The launch/floor views show overlapping trash dominating the interior, with exposed dirt toward the rim. Dan judges visual/gameplay acceptance.

## Targeted testing

The opt-in fixture uses ordinary motor simulation, 0.02s physics steps, bounded placement/initial speed, muted audio and isolated saves. No teleports or resets during each traversal. Only one normal motorcycle jump, short-jump empty-versus-trash comparisons and a floor start/steer/escape for motorcycle and ATV. No full races, AI checks or unrelated courses.

First run retained in `editor-checks`: the initial coefficient 1.65 failed the slowdown requirement (6.46s through trash versus 6.72s empty). It reduced bouncing enough to escape faster. Its ATV-labelled segment also retained motorcycle configuration because the fixture omitted Pause before QuitRace; those labels are not ATV evidence. Corrected the fixture to use the menu sequence and assert actual profile identity, and increased only the dump resistance coefficient to 5. The final bounded run is recorded separately in `final-checks`.

All **21 final assertions pass** in [final-checks/done.txt](final-checks/done.txt), with [ordinary physics trace](final-checks/pace.csv):

- Normal motorcycle clears the dump with **2.16s flight / 2.28s crossing**, exactly matching the preceding release; no interior contact and CP2 progression passes.
- Motorcycle short-jump escape: **9.32s trash / 6.72s empty**, adding **2.60s** of actual traversal time.
- ATV short-jump escape: **9.86s trash / 6.70s empty**, adding **3.16s** of actual traversal time. Actual `moto` and `atv` profiles are asserted.
- Both short jumps land amid trash near the floor. Both vehicles also pull away from rest, steer through all four floor waypoints and climb out; 486/515 supported maneuver frames respectively.
- Zero resets, hard stops or trapped vehicles. During trash runs, minimum supported upright dot is at least 0.820; maximum upward speed is 5.30m/s versus approximately 13m/s in the empty-bowl cases. No unexpected upward launch or contact flip observed.
- No debris colliders, no global motor changes, no artificial penalties. Grounding and launch/bowl/floor visual inspection pass.

Targeted gameplay testing is complete and stopped. These bounded driving fixtures do not claim exhaustive human driving coverage; Dan reviews density and feel.

Unity's first authoring attempt could not resolve the new script until a full recompile. A capture CLI timeout occurred, but its scheduled operation completed and all three resulting images were inspected. No result is inferred from transport success alone.

## Delivery

Delivered **0.40.0-review1 / game-40000** from pushed completion source `c701d791f2a2648bca1bf72ab8ed69f07d735ccd`. Fresh Windows build succeeded with zero errors/seven warnings. Full 231-file identity, remote asset verification, public download/startup, production Play-Racer.cmd and cleanup pass. See [publication](PUBLICATION.md). Gameplay tests remain stopped; Reverse and optional shortcuts remain deferred.
