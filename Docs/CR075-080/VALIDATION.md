# CR-075–080 combined review

Safety checkpoint: `13f989a408aaa6d085167d7cb480d9886af1997c`. This is a technical review delivery, awaiting Dan's driving, controller and listening review. CR-081 remains approved and queued; no ghosts, collectibles, neighborhood expansion, split screen or networking are included.

## Routes and signs

The modeled west connector is **Trickum Road**. **Hwy 92** identifies the northern commercial road. **South Cherokee Lane** ends at the separate **Jamerson Road**; the retained distant destination board says **TO Jamerson Road**. Historical object identities are not globally renamed and the neighborhood topology is unchanged.

The four `inventory-<scene>.txt` files list active/inactive mounted lettering, shortcut entrance/exit stations and bypass gates, approach sightline probes and activities. `*-approach.png` are road-height views at the actual approaches, without debug labels. They are automated camera captures, not Dan's driving evidence. Reverse route geometry/maps from `../CR070-074/map-StreetLoopReverse.png` and `map-ForestLoopReverse.png` remain applicable.

Both reverse Street branches (Laurel Switchbacks, Granite Creek Cut) and both reverse Forest branches (Granite Saddle, Fern Gully) have physical direction-specific boards. Each final approach placement has 0/135 obstructed sightline rays from three approach distances. The selected side minimizes occlusion without clearing whole areas of trees. Exact retired hairpin sign exclusion remains. Physical lettering uses the existing depth-tested font material.

Wrong-way detection requires approximately five seconds of grounded wrong-direction movement. Orientation alone and airborne rotation do not count. Local route/branch history guides reacquisition; Forest street excursions orient guidance toward the associated trail. Candidate3 ordinary street excursions, 570–590 metres outside the trail corridor, produced visible warnings at 5.28 seconds for both eligible vehicles on both Forest directions.

## Recovery and ramps

See `INVESTIGATION.md` for the reproduced old shoulder obstruction and `RAMP-TESTING.md` for the standing requirement. Ramp traces record ordinary simulation frames at the actual authored location, including velocity, contact normal, suspension lift, alignment torque and recovery. Setup placement occurs before each measured approach. No boost, handling adjustment or global collision disable masks the issue.

Candidate2 reverse ramp: all 60 approaches cleared and recovered. Forward regression exposed seven side-contact failures in the unchanged stepped shoulder geometry; these failures are retained. Continuous supported bevels and rollout now replace those steps in both Street directions, retaining each launch run and height. A traversal marked `completed` means it cleared the ramp; minimum upright values and contact traces must also be considered when judging a clean landing.

Candidate3 recovery: 282/282 assertions across all eligible profiles/course variants. These include actual nearby tree, water and ramp locations plus controlled hill-bottom, occupied-pad and overhead fixtures, followed by ordinary-frame support checks. Branch midpoint/cave recovery preserves earned station/gate state. These are automated fixtures, not a claim that a person drove every hazard. Candidate3 AI obstruction tests: 24/24, recovering around 12 seconds and resuming measurable route travel without repeated recovery loops.

## Activities and controls

Select **Free Roam / explore + arcade activities** from the main menu after selecting a track/vehicle. Forest remains motorcycle/ATV only. Escape/Start opens the pause menu; cycle Activity, drive to its compass/distance location, then Start/retry. R/Y performs local recovery. Return to menu provides race selection.

Free roam has ambient traffic and existing world/audio systems, no opponents/countdown, race gates/laps/finish, checkpoint penalties or wrong-way warnings. Race HUD/markers are hidden. Activity storage is independent of race/lap storage. Returning to a race restores countdown, opponents and gate markers.

Jump feedback measures horizontal takeoff-to-landing displacement and air time; points are `round(10 × metres + 100 × seconds)`. Challenge medals use distance, with vehicle-specific targets shown in the pause menu. Takeoff requires preceding grounded stability, forward movement through the activity launch area, and at least 0.25 seconds airborne. A successful landing requires 0.3 seconds supported/upright, no wipeout, water immersion, hard descent or invalid solid contact. Reset, discontinuous teleport, spawn drops and jitter cannot produce a successful award. Starting/retrying midair requires a new grounded launch.

Smash uses only the authored existing fence sections, distinct object identities per attempt, with 3/6/10 targets in 8 seconds. People, animals, debris and unrelated props are excluded. Retry safely restores props and clears the attempt set; reset cancels and pause freezes the timer.

Two speed traps per variant accept either crossing direction, require real grounded movement and aligned velocity, and rearm only after leaving the plane by 25 metres. Feedback displays km/h; stored speed is m/s. Best values and medals persist separately by activity/course/rules/vehicle. Points and medals do not affect power, gate exemptions, penalties or classification.

Candidate3 jump calibration used actual launches: reverse Street ranges 15–54m depending on speed/profile; the initial Forest forward motorcycle measurement was inflated by bounce travel and is superseded below; reverse Forest reached 43.76/44.33m. Final thresholds reflect those differences. Initial straight-line speed pilots and misplaced airborne Forest cameras failed and are retained; cameras were moved to grounded stretches and subsequent pilots follow the actual road.

## Evidence boundaries

`candidate*` directories retain successful and failed diagnostic runs. RULE-labelled smash tests directly exercise event accounting and persistence and are separate from physical contacts. Pause fixtures were corrected to sample the same Unity frame clock. No failed attempt is silently counted as passing. Isolated `-racerTestSave` folders protect personal saves.

Human route discovery/readability, driving enjoyment, perceived medal difficulty, physical-controller operation and speaker-level listening remain for Dan. Performance measurements use explicit offscreen 1280×720 rendering on this machine; they are not universal hardware guarantees. The Unity build retains existing optional Pipeline-configuration and future collision-prebaking warnings.

## Integrated activity and preservation results

`final/` uses review5's first-touchdown scoring and full visual-envelope clearance. Later recovery-only changes are rechecked separately. All test saves are isolated.

| Automated check | Actual result |
| --- | --- |
| Hazard recovery, all eligible profiles and four variants | 282/282 |
| Local free-roam recovery, both travel directions | 80/80 |
| Free-roam/race transitions, pause/retry and exploit rules | 96/96 |
| Cold-process activity persistence | 12/12 |
| Physical midair reset at both Street jumps | 8/8 rejected as awards |
| Physical smash and deliberate retry | 24/24; every eligible profile reaches ten distinct props in 4.2–4.7 seconds against the final eight-second timer |
| Physical Street jump scoring | 24/24 successful awards, 18/28/38 m/s approaches |
| Physical Forest jump scoring | 12/12 successful awards, both eligible vehicles and directions |
| Top-ten records and save behavior | 29/29 |
| Measured versus estimated results | 96/96 |
| Vehicle art/eligibility checks | 116/116 |

`candidate4/` additionally contains 144/144 physical speed-trap assertions (both traps, directions, eligible vehicles, repeat/rearm and units), 24/24 physical wrong-way assertions, and 280/280 navigation/shortcut rule assertions. `final-guards/` retains the earlier independent lifecycle passes and menu captures. Earlier failures remain in their original directories.

Corrected Forest forward jump calibration is 25.71/41.93/60.66m for motorcycle and 22.46/30.00/38.62m for ATV at the three tested paces. Final bronze/silver/gold targets are motorcycle 24/40/58m and ATV 15/28/36m. Reverse Forest measured 22.61/36.36/43.76m and 20.91/33.54/44.33m; targets are 18/30/40m. Street forward targets are 20/45/75m; reverse uses measured vehicle-specific thresholds in the inventories. These establish achievable targets through automated pilots, not subjective human difficulty acceptance.

## Ramp matrix and retained limitations

`final-matrix-*` contains 240 ordinary-frame approaches at the actual authored Trickum ramps: all eligible profiles, 12/24/36 m/s, five lines, plus 43 m/s edge/interior regression. This includes the street ramp in each Forest free-roam environment. All 144 interior-line trials traversed the ramp; one tourer 36 m/s trial initially lacked enough settling observations, then passed the same physical case with a two-second observation interval (`final/tourer-36-centred-settle`). All 240 local recovery checks passed.

Four extreme edge trials stalled/crashed: Street motorcycle at 43 m/s on both edges, Forest motorcycle at 36 m/s on the right edge, and Forest Reverse motorcycle at 43 m/s on the right edge. Other edge traversals can include substantial banking or a rollover; a `completed` traversal is not a claim of a clean landing. The scoring system rejects invalid landings. Per-frame CSVs retain actual contact normals, support, speed, torque, minimum upright and outcome. `ramp-retained-incidents.json` indexes the four crashes and settling follow-up.

Unmodified Forest trail-ramp regression passed 21/24 assertions forward and 20/20 reverse (`candidate4/jumps-*`). The three forward failures are retained: the motorcycle's imperfect first-jump line wiped out and recovered once; two third-jump assertions failed the global minimum-upright threshold during flight although the pilot subsequently landed without recovery. These are limitations of the current trail/pilot envelope, not silently reclassified passes. Human off-centre handling review remains open.

## Race evidence and recovery follow-up

`final-races/` retains review5's normal-window and diagnostic extended-window runs. All four variants and all four racers completed three laps with the diagnostic 300-second finish grace. The normal 90-second grace exposed repeated Street recovery and a slow Street Reverse player DNF. The production grace was not extended to conceal these problems. Street's extended run includes eight legitimate outside-span gate misses for the ATV on lap two; its penalty ledger records CP04–CP11 at five seconds each. Legal branch entitlements remain separately validated with no unrelated gate exemptions.

The follow-up adds persistent partial-obstruction tests, an alternate recovery departure lane and continuous local route tracking independent of supported-position history. This prevents a long shoulder excursion from freezing the safe anchor far behind. A verified branch exit seeds its earned road location; abandonment cannot grant that anchor. Final follow-up and serial performance results are recorded after completion below.


The 32 `*-road-role-*.png` captures show both road-height approaches to the mounted Trickum Road and TO Jamerson Road faces in every variant. Representative images were visually inspected after capture; lettering is physical and unobstructed in those views. These supplement the shortcut approach captures and inventories, not a claim that Dan has driven them.

Review7's persistent-obstruction fixture retains one strict timing failure: the Street tourer escaped after two recoveries but reached only 26.55m beyond the obstruction at the 35-second cutoff, travelling at 23.32m/s. The fixture demanded 60m. The per-frame trace demonstrates resumed forward travel; the failed assertion is preserved rather than counted as an unconditional pass.

## Final review7 follow-up

- Hazard recovery: **282/282**; roaming recovery: **80/80**; lifecycle/activity rules: **96/96** (`verified7/`).
- AI obstruction checks: **35/36**, with the single strict deadline failure explained above. Every vehicle resumed physical travel; no indefinite stall is reported as passing.
- Current-build Forest street-ramp retests: **80 cases**, all local resets succeeded; all interior lines completed. One extreme motorcycle edge crash in each Forest environment remains, with traces (`matrix7-*`). Street ramp meshes and vehicle motor hashes match the earlier 160-case Street matrix. The Forest meshes were rechecked on the final authored build rather than relying on an unmatched serialized-asset hash.
- Normal three-lap races: Forest and Forest Reverse finished for all four racers. Street finished for the player and two opponents; the ATV was still moving at about 33m/s near the final lap's end when the ordinary finish window classified it DNF. Street Reverse finished for three opponents; the player was still moving at about 33m/s near the line at the cutoff. The motorcycle's one outside-span gate miss received its ordinary five-second penalty.
- The additional final-build Street Reverse diagnostic run completed three laps for all racers, without missed gates (`reverse-finish7/`). Review6 also completed all four normal-window races for all racers; differing traffic/AI variation can change the result. Production finish grace remains 90 seconds, and measured versus estimated classification is preserved.

These are automated physical runs. A completed race or recovery does not establish human enjoyment, perfect AI lines, or universal completion inside the finish window. No historical record is recategorized as comparable to the changed course versions.

Final navigation follow-up passed **24/24 physical warning/reset/pause assertions** and **280/280 route/shortcut rule assertions** on review7. Visible wrong-way onset was approximately 5.03–5.10 seconds of meaningful wrong-direction travel. Emulated device input is distinguished from physical-controller review.

## Serial performance measurements

Review7, NVIDIA GeForce GTX 1660 Ti, 1280×720 explicitly rendered offscreen, 120 FPS cap, normal time scale, 150 seconds per run. Only one Racer process ran at a time; builds and packaging were idle. Existing traffic, household population, wildlife and actual radio playback remained enabled. Counts reflect each run's normal variation. Figures are frame times, not a universal hardware guarantee.

| Race variant | Median ms | p95 ms | People | Traffic | Selected wildlife |
| --- | ---: | ---: | ---: | ---: | ---: |
| Street | 8.339 | 16.328 | 19 | 20 | 8 |
| Forest | 8.334 | 10.167 | 21 | 4 | 8 |
| Street Reverse | 8.580 | 17.906 | 18 | 20 | 8 |
| Forest Reverse | 8.334 | 10.220 | 18 | 4 | 7 |

All four racing runs passed their radio/ordinary wildlife assertions and exited normally. Raw timing, wildlife occupancy and process-memory evidence is in `performance7/`. Free-roam measurements follow below.

| Free-roam variant | Median ms | p95 ms | People | Traffic | Selected wildlife |
| --- | ---: | ---: | ---: | ---: | ---: |
| Street | 8.334 | 12.805 | 16 | 20 | 8 |
| Forest | 8.334 | 8.566 | 23 | 4 | 5 |
| Street Reverse | 8.334 | 14.786 | 22 | 20 | 6 |
| Forest Reverse | 8.334 | 9.406 | 16 | 4 | 8 |

All four roaming runs also passed their radio/ordinary wildlife assertions and exited normally. All eight runs had radio playing and normal time scale throughout their measured drive. World-only performance captures omit overlay UI; lifecycle/HUD assertions and separate menu screenshots cover that presentation. These measurements do not replace Dan's controller or listening review.
Across the eight serial runs, peak process working set ranged from 563.66 to 1007.52 MiB (see performance-summary.json).

## Windows package and provenance

Version **0.13.0-review7**, Windows x64, Unity 6000.6.1f1. The build succeeded with zero errors and two existing warning categories: optional Pipeline runtime configuration and future Unity collision-prebaking requirements. Saved source verification matched all **2,690** entries in `SOURCE-SHA256.txt`. The editor-only road-photo helper was added after compilation; runtime sources and authored scenes are unchanged.

The existing `Tools/Package-Racer.ps1` workflow verified **436 files** by SHA256 across the extracted ZIP, versioned runtime and complete `Builds/Latest`, including **187 deliberately staged songs**. It preserved the previous Latest and ZIP/runtime copies, and did not read or copy external music collections. `Play-Racer.cmd` is unchanged from the safety checkpoint. No external upload occurred. `package-precommit.json` records this pre-commit verification; final archive hash and completion stamp are maintained in `Builds/PACKAGE-LATEST.json` and the packaged `VERSION.txt` after the evidence commit.

Build: `Builds/Racer-0.13.0-review7-Windows/Racer.exe`; archive: `Builds/Racer-0.13.0-review7-Windows.zip`; normal entry point: `Play-Racer.cmd` / `Builds/Latest/Racer.exe`.
Installed Builds/Latest smoke: all four variants passed 24/24 lifecycle/activity assertions (96/96 total), with isolated saves and clean process exits; evidence in packaged-latest/. This tests the packaged executable and data at the normal launch location.
