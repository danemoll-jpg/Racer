# CR-081 + CR-090 validation — final verification in progress

Safety checkpoint: `8419e9194985955bb56a41854f1b5dce7968b270`.
First implementation checkpoint: `38fb5906e81b26f3a5cc615be8834227af72b15a`.
Target delivery: **0.15.0-review2**. Latest is not replaced until packaging verifies the complete runtime.

## Evidence and interpretation

Every game test uses an isolated `Temp/cr081-*` save and temporary master mute, plus the opt-in runtime listener guard. No saved player audio settings were changed. No physical controller, human driving or listening acceptance is claimed. Virtual controller events are identified as automated tests.

`process.json` identifies the actual executable, assembly SHA-256, flags, isolated save, exit status and timeout. Accelerated driving uses the normal authored scenes and 0.02-second physics steps; it is not a performance measurement. Rendered feedback captures use the compiled game's actual HUD. Fixture tests are labeled separately from physical driving.

## Retained baseline and superseded candidates

- Accepted 0.14.0-review2 baseline: 744 Trickum traversals, 173 non-clean cases, no failed local recoveries. See `../CR082-089/cr081-baseline`. The old probe forced Forest ambient ramp tests into free roam even when a nominal race flag was supplied; those rows do not prove Forest race behavior.
- `first-geometry`: invalid evidence of mountain traversal. CircuitBoundary clamped the vehicle back into the old map, and the initial probe misread wrapped station progress. Both defects were corrected and tests repeated.
- `boundary-fix`: 48/48 race speed-camera checks; 55/55 ledger/collection checks. Eight centered, forward mountain ramp traversals at 24 m/s completed with one award each and successful recovery. Opposite creek departures prompted wider support and tree clearance.
- `refined`: migration, top-ten/tie/dedup/reload fixtures and ghost replacement/rejection fixtures passed. The initial headless view coroutine used an unsupported end-of-frame wait; the capture harness was corrected.
- `inspection`: rendered ten-entry board after fourteen actual crossings, unknown-date historical migration, and readable race/jump feedback inspected. Opposite creek motorcycle/ATV instability remained; extended observation showed that some car attempts recovered their route after leaving the original narrow observation corridor.
- `final` is the **superseded review1 candidate**, not the delivery evidence. Rules 96/96; local recovery 282/282; roam recovery 80/80; household/system checks 474/474; radio continuity 41/41; old lap/race boards 29/29. Street speed cameras passed 48/48 in each direction variant; Forest failures exposed an inappropriate grounded/jump warmup restriction, now removed from cameras. Candidate race traces exposed terrain changes near a reverse shortcut; the original race/shortcut corridors have since been restored from the safety checkpoint. Candidate collected-at-spawn and relocated-shore placement failures are retained. Partial mountain matrices were stopped under memory pressure and then superseded by these fixes.
- `CR081-protect` was an unintended diagnostic build made by the earlier permissive job dispatcher before the new authoring method had compiled. It did **not** restore terrain. The dispatcher now rejects unknown jobs; only the later per-scene `route-protection-*.txt` files document actual restoration.

The safety scene copies used for terrain comparison are local, ignored validation inputs. Authored race roads, gates and shortcut arrays were not regenerated. Nearby terrain support was restored from those original meshes; expanded property and mountain terrain remains outside the protected corridors. Whole-tree meshes and colliders are kept coherent. `support-audit-*.csv` samples the actual exploration routes; `authored-ramp-geometry-*.csv` records supported height and normals around both new ramps.

## Final validation

Final `release2` results, packaging checks, performance and completion commit will be recorded here after they finish. The earlier candidate successes above are not substituted for that validation.

## Known retained limitations

CR-087's forward ramp instability and opposite-direction reverse outer-edge motorcycle roll remain open. A completed traversal or successful reset alone is not a clean landing. New mountain ramps are challenging; final speed, shoulder and opposite-approach failures must also be disclosed individually. Human review of the reference-informed house proportions, fences, sightlines, controller feel and ramp behavior remains necessary.

Player controls, map and combined checklist: [README-player.txt](README-player.txt).
