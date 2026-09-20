# CR-097–100 / 0.17.0-review1 validation

Reviewed baseline: **0.16.0-review3**. Implementation is delivered for Dan's review, not blanket acceptance. Map/exploration and unreviewed systems remain awaiting further testing.

The final Windows build succeeded in Unity 6000.6.1f1 (0 errors, 3 collision pre-baking advisories). Runtime assembly SHA256: `3118C2702EC5EF37BD83C4DBA8279B9F04192237F593DB16941BD6B7EA36F8B3`. Safety checkpoint: `acafa41ce76c410e4b970dbbd7c9cf14e487fc8e`. Completion/package identity is recorded in VERSION.txt and package verification metadata.

## Scope and evidence

Dan narrowed this pass to changed properties/terrain, summit, title/audio, affected nearby travel/collectibles and one brief gameplay check. Pending broad regression runners were stopped; their partial output is retained and is not a completed suite. No further unrelated map, collectible, ghost, leaderboard or four-course regression suite is required for this delivery. Earlier completed extra checks are historical evidence, not broader acceptance.

Tests drive Unity rigidbodies using automated input/steering in rendered Windows players. Ordinary-frame tests use timeScale=1; the large summit matrix uses timeScale=4 with the same physics fixed step. All ordinary tests use isolated temporary saves with Master=0. Only the targeted 2.2-second audio capture temporarily unmutes its isolated player, then remutes. No user's saved settings were changed; no diagnostic mute is packaged. No physical controller, Steam Deck hardware, human driving or subjective listening test is claimed.

## Properties and nearby content

`delivery/house3-*` records ordinary-frame in/out traversal by every eligible vehicle in all four saved variants, safe reset, no fence breakage, and structural checks. Final terrain/road collider recooking and the nine-column road-conforming apron repair supersede candidate failures. All 24 eligible vehicle/direction/variant traversals and their resets passed at normal frame rate. The changed-property/rules/fence-activity checks total 216 explicit passes, zero failures, across 12 completed processes.

Every scene audits 336 retained/repaired fence endpoints with maximum measured gap 0. Dan/House 2 fronts are parallel; Kyle's named fences are absent; neighbors across the street remain. The dome, both campers and 22 stone clue cairns are retained. Final ordinary gameplay views are in `delivery/views`, including the Rocky Way entrance and frontage. All 17 scene/resource files match between the physical driveway test build and final portable player (`delivery-portable-content-match.json`); the final assembly adds only the test harness's launch-from-rest option.

Regeneration initially orphaned the fence-smash activity's prop references; a first rebind selected a steep run and failed physical contact tests. The final generator binds a clear, almost level 14-panel run while preserving the activity ID, target, time and records. `delivery/smash-*` verifies actual contacts and retry/PB behavior. Those tests are justified by the fence change; earlier failures remain in `final/smash-*`.

Affected neighborhood, ridge and summit east-approach travel destinations were checked for supported, clear arrivals for eligible vehicles; travel during a race is rejected. Nearby woodland-01–04 approaches and resets passed. Existing fog/save identity and collectible IDs are preserved; discovery-gated map identity and open driveway/return lines were regenerated. Existing completed evidence is in `release/map-*`, `release/collections-*` and `portable-runtime/map-*`; no additional broad suite was run after the scope correction. One completed two-lap Street Loop player check (`release/ghost-race-StreetLoopGreybox-original`) supplies the brief general gameplay check: four eligible vehicle results completed, with automated recoveries disclosed. It does not establish clean-ghost acceptance.

## Summit physical results and remaining limitation

Route: **lake gateway → existing mountain ascent → camp/ridge junction → east approach signs and teal flags → westbound Summit Homeward Flight**. Brake on the supported western rollout; use the right/northern ridge return. The map landmark appears after discovery. Driving uphill into the launch face is the wrong approach. No boost, handling change or relaxed scoring threshold was added.

Before edits, the narrower baseline ramp-centered fixture measured about 3 seconds airborne; it did not reproduce Dan's entire ordinary discovery path. The later baseline ordinary-trail run cannot retroactively fill that gap. The accidentally early `CR097-first-geometry` build preceded its queued scene mutation and is not corrected-geometry evidence. `first-playable` is the actual first corrected geometry, tested at ordinary frame rate with launch/crest/landing positions, speeds, contact/suspension traces and gameplay captures. All raw failures remain.

After edits, the compiled summit matrix (`final/summit-*`, `final/wrong-face-*`) includes 376 cases: 360 forward speed/line/mode/eligible-vehicle combinations, 12 wrong-face cases and four ordinary-frame 1280×800 center repetitions. Target approach speeds are 24/32/40 m/s; lines include center, ±6 m and actual vehicle-adjusted side edges. All cases completed and reset; minimum-up checks passed. Forward uninterrupted flight is **4.00–5.74 seconds**, measured underside clearance at the defined crest reference **31.08–38.94 m**, launch speeds **22.22–37.97 m/s**. The reference is a fixed cross-section, not a claim of minimum clearance over every point of the mountain. Northern return traversals passed. Wrong-face approaches remain rough (minimum up about 0.67), but completed/recovered; they are not the intended route.

The final assembly was also tested from a stopped runway start, without seeded initial velocity, in roam and race at normal frame rate (`portable-runtime/summit-from-rest-*`). All eight runs completed/reset. Results by vehicle below are roam values; race repeats matched the physical range.

| Vehicle | Launch speed m/s | Continuous flight s | Crest underside clearance m | First landing speed m/s |
|---|---:|---:|---:|---:|
| Original | 28.654 | 4.860 | 36.070 | 25.216 |
| Tourer | 28.442 | 4.740 | 34.933 | 25.909 |
| Bike | 30.462 | 4.980 | 36.307 | 21.457 |
| ATV | 30.280 | 4.980 | 36.955 | 21.285 |

Example original: launch (968.694,180.261,190.000), crest reference (944.352,193.378,190.000), first landing (863.762,152.006,189.266). Full per-case positions, traces and ordinary views are beside each flight.csv.

**Retained failure: every tested forward giant flight receives zero jump score because the existing activity rejects the hard landing.** The crest clearance, airtime, eventual supported rollout and recovery are verified; a clean scored landing is not. Do not describe these results as an all-pass giant-jump activity. Dan's route readability and landing-feel review remain necessary.

## Title and audio

Final assembly `portable-runtime` title checks passed (79/79 explicit assertions): Radio On with keyboard at 1280×720; Radio Off/Master mute with input held from launch and virtual gamepad at 1280×800; early mouse skip during an eight-second audio load delay; targeted audio check; menu focus/consumed press; pause, race/menu return and track reload. Three complete theme loops remain on title with one voice start, no timed advance and no radio. On starts radio only after menu entry; Off stays silent; saved station and volume remain unchanged. Exactly one enabled listener, no stale title source after skip, and no repeated title/audio on scene returns.

The entire exact approved PNG is aspect-fitted with matte and separate prompt. Both 720p and 800p captures were visually inspected. Source SHA256 is recorded in `title-source-hashes.json`; both original MP3s remain in Assets/TitleSource. Runtime loads only embedded Resources/Title assets and never Downloads or the source MP3 paths. Title music is separate from radio channels.

Voice PCM is 1.750208 s (25.08 ms leading and 6.90 ms trailing near-silence). Theme PCM is 31.36 s, derived from the supplied 31.6 s source using a 240 ms circular crossfade. No leading/trailing digital silence; boundary step 0.01373 versus maximum internal step 0.28854. Three-loop playback was checked in multiple cold processes. The final targeted DSP capture measured 212,992 samples, peak 0.311104, RMS 0.015724, zero clipped samples. Numerical continuity/DSP checks do not establish subjective musical phrase or listening acceptance.

A full portable candidate ZIP was extracted and launched at both 1280×720 and 1280×800 (`extracted/title-*`): 44/44 explicit assertions passed, including three loops, held virtual input and radio On/Off. The actual delivery ZIP is additionally hash-verified in full by the established packaging workflow and receives a final extracted startup check. The runtime does not need the Downloads assets to exist.

## Retained history and limitations

`INTERMEDIATE-FAILURES.md` retains candidate driveway collisions/crossing, floating fence endpoints, apron problems and earlier title-fixture mistakes. The corrected final driveway supersedes them. The accelerated Street Reverse collector fixture failed 16 approaches, while an ordinary-frame repeat and the reviewed baseline each reached all 24; preserved raw failures are not silently converted to passes. Extra broad ghost/race experiments also retain reverse-course AI/automated-race completion failures; they are not evidence that all races/ghosts passed. No unrelated repair was pursued after Dan narrowed scope.

CR-087 ramp/edge failures, Creek Leap/Granite Saddle shortcut failures, AI finish-grace/recovery timing issues, neighborhood performance spikes and other prior documented limits remain open. Map/exploration remains awaiting further testing. No mountain race, split screen or online multiplayer was added.
