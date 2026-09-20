# CR-081 + CR-090 validation — 0.15.0-review3

Safety checkpoint: `8419e9194985955bb56a41854f1b5dce7968b270`.
First implementation checkpoint: `38fb5906e81b26f3a5cc615be8834227af72b15a`.
Delivery: **0.15.0-review3**. Complete-runtime packaging verification is recorded below.

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

Review2 (`release2`, assembly `7527452DDE0DFEF63EB8A9BFE3426622521A2EE8F14DFF6EEA222D5F5DFDEDC3`) passed 60 collection/record/ghost-fixture checks, all 24 collectible approaches and local resets, 474 household checks, 41 radio-continuity checks, 29 old-board checks and 43 checks after fourteen actual speed crossings. Rendered house front/rear, mountain, speed result and populated ten-entry board were inspected. Forest reverse motorcycle and ATV three-lap races finished without missed gates and saved real clean ghosts. Both replay observers passed pause/time/pose freeze and zero collider/Rigidbody checks. These are compiled observations, not controller or human acceptance.

Continuous review2 mountain traversal completed both directions: 1095.9/1110.7m and 1095.8/1110.7m; minimum upright vector Y .903 and .837; no water entry; both local recoveries succeeded. However the shore connection at x about 680 triggered the old street boundary and teleported the car roughly 300m. This is an actual defect, not a failed pilot alone. The boundary exemption now includes the authored lake-return corridors, preserving gate/shortcut independence. The test pilot's station window was also narrowed to avoid confusing the shared out/back segment near its entrance. Interrupted review2 runs are retained and marked superseded.

The correction is checkpointed at `f3d732d676ef4f7f7ae57b329855cad4f4729a4e`. Final `release3` results, performance and packaging checks are recorded below. Earlier candidate successes are not substituted for that final validation.

Review3 assembly: `EE44F9706AC6E2274DFC6A9C27973EC8FB19118E9BC1C3A76D2FF7A6A6FB94B9`. Focused compiled checks: 60/60 collection/ledger/ghost fixtures; 4/4 collectible counts after a separate process launch and all course changes; 5/5 virtual Gamepad menu checks. Continuous mountain and shore loops both complete in both directions. Mountain length 1110.7m, minimum upright Y .903/.837 and maximum lateral 4.03/3.49m. Shore length 798.7m, minimum upright .819/.818 and maximum lateral 1.00/1.01m. All four traces remain dry and recover successfully. The prior roughly 300m boundary teleport is absent.

One review3 Forest reverse motorcycle tree-reset diagnostic failed its locality assertion (delta -393.58m) while passing supported landing and preserved progress/time/penalty checks. The fixture explicitly clears recovery history before teleporting to its setup location. The observed delta matches fallback from the previous hill fixture's stale station (about 460.73) to spawn-minus-2 (about 67.19); the ordinary anchored candidate search retreats at most 160m. This indicates the fixture did not establish an earned anchor before requesting this reset, rather than evidence of a 393m retreat from an earned anchor. The original failure is retained and the repeat is reported below; the original assertion is not counted as passing.

All twelve eligible player/course combinations completed their three-lap races. Of 48 player/AI entries, 46 finished; all 48 had zero missed-gate penalties. The two AI DNFs were Street Classic in the forward Street motorcycle run (2/3 laps, 7 driver recoveries) and Street Classic in the reverse Street ATV run (2/3 laps, zero driver recoveries). Both use the normal finish-grace window; no extended deadline is substituted for a normal finish. Each player race wrote one total-race entry. The deliberately forced first-lap reset is separate from RoadDriver's recovery counter and correctly excludes that lap from ghosts while preserving the broader existing boards.

The forward Street tourer repeats the known persistent-obstruction timing failure: two recoveries, only 26.55m against the fixture's 60m target at 35 seconds, but travelling 23.31m/s and escaping at the deadline. This remains a failed timing assertion, not a claim of an indefinite stall or a new pass. The prior CR-075–080 report retained the equivalent 23.32m/s case.

The compiled reverse Street marked jump awarded exactly once in 12/12 actual race approaches (four vehicles, 18/28/38m/s). Rendered race feedback was inspected. Both new mountain result captures were inspected as well: Fern Creek 65.4ft / silver / PB / 1.36s / 335 points and High Ridge 134.3ft / gold / PB / 1.86s / 595 points. The Jumps empty state and controller selection are readable. Marked site inventory: ACTIVITIES.md.

## Known retained limitations

Final compiled matrices total **1,500 ramp traversals**: 744 Trickum (173 non-clean), 720 mountain (316 non-clean), and 36 extra activity approaches (zero non-clean). Every measured reset succeeded. Trickum's final aggregate matches the retained baseline's 173 non-clean count; it is not blanket success. Actual opposite outer-edge reverse Street motorcycle failure remains. Detailed per-matrix counts and individual failure rows are preserved in `release3/summary.json`.

Other final checks: automatic cameras 144/144; gate/activity rules 96/96; navigation/pause/finish/reset rules 280/280; household/existing systems 474/474; muted radio continuity 41/41; old lap/race boards 29/29; actual top-ten driving/reopening 43/43; free-roam recovery 80/80. Initial race recovery assertions were 281/282, followed by the 46/46 Forest reverse repeat. See the explicit failures below rather than interpreting these totals as universal physical acceptance.

The targeted accepted-build Street Classic Creek Leap comparison also reproduces one recovery on the clean attempt (27/28 checks). Thus both newly reported shortcut failures are reproducible on 0.14.0-review2 as well as review3. Neither is silently relabeled a pass.

The twelve compatible course/vehicle categories each saved an actual clean-lap pose file. Forward Street ATV required a separate slower four-lap solo run after its race's repeated resets correctly prevented a save. That valid lap was 158.178015s with 2,639 samples (532,708 bytes). The host audit checks saved timestamps/finite poses/storage bounds and matches ghost duration to an actual lap-board time. All twelve categories passed; cold-relaunch evidence is reported separately below.

Final rendered overlap evidence: `release3/mountain-LakeWoods-no/moto-213-1-16-0.png` shows Fern Creek 68.6ft / silver / new best / 1.44s / 353 points, the Pine climb acorn pickup (2/24), and actual radio artist/song text simultaneously, without overlap. Final `actual-top-ten/top-ten-after-driving.png` and `property-views/home-rear.png` were also inspected.

The repeated Forest reverse recovery diagnostic passed 46/46. Its original locality failure remains in the evidence and is not erased by the repeat. Physical shortcut checks returned Street 109/112, Street Reverse 56/56, LakeWoods 14/14 and Forest Reverse 22/26. The Street Classic, Touring and ATV Creek Leap pilots each completed with one recovery and zero misses, failing the clean-run assertion. Granite Saddle completed but never registered branch entry and reported four misses in both motorcycle and ATV runs. A targeted motorcycle repeat against accepted 0.14.0-review2 reproduced the same two failed assertions and four misses (`baseline-comparison/granite-saddle`, 11/13). This is a retained physical test failure, not proof of penalty-free shortcut traversal. All twelve full-race player runs had zero misses; those do not substitute for the failed isolated shortcut checks.

All 720 new mountain ramp cases completed their test matrix, with zero failed local resets. There were 316 non-clean traversals when either incomplete traversal or minimum upright Y below .65 is counted. The 144 marked-forward center/±2m approaches at 16/24m/s (about 36/54mph) all completed cleanly and awarded exactly once. Opposite approaches awarded zero. The 32m/s cases exceed the posted 35–55mph guidance; actual shoulder edges and opposite departures remain unstable. Some traces land validly and lose stability later on the following bend: an earlier valid landing award is not evidence that the entire downstream traversal remained clean. Raw geometry, inputs, speeds, contact and stability traces remain under each matrix directory.

CR-087's forward ramp instability and opposite-direction reverse outer-edge motorcycle roll remain open. A completed traversal or successful reset alone is not a clean landing. New mountain ramps are challenging; final speed, shoulder and opposite-approach failures must also be disclosed individually. Human review of the reference-informed house proportions, fences, sightlines, controller feel and ramp behavior remains necessary.

Player controls, map and combined checklist: [README-player.txt](README-player.txt).

## Performance and release integrity

Two serial 75-second compiled runs used real-time physics, explicit 1280×720 camera rendering, a 120 FPS cap and a 10-second warmup. Each had 20 active traffic drivers, one ambient people system and the Groove radio channel active, with output muted. These are offscreen measurements, not human display/controller acceptance.

| Area | Median ms | p95 ms | p99 ms | Peak process MiB |
|---|---:|---:|---:|---:|
| Mountain | 8.337 | 22.636 | 41.440 | 573.70 |
| Home / neighborhood | 9.365 | 53.734 | 111.741 | 629.13 |

Frame-time spikes remain, especially around the neighborhood. This pass does not claim a locked 60/120 FPS. The home probe's raw text shares the generic “Expanded mountain physical drive” heading; its `-area home` process flags and rendered capture identify the actual measured area. Both captures were inspected.

The source audit matches all 162 files in release-source-manifest.json and the final assembly hash. Final ramp counts are also available in [RAMP-MATRIX.md](RAMP-MATRIX.md).

All four separate-process ghost relaunches completed a lap without missed gates or recovery, wrote one total-race entry, and passed pause/time/pose freeze plus zero collider/Rigidbody checks. The final Forest replay capture was inspected. The fresh-process activity archive check passed 3/3: saved categories/PBs/medals load unchanged and do not create race progress. Collection relaunch across all four courses passed 4/4 with all 24 discoveries retained. `ghost-file-audit.json` includes the original twelve categories, slower solo fallback and cold-relaunch files.

Coverage limits: the full new mountain speed/shoulder/opposite matrix was performed in free roam. Automatic race jump feedback was physically verified at Trickum, but a complete race-mode replay of every mountain and Forest opening jump/profile combination was not performed. Forest ambient Trickum probes are physical ramp evidence, not awards for the separately located Forest opening activity. The explicit free-roam challenge/retry tests also do not by themselves prove unselected automatic race triggering. Keep these distinctions when reviewing the 1,500-case total.

No physical controller, human steering/ramp acceptance, listening test, exhaustive multi-hour soak, or exact photographic architectural match is claimed. Household exclusivity, terrain support, nearby fences and preserved gate/record rules have automated evidence; final visual proportions and remembered geography require Dan's review.

Packaging verified all 437 files across the extracted ZIP, versioned runtime and complete Latest. It preserves 187 playable staged songs plus two original M4A files (not radio-decodable); previous Latest/versioned files and older archives remain preserved. `package-initial-verification.json` records the first verification; release metadata is stamped and packaged again after the completion commit. A muted launch from `Builds/Latest/Racer.exe` passed all five virtual-controller menu checks with the same assembly hash. Final evidence comprises 107 completed processes, zero timeouts, and zero matches for the six exception classes listed in integrity-audit.json.
