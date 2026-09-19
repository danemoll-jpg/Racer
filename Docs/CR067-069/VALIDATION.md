# CR-067–069 combined review

Status: implemented and technically validated; awaiting Dan's review.
Windows review version: 0.11.0-review1. Safety checkpoint:
`255a61b2095c1cad871b16640afff30eb979df22`.
The initial Git write failed on `.git/index.lock` permissions; supported elevation
successfully committed all saved changes before project edits.

## Exact property and geometry

The corrected property is **Remembered house behind southern hairpin**, world
position `(484, 23.90, -596.20)`. It is neither Dan's house nor the Fox Gully
stunt residence. The original StreetLoopBuilder and StreetLoopRevision explicitly
place that remembered house behind the southern turn. Commit `b374ee2` later
introduced `South Cherokee Lane south` at road station 1195, pointing south.
Its 9m pavement begins before the junction and crosses the intended hairpin;
its 17m-wide tunnel canopy lies beside the house around `(503.68, 28.69, -597.45)`.
The structure reported as a rear bridge is that decorative traffic canopy.
The before views and mesh bounds identify this one property/extension together.

Removed the entire extension hierarchy, including **25 colliders per scene**,
its CircuitBoundary, four decorative traffic slots, closure/gantry, painted dashes,
pavement/shoulders, inactive old embankment, and tunnel structures. Removed its
creation call from CombinedWorldUpdate. ContinuationTerrain only processes live
ContinuationTraffic roots and cannot regenerate the removed site. The other three
continuations remain: Hwy 92 east/west and Jamerson Rd west.

RaceRoad and WoodlandRoute arrays are separate from the decorative traffic route.
RoadDriver uses those accepted race/ambient road arrays; VehicleRespawn consults
CircuitBoundary dynamically. No AI, gate, branch, or recovery reference depends
on the removed root. Historical mesh assets can remain unreferenced on disk;
there is no inactive scene root or collider keeping the old geometry alive.

The original continuous ground supplies the restored driving and rear surfaces.
A 4.4m gravel core with soft shoulders follows a narrow house-to-road access and
stops painting inside the 5m centerline corridor. It changes 70 terrain colors,
not heights, normals, triangle topology or collider shape. Six supported trees
fill the released rear corridor. The scoped authoring operation was run again:
no duplicate trees, no new extension, no cumulative driveway repaint.

`preservation.txt` compares every surviving transform and confirms all pose values
are preserved; the sole changed existing Transform is the continuation parent,
whose child list loses the removed root. Both courses' RaceRoad, WoodlandRoute,
ForestLayout and ShallowWater components are byte-identical to the checkpoint.
Street records use **street-v10-hairpin** because removing an overlapping physical
surface can affect timing. Forest remains **lake-v3-shallows**. Historical saves
are retained, not rewritten.

| View | Before | After |
|---|---|---|
| Overhead | [Before](before-overhead.png) | [After](after-overhead.png) |
| Low approach | [Before](before-driving.png) | [After](after-driving.png) |

`final/hairpin` contains actual chase-camera physical traversal captures and CSV:
all four profiles cross the hairpin and re-enter from its shoulder. All eight
attempts complete, with zero recovery, zero wrong-way warnings and minimum upright
0.984 or better. The 240m test segment takes about 9.1–10.6 seconds. Full-race
hairpin speed samples are compared with 0.10.0 under
`hairpin-speed-comparison.txt`: no systematic slowdown is observed, but random
traffic/mistake seeds differ, so this is not a controlled causal benchmark.

## Household diagnosis and behavior

The old release made clock-seeded independent random choices both at Awake and
RestartRace, with no saved repetition control. Forced fixtures had demonstrated
that states existed, but not that drivers could see them. The first candidate
unforced release run reproduced the visibility failure: football 0/3 and smokers
0/2 visible in sampled normal street approaches, while coffee was 2/2 visible.
Football was roughly 37–44m behind the road and used smaller figures; smokers were
farther into the friend's property. Those diagnostic failures remain in the
initial folders rather than being reported as passing.

Each course now keeps its own saved shuffled three-visit Dan bag (football,
coffee, empty) and independent two-visit friend bag (smokers, empty). A refill
avoids repeating the previous visit across its boundary. Each new race consumes
one visit and saves the remaining bag; scene loading/menus consume none. Fresh
bags use fresh entropy, not a fixed startup seed. Coffee cannot repeat immediately;
all three Dan states occur within at most five consecutive races. Friend occupancy
alternates independently of Dan's bag. Laps, pause, recovery and passing the property
do not select a new arrangement. The old forced-seed path remains diagnostic-only.

Football remains inside Dan's original fenced yard, 14–17m from the road with a
visible three-person throwing/catching triangle and a larger football. Every ball
flight interpolates inside that triangle. Smokers remain at the friend's original
frontage, 13m from the road, with raised-hand cigarette gestures and visible smoke.
Coffee stays at its accepted position. Figures/props remain non-colliding; no
floating labels are added. All inactive pool states are explicitly disabled.

Ordinary unforced standalone visits use one isolated persisted save, four races
per launch, two launches per course. `final/households-*/ordinary-visits.csv` records
selection, seed, active count, and actual camera-frustum plus terrain/solid-occlusion
visibility from four standard chase-camera street approaches. All four experiences
were demonstrated within four new races on each course. All four launch suites
pass 22/22 checks; selected football is visible 3/3, coffee 2/2 and smokers 2/2 in
at least one approach sample. Every frame is retained as a PNG. These are ordinary
selection paths with diagnostic camera placement, not human-driven acceptance.

**Location limitation:** the original homes remain beside the neighborhood street
in both scenes. The Forest race route is about 108m from Dan's yard and does not
pass these properties. Forest household evidence is explicitly a street visit in
that scene, not a claim that they can be seen from its active racing line. Moving
homes/occupants onto the Forest trail would violate the original-location constraint.

## Wrong-way guidance

A runtime WrongWayGuidance component tracks signed local centerline displacement
with a 16m temporal projection window on the main course, wrapped at the lap seam.
An earned active shortcut supplies its own route direction. Real displacement
must agree with signed route travel; vehicle facing is not used. The HUD arrow
rotates relative to the camera's horizontal heading using the local route tangent,
not a vector to a distant gate. No vehicle, progress, penalty or reset is forced.

Wrong travel starts accumulating below -4.5m/s, sustains below -3m/s, and warns
at three seconds. Grounding requires at least two suspension contacts. Airborne,
unsupported, stationary/very slow and invalid corridor samples clear accumulation.
Correct travel clears the visible hint within 0.15 seconds. Pause freezes time.
Recovery, finish, restart and track lifecycle clear guidance. Brief reverse/spins
cannot accumulate the three continuous seconds required.

The compact screen-space panel sits below the existing race notice and shows
`Wrong Way`, a local directional arrow, and the active reset binding. Keyboard is
**R**; generic/Xbox-style gamepad north is **Y**. Device-specific button display
names, such as Triangle, are retained when supplied by Input System. Controller
input in these tests is emulated, not a physical-controller test.

The first candidate exposed generic `Button North` wording and a test that disabled
the race's reset-event subscription. The label is corrected; the reset fixture now
keeps the actual notification path live. Signed-route tests cover both courses,
the hairpin, lap seam, all four Street branches and the Forest cave branch, including
backwards-facing valid forward movement, wrong movement, delay, stop/slow reverse,
airborne samples, pause, recovery, restart and prompts. Physical pilots separately
exercise sustained reverse-route travel, actual keyboard/controller reset input,
and correct travel.

## Delivery scope

Technical validation does not establish human driving, subjective
readability/action recognition, sound quality, enjoyment or physical-controller
acceptance. Those remain for Dan. No split screen, networking, stunts, ghosts,
external upload, new vehicle physics or audio changes were implemented.

## Dan's concise review checklist

- [ ] Hairpin: all four vehicles, wide entry/re-entry, local reset and restart; inspect the retained house, narrow driveway and wooded rear.
- [ ] Households: start up to eight new races and relaunch; see coffee, football, smokers and nobody at the two original properties; assess recognizable poses and no visible swaps during a pass.
- [ ] Wrong way: drive backwards along each course for three seconds, then correct course; verify local arrow and R/Y prompt, pause, cave/shortcuts, hairpin, seam, spins and brief reverse.
- [ ] Preservation: garage/colors, records/history, AI estimates/mistakes, radio/music, signs, water, wildlife/audio and jumps; test a physical controller.

## Verified regression results

- Eight physical hairpin attempts / four profiles: 17/17 checks; zero recoveries or wrong-way warnings.
- Ordinary households: 22/22 checks on each of four application launches, eight races per course; no forced seed. All four experiences visible within four races on each course's retained neighborhood street.
- Physical wrong-way pilots: 6/6 on Street and 6/6 on Forest. Warning begins at 3.378 / 3.294 seconds including acceleration. Emulated R and Y perform actual local recovery; R event displacement 0.98m / 0.04m and unchanged gate/penalty state. The initial 0.1-second Street R assertion was premature; the event-based fixture now observes the actual reset.
- Full two-lap races with normal AI estimation disabled: 6/6 Street and 6/6 Forest. All racers finish with zero misses. Local recovery can still occur through the preserved AI mistake/recovery behavior; zero-recovery claims apply only to the focused hairpin matrix.
- AI estimates: 96/96 each course; records and historical-category rules: 29/29; vehicle art/paints/clones: 116/116; radio: 24/24.

The `final` folders hold the successful household, hairpin, art and radio evidence.
`verified` holds fresh-save full races, AI estimates, records and physical guidance.
Earlier count-based record checks reused an initial diagnostic save and found more
than one saved result; those reports are retained as fixture failures. Repeating
with independent save directories passed without changing records production code.
The first added finish fixture reused identical timestamps on later laps, so the
race never finished; its corrected monotonically increasing timestamps and final
lifecycle results are recorded with the packaged-source checks.

Performance comparison: Street measured 16,622 rendered frames at 1280x720,
normal time scale, 120fps cap, 20 people, eight wildlife slots, traffic and radio.
Median 8.334ms; p95 12.297ms. The 0.10.0 report recorded median 8.334ms and p95
11.899ms with 22 people and different random seeds. The median is unchanged;
the measured p95 is 0.398ms higher. These are different randomized workloads,
not a controlled attribution or proof of zero possible slowdown. Current p95
remains within the 16.67ms 60fps budget. This is explicitly rendered offscreen
performance, not foreground presentation or human-controller acceptance.

Forest performance: 17,924 frames, median 8.334ms / p95 8.676ms, with 18 people,
five wildlife slots, four traffic vehicles and radio; same resolution/cap and
normal time scale. Prior 0.10.0 p95 was 8.568ms under a different randomized
population. Both current course p95 values remain below 16.67ms. Wildlife sightings,
longer empty periods and continuing radio playback pass 4/4 on each course.

## Final build and delivery verification

The final clean Windows x64 non-development build succeeds with zero errors and
one existing optional RuntimePipelineConfig warning. Its Assembly-CSharp.dll
SHA256 is BA52B566653B4535142D5E6CBD2CB523481161B010F909F7ED7E212BF02A1EC0,
identical to the final tested release assembly. The earlier pending-editor-code
warning is absent after recompilation and the clean build.

Final lifecycle suite: Street 68/68, Forest 36/36. Final physical pilots: 6/6 each;
warning onset 3.374 / 3.299 seconds including acceleration. R recovery preserves
progress/penalty state, moves locally 0.68m / 0.05m and clears guidance; emulated Y
also clears through actual recovery. Finish and track-change clearing pass.

Packaging verifies all **429 files and 187 staged songs** by SHA256 across the
versioned runtime, complete Latest folder and extracted ZIP. SOURCE-SHA256.txt
records 2,514 source/assets/settings/package files, verified against the workspace.
Play-Racer.cmd is unchanged. Previous runtimes, ZIPs and Latest music are retained
under Builds/Preserved. No external upload occurred.

The completion commit is stamped into VERSION.txt and repackaged with the same
workflow after committing this report. Builds/PACKAGE-LATEST.json is the authoritative
final package hash/verification record; package-before-commit-stamp.json documents
the pre-stamp verification. Source and compiled gameplay remain unchanged during
that metadata stamp. Use Builds/Latest/Racer.exe or the existing Play-Racer.cmd.

Post-packaging smoke tests launched the complete Builds/Latest player with fresh
isolated saves: Street 68/68 and Forest 36/36, both exiting successfully. This
checks the actual packaged runtime, including finish/restart/track lifecycle.
