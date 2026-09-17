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
- **Blue 1, 2, 3** = the other three original houses in the immediate area around Dan's house.
- **Blue circle across the street** = Dan's friend's house; this still exists and should remain.
- The other blue circle is not important.
- **Green-marked areas** = areas that should feel wooded/open rather than filled with newer subdivisions.
- **Black X marks** = modern roads/development that should be removed or ignored for the remembered version.
- **Yellow circles** = important elevation/road-profile areas described below.

## Environment / Era Direction

This is **not a current-day reconstruction**. Prefer Dan's remembered older version of the area whenever modern map data conflicts with memory.

- Remove/ignore newer subdivision roads and dense newer neighborhood build-out where marked.
- Replace those newer developments mainly with woods/open land; substantially increase trees so the area feels heavily forested.
- The area around Dan's childhood house should be sparse: Dan's house, houses 1/2/3, the friend's house across the street, and plenty of wooded space.
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
- [x] Three nearby original houses identified.
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
- [x] Add houses 1, 2, and 3 as simple placeholder masses.
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
- [x] Dan's house and houses 1/2/3 are represented.
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

- Key landmarks: Dan's childhood house; houses 1/2/3; friend's house across the street; placeholder house behind hairpin.
- Things that must remain recognizable: full real road loop; hilly character of Dan's street; sparse wooded childhood-house area.
- Things we can fictionalize/approximate: exact placement of less-important houses; details of businesses on main road; minor vegetation/building placement.
- Possible later gameplay alteration: consider replacing the house behind the hairpin with a lake if that proves more fun, but **not during Phase 2**.
- Bugs: BUG-001 was addressed in the revision and passed the recorded technical checks; Dan has now accepted the track overall. No new track issue reported.
- Dan's earlier review notes (addressed by the accepted revision): Initial hill too small; second drop too shallow/slow; final hill too drawn-out/subtle; too few rolling hills; House 3 needs greater setback and steep downhill placement; friend's house needs less setback and slight downhill placement; more random houses; businesses on both sides of main street; substantially more forest. See CR-001 through CR-009.

---

# PHASE 3 — Add Race Systems to the Real Loop

## Goal
Turn the already-built real closed road loop into a functioning race course by adding race systems while preserving the Phase 2 environment.

**Status:** Ready to begin after Dan's Phase 2 acceptance. Include CR-010 as a small tuning task alongside race systems; do not delay Phase 3 for a separate handling phase.

## TODO
- [ ] CR-010: Make steering a little tighter/more responsive during ordinary road driving, preserving predictable high-speed handling.
- [ ] Confirm the Phase 2 real loop has suitable race flow.
- [ ] Make only minor race-flow adjustments if necessary.
- [ ] Establish start/finish area.
- [ ] Add checkpoint system.
- [ ] Add lap counting.
- [ ] Add race timer.
- [ ] Add wrong-way/course validation where needed.
- [ ] Add race restart.
- [ ] Add basic HUD for lap/time/checkpoint status.
- [ ] Complete multiple test laps.

## Tell Astra/Codex

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
- [ ] Player can repeatedly complete valid laps.
- [ ] Invalid lap skipping is reasonably prevented.
- [ ] Restart works.
- [ ] HUD works.
- [ ] Circuit has enjoyable flow.
- [ ] Original street is still recognizable.
- [ ] Steering feels a little tighter to Dan without twitchiness or loss of predictable high-speed handling.
- [ ] Steering changes preserve controller/keyboard input, reverse, braking, suspension, camera and reset behavior.

---

# PHASE 4 — Jumps and Arcade Spectacle

## Goal
Make the course delightfully irresponsible.

## TODO
- [ ] Select first major jump location.
- [ ] Build takeoff ramp naturally into environment.
- [ ] Build safe landing zone.
- [ ] Tune vehicle air control if needed.
- [ ] Improve suspension/landing behavior if needed.
- [ ] Add reset logic for failed jumps.
- [ ] Add second stunt feature only after first works.
- [ ] Test jumps at different speeds.

## Tell Astra/Codex

> Begin Phase 4 only. Add the first major Forza Horizon-style stunt jump to the existing circuit.
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
- [ ] Jump is fun.
- [ ] Successful landings feel satisfying.
- [ ] Failed jumps do not ruin the race permanently.
- [ ] Jump does not destabilize ordinary car physics.
- [ ] Lap logic remains valid.

### Jump Ideas / Notes
- Jump #1:
- Future jump:
- Ridiculous idea worth trying later:

---

# PHASE 5 — Intentional Shortcuts and Alternate Routes

## Goal
Allow players to discover faster/riskier ways around parts of the circuit.

## TODO
- [ ] Add first obvious-but-risky shortcut.
- [ ] Add shortcut-aware checkpoint logic.
- [ ] Add off-road surface behavior if useful.
- [ ] Add breakable/lightweight barriers where appropriate.
- [ ] Add shortcut re-entry that is safe and readable.
- [ ] Measure whether shortcut saves time.
- [ ] Add second shortcut only after first is fun.
- [ ] Prevent unintended giant course cuts.

## Tell Astra/Codex

> Begin Phase 5 only. Add ONE intentional shortcut to the existing circuit.
>
> This game should reward exploration like an arcade open-world racer. A shortcut may go through a yard, dirt path, alley, field, construction area, or another invented path that fits the current map.
>
> Requirements:
> - It must be a deliberately valid route, not an exploit.
> - It should save some time when driven well.
> - It should carry some risk or require more skill than the standard road.
> - Existing lap/checkpoint logic must recognize it as valid.
> - Prevent unrelated massive course cutting.
> - Use placeholder visuals if necessary.
> - If barriers are needed, simple breakable fences/signs/objects are acceptable.
>
> Implement and test ONE shortcut before adding others.
>
> Tell me the normal-route time versus shortcut time from your testing if measurable.

## Acceptance Test
- [ ] Shortcut is clearly usable.
- [ ] Shortcut is genuinely faster when executed well.
- [ ] It does not invalidate lap logic.
- [ ] Player can rejoin the circuit cleanly.
- [ ] Unintentional cuts remain controlled.

### Shortcut Ideas / Notes
- Shortcut #1:
- Shortcut #2:
- Shortcut #3:

---

# PHASE 6 — Track Personality and Destructible Environment

## Goal
Replace the sterile prototype feeling with a playful environment.

## TODO
- [ ] Improve house/building silhouettes.
- [ ] Add trees/vegetation.
- [ ] Add fences.
- [ ] Add mailboxes/signs/street furniture.
- [ ] Add breakable lightweight props.
- [ ] Add road markings.
- [ ] Add sidewalks/curbs where useful.
- [ ] Improve terrain transitions.
- [ ] Improve lighting.
- [ ] Add basic environmental audio.
- [ ] Preserve frame rate.

## Astra Direction
Do this in small batches. One visual/environment category per request rather than a giant beautification pass.

---

# PHASE 7 — Racing Game Feel

## Goal
Make the prototype feel like a game rather than a Unity demonstration.

## TODO
- [ ] Countdown.
- [ ] Finish/result screen.
- [ ] Best lap tracking.
- [ ] Personal best saving.
- [ ] Speed display.
- [ ] Better reset feedback.
- [ ] Pause menu.
- [ ] Settings.
- [ ] Controller-friendly UI navigation.
- [ ] Audio feedback.
- [ ] Optional stunt scoring exploration.
- [ ] Optional speed traps exploration.

---

# PHASE 8 — Visual Polish

## Goal
Improve appearance only after gameplay works.

## TODO
- [ ] Replace important placeholder assets.
- [ ] Improve materials.
- [ ] Improve vegetation.
- [ ] Improve road surfaces.
- [ ] Improve lighting/time-of-day.
- [ ] Add subtle post-processing.
- [ ] Optimize shadows.
- [ ] Add LODs where necessary.
- [ ] Profile performance.
- [ ] Fix visual popping/collision mismatch.

---

# PHASE 9 — Optional Expansion

These are NOT required for the first game.

- [ ] Additional cars.
- [ ] Car selection.
- [ ] More circuits.
- [ ] Larger neighborhood.
- [ ] More stunt areas.
- [ ] Free-roam mode.
- [ ] AI traffic.
- [ ] AI racers.
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
**Status:** Planned — include alongside Phase 3 race systems; not a blocker to starting Phase 3.  
**Current behavior:** Dan finds steering a little loose while driving the accepted roads.  
**Requested change:** Make a modest steering-response/precision adjustment using existing tuning where possible. Preserve progressive input and stable high-speed handling; avoid a physics overhaul or unrelated system changes. Record before/after values and test typical bends, hairpin, hills, faster sections and reverse with keyboard/controller inputs as available.  
**Reason:** Improve driving feel without holding up the next phase.  
**Phase affected:** Phase 3; Phase 2 remains accepted.  
**Result:** Not implemented in this documentation update. Dan will review steering feel with the Phase 3 build.

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

---

# SESSION HANDOFF

**Current phase:** Phase 2 ACCEPTED; Phase 3 ready to begin with race systems and CR-010 steering refinement. This update changes planning only; no Unity work performed.
**Safety checkpoint:** `b8d7f62c37af9da718769d28295b417531ca118d` committed Dan's saved TODO before edits. Completion commit contains this handoff; its ID is reported in the task's final response.
**Scene:** `Assets/Scenes/StreetLoopGreybox.unity`. Saved and reloaded. Open it and press Play. Fixed spawn remains at the northern entrance, adjusted vertically with the ground.
**Completed work:** One continuous visible/collision heightfield replaces floating road/shoulder slabs. Same X/Z loop and landmark sequence. Initial hill approximately 8→82 m; second drop 82→27 m with stronger short descent (peak grade 54%); final 29→8 m descent concentrated into about 134 m; frequent neighborhood rolls. House 3 now 113.54 m from road centre and 28 m below it; friend 37.85 m away and 3 m below. Twenty approximate residences, 22 businesses (11 per main-road side), 6,118 low-poly trees in 90 batches. All measurements are interpretive estimates. House 3 has a deliberately steep hillside and locally blended outer shoulder; normal shoulders blend to 16 m from centre. No horizontal route changes.
**Preserved:** PrototypeTrack, PrototypeCar.prefab, handling/tuning, suspension, input/controller bindings, chase camera and reset source are unchanged. No race systems, HUD, shortcuts, ramps, breakable fences or polish phase.
**Rebuild:** From saved StreetLoopGreybox outside Play, use Racer > Rebuild Phase 2 Feedback Environment. This regenerates the three named environment roots. Change tuning in StreetLoopBuilder/StreetLoopRevision; generated-root hand edits are replaced. Racer > Validate Phase 2 (Play mode) tests the full loop; Racer > Validate Phase 2 Shoulders (Play mode) tests re-entry.
**Actual verification:** Original unsafe re-entry reproduced. Final full-loop drives: 564.1/564.6 simulated seconds, maximum centre error 2.82/2.78 m, minimum upright 0.880, zero airborne steps. Final shoulder matrix 180/180 passed at 3/8/15 m/s, both sides and nominal 25/55-degree approaches; zero under-road or airborne steps. Full lateral support scan and sampled prop-clearance checks passed. Virtual keyboard, R reset and camera checks passed. Final reload/compilation/Console results are recorded in Docs/PHASE2_REVISION_CONSOLE.txt. Physical controller was NOT tested.
**Visual/performance checks:** Reviewed overhead, chase-camera and road-height landmark captures. Final visible meshes and colliders share geometry, including refreshed GPU buffers. Live editor forest comparisons showed no material regression in short 180-frame samples; dense-forest on/off median 5.01/7.07 ms and p95 14.31/13.98 ms at 734x293. Noise and low resolution limit this finding; no standalone/full-resolution performance certification. Temporary test state restored.
**Iteration history / limits:** Initial straight crossing tests left shoulder corridors on bends and reached trees/steep slopes; these exploratory failures are preserved separately. A curved-path upright failure near House 3 led to a local slope fix, then the final matrix passed. Strong 54% short grade, approximate placements and soft 2 m-grid road edges need Dan's judgment. Maximum-speed and arbitrary deep off-road trajectories remain unverified. Only Dan can accept feel/recognizability.
**Full evidence and all-ten-item retest checklist:** `Docs/PHASE2_REVISION.md`, associated PHASE2_REVISION_*.txt and Revision_*.png. Earlier PHASE2_VALIDATION.md / PHASE2_TEST_RESULTS.txt remain historical evidence, not proof of safe re-entry or acceptance.
**Dan's latest review:** Played through the track; it seems fine. Requests slightly tighter steering on roads during the next phase, without holding up progress.
**Next authorized implementation scope:** Phase 3 race systems plus CR-010. Use the Phase 3 commit-first prompt. Preserve the accepted environment and keep steering changes small and reversible.
**Dan's next tests:** After Phase 3 implementation, review laps/checkpoints/timer/HUD/restart and compare steering precision on ordinary bends, the hairpin and faster sections. Confirm stability, camera and reset remain comfortable; physical controller testing is still separate.
**Ready for next phase?:** Yes. Dan has accepted Phase 2. Phase 3 is cleared to begin; Phase 4 remains out of scope.

**Previous implementation summary (historical, before Dan's test):** Added the traced 4.65 km loop, significant entrance climb, rolling hills, steeper second-circle descent, gradual lower return, interpolated ground, Dan's house plus houses 1/2/3, friend's house, hairpin house, nine approximate residences, five main-road businesses, and simple woods/open land replacing later subdivisions. No race systems, HUD, shortcuts, ramps, or polish pass.
**Previous validation (historical; not proof of off-road re-entry or current acceptance):** Both full-loop drives passed with real Phase 1 physics at conservative speeds (about 560 simulated seconds each). Maximum road-centre error 2.84 m; minimum upright dot 0.971. Zero missing road-support samples or blocked corridor samples. Scripts compiled; saved scene reloaded and played. Final live Console: 0 errors, 0 warnings. See `Docs/PHASE2_VALIDATION.md` and `Docs/PHASE2_TEST_RESULTS.txt`.

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


