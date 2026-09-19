# CR-070–074 integrated Windows review

Safety checkpoint: `90abc69769528600ad652df1c3dbad985ae482d8` (verified before edits).
Release: **0.12.0-review1**, implemented and awaiting Dan's review. Failed and superseded candidate evidence is retained and distinguished below.

## Scope and preserved systems

- Removed exactly `Breakable sign - South Cherokee Lane Jamerson Rd` at **(457.62, 25.14, -535.22)** in both source scenes: two lettering faces and its exclusive board/post/collider/breakable component. The matching generator and two catalog entries per course were removed. Matching normalizes Road/Rd and line breaks and is restricted to the original location. Other physical sign lettering remains. Repeated generation/restoration records are in `sign-restoration-restarts.txt`.
- Forward scene block audit removes only that 22-block sign hierarchy; the only changed transform is its parent's child list. Other changes are new component defaults and wildlife habitat/audio references. Houses, corrected hairpin, driveway, woods, lake and forward cave transforms/meshes remain. `forward-scene-audit.json` records the comparison with the checkpoint.
- Four selectable courses: **Track → Street Loop / Forest Loop / Street Loop Reverse / Forest Loop Reverse**. Street allows all four vehicles; Forest allows motorcycle/ATV for player and AI. Civilian traffic uses the original street route and lane direction. Original branch components are removed from the reverse copies; reverse geometry is confined to those scenes and copied mesh assets.
- Record namespaces remain separate: `street-v10-hairpin`, `lake-v3-shallows`, `street-reverse-v1`, `forest-reverse-v1`, plus the existing vehicle/rules/roster/lap categories. Historical boards/saves remain. AI remaining-distance estimates use the active main/branch route and remaining laps.
- No handling/physics/paint changes, hidden boost, forced wrong-way reset, new penalties, split screen, networking, stunt scoring or ghosts. Radio collections, staged songs, household variation, water and local recovery remain.

## Wrong-way behavior

Fresh old-build testing reproduced the old straight fixture around 3.37 seconds but did not reproduce Dan's reported real-play delay. Inspection found strict sample resets and a small HUD. The replacement measures signed displacement along the active route/branch, freezes brief noise, performs bounded reacquisition after 0.25 s outside local search, and clears after 0.18 s of correct travel. Stops/airborne uncertainty lasting 0.75 s clear evidence. Body heading alone cannot trigger it. Pause freezes timing; transitions and local recovery clear it.

The high-contrast red/white banner includes a yellow local-direction arrow and the current **R / Y** reset binding. Final physical onset was **5.031 s Street, 5.041 s Forest, 5.032 s Street Reverse, 5.097 s Forest Reverse**, from actual meaningful negative movement to the active visible HUD at normal game speed. All four physical suites passed 6/6, including pause, correction, keyboard/gamepad recovery and transitions. [Rendered onset](final/physical-street/physical-wrong.png) and the corresponding `physical-trace.csv` are retained for each variant. Hardware controller input and subjective readability remain Dan's review; binding tests emulate actual input-device events.

Branch reacquisition now widens to the authorized deviation corridor after 0.25 s; if the vehicle reaches the main road while retaining shortcut entitlement, guidance can use that nearby main-road tangent without revoking credit. Coordinate-system changes rebase the derivative rather than treating a projection jump as movement. Final rules passed **280/280** across all four variants, including noisy samples, stationary/spin/airborne/backwards-facing cases, brief reversing, shortcuts, seams, reacquisition and lifecycle clearing. Both street hairpin traversal suites passed 17/17 on the unchanged route geometry.

## Occupants and wildlife

Drivers have stylized eyes/pupils/brows, nose/smile, ears and shared chestnut/charcoal/gold hair. Riders retain open-face helmet crowns, backs, cheeks and chin straps; only nape hair is exposed. Car cabin sightlines use an open steering rim and revised seat/pillar placement. Reviewed black-paint front details and helmet openings show readable faces. Front/side/chase/garage and physics/paint assertions are retained in art evidence.

Per vehicle, cars now use 23 active renderers / 9 materials / 16,804 triangles, motorcycle 17 / 8 / 17,236, ATV 23 / 8 / 18,060. Previous counts were cars 20 / 7 / 8,044, motorcycle 14 / 6 / 7,996, ATV 20 / 6 / 8,820. Shared materials and merged fixed meshes bound draw costs; this is simple geometry, not facial animation.

Deer and coyotes use project-created silhouettes and sounds, with idle/walk/flee movement, offscreen selection, pooling/culling and cooldowns. Existing birds/squirrels/frogs/bats remain. Animals have no racer-blocking colliders. Earlier ordinary runs observed both new species without forced placement. Actual Unity listener waveform captures include engine/radio; they are before the OS endpoint and do not claim human listening acceptance.

## Reverse route design

Street retains its main road and receives a reverse roadworks transition plus a flush approach over the west pavement seam. Forest receives five scoped reverse jump profiles with supported approaches/landings; the linked descent has a wider reverse landing. All have a viable main route; no boost substitutes for geometry.

- **Laurel Switchbacks:** narrow rising woodland S-turns, bypass CP12/13, technical street rejoin.
- **Granite Creek Cut:** tight entrance between preserved commercial properties, diagonal gully gap and tangent-matched street rejoin; bypass CP1/2/3/4.
- **Fern Gully:** descending woodland route through a new transverse grotto opening in the reverse copy of the cave, bypass CP2/3. Roof and original longitudinal passage remain.
- **Granite Saddle:** controlled downhill entry, narrow gully jump and ridge rejoin, bypass CP1.

Maps identify every branch, bypass set, entrance and rejoin: `map-StreetLoopReverse.png`, `map-ForestLoopReverse.png`. Mandatory gates are kept away from mouths/rejoins. Entry earns explicit bypass entitlement; deviation/local recovery cannot revoke it. Unrelated genuine misses retain the existing one-time five-second charge.

## Evidence and limits

`INVESTIGATION.md` retains failed and superseded candidates and the reasons for each correction. Repeated comparisons use real motor/brake/steering simulation and race sampling. Scripted clean driving, three-second braking/steering errors and actual local recovery are distinct trials. They establish measured behavior, not human fun or universal difficulty. A moderate mistake may retain part of a large shortcut gain; severe line/recovery losses can erase it. Dan's assessment of challenge, readability, physical controller feel and speaker-level audio remains open.

## Final test results and provenance

- Final candidate 8: wrong-way rules **88/88 Street, 56/56 Forest, 72/72 Street Reverse, 64/64 Forest Reverse**; physical onset **6/6 each**; repeated Forest shortcut trials **56/56**. A ten-second lack-of-forward-progress guard recovers an oscillating reverse-branch AI using the existing local recovery; ordinary player vehicles do not use that AI driver.
- Fresh-save two-lap races: **6/6 each course**, all 16 racers finished with **zero missed gates**. Recovery counts by roster slot: Street 0/0/0/1; Forest 0/0/1/0; Street Reverse 2/0/1/0; Forest Reverse 1/1/0/0. These are physical scripted player/AI races, not flawless human laps. Full times are in [final-races](final-races/standalone-race-street.json) and each course's checks.
- Candidate 7 tests unchanged by the final guidance/stall refinements: Street shortcut matrix **112/112**, reverse main jumps **8/8 Street / 20/20 Forest**, layout/support **21/21 each**, art **116/116**, wildlife **46/46**, mixed audio **17/17**, historical records **29/29**, reverse remaining-distance/finish estimates **96/96 each**. Every eligible vehicle is included in the shortcut and jump matrices, with reduced-speed/lateral approaches and actual local recovery.
- [Repeated timing table](final/timing-comparison.md): two repeats for normal, clean, imperfect and recovery trials per eligible vehicle and branch. Street uses candidate 7's identical branch geometry/speeds; Forest uses candidate 8 after the AI oscillation guard. All clean shortcut trials finished without recovery or missed gates. Clean gains: **Laurel 7.38–10.97 s; Granite Creek 1.52–3.94 s; Fern 5.77–5.79 s; Granite Saddle 1.21–1.32 s**. Moderate mistakes still retain part of Laurel/Fern's larger gain; the tested gap-route mistakes erase the gain. Local reset can align the scripted pilot better than its nominal clean line, so these are not optimal human times.
- Intermediate final-race assertions failed because the runner reused an existing test save and incorrectly expected exactly one record. It correctly retained both race entries. `reused-test-save-preservation.json` documents this; the runner now creates a unique test-save path, and all four fresh-save suites pass. No user saves were deleted or migrated.
- Actual mixed listener audio with normal engine/radio: deer snort onset correlation **0.894**, matched signal/residual **+6.02 dB** over 40–450 ms; whole central deer clip including quieter rustle **0.678 / −0.71 dB**. Coyote **0.979 / +13.73 dB**. Peaks around 0.30, no clipping. Analysis JSON is in `candidate7/audio`; local raw captures contain staged music and are intentionally excluded from Git. Project-created species WAVs and licensing notices are included.

The final presentation pass corrects only newly authored reverse sign facing and fits their lettering to the boards; route geometry, colliders, vehicle capabilities and timing remain unchanged. Its four `entrance-*.png` approach renders and `marker-facing.txt` identify the correction. The final Windows build succeeded with **0 errors / 2 warnings in 59.03 s**: optional Pipeline runtime configuration is absent, and Unity warns that 180 existing meshes rely on automatic collision pre-baking that a future Unity version will discontinue. Collision is present and tested in this build. `SOURCE-SHA256.txt` fingerprints all 2,669 Assets/Packages/ProjectSettings files. See `INVESTIGATION.md` for the failure and correction.

Final rebuilt-player smoke checks pass **72/72 Street Reverse / 64/64 Forest Reverse**, plus **21/21 each** layout/support suite, including course selection, sign exclusion, direction, route eligibility and bypass rules. Evidence: `release-smoke`. This is the player used for packaging.

## Combined ordinary-run performance

Serial 150-second ordinary motor runs, normal time scale, 1280×720 explicitly rendered offscreen, 120 Hz cap; people, street-lane traffic, engine, radio and wildlife active. Median was 8.334 ms in every course. These are local-machine measurements, not a universal frame-rate guarantee or a foreground display/input latency measurement. One brief Editor approach render occurred during the Forest run; no build, authoring pass or second game ran alongside these tests.

| Course | p95 frame ms | Peak process MiB | People / wildlife / street traffic | Occupied / empty seconds | Sightings / calls |
|---|---:|---:|---|---|---|
| Street | 13.162 | 555.88 | 18 / 8 / 20 | 11 / 139 | 6 / 6 |
| Forest | 8.929 | 770.53 | 22 / 8 / 4 | 13 / 137 | 7 / 7 |
| Street Reverse | 12.423 | 605.31 | 16 / 7 / 20 | 6 / 144 | 4 / 4 |
| Forest Reverse | 8.642 | 772.98 | 18 / 7 / 4 | 10 / 140 | 6 / 3 |

All four ordinary suites passed 4/4. Forest's four civilian vehicles remain on the actual neighborhood streets, not forest trails. [Ordinary coyote](performance-final/performance-street/ordinary-Wildlife%20Coyote.png) and [ordinary deer](performance-final/performance-forest/ordinary-Wildlife%20Deer.png) were selected offscreen and encountered naturally; diagnostic close views/animations are in `candidate7/wildlife`. The wide ordinary deer image is a distant sighting, not a close-up silhouette acceptance claim.

## Delivery and review checklist

The existing preservation-first packaging workflow verified **436 files and 187 staged songs**, byte-for-byte SHA256 agreement between the versioned runtime, complete Latest and extracted ZIP. The previous Latest is preserved under `Builds/Preserved/20260919-194311-564-423952/Latest`. `Play-Racer.cmd` matches the safety-checkpoint Git blob and original SHA256. All 2,669 source-manifest entries match the saved source tree (`source-and-launcher-verification.json`). No external collection was scanned or copied, and no upload was made.

Runtime: `Builds/Racer-0.12.0-review1-Windows`; ZIP: `Builds/Racer-0.12.0-review1-Windows.zip`; normal launcher: `Play-Racer.cmd` → complete `Builds/Latest`. `package-precommit.json` records the verified package before the completion ID is available. After the completion commit, VERSION.txt is stamped and the same packaging/verification workflow runs again; `Builds/PACKAGE-LATEST.json` is authoritative for the final ZIP hash and preservation location. Both commit IDs are in the final packaged VERSION.txt and delivery response.

- [x] CR-070 exact sign removal and preservation audit.
- [x] CR-071 actual five-second visible guidance with local direction/device prompt.
- [x] CR-072 stylized occupants and cabin/helmet views.
- [x] CR-073 selectable reverse courses, scoped transitions, four distinct branches, maps and repeated physical comparisons.
- [x] CR-074 occasional deer/coyotes, ordinary sightings and mixed audio/performance evidence.
- [ ] Dan's acceptance: challenge/risk-reward, controller feel, readability, faces and speaker-level ambient balance. Earlier human reports remain open.
