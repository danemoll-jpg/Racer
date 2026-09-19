# 0.6.1 integrated correction — awaiting Dan's review

Scope: BUG-004–007, CR-041–045 and CR-036. No lake circuit, multiplayer, legacy environment rebuild, vehicle tuning, garage redesign, or unrelated house relocation.

Checkpoint: `88064bd196d68e40bfb624f2d5879613d962dbf5`. Initial staging/commit permission failures were retried through the supported elevated mechanism. The checkpoint succeeded before project changes.

## Identity and limits of the baseline reproduction

The actual original Latest was opened visibly. Its VERSION identifies 0.6.0-review1, source `a4eaa16dd86e96a300cfc16a54c342e1ec043763`. Its Assembly-CSharp SHA256 matched the versioned executable: `96A25B35B8B085EC0C8213FCCA6CB5CE9740C7E1B0CB5317512F3A872AC74BD7`. There were no Assets/Packages/ProjectSettings differences between that source and the checkpoint. Dan's persisted settings were read, not overwritten: master .8, ambience 1, feedback 1, vehicle .75, motorcycle, Hard, 60 fps/VSync.

The old standalone's existing ordinary-frame motorcycle/gully harness completed three branch attempts and two road comparisons with zero penalties. That does **not** reproduce Dan's exact false-penalty or silent-impact incident. The old executable lacks the new detailed branch/audio instrumentation. Source defects below are established; the exact combination in Dan's original run remains unobserved. Historical passing fixtures are not used to contradict his report.

## Race-credit changes

The old entrance required a single narrow plane crossing, body heading >.4, and height within four metres. Exit also required body heading >.5. A brief road proximity excursion or reverse out of the entrance immediately discarded branch context; no partial bypass credit survived abandonment. These transitions could turn an imperfect legal traversal into ordinary misses. Separately, RaceDirector rejected every physical gate unless nose heading exceeded .25, and all gates had a three-metre vertical half-height.

Entrances now use forward swept movement within a bounded 24m entrance apron and supported shoulder envelope. The branch earns contiguous, bounded movement evidence. Airborne evidence is bounded vertically and laterally, and movement steps over 10m cannot award credit. Nose orientation is irrelevant. Local recovery goes at/behind earned progress. Reversing retains context and previously earned gates.

Partial rejoin policy: off-route movement freezes earned evidence. Only 1.5 seconds of sustained forward travel on a separated main road abandons branch tracking. Gates already witnessed by contiguous branch progress stay credited. Remaining unearned gates follow ordinary road rules. A verified exit resolves the designated sequence. An entrance touch cannot authorize distant gates or a finish. Player and AI use the same sampling function; AI driving currently uses the safe road fallback instead of unvalidated stunts.

CP14 alone has an above-road 24m envelope, retaining its six-metre lateral half-width and three-metre below-gate limit. Other gates retain their bounded normal volumes. Swept direction, duplicates, >10m teleports, ordered progress and armed finish rules remain enforced.

**Penalty policy:** each genuine missed gate is exactly +5 seconds; three misses are +15; authorized shortcut gates are zero. No distance surcharge or replacement punishment. Elapsed/adjusted totals still have fractional seconds. `street-v6-flat5-*` separates new records; historical files and settings remain intact. Broad-cut tradeoff: a long cut can save more than its five-second-per-missed-gate cost. That is an explicit consequence of the requested simple policy, not secretly offset by a surcharge or forced restart.

110/110 final desktop checks passed: 98 swept/record fixtures plus 12 actual dynamic-body FixedUpdate crossings. Every profile crossed with a backwards-facing body while moving forward, crossed high while spinning, and rejected genuine reverse travel. Other cases include shoulders, duplicates, teleports, finish rejection, reverse/re-traverse, sustained partial rejoin, shared AI credit and byte-preserving record migration. The dynamic tests set an initial pose/velocity and let normal physics/RaceDirector updates run; they do not manually invoke Sample during travel.

## House, fences and jump

The floating structure was the exact `Approximate older residence` at (44,30.38,-451), yaw274.39. Only that authorized site was adapted. The new supported house is centred at (39.77,24.59,-447.34), yaw220.84. Its 44m interior rises six metres, with a 20m clear floor/opening, breakable sliding doors and upper exit window, structural side support, piers and roof. It uses physical ramp travel and the original gully terrain landing; no boost or teleport.

Failures retained: the initial 10m aperture caught motorcycle/ATV lines. A 20m widening alone still failed motorcycle runs because redundant solid crossbeams overlapped the continuous ramp surface. Removing those 12 embedded beam colliders preserved visible support and eliminated the repeated interior impacts. Final motorcycle clean/varied/recovery runs: 20.496/18.670/21.725s, both panes broken in each, zero charges/buzzes, minimum up .961/.970/.971. Road comparisons: 19.404/19.347s. The stunt is fun-oriented; the cautious clean run is slower than the road, while the quicker varied run saves about .7s.

The property layout has 169 breakable sections (48 chain-link, 121 white-X): Dan's chain-link and Houses #2/#3's white large-X fencing. Each has front/side/rear runs, a 12m front access opening and a 6m woodland opening. Front runs are 11m from the local road centre, beyond live lanes/shoulder. All46 unrelated building sites were compared with the checkpoint scene: positions, rotations and scales are unchanged. Player motor/profile/configuration source and all StreetLoop terrain assets have no changes from the checkpoint. Original oriented sensor overlap now controls restoration; a broad AABB had incorrectly suppressed wide diagonal glass restoration while the car was outside the doorway. Debris remains capped at 24 moving props and four seconds.

Jamerson is raised pavement works: existing supported 40m/6.2m takeoff, asphalt surface, graded aggregate shoulder, orange breakable work markers and an open-right-lane sign/bypass. Collision retains the tested continuous takeoff. Eight ordinary-frame flights (two/profile) credited CP14 and landed. Motorcycle fast approach 51.224m/s, apex17.531m above ramp base, air3.297s. A full-speed ATV drifted about6.4m left at **later CP15**, outside its six-metre half-width: a genuine +5 miss, retained rather than counted as a clean run.

| View | Before | After |
|---|---|---|
| Gully | [Floating house](before-checkpoint-gully.png) | [Supported house](after-wide-gully.png), [interior](after-interior.png), [exit](after-exit.png) |
| Dan | [Before](before-archive-clear-dan.png) | [Perimeter](after-wide-dan.png) |
| House #2 | [Before](before-archive-clear-Original-house-2.png) | [Perimeter](after-wide-Original-house-2.png) |
| House #3 | [Before](before-archive-clear-Original-house-3.png) | [Perimeter](after-wide-Original-house-3.png) |
| Jamerson | [Before](before-archive-clear-jamerson.png) | [Roadworks](after-wide-jamerson.png) |

## Captured impact audio

Original generated clips were valid, but gains were only .10–.28 before the vehicle/master controls, fully spatial attenuation began at8m, priority was160, and full voices silently dropped new events. Distant impacts could consume the common onset budget. These are concrete weaknesses; they are not proof of the exact cause of Dan's unrecorded silent run.

The revised system uses .24–.62 speed-dependent gains, 14m minimum distance/75m reach, priority80, 80% spatial blend, bounded four-voice replacement, and rejects distant events before consuming the onset budget. Five materials have original synthesized variants, including glass. Master and Vehicle remain authoritative; no saved volumes are overridden.

`audio-final/` contains actual listener DSP mix WAVs during physical authored-prop hits with engine throttle. All15 material/volume combinations broke the intended prop and emitted events. Default master.8 peaks .120–.196; lowered master.25 peaks .031–.069; all five master-zero captures have peak/RMS exactly0; no clipped samples. These are mixed audio samples, not just AudioSource assertions. OS-endpoint capture and subjective listening were **not** performed. Dan should judge impact prominence/material character on his actual speakers.

## Traffic, AI and performance

Configured population: 16 dedicated Hwy92 cars, four per lane/direction pair, plus four local-road cars; inspector bounded0–24 highway population. Following uses physical obstacle queries; passing checks destination-lane space; merging inner traffic yields. Recycling checks both old/new player distances >=220m, excludes visible recycling within500m, and checks same-lane55m spacing plus immediate collision clearance. Only ambient traffic is pooled; racers never get teleport catch-up or extra vehicle capability.

The first traffic trace exposed an excessively broad recycling veto: any car within55m, even in another lane, and far horizon visibility prevented reuse, dispersing highway cars onto local roads. That failed candidate is retained as `traffic-recycle-before.csv` and `normal-interrupted-traffic-recycle/`. The race was interrupted; it is not a completed pace result.

Normal/Hard use more of each existing profile's cornering/braking capability, higher supported hill/crest targets, less Normal lifting and stronger throttle response. Easy's pace constants remain unchanged. Exact race/sector/traffic/performance results are recorded separately after the integrated race runs finish. Automated reference-pilot gaps do not establish Dan's winning chance.

## Evidence and remaining review

- `../CR034-039/correction-routes1/`: 80 initial ordinary-frame attempts across all four routes/profiles, including retained house failures; all gate charges zero.
- `../CR034-039/correction-gully-inside-recovery/`: widened-house matrix, with motorcycle failures retained and all other profiles passing.
- `../CR034-039/correction-moto-continuous/`: motorcycle rerun after removing embedded collider seams.
- `rules-partial-first/`: two retained fixture failures; the old southwest cut has no bypassed gates and needs a longer separated-road observation interval. Corrected assertions pass without weakening runtime rules.
- `audio-loaded-engine/`: glass-restoration failure retained; superseded by `audio-final/`.
- `jamerson/`: all eight complete flight traces, including the genuine later ATV miss.

Review checklist (ten feedback items):
1. Take shortcuts imperfectly: no bypass penalties/buzzes; reverse/recover/rejoin.
2. Jump CP14 and cross facing backwards while travelling forward: one credit.
3. Deliberately miss gates: exactly five seconds each.
4. Race Normal and Hard: assess challenge; confirm Easy remains comfortable.
5. Drive through the gully doors, ramp and upstairs window; recover after failure.
6. Check all three property perimeters and driveway/woodland access.
7. Hear wood, chain-link, sign/mailbox and glass; try lowered volume and mute.
8. Drive all four busy Hwy92 lanes through traffic and merges.
9. Review the Jamerson roadworks design, flight and safe bypass.
10. Check garage/colors, pause/settings, local reset, restart, Quit Race, results and old/new records; lake circuit remains backlog.

Build/package identity, final flow checks, completed race measurements and completion commit are added to SESSION HANDOFF and the final delivery report. No upload or distribution is authorized or performed.


