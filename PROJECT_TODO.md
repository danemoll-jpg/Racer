# Racer
## Project Management / TODO / Astra-Codex Handoff

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

These rules apply throughout the project.

**Consistent testing entry point:** Dan launches `Play-Racer.cmd` at the project root, or `Builds/Latest/Racer.exe`. After each new validated playable build, update the complete `Builds/Latest` runtime folder (including VERSION.txt); never replace only its executable. Keep older versioned builds separately. Current combined review build is 0.6.2-review1; Builds/Latest/VERSION.txt records its source. BUG-004/008 and CR-046–049 are implemented awaiting Dan review; technical validation does not close the earlier human reports. See Docs/CR046-049/VALIDATION.md and its short combined playtest checklist. A source commit alone does not update a compiled player.

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

**Status:** ACCEPTED — Dan answered yes to the full Phase 7 review checklist. Core countdown/results/persistent bests/pause/settings/navigation/audio accepted; input device was not specified. Phase 6 remains accepted. Evidence and limits: `Docs/Phase7/VALIDATION.md`; controls, timing, save/settings and audio rules: `Docs/Phase7/RULES.md`. Optional stunt scoring/speed traps remain deferred; Phase 8 is next, starting with CR-019 commercial layout and coordinated frontage polish.

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
- [ ] Optional stunt scoring exploration.
- [ ] Optional speed traps exploration.

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

These are NOT required for the first game.

- [ ] Optional feasibility follow-up: private online play for Dan and a friend. Not authorized for implementation yet; preserve solo-first scope. Need shared car/race/prop state, latency/disconnect handling and two-PC testing; choose platform/join/host behavior before implementation.

- [x] CR-027: Initial four-vehicle roster implemented: existing car, Longroof GT, Needle 600 motorcycle and Trail Four ATV; awaiting Dan review.
- [x] CR-027: Persistent garage, reusable profiles and vehicle-separated records; technical checks pass, awaiting Dan review.
- [ ] CR-040: Future separate lake/woodland circuit near the friend's house; not part of this delivery.
- [ ] Larger neighborhood.
- [ ] CR-039: Three varied woodland stunt shortcuts (stream leap, gully route, ridge/woodland jump), integrated with legal race progress.
- [ ] Free-roam mode.
- [x] CR-023: Light configurable two-way traffic implemented; awaiting review.
- [x] CR-023 initial three-opponent implementation; Dan reports opponents not visible/competitive enough. CR-026 revises grid, visibility and difficulty with CR-027.
- [ ] Collectibles.
- [ ] Speed traps.
- [ ] Danger signs / jump-distance challenges.
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
**Status:** IMPLEMENTED IN 0.6.2-review1 — awaiting Dan review; not accepted/closed.
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

### CR-040 — Future lake/woodland circuit behind friend's house
**Status:** BACKLOG — later track, not an additional deliverable in the current woodland batch.  
**Concept:** Separate circuit beginning near the friend's house, using the lake behind it, passing Dan's house and continuing through existing woods with jumps, gullies and creeks. Prioritize fun over realism. Lake position/size and exact route need authoring review when this item starts; do not move homes or construct the lake automatically now. Reuse validated woodland sections where useful while keeping distinct start/gates/records per course.  
**Result:** Deferred; preserve room and avoid unnecessary architectural coupling, but do not implement a track editor or second full circuit now.

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
**Status:** IMPLEMENTED IN 0.6.2-review1 — awaiting Dan review; not accepted/closed.
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
**Requested change:** Add persistent user-owned music library via Settings with a default writable Music folder and Open Folder/Rescan actions, plus choosing another folder. Players drag files into that folder in Explorer; literal OS file-drop into game is optional only if reliable. No Unity reimport/rebuild for new songs. Support and test common DRM-free local audio formats (prioritize MP3, WAV and Ogg; document verified limitations). Read artist/title tags where available with a maintained license-compatible metadata reader; use safe filename fallback, never claim AudioClip.name is artist metadata. Shuffle without immediate repeats where possible, next/previous history, play/pause or on/off, brief artist-title toast on track change and on demand. Gameplay-only D-pad Right=next, Left=previous, Up=show track, Down=toggle; inspect bindings and preserve D-pad menu navigation, with keyboard/settings alternatives. Add independent persistent Music volume under Master; keep race/smash/tire cues audible. Bounded async loading/streaming/cache, safe corrupt/missing/removed files and empty library, no main-thread stalls or full-library decoding. Define pause/menu/restart/quit behavior consistently. No network, DRM service integration, uploads, original-file edits, or packaging/git inclusion of user music/local library manifests. Use generated or redistributable test clips/tags; ship empty library instructions, not personal songs. Metadata displayed as text, not interpreted markup.
**Combined result:** Local-folder MP3/WAV/Ogg radio with ATL metadata, shuffle/history, compact title toast, gameplay D-pad and keyboard bindings, folder/open/rescan/chooser/settings actions, persisted independent Music volume/radio preference. Bounded async local-only reads; empty/corrupt/missing/removed cases tested. Native picker, physical controller and subjective listening remain Dan checks. Setup: Docs/CR046-049/RADIO.md.

---

# DECISION LOG

Record choices we do not want to repeatedly reconsider.

| Date | Decision | Reason |
|---|---|---|
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

**Current delivery: 0.6.2-review1 — awaiting Dan review.** Combined BUG-004/008 and CR-046–049 implementation is complete. Safety checkpoint `5cc1d94fe84a9035db34dc688588bee940dc986e`; completion commit is reported in the delivery response and Builds/Latest/VERSION.txt. Open Play-Racer.cmd or Builds/Latest/Racer.exe; full local package Builds/Racer-0.6.2-review1-Windows.zip. Complete-runtime/source/ZIP verification: Docs/CR046-049/PACKAGE-VERIFICATION.txt.

Explicit shortcut entry credit survives imperfect driving/recovery; genuine misses are5s exactly once with aggregate notices and paged ledger. Five gates relocated; record category street-v7-entitlement preserves historical saves. House-floor contact/facade correction carries motorcycle momentum without a boost. Stronger grounded tire feedback, supported decorative road closures/traffic and local music radio are integrated. Difficulty unchanged; no multiplayer/new track/house relocation.

Evidence:96/96 physical route/road runs, all4 vehicles/4 routes, zero misses;3-lap mixed race4/4 finishes/zero misses/one local recovery; 116/116 accounting checks, 99/99 standalone combined checks and 137/137 preservation-flow checks. Additional measured reverse/deviation/recovery results and technical limits are in Docs/CR046-049/VALIDATION.md. Actual DSP samples verify MP3/WAV/Ogg and tire/engine/music mixing; no subjective or physical-controller acceptance claimed. Exact14-miss incident remains unidentified, so keep the work awaiting Dan rather than closing BUG-004 from automated results.

Radio: Settings > Music / local radio > Open Music folder, add your own songs in Explorer, then Rescan; custom folder chooser available. Default `%USERPROFILE%/AppData/LocalLow/DefaultCompany/Racer/Music`. D-pad right/left/up/down = next/previous/show/toggle during racing; keyboard ]/[ /I/M. Music has independent volume under Master and continues through pause/menus/restart/Quit Race. Format/cap/license details: Docs/CR046-049/RADIO.md. No personal music/manifests are committed or packaged; nothing distributed externally.

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


