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

**Focused building batch:** ACCEPTED — Dan says it looks fine. Photo-based home accuracy is deferred to CR-013 and does not block further work. CR-012 removes House 1 and extends Dan's yard into its site; Houses 2/3 retain their labels and exact placement. All 25 retained residences and 22 businesses now use reusable low-poly architecture. Phases 2–5 remain accepted. This building batch is accepted; remaining Phase 6 categories are still pending. No Phase 7. Full evidence and limitations: `Docs/PHASE6_VALIDATION.md`.

## TODO
- [x] CR-012 implementation: Remove only the house marked #1, its dedicated collision and obsolete house-specific props/access; expand Dan's yard naturally into its former site. Retain Houses #2/#3 without renumbering, and preserve their positions/elevations.
- [x] Verify the expanded yard is continuous, grounded and free of invisible House #1 collisions; update relevant generation logic so House #1 does not return.
- [x] Improve house/building silhouettes in this focused batch: 4 residential + 4 commercial reusable variants, grounded entrances and aligned simple collision.
- [x] Dan visually approves this building batch and CR-012; physical-controller testing remains unconfirmed.
- [x] Initial tree appearance pass: three reusable crown shapes, coherent variation, unchanged 6,102 placements/colliders and 90 visual batches. Historical report: Docs/VEGETATION_VALIDATION.md. CR-014 below expands this coverage.
- [x] Implement CR-014 woodland expansion: 6,102 to 11,897 trees, connected canopy with explorable trunk spacing; protected accepted sites and gameplay retained. Awaiting Dan's review.
- [x] Profile baseline, intermediate and full coverage in ordinary frames; repeat 1440x900 standalone comparisons and rendering-cost diagnostics. Results/variability/limits: Docs/CR014/VALIDATION.md.
- [ ] Dan approves woodland coverage, off-road access and smoothness after CR-014; vegetation batch remains unaccepted and Phase 6 incomplete.
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
- [ ] CR-013 (optional backlog): Refine homes from Dan's reference photos if he finds and supplies them; no photos required to continue current work.
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
**Status:** IMPLEMENTED — AWAITING DAN'S REVIEW of forest coverage, driveability and smoothness. CR-014 remains open; vegetation batch unaccepted; Phase 6 incomplete.  
**Baseline behavior:** The visual refresh kept 6,102 placements and added no trees. Dan says the neighborhood should be much more forested because much of it was woodland; he wants to drive through it and is concerned about frame rate.  
**Requested change:** Substantially increase the area covered by connected woodland and improve its dense wooded appearance, not simply crown detail or uniform tree count. Keep navigable spaces between solid trunks, supported ground and ordinary off-road access. Avoid making the woods an impassable wall, a visual backdrop, or a set of narrow prescribed corridors. Natural obstacles and slopes may remain; not every gap must fit the car. Use canopy coverage and restrained non-colliding understory where useful.  
**Preserve:** Expanded yard/former House 1 site, Houses 2/3, all accepted building sites, road/shoulder support, existing shortcut/jump clearances, vehicle tuning and race systems. Free off-road traversal does not authorize new named shortcuts or relaxed checkpoint validation.  
**Performance:** Establish a comparable baseline, then profile increased coverage in stages. Measure ordinary-frame road and forest driving, multiple representative camera locations, frame-time spikes, CPU/GPU bottlenecks and memory where available. Use representative window/resolution settings and record them; the prior 734x293 samples and variable host timings do not establish performance at Dan's normal settings. Choose optimizations from measured bottlenecks; keep visual/collision consistency and safe collision availability during driving. Record actual results, hardware/settings and limits rather than promising a frame-rate target not yet supplied.  
**Acceptance:** Dan judges the neighborhood substantially more wooded, can explore between trees and rejoin roads predictably, and finds performance acceptable. Compare before/after coverage and measured smoothness; test accepted gameplay and yard protections.  
**Result:** Added 5,795 trees in two measured stages (8,999 then 11,897 total), retaining all original 6,102. Expanded central removed-subdivision land, eastern woods and southern woodland. Actual projected canopy coverage central/east/south rose 10.77/8.97/9.18% to 43.76/45.55/40.74%. New trunks are 0.75m wide with minimum 6.8m center spacing; mature overlapping crowns preserve protected clearances. Kept 90 shared-material spatial batches; forest triangles 416,952 to 810,312. Fixed stale GPU mesh buffers in the vegetation refresh. Every visible trunk aligns with solid collision; all accepted non-forest scene records unchanged. Three ordinary-frame slow forest drives each covered about 150m, plus contact/reverse/turn/reset, forest-road crossing, ordinary-frame jump/shortcut and existing race regressions. Repeated 1440x900 standalone measurements show a modest rendering cost with substantial host variation and unresolved spikes; player allocation approximately 253 to 292MiB. No universal frame-rate target, parity or speedup claimed. See Docs/CR014/VALIDATION.md for matched views, raw tests, settings, performance and limitations.

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

---

# SESSION HANDOFF

**Current phase:** Phases 2–5 and the Phase 6 building batch remain ACCEPTED. CR-012 CLOSED; CR-013 OPTIONAL BACKLOG. CR-014 is implemented and AWAITING DAN'S REVIEW of coverage, driveability and smoothness. Vegetation remains unaccepted. Phase 6 is incomplete; Phase 7 and other environment categories were not started.

**Safety checkpoint:** `fcd82ea507732b35f5b025ae842d7fbd3a991636`. Initial staging failed with permission denied creating `.git/index.lock`; the elevated retry succeeded before any project content changes. Completion commit is reported in the task response.

**Scene:** `Assets/Scenes/StreetLoopGreybox.unity`, saved/reloaded, outside Play mode, not dirty. All 11,897 trunk colliders and 90 forest renderers enabled. Suggested exploration: central removed-subdivision woods west of Dan's street (x -300, z 260), southwestern interior (x -430, z -200), and eastern woods (x 590, z 100). These are test locations, not new named shortcuts.

**Coverage:** Added 5,795 trees to the original 6,102 in two measured stages: 8,999 then 11,897 total. Central/eastern/southern projected canopy coverage 10.77/8.97/9.18% to 43.76/45.55/40.74%. Broad connected woods and irregular glades replace the sparse appearance. New mature crowns overlap overhead; new 0.75m trunks retain at least 6.8m center spacing from any other trunk. Original close clusters remain. No understory clutter, pass-through trees or distance-based collision activation.

**Rendering/workflow:** Shared indexed low-poly crown/trunk assets, one material, 90 spatial 160m batches. Forest triangles 416,952 to 810,312; serialized vegetation meshes approximately 109MiB. Fixed stale GPU buffers in the prior mesh refresh with explicit buffer replacement/upload. `CR014Woodland.Plan/Apply` only handles the saved woodland layout; replanning is guarded after placement. `Phase6Vegetation.Refresh()` remains the repeatable forest-only visual refresh. Building-site and House 3 sightline exclusions now join the yard/road/shortcut exclusions. No whole-environment rebuild was run. Benchmark/driving tools are opt-in; normal game systems and tuning were not changed.

**Preservation:** Only the forest parent's child list changed among retained saved-scene records; old forest render batches were replaced and new trunks added. All original trunks and every accepted building, label, terrain/road, shortcut, jump, car/camera/input/reset/race/HUD record match the checkpoint. 95,176 visible trunk/collider corners match at 1mm rounding. Zero yard trunks or unsupported new tree bases; 1,458 protected-corridor volume probes pass. All 47 building sites and 1,579 expanded-yard support probes pass. Terrain visible/collision meshes and seams remain unchanged.

**Actual driving:** Three ordinary-frame virtual Gamepad forest drives each covered about 150m in 40s at roughly 3.5–4m/s, maximum path error 0.89–1.05m and upright at least 0.988. Contact/braking/reverse/turn/reset check passed against a new solid trunk. A 118m forest-road crossing stayed supported/upright. Ordinary-frame jump from rest: 31.79m/s takeoff, 1.74s flight, landed upright; shortcut: 24.31m/s peak, 2.88m path error, upright 1.000, camera/HUD active. Three manually stepped mixed racing-speed laps passed (551.52s, 25.42m/s mean, 39.71m/s peak), alongside all 37 race/input/HUD/reset/restart checks and jump/bypass/shortcut recovery tests. The original sharp-grid 7m/s profile paths hit trees in two areas; those are not claimed as traversal passes or comparable moving-view timings. Slower smoothed test trajectories navigate existing gaps without scenery changes. Physical-controller testing remains unperformed.

**Performance:** GTX 1660 Ti, i7-9700, 65,341MB system RAM, Unity 6000.6.1f1, PC quality, D3D11, vSync 0, target -1. Normal Editor 734x293 samples captured before/stage1/full; supplemented with repeated visible 1440x900 Windows development-player runs. Standalone road medians before 6.92/11.25ms vs final 4.34/11.10ms; p95 15.38/15.52 vs 12.35/16.64ms. Southwestern drive medians before 5.23/12.61 vs final 3.22/12.53ms. Dense fixed view varied from final 13.87/7.80ms to later 4.26/4.48ms with no forest reduction, demonstrating host/runtime-state variability. An immediate rendering-off/on comparison with all colliders retained measured about 0.38–0.95ms additional median rendering cost; physics p95 about 0.16–0.18ms. Player allocation approximately 253 to 292MiB (+39MiB); final multi-route reserved capacity reached 800MiB. Intermittent spikes remain (final standalone up to ~90ms; Editor ~128ms). No frame-rate guarantee, parity or improvement claimed. Full coverage retained; further GPU/LOD/batch-size investigation remains an option if Dan dislikes smoothness.

**Build/Console:** Representative standalone testing completed using a build-only workaround that omits an already-disabled SSAO renderer feature whose stripped resources broke the first player. The accepted Editor feature list is restored. Initial hidden-player and failed-renderer timings are explicitly invalid. Final corrected build succeeded with 0 errors / 1 Pipeline runtime-configuration warning; final live Console 0 errors / 1 warning, compilation successful. Earlier tooling timeouts and renderer-restoration serialization errors are recorded; restoration now uses the feature-reference list and its verified build is clean. Player startup post-processing stripping warnings remain a limitation. Build-generated PC prefilter settings were restored; Unity serialized a default zero-intensity Bloom filter and its runtime-settings cache without intentional visual/quality changes.

**Evidence and remaining review:** `Docs/CR014/VALIDATION.md` contains six matched before/after coverage pairs, staged canopy measurements, raw driving/performance reports, preservation evidence and reproducible tools. Stylized forest floor/crowns, distant terrain speckling and existing fast angled-jump limits remain. Not every gap or slope was tested; no physical controller, GPU capture, or controlled host-load benchmark. Dan should explore from ordinary shoulders, test turns/reverse/trunk contact/re-entry, check the expanded yard and Houses 2/3, drive a normal lap/shortcut/jump/bypass, and judge smoothness at normal settings. Keep CR-014 and vegetation awaiting his review; earlier approvals remain intact.

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
