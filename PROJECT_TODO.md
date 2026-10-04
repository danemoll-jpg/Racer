# Woodstock Rush
## Project Management / TODO / Astra-Codex Handoff

## Mandatory standing workflow

- **PROJECT_TODO.md is the source of truth** for project state, bugs, decisions, backlog and current work.
- **[CODEX_RULES.md](CODEX_RULES.md) contains the mandatory standing workflow/release rules.** Every future Codex task MUST read and follow it before making project changes, even when the individual prompt does not repeat those rules.
- A later explicit instruction from Dan may override a standing rule for that specific task.
- Do not silently modify or weaken CODEX_RULES.md. Changes to standing rules require an explicit instruction from Dan.
- Root AGENTS.md points future Codex tasks to both files.
- From 2026-10-02 the coding agent is Claude Code. The same two files govern it; root CLAUDE.md (created in the 0.67 round) is its discovery pointer.

## CURRENT — Blender vehicles for the whole garage + rider customization — 0.75.0-review1 — DELIVERED, AWAITING DAN'S REVIEW

- **DELIVERED:**
  - Source `e1182daf671796f6f197d1b4fe9fb5aa3df82cd3` pushed and verified on origin/main.
  - Fresh 0.75.0-review1 Windows build: 0 errors, 2 warnings, 2m35s.
  - Published [game-75000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-75000) (known draft-lookup miss, completed with `--resume-draft`).
  - All 232 Latest files match the public signed manifest; public download, signature, install and startup pass.
  - [Delivery evidence](Docs/Report075/PUBLICATION.md).
- **Play-Racer.cmd (unchanged):** launched a responsive managed 75000, muted, with settings restored byte-for-byte; no pending updates. Latest root, current 75000 and previous 74000 retained.
- **Cleanup:**
  - Builds 10,422,915,497 → 8,201,176,166 bytes; 1.80 GB hosted check install and 1.95 GB scratch outside the project removed.
  - C: free 295,396,601,856 bytes.
  - All debug report history preserved.
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.75 (the new garage models and rider customization; rule 12) and of 0.74. Dan's queued 0.76 round below starts only when he starts it. A documentation-only delivery commit follows; playable source remains `e1182daf`.

### Results (2026-10-04, Claude Code)

- **Safety checkpoint:** `58d7df9d` (this TODO plan), pushed before any change. Version 0.75.0-review1 / build 75000.
  - Evidence: [Docs/Report075/VALIDATION.md](Docs/Report075/VALIDATION.md), [Models/](Docs/Report075/Models/) (render sheets), [Game/](Docs/Report075/Game/), [Checks/](Docs/Report075/Checks/), [Bench/](Docs/Report075/Bench/).
  - Blender scripts `Tools/Blender/kit.py` (shared helpers), `rider.py`, `trailfour.py`, `cars.py` (both cars), `render_rider.py`, `render_vehicle.py`, `contact_sheet.py`; `needle600.py` unchanged. Sources `SourceArt/Blender/{Rider,TrailFour,StreetClassic,LongroofGT}.blend`; FBX in `Assets/Resources/VehicleModels/`.
  - Tools `Tools/Report075/` (checks runner, sheets, bench, release scripts); play-mode checks `Assets/Scripts/Report075Checks.cs`; `ConditionsBench -conditionsModels` (mixed grid, New vs Classic).
- **Part A — models: PASS.** Roster = Needle 600, Trail Four, Street Classic, Longroof GT. New: Trail Four (9,740 tris, 2 passes), Street Classic coupe (10,628, 2 passes), Longroof GT wagon (10,816, 2 passes; one parametric car script); worst case with rider 19,876. Fitted to the existing vehicles (wheels, colliders, physics, camera unchanged); wheels spin, front wheels steer, ATV bars and car steering wheels turn; cars have a cabin tub, glass, seats, dash, the driver at the left seat visible through the glass. Paint on bodywork only (Black/Red, all four). Lamps glow at night. Wipe-out/reset fine. Model: Classic / New is one setting for every vehicle (save field still `newMotorcycle`, so earlier choices carry over), default New; AI follow it; ambient traffic always classic. One lap by Day and by Night per vehicle with mixed AI fields: 8/8 finished, 0 missed gates, records saved.
- **Part B — rider: PASS.** `RiderLook` + parametric rider (`Rider.fbx`, poses Moto / Atv / Car): Man / Woman, 6 skin tones, Short / Medium / Long / Ponytail / Bald in 8 colours, None / Flat cap / Baseball cap / Beanie / Cowboy hat, T-shirt / Long sleeve / Jacket, Jeans / Shorts; colours = vehicle swatches (jeans as darker denim). Hair is split at a hat band; under a hat only the lower part shows (Blender coverage check 0 outside; in game 50/50 combinations). Garage "Rider…" page: live preview framed on the rider, Randomize, Back, ten ‹ › rows; every row changes the preview and saves; persists across a fresh read. Default = the 0.73 rider (man, medium skin, short dark-brown hair, black flat cap, blue T-shirt, jeans). AI: random per race (race seed), varied shirt colours, both bodies, never the player's look. Classic: old rider and a note on the page.
- **Fixed in passing:** a clone made in the frame its source vehicle was rebuilt (traffic/AI at race start right after a garage change) inherited a hidden, not-yet-destroyed copy of the player's model; `VehicleConfiguration.Apply` now removes it.
- **Frame rate (3840×2160): PASS.** Worst view Street Night/Rain and Night/Snow 4.60 ms (0.74 worst 4.60 ms); mixed grid New vs Classic +0.06–0.07 ms.
- **Decisions recorded:** default hat colour Black (the palette has no brown; the 0.73 cap was hair-coloured); shoes stay brown for every rider; the ghost uses the player's current rider; skin tones named Very light / Light / Medium / Tan / Brown / Dark.
- **Note:** Dan added the queued 0.76 section below while this round ran; it was not started.

- **Authorized by Dan (2026-10-04).** "I am anxious to build other vehicles and I would like to have a few different model choices (or maybe just a man or a woman) but have some simple customization, such as skin color, different hats (or no hat), different shirts, and different pants, with the ability to select colors. Oh and hair color. Nothing elaborate, just something to give some variety (the AI would be random)."
- Runs unattended: no design questions; decisions are below. Stop only for a real external blocker (rule 7).
- **Starting point:** main at the 0.74 documentation commit ("Record 0.74 delivery"); playable source `8d1fbf9b`, 0.74.0-review1 / game-74000. This TODO edit is uncommitted and belongs in the safety checkpoint. Dan has not reviewed 0.74 yet; do not change 0.74 work in this round.
- Uses the approved 0.73 pipeline: Blender at `F:\blender\blender.exe`, scripts in `Tools/Blender/`, sources in `SourceArt/Blender/`, FBX under `Assets/Resources/VehicleModels/`, render-look-revise with at most three passes per model.

### Part A — New Blender models for every remaining player vehicle

- Build a new model for each remaining vehicle the player can select in the garage (the ATV and the cars; use the actual roster in the project). Same standard and rules as the 0.73 motorcycle: clearly better proportions and detail in the same stylized low-poly world; fitted to the existing vehicle so physics, colliders, wheel positions, ride height and camera framing do not change; wheels spin, front wheels/front end steer; paint on bodywork only, every colour including black; working headlights and tail lights for night; wipeout/reset fine.
- Cars: a visible cabin with windows and a seated driver; the driver must fit the cabin (CR-082) and be visible through the glass.
- Budget per vehicle with rider roughly 5–20k triangles; frame rate at 3840×2160 within noise of 0.74.
- The garage "Model: Classic / New" choice now applies to every vehicle that has a new model (one setting, default New, remembered). Classic models stay in the project untouched. AI vehicles follow the same setting. Ambient traffic vehicles are NOT changed in this round.

### Part B — Rider customization

Simple, as Dan asked. One parametric rider built in Blender and assembled in Unity from parts, so options combine freely.

- **Body:** Man / Woman (two body shapes; same height class so vehicle fit is unchanged).
- **Skin tone:** 6 swatches from light to dark.
- **Hair:** style Short / Medium / Long / Ponytail / Bald; hair colour 8 swatches (black, dark brown, brown, auburn, red, blonde, grey, white).
- **Hat:** None / Flat cap / Baseball cap / Beanie / Cowboy hat; hat colour from the colour palette. Hair and hat must not poke through each other (hide or swap the hair top under a hat).
- **Shirt:** T-shirt / Long sleeve / Jacket; colour from the palette.
- **Pants:** Jeans / Shorts; colour from the palette.
- **Colour palette:** reuse the vehicle colour swatches (including black and white) so the UI and saving work the same way.
- **Poses:** the same rider works on every vehicle: motorcycle pose, ATV pose, seated car pose. Fixed poses are fine; keep existing lean/steer motion where it exists.
- **Garage UI:** a new "Rider" page in the garage in the existing option-row style, with a live preview of the rider on the selected vehicle; controller, keyboard and mouse. A "Randomize" action. Everything is remembered between sessions. **Default = the current rider's identity** (man, flat cap, blue shirt, same hair and skin as now) so nothing changes until the player chooses.
- **AI riders:** each AI gets a random combination per race, stable for that race (same rider from start to finish and in results), with good variety across the field; never an exact copy of the player's rider when avoidable.
- Applies to the New models. With "Model: Classic" the old rider is shown and the Rider page says customization needs the New models.
- Ambient people (household scenes, snow scenes, traffic drivers) are NOT changed in this round.
- Ghosts and records: appearance is cosmetic only; no record categories change. A ghost may show the player's current rider.

### Verification (targeted, rule 11)

- Blender render sheets for every new vehicle (old vs new) and for the rider options (both bodies, each hair style, each hat, each shirt, each pants type) in `Docs/Report075/Models/`.
- In game: each new vehicle in the garage, one race lap by day and one at night, colour change including black, wipeout and reset, Classic/New both ways.
- Rider page: every row changes the preview; hat + each hair style without clipping; settings persist across relaunch; Randomize; an AI field showing varied riders on each vehicle type; a car with the rider visible in the cabin.
- Frame-rate table against 0.74.
- `Docs/Report075/VALIDATION.md`. One implementation, then stop (rule 12). Dan judges the look.

## QUEUED NEXT — Restore the races, one dedicated Free Roam world, audible storms — target 0.76.0-review1 — NOT STARTED (start only after 0.75 is published and verified, and only when Dan starts it)

- **Authorized by Dan (2026-10-04)** from debug session `2026-10-04_06-30-49-801_f2ea73` (CLOSED, exported as `..._f2ea73_6fd893dc.zip`; 2 reports, captured on 0.74.0-review1 build `6deff347`) plus his written review of 0.74. Folder with full-size screenshots: `C:\Users\danmo\AppData\LocalLow\DefaultCompany\Racer\DebugReports\2026-10-04_06-30-49-801_f2ea73`.
- This section was written while the 0.75 round was running. It is NOT part of 0.75. When Dan starts it, rename this heading to `## CURRENT — ...` and the delivered 0.75 heading to `## Previous delivery — ...`.
- **Scope is exactly Parts A–C below, in that order.** Part A is a regression repair and comes first, verified before anything else.

### What went wrong in 0.74 (read this first)

- 0.74 BUG-003 asked for a less steep House 3 driveway. The TODO said, from Dan's understanding at the time, that the driveway was not part of any race. Code replaced the 0.30 straight driveway with the pre-0.30 winding driveway in seven scenes and moved the "House 3 valley driveway" road onto it.
- **Dan's review:** "We need to revert the driveway and restore the race. I hated this version of the Forest Reverse track. The main route this way was very awkward and was purposely replaced because it was so bad before." The change altered a course Dan had accepted. That is a 5A violation in effect, whatever the wording of the request was.
- **Lesson, now a standing instruction for this project:** before changing any world geometry in a course scene, check whether a race route, shortcut, jump or AI line in THAT scene uses it or passes near it, by reading the scene's route data, not by trusting a description. If it does, do not change it for a Free Roam or cosmetic request; stop that item and report.
- Dan had also not realised that each course is its own copy of the world and that Free Roam runs inside whichever course scene is loaded (`Race.FreeRoam = true` in that scene), so Free Roam differs from course to course. Part B changes that.

### Part A — Restore every race to its 0.73 state around the House 3 driveway (URGENT)

- In every course scene, restore the House 3 driveway area, the "House 3 valley driveway" road/route, the slot, the hillside, the lake surroundings, trees, fences and signs to exactly what 0.73 had (playable source `859d068a`; use Git, rule 5 / 5A.2). This fully undoes 0.74 BUG-003 in the race scenes, including Forest Loop Reverse, where the drive now crosses the gap-jump trail.
- **Forest Loop Reverse must race exactly as it did in 0.73:** same main route, same line through this area, same checkpoints, jumps and AI behaviour. Prove it: compare route data and the affected meshes against 0.73, and run one Forest Loop Reverse race lap with the AI field.
- Keep the other 0.74 Part A fixes (tree moved out of the brick-house driveway, diamond sign removed, Mountain Forward BUG-004/005).
- Do NOT attempt a new driveway fix in the race scenes. The steep straight driveway stays as it was in 0.73 there.

### Part B — One dedicated Free Roam world

**Dan's request (2026-10-04):** "The whole area around the storm drain is a glitchy mess. I really wanted this area in Free Roam to be like it is in the Dan's Backyard track, because that is more representative of how it was in reality, with the trash dump and gullies back there. The storm drain didn't actually exist but it fit there... If it messes up the track for the forest tracks, I would rather the area as it is in the forest tracks exist only in the race form." His two reports show the result of 0.74 copying the tunnel into other scenes:
- **BUG-001** Street Loop Forward, Free Roam (96.50, 50.25, 90.90), heading 44. "Big hole." Flat pale sheets poke through the ground with open gaps between them.
- **BUG-002** Street Loop Forward, Free Roam (146.83, 59.18, 70.67), heading 59. "Lots of holes around here and vehicle keeps slowing down." A dark box-like tunnel exterior stands exposed, pale slivers and gaps lie across the ground, and the vehicle is slowed (probably by the copied drain-flow water footprint).

**What is "real" in this world (Dan, 2026-10-04) — use this to decide what belongs in Free Roam:**
- The road loops (Street Loop) follow real roads Dan took from maps; only their shortcuts are invented. They are the true base world.
- Dan's Backyard is the area he designed deliberately, as he remembers it (trash dump, gullies). It is the authoritative version of that area. The storm drain is invented but fits and stays.
- The Forest Loop trails were built with too much freedom before the Backyard existed and run through that same area; the Mountain race roads grew out of a mountain meant only for paths to the top and one huge jump, and many of them hang in the air. Both are RACE-ONLY inventions. They do not need to exist in Free Roam. The mountain itself, its paths to the top and the giant jump do.
- So where versions of an area conflict, Free Roam takes: real roads, then the Backyard version, never the Forest or Mountain race version.

**Design (decided; do not ask):** Free Roam stops running inside the course scenes. It gets its own scene, so Free Roam is the same world every time and Free Roam changes can never touch a race again.

1. **Create `FreeRoamWorld`** as a copy of the **Dan's Backyard Loop Reverse** scene, because that scene already has the trash dump, the gullies and the storm-drain tunnel in their real, working form. Free Roam always loads this scene, whichever course is selected in the menu. Starting Free Roam places the player at the start location of the selected course (as now), inside this world.
2. **Strip it to a Free Roam world:** race gates, race arrows, race-only barriers, start grid and course-specific signs that only make sense in the Backyard Reverse race are hidden or removed there, as Free Roam hides them today. Keep everything that is world: the dump, gullies, tunnel (lights, flow, rats, Snow ice), ramps and jumps that are fun to ride.
3. **Bring in what Free Roam has elsewhere, in its accepted form:**
   - the accepted Forest Loop Forward cave (0.74 Part C target) with bats and audio;
   - the Summit Homeward giant jump with the 0.71 flat open landing and the 0.72 clean return trail, and the open straight path up the mountain;
   - the campsite and the "Campsite" landmark; all landmarks, the exploration map/fog, fast travel, waypoints;
   - every Free Roam activity, jump score, speed trap and all 24 Woodland Acorns, with the player's existing progress and records carried over unchanged;
   - household scenes, snow scenes, wildlife, traffic, signs, weather, the day-night clock and calendar.
   Audit all eight course scenes for Free Roam content and list anything that exists in one of them but cannot be carried into `FreeRoamWorld` (for example the Mountain race roads, which exist only in the Mountain race scenes). Do not silently drop content: report the list for Dan.
4. **House 3 driveway in `FreeRoamWorld` only:** keep the 0.74 winding driveway here (Dan: "maybe keep the change of winding driveway for Free Roam"). It must be clean and drivable, with no leftover tree fragments. Dan will judge it in Free Roam; it no longer affects any race.
5. **Remove 0.74's Free-Roam-only copies from the course scenes:** the `FreeRoamOnly` tunnel content and the three reshaped terrain tiles that 0.74 Part B added to Street Loop F/R, Lake Woods, Forest Loop Reverse and Mountain Loop F/R are deleted, restoring those areas to their 0.73 race state. Free Roam no longer runs there, so nothing is lost. Likewise, earlier Free-Roam-only content in the course scenes (for example the Summit Homeward jump) may stay inactive in races as it is today; do not spend effort removing it unless it is in the way.
6. **Races are untouched by all of this** apart from Part A and item 5 restoring 0.73 geometry. Every course must still load, race and save records as before.

7. **Course routes on the Free Roam map, as overlays only (Dan, 2026-10-04).** Dan had the map built so he could see the tracks on it. He did not realise that choosing a track to show was loading that course's copy of the world. He wants to keep seeing the tracks without the world changing.
   - First establish how the map shows a course route today and what it loads or switches when a different course is chosen.
   - **Required:** in Free Roam, the map can show the route of ANY course (main route and its shortcuts, in the existing main/shortcut colours, with direction and start/finish) as a drawn overlay on the one Free Roam world. Choosing which course routes to show never loads another scene, never changes the world and never moves the player. Several routes can be shown at once; the choice is remembered.
   - Take the route lines from each course scene's own route data, exported to shared data the map can read without that scene being loaded. Regenerate that data from the scenes with a tool, so it stays correct when a course changes.
   - Forest and Mountain race routes follow race-only geometry that does not exist in Free Roam. Draw them anyway, in a visibly different style (for example dashed) with a short legend note such as "race-only route", so Dan can see where each race runs without expecting a road there.
   - Racing a course is still started from the menu as now. No other map changes.

**Quality bar for `FreeRoamWorld`:** the storm-drain / dump / gully area must be solid and clean: no holes, no exposed tunnel box, no see-through seams, collision matching what is visible (rule 4), no unexplained slowdown. Ride the tunnel end to end and the gullies and dump around it.

### Part C — Thunder and lightning that Dan can actually see and hear

- **Dan, after 0.74:** "I am still not hearing thunder and not sure I am really seeing much lightning still." 0.74's own recordings counted 12–13 strikes in three minutes, so the system fires in the test harness but is not reaching Dan in real play. His session was Free Roam, Day, Rain, on the motorcycle.
- **Reproduce the way Dan plays, in the installed build:** Free Roam with Weather = Rain during the daytime part of the live cycle, riding with engine sound and the radio on, default volumes. Then a Rain race by day. Establish why he does not perceive it. Check at least: whether storms actually run in Free Roam's blended day-night look (not only in fixed race presets); the thunder's real loudness at the listener against engine and music; whether distance attenuation or the "under cover" muffle is wrongly applied in the open; whether the Ambience volume or the Lightning setting in Dan's saved settings suppresses it; whether daytime bolts and flashes are simply too faint against the bright overcast.
- **Required result:** in Rain, in races and in Free Roam, at any time of day, a rider with engine and radio on clearly hears thunder and clearly sees lightning within the first 30 seconds and regularly after that. Thunder must be loud and full enough to stand out over the engine (mix it so; do not rely on the player raising a volume). Daytime lightning must be obvious: a bright, thick, high-contrast forked bolt that lasts long enough to register (a few tenths of a second, with an afterglow), plus a visible sky and cloud brightening.
- Comfort rules from 0.73/0.74 still apply (no strobing, at most two pulses, "Lightning flashes: Off" removes the screen flash only).
- Evidence: a short table from real play sessions (Free Roam Day/Rain, Free Roam Night/Rain, Race Day/Rain) with strike count in two minutes and the measured thunder level against the engine at cruising speed.

### Verification for this round (targeted, rule 11)

- Part A: Forest Loop Reverse race lap as described; a mesh/route comparison against 0.73 for each restored scene; the House 3 area screenshots match 0.73.
- Part B: start Free Roam from each of the eight courses and confirm the same world loads, at that course's start location; show each course's route on the map in turn and several together, confirming the scene and player position never change; ride the dump, gullies and tunnel; visit the cave, giant jump, campsite; acorn count and records unchanged; clock, weather, waypoint and fast travel work; one race on each course family still loads and finishes.
- Part C as described.
- `Docs/Report076/VALIDATION.md` with a PASS/explained disposition per item.

## Previous delivery — 6 reports, storm and rain sound, dawn, continuous Free Roam calendar with moon phases, map waypoints, tunnel and cave in Free Roam — 0.74.0-review1 — DELIVERED, AWAITING DAN'S REVIEW

- **Authorized by Dan (2026-10-04)** from debug session `2026-10-04_00-23-11-202_43e8e2` (CLOSED, exported as `..._43e8e2_8460ce84.zip`; 6 reports, all captured on 0.73.0-review1 build `557d959c`) plus eight written requests in chat. Folder with full-size screenshots: `C:\Users\danmo\AppData\LocalLow\DefaultCompany\Racer\DebugReports\2026-10-04_00-23-11-202_43e8e2`. READ every comment and LOOK at every screenshot before changing anything.
- **Dan's verdict on 0.73:** "I really like the way things are looking. It looks so much better than it did when I started." The Blender motorcycle and rider approach is APPROVED; more vehicles and rider customization follow in the queued 0.75 round below. Snow scenes, frozen water and clouds drew no complaints; do not retune them except as written here.
- **Dan is asleep; this round runs unattended.** Every design decision needed is written below. Do not stop to ask about design: make the smallest reasonable choice, record it, continue. Stop only for a real external blocker (rule 7).
- **Starting point:** main `4fd27957` (documentation commit after the 0.73 delivery; playable source as recorded in the 0.73 DELIVERED entry, game-73000). This TODO edit and the archive move are uncommitted and belong in the safety checkpoint.
- **Scope is exactly Parts A–G below.** Do geometry (A, B, C) first and verify it before the look/audio/UI parts. Section 5A applies to all geometry. Weather and time of day stay visual/audio only.
- **When this round is fully delivered (published and verified), continue with the queued 0.75 section below, as a separate round with its own safety checkpoint, commit, build, release and report.** Dan authorized running both back to back.

- **DELIVERED:**
  - Source `8d1fbf9b6d00ba7570542494f76ce676de2511a6` pushed and verified on origin/main.
  - Fresh 0.74.0-review1 Windows build: 0 errors, 37 warnings (obsolete-API notices), 2m44s.
  - Published [game-74000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-74000) (known draft-lookup miss, completed with `--resume-draft`).
  - All 232 Latest files match the public signed manifest; public download, signature, install and startup pass.
  - [Delivery evidence](Docs/Report074/PUBLICATION.md).
- **Play-Racer.cmd (unchanged):** launched a responsive managed 74000, muted, with settings restored byte-for-byte; no pending updates. Latest root, current 74000 and previous 73000 retained.
- **Cleanup:**
  - Builds 10,315,067,277 → 8,098,028,572 bytes; 1.80 GB hosted check install and 1.88 GB scratch outside the project removed.
  - C: free 295,299,096,576 bytes.
  - All debug report history preserved.
- **SESSION HANDOFF: STOP.** The 0.74 session prompt limited the work to this section, so the queued 0.75 round was not started; it is next. A documentation-only delivery commit follows; playable source remains `8d1fbf9b`.

### Results (2026-10-04, Claude Code)

- **Safety checkpoint:** `8da01e4e` (this TODO plan and the archive move), pushed before any change. Version 0.74.0-review1 / build 74000.
  - Evidence: [Docs/Report074/VALIDATION.md](Docs/Report074/VALIDATION.md) (per-item disposition, ride table, storm table, clock and waypoint tables, frame-rate table), [Views/](Docs/Report074/Views/), [Checks/](Docs/Report074/Checks/), [Profiles/](Docs/Report074/Profiles/), [Bench/](Docs/Report074/Bench/).
  - Tools in `Tools/Report074/` (authoring `Report074Author/Drive/Tunnel/Cave.cs`, probes, batch scripts); play-mode checks `Assets/Scripts/Report074Checks*.cs`; 4K evidence through `ConditionsBench.cs` (Dawn added, moon phases, lightning).
- **Part A — 6 reports: PASS.**
  - BUG-001: the brick-house driveway tree (Street Loop Forward only) moved 6 m south onto the grass, re-grounded.
  - BUG-002: the House 3 diamond sign ("Simple bend warning") removed in all 8 course scenes.
  - BUG-003: the 0.30 straight driveway (steepest 2 m grade **205 %**, 64°) replaced in the seven scenes that had it by the original winding driveway (slot filled back to the hillside, line round the lake's west tip and north shore, 8 m clear of the pool, level start at the gate, 7 m gravel, terrain benched ≤3 m cut / 3.5 m fill, 1–7 trees per scene moved out of the width, fences/sign/road follow). Steepest after **32 %** (natural hillside). Moto, ATV and both cars drive it down and up at 10 m/s without stall, wipe-out or reset. Street Loop Reverse untouched (it always had the winding drive). Race ramp untouched.
  - **Exception — Forest Loop Reverse:** the gap-jump embankment lies on the original line and is protected; the drive crosses that trail at grade, so going up a vehicle hops 0.36–0.8 s onto the trail's raised edge, and the descent off it reaches 38–46 % for a few metres. It completes in every vehicle, no stall.
  - BUG-004: one smooth collidable surface (0.71 junction method) over the Mountain Forward s 12 trail edge and bank; 0.71 Climbing Ridge Cut fix kept (5A.6 entrance ride PASS). BUG-005: the s 1503 left-bank lip smoothed flush with the road edge. Rides at 20/26 m/s PASS.
  - BUG-006: see Part C.
- **Part B — storm-drain tunnel in every Free Roam: PASS.** Copied from Backyard Reverse into Street Loop F/R, Lake Woods, Forest Loop Reverse, Mountain Loop F/R as Free Roam-only content (`FreeRoamOnly`: culvert, lights, flow (freezes in Snow), gully takeoff/landing, rat encounter); three terrain tiles take the Backyard shape in Free Roam only (≈7,360 m², no route within 20 m), trees on the changed ground re-grounded and those in the tunnel line hidden, in Free Roam only. Races unchanged. Backyard scenes unchanged (drain and its race data identical). Map landmark "Storm drain tunnel" in all 8 scenes. Rides in Clear and Snow from several courses PASS.
- **Part C — one cave: PASS.** Mountain Loop F/R now have the Lake Woods (Forest Forward) cave (terrain there identical); Forest Forward bats also fly in Free Roam. Forest Loop Reverse left as is (its Fern Gully branch runs through its cave).
- **Part D — rain and storm: PASS.** New generated 24 s rain loop (soft bed, swells, light patter, no hiss; quieter, muffled under cover). Rain storms strike every 8–25 s (20 % quick pairs), 260–1500 m away; thunder arrives at the speed of sound (near crack, far roll), louder than rain; distant rumbles with cloud glow; forked bolts drawn at the strike's bearing by day and night. 3-min recordings: Day 13 strikes (6 near / 7 far, longest gap 24.7 s), Night 12. "Lightning flashes: Off" removes the screen flash only.
- **Part E — Dawn: PASS.** Time of Day options Dawn / Day / Dusk / Night (Dawn saved as 3, older saves unchanged): low sun in the east, cool lavender-pink light, pink clouds, ground mist in hollows; Free Roam passes through it (about 04:36–07:36). One Dawn race finished, record saved.
- **Part F — Free Roam clock and calendar: PASS.** The Free Roam day/time is saved when Free Roam ends (race start, opening the menu, another course) and on quit, and resumes there; 30-day calendar ("Day N hh:mm" in the HUD and debug conditions), midnight advances the day, day 30 → day 1; moon phase from the calendar (disc shows the phase; night light by illumination); Night races use a full moon; missing/unreadable value → Day 1 08:00.
- **Part G — map waypoints: PASS.** Free cursor or mouse click sets a waypoint anywhere (fogged areas too; landmarks still select), a new one replaces it, Backspace / left-stick press or right-click clears it; beacon beam in the world and a HUD distance + arrow; "Destination reached" within 15 m; not saved; nothing in races.
- **Frame rate (3840×2160): PASS.** Worst view Street Dawn/Clear 4.60 ms (0.73 worst 4.57 ms); Night/Rain with strikes every 2 s 4.51 ms (0.73: 4.53 ms).
- **Notes for Dan:** the Free Roam pause menu is the main menu, so "returning to the menu" saves the clock when that menu opens; resuming Free Roam continues from there. Moon: day 1 new, day 15–16 full.

### Part A — 6 reports

Street Loop world, Free Roam (shared world: apply to every scene that contains the location):
- **BUG-001** (446.00, 81.45, 9.07), heading 250. "Can we move this tree out of the driveway?" A tree stands in the middle of the dirt driveway leading to the brick house. Move it a few metres to the side onto the grass, grounded, with its collider.
- **BUG-002** (501.84, 82.02, -129.83), heading 305. "Either remove this sign or lower it." A diamond warning sign on a tall post at the top of the steep driveway beside the fence gate; it stands too high and shows no readable face from here. Remove it (simplest; it tells the driver nothing).
- **BUG-003** (495.06, 73.61, -138.61), heading 237. "Can we make the slope of this driveway a little less steep?" The driveway drops through a deep cut toward the green house below.
  - **Dan clarified (2026-10-04):** the driveway itself is NOT part of any race; a separate ramp nearby serves the race going that way. The driveway is "very steep to the point of being almost undriveable", and he wants it fixed regardless.
  - **Required:** make it comfortably driveable both down and up for every vehicle (motorcycle, ATV, cars) at ordinary speed, with no bottoming out, no launch over the top lip and no stall or wheelspin on the way up. Regrade as much as that takes: lengthen the slope, round the top and bottom, and widen or re-cut the cutting if needed. This is more than a small tweak; do what makes it properly usable.
  - Rule 4 dependencies: gate and fences at the top, mailbox, the cut walls, the house, its parking area and anything else resting on the changed surface must stay grounded and connected.
  - Leave the nearby race ramp and its approach/landing exactly as they are (protected jump, 5A.5), and confirm the race line that uses it is unchanged.

Mountain Loop Forward, Race:
- **BUG-004** (739.27, 86.29, -125.64) main s 12, heading 154. "Fix this hole." A row of jagged gaps/holes runs along the trail edge just ahead-right of the bike, near the Climbing Ridge Cut junction. Close them with collidable ground flush with the trail (0.69 edge rules). This junction has been reworked in 0.69–0.71: check Git first, and keep the 0.71 corner-cut fix working.
- **BUG-005** (1043.00, 150.97, 25.05) main s 1503, heading 354. "Smooth this out." On the left a terrain sheet rides up over the road edge with a torn, lifted lip; the pavement beside it is lumpy. One smooth connected surface, flush with the road.

Free Roam started from Mountain Loop Forward:
- **BUG-006** (-24.70, 22.78, -42.15), heading 157. "Can we have this cave match exactly the version that is on the Forward Forest trail? This is the old bad version." See Part C.

### Part B — The storm-drain tunnel shortcut must exist in Free Roam

- **Dan:** "When the water sewer tunnel shortcut was built for Reverse Dan's Backyard, it said it was going to put this in the Free Roam world, but I don't see it."
- Find the tunnel/culvert shortcut built for Dan's Backyard Loop Reverse (0.54–0.56 records are in the archive) and the promise about Free Roam. Establish in which scenes its geometry is present and active.
- **Required:** the tunnel is present, open at both ends and rideable in Free Roam no matter which course Free Roam was started from, identical to the Backyard Reverse version (geometry, lighting inside, the rats/audio if they belong to it, and the 0.73 frozen drain flow in Snow).
- Races on other courses must not change. If the tunnel's geometry would alter another course's route, terrain or checkpoints, activate it there in Free Roam only (the same way the Summit Homeward jump is Free-Roam-only in other scenes).
- Add it to the exploration map as a landmark if landmarks of this kind exist.

### Part C — One cave everywhere: the accepted Forest Forward cave

- The Forest Loop Forward cave is the accepted version (0.58–0.60 records). BUG-006 shows an older cave in the Free Roam world started from Mountain Loop Forward.
- **Required:** in every scene where the cave appears and is NOT that scene's own race route, the cave matches the Forest Loop Forward version exactly (geometry, colliders, entrance hillside, obstacles, lighting, bats/audio). List which scenes were changed.
- **Forest Loop Reverse:** if its cave differs because the Reverse course needs it to, leave it and report the difference; do not change a course's own race geometry in this part.

### Part D — Rain sound, and a storm with real presence

- **Rain sound (Dan: "sounds a little too much like static and gets grating after a while"):** replace it. It should sound like rain: a softer, lower, rounder bed with gentle variation over time and occasional heavier gusts, plus light patter detail, with no hiss. A free CC0 recording is allowed if one can be obtained; otherwise synthesize it properly (shaped/filtered noise layers, slow modulation, droplet transients), not plain white noise. Loop seamlessly. Slightly quieter than now by default, still on the Ambience volume. Under cover it stays muffled.
- **Thunder and lightning (Dan: "I noticed thunder and lightning literally once and never saw it again. Was hoping it would make more of a presence"):**
  - Strikes much more often: an irregular gap of about 8–25 seconds, with occasional close pairs.
  - Each strike is clearly visible at ANY time of day: a visible forked bolt in the sky in a random direction, plus the sky/cloud flash. In Day the bolt and a clear brightening must still be noticeable.
  - Vary the distance: near strikes are bright with a loud crack arriving quickly; far strikes are dimmer with a long low rumble after several seconds. Between strikes add occasional distant rumbles with only a faint glow in the clouds.
  - Thunder louder and fuller than now relative to the rain.
  - Keep the 0.73 comfort rules: no strobing, at most two pulses per strike, road/arrows/gates readable, "Lightning flashes: Off" still removes the screen flash and keeps the sound (bolts may stay visible when Off, without the full-screen flash).

### Part E — Dawn

- Dan asked why there is a Dusk race option but no Dawn. There was no reason beyond the original list; add it.
- **Race setup Time of Day becomes: Dawn / Day / Dusk / Night.** Dawn must look clearly different from Dusk: sun low in the opposite part of the sky, cool pink-to-pale-gold light, bluish shadows, light ground mist in low areas that burns off toward Day. Headlights on at Dawn as at Dusk.
- The Free Roam cycle passes through the same Dawn look in the early morning.

### Part F — Continuous Free Roam time, and a 30-day calendar with moon phases

- **Dan:** "It will remember the time it was when either you start a race or exit the game, and when you return to Free Roam... it will pick up from where it left off, so we don't always start with 8 am and rarely see the rest of the day."
- **Required:** the Free Roam clock is saved whenever Free Roam ends for any reason (starting a race, returning to the menu, quitting the game) and Free Roam always resumes from the saved time. Time does not advance while the player is not in Free Roam. First ever start: 08:00 on day 1. Speed stays 1 real minute = 1 game hour.
- **30-day calendar (Dan wanted this as a nice touch; it is cheap to do with the saved clock, so it is included now):** the saved time includes a day number 1–30 that advances at midnight and wraps. The moon shows the correct phase for the day (new moon on day 1, full around day 15), rising and setting sensibly, and moonlit nights are a little brighter near full and darker near new, while staying playable. Show "Day N" with the clock in the Free Roam HUD text and in the debug conditions line.
- **Races:** Night races use a fixed full moon so race conditions are always the same.
- Stored with the other settings/saves; survives relaunch and updates; a missing or unreadable value falls back to day 1, 08:00.

### Part G — Map: set a destination anywhere and drive to it

- **Dan:** "On the map I would like to be able to place a destination literally anywhere on the map so I can drive to it, not just a small set of places."
- In the Free Roam map, the player can move a cursor freely (stick / mouse / keys) and place ONE custom waypoint at any point of the world, including undiscovered/fogged areas. Placing a new one replaces the old; a button clears it. Controls shown in the map's existing hint style.
- This is a destination to DRIVE to, not a teleport. Existing fast travel to discovered destinations is unchanged.
- While a waypoint is set, in Free Roam: a marker on the minimap/map, a tall visible beacon in the world at the waypoint (readable by day and night), and a HUD line with distance (imperial, as elsewhere) and a direction arrow. Straight-line guidance is enough; no road routing.
- On arrival (within about 15 m) show a short "Destination reached" note and clear the waypoint. The waypoint is not saved across game restarts.
- No effect in races.

### Verification for this round (targeted, rule 11)

- Part A: before/after at each coordinate; drive BUG-003's driveway down and up with a motorcycle, an ATV and a car, reporting the steepest grade before and after; ride through BUG-004/005 at speed; 5A.6 for the Climbing Ridge Cut entrance.
- Part B: ride the tunnel end to end in Free Roam started from two different courses, in Clear and in Snow; one Backyard Reverse race using the shortcut still credits correctly.
- Part C: side-by-side views of the Forest Forward cave and each changed scene's cave; one ride through in Free Roam.
- Part D: a 3-minute Rain recording note with strike times at Day and at Night (count, gaps, near/far mix); the Off setting.
- Part E: Dawn versus Dusk screenshots from the same four views; one Dawn race.
- Part F: leave Free Roam at a known time by each of the three exits and confirm the resume time; cross midnight and confirm the day and moon phase advance; day 30 wraps to day 1.
- Part G: place, replace, clear and reach a waypoint with controller and with mouse; one placed in a fogged area.
- Frame rate at 3840×2160 for Dawn/Clear and Night/Rain during strikes against 0.73.
- `Docs/Report074/VALIDATION.md` with a PASS/explained disposition per item.

### Outstanding after this round (as of 2026-10-04)

- 0.75 (the queued round) was run in its own session after this one; see the 0.75 section above.
- Awaiting Dan: review of 0.74 (especially the House 3 driveway feel, the Forest Loop Reverse crossing exception, Dawn look, storm sound and bolts); recheck of the Reverse s 1583 bump; whether he has seen any rival vanish after a jump since 0.72.
- Open: CR-118 intermittent spoken-title clipping. Possibly stale: CR-010 slightly tighter steering.
- Deferred: physical Steam Deck / controller / save-migration checks; friend test of the packaged build on another PC.

## Previous delivery — Snow scenes and frozen water, thunderstorms, clouds, first Blender models (motorcycle + rider) — 0.73.0-review1 — DELIVERED, LOOK AND BLENDER APPROACH APPROVED BY DAN (follow-ups in 0.74)

- **DELIVERED:**
  - Source `a877a39dd5b3e7560c698559f9f8e688bc04f822` pushed and verified on origin/main.
  - Fresh 0.73.0-review1 Windows build: 0 errors, 2 warnings, 3m05s.
  - Published [game-73000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-73000) (known draft-lookup miss, completed with `--resume-draft`).
  - All 232 Latest files match the public signed manifest; public download, signature, install and startup pass.
  - [Delivery evidence](Docs/Report073/PUBLICATION.md).
- **Play-Racer.cmd (unchanged):** launched a responsive managed 73000, muted, with settings restored byte-for-byte; no pending updates. Latest root, current 73000 and previous 72000 retained.
- **Cleanup:**
  - Builds 10,003,661,968 → 7,905,379,910 bytes; 1.73 GB scratch outside the project removed.
  - C: free 285,870,125,056 bytes.
  - All debug report history preserved.
- **SESSION HANDOFF: STOP.** Wait for Dan's review of 0.73 (frozen water and snow scenes, thunderstorms, clouds, the Blender motorcycle and rider). A documentation-only delivery commit follows; playable source remains `a877a39d`.

- **Authorized by Dan (2026-10-03)** from debug session `2026-10-03_20-51-21-371_00b27e` (CLOSED, exported as `..._00b27e_df2aae45.zip`; 2 entries, both captured on 0.72.0-review1 build `b6b54925`, both feature requests, not bugs) plus his written requests in chat. Folder with full-size screenshots: `C:\Users\danmo\AppData\LocalLow\DefaultCompany\Racer\DebugReports\2026-10-03_20-51-21-371_00b27e`. LOOK at both screenshots before placing anything.
- **Dan's review of 0.72:** he played it and reported no bugs; he is adding to the weather feature. Treat 0.72 time of day / weather, the AI jump fixes and the Free Roam trail as working. Do not retune them.
- **Starting point:** main `662281d2` (documentation commit; playable source `859d068a`, 0.72.0-review1 / game-72000). This TODO edit is uncommitted and belongs in the safety checkpoint.
- **Scope is exactly Parts A–D below, in that order.** All design decisions needed are written here; do not stop to ask about design. Make the smallest reasonable choice, record it, continue. Stop only for a real external blocker (rule 7).
- Weather stays **visual and audio only** (Dan's standing decision), with the single exception spelled out in Part A2.

### Results (2026-10-03, Claude Code)

- **Safety checkpoint:** `f850d54f` (this TODO plan and the archive move), pushed before any change. Version 0.73.0-review1 / build 73000.
  - Evidence: [Docs/Report073/VALIDATION.md](Docs/Report073/VALIDATION.md) (per-item disposition and the frame-rate table), [Look/](Docs/Report073/Look/), [Water/](Docs/Report073/Water/), [Scenes/](Docs/Report073/Scenes/), [Storm/](Docs/Report073/Storm/), [Models/](Docs/Report073/Models/).
  - Tools in `Tools/Report073/`; play-mode checks `Assets/Scripts/Report073Checks.cs`; 4K evidence through `ConditionsBench.cs` (extended: Night/Rain measured, dusk-to-night strip, motorcycle shots, New vs Classic frame time).
- **Part A — Snow: PASS.**
  - A1: all water freezes in Snow (Dan's pool, House 3 pool and lake, Friend's lake, J1 creek, Woodland creek, Fern creek, the culvert storm-drain flow): `WorldLook` swaps each water renderer to a frosted ice material (`Resources/WaterIce.mat`, generated dusting texture, environment reflections off); Clear/Rain untouched; no ripples or splash sounds on ice.
  - A2: `ShallowWater` switches on a collidable ice face (water shape) on level water bodies; the slowdown is measured as if the vehicle were on the bed under the ice, so it equals the water's. Not on Fern creek (22° sloped channel, buried except where it overhangs drops) or the 3 cm drain footprints: vehicles stay on the ground there as now. House 3 lake crossing: over water moto 1.78 s Clear / 1.80 s Snow, ATV 2.00 / 1.96 s; whole 72 m line ATV 7.00 / 6.47 s (in Clear it also climbs out of the lake bed). Body on top of the ice (+0.43 / +0.49 m). Snow race on Forest Loop finished with the record saved.
  - A3/A4: `SnowScenes` (AmbientLife figures, no colliders, Snow only, races and Free Roam): two people sledding on the grass 11 m right of the road centre below Dan's BUG-001 position (slide, stand, pull the sled back up); three people playing broom hockey on the frozen pool beside the deck (BUG-002). Absent in Clear/Rain/menus; household scenes untouched.
- **Part B — thunder and lightning: PASS.** In Rain a strike every 20–60 s (irregular): 1–2 short flashes (sky, clouds and image; strongest at night), thunder 0.8–4.7 s later on the Ambience volume (generated, no asset). Settings > Display "Lightning flashes: On / Off" (default On, remembered; Off keeps the thunder). Under cover no flash, thunder muffled. Night/Rain race: 3 strikes, gaps 43 / 50 s.
- **Part C — clouds: implemented (Dan judges the look).** `SkyClouds` + `Racer/StylizedClouds` shader: faceted low-poly clusters 410–560 m up (highest ground/tree 197 m), drifting with one wind; Clear scattered, Rain dark overcast lit by lightning, Snow pale even overcast (overcast adds a flat deck and a haze-coloured horizon ring). Lit by the main light (sun, dusk, moon). No cloud shadows. Frame rate 3840×2160 within ±0.1 ms of 0.72 (worst Night/Snow Street 4.57 ms = 219 fps; Night/Rain 4.53 ms).
- **Part D — Blender motorcycle and rider: implemented (pilot, Dan decides).** Blender 3.6.1 is at `F:\blender\blender.exe` (not the default path). `Tools/Blender/needle600.py` → `SourceArt/Blender/Needle600.blend` + `Assets/Resources/VehicleModels/Needle600.fbx`; `render_views.py` renders. 3 revision passes. 11,592 triangles (classic 16,468). Fitted to the existing vehicle (wheels, collider, physics, camera unchanged); front end steers about the fork axis; paint on bodywork only (black checked); lamps glow at night; wipe-out/reset fine; garage "Model: New (Classic / New)" (default New, remembered), AI motorcycles follow it. Frame time New 3.93 ms vs Classic 3.94 ms. ATV/cars unchanged.
- **Notes for Dan:** the House 3 pool (1.34 m deep) becomes a level ice floor in Snow, so a vehicle no longer drops into it; Clear scattered clouds are sparse from the road (22 of 64 cloud slots; one value in `SkyClouds`).

### Part A — Snow: frozen water and two winter scenes

**A1. Every body of water freezes when the weather is Snow** (races and Free Roam, all courses): the lake, creeks, the pool at Dan's house and any other water surface. Ice look: pale blue-white, matte with a soft sheen, light snow dusting, no water animation or sky-mirror reflection, no splash effects or water sounds. In Clear and Rain, water is exactly as now.

**A2. Riding on ice (Dan's decision, 2026-10-03): "slows you down but you don't sink."**
- When frozen, the vehicle rides ON the ice surface instead of sinking into the water.
- It is slowed by the same amount the water slows it today (same resistance values, same affected area), so lap times in Snow stay comparable with other conditions.
- No slipperiness and no other handling change. AI treats it the same as the player.
- This is the only physics-adjacent change allowed in this round. Verify one water crossing (motorcycle and ATV) in Snow against the same crossing in Clear: time through the crossing should match closely, and the vehicle stays on top.

**A3. Scene: two guys sledding (Snow only).**
- **Entry BUG-001** Dan's Backyard Loop Reverse, Free Roam (519.21, 74.77, -153.47), heading 172. "When it is snowing, can we have two guys sledding down this hill on a sled together, on the right side of the road?"
- The road drops away downhill ahead of this point. Put the scene on the RIGHT side of the road as seen from this position and heading, on the grass slope beside it, never on the pavement or in the driving width.
- Two figures seated together on one sled. They slide down the hill, then reset to the top and go again (walk/pull the sled back up, or fade and restart; simplest that looks natural). Loop while Snow is active.

**A4. Scene: broom hockey on the frozen pool (Snow only).**
- **Entry BUG-002** Dan's Backyard Loop Reverse, Free Roam (400.78, 80.17, -5.15), heading 197. "When it is snowing this and every other body of water should be frozen. Here, can we have 3 guys playing hockey with brooms and a ball?"
- This is the rectangular pool beside the deck at Dan's house. Three figures on the ice holding brooms (not hockey sticks), knocking a ball between them: simple looping movement, the ball visibly passed around, figures stay on the ice.

**Shared rules for A3/A4:**
- Build them with the existing ambient-people system (`AmbientLife` and the household scenes: same figure style, same way of appearing). They exist only while the weather is Snow, in every scene of the shared world that contains these locations, in races and Free Roam.
- Grounded on the snow/ice (rule 4). No colliders in the driving width; give the figures the same collision behaviour as the existing household figures.
- They must not interfere with the existing household scenes at Dan's property (coffee / football / empty). If the pool scene and a household scene would overlap in space, the household scene keeps its spot.
- Simple readable animation is enough; no new animation system.

### Part B — Thunder and lightning in Rain

- In Rain (any time of day; races and Free Roam): occasional lightning and thunder. Not constant: irregular gaps, roughly one strike every 20–60 seconds.
- Lightning: a brief flash that lights the sky and the world (one or two quick pulses). Optionally a visible bolt in the distant sky. Strongest at Night and Dusk, subtle in Day.
- Thunder: a rumble that follows each flash after a short, varying delay, on the Ambience volume like the rain sound. Use a generated or CC0 sound; no paid assets.
- Comfort and readability: flashes must be short and not strobe; never more than two pulses per strike; the road, arrows and gates stay readable through a flash. Add a setting "Lightning flashes: On / Off" in the existing options menu (default On); Off keeps the thunder sound but removes the screen flash.
- No gameplay effect. None under cover (cave, tunnel): thunder muffled there.

### Part C — Clouds

Dan asked how complicated clouds are. Answer recorded here: stylized clouds are a small addition on the 0.71/0.72 look system; true volumetric clouds are not worth their cost at 3840×2160 on this GPU and would not match the low-poly style. Build the stylized kind.

- Stylized low-poly / soft-shaded clouds that match the world, drifting slowly with a consistent wind direction.
- Per condition: Clear = scattered fair-weather clouds; Rain = heavy dark overcast (lightning lights it from within); Snow = pale even overcast.
- Per time of day: lit by the sun colour at Day, warm at Dusk, dim and moonlit at Night without hiding all the stars in Clear.
- In Free Roam they follow the day-night blend smoothly. Cloud shadows on the ground are optional; include them only if cheap and not distracting.
- Clouds never dip into the playable space or the Mountain summit; they are sky only.
- "Day / Clear" changes only by gaining the scattered clouds; everything else in that preset stays as accepted.

### Part D — First Blender models: motorcycle and rider (pilot for Dan's approval)

This is graphics upgrade step 2, pilot only. Dan has Blender installed on this PC. The goal is to find out how good a scripted Blender pipeline can make the motorcycle and its rider, and to let Dan compare old and new in the game.

- **Pipeline:** locate the installed Blender (`blender.exe`, normally under `C:\Program Files\Blender Foundation\`; if it cannot be found, that is a rule-7 blocker for Part D only: finish Parts A–C, deliver, and report). Run it from the command line with Python scripts. Keep the scripts in `Tools/Blender/` and the generated `.blend` sources in a source-art folder in the repo so models can be regenerated and edited later. Export to a Unity-friendly format (FBX or glTF) under `Assets/`.
- **Look at your own work:** render the model from Blender (front, side, three-quarter, top) and look at the renders; revise the model; repeat. At most three revision passes per model, then stop (rule 12). Keep the renders.
- **Motorcycle:** a clearly better dirt-bike-style motorcycle in the same stylized low-poly world: real proportions, two proper wheels with tyres and rims, front fork and rear swingarm, handlebars with grips, engine block, tank, seat, exhaust, fenders, foot pegs, a headlight and tail light positioned to work with the 0.72 night lights. Wheels spin and the front end steers as the current one does.
- **Rider:** a better-proportioned stylized person in a proper riding pose: hands on the grips, feet on the pegs, seated on the seat. Keep the current rider's identity (helmet-free, flat cap, blue shirt, visible face and hair per CR-072/CR-082). A fixed riding pose is acceptable; keep whatever lean/steer motion the current rider has if it can be kept simply. No skeletal animation system is required for the pilot.
- **Must keep working:** player colour selection including black (paint goes on the bodywork, not the whole bike); vehicle physics, colliders, wheel positions, ride height and camera framing unchanged (the new model is fitted to the existing vehicle, not the other way round); night headlights; wipeout/reset behaviour; garage display; AI motorcycles and their different riders/colours.
- **Budget:** roughly 5–15k triangles for bike plus rider; a small number of materials; no texture dependencies that need paid tools. Frame rate at 3840×2160 must stay within noise of 0.72.
- **How Dan compares:** add a garage option for the motorcycle, **Model: Classic / New**, default New, remembered. It switches the player's motorcycle and rider, and AI motorcycles follow the same setting. The classic model stays in the project untouched. ATV and cars are NOT changed in this round.
- **Evidence:** Blender render sheet old vs new, and in-game screenshots (garage, race Day, race Night, from the chase camera and one close side view) in `Docs/Report073/Models/`.
- One implementation, then stop. Dan decides whether the approach is good enough to continue to the other vehicles.

### Verification for this round (targeted, rule 11)

- Part A: screenshots of each water body in Snow and in Clear; both scenes in Snow at Day and Night; confirm neither scene exists in Clear or Rain; the A2 crossing comparison; one Snow race on a course that crosses water finishes normally with records saved.
- Part B: one Rain race at Night with at least three strikes observed; the Off setting; under-cover behaviour.
- Part C: the 9 condition views from 0.72 retaken with clouds; Free Roam dusk-to-night with clouds; frame-rate table at 3840×2160 for Day/Clear, Night/Rain, Night/Snow (must stay well above 60 fps; report against 0.72).
- Part D: one race lap on the new motorcycle at Day and at Night, one wipeout and reset, colour change including black, Classic/New switch both ways, an AI field containing motorcycles.
- `Docs/Report073/VALIDATION.md` with a PASS/explained disposition per item.

### Outstanding after this round (as of 2026-10-03)

- Awaiting Dan: review of 0.73, especially his verdict on the Blender motorcycle and rider (continue to ATV/cars or change approach), the cloud look (rule 12) and the snow scenes; recheck of the Reverse s 1583 bump.
- From earlier results, not raised by Dan: the widest 32 m/s corner cut at the Climbing Ridge Cut entrance can cross into the far bank; the full-throttle line brushes the Downhill Ridge Cut berm; airborne riders at ~38 m/s can still clear the Homeward berm (race scenes); High Ridge Drop barrier is a suggestion only; remaining edge stations in `Docs/Report069/partA-remaining.txt`.
- Open: CR-118 intermittent spoken-title clipping.
- Possibly stale, needs Dan's yes/no: CR-010 slightly tighter steering.
- Deferred: physical Steam Deck / controller / save-migration checks; friend test of the packaged build on another PC.
- Later, not authorized: weather affecting grip; remaining vehicles and drivers in Blender; world scenery upgrade; stunt track; Trickum course; vehicle stats.

## Previous delivery — Time of day and weather + AI lost after jumps + 1 report — 0.72.0-review1 — DELIVERED, REVIEWED BY DAN (no bugs reported; additions in 0.73)

- **DELIVERED:**
  - Source `859d068a2b758f73a9db7985b1f9fb69c3e367f7` pushed and verified on origin/main.
  - Fresh 0.72.0-review1 Windows build: 0 errors, 33 warnings, 4m04s.
  - Published [game-72000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-72000) (known draft-lookup miss, completed with `--resume-draft`).
  - All 234 Latest files match the public signed manifest; public download, signature, install and startup pass.
  - [Delivery evidence](Docs/Report072/PUBLICATION.md).
- **Play-Racer.cmd (unchanged):** launched a responsive managed 72000, muted, with settings restored byte-for-byte; no pending updates. Latest root, current 72000 and previous 71000 retained.
- **Cleanup:**
  - Builds 9,996,388,496 → 7,899,039,178 bytes.
  - C: free 307,678,658,560 bytes.
  - All debug report history preserved.
- **SESSION HANDOFF: STOP.** Wait for Dan's review of 0.72 (time of day and weather, AI on the Mountain jumps, the Free Roam trail). A documentation-only delivery commit follows; playable source remains `859d068a`.

- **Authorized by Dan (2026-10-03)** from debug session `2026-10-03_12-02-07-031_bc36a2` (CLOSED, exported as `..._bc36a2_f6f5231c.zip`; 1 report, captured on 0.71.0-review1 build `4f3fb34c`), his written request to investigate AI riders getting lost after big jumps, and his request to start the next feature. Folder with the full-size screenshot: `C:\Users\danmo\AppData\LocalLow\DefaultCompany\Racer\DebugReports\2026-10-03_12-02-07-031_bc36a2`.
- **Dan is away for a few hours and wants this round to run unattended.** Every design decision needed is written below. Do not stop to ask about design; make the smallest reasonable choice, record it, and continue. Stop only for a real external blocker (rule 7).
- **Starting point:** main `06f73ee9` (documentation commit; playable source `c022d3d6`, 0.71.0-review1 / game-71000). This TODO edit and the archive move are uncommitted and belong in the safety checkpoint.
- **Scope is exactly Parts A–C below, in that order.** Verify A and B before starting C, so geometry/AI evidence is not mixed with the look change. Section 5A applies to all geometry.
- **Dan accepted the 0.71 "Clear Day" look (2026-10-03): "looks good, definite improvement."** Build on it as delivered; do not retune it.

### Results (2026-10-03, Claude Code)

- **Safety checkpoint:** `b8f5ccbc` (this TODO plan and the archive move), pushed before any change. Version 0.72.0-review1 / build 72000.
  - Evidence: [Docs/Report072/VALIDATION.md](Docs/Report072/VALIDATION.md), [AI_JUMPS.md](Docs/Report072/AI_JUMPS.md), [LOOK.md](Docs/Report072/LOOK.md).
  - Tools in `Tools/Report072/`; play-mode checks `Assets/Scripts/Report072Checks.cs` (rides, AI races) and `Report072CondChecks.cs` (menus, conditions races, Free Roam cycle, debug metadata); look evidence runner `Assets/Scripts/ConditionsBench.cs` (command-line opt-in only).
- **Part A — BUG-001 terrain tear: PASS.**
  - Cause: the Summit Homeward **supported-return ribbon** (14 m strip, vertices only at its edges) folds into bow-ties at its hairpin and its two legs overlap at different heights; beside it a spiked terrain bank (to ~84 m, then a 4–5 m cliff) and stray crown fragments (the "pale faceted terrain"), all hidden by the catch mound until 0.71.
  - Fix in all six Street-Loop-world scenes: the ribbon rebuilt as one clean surface (0.5 m grid, smooth outline, original end lines); the broken part (hairpin + overlap) follows the ground; inside the protected Homeward landing/run-out corridor it keeps the 0.71 driving surface exactly; collider = top surface only (a collidable edge skirt launched a landing moto and was removed). Spiked bank smoothed (2 trees re-grounded); 0.71 tree leftovers fixed (6 trunkless crowns removed, 6 lobes moved with their trunks, 4–6 stray fragments removed).
  - Box perimeter scan, all six scenes: folds 0, terrain through the trail 0, visual/collision mismatches 0; the remaining hits are natural slopes/creases (listed).
  - Rides 6/6 (moto/ATV, both ways through the hairpin, across Dan's position). Homeward flights (3 moto + 3 ATV): distances and touchdowns equal 0.71, upright, no reset, open run-out; lean during the coasting run-out slightly larger in some runs (min up 0.67–0.98 vs 0.96–0.98).
- **Part B — AI lost after big jumps: PASS.**
  - Cause: rivals were reset in mid-air (5 s "no progress" during a 5.6–6.4 s flight) and the earned-anchor recovery sent them 400–1,062 m back (off the minimap); rivals also had no fall-through failsafe (`VehicleRespawn` disabled on clones). Real failures: Homeward 31.4 m/s take-offs overshoot; South Face < 37 m/s land short; the Homeward run-up U-turn.
  - Fix (`RoadDriver`, `VehicleRespawn`, `MountainFlights` data): airborne riders are neither stuck nor progressing; tracked progress counts on decks; rival fall-through failsafe; racing AI uses the player's 0.68 nearest-point recovery; Homeward AI take-off limit 29.5 m/s; South Face entry 31 m/s, full-throttle run-up on the fastest line; commit to a flight only when facing it; full lock beyond 90° on the Mountain courses.
  - Before → after: Gully Fwd 6/57 → 17/18; Homeward 6/9 → 18/18; South Face 1/141 → 12/12; Gully Rev 0/79 → 18/18. Recoveries 273 (median 409 m) → 6 in 4 races (0–76 m). Every rival finished all four after races; none vanished from the minimap.
- **Part C — time of day and weather: implemented (Dan judges the look).**
  - Day (Clear Day unchanged) / Dusk / Night × Clear / Rain / Snow on `WorldLook`; race setup options (remembered), Free Roam live cycle (1 real minute = 1 game hour, from 08:00, clock in the HUD, Free Roam Weather option); headlights on every vehicle, glowing arrows/gates, stars and moon; rain/snow particles (none under cover), wet roads, snow cover, rain sound on the Ambience volume; menus/garage Clear Day; debug HUD and bug reports record the conditions.
  - Frame rate 3840×2160 (GPU median): Day/Clear 3.39–3.72 ms (0.71: 3.47–3.85), worst Night/Snow Street 4.37 ms = 229 fps.
  - Checks: menu rows with controller/keyboard/mouse, persistence, garage Clear Day; Night/Rain race (Street) and Dusk/Snow race (Forest) start→finish→results→record; Free Roam dusk→night; debug metadata.

### Part A — Free Roam terrain tear (1 report)

- **BUG-001** Street Loop Forward, Free Roam (692.07, 79.81, 92.50), heading 28. "Clean this up." A dirt patch ahead of the bike is torn: overlapping grey slivers and a dark crack run across it, its left edge is a raw cut face, and pale faceted terrain pokes up beside it. It lies in or beside the area 0.71 reshaped when it removed the Homeward catch mound (check first, rule 5).
- Replace with one clean, connected, collidable surface meeting the trail and the surrounding ground smoothly. Check the whole perimeter of the 0.71 reshaped box (launch-frame s 316–545, |x| ≤ 50 m) and the re-seated trails for the same kind of tear and fix what is found, listing each. Apply to all six Street-Loop-world scenes. The jump approach, lip and the open landing/run-out stay as delivered.

### Part B — AI riders get lost after big jumps (Mountain Loop Forward and Reverse)

- **Dan's report:** "It seems sometimes AI riders get lost after big jumps. On the first jump on Mountain Forward I have seen them land but then just disappear. At first I thought maybe they were resetting but I have seen it multiple times. I have seen them land on the second ramp multiple times. On Reverse I think it happens on the second big jump, because I can see them in front of me on the radar and they suddenly disappear."
- **Dan's follow-up (2026-10-03), important:** he also sees AI come up short on the jumps, but separately he has watched AI riders **land exactly where they should and then disappear**. "Maybe resetting, but why if they landed successfully?" So there are two problems: failed jumps, and a recovery that fires on a rider who is fine.
- **Lead for the "landed fine, then vanished" case (read from `Assets/Scripts/RoadDriver.cs`; confirm before changing):** `TrackRecoveryProgress` treats a car as off-route when it is not within `halfWidth + 3` m horizontally and 6 m vertically of the projected route point. It then fixes a `rejoinTarget` at that moment and counts `rejoinStuck` whenever the car is not getting at least 2 m closer to that fixed target. `racerStuck` fires after 5 s of that (`noProgress > 5 && trackingRejoin`) and calls `TryRecover` → `TryRecoverLocal`, i.e. the old earned-anchor placement. A long flight, or a landing on a deck where the route projection resolves to another level or a station far from the car (multi-level mountain, flight chord across a bend), can keep `near` false while the rider is flying and then riding correctly, so a healthy rider is teleported back a few seconds after landing. Check whether the projection used here (`DriveRoad.Project` vs tracked progress) is the cause, and log `near`, `trackingRejoin`, `rejoinStuck` and the projected station through each big jump.
- A rider that is airborne, or that has landed and is moving forward along the route, must never be classed as stuck. Fix the detection, not only the place it recovers to.
- **Already known from earlier rounds (likely the same thing):**
  - Production AI stops at the Homeward deck in Forward (s 2316) and undershoots/stops at the South Face receiving deck in Reverse (s ≈ 1181–1183); both recorded as pre-existing since 0.63.
  - 0.68 changed only the PLAYER reset to "nearest track point". AI recovery still uses the old earned-anchor selection, which 0.68 measured sending a vehicle to the start line (1,120 m back) after a Reverse South Face undershoot. A rival that is teleported far back would vanish from the minimap exactly as Dan describes.
- **Investigate first, with real races, not a single harness line:** run Mountain Loop Forward and Reverse races with the normal AI field for several laps and log, for every AI at every big jump: takeoff speed, where it lands, whether it stops/wipes out, and every AI recovery (from position/station → to position/station, distance moved). Also confirm what the minimap does with a rival that is recovering, far away or classified by `AiFinishEstimate`. State plainly what causes the disappearance.
- **Required outcome:**
  1. **AI completes the main-route big jumps reliably** in both directions (target: at least 9 of 10 attempts per jump land and continue). Fix the actual cause in the AI's approach at those jumps (speed plan, line, throttle/brake points). Do not change jump geometry, landing geometry or anything the player drives on to achieve this, and do not retune AI globally.
  2. **When an AI still fails, it recovers near where it failed**, using the same nearest-usable-track-point selection as the player (0.68 `RecoverNearest`), facing forward. It must not be sent far back and must not gain laps, gates or positions it has not earned.
  3. A rival never silently vanishes from the minimap while it is still racing. If the marker is being hidden by something other than a teleport, fix that.
- This is an explicitly authorized AI/recovery change (rule 6 exception), limited to the above. Race results, AI difficulty elsewhere and `AiFinishEstimate` classification rules stay as they are.
- Evidence in `Docs/Report072/AI_JUMPS.md`: per-jump success table before and after, and recovery distances before and after.

### Part C — NEW FEATURE: time of day and weather (visual only)

Dan's decisions (2026-10-03), all final for this round:

- Weather and time of day are **visual and audio only**. No grip, handling, AI or physics change. Lap/race records and ghosts stay in the same categories as today.
- **Races:** time of day and weather are **options the player picks at race setup**. They stay fixed for the whole race.
- **Free Roam:** a **live day-night cycle**, starting speed **1 real minute = 1 game hour** (24-minute day). Keep the speed as one clearly named value so it can be changed later.

**What to build, on the 0.71 `WorldLook` / `LookPreset` system:**

1. **Time-of-day presets:** Day (the existing Clear Day, unchanged), Dusk (warm low sun, long shadows), Night (moonlit, dark blue, stars). Night must be genuinely playable: a rider must be able to race every course at night.
2. **Weather presets:** Clear, Rain, Snow. Each combines with each time of day (9 combinations).
   - **Rain:** falling rain around the camera that reads at speed, overcast sky, heavier haze, darker wet-looking roads with stronger sheen, rain ambience audio on the Ambience bus.
   - **Snow:** falling snow, pale overcast sky, white haze, and a snow tint on grass/terrain via the ground shader so the world reads as snow-covered; roads and trails stay distinguishable from the ground.
   - No rain or snow inside the cave/tunnel sections.
3. **Night support:**
   - Headlights on the player vehicle, AI vehicles and traffic, lighting the road ahead; tail lights visible.
   - Ground arrows, checkpoint gates and the next-gate marker stay clearly visible at night (emissive/unlit). Signs readable when headlights reach them.
4. **Race setup UI:** two new options on the existing race setup screen, **Time of Day: Day / Dusk / Night** and **Weather: Clear / Rain / Snow**, default Day / Clear, remembered between sessions, working with controller, keyboard and mouse in the existing option style. No other menu changes.
5. **Free Roam:** the cycle blends continuously through the presets (`LookPreset.Lerp`), starting at 08:00 each time Free Roam starts. Headlights switch on automatically when it gets dark. Show the game clock in the existing Free Roam HUD text. Add **Weather: Clear / Rain / Snow** to Free Roam start in the same option style, default Clear. The sun and moon should move smoothly; shadows must not flicker or pop.

**Constraints:**

- Day / Clear must look exactly as 0.71 delivered.
- Readability first in every combination: arrows, gates, signs, minimap and HUD remain clear. Rain and snow must not hide the road ahead at racing speed.
- No motion blur, depth of field, film grain, lens dirt or chromatic aberration.
- **Performance at 3840×2160 (GTX 1660 Ti):** measure the same three views as 0.71 for Day/Clear, Night/Clear, Day/Rain, Night/Snow. Every combination must stay well above 60 fps; report the table. Headlights should be cheap (limit real-time shadow-casting lights; AI/traffic lights need not cast shadows).
- Menus and garage are unaffected by race options (garage stays Clear Day).
- Debug Mode: add the current time of day and weather to the debug HUD text and to each bug report's metadata (Markdown and JSON), so future reports show the conditions.

**Evidence:** screenshots of all 9 combinations from one fixed view per course family (Street, Forest, Backyard, Mountain), plus cave/tunnel at night, in `Docs/Report072/Look/`; a short capture list of the Free Roam cycle at 08:00, 12:00, 18:00, 21:00, 00:00 and 05:00; the frame-rate table.

**Rule 12:** one considered implementation, then stop. Dan judges the look and feel.

### Verification for this round (targeted, rule 11)

- Part A: before/after at the coordinate and each other tear found; one ride across each in Free Roam.
- Part B: as described; plus one full 3-lap race in each Mountain direction after the change with final positions and no vanished rivals.
- Part C: one short race at Night/Rain and one at Dusk/Snow on different course families (start, checkpoints, finish, results, records saved normally); Free Roam through one full dusk-to-night transition; options persist across relaunch; race setup navigation with controller and keyboard.
- 5A.6 neighbour checks only where Part A or B touched something.
- `Docs/Report072/VALIDATION.md` with a PASS/explained disposition per item.

### Outstanding after this round (as of 2026-10-03)

- Awaiting Dan: review of 0.72 (the time-of-day and weather looks, rule 12; AI on the Mountain jumps; the Free Roam trail); recheck of the Reverse s 1583 bump.
- From 0.72 results, for Dan's review: Homeward run-up U-turn — a rival occasionally stalls on its outer corner and is put back ~75 m (2 of 18 passes); the Homeward coasting run-out leans a little more in some flights after the trail rebuild; the Free Roam cycle speed is one value (`WorldLook.FreeRoamHoursPerRealMinute`).
- From earlier results, not raised by Dan: the widest 32 m/s corner cut at the Climbing Ridge Cut entrance can cross into the far bank; the full-throttle line brushes the Downhill Ridge Cut berm; airborne riders at ~38 m/s can still clear the Homeward berm (race scenes); High Ridge Drop barrier is a suggestion only; remaining edge stations in `Docs/Report069/partA-remaining.txt`.
- Open: CR-118 intermittent spoken-title clipping.
- Possibly stale, needs Dan's yes/no: CR-010 slightly tighter steering.
- Deferred: physical Steam Deck / controller / save-migration checks; friend test of the packaged build on another PC.
- Later, not authorized: weather affecting grip; graphics upgrade steps 2–3 (vehicle/driver remodel, world scenery).

## Previous delivery — Project cleanup and Mountain polish — 0.61.0-review1

The following records the previous 0.61 delivery. Its polish acceptance is superseded by the urgent regression correction above; older completed backlog decisions remain closed.

### ACTIVE / KNOWN

- CR-118: intermittent spoken-title clipping remains open; no later definitive human resolution. No audio retuning in this pass.
- This pass is implemented and awaits Dan's gameplay review: local Forest Forward tree grounding; Free Roam startup and shaped Start/Menu hint; ineffective Race Complete root Back prompt removed; Mountain support/prop grounding and Reverse Summit Traverse forward-facing merge.
- Targeted checks: Forest/cave preservation, controller/keyboard UI flow, representative Forward drive, 1,215 support probes, and final motorcycle/ATV production-driver traversals continuing beyond the Reverse merge. Both final traversals complete with zero resets/recoveries. Early fixture failures and local seam correction are documented in [Docs/MountainPolish/VALIDATION.md](Docs/MountainPolish/VALIDATION.md). No global physics/AI/recovery retuning.
- Safety checkpoint: clean `b29330bc4ceecf8eda2dba77697583f538db002e`. Completion source `56e63e4797d912f5e5822dee9f39eb3afc47919a` pushed/verified on origin/main. Fresh Windows build: zero errors, 21 warnings, 3m33s. Published [game-61000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-61000); all 233 Latest files match the public signed manifest. Public install/startup and unchanged Play-Racer.cmd launch of managed 61000 pass; settings restored and soundtrack preserved. [Delivery evidence](Docs/MountainPolish/PUBLICATION.md).
- Cleanup: Builds **9,565,965,293 → 7,593,020,763 bytes**; **3,578,626,950 bytes** disposable copies removed; C: free **294,350,192,640 bytes**. Current 61000 / previous 60000 retained. **STOP for Dan’s gameplay review.**

### FUTURE EXPANSION

- **Time of day and weather (added by Dan 2026-10-03; AUTHORIZED as 0.72 Part C).** Built as additional presets on the 0.71 look system. **Dan's decisions (2026-10-03):** (1) weather is VISUAL ONLY to start: no grip/handling change, so records and ghosts stay comparable; (2) RACES use one fixed time of day per race (at least at first), no change during a race; (3) FREE ROAM / open world gets a live day-night cycle. Night needs vehicle headlights and readable arrows/gates/signs. Rain: particles, darker sky, fog, wet-look road, rain audio. Snow: particles plus a white ground tint. (4) In RACES, time of day and weather are an OPTION the player picks at race setup. (5) FREE ROAM cycle speed starts at 1 real minute = 1 game hour (a full day in 24 minutes); Dan will adjust after trying it. Make the speed a single easily changed value.
- **Graphics upgrade (added by Dan 2026-10-02).** Step 1, lighting and atmosphere, is authorized as 0.71 Part C; steps 2–3 below are not authorized yet. Minimum goal: better-looking vehicles and drivers. Dan has Blender installed. Suggested order when scheduled: (1) lighting/post-processing/material pass in the existing URP setup; (2) one pilot vehicle + driver remodel for Dan's approval before doing the rest; (3) world/scenery later, if wanted. Must preserve vehicle colliders, handling, camera clearance, color selection and driver/vehicle identity. Free CC0 assets may be proposed; no paid assets without Dan's approval.
- New Trickum-area course.
- Dedicated stunt track.
- More vehicles and visible, measurable vehicle statistics.
- Possible collectible-completion special vehicle.
- Selectable drivers, appearances and clothing colors.
- Multiplayer / split-screen.

### DEFERRED

- Physical Steam Deck gameplay/controller and migration checks while Dan's Deck is unavailable.

### SOMEDAY / IDEAS

- Private online friend play.
- Larger-world import / generation tooling.

### COMPLETED / ACCEPTED

- Per Dan, 2026-10-04: the 0.73 look (snow, frozen water, clouds) and the Blender motorcycle + rider are approved ("I really like the way things are looking"); the Blender approach continues to the other vehicles.
- Per Dan, 2026-10-03: the 0.71 Clear Day look is accepted ("definite improvement").
- Per Dan, 2026-10-02: 0.68.0-review1 accepted and closed, including the player reset rule (nearest track point, facing forward, always succeeds). Reopen only if Dan raises it.
- Per Dan, 2026-10-02: 0.66 BUG-001 (lower main route) and BUG-002 (South Face Summit jump) verified; CR-087 Trickum ramp complete; separate lake/woodland circuit (CR-040) complete. Do not reopen without Dan's request.
- Dan's Backyard Loop Forward and Reverse accepted. The old back-property dirt trail / gully concept became this course and is complete, not a future track.
- Forest Forward cave accepted for now. No additional cave work; reopen only on Dan's explicit request.
- House 3 / Forest / Laurel AI issue complete, including BUG-009 and associated mapping/stuck-AI work. A route atlas is no longer an active prerequisite.
- General all-track navigation-arrow pass removed from active backlog. This pass addresses only the reported Mountain Reverse Summit Traverse rejoin.
- Ghosts accepted; older human-test-pending wording is superseded.
- Other later accepted/closed reconciliations remain authoritative.

## History archive

- 2026-10-04 (fifth move): the 0.71 "Previous delivery" section was moved verbatim to the end of the archive.
- 2026-10-03 (fourth move): the 0.69 and 0.70 "Previous delivery" sections were moved verbatim to the end of the archive.
- 2026-10-03 (third move): the 0.67 and 0.68 "Previous delivery" sections were moved verbatim to the end of the archive.
- 2026-10-02 (second move): the 0.62–0.66 "Previous delivery" sections were moved verbatim to the end of the archive.
- Everything formerly below this point (historical delivery records, phases 0-9, the old bug tracker, CR-001 through CR-121, the decision log and old session handoffs) was moved VERBATIM to [PROJECT_TODO_ARCHIVE.md](PROJECT_TODO_ARCHIVE.md) on 2026-10-02 at Dan's request, to keep this file small enough to read in full every round.
- The archive is reference only. Unchecked boxes and "open/pending" labels inside it are superseded by the lists above; the latest explicit decision wins. Search it when a bug or rule needs history (known-good commits, earlier decisions, CR details). Do not delete it and do not append new work to it.
- When this file grows again, move the oldest "Previous delivery" sections to the end of the archive, verbatim.
