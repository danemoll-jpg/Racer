# CR-056 — Forest Loop / 0.8.0-review1

Implemented for review; awaiting Dan's review. Automated evidence does not establish enjoyment, physical-controller acceptance or human playtest approval.

Safety checkpoint: `a4c7825042eefbac16bba13d3b2592fb15d7dc8d`. Git's sandbox index-lock denial was retried successfully through elevation before project edits.

## Revision

- The second scene retains its `LakeWoods` scene identity for compatibility, but selection, HUD, results and record labels say Forest Loop. New records use `lake-v2-forest`; `lake-v1` boards remain historical and untouched.
- The existing lake was at (642,78,-20), with the old grid at approximately (511,85,-125). The revision retains the lake location, exposes its shoreline and puts the grid behind the existing friend's house, on the western shore. All house roots remain in their accepted positions.
- The roughly 2.12 km main loop has six jump opportunities, compared with three previously: creek crossing, deep gully, linked root roller and kicker, ridge drop, and homeward leap. Echo Cave adds a seventh jump. `route-map.svg` labels the takeoffs and supported landing zones.
- Typical dirt width is 7.2 m, with 10 m passing pockets and a 12 m grid. Measured landing fans receive local tree clearance, and the homeward launch is moved onto a straight approach. Dense trees remain close to the rest of the route; forest visuals are combined into spatial mesh batches.
- Echo Cave is a separate rock enclosure with bends, lighting and a gap on its straight diagonal. Its entitlement entrance is inset past the shared fork, so ordinary main-route travel cannot accidentally acquire cave recovery context. Granted bypass credit remains irrevocable through imperfect driving and recovery; no extra shortcut penalty was introduced.
- Player selection, restored choices, fixed/random/mixed AI choices, spawned opponents and new record insertion enforce motorcycle/ATV eligibility. The original street track still permits all four profiles.
- Civilian drivers use the retained street centerline for spawning, steering, avoidance, recycling and recovery. Race opponents use the forest centerline. Street traffic remains enabled.

## Slowdown diagnosis

The investigation separated collision-cache faults, real vegetation contacts and authored transition seams:

1. The supporting-face correction for speculative CCD cached only the first loaded scene. Selecting Forest Loop after the street scene left its terrain colliders absent from that cache. On the revised candidate before this fix, motorcycle launch minima were 10.81, 0.04, 9.60, 6.64, 5.33 and 13.07 m/s at the six tested locations. All tests began at 32 m/s and then held full throttle with zero brake. The cache now refreshes for the newly loaded scene. Actual wall, tree, vehicle and underside collisions remain active; no launch force or speed boost was added.
2. Trees intruded into swept landing paths. The original-build editor trace includes a first-jump motorcycle trunk contact followed by a drop from 42.24 m/s takeoff to 18.99 m/s at renewed support. Later candidate traces identified two remaining intended-speed landing obstructions, which received local clearance corrections.

3. The candidate cave fork selected the nearest of two different path heights, leaving a lateral ridge. Continuous weighted surface blending removes that ridge. In the 2–6 second ATV entry window, peak ground impulse fell from 1537.94 N·s to zero; the first measured kick was 237.882 N·s with zero brake. The final-climb street blend also received a continuous taper instead of a coordinate cutoff.

`before/` contains the original-course editor measurements. `candidate-before-contact-fix/` and `after/` provide the same revised main geometry before/after the scene-cache fix. The latter is a full-throttle stress probe, not a recommended-speed guarantee: real overspeed departures and tree impacts are retained in its logs. `nominal-before-clearance/` retains the failed first intended-speed pass. Final intended-speed evidence is recorded separately. The intermediate `nominal/` run also exposed a curved homeward landing; the final revision moves that launch 100 m earlier onto its straight approach. AI follows the tested 32 m/s central line through authored launch zones; unchanged difficulty coefficients still control braking/cornering elsewhere.

The full-throttle cache-fix comparison produced motorcycle launch minima of 38.79, 40.40, 40.50, 39.40, 39.51 and 39.47 m/s after the repair. These are measured over the launch window, not artificial target speeds.

Telemetry includes commanded pedals/steering, speed, grounded probe count, body attitude/angular speed, contact collider and impulse. Later passes also record summed suspension lift and alignment torque. Initial poses and velocities are test fixtures; subsequent movement uses the normal rigidbody and vehicle motor.

## Preservation and scope

Radio channels, recursive/custom/bundled music, top-ten storage, difficulty constants and handling coefficients are preserved. The original street scene and its authored terrain/building assets are not regenerated. Shared vehicle-force code only gains diagnostic observations; the contact-cache lifecycle repair applies the existing correction to the correct scene.

Failed candidates and test limitations are retained as evidence, not counted as passing tests. In particular, directories ending `-final`, `-clear` and `-straight` are development candidates; their filenames do not establish a passing final result. Final delivery results are identified explicitly below. Wildlife, subjective fun and physical-controller approval remain outside automated completion.

## Seven-comment review checklist

- [ ] Name: Forest Loop is clear in selection, HUD, results and records; historical records remain available.
- [ ] Lake/start: lake and shoreline are unmistakable from the grid behind the friend's house and the opening route.
- [ ] Slowdowns: try every jump, trail transition, and the original house/road jumps; distinguish normal climbing/impacts from abrupt false braking.
- [ ] Cave: attempt clean and imperfect runs, the gap, falls and local recovery; verify useful time savings and retained bypass credit.
- [ ] Jump variety: review all six main jumps plus the cave jump, especially the visible creek, deep gully, linked jumps and drop.
- [ ] Traffic: civilian cars stay on actual streets, including crossings and recovery; opponents continue racing the forest course.
- [ ] Forest riding: motorcycle/ATV eligibility, narrow paths, passing pockets, readable vegetation, multi-lap racing and performance.

## Final test and package results

The delivery geometry is identified by these final evidence directories, not earlier candidate filenames:

- `nominal/`: **12/12** motorcycle/ATV jump runs complete, with zero recorded trunk/cave-wall impacts over 50 N·s. The pilot targets 32 m/s and uses normal pedals, suspension, stability and rigidbody contacts. Normal uphill losses remain.
- `final-main-moto/`, `final-main-atv/`: **8/8 riders finish two laps** (16 completed rider-laps), zero missed gates. Motorcycle-led race: no automatic recoveries. ATV-led race: two AI riders recover once each; player and remaining rider need none. Both explicit local-recovery progress checks pass.
- `smooth-cave-moto/`, `smooth-cave-atv/`: **12/12 physical attempts complete**, including eight cave attempts and four comparison runs on the main route. Every attempt has zero missed gates and zero penalty. All four requested cave recoveries succeed.
- `rules.txt`: **28/28** eligibility, historical-record, gate placement, traffic placement/recovery/recycling and physical cave-fall checks pass. `falls.csv` records gravity-driven topple fixtures starting at up=-0.342 and recovering to up=0.998 with four supported probes; no extra gate, penalty or earned progress.
- `records/`: **29/29** top-ten/storage checks pass.
- Final full-race traffic traces contain **1,320 samples**, all using the street route. Maximum street-centerline distance is 6.909 m, within the retained highway lanes. Four guarded civilian recoveries and four guarded highway recycles were also exercised successfully; no global traffic removal.
- `street-routes-moto/`: **24/24** original street shortcut attempts complete, including the house/gully jump, imperfect lines and recovery; **77/77** route invariants pass. `street-road-jump/`: all eight vehicle/speed flights land and credit the jump gate. The fastest ATV stress run misses a later gate and receives its ordinary five seconds; it is not a zero-penalty pass.

| Jump | Motorcycle takeoff / first landing, m/s | ATV takeoff / first landing, m/s |
|---|---:|---:|
| Creek crossing | 30.19 / 27.96 | 29.51 / 27.10 |
| Deep gully | 31.51 / 34.19 | 31.49 / 34.18 |
| Root roller | 31.36 / 29.87 | 31.51 / 32.09 |
| Linked kicker | 31.49 / 27.43 | 31.49 / 30.12 |
| Ridge drop | 31.02 / 29.97 | 31.06 / 30.11 |
| Homeward leap | 31.36 / 31.35 | 31.29 / 30.95 |

The before/after cache-fix comparison is the earlier identical-geometry full-throttle experiment; the table above is the final intended-speed run. These are different test conditions and should not be conflated. Airtime fields in the raw summaries accumulate unsupported samples over each probe window, rather than claiming one uninterrupted flight.

| Timed segment | Motorcycle | ATV |
|---|---:|---:|
| Main route | 33.019 s | 33.058 s |
| Clean cave | 32.248 s | 32.527 s |
| Faster repeat with imperfect entrance line | 31.657 s | 31.997 s |
| Braking + local recovery | 34.017 s | 34.398 s |
| Excursion, reverse + recovery | 34.880 s | 35.477 s |

The shortcut advantage is deliberately modest: 0.53–0.77 s in clean baseline runs, up to 1.36 s in the faster repeats. A recovery erases it naturally. The two paths use identical controls on their common exit section. No launch boost or shortcut-failure charge is used.

The initialized geometry audit samples the main route every 2 m across three swept lanes and finds no cave-wall intersections or false cave entries. The spline is guidance, not the collider surface: its height can differ locally from preserved/sculpted terrain; physical driving and contact traces are the acceptance evidence.

Rendered performance: explicit offscreen 1280×720, normal simulation speed, 9,648 frames, median **8.334 ms**, p95 **8.336 ms** (120 FPS cap), observed peak working set **795,058,176 bytes / 758 MiB**. All four riders finish the one-lap rendering run with zero misses or automatic recovery. This is a local rendering measurement, not a promise for other hardware. The final visual-only creek enlargement keeps the same primitive/material and changes no collider.

Packaging verification: **428 files and 187 staged songs** match SHA256 across the extracted ZIP, versioned Windows runtime and complete Builds/Latest. The source manifest covers 2,433 files under Assets, Packages and ProjectSettings. Play-Racer.cmd remains unchanged; previous Latest/runtime/ZIP are preserved by the packaging workflow. `package-precommit.json` records the pre-commit verification. The final completion ID is stamped into VERSION.txt and the same workflow is rerun; final archive hash/report lives at Builds/PACKAGE-LATEST.json. No external upload or custom music-library import occurred.

Remaining review limits: modest shortcut reward, placeholder low-poly art, occasional AI recovery, overspeed/large off-line impacts, and hardware-dependent performance. Scripted completion does not establish fun, physical-controller quality, visibility on Dan's display or human approval. CR-040/056 remain awaiting Dan's review.






## Gameplay views

- [Grid beside the lake](gameplay-grid.png)
- [Visible creek crossing](creek-view/creek-0.png)
- [Cave jump and supported landing](smooth-cave-moto/cave-gameplay-1.png)
- [All takeoffs and landings on the route map](route-map.svg)

Final player build succeeds with zero errors. The two build warnings concern future Unity mesh pre-baking behavior and the deliberately excluded Runtime Pipeline; final tested players report no runtime exceptions/assertions. The creek visibility enlargement changes only a non-colliding water primitive; its follow-up motorcycle and ATV probes both pass.
