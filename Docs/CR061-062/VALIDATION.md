# CR-061 / CR-062 — 0.9.1-review1

Status: implemented, awaiting Dan's review. This report supersedes the CR-058 blanket-text-removal result and the earlier bat sound-dispatch evidence.

## Signs

Safety checkpoint: `0553614da30e2bbcd0fe4619ee5ba1aa302aba97`. The initial sandbox index-lock permission error was recovered through the supported elevated retry before edits.

Recovered original text, local transforms, font settings and material references from pre-regression commit `8fee1f5475e6c2b9ee0a9eb646cd2e61e40c4664`. Historical catalogs are in `Assets/Track/Signs`; inventories are `history-Street.txt` and `history-Forest.txt`. Each track retains its 22 storefront faces and restores 17 deleted road/shortcut/jump faces: 39 historical faces per track. Forest additionally has the new physical warning. Names include Hwy 92, South Cherokee Lane, Jamerson Rd, Creek Leap, Fox Gully and Pine Ridge, plus speed and shortcut advice. Original wording and placement are retained.

`SceneryText.IsFloating` removes only known unmounted authoring labels. Mounted text is explicitly marked; unknown text is preserved. No blanket TextMesh or world-space Canvas disable remains. The scene-processing callback restores catalog lettering during builds/play; it does not depend on temporary historical assets. Duplicate-named panels are resolved using their original world positions.

The Forest warning reads exactly `Warning: Cave Ahead Enter at your Own Risk`, with one line break after Ahead. It sits before the cave, outside the line, on a wide physical panel. Release captures include actual vehicle/chase approach views and separate detail views. Tests drive into every text-bearing breakable prop, check the moving child lettering, debris hiding, restoration pose/visibility and race restart. Screenshots are renderer captures from the standalone player, not object-count substitutes or human driving.

## Wildlife and sound

Blue songbirds, bushy-tailed squirrels and frogs use fixed non-colliding pools. Forest has 14 habitat slots, Street 15; at most eight selected, with 40% per-slot occupancy. Selection and idle phases vary; hidden selected animals are prepared beyond the camera view at any distance; Unity frustum-culls rendering and animation updates skip distances beyond 160 m. Visible individuals survive reselection. Birds flap/fly away, squirrels forage/scurry, frogs pulse their throat/hop. Dry shore sites face away from water; all sites exclude racing lanes, cave turns and civilian streets. Existing people and street-only traffic are retained.

Seven mono PCM assets contain two call excerpts per species and a bat-call/wing-foley mix. All sources are CC0; exact links, credits and processing are in `Assets/Audio/Wildlife/LICENSE.txt`, shipped as `Licenses/Wildlife-CC0.txt`. Bat wings are disclosed leaf foley; calls are recordings, not synthetic beeps. Calls share one spatial wildlife voice with 7–14 second spacing; individual calls wait 20–45 seconds. Bats reserve the voice interval and retain the 45-second cooldown plus 12 seconds away before rearming.

The old bat clip was quiet and played at the stationary roost. The replacement has mastered recorded material, higher gain, linear distance attenuation and a source following the flight. Wildlife uses Ambience under Master; radio remains under Music. No saved user preferences are migrated or overwritten. Tests use isolated save directories.

Actual final listener capture: one enabled listener, 48 kHz, Master 0.8, Vehicle 0.75, Music 0.6, Ambience 1; radio really playing and virtual throttle applied to the engine. Stereo peaks remain below 0.54 with zero clipped samples. Matched-waveform analysis finds bird/squirrel/frog/bat calls in the captured mix, with correlations 0.705/0.821/0.776/0.915 and estimated call-to-residual levels -0.04/+3.17/+1.80/+7.10 dB respectively. This measures waveform presence, not perceptual loudness. Frog placement was corrected after an earlier capture showed excessive distance/masking. The final frog capture is 12.46 m from the listener. Master mute produces exact zero captured samples; active species volume responds to Ambience mute. Cooldown, lingering, leaving/rearming and restart are exercised.

Capture occurs at Unity's AudioListener before the OS endpoint. No claim of subjective listening, Dan's actual speakers/headphones, or OS volume validation is made. Engine playback is loaded with throttle in a stationary fixture; ordinary moving runs separately exercise the full mix. Ordinary driving exposed a culling defect: the earlier 130 m activation threshold could keep an animal hidden for its entire visible approach. The bounded pool now prepares offscreen animals regardless of distance and culls distant animation instead. `verified-*` ordinary runs validate this correction. Final `release-*` sign/audio evidence supersedes `candidate-*` and `final-*` iteration captures; the early authoring error is retained as diagnostic history.

## Preservation and delivery

`scene-preservation.json`: all 28,539 pre-existing scene transforms retain position, rotation and scale. No course, water, AI judgment, vehicle eligibility, record-category, garage/color or local-recovery implementation was changed. RaceDirector adds only wildlife reselection on restart. AmbientLife changes only bat audio; the people scenes remain intact. Play-Racer.cmd is unchanged. No split screen, networking, stunts or ghosts were implemented.

Windows non-development build: Unity 6000.6.1f1, zero errors. The final incremental build retains one optional Runtime Pipeline configuration warning in build.txt; an earlier full rebuild also warned about pre-existing mesh collision prebake settings for a future Unity version.

The existing packaging workflow preserves staged music and prior builds, checks ZIP extraction hashes, and replaces the complete versioned runtime and Builds/Latest. No external upload.

## Dan's playtest checklist

- [ ] Drive both tracks: read road names, storefronts, jump/shortcut signs; check no floating scenery/debug labels returned.
- [ ] Read the exact cave warning on approach without leaving the racing line.
- [ ] Hit a sign, watch its lettering fall with it, then restart and revisit.
- [ ] Find birds, squirrels and frogs over several runs; assess recognizable silhouettes/motion, sparse sightings and empty stretches.
- [ ] With normal engine/radio settings, listen for each species and the bat flutter/chirp; check Ambience/Master mute and saved settings after reopening.
- [ ] Linger, reverse, recover and revisit the cave; verify a brief readable swarm without repeat spam or obstruction.
- [ ] Check people, street traffic, water, garage/colors, historical top-ten records and controller performance.

Simple procedural art, no IK, and short animation loops remain limitations. Automated/offscreen results do not establish foreground display/controller feel or subjective sound acceptance. All new work remains awaiting Dan's review.

## Completed checks

- Release signs: Forest 49/49; Street 44/44. Actual chase screenshots include road names, stores, advice and the cave warning. Each historical breakable sign is exercised, not just counted.
- Release audio: 13/13, with the WAV captures and waveform analysis in release-audio-LakeWoods.
- Wildlife fixtures after culling correction: 42/42 in verified-wildlife-LakeWoods; seeds/positions recorded for each species, 30 occupancy seeds, no colliders, recovery/pause/restart checks.
- Forest preservation: 28/28 in preservation-forest (eligibility, historical boards, street-only traffic recovery/recycle and physical cave recovery).
- Existing comprehensive flow suite: 133/137 in preservation-flow. Four `Trim/rider unchanged` assertions fail for all profiles; the previous 0.9.0-review1 player reproduces the same four failures in preservation-baseline. The assertion demands an empty property block on every non-body renderer. This pass does not establish why that legacy assumption fails and does not modify vehicle paint/configuration. Persisted colors, body paint, audio settings, recovery, breakables and race flow checks pass. Do not call the entire suite passing.
- Corrected-pool preliminary ordinary runs: four distinct Forest sightings and five Street sightings from differing natural seeds, radio active. The preliminary occupied-second counter also included the invisible audio source; use the final ordinary runs below for animal-only counts. The runtime's distinct sighting count already excludes that source.

Git diff whitespace diagnostics flag Unity-generated empty YAML values in saved scenes; these are left in Unity's serialized format rather than hand-editing scene files.

Final Forest ordinary run: natural seed 109388562, seven selected animals, 15 occupied and 135 empty one-second samples, seven distinct runtime sighting events and six calls. Median 8.334 ms / p95 8.335 ms at 1280x720 with 120 fps cap; 16 people, four civilian vehicles and radio playback active. This is a 150-second scripted ordinary route-following run, without animal placement/forced occupancy. Visibility samples use the near camera frustum and are not occlusion-aware; small animals may be hidden by terrain or trees. Captures establish rendered views, not subjective recognition at racing speed. Distinct natural seeds and earlier corrected-pool runs demonstrate variation; no FPS guarantee on other hardware.

Final Street ordinary run: natural seed 109550390, eight selected animals, 12 occupied and 138 empty one-second samples, seven distinct runtime sighting events and five calls. Median 8.334 ms / p95 9.361 ms at the same 1280x720/120 fps cap; 15 people, 20 civilian vehicles, radio active. Both final ordinary runs pass 4/4. Ordinary-final-* contains the animal-only visibility diagnostics and periodic chase-camera captures. No runtime exceptions were found in these final logs.

Package verification: 429 files and all 187 staged songs match SHA256 across extracted ZIP, versioned runtime and complete Latest. Prior Latest/runtime/ZIP were preserved by the existing workflow; Play-Racer.cmd is unchanged. package-validation.json records the pre-commit consistency pass. After the completion commit, VERSION.txt is stamped with its full ID and the same workflow is rerun; Builds/PACKAGE-LATEST.json is authoritative for that final metadata-stamped ZIP hash.
