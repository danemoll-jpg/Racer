# Woodstock Rush
## Project Management / TODO / Astra-Codex Handoff

## Mandatory standing workflow

- **PROJECT_TODO.md is the source of truth** for project state, bugs, decisions, backlog and current work.
- **[CODEX_RULES.md](CODEX_RULES.md) contains the mandatory standing workflow/release rules.** Every future Codex task MUST read and follow it before making project changes, even when the individual prompt does not repeat those rules.
- A later explicit instruction from Dan may override a standing rule for that specific task.
- Do not silently modify or weaken CODEX_RULES.md. Changes to standing rules require an explicit instruction from Dan.
- Root AGENTS.md points future Codex tasks to both files.
- From 2026-10-02 the coding agent is Claude Code. The same two files govern it; root CLAUDE.md (created in the 0.67 round) is its discovery pointer.

## CURRENT — World scenery upgrade + rider gestures (fist wave, victory celebration) — 0.78.0-review1 — DELIVERED, AWAITING DAN'S REVIEW

- **Authorized by Dan (2026-10-04).** He accepted the recommendation to upgrade the world scenery next (graphics upgrade step 3), before filming the trailer, and asked for rider gestures: "a button for waving your fist at someone. And the AI drivers would do this if someone runs into them or something, and maybe a celebratory dance, two fists in the air, when someone wins a race."
- **Starting point:** main at the 0.77 documentation commit; playable source as recorded in the 0.77 DELIVERED entry (0.77.0-review1 / game-77000). This TODO edit is uncommitted and belongs in the safety checkpoint. Dan has not reviewed 0.77 yet; do not change 0.77 work in this round.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–B below.** Do Part B first (small), then Part A.

- **DELIVERED:**
  - Source `1f2de343f379928bf08c812f2ca2b18ec3f96378` pushed and verified on origin/main.
  - Fresh 0.78.0-review1 Windows build: 0 errors, 2 warnings, 2m42s.
  - Published [game-78000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-78000) (known draft-lookup miss, completed with `--resume-draft`).
  - All 236 Latest files match the public signed manifest; public download, signature, install and startup pass.
  - [Delivery evidence](Docs/Report078/PUBLICATION.md).
- **Play-Racer.cmd (unchanged):** launched a responsive managed 78000, muted, with settings restored byte-for-byte; no pending updates. Latest root, current 78000 and previous 77000 retained.
- **Cleanup:** Builds 10,066,636,651 → 7,949,978,005 bytes; 1.72 GB hosted check install and 2.03 GB scratch outside the project removed. C: free 274,774,908,928 bytes.
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.78 (the new scenery look — Settings > Display > Scenery switches New / Classic — and the gestures: F / RB; rule 12) and of 0.77. A documentation-only delivery commit follows; playable source remains `1f2de343`.

### Results (2026-10-04, Claude Code)

- **Safety checkpoint:** `a696e27a` (this TODO plan), pushed before any change. Version 0.78.0-review1 / build 78000.
- Evidence: [Docs/Report078/VALIDATION.md](Docs/Report078/VALIDATION.md), [Scenery/Pairs/](Docs/Report078/Scenery/Pairs/) (Classic | New, Day / Night / Snow, every scene), [Scenery/Kit/](Docs/Report078/Scenery/Kit/), [Gestures/](Docs/Report078/Gestures/), [Bench/](Docs/Report078/Bench/).
- Code:
  - Part B: `RiderGestures.cs` (`RiderArms` rig + IK, `RiderGestures`), `Tools/Blender/rider.py` (arms exported as jointed parts), hooks in `VehicleVisual` (rig), `VehicleConfiguration` (adds the component), `VehicleInput` ("Fist wave" F / RB), `RaceDirector` (winner celebrates), `TrailerMode` (help line).
  - Part A: `Scenery.cs` (`Scenery` switch, `SceneryWorld`), `SceneryTrees.cs`, `SceneryBuildings.cs`, `SceneryProps.cs`, `SceneryWater.cs`, `SceneryGround.cs`; shaders `Assets/Scenery/Foliage.shader`, `Building.shader` (materials in `Resources/Scenery`); New-only branches in `SurfaceLighting.hlsl` and `MarkedGround.shader`; Blender tree kit `Tools/Blender/scenery_trees.py` → `Resources/Scenery/SceneryTrees.fbx` (render `render_scenery.py`); Settings > Display row; save field `classicScenery`.
  - Checks `Report078Checks.cs`, `Tools/Report078/`; `ConditionsBench -conditionsScenery`.
- **Part B — gestures: PASS.** Fist wave on all four vehicles × both bodies × chase and first person (left fist beside the head on the bikes, out of the driver's window in the cars, ahead of the face in first person; the other hand stays put; 1.7 s; cooldown). AI shook a fist after being rammed. Victory: only the winner celebrates (player win and AI win checked). Classic: nothing happens. Driving identical with gestures spammed (0.0000 m apart).
- **Part A — scenery: PASS (look for Dan to judge, rule 12).** A visual replacement built at load, no scene/collider/route file touched: every static collider identical New / Classic / New in all nine scenes; three course families raced with 3 AI (0 missed gates, New and Classic); giant jump 242.5 m in 8.00 s either way; the 0.77 Trailer Mode camera and conditions checks pass on the new scenery. Details per family in VALIDATION.md.
- **Frame rate (3840×2160, GTX 1660 Ti): PASS.** Worst view (mountain summit, Night/Snow) 8.57 ms = 117 fps (Classic 5.73 ms; 0.77 worst view 4.6 ms); Street 7.82 ms, Forest 7.15 ms, Mountain 4.30 ms, woods 6.08 ms. New costs about 1–3 ms of GPU. Reduced to get there: far trees have no trunk beyond 450 m, trees drawn to 1,100 m, detailed shadow-casting trees within 90 m.
- **Decisions recorded:**
  - "Kyle's house" is the "Friend across street" building (the archive: "Friend is Kyle"); it keeps its model and gets detail only. The TODO's "friend's house" is therefore the same house. "The brick house" could not be identified separately (Kyle's and Dan's are the brick ones); nothing else brick-specific was protected.
  - Fox Gully (the drive-through house with its ramps) is left as it is.
  - Fist wave on the LEFT hand everywhere (right hand on the throttle / wheel; US cars, so the driver's window is on the left). In Trailer Mode RB keeps its 0.5× toggle; F works there.
  - Celebration starts when the first racer crosses the line; with no opponents (time trial) nothing happens. The results panel partly covers the player's celebration; the camera flow was not redesigned.
  - Fence sections (one merged mesh each) and the cave / vault / portal rock were left unchanged (risk to accepted geometry or little gain). There are no stepping stones in the world.
  - Buildings, rocks, props, ground and shores are generated in the game from the existing colliders and meshes (so they fit the collision exactly); the Blender pipeline was used for the trees and bushes and the rider.
- **Fixed in passing:** first person put the camera between the face and the socks for riders in Shorts (the socks share the "eyes" material slot); `CameraViews` now uses the face's eyes only.
- **Limitations / for Dan's eye:** the look of everything (rule 12); scene load takes about 1–1.5 s longer with New (the scenery is built at load); the old forest's crowns that had no trunk become crown-only clumps (about 1,200–1,400 per scene); the Backyard Reverse undergrowth corridor is drawn as bushes.

### Part A — World scenery upgrade (graphics step 3)

The vehicles, riders, lighting, sky and weather are new; the trees, buildings, rocks and ground are still the original blockout. Bring the scenery up to the same standard with the approved Blender pipeline (`F:\blender\blender.exe`, scripts in `Tools/Blender/`, sources in `SourceArt/Blender/`, render–look–revise, at most three passes per asset family).

**The one rule that keeps this safe: this is a VISUAL replacement only.**
- No terrain shape, road, trail, jump, ramp, barrier, checkpoint, route or collider is moved, reshaped, added or removed, in any scene. Section 5A applies in full. Every object keeps its position, footprint and collision; only what is drawn changes.
- New visuals must match the existing collision closely (rule 4): a new tree trunk stands where the old trunk collider is and is about as thick; a new building has the same footprint and height class; a new rock covers the old rock's collider. Nothing a vehicle can touch may look different from where it collides.
- Applies to all nine scenes (the eight courses and `FreeRoamWorld`) so the world looks the same everywhere. Races must drive identically: same lap behaviour, AI lines and records.

**What to upgrade, in the same stylized low-poly style as the new vehicles (cleaner shapes, better proportions, more variety, not realism):**
1. **Trees and bushes:** a small kit of varied species and sizes (broadleaf, pine, young/old, a few bushes and shrubs), with shaped crowns instead of single blobs, visible branching on near trees, natural trunk taper and colour variation. A gentle wind sway in the shader. They must take the Snow look (snow on crowns) and read well at Dawn, Dusk and Night.
2. **Rocks and boulders:** a kit of faceted rocks replacing the plain lumps, including the mountain outcrops and barrier rocks (same collision).
3. **Buildings:** houses, garages, sheds, the kennel, pool house and the businesses get proper roofs with overhangs, window and door frames, trim, chimneys, porches/steps, gutters, and lit windows at night. Same place, footprint and collision for every building (the visual-only rule above). **How much freedom, per Dan (2026-10-04):**
   - **Dan's house** (with its deck, pool, pool house, kennel and garage) is the only building modelled with some accuracy on the real one. Keep its design, layout, storeys, roof type and colours exactly; add detail only. When unsure, leave it as it is.
   - **Kyle's house:** improve detail only and keep its current shape. Dan will redo it properly later from photos (backlog); do not invent a new design for it now.
   - **Every other house and building** (House 3, the brick house, the friend's house, the other neighbours, the businesses): the real ones no longer exist and Dan does not mind how they look. Design freely within the same footprint and height class so collision is unchanged: give them varied, believable styles for an older rural neighbourhood so the street does not look copy-pasted.
4. **Fences, gates, mailboxes, signs, posts, the campsite, stepping stones and other props:** cleaner models, same collision and same sign text.
5. **Ground and roads:** better surface materials through the existing ground shader (subtle variation in grass and dirt, a worn road with clean painted lines and edges, gravel driveways), and sparse ground detail near the camera where cheap (grass tufts, small flowers, leaf litter under trees) with no collision. Roads, trails, arrows and gates must stay at least as readable as now.
6. **Water edges:** lake and creek shores get a simple bank/reed treatment so water does not end in a hard line. Ice in Snow still works.

**Performance (measured, not assumed):** the world has thousands of trees. Use instancing/batching and distance levels of detail so the frame rate at 3840×2160 on the GTX 1660 Ti stays above 100 fps in the worst view (0.77 worst was about 4.6 ms); report the same three views plus a forest view and the mountain summit view, against 0.77. If a detail costs too much, reduce it and say which.

**A switch for comparison and safety:** Settings > Display gets **Scenery: Classic / New** (default New, remembered), switching the whole world's visuals. Classic stays in the project untouched.

**Evidence:** Blender render sheets per asset family; before/after pairs from fixed viewpoints on every course family and in `FreeRoamWorld` (Dan's house, the brick house, House 3, the businesses, a forest stretch, the dump and gullies, the lake, the mountain summit) at Day, Night and Day/Snow, in `Docs/Report078/Scenery/`.

**Rule 12:** one considered implementation, then stop. Dan judges the look.

### Part B — Rider gestures

Built on the 0.75 parametric rider (New models). Purely cosmetic: no effect on steering, speed, physics, AI driving or records.

- **Arm movement:** the rider currently has fixed poses. Give the arms what they need to move (a simple shoulder/elbow rig in the Blender rider, or separately pivoting arm parts), for all three poses (motorcycle, ATV, car) and both bodies, with every shirt type.
- **Fist wave (player):** a new button (pick a free key and controller button; show it in Settings > Controls and the controls hints). While pressed briefly, the rider raises one arm and shakes a fist two or three times, then returns to the controls. On the motorcycle and ATV the other hand stays on the bars. In the cars the driver shakes a fist out of the side window (or raised inside the cabin if the window treatment makes that cleaner), visible from outside. The vehicle stays fully controllable throughout. A short cooldown stops spamming. Works in races and Free Roam; visible in first person as the player's own arm.
- **AI fist wave:** an AI rider shakes a fist at whoever just ran into them: when another vehicle (player or AI) hits them hard enough to knock their line, or forces a wipeout they recover from. Aim it roughly toward the offender when practical. Not more than once every several seconds per rider, and not during their own wipeout. A little random variation so not every rider reacts every time.
- **Victory celebration:** when a race is won, the winner (player or AI) raises both fists in the air and pumps them two or three times as they cross the line and coast. On the motorcycle the rider goes no-handed for that moment; the bike stays upright and controlled as it does now after the finish. In the cars, a fist out of the window or both hands up inside the cabin, whichever reads better. Second and third place do nothing special. Show the celebration to the camera: the finish/results moment should frame the winner if the existing flow allows it without redesign.
- **Classic models:** gestures need the New models; with Classic selected nothing happens.
- Verify: fist wave on all four vehicles, both bodies, in chase and first person; AI reaction after a deliberate shunt in a race; player win and AI win celebrations; no change to lap times or handling with gestures spammed.

### Verification for this round (targeted, rule 11)

- Part B as described.
- Part A: collision unchanged. Prove it by comparing every collider and route in each scene before and after (identical), and by one race lap per course family with the AI field matching 0.77 behaviour; one Free Roam ride through the dump, gullies, tunnel and up the mountain to the giant jump.
- Scenery Classic / New switch both ways without reloading problems; Snow, Night headlights, lightning and Trailer Mode cameras all work with the new scenery (cameras must not clip into new geometry).
- Frame-rate table as described. `Docs/Report078/VALIDATION.md` with a PASS/explained disposition per item.

### Outstanding after this round (as of 2026-10-04)

- Awaiting Dan: review of 0.77 (camera views, first person, Trailer Mode) and of 0.78 (the new scenery look and the gestures); then filming the trailer from the shot list in the project.
- Possible follow-ups if Dan wants them (not scheduled): new fence models; cave / vault rock; a camera that frames the winner's celebration behind the results panel.
- Small, not scheduled: bury the storm-drain culvert box; weather affecting grip; CR-118 spoken-title clipping; Mountain berm leftovers; a way to skip a day in Free Roam; CR-010 steering (needs Dan's yes/no).
- Later: traffic vehicles in Blender; vehicle stats and more vehicles; acorn-completion special vehicle; stunt track (route shown on the map before building); Trickum course; split-screen/online; VR.
- Deferred: Steam Deck checks; friend test of the packaged build.

## QUEUED NEXT — Free Roam HUD cleanup: clear day and clock, minimap toggle, everything else hidden unless needed — target 0.79.0-review1 — NOT STARTED

Written by Claude (chat) on 2026-10-04 while the 0.78 round was running. Do not start this until 0.78 is delivered. Small round. Do not stop for design questions; the decisions are below.

**Dan's request, in his words (screenshot of the Free Roam text block, 0.77):** "I want the day and time separate from this other junk. It just too much text right here in general. I think if you are on an activity it is fine to have it somewhere but otherwise it should be hidden. I want the day and time clearly readable."

What is on screen today in Free Roam, all in one yellow block: `FREE ROAM Day 1 08:21 / Trickum pavement jump / 0.66 mi SW`, `Esc or Start: activities, retry, menu`, `WOODLAND ACORNS 0/24 / progress in pause menu`.

### Part A — Day and clock as their own element

1. In Free Roam, show the day and the time as a separate, dedicated HUD element, away from any other text, for example `Day 1` and `08:21` in a top corner. Nothing else shares that element (no "FREE ROAM" label, no activity name, no distance).
2. It must be clearly readable at 3840×2160 from a normal seat and against every sky: Dawn, Day, Dusk, Night, Rain, Snow. Larger than the present text, with an outline or a soft dark backing. Verify with screenshots at 4K in at least Day Clear, Night Clear and Day Snow.
3. A small moon-phase icon beside it is welcome if it is cheap and stays clean; skip it otherwise.
4. It must not overlap the speedometer, the radar/minimap, or the Trailer Mode help; it hides with the rest of the HUD in Trailer Mode (H).

### Part B — Remove the standing text block

1. The three standing lines go away during normal Free Roam driving. By default the only Free Roam text on screen is the day and clock from Part A (plus the existing speed/radar elements, unchanged).
2. Activity text appears only while it is relevant, and in its own place (not attached to the clock):
   - when the player is at an activity start (close enough to begin it): its name and how to start it;
   - during an attempt: the existing timer/score line;
   - for a few seconds after an attempt: the result.
   The "nearest activity / distance / direction" line is no longer shown all the time. That information stays available on the map and in the pause menu. If a map waypoint is set, its existing guidance stays as it is.
3. Acorns: no standing counter. Show `WOODLAND ACORNS n/24` for a few seconds when one is collected; the total stays in the pause menu.
4. `Esc or Start: activities, retry, menu`: show it for a few seconds when Free Roam begins, then hide it.
5. Races are not changed by this round.

### Part C — Minimap in Free Roam, with an on/off toggle

Dan (2026-10-04): "I wouldn't mind having the minimap in free roam as well which can be toggled on and off."

1. Show the minimap in Free Roam, the same one races use (reuse it; do not build a second one). It shows the player, the roads, and the map waypoint when one is set. Activity starts on it are welcome if they are cheap and stay uncluttered.
2. One key and one controller button toggle it on and off during Free Roam. Pick ones that are free in Free Roam and do not clash with the full map, camera views, reset or Trailer Mode keys; list the binding in the controls help and in the final report.
3. The choice is saved and remembered across sessions. Default: on.
4. It must not overlap the day and clock element from Part A or the speedometer, and it hides with the rest of the HUD in Trailer Mode (H).
5. Race minimap behaviour is unchanged.

Note on acorns: the `WOODLAND ACORNS 0/24` in Dan's screenshot came from one of Code's own test screenshots, not from his save. There is nothing to investigate.

### Verification

- 4K screenshots of Free Roam: normal driving (clock only), at an activity start, during an attempt, just after collecting an acorn, minimap on and minimap off, and Trailer Mode HUD off.
- Confirm race HUDs are identical to 0.78.
- Standard rule steps: TODO update, commit, push, build, publish, Play-Racer.cmd check, cleanup, final report.

## Previous delivery — Trailer / photo mode, auto camera, first-person and other views — 0.77.0-review1 — DELIVERED, AWAITING DAN'S REVIEW

- **DELIVERED:**
  - Source `952de374e30c9f550fe24681d2c5de9464f18e1b` (implementation `dd0bab13` + version) pushed and verified on origin/main.
  - Fresh 0.77.0-review1 Windows build: 0 errors, 2 warnings, 3m21s.
  - Published [game-77000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-77000) (known draft-lookup miss, completed with `--resume-draft`).
  - All 236 Latest files match the public signed manifest; public download, signature, install and startup pass.
  - [Delivery evidence](Docs/Report077/PUBLICATION.md).
- **Play-Racer.cmd (unchanged):** launched a responsive managed 77000, muted, with settings restored byte-for-byte; no pending updates. Latest root, current 77000 and previous 76000 retained.
- **Cleanup:**
  - Builds 10,063,705,343 → 7,948,158,290 bytes; 1.72 GB hosted check install and 1.86 GB scratch outside the project removed.
  - C: free 284,221,530,112 bytes.
  - All debug report history preserved.
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.77 (Trailer Mode, Auto, the player views — especially first person; rule 12). A documentation-only delivery commit follows; playable source remains `952de374`.


### Results (2026-10-04, Claude Code)

- **Safety checkpoint:** `9fe109d4` (this TODO plan and the archive move), pushed before any change. Version 0.77.0-review1 / build 77000.
- Evidence: [Docs/Report077/VALIDATION.md](Docs/Report077/VALIDATION.md); bindings: [Docs/TrailerMode/TRAILER_MODE.md](Docs/TrailerMode/TRAILER_MODE.md).
- Code:
  - `CameraViews.cs` (Part C).
  - `CameraViews.Trailer.cs` (trailer cameras, Auto).
  - `TrailerMode.cs` (mode, clean screen, guides, slow motion, conditions, screenshots, hint).
  - `RaceMenus.Trailer.cs` (pause-menu page).
  - Small hooks:
    - `ChaseCamera` keeps its pose internally and gains an offset scale; the chase view itself is unchanged.
    - `WorldLook` gains Trailer conditions and `LookHour`.
    - `WeatherEffects.StrikeNow`.
    - `RaceFlow.MarkDebugMovement(reason)` and attaching the components.
    - `RacerSave.cameraView`.
    - `LocalRadio` leaves the D-pad to Trailer Mode.
    - `RaceMenus.Core` adds the menu rows and the controls list.
  - Checks: `Report077Checks.cs` and `Tools/Report077/`. In-game 4K probe: `TrailerShots.cs` (`-trailerShots`).
- **Part A — Trailer Mode: PASS.**
  - **Toggle:** F8 or pause menu > Trailer Mode (race and Free Roam menus). The page holds every setting for controller players.
  - **Clean screen:** the HUD canvas and the debug panel are not drawn. H brings the HUD back; G shows the arrows, chevrons, gates and waypoint beacon.
  - **Cameras:** Chase, Orbit, Side, Front, Fixed, Flyover, Free and Auto, plus First person (keys 1–9; D-pad on a controller). They are smoothed, kept out of terrain and blended when changed by hand. The free camera stops at the ground and the vehicle's controls are off meanwhile.
  - **Slow motion:** Z / LB held gives 0.25×; X / RB toggles 0.5×. The physics step scales with it; giant-jump flight 244.2 m against 242.5 m at normal speed. Sound is turned down while slowed.
  - **Free Roam conditions:** time, ±1 h, clock pause, weather, moon and lightning now. `settings.json` was byte-identical after a session, and the clock, calendar and weather were restored exactly.
  - **Screenshot key:** P / F12 / R3 saves a PNG at the window's full resolution with no UI to `Screenshots` beside `DebugReports`. Checked at 3840×2160 in the game, with the HUD hidden at Day, Night/Rain and Dusk/Snow.
  - **Records:** a race in Trailer Mode saves no record ("TRAILER MODE / competitive records disabled").
  - **Frame time with the mode off:** within noise (+0.28 ms on a heavily loaded PC).
- **Part B — Auto: PASS.** Over 70 s there were 12 shots, held 5.3–8.0 s, never the same camera twice in a row. It cuts at a jump's take-off and holds until landing, never mid-air or mid-wipeout, checks the line of sight, and leaves a blocked shot after 0.6 s. Hold and skip work. First person is used sparingly.
- **Part C — views: PASS.**
  - Chase (unchanged default), Far chase, First person and Front, cycled with V / X. The choice is saved and shown in Settings > Controls.
  - All four vehicles were checked in Free Roam by day and at Night/Rain, and in a Night/Rain race.
  - **First person:** the camera sits 4.5 cm ahead of the rider's eyes; eyes, mouth, brows, hair and hat are shadows-only. Bikes look down 16° (ATV 24°) so the bars, grips and hands are in view. Cars show the windscreen frame, bonnet, dash and wheel. It leans 40 % with the motorcycle and looks a little into corners.
  - **Front:** ahead of the bodywork.
  - **Wipeouts and resets:** the view eases out to the chase camera and comes back afterwards.
  - Giant jump in first person: motorcycle and ATV pass.
  - A first-person race saved its record.
- **Decisions recorded:**
  - Conditions on demand apply in Free Roam only (races keep Race Setup).
  - Trailer Mode stays on across scene changes until turned off.
  - The first-time hint shows once per game session, so nothing is written to the save.
  - Manual Fixed: the camera stays where it is; the action key re-plants it beside the road ahead.
  - Game sound is turned down, not pitched, in slow motion.
- **Limitations / for Dan's eye:**
  - The look of each camera and of first person is for Dan to judge (rule 12).
  - Auto's line-of-sight check uses colliders: tree trunks, terrain and buildings, but not leafy crowns, which have no colliders.


- **Authorized by Dan (2026-10-04).** He wants to record a short trailer of the game with OBS, "even just for the fun of it", and asked for the game to be made easy to film. **Dan's review of 0.76 (2026-10-04): "no bugs to report."** The restored races, the dedicated Free Roam world, the storms and the garage screens are accepted; do not rework them.
- **Starting point:** main at the 0.76 documentation commit; playable source as recorded in the 0.76 DELIVERED entry (0.76.0-review1 / game-76000). This TODO edit and the archive move are uncommitted and belong in the safety checkpoint.
- Free Roam now runs in its own scene, `FreeRoamWorld`. Build the Free Roam side of this feature there; races keep working in their course scenes.
- Design decisions are below; do not stop to ask about design.

### Part A — Trailer mode (also usable as a photo mode)

A mode for capturing clean footage and screenshots, available in Free Roam and in races. It changes nothing about gameplay, physics, AI or records. Using it in a race invalidates that race for records, exactly as Debug movement does today.

- **Toggle:** one key (F8) and a pause-menu entry "Trailer Mode". A small first-time hint lists the controls; after that nothing is drawn unless asked for.
- **Clean screen:** hides every on-screen element at once: HUD, speedometer, minimap, lap/position box, Free Roam text, waypoint line, notifications, radio song text, debug panel. One key (H) brings the HUD back temporarily. Ground arrows, gates and the waypoint beacon get their own toggle (G), default hidden in Trailer Mode.
- **Cameras, switchable while riding (number keys / D-pad):**
  1. Chase (the normal camera, but smoother and a little lower and wider).
  2. Orbit: circles the vehicle slowly at an adjustable distance and height.
  3. Side tracking: low, alongside the vehicle, left or right.
  4. Front: looks back at the rider and vehicle from ahead.
  5. Fixed: the camera stays where it is and turns to follow the vehicle as it rides past (press again to re-plant it at the current spot).
  6. Flyover: a high, slow, drifting view over the vehicle.
  7. Free camera: the existing detached inspection camera, with smoothing, usable without the debug overlay.
  All camera moves are smoothed (no snapping); field of view adjustable; cameras must not clip through terrain.
- **Slow motion:** hold a key/button for 0.25× speed with smooth ramp in and out; a toggle for 0.5×. Audio pitch handling should sound acceptable or be muted in slow motion.
- **Conditions on demand (Free Roam and Trailer Mode only):** keys or a small panel to set the time of day instantly (cycle Dawn / Day / Dusk / Night, plus nudge the clock ±1 hour), pause/resume the clock, set the weather (Clear / Rain / Snow), set the moon phase (cycle through the phases), and trigger a lightning strike now. Leaving Trailer Mode restores the saved Free Roam clock, calendar and weather exactly as they were; nothing done here is written to the save.
- **Screenshot key:** saves a full-resolution PNG without any UI to a `Screenshots` folder beside the debug reports, and opens that folder from the pause menu.
- Works with keyboard and controller; show the bindings in `Docs/TrailerMode/TRAILER_MODE.md`.
- Frame rate at 3840×2160 unaffected when the mode is off.

### Part B — Auto camera for Trailer Mode (added by Dan, 2026-10-04)

- An eighth Trailer Mode option, **Auto**: the game directs itself. It cuts between the Trailer Mode cameras (orbit, side tracking, front, fixed, flyover, chase) every few seconds while Dan just rides, like a replay or attract mode.
- Sensible direction, not random noise: hold each shot about 4–8 seconds; never repeat the same camera twice in a row; prefer Fixed when the vehicle is about to pass a spot with a clear view, Side tracking or Orbit on straights, Flyover or a low Fixed view for jumps (switch as the vehicle takes off and hold until it lands); avoid cutting mid-air or mid-wipeout; never choose a shot where terrain, trees or buildings block the vehicle (check line of sight and pick another).
- Cuts are instant (no swooping between cameras). One key skips to the next shot; another holds the current shot until released.
- Works in Free Roam and in races. Slow motion and the conditions controls from Part A still work while Auto is running.

### Part C — Player camera views for normal play (added by Dan, 2026-10-04: "a couple different POVs, first person especially")

Selectable in ordinary racing and Free Roam, not only in Trailer Mode. A camera button cycles through them (pick a free key and controller button, show it in the existing controls hints) and the choice is remembered. These are normal play views: races stay eligible for records in every one of them.

1. **Chase** — the current camera, unchanged, and still the default.
2. **Far chase** — further back and higher, for seeing more of the road and for big jumps.
3. **First person** — from the rider's or driver's eyes.
   - Motorcycle and ATV: the view over the handlebars, with the bars, front fender/wheel and the rider's hands and arms visible; the rider's own head is hidden from this camera so it never blocks the view; the view leans with the vehicle a little, not fully, so it is comfortable.
   - Cars: from the driver's seat inside the cabin, with the dashboard, steering wheel (it already turns) and windscreen frame visible, looking through the glass.
   - Slightly wider field of view than chase; a small look-ahead into corners; landing and bump shake kept gentle. Rain, snow, headlights at night and the HUD all work in this view. Other riders' models are unaffected.
4. **Front / hood** — a clean forward view from the front of the vehicle with no bodywork in the way (bumper level on cars, just ahead of the bars on the motorcycle and ATV).

- Wipeouts and resets: the first-person and front views must not spin the player wildly; on a wipeout, ease out to the chase camera until the vehicle is reset, then return to the chosen view.
- The existing reverse/wrong-way guidance, arrows and gates must stay readable in every view.
- The Trailer Mode cameras (Part A) are separate and unchanged; First person is also added to the Trailer Mode camera list and to the Auto rotation, used sparingly.
- Verify: each view on each of the four vehicles in a race and in Free Roam, by day and at night in rain; a jump and a wipeout in first person; the choice persists across relaunch; a race finished in first person saves its record.

### Verification (targeted, rule 11)

- Each camera in Free Roam and in one race; slow motion through the Summit Homeward giant jump; conditions set and restored (save file byte-identical before and after a Trailer Mode session); HUD fully hidden in a 4K screenshot at Day, Night/Rain and Dusk/Snow; a race started in Trailer Mode does not save a record.
- `Docs/Report077/VALIDATION.md`. One implementation, then stop.

## Previous delivery — Restore the races, one dedicated Free Roam world, audible storms, garage screens — 0.76.0-review1 — DELIVERED, REVIEWED BY DAN (no bugs to report)

- **DELIVERED:**
  - Source `03696d00f91a3860845381fecb52d6f635b9eeec` pushed and verified on origin/main.
  - Fresh 0.76.0-review1 Windows build: 0 errors, 39 warnings, 6m44s.
  - Published [game-76000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-76000) (known draft-lookup miss, completed with `--resume-draft`).
  - All 235 Latest files match the public signed manifest; public download, signature, install and startup pass.
  - [Delivery evidence](Docs/Report076/PUBLICATION.md).
- **Play-Racer.cmd (unchanged):** launched a responsive managed 76000, muted, with settings restored byte-for-byte; no pending updates (first try the launcher waited on its window while the PC was in use; second try passed). Latest root, current 76000 and previous 75000 retained.
- **Cleanup:**
  - Builds 10,149,122,192 → 8,033,624,052 bytes; 1.72 GB hosted check install and 5.72 GB scratch outside the project removed.
  - C: free 290,636,931,072 bytes.
  - All debug report history preserved.
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.76 (Free Roam World, the not-carried list and the visible culvert below, the storms, the garage). The queued 0.77 round below starts only when Dan starts it. A documentation-only delivery commit follows; playable source remains `03696d00`.


### Results (2026-10-04, Claude Code)

- **Safety checkpoint:** `8f2e4f95` (this TODO plan and the archive move), pushed before any change. Version 0.76.0-review1 / build 76000.
  - Evidence: [Docs/Report076/VALIDATION.md](Docs/Report076/VALIDATION.md), [Storm/](Docs/Report076/Storm/), [Garage/](Docs/Report076/Garage/), [Views/](Docs/Report076/Views/), [Checks/](Docs/Report076/Checks/), [Audit/](Docs/Report076/Audit/).
  - Tools `Tools/Report076/` (scene restore `restore_scenes.py` / `scene_yaml.py`, audit, world authoring, probes, views); play checks `Assets/Scripts/Report076Checks.cs`; evidence runners in the game (command-line opt-in) `StormProbe.cs` (`-stormProbe`) and `GarageShots.cs` (`-garageShots`); route export `Assets/Scripts/Editor/CourseRouteExport.cs` (menu Racer/Export course routes).
- **Part A — races restored: PASS.** The 8 course scenes were restored document by document to 0.73 (`a877a39d`, same scenes as `859d068a`) and the 23 shared meshes 0.74 BUG-003 had edited were checked out from 0.73; kept: BUG-001 tree, BUG-002 sign removal, Mountain Forward BUG-004/005, the 0.74 Part C cave in Mountain F/R. Every file each race scene loads now equals 0.73 apart from those fixes; Forest Loop Reverse route data identical point for point; House 3 west view pixel-identical to the 0.73 shot. Forest Loop Reverse lap with 3 AI: finished, 0 missed gates, all AI finished, record saved. Street / Mountain / Backyard races load, finish and save records (the test autopilot misses one Backyard gate; the scenes are identical to 0.73 and AI code is unchanged since 0.72; all AI finish). 79 orphaned 0.74 meshes deleted.
- **Part B — one Free Roam world: PASS (list for Dan below).**
  - `FreeRoamWorld` = copy of the 0.75 Backyard Reverse scene (dump, gullies, storm drain with lights, flow, rats, Snow ice; the 0.74 winding driveway) with its own copies of the driveway meshes; course id `free-roam-world`; in the build.
  - Free Roam from any course loads it and starts at that course's own start (8/8, on the ground, no reset), with that course's name and vehicle rule. Race Setup / Start Race / Return to Menu go back to the course's scene; Tracks to the chosen course; the Free Roam clock is saved and resumes.
  - The Backyard races' own direction arrows are hidden there (29 teal, 20 gold); gates, grid and race barriers off as before.
  - Driveway (Free Roam only): 5 canopy lobes left floating over the drive by 0.74's tree moves now follow their trees; 3 trunkless crowns 8–23 m up near the lake end removed; nothing in the driving width. Motorcycle and Street Classic down and up: no stall, wipe-out or reset.
  - Rides: tunnel end to end (motorcycle, ATV) and the Backyard Reverse line through the dump and gullies: complete, no slowdown. Dan's 0.74 BUG-001 spot: clean.
  - Activities: the world's 7 sites show the best of their own and the same site's earlier course results (read only; nothing in the save changes); with Dan's save every personal best ≥ before. Acorns 22/24, explored map and landmarks unchanged. Weather, waypoint, fast travel PASS.
  - Map: Track on the map now toggles any number of course routes as overlays ([x] list), main cyan / shortcuts gold / direction / start-finish; Forest and Mountain routes dashed with "race-only route" legend; never loads a scene or moves the player; remembered. Route data regenerated from the scenes (`CourseRouteExport`), now with each course's start and vehicle rule.
- **Part C — storms: PASS.** Cause (measured with the 0.75 code as Dan plays): thunder energy was almost all below 150 Hz (inaudible on ordinary speakers; its audible part −25…−27 dBFS, level with the engine and under the radio); bolts lit 0.1 s in a random direction (mostly out of view); day flashes ×0.62. Not the cause: storms run in Free Roam's blend, no voice culling, no false "under cover", Dan's settings. Fix: fuller thunder (mid-band roll + rattle, saturation; audible part +12 dB), far thunder kept full, engine −30 % / radio −40 % dip under thunder, high priority; nearer strikes, 4 of 5 bolts in view, bolt twice as thick, lit 0.2 s + return stroke + afterglow (~1 s), flash as strong by day, still ≤ 2 pulses. After (2 min each): Free Roam Day/Rain 8 strikes, first at 9.9 s, 6/8 bolts in view, thunder +22…+27 dB over the engine above 150 Hz; Night/Rain 11, 5.6 s, 11/11, +18…+29 dB; Race Day/Rain 7, 5.9 s, 5/7, +20…+27 dB.
- **Part D — garage: PASS.** Cause: preview texture 640×280 drawn into a 620×240 box (stretched) with a high, fixed camera. Now a fixed preview panel left of a wider card (only the list scrolls); texture = panel pixel size, camera aspect follows; whole vehicle and rider from a three-quarter view; right stick / Q-E / mouse drag rotate 360°, slow auto-turn after 3 s, right stick no longer navigates in the garage; footer hint. Rider page: same panel, whole rider head to feet (car body hidden there), preview never moves with the first / middle / last row; 3840×2160 and 1280×1024 shots. The optional per-part close-up was not added.
- **Not carried into Free Roam World (for Dan):** the Forest Loop Forward cave and its bats (a roofed cut along the Forest Loop trail trench beside the dump / gullies / storm drain; carrying it needs the Forest race terrain in the Backyard area — by Dan's rule it stays in the Forest races); Forest opening jump and Forest speed traps 1/2 (on race-only Forest trails); South Face Summit Flight and all Mountain race roads (race-only); Street / Forest Reverse direction guidance and the Forest Reverse cave atmosphere (race-only); Street Loop Reverse's alternative jump / trap placements (the Forward ones are in, their Reverse results count).
- **Needs Dan's eye:** in Free Roam World (as in the Backyard Reverse race) the long culvert's concrete box stands up to 3.8 m out of the ground along its north half beside the dirt path. A quick earth cover looked worse and was removed; burying it would be a separate shaping job.
- **Standing instruction (Dan, this round):** before changing world geometry in a course scene, read that scene's route data to see whether a race route, shortcut, jump or AI line uses or passes near it; if so, do not change it for a Free Roam or cosmetic request — stop and report. Free Roam changes now belong in `FreeRoamWorld`.


- **Authorized by Dan (2026-10-04)** from debug session `2026-10-04_06-30-49-801_f2ea73` (CLOSED, exported as `..._f2ea73_6fd893dc.zip`; 2 reports, captured on 0.74.0-review1 build `6deff347`) plus his written review of 0.74. Folder with full-size screenshots: `C:\Users\danmo\AppData\LocalLow\DefaultCompany\Racer\DebugReports\2026-10-04_06-30-49-801_f2ea73`.
- **Starting point:** main at the 0.75 documentation commit; playable source as recorded in the 0.75 DELIVERED entry (0.75.0-review1 / game-75000). This TODO edit and the archive move are uncommitted and belong in the safety checkpoint.
- **Dan's review of 0.75:** he tested the new vehicles and rider customization and raised only the two garage screen issues in Part D. Treat the models and options as accepted; do not rework them.
- **Scope is exactly Parts A–D below, in that order.** Part A is a regression repair and comes first, verified before anything else.

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

### Part D — Garage screens (Dan's review of 0.75)

Dan's only complaints about 0.75 are about how the garage shows things, not the models themselves.

**D1. Vehicle selection screen.** "The vehicle selection screen has an awful squished vehicle at an angle. Can we have it be more visible to what it looks like? Maybe even be able to rotate it?"
- Find why the vehicle looks squished (wrong aspect ratio on the preview camera or render texture, non-uniform scale on the preview object, or a stretched UI image) and fix the cause so the vehicle is shown in true proportions.
- Show the selected vehicle large and clearly, whole vehicle in frame with a little margin, from a flattering three-quarter view, well lit, against a clean background, with its rider.
- **The player can rotate it:** right stick / mouse drag / Q and E turn the vehicle a full 360° about its vertical axis; when left alone for a few seconds it turns slowly on its own. Rotation must not interfere with the existing menu navigation (left stick / D-pad / arrow keys / mouse clicks on options). Show the control in the screen's existing hint style.
- Colour changes and the Classic / New switch update the same preview immediately.

**D2. Rider page.** "The avatar scrolls off the screen as you get further down the list (you can't even see the shirt you are putting on them). The avatar should be fully visible the entire time."
- The rider preview is fixed in place and fully visible from head to feet at all times, whichever option row is selected and however far the list is scrolled. Only the option list scrolls (or lay the ten rows out so no scrolling is needed); the preview never moves, is never covered by the list and never leaves the screen.
- Same rotate control as D1, so shirt, pants, hair and hat can be seen from any side. Optional if cheap: the preview frames the part being edited a little closer (head for hair/hat, torso for shirt, legs for pants) while still showing the whole rider.
- Works at 3840×2160 and at lower resolutions/aspect ratios without clipping; controller, keyboard and mouse.
- Verify with screenshots of the first, middle and last option rows selected, and of D1 at three rotation angles for each of the four vehicles.

### Verification for this round (targeted, rule 11)

- Part A: Forest Loop Reverse race lap as described; a mesh/route comparison against 0.73 for each restored scene; the House 3 area screenshots match 0.73.
- Part B: start Free Roam from each of the eight courses and confirm the same world loads, at that course's start location; show each course's route on the map in turn and several together, confirming the scene and player position never change; ride the dump, gullies and tunnel; visit the cave, giant jump, campsite; acorn count and records unchanged; clock, weather, waypoint and fast travel work; one race on each course family still loads and finishes.
- Part C as described.
- `Docs/Report076/VALIDATION.md` with a PASS/explained disposition per item.

## Previous delivery — Blender vehicles for the whole garage + rider customization — 0.75.0-review1 — DELIVERED, REVIEWED BY DAN (models accepted; garage screen fixes in 0.76)

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

## Previous delivery — Project cleanup and Mountain polish — 0.61.0-review1

The following records the previous 0.61 delivery. Its polish acceptance is superseded by the urgent regression correction above; older completed backlog decisions remain closed.

### ACTIVE / KNOWN

- CR-118: intermittent spoken-title clipping remains open; no later definitive human resolution. No audio retuning in this pass.
- This pass is implemented and awaits Dan's gameplay review: local Forest Forward tree grounding; Free Roam startup and shaped Start/Menu hint; ineffective Race Complete root Back prompt removed; Mountain support/prop grounding and Reverse Summit Traverse forward-facing merge.
- Targeted checks: Forest/cave preservation, controller/keyboard UI flow, representative Forward drive, 1,215 support probes, and final motorcycle/ATV production-driver traversals continuing beyond the Reverse merge. Both final traversals complete with zero resets/recoveries. Early fixture failures and local seam correction are documented in [Docs/MountainPolish/VALIDATION.md](Docs/MountainPolish/VALIDATION.md). No global physics/AI/recovery retuning.
- Safety checkpoint: clean `b29330bc4ceecf8eda2dba77697583f538db002e`. Completion source `56e63e4797d912f5e5822dee9f39eb3afc47919a` pushed/verified on origin/main. Fresh Windows build: zero errors, 21 warnings, 3m33s. Published [game-61000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-61000); all 233 Latest files match the public signed manifest. Public install/startup and unchanged Play-Racer.cmd launch of managed 61000 pass; settings restored and soundtrack preserved. [Delivery evidence](Docs/MountainPolish/PUBLICATION.md).
- Cleanup: Builds **9,565,965,293 → 7,593,020,763 bytes**; **3,578,626,950 bytes** disposable copies removed; C: free **294,350,192,640 bytes**. Current 61000 / previous 60000 retained. **STOP for Dan’s gameplay review.**

### FUTURE EXPANSION

- **Kyle's house from photos (added by Dan 2026-10-04; waiting on Dan).** Rebuild Kyle's house accurately once Dan supplies pictures of it. Until then it keeps its current shape. (In the scenes it is the "Friend across street - blue circle" building; 0.78 added detail only.)
- **Traffic vehicles in Blender (added by Dan 2026-10-04; low priority, not authorized yet).** Upgrade the ambient traffic cars with the approved Blender pipeline, as was done for the four garage vehicles in 0.73/0.75. Traffic drivers could use the 0.75 parametric rider with random looks.
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

- Per Dan, 2026-10-04: 0.76 accepted with no bugs reported (races restored to 0.73 around House 3, dedicated `FreeRoamWorld`, course routes as map overlays, audible/visible storms, garage preview with rotation).
- Per Dan, 2026-10-04: the 0.75 Blender vehicle designs (ATV, both cars) and the rider customization options are good and accepted. Only the garage screens needed work (0.76 Part D).
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

- 2026-10-04 (seventh move): the 0.73 and 0.74 "Previous delivery" sections were moved verbatim to the end of the archive.
- 2026-10-04 (sixth move): the 0.72 "Previous delivery" section was moved verbatim to the end of the archive.
- 2026-10-04 (fifth move): the 0.71 "Previous delivery" section was moved verbatim to the end of the archive.
- 2026-10-03 (fourth move): the 0.69 and 0.70 "Previous delivery" sections were moved verbatim to the end of the archive.
- 2026-10-03 (third move): the 0.67 and 0.68 "Previous delivery" sections were moved verbatim to the end of the archive.
- 2026-10-02 (second move): the 0.62–0.66 "Previous delivery" sections were moved verbatim to the end of the archive.
- Everything formerly below this point (historical delivery records, phases 0-9, the old bug tracker, CR-001 through CR-121, the decision log and old session handoffs) was moved VERBATIM to [PROJECT_TODO_ARCHIVE.md](PROJECT_TODO_ARCHIVE.md) on 2026-10-02 at Dan's request, to keep this file small enough to read in full every round.
- The archive is reference only. Unchecked boxes and "open/pending" labels inside it are superseded by the lists above; the latest explicit decision wins. Search it when a bug or rule needs history (known-good commits, earlier decisions, CR details). Do not delete it and do not append new work to it.
- When this file grows again, move the oldest "Previous delivery" sections to the end of the archive, verbatim.
