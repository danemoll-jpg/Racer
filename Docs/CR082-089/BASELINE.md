# CR-082–089 correction pass: baseline evidence

Safety checkpoint: `b1d076d948e2a8ce57bdfaeebd698b87e77bd5b6`. Saved project status was clean after verification. Both staging and committing initially failed on sandbox creation of `.git/index.lock`; supported elevated retries succeeded. No locks, hooks, signing, history or saved changes were discarded.

The unchanged installed baseline player was `Builds/Latest/Racer.exe` (the packaging workflow preserves that older complete runtime under Builds/Preserved), 0.13.0-review7, completion source `55656a28365c4037f8e9615a2344948be1002313`. Its ordinary-frame race matrix is retained in `../../CR082-089/baseline/`. The original validation runner's relative tag also placed its player logs under that root evidence directory; these are retained.

The existing compiled probe has no switch for both Street free-roam directions. It also omits pedal columns and reports only the last callback's first contact. A diagnostic-only baseline build in `Builds/CR087-baseline-instrumented` adds direction/mode switches, input columns, total velocity, water and upright telemetry. Its gameplay motor, configuration, contact handling and saved geometry are unchanged. `baseline-instrumentation.patch` and `baseline-hashes.json` record the distinction. No game correction preceded these baseline tests. Later diagnostics add all contact points and velocity components; they are not retroactively attributed to the baseline.

## Physical findings

- Original Reverse Street motorcycle, 12 m/s target, local lateral line -3: forward speed 11.859 → 6.665 m/s in one 0.02-second step at approximately (-629.521, 8.396, -83.927). Four suspension probes remain grounded. The speed controller requests zero brake at this speed. The identical instrumented reverse free-roam traversal reproduces it with explicit zero brake/zero water and upright 0.994.
- Reverse Street layout in free roam, opposite travel direction, Street Classic, 36 m/s, right-edge line 1.7: total velocity 34.989 → 3.333 m/s in 0.02 seconds at approximately (-624.843, 8.870, -78.819); upright 0.979, brake zero, water zero. This is a physical momentum loss, not just speedometer projection.
- The reverse takeoff is excluded by the name allowlist in `VehicleSurfaceContacts`; it recognizes `Takeoff -` but not `Reverse supported roadworks transition`. Sweep CCD is also absent from this surface-correction subscription. This is a testable cause hypothesis until corrected-versus-baseline driving confirms it.
- The active reverse mesh is `Reverse course authoring/Reverse supported roadworks transition`, `Assets/Track/ArcadeReview/TrickumReverse.asset`. Its frame is `Phase 4 - Connector Jump`, approximately (-626.16, 7.80, -109.99). The forward takeoff collider is disabled in Reverse Street; obsolete graded shoulder objects are inactive. Forward Street uses `Takeoff - 40m x 6m, 6.2m rise`, `Assets/Track/ArcadeReview/TrickumForward.asset`.
- Forward-layout traces include substantial yaw during some flights: forward-projected speed can decline while total velocity remains high. Other drops coincide with landing and removal of downward velocity. These are distinct from the reproduced reverse incline contact losses. Unexplained forward-layout slowdown is not declared resolved from reverse evidence.

`inventory-*.txt` records all four saved scenes' nearby collider identities, enabled/active/trigger states and raycast support heights/normals/triangle indices. `baseline-speed-events.csv` records speed before/at/half-second after losses, input, water, support and contacts. Do not count each event as a separate failed traversal, or equate forward-projected speed loss with lost total momentum.

## Completed instrumented free-roam matrix

Each row below covers 60 runs: four eligible vehicles × 12/24/36 m/s targets × local lateral lines -1.5, -3, 0, -4.7, 1.7. Both edge lines intentionally contact the bevel. Initial placement/speed occurs before the measured approach; subsequent movement uses motor simulation and ordinary physics frames, with no position forcing or launch boost. Concurrent diagnostic players were used; these are not performance measurements or physical-controller tests.

| Loaded layout / travel | Runs | Probe non-completion/unstable-final outcomes | Runs with upright below 0.65 at any point | Failed local recoveries |
| --- | ---: | ---: | ---: | ---: |
| Forward Street / forward | 60 | 2 | 23 | 0 |
| Forward Street / opposite | 60 | 1 | 18 | 0 |
| Reverse Street / reverse | 60 | 0 | 10 | 0 |
| Reverse Street / opposite | 60 | 0 | 9 | 0 |

Many runs marked `completed` nevertheless lose speed abruptly or bank/roll. Completion is not clean traversal. Failed cases remain in the raw CSVs and images. No human driving, visual acceptance, listening or physical-controller coverage is claimed.

## Contact API reference

Unity documents a separate sweep-CCD contact callback and its inverted normal convention: [ContactModifyEventCCD](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Physics.ContactModifyEventCCD.html). The repair must use the real authored top face and preserve side/underside response; no ignored contact, propulsion or global collision disable is an acceptable substitute.

