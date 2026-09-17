# Childhood Street Racing Project
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
- [ ] Save a known-good baseline commit.

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
- [ ] Baseline has been preserved in Git.

---

# PHASE 1 — Arcade Driving Prototype

## Goal
Prove that driving feels fun before building the real street.

## TODO
- [ ] Create a simple placeholder car.
- [ ] Implement acceleration.
- [ ] Implement braking/reverse.
- [ ] Implement steering.
- [ ] Implement basic traction/grip.
- [ ] Implement arcade-friendly stability.
- [ ] Create chase camera.
- [ ] Add Xbox-style controller support.
- [ ] Keep keyboard controls available for testing.
- [ ] Add reset/respawn button.
- [ ] Create a flat test area.
- [ ] Add a few temporary turns/ramps to test handling.
- [ ] Tune driving until approved.

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
- [ ] Car is fun enough to drive for several minutes.
- [ ] Controller works.
- [ ] Camera behaves properly.
- [ ] Reset works.
- [ ] Car can use ramps without physics going berserk.
- [ ] No compile errors.

### Tuning Notes
Add observations here:
- Handling:
- Steering:
- Braking:
- Camera:
- Jump behavior:
- Bugs:

---

# PHASE 2 — Childhood Street Greybox

## Goal
Build a recognizable but deliberately simplified version of the street.

## Inputs Dan Will Provide
- [ ] Street/location information or map reference.
- [ ] Approximate start/end points to recreate.
- [ ] Important landmarks/houses/features worth recognizing.
- [ ] Photos or screenshots if useful.
- [ ] Notes about what can be fictionalized.

## TODO
- [ ] Create rough road alignment.
- [ ] Establish approximate scale.
- [ ] Add terrain/ground.
- [ ] Add rough lots/building masses.
- [ ] Add obvious landmarks as simple placeholder geometry.
- [ ] Test entire street with the Phase 1 vehicle.
- [ ] Adjust widths/turn radii for enjoyable driving.
- [ ] Confirm the result feels recognizable enough.

## Tell Astra/Codex

> Begin Phase 2 only. We are creating a GREYBOX version of my childhood street from the references I provide.
>
> Accuracy priority:
> 1. Recognizable road layout and important landmarks.
> 2. Good driving scale and sight lines.
> 3. Geographic precision is secondary.
>
> Build the street using simple geometry and placeholder materials. Do not spend time making houses beautiful yet.
>
> Keep real-world inspiration separate from gameplay modifications wherever practical so we can alter the circuit later without rebuilding everything.
>
> The Phase 1 vehicle must remain playable.
>
> Do not yet close the road into a racing circuit, add formal checkpoints, decorate heavily, or build final jumps/shortcuts.
>
> At the end, test driving the entire greybox and report any places where the real geometry needed to be exaggerated for gameplay.

## Acceptance Test
- [ ] Dan recognizes the street.
- [ ] Road scale feels believable.
- [ ] Car can drive the complete street.
- [ ] No major collision problems.
- [ ] Placeholder buildings/landmarks give enough visual orientation.

### Street Notes
- Key landmarks:
- Things that must remain recognizable:
- Things we can fictionalize:
- Scale changes:
- Bugs:

---

# PHASE 3 — Turn the Street Into a Circuit

## Goal
Transform the street into a fun closed racing loop while retaining its identity.

## TODO
- [ ] Decide how the ends connect.
- [ ] Build fictional connecting road/route.
- [ ] Ensure smooth circuit flow.
- [ ] Establish start/finish area.
- [ ] Add checkpoint system.
- [ ] Add lap counting.
- [ ] Add race timer.
- [ ] Add wrong-way/course validation where needed.
- [ ] Add race restart.
- [ ] Add basic HUD for lap/time/checkpoint status.
- [ ] Complete multiple test laps.

## Tell Astra/Codex

> Begin Phase 3 only. Turn the existing childhood-street greybox into a CLOSED ARCADE RACING CIRCUIT.
>
> Preserve the recognizable portion of the street where possible, but invent whatever connecting road or environment is necessary to make the route loop naturally.
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

---

# SESSION HANDOFF

Update this section before bringing the file back to ChatGPT.

**Current phase:** Phase 0  
**Last completed task:** None yet  
**Current playable state:** Not started  
**Astra's latest summary:**  
**New bugs:**  
**Things Dan wants changed:**  
**Questions/uncertainties:**  
**Ready for next phase?:** No  

---

# How Dan and ChatGPT Will Use This File

At the end of an Astra/Codex work session:

1. Update the checkboxes Astra completed.
2. Paste Astra's final summary into `SESSION HANDOFF`.
3. Add any bugs under `BUG TRACKER`.
4. Add personal complaints/changes under `CHANGE REQUESTS`, even if they seem minor.
5. Bring this `.md` file back to ChatGPT.
6. ChatGPT will:
   - review what changed;
   - decide whether the current phase actually passes;
   - update priorities;
   - write the next Astra/Codex prompt;
   - split difficult fixes into manageable tasks;
   - update this project plan;
   - keep later phases from ballooning prematurely.

The file is the project's source of truth. We do not casually rebuild the project plan from memory.
