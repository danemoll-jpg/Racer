# CR-022–CR-025 validation log

Safety checkpoint: 30b82438708dc62754ff304528cf484069b94593. The first staging attempt failed with index.lock permission denied; elevated staging/commit succeeded before project edits.

Initial scene inspection: StreetLoopGreybox; saved/clean scene; 44 m/s top-speed parameter, 13.5 m/s² acceleration, 24 m/s² braking. Trial revised values: 49 / 14.5 / 24. Steering, grip, suspension, mass and camera unchanged.

Recovered failures:
- Initial play run did not start: an empty route-distance cache restored by Unity caused IndexOutOfRangeException. Initialization now checks cache length and positive course length. No driving result inferred from that run.
- First moving AI/traffic attempt stopped in a queue at the jump bypass, around CP14. Both directions selected the same narrow lane. Source positions and speeds are preserved in initial-stall.txt. Revised drivers separate the bypass lane and supported shoulder, with cautious passing outside the narrow segment.
- The scene setup CLI response timed out after 5s although the operation succeeded. A subsequent live inspection verified 49 m/s, 2331 road points and a saved scene; the mutation was not blindly repeated.

Evidence so far:
- rules.txt: 17 isolated progress/save assertions pass.
- route-rules.txt: normal, approved shortcut and jump route replay complete penalty-free; ordinary one/two missed gates cost 5/10s and finish; repeated/wrong-way finish crossings award no laps; teleport does not create a penalty chain.
- Large-cut replay: 19 misses, 69.92s synthetic driving, 235.10s penalties, 305.02s adjusted. This is a route-rule replay, not a physically driven cut.
- classification.txt: physical finisher at 100s +20s ranks behind a 105s clean finisher; stuck racer becomes DNF under the bound.
- pause.txt: all eight vehicle poses and the race clock remain unchanged across a pause; listener pauses.
- First revised ordinary-frame one-lap race: all three AI finish, zero recoveries/misses; player 161.416s; opponents 165.091,166.911,176.898s. Player peak 41.33m/s. Editor median/p95 16.72/23.48ms. Ordinary physics autopilot, not physical controller input.

The AI/traffic population uses zero additional audio sources; the scene remains at nine sources. Driver acceleration/top-speed are copied from the player motor, with lower desired pace and no rubber-banding.

No friend-PC or physical-controller validation is claimed. Final Windows and package evidence follows below.

Final Editor follow-up:
- Second AI/traffic run: all three AI finish, zero recoveries/misses; median/p95 16.70/22.39ms. Solo/no-traffic: 161.318s, zero misses; median/p95 16.67/21.42ms. Conditions: i7-9700, GTX 1660 Ti, 734x293 Game view, VSync1/cap60. The two AI runs have p95 about 1.0–2.1ms above baseline; this is capped Editor timing, not a pure CPU benchmark.
- Final cut accounting credits only new forward road distance within 18m of the road reference, bounded by physical sample movement; circling/reversing/off-road odometer cannot reduce the cut charge. Replayed giant cut: 482.36s penalty, 552.29s adjusted. Ordinary misses remain 5/10s. Earlier 235.10s result is superseded by this stronger accounting.
- Recovery assertion: remote pad stays >40m from player, no gate/penalty gain, near-player recovery refused. Grid minimum separation 8.78m. Recovery returns 8m behind last stable sample and suppresses progress until returning to that sample. Traffic recovery also requires both positions >200m away and outside the camera view.
- Cue assertions: one authoritative ding; repeat suppressed; one player miss buzz; AI miss does not affect player counters/penalty; cooldown prevents overlapping buzzes; countdown/pause suppress cues; master mute applies. Reset retains race penalties; restart clears progress, penalties and cue counters.
- Virtual-Gamepad handling: revised straight test reaches 45.60m/s; brake starts at45.27m/s and holding for3s passes through stop into -5.52m/s reverse. Min upright0.999; max3D road distance7.44m. A40m/s initial-speed corner-entry fixture brakes to12.69m/s, max road distance3.99m, min upright0.986. These include deliberate initial placement and (for that corner fixture) injected initial speed.
- Hills:28.97m/s, min upright0.963, max3D road distance10.13m; hairpin13.02m/s, min upright0.973. The pursuit driver made shoulder excursions; no claim of perfect lane holding or subjective approval.
- Jumps:32.76 and39.52m/s peaks;1.80/2.06s airborne; both end with4 grounded wheels, min upright0.980/0.979.
- Physical mailbox fixture breaks one prop; moving debris returns to0; restart restores the prop and clears progress/penalties.
- Failed late-braking attempt retained in handling-initial.txt: peak45.78m/s,49m road departure at end bend. The test held full throttle past the straight. It was rerun with earlier braking; vehicle steering/braking parameters were not changed to conceal the failure.
- One rerun waited at Ready because the unfocused Editor had runInBackground=false; explicitly enabling ordinary background frames resumed the harness. No simulation result inferred during the wait.

The repeated record-isolation test initially reused its own saved new-category record and failed an empty-category assertion. The fixture now uses a new GUID directory per run; all 17 checks pass. This was test isolation, not loss or mixing of records.

## Windows release and package

- Build 0.3.0-review1, Windows x64, Unity 6000.6.1f1, source de84d067cb14773d08309cf697037aca30522fac. Non-development, no debugger/profiler connection or runtime Pipeline. Build succeeded with zero errors and one expected missing RuntimePipelineConfig warning. Player logs contain stripped unused depth-of-field/Panini shader warnings, with no runtime exception in the race run.
- Visible 1280x720 standalone three-lap race, physics autopilot, four racers/four traffic: YOU 481.793s, EMBER 486.893s, BLUE 489.203s, GOLD 510.308s. All finished three laps; zero misses and zero recoveries for all racers. Peak player speed 41.33m/s. 15,307 sampled frames, median33.33ms / p9533.37ms. This was a capped/background-capable visible window and is not an uncapped CPU benchmark. Compare AI cost using the matched Editor baseline above, not across resolutions or frame pacing. Screenshot standalone-results.png shows final adjusted standings and mode controls.
- Total ordinary-frame race evidence: two one-lap Editor races plus one three-lap Windows race, 9/9 AI finishes; one completed solo/off Editor baseline. Rule-cut/ranking/recovery fixtures are synthetic and are not mislabeled as complete human-driven races.
- Visible standalone virtual-Gamepad handling, solo/traffic off: peak45.60m/s; brake from45.24m/s through stop to-5.45m/s after3s. Corner fixture starts40m/s and ends12.69m/s, max3D road distance3.95m. Hills peak29.01m/s and road distance10.12m; hairpin13.12m/s. Jump peaks32.76/39.52m/s, airborne1.80/2.06s, both end four wheels grounded and minimum upright0.980. Mailbox breaks with one moving debris body, settles to zero, then restart restores it and clears penalties/progress. See standalone-handling.txt. No runtime exceptions observed in this run.
- ZIP: Builds/Racer-0.3.0-review1-Windows.zip, 76,322,288 bytes, SHA256 EAE1BE01EA588EBBABD5039E79C3DE539AEFED282C9F49A18EDBB496FF11A429. Fresh extraction and complete Builds/Latest both match all 228 manifest file hashes. Old Latest retained at Builds/Latest-before-0.3.0-review1. Runtime, notices, README and exact-source VERSION.txt included; test evidence/debug artifacts excluded. No upload/distribution.
- Remaining review limits: no physical gamepad or subjective sound evaluation; no friend PC/download test; no sustained adversarial multi-car pileup coverage. AI can make shoulder excursions, and congestion/recovery behavior remains a review area. Forward-speed parameter49m/s is not a claim that49m/s was reached. Final speed, handling and penalty feel await Dan.
