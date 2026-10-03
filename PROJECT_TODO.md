# Woodstock Rush
## Project Management / TODO / Astra-Codex Handoff

## Mandatory standing workflow

- **PROJECT_TODO.md is the source of truth** for project state, bugs, decisions, backlog and current work.
- **[CODEX_RULES.md](CODEX_RULES.md) contains the mandatory standing workflow/release rules.** Every future Codex task MUST read and follow it before making project changes, even when the individual prompt does not repeat those rules.
- A later explicit instruction from Dan may override a standing rule for that specific task.
- Do not silently modify or weaken CODEX_RULES.md. Changes to standing rules require an explicit instruction from Dan.
- Root AGENTS.md points future Codex tasks to both files.
- From 2026-10-02 the coding agent is Claude Code. The same two files govern it; root CLAUDE.md (created in the 0.67 round) is its discovery pointer.

## CURRENT — Time of day and weather + AI lost after jumps + 1 report — target 0.72.0-review1 — NOT STARTED

- **Authorized by Dan (2026-10-03)** from debug session `2026-10-03_12-02-07-031_bc36a2` (CLOSED, exported as `..._bc36a2_f6f5231c.zip`; 1 report, captured on 0.71.0-review1 build `4f3fb34c`), his written request to investigate AI riders getting lost after big jumps, and his request to start the next feature. Folder with the full-size screenshot: `C:\Users\danmo\AppData\LocalLow\DefaultCompany\Racer\DebugReports\2026-10-03_12-02-07-031_bc36a2`.
- **Dan is away for a few hours and wants this round to run unattended.** Every design decision needed is written below. Do not stop to ask about design; make the smallest reasonable choice, record it, and continue. Stop only for a real external blocker (rule 7).
- **Starting point:** main `06f73ee9` (documentation commit; playable source `c022d3d6`, 0.71.0-review1 / game-71000). This TODO edit and the archive move are uncommitted and belong in the safety checkpoint.
- **Scope is exactly Parts A–C below, in that order.** Verify A and B before starting C, so geometry/AI evidence is not mixed with the look change. Section 5A applies to all geometry.
- **Dan accepted the 0.71 "Clear Day" look (2026-10-03): "looks good, definite improvement."** Build on it as delivered; do not retune it.

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

- Awaiting Dan: review of 0.72; recheck of the Reverse s 1583 bump.
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

## Previous delivery — 9-report follow-up: quit confirmation, Free Roam mountain path, taller crest barrier, campsite, smoothing — 0.70.0-review1 — DELIVERED, REVIEWED BY DAN (follow-ups in 0.71)

- **DELIVERED:**
  - Source `459f3f873a91c0e34a9639339b2f16a8b0ffad5b` pushed and verified on origin/main. The completion commit is `67d08694`; `459f3f87` removes temporary editor tool copies it accidentally included.
  - Fresh 0.70.0-review1 Windows build: 0 errors, 20 warnings, 4m08s.
  - Published [game-70000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-70000). The known draft-lookup miss was resolved with `--resume-draft`.
  - All 233 Latest files match the public signed manifest; public download, signature, install and startup pass.
  - [Delivery evidence](Docs/Report070/PUBLICATION.md).
- **Play-Racer.cmd (unchanged):** launched a responsive managed 70000, muted, with settings restored byte-for-byte; no pending updates. Latest root, current 70000 and previous 69000 retained.
- **Cleanup:**
  - Builds 9,915,700,993 → 7,841,821,157 bytes.
  - C: free 307,555,516,416 bytes.
  - All debug report history preserved.
- **SESSION HANDOFF: STOP.** Wait for Dan's review of 0.70 and his decision on Part B. A documentation-only delivery commit follows; playable source remains `459f3f87`.

### Results (2026-10-02, Claude Code)

- **Safety checkpoint:** `236ecc73` (this TODO plan and the archive move), pushed before any change. Version 0.70.0-review1 / build 70000.
  - Evidence: [Docs/Report070/VALIDATION.md](Docs/Report070/VALIDATION.md) and [BARRIERS.md](Docs/Report070/BARRIERS.md).
  - Tools in `Tools/Report070/`; play-mode checks `Assets/Scripts/Report070Checks.cs`, `Report070QuitChecks.cs`, `Report070CampChecks.cs`.
- **Part A — quit confirmation: PASS (16/16 checks).**
  - Main menu QUIT GAME and pause menu Quit Game now open the existing dialog: "QUIT WOODSTOCK RUSH?" [CANCEL] [QUIT].
  - Focus starts on CANCEL; B / Esc cancel back to the same menu.
  - Tested with keyboard, controller and mouse. QUIT exits. END RACE dialog unchanged; Alt+F4 not intercepted.
- **Part B — BUG-001/002: STOPPED, nothing changed (TODO item 3).**
  - The "mound" is the **catch landing of the Summit Homeward Flight** giant jump (CR-094/CR-103, `Ground_CR103 smooth landing` plus its shaped terrain). It is a Free Roam activity Dan asked for and approved on 2026-09-21.
  - It is already inactive in both Mountain race scenes. It is unchanged since `752ca2f1` (0.18), so it is not a regression.
  - **Needs Dan's decision:**
    1. cut a rideable notch through its high end;
    2. lower that end (this changes the catch area);
    3. remove the jump from Free Roam.
- **Part C — Reverse crest barrier: PASS.**
  - **Reproduced first:** 8 full-throttle runs (4 moto, 4 ATV) left the crest at 31–35 m/s, crossed the outside of the bend 4–9.5 m above the road (Dan: about 9 m at 32 m/s), and all 8 landed 50–88 m off.
  - **Built** one natural rock outcrop filling the gore between the main road and the Summit Traverse entrance:
    - 12 m tall, sized from the measured flights;
    - 85° cliff 7 m beyond the main edge (just behind the OPTIONAL SHORTCUT sign, which stays in front of it, unmoved) and 1 m beyond the Summit Traverse edge;
    - grounded and collidable;
    - the 0.69 berm here was absorbed.
  - **After:** all 8 runs stay on the summit plateau (two wipe out against the cliff and are reset; two ATVs end in a pre-existing 3–4 m dip beside the sign). The Summit Traverse entrance and the clean lines complete. The crest itself is unchanged.
- **Part D — campsite: PASS.**
  - Mountain Forward: the camp floated 16–19 m. Each piece is now seated on the scene's own ground: tent, each figure with its log, campfire, each stone.
  - Mountain Reverse: the pieces were sunk 0.3–1.7 m; now raised. That camp is hidden under the South Face run-up ramp.
  - Terrain unchanged. The Street Loop / Free Roam camp is unmoved.
  - **Landmark "Campsite":** an ordinary map destination in all 8 course scenes, with discovery and fast travel. 5/5 checks, run on an isolated map save.
- **Part E — Climbing Ridge Cut entrance flips (BUG-008): PASS, cause found.**
  - **Reproduced:** every entry at ≥ 14 m/s hit hidden faces just under the junction surface at branch s 9–11 and was launched, then rolled 38–86° (one 180°).
  - **Cause:**
    1. the 0.68 seam cover here is long slivers 5–11 cm under the pavement with faces tilted up to 39°;
    2. **0.69 regression:** crumpled CR133 earth-bank faces 3–20 cm under the 0.69 BUG-007 patch.
  - **Fix:** in the junction only:
    - the seam-cover slivers were subdivided, and the parts under the pavement pushed 0.6 m down;
    - earth-bank vertices whose triangles lie fully under the cover were lowered (127).
    - Nothing within 0.4 m of an edge changed, so the left-edge slot stays closed and the view matches 0.69. (A wider pass made a visible notch; it was reverted.)
  - **After:** all entries at 8–26 m/s, every line, moto and ATV, are clean (roll ≤ 8°).
  - **Remains:** at 32 m/s (72 mph) entries still roll (71–180°), from faces under the 0.69 patch that cannot be lowered without visible notches. Dan to judge.
  - The AI enters and rejoins; the Climbing Ridge Cut jump line and overshoot are unchanged.
- **Part F — PASS.**
  - **BUG-003 is not a trench:** it is a 3–6 m sawtooth cliff in the terrain 10–50 m off the highway.
    - Smoothed to an even ~13° slope (no 1 m cell rises more than 0.49 m).
    - 7–11 trees per scene were re-grounded: trunk collider plus their batched pieces.
  - **BUG-004:** a 0.25–0.45 m lump where the highway strip ends. The verge now meets the strip within ±0.05 m and falls away smoothly.
  - Both are applied in all 8 course scenes, since every scene carries this part of the shared world.
  - **BUG-009:** both gores at the Climbing Ridge Cut rejoin are now one smooth surface each, flush 3 cm under the pavement edges. Terrain under them is 0.4 m down; the 0.69 berm is untouched.
- **5A.6 neighbour checks:** the 0.69 sets were re-run unchanged (VALIDATION.md).
  - **Same as 0.69:**
    - Forward checks 20/20; Forward drives 9/10; Reverse checks and drives.
    - This covers Summit Traverse, South Face, Downhill and Climbing Ridge Cut, Homeward, the 0.68 berms, the 0.69 BUG-008 berm, the lower main route and the edges.
  - **Explained differences:**
    - Forward AI Climbing Ridge Cut: a harness start-line wrap that leaves entry to the random shortcut roll. With the plan held, the AI enters and rejoins.
    - Reverse crest line and ATV throttle drive: the outcrop at work. The crest still launches the full-throttle line; it lands upright with one brush in 3/3 repeats, but rolled once in the 0.69-harness session.
    - Reverse Downhill Ridge Cut full-throttle moto line: rolls after brushing the 0.68 berm twice. Nothing there changed; this is a known marginal line.
- **Discovery:** `Builds/LauncherRelease-69000/assets` was empty (the 0.69 signed manifest and catalog were missing locally). Both were restored, byte-identical to the published game-69000 assets and signature-verified, from `Builds/Latest/game-manifest.json` and the public release.

- **Authorized by Dan** from debug session `2026-10-02_20-28-25-546_11ee21` (CLOSED, exported as `..._11ee21_f7519669.zip`) plus his written request for a quit confirmation. All 9 reports were captured on 0.69.0-review1 (build `dd3589a4`), so all 9 count. Folder with full-size screenshots: `C:\Users\danmo\AppData\LocalLow\DefaultCompany\Racer\DebugReports\2026-10-02_20-28-25-546_11ee21`. READ every comment and LOOK at every screenshot before changing anything. If the folder is missing, ask Dan for the ZIP.
- **Starting point:** main `f3d5c38c` (documentation commit; playable source `be4f86e5`, 0.69.0-review1 / game-69000). This TODO edit and the archive move are uncommitted and belong in the safety checkpoint.
- **Scope is exactly Parts A–F below.** Section 5A applies to all geometry. 0.68 is accepted and closed. The 0.69 edge/shoulder work stays; only the specific places below are corrected.
- Dan's overall verdict on 0.69: the reports are getting fewer. Keep changes local so that continues.

### Part A — Quit Game confirmation (written request)

- Every control that quits the application (main menu "Quit Game" and any other quit-to-desktop path) must first show a confirmation, e.g. **"Quit Woodstock Rush?" — [Cancel] [Quit]**.
- Focus defaults to **Cancel**. B / Esc cancels. Works with controller, keyboard and mouse, using the existing confirmation-dialog pattern (as used for END RACE / RETURN TO MENU). No other menu changes.
- "END RACE / RETURN TO MENU" already confirms and is unchanged. Alt+F4 / closing the window is not intercepted.

### Part B — Free Roam: restore the straight path up the mountain

- **BUG-001** Street Loop Forward, Free Roam (895.79, 151.05, 154.34), heading 231. A very large, steep green/brown mound rises directly across the dirt path. Dan: "This was likely put there for the race tracks but it completely impedes Free Roam. Can we remove it in Free Roam only? But make sure it still exists in the races so it doesn't mess up the tracks."
- **BUG-002** Street Loop Forward, Free Roam (761.93, 109.29, 80.45), heading 76. "This is the other side of this. It used to be a straight path up the mountain but now you can [only] get by on either side." The screenshot is the bike against the steep brown face of the same obstruction.
- **Hint from the coordinates:** the two captures are ~150 m apart and lie along the Mountain Loop Reverse main climb (0.67 reports put that road at (746, 91, 64) s 136 → (922, 157, 155) s 369). The obstruction is probably race-authored mountain geometry (a raised roadbed/embankment, fill or shoulder) present in the Free Roam world. Identify exactly which object(s)/meshes it is and which commit introduced it (rule 5) before changing anything.
- **Required:**
  1. In **Free Roam**, the straight path up the mountain between these two points is open and rideable again, as it was before. Remove, hide or reshape the obstruction for Free Roam only, and make sure the ground left behind is continuous and supported (no hole, no floating objects; rule 4).
  2. In **races** (Mountain Loop Forward and Reverse and any other course that uses that geometry) nothing changes: same road, same collision, same support. Verify by comparing the race scenes before/after.
  3. If the obstruction turns out to be a Free Roam activity feature (e.g. a jump Dan asked for earlier), do not delete it: report what it is and stop on this item.

### Part C — Mountain Loop Reverse: the crest barrier must actually stop a full-speed rider

- **BUG-005** (1008.30, 163.05, 129.66) main s 463, heading 151. "This is not nearly high enough. I go flying over this still. Must be much higher. Test against the hill leading up here at full speed and see."
- **BUG-006** (994.30, 172.40, 122.12) main s 449, heading 112, captured **airborne at 32.0 m/s (71.5 mph)**. "See?" The bike is about 9 m above the road surface (road ≈ 163 at this station) and far above the 1.8 m berm built in 0.69 at s 446–486.
- The 0.69 verification used overshoot runs that did not reproduce Dan's real approach. **Reproduce first:** full throttle up the hill leading to this crest (motorcycle and ATV), and record where the vehicle is in the air at s 440–490 (height above road, lateral position, speed). The run must reach roughly Dan's captured state before any design is chosen.
- **Then build a barrier sized from those measured trajectories**, with margin: a tall natural rock wall / cliff outcrop on the outside of the bend, high and long enough that a full-throttle motorcycle and ATV coming over the crest cannot clear it or pass its ends. Grounded, collidable, matching the mountain. A wipeout against it followed by the 0.68 nearest-point reset is an acceptable outcome; leaving the track is not.
- Keep the driving width, the **Summit Traverse shortcut entrance** and its flight/approach, and the sign visible. Replace or absorb the 0.69 berm here rather than stacking mismatched pieces. If no barrier of sensible size can contain the measured launch, say so with the numbers and propose the smallest alternative (for example easing the crest) instead of building it; do not change the crest without Dan's approval.
- Verify with at least three full-throttle passes per vehicle. Update BARRIERS.md.

### Part D — Mountain campsite (Mountain Loop Forward; check Reverse)

- **BUG-007** (1013.84, 160.89, 81.02) main s 1572, heading 285. The summit campsite (dome tent, figure, platform) floats in the air to the upper left. Dan: "Can we lower this scene for race mode so that it is on the ground, but in Free Roam don't move it. Looks like there are major differences in elevation here. Also in Free Roam can we mark this as Campsite landmark?"
- **Race scenes:** seat the whole campsite group on the actual ground in Mountain Loop Forward, and in Reverse if it floats there too (rule 4 grounding: tent, figure, props, cairns, colliders together). Do not change the terrain to meet it.
- **Free Roam:** do not move the campsite. Add it as a named landmark **"Campsite"** using the existing exploration-map / landmark system (same behaviour as other landmarks: map label, discovery, fast-travel destination if landmarks have one). No new system.

### Part E — Something flips the bike at the Climbing Ridge Cut entrance (Mountain Loop Forward)

- **BUG-008** Climbing Ridge Cut 3.9 m (750.42, 87.12, -119.04), heading 62. "Something around here is flipping me."
- **Likely a 0.69 regression — check first (rule 5).** This is the junction where 0.69 replaced the ribbed earth-bank wedge with a new smooth surface (its BUG-007 at (759.37, 87.75, -113.37)) and built flush shoulders; 0.69 also listed "7 single stations where the new shoulder overlaps a pavement edge by 2–11 cm" and "a faint stepped rim on the far side of the BUG-007 patch". Compare contacts here against 0.68.
- Reproduce with motorcycle (and ATV) entering the shortcut at several speeds and lines, find the surface that produces the impulse (lip, overlapping collider, hidden face under the pavement, sign/post collider), and remove that cause locally. If it cannot be reproduced in a bounded investigation, change nothing and say so. The Climbing Ridge Cut jump system is protected.

### Part F — Smoothing

- **BUG-003** Street Loop Forward, Race (226.34, 9.71, 544.45) main s 4574, heading 138. "Can we smooth this out." Beside the road the ground breaks into hard creases and a stepped, sawtooth-walled trench running through the trees. Blend it into a smooth natural slope/gully. Road surface unchanged; trees stay grounded.
- **BUG-004** Street Loop Forward, Race (264.50, 8.75, 562.17) main s 4614, heading 284. "Smooth here as well." The right-hand verge of the highway is lumpy and uneven where it meets the asphalt. Smooth the verge and make it meet the road cleanly. Highway surface, lane markings and buildings unchanged.
- **BUG-009** Mountain Loop Forward (1015.00, 139.88, -54.59) main s 1407, heading 275. "Smooth this out." Lumpy faceted green sheets on the right overlap the pavement edge, with a stepped patch on the left, at the Climbing Ridge Cut rejoin beside the 0.69 berm (s 1390–1434). Replace with smooth shoulders flush with the road; the berm keeps doing its job.
- Shared-world note: BUG-003/004 geometry is in the Street Loop world. Check whether the same meshes appear in other scenes and keep them consistent.

### Verification for this round (targeted, rule 11)

- Before/after view at each of the 9 coordinates, with a PASS/explained disposition per bug in `Docs/Report070/VALIDATION.md`.
- Part A: quit from every quit control with controller, keyboard and mouse: Cancel returns, Quit exits, default focus is Cancel.
- Part B: ride the straight path between the BUG-002 and BUG-001 positions in Free Roam, both directions; then Mountain Loop Forward and Reverse climbs in Race are identical to 0.69 (motorcycle + ATV).
- Part C as described. 5A.6 neighbour checks for every geometry change, covering at minimum Summit Traverse entry/rejoin, South Face Summit jump, Climbing and Downhill Ridge Cut jumps, and the 0.68/0.69 berms. Results must match 0.69 except where this round intentionally changes them.

### Outstanding after this round (as of 2026-10-02)

- Awaiting Dan: gameplay review of 0.70; recheck of the Reverse s 1583 bump.
- **Needs Dan's decision (Part B):** what to do about the Summit Homeward Flight catch landing blocking the Free Roam path (notch / lower its end / remove from Free Roam). Nothing was changed.
- From 0.70 results, for Dan's review:
  - Climbing Ridge Cut entries at 72 mph still roll;
  - the Reverse crest full-throttle line still launches (the outcrop now holds it);
  - a pre-existing 3–5 m dip in the crest gore beside the sign;
  - the Downhill Ridge Cut full-throttle moto line rolled in 0.70's runs.
- From earlier results, not raised by Dan: the full-throttle line brushes the Downhill Ridge Cut berm; airborne riders at ~38 m/s can still clear the Homeward berm; High Ridge Drop barrier is a suggestion only; remaining edge stations in `Docs/Report069/partA-remaining.txt`; production AI stops at the Homeward deck and undershoots the South Face deck (both pre-existing).
- Open: CR-118 intermittent spoken-title clipping.
- Possibly stale, needs Dan's yes/no: CR-010 slightly tighter steering.
- Deferred: physical Steam Deck / controller / save-migration checks; friend test of the packaged build on another PC.
- Backlog (not authorized): graphics upgrade; see FUTURE EXPANSION.

## Previous delivery — 8-report follow-up: road edges, barriers, signs, grass on road — 0.69.0-review1 — DELIVERED, REVIEWED BY DAN (follow-ups in 0.70)

- **DELIVERED:**
  - Source `be4f86e56849fa24fd13659b7e41e6b644ffd329` pushed and verified on origin/main.
  - Fresh 0.69.0-review1 Windows build: 0 errors, 11 warnings, 3m28s.
  - Published [game-69000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-69000). The known draft-lookup miss was resolved with `--resume-draft`.
  - All 233 Latest files match the public signed manifest; public download, signature, install and startup pass.
  - [Delivery evidence](Docs/Report069/PUBLICATION.md).
- **Play-Racer.cmd (unchanged):** launched a responsive managed 69000, muted, with settings restored byte-for-byte; no pending updates. Latest root, current 69000 and previous 68000 retained.
- **Cleanup:**
  - Builds 9,843,285,097 → 7,784,122,345 bytes.
  - C: free 314,369,523,712 bytes.
  - All debug report history preserved.
- **SESSION HANDOFF: STOP.** Wait for Dan's review of 0.69. A documentation-only delivery commit follows; playable source remains `be4f86e5`.

### Results (2026-10-02, Claude Code)

- **Safety checkpoint:** `20277246` (this TODO plan), pushed before any change. Version 0.69.0-review1 / build 69000.
  - Evidence: [Docs/Report069/VALIDATION.md](Docs/Report069/VALIDATION.md) and [BARRIERS.md](Docs/Report069/BARRIERS.md).
  - Tools in `Tools/Report069/`; play-mode harness `Assets/Scripts/Report069Checks.cs`.
- **Part A — BUG-004 and all Mountain edges: PASS (with listed remainders).**
  - **Cause (not the voxel terrain):**
    - The Reverse pavement (`reverse-branch-join.asset`) is a world-axis 0.5 m grid, so diagonal edges stair-step. The green notches are the 0.67 shoulder showing through.
    - Separately, the 0.67 edge shoulders start 0.12 m under the edge and fall at 37° (median −0.47 m at 0.3 m out), so most edges were a step.
  - **A1 (source fix):**
    - Pavement boundary vertices on road sides move onto a local least-squares edge line: Reverse 438 vertices (≤ 0.40 m), Forward 8.
    - No triangle flips. A "do no harm" pass undoes any move that makes the edge less straight.
    - Centre line, width, grade and banking are unchanged.
  - **A2 (flush shoulders):** collidable earth shoulder tucked under the edge, flush, 1 m verge at −4%, then 1:2 (1:1.33, 1:0.7 lower) to the ground, or rising 1:2 to a bank.
    - Terrain above the verge is lowered under it, never toward a road below.
    - Tree-aware: no trunk buried more than 1 m. Posts/signs in the footprint were raised.
    - Built: Forward 1,909 stations, Reverse 4,332.
  - **Open 0.5 m edge stations, before → after:**

    | Scene | Steps | Sawtooth |
    |---|---|---|
    | Forward | 1,345 → 228 | 125 → 130 |
    | Reverse | 3,776 → 520 | 283 → 193 |

  - **What remains** is junction mouths, natural rock barriers, steep walls, tree stations and stretches where straightening was undone. It is listed with coordinates in `Docs/Report069/partA-remaining.txt`.
  - **Protected, unchanged:** whole flight systems from the approach, activity jumps, jump-exclusion zones, multi-level and covered roads, 0.68 berms, junctions.
  - **Rides:** BUG-004 rides off and straight back on (moto/ATV). So do the other Reverse samples except s 1665 (natural rock barrier) and Summit Traverse s 452.5 (protected Fern Creek zone, unchanged).
  - **Resets** beside corrected edges restore in 0.82 s at the nearest point.
  - **Discovery:** a first build moved 3 vertices on the South Face run-up. AI takeoffs dropped and stopped at s 1092. Fixed by protecting whole flight systems from their approach, then rebuilt from scratch.
- **Part B — PASS, with limitations.**
  - Earth berms (0.68 design, 1.8 m, 85° face):
    - BUG-005: Reverse main s 446–486, right; Summit Traverse entrance open.
    - BUG-008: Forward main s 1390–1434, right, opposite the Climbing Ridge Cut rejoin.
  - **BUG-008:** shortcut overshoots that fell ~40 m are now kept at the rejoin.
  - **BUG-005:** 3 of 4 overshoots that fell 24–43 m now stop within 2–7 m. One ATV at 30 m/s rides off the berm's far end (15.6 m).
  - **Clean lines:** complete. The full-throttle Reverse crest line brushes the BUG-005 berm, and no longer rolls over.
  - **Other direction:** not needed (Forward climbs into that bend; the Reverse road is straight at the BUG-008 place).
- **Part C:**
  - **BUG-001 — regression from 0.67:** `PalePatch` recoloured 276 vertices of `StreetLoopGreybox-CR129-junction-Ground_480_640.asset`, which carries the Street Loop asphalt as vertex colour.
    - The asset was restored to 0.66 (`eec4e911`). The cairn stays grounded.
    - The "pale rectangle" is flat street-level grass (lighting), not a pad; left.
  - **BUG-006:** the torn gore at the deck end was replaced by a smooth collidable surface, flush with the pavement.
  - **BUG-007:** the ribbed earth-bank wedge at the Climbing Ridge Cut junction was replaced by one smooth surface, flush with both trails. Production AI now enters and rejoins the Climbing Ridge Cut (it reset before entering in 0.68).
- **Part D:**
  - **BUG-002:** both signs and their posts removed in all 8 course scenes. The Fence Line Smash activity is untouched.
  - **BUG-003:** LAKE SHORE board raised 3.0 m in all 8 scenes. It clears the hillside under its whole width; the post is visible and grounded.
- **5A.6 neighbour drives** (same sets as 0.68) match 0.68: South Face (AI stop 1183, throttle 1144), lower main route, Summit Traverse both scenes, Downhill / Climbing Ridge Cut, Homeward landing / runout, the three 0.68 berms.
- **Remaining for Dan's review:**
  - the BUG-005 ATV overshoot past the berm end, and the crest line brushing that berm;
  - a faint stepped rim on the far side of the BUG-007 patch;
  - 7 single stations where the new shoulder overlaps a pavement edge by 2–11 cm.

- **Authorized by Dan** from debug session `2026-10-02_17-29-13-319_325918` (CLOSED, exported as `..._325918_47fd0b7a.zip`). All 8 reports were captured on 0.68.0-review1 (build `fd4431e2`), so all 8 count. Folder with full-size screenshots: `C:\Users\danmo\AppData\LocalLow\DefaultCompany\Racer\DebugReports\2026-10-02_17-29-13-319_325918`. READ every comment and LOOK at every screenshot before changing anything. If the folder is missing, ask Dan for the ZIP.
- **Starting point:** main `10538c77` (documentation commit; playable source `52393f8b`, 0.68.0-review1 / game-68000). This TODO edit is uncommitted and belongs in the safety checkpoint.
- **Scope is exactly Parts A–D below.** Section 5A applies to all geometry: driving lines, jumps, flight corridors, tunnels and lower routes are protected. The 0.68 reset rule, failsafe, seam-cover colliders, landing runout, berms and arrow cleanup are accepted work; do not undo them.
- **0.68 is CLOSED / ACCEPTED by Dan (2026-10-02).** The reset rule works as described; Dan will keep using it and will raise a flag himself if he wants it changed. Do not reopen or retune any 0.68 item.

### Part A — Road edges: remove the sawtooth and make edges drivable (Mountain Loop, both directions)

- **BUG-004** Mountain Loop Reverse (827.31, 124.59, 108.94) main s 254, heading 120. Dan: "can we get rid of this sawtooth stuff and smooth out the roads? If I fall off the track I should be able to just get right back on." The screenshot shows the jagged zigzag outline where pavement meets terrain. 0.67 recorded "jagged pavement outlines remain" as a known limitation; Dan now wants it gone.
- **Outcome required, all Mountain routes (main and branches), Forward and Reverse scenes:**
  1. The visible boundary between pavement and terrain is a clean, smooth line that follows the road. No sawtooth/stair-step outline, no slivers, no see-through seams.
  2. Wherever terrain adjoins the road, the shoulder meets the pavement flush: no lip, step, trench or gap that stops a motorcycle or ATV from riding off the road and straight back on. Collision matches what is visible (rule 4).
  3. Road centre line, width, grade and banking are unchanged. This is an edge and shoulder correction, not a road rebuild.
- **Not edges to "fix":** jump lips and landings, flight gaps, tunnel mouths, bridges/elevated ribbons over a lower route, and the 0.68 berms. Leave those as they are.
- Find the cause first (the 0.64 voxel terrain is clipped against pavement in grid steps) and fix it at the source for the affected meshes if that is the smallest reliable change; otherwise correct edges locally. Do NOT regenerate the mountain terrain broadly (5A.3).
- Record before/after counts of sawtooth/step edge stations per scene in `Docs/Report069/VALIDATION.md`. If some stretch cannot be made flush without touching a protected feature, list it with coordinates and leave it.

### Part B — Barriers at two crest-then-bend places (Mountain Loop)

Dan asked in 0.68 for barriers after jumps that lead straight into a turn. These two are the same problem at a hill crest instead of a ramp.

- **BUG-005** Mountain Loop Reverse (995.76, 163.84, 121.08) main s 450, heading 98. "This place in particular needs a barrier. When you come off the hill it is too easy to go flying off the edge here." The exposed edge is ahead/left of the "OPTIONAL SHORTCUT / SUMMIT TRAVERSE" sign. **The Summit Traverse shortcut entrance (gold arrow, right) must stay open and unobstructed.**
- **BUG-008** Mountain Loop Forward (1021.31, 139.27, -66.87) main s 1403, heading 88. "Another place that needs a barrier, you come over the hill and there is no straight road so you go flying off the track." The road bends away just past the crest, with a drop on the outside.
- Build as in 0.68 Part E: natural berm/rock on the OUTSIDE of the bend, grounded, with colliders, shaped to deflect along the road, high enough for a motorcycle and ATV arriving at full speed over the crest. Nothing in the driving width, flight corridors or shortcut entrances. Check whether the same location exists in the other direction's scene and needs the same barrier.
- Add both to `Docs/Report068/BARRIERS.md` (or a 069 copy). Verify each with one full-throttle motorcycle and one ATV pass: clean line unobstructed, overshoot kept on track.

### Part C — Terrain lying on the road

- **BUG-001** Street Loop Forward, Free Roam (323.64, 8.73, 542.52) main s 8, heading 228. "What happened here? Why is the grass on the road now? Need to remove this." Grass-coloured terrain now covers part of the pavement near the start. **Regression — check Git first (rule 5).** This is the spot where 0.67 grounded the cairn (its BUG-001) and where 0.68 "seated the start on its support"; compare the ground/road meshes and materials here against 0.66 (`eec4e911`) and restore the road surface. The cairn stays grounded. The pale flat rectangle beside the cairn is still visible; blend or remove it if it is a leftover pad. Check the same location in the other Street Loop / shared-world scenes.
- **BUG-006** Mountain Loop Reverse (730.36, 99.60, -294.35) main s 2740, heading 318. "Fix this." (Reading confirmed by Dan.) A lump of green terrain overlaps the right side of the pavement, with a torn/see-through patch in it. Remove the terrain from the driving surface and close the tear with a clean, collidable shoulder (Part A outcome).
- **BUG-007** Mountain Loop Forward, Climbing Ridge Cut 14 m (759.37, 87.75, -113.37), heading 163. "Smooth out the grass." Lumpy grass sheets overlap the trail ahead and expose a ribbed, see-through underside at their edge. Replace with one smooth connected surface meeting the trail flush. The Climbing Ridge Cut jump system is protected.

### Part D — Signs (Street Loop Forward, Free Roam; shared world)

- **BUG-002** (471.15, 85.67, -20.84) main s 640, heading 220. "I think just remove these signs." Remove BOTH: "FENCE LINE SMASH / 3 / 6 / 10 PROPS IN 8 s" and "ANDERSON'S / LAKE / MOUNTAIN TRAILS", with their posts. Remove them in every scene where they appear. Signs only: the Fence Line Smash activity, its scoring and the fences stay.
- **BUG-003** (538.99, 81.03, -83.76) main s 730, heading 164. "Raise this sign out of the dirt." The "LAKE … / BOTH TRAILS …" sign is half buried in the hillside. Reseat it on the ground with its post visible and the whole face readable from the trail; apply to every scene where it appears.

### Verification for this round (targeted, rule 11)

- Before/after view at each of the 8 coordinates, with a PASS/explained disposition per bug in `Docs/Report069/VALIDATION.md`.
- Part A: ride off the road and back on (motorcycle and ATV) at BUG-004 and at four other sample points per scene chosen from the worst stations before the fix; edge-step probe counts before/after.
- 5A.6 neighbour checks for every geometry change, covering at minimum the lower main route tunnel, South Face Summit jump, Summit Traverse entry/rejoin, Climbing and Downhill Ridge Cut jumps, Homeward landing runout and the three 0.68 berms. Results must match 0.68.
- One reset from off-track beside a corrected edge in each Mountain scene, to confirm the 0.68 reset rule still places at the nearest point.

### Outstanding after this round (as of 2026-10-02)

- Awaiting Dan: gameplay review of 0.69 (edge feel, the two new berms, BUG-006/007 surfaces); recheck of the Reverse s 1583 bump.
- From 0.69 results, not raised by Dan: BUG-005 ATV overshoot past the berm end; remaining junction/rock-barrier edge stations (partA-remaining.txt).
- From the 0.68 results, not yet raised by Dan: the full-throttle line brushes the Downhill Ridge Cut berm; airborne riders at ~38 m/s can still clear the Homeward berm; High Ridge Drop barrier is a suggestion only; production AI stops at the Homeward deck (s 2316) and undershoots the South Face deck (both pre-existing).
- Open: CR-118 intermittent spoken-title clipping.
- Possibly stale, needs Dan's yes/no: CR-010 slightly tighter steering.
- Deferred: physical Steam Deck / controller / save-migration checks; friend test of the packaged build on another PC.
- Backlog (not authorized): graphics upgrade; see FUTURE EXPANSION.

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

- 2026-10-03 (third move): the 0.67 and 0.68 "Previous delivery" sections were moved verbatim to the end of the archive.
- 2026-10-02 (second move): the 0.62–0.66 "Previous delivery" sections were moved verbatim to the end of the archive.
- Everything formerly below this point (historical delivery records, phases 0-9, the old bug tracker, CR-001 through CR-121, the decision log and old session handoffs) was moved VERBATIM to [PROJECT_TODO_ARCHIVE.md](PROJECT_TODO_ARCHIVE.md) on 2026-10-02 at Dan's request, to keep this file small enough to read in full every round.
- The archive is reference only. Unchecked boxes and "open/pending" labels inside it are superseded by the lists above; the latest explicit decision wins. Search it when a bug or rule needs history (known-good commits, earlier decisions, CR details). Do not delete it and do not append new work to it.
- When this file grows again, move the oldest "Previous delivery" sections to the end of the archive, verbatim.
