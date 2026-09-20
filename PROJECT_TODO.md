# Woodstock Rush
## Project Management / TODO / Astra-Codex Handoff

**CURRENT DELIVERY — CR-097–100 / 0.17.0-review1:** Property/driveway repairs, summit guidance/geometry, dome tent/stone cairns and dedicated approved title art/audio are implemented for Dan review. Safety checkpoint `acafa41ce76c410e4b970dbbd7c9cf14e487fc8e`. Launch `Play-Racer.cmd` or `Builds/Latest/Racer.exe`; full ZIP `Builds/Racer-0.17.0-review1-Windows.zip`. Implementation completion `14240a16f9018c8f42e1a2943ef2473de6268a62`. All 440 runtime/Latest/extracted ZIP files match SHA256; 187 playable staged songs and two original M4A files are preserved. Final extracted startup: 21 assertions passed from the portable directory. Package identity is in VERSION.txt and Docs/CR097-100/package-final-verification.json. See [actual validation and failures](Docs/CR097-100/VALIDATION.md), [implementation](Docs/CR097-100/IMPLEMENTATION.md), and [concise Dan checklist](Docs/CR097-100/README-player.txt). Summit flights clear the crest for 4.00–5.74 seconds but still receive zero score for hard landings. Map/exploration and other unreviewed systems remain awaiting further testing. No physical Deck/controller or subjective listening claim.

**CURRENT AUTHORIZATION — CR-091–096:** Implemented in 0.16.0-review3 with compiled-player validation; full runtime/ZIP delivery is recorded below. Includes finish-gate lap recovery, property/fence/sign cleanup, permanent campsite, summit jump, relocated collectibles and persistent exploration map with safe free-roam travel. Mountain race remains future backlog. See Docs/CR091-096/VALIDATION.md for actual results and retained limitations; this is not blanket acceptance.

**BASELINE AUTHORIZATION — 2026-09-20:** Dan requests one combined delivery: CR-081 plus CR-090, including automatic activity feedback/top-ten boards, personal-best ghosts, collectibles, wooded neighborhood expansion and reference-based home reconstruction. Earlier queued/deferred/excluded wording for these items is superseded. Work through internal implementation/testing checkpoints without waiting for another feature selection. Multiplayer/split screen remain excluded.

**PREVIOUS REVIEWED BASELINE — CR-091–096:** **0.16.0-review3**, implemented and awaiting Dan review. Launch `C:\Users\danmo\Racer\Play-Racer.cmd` or `Builds/Latest/Racer.exe`; full ZIP `Builds/Racer-0.16.0-review3-Windows.zip`. The complete runtime, extracted ZIP and Latest are SHA256-verified through the existing preservation workflow, with 187 playable staged songs and two original M4A files. Safety checkpoint: `a89577e9b0d3dfb4bf61652dd6ea71bae39bcdd1`. Completion commit: `87d53b7d609b77f1a5e24106845fb183a57a9d1d`. Final package verification: Docs/CR091-096/package-final-verification.json. See [implementation](Docs/CR091-096/IMPLEMENTATION.md), [actual validation and limits](Docs/CR091-096/VALIDATION.md), [controls and Dan's checklist](Docs/CR091-096/README-player.txt), and [placement inventory](Docs/CR091-096/PLACEMENTS.md). Final systems: 1,246 explicit assertions pass; 372 summit cases retain 16 non-scoring forward edge cases, including 12 instability cases. This is not blanket acceptance. No human/controller/listening validation is claimed.
**BASELINE DELIVERY — preserved combined review build:** CR-081 and CR-090 source/content are implemented together in **0.15.0-review3**. Launch `C:\Users\danmo\Racer\Play-Racer.cmd` or `C:\Users\danmo\Racer\Builds\Latest\Racer.exe`. Full Windows ZIP: `Builds/Racer-0.15.0-review3-Windows.zip`. All 437 packaged files match the extracted ZIP, versioned runtime and Latest; 187 playable songs plus two staged M4A originals are preserved. Safety checkpoint `8419e9194985955bb56a41854f1b5dce7968b270` succeeded before edits. Internal checkpoints: `38fb5906e81b26f3a5cc615be8834227af72b15a`, `f1719c6448800607b0eb7bc613245878eb2ebe3f`, and shore-boundary correction `f3d732d676ef4f7f7ae57b329855cad4f4729a4e`. Completion commit: `7e70659cc41c41d3f124fc949f601443f17ba5c3`. Final metadata/package verification is recorded in Docs/CR081-090/package-final-verification.json. Final tests: 1,500 ramp traversals with retained failures; twelve eligible player/course races completed; all twelve clean-ghost categories saved; all 24 acorns and four-course/cold persistence checked. No test timeout; no human/controller/listening acceptance claimed. See [implementation](Docs/CR081-090/IMPLEMENTATION.md), [validation and retained failures](Docs/CR081-090/VALIDATION.md), and [controls, trail guide and combined checklist](Docs/CR081-090/README-player.txt).

Activity Records (Speed Traps/Jumps) and Exploration / personal best (ghost toggle and 24-acorn progress) are available from main, pause and results menus. Automatic activity feedback works during races and free roam; clean-lap ghosts use actual timestamped poses and a separate stricter category. Historical boards/PBs and incompatible ghost files are preserved. Supplied Street View, terrain and family architecture screenshots were inspected; no private likeness or photo textures were created. All ordinary tests use temporary mute and isolated saves, preserving player audio preferences and normal delivered audio. CR-087 unresolved ramp cases and human/controller acceptance remain open. Additional retained limits: Creek Leap/Granite Saddle physical shortcut-test failures (also reproduced on the accepted build), two AI finish-grace DNFs, one AI recovery timing failure, neighborhood frame-time spikes, and incomplete race-mode mountain/Forest-jump matrix coverage. All failures and test distinctions are retained in the validation report.

**Project concept:**  
A small single-player arcade racing game inspired by *Forza Horizon*, built around one street from Dan's childhood. The street should be recognizable but is not intended to be a geographically perfect simulation. It will be modified into a closed circuit with exaggerated jumps, optional shortcuts, destructible/lightweight obstacles, and fun arcade driving.

**Primary development stack:**  
- Unity 6+
- Official Unity Plugin for Codex
- GPT-6 Astra / Codex for implementation
- Blender only when useful for custom art or geometry
- Git for version control

---

# Project Rules

- [ ] Ramp work starts with ordinary-frame driving of the existing baseline, or the first playable geometry, BEFORE implementation. Repeat after changes and in the final compiled package. Cover applicable race/free-roam directions, every eligible vehicle, intended speeds, center/off-center/actual side edges, AI approaches and stuck recovery. Record input, speed before/at/after transitions, contact normals, suspension and stability. Retain actual authored ramp IDs and every failed case; fixture success cannot close an unreproduced human report. See CR-087 and Docs/CR082-089/BASELINE.md.

These rules apply throughout the project.

**Consistent testing entry point:** Dan launches `Play-Racer.cmd` at the project root, or `Builds/Latest/Racer.exe`. After each new validated playable build, update the complete `Builds/Latest` runtime folder (including VERSION.txt); never replace only its executable. Keep older versioned builds separately. Current property/title/summit review build is 0.17.0-review1; reviewed baseline is 0.16.0-review3; Builds/Latest/VERSION.txt records its source and completion checkpoint. CR-061/062 are implemented awaiting Dan review; CR-057–060 and earlier reports remain awaiting review. BUG-004/008 and CR-046–049 remain awaiting Dan review; technical validation does not close earlier human reports. See Docs/CR081-090/VALIDATION.md for the combined exploration/activity delivery and Docs/CR082-089/VALIDATION.md for the accepted correction delivery (CR-087 partly resolved, retained failures recorded), Docs/CR075-080/VALIDATION.md for the preceding delivery, Docs/CR070-074/VALIDATION.md for the preceding route delivery, Docs/CR067-069/VALIDATION.md for the preceding delivery, Docs/CR064-066/VALIDATION.md for the prior delivery, Docs/CR061-062/VALIDATION.md and Docs/CR057-060/VALIDATION.md for earlier revisions. A source commit alone does not update a compiled player.

- [ ] Keep the game **single-player only** unless this document is deliberately changed later.
- [ ] Prioritize **fun arcade handling** over realistic simulation.
- [ ] Build and verify **one phase at a time**.
- [ ] Do not begin a later phase until the current phase is playable and accepted.
- [ ] Astra should inspect the existing project before modifying it.
- [ ] Astra should preserve working systems unless a requested change requires altering them.
- [ ] Astra should compile/test after meaningful changes and report unresolved errors.
- [ ] Do not add paid Unity assets without Dan's explicit approval.
- [ ] Prefer simple placeholder art before spending time on polish.
- [ ] Maintain controller support throughout development.
- [ ] Keep systems modular so the street, vehicle, shortcuts, jumps, and gameplay rules can be changed independently.
- [ ] Commit or otherwise preserve a known-good state before large changes.
- [ ] Record bugs, change requests, and decisions in this file.
- [ ] Every Astra/Codex work prompt must begin by committing all current saved changes as a safety checkpoint before making modifications.

---

# Definition of the First Playable Version

The first playable version is complete when:

- [ ] The project launches without compile errors.
- [ ] One car can be driven with a controller.
- [ ] There is a chase camera.
- [ ] The childhood street exists in recognizable rough form.
- [ ] The street has been modified into a closed circuit.
- [ ] The player can complete laps.
- [ ] Checkpoints prevent obvious accidental course skipping.
- [ ] The car can reset/respawn if stuck.
- [ ] At least one intentional shortcut exists.
- [ ] At least one large arcade-style jump exists.
- [ ] The game can restart a race without restarting Unity.

---

# PHASE 0 — Project Setup and Safety Net

## Goal
Create a clean Unity foundation before any actual game content is built.

## TODO
- [x] Install/use Unity 6 or later.
- [x] Install the official Unity Plugin for Codex.
- [x] Create the Unity project.
- [x] Choose a lightweight 3D rendering setup suitable for a small PC game.
- [x] Create an organized folder structure.
- [x] Initialize Git repository.
- [x] Add an appropriate Unity `.gitignore`.
- [x] Create a simple README describing the project.
- [x] Confirm the project opens and compiles with zero errors.
- [x] Create an empty `PrototypeTrack` scene.
- [x] Save a known-good baseline commit.

## Tell Astra/Codex

> We are beginning Phase 0 of a small single-player arcade racing game in Unity 6+. The long-term concept is a Forza Horizon-inspired circuit based on one real childhood street, but DO NOT build the street, car, races, or gameplay yet.
>
> First inspect the current Unity project and use the official Unity skills where appropriate.
>
> Your task is only to establish a clean project foundation:
> 1. Verify this is a Unity 6+ project and that it compiles.
> 2. Use a sensible lightweight 3D render pipeline for a small PC arcade racing game.
> 3. Create a clear Assets folder structure for Scenes, Scripts, Prefabs, Materials, Models, Audio, UI, Track, Vehicles, and Tests as appropriate.
> 4. Create and save an empty scene named `PrototypeTrack`.
> 5. Set up Git-friendly Unity project settings where appropriate and make sure generated/library files are not meant to be committed.
> 6. Create or update a short README describing the project.
> 7. Do not install paid assets.
> 8. Do not implement vehicle physics or track generation yet.
> 9. Verify there are no compile errors when finished.
>
> At the end, summarize exactly what you changed, list any files/packages created or modified, and identify anything I need to do manually.

## Acceptance Test
- [x] Unity opens cleanly.
- [x] `PrototypeTrack` scene loads.
- [x] Console contains no compile errors.
- [x] Folder structure is understandable.
- [x] Baseline has been preserved in Git.

---

# PHASE 1 — Arcade Driving Prototype

## Goal
Prove that driving feels fun before building the real street.

## TODO
- [x] Create a simple placeholder car.
- [x] Implement acceleration.
- [x] Implement braking/reverse.
- [x] Implement steering.
- [x] Implement basic traction/grip.
- [x] Implement arcade-friendly stability.
- [x] Create chase camera.
- [x] Add Xbox-style controller support.
- [x] Keep keyboard controls available for testing.
- [x] Add reset/respawn button.
- [x] Create a flat test area.
- [x] Add a few temporary turns/ramps to test handling.
- [x] Tune driving until approved.

## Tell Astra/Codex

> Begin Phase 1 only. Build a simple arcade driving prototype in the existing Unity project.
>
> The target feel is accessible and playful, closer to Forza Horizon than a racing simulator. Do not attempt realistic tire simulation unless necessary for good arcade behavior.
>
> Requirements:
> - One placeholder vehicle.
> - Acceleration, braking, reverse, and steering.
> - Stable arcade handling that is difficult to accidentally flip during ordinary driving.
> - Some controllable sliding is acceptable, but the car should not feel like it is permanently on ice.
> - Xbox-compatible controller support from the beginning, plus keyboard fallback.
> - Third-person chase camera with smooth follow behavior.
> - A reset/respawn control for a stuck or flipped vehicle.
> - A simple flat grey-box driving area with a few turns and temporary ramps for testing.
>
> Keep vehicle code modular because we will tune physics repeatedly later.
>
> Do NOT build the real street, lap system, AI traffic, menus, visual polish, or multiple cars.
>
> Test the project and fix compile errors before stopping.
>
> Report the controls, major tuning parameters, files changed, and anything you think I should test manually.

## Acceptance Test
- [x] Car is fun enough to drive for several minutes.
- [ ] Controller works.
- [x] Camera behaves properly.
- [x] Reset works.
- [x] Car can use ramps without physics going berserk.
- [x] No compile errors.

Dan completed a hands-on driving review and reported that the car seems to drive fine. Handling, steering, braking/reverse, camera feel, and jump behavior are accepted for now. Physical Xbox controller acceptance remains separate unless Dan explicitly confirms testing it.

### Tuning Notes
- Handling: Four ray suspension probes, 1200 kg rigidbody, centre of mass (0, -0.35, 0). Acceleration 12 m/s²; forward speed target 38 m/s; reverse target 11 m/s. Grip response 7/s capped at 22 m/s². Full-lock 10-second cornering stayed upright; 7 m/s lateral slip recovered within one second. Dan must approve enjoyment and forgiveness.
- Steering: 32° low-speed / 10° high-speed, response 7/s, yaw response 8/s, 2.6 m wheelbase; left-stick deadzone 0.12. Reverse steering follows the direction of travel.
- Braking: 24 m/s², reverse acceleration 7 m/s². Hold LT/S through a stop to reverse. Tested 26.45 m/s after four seconds of acceleration, 2.16 m/s after one second braking, then -9.08 m/s after three more seconds holding reverse.
- Camera: World-up chase view, offset (0, 3.6, -7.5), position smoothing 0.16 s, heading response 6/s, look-ahead 3 m, FOV 65°. Spherecast obstruction handling; camera snaps on respawn. Rendered view inspected and real-time follow checked. Camera comfort still requires Dan's feedback.
- Jump behavior: Temporary 1.8 m / 5 m ramps tested at 15 and 25 m/s. All four runs jumped and landed upright, without inversion or runaway physics. Highest car position 7.48 m. Off-angle and maximum-speed impacts remain exploratory manual tests.
- Reset: Y / R returns upright to fixed clear pad (0, 1.1, -45), clears linear/angular velocity and steering. Falling below y=-15 automatically resets. No checkpoints or nearest-road logic.
- Controls: RT or W/Up accelerates; LT or S/Down brakes/reverses; left stick or A/D/Left/Right steers; Y or R resets. Click Game view while playing for keyboard focus. Xbox bindings verified using virtual Gamepad input; physical hardware unverified.
- Verification: 20 repeatable checks passed via Racer > Validate Phase 1 (Play mode). Additional real-time input test drove about 29 m at 0.6 throttle, reaching 16.64 m/s with camera following. Compile succeeded. Detailed test record: Docs/PHASE1_VALIDATION.md.
- Limitations: Placeholder wheels do not animate; suspension is intentionally approximate; collision/edge impacts can still flip the vehicle; reset is always to the starting pad. No known unresolved runtime errors from tested scenarios. No menus, race rules, final street, or Phase 2 work.

---

# PHASE 2 — Childhood Street / Real Loop Greybox

## Goal
Build a recognizable, drivable greybox of the **full real road loop** Dan remembers from childhood, using the older remembered landscape rather than the current-day development pattern.

The loop itself is based on real roads. Phase 2 should include the full road loop, but **not** race systems such as laps, checkpoints, timers, HUD, or start/finish logic.

**Status:** ACCEPTED — Dan played through the revised track and reported that it seems fine. Phase 3 is cleared to begin, with a small steering adjustment included as CR-010. Phase 3 implementation has not started in this documentation update.

## Reference Images

Place the map references in the project under:

`Docs/References/`

Recommended names:
- `area_map_satellite.png`
- `area_map_plain.png`
- `area_map_annotated.jpg`

The annotated image is the primary guide.

## Annotated Map Legend / Memory Notes

- **Red line** = the full real road loop to recreate.
- **Blue X** = Dan's childhood house.
- **Corrected memory:** Only two neighboring houses belong beside Dan's childhood house: retain the houses marked **2** and **3**, with their existing labels. Remove the house marked **1** and expand Dan's yard into its former site. This supersedes the original map interpretation and any older instructions to retain all three.
- **Blue circle across the street** = Dan's friend's house; this still exists and should remain.
- The other blue circle is not important.
- **Green-marked areas** = areas that should feel wooded/open rather than filled with newer subdivisions.
- **Black X marks** = modern roads/development that should be removed or ignored for the remembered version.
- **Yellow circles** = important elevation/road-profile areas described below.

## Environment / Era Direction

This is **not a current-day reconstruction**. Prefer Dan's remembered older version of the area whenever modern map data conflicts with memory.

- Remove/ignore newer subdivision roads and dense newer neighborhood build-out where marked.
- Replace those newer developments mainly with woods/open land; substantially increase trees so the area feels heavily forested.
- The area around Dan's childhood house should be sparse: Dan's house with its expanded yard, houses 2/3, the friend's house across the street, and plenty of wooded space.
- Add more approximate/random houses in sensible residential locations where exact memory is unavailable, while preserving the sparse older settlement pattern.
- There should be a placeholder house behind the hairpin turn for now because Dan remembers one there. A lake may be explored later as a gameplay change, but not in Phase 2.
- The storage facility is not important and does not need to be preserved accurately.
- The main road should have businesses/stores along both sides throughout its relevant length, with plausible gaps and access.
- The connecting road can have several houses along it.
- The far/end portion of Dan's road can also have several houses.

## Terrain / Elevation Direction

Dan's street is a hilly/mountain road, although not an extreme high mountain.

From the **first yellow circle to the last yellow circle** is the main hilly street section.

- Coming from the main road onto Dan's street, there is a **large uphill climb**; make it substantially bigger than the first Phase 2 implementation.
- After the initial climb, include **more frequent, noticeable rolling hills up and down** throughout the street; the first implementation felt too flat overall.
- The **second yellow circle** marks a **much deeper, faster downhill drop** than currently built. Shorten the descent distance and strengthen its fall while keeping it drivable. Dan and his friend used to sled down this hill when it was icy.
- The **last yellow circle** marks the end of Dan's street. Dan's latest feedback requires a **shorter, more noticeable descent** than currently built. Keep it gentler than the second major drop, but not stretched out or barely perceptible; this refines the original gradual-descent direction.
- The rest of the loop is relatively flatter than Dan's street.

Elevation character is a major part of making the location recognizable and should not be reduced to a flat map. Revise road heights and supporting terrain together: continuous roadbed, blended shoulders, and matching collision surfaces must prevent a floating slab or under-road re-entry.

House 3 must sit farther from the road and down a steep hill. The friend's house must be closer to the road but slightly downhill, still across the street.

## Phase 2 Inputs

- [x] Street / loop map references supplied.
- [x] Full real loop identified in red.
- [x] Childhood house identified.
- [x] Original house markers reviewed; corrected memory retains only neighboring Houses 2 and 3. House 1 removed under CR-012; building batch accepted and CR-012 closed.
- [x] Friend's house identified.
- [x] Wooded areas / later development removals identified.
- [x] Major elevation character described.
- [x] Main-road / connecting-road development character described.
- [x] Exact placement of less-important houses may be approximate.

## TODO

- [x] Create the full rough road loop shown in red.
- [x] Establish approximate real-world scale suitable for driving.
- [x] Shape terrain to reflect the remembered hills and elevation changes.
- [x] Add the significant uphill entrance onto Dan's street.
- [x] Add rolling smaller hills along Dan's street.
- [x] Add the larger downhill drop at the second yellow circle.
- [x] Add the gradual downhill near the last yellow circle.
- [x] Add Dan's childhood house as a simple placeholder mass.
- [x] Initial placeholders added (historical). CR-012 now removes House 1; retain Houses 2 and 3.
- [x] Add the friend's house across the street.
- [x] Add the placeholder house behind the hairpin turn.
- [x] Replace marked newer subdivisions/development with wooded/open areas.
- [x] Add approximate/random houses where appropriate on the connecting road and other remembered residential areas.
- [x] Give the main road a basic commercial/business character.
- [x] Preserve `PrototypeTrack` and create a separate greybox scene for the real loop.
- [x] Test the entire loop with the Phase 1 vehicle.
- [x] Adjust road width/curve smoothness only as much as needed for comfortable driving.
- [x] Confirm the result is recognizable enough to Dan — overall track accepted after playthrough.

## Focused Phase 2 Revision Pass — Dan's Latest Playtest

**Status: BUG-001 and CR-001–CR-009 implemented and technically verified; Phase 2 now accepted following Dan's playthrough.** Preserve the current full loop and environment. The small steering refinement is scheduled with Phase 3 as CR-010 and does not reopen Phase 2.

- [x] BUG-001: Fix floating road/terrain mismatch with continuous roadbed support, blended shoulders, and matching collision surfaces; verify plausible off-road re-entry without the car passing beneath the road.
- [x] CR-001: Make the initial neighborhood uphill substantially bigger.
- [x] CR-002: Make the second hill drop much deeper and faster over a shorter distance.
- [x] CR-003: Shorten and strengthen the final hill so its descent is noticeable, while gentler than the second drop.
- [x] CR-004: Add more frequent, perceptible rolling hills; the road currently feels too flat overall.
- [x] CR-005: Move House 3 farther from the road and down a steep hill.
- [x] CR-006: Move the friend's house closer to the road but slightly downhill.
- [x] CR-007: Add more approximate/random houses in appropriate residential areas.
- [x] CR-008: Add businesses/stores along both sides of the main street.
- [x] CR-009: Substantially increase trees and forest density throughout the remembered wooded areas.
- [x] Verify the entire revised loop in both directions and representative off-road re-entry locations with the unchanged Phase 1 car (180 traversals at 3/8/15 m/s; see final report for scope).
- [x] Record technical results and Dan's subsequent playthrough: track seems fine; proceed to Phase 3 with minor steering refinement.

## Tell Astra/Codex

### Archived prompt — completed Phase 2 revision pass

Historical instructions below applied before acceptance. For the next session, use the Phase 3 prompt; its scope supersedes this archived prompt's Phase 3 prohibition.

> Before making any modifications, inspect the current project state and create a Git commit of all current saved changes as a safety checkpoint. Do not modify anything until that commit succeeds. If the working tree is already clean, record the existing HEAD as the safety checkpoint and report that no saved changes needed committing; if a required commit fails, stop and report the blocker.
>
> Continue the Unity project Racer with a focused Phase 2 revision pass only. Read PROJECT_TODO.md and inspect Docs/References/, especially the annotated map, plus the current StreetLoopGreybox scene and its generation workflow. Phase 1 driving is accepted for now. Phase 2 is NOT accepted: Dan tested it and reported BUG-001 and CR-001 through CR-009. Do not start Phase 3.
>
> Preserve the existing full real loop layout, road connections, landmark sequence, and remembered older landscape. Work in Assets/Scenes/StreetLoopGreybox.unity and its supporting environment assets/tools. Preserve PrototypeTrack and the Phase 1 vehicle, handling/tuning, suspension, input/controller support, chase camera, and reset systems. Correct the environment rather than masking its faults with vehicle changes. Keep this a simple greybox.
>
> Implement all ten feedback items:
>
> 1. BUG-001 — Fix terrain-road blending and physical support first. The road sits too high above the terrain; when Dan drives off-road and tries to re-enter, most of the car stays beneath the road. Inspect both visible geometry and collision/suspension contact surfaces. Shape continuous supporting ground/roadbed beneath the road and blend it into traversable shoulders and surrounding slopes. Keep road and terrain colliders aligned with the visible surfaces, without gaps, exposed slab edges, abrupt collision lips, or overlapping surfaces that snag the car. The road must not remain a floating slab. At ordinary traversable shoulders, driving off-road and back onto the road must be physically plausible, with the car supported throughout and no passing under/through the road, clipping, falling through, or artificial launch. Do not hide the problem with teleporting, invisible walls, altered vehicle suspension, or merely thicker visible pavement. Steep natural slopes can remain steep; distinguish those from normal re-entry shoulders.
> 2. CR-001 — Make the initial uphill into the neighborhood substantially bigger and more noticeable than the current implementation.
> 3. CR-002 — Make the second hill/second yellow-circle descent a much deeper drop over a shorter distance: it must fall faster and feel clearly steeper, while remaining drivable.
> 4. CR-003 — Shorten the last hill and make its descent more noticeable. The current version is too drawn-out and subtle. Keep it gentler than the second major drop, but clearly perceptible from the driving view. This updates the earlier gradual-descent guidance.
> 5. CR-004 — Add more frequent, perceptible rolling hills to the road, especially throughout the neighborhood section. The loop currently feels too flat. Keep the other sections relatively flatter than the neighborhood without leaving the overall drive unnaturally flat.
> 6. CR-005 — Move House 3 farther from the road and down a steep hill; shape the surrounding ground so its lower position and setback read clearly.
> 7. CR-006 — Move the friend's house closer to the road, still across the street at the remembered location, but slightly downhill from it. Blend its site into the slope.
> 8. CR-007 — Add more approximate/random placeholder houses in sensible residential locations, including the connecting road and far end of Dan's street. Preserve the key houses, hairpin house, and sparse older settlement pattern; do not recreate newer subdivisions.
> 9. CR-008 — Add businesses/stores along both sides of the main street throughout its relevant length, rather than a few isolated blocks. Use simple varied building masses and plausible setbacks/access gaps; keep the driving corridor clear.
> 10. CR-009 — Substantially increase tree coverage and density so the area feels heavily forested, especially around the childhood-house area and where newer subdivisions were removed. Use simple efficient placeholders, preserve road/shoulder clearance and useful sightlines, and check play-mode performance after the increase.
>
> Revise the road profile and its supporting terrain together. Preserve the horizontal loop layout; make only small local geometry adjustments needed for driveability, and report them. Do not invent exact surveyed heights or distances: use Dan's qualitative feedback and reference landmarks, expose practical tuning values where useful, and document the resulting elevation/setback changes and any guesses. Keep changes reproducible through any existing scene-generation workflow so rebuilding does not discard the revisions.
>
> Validate the revised scene with the unchanged Phase 1 car:
>
> - Drive the complete loop in both directions. Check uphill traction, crests, the deeper second drop, the shorter final descent, and rolling hills for grounding, smooth transitions, and unobstructed travel.
> - Reproduce the original off-road re-entry bug before fixing it where possible, then test leaving and rejoining on both sides at representative flat sections, bends, hill approaches/crests/descents, and terrain joins. Test slow and ordinary driving speeds and shallow/oblique approaches. Verify visible and physical support agree; road-center sampling alone is not sufficient.
> - Inspect House 3, the friend's house, additional houses, both sides of the commercial road, and forest coverage from the driving camera and an overview.
> - Confirm scene save/reload, clean compilation, no new Console errors/warnings, and that keyboard controls, camera, and reset still behave as before. Report physical-controller testing separately; do not claim hardware verification unless performed.
> - Check that added trees/buildings do not obstruct the road or intended shoulder transitions and do not introduce a material play-mode performance regression. Report observed results and any limits; do not claim tests that were not run.
>
> Update PROJECT_TODO.md with actual work completed, BUG-001/CR-001–CR-009 implementation and verification results, revised acceptance notes, and SESSION HANDOFF. Preserve earlier validation as historical evidence; it did not establish safe off-road re-entry or Dan's subjective acceptance. Leave Phase 2 NOT ACCEPTED / awaiting Dan's retest even if technical checks pass. Do not check Dan's recognition/feel approval on his behalf.
>
> Do not add start/finish, checkpoints, laps, timers, HUD, race restart systems, shortcuts, stunt ramps/jumps, breakable-fence gameplay, or a visual polish pass. Do not begin Phase 3 or later phases.
>
> When the revision work is stable, create a completion Git commit. Report the safety and completion commit IDs, scene to open, files changed, each feedback item's outcome, terrain-road support solution, elevation/placement changes and guesses, actual test results and remaining issues, and a focused checklist for Dan to retest all ten items before accepting Phase 2.

## Acceptance Test

- [x] The full real road loop exists and is drivable.
- [ ] Dan recognizes the road layout.
- [ ] The initial uphill onto Dan's street feels significant.
- [ ] Rolling hills are frequent and noticeable enough to resolve the overly flat driving feel.
- [ ] The larger downhill drop feels recognizable.
- [ ] The final downhill is shorter and clearly noticeable, while gentler than the second drop.
- [x] Original house placeholders represented (historical); corrected arrangement is tracked under CR-012.
- [x] Friend's house across the street is represented.
- [x] Newer subdivisions are substantially removed/replaced with woods/open land.
- [ ] Main road/commercial area feels appropriately more developed.
- [ ] Connecting road has suitable residential development.
- [ ] Car can drive the complete revised loop without major collision problems.
- [ ] Road has continuous terrain/roadbed support and blended shoulders; representative off-road exits/re-entries on both sides do not pass under/through the road or snag on slab edges.
- [ ] House 3 is farther from the road and down a steep hill.
- [ ] Friend's house is closer to the road and slightly downhill.
- [ ] More random houses suit the remembered older residential areas.
- [ ] Businesses/stores line both sides of the main street.
- [ ] Substantially more trees make the area feel heavily forested.
- [x] No compile errors.
- [x] Dan approves the greybox as recognizable enough to proceed — track seems fine after playthrough.

**Acceptance notes (2026-09-17):** Phase 2 is **ACCEPTED**. Dan played through the revised track and said it seems fine. This is overall track acceptance, not a claim that he individually repeated every technical test above. Unchecked detailed items retain that distinction and do not block Phase 3. Existing technical evidence remains as recorded. Dan requests slightly tighter steering during the next phase (CR-010); this is a modest tuning follow-up, not a Phase 2 blocker. Physical controller verification remains separate.

### Street / Terrain Notes

- Key landmarks: Dan's childhood house with expanded yard; houses 2/3; friend's house across the street; placeholder house behind hairpin.
- Things that must remain recognizable: full real road loop; hilly character of Dan's street; sparse wooded childhood-house area.
- Things we can fictionalize/approximate: exact placement of less-important houses; details of businesses on main road; minor vegetation/building placement.
- Possible later gameplay alteration: consider replacing the house behind the hairpin with a lake if that proves more fun, but **not during Phase 2**.
- Bugs: BUG-001 was addressed in the revision and passed the recorded technical checks; Dan has now accepted the track overall. No new track issue reported.
- Dan's earlier review notes (addressed by the accepted revision): Initial hill too small; second drop too shallow/slow; final hill too drawn-out/subtle; too few rolling hills; House 3 needs greater setback and steep downhill placement; friend's house needs less setback and slight downhill placement; more random houses; businesses on both sides of main street; substantially more forest. See CR-001 through CR-009.

---

# PHASE 3 — Add Race Systems to the Real Loop

## Goal
Turn the already-built real closed road loop into a functioning race course by adding race systems while preserving the Phase 2 environment.

**Status:** Phase 3 race systems ACCEPTED — Dan reports all tests passed. CR-010 remains open as a non-blocking handling refinement: improvement was barely noticeable and the car still feels a little floaty, though not terrible. Phase 2 remains ACCEPTED. Phase 4 is the next planned phase; no implementation started in this documentation update. See Docs/PHASE3_VALIDATION.md for prior technical evidence.

## TODO
- [x] Implement and technically test the initial CR-010 steering adjustment. Subjective improvement remains insufficient; follow-up is open and non-blocking.
- [x] Confirm the existing loop supports complete race laps (technical checks and Dan's reported tests passed).
- [x] Assess race-flow adjustments: none required; preserve the accepted layout.
- [x] Establish start/finish area.
- [x] Add checkpoint system.
- [x] Add lap counting.
- [x] Add race timer.
- [x] Add wrong-way/course validation where needed.
- [x] Add race restart.
- [x] Add basic HUD for lap/time/checkpoint status.
- [x] Complete multiple test laps.

## Tell Astra/Codex

**Historical implementation prompt:** Phase 3 race systems have now passed Dan's review. The earlier prohibition on Phase 4 below applied before that review; do not rerun completed race-system work.

> Before making modifications, inspect the current project state and commit all current saved changes as a safety checkpoint. Do not modify anything until the commit succeeds. If already clean, record the existing HEAD as the checkpoint; if a required commit fails, stop and report it.
>
> Read PROJECT_TODO.md. Dan has played through and accepted the Phase 2 track. Begin Phase 3 only: add race systems to the existing real closed-loop greybox and include the small steering refinement in CR-010. Do not reopen Phase 2 or create a separate prerequisite handling phase.
>
> Preserve the Phase 2 real road loop and remembered environment. Do not invent a new connector road because the loop already exists in the real road layout.
>
> Implement:
> - Start/finish line.
> - Ordered checkpoint system.
> - Lap counting.
> - Basic race timer.
> - Race restart.
> - Minimal HUD showing only information needed for testing.
> - Course validation sufficient to prevent accidental invalid laps.
>
> IMPORTANT: Later phases will intentionally add shortcuts. Do not design the checkpoint system so rigidly that every future alternate route becomes painful to support. Make the checkpoint/course system modular enough for multiple valid paths between required route gates.
>
> CR-010 — Dan wants steering a little tighter while driving on roads. Interpret this as a modest improvement in steering response and cornering precision, not a physics overhaul. Inspect current tuning and try small changes to steering response and/or speed-dependent steering authority first; do not blindly increase every steering/grip value. Preserve progressive controller input, keyboard support, predictable high-speed stability, braking, suspension, camera and reset behavior. Keep the accepted track unchanged. Record original and revised values so the adjustment is easy to compare or revert. Test ordinary road bends, the hairpin, hills and faster sections, plus reverse steering; avoid twitchiness, sudden oversteer or new flipping. Report virtual-input versus physical-controller testing honestly. Leave final steering-feel approval to Dan during Phase 3 review. This tuning task must not hold up starting the race systems.
>
> Do not add final shortcuts or spectacular jumps yet.
>
> Test several complete laps, checkpoint order/invalid skipping, restart and HUD, alongside the steering checks. Resolve compile/runtime errors and report actual test coverage. Update Phase 3 checkboxes, CR-010 and SESSION HANDOFF only for work completed, then create a completion commit when stable. Report both checkpoint/completion commits, files changed, steering values before/after, and what Dan should test. Do not begin Phase 4.

## Acceptance Test
- [x] Player can repeatedly complete valid laps. (Technical tests; physical hardware remains untested.)
- [x] Invalid lap skipping is reasonably prevented. (Technical tests; physical hardware remains untested.)
- [x] Restart works. (Technical tests; physical hardware remains untested.)
- [x] HUD works. (Technical tests; physical hardware remains untested.)
- [x] Circuit is acceptable to proceed — Dan reports all tests passed; mild handling floatiness is tracked separately.
- [x] Original street remains acceptable — previously accepted track retained; Dan reports tests passed.
- [ ] Steering improvement is clearly noticeable to Dan without twitchiness or loss of stability — not yet satisfied; CR-010 remains a non-blocking follow-up.
- [x] Steering changes preserve controller/keyboard input, reverse, braking, suspension, camera and reset behavior. (Technical tests; physical hardware remains untested.)

**Dan's latest review:** All tests passed, but handling changed little and still feels a bit floaty, though not terrible. Accept the race systems and retain CR-010 as a mild follow-up rather than holding up progress. Input device was not specified; this does not establish physical-controller testing or new automated coverage.

---

# PHASE 4 — Jumps and Arcade Spectacle

**Status:** ACCEPTED — Dan approved Phase 4 and the combined CR-010/CR-011 changes. Phases 2 and 3 remain ACCEPTED. One jump only; Phase 5 now has one shortcut awaiting approval. Detailed Phase 4 implementation evidence and limitations: `Docs/PHASE4_VALIDATION.md`.

## Goal
Make the course delightfully irresponsible.

## TODO
- [x] CR-011: Forward speed parameter 38→44 m/s; acceleration 12→13.5 m/s². Technically tested and approved by Dan.
- [x] Record original/revised tuning and compare at representative racing speeds.
- [x] CR-010: Diagnose yaw, lateral slip and body motion; apply modest yaw/grip changes, leaving suspension and steering angles unchanged.
- [x] Select first major jump: northbound western connector, between CP13 and CP14.
- [x] Build one supported 24 m × 6 m takeoff, 3 m rise, with readable approach sign.
- [x] Mark a 108 m × 15 m landing corridor on existing supported road/shoulders; retain ordinary road lane beside ramp.
- [x] Assess air control: existing stability suffices in tested cases; no new control added.
- [x] Assess suspension/landing: no changes needed for intended-speed tested landings.
- [x] Reuse existing fixed-spawn reset; verify recovery cannot preserve skipped checkpoint credit.
- [ ] Second stunt feature — explicitly deferred; NOT authorized in this session.
- [x] Test slow, intended, fast, angled, undershoot, overshoot, bypass and recovery cases; document limits.

## Tell Astra/Codex

> Before making modifications, inspect the project and commit all current saved changes as a safety checkpoint. Do not modify anything until the commit succeeds. If already clean, record HEAD as the checkpoint; if a required commit fails, stop and report it.
>
> Begin Phase 4 only. Phase 3 race-system tests passed Dan's review. Add the first major Forza Horizon-style stunt jump to the existing circuit.
>
> Include a small, non-blocking CR-010 follow-up: Dan barely noticed the steering change and still finds handling mildly floaty. Diagnose steering/yaw lag versus lateral sliding versus body/suspension motion before tuning. Compare a modest reversible adjustment on identical ordinary road sections, record original/revised values, and preserve stable high-speed driving, input support, camera, reset and race systems. Do not turn this into a vehicle overhaul. Leave subjective handling approval to Dan.
>
> CR-011 — Dan also wants the car a bit faster because slow testing does not give a representative driving feel. Inspect the current acceleration and speed limits first. Start with roughly a 15–20% increase in forward top speed and a modest acceleration increase. These are starting points to validate, not mandatory values if unstable. Keep the changes reversible and record original/revised values.
>
> Tune the tighter, less-floaty handling at the revised speeds. Preserve controllable braking, predictable steering, progressive input, and working race systems. Size the jump approach and landing area for the revised performance.
>
> Keep slow checks for basic correctness, but do not validate primarily with earlier conservative automated targets. Test sustained driving at representative racing speeds, near-top-speed travel on suitable straights, braking into corners, and jump approaches at the revised speeds. Slow appropriately for tight turns; do not expect every corner to work at full throttle. Report actual speeds reached, test conditions, stability/track limitations and any checks not performed. Leave final speed and handling approval to Dan.
>
> The jump should feel intentionally exaggerated and exciting rather than realistic.
>
> Requirements:
> - Use the existing vehicle.
> - Create a clear approach and readable takeoff.
> - Give the player enough landing room.
> - Tune vehicle behavior so landing is forgiving without eliminating the possibility of failure.
> - Add limited arcade air-control only if it improves the experience.
> - Failed jumps must recover cleanly through the existing reset/respawn system.
> - Do not break normal lap/checkpoint progression.
>
> Build ONE polished gameplay jump first. Do not scatter random ramps everywhere yet.
>
> After implementation, report recommended approach speed, tuning changes made to the car, and any physics compromises.

## Acceptance Test
- [x] Dan approves the revised speed, acceleration and handling together.
- [x] Representative racing-speed tests cover sustained road driving, near-top-speed straights, corner-entry braking and revised-speed jump approaches; actual speeds and limitations are recorded.
- [ ] Faster performance preserves predictable steering, controllable braking, stable landings and valid race/reset behavior.
- [ ] Jump is fun.
- [ ] Successful landings feel satisfying.
- [ ] Failed jumps do not ruin the race permanently.
- [ ] Jump does not destabilize ordinary car physics.
- [ ] Lap logic remains valid.

### Jump Ideas / Notes
**Acceptance update:** Dan replied "approved" after the Phase 4 completion report. This records overall acceptance of the jump and combined speed/handling changes, not a claim that every detailed manual test was individually repeated. Physical-controller coverage remains unconfirmed. Remaining detailed checkboxes do not block overall acceptance. No new implementation or tests were performed for this approval update.

- Jump #1: Orange ramp after CP13; recommended 110–120 km/h (31–33 m/s). Intended 32 m/s centered takeoff measured 31.59 m/s, ~1.72 s air time, apex 6.31 m above road. Fast -3° approaches drift outside the marked landing corridor; see validation limits.
- Future jump: Deferred pending a separate implementation request.
- Ridiculous idea worth trying later:

---

# PHASE 5 — Intentional Shortcut

**Status:** ACCEPTED — Dan confirmed the shortcut works and agreed to move on. Phases 2–4 remain accepted. Phase 6 is next; no new implementation in this documentation update.

## Goal
Allow players to discover a faster, narrower alternative through the remembered woodland.

## TODO
- [x] Add one readable dirt shortcut inside the southwest bend, after CP11 and before CP12.
- [x] Inspect ordered checkpoint logic; retain every gate unchanged. Both paths already fit between required gates, so no alternate-route code is needed.
- [x] Assess off-road behavior and barriers: neither is necessary for this first passage. No grip changes or destruction system.
- [x] Build supported entrance and re-entry using the existing continuous terrain/collision mesh.
- [x] Make only local environment edits: one terrain tile, one tree cleared, signs and edge markers.
- [x] Compare matched normal/shortcut runs including approach and re-entry; retain failed-attempt evidence.
- [x] Test slow/intended/fast/misaligned driving, recovery, mixed laps, skipped/repeated/wrong-way gates, reset, restart, accepted jump, braking, HUD and virtual controls.
- [x] Save/reload scene, compile and inspect Console and visual captures.
- [x] Dan accepts the shortcut overall and agrees to move on. Individual test/device coverage is not implied.
- [ ] Second shortcut — deferred; not authorized.

## Shortcut and measured results
Follow the normal race to CP11 at the southwest bend. The yellow WOODLAND CUT sign appears just before the gate. Pass CP11, then bear right onto the brown inside path; rejoin northbound before CP12. Entrance **(-560.08,6.36,-551.35)**; rejoin **(-620.04,6.39,-449.38)**. Hierarchy: `Phase 5 - Southwest woodland shortcut`. Start around **80–90 km/h**; the sign's upper 95 km/h requires more precision. Straighten for re-entry; missing the entrance leaves the normal road available.

Three matched pairs, identical common start/end and paired injected starting speeds of 22/24/26 m/s: normal **11.506 / 11.445 / 11.385 s**, shortcut **9.528 / 9.487 / 9.459 s**. Mean saving **1.954 s (17.1%)**; no failures in those six paired runs. The conservative follower slowed to about 17 m/s at the shortcut entrance; separate sustained 24–26 m/s attempts verified faster entries. Stress matrix: six clean and eight failed core-corridor attempts; 34–42 m/s approaches ran wide, with a worst 11.48 m line error. All these stress attempts eventually recovered upright, so the risk is forgiving rather than a guaranteed crash.

Nine racing-speed laps passed: three normal, three shortcut, and shortcut/normal/shortcut. Peak **39.71 m/s**, average **25.35–25.45 m/s**. Every existing gate and jump remained valid. Final 37-check race/input/HUD/reset regression passed; failed shortcut resets, unrelated skipped checkpoints, repeated and wrong-way crossings awarded no extra lap progress. Ground support passed 273 samples. Four 18 m/s off-road recoveries and ±5° entrance/re-entry tests at 24 m/s stayed supported and upright.

An ordinary-frame **virtual Gamepad** shortcut run also passed: 9.468 game seconds, 24.32 m/s peak, 2.86 m maximum line error, upright 1.000; camera followed and HUD displayed speed. It touched the feathered path edge. Physical-controller testing and subjective approval remain Dan's responsibility. Original failed test-harness runs and their corrections are retained rather than hidden.

No vehicle tuning, camera/input/reset/race source, gate geometry, jump, road loop or landmarks changed. Local work: 548 terrain vertices affected within one tile; one tree collider plus crown/trunk visuals cleared; two signs and twelve edge markers. Same mesh provides visible and physical support. No new surface penalty, barrier or destruction code. Existing rules still permit minor cuts between gates.

Full test conditions, raw-report links, limitations and manual checklist: **`Docs/PHASE5_VALIDATION.md`**. Completion commit is reported in the task's final response. Do not run the older Phase 2 environment rebuild over this local modification.

## Acceptance Test — Overall phase accepted

Dan confirmed the shortcut works and agreed to move on. Detailed unchecked items below do not imply individually verified manual/device coverage and do not block progression.
- [ ] Entrance is recognizable and deliberately usable.
- [ ] Clean shortcut runs feel usefully faster.
- [ ] Narrower line supplies sufficient, manageable risk.
- [ ] Re-entry and off-road recovery feel predictable.
- [ ] Normal, shortcut and mixed laps progress correctly.
- [ ] Physical controller and camera comfort reviewed.

---
# PHASE 6 — Track Personality and Destructible Environment

## Goal
Replace the sterile prototype feeling with a playful environment.

**Focused building batch:** ACCEPTED — Dan says it looks fine. Photo-based home accuracy is deferred to CR-013 and does not block further work. CR-012 removes House 1 and extends Dan's yard into its site; Houses 2/3 retain their labels and exact placement. All 25 retained residences and 22 businesses now use reusable low-poly architecture. Phases 2–5 remain accepted. This building batch is accepted; Phase 6 is now ACCEPTED after Dan's approval. Phase 7 is ready to begin. Full evidence and limitations: `Docs/PHASE6_VALIDATION.md`.

## TODO
- [x] CR-016 implemented in the combined CR-016/CR-017 batch: Dan's house 116.581m south; yard 9003.472 to 2250.869m² (25%); 107 new trees. Accepted for now; exact house placement deferred under CR-018. Evidence in Docs/CR016-017/VALIDATION.md.
- [x] Implement CR-015 house-placement corrections; grounded sites, persistent authoring data and annotated before/after evidence in Docs/CR015/VALIDATION.md. Accepted as part of Dan's overall Phase 6 approval; individual test coverage is not implied.
- [x] Four-mailbox/two-sign appearance accepted. Combined CR-016/CR-017 batch now adds yielding behavior, three gameplay signs and nine fence sections; behavior accepted by Dan.
- [x] CR-012 implementation: Remove only the house marked #1, its dedicated collision and obsolete house-specific props/access; expand Dan's yard naturally into its former site. Retain Houses #2/#3 without renumbering, and preserve their positions/elevations.
- [x] Verify the expanded yard is continuous, grounded and free of invisible House #1 collisions; update relevant generation logic so House #1 does not return.
- [x] Improve house/building silhouettes in this focused batch: 4 residential + 4 commercial reusable variants, grounded entrances and aligned simple collision.
- [x] Dan visually approves this building batch and CR-012; physical-controller testing remains unconfirmed.
- [x] Initial tree appearance pass: three reusable crown shapes, coherent variation, unchanged 6,102 placements/colliders and 90 visual batches. Historical report: Docs/VEGETATION_VALIDATION.md. CR-014 below expands this coverage.
- [x] Implement CR-014 woodland expansion: 6,102 to 11,897 trees, connected canopy with explorable trunk spacing; protected accepted sites and gameplay retained. Accepted as part of Phase 6 overall.
- [x] Profile baseline, intermediate and full coverage in ordinary frames; repeat 1440x900 standalone comparisons and rendering-cost diagnostics. Results/variability/limits: Docs/CR014/VALIDATION.md.
- [x] Dan reports tree coverage is much better; retain the expanded woodland as the visual baseline.
- [x] Dan approves Phase 6 overall. This does not claim individual test/device coverage or erase documented forest-follower and frame-time limitations.
- [x] Add nine restrained lightweight fence sections at Dan's yard, House #2 and the friend's property; integrated batch accepted by Dan.
- [x] Dan approves the appearance of the four mailboxes and two signs. CR-017 behavior is implemented separately; Dan subsequently approved the combined CR-017 behavior batch.
- [x] CR-017 implemented: 18 yielding assemblies, bounded debris, race-restart restoration; integrated tests passed; Dan approved the batch.
- [x] Add restrained surface-following road markings: main-road edge lines and sparse center dashes; neighborhood center dashes only.
- [x] Ten central commercial frontages receive flush paving and blended gravel. Raised curbs/continuous residential sidewalks are unnecessary; preserve open shoulders.
- [x] Blend local commercial joins; verify foundations and terrain seams. Terrain geometry, houses and yards preserved.
- [x] Improve daytime lighting with the existing sun, tri-light ambient and consistent terrain/forest shading; verify visible player rendering.
- [x] Add restrained generated wind/leaves and occasional birds; two sources, documented CC0 assets and playback/loop/restart checks.
- [x] Complete matched visible-player profiling and gameplay validation. Host-dependent spikes remain; no universal smoothness guarantee.

## Astra Direction
Phase 6 is ACCEPTED following Dan's overall approval. Preserve this environment while beginning a coherent Phase 7 game-flow delivery. CR-013 photo accuracy and CR-018 exact house placement stay deferred. Historical test limitations remain documented; approval does not imply new measurements.

---

# PHASE 7 — Racing Game Feel

**Status:** ACCEPTED — Dan answered yes to the full Phase 7 review checklist. Core countdown/results/persistent bests/pause/settings/navigation/audio accepted; input device was not specified. Phase 6 remains accepted. Evidence and limits: `Docs/Phase7/VALIDATION.md`; controls, timing, save/settings and audio rules: `Docs/Phase7/RULES.md`. At that historical checkpoint optional stunt scoring/speed traps were deferred (superseded by implemented CR-079/080, awaiting review); Phase 8 was next, starting with CR-019 commercial layout and coordinated frontage polish.

## Goal
Make the prototype feel like a game rather than a Unity demonstration.

## TODO
- [x] Countdown.
- [x] Finish/result screen.
- [x] Review/reuse existing current-race best-lap tracking and integrate it with results/persistent personal bests.
- [x] Personal best saving.
- [x] Speed display already exists from CR-011; preserve and integrate it with Phase 7 UI.
- [x] Better reset feedback.
- [x] Pause menu.
- [x] Settings.
- [x] Controller-friendly UI navigation.
- [x] Audio feedback.
- [x] Stunt scoring implemented under CR-079; awaiting Dan review.
- [x] Speed traps implemented under CR-080; awaiting Dan review.

---

# PHASE 8 — Visual Polish

**Status:** ACCEPTED FOR NOW — Dan reports "pretty sure all is ok." This records overall provisional acceptance, not confirmation of every test/device or resolution of known limits. CR-019 commercial revision is ACCEPTED/CLOSED. Combined visual/performance delivery and fresh Windows build complete. Evidence, measurements and limitations: Docs/Phase8/VALIDATION.md; matched views: Docs/Phase8/review.html.

## Goal
Improve appearance only after gameplay works.

## TODO
- [x] CR-019 independent storefront order, spacing, setbacks and coordinated frontage; accepted by Dan.
- [x] Replace important visible placeholder car body/cabin with cohesive low-poly bodywork; accepted buildings retained.
- [x] Improve material consistency and road surfaces with filtered asphalt grain and existing-palette values.
- [x] Improve vegetation readability with directional crown shading; retain existing variations and all 11,986 trees.
- [x] Improve existing daytime grounding through ground shadow reception; no time-of-day system needed.
- [x] Evaluate optional subtle post-processing: deferred, direct surface/text fixes address visible defects without an effects stack.
- [x] Evaluate shadow cost/quality: keep existing 40m/2048/four-cascade settings; measured added shading cost and host variability disclosed.
- [x] Evaluate LOD/culling: no isolated bottleneck justifies new transitions/collision risks; retain accepted batching and coverage.
- [x] Profile ordinary-frame standalone road/commercial/forest/gameplay views twice, matched settings, frame distributions/spikes recorded.
- [x] Fix text visible through terrain and align arrows to slopes; verify seams, grounding, near/far rendering and exact collision preservation.
- [x] Integrated traversal, mixed full race, jump, props, flow/settings/save reload and fresh visible Windows build validation.
- [x] Dan accepts Phase 8 overall for now ("pretty sure all is ok"); exact setup, individual test coverage and physical-controller testing remain unspecified.
- [ ] CR-018 deferred optional backlog: exact childhood-house placement only when Dan requests it.
- [ ] CR-013 deferred optional backlog: home refinements only from Dan's supplied reference photos.

**Build:** Builds/Phase8/Racer.exe. Scene: Assets/Scenes/StreetLoopGreybox.unity. No Phase 9 work.

---

# CORE FOLLOW-UP — Vehicle Audio and Friend Playtest Package

- [x] CR-022: Checkpoint ding/buzz integrated with CR-023/024/025; awaiting review.

- [x] CR-020: Vehicle audio and persistent Vehicle Volume implemented; subjective review pending.
- [x] CR-021: Complete local Windows ZIP built; extracted launch/settings persistence checked. Manual driving/flow and listening review remain.
- [ ] Dan reviews audio and a friend tests the extracted package on a compatible PC; do not infer success from local testing.

---

# PHASE 9 — Optional Expansion

**Next authorized expansion:** CR-034–039 as one coordinated woodland arcade-racing delivery: black paint, destruction/fences, difficulty balance, four-lane Hwy 92/faster traffic, road-name signs, and three varied woodland stunt shortcuts. CR-040 separate lake/woods circuit remains future backlog. Latest scope explicitly permits these local road/terrain/fence changes over older preservation directions, while keeping houses/yards fixed.

Historical optional-expansion list below is superseded by current CR-075–081 authorizations; unchecked items here do not cancel those approvals.

- [ ] Optional feasibility follow-up: private online play for Dan and a friend. Not authorized for implementation yet; preserve solo-first scope. Need shared car/race/prop state, latency/disconnect handling and two-PC testing; choose platform/join/host behavior before implementation.

- [x] CR-027: Initial four-vehicle roster implemented: existing car, Longroof GT, Needle 600 motorcycle and Trail Four ATV; awaiting Dan review.
- [x] CR-027: Persistent garage, reusable profiles and vehicle-separated records; technical checks pass, awaiting Dan review.
- [ ] CR-040: Future separate lake/woodland circuit near the friend's house; not part of this delivery.
- [ ] Larger neighborhood — approved and queued under CR-081.
- [ ] CR-039: Three varied woodland stunt shortcuts (stream leap, gully route, ridge/woodland jump), integrated with legal race progress.
- [x] Free-roam mode implemented under CR-078; awaiting Dan review.
- [x] CR-023: Light configurable two-way traffic implemented; awaiting review.
- [x] CR-023 initial three-opponent implementation; Dan reports opponents not visible/competitive enough. CR-026 revises grid, visibility and difficulty with CR-027.
- [ ] Collectibles — approved and queued under CR-081.
- [x] Speed traps implemented under CR-080; awaiting Dan review.
- [x] Physical jump challenges and medals implemented under CR-079; awaiting Dan review.
- [ ] Larger real-world map import pipeline.
- [ ] OpenStreetMap-assisted generation.
- [ ] Automated world-generation tools.

---

# BUG TRACKER

Use this section whenever something is wrong.

## Active Bugs

### BUG-001 — Floating road / unsafe off-road re-entry
**Status:** Resolved in sampled technical checks; track accepted by Dan after playthrough. No new reproduction reported; no claim of exhaustive off-road certification.  
**Phase introduced:** Phase 2.  
**Description:** Road sits too high above the terrain, with insufficient terrain-road blending/support.  
**Steps to reproduce:** In StreetLoopGreybox, drive the Phase 1 car off the road onto nearby terrain, then attempt to return to the road. Dan did not specify an exact location; inspect representative shoulders throughout the loop.  
**Expected behavior:** Continuous physical roadbed and blended traversable shoulders support the vehicle onto the road; collision and visible geometry agree. Road does not behave as a floating slab.  
**Actual behavior:** Most of the car remains beneath the elevated road during attempted re-entry.  
**Screenshots/logs:** Dan's latest written Phase 2 playtest feedback; no specific reproduction coordinates supplied. Earlier road-center validation did not establish safe off-road re-entry.  
**Astra fix attempt(s):** Reproduced up to 0.496 m body-below-surface re-entry and a 1.061 m edge height change. Replaced stacked ribbons/coarse ground with a single continuous visible/collision heightfield, blended shoulders and terrain. Fixed stale GPU mesh buffers during rebuild; all Phase 1 systems preserved.  
**Result:** Final 180/180 curved shoulder traversals passed at 3/8/15 m/s, both sides and nominal 25/55-degree approaches; zero under-road or airborne steps. Whole-loop drives passed both ways. Lateral +/-16 m sampling found no missing/stacked support; visible/collision meshes match. Earlier unsuccessful exploratory paths and one corrected steep site transition are retained in reports. Tests do not certify arbitrary steep slopes or top-speed recovery. Dan subsequently accepted the track overall; detailed technical coverage remains limited to the recorded scenarios.

### BUG-002 — Unexplained motorcycle/ATV braking and failed ramp approaches
**Status:** IMPLEMENTED / AWAITING DAN APPROVAL — combined revision validated and locally packaged.
**0.5.0 evidence:** Ordinary-frame baseline reproduced backwards speculative-CCD normals on upward ramp/ground faces with throttle1/brake0, including motorcycle51.297→15.903m/s in40ms. Scoped face-normal/separation correction retains collisions; contact-only repeats remove severe losses. All-four-profile standalone road/hill/ramp repeats and60/60 asymmetric-contact fixtures pass. Raw failed/successful telemetry retained in Docs/CR028-033.
**Reported:** Bike/ATV intermittently slow as if hitting something without visible contact. Dan reports every attempted ramp approach slowed so much the jump was impossible.  
**Investigation:** Reproduce ordinary-frame driving with real player inputs before tuning; inspect contacts/CCD, suspension and grounding, body/ramp geometry, throttle/brake requests, stability/traction limits, surface resistance and wipeout state. Existing evidence of sharp bike slowdown and wide ATV landing is not a clean pass. Fix the cause without globally disabling collisions, adding a ramp speed boost or weakening cars to hide it.  
**Acceptance:** Repeated bike/ATV road/hill/ramp approaches preserve appropriate momentum without phantom braking; verify approach/takeoff speeds and grounded/airborne behavior, then retest class contact asymmetry and landings. Preserve disclosed failed cases.

### BUG-003 — Excessive reset/wipeout notifications
**Status:** IMPLEMENTED / AWAITING DAN APPROVAL — combined revision validated and locally packaged.
**0.5.0 evidence:** Impact/landing triggers falsely treated ordinary support contacts as wipeouts. Sustained overturned state now emits one short transition hint; normal bumps/landings do not cut control or repeat messages. Zero wipeout samples in24 corrected standalone driving fixtures; quiet normal HUD.
**Reported:** Reset message appears too frequently and annoys Dan.  
**Requested fix:** Identify repeat-triggering and misleading wipeout detection, emit only meaningful state-transition feedback, keep recovery help compact and dismissible/short-lived, and avoid prompts during normal driving, normal landings, recoverable bumps or menus. No per-frame message recreation or repeated sound spam. Validate together with CR-029.

### BUG-004 — Designated shortcuts wrongly charge bypassed gates
**Status:** OPEN — latest playtest still reports ambiguous shortcut gates 6/9; see CR-050.
**Reported:** Dan received checkpoint penalties while using authored shortcuts despite explicit penalty-free bypass rules. Reproduce player-like imperfect entries/airborne travel/rejoins in actual Latest, confirm package source, log per-racer branch/gate/penalty state, and fix recognition and exit/abandonment handling without requiring a perfect centerline. Authorized bypass gates must never receive miss/cut charges or buzzes on a legitimate traversal. Local recovery must preserve earned branch context without allowing entry-touch giant-cut exploits.  
**Acceptance:** Repeated human-like runs of every shortcut/all profiles including recovery/reverse/edge/airborne cases finish with zero bypassed-gate charges; unrelated ordinary misses still work.
**Historical 0.6.1 result (superseded by Dan’s report):** 80/80 ordinary-frame route/road attempts, 48 verified branch exits and16 local recoveries: zero misses, charges or buzzes. Swept guards cover reversal, abandonment, entrance-touch exploits and shared AI rules. Exact old user incident was not reproduced; source recognition/context defects were corrected.
**Combined result:** Explicit entry entitlement survives deviations/recovery, relocated boundary gates, authoritative five-second ledger and aggregate notifications.96/96 physical route/road runs passed; unrelated real road misses still charge5s. Exact14-miss incident not reproduced; Dan review remains required.

### BUG-005 — Airborne or backwards-facing valid crossings denied
**Status:** IMPLEMENTED IN 0.6.1-review2 — integrated validation recorded; awaiting Dan's approval. See Docs/CR041-045/VALIDATION.md and AI-RACES.md for actual results and limitations.
**Reported:** Motorcycle can jump above the Jamerson checkpoint without credit; landing backwards-facing through a gate center is treated as a miss. Existing RaceDirector uses body heading as well as crossing direction. Determine valid forward passage from swept movement/route progress, not nose orientation or grounded state. Give supported intended jump trajectories an explicit suitable vertical gate envelope or landing/progress resolution; no globally unbounded gate volume. Keep physically wrong-direction crossings, repeated crossing, teleport and finish exploits rejected.  
**Acceptance:** Forward travel through/above a gate on an intended jump earns one credit even if airborne or facing backwards; truly wrong-way travel does not. Test all profiles/representative trajectories and compound shortcut crossings.
**Result:** 111/111 rule/dynamic checks and8/8 actual jump flights passed intended crossing credit. Direction uses movement; only CP14 has the24m upper envelope. A fast ATV genuinely missed later CP15 (+5), retained in evidence.

### BUG-006 — Smash audio not audible in play
**Status:** IMPLEMENTED IN 0.6.1-review2 — integrated validation recorded; awaiting Dan's approval. See Docs/CR041-045/VALIDATION.md and AI-RACES.md for actual results and limitations.
**Reported:** Dan heard no effects on impacts. Reproduce with persisted user volume settings and actual release without overwriting them; inspect event delivery, listener/routing, attenuation, cooldown/pooling, generation/loading and engine/ambience masking. Verify captured audible output and material differences, not merely an AudioSource call. No force-overriding mute.  
**Acceptance:** With relevant levels enabled, wood/chain-link/sign/mailbox/house-glass hits are clearly audible, balanced, repeatable and not clipped/spammed in a visible standalone player.
**Result:** Five materials physically hit at default/lowered/muted levels;15 DSP mix captures, no clipping, muted peak/RMS exactly zero. Gains/attenuation/voice handling improved. No OS-endpoint or subjective listening claim; Dan must review actual audible prominence.

### BUG-007 — Floating house over Fox Gully
**Status:** IMPLEMENTED IN 0.6.1-review2 — integrated validation recorded; awaiting Dan's approval. See Docs/CR041-045/VALIDATION.md and AI-RACES.md for actual results and limitations.
**Requested resolution:** CR-041 authorizes grounding and locally converting this specific house into a drive-through stunt. Other houses remain fixed. No unexplained floating slab or hidden solid collider across the route.
**Result:** Only the identified floating Fox Gully residence was adapted into the supported glass-entry/ramp/upper-window stunt. All12 final gully branch attempts crossed both panes. All46 unrelated building transforms unchanged.

### BUG-008 — Motorcycle loses momentum at gully-house jump
**Status:** IMPLEMENTED IN 0.6.2-review1 — awaiting Dan review; not accepted/closed.
**Reported:** Motorcycle slows again at the house jump, similar to the earlier ramp issue. Inspect actual input/velocity/contact normals/separation, panes/debris/restoration, internal floor seams, suspension/stability/wipeout and collision filtering. Re-test the prior scoped CCD fix rather than assume the same cause. No compensating launch boost or globally disabled collision.  
**Acceptance:** Repeated ordinary-frame runs retain appropriate momentum through glass/interior/lip and land safely within intended speeds; all profiles/class-contact rules remain functional. Report stage-by-stage speeds and failed cases.
**Combined result:** Reproduced entrance/lip losses with contact telemetry before edits. Corrected supporting normals and inset only the lower exit facade. Motorcycle lip17.90→29.53m/s without boost or tuning changes; repeated all-profile driving passes. See Docs/CR046-049/VALIDATION.md.

---

# CHANGE REQUESTS

Use this for things that are not bugs but that Dan wants changed.

### CR-001 — Larger neighborhood entrance hill
**Status:** Implemented and technically checked; accepted as part of Dan's overall Phase 2 track review.  
**Current behavior:** Initial hill is too small.  
**Requested change:** Make the initial climb substantially bigger and clearly noticeable from the driving view.  
**Reason:** Restore the remembered significant uphill entrance.  
**Phase affected:** Phase 2 only.  
**Result:** Implemented in the reproducible environment revision. See the per-item implementation/elevation/setback table and actual verification in `Docs/PHASE2_REVISION.md`. Dan subsequently accepted the revised track overall; no separate per-item retest claim is implied.

### CR-002 — Deeper, faster second drop
**Status:** Implemented and technically checked; accepted as part of Dan's overall Phase 2 track review.  
**Current behavior:** Second hill does not drop quickly or deeply enough.  
**Requested change:** Increase the vertical drop and shorten its descent distance while preserving driveability.  
**Reason:** Restore the distinctive steep sledding hill.  
**Phase affected:** Phase 2 only.  
**Result:** Implemented in the reproducible environment revision. See the per-item implementation/elevation/setback table and actual verification in `Docs/PHASE2_REVISION.md`. Dan subsequently accepted the revised track overall; no separate per-item retest claim is implied.

### CR-003 — Shorter, noticeable final hill
**Status:** Implemented and technically checked; accepted as part of Dan's overall Phase 2 track review.  
**Current behavior:** Last hill is drawn out and barely perceptible.  
**Requested change:** Shorten and strengthen this descent, keeping it gentler than the second major drop.  
**Reason:** Refine the earlier gradual-descent direction to match Dan's test feedback.  
**Phase affected:** Phase 2 only.  
**Result:** Implemented in the reproducible environment revision. See the per-item implementation/elevation/setback table and actual verification in `Docs/PHASE2_REVISION.md`. Dan subsequently accepted the revised track overall; no separate per-item retest claim is implied.

### CR-004 — More frequent rolling hills
**Status:** Implemented and technically checked; accepted as part of Dan's overall Phase 2 track review.  
**Current behavior:** Road feels too flat overall with too few hills.  
**Requested change:** Add more frequent, perceptible rolling crests and dips, especially throughout the neighborhood; other loop sections remain relatively flatter.  
**Reason:** Restore the remembered hilly character.  
**Phase affected:** Phase 2 only.  
**Result:** Implemented in the reproducible environment revision. See the per-item implementation/elevation/setback table and actual verification in `Docs/PHASE2_REVISION.md`. Dan subsequently accepted the revised track overall; no separate per-item retest claim is implied.

### CR-005 — House 3 setback and downhill placement
**Status:** Implemented and technically checked; accepted as part of Dan's overall Phase 2 track review.  
**Current behavior:** House 3 is too close to the road and lacks the requested steep downhill separation.  
**Requested change:** Place House 3 farther from the road and down a steep hill; reshape its supporting terrain.  
**Reason:** Match the remembered house placement.  
**Phase affected:** Phase 2 only.  
**Result:** Implemented in the reproducible environment revision. See the per-item implementation/elevation/setback table and actual verification in `Docs/PHASE2_REVISION.md`. Dan subsequently accepted the revised track overall; no separate per-item retest claim is implied.

### CR-006 — Friend's house setback and elevation
**Status:** Implemented and technically checked; accepted as part of Dan's overall Phase 2 track review.  
**Current behavior:** Friend's house needs to be closer to the road and slightly lower.  
**Requested change:** Move it closer to the road but slightly downhill, preserving its across-the-street location and blending its site into the slope.  
**Reason:** Match the remembered house placement.  
**Phase affected:** Phase 2 only.  
**Result:** Implemented in the reproducible environment revision. See the per-item implementation/elevation/setback table and actual verification in `Docs/PHASE2_REVISION.md`. Dan subsequently accepted the revised track overall; no separate per-item retest claim is implied.

### CR-007 — Additional random houses
**Status:** Implemented and technically checked; accepted as part of Dan's overall Phase 2 track review.  
**Current behavior:** Too few additional houses are present.  
**Requested change:** Add more approximate/random houses in sensible residential locations, including the connecting road and far neighborhood end; retain key landmarks and avoid newer subdivisions.  
**Reason:** Give the older residential environment the intended population.  
**Phase affected:** Phase 2 only.  
**Result:** Implemented in the reproducible environment revision. See the per-item implementation/elevation/setback table and actual verification in `Docs/PHASE2_REVISION.md`. Dan subsequently accepted the revised track overall; no separate per-item retest claim is implied.

### CR-008 — Businesses along both sides of main street
**Status:** Implemented and technically checked; accepted as part of Dan's overall Phase 2 track review.  
**Current behavior:** Current commercial coverage is insufficient.  
**Requested change:** Place simple businesses/stores along both sides of the main street throughout its relevant length, with plausible gaps/access and clear road space.  
**Reason:** Match Dan's remembered commercial character.  
**Phase affected:** Phase 2 only.  
**Result:** Implemented in the reproducible environment revision. See the per-item implementation/elevation/setback table and actual verification in `Docs/PHASE2_REVISION.md`. Dan subsequently accepted the revised track overall; no separate per-item retest claim is implied.

### CR-009 — Substantially denser forest
**Status:** Implemented and technically checked; accepted as part of Dan's overall Phase 2 track review.  
**Current behavior:** There are far too few trees for the remembered heavily forested area.  
**Requested change:** Substantially increase tree density/coverage, especially around the childhood houses and removed developments, using efficient placeholders and preserving road/shoulder clearance.  
**Reason:** Restore the heavily wooded older landscape.  
**Phase affected:** Phase 2 only.  
**Result:** Implemented in the reproducible environment revision. See the per-item implementation/elevation/setback table and actual verification in `Docs/PHASE2_REVISION.md`. Dan subsequently accepted the revised track overall; no separate per-item retest claim is implied.

### CR-010 — Slightly tighter steering during Phase 3
**Status:** ACCEPTED / CLOSED — Dan approved the Phase 4 follow-up together with the speed changes.
**Current behavior:** After Phase 3 testing, Dan barely noticed a handling difference. The car still feels a bit floaty, though not terrible. All other reported tests passed.  
**Requested change:** Make a modest steering-response/precision adjustment using existing tuning where possible. Preserve progressive input and stable high-speed handling; avoid a physics overhaul or unrelated system changes. Record before/after values and test typical bends, hairpin, hills, faster sections and reverse with keyboard/controller inputs as available.  
**Reason:** Improve driving feel without holding up the next phase.  
**Phase affected:** Initial adjustment in Phase 3; carry a small follow-up into Phase 4 planning. Phases 2 and 3 remain accepted.  
**Result:** StreetLoopGreybox car overrides: steering response 7→8/s; low-speed angle 32→33 degrees; high-speed angle remains 10 degrees. Other vehicle tuning/source is unchanged. Eighteen original/revised steering checks passed, including road bends, hairpin, hills, fast sections, reverse and fixed-input response. Original automated testing used virtual input only. Dan subsequently reported little perceived improvement and lingering mild floatiness; his input device was not specified. Revert those two instance values to 7 and 32 for comparison. See Docs/PHASE3_VALIDATION.md and Docs/CR010_TEST_RESULTS.txt.

**Phase 4 follow-up result:** Actual baseline was steering 8/s, angles 33°/10°, yaw 8/s, grip 7/s capped at 22 m/s². Revised yaw 9/s, grip 8.5/s capped at 25 m/s²; steering response/angles and suspension remain unchanged. Identical-road comparisons reduced lateral peak 0.51→0.41 m/s on bends, 1.23→1.02 at the hairpin, and 1.63→1.32 on hills. At 25 m/s, a 0.3 steering step raised 100 ms yaw 0.487→0.590 rad/s; a 4 m/s slip disturbance left 0.437→0.266 m/s after 0.3 s. Body-roll settling was already quick. This supports a yaw/grip refinement, not a conclusive diagnosis of Dan's subjective feel. Paired 35/38 m/s straights, 20 m/s bends/hills, 9 m/s hairpin and -8 m/s reverse passed. Speed changes also shift the existing angle interpolation. No jump-specific physics changes. See `Docs/PHASE4_ROADS.txt` and `Docs/PHASE4_VALIDATION.md`; Dan subsequently approved this follow-up.

### CR-011 — Faster car and representative racing-speed testing
**Status:** ACCEPTED / CLOSED — Dan approved the combined Phase 4 speed, acceleration and handling changes.
**Current behavior:** Dan wants more speed and considers slow-speed testing insufficient to judge driving feel.  
**Requested change:** Inspect current limits/tuning, then moderately increase forward top speed and acceleration. Start with roughly 15–20% more top speed and a modest acceleration increase; revise as needed for stability. Tune jointly with CR-010 and size the first jump for the resulting performance. Keep original/revised values for comparison or reversal.  
**Reason:** Evaluate and enjoy the car at representative racing speeds rather than relying mainly on conservative automated driving.  
**Phase affected:** Phase 4; Phases 2 and 3 remain accepted.  
**Validation:** Retain slow correctness checks, then test sustained racing-speed driving, near-top-speed straights, braking into corners and revised-speed jump approaches. Slow appropriately for tight turns. Report actual speeds reached, stability and track limitations; do not claim unperformed tests.  
**Acceptance:** Dan approves the combined speed and handling; steering, braking, landing/recovery and race systems remain predictable and functional.  
**Result:** Scene-instance speed parameter 38→44 m/s (+15.8%) and acceleration 12→13.5 m/s² (+12.5%). From rest on the same ~220 m stretch, reached 33.93→38.41 m/s. Revised main-road run reached 41.183 m/s (148.3 km/h) naturally, with 6.90 s above 40 m/s. Three actual continuous racing-speed laps passed: 552.36 s traversal, 25.47 m/s average, 39.71 m/s peak, all checkpoints/jumps valid. Braking 35→0.85 m/s stayed 1.40 s / 24.64 m. Existing reverse, camera, input and reset preserved; HUD adds speed. First jump recommended 110–120 km/h. Artificial high-speed stress tests are distinguished from naturally achieved speeds. Full values, landing limits, recovery and physical-controller limitations: `Docs/PHASE4_VALIDATION.md`.

### CR-012 — Correct neighboring house count and expand Dan's yard
**Status:** ACCEPTED / CLOSED — Dan says the building batch looks fine, including the house-count correction and expanded yard. Phases 2–5 remain accepted.
**Current behavior:** Earlier memory guidance included three neighboring houses labeled 1, 2 and 3.  
**Requested change:** Dan corrected his memory: only two neighboring houses. Keep Houses 2 and 3 with their current labels and placement; remove House 1 and expand Dan's childhood-house yard into that space. Preserve Dan's house, the friend's house across the street and the hairpin house. Remove House 1's dedicated colliders and obsolete house-specific props/access without removing shared assets or unrelated objects. Blend the former footprint into a continuous yard; do not replace it with another structure or road.  
**Reason:** Correct Dan's remembered landscape. Latest correction supersedes older map legends, prompts and preservation instructions referencing the three neighboring houses.  
**Phase affected:** Phase 6 focused building batch; prior phase acceptance remains intact.  
**Acceptance:** House 1 and its invisible collision are gone; Houses 2 and 3 are unchanged and not renumbered; Dan's house stays in place with a visibly larger, naturally blended yard. Roads/shoulders and accepted gameplay remain unaffected. Relevant generation logic preserves the correction. Dan reviews the result.  
**Result:** Confirmed map marker (1028,951) and scene `Original house 1` at (415.8,82.44644,-1.1); removed its full hierarchy and 3 dedicated colliders. Expanded Dan's yard by clearing 15 local trees/colliders and blending 3,069 terrain color entries across 3 tiles, with zero terrain position/normal/index changes. All 47 surviving site transforms and names match baseline, including Houses 2/3. 1,579 yard support/obstacle probes and two virtual PhysX yard traversals passed. Architecture is approximate. Two foundation bottoms extend locally (House 2 by 0.071m; one unrelated retained residence by 0.359m) without moving floors/buildings. Legacy generators omit House 1 and reserve the yard; full rebuild is guarded and was not run. See `Docs/PHASE6_VALIDATION.md` for images, generation workflow, collision details, tests and technical limitations. Dan subsequently approved the visual result.

### CR-013 — More accurate homes from future reference photos
**Status:** BACKLOG / OPTIONAL — await reference photos if Dan finds and supplies them; not a blocker to current phases.  
**Current behavior:** Dan approves the current building pass as visually fine. Architectural details remain approximations.  
**Requested change:** When Dan supplies photos, identify which home each depicts and use visible evidence to refine its overall form, roof, facade, windows, entrances and materials. Prioritize the photographed homes Dan identifies. Confirm any unclear house identity or era before applying changes; do not infer unseen details as fact. Preserve remembered older features if modern photographs differ, using Dan's guidance.  
**Scope:** A future focused visual-accuracy pass, tentatively under Phase 8. Do not collect photos, rebuild homes or pause current progress for this backlog item. Preserve the accepted two-neighbor arrangement (Houses 2/3), expanded yard, building placement/elevation, road support and gameplay unless Dan specifically requests a correction.  
**Acceptance:** Compare revised homes with the supplied references and obtain Dan's approval; retain simple reliable collision and reasonable performance.  
**Result:** Not started; no new home reference photos supplied in this request.

### CR-014 — More extensively wooded neighborhood with drivable forest
**Status:** IMPLEMENTED — Dan reports coverage is much better; retain it. Explicit driveability/smoothness approval remains unconfirmed, so CR-014 is not marked fully closed. Phase 6 remains incomplete; CR-015 and the roadside batch below are implemented and awaiting visual approval.  
**Baseline behavior:** The visual refresh kept 6,102 placements and added no trees. Dan says the neighborhood should be much more forested because much of it was woodland; he wants to drive through it and is concerned about frame rate.  
**Requested change:** Substantially increase the area covered by connected woodland and improve its dense wooded appearance, not simply crown detail or uniform tree count. Keep navigable spaces between solid trunks, supported ground and ordinary off-road access. Avoid making the woods an impassable wall, a visual backdrop, or a set of narrow prescribed corridors. Natural obstacles and slopes may remain; not every gap must fit the car. Use canopy coverage and restrained non-colliding understory where useful.  
**Preserve:** Expanded yard/former House 1 site, Houses 2/3, all accepted building sites, road/shoulder support, existing shortcut/jump clearances, vehicle tuning and race systems. Free off-road traversal does not authorize new named shortcuts or relaxed checkpoint validation.  
**Performance:** Establish a comparable baseline, then profile increased coverage in stages. Measure ordinary-frame road and forest driving, multiple representative camera locations, frame-time spikes, CPU/GPU bottlenecks and memory where available. Use representative window/resolution settings and record them; the prior 734x293 samples and variable host timings do not establish performance at Dan's normal settings. Choose optimizations from measured bottlenecks; keep visual/collision consistency and safe collision availability during driving. Record actual results, hardware/settings and limits rather than promising a frame-rate target not yet supplied.  
**Acceptance:** Dan judges the neighborhood substantially more wooded, can explore between trees and rejoin roads predictably, and finds performance acceptable. Compare before/after coverage and measured smoothness; test accepted gameplay and yard protections.  
**Result:** Added 5,795 trees in two measured stages (8,999 then 11,897 total), retaining all original 6,102. Expanded central removed-subdivision land, eastern woods and southern woodland. Actual projected canopy coverage central/east/south rose 10.77/8.97/9.18% to 43.76/45.55/40.74%. New trunks are 0.75m wide with minimum 6.8m center spacing; mature overlapping crowns preserve protected clearances. Kept 90 shared-material spatial batches; forest triangles 416,952 to 810,312. Fixed stale GPU mesh buffers in the vegetation refresh. Every visible trunk aligns with solid collision; all accepted non-forest scene records unchanged. Three ordinary-frame slow forest drives each covered about 150m, plus contact/reverse/turn/reset, forest-road crossing, ordinary-frame jump/shortcut and existing race regressions. Repeated 1440x900 standalone measurements show a modest rendering cost with substantial host variation and unresolved spikes; player allocation approximately 253 to 292MiB. No universal frame-rate target, parity or speedup claimed. See Docs/CR014/VALIDATION.md for matched views, raw tests, settings, performance and limitations.

### CR-015 — Refine the remembered house placements
**Status:** IMPLEMENTED / FURTHER REVISION REQUIRED for Dan's house and yard under CR-016. Mailboxes/signs visually accepted. No new acceptance of House #2/#3 inferred; keep their current placements during CR-016. Phase 6 remains incomplete.  
**Requested change:** Move Dan's childhood house only slightly farther back from the road. Move House #2 toward Dan's house. House #2 should be directly across the street from the friend's house. Move House #3 toward House #2 along the neighborhood, placing it just before the big downhill drop while retaining its low valley setting. Keep House #1 absent and do not renumber Houses #2/#3.  
**Placement guidance:** Use the current scene and map to identify the houses and the second-circle/big drop. Treat directions relative to the approach from the main-road neighborhood entrance. Make conservative local changes, document offsets and provide an annotated overview. Keep the friend's house as the reference rather than moving it to manufacture alignment. Update foundations, related access/props, local tree clearances and generation exclusions as necessary; preserve the enlarged yard and broad new woodland coverage. These requested offsets supersede prior exact-position preservation rules only for the affected houses.  
**Preserve:** Accepted building designs, House #3's valley elevation character, roads/hill profile, terrain support, accepted vehicle tuning, jump, shortcut and race systems. Do not run a legacy full rebuild.  
**Acceptance:** Dan's setback change is small; House #2 is closer to Dan's house; across-street alignment matches the clarified reference; House #3 sits toward House #2 before the big drop and remains in the valley. All buildings are grounded with matching collision and clear local access. Dan reviews before/after views.  
**Result:** Dan moves (-2.219,-0.016,-2.019)m, exactly +3m horizontal setback. House #2 moves (-21.491,+0.272,+55.783)m toward Dan onto the perpendicular through the fixed friend's road projection; across-street error <0.001m and original 58.367m centerline setback retained. House #3 moves (-11,0,+60)m to (418,32.755,-160); nearest road Z=-115.047 lies before the big descent at Z=-132, and the home remains 53.071m below the crest road. Designs/rotations, House #1 removal, expanded yard and unrelated landmarks remain intact. One terrain tile locally extends the valley (1,824 vertices; max lowering 42.881m; no road/shoulder edits within 18m); foundations/steps regrounded. Nine conflicting original trees removed, 16 surviving local trunks regrounded; 11,888 trees retained. Four reusable mailboxes and two simple signs have no small collision snag surfaces. Authored placement JSON and generation exclusions updated; stale building GPU buffers fixed. No legacy full rebuild. All placement/foundation/seam/yard checks pass, four local access drives and road runs complete upright, three mixed laps and race/jump/shortcut regressions pass within prior angled-jump limits. Final compile succeeds, saved scene reloads cleanly, Console 0 errors/0 warnings. Editor timings remain variable with spikes up to 253ms; no physical-controller or new standalone-performance claim. Annotated views, offsets, assets, exact tests, caveats and review checklist: `Docs/CR015/VALIDATION.md`. CR-015 and roadside batch await Dan's visual review.

### CR-016 — Move Dan's house south and substantially shrink its yard
**Status:** ACCEPTED FOR NOW / CLOSED — Dan approved the integrated batch. House placement is not exact, but further adjustment is deferred to optional CR-018 and must not block progress.  
**Requested change:** Move Dan's childhood house much farther south, near but not at the southernmost point of the current yard. Reduce the yard to approximately one quarter of its current ground area and restore trees across the released area. Interpret one quarter as area, not one quarter of both dimensions. Capture the current yard outline and establish map south before editing; do not infer compass direction solely from a Unity axis.  
**Implementation guidance:** Make a compact natural yard around the relocated home with a modest boundary margin and usable road access. Preserve accepted architecture and orientation unless necessary local grounding requires adjustment. Blend foundations/steps and access naturally; avoid large terrain excavation. Update yard/house authoring data, tree-placement exclusions and render/collision batches so the previous oversized clearing is not recreated. Reforest with the current efficient, drivable woodland style; keep the remaining yard and access clear. Relocate only this house's associated mailbox/access if needed.  
**Supersedes:** Earlier small-setback-only direction and enlarged-yard preservation for Dan's site. House #1 remains absent; keep House #2, House #3 and the friend's house in their current positions, without inferring their explicit approval from this feedback. Preserve roads, hill profiles, accepted gameplay and unrelated clearances.  
**Acceptance:** Annotated before/after overview shows north, old/new house location, old/new yard footprints and approximate ground areas/ratio. House sits near the old yard's southern end, yard is roughly 25% of its previous area, released land is wooded and traversable between trunks, and the house remains grounded with usable access. Dan reviews the result.  
**Result:** House moves (349.7807,74.9703,103.5814) to (415.8,83.8196,-13), 116.581m south and 15.1m inside the old southern edge. Accepted design/rotation retained; foundation/steps regrounded without terrain height edits. Yard horizontal area 9003.472→2250.869m² (25.000%); sampled sloped surface 9070.2→2281.4m² (~25.15%). Narrow road access retained and Dan's mailbox relocated. Added 107 solid trunks, minimum center spacing 6.803m; all 11888 prior trees retained. Only two local forest batches change visible geometry; 88 remain exact. Placement JSON, compact-yard/access exclusions and crown limits updated. House #1 absent; all 46 other buildings fixed; all 100 terrain meshes retain heights/normals/topology. Annotated before/after views and full tests: Docs/CR016-017/VALIDATION.md. Local virtual-input drives cover 119.8m of new woodland and access both ways; upright >=0.979. Access follower stopping overshoot 6–7m and a 1.7s Editor stall are disclosed. Not physical-controller or subjective approval.

### CR-017 — Breakable mailboxes, signs and lightweight fences
**Status:** ACCEPTED / CLOSED — Dan approved the integrated house/yard, fences and breakable-prop batch. Approval does not imply unreported device/test coverage.  
**Requested change:** Preserve accepted mailbox/sign appearance and make lightweight roadside scenery yield on vehicle impact, with bounded cleanup and predictable restart behavior. Dan authorized this together with the house/yard correction and fences.  
**Result:** Reusable `BreakableProp` on four mailboxes, two bend signs, the jump sign, two shortcut direction signs and nine timber fence sections. Fences sit beside Dan's west yard edge (~390,-1 XZ), House #2 (~408,-39) and the friend's property (~516,-54); approaches remain open. Simple box triggers, 0.2m/s threshold, original assembly knocked aside once per cycle; no solid solver impulse, cloned fragments or debris rigidbodies. Maximum 24 moving assemblies; four-second cleanup. Race restart first resets car/progress, then restores props, deferring any original bounds overlapping a vehicle. Scene reload restores authored state. Ordinary vehicle reset leaves destruction unchanged and preserves existing race invalidation. Vehicle tuning, camera, HUD and checkpoint logic remain unchanged.  
**Validation:** 20 ordinary-frame virtual Gamepad cases at 3/30m/s, centered/glancing across five prop types; all broke once and remained upright. Consecutive three-fence drive passed. Eight 18-prop contact bursts, five repeated race restarts, cleanup and overlap deferral passed. 35-piece stress capped at 24, retired oldest 11, created no rigidbodies and cleaned fully. Three mixed laps and race/input/HUD/reset regressions passed; jump/bypass checks retain prior angled-jump limitations. Same-view destruction profiling: intact median/p95 11.39/18.87ms; 30 bursts 10.05/19.29ms, max115.2ms, one rigidbody. No controlled-host speedup or physical-controller claim. Exact locations, raw checks, before/after views and review checklist: Docs/CR016-017/VALIDATION.md.

### CR-018 — Optional later refinement of Dan's house position
**Status:** BACKLOG / NON-BLOCKING — current house/yard accepted for now.  
**Requested change:** Dan says the house is not perfectly placed but explicitly does not want to worry about it now. Revisit only when he supplies further placement guidance or asks to resume this item; optionally coordinate with CR-013 photo references.  
**Scope:** Do not move the house, change its yard or reopen CR-016 during current Phase 6 work. No assumed new coordinates or automatic relocation.  
**Result:** Deferred; no new placement instructions supplied.

### CR-019 — Remove mirrored commercial layout and uniform spacing
**Status:** ACCEPTED / CLOSED — Dan reports the first Phase 8 commercial-street batch passed.  
**Reported behavior:** Dan sees the same stores directly opposite one another, with identical spacing on both sides of the main street. Repeated stores are acceptable; mirrored pairs and uniform placement are not.  
**Requested change:** Vary store order and positions independently on each side. Stagger opposing buildings along the road, use varied realistic gaps and modest setbacks, and avoid matching storefronts directly opposite each other or repeating the same spacing sequence. Reuse existing stores and retain the current commercial coverage/count where feasible; no need to make every store unique. Use intentional, reproducible layout rather than randomizing every load.  
**Coordination:** Move building visuals, colliders, foundations, shop text/signs, frontage paving and access as coherent sites. Update authored placement and relevant visual batching/generation so refreshes preserve the arrangement. Preserve road/hill geometry, driveable shoulders, woodland access, houses/yards, jump/shortcut, breakable behavior and accepted vehicle/race/UI/save systems. No legacy whole-environment rebuild.  
**Acceptance:** Before/after overhead views and drives in both directions show no obvious mirrored store pairs or identical spacing rhythm. Buildings remain grounded, separated and accessible, and repeated assets are distributed naturally. Performance and local clearance remain acceptable; Dan reviews the result.  
**Result:** All 22 existing sites retained, 11 per side. West-to-east South: M-A-D-M-G-A-D-M-G-A-D; North: D-M-G-A-D-M-G-A-D-M-A (Market/Diner/Auto/General). Independent center spacing ~47–121m South, ~50–117m North; road-normal setbacks 29–34m. Matching opposite footprint intervals separated by at least 58.60m; smallest same-row footprint gap 21.40m. Sites move as complete prefab/sign/collider assemblies, with refitted foundations/steps, relocated flush paving and gravel access and muted existing-palette material overrides. Nine conflicting trunks removed; remaining 11,986 trunks preserved, no terrain-height edits. Explicit slot tables, stable site IDs, shared generation rules and synchronized visual/frontage refresh prevent mirrored regeneration; legacy whole-environment rebuild not run.  
**Actual validation:** Save/reload and repeat apply pass with identical site inventory and zero repeat paint changes; batched vertices match sources. Both ordinary-frame virtual-Gamepad drives pass at ~32m/s with braking to ~12.7m/s, >11m shoulder excursion/re-entry and zero non-terrain contacts. Six final countdown/pause/restart smoke checks pass using isolated test storage. Houses, terrain shape/markings and breakable placements unchanged. Matched 734x293 Editor sample, VSync1/cap60: before median/p95 16.74/20.21ms, after 16.74/20.83ms; not standalone or 1080p parity. Compilation succeeds; final Console 0 errors/warnings after historical/tool messages archived. Full settings, recovered test/tool failures, limitations and before/after evidence: `Docs/CR019/VALIDATION.md`, `Docs/CR019/review.html`. Dan subsequently reported this batch passed; the remaining Phase 8 delivery is implemented and awaiting review in Docs/Phase8/VALIDATION.md.

### CR-020 — Missing vehicle audio
**Status:** ACCEPTED FOR NOW / CLOSED — Dan reports the sounds are working. No exhaustive listening/device coverage inferred; additional checkpoint feedback is tracked separately as CR-022.  
**Requested change:** Add coherent engine idle/acceleration/coasting/reverse audio, tire slip and road/off-road rolling feedback, plus restrained impact/landing/prop feedback. Use existing vehicle state; audio-only simulated RPM/gears are allowed without changing approved physics. Avoid constant squeal or repeated impact sounds. Preserve pause/countdown/reset/results audio behavior and integrate a persistent vehicle-volume setting with master mute. Use redistributable licensed or original assets, document sources and quality limits.  
**Acceptance:** Dan can hear responsive car behavior without clipping, abrupt loop seams, excessive repetition or ambience/UI masking. Virtual technical tests do not establish subjective sound quality.  
**Result:** Six bounded voices: layered engine idle/load with smoothed audio-only revs, reverse load, actual lateral-slip tires, vertex-color road/off-road rolling, shared cooldown for impacts/landings/props. Physics/tuning unchanged. Legacy settings default Vehicle Volume to 75%. Audio checks 24/24 and flow regression 37/37 pass in Editor using virtual input and isolated saves. Details: Docs/CR020-021/VALIDATION.md.

### CR-021 — Shareable solo Windows package
**Status:** PACKAGED, USER TEST DEFERRED — Windows x64 solo 0.2.0-review1. Dan cannot test the download yet; retain existing technical evidence and limitations. Not a blocker to further development.
**Requested change:** Produce a clean standalone Windows build and versioned ZIP containing all required runtime files, controls/readme and audio/asset license notices. No Unity Editor needed to play. Do not send only the executable. Exclude repository/source, development back-up/debug artifacts and personal/test saves; preserve every required runtime dependency. Launch an extracted copy outside the project and validate core flow and offline solo use.  
**Distribution:** Prepare the archive locally. Do not upload, publish, email or send it without an explicit destination/request. Include size, checksum and version/commit information. Other operating systems require their own builds.  
**Result:** Builds/Racer-0.2.0-review1-Windows.zip, 76,284,958 bytes; SHA-256 A2124C11721AC816B60957C1AAF94D05FFB91857C3ED5997423736CF7E4D8303. Source 72f037ea9d5f80a527e5172c6bb27f6a9e82bfed. Non-development build succeeds, complete runtime/licenses/README included; 228 extracted files verified. Visible launch, mouse menus/countdown and volume persistence pass outside project with isolated saves. Keyboard automation did not reliably reach release; extracted driving/pause/restart/results and subjective audition remain manual review. See Docs/CR020-021/VALIDATION.md.

### CR-022 — Checkpoint success and missed-checkpoint audio
**Status:** IMPLEMENTED — integrated 0.3.0-review1; validated locally, awaiting Dan's review.
**Requested change:** Play a short pleasant ding when the next required checkpoint is validly credited, and a distinct brief buzz when a checkpoint is missed. Route both through existing race/UI volume and master mute, not Vehicle Volume. Match existing audio style with original or redistributable assets.  
**Rules (original request; CR-025 supersedes invalidation/recovery preservation where needed):** Inspect current checkpoint progression and invalidation. Trigger the ding exactly once from authoritative race progress, not every collider contact. For misses, use existing invalidation where appropriate and inspect whether a gate can be physically bypassed without an immediate error. If needed, add feedback-only route-aware detection when the player passes beyond the expected checkpoint without credit, respecting road direction/curves and valid shortcut paths. Do not make wall-clock timeout or a simple world-axis comparison the definition of a miss. Do not alter gate geometry, accepted race validity, timing or recovery rules merely for sound.
**Edge cases:** Avoid repeated buzzing each frame, ping-pong gate crossings, wrong-way/repeated-gate success dings, paused/countdown/results sounds and false misses after reset/restart/teleport. Rearm cues with appropriate race/lap/expected-gate state. If a missed gate can still be recovered under existing rules, preserve recovery. Coordinate finish/lap cues to avoid overlapping success sounds.  
**Acceptance:** Correct next gate produces one audible ding; skipping the expected gate produces a prompt single buzz for that missed-gate episode. Normal, shortcut and jump routes remain valid without false buzzes. Pause/reset/restart and mute/settings behave correctly, with bounded audio sources and no record/settings corruption. Dan reviews volume and clarity.  
**Result:** Authoritative player ding and missed-gate buzz implemented with a 0.5s buzz cooldown, Race/UI volume and master mute. AI/traffic do not play player cues. Countdown/pause/reset/restart suppression and finish coordination verified; subjective sound review remains with Dan. See Docs/CR022-025/VALIDATION.md.

### CR-023 — First AI opponents and light traffic
**Status:** IMPLEMENTED / REVISION REQUESTED — CR-026 adds clearly visible grid starts and genuinely competitive difficulty; Dan does not yet see meaningful AI competition.
**Scope:** Start with three AI race opponents and a small configurable traffic population (initially about four vehicles, reduce if congestion/performance requires it). Reuse existing vehicle assets with readable visual differences. Race AI follows the existing circuit with per-car progress, recoverable driving, safe start positions, player-relative standings and finish results. Traffic follows simple two-way lanes at sensible speeds and never participates in race progress. Support a traffic toggle; retain the approved solo/time-trial experience with AI disabled and keep record categories separate when rules differ.  
**Driving:** Slow for bends/hairpin/hills, avoid other cars and static obstacles, brake/yield, and recover from stalls without teleporting onto the player or granting progress. Do not hardcode world-axis lane directions or require frame-by-frame teleporting. Preserve player physics and race validity. Initially let AI racers use the normal road/bypass; shortcut and jump remain available to the player. No new roads, full city simulation, police, public networking or adaptive rubber-banding.  
**Integration:** Audit single-player assumptions in race progress, lap gates, reset, camera/audio, HUD, saves and breakable-prop detection before adding cars. Each racer gets independent valid progress; traffic cannot credit a race. Preserve countdown/pause/restart/results semantics and bound pooled traffic/recovery behavior. Keep vehicle audio and collision cost controlled.  
**Acceptance:** Complete races with three opponents, sensible position updates and finish results; traffic traverses both directions without persistent blockages. Test density/congestion/performance, safe recoveries and reset/restart, independent gate credit, passing/collisions and CR-022 cues. Technical runs are not Dan's acceptance.  
**Result:** Three named/tinted prototype-car opponents, shared GO clock, independent progress and adjusted standings; four two-way traffic cars (hard cap six). Road-relative lanes, bend/hill braking, cautious passing and safe recovery. Ready/results buttons retain Solo and traffic On/Off. Two Editor one-lap races: all six AI finishes, zero recoveries/misses. Visible default three-lap Windows race also completed with all three AI finishing, zero recoveries/misses: 9/9 AI finishes across all three races. See Docs/CR022-025/VALIDATION.md.

### CR-024 — Further player-car speed increase
**Status:** IMPLEMENTED — integrated 0.3.0-review1; validated locally, awaiting Dan's review.
**Requested change:** Make the player car faster again. Inspect actual approved speed/acceleration first (last reported 44m/s and 13.5m/s²). Try a further roughly 10–15% forward-speed increase with a modest acceleration improvement; treat as provisional tuning, not a required unstable target. Preserve controllable steering/braking, validate hills/corners/jump/traffic at actual achieved speeds and report before/after values. AI opponents should remain reasonably competitive without hidden speed cheats.  
**Acceptance:** Dan approves increased pace and controllability; accepted race/AI/traffic/jump/recovery still function. Do not silently retune unrelated physics or move accepted scenery.  
**Result:** Scene tuning changed from 44 to 49m/s (+11.36%) and acceleration 13.5 to 14.5m/s² (+7.41%); braking remains 24m/s². Steering/grip/suspension/camera unchanged. Virtual Gamepad reached 45.60m/s; early braking and a 40m/s corner-entry fixture stayed upright. Jump fixtures at 32.76/39.52m/s landed upright. Late-braking failure and shoulder excursions are retained in the evidence. Dan retains handling approval. See Docs/CR022-025/VALIDATION.md.

### CR-025 — Missed-checkpoint time penalties instead of forced restart
**Status:** IMPLEMENTED — integrated 0.3.0-review1; validated locally, awaiting Dan's review.
**Requested change:** A missed expected checkpoint produces one buzz, a clear HUD penalty notification and a time penalty; the racer continues toward the next checkpoint without restarting the lap/race. Use configurable per-gate penalties, initially around five seconds for an ordinary miss, increased where measured shortcut savings demand it. Each genuinely skipped gate is charged exactly once. Normal, approved shortcut/jump paths receive no penalty.  
**Rules:** Use route-aware detection and distinguish misses from pause, intentional reversing, stationary play, reset/teleport and harmless off-road driving. Preserve existing manual reset semantics unless explicitly required and documented; do not interpret teleport as many missed gates. Wrong-way/repeated crossings cannot award free laps. Require normal forward start/finish crossing to complete laps; multi-gate cuts must incur all relevant penalties and cannot create a faster winning exploit. Existing unlimited giant-cut prevention must be reconciled with penalty continuation, not silently disabled.  
**Timing/results:** Keep driving elapsed time and accumulated penalties separate; show adjusted total and penalty breakdown. Apply equivalent missed-gate rules to AI racers. Track position is physical valid/penalized route progress, not a guaranteed final ranking: standings/results must communicate penalty-adjusted order, wait for remaining racers only within a bounded finish policy, and distinguish DNF/provisional places rather than declaring the earliest physical finisher automatically the winner.  
**Records:** Version/save records by speed/rules/mode so changed car performance and penalty rules do not corrupt old personal bests. Penalized laps may qualify only in the appropriate new category using adjusted time. Preserve old records, including aborted-race valid-lap policy where compatible.  
**Acceptance:** Missing one/multiple gates gives one clear cue per miss episode, accurate per-gate penalty and continued race progress; no forced restart due solely to missing a checkpoint. Repeated crossings, deliberate cuts, reset/restart/pause and AI events cannot duplicate/avoid penalties or spam player audio. New results and saved records use correct adjusted times. Dan reviews penalty feel.  
**Result:** Misses continue racing: base 5s per gate plus max(0, sector length minus verified new forward road travel minus 25m)/10 for cuts. Road-relative direction/progress resolves gates 18m beyond them or at a later forward gate. Ordinary misses cost 5/10s; large-cut replay costs 482.36s. Proper forward finish and mid-course witness prevent line abuse. Manual reset abandons the current lap, retaining race clock/penalties; occupied pad waits. Adjusted standings use bounded finish/DNF policy and provisional display. Versioned speed/rules/mode/traffic/lap records preserve old records/settings. See Docs/CR022-025/VALIDATION.md.

### CR-026 — Visible starting grid and competitive AI difficulty
**Status:** REVISION REQUIRED — Dan still finds all difficulty levels too easy; CR-036 defines the new Easy/Normal/Hard targets. Prior completion/timing results remain evidence, not human acceptance.
**0.5.0 evidence:** Per-profile corner/braking/crest planning and relative-velocity obstacle response; Easy/Normal/Hard use identical underlying player capabilities. Full three-lap mixed Windows races measure actual pace, braking, misses and recoveries. Initial aggressive tuning and finished-vehicle rollback failures retained; corrected finish parking passed repeated full races:12/12 racers finished,0DNFs; Normal ATV2misses and Hard ATV1recovery remain disclosed. Human competitiveness still requires Dan.
**Feedback:** Dan knows AI is present but does not see opponents and wants nearby competition, a proper shared grid and skill levels. Current reports say AI copies player tuning; that is not proof of visible or competitive human racing.  
**Requested change:** Diagnose actual starting placement/rendering/mode selection and AI pace. Build a readable safe grid with opponents visible during countdown. Add Easy/Normal/Hard based mainly on braking, cornering, consistency and racecraft; equal-class AI gets the same underlying vehicle capabilities as the player. Do not secretly boost speed or teleport to keep up. Show selected mode/difficulty clearly. Benchmark full races and visible interaction, not just completion counts.  
**Integration:** AI behavior follows chosen vehicle profiles; compare like-for-like where possible and make class differences explicit. Keep three opponents initially; do not require AI riding bikes/ATVs for the first roster if that compromises reliability. Records must distinguish relevant vehicle/class/difficulty rules.  
**Result:** Visible forward grid, Easy/Normal/Hard driver judgment, three car opponents and same-capability car profiles implemented. Final Windows one-lap series: 12/12 racer finishes, zero misses/recoveries. AI ranges Easy 2:48.779-2:52.827, Normal 2:34.826-2:40.887, Hard 2:30.138-2:35.188; reference player 2:54.931/2:42.734/2:36.938. Initial Normal player DNF fixed by finish runoff and retained as evidence. See Docs/CR026-027/VALIDATION.md for final evidence and review limitations.

### CR-027 — Vehicle garage with cars, motorcycle and four-wheeler
**Status:** IMPLEMENTED / AWAITING DAN APPROVAL — combined0.5.0 revision addresses BUG-002/003 and CR-028–033. The0.4.0 results below remain historical; use Docs/CR028-033/VALIDATION.md for current behavior and coverage.
**First roster:** Preserve original car; add one distinctly different car, one motorcycle and one four-wheeler/ATV (four selectable vehicles total). Reusable profiles and garage flow allow later expansion without making the first delivery unbounded.  
**Requested behavior:** Motorcycle and ATV are faster and tighter/more responsive than the baseline car, with more risk. Cars can shove and destabilize them; bikes/ATVs cannot meaningfully shove cars or act as battering rams. This is deliberate arcade asymmetric vehicle contact, not a claim of realistic physics. Specify/test both initiator directions, glancing/sustained contact, speeds and reset cases. Do not obtain asymmetry by making small vehicles intangible or globally disabling collisions.  
**Driving:** Distinct grounded handling appropriate to two/four wheels, visible steering/rolling and restrained bike lean; no requirement for a ragdoll or full motorbike simulation. Use a simple rider silhouette for readability. Bike/ATV share the small-vehicle contact disadvantage; bike can be less stable than the ATV. More risk must not mean ordinary normal-speed turns randomly fail. Clear wipeout/reset feedback using accepted race-reset semantics, safe spawn/grid, per-vehicle camera and audio defaults, and ground/jump/reverse-or-low-speed-maneuver behavior that is explicit. Do not reduce existing car quality to make new vehicles feel better.  
**Selection:** Preview, names/classes, intelligible speed/handling/stability/contact-strength differences, persistent selection, keyboard/mouse/controller navigation. Apply selection before races with safe countdown/restart; preserve selected-vehicle-specific resets and player input/audio ownership. Separate compatible records by stable vehicle/profile identity and game rules without deleting legacy records.  
**Preserve:** Current map/props/forest, accepted game flow, penalties/cues, traffic options, audio volumes/settings and downloadable build workflow. No networking or paid assets.  
**Acceptance:** All four vehicles are selectable and distinct; bike/ATV beat baseline car in straight-speed and controlled responsiveness comparisons, while car-vs-small-vehicle contact visibly favors the car without explosive physics or penetration. Full race, grid/AI, traffic/collision, jump, reset, pause/restart/results, records and package validation with honest test limits. Dan approves feel and competition.  
**Result:** Four stable profiles, pre-race garage/preview, persistent selection, profile/rules/mode/difficulty records, wheels/lean, class contact rules and per-vehicle camera/audio defaults implemented. Bike +24.5% straight speed / +54.4% turn response; ATV +14.5% / +51.0% versus unchanged baseline. Solid asymmetric contact: 60/60 fixtures pass, maximum overlap 3mm, no measured car shove. All four road suites remain upright; ATV fast jump runs wide and bike hill pursuit slows sharply. Final standalone flow 24/24 passes; isolated records/migration and ownership verified. See Docs/CR026-027/VALIDATION.md for measurements, limitations and local package.

### CR-028 — Quit race without quitting the game
**Status:** IMPLEMENTED / AWAITING DAN APPROVAL — combined revision validated and locally packaged.
**0.5.0 evidence:** Pause now has Quit Race / Return to Menu, distinct from Restart Race and Quit Game. Abandons event/AI/grid/audio, returns Ready, preserves settings and legitimate records, awards no incomplete race. Virtual keyboard/mouse/gamepad and countdown/racing quit/new-race checks pass.
**Requested change:** Pause menu needs Quit Race / Return to Menu. Abandon the active event and return to Ready/garage without exiting the app. Stop AI/race/audio state safely, preserve saved records/settings and do not award an incomplete race record. Keep Quit Game distinct; navigation and new-race setup remain functional.  
**Result:** Implemented in the combined0.5.0 review; current evidence and limits: Docs/CR028-033/VALIDATION.md. Awaiting Dan's review.

### CR-029 — Local wipeout recovery without lap loss
**Status:** IMPLEMENTED / AWAITING DAN APPROVAL — combined revision validated and locally packaged.
**0.5.0 evidence:** R/Y rights in place or chooses clear supported local candidates/history within60m, avoiding overlap and predicted moving traffic. Preserves clock, penalties, completed laps/current earned progress; rejects forward road/gate credit. No recovery charge or lap abandonment. Unsafe fallback waits locally and retries; never silently START. All four profiles tested upright/flipped/tree/slope/moving-car and finish-plane abuse.
**Requested change:** R/Y should right the vehicle at its current supported position, or the closest safe local position if blocked/unsupported. Clear unstable motion and protect against overlap with traffic, scenery or other racers. Never send ordinary wipeouts to START or silently abandon current lap. Preserve elapsed clock, completed/current valid gate progress and existing penalties; add no reset penalty or checkpoint-miss chain solely for recovery. Do not move forward across unearned gates or finish credit. Wipeout itself and elapsed time are the punishment. Race Restart remains the explicit full restart. Update HUD/rules/save compatibility for this deliberate supersession.  
**Result:** Implemented in the combined0.5.0 review; current evidence and limits: Docs/CR028-033/VALIDATION.md. Awaiting Dan's review.

### CR-030 — Compact driving HUD
**Status:** IMPLEMENTED / AWAITING DAN APPROVAL — combined revision validated and locally packaged.
**0.5.0 evidence:** Compact upper-left elapsed lap/race stopwatch plus lap/position fractions; lower-right km/h. Temporary penalty notices; adjusted details in Pause/Results. No persistent debug/instructions. Solo1/1 and1024x768,1280x720,1680x720 screenshots/checks; garage/results retained.
**Requested change:** Replace the oversized driving overlay with a small readable speedometer plus current-lap and race elapsed/adjusted timing, position as rank/field fraction and lap as current/total fraction. Include speed units and make penalties understandable using compact temporary feedback/details in pause/results; do not leave debug tables, long help text or persistent banners across the road view. Respect safe areas/aspect ratios and use concise labels or icons; solo mode sensible. Keep garage/results readable separately.  
**Result:** Implemented in the combined0.5.0 review; current evidence and limits: Docs/CR028-033/VALIDATION.md. Awaiting Dan's review.

### CR-031 — Bigger usable jumps
**Status:** IMPLEMENTED / AWAITING DAN APPROVAL — combined revision validated and locally packaged.
**0.5.0 evidence:** After CCD diagnosis/fix, principal ramp changed from24m/3m to smooth40m/6.2m rise; supported wide landing, original bypass/gates and scenery retained. Eight measured standalone jumps: entries35.68–41.74m/s, takeoffs34.93–40.88m/s, flights2.46–2.86s, landings83.57–111.05m beyond lip, all upright. No launch impulse/speed boost. Dan still judges feel.
**Requested change:** After fixing BUG-002, enlarge the existing principal jump for a clearly bigger and satisfying flight, especially motorcycle/ATV. Tune smooth takeoff geometry, flight/landing clearance and supported landing zone for all four vehicles at documented speeds; preserve the bypass and gate validity. No hidden launch impulse or automatic speed injection to mask braking. Favor one well-tested larger jump over many untested ramps.  
**Result:** Implemented in the combined0.5.0 review; current evidence and limits: Docs/CR028-033/VALIDATION.md. Awaiting Dan's review.

### CR-032 — Selectable and mixed AI vehicle roster
**Status:** IMPLEMENTED / AWAITING DAN APPROVAL — combined revision validated and locally packaged.
**0.5.0 evidence:** Three configurable real-profile opponent slots plus Random/Mixed, visible resolved roster, explicit reroll and stable restart/rematch. Real motorcycle/ATV motors and collision vulnerability; independent progress/penalties/results. Solo and traffic-off retained. New record category includes resolved roster.
**Requested change:** Pre-race opponent slots let Dan choose from all four profiles, or choose Random/Mixed. Resolve and show random roster before GO, retain it for restart/rematch unless rerolled. AI motorcycle/ATV must genuinely drive their class, not car visuals reskinned. Tune per-profile road/traffic/jump-bypass control and collision vulnerability; safe profile-sized grid, independent audio/progress and bounded recovery. Remove previous car-only limitation. Include vehicle roster/rules in record categories where fairness requires.  
**Result:** Implemented in the combined0.5.0 review; current evidence and limits: Docs/CR028-033/VALIDATION.md. Awaiting Dan's review.

### CR-033 — Player vehicle color selection
**Status:** IMPLEMENTED / AWAITING DAN APPROVAL — combined revision validated and locally packaged.
**0.5.0 evidence:** Six body swatches for all four profiles; immediate preview and persistent per-profile choice. MaterialPropertyBlock isolates body panels from trim/tires/riders/shared assets/opponents. Keyboard/mouse/virtual-gamepad access and persistence checks; physical controller remains untested.
**Requested change:** Add simple body-color swatches in garage for all four profiles with immediate preview, persistent per-profile color and identical appearance in race/results. Keep trim/tires/rider materials separate, avoid changing shared material assets or AI colors unintentionally, keep source/batched rendering consistent and use keyboard/mouse/controller navigation.  
**Result:** Implemented in the combined0.5.0 review; current evidence and limits: Docs/CR028-033/VALIDATION.md. Awaiting Dan's review.

### CR-034 — Black vehicle paint
**Status:** IMPLEMENTED / AWAITING DAN REVIEW — combined 0.6.0-review1; technical evidence is not subjective acceptance.
**Requested change:** Add a clearly black paint swatch, especially for the player's car. Integrate with existing per-profile preview, selection and persistence; permit black body paint on all supported vehicles without tinting tires, trim, riders, glass or shared opponent materials. Preserve readable highlights so black still has shape.  
**Result:** Seven stable swatches including Black; per-profile persistence and isolated body-panel properties. All four body/trim fixtures and visible garage/race-flow checks pass. See Docs/CR034-039/VALIDATION.md and flow screenshots.

### CR-035 — Satisfying destruction audio and remembered property fences
**Status:** 0.6.1 CORRECTION DELIVERED — awaiting Dan's playtest; see BUG-004–007 and CR-041–045. Earlier technical passes remain historical evidence.
**Requested change:** Improve impact/break sounds so smashing lightweight objects is satisfying, with material-appropriate wood crack/splinter, metal/chain-link rattle/clatter and relevant sign/mailbox feedback. Strength should respond to impact intensity with bounded overlapping voices, variation and cooldowns. Add chain-link fencing around appropriate portions of Dan's childhood property and white wooden crossbuck fencing (large X shapes) for Houses #2/#3. Treat placement/extents as approximate; keep driveways/access open and do not move houses/yards. Add additional selected smashable props along suitable approaches, not everywhere. Use existing yielding/breakable architecture, not a heavyweight destruction simulator.  
**Preserve:** Local reset, bounded debris, safe vehicle-class contact, master/vehicle/UI volume semantics, race-restart restoration and traversable woodland. Use original or redistributable audio with documented licenses.  
**Acceptance:** Visible and audible break reactions agree, sounds are punchy without clipping or repetitive chatter, and repeated impacts/restoration remain performant and cannot trap/launch vehicles. Dan reviews the sound and fence appearance.  
**Result:** Original material-specific wood/chain-link/mailbox/sign synthesis, three timbres/five pitches, four spatial voices, 75ms onset cooldown and prewarming. Chain-link at Dan's property; white large-X side/front fences at Houses 2/3; central access remains open. Material overrides are saved on prefab instances. Existing yielding/24-piece cleanup/restoration preserved; 33/33 combined audio/highway checks pass. No third-party samples. Sound satisfaction awaits Dan.

### CR-036 — Player-defined difficulty balance
**Status:** ACCEPTABLE FOR NOW / REVISIT AFTER MORE PLAY — Dan may reassess difficulty; no AI pace retuning in the next delivery.
**Requested change:** Easy should be comfortably winnable with ordinary competent driving. Normal should be somewhat challenging with a good chance to win. Hard should punish mistakes through lost position/time and require a strong run. Dan finds all current levels too easy. Tune skill/pace per vehicle using actual route/profile constraints, not just beating a conservative automated reference driver. Preserve fair profile capabilities, visible grid, mixed/selected roster and absence of hidden speed boosts/teleport catch-up.  
**Validation:** Compare comparable clean runs and deliberate error/recovery runs; report lap/sector times, gaps, excessive braking, AI errors and traffic influence across all vehicle classes. Human difficulty remains Dan's judgment. Optional authored shortcuts should be usable by qualified AI with difficulty-appropriate choice, with safe main-road fallback; distinguish racecraft from unfair physics.  
**Result:** Normal/Hard driver pace strengthened with unchanged player motors/capabilities and Easy constants. Both20-traffic three-lap races finished4/4 with zero misses/recoveries. All profiles improved best laps, but Hard tourer/motorcycle total races slowed in congestion. Exact laps, sectors, gaps, braking and Easy/mistake results are in Docs/CR041-045/AI-RACES.md; no human difficulty acceptance claimed.

**Latest difficulty feedback:** Hard feels improved, but Dan requests more pace in BOTH Normal and Hard. Preserve Easy's forgiving goal; tune fair profile-specific driving and compare actual lap/sector gaps under increased highway traffic. Do not claim benchmark superiority proves human competition.

### CR-037 — Four-lane Hwy 92 and faster highway traffic
**Status:** 0.6.1 CORRECTION DELIVERED — awaiting Dan's playtest; see BUG-004–007 and CR-041–045. Earlier technical passes remain historical evidence.
**Requested change:** Make the existing main-road/Hwy 92 corridor a broad four-lane road (two lanes in each direction). Preserve its general route and continuity with the neighborhood roads; add supported width, appropriate markings/shoulders and clear lane transitions. Traffic should normally travel faster here than on South Cherokee Lane and Jamerson Rd, subject to curves, junctions, spacing and braking. Do not use a global traffic speed increase.  
**Coordination:** Inspect physical carriageway widths and route identity before editing. Preserve buildings and their accepted non-mirrored order where possible; move commercial frontage/sites only minimally if needed to avoid the wider carriageway and document offsets. Keep houses/yards fixed. Update road/terrain/render/collision, lane graphs, AI obstacle queries/passing/grid as affected, frontage access, race gate span and route-progress detection together. No false misses, lane discontinuities, floating edges or restored mirrored shops. No extra highway segments or recreation of present-day subdivisions.  
**Result:** Northern Hwy92 now16.4m asphalt/four4.1m lanes with supported shoulders,70m transitions, markings, lane-specific traffic/passing and widened gate spans. Traffic local ceiling17→29m/s; four lane/direction cases traversed both transitions with27.40–28.85m/s peaks and no jams. All house/shop transforms unchanged; no commercial offsets required. No global traffic-speed increase.

### CR-038 — Smashable road-name signs
**Status:** IMPLEMENTED / AWAITING DAN REVIEW — combined 0.6.0-review1; technical evidence is not subjective acceptance.
**Requested change:** Add legible road-name signs using exactly Hwy 92, South Cherokee Lane and Jamerson Rd. Identify the existing main/neighborhood/connecting road mapping from authored references; if a junction/segment mapping is ambiguous, resolve it rather than invent a different road. Place signs sensibly at relevant junctions and use the accepted breakable/restore system. Signs may break but route identity must not depend on an intact physical sign. Preserve access and sightlines.  
**Result:** Two-sided breakable junction signs use exactly Hwy 92, South Cherokee Lane and Jamerson Rd. Reference mapping: northern main road/eastern neighborhood/southern connector. Lettering fitted within boards and visually checked; original restoration/clearance retained. See road-sign.png.

### CR-039 — Varied woodland stunt shortcuts
**Status:** 0.6.1 CORRECTION DELIVERED — awaiting Dan's playtest; see BUG-004–007 and CR-041–045. Earlier technical passes remain historical evidence.
**Direction:** Dan wants San Francisco Rush-inspired shortcuts: challenging, discoverable, varied and fun, with large jumps. Fun takes precedence over strict geographic realism. Start with THREE authored routes in the existing footprint, retaining the existing shortcut: a creek/stream leap, a gully traversal, and a distinct ridge/woodland jump route. Use original designs/art, not copied courses.  
**Requirements:** Each route has a readable entrance, different driving challenge, supported drivable ground, well-tested approach/landing/rejoin and a meaningful risk/reward versus the main road. Preserve dense woods outside the necessary corridor; car-width routes still demand more precision than roads. Bike/ATV should excel without making cars categorically unusable. Fit creek/gully terrain locally without moving accepted homes, damaging the highway or breaking terrain seams; simple water treatment with explicit recovery rules, not a large water simulation.  
**Race integration (latest clarification):** Designated shortcuts MAY bypass multiple existing checkpoints without missed-gate penalties, cut charges or warning buzzes. Their time saving is the reward; failure already costs time through driving/recovery, so do not add an arbitrary shortcut penalty. Author entry/progress/exit tracking and an explicit bypassed-gate list for each route; suspend those gate penalties during legitimate branch travel, resolve appropriate progress on verified exit/rejoin and continue normal race tracking. A shortcut need not fit between adjacent existing gates. Normal penalties remain for unrelated course cuts. Entry contact alone must not authorize skipping the course; retain forward finish/lap validity and consistent branch-aware standings. Define abandonment/reversing/partial rejoin and local-reset behavior without retroactive penalty chains for gates legitimately bypassed. Local recovery stays on safe nearby route support without unearned forward progress. Apply the same rules to AI. Compare common entry/exit timings across multiple clean and failed attempts, with no false gate warnings on legal routes. This supersedes older requirements to pass every original required gate or confine shortcuts between existing gates.  
**Performance/authoring:** Reproducible focused route/terrain/forest edits, bounded geometry/collision/audio costs, no legacy whole-world rebuild. Refresh affected generation data. Reuse route definitions later without rebuilding the race architecture now.  
**Acceptance:** Three distinct fun paths, clean legal runs save useful time or offer an explicitly explained reward, forgiving local recovery, all profiles tested, no false penalty/position behavior, unchanged core race/records/UI, measured before/after performance. Dan reviews fun/difficulty.  
**Result:** Creek Leap900→1500m bypassesCP4–6; Fox Gully1580→2150 bypassesCP7–9; Pine Ridge2220→3000 bypassesCP10–12. Existing southwest cut retained. Per-racer directed entry, contiguous horizontal evidence, supported shoulder/airborne envelope, branch standings, backward safe recovery and verified exit grant only earned gate credit. Clean branches incur no miss/cut/buzz penalties; unrelated cuts/early abandonment retain ordinary rules. All four vehicles have repeated clean passes;77/77 rule fixtures include complete laps, reversing, off-route/recovery and teleport/finish abuse.60 comparison attempts plus retained failed iterations documented in ROUTE-TIMINGS.md. Gully's conservative gain is modest; fun remains Dan's review.26 local terrain tiles/17,060 vertices,115 corridor trees; no house/yard moves or legacy rebuild.

### CR-040 — Lake/woodland circuit behind friend's house
**Status:** REVISION REQUIRED — Dan reports seven substantial design/physics issues in 0.7.0-review1; CR-056 supersedes the all-four-vehicles requirement for this forest course.
**Concept:** A selectable second circuit starting near the friend's house/lake, passing Dan's house and continuing through the woods with varied large jumps, gullies and creek crossings. Fun above geographic realism. Author the lake and route as needed without moving accepted homes/yards or breaking the original course. Reuse suitable existing terrain/routes but make this a distinct playable race. All four vehicles and AI must complete it; include readable approaches, supported landings, sensible local recovery and optional risk/reward shortcuts with explicit penalty-free bypass credit. Gates must be clear of shortcut mouths/landing zones. Preserve existing difficulty tuning while authoring suitable AI lines/speeds for this course.
**Acceptance:** Track selection, starting grid, AI race completion, independent gate/lap/finish state, recovery, results and records work on both tracks. Inactive-course gates do not trigger. Compare performance and test all vehicle profiles, shortcuts and multi-lap races in the standalone build. No track editor, multiplayer or exact house relocation.
**Result:** Separate selectable 1.91 km Lake & Woods scene; accepted houses and original scene preserved. Seven active gates, independent course version, three supported jumps/creeks and explicit Birch Hollow CP03 bypass. Both courses completed two laps with all four profiles/AI and zero misses; 24 lake route attempts and 12 jump runs passed. See Docs/CR040-054-055/VALIDATION.md for the failed first candidate, fixes and limits.
### CR-041 — Grounded drive-through gully house
**Status:** IMPLEMENTED IN 0.6.1-review2 — integrated validation recorded; awaiting Dan's approval. See Docs/CR041-045/VALIDATION.md and AI-RACES.md for actual results and limitations.
**Requested change:** Identify the specific floating house over Fox Gully. Ground/support it and build a deliberately fun stunt: enter through breakable sliding glass doors, follow a visibly supported interior rise/ramp to the second floor and launch through a breakable second-story window to a tested landing/rejoin. Make the two-story path physically legible without teleport/hidden launch boost; preserve route legality/local recovery. Allow all four vehicles with adequate clearance. Only this house/site may be adapted or repositioned as needed; document identity and before/after changes. This is not a reopening of unrelated home-placement/photo backlog.  
**Result:** 44m supported interior rise,20m clear openings, sliding entry glass and upper exit glass; no launch boost/teleport. Initial narrow-aperture and embedded-crossbeam failures retained. Final all-profile normal/varied/interior-recovery attempts passed with both panes broken and zero charges/buzzes.

### CR-042 — Predictable five-second checkpoint penalties
**Status:** IMPLEMENTED IN 0.6.1-review2 — integrated validation recorded; awaiting Dan's approval. See Docs/CR041-045/VALIDATION.md and AI-RACES.md for actual results and limitations.
**Requested change:** Genuine ordinary missed gates cost exactly 5 seconds each; designated legal shortcut bypasses cost zero. Remove the hidden distance surcharge currently added by RaceDirector.ResolveMisses: ordinaryPenalty + max(0, sector - verifiedTravel - 25)/cutPenaltyMetresPerSecond. Multiple real misses total clear multiples of five; adjusted elapsed totals can still have fractional race seconds. Keep route-order/finish/anti-teleport validation rather than covertly adding fractional charges. Any broad-cut tradeoff must be explicit; do not impose a new hidden punishment or force normal missed-gate restarts. Version rules/records and preserve legacy results.  
**Result:** Each genuine miss exactly+5s; authorized bypass zero; no distance surcharge. street-v6-flat5 record category preserves historical files/settings. Broad cuts may save more than their flat missed-gate cost; ordered progress, bounded movement and finish checks remain.

### CR-043 — Surround properties with remembered fences
**Status:** IMPLEMENTED IN 0.6.1-review2 — integrated validation recorded; awaiting Dan's approval. See Docs/CR041-045/VALIDATION.md and AI-RACES.md for actual results and limitations.
**Requested change:** Dan wants perimeter fencing around the houses, with the front run near the street: chain-link for his home and white large-X wooden crossbuck fences for Houses #2/#3. Add side/rear continuity, leaving sensible driveway/pedestrian access and shoulder sightlines. Keep house positions/yards except explicit CR-041 stunt site. Near-street means appropriate setback, not within the live lane/re-entry corridor. Retain breakability, clear collision and restart restoration.  
**Result:** 169 breakable perimeter sections:48 chain-link at Dan,121 white-X at Houses2/3. Front/side/rear runs,12m front access and6m woodland openings; front setback11m from road centre. Other house positions unchanged. Repeated physical material hits and restoration checks passed.

### CR-044 — Much busier Hwy 92 traffic
**Status:** IMPLEMENTED IN 0.6.1-review2 — integrated validation recorded; awaiting Dan's approval. See Docs/CR041-045/VALIDATION.md and AI-RACES.md for actual results and limitations.
**Requested change:** Increase highway-specific population/coverage substantially above the sparse current scene; fill all four lanes with believable gaps and varied vehicle spacing, avoiding a convoy or a jam. Preserve faster highway vs local-road speed behavior; keep lower density elsewhere. Use bounded scalable spawning/pooling, safe following/lane transitions, no near-player pop-in and configurable density. Determine counts from measured congestion/performance and report them, not simply speed up four cars.  
**Result:** 16 dedicated highway plus4 local cars. Full Normal/Hard traces averaged11.49/11.68 highway cars and21.91/22.30 passes/minute; all four lanes represented.294 recycles, minimum player distance220m, zero traffic recoveries. Safe recycling permits temporary density dips. Measured core same-lane minimum gaps26.45/24.52m; detailed limits in AI-RACES.md.

### CR-045 — Make the Jamerson jump part of believable roadside scenery
**Status:** IMPLEMENTED IN 0.6.1-review2 — integrated validation recorded; awaiting Dan's approval. See Docs/CR041-045/VALIDATION.md and AI-RACES.md for actual results and limitations.
**Requested change:** Replace the arbitrary freestanding ramp with a readable environmental launch feature on Jamerson: e.g. roadworks with a supported raised pavement/graded approach. Choose one coherent original design, explain it and preserve a bypass. It should appear to belong to the road environment while still offering a large satisfying intentional gameplay jump. Coordinate collision/approach/landing/checkpoint envelope and all-profile tests, including fast motorcycle flights. No magic boost or new unrelated course segment.  
**Result:** Jamerson raised-pavement roadworks with supported takeoff, graded shoulder, breakable markers and open-right bypass.8/8 flights landed and credited CP14; fast motorcycle51.224m/s,17.430m apex,3.263s airtime. Fast ATV later CP15 miss retained as genuine+5.

### CR-046 — Reliable latched shortcut credit and complete penalty accounting
**Status:** FOLLOW-UP REQUIRED — gate 6/9 placement and bypass behavior remain unresolved; see CR-050.
**Requested change:** BUG-004 remains unresolved in human play: Dan saw about six notices but accumulated fourteen misses; exact causes not yet known. Relocate ambiguous approach/exit gates clearly before shortcut entrances and clearly after rejoins, with spacing for speed/flight. Once a legitimate shortcut entry is earned, latch its explicit bypass entitlement across lateral deviations, falls, stopping, reversing and local recovery until verified exit or deliberate main-road rejoin/abandonment. Do not require perfect path adherence or retroactively charge authorized bypassed gates. Distinguish earned protected gates from unrelated later-course skipping; finish/anti-teleport validity remains. Audit every mutation of player miss/penalty totals; no silent charges with suppressed notification. Add compact +5 or aggregated Nx+5 notices and a pause/results ledger containing lap, gate, cause, amount and branch context; ledger/count/sum must reconcile. Log position/time/route transitions for optional reproducible review, not a permanent debug HUD. True misses exactly5s, authorized bypasses0. Test human-like excursions, close gate ordering, loading/reset/restart and all vehicles. Prior fixtures are not proof of resolving Dan's incident.
**Combined result:** Latched explicit bypass entitlement, completion/abandonment transitions, five checkpoint relocations, single5s charge path, traceable ledger and unsuppressed aggregate charge notices. street-v7-entitlement records preserve history. Automated/physical evidence and remaining limits: Docs/CR046-049/VALIDATION.md.

### CR-047 — More audible tire squeal
**Status:** IMPLEMENTED IN 0.6.2-review1 — awaiting Dan review; not accepted/closed.
**Requested change:** Increase useful tire-slip/braking feedback for actual sliding and hard cornering. Inspect slip/ground/surface thresholds, mixing and attenuation; no constant squeal on every ordinary turn or while airborne. Apply appropriate road/off-road distinctions and per-profile behavior; preserve vehicle/master volume and radio balance. Verify audible output under engine/music, not just trigger counts.
**Combined result:** Progressive grounded slip/braking squeal, loose-ground scrub and small-profile pitch distinction. Actual tire/engine/radio DSP captured without clipping; Master/Vehicle preserved. Subjective listening awaits Dan.

### CR-048 — Decorative road continuations beyond circuit turns
**Status:** IMPLEMENTED IN 0.6.2-review1 — awaiting Dan review; not accepted/closed.
**Requested change:** Extend each relevant existing road beyond the loop-turn junctions so Hwy92, South Cherokee Lane and Jamerson Rd read as roads into a wider area. These are scenery/traffic connections, not a new playable course. Clearly communicate and physically enforce a player boundary that cannot be smashed or jumped through during supported play, while traffic can enter/leave without blockage. Use a visually coherent closure/controlled access or separate traffic passage; no cars visibly phasing through a solid closed wall. Preserve lane continuity, supported terrain, safe recycling and frame budget. Prevent following traffic out, local reset bypass and false gates/penalties. Keep houses/yards and playable race loop fixed; inspect all junctions rather than blindly extending every polyline corner.
**Combined result:** Four supported terrain-following continuations at the actual Hwy92/South Cherokee/Jamerson junctions; player closures and separate beyond-closure traffic with covered recycling. Flying/recovery boundary checks and existing-loop checks pass; houses/yards unchanged.

### CR-049 — Personal local-music radio
**Status:** IMPLEMENTED IN 0.6.2-review1 — awaiting Dan review; not accepted/closed.
**Requested change:** Add persistent user-owned music library via Settings with a default writable Music folder and Open Folder/Rescan actions, plus choosing another folder. Players drag files into that folder in Explorer; literal OS file-drop into game is optional only if reliable. No Unity reimport/rebuild for new songs. Support and test common DRM-free local audio formats (prioritize MP3, WAV and Ogg; document verified limitations). Read artist/title tags where available with a maintained license-compatible metadata reader; use safe filename fallback, never claim AudioClip.name is artist metadata. Shuffle without immediate repeats where possible, next/previous history, play/pause or on/off, brief artist-title toast on track change and on demand. Gameplay-only D-pad Right=next, Left=previous, Up=show track, Down=toggle; inspect bindings and preserve D-pad menu navigation, with keyboard/settings alternatives. Add independent persistent Music volume under Master; keep race/smash/tire cues audible. Bounded async loading/streaming/cache, safe corrupt/missing/removed files and empty library, no main-thread stalls or full-library decoding. Define pause/menu/restart/quit behavior consistently. No network, DRM service integration, uploads, original-file edits, or Git inclusion of user music/local library manifests. CR-053 now authorizes packaging explicitly staged bundled music; custom collections remain excluded. Use generated or redistributable test clips/tags; ship library instructions; include deliberately staged bundle tracks as specified in CR-053. Metadata displayed as text, not interpreted markup.
**Combined result:** Local-folder MP3/WAV/Ogg radio with ATL metadata, shuffle/history, compact title toast, gameplay D-pad and keyboard bindings, folder/open/rescan/chooser/settings actions, persisted independent Music volume/radio preference. Bounded async local-only reads; empty/corrupt/missing/removed cases tested. Native picker, physical controller and subjective listening remain Dan checks. Setup: Docs/CR046-049/RADIO.md.

---

### CR-050 — Resolve shortcut boundary gates 6 and 9
**Status:** IMPLEMENTED IN 0.6.3-review1 — awaiting Dan review; technical checks and limits in Docs/CR050-053/VALIDATION.md.
**Feedback:** Gate 6 remains awkward; gate 9 is immediately after an exit, appears outside shortcut credit, and is extremely close to gate 10. Verify displayed numbers versus internal IDs before editing.
**Requested change:** Audit the physical shortcut entries, landing/rejoin envelopes and bypass lists together. Move gate 6 to an unambiguous approach/rejoin position appropriate to its route. Consolidate redundant gate 9 with gate 10 or move it far enough downstream to give meaningful recovery/steering time; choose from actual geometry, not arbitrary offsets. Align route entitlements, next-gate HUD, ledger, AI progression, lap completion and record versioning. No mandatory gate at a shortcut mouth/landing and no penalties for its authorized bypass. Preserve imperfect-driving/recovery credit and exactly +5s for true misses. Show before/after gate/route evidence and ordinary-frame tests with all profiles.
**Acceptance:** Both named gates are understandable at speed; shortcut exits have room to land/rejoin; no double or hidden charges, road route and multi-lap races still work. Await Dan review.

### CR-051 — Traffic vehicle and color variety
**Status:** IMPLEMENTED IN 0.6.3-review1 — awaiting Dan review; technical checks and limits in Docs/CR050-053/VALIDATION.md.
**Requested change:** Replace identical ambient traffic with a visibly mixed pool of at least four recognizable body types (for example sedan, wagon/SUV, pickup and van) and varied plausible paint colors. Existing assets/simple art are acceptable; body variety must be visible beyond recoloring one mesh. Weighted selection should avoid conspicuous identical convoys without forcing a repeating pattern. Keep a vehicle appearance stable while visible; recycling may assign a new combination offscreen. Fit colliders/lane clearance and pooling to each body. Apply to highway/local/decorative continuation traffic; preserve racer selections, traffic density/flow, and performance.
**Acceptance:** Representative standalone drives visibly show different bodies/colors on both roads without overlaps, color popping or new congestion.

### CR-052 — Recursive music collection selection
**Status:** IMPLEMENTED IN 0.6.3-review1 — awaiting Dan review; technical checks and limits in Docs/CR050-053/VALIDATION.md.
**Requested change:** Selecting a collection root or artist folder includes supported files in all nested artist/album folders by default. Provide an Include subfolders setting and visible scan status/counts. Rescan discovers additions/removals; shuffle spans the discovered collection rather than favoring the first album. Keep metadata/filename fallback and current controls. Scan asynchronously with cancellation, bounded metadata/cache work, graceful inaccessible/corrupt files, no junction/symlink traversal loops or duplicate discovery, and no full-library audio decode. Review the existing 2048-top-level-file limit for large collections; disclose any cap and skipped count instead of silently truncating. Persist root and recursion preference. Do not broaden supported codecs without implementing/testing them.
**Acceptance:** Select a synthetic root with several artists/albums, an artist alone and a flat folder; all expected supported tracks are discoverable/playable across levels, including a large-library fixture, without driving stalls.

### CR-053 — Portable bundled Music folder and repeatable packaging
**Status:** IMPLEMENTED IN 0.6.3-review1 — awaiting Dan review; technical checks and limits in Docs/CR050-053/VALIDATION.md.
**Requested change:** Dan wants a Music folder alongside the game where he can place selected tracks for inclusion in his personal friend ZIP. Add explicit Bundled music / Custom folder source selection. Resolve the bundled location relative to the installed game, not the developer machine or working directory. Scan recursively. Document the stable source folder Dan populates and the packaging command/button that copies only that folder to the versioned build, complete Latest runtime and ZIP. Preserve this folder through rebuilds and protect tracks added to Latest from silent deletion; define safe migration/preservation before replacing Latest. Updating a ZIP after adding songs must not require recompiling Unity.
**Boundaries:** Authorization covers packaging songs Dan deliberately places in the bundle folder, not copying his entire selected custom collection or uploading anything. Personal audio stays out of Git; scripts, ignore rules and instructions may be committed. No original metadata/file edits. Keep external custom collections supported and unbundled unless Dan explicitly stages chosen tracks. Historical no-music packages remain historical evidence.
**Acceptance:** Extract the ZIP elsewhere and play nested bundled tracks without an absolute machine-specific path. Repackage/rebuild and verify staged songs survive. Verify only staged bundle files are included, custom library contents are excluded, and an empty bundle works. Use generated test audio; if Dan has not populated songs, report that rather than substituting a personal collection.

### CR-054 — Top ten lap times and total race times
**Status:** IMPLEMENTED IN 0.7.0-review1 — automated validation recorded; awaiting Dan review.
**Requested change:** Persist two distinct local top-10 boards: fastest eligible completed laps and fastest eligible completed total races. Multiple different runs by Dan may occupy the lists; this is not limited to one entry per player. Sort ascending using full timing precision with stable ties; show rank, time, vehicle and date, plus penalty details where applicable. Results should flag a new personal best/top-10 entry; provide a compact Records menu with Lap/Race tabs and relevant category selectors.
**Rules:** Use the existing authoritative race timer and eligibility rules. Race records use full completed race elapsed time plus penalties exactly once; lap records use lap time plus penalties attributable to that lap exactly once. Avoid mixing timer values that already include penalties with a second addition. Authorized shortcut bypasses remain zero. Associate delayed penalties with the correct lap and finalize records only when its accounting is resolved. Quit/restart/incomplete races never create total-race entries; eligible completed laps may remain. Prevent duplicate inserts when results reopen or saves reload, while retaining distinct attempts with tied times. Local recovery follows existing eligibility and adds no extra punishment.
**Comparability:** Key boards by track/course rules version and existing relevant vehicle/profile, difficulty and roster categories; total-race boards additionally distinguish lap count. Do not mix unequal race lengths or obsolete course layouts. Label the selected category clearly. Preserve old records/saves; migrate valid known bests once, retaining historical categories. Do not invent missing old timing/penalty metadata; keep incompatible legacy records in history.
**Acceptance:** More than ten eligible attempts produce the correct fastest ten in each board, persist through restart, remain isolated between tracks/configurations, and reconcile with penalties. Test ties, repeated results/save loads, faster/slower attempts, incomplete races, lap attribution, and legacy migration. No online leaderboard required.
**Result:** Separate persistent Lap/Race top-ten boards, full-precision stable ties, attempt-based deduplication, authoritative finalized penalties and category isolation. Known v8 bests migrate once with unknown dates; historical files preserved. 29/29 standalone checks; rendered boards inspected. Main menu or Results > Records / Top 10. See Docs/CR040-054-055/VALIDATION.md.

### Historical feature options — superseded by CR-075–081 approval
- Stunt challenges on the existing map: jump distance/airtime, smash targets and optional medals.
- Personal-best ghost time trial: race a saved best run to learn faster lines.
This was the earlier scope boundary. Stunt challenges are now implemented under CR-079 awaiting review; ghosts and photo-informed home/placement work are approved and queued under CR-081. Difficulty retuning remains outside this delivery.
---

### CR-055 — Default bundled music and folder-named radio channels
**Status:** IMPLEMENTED IN 0.7.0-review1 — automated validation recorded; awaiting Dan review.
**Requested change:** Use the portable Music folder beside the installed game as the default music source on a fresh setup; retain explicit custom-folder selection and saved preferences. Each immediate child folder is a channel named after that folder. Recursively include its artist/album subfolders within the same channel, rather than turning every nested folder into a channel. Supported tracks directly in the Music root form a General channel when present. Apply the same channel interpretation when a custom collection root is explicitly selected. Empty/unplayable channels are skipped; no music means a clear Off/empty state.
**Controls:** Replace the gameplay radio toggle with one cyclic action: channel 1 -> channel 2 -> ... -> Off -> channel 1. Use the existing D-pad Down and keyboard M bindings unless inspection reveals a conflict. Keep D-pad Left/Right for previous/next song within the current channel and Up for current channel/artist/title; preserve menu navigation. Expose equivalent Settings controls. Show a compact channel/track toast on switch and an Off confirmation. Order channels predictably; shuffle tracks within each channel without immediate repeats when possible. Persist chosen source/channel/off state and independent music volume. Rescans, renamed/removed folders and rapid switching must not cause stale audio, overlap, hangs or errors.
**Packaging:** Preserve CR-053 staging in BundleMusic, recursive portable Music output, and Package-Racer.cmd repackaging without Unity compilation. Preserve folder names/hierarchy in ZIP and future builds. Never automatically copy custom collections; only explicitly staged tracks ship. Personal audio stays out of Git and nothing is uploaded by this task.
**Acceptance:** Fresh extracted build defaults to bundled Music; generated channel folders with nested albums play under the correct folder-named channel. One repeated button action visits each playable channel and Off reliably, including zero/one/multiple-channel cases. Verify track skip/history stay in-channel, recursive playback, rapid switches, rescans/removal, persistence, volumes and gameplay/menu input isolation. All three unfinished/new deliverables remain awaiting Dan review after implementation.
**Result:** Portable bundled source on fresh preferences, folder-named channels with nested albums, General root channel, one channel/Off cycle, independent shuffle/history and saved source/channel/volume. 16/16 generated-fixture radio checks and 4/4 extracted portable/DSP checks passed. Existing BundleMusic preserved; Package-Racer.cmd stages only deliberate music, preserves prior complete runtimes/ZIP and needs no Unity compilation for song changes. See Docs/CR040-054-055/RADIO.md.

---

### CR-056 — Forest Loop arcade trail redesign
**Status:** POSITIVE HUMAN REVIEW — Dan says much better; retain Forest Loop direction and address CR-057 through CR-059. This is not blanket closure of all individual checks.
**Latest feedback:** Dan likes the overall progress but reports an unclear/missing lake, wrong start location, recurring jump slowdowns, an easy straight shortcut, too few jumps, civilian traffic offroad and paths that feel like wide roads.
**Scope:**
- Rename the second course Forest Loop across track selection, HUD/results, records labels and documentation; preserve internal save identifiers or migrate safely.
- Position the start behind the existing friend's house by a clearly visible lake. Inspect whether prior lake geometry exists but is hidden/misplaced before replacing it. Provide an unmistakable shoreline/water feature visible from the grid and opening route; preserve house placement and original course.
- Audit every launch/landing and trail transition. Diagnose speed losses with input, speed, contact and suspension/stability telemetry, correct abrupt ramp tangents/seams/overlaps and collision defects, and retain intentional physical effects. Do not mask defects with hidden launch boosts or globally disabled collisions. Recheck the original house/road jump regressions.
- Replace the wide straight shortcut with a genuine optional cave route: rock enclosure, readable tight bends, a jump/gap and supported landing. It must reward skilled clean traversal, cost time naturally on failure, remain recoverable, and preserve penalty-free authorized gate bypasses. Avoid blind unavoidable impacts, narrow snagging or checkpoint requirements inside flight zones.
- Make jumps central to the whole lap. Author at least six distinct intentional main-route jump opportunities, including a visible creek/river crossing, deep gully leap, varied elevation/drop and linked smaller jumps, plus the cave jump. Treat this as a design target, not six copies of one ramp. Supply a labeled route map with each takeoff/landing and compare total jump count to the prior build. Arcade spectacle and variety over straight travel; credible visible terrain support and readable lines still required.
- Restrict civilian traffic to actual street routes, including spawns, waypoints, avoidance and recovery. Only race opponents may drive forest/cave trails. Street crossings can retain legitimate road traffic. Wildlife is an optional future ambience idea, not required for this pass and not a substitute for requested work.
- Narrow trails into dense forest paths with varied bends/elevation, close trees/undergrowth/rocks, dirt/ruts and no suburban-road appearance. Use motorcycle/ATV-only eligibility for Forest Loop, as Dan permits, to avoid widening trails for cars. Match usable width to their swept clearance at bends/speed; allow deliberate passing pockets and grid space rather than an impassable single-file corridor. Apply eligibility consistently to player selection, AI roster, saved selections and records with clear UI feedback. Original street course retains all vehicles.
**Validation:** Motorcycle/ATV human-like driving and AI full races through the revised route/cave; every jump tested at intended speeds plus imperfect approaches, local recovery and multi-lap gate/penalty checks. Show before/after speeds and contact findings for recurring slowdown. Demonstrate lake/start and visible jump/cave layout from gameplay views, not only overhead or automated success. Ensure no ambient car spawns, paths or recovers onto trails. Measure performance with dense foliage. Increment revised course record version while preserving historical boards; preserve current radio channels/bundled music, top-ten boards, difficulty constants and original course. Await Dan review; do not claim fun or physical controller acceptance from automated tests.

**Actual implementation / evidence:** Forest Loop is a 2.12 km motorcycle/ATV course with six main jumps (previously three), a 533.5 m enclosed Echo Cave route, matched fork surfaces, a graded street crossing, close forest vegetation and passing pockets. Original street scene, accepted house poses, handling/difficulty constants, radio, saved IDs and historical boards remain preserved. New records use `lake-v2-forest`. Checkpoint: `a4c7825042eefbac16bba13d3b2592fb15d7dc8d`; completion source is recorded in the delivery response/runtime VERSION.txt.

Final evidence: 12/12 main jump runs; 8/8 riders finish two-lap races with zero misses; 12/12 cave/comparison attempts with zero penalty and four successful cave recoveries; 28/28 eligibility/traffic/fall checks; 29/29 record checks. Civilian routing stays on streets in 1,320 race samples plus explicit recovery/recycle tests. Original street route suite passes 24/24 attempts and 77/77 invariants; all eight road-jump flights land, with one fastest-ATV later missed gate retained as a real five-second miss. Detailed contact/suspension traces, before/after measurements, gameplay views and the labeled route map are in Docs/CR056/VALIDATION.md.

**Dan's seven-comment review checklist (still open):**
- [ ] Forest Loop naming and versioned records; historical boards remain available.
- [ ] Lake/shoreline visible from the grid behind the existing friend's house and opening route.
- [ ] All launches/transitions feel free of unintended braking; recheck original house/road jumps.
- [ ] Cave bends/gap feel challenging; clean riding saves time, falls recover locally and bypass credit survives mistakes.
- [ ] Six distinct main jumps plus the cave jump; creek, gully, linked jumps and drop read clearly in gameplay.
- [ ] Civilian traffic remains on real streets through spawning, avoidance, recycling and recovery.
- [ ] Motorcycle/ATV eligibility, narrow forest paths, passing space, visibility and performance feel right.

**Remaining limitations:** Cave savings are modest (0.53–0.77 s clean baseline, up to 1.36 s faster repeat); mistakes erase them naturally. Two AI riders each needed one automatic recovery in the final ATV-led two-lap race. Overspeed and large off-line impacts remain physical risks. Low-poly art, subjective fun, Dan's display/hardware and physical-controller acceptance still require human review. Wildlife is deferred. Rendered 1280×720 test: median 8.334 ms / p95 8.336 ms at the 120 FPS cap, about 758 MiB observed peak working memory. Packaging verifies all 428 runtime files and 187 staged songs across ZIP/versioned/Latest; the completion source ID is stamped through the same packaging workflow. See the validation report and Builds/PACKAGE-LATEST.json; nothing is uploaded externally.

---

### CR-057 — Shallow driveable lake with water resistance
**Status:** IMPLEMENTED — awaiting Dan review in 0.9.0-review1. See Docs/CR057-060/VALIDATION.md.
**Feedback:** Player falls beneath a blue water circle onto deep dry-looking ground. Dan wants shallow water with part of the vehicle submerged, still driveable at substantially reduced speed until reaching shore.
**Requested change:** Shape a shallow supported lakebed and continuous drive-out shoreline; align actual water surface/rendering and physical depth. Keep wheels grounded and vehicle partly submerged at typical lake positions, without an overhead blue-disc appearance or immediate forced reset. Detect actual overlap/submersion, not simply horizontal lake bounds: jumping over water must not trigger braking in the air. Smoothly apply strong water drag/reduced propulsion while immersed and remove it on exit, respecting existing vehicle settings; retain enough steering/traction to drive out. Apply equivalent physical effects to AI. Add restrained splash/ripple and water-entry/exit audio as feedback. Do not add a boating/buoyancy simulation, hidden launch boost, extra time penalty or new engine-failure mechanic. Local recovery remains available. Check accessible creek crossings for the same depth/trigger defects without making airborne jumps sticky.
**Acceptance:** Motorcycle/ATV can enter, stop, turn, reverse and exit at multiple shore points; visible partial immersion matches depth, slowing is clear and temporary, no stranded pits or stale drag after recovery/restart/track changes. Verify airborne crossings and dry ramps keep momentum. Version affected course records if timing comparability changes; preserve history.

### CR-058 — Remove floating world labels
**Status:** REGRESSION REPORTED — sign-mounted text was removed too; correct under CR-061 while retaining floating-label removal.
**Requested change:** Remove floating location, jump, cave, route and development text labels. Retain necessary compact screen HUD, menu text, race countdown/results, radio toast and functional checkpoint markers; replace necessary world-space checkpoint text with physically mounted signage if needed. Do not remove detection volumes or route logic with labels. Most scenery labels need no replacement. Optional single physical cave sign may use the name Deadwood Hollow; keep readable, clear of the racing line and consistent with existing breakable-sign behavior. Essential direction signage must remain useful after destruction/reset. Debug labels remain off in normal releases.
**Acceptance:** Both tracks have no floating world text in normal play; navigation, gate visibility, timing, radio and HUD remain usable. Inspect driving views, not just hierarchy counts.

### CR-059 — Occasional believable AI driving mistakes
**Status:** IMPLEMENTED — awaiting Dan review in 0.9.0-review1. Seeded input/line/braking variation; comparison results and limitations in Docs/CR057-060/VALIDATION.md.
**Feedback:** Forest Loop is harder than Street Loop, which Dan accepts, but opponents look too perfect. Prior automated recoveries do not prove visible believable variation in Dan's races.
**Requested change:** Add bounded, context-aware driver variation in line choice, braking timing and corner/jump approach. Errors should arise through normal driving inputs and physics, such as a wide corner, overbraking or an imperfect landing, with natural time loss and recovery. Avoid arbitrary pauses, random forced crashes, teleporting, fake slowdowns, synchronized incidents, or scripted errors because the player is behind. Preserve fair gate/shortcut/water rules. Easy drivers make more/larger errors; Normal occasionally; Hard rarely but not never. Use per-driver variation/cooldowns and reproducible seeds for diagnostics without replaying identical incidents every race. Do not broadly nerf base pace or make difficult cave jumps routinely impossible. Monitor stuck/recovery behavior to preserve competitive finishes and motorcycle/ATV-only forest rosters.
**Acceptance:** Compare several full races/seeds at each difficulty on both tracks; report actual error events, natural lap/time costs, recovery/finish rates and representative replay/log evidence. Include an errors-disabled baseline. No requirement that every driver crashes each race. Keep parameters tunable for Dan review; update record categories when rules change.

### Historical optional-addition proposal — superseded by CR-079 and CR-081
Recommended: optional stunt challenges using existing jumps (airtime/distance medals and smash targets) with compact screen feedback, no floating labels and no forced effect on race timing. Personal-best ghost time trials remain another proposal. That earlier selection boundary is superseded: CR-079 stunt challenges are implemented awaiting review; CR-081 ghosts and photo-informed home/placement work are approved and queued for the following pass.

---

### CR-060 — Cave bats, wildlife and varied neighborhood life
**Status:** IMPLEMENTED — awaiting Dan review in 0.9.0-review1 with CR-057–059. Stunt challenges and ghost mode remain unselected and unimplemented.
**Cave bats:** Trigger a small animated bat swarm flying toward and then around/past the approaching rider at the cave entrance, with flutter/chirp audio. Establish a believable roost/departure path; no static bat sprites or mere particle dots as final presentation. Keep silhouettes readable but brief, do not obscure the turn/jump, push/brake the vehicle or cause physical damage. Use cooldown/rearm after leaving the area so lingering, reversing, recovery and AI entrants do not spam swarms or sounds. Pool/cull and verify motorcycle/ATV and race/restart behavior. Small ambient birds are an optional complement within wildlife scope; no large ecosystem simulation or roaming road hazards required.
**Neighborhood vignettes:** On suitable runs, choose one of three states at Dan's property: three kids playing a simple looping game of catch with a football in the front yard; two adult men just outside the yard fence chatting/drinking coffee; or nobody. At the friend's house independently choose two adult men smoking/chatting out front, or nobody. Use simple stylized figures, recognizable held props and animations (throw/catch/turn, sip, restrained smoke/hand gesture); not photorealistic likenesses. Keep the ball in the yard, figures on supported ground and preserve accepted house/fence placements. Do not request personal photos or move houses for this feature.
**Other people:** Add occasional sidewalk walkers and small idle groups at homes/storefronts with more density on Hwy 92 and sparse occupancy on South Cherokee Lane. Match people to actual supported sidewalk/frontage space; no standing in traffic lanes, water or forest racing trails. Restrained clothes/body/animation variation; avoid identical crowds or synchronized loops. Generic background ambience optional, no dialogue system.
**Variation/lifecycle:** Choose weighted scene occupancy at race/world setup with some empty visits; hold selections stable while visible and vary between races. Refresh only out of view when appropriate, never pop people in/out as the player passes. Persist selection through local recovery/pause; restart may reseed. Use reproducible seeds for testing. Population caps, pooling, distance culling and simple animation budgets; no extensive pedestrian simulation. These are ambient figures, not collision obstacles, smash targets or ragdolls; if necessary use brief avoidance reactions without adding racing penalties or impacts. Preserve driving/camera visibility and do not add floating labels.
**Acceptance:** Show all requested vignettes plus empty states using forced test seeds, and demonstrate variation over ordinary runs. Verify supported feet/props, in-yard football, correct regional density, no visible spawning, no vehicle snags and no gate/race interference. Test bat timing/cooldown/audio and all restart/recovery transitions. Measure performance with people, wildlife, traffic and music active; disclose art/animation limitations honestly. Await Dan review.

---

### CR-061 — Restore physical sign lettering; remove only floating labels
**Status:** IMPLEMENTED — awaiting Dan review in 0.9.1-review1; regression correction for CR-058.
**Feedback:** Floating text removal also removed text attached to every physical sign. Dan intended only unmounted world labels to disappear.
**Requested change:** Restore existing physical sign text on both tracks from pre-regression source/assets/history, preserving original wording, positions and breakable behavior. This includes road names, storefront signs and other physical signage, not only cave signs. Inspect the removal/generation logic so sign text survives scene generation/rebuild/restoration; do not globally disable TextMesh/world-space text. Keep free-floating location/debug/jump text removed. Add a physical sign before the Forest cave with exactly: "Warning: Cave Ahead Enter at your Own Risk". Line breaks may improve legibility without changing words/capitalization. Place it early enough to read on approach and outside the racing line. This warning supersedes the optional cave-name sign requirement.
**Acceptance:** Before/after inventory and driving-view checks of existing signs on both tracks; exact cave warning readable, mounted and lit; no restored floating labels, altered gate logic or blocked route. Break/destroy/restore/restart/build tests confirm lettering follows its sign lifecycle.

### CR-062 — Modest varied wildlife with audible animal sounds
**Status:** IMPLEMENTED — awaiting Dan review in 0.9.1-review1; expands CR-060 and repairs bat audio.
**Feedback:** Dan saw only bats and heard no bat sounds. Additional wildlife was optional in the prior scope; it is required now.
**Requested change:** Add a modest population with at least three recognizable non-bat animal types, such as birds, squirrels and frogs, distributed appropriately across woodland, lake/creek margins and neighborhood edges. Occasional sightings with idle/move/flee animation, varied locations/occupancy and empty periods; not a crowd at every bend. Each type has suitable audible calls/movement sounds, including occasional quiet species sounds while nearby. Repair cave bats with audible flutter/chirps on approach/scatter and retained cooldown. Prior sound-dispatch counts alone do not establish audibility.
**Audio:** Diagnose actual clip output, gain, listener/routing, attenuation, masking by engine/radio and cooldown. Use recognizable quality audio compatible with project licensing; avoid claiming generic synthetic tones are convincing animal calls. Preserve mute and existing audio preferences. Bounded spatial voices, distance attenuation and sensible intervals; no constant simultaneous calls. Verify real output with default engine/music settings and distinguish capture evidence from subjective listening.
**Lifecycle:** Pool/cull with offscreen placement and seeded test scenarios; no visible pop-in, synchronized loops, vehicle collision/slowdown or obstructed jump/cave view. Preserve people scenes and current course/traffic rules. Show every species using test seeds and ordinary varied drives, then measure combined wildlife/people/traffic/audio performance. Await Dan review.

### CR-063 — Two-player local split-screen feasibility / proposed future pass
**Status:** DISCUSSION ONLY — user asks feasibility, implementation not authorized yet.
**Assessment:** Feasible as a substantial feature pass, not a camera-only toggle. Current VehicleInput has unrestricted Gamepad bindings, RaceDirector initializes one human YOU racer, RaceHud references a single race/player, and RaceFlow uses shared pause/audio state. Existing RacerState/multi-racer progress and target-based ChaseCamera provide useful starting points. Needs paired device input, two human entries/vehicle selections, cameras and viewport HUDs, independent recovery/progress/results/record identity, shared-world pause/quit/finish policies, controller disconnect handling and intentional shared radio/audio ownership. Two-view rendering and ambience culling must be measured on target hardware; no fixed FPS promise. Proposed initial scope: two local controllers on one PC/screen, both existing tracks with their eligibility rules, shared audio and optional AI. No online networking/hosting. Do not change the current single-player rule or start implementation until selected.

---

### CR-064 — Optional estimated AI finish classification
**Status:** IMPLEMENTED — awaiting Dan review in 0.10.0-review1.
**Requested change:** Add a persisted option to finish remaining AI immediately when the human completes the race, plus an end-of-race action to skip waiting if the option is off. Existing full simulation remains available. This applies only to unfinished AI; never advance or finalize another human, including future split screen. Already finished AI retain measured results; existing DNFs remain DNFs.
**Estimation:** Snapshot each AI's valid multi-lap route/branch progress and recent representative moving/sector/lap pace; estimate remaining duration, using a documented vehicle/course/difficulty fallback for stationary or insufficient samples. Account for remaining laps and valid shortcut geometry; do not infer progress from straight-line distance or heading. Do not divide by near-zero current speed, let a single crash dominate the estimate, or invent future gate misses. Add already incurred penalties exactly once. An unfinished AI's projected physical finish cannot precede the snapshot; final adjusted ordering still follows the normal timing rules. Label projected totals Estimated or ~, distinguish measured finishes, and make finalization idempotent. No teleport-through-gates simulation, fabricated lap records or estimated times in measured top-ten boards. Freeze/settle AI safely after finalization and restore normal behavior on restart.
**Acceptance:** Near/far finishers, multiple laps behind, shortcut/water/stuck/recovering AI, ties, existing penalties/DNF/finished entries, repeated clicks, option persistence and restart. Human results remain measured, only AI projected, no wait required when enabled. Keep estimation/classification separate from legitimate gate progression.

### CR-065 — More recognizable stylized vehicles and drivers
**Status:** IMPLEMENTED — awaiting Dan subjective art review in 0.10.0-review1.
**Requested change:** Preserve cartoony art direction but replace overly abstract player/AI racer shapes with coherent stylized models across both cars, motorcycle and ATV. Improve body silhouette/proportions, wheel arches/tires/rims, windows, lights, bumpers and material separation; add recognizable bike/ATV frame, seat, bars and mechanical details without excessive geometry. Riders/drivers should have readable head/helmet, torso, arms/hands and legs, appropriate seated posture with hands on steering/handlebars and feet on controls. Visible car occupants should fit cabin/window geometry; no need to model hidden interiors. Simple pose/lean/steering response should follow existing driving animation without changing handling. Reuse licensed suitable assets or improve project art; no paid purchases without authorization.
**Preservation:** Keep physics/collider dimensions, center of mass, vehicle profiles/speed, paint selections and eligibility stable unless an actual existing defect requires a documented fix. Visual polish must not regress ramps, camera clearance or collision asymmetry. Fix genuine existing trim/rider property-block failures reported in 0.9.1 rather than changing tests to hide them. Inspect lit chase, side/front and garage views for all profiles/colors including black. Keep ambient traffic stylistically compatible without automatically rebuilding people/wildlife. Bound draw calls/materials and compare runtime performance. Human art review remains pending.

### CR-066 — Station-change name and two-line radio metadata
**Status:** IMPLEMENTED — awaiting Dan review in 0.10.0-review1.
**Requested change:** On station changes show the actual channel folder display name followed by Radio, e.g. Classic Punk Radio. This station announcement is for channel changes only; do not prefix normal song information or rename folders on disk. Off remains Radio Off. A virtual General channel uses General Radio. Preserve current triggers/duration for song information (track changes/on-demand and existing metadata refresh behavior); format it as exactly two labeled lines: Artist: The Blahs followed by Song: BlahBlahBlah. Use Unknown Artist and filename-based song fallback for missing metadata; plain-text handling, Unicode support within available font, readable wrapping/truncation and compact layout. Ensure station toast remains visible long enough to read when async track metadata arrives, then show song text; no stale information from a previous channel. Keep radio controls, folder hierarchy, scanning, shuffle/history, music volume and bundled packaging unchanged.
**Acceptance:** Folder names with spaces/Unicode, tagged/untagged tracks, rapid switches, on-demand display, natural next-track playback, Off, empty library and failed track cases; no station suffix on song display and no new song-popup frequency. Verify in standalone with saved library/preferences intact.

---

### CR-067 — Restore hairpin clearance and wooded house rear
**Status:** IMPLEMENTED — awaiting Dan review in 0.11.0-review1. Technical evidence and limits: Docs/CR067-069/VALIDATION.md.
**Feedback:** A house driveway extends onto and covers the hairpin; an unwanted additional road and bridge behind the house should be removed, leaving the house and woods.
**Requested change:** Identify the exact hairpin/property and geometry from the current scene/generator and historical versions before editing. Do not assume it is Dan's house or the gully stunt residence. Shorten/reshape the offending driveway to meet the street naturally without covering the turn; remove the unwanted rear road/bridge meshes, colliders and their generating logic. Restore supported terrain/woods, with no hidden road collider or floating remnants. Preserve the house pose, legitimate access, intended hairpin profile, accepted course routes, lake/cave/jumps and unrelated street continuations. Inspect route/AI/traffic/recovery references before removing geometry; if identification remains ambiguous, document candidate views and obtain the missing location rather than deleting multiple sites.
**Acceptance:** Before/after driving and overhead views identify the corrected property. Every Street-eligible vehicle and AI traverses the hairpin without overlap, snagging or new slowdown; offroad reentry and restart/regeneration are clean. The house rear visibly contains woods, not a road/bridge. Both course routes remain valid and unrelated scenery unchanged.

### CR-068 — Visible, reliable household scene variation
**Status:** IMPLEMENTED — awaiting Dan review in 0.11.0-review1. Unforced selections and visibility are verified on the original neighborhood street; Forest race-line visibility remains limited by the preserved property locations.
**Requested change:** Diagnose ordinary-release scene selection and visibility across launch/restart/track changes, persisted seeds/defaults, pool reuse, culling, occlusion and player approach. Forced-seed fixtures do not establish ordinary variation. Preserve location intent: coffee or football or empty at Dan's property; smokers or empty at friend's property. Do not relocate smokers to Dan's yard merely to create three exclusive states. Use a shuffled/weighted schedule with repetition control across new races so coffee cannot dominate indefinitely, while retaining empty visits and independent friend-house occupancy. Keep each selection stable while visible and through pause/local recovery; choose a new arrangement on a new race, not each lap. Preserve enough session/relaunch history to avoid a fixed startup coffee pattern. Ensure football/smoking gestures, props and positions are recognizable from a normal pass without intrusive labels. People remain ambient and do not obstruct vehicles.
**Acceptance:** Test ordinary unforced launch/new-race sequences, including relaunch and both tracks; record selections and actual visible occupants from driving views. Demonstrate football, coffee, smokers and empty states in a small representative sequence (target up to eight new races), plus forced checks only for diagnosis. No popping, stale pooled coffee figures, missing football props or culling that hides the alternate scenes. Document selection behavior and limits.

### CR-069 — Delayed wrong-way arrow and local-reset hint
**Status:** FOLLOW-UP REQUIRED — actual warning late/unobvious for Dan; CR-071 sets approximately five seconds and prominent visible guidance.
**Requested change:** After about three continuous seconds of meaningful wrong-way driving, show a compact screen-space directional arrow toward the correct local course direction and a wrong-way/local-reset hint. Use actual signed travel relative to valid local course/active shortcut progression, not vehicle nose direction alone or straight-line direction toward a distant checkpoint. Gate/lap seams and hairpins require continuous local route tracking. The arrow should guide turning back, not point across terrain toward the next gate. Respect input device and actual binding: default keyboard R / controller Y, with correct glyph or action label when available. Keep this a screen HUD hint, not floating world text.
**Behavior:** Brief spins, airborne rotation, backwards-facing valid landings, local maneuvering/reverse to escape, stationary vehicles and valid shortcuts must not trigger it spuriously. Use speed threshold, delay and hysteresis; clear promptly after correct travel/recovery and on finish, menus/restart/track change. Pause must not advance the timer. No forced reset, added time penalty, repeated popup spam or return to start. Existing local recovery preserves race progress/time/penalties. Keep normal HUD compact; any future per-player adaptation is not split-screen implementation now.
**Acceptance:** Sustained wrong-way travel displays arrow/hint; correct travel clears it. Test both courses, hairpin, lap seam, cave/shortcuts, airborne spins/backwards landings, stopping, brief reverse, recovery, pause/restart and keyboard/controller prompts. Validate state behavior and visual readability; distinguish emulated input from physical-controller tests.

---

### CR-070 — Remove specific hairpin junction sign
**Status:** IMPLEMENTED — awaiting Dan review in 0.12.0-review1. Results and limitations: Docs/CR070-074/VALIDATION.md.
**Requested change:** Remove the physical sign near the southern hairpin bearing South Cherokee Lane / Jamerson Road (inspect spelling/line variants). Identify this exact sign before editing; remove its text, support/collider if exclusive, and generator/restoration reference. Do not delete other signs or repeat the prior global lettering removal. Preserve hairpin/property woods fix and necessary course navigation. Verify both existing scenes and new reverse variants, rebuild/restart and breakable restoration do not recreate it.

### CR-071 — Obvious wrong-way guidance at approximately five seconds
**Status:** HUMAN FOLLOW-UP REQUIRED — misleading/unclear reverse navigation, missing off-route warning and ramp/recovery failures; see CR-075–077.
**Feedback:** Dan finds the warning too late and insufficiently obvious, despite prior reports of 3.3-second fixture results. Diagnose actual release build/settings, route/corridor validity, timer resets/suppression and HUD readability before simply changing a constant.
**Requested change:** Show a prominent high-contrast screen-space Wrong Way banner, clear local-direction arrow and active-device local-reset instruction after about five seconds of sustained meaningful wrong-way driving. Measure from actual sustained wrong-direction travel to visible warning in normal gameplay. Brief noisy samples should not indefinitely restart the timer; retain sensible hysteresis and avoid false warnings from stationary movement, spins, airborne rotation, backwards-facing valid crossings, brief escape reversing and legal shortcuts. If route confidence is lost, handle bounded local reacquisition rather than silently suppressing genuine wrong-way travel forever. Do not guide across terrain toward a distant gate. Clear promptly after correction/recovery and on race lifecycle transitions; pause does not advance timing. No forced reset, extra penalties or repeated popup spam. Test all four forward/reverse course variants, hairpin, lap seams, cave/jumps and device hints; rendered evidence must show onset timing/readability, not only internal flags.

### CR-072 — Stylized driver faces and visible hair
**Status:** IMPLEMENTED — awaiting Dan review in 0.12.0-review1. Results and limitations: Docs/CR070-074/VALIDATION.md.
**Requested change:** Add recognizable cartoony eyes/brows, nose/mouth and simple hair shapes/colors where visible to player and AI drivers/riders. Preserve helmeted motorcycle/ATV silhouettes; show face through appropriate open visor/helmet opening and only exposed hair rather than hair clipping through a helmet. Car occupants get visible face/hair where cabin sightlines permit. Use coherent proportions/skin/hair variation and simple geometry/textures, not photorealism or unnecessary facial animation. No physics/handling/paint changes. Check garage/front/side/chase views, black paint/window visibility, head pose, helmet/hand clipping and material/draw-call budget. Preserve existing body/trim shader-property correctness.

### CR-073 — Reverse versions of both courses with distinct risk/reward shortcuts
**Status:** HUMAN FOLLOW-UP REQUIRED — misleading/unclear reverse navigation, missing off-route warning and ramp/recovery failures; see CR-075–077.
**Requested change:** Add Street Loop Reverse and Forest Loop Reverse as distinct selectable course variants, reusing as much original main-route geometry as possible. Forward courses retain accepted behavior. Street variants allow all four vehicles; Forest variants remain motorcycle/ATV only. Author reversed grids/gates/finish ordering, route progress, AI racing lines/speeds, local recovery, wrong-way direction, scenery cues and record categories together. Street civilian traffic retains real-world lane direction rather than reversing just because the race reverses. No civilian traffic on forest trails.
**Jumps/routes:** Inspect every main-route launch/drop/landing and one-way stunt feature from the reverse approach. Reuse suitable geometry; author scoped reverse-only transitions/bypasses where physically necessary rather than running backwards into a launch wall. No abrupt ramp seams, hidden boost compensation or broad forward-layout changes. Preserve lake, houses/woods, cave, hairpin fix and signs except CR-070. Resolve inactive variant colliders/triggers/generation so two layouts do not interfere.
**Shortcuts:** Author at least two distinct optional shortcut choices per reverse course as a design target, not reversed copies of existing shortcuts. Favor varied tight woodland/cave/rock turns, visible creek/gully gap jumps, banked descent/landing approaches and technical rejoins; choose geographically suitable designs without moving accepted properties. Each should save worthwhile measured time when executed cleanly, but poor entry/line/speed should naturally erase that gain through lost momentum or local recovery. Compare repeated clean and imperfect route times to the normal route; demonstrate a meaningful choice rather than trivial straight-wide cuts or unavoidable failures. Provide readable entrances/takeoffs/landings, viable normal-route alternatives, consistent authorized penalty-free bypass credit through deviations/recovery, and required gates clear of shortcut mouths/flight/rejoin zones. Avoid reintroducing phantom jump braking. Map each branch, bypass set and rejoin; test every eligible vehicle. Design challenge is for Dan to judge, not implied by AI completion alone.
**Systems/validation:** Separate course/direction/rules record categories, preserve historical top-ten boards and saves, and adapt AI estimated finishes to new route/branch/lap distances. Preserve radio, signs, ambient life and course eligibility in menus/saved selections. Test full multi-lap player/AI races across four variants, ordinary misses versus authorized bypasses, local recovery, direction detection and all jumps/shortcuts. Include gameplay views, route maps, clean/mistake time comparisons and performance measurements. No split screen/networking/stunts scoring/ghost implementation.

### CR-074 — Deer and coyote ambient wildlife
**Status:** IMPLEMENTED — awaiting Dan review in 0.12.0-review1. Results and limitations: Docs/CR070-074/VALIDATION.md.
**Requested change:** Add modest occasional deer and coyote sightings to suitable wooded edges/clearings on course variants, alongside existing animals. Recognizable silhouettes, proportions and idle/walk/flee animation; deer ears/legs and suitable occasional snort/rustle, coyote body/tail and occasional call. Species-appropriate spatial audio must be audible nearby under normal engine/radio without constant calls or every animal making noise continuously. Use compatible licensed assets/audio or quality project art, no paid purchase without authorization. Maintain offscreen placement, varied/empty occupancy, pooling/culling, cooldowns and normal-run visibility. Animals remain ambient, do not block racers, cause vehicle braking or obscure jump approaches; no hunting/collision mechanic. Verify both species using forced and ordinary runs, actual mixed audio output, lifecycle correctness and combined wildlife/people/traffic/music performance. Preserve existing animals and recorded calls.

---

### CR-075 — Direction-aware signs, shortcut discovery and route guidance
**Status:** IMPLEMENTED — combined CR-075–080 delivery awaiting Dan review. See Docs/CR075-080/VALIDATION.md.
**Feedback:** Dan found only one shortcut per reverse track; opposite-direction signs mislead him. Forest Reverse led him onto a street going the wrong way without warning. A Hwy 92 sign on Trickum is misleading immediately after leaving 92.
**Requested change:** Audit all four variants together. Distinguish physical road identity from destination directions: modeled connector is Trickum Road, not Jamerson; South Cherokee Lane ends at separate Jamerson Road. Correct affected current signage/catalog text with a verified road-role mapping, not a global string replacement or new road build. Directional/shortcut arrows and entrances must match active variant; physical road-name signs remain accurate whichever way approached. Preserve lettering and removed hairpin-sign exclusion. Make both authored shortcuts per reverse track visible/readable from normal approaches using physical signs and terrain sightlines; do not add floating labels. Clarify Forest Reverse junctions/continuation and wrong-way detection after leaving the trail onto a street. Reacquire a valid local route using route/branch history; do not suppress indefinitely outside a corridor or falsely flag legitimate shortcuts. Visible wrong-way cue at about five seconds remains required in races. Free roam has no wrong-way warnings.
**Acceptance:** Driving-view sign/entrance inventory and map for four variants, both reverse shortcuts found without debug overlays, deliberate wrong-street excursion triggers useful warning, valid cave/jump/shortcut cases do not. Preserve gates, true +5s misses and authorized zero-penalty bypasses.

### CR-076 — Safe on-course local player reset and stalled-AI recovery
**Status:** IMPLEMENTED — combined CR-075–080 delivery awaiting Dan review.
**Requested change:** Reset player onto clear, supported active road or forest trail facing correct local direction; never strand behind a tree, at hill bottom or beside a ramp. Resolve from last valid earned main/branch progress; do not grant forward gates/laps or return to the start line. Validate ground normal/support, full vehicle clearance, overhead space and traffic/other racer occupancy. Use bounded alternate candidates and a known-safe prior course location if nearest candidate is blocked. Keep penalty/elapsed/progress state; no added punishment. Forest resets onto trail, not a nearby unrelated paved street. Free roam uses recent safe supported road/trail recovery without imposing race direction/progress.
**AI:** Monitor route progress over time, distinguish a brief intentional stop/traffic wait from being wedged, attempt bounded unsticking then recover to a safe earned route position. No lap-long stationary racer, recovery loop or forward progress cheat. Preserve measured/estimated finish distinctions. Test tree/hill/ramp side/underside/water/cave cases, traffic occupancy, all eligible profiles and four variants.

### CR-077 — Reverse Trickum ramp fix and mandatory ramp regression coverage
**Status:** IMPLEMENTED — combined delivery awaiting Dan review; standing ramp matrix in Docs/CR075-080/RAMP-TESTING.md.
**Feedback:** Player stuck on reverse Trickum ramp; AI wedged at its side for a full lap. Previous nominal scripted passes did not cover this behavior.
**Requested change:** Reproduce in current release with vehicle/input/speed/contact/suspension evidence. Correct geometry seams, side edges, approaches/launch tangents, collider overlaps or false braking/stability behavior rather than hiding with boosts or disabling collisions. Verify current authored ramps, not obsolete test coordinates. Every new/modified ramp requires ordinary-frame dynamic driving with every eligible vehicle: centered and moderately off-center entries, intended low/normal/high approach speeds, side-edge contacts, launch/landing and recovery. Distinguish expected major off-line crashes from routine snagging; retain failed trials. AI must approach, clear or recover from edges. Keep original courses' ramps passing.
**Evidence:** Inventory actual ramp IDs/course/profile/approach/speed/outcome, pre/post velocity/contact findings and gameplay views. Do not equate code assertions, teleport-through fixtures or one nominal pilot with robust ramp testing. This rule applies to future authoring prompts too.

### CR-078 — Free roam
**Status:** IMPLEMENTED — combined delivery awaiting Dan review.
**Requested change:** Add explicit Free Roam mode for exploring existing Street/Forest areas and their usable routes/shortcuts without race countdown, opponents, lap/finish requirements, checkpoint penalties or wrong-way warnings. Preserve traffic only on streets, scenery/people/wildlife, water, radio, destructibles, vehicle choices appropriate to the selected environment and reliable local reset. Hide race-only HUD/markers while retaining speed and optional activity feedback. Reuse course variants where direction-specific geometry requires it; no new world rebuild. Provide clear return-to-menu and race selection; isolate saves/records so roaming never produces race/lap times. Do not silently widen Forest for cars.
**Acceptance:** Explore/turn around/leave route/recover without race alerts or penalties, enter/exit mode repeatedly, physics/audio/ambient lifecycle preserved, race modes still work. No AI racers in free roam unless separately selected in future.

### CR-079 — Stunt scoring and jump/smash challenges
**Status:** IMPLEMENTED — combined delivery awaiting Dan review. Dan's explicit approval supersedes old unselected/deferred statements.
**Requested change:** Add earned airtime/jump-distance scoring and optional authored jump/smash challenges with bronze/silver/gold targets, compact feedback and persistent personal bests/medals. Reuse existing jumps and breakable props, not people/wildlife. Measure actual takeoff-to-landing travel and airborne time; exclude reset/teleport, spawn drops, jitter and invalid/wipeout landings from successful awards. Define readable scoring/landing rules. Smash targets score unique eligible props per attempt, not debris or instant repeat restoration. Allow deliberate replay through an explicit retry. Keep points separate from race times/penalties, no hidden speed boosts or race gate exemptions. Optional race stunt feedback may coexist without changing classification; full challenges available in free roam. No mandatory challenge entry during races. Use physical markers only if needed, not restored floating scenery labels; results/HUD text is screen-space.
**Acceptance:** Before/after bests persist; valid/failed jumps, recovery midair, repeated props/attempts, pause/restart and course/rules/vehicle categories behave correctly. Tune medals using measured eligible-vehicle runs so reachable and challenging; do not award gold automatically. Bound/cap score exploits and avoid HUD clutter. Human fun review pending.

### CR-080 — Speed traps with saved bests
**Status:** IMPLEMENTED — combined CR-075–080 delivery awaiting Dan review.
**Requested change:** Place a modest set of readable speed-trap activities on suitable existing road/trail stretches; show measured crossing speed, personal best and medals with compact optional feedback. Validate crossing direction/volume and real vehicle speed; no repeated awards while lingering, teleport/reset credit or automatic best from wrong mode/category. Maintain deliberate repeat attempts, store activity/course/rules/vehicle categories and define direction handling. Use discreet physical signs/camera props if needed; avoid blocking lanes or new floating labels. Free-roam results and race-time records remain separate; traps never create checkpoint penalties or alter classification. Test all eligible vehicles, repeated passes, reverse variants, unit formatting and persistence.

### CR-081 — Current combined pass: ghosts, collectibles and neighborhood/home expansion
**Status:** IMPLEMENTED in combined 0.15.0-review3 — awaiting Dan review. Automatic tests, retained failures and unperformed coverage are documented in Docs/CR081-090/VALIDATION.md; this is not blanket acceptance.
**Ghosts:** Personal-best ghost time trials against a saved valid run; non-colliding playback, correct track/direction/rules/vehicle identity, explicit incompatible-run handling and save/record integrity. Historic instructions calling ghosts unselected are superseded.
**Collectibles:** Exploration pickups with persistent discovery/counts, reachable placements and replay/reset rules, coordinated with the expanded area and free roam; no forced race penalties.
**Geography:** Friend is Kyle. Beyond the lake behind Kyle's house, terrain rises into a wooded mountainside. Use supplied terrain screenshots for ridge relationships while retaining childhood wooded setting, not modern subdivisions. The modeled connecting road is Trickum Road; South Cherokee Lane ends at separate Jamerson Road. Current sign correction is CR-075; larger geometry/topology refinement belongs here.
**Home references:** Photo-based accuracy CR-013 and placement refinement CR-018 now have user-supplied references and are queued, no longer indefinitely awaiting photos. Use filename-mapped Street View sequence from Dan's house to hilltop for fence/driveway/property relationships, not Google address overlays as house identity. Kyle's wooded descending driveway lies opposite the transition from Dan's fence toward House 2; House 3 entrance drops into woods. Preserve remembered earlier fence styles unless Dan corrects them; views are 2007/2008 and not exact surveying data.
**Dan's house:** Long low brick front, shallow roof, white porch posts/trim/shutters and shrubs; reads one story from front. Terrain falls behind, exposing a lower level under elevated wooden deck; pool at lower rear yard level. Separate large garage/kennel downhill, smaller pool house to its left in the described view. Brick chimney, deck railing/white lattice, stone landscaping and fences are visible references. Model terrain/buildings coherently; unseen dimensions/details remain approximate. No private-person likeness reconstruction required. Houses 2/3 can remain approximate; Kyle photos may follow without blocking current work.
**Reference locations:** Supplied screenshots remain in C:/Users/danmo/Downloads: MyHouse-01.png; My House #2.png through My House #6.png; House 2 - #1.png through House 2 - #4.png; House #3 Entrence.png; Kyl;es House.png; The hill along with their entrence and the rest of the gate.png; The ending of my house fence, The beginning of house 2s fence and across the street Kyles driveway.png. Terrain references are the 2026-09-19 18_30_56 and 18_31_55 Racer/ForestLoopReverse screenshots. Family architecture references are the six 2026-09-19 19_19_37, 19_20_02, 19_20_21, 19_21_40, 19_23_46 and 19_24_40 screenshots supplied in this conversation. Inspect actual filenames/images when this pass begins; do not use screenshots as in-game texture assets or infer exact measurements from them.

---

### CR-082 — Recognizable helmet-free human drivers
**Status:** ACCEPTED — Dan approves the 0.14.0-review2 correction pass on 2026-09-20; separate activity result/scoreboard follow-up is CR-090. Prior technical evidence remains historical; no new device/test coverage inferred.
**Requested change:** All player/AI drivers should read as stylized people, not uniform orange robots. Remove helmets; provide visible hair, faces with eyes/brows/nose/mouth, natural head/limb proportions and seated posture. Distinguish skin, hair, shirts, trousers and shoes with coherent varied colors; vehicle paint must never recolor occupants wholesale. Inspect shared materials/property blocks and runtime prefab generation for orange overrides. No photorealistic likeness requirement. Show actual garage/front/side/chase screenshots of all vehicle occupants before claiming completion; preserve vehicle handling, colliders, colors and budgets.

### CR-083 — Globally exclusive household scenarios
**Status:** ACCEPTED — Dan approves the 0.14.0-review2 correction pass on 2026-09-20; separate activity result/scoreboard follow-up is CR-090. Prior technical evidence remains historical; no new device/test coverage inferred.
**Requested change:** Select at most ONE vignette across both properties per race/roam session: two adults drinking coffee at Dan's fence OR three kids playing football at Dan's yard OR two adults smoking at Kyle's frontage OR nobody. These represent Dan/Kyle (plus brother in the three-person scene), so simultaneous scenes duplicate people. Do not infer identifiable likenesses from this explanation. Keep locations and repetition-controlled variation, but use one shared selection and clear pooled state across both sites. Stable through laps/pause/local reset, reseed on new session without visible swaps. Unrelated generic pedestrians may remain. Test all forced/ordinary states across track/mode/relaunch; count global active groups, no pooled duplicates.

### CR-084 — Track-menu changes must not skip radio songs
**Status:** ACCEPTED — Dan approves the 0.14.0-review2 correction pass on 2026-09-20; separate activity result/scoreboard follow-up is CR-090. Prior technical evidence remains historical; no new device/test coverage inferred.
**Requested change:** Changing selected course/variant in menu must not issue next-song/channel input or restart playback. Inspect shared bindings/action maps, button event propagation, menu focus and radio object lifecycle; determine rather than assume cause. Retain current song/channel and playback position during selection, except natural track ending or explicit radio action. Test keyboard/controller navigation, rapid selection, menu return and all four courses with unchanged library/settings. Preserve explicit radio controls and song-change UI timing.

### CR-085 — Forgiving visible checkpoint edges
**Status:** ACCEPTED — Dan approves the 0.14.0-review2 correction pass on 2026-09-20; separate activity result/scoreboard follow-up is CR-090. Prior technical evidence remains historical; no new device/test coverage inferred.
**Requested change:** Reconcile rendered gate opening with trigger/swept detection and eligible vehicle extents. Accept legitimate edge overlap within a small documented tolerance so a vehicle visibly passing inside is credited; do not require only center-point passage. Use continuous/swept checks at speed and correct vertical jump coverage; prevent double credit/repeated backward exploits and keep true misses outside tolerance. Apply to all courses/start-finish gates, preserve authorized shortcuts and +5 true misses once. Test left/right edges, airborne/tall/fast profiles and outside negatives; show geometry overlays only in diagnostic evidence. Preserve race integrity and historical record categories.

### CR-086 — Imperial display units
**Status:** ACCEPTED — Dan approves the 0.14.0-review2 correction pass on 2026-09-20; separate activity result/scoreboard follow-up is CR-090. Prior technical evidence remains historical; no new device/test coverage inferred.
**Requested change:** Convert speedometer, speed traps, menus/help, stunt/jump distances, challenge targets/results and records display consistently. Keep canonical physics/save units; migrate display preferences safely without multiplying stored values or breaking old scores/medals. Label units, sensible rounding, preserve precision for comparisons. No change to handling, distances, timing or difficulty. Test conversion and persisted legacy/new data across race/free roam. No unsolicited metric-first default.

### CR-087 — Trickum ramp failure: test first in races AND free roam
**Status:** ACCEPTED FOR NOW — Dan approves this build; retain documented unconfirmed forward slowdown, instability/outer-edge roll and prior test limitations as known evidence, not newly fixed or retested. Standing ramp-first testing remains mandatory.
**Feedback:** Reverse Street ramp still slows Dan; same jump slows in BOTH directions in free roam although forward racing seemed fine. Current matrix success does not supersede this observation.
**Required first work after safety commit:** Reproduce baseline in current compiled player for forward/reverse racing and both free-roam directions, all eligible vehicles with emphasis on reported bike/ATV history. Inspect mode-specific geometry/duplicate colliders, loaded variant overlays, ramp/gate overlap, water/surface regions, contacts, suspension, stability/braking/input and speed before/at/after transition. Actual driving, no teleport-only proof. Record fails before fixes; if not reproduced, keep report unresolved and document precise coverage rather than declaring solved. Correct root cause without hidden boost or disabled collisions. Gate placement is coordinated with CR-088.
**Standing requirement:** Any new/modified ramp must be tested at the BEGINNING of its authoring work (existing baseline or first playable geometry) and after changes, in all applicable race/free-roam direction variants, every eligible vehicle, centered/off-center/side-edge lines and intended speed range. No building a whole pass on untested ramps. Keep failure evidence, AI approaches/stuck recovery and actual authored IDs/positions; report abrupt speed losses and contact cause separately from expected climbing speed changes. Recheck final compiled package.

### CR-088 — Reverse Street entrance, vegetation and gate corrections
**Status:** ACCEPTED — Dan approves the 0.14.0-review2 correction pass on 2026-09-20; separate activity result/scoreboard follow-up is CR-090. Prior technical evidence remains historical; no new device/test coverage inferred.
**Requested change:** (1) Make first reverse shortcut visible with clear active-direction physical signage and approach sightlines. (2) Remove/fix entire offending trees including canopy/LOD/billboards and generator references; no floating branches left after trunk clearing. Preserve coherent supported trees framing route. (3) Forward third-shortcut exit must not advertise/visually read as reverse entrance: use active-direction cues/appropriate scenic shaping without blocking its forward exit. (4) Align reverse start/finish visual gate and logical timing/grid crossing with actual reverse start location, not inherited forward location; prevent lap-zero/duplicate finish credit. (5) Remove/consolidate or relocate redundant ramp-area gates; no mandatory gate in launch ramp or tightly adjacent gate about ten feet away. Place clear required crossing after landing/rejoin or before committed approach with adequate speed spacing; update ordering, bypass sets, AI, HUD, wrong-way/reset references and record versioning together. No regression to forward course or hairpin woods/sign removals.
**Acceptance:** Map and ordinary driving views verify both reverse entrances, valid forward exit, complete trees, distinct reverse grid/finish and sensible ramp gate spacing. Full multi-lap races all eligible profiles with edge/airborne credit, normal misses and shortcut entitlement.

### CR-089 — Forest Reverse start makes main route clear
**Status:** ACCEPTED — Dan approves the 0.14.0-review2 correction pass on 2026-09-20; separate activity result/scoreboard follow-up is CR-090. Prior technical evidence remains historical; no new device/test coverage inferred.
**Requested change:** First shortcut currently reads as natural straight-ahead course from grid while required main route is awkward/off to side. Reorient grid/initial lead-in or locally reshape junction so normal route is the obvious continuation and shortcut a deliberate optional risk/reward branch. Use physical signs/chevrons and supported terrain, not floating text or wholesale course replacement. Preserve challenging shortcut, required lake/Kyle setting and motorcycle/ATV eligibility. Coordinate first gate/grid/AI/recovery/wrong-way guidance and independent timing categories. Verify first-time driving view and both choices without debug overlays; test race and roam, preserve other variants.

---

## CR-082–089 delivery checklist (0.14.0-review2)

- [x] Safety checkpoint b1d076d948e2a8ce57bdfaeebd698b87e77bd5b6; baseline ramp driving preceded corrections; repeated in final compiled player.
- [x] Helmet-free faces/hair, separate clothing colors and controls; actual garage/front/side/chase views for all profiles in Docs/CR082-089/PEOPLE.md.
- [x] One shared coffee/football/smoking/nobody choice across both properties; pooled cleanup and stable lap/pause/reset behavior.
- [x] Radio channel/song/position retained across course selection; explicit controls and natural completion preserved.
- [x] Both gate edges accept vehicle extent plus 7.9 inches; real misses remain exactly +5 once; shortcuts and start/finish validated.
- [x] mph/miles/feet in UI, activities, records and signs; old canonical values retained without rescaling.
- [x] Reverse first entrance signs, whole-tree repair, forward-exit-only cues, real reverse grid/finish, consolidated post-landing gate and coordinated route systems.
- [x] Forest Reverse supported main departure and optional shortcut; initial driving views and both choices tested.
- [x] Three-lap races on all four variants: all 16 participants finished, zero missed gates; records/stunts/traps/recovery/estimates regression checks recorded.
- [ ] CR-087 remains partly open: forward reported unexplained slowdown not independently confirmed; forward-ramp instability and one outer-edge reverse motorcycle roll retained. Do not close from successful fixtures.
- [ ] Retain AI Tourer obstruction deadline failure (moving again at deadline), and earlier candidate 2/3-lap DNF at normal finish grace; neither erased by final seeded race success.
- [ ] Dan's human driving, visual acceptance, listening and physical-controller tests remain separate and pending. Concurrent hidden tests are not a performance benchmark.
- [x] Versioned Windows package and complete Latest use the packaging workflow, preserve Play-Racer.cmd and staged music; see Docs/CR082-089/PACKAGE.md and Builds/PACKAGE-LATEST.json.
- [x] CR-081 remains APPROVED / QUEUED after this correction pass; no networking/split screen.

Actual case counts, failed cases, authored ramp IDs, before/after evidence and package details: Docs/CR082-089/VALIDATION.md. The completion commit is recorded in the delivered VERSION.txt.

---
### CR-090 — Visible automatic activity results and persistent scoreboards
**Status:** IMPLEMENTED in combined 0.15.0-review3 — awaiting Dan review. See Docs/CR081-090/VALIDATION.md for automatic/race/free-roam result evidence and remaining test coverage.
**Original feedback/evidence (superseded by the implementation above):** Dan sees speed-trap signs but has never seen results, and wants scoreboards for speed traps and jumps. Current ArcadeActivities checks speed traps only under race.FreeRoam; activity awards also require FreeRoam. This is a likely explanation for racing observations, not proof all free-roam triggers work. Existing activity menu only exposes a selected-site PB, not full boards.
**Requested behavior:** Passing a speed trap normally must automatically measure/show a clear short screen result in BOTH races and free roam, with site name, speed in mph, medal, PB/new-best status. No pause-menu activation required. Audit actual sign/measurement plane correspondence, direction/rearm rules, swept detection, HUD priority and saved preferences in compiled build. Do not silently swallow results behind race/radio messages; queue/prioritize briefly without obstructing driving or changing radio behavior. Valid landings at authored jump sites must likewise automatically show distance in feet, airtime/points where applicable, medal and PB in both modes; explicit timed challenge/retry remains optional. Invalid landings/resets never get valid scores; provide clear bounded failure feedback when appropriate.
**Boards:** Add controller-accessible Activity Records from main menu and pause/results as appropriate, with Speed Traps and Jumps tabs, per-site/course/direction/vehicle category selectors and top ten distinct valid attempts. Descending speed/distance, rank, mph/feet, vehicle, date, medal and useful jump airtime/points details; explicit empty state and current best. Keep metric internal precision and imperial presentation. Preserve activity PBs/medals; migrate eligible historic PB once without inventing missing history/dates. No duplicate entries on result reopening/save reload; tied distinct attempts allowed. Keep race lap/total-time boards separate and unchanged. Use compatible rules/mode categories if competitive conditions differ; clearly label rather than silently mix incompatible scores.
**Integrity:** Swept high-speed crossings, actual supported opening/direction, thresholded exit/rearm, no repeated award while parked, teleport/reset protection, clean takeoff/landing validation and no debris farming. Activities never alter race timing, penalties, AI finish classification or gate credit. Share a consistent award/save/display path across modes.
**Acceptance:** Every authored speed trap and jump site checked in applicable direction/course and eligible profile coverage in race and free roam. Demonstrate actual rendered results after driving through/landing, not solely event assertions; include sites/signs inventory. More than ten attempts, ties, better/worse scores, pause/resume, reset/teleport, retry, restart/save reload, migration, radio overlap and empty states. Preserve full package/music and all accepted systems; await Dan review of this follow-up.

---

### CR-091 — Missed start/finish must not discard a completed lap
**Status:** IMPLEMENTED in 0.16.0-review3 — awaiting Dan review; actual results and residual limits are in Docs/CR091-096/VALIDATION.md. Original report: missing the finish opening silently loses the whole lap.
Count a legitimately completed circuit when the driver passes the finish region outside its gate, add exactly one +5-second missed-gate penalty and obvious feedback, and finish the race on the final lap. Use validated route/lap progress and swept finish-region crossing, not merely proximity or vehicle heading. Legal shortcut progress remains valid. Prevent duplicate laps/charges from lingering, reversing, reset/teleport, restart or crossing at the initial start. Preserve other genuine-miss accounting. Penalized laps are ineligible for clean-lap ghosts. Test all four variants, ordinary/airborne/edge crossings, final lap, shortcuts and anti-duplicate cases.

### CR-092 — Property spacing, continuous grounded fences and entrance sign
**Status:** IMPLEMENTED in 0.16.0-review3 — awaiting Dan review; actual results and residual limits are in Docs/CR091-096/VALIDATION.md. See Docs/CR091-096/VALIDATION.md and README-player.txt.
Dan's front and House 2's front must run parallel to their local street alignment. Move House 2 and its fencing at least roughly 200 feet farther ALONG the street away from Dan's house, not just farther back from the road. Extend Dan's roadside fencing through the added separation. Coordinate House 2/House 3 fence boundaries: House 2's fence reaches the second neighbor's fence, which starts earlier on the approach.
Continuous roadside fencing across Dan, House 2 and House 3, interrupted ONLY at driveway gates. Dan has TWO driveway gate openings, one on either side of his house. House 1 remains absent. Ground every post/panel to the terrain; relocation alone is not a floating-fence fix. Mainly wood fencing behind the pool; remove misplaced rear chain-link, retaining appropriate roadside/property fencing.
Lower Kyle's house/property so the roof is approximately at road elevation; a genuinely descending driveway must remain driveable and terrain-supported.
Place exact text "Rocky Way Acres" on a physical sign spanning above House 3's descending driveway between tall supports, matching C:/Users/danmo/Downloads/Rocky Way Acres.png. House 3 is the SECOND neighbor, not House 2. Provide vehicle clearance and contained readable lettering; retain breakable-prop behavior. Preserve supported architecture, deck/pool/outbuildings, hairpin clearance and race routes. Use previously supplied references; do not invent exact surveyed dimensions.

### CR-093 — Fit text to physical signs across the world
**Status:** IMPLEMENTED in 0.16.0-review3 — awaiting Dan review; actual results and residual limits are in Docs/CR091-096/VALIDATION.md. See Docs/CR091-096/VALIDATION.md and README-player.txt.
Audit all four course variants and free-roam signage. Fit/wrap lettering with margins within the actual sign face; enlarge boards appropriately when necessary rather than make text unreadably tiny. Preserve exact names/warnings, physical mounting, directional correctness and destruction/restoration behavior. No floating text or blanket deletion of sign lettering.

### CR-094 — Permanent mountainside campsite and summit launch
**Status:** IMPLEMENTED in 0.16.0-review3 — awaiting Dan review; actual results and residual limits are in Docs/CR091-096/VALIDATION.md. See Docs/CR091-096/VALIDATION.md and README-player.txt.
Add two recognizable stylized guys seated at a campfire a few feet from a small round pop-up tent on the mountainside. This campsite may always exist and is independent of the exclusive coffee/football/smoking neighborhood rotation; do not multiply those neighborhood scenes. Keep tent/people/fire grounded with safe trail clearance and bounded animation/audio cost.
Add a large summit ramp aimed back toward the houses, with spectacular airtime, a deliberate supported landing area and return route. Do not aim the landing into homes/fences or manufacture momentum through hidden collision steps. Make approach, direction and risk understandable; integrate a compatible authored jump activity/result/record. Test at the FIRST playable geometry and after final changes across eligible cars/bike/ATV, intended speeds, center/off-center/edge approaches and applicable modes/directions. No claim reverse scoring is required, but return approaches must be safe/readable. Retain all existing unresolved ramp evidence.
A mountain race is FUTURE BACKLOG, not authorized for implementation this pass. Preserve the existing mountain trails and four race variants.

### CR-095 — Hidden exploration collectibles instead of roadside giveaways
**Status:** IMPLEMENTED in 0.16.0-review3 — awaiting Dan review; actual results and residual limits are in Docs/CR091-096/VALIDATION.md. See Docs/CR091-096/VALIDATION.md and README-player.txt.
Keep 24 collectibles but redistribute MOST off the ordinary race/road line throughout woods, gullies, lakeside pockets, behind landmarks and optional trails/skill locations. A few visible introductory finds are fine. Hide fairly with visual clues, reachable approaches and safe exit/reset; none inside colliders or requiring impossible jumps. Maintain a placement inventory and physically verify every location.
Preserve existing found IDs/progress through relocation by default; provide an explicit confirmed collectibles-only restart for Dan to search the revised layout. Do not silently erase discoveries, race records, settings, ghosts or map exploration. State clearly how relocated previously collected items behave. Validate partial/full progress and relaunch.

### CR-096 — Entire-area exploration map with persistent fog and safe fast travel
**Status:** IMPLEMENTED in 0.16.0-review3 — awaiting Dan review; actual results and residual limits are in Docs/CR091-096/VALIDATION.md. See Docs/CR091-096/VALIDATION.md and README-player.txt.
Create an accurate map from actual world coordinates for the entire playable area, with player position/heading, controller and mouse navigation, pan/zoom and waypoint placement. Show a discoverable outline/extent without leaking unrevealed terrain detail. Driving locally reveals fog; exploration persists and revealed areas stay revealed across tracks/relaunch. Expose no hidden collectible coordinates automatically. Discovered landmarks and collected-item locations can appear; regional found/total counts support searching without claiming every revealed patch has been exhaustively searched.
Fast travel is FREE-ROAM ONLY to discovered safe destinations selectable by mouse/controller. Snap destinations to validated supported ground with vehicle clearance; show unavailable destinations clearly. Teleport must not reveal intervening terrain, award race laps/gates, trigger speed/jump records, collect items en route or create a valid ghost. Cancel in-progress stunt attempts and reset relevant movement histories safely. A race map may show route information, but race fast travel is disabled.
Test map/world alignment, lake/mountain elevation relationships, reveal radius/edge cases, persistence, controller navigation, waypoint behavior and safe arrival for each eligible vehicle. Bound fog-save/render cost. Version world/map data without silently wiping saves.

### Combined review / delivery — CR-091–096
Dan accepts the general house representation and mountain paths as a basis, with the explicit revisions above. This is NOT blanket acceptance of 0.15.0-review3 or unreviewed ghosts/activity boards. Preserve its documented ramp/shortcut/AI/performance limitations and address regressions introduced by this pass.
- [x] Automated: missing a finish gate completes a legitimate lap with one +5s penalty; final lap finishes; no reset/reverse farming.
- [ ] House 2 separation, parallel fronts, two Dan gates, continuous grounded frontage, rear wood fence, lowered Kyle and House 3 overhead sign reviewed.
- [ ] Sign lettering fits/readable in race and roam; no lost text.
- [x] Permanent campsite, summit jump/landing/return and first-geometry/final ramp evidence delivered; extreme-edge limits remain documented.
- [x] Automated: hidden collectibles have all 24 reachable locations, preserved progress and optional collection-only restart.
- [x] Automated: whole-world map, saved fog, regional counts, waypoint and safe discovered-destination free-roam travel function.
- [x] Four-course race/shortcut/recovery/AI, ghosts/boards, radio/menu, destruction and save regressions checked; performance measured. Retained failures are not acceptance.
- [x] Ordinary tests temporarily muted. Briefly unmute only for necessary audio checks, then remute; saved preferences and shipped audio unchanged.
- [x] Complete Windows runtime/ZIP, previous builds/music preserved, version/commit evidence and concise new playtest guide delivered.
- [ ] Dan reviews new build. Mountain race, multiplayer and split screen remain future work.

Technical detail: 372 final summit cases; 216/216 center/normal-offset runs scored, 16/144 actual-edge forward cases did not score, and all local resets succeeded. Twelve eligible three-lap player races completed across the compiled regression candidates, with one AI grace-period DNF and three Creek Leap clean-run failures retained. Final four-course race smoke, all 96 collectible approaches, cold fog/ID reload, map/travel, records, radio and restoration checks pass. Human/property/sign/controller/listening acceptance remains unchecked. See Docs/CR091-096/README-player.txt for Dan's new checklist and Docs/CR091-096/PLACEMENTS.md for the complete spoiler inventory.

---

### CR-097 — Restore property fences and House 3 driveway; remove Kyle's fence
**Status:** IMPLEMENTED / COMPILED PHYSICAL VALIDATION / AWAITING DAN REVIEW.
Dan reports the fence between his house and House 2 is missing. Restore the connecting roadside run AND the property-separating fence between the homes, using references rather than substituting only an unrelated frontage section. Preserve the agreed spacing, parallel fronts, continuous three-property roadside fencing and Dan's two driveway gates. Ground every panel/post.
House 3 (the second neighbor, Rocky Way Acres entrance) no longer has a supported driveway: entering drops the player into a pit. Restore a continuous physically supported, smoothly graded drive from the road, beneath the overhead sign, down to its house. Remove holes/collider seams; do not flatten the intended valley or move the overhead sign to hide the fault. Verify driving in and back out with every vehicle and safe recovery after failed approaches.
Kyle has NO fence. Remove all fencing belonging to his property, including the floating fence. Keep neighboring fences across the street, his lowered house and descending driveway intact. Inspect all relevant course/free-roam scene variants and source generation so rebuilds retain the fixes.

### CR-098 — Summit jump, recognizable stone clues and domed tent
**Status:** IMPLEMENTED / CREST CLEARANCE VERIFIED; HARD-LANDING SCORE FAILURE RETAINED / AWAITING DAN REVIEW.
Dan cannot clear the mountain top with the apparent summit launch, or cannot identify the intended launch. First locate/reproduce from ordinary player approaches, then provide unmistakable physical approach guidance and map/landmark identification consistent with discovery rules. Test from first playable corrected geometry. Ensure the intended summit jump launches past the mountain crest toward the neighborhood with dramatic airtime and a deliberate reachable supported landing/return; a nominal airborne frame or scoring event is not success. Record actual launch/crest/landing clearance and speed, vehicle, route, camera views and failed cases. Cover eligible vehicles, ordinary and fast speeds, center/off-center/edge approaches and applicable modes; no hidden impulse used to conceal a snag.
Replace spherical/ball campsite tent with a small recognizable DOME tent: flat base on terrain, arched fabric roof, visible poles/seams and entrance flap. Preserve two seated guys, campfire and its independence from household vignette exclusivity.
The screenshot C:/Users/danmo/Downloads/whats this.png shows clue cairns: DiscoveryDetails.cs builds three smooth brown spheres under "Acorn clue / <id>" using "Cairn stone". Explain purpose in handoff; improve recognition using irregular angular gray/weathered stones and modest varied placement, not repeated smooth brown blobs. Keep collectibles hidden and preserve IDs/progress; don't silently remove a gameplay clue system or expose every pickup.
All current tests/retained failures remain historical, not proof this user report is fixed.

### CR-099 — Approved startup artwork, spoken title and looping theme
**Status:** IMPLEMENTED / COMPILED AND EXTRACTED STARTUP VALIDATION / AWAITING DAN LISTENING & INPUT REVIEW.
Use the EXACT approved Woodstock Rush artwork, not regenerated art. Durable handoff asset: outputs/WOODSTOCK_RUSH_TITLE.png beside this planning copy; original C:/Users/danmo/.codex/generated_images/01a0ae2c-3129-7bd2-974a-13c76f7df420/exec-e11df3f6-6adb-410e-bcb5-2699f1d55fc6.png.
Source audio exists locally: C:/Users/danmo/Downloads/Woodstock Rush Spoken.mp3 and C:/Users/danmo/Downloads/Woodstock Rush.mp3. Handoff copies are under outputs/TitleAssets. Import explicit title assets into project/package, never require Downloads at runtime and never add them automatically to radio channels.
On fresh application startup show the artwork before the main menu. Play Spoken once, with theme starting quietly underneath and ducked for intelligibility; theme continues seamlessly looping until a fresh deliberate keyboard/controller/mouse button press. No timed auto-advance. Add a modest "Press any button" prompt without obscuring artwork.
On advance stop/fade title audio promptly, consume the input (do not also activate a menu item), then show main menu and only then permit radio playback according to existing station/Off/volume preferences. NO radio during splash, even for persisted radio objects/preferences. Do not replay spoken/title on ordinary track changes, pause, Quit Race or scene reload; replay only on new application start unless a future explicit replay feature is requested.
Honor Master mute/volume; separate startup audio from radio state so Radio Off doesn't accidentally disable the title theme. Choose/document reasonable music/voice routing without altering saved preferences. Stop loops/one-shots cleanly on skip/quit/restart; no duplicate audio/listeners. Use provided recording, not TTS.
Preserve artwork aspect ratio and full logo/vehicles at 16:9 and Steam Deck 1280x800 (16:10), using deliberate matte/letterbox if needed rather than stretching/cropping title. Support resizing and controller focus. Verify MP3 encoder padding/end gaps and loop at a clean musical boundary; keep source originals unchanged.
Test cold start with radio On/Off, Master muted, delayed audio loading, early skip, held input at startup, keyboard/controller/mouse, theme wrapping multiple times, transition once, and menu/race return. Ordinary tests muted; only brief targeted audio checks then remute. Never ship a diagnostic mute.

### CR-100 — Map/exploration follow-up validation and combined review
**Status:** TARGETED VALIDATION PER LATEST AUTHORIZATION / MAP-EXPLORATION ACCEPTANCE STILL PENDING.
Recheck map/world alignment, saved fog/waypoints/regional counts, safe discovered-destination free-roam travel, collectible persistence, and updated property/summit destinations after terrain changes. Travel cannot place vehicles in the repaired pit, inside fences or on invalid mountain surfaces. Keep race fast travel disabled and preserve save/ghost/record integrity.
Keep prior known ramp/shortcut/AI/performance limitations open unless reproduced and actually fixed. Preserve missed-finish +5s lap completion behavior. No mountain race/multiplayer/split screen in this pass.
Combined human checklist: boundary fence/two gates/frontage; House 3 drive in/out beneath Rocky Way Acres; no Kyle fence; clear summit approach and full crest clearance/landing; dome tent and readable rock clues; title speech once/theme loop/skip/no premature radio at 16:9 and Deck ratio; map/fog/travel/save regression. Build and validate complete Windows package; preserve previous builds, staged radio music and user settings. Update actual evidence, failures, version/source IDs and completion commit; await Dan review.


**2026-09-20 scope correction and actual delivery:** Dan explicitly narrowed remaining testing to this pass's changes. Pending broad suites were stopped; no new full collectible/map/ghost/leaderboard/four-course regression run is required. Complete the physical House 3 in/out/reset checks, summit speed/line/mode/vehicle coverage, targeted title/audio, affected nearby travel/collectibles and one brief gameplay check. Completed extra evidence and unrelated failures remain documented without expanding scope. Source generators and all four scene variants retain the property/terrain fixes. The fence-smash binding regression caused by replacing panels was reproduced and repaired. The supplied approved art and both audio tracks are embedded; source files are preserved separately from radio. Staged music and older deliveries are preserved by Tools/Package-Racer.ps1.

**Dan checklist for this pass:** (1) Dan/House 2 boundary, continuous frontage and two gates; no Kyle fence. (2) Rocky Way Acres drive down/up with each vehicle and reset. (3) Follow lake ascent → camp/ridge → east teal runway → westbound jump; brake and take the northern return; assess hard landing/zero score. (4) Dome, two campers/fire and irregular gray clue cairns (subtle hints to hidden acorns, not pickups). (5) Cold title, held/early button, full artwork at both ratios, voice once/theme loop/Master/Radio Off and menu focus. (6) Nearby map/travel and one brief normal drive. Broader map/exploration review remains pending.

---

# DECISION LOG

Record choices we do not want to repeatedly reconsider.

| Date | Decision | Reason |
|---|---|---|
| 2026-09-20 | Accept correction pass; add CR-090 activity results/boards | Speed-trap signs alone are insufficient; automatic visible results in race/roam and top-ten jump/trap boards, retain known technical limits |
| 2026-09-20 | Prioritize CR-082–089 correction pass | Test Trickum first in race/roam both directions; human drivers without helmets, globally exclusive vignettes, menu radio isolation, gate tolerance, imperial display and reverse layout corrections; approved CR-081 stays queued |
| 2026-09-19 | Approve route fixes + arcade challenges now, world/ghost/collectibles next | CR-075–080 current; CR-081 approved queue includes corrected Trickum/Jamerson geography and supplied home references; earlier feature exclusions superseded |
| 2026-09-19 | Bundle CR-070 through CR-074 | Remove exact hairpin sign, visible five-second wrong-way cue, faces/hair, reverse tracks with risk/reward shortcuts, and deer/coyotes |
| 2026-09-19 | Bundle CR-067 through CR-069 | Restore hairpin/property woods, fix repeated coffee scene, and show wrong-way direction/local-reset hint only after sustained wrong travel |
| 2026-09-19 | Bundle CR-064 through CR-066 | Optional projected AI finishes, clearer cartoony vehicles/drivers and requested radio formatting; split screen still estimate only |
| 2026-09-19 | Correct sign text and require wildlife audio under CR-061/062 | Only unmounted labels should disappear; cave warning exact wording, varied animals with sounds; split screen CR-063 remains discussion |
| 2026-09-19 | Add CR-060 to pending water/labels/AI pass | Cave-entry bats, variable football/coffee/smoking scenes and street pedestrians, dense on Hwy 92 and sparse on South Cherokee; simple stylized ambience |
| 2026-09-19 | Bundle CR-057 through CR-059 after positive Forest Loop review | Shallow slow driveable lake, physical signs instead of floating labels, occasional fair AI mistakes; optional larger additions await selection |
| 2026-09-19 | Revise second track as Forest Loop under CR-056 | Visible lake/start behind friend, smooth frequent jumps, challenging cave, narrow motorcycle/ATV trails and street-only civilian traffic; wildlife deferred |
| 2026-09-19 | Deliver omitted CR-040/054 plus CR-055 radio channels | Bundled Music becomes default; immediate child folder names label recursive channels; one button cycles channels and Off |
| 2026-09-19 | Authorize CR-040 second lake/woods track and CR-054 top-ten lap/race records alongside CR-050 through CR-053 | Dan agrees to recommended next track and requests both record lists; later approval supersedes previous CR-040 backlog restriction |
| 2026-09-19 | Initial request: bundle CR-050 through CR-053; feature choice initially pending | Dan reports shortcut gate placement, identical traffic, nested library and portable music needs; explicitly permits selected music in friend package, without authorizing upload |
| TBD | Single-player first | Keep scope manageable |
| TBD | Arcade handling over simulation | Matches the intended Forza Horizon-style experience |
| TBD | One childhood street first | Small enough to actually finish |
| TBD | Greybox before visual polish | Gameplay first |
| 2026-09-17 | Phase 1 handling accepted | Dan tested the prototype and reported that it seems to drive fine |
| 2026-09-17 | Phase 2 uses the full real road loop | The loop already exists in the real road layout; no fake connector is needed |
| 2026-09-17 | Prefer remembered older landscape over current development | Newer subdivisions should be replaced largely with woods/open land where indicated |
| 2026-09-17 | Commit before every Codex work session | Dan saves locally between sessions and wants a safety checkpoint before Astra changes anything |
| 2026-09-17 | Phase 2 not accepted; run a focused revision pass before Phase 3 | Dan reported BUG-001 and nine environment changes after playtesting |
| 2026-09-17 | Preserve current real loop layout and Phase 1 driving systems during revisions | Fix environment support, elevation, placement, and density without changing accepted driving behavior |
| 2026-09-17 | Last descent must be shorter and more noticeable, still gentler than second drop | Latest playtest refines the earlier gradual-descent guidance |

| 2026-09-17 | Phase 2 track accepted after Dan's playthrough | Dan reports the track seems fine; supersedes the earlier Phase 2 hold |
| 2026-09-17 | Include slight steering tightening (CR-010) in Phase 3 | Improve road-driving feel without delaying race-system work |

| 2026-09-17 | Accept Phase 3 race systems after Dan reports all tests passed | Mild floatiness remains a non-blocking CR-010 follow-up, not a reason to repeat the race-system phase |

| 2026-09-17 | Include CR-011 speed/acceleration increase with Phase 4 handling work | Dan wants a faster car and testing at representative racing speeds; 15–20% top-speed increase is an initial target subject to validation |

| 2026-09-17 | Accept Phase 4 and close CR-010/CR-011 | Dan replied "approved" to the completion report; physical-controller testing remains unconfirmed |

| 2026-09-17 | Phase 5 accepted; move to Phase 6 in small batches | Dan confirmed the shortcut works and agreed to move on; begin with house/building silhouettes |

| 2026-09-17 | Keep Houses 2 and 3; remove House 1 and expand Dan's yard | Dan corrected his memory to two neighboring houses; labels remain unchanged and older three-house directions are superseded |

| 2026-09-17 | Accept the Phase 6 building batch and close CR-012 | Dan says it looks fine; this does not complete all of Phase 6 |
| 2026-09-17 | Backlog photo-based home accuracy as CR-013 | Dan may supply photos later; current progress must not wait for them |

| 2026-09-17 | Revise vegetation for substantially more woodland and drivable forest (CR-014) | Dan recalls broad wooded areas and wants off-road exploration; measure performance rather than assuming added trees are affordable or prohibitive |
| 2026-09-17 | Implement CR-014; keep vegetation awaiting Dan's review | 11,897 solid trees, broad woodland expansion, preserved accepted sites and gameplay; staged/standalone measurements and virtual driving recorded in Docs/CR014/VALIDATION.md |

| 2026-09-17 | Implement CR-015 and the restrained roadside batch; await visual approval | Three house offsets, local valley extension, nine conflicting trees removed, four mailboxes/two signs; accepted systems retained. Evidence and measured limitations in Docs/CR015/VALIDATION.md |
| 2026-09-17 | Retain improved woodland and schedule CR-015 with the next Phase 6 batch | Dan reports tree coverage is much better and requests small house-location corrections; House #2 should be directly across the street from the friend's house. |

| 2026-09-17 | CR-016 supersedes Dan's small setback and enlarged yard | Move his home toward the current yard's southern end; keep about one quarter of the yard area and reforest the rest |
| 2026-09-17 | Mailboxes/signs visually approved; breakability tracked as CR-017 | Dan wants them breakable in the final game; behavior is a later focused pass |

| 2026-09-17 | Deliver CR-016, restrained fences and CR-017 together | Larger coherent Phase 6 batches preferred; implementation and integrated validation complete, awaiting Dan's review |

| 2026-09-17 | Accept combined CR-016/fences/CR-017 batch; defer exact home position as CR-018 | Dan approved and does not want more placement work now |
| 2026-09-17 | Next delivery groups remaining Phase 6 environment work | Road markings, selective curbs, local terrain transitions, basic lighting/audio and performance checks; preserve gameplay and avoid another house-placement pass |

| 2026-09-17 | Phase 6 accepted; proceed to Phase 7 core game flow | Dan approved the environment delivery; preserve current house placement and leave optional scoring/speed traps deferred |


| 2026-09-17 | Phase 7 accepted after Dan reports all checklist items passed | Device-specific coverage is not inferred; preserve delivered game flow |
| 2026-09-17 | CR-019: remove mirrored stores and uniform main-road spacing | Repeated stores are fine, but opposite sides should have independent order and spacing |
| 2026-09-17 | Deliver first Phase 8 commercial polish batch; await Dan's visual review | Retain 22 stores with independent authored rows, coordinated frontage, local tree clearance and preserved accepted systems |
| 2026-09-17 | CR-019 commercial-street batch accepted | Dan reports passed; preserve independent store layout and proceed with remaining Phase 8 polish/performance together |

| 2026-09-18 | Phase 8 accepted for now; stop automatic phase expansion | Dan reports pretty sure all is ok; retain known limitations and optional backlog, prepare first-playable handoff |

| 2026-09-18 | Add missing vehicle audio and prepare solo friend package as the next delivery | Ambience/UI cues did not cover the car; online two-player is possible future work, not part of this build |

| 2026-09-18 | Vehicle sounds accepted for now; download test deferred; bundle CR-022 next | Dan reports sounds working, cannot test package yet, and requests checkpoint ding/miss buzz alongside the next feature |

| 2026-09-18 | Bundle CR-022 with CR-023 AI racers and light traffic | Dan does not want a two-sound-only update; preserve single-player scope and deliver a meaningful racing update |

| 2026-09-18 | Add more speed and missed-checkpoint penalties to AI/traffic delivery | Dan wants faster driving and continuation after missing gates, not restarting; reconcile results/records and old invalidation rules |

| 2026-09-18 | Combine garage/cars/bike/ATV with competitive AI grid/difficulty | Dan wants more vehicle types; bikes/ATVs faster and tighter but vulnerable to cars and unable to meaningfully shove them |

| 2026-09-18 | Combine nine-item vehicle/racing playtest revision | Fix bike/ATV phantom braking, quiet reset feedback, local recovery, quit-to-menu, compact HUD, bigger jump, competitive mixed AI and garage colors |
| 2026-09-18 | Local recovery supersedes start-line/lap-abandon reset | Dan wants to continue after wipeouts; retain time/progress/penalties without granting forward gate credit |

| 2026-09-18 | Combine CR-034–039 into the next woodland arcade update | Dan requests black paint, satisfying smashing/property fences, stronger difficulty, four-lane Hwy 92, named breakable signs and varied woodland jumps/shortcuts |
| 2026-09-18 | Backlog separate lake/woods circuit as CR-040 | Future track starts near friend, passes Dan's house and enters woods; fun above exact realism |

| 2026-09-18 | Designated shortcuts may bypass multiple checkpoints penalty-free | Closely spaced gates must not constrain fun routes; clean shortcut time saving is the reward, failures cost time naturally, unrelated cuts still incur penalties |

| 2026-09-18 | Revise 0.6.0 from Dan's ten-item playtest | Legal shortcut penalties, airborne/backwards-facing credit, hidden distance surcharge, floating house, inaudible impacts, fence extent, traffic density, Normal/Hard pace and Jamerson jump appearance need work |
| 2026-09-18 | Use zero for authorized bypasses and flat five seconds per true missed gate | Remove confusing hidden fractional distance charges; keep explicit route/finish validation and version records |

| 2026-09-18 | Accept 0.6.1 overall except stated follow-ups; defer difficulty retuning | Dan approves for the most part but shortcut charges and motorcycle house-jump momentum remain issues |
| 2026-09-18 | Bundle CR-046–049 and BUG-008 | Reliable penalty accounting/shortcut states, more tire sound, road continuations and personal local radio; no other expansion |

---

# SESSION HANDOFF

**CURRENT DELIVERY — CR-097–100 / 0.17.0-review1:** Property/driveway repairs, summit guidance/geometry, dome tent/stone cairns and dedicated approved title art/audio are implemented for Dan review. Safety checkpoint `acafa41ce76c410e4b970dbbd7c9cf14e487fc8e`. Launch `Play-Racer.cmd` or `Builds/Latest/Racer.exe`; full ZIP `Builds/Racer-0.17.0-review1-Windows.zip`. Implementation completion `14240a16f9018c8f42e1a2943ef2473de6268a62`. All 440 runtime/Latest/extracted ZIP files match SHA256; 187 playable staged songs and two original M4A files are preserved. Final extracted startup: 21 assertions passed from the portable directory. Package identity is in VERSION.txt and Docs/CR097-100/package-final-verification.json. See [actual validation and failures](Docs/CR097-100/VALIDATION.md), [implementation](Docs/CR097-100/IMPLEMENTATION.md), and [concise Dan checklist](Docs/CR097-100/README-player.txt). Summit flights clear the crest for 4.00–5.74 seconds but still receive zero score for hard landings. Map/exploration and other unreviewed systems remain awaiting further testing. No physical Deck/controller or subjective listening claim.

**CURRENT AUTHORIZATION — CR-091–096:** Implemented in 0.16.0-review3 with compiled-player validation; full runtime/ZIP delivery is recorded below. Includes finish-gate lap recovery, property/fence/sign cleanup, permanent campsite, summit jump, relocated collectibles and persistent exploration map with safe free-roam travel. Mountain race remains future backlog. See Docs/CR091-096/VALIDATION.md for actual results and retained limitations; this is not blanket acceptance.

**CURRENT COMBINED SCOPE — 2026-09-20:** Implement CR-081 and CR-090 together. All following statements that still queue CR-081 describe prior scope and are superseded. Preserve the 0.14.0-review2 acceptance and its documented technical limitations, especially CR-087; do not claim new implementation/testing in this planning update.

**Concrete ghost scope:** Optional local personal-best clean-lap ghosts on all four course variants with eligible vehicles. Timestamp and interpolate actual poses with non-colliding visuals, not simulated controller input. Persist only completed valid clean laps without reset/teleport or missed-gate penalties; authorized shortcuts remain valid. Label the clean-lap category clearly and preserve broader existing race/lap records. Replace only with a faster valid compatible lap. Match course/direction/vehicle/route/rules/handling version, synchronize lap start and pause, handle restart/finish and corrupt or incompatible saves clearly. Preserve old data; no silent incompatible playback. Controller-accessible toggle and bounded storage/allocation cost. Verify playback timing/position, replacement, persistence and invalid-run rejection.

**Concrete collectible scope:** Add 24 recognizable free-roam collectibles across the existing area and new mountain/lake trails, mixing easy exploration and challenging reachable jump locations. The count is a bounded implementation target. Stable IDs, one-time saved discovery, pickup feedback, found/total count and controller-accessible summary. No race requirement, penalties, currency, upgrades or grind. Verify every approach and safe return/reset, duplicate contacts, partial/full completion, save/relaunch and stable discovery through world updates. Any collection reset must be explicit and collection-only.

**Concrete world scope:** One connected wooded mountainside beyond Kyle's lake, with at least two return trails, varied elevation, a gully/creek jump and another distinct challenging jump. Preserve all four race variants; no additional championship required. Correct Trickum versus separate Jamerson topology/signage rather than globally replacing names. Rebuild Dan's house and property using the CR-081 photo descriptions and actual supplied images: low brick front, downhill exposed lower rear level, raised deck/chimney, lower pool/stone landscaping, separate downhill garage/kennel and smaller pool house to its left in the described view. Refine neighboring fences/driveways with House 1 still absent. Unseen dimensions and House 2/3 facades remain approximate; future Kyle photos do not block work. Childhood wooded setting and remembered fences take precedence over modern subdivisions/later imagery. No personal likeness reconstruction from the family photos.

**Integration requirements:** Ground buildings/trees and blend terrain/driveways without slabs or hairpin obstruction. Keep the unwanted road/bridge behind the hairpin house removed. Only one coffee/football/smoking/empty household scenario globally. Civilian traffic stays on streets. Test ramps on first playable geometry and final geometry in applicable modes/directions, all eligible vehicles, intended speeds and center/off-center/edge approaches, with actual speed/contact/stability evidence and AI stuck recovery. Preserve and disclose CR-087 unresolved cases. Safe reset must return to supported reachable route terrain. Finalize geometry and compatibility versions before validating ghosts/records/collectibles; archive old incompatible record categories rather than erase or mix them.

**Combined acceptance:** Automatic visible trap/jump results in races and roam; persistent activity top tens; accurate non-colliding ghost replay on four variants; all 24 collectibles reachable and saved; connected mountain trails and tested jumps; recognizable supported home/outbuildings; four-course gate/shortcut/radio/AI/reset regressions and measured performance; complete Windows runtime/ZIP with bundled music preserved; concise playtest guide. New work remains awaiting Dan's review.

**Latest human review — 2026-09-20:** Dan approves 0.14.0-review2 except missing activity scoreboards and unseen speed-trap feedback. CR-090 adds automatic race/free-roam trap and jump results plus persistent top-ten activity boards. CR-082–089 accepted for now with documented technical limitations retained. CR-081 ghosts/collectibles/neighborhood/home expansion remains approved and queued, not canceled; split screen remains discussion only. This planning update does not implement or test game changes.

**Prior scope/evidence follows:**


**PREVIOUS DELIVERY SCOPE — Woodstock Rush correction pass:** CR-082–089 cover every latest General/Reverse Road/Reverse Forest/Free Roam report. After safety commit, test Trickum baseline first across modes/directions before other implementation. Previous all-helmet and independent household rules are superseded; use helmet-free people and one global vignette at most. Imperial display is requested. CR-081 ghosts/collectibles/neighborhood/home work remains APPROVED and queued after these corrections, not canceled or awaiting reauthorization. Game name is Woodstock Rush; internal Racer paths remain unchanged. No game edits/tests in this planning update and no blanket user acceptance inferred.

**Prior implementation and authorization history follows:**


**PREVIOUS DELIVERY SCOPE — Route fixes + arcade challenges:** Dan approves CR-075–080 together: direction-aware signage/shortcut discovery and wrong-way reliability, safe on-course player reset/stalled-AI recovery, reverse Trickum ramp and standing ramp tests, free roam, stunt scoring/challenges, speed traps. Fix and validate routes/recovery/ramps first, then integrate activities in the same delivery. CR-081 records approved following-pass ghosts, collectibles, larger neighborhood and photo-informed home work; do not omit or ask for reauthorization, but do not implement these queued features now. Previous exclusions of stunts/free roam etc. are superseded. Split screen/networking remain outside this pass. The combined implementation, retained failures, automated evidence and review boundaries are recorded in Docs/CR075-080/VALIDATION.md. Earlier document-only status is superseded; no human acceptance is claimed.

**Historical implementation evidence follows; latest human reports supersede apparent reverse-route/ramp test success:**


**Combined CR-075–080 review checklist (0.13.0-review7):**
- [x] Scoped Trickum / Hwy 92 / destination Jamerson roles; direction-specific shortcut boards, all-four route inventories and automated approach captures; retired hairpin exclusion retained.
- [x] Five-second grounded wrong-way guidance with bounded local reacquisition, including Forest street excursions.
- [x] Earned local supported recovery, complete vehicle/rider clearance, occupied-pad alternatives and bounded AI recovery; integrated race follow-up documented in the validation report.
- [x] Reproduced reverse shoulder snag; continuous collidable bevel/rollout on current authored Trickum ramps; ordinary-frame all-vehicle speed/line matrix and future standing requirement recorded.
- [x] Explicit Free Roam; race-only state/HUD suppressed, local reset and clean return to races; eligibility and ambient systems retained.
- [x] Independent jump airtime/distance points, physical jump/smash challenges, deliberate retry, saved compatible bests/medals and two-way speed traps.
- [x] Historical records retained under prior course categories; changed timing categories versioned. Atomic save replacement retry verified with isolated saves.
- [ ] Dan's driving/readability/fun, physical-controller and listening review. Automated checks do not close this item.

**Build and automated evidence:** 0.13.0-review7 Windows package and complete Builds/Latest verified through the existing workflow: 436 matching runtime/ZIP files, 187 staged songs, 2,690 source hashes; Play-Racer.cmd unchanged. Final source/commit metadata is in VERSION.txt and Builds/PACKAGE-LATEST.json. Final-build hazard recovery 282/282, roaming recovery 80/80, lifecycle rules 96/96, physical navigation 24/24 and shortcut rules 280/280. AI obstruction assertions 35/36; the retained timing failure shows an escaping tourer at 23.32m/s, not an indefinite stall. Eight serial 150-second race/roam performance runs passed with world systems/radio active; median 8.33–8.58ms, p95 8.57–17.91ms on this machine. Full multi-lap outcomes, ordinary finish-window DNFs, extended diagnostic finishes and ramp failures are documented in Docs/CR075-080/VALIDATION.md. No external upload.

**Known retained limits:** extreme high-speed motorcycle ramp-edge crashes; three failed assertions on unchanged forward Forest trail jumps, with exact conditions and successful local recovery in the evidence. Earlier candidate race DNFs/recovery loops and failed test fixtures are retained and distinguished from later follow-up runs. No subjective acceptance is claimed. CR-081 remains approved and queued below/above with all provided geography/home/photo references intact.
**Previous delivery — 0.12.0-review1: CR-070 through CR-074 implemented together, awaiting Dan review.** Safety checkpoint: `90abc69769528600ad652df1c3dbad985ae482d8`, verified before edits after supported elevated Git retry. Completion ID is reported in the delivery response and packaged VERSION.txt. Launch with the preserved `Play-Racer.cmd`; the complete `Builds/Latest` and versioned Windows runtime are delivered through `Tools/Package-Racer.ps1`. No external upload. Full evidence and limitations: `Docs/CR070-074/VALIDATION.md`; route maps and repeated timing table are linked there.

Implementation checklist (technical delivery, not Dan's acceptance):

- [x] CR-070: exact southern hairpin South Cherokee Lane / Jamerson Rd sign, its two faces and exclusive support/collider removed; scoped Road/Rd/line-variant generator/catalog exclusion and repeated restoration checks. Other signs, hairpin, driveway, woods and forward scene geometry preserved.
- [x] CR-071: prominent five-second signed-movement warning, local arrow and active R/Y reset instruction. Final rendered physical onset 5.031 / 5.041 / 5.032 / 5.097 seconds across Street / Forest / Street Reverse / Forest Reverse; all physical suites 6/6, rules 280/280. Includes bounded branch/main reacquisition, noise, pauses, stationary/spin/airborne/landing/escape reverse, shortcuts, seams and lifecycle clearing.
- [x] CR-072: stylized faces and exposed hair, open helmet faces and improved car sightlines; black/front/side/chase/garage checks, art/paint/physics 116/116. Geometry/material costs are documented; no facial animation or handling change.
- [x] CR-073: four Track menu choices with separate rules/records; Street Reverse all vehicles, Forest Reverse motorcycle/ATV. Reverse grids/gates/AI/recovery, five Forest transitions and Street reverse roadworks/bypass; original traffic lane direction. Laurel Switchbacks and Granite Creek Cut; Fern Gully and Granite Saddle. Repeated eligible-vehicle comparisons: Street 112/112, Forest 56/56; all clean trials zero recovery/misses. Clean gains respectively 7.38–10.97 / 1.52–3.94 / 5.77–5.79 / 1.21–1.32 seconds. Maps identify bypassed gates/rejoins. Jump suites 8/8 and 20/20; support layouts 21/21 each.
- [x] CR-074: occasional deer/coyotes with project-created silhouettes, idle/walk/flee and spatial calls, preserving existing species/pooling/culling/empty occupancy. Forced wildlife 46/46, audio 17/17; both species encountered in ordinary runs. Actual engine/radio listener-mix correlation and audibility measurements recorded; no racer-blocking animal colliders.

**Integrated verification:** All four fresh-save two-lap player/AI races pass 6/6, all 16 racers finish with zero missed gates; some AI/local recoveries remain (0–2 per racer). Historical record suite 29/29; reverse finish estimates 96/96 each. Final serial normal-speed combined runs pass 4/4 per course with people, wildlife, street traffic and radio: p95 frame times 13.162 / 8.929 / 12.423 / 8.642 ms at explicitly rendered 1280×720, 120 Hz cap; peak process 556–773 MiB. Staged music and complete runtime/ZIP consistency are verified by the packaging report.

**Open review/limitations:** Scripted physical driving and emulated device events are not human play/controller acceptance. Moderate errors can retain some of the larger Laurel/Fern advantage; gap-route errors erased their smaller gain. Dan should judge challenge, entrance/HUD readability, cabin views and speaker-level wildlife mix. Audio capture is before the OS endpoint; performance is offscreen/capped on this machine. The successful build has two warnings: optional Pipeline configuration and future Unity collision pre-baking requirements for 180 existing meshes; current collision tests pass. Earlier failed candidates and a reused-test-save fixture failure are retained with explanations, not cited as passing. All new work and earlier human reports remain awaiting Dan's review. No split screen, networking, stunt scoring or ghosts added.

**Prior implementation evidence follows:**


**Previous delivery — 0.11.0-review1: CR-067 through CR-069 implemented together, awaiting Dan review.** Safety checkpoint: `255a61b2095c1cad871b16640afff30eb979df22` (sandbox Git denial recovered through supported elevation before edits). The completion commit is reported in the delivery response and packaged VERSION.txt. See Docs/CR067-069/VALIDATION.md for actual evidence, initial failed fixtures and limitations. Prior human reports remain open; these checks are not blanket acceptance.

CR-067 identifies the **Remembered house behind southern hairpin**, world `(484, 23.90, -596.20)`, and the later CR-046–049 **South Cherokee Lane south** decorative extension. Its pavement overlapped the turn; its rear traffic tunnel canopy was the reported bridge-like structure. Removed that one hierarchy, 25 colliders per scene, boundary and decorative traffic, and its generating call. The house pose, accepted centerlines/branches, terrain heights/normals/triangles, lake/cave/jumps and the other three continuations are preserved. A narrow flush gravel access and six supported rear trees remain. Matching before/after overhead and low driving views are in Docs/CR067-069. Repeat scoped regeneration remains clean. Street records move to street-v10-hairpin; Forest stays lake-v3-shallows; historical saves are retained.

CR-068: unforced release tests reproduced invisible football/smokers despite selection. Football was too far back/small; smokers were too deep into the friend's property. They now occupy visible frontage positions within the original properties, with clearer figures/ball/smoke; coffee is unchanged. Saved per-course shuffled bags choose football/coffee/empty at Dan's and independently smokers/empty at the friend's. New races consume one selection, with no immediate repeats across bag boundaries; loading, laps, pause and recovery do not. Four launches / eight ordinary new races per course show all four experiences within four races, with selected occupants visibly present from sampled normal street approaches (football 3/3, coffee 2/2, smokers 2/2). All four launch suites pass 22/22. A supplemental 600-visit save/reload check confirms repetition control. **Location limit:** these original properties remain beside the neighborhood street in both scenes; the Forest racing line is about 108m from Dan's yard and does not pass them. Forest captures are explicitly neighborhood-street visits, not active-trail visibility.

CR-069: after three continuous seconds of meaningful signed wrong-route movement, a compact local-tangent arrow, Wrong Way text and active local-reset control appear. Default keyboard **R**, Xbox-style controller **Y**; device-specific display labels are retained. Speed threshold/hysteresis, ground/corridor checks, bounded main-route projection and active-branch direction avoid heading-only and distant-checkpoint guidance. Correct travel/recovery clears promptly; pause does not advance time; finish/restart/track/menu transitions clear. No reset, penalty or start-line return is forced. Both physical wrong-way pilots warn after about 3.3 seconds; emulated R/Y actually reset locally with progress/penalties retained. Controller tests are emulated, not physical hardware.

Technical results: all eight four-profile hairpin/re-entry runs complete with zero recovery or false warning; full two-lap races finish all racers with zero misses on both courses. Fresh-save race checks 6/6 each, AI estimates 96/96 each, historical records 29/29, vehicle art/paint 116/116, radio 24/24. Initial reused-save count assertions and a synthetic finish-timestamp fixture were diagnosed and retained, not hidden. Source geometry invariants are verified; no original vehicle physics, wildlife/audio, radio production formatting, water or jump route code was retuned. Rendered 1280x720/120-cap medians are 8.334ms on both courses, p95 12.297ms Street / 8.676ms Forest. Prior p95 values were 11.899/8.568ms with different random workloads; this small observed increase is disclosed, not attributed causally or claimed absent. Both remain under a 16.67ms frame budget. Human driving/gesture readability/listening/controller acceptance remains Dan's review.

Concise review checklist:
- [ ] Hairpin: four vehicles, AI, wide re-entry, local reset and restart; confirm narrow access, preserved house and wooded rear without remnants/snags.
- [ ] Households: eight new races plus relaunch, both scenes' original street locations; see coffee/football/smokers/empty, recognizable actions and no swaps during a pass.
- [ ] Wrong way: sustain reverse-route travel then correct; check arrow/R/Y, hairpin, cave/shortcuts, lap seam, spins/landings, stopping/brief reverse, pause/recovery/restart.
- [ ] Preservation: records/history, garage/colors/art, AI estimates/mistakes, radio/music, signs, water, wildlife/audio and jumps; test physical controller.

No split screen, networking, stunt challenges, ghosts or external upload. Final clean build: zero errors, one existing optional pipeline warning. Packaged Latest smoke: Street 68/68 and Forest 36/36; physical wrong-way pilots 6/6 each. All 429 runtime files and 187 staged songs match across Latest/versioned/extracted ZIP, with 2,514 source hashes verified. Complete package evidence is recorded in the validation report; Play-Racer.cmd and deliberately staged music are preserved through Tools/Package-Racer.ps1.

**Previous implementation evidence follows:**


**Previous delivery:** 0.10.0-review1 implements CR-064–066 together; all remain awaiting Dan review. Safety checkpoint: `1192d22080a6439968ade7ee5aad9fa0ff96ce21` (saved TODO changes committed before edits after supported elevated Git retry). Completion source is recorded in Builds/Latest/VERSION.txt. See Docs/CR064-066/VALIDATION.md for methods, evidence, limitations and full review checklist. Earlier human reports remain pending; CR-063 split screen remains feasibility discussion only.

- [x] Persisted Estimate AI at your finish option (default Off), plus post-finish Pause > Skip waiting; explicit AI-only classification, route/branch/lap distance, robust moving median or documented course/profile/difficulty fallback, penalties once, Estimated labels, no fabricated records, idempotence and restart. Standalone checks: 96/96 per course.
- [x] Revised four original cartoony models: coherent bodies, arches, detailed wheels, glazing, lights/bumpers, motorcycle/ATV mechanics, seated helmeted occupants and steering/lean. Seven paints and physics/clone isolation: 116/116. Renderers bounded at 20/20/14/20 and 6–7 used shared materials. Front/side/chase and garage captures include black. Physics, colliders and handling remain unchanged.
- [x] Investigated old trim/rider assertions: only six water-ripple LineRenderers have Unity-owned nonempty blocks, with paint properties unset. Replaced the incorrect empty-block assumption with before/after material and shader-property invariance for every non-body renderer, including water. No water block cleared or renderer excluded. Existing full flow now 137/137.
- [x] Folder-name Radio announcements, General Radio and Radio Off; plain Artist/Song lines, Unknown Artist/filename fallback, long/Unicode handling, readable async hold and stale-load rejection. New suite 24/24; existing radio controls/scanning/history suite 16/16.
- [x] Preserved records (29/29), Forest rules/recovery (28/28), Street signs (44/44), Forest signs/cave warning (49/49). The obsolete jump fixture remains 6/12 in both this build and the unchanged 0.9.1 baseline; exact failures and current authored-ramp evidence are retained, not called passing.
- [x] Normal two-lap races with estimation Off: 6/6 checks per course, all four racers measured, zero gate misses. Current Forest ramps: 12/12 nominal motorcycle/ATV runs completed with landings. Authored Street connector: 8/8 ordinary-frame flights across four profiles landed and credited gate 12; seven had no penalties, while full-speed ATV subsequently incurred one gate miss / five seconds, reproduced in the unchanged 0.9.1 player. Normal-rate rendering at 1280×720: Street median/p95 8.334/11.899 ms; Forest 8.334/8.568 ms. Prior p95 9.361/8.335 ms used different random populations, so this is an observed comparison, not an isolated art benchmark.
- [ ] Dan: review near/far estimated AI, normal waiting/skip action, persistence/restart and measured human results.
- [ ] Dan: approve all four vehicle/driver appearances and black paint in garage and racing; inspect hands/feet, cabin fit and camera clearance.
- [ ] Dan: review station changes and labeled song lines; actual listening, physical-controller feel and both courses remain human review tasks.

No networking, split-screen implementation, stunt challenges, ghost mode, asset purchase or external upload. Play-Racer.cmd and the existing BundleMusic packaging workflow remain the entry point and music source. Complete versioned runtime, Latest and extracted ZIP match all 429 files by SHA256, including 187 staged songs; no executable-only replacement. Final source/ZIP metadata is in Builds/Latest/VERSION.txt and Builds/PACKAGE-LATEST.json.

**Previous implementation evidence follows:**


**Previous delivery:** 0.9.1-review1 implements CR-061/062 together and awaits Dan's review. Safety checkpoint: `0553614da30e2bbcd0fe4619ee5ba1aa302aba97`. Git's sandbox staging denial was recovered through the supported elevated retry before edits. Completion source is recorded in Builds/Latest/VERSION.txt.

Recovered historical lettering from `8fee1f5475e6c2b9ee0a9eb646cd2e61e40c4664`: 17 restored plus 22 retained storefront faces on each track. Physical road names, jump/shortcut advice and stores survive generation/build and break/restore/restart; only known unmounted labels are removed. Forest adds the exact physical warning `Warning: Cave Ahead Enter at your Own Risk`, readable in the captured chase approach. All 28,539 existing scene transforms retain their poses.

Added blue songbirds, squirrels and frogs, with a fixed pool, at most eight selected, varied idle/flee motion and offscreen activation. CC0 recorded calls and wing foley replace weak synthetic bat chirps. Final 48 kHz listener captures with actual radio playback and normal engine/music settings establish all four sound waveforms in the mix, no clipped samples, and exact zero output under Master mute. Frog placement was brought nearer the trail after initial masking evidence. Bat linger/rearm cooldown, ambience mute, local recovery and restart are tested. Captures are before the OS endpoint: no subjective listening or speaker/headphone acceptance is claimed.

Read Docs/CR061-062/VALIDATION.md and its release evidence for results and limitations. Art remains simple procedural shapes and short animations. Prior water/AI/people/traffic/garage/colors/records/radio systems and Play-Racer.cmd are retained. Existing packaging preserves staged music and replaces the whole Latest runtime; no external upload. CR-063 remains discussion only; networking, stunts and ghosts are not implemented.

Current delivery technical results: physical signs 49/49 Forest and 44/44 Street; audio 13/13; corrected wildlife fixtures 42/42; Forest preservation 28/28. Existing full flow suite is 133/137: four non-body trim/rider property-block assertions also fail in the previous 0.9.0 player. These are retained pre-existing limitations, not passing claims; saved colors/audio settings and recovery checks pass. The first ordinary run exposed a culling defect, corrected by preparing the bounded pool offscreen before approach and skipping distant animation. See final ordinary captures/results in Docs/CR061-062 for varied sightings/empty periods and the camera-frustum counting limitation. Final build has zero errors and one existing optional Runtime Pipeline warning.

Final ordinary runs: Forest 15 occupied / 135 empty one-second samples, Street 12 / 138; seven distinct runtime sighting events each, natural seeds and normal time scale. With radio/people/traffic active, 1280x720 offscreen p95 was 8.335 ms Forest and 9.361 ms Street at a 120 fps cap. Samples are camera-frustum based, not occlusion-aware or subjective recognition. Both final ordinary checks pass 4/4. Source manifest: 2,488 files; package retains 187 staged songs.

Concise sign/wildlife playtest:

- [ ] Drive both tracks and read road/storefront/shortcut signs; confirm no floating labels returned.
- [ ] Read the exact cave warning on approach; hit a sign and check lettering follows destruction/restoration/restart.
- [ ] Look for birds, squirrels and frogs across several races, with varied sightings and empty stretches; assess animation and no visible popping/snags.
- [ ] Listen to species and bats over normal engine/radio; mute/unmute Ambience/Master and reopen to check saved settings.
- [ ] Linger/recover/revisit cave to assess cooldown, brief flight and clear turns/jumps.
- [ ] Check combined performance, neighborhood people/street traffic, water, eligibility, recovery, garage/colors and historical top-ten records.

New work and all earlier unresolved human reports remain awaiting Dan's review.

**Prior delivery evidence:**


**Previous delivery:** 0.9.0-review1 combines CR-057–060 and awaits Dan's review. Shallow supported water and drive-out shores, depth-based shared player/AI resistance, removal of floating labels, seeded AI judgment errors, pooled cave bats and variable football/coffee/smoking/pedestrian scenes. The accepted Forest layout, jumps/cave, motorcycle/ATV eligibility and Street course are retained. Stunts and ghosts remain unselected. Read Docs/CR057-060/VALIDATION.md for authoritative tests and the combined checklist.

Safety checkpoint: `8fee1f5475e6c2b9ee0a9eb646cd2e61e40c4664`. Both Git permission failures were recovered through successful elevated retries before edits.

Actual combined results: 86/86 scripted water/labels/occupancy/bat checks in `probe3`; all 108 AI entries finish the 36-race two-lap matrix (three seeds, both tracks, all difficulties, paired errors-disabled baselines). There are 272 bounded judgment events: 131 wide-line, 121 early-braking and 20 jump-approach events, decreasing by difficulty. AI recoveries total eight with errors versus six without. One Normal Street AI misses a gate; one Easy Street reference-pilot entry DNF's in traffic (35/36 reference entries finish). These are retained limitations, not blanket passing claims. Whole-race mean timing differences range from -5.545 to +8.518 seconds and include traffic/recovery interactions; they are not isolated per-error penalties. The final jump regression completes all 12 motorcycle/ATV runs, and record preservation checks pass 29/29.

Preservation: 28,539 retained scene transforms have unchanged poses; Forest road/cave/jump/gate data is identical. Street route points/gates are retained. Water fixes also remove the old closure plane through the Forest lake and a steep west-shore seam. New comparable record categories are `street-v9-life` and `lake-v3-shallows`; historical boards/saves remain available. Garage/colors, local recovery, radio channels, staged music and Play-Racer.cmd are preserved.

Ambient limits: 35 pooled figure slots, at most 33 selected figures; 24 highway versus four sparse residential slots; eight pooled bats. Forced seeds 1/3 show football with/without smokers, 9/13 coffee with/without smokers, and 2/4 empty Dan with/without smokers. Clock-seeded runs show ordinary variation. Figures are non-colliding procedural low-poly art with modest gestures and short walking loops; no full IK, facial animation or dialogue. Water/bat audio is synthetic. Human driving, subjective listening, physical-controller quality, gesture readability and fun remain unverified and awaiting Dan.

Final follow-up checks: 68/68 actual water/lifecycle checks, including dry-shore entry, immersed turning/stopping/reversing out, player/AI feedback, sound dispatch, pause/recovery/restart/track cleanup and bat rearm. Final scene checks pass 185/185 Forest and 157/157 Street across all six household combinations, foot support and traffic/trail clearance. Corrected coffee placement is outside the accepted front fence; football stays inside the yard. Final captures are in Docs/CR057-060/scenes-forest and scenes-street; earlier household captures are explicitly superseded. The final Windows build succeeds with zero errors and one existing optional Runtime Pipeline configuration warning.

Rendered local performance at 1280×720/120 fps cap: Forest p95 8.404 ms, Street 10.488 ms, and the final eight-bat cave fixture 8.367 ms. People, track traffic and radio playback were enabled. Forest finished four riders; Street was a 180-second performance window, separate from the full-race matrix. These offscreen measurements do not establish foreground/controller acceptance. Final runtime/Latest/ZIP package consistency is recorded in Docs/CR057-060/package-final.json and the metadata-stamped Builds/PACKAGE-LATEST.json; source manifest covers 2,458 files. New work remains awaiting Dan's review.

Concise combined playtest:

- [ ] Enter lake/creeks on motorcycle and ATV; stop, turn, reverse, exit several shores and recover. Confirm temporary resistance, partial immersion and unaffected airborne jumps/dry ramps.
- [ ] Drive both tracks without floating labels; confirm visible gates, usable HUD/results/radio and unchanged ordinary penalties.
- [ ] Race Easy/Normal/Hard; assess occasional errors, natural recovery, competition and cave reliability.
- [ ] Check brief bat approach/scatter/audio; linger, reverse, recover and revisit without spam or control interference.
- [ ] See football, coffee, smokers and empty visits; assess feet/props, in-yard ball, highway/South Cherokee density, stable occupancy and no visible pop-in/snags.
- [ ] Restart/change tracks, review historical records/garage/colors, and test traffic plus music performance with the physical controller.


**Previous delivery:** 0.8.0-review1 implements CR-056 Forest Loop and awaits Dan's review. Launch Play-Racer.cmd / Builds/Latest/Racer.exe. Read Docs/CR056/VALIDATION.md for final evidence, limitations, route map and seven-comment checklist. CR-040 remains unaccepted; original street/radio/records are preserved and wildlife is deferred.

**Prior implementation evidence follows; earlier completion claims do not validate the latest feedback:**


**Current delivery — 0.7.0-review1: CR-040, CR-054 and CR-055 implemented together, awaiting Dan review.** CR-050 through CR-053 remain preserved. Safety checkpoint: `522917554b0bbc7c8eebf664790b2c8573d0a88f`; completion commit is reported in the delivery response and runtime VERSION.txt. See Docs/CR040-054-055/VALIDATION.md for actual evidence and limitations. Earlier exclusions below are historical.

Launch Play-Racer.cmd or Builds/Latest/Racer.exe. Main menu > Track selects Street Loop / Lake & Woods. Main menu or Results > Records / Top 10 opens Lap/Race boards and saved-category selectors. The new scene has its own 1.91 km circuit, seven active gates, three supported jumps/creeks and a marked Birch Hollow shortcut. The original scene, accepted building poses, handling/difficulty and core racing/traffic/destruction systems remain unchanged.

Automated evidence: both tracks finished two laps with all four profiles/AI and zero misses; 96/96 original route attempts, 24/24 lake route attempts and 12/12 targeted lake jump runs passed, with 40 explicit route recoveries. Records: 29/29 checks. Radio: 16/16 generated-fixture checks, plus 4/4 extracted portable/DSP checks. Failed first-candidate geometry and the corrected tests are retained in the validation report. These are not human driving, actual listening, native-picker interaction or physical-controller acceptance. BUG-004 and other outstanding human reports remain open.

Radio: gameplay D-pad Down / M cycles every channel and Off; Left/Right or [ / ] selects previous/next song inside the active channel; Up / I shows channel and artist/title. Settings > Music provides equivalent controls, independent volume, source selection and rescan. Fresh preferences use Music beside Racer.exe. Immediate folder names are channels; nested albums remain in their parent channel; root tracks form General. Explicit existing custom-source and Off preferences are retained.

Repackage music without Unity: copy deliberately selected supported songs into BundleMusic/<Channel>/<Artist>/<Album>, then double-click Package-Racer.cmd, or run `pwsh -NoProfile -File Tools/Package-Racer.ps1 -Version 0.7.0-review1` from the project root. Existing staged music is preserved. The script preserves previous complete Latest/runtime/ZIP under Builds/Preserved before replacement and verifies extracted/runtime hashes. Songs added only to Latest are preserved in that backup; copy selected ones into BundleMusic and package again to include them. Custom collections are never copied automatically. Personal audio stays out of Git; nothing is uploaded.

Concise Dan checklist:
- [ ] CR-040: select both tracks, drive every vehicle over multiple laps with AI; review jump fun/landings, shortcuts and local recovery.
- [ ] CR-054: inspect Lap/Race boards, category separation, ties/dates/new PB/top-ten entries and save/relaunch behavior.
- [ ] CR-055: listen to folder channels/nested albums, cycle through Off, skip/back, rescan and volume; test physical D-pad and menu isolation, then repackage/extract elsewhere.
- [ ] Regression: garage/colors, HUD, difficulty/handling, traffic variety, destructibles, gates and race flow.

**Authorization for this delivery (historical planning):** Complete CR-040 lake/woods track and CR-054 top-ten lap/total-race boards omitted from 0.6.3-review1, together with CR-055 default bundled folder and folder-named radio channels using one channel/Off cycle button. Earlier exclusions below describe the prior task only and do not override this authorization. Preserve delivered CR-050 through CR-053 and existing difficulty/handling. This planning update does not claim implementation or new testing.

**Previous delivery evidence follows:**
**Previous delivery — CR-050 through CR-053 only: 0.6.3-review1, awaiting Dan review.** The current task explicitly excludes starting lake/woods, stunt challenges, ghosts or multiplayer. CR-054 records boards were also outside this task. Earlier saved planning notes below are retained as history, not blanket authorization or acceptance for this pass. Difficulty/handling constants and house/road/shortcut geometry are unchanged. BUG-004 and earlier outstanding human checks remain open.

Safety checkpoint: `04447830beeb1563ab94f71c60bfec3bb1ce4520`. Git permission denials were safely retried with supported elevation. Completion commit is reported in the delivery response and runtime VERSION.txt. Launch `Play-Racer.cmd`; complete runtime is `Builds/Latest`, versioned runtime/ZIP is `Builds/Racer-0.6.3-review1-Windows`. No upload/distribution occurred.

Gate labels match internal IDs. Old CP06 moves from 1427 m to 1680 m (180 m after Creek rejoin), consolidating old CP07. Old CP09/10 consolidate into new CP08 at 2330 m (180 m after Fox rejoin). Seventeen numbered gates remain. All detection, labels, bypass arrays, HUD/AI ordering and lap counts share the revised array. Creek bypasses 4/5; Fox 6/7; Pine 8/9/10. Old record categories remain intact; new runs use street-v8-landings. Before/after renders and actual-geometry map: Docs/CR050-053.

Ambient traffic now uses original low-poly sedan, wagon/SUV, open-bed pickup and tall van bodies with nine plausible paints, weighted shuffled selection, per-body collision bounds and offscreen-only appearance recycling. Population/pace/lane settings remain unchanged for highway/local roads; decorative-extension cars use the same pool. Actual route/race outcomes, including any collision-related AI delay, are recorded in VALIDATION.md rather than treated as human difficulty acceptance.

Music: Settings > Music / local radio > Music source / collection setup. Bundled music uses Music beside the installed executable; Custom folder retains the chosen collection root. Include subfolders defaults On; source/root/recursion persist. One cancellable background scan reports found/skipped/limited work. Explicit bounds replace silent 2048 truncation (100,000 tracks / 250,000 entries / 20,000 folders / 30 seconds; per-file codec caps retained). Generated nested WAV/MP3/Ogg tests, large discovery, cancellation, rescan, directory-loop exclusion and portable extracted playback are documented.

**Adding songs to the shareable ZIP:** place chosen songs/subfolders in project `BundleMusic`, then double-click `Package-Racer.cmd` (or run `pwsh -NoProfile -File Tools/Package-Racer.ps1 -Version 0.6.3-review1`). No Unity recompile is needed. Only staged music is included; selected external collections are never copied. The script preserves previous complete Latest/runtime/ZIP under Builds/Preserved, printing the path. To bundle songs added directly to old Latest/Music, copy the chosen files from that preserved folder into BundleMusic, resolve conflicts, and package again. Future deliveries must use this preservation workflow. Generated test songs were moved out of staging; Dan has not staged songs yet. Personal audio/staging remain ignored by Git. See Docs/CR050-053/RADIO.md.

Automated results: 96/96 extended physical route/road attempts, zero misses/penalties, 32 recoveries; 111/111 rules, 99/99 combined and 137/137 preservation-flow checks; 41/41 recursive-collection checks and 4/4 extracted portable playback checks. These are not human listening/controller/playtest acceptance. Full traffic-race outcomes and timing limits remain in Docs/CR050-053/VALIDATION.md.

Short Dan checklist: (1) both affected shortcut exits at speed/wide/airborne with every vehicle; (2) fall/reverse/local recovery and multiple laps, zero authorized misses; (3) one deliberate road miss = one +5s, pause/results ledger reconciles; (4) varied traffic bodies/colors without visible recycling or congestion; (5) collection root/artist/flat folder, recursion toggle, rescan and controls/volume; (6) add selected bundled songs, repackage, extract elsewhere and listen; (7) garage/colors, compact HUD, destructibles and race flow. Physical controller testing, actual listening and subjective playability remain Dan's checks. Do not close them from automated results.


**Earlier saved planning note — 2026-09-19 (not the scope of this delivery):** Next combined delivery includes authorized lake/woods second track CR-040, pending CR-050 through CR-053 (gates 6/9, traffic variety, recursive libraries, bundled music), and CR-054 separate persistent top-10 lap/race boards. BUG-004 remains open; positive feedback is not blanket test acceptance. Preserve difficulty and original course. Stunt challenges, ghosts, multiplayer and exact house work remain deferred. New requests are not yet implemented.

**Prior build handoff follows (historical evidence, not completion of the new requests):**


**Previous delivery: 0.6.2-review1 — awaiting Dan review.** Combined BUG-004/008 and CR-046–049 implementation is complete. Safety checkpoint `5cc1d94fe84a9035db34dc688588bee940dc986e`; completion commit is reported in the delivery response and Builds/Latest/VERSION.txt. Open Play-Racer.cmd or Builds/Latest/Racer.exe; full local package Builds/Racer-0.6.2-review1-Windows.zip. Complete-runtime/source/ZIP verification: Docs/CR046-049/PACKAGE-VERIFICATION.txt.

Explicit shortcut entry credit survives imperfect driving/recovery; genuine misses are5s exactly once with aggregate notices and paged ledger. Five gates relocated; record category street-v7-entitlement preserves historical saves. House-floor contact/facade correction carries motorcycle momentum without a boost. Stronger grounded tire feedback, supported decorative road closures/traffic and local music radio are integrated. Difficulty unchanged; no multiplayer/new track/house relocation.

Evidence:96/96 physical route/road runs, all4 vehicles/4 routes, zero misses;3-lap mixed race4/4 finishes/zero misses/one local recovery; 116/116 accounting checks, 99/99 standalone combined checks and 137/137 preservation-flow checks. Additional measured reverse/deviation/recovery results and technical limits are in Docs/CR046-049/VALIDATION.md. Actual DSP samples verify MP3/WAV/Ogg and tire/engine/music mixing; no subjective or physical-controller acceptance claimed. Exact14-miss incident remains unidentified, so keep the work awaiting Dan rather than closing BUG-004 from automated results.

Radio: Settings > Music / local radio > Open Music folder, add your own songs in Explorer, then Rescan; custom folder chooser available. Default `%USERPROFILE%/AppData/LocalLow/DefaultCompany/Racer/Music`. D-pad right/left/up/down = next/previous/show/toggle during racing; keyboard ]/[ /I/M. Music has independent volume under Master and continues through pause/menus/restart/Quit Race. Format/cap/license details: Docs/CR046-049/RADIO.md. No personal music/manifests are committed or packaged; nothing distributed externally.

Automated results: 96/96 extended physical route/road attempts, zero misses/penalties, 32 recoveries; 111/111 rules, 99/99 combined and 137/137 preservation-flow checks; 41/41 recursive-collection checks and 4/4 extracted portable playback checks. These are not human listening/controller/playtest acceptance. Full traffic-race outcomes and timing limits remain in Docs/CR050-053/VALIDATION.md.

Short Dan checklist: imperfect shortcuts and ordinary+5s misses/ledger; motorcycle/ATV house momentum then both cars; tire audibility with radio; all road closures and traffic; local folder/skip/history/control/volume behavior and driving hitches; preserved garage/colors/recovery/race flow/records. Physical controller, native chooser, listening and Dan acceptance remain outstanding. Exact photo-based homes and lake/woods work remain backlog.

**Previous implementation evidence (0.6.1-review2):** The following records describe the prior build, not validation of the newly requested work. The latest user report supersedes the apparent shortcut-test success; BUG-004 remains open. These historical commit IDs do not replace the required safety checkpoint for the next session.

Safety checkpoint: 88064bd196d68e40bfb624f2d5879613d962dbf5, verified before changes. Git permission failures were safely retried through supported elevation. No changes discarded, hooks/signing bypassed or history rewritten. Executable source:566b4180fa633d182deb7559ae31c025f36ab52f. The completion commit is reported in the delivery response; it contains final evidence/documentation for this source.

Open Play-Racer.cmd or Builds/Latest/Racer.exe. Full Windows package:Builds/Racer-0.6.1-review2-Windows.zip. All231 staged/extracted/Latest files hash-verified; launcher visibly opened the correct Latest on Dan's desktop. VERSION metadata matches executable source. Saved vehicle/colors/volumes remained unchanged; old builds and records preserved. Nothing uploaded/distributed.

Race credit uses bounded swept movement, persistent earned shortcut context, sustained partial-rejoin handling and movement direction rather than body heading. CP14 alone has the24m upper jump envelope. Each genuine miss is exactly+5 seconds; authorized bypass zero. street-v6-flat5 records preserve historical categories. Broad cuts can save more than their flat charge; there is no hidden replacement punishment. Original0.6.0 build/source identity was verified, but its limited existing harness did not reproduce Dan's exact false-penalty/silent-impact incident.

Final user-desktop validation:80/80 ordinary-frame route/road attempts across all four routes/profiles,48 branch exits,16 local recoveries, zero misses/charges/buzzes. All12 gully attempts broke both panes.111/111 rule/dynamic checks plus77/77 route invariants. All8 Jamerson flights landed/credited CP14; fast ATV's later CP15 miss was genuine+5 and remains disclosed.137/137 menu/recovery/settings/restoration/results flow checks passed. Reversal/abandonment combinations use swept fixtures in addition to ordinary-frame recovery; no exhaustive human-driving/controller claim.

The exact gully residence is now a supported44m/6m-rise drive-through house. Narrow-doorway and overlapping-crossbeam failures were retained before the final20m opening/continuous ramp fix.169 perimeter fence sections surround the three properties; all46 other building transforms and player profile/motor tuning remain unchanged. Jamerson uses supported pavement roadworks with a bypass. Before/after views are linked in Docs/CR041-045/VALIDATION.md.

Audio:15 physical authored-prop DSP mix captures with engine throttle at default/lowered/muted settings; default/lowered nonzero, mute exactly zero, no clipping. Attenuation, gains and bounded voice handling improved; oriented restoration fixes wide diagonal glass. No OS-endpoint capture or subjective listening performed. Dan's speaker-level impact assessment remains required.

Traffic:16 dedicated Hwy92 +4 local cars, about22 passes/minute, mean11.5–11.7 cars observed on highway, temporary2–17 occupancy due safe recycling.294 measured recycles at least220m from player; no traffic recoveries. Normal/Hard full races both finished4/4 with zero misses/recoveries. Best laps improved for every profile; Hard tourer/motorcycle total times regressed in congestion. Easy constants unchanged. Actual lap/sector/gap/mistake results and limits:Docs/CR041-045/AI-RACES.md. Automated pilots do not establish human difficulty.

Comparable foreground performance:one visible standalone,1280x720, VSync off,120fps cap, one Normal lap, same mixed roster. Old4-traffic median/p95=8.33/8.35ms, peak working set482.2MiB. New20-traffic median/p95=8.33/8.34ms, peak480.6MiB. This is a capped local comparison, not a worst-case FPS guarantee. Build:zero errors; existing future collision-prebake warning and intentionally absent optional Runtime Pipeline warning retained.

Dan's ten-item checklist is in Docs/CR041-045/VALIDATION.md:shortcut legality; airborne/backwards credit; flat penalties; Normal/Hard/Easy feel; gully stunt/recovery; property perimeters/access; audible impacts/mute; busy four-lane traffic; environmental Jamerson jump/bypass; preserved garage/menu/recovery/results/records. CR-040 remains backlog. That checklist is historical; proceed only with the newly requested combined pass above and await review of its build.

---
# How Dan and ChatGPT Will Use This File

At the beginning of every Astra/Codex work session:

1. Astra/Codex must inspect the current project and create a Git commit of all saved changes before modifying anything.

At the end of an Astra/Codex work session:

1. Update the checkboxes Astra completed.
2. Paste Astra's final summary into `SESSION HANDOFF`.
3. Add any bugs under `BUG TRACKER`.
4. Add personal complaints/changes under `CHANGE REQUESTS`, even if they seem minor.
5. Make a completion Git commit once the phase work is stable.
6. Bring this `.md` file back to ChatGPT.
7. ChatGPT will:
   - review what changed;
   - decide whether the current phase actually passes;
   - update priorities;
   - write the next Astra/Codex prompt;
   - split difficult fixes into manageable tasks;
   - update this project plan;
   - keep later phases from ballooning prematurely.

The file is the project's source of truth. We do not casually rebuild the project plan from memory.



Delivery performance evidence: sequential 1280x720 offscreen normal-speed races passed both courses with all four vehicles; p95 8.441 ms Street / 8.350 ms Lake at a 120 Hz cap, peak working sets 528.54 / 555.17 MiB. Foreground presentation, human driving/listening and physical controller review remain pending. Final records regression suite: 29/29.

Release package verified: 428 matching runtime/Latest/extracted ZIP files and 187 deliberately staged tracks; BundleMusic hashes unchanged, prior complete Latest preserved. Final metadata-stamped report: Builds/PACKAGE-LATEST.json. No external upload or personal audio in Git.
