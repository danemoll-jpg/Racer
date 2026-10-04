# Woodstock Rush
## Project Management / TODO / Astra-Codex Handoff

## Mandatory standing workflow

- **PROJECT_TODO.md is the source of truth** for project state, bugs, decisions, backlog and current work.
- **[CODEX_RULES.md](CODEX_RULES.md) contains the mandatory standing workflow/release rules.** Every future Codex task MUST read and follow it before making project changes, even when the individual prompt does not repeat those rules.
- A later explicit instruction from Dan may override a standing rule for that specific task.
- Do not silently modify or weaken CODEX_RULES.md. Changes to standing rules require an explicit instruction from Dan.
- Root AGENTS.md points future Codex tasks to both files.
- From 2026-10-02 the coding agent is Claude Code. The same two files govern it; root CLAUDE.md (created in the 0.67 round) is its discovery pointer.

## CURRENT — Snow scenes and frozen water, thunderstorms, clouds, first Blender models (motorcycle + rider) — 0.73.0-review1 — DELIVERED, AWAITING DAN'S REVIEW

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

## Previous delivery — Lighting and atmosphere pass + 3-report follow-up + Free Roam landing — 0.71.0-review1 — DELIVERED, CLEAR DAY LOOK ACCEPTED BY DAN (follow-ups in 0.72)

- **DELIVERED:**
  - Source `c022d3d61c2c4a59cff612f8aaaafccd193860c5` pushed and verified on origin/main.
  - Fresh 0.71.0-review1 Windows build: 0 errors, 19 warnings, 2m52s.
  - Published [game-71000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-71000) on the first attempt.
  - All 233 Latest files match the public signed manifest; public download, signature, install and startup pass.
  - [Delivery evidence](Docs/Report071/PUBLICATION.md).
- **Play-Racer.cmd (unchanged):** launched a responsive managed 71000, muted, with settings restored byte-for-byte; no pending updates. Latest root, current 71000 and previous 70000 retained.
- **Cleanup:**
  - Builds 9,966,067,109 → 7,877,434,994 bytes.
  - C: free 309,717,004,288 bytes.
  - All debug report history preserved.
- **SESSION HANDOFF: STOP.** Wait for Dan's review of 0.71 (geometry and the look). A documentation-only delivery commit follows; playable source remains `c022d3d6`.

### Results (2026-10-03, Claude Code)

- **Safety checkpoint:** `e42ea6bb` (this TODO plan), pushed before any change. Version 0.71.0-review1 / build 71000.
  - Evidence: [Docs/Report071/VALIDATION.md](Docs/Report071/VALIDATION.md) and [LOOK.md](Docs/Report071/LOOK.md).
  - Tools in `Tools/Report071/`; play-mode checks `Assets/Scripts/Report071Checks.cs`; look evidence runner `Assets/Scripts/LookBench.cs` (command-line opt-in only).
- **Part A — Free Roam Homeward catch mound: PASS.**
  - Active in all six Street-Loop-world scenes (both Street Loops, both Forest Loops, both Backyards); changed in all six. Mountain race scenes: inactive there, unchanged (no file or referenced asset touched).
  - The landing sheet was removed. The terrain in the launch frame's s 316–545, |x| ≤ 50 m became the smooth surface spanning the surrounding untouched ground: no mound, no cliff.
  - The buried trails, 4 acorn cairns and ~30 trees came out; all re-seated. The routes in the box, the supported-return ribbon and the RIDGE RETURN sign were re-seated.
  - 59–71 trees were removed from the run-out line (|x| ≤ 22 m, s 300–690). Jump approach, lip and site are untouched; no sign text needed changing.
  - Summit giant flights now score up to 2000 m / 20 s. Dan confirmed during the round that he never wanted a cap.
  - **Flights** (3 per vehicle, full throttle from rest):
    - moto 230–231 m → 242.5–243.5 m;
    - ATV 207–208 m → 223.5–224.0 m.
    - All land upright on open ground and roll out 80–94 m with no obstacle. All 6 score (0.70: one hard-landing reject, three tree hits).
  - **Straight path between the 0.70 positions:** 0.70 blocked at the mound's end cliff; 0.71 rideable both ways, moto and ATV, no stop or reset.
- **Part B — Climbing Ridge Cut (Mountain Loop Forward): PASS, one remainder.**
  - Cause: several terrain sheets stacked at the junction with steep faces where they cross (earth banks under the 0.69 patch, the `Ground_720_320` slab).
  - Fix: one smooth collidable surface per place (entrance + both shoulders; rejoin hollow), tucked 3 cm under the pavement edges and flush with the kept 0.70 gores and 0.69 berm. Every other sheet under it was lowered 0.8 m where covered.
  - **BUG-001:** slab down 2–3 m, notch closed.
  - **BUG-002:** corner-cut lines (48 runs, 14–32 m/s, moto/ATV): rolls past 60° went 22 → 1. All 13 0.70 entry lines are now 7–12°; in 0.70 the 32 m/s lines rolled 87–180°.
  - **Remainder:** the widest 32 m/s corner cut can carry a vehicle over the far side of the trail at s 30–40 into the cut bank (ATV 80°, recovers). That is not a hidden face; left for Dan.
  - **BUG-003:** the hollow north of the shortcut and west of the main road was filled to a smooth grade flush with both road edges (up to +10.6 m); 9 trees raised with it.
  - **5A.6:** CRC jump line (max air 1.00 / 0.92 s), AI entry and rejoin, rejoin line, berm overshoots and main s 1360–1460 all match 0.70.
  - Mountain Reverse is unchanged: it shares none of these meshes.
- **Part C — Clear Day look: implemented (Dan judges the look).**
  - `WorldLook` applies one shared preset to every scene at load. `LookPreset` (+ `Lerp`) is structured for later presets and a day-night blend.
  - The preset covers: sun, trilight ambient, procedural sky and matched haze (the horizon void is hidden), ground-shader response (grass / dirt / road distinct, road sheen, shape), water (sky reflection), Neutral tonemapping, gentle grade and mild bloom.
  - Pipeline asset: HDR on, shadows 40 → 80 m. MSAA 2× is unchanged.
  - **Dropped:** screen-space AO, which cost ~1.4 ms (+45%) at 4K.
  - **Frame rate** at 3840×2160 (GPU median, GTX 1660 Ti):
    - Street 3.49 → 3.85 ms (+10%);
    - Forest 3.21 → 3.63 ms (+13%);
    - Mountain 3.01 → 3.47 ms (+15%).
    - 260–290 fps; well above 60 everywhere.
    - The remaining ~0.4 ms is the post chain as a whole; no single setting is over 10%.
  - Before/after pairs for all 8 course scenes, the cave approach, the lower route and the garage are in `Docs/Report071/Look/`.

- **Authorized by Dan** from debug session `2026-10-03_00-37-35-649_a733ff` (CLOSED, exported as `..._a733ff_54e5751c.zip`) plus his written decision on 0.70 Part B. All 3 reports were captured on 0.70.0-review1 (build `25ce7af3`). Folder with full-size screenshots: `C:\Users\danmo\AppData\LocalLow\DefaultCompany\Racer\DebugReports\2026-10-03_00-37-35-649_a733ff`. READ every comment and LOOK at every screenshot before changing anything. If the folder is missing, ask Dan for the ZIP.
- **Dan's review of 0.70:** "No problems on the Reverse track and just a couple on the [Forward] track." Mountain Loop Reverse is clean as of 0.70, including the 12 m crest outcrop. Do not change Mountain Reverse this round except where a shared mesh requires the same fix.
- **Starting point:** main `a5b2ebaa` (documentation commit; playable source `459f3f87`, 0.70.0-review1 / game-70000). This TODO edit is uncommitted and belongs in the safety checkpoint.
- **Scope is exactly Parts A–C below.** Section 5A applies to all geometry. Keep changes local. Do Parts A and B first and verify them before Part C, so geometry evidence is not mixed with the look change.

### Part A — Free Roam: replace the Homeward catch mound with a flat, open landing (Dan's decision on 0.70 Part B)

- 0.70 found that the Free Roam obstruction (0.70 BUG-001 at (895.79, 151.05, 154.34) and BUG-002 at (761.93, 109.29, 80.45)) is the **catch landing of the Summit Homeward Flight giant jump** (CR-094/CR-103, `Ground_CR103 smooth landing` plus its shaped terrain), and stopped for Dan's decision.
- **Dan's decision (2026-10-03):** the mound is not necessary. "I wanted something that would let me jump really far. If it is getting in the way of me jumping even further then just give me a flat area where it isn't blocking the main path."
- **Required, Free Roam world only:**
  1. **Keep the jump** (approach, launch ramp, lip) exactly as it is. The point of this feature is a very long jump.
  2. **Remove the raised catch mound.** Replace it with natural ground at the surrounding terrain level: a broad, flat, open, obstacle-free landing/run-out along the flight line, long enough that a full-throttle motorcycle and ATV land on it and can keep rolling. Nothing should stop the flight short; a longer jump than before is the desired result.
  3. **The straight path up the mountain is open again** between the two 0.70 positions, rideable in both directions. The landing area must not sit across that path as an obstacle.
  4. Ground left behind is continuous, supported and collidable (rule 4): re-ground trees, rocks, signs and props that stood on or against the mound; no holes, floating objects or exposed undersides.
  5. Jump activity scoring/records for this jump keep working (distance should now be able to read higher). If a sign or landmark text describes the catch landing, update the wording; do not add new signs.
- **Races are untouched.** The mound is already inactive in both Mountain race scenes; confirm that and confirm those scenes are byte-identical in this area after the change.
- Verify: three full-throttle Free Roam jumps per vehicle (motorcycle, ATV): record flight distance before (0.70) and after, landing position, and that the vehicle lands on supported ground and rolls out. Then ride the straight path between the two 0.70 positions both ways.

### Part B — Mountain Loop Forward: Climbing Ridge Cut (3 reports)

- **BUG-001** Climbing Ridge Cut 24 m (767.34, 88.23, -107.26), heading 16. "Hole." A dark gap/notch sits at the left edge of the trail where the grass shoulder meets it, a few metres ahead of the bike; a grey slab wedge also sticks out at the right edge. Close the hole with collidable ground flush with the trail, and tuck or remove the slab.
- **BUG-002** Climbing Ridge Cut 6 m (753.74, 87.67, -120.91), heading 39. "If you cut through the grass here (which is hard not to) you get flipped." This is the grass wedge on the inside of the shortcut entrance, beside the OPTIONAL SHORTCUT / CLIMBING RIDGE CUT sign; its surface is visibly crumpled. 0.70 fixed entries that stay on the pavement and recorded that faces under the 0.69 patch remain. Dan's line cuts the corner across the grass.
  - Reproduce by entering across the grass wedge at several speeds (including full throttle) and lines. Make the wedge one smooth, drivable surface with clean collision, with nothing hidden under it that a wheel or chassis can catch (lower or remove buried faces; a small visible change to the wedge is acceptable here, Dan prefers not being flipped).
  - If the sign post is part of the cause, move the sign a little further from the driving line; keep it readable.
  - Target: no flip for corner-cutting entries up to full throttle, motorcycle and ATV. This also covers the 0.70 remainder ("at 32 m/s entries still roll").
- **BUG-003** Climbing Ridge Cut 278 m, at the rejoin (998.48, 137.80, -57.98) main s 1393, heading 56. "Fix this hole." A crater-like pit with steep walls sits in the gore between the shortcut trail and the main road, directly beyond the trail. Fill it to a natural, smooth grade continuous with the surrounding ground and both road edges. The 0.69 berm and the two 0.70 gore surfaces stay functional.
- The Climbing Ridge Cut jump system is protected; verify its jump line is unchanged. Check whether the same meshes exist in Mountain Reverse and keep them consistent only if they are shared.

### Part C — NEW FEATURE: lighting and atmosphere pass (all courses)

Dan chose this as the first step of the graphics upgrade (2026-10-03). It is a look change only: no geometry, physics, AI, routes, UI layout or audio.

- **Goal:** the game should look clearly better at first glance on every course and in Free Roam, while staying the same stylized low-poly world. Today the scenes use flat default lighting, a plain sky and a grey-brown void at the horizon.
- **Build it once, as named presets.** One shared look setup applied to every scene by code/profile, not hand-edited per scene. Structure it as a small set of named presets with exactly one preset implemented now: **"Clear Day"**. Day/night and weather (rain, snow) are planned next and must be addable later as further presets without rework, including a live day-night cycle in Free Roam that blends between presets over time. Design the preset data so two presets can be interpolated (sun angle/colour, sky, fog, ambient, exposure). Do not build them now.
- **What "Clear Day" should include (URP, existing pipeline assets):**
  - sun direction, colour and intensity chosen for good shape and depth on hills and trees; soft shadows with sensible distance and cascades so shadows are clean near the vehicle and do not pop;
  - ambient / environment lighting so shaded sides are not flat or black;
  - a proper sky (gradient or procedural) with a horizon that hides the edge of the world; light distance haze/fog matched to the sky colour;
  - a post-processing volume: tonemapping and gentle colour grading, ambient occlusion, mild bloom, anti-aliasing;
  - a light material pass where it is cheap and safe: road, dirt trail, grass and lake water should read as different surfaces (the lake may get a simple reflective/animated water look).
- **Hard constraints:**
  - **Readability first.** Ground arrows, gates, signs, the minimap, HUD and menu text must be at least as clear as now. The image must not get darker or murkier overall.
  - No motion blur, depth of field, film grain, lens dirt, chromatic aberration or heavy vignette.
  - **Performance:** Dan plays at 3840×2160. Measure frame rate before and after at three fixed views (one Street Loop, one Mountain, one Forest/Backyard). Keep it at or above 60 fps there and within about 10% of the 0.70 figures; if a setting costs more than that, reduce or drop it and say which.
  - Applies in races, Free Roam, garage and menus consistently. The cave/underground sections must stay playable (not black); give them the lighting they need.
  - Vehicle colours and driver appearance must still read true in the garage and in play.
- **Evidence:** before/after screenshot pairs from the same fixed viewpoints on all 8 course scenes plus the garage, in `Docs/Report071/Look/`, and the frame-rate table.
- **Rule 12:** one considered implementation, then stop. Dan judges the look and asks for adjustments.

### Verification for this round (targeted, rule 11)

- Before/after view at each of the 3 coordinates and of the Free Roam landing, with a PASS/explained disposition in `Docs/Report071/VALIDATION.md`.
- 5A.6 neighbour checks limited to what this round touches: Climbing Ridge Cut entrance, jump and rejoin; 0.69 BUG-008 berm; Forward main s 1380–1440. Results must match 0.70 except where intentionally changed.

### Outstanding after this round (as of 2026-10-03)

- Awaiting Dan: gameplay review of 0.71; his verdict on the Clear Day look (rule 12, no further tuning without his notes); recheck of the Reverse s 1583 bump.
- From 0.71 results, for Dan's review:
  - frame-time cost of the look at 4K is +10–15% (Forest/Mountain 3–5 points over the ~10% target). The lever is the post chain (estimated ~0–4% without it, losing tonemapping/grade/bloom). AO was dropped (+45%);
  - the widest 32 m/s corner cut at the Climbing Ridge Cut entrance can run over the far side of the trail into the cut bank (ATV 80°, recovers).
- Next feature after this round (Dan is interested, not yet authorized): time of day (day / dusk / night) and weather (sun, rain, snow) as further look presets. Dan decided 2026-10-03: weather visual only; races keep one fixed time of day; Free Roam gets a live day-night cycle (details in FUTURE EXPANSION).
- From earlier results, not raised by Dan: the full-throttle line brushes the Downhill Ridge Cut berm; airborne riders at ~38 m/s can still clear the Homeward berm (race scenes); High Ridge Drop barrier is a suggestion only; remaining edge stations in `Docs/Report069/partA-remaining.txt`; production AI stops at the Homeward deck and undershoots the South Face deck (both pre-existing).
- Open: CR-118 intermittent spoken-title clipping.
- Possibly stale, needs Dan's yes/no: CR-010 slightly tighter steering.
- Deferred: physical Steam Deck / controller / save-migration checks; friend test of the packaged build on another PC.

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

- 2026-10-03 (fourth move): the 0.69 and 0.70 "Previous delivery" sections were moved verbatim to the end of the archive.
- 2026-10-03 (third move): the 0.67 and 0.68 "Previous delivery" sections were moved verbatim to the end of the archive.
- 2026-10-02 (second move): the 0.62–0.66 "Previous delivery" sections were moved verbatim to the end of the archive.
- Everything formerly below this point (historical delivery records, phases 0-9, the old bug tracker, CR-001 through CR-121, the decision log and old session handoffs) was moved VERBATIM to [PROJECT_TODO_ARCHIVE.md](PROJECT_TODO_ARCHIVE.md) on 2026-10-02 at Dan's request, to keep this file small enough to read in full every round.
- The archive is reference only. Unchecked boxes and "open/pending" labels inside it are superseded by the lists above; the latest explicit decision wins. Search it when a bug or rule needs history (known-good commits, earlier decisions, CR details). Do not delete it and do not append new work to it.
- When this file grows again, move the oldest "Previous delivery" sections to the end of the archive, verbatim.
