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
- Replace those newer developments mainly with woods/open land.
- The area around Dan's childhood house should be sparse: Dan's house, houses 1/2/3, the friend's house across the street, and plenty of wooded space.
- Additional houses may be placed approximately/randomly in sensible locations where exact memory is unavailable.
- There should be a placeholder house behind the hairpin turn for now because Dan remembers one there. A lake may be explored later as a gameplay change, but not in Phase 2.
- The storage facility is not important and does not need to be preserved accurately.
- The main road can have a reasonably busy commercial/business feel.
- The connecting road can have several houses along it.
- The far/end portion of Dan's road can also have several houses.

## Terrain / Elevation Direction

Dan's street is a hilly/mountain road, although not an extreme high mountain.

From the **first yellow circle to the last yellow circle** is the main hilly street section.

- Coming from the main road onto Dan's street, there is a **fairly large uphill climb**.
- After the initial climb, the road flattens somewhat but continues with **smaller rolling hills up and down** throughout the street.
- The **second yellow circle** marks a **fairly large downhill drop**. Dan and his friend used to sled down this hill when it was icy.
- The **last yellow circle** marks the end of Dan's street where the road descends more gradually; it is downhill but not especially steep.
- The rest of the loop is relatively flatter than Dan's street.

Elevation character is a major part of making the location recognizable and should not be reduced to a flat map.

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

- [ ] Create the full rough road loop shown in red.
- [ ] Establish approximate real-world scale suitable for driving.
- [ ] Shape terrain to reflect the remembered hills and elevation changes.
- [ ] Add the significant uphill entrance onto Dan's street.
- [ ] Add rolling smaller hills along Dan's street.
- [ ] Add the larger downhill drop at the second yellow circle.
- [ ] Add the gradual downhill near the last yellow circle.
- [ ] Add Dan's childhood house as a simple placeholder mass.
- [ ] Add houses 1, 2, and 3 as simple placeholder masses.
- [ ] Add the friend's house across the street.
- [ ] Add the placeholder house behind the hairpin turn.
- [ ] Replace marked newer subdivisions/development with wooded/open areas.
- [ ] Add approximate/random houses where appropriate on the connecting road and other remembered residential areas.
- [ ] Give the main road a basic commercial/business character.
- [ ] Preserve `PrototypeTrack` and create a separate greybox scene for the real loop.
- [ ] Test the entire loop with the Phase 1 vehicle.
- [ ] Adjust road width/curve smoothness only as much as needed for comfortable driving.
- [ ] Confirm the result is recognizable enough to Dan.

## Tell Astra/Codex

> **Before making any changes, inspect the current project state and create a Git commit of all current saved changes as a safety checkpoint. Do not modify anything until that commit succeeds.**
>
> We are continuing work on the Unity project **Racer**.
>
> First:
> 1. Inspect the current Unity project.
> 2. Read `PROJECT_TODO.md` in the project root and treat it as the source of truth.
> 3. Inspect the map reference files in `Docs/References/` and use them as the primary visual references for Phase 2.
>
> Dan has reviewed Phase 1 and says the car seems to drive fine, so we are cleared to begin **Phase 2 only**.
>
> ## Phase 2 Goal
> Build a **drivable greybox version of the full real loop** Dan remembers from childhood.
>
> This is not a current-day accurate map recreation. It should represent **Dan's remembered older version of the area**, not the modern built-up neighborhood layout.
>
> ## High-level route guidance
> Use the annotated map as the main guide.
>
> - The **red route** shows the full loop to recreate.
> - The loop is based on real roads; do not invent a separate connector road merely to close the circuit.
> - The route includes the main road, the connecting road, Dan's neighborhood/mountain-road section, and the lower return section that completes the real loop.
>
> ## Annotated map guidance
> - **Blue X** = Dan's childhood house.
> - **Blue 1, 2, 3** = the other three original houses near Dan's house.
> - **Blue circle across the street** = Dan's friend's house; preserve it.
> - The other blue circle is not important.
> - **Green-marked areas** should feel wooded/open instead of filled with newer subdivisions.
> - **Black X marks** indicate modern roads/development to remove or ignore for this remembered version.
> - **Yellow circles** identify important road/elevation areas described below.
>
> ## Environment and memory guidance
> Prefer Dan's remembered version of the place whenever memory and the current map conflict.
>
> - Remove or ignore newer subdivision roads and newer dense development where marked.
> - Replace much of those removed areas with **woods/open land**.
> - Around Dan's childhood house, preserve a sparse feel: Dan's house, houses 1/2/3, the friend's house across the street, and substantial wooded space.
> - Add a few additional houses approximately/randomly in sensible locations where Dan does not remember exact placement.
> - Put a simple placeholder house behind the hairpin turn for now.
> - The storage facility is not important and does not need accurate recreation.
> - The main road may have a reasonably busy commercial/business feel.
> - The connecting road may have several houses along it.
> - The far/end portion of Dan's road may also have several houses.
>
> ## Terrain / elevation guidance
> Dan's neighborhood street is a **hilly/mountain road**, though not an extreme mountain.
>
> From the **first yellow circle to the last yellow circle** is the main hilly street section:
>
> - coming from the main road onto Dan's street, there is a **fairly significant uphill climb**;
> - after that, the road somewhat levels out but still has **multiple smaller rolling hills**;
> - the **second yellow circle** marks a **fairly large downhill drop**;
> - the **last yellow circle** marks the end of the street where it slopes downhill more gradually.
>
> The rest of the loop is relatively flatter.
>
> This elevation character is important and should be reflected in the greybox terrain and road profile.
>
> ## Implementation requirements
> Build a **simple greybox** only:
>
> - simple road geometry
> - simple terrain/ground shaping
> - simple placeholder houses/building masses
> - simple placeholder woods/trees if useful
> - simple materials only
>
> Preserve and reuse the existing Phase 1 vehicle so the loop can be driven immediately.
>
> Create a **new scene** for this phase, preferably `StreetLoopGreybox`, while preserving `PrototypeTrack` for reference/testing.
>
> ## Accuracy priorities
> Prioritize in this order:
>
> 1. Recognizable road layout and full real loop structure.
> 2. Recognizable terrain/elevation feel.
> 3. Dan's childhood house, houses 1/2/3, and the friend's house.
> 4. A wooded, older, less-developed feeling.
> 5. Driveability with the Phase 1 vehicle.
> 6. Geographic precision is secondary.
>
> If exact memory and current map data conflict, prefer **Dan's remembered version**.
>
> If needed for playability, make only **small** adjustments to road width, curve smoothness, or spacing, and clearly report them.
>
> ## Important scope limits
> Do NOT add:
>
> - start/finish line
> - checkpoints
> - lap system
> - timer
> - HUD
> - shortcuts
> - stunt ramps or jumps
> - breakable-fence gameplay
> - major visual polish
> - formal circuit/race systems
>
> Phase 2 includes the full real closed road loop, but it is still only a **memory-based drivable environment greybox**.
>
> ## Testing
> Test that:
>
> - the new scene loads correctly,
> - the Phase 1 car can drive the full loop,
> - the major hill sections are traversable,
> - there are no major collision/blocking problems,
> - there are no compile errors,
> - and the Console ends cleanly.
>
> Update `PROJECT_TODO.md` for Phase 2 only where work is actually completed.
>
> Update the `SESSION HANDOFF` section with:
> - current playable state,
> - what was added/changed,
> - any known issues,
> - any places where geometry had to be exaggerated or simplified,
> - any areas where exact placement was guessed,
> - and what Dan should test next.
>
> When the work is stable, create a new Git commit for the completed Phase 2 implementation.
>
> Do not begin Phase 3.
>
> ## Final report
> At the end, give me:
>
> - the name of the scene to open,
> - what roads/terrain/features were built,
> - what landmarks were included,
> - where you simplified or guessed,
> - what files/scenes/prefabs/scripts were created or changed,
> - whether the full loop is drivable,
> - whether the Console has zero errors,
> - and exactly what Dan should test manually.

## Acceptance Test

- [ ] The full real road loop exists and is drivable.
- [ ] Dan recognizes the road layout.
- [ ] The initial uphill onto Dan's street feels significant.
- [ ] Smaller rolling hills are present along the street.
- [ ] The larger downhill drop feels recognizable.
- [ ] The gradual downhill at the road's end feels appropriate.
- [ ] Dan's house and houses 1/2/3 are represented.
- [ ] Friend's house across the street is represented.
- [ ] Newer subdivisions are substantially removed/replaced with woods/open land.
- [ ] Main road/commercial area feels appropriately more developed.
- [ ] Connecting road has suitable residential development.
- [ ] Car can drive the complete loop without major collision problems.
- [ ] No compile errors.
- [ ] Dan approves the greybox as recognizable enough to proceed.

### Street / Terrain Notes

- Key landmarks: Dan's childhood house; houses 1/2/3; friend's house across the street; placeholder house behind hairpin.
- Things that must remain recognizable: full real road loop; hilly character of Dan's street; sparse wooded childhood-house area.
- Things we can fictionalize/approximate: exact placement of less-important houses; details of businesses on main road; minor vegetation/building placement.
- Possible later gameplay alteration: consider replacing the house behind the hairpin with a lake if that proves more fun, but **not during Phase 2**.
- Bugs:
- Dan's review notes:

---

# PHASE 3 — Add Race Systems to the Real Loop

## Goal
Turn the already-built real closed road loop into a functioning race course by adding race systems while preserving the Phase 2 environment.

## TODO
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

> Begin Phase 3 only. Add race systems to the existing real closed-loop greybox.
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
> Do not add final shortcuts or spectacular jumps yet.
>
> Test several complete laps before finishing.

## Acceptance Test
- [ ] Player can repeatedly complete valid laps.
- [ ] Invalid lap skipping is reasonably prevented.
- [ ] Restart works.
- [ ] HUD works.
- [ ] Circuit has enjoyable flow.
- [ ] Original street is still recognizable.

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

### BUG-001
**Status:**  
**Phase introduced:**  
**Description:**  
**Steps to reproduce:**  
**Expected behavior:**  
**Actual behavior:**  
**Screenshots/logs:**  
**Astra fix attempt(s):**  
**Result:**  

---

# CHANGE REQUESTS

Use this for things that are not bugs but that Dan wants changed.

### CR-001
**Status:**  
**Current behavior:**  
**Requested change:**  
**Reason:**  
**Phase affected:**  
**Result:**  

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

---

# SESSION HANDOFF

Update this section before bringing the file back to ChatGPT.

**Current phase:** Phase 2 — ready to begin  
**Last completed task:** Phase 1 hands-on review completed; Dan reports the car seems to drive fine. Phase 1 accepted for handling/camera/braking/jump feel.  
**Current playable state:** `PrototypeTrack` remains playable with the Phase 1 placeholder car, keyboard/controller input, chase camera, ramps, and reset system.  
**Astra's latest summary:** Phase 1 implementation and validation completed previously. Dan has now completed the required subjective driving review and accepted the current vehicle feel.  
**New bugs:** None reported by Dan during the Phase 1 hands-on test.  
**Things Dan wants changed:** Begin Phase 2 using the full real road loop from the annotated map. Recreate the older remembered environment rather than current-day subdivisions. Preserve the key houses and hilly terrain described in the Phase 2 section.  
**Questions/uncertainties:** Exact placement of less-important houses can be approximate. The placeholder house behind the hairpin should remain for now; a possible lake there is a later gameplay idea. Physical Xbox controller acceptance remains unconfirmed unless Dan separately tests it.  
**Ready for next phase?:** Yes — Phase 2 is authorized.  

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
