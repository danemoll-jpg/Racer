# Woodstock Rush
## Project Management / TODO / Astra-Codex Handoff

## Mandatory standing workflow

- **PROJECT_TODO.md is the source of truth** for project state, bugs, decisions, backlog and current work.
- **[CODEX_RULES.md](CODEX_RULES.md) contains the mandatory standing workflow/release rules.** Every future Codex task MUST read and follow it before making project changes, even when the individual prompt does not repeat those rules.
- A later explicit instruction from Dan may override a standing rule for that specific task.
- Do not silently modify or weaken CODEX_RULES.md. Changes to standing rules require an explicit instruction from Dan.
- Root AGENTS.md points future Codex tasks to both files.
- From 2026-10-02 the coding agent is Claude Code. The same two files govern it; root CLAUDE.md (created in the 0.67 round) is its discovery pointer.

## CURRENT — Lighting and atmosphere pass + 3-report follow-up + Free Roam landing — target 0.71.0-review1 — NOT STARTED

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

- Awaiting Dan: gameplay review of 0.71; recheck of the Reverse s 1583 bump.
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

## Previous delivery — Reset rule rewrite, fall-through safety, Mountain fixes, landing, barriers — 0.68.0-review1 — DELIVERED, ACCEPTED BY DAN 2026-10-02

### Results (2026-10-02, Claude Code)

- **Safety checkpoint:** `d2a196e3` (this TODO plan), pushed before any change. Version 0.68.0-review1 / build 68000. Evidence: [Docs/Report068/VALIDATION.md](Docs/Report068/VALIDATION.md); tools in `Tools/Report068/`; play-mode harness `Assets/Scripts/Report068ResetChecks.cs`.
- **Part A — reset rule (Dan's standing decision) — PASS.**
  - The cause matched the source reading: earned-anchor-only candidates, run-up samples rejected by jump exclusions, and an endless retry with no fallback.
  - Reproduced on 0.67 code:
    - off-track landing after the Homeward run-up → start marker (93–109 m);
    - Reverse South Face undershoot → start line (1,120 m back);
    - BUG-008 overshoots → 15–108 m back;
    - Free Roam start / quit-race → fell through the world.
    - The exact waiting loop was not reproduced by the harness; its cause is shown in the source.
  - New `VehicleRespawn.RecoverNearest`: nearest route point around tracked progress, on the raced route (branch first), facing the race direction.
    - It steps outward 2 m at a time, forward first.
    - It never steps back across a ramp/flight exclusion it reached from outside, and never crosses START/FINISH.
    - It always places, with last resorts of anywhere on the route, then the start position.
    - The delay, validation, wipeout detection and checkpoints are unchanged.
  - AI selection is unchanged, with the same guaranteed placement after 3 s of rejections. Free Roam can no longer hang.
  - Failsafe below -15 in all scenes (immediate).
  - Quit race seats the vehicle on validated ground.
  - The start is seated on its support.
  - All targeted cases restore in 0.80–0.83 s at the nearest usable point (2–44 m, mostly under 30 m), facing forward, with no waiting message. Failsafe 0.02 s.
- **Fall-through holes:**
  - Rule-5 comparison of every 0.67-modified ground mesh against 0.66: 0.67 created no voids.
  - 0.67's visual-only seam covers hid bottomless or deep slots (up to ~1.1 m wide). These are 538 (Forward) and 2,050 (Reverse) triangles, including a cluster beside the Homeward landing where Dan overshot. All now have matching colliders.
  - **BUG-009 root cause:** the Mountain Forward start marker was 1.49 m below the pavement with nothing under it (pre-dates 0.67). Raised; the code also seats any start on its support.
  - The exact BUG-007 pass-through was not reproduced.
- **Part B:**
  - **BUG-003:** a 0.67 shoulder sheet lying on the pavement was tucked under it (69 vertices). The "blank board" is the back of "<<< SHORTCUT 2 / DOWNHILL RIDGE CUT", whose text already faces Reverse traffic (Dan was facing backwards); kept.
  - **BUG-004:** sign removed.
  - **BUG-005:** the 0.3–0.6 m pavement crease is now a smooth 20 m vertical curve. A 5 m blend still launched a 35 m/s bike; AI roll went from 81° to 1.4°. The arrow was reseated.
  - **BUG-006:**
    - Collidable shoulder from both pavement edges over the MountainCut cap (s 428–488); 459 shard vertices lowered under it.
    - The "sky slab" is the back/underside of the OPTIONAL SHORTCUT / SUMMIT TRAVERSE sign; kept.
- **Part C — BUG-008:**
  - Measured: only ~33–36 m/s takeoffs land on the 17 m deck. Full-throttle moto/ATV land on the 167 m paved descent, which is already 9+ m wide each side, then meet the 68° left bend too fast.
  - Approach, ramp, lip and deck are unchanged.
  - The landing zone continues through the bend as a runout up to 15 m wide on the outside (s 2474–2545), bounded by the Part E berm.
  - The clean line is clear. At 38 m/s airborne riders can still clear the berm; the new reset restores them at the bend. Dan judges the feel.
- **Part D:** "END RACE / RETURN TO MENU" (confirm: "END RACE AND RETURN TO MENU?"). Text only; navigation unchanged.
- **Part E** ([BARRIERS.md](Docs/Report068/BARRIERS.md)):
  - Earth berms with an 85° rock inner face are built at:
    - the Forward Homeward bend (2.4 m);
    - the Reverse South Face bend;
    - Reverse Downhill Ridge Cut (1.8 m).
  - Clean lines are clear, except that the full-throttle line brushes the narrow Downhill Ridge Cut turn.
  - No-steering overshoots are kept on track at the South Face and Downhill sites, and for the ATV at the Homeward bend.
  - The High Ridge Drop berm (shortcut entrance) was removed after it touched the clean line; it is now a suggestion.
- **Part F** ([ARROWS.md](Docs/Report068/ARROWS.md)):
  - 538 → 302 arrows on all 8 courses: 146 inherited/off-course (72 Street Loop arrows in each Backyard scene), 10 duplicates, 80 repeated on straights.
  - Every turn/fork/shortcut entry keeps guidance. 7 kept arrows were reseated flat.
- **5A.6 neighbour checks** (moto + ATV) match 0.67:
  - lower main route s 1460–1830;
  - South Face jump from s 800 (known AI stop at 1180.9);
  - Summit Traverse, both scenes;
  - start area;
  - Climbing / Downhill Ridge Cut throttle.
  - The production AI stopping at the Homeward deck (s 2316) is confirmed identical on the 0.67 scene (pre-existing).
- **Not changed:** the Reverse s 1583 bump (awaiting Dan); the Mountain Reverse world edge north of z ≈ 380 (outside every route; the failsafe covers it).
- **DELIVERED:**
  - Source `52393f8bbe7684f99060532b4b65108bdf5c62e4` pushed and verified on origin/main.
  - Fresh 0.68.0-review1 Windows build: 0 errors, 20 warnings, 3m37s.
  - Published [game-68000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-68000). The known draft-lookup miss was resolved with `--resume-draft`.
  - All 234 Latest files match the public signed manifest; public download, signature, install and startup pass.
  - [Delivery evidence](Docs/Report068/PUBLICATION.md).
- **Play-Racer.cmd (unchanged):** launched a responsive managed 68000, muted, with settings restored byte-for-byte; no pending updates. Latest root, current 68000 and previous 67000 retained.
- **Cleanup:**
  - Builds 9,733,604,882 → 7,713,741,731 bytes.
  - C: free 299,698,380,800 bytes.
  - All debug report history preserved.
- **SESSION HANDOFF: STOP.** Wait for Dan's review of 0.68. A documentation-only delivery commit follows; playable source remains `52393f8b`.

### Original scope (as planned)


- **Authorized by Dan** from debug session `2026-10-02_13-02-35-687_63de1f` (CLOSED, exported as `..._63de1f_e068b7b8.zip`). Folder with full-size screenshots: `C:\Users\danmo\AppData\LocalLow\DefaultCompany\Racer\DebugReports\2026-10-02_13-02-35-687_63de1f`. READ every comment and LOOK at every screenshot before changing anything. If the folder is missing, ask Dan for the ZIP.
- **Only BUG-003 to BUG-009 count.** BUG-001 and BUG-002 in that session were captured at 13:02–13:03 on the OLD 0.66 build (`34ba59b9`) while 0.67 was still being built; the session was then resumed by 0.67 (build `78170779`). Do not treat 001/002 as 0.67 failures. 001's sign is re-reported as BUG-004. 002 ("bad bump", Reverse main s 1583, 842.3/101.4/-115.1) was not re-reported on 0.67; Dan will recheck it. Do not change it this round.
- **Starting point:** main `3debbe88` (documentation commit; playable source `abb1652d`, 0.67.0-review1 / game-67000). This TODO edit is uncommitted and belongs in the safety checkpoint.
- **Scope is exactly Parts A–F below.** Section 5A applies to all geometry. Local fixes only.

### Part A — Reset rule rewrite + nobody falls through the world (highest priority)

**STANDING DESIGN DECISION — Dan, 2026-10-02 (supersedes the "accepted reset/recovery baseline" and the "earned sample only" behavior from CR-076 for the PLAYER):**

> A reset puts the player on the **closest point of the track to where the vehicle is now, facing the correct race direction**. It must not send the player back over road already cleared. The wreck and the existing few-second delay are the whole punishment. A reset must **always** succeed; "Waiting for clear course support" must never be something the player sits and watches. Dan has asked for this several times without success. This is the fix that matters most this round.

- **What happened (BUG-007 + BUG-008, Mountain Loop Forward):** Dan overshot the BUG-008 jump, landed far off the track, and pressed reset. He got "Waiting for clear course support — race clock continues" repeatedly, was never restored, and then fell through a hole, captured at (684.53, **-334.58**, 71.83), main s 2560.
- **Why the current code does this (read from `Assets/Scripts/VehicleRespawn.cs`; confirm before changing):**
  - `TryRecoverLocal` only considers the last *earned* anchor (`safeStation`) and older `history` samples behind it — "never nearest arbitrary ground". The vehicle's current position is used only for diagnostics.
  - The anchor only advances while the vehicle is grounded, upright, inside the road width, facing and moving forward. During a jump `awaitingLanding` freezes it at the pre-ramp sample, and samples in launch/landing zones are rejected by `UnsafeJump`. After a missed jump the only candidates are therefore before the ramp, often far back, or all rejected.
  - When every candidate is rejected it sets `Pending`, shows "Waiting for clear course support" and retries the **same** candidates every 0.5 s forever. There is no fallback and no fall-through protection.
- **Required player behavior:**
  1. **Target = nearest route point to the vehicle's current position**, on the route the player is racing (main, or the branch they are on), searched around the tracked progress (`trackingStation` / `ProjectNear`) so a multi-level mountain does not snap to an unrelated part of the lap above or below. Facing the route's forward direction for the active race direction (BUG-020 rule).
  2. If that exact spot is unusable (unsupported, obstructed, inside a ramp/flight/landing exclusion), step outward along the route in small increments in **both** directions and take the nearest usable station. On a tie prefer forward. Do not jump back to a distant earned sample.
  3. **Missed jumps:** the nearest usable station is normally at or just past the landing. Use it. Do not return the player to before the ramp or earlier. (Dan: "If you need to use it you are already being punished.")
  4. **Guaranteed result:** the search is bounded in time (well under the current visible wait) and must end with a placement. Last resort order: nearest usable station anywhere on the current route → start position. The waiting message must not persist or repeat.
  5. Checkpoints/penalties are NOT changed: if the nearest point is past a missed gate, the existing missed-checkpoint penalty rules apply as they do today. Reset must not itself award laps.
  6. Keep the existing delay, the support/clearance validation of the chosen spot, and traffic/other-racer occupancy checks.
- **AI and Free Roam:** leave AI recovery selection as it is, but give it the same "never wait forever" fallback. Free Roam reset keeps its current nearest-road behavior; make sure it also cannot hang.
- **Fall-through protection (BUG-007, BUG-009):**
  - **BUG-009** Mountain Loop Forward (728.22, 80.69, -10.35), main s 2635: on stopping/quitting the race here Dan fell through the world. Leaving a race (quit / end race / handoff to Free Roam) must place the vehicle on the nearest validated solid ground with full clearance.
  - **Failsafe, all scenes:** any vehicle that drops well below the world is recovered immediately through the rule above. Nobody falls indefinitely.
  - **Find and close the real hole(s).** Likely a 0.67 regression — check first (rule 5): 0.67 added "collider-free seam covers" (Forward 170 runs, Reverse 467 runs) and lowered terrain under pavement. A surface that looks solid but has no collider breaks rule 4. Inspect the ground around where Dan left the track after the BUG-008 jump and above x≈684, z≈72, and s 2560–2640 in the Forward scene. Then audit every 0.67 seam cover in both Mountain scenes: anywhere a vehicle can reach must have solid collision under it or be physically unreachable. Fix locally; list what was fixed.
- This is an explicitly authorized change to the recovery system (rule 6 exception). Change target selection and the fallback only; do not retune physics, wipeout detection, the delay, or checkpoint logic. All tracks benefit, but verify per the list below, not with a full matrix.

### Part B — Mountain Loop Reverse geometry and signs

- **BUG-003** (814.16, 95.49, -171.39) main s 1507, heading 176 — "fix this". Visible defects: green terrain sheets/wedges lying across the pavement at right and ahead-right with a torn terrain edge beside the gold-arrow branch entry, and a blank dark sign board (no readable face from the route) left of centre near the gate. Clean the terrain off the driving surface and close the torn edge. The blank board: turn its text toward approaching Reverse traffic if it carries useful information, otherwise remove it (standing sign principle). *Dan's comment was not specific; if he corrects this reading, his correction wins.*
- **BUG-004** (810.26, 95.43, -159.19) main s 1523, heading 347 — REMOVE the "OLD CUT CLOSED / MAIN ROUTE >>>" sign and post.
- **BUG-005** (744.70, 85.84, -113.38) main s 2966, heading 148 — "fix". A raised lip/step runs diagonally across the pavement and a teal arrow is cut by it (part of the arrow appears as a detached square beyond the lip). Remove the step so the pavement is one smooth surface and reseat the arrow flat on it.
- **BUG-006** (1005.11, 162.65, 121.37) main s 457, heading 7 — "eliminate this gap... at least cover it up (might have to be race only change)". The pavement sits above the terrain with a visible dark gap under its left edge, sharp terrain shards poke up around both edges, and a very large dark flat slab hangs in the sky ahead. Close the gap with collidable, grounded support (see Part A: no visual-only covers where a vehicle can reach), remove the shards, and identify the slab: if it is stray geometry, remove it. If any of this is part of a protected jump/flight system or a Free Roam feature, cover it for Race only, as Dan allows.

### Part C — Mountain Loop Forward landing

- **BUG-008** (802.79, 177.67, 90.57) main s 2379, captured airborne at 65.9 mph, heading 250. Dan: the landing area should be larger, or the jump scaled down a little; he almost always carries too much momentum to hit the landing, and he **prefers to keep the jump**.
- Leave the approach, ramp and lip unchanged. Enlarge the landing zone (longer and, if needed, wider) so a full-throttle motorcycle and ATV land on it, with grounded support and matching colliders, and a clean continuation to the route. Only if a larger landing cannot fit without breaking neighboring protected routes, propose the smallest ramp reduction instead and say why.
- This is a Dan-requested change to a protected jump, limited to its landing/recovery. One implementation, verified at full throttle and at a moderate speed; then stop (rule 12). Dan judges the feel.

### Part D — Pause menu wording

- **BUG-009 (second half):** in the in-race menu it is not obvious which option leaves the race. Label that option clearly, e.g. **"End Race / Return to Menu"**. Text and, if needed, button width only; no menu redesign. Controller and keyboard navigation unchanged.

### Part E — Guardrails / natural barriers after jumps (Mountain Loop Forward and Reverse only)

- **Dan's request:** add guardrails or natural formations at the more perilous places to keep riders from flying off the track, **particularly where a jump is followed immediately by a turn**. Mountain tracks only.
- First list every Mountain jump (main and branches, both directions) whose landing is followed by a turn or a drop-off within the distance a full-speed landing needs to settle. Put the list with coordinates in `Docs/Report068/BARRIERS.md`.
- At those places add a barrier on the OUTSIDE of the post-landing turn / exposed edge. Prefer natural formations that match the mountain (rock outcrops, boulder lines, earth berms); a simple guardrail is fine where rock would look wrong. Grounded, with matching colliders, tall enough to stop a motorcycle and an ATV, shaped to deflect along the road instead of stopping a vehicle dead or launching it.
- Barriers must not intrude into the driving width, the jump's flight corridor, any shortcut entrance/exit, lower routes or tunnels (5A.4/5A.5). Do not fence the whole mountain: only the listed post-jump places. Other perilous spots you notice go in the list as SUGGESTIONS for Dan, not built.
- Verify each new barrier with one full-throttle motorcycle and one ATV pass: the clean line is unobstructed, and an overshoot is kept on the track.

### Part F — Remove redundant ground arrows (all courses)

- **Dan, 2026-10-02:** the game "goes a little crazy with the arrows"; redundant arrows may be removed. If a removal bothers him he will ask for it back.
- Remove:
  - arrows inherited from another course that do not belong to the active route, starting with the ~75 inherited Street Loop arrows in Backyard Forward listed in the 0.67 author notes;
  - arrows that duplicate another arrow or a Route Atlas arrow for the same instruction within a short distance;
  - arrows that point the wrong way for the active direction;
  - strings of repeated arrows on stretches with no turn, fork or other decision (keep at most an occasional reassurance arrow on long unclear stretches).
- Keep: arrows before turns, forks and confusing junctions; shortcut entry and rejoin arrows (gold) and the main-route arrow that distinguishes them; arrows Dan asked for in earlier rounds (e.g. the 0.67 BUG-020 straight arrow).
- Arrows only. No signs, gates, minimap, route lines or wrong-way guidance changes. Any arrow kept must sit flat on the pavement (0.67 BUG-006 rule).
- Record per course and direction: count before, count after, and the removed arrows with coordinates in `Docs/Report068/ARROWS.md`, so any single removal can be reversed. Verification: one pass per changed course/direction confirming every turn, fork and shortcut entry still has guidance. No lap matrix.

### Verification for this round (targeted, rule 11)

- Part A reset rule: first reproduce on 0.67 (overshoot the BUG-008 jump, land off-track, reset → waiting message). Then show, with before/after station and distance for each case: overshoot of the BUG-008 jump; a missed Reverse jump; off the side of an elevated Mountain road; upside-down on the road; off-track on one non-Mountain course (Backyard ramp at 0.67 BUG-020). Each must restore within the normal delay, at the nearest usable track point, facing forward, with no waiting loop and no long setback.
- Part A fall-through: reproduce both on 0.67, then show them fixed; failsafe test by placing a vehicle below the world in one race and one Free Roam scene; quit-race placement at BUG-009's location and one elevated-road location.
- Before/after view at each of the seven coordinates with a PASS/explained disposition in `Docs/Report068/VALIDATION.md`.
- 5A.6 neighbor checks around every geometry change, including the 0.66/0.67 protected features that are nearby (lower main route tunnel, South Face Summit jump, Summit Traverse, Ridge Cut jumps). Motorcycle + ATV through each changed corridor; no broad matrix.

### Outstanding after this round (as of 2026-10-02)

- Awaiting Dan: gameplay review of 0.68 (the reset feel, BUG-008 landing/runout and the three berms); recheck of the Reverse s 1583 bump on the current build.
- From 0.68 testing, not built: barrier suggestions in BARRIERS.md (High Ridge Drop entrance, Eastbound Gully bend, Reverse High Ridge Drop); Homeward airborne overshoots of 38 m/s or more clear the bend berm; production AI stops at the Homeward deck (pre-existing, same class as South Face).
- From the 0.67 results, not yet raised by Dan: 74 Reverse intrusion samples; jagged pavement outlines.
- Open: CR-118 intermittent spoken-title clipping. Known limitation: production AI undershoots the South Face receiving deck (since 0.63).
- Possibly stale, needs Dan's yes/no: CR-010 slightly tighter steering.
- Deferred: physical Steam Deck / controller / save-migration checks; friend test of the packaged build on another PC.
- Backlog (not authorized): graphics upgrade; see FUTURE EXPANSION.

## Previous delivery — 20-report cleanup + Debug session persistence — 0.67.0-review1 — DELIVERED

### Results (2026-10-02, Claude Code)

- **Safety checkpoint:** `8d93f31a` (TODO plan + PROJECT_TODO_ARCHIVE.md committed and pushed before any change). Version 0.67.0-review1 / build 67000.
- **Part A — PASS.** The cause matched the source reading.
  - New `DebugReportSession.ResumeLatest`, read-only, resumes the newest session folder if it is OPEN, has reports and its JSON is readable with a matching ID. Otherwise the next F4 starts a new session.
  - `DeveloperLocationHud` resolves this once per launch.
  - 20/20 focused checks PASS (`DebugSessionPersistenceChecks`): relaunch → BUG-003 in the same folder, scene reload, export → NEW, confirmed new session → NEW, corrupt json → NEW with the folder byte-identical, older orphan not resumed, history byte-identical.
  - DEBUG_MODE.md updated.
  - Dan's `..._547472` was closed via `DebugReportSession.Close()` (close without export; 20 reports + 20 screenshots kept).
  - **Discovery:** a newer OPEN session `2026-10-02_13-02-35-687_63de1f` (2 reports, created by installed 0.66 at 13:02 during this round) exists; it was left untouched, and 0.67 will resume it.
- **Part B:** all 20 handled; dispositions in [Docs/Report067/VALIDATION.md](Docs/Report067/VALIDATION.md).
  - **Shared cause behind BUG-002 and the s 1576 launch:** the vehicle body dips below the pavement and struck 0.64 voxel terrain lying 0.5–1.2 m under it. Fixed locally by lowering terrain vertices under pavement to 1.2 m below it (barriers excluded). The arrow had no collider.
  - **BUG-006:** legacy CR120/121 arrows were 1–3.7 m above the regraded pavement. They were hidden where they duplicate Route Atlas arrows, and draped elsewhere.
  - **BUG-007/008:** collider-free seam cover for see-through edge gaps; sloped shoulders where a jagged edge steps straight down; pavement shelf at s 1614–1617 regraded (≤0.16 m, top layer only).
  - **BUG-009/010:** connected South Face hillside with a tunnel around the protected lower-route clearance.
  - **Signs and objects:**
    - BUG-004, BUG-005 and BUG-013 signs removed.
    - BUG-003 and BUG-014 signs reseated.
    - BUG-011 spheres became faceted rocks with colliders.
    - BUG-001 cairn grounded in all 8 scenes.
    - BUG-019 inherited Street Loop arrow removed.
    - BUG-020 route polyline aligned to the run-up, fixing both the arrow and the reset heading.
  - **Partial / explained:**
    - BUG-001 faint flat-area shading;
    - BUG-016 grey slab is pavement material;
    - BUG-018 pale band is the trail's own driving-surface material (553 buried duplicate triangles removed);
    - jagged pavement outlines remain (gaps behind them closed).
- **Part C:** flagged edge stations, Forward 207→92 and Reverse 408→143.
  - The remainder are protected jump/flight systems, multi-level routes, tunnel faces, or steep but continuous embankments under raised berms.
  - Shoulders: Forward 18 runs / 604 stations; Reverse 32 runs / 1,238 stations. Seam covers: Forward 170 runs; Reverse 467 runs. Coordinates are in `Docs/Report067/partC-*.txt`.
- **5A.6 checks (moto + ATV, muted):**
  - Reverse s 1540–1830 went from 110° rolls / minUp −0.34 to 0° / 0.93.
  - Unchanged from 0.66: South Face jump, Summit Traverse (both directions), Downhill and Climbing Ridge Cut jumps, and the known AI South Face undershoot at 1180.9.
  - Backyard ramp: AI moto recovery removed. The ATV lands harder on the corrected straight line (roll 23–37°), but completes with no reset.
- **Tools:** kept in `Tools/Report067/` (authoring, probes, drive harness, release pipeline). Root `CLAUDE.md` created.
- **Remaining limitations:**
  - The items above;
  - 74 Reverse intrusion samples (barrier rocks and edge slivers);
  - ~75 other inherited Street Loop arrows in Backyard Forward (listed in author-notes for Dan);
  - production AI South Face undershoot (since 0.63).

- **DELIVERED:**
  - Source `abb1652d5cf9a5a731d4eff751bf703bd7893822` pushed and verified on origin/main.
  - Fresh 0.67.0-review1 Windows build: 0 errors, 20 warnings, 3m52s.
  - Published [game-67000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-67000). The known draft-lookup miss was resolved with `--resume-draft`.
  - All 232 Latest files match the public signed manifest; public download, signature, install and startup pass.
  - [Delivery evidence](Docs/Report067/PUBLICATION.md).
- **Play-Racer.cmd (unchanged):** launched a responsive managed 67000, muted, with settings restored byte-for-byte; no pending updates. Latest root, current 67000 and previous 66000 retained.
- **Cleanup:**
  - Builds 9,712,075,132 → 7,696,949,672 bytes.
  - C: free 301,155,147,776 bytes.
  - All debug report history preserved.
- **SESSION HANDOFF: STOP.** Wait for Dan's review of 0.67. A documentation-only delivery commit follows; playable source remains `abb1652d`.

### Original scope (as planned)

**Tooling change (Dan, 2026-10-02):** development moves from Codex to Claude Code. CODEX_RULES.md keeps its filename and applies unchanged; wherever it says "Codex", read "the coding agent". Planning, bug triage and this TODO section were prepared in Claude chat; implementation is done by Claude Code in this repository.

- **Authorized by Dan** from debug session `2026-10-02_03-04-39-449_547472` (20 reports, build 0.66.0-review1 / `34ba59b9b198412590505861745fcd79`) plus his written report of the Debug session bug. Original folder with full-size screenshots: `C:\Users\danmo\AppData\LocalLow\DefaultCompany\Racer\DebugReports\2026-10-02_03-04-39-449_547472` (BUG_REPORT.md, bugs.json, Screenshots\BUG-001..020.png). READ every comment and LOOK at every screenshot before changing anything. If the folder is missing, ask Dan for the ZIP; do not work from this summary alone.
- **Starting point:** main `c6432b12` (documentation commit; playable source `eec4e911`, 0.66.0-review1 / game-66000). The TODO edits and the new PROJECT_TODO_ARCHIVE.md are uncommitted and belong in the safety checkpoint.
- **Scope is exactly Part A + Part B + Part C below.** No other tracks, physics, AI, recovery system, UI, audio or route redesign. Coordinates are authoritative (rule 5). Section 5A applies to every geometry item: the 0.64 broad terrain regeneration is what caused the 0.65/0.66 regressions, so make LOCAL mesh/terrain corrections only and do not re-run or extend `Tools/Author-SolidReportTerrain.cs` over an area.
- **0.66 BUG-001/002 (lower main route sheets, South Face Summit jump) were verified by Dan on 2026-10-02.** Both are accepted, protected features; do not alter them.

### Part A — Debug session must survive quitting the game

- **Dan's report:** exiting the game ends the current debug session. It must stay open until he presses the export (ZIP) button.
- **Evidence:** BUG-001 screenshot of this session shows `Session: NEW / Reports 0` at first capture after launch, and bugs.json on disk is still `closed:false, exported:false`.
- **Cause found by reading source (confirm before fixing):** `DeveloperLocationHud` holds the session only in `static DebugReportSession session`. Nothing reloads it at startup, so after a relaunch `EnsureSession()` creates a new folder and the previous OPEN session is orphaned on disk. `DebugReportSession` has no constructor that loads an existing folder.
- **Required behavior:** on first use after launch, resume the most recently started session under `persistentDataPath/DebugReports` that is OPEN (`closed:false`) and has at least one report; continue its numbering (next F4 = next BUG number), HUD/menu show its ID, OPEN state and count. A session ends ONLY by successful Export, or by START NEW DEBUG SESSION with its existing confirmation. Scene/track changes, returning to the main menu, quitting and relaunching must not end it. Unreadable/corrupt bugs.json: leave that folder untouched and start a new session. Never delete or rewrite history.
- Older orphaned OPEN sessions (one is created by every earlier quit) are left untouched; only the newest is resumed.
- **Session `..._547472` itself:** Dan has already zipped and delivered it. After its contents are processed, mark it CLOSED through the same code path as "close without export" (files preserved) so the new build does not resume a 20-report session he has already handed over. Report this in the final report.
- Update `Docs/DebugReporting/DEBUG_MODE.md`. Targeted checks only: capture -> quit -> relaunch -> next capture continues same session/number; export -> relaunch -> NEW; new-session-confirm -> relaunch -> NEW; corrupt json -> NEW with old folder untouched.

### Part B — 20 reports (Unity XYZ; s = road progress in metres)

Street Loop — Forward (scene StreetLoopGreybox), Free Roam:
- **BUG-001** (314.69, 9.14, 538.04) heading 236.5 — "put cairn on ground". Stone cairn about 8–10 m ahead of the camera floats above the grass slope, over a paler flat rectangular patch. Ground the cairn (and its collider); remove/blend the patch if it is a leftover pad. It is an acorn-clue cairn (`DiscoveryDetails.cs`), so check whether the same cairn appears in other scenes of the shared world.

Mountain Loop — Reverse (scene MountainLoopReverse), Race:
- **BUG-002** (746.32, 90.97, 64.20) main s 136 — "something causes bikes to flip sometimes (could it be the arrow)". Large teal ground arrow on the climb. DIAGNOSE FIRST: does the arrow (or anything at this station) have a collider or raised geometry, or is there a pavement seam/step? Check Git history for when it changed. Fix the actual cause. If it cannot be reproduced or identified in a bounded investigation, change nothing here and say so (rules 11/12).
- **BUG-003** (921.90, 156.89, 155.56) main s 369 — brown SUMMIT sign beside the road is sunk too low / half hidden. Reseat it at a readable grounded height. (Dan allowed "or only show in Free Roam"; it would still be too low there, so fix the height. Hiding it during races as well is acceptable.)
- **BUG-004** (948.03, 165.14, 147.70) main s 397 — REMOVE the left-hand sign "<<< MAIN ROUTE / LEFT TO FULL RUN-UP". Keep the "OPTIONAL SHORTCUT / SUMMIT TRAVERSE" sign and the gate ahead.
- **BUG-005** (999.75, 161.76, 89.38) main s 953 — REMOVE the "SUMMIT CAMP / Look for the cairns" sign and its post. The large boulder floating above the slope to the right must lie on the ground. Small cairn at the sign base stays, grounded.
- **BUG-006** (815.51, 96.64, -148.82) main s 1535 — teal direction arrows float above the road and cast shadows; Dan thinks they are not always there. Find why they are intermittent and why they are off the surface, then seat every arrow on this stretch (through BUG-007/008, where the same floating arrows are visible) on the final pavement.
- **BUG-007** (825.20, 99.93, -117.54) main s 1567 — sawtooth hole between the left road edge and the terrain, before the "Fern Creek Leap" sign. Close it, and close any other road-edge/terrain gaps of the same kind found by the Part C sweep.
- **BUG-008** (846.63, 102.07, -110.01) main s 1589 — "smooth the bump" in the pavement just ahead. Local grade smoothing only; the Fern Creek Leap jump system is protected (5A.5). Jagged pavement edges and holes on both sides here belong to BUG-007.
- **BUG-009** (985.35, 131.97, -106.06) main s 1734 — a mountainside is missing: the right road edge drops into open void and a terrain slab hangs overhead at upper left.
- **BUG-010** (987.86, 132.40, -92.78) main s 1739, heading 37 — same area: the upper road is a thin unsupported ribbon over open space, with vertical green curtain faces. **BUG-009/010 are inside the 0.66 BUG-001 repair zone (lower main route under the South Face receiving road, s 1713–1766).** Build real, connected mountain support/hillside AROUND the lower-route clearance volume and the South Face flight corridor. Do not re-block the lower route (5A.4). Re-run the 0.66 traversals afterwards.
- **BUG-011** (1046.89, 150.85, 24.40) Summit Traverse 110.65 m, heading 208.9 — pale rounded grey shapes float beside the road edge. Make them read as real rocks/boulders and ground them with matching colliders.
- **BUG-012** (1006.88, 137.03, -63.03) Summit Traverse 218.80 m — floating road. "Fix here and wherever else there are floating roads" — see Part C.
- **BUG-013** (785.35, 96.00, -184.02) Downhill Ridge Cut 34.39 m — REMOVE the "SHORTCUT JUMP / STRAIGHT / CLEAR THE MAIN ROAD" sign and post. Ramp and orange arrows unchanged.
- **BUG-014** (701.91, 82.64, -104.54) Downhill Ridge Cut 150.60 m — "MOUNTAIN TRAILS / Creek ascent / Ridge return" sign sits low on the driving surface. Move it off the pavement to a grounded, readable roadside position, or show it only in Free Roam; simplest wins.

Mountain Loop — Forward (scene MountainLoop), Race:
- **BUG-015** (1371.13, 101.15, -236.74) main s 786 — floating road; gap/translucent support along the left edge.
- **BUG-016** (731.26, 92.21, 32.30) Summit Traverse 294.59 m — floating road; unsupported grey slab along the left edge.
- **BUG-017** (963.81, 154.28, 97.12) Summit Traverse 48.21 m — more floating road; the road ribbon is visible from below/aside with nothing under it.
- **BUG-018** (731.44, 79.66, -26.64) main s 2653, heading 263 — "mesh mess": overlapping terrain sheets, slivers and a z-fighting pale patch where the trails meet near the lake. Replace with one clean connected surface; road lines, gates and the junction layout unchanged.

Dan's Backyard Loop — Forward (scene DansBackyardForward), Race:
- **BUG-019** (469.23, 80.94, 3.13) main s 0.87 — REMOVE the teal arrow on the road here; it points the wrong way.
- **BUG-020** (68.18, 45.08, 61.54) main s 737.90, heading 75 — the arrow here should point straight along the trail. Also, resetting here leaves Dan facing the same skewed direction as the arrow at the edge of the ramp; per CR-076 a reset must face the correct local route direction. Find the shared cause (likely the tangent/heading both are derived from at this station) and fix it locally. Do not retune the recovery system globally. The ramp is a protected jump.

### Part C — bounded sweep for the two "everywhere else" requests

- Dan asked for "whatever other gaps you find" (BUG-007) and "wherever else there are floating roads" (BUG-012). Scope: Mountain Loop main route and all Mountain branches, both direction scenes. Use the existing support-probe tooling to list unsupported pavement and road-edge gaps, fix each LOCALLY, and record the list with coordinates. Where Forward and Reverse share geometry, apply each fix to both scenes.
- Multi-level clearance first (5A.4): intentional flight gaps, tunnels, underpasses and lower routes are NOT floating roads. Do not fill them.
- Other tracks are out of scope for the sweep.

### Verification for this round (targeted, rule 11)

- Before/after view at each of the 20 coordinates, with a PASS/explained disposition per bug in `Docs/Report067/VALIDATION.md`.
- 5A.6 neighbor check for every geometry change, covering at minimum: lower main route s 1600–1820 and the South Face Summit jump (0.66 results must still hold), Fern Creek Leap, Summit Traverse entry/rejoin, Downhill Ridge Cut jump, Backyard ramp at BUG-020. Motorcycle + ATV through each changed corridor; no broad matrix.
- Part A checks as listed above.

### Also in this round (housekeeping)

- Create root `CLAUDE.md` with the same content/purpose as `AGENTS.md` (read CODEX_RULES.md, then PROJECT_TODO.md, before any change). Leave AGENTS.md and CODEX_RULES.md as they are.
- At completion, update this section with the actual results per rule 13, then build, publish and verify per rules 14–26.

### Outstanding after this round (as of 2026-10-02)

- Awaiting Dan: gameplay review of this round (0.67.0-review1). Dan has an open 0.66 debug session `2026-10-02_13-02-35-687_63de1f` (2 reports) that 0.67 will resume.
- Open: CR-118 intermittent spoken-title clipping. Known limitation: production AI undershoots the South Face receiving deck at ~35.8 m/s (unchanged since 0.63).
- Closed by Dan on 2026-10-02: 0.66 BUG-001/002 verified; CR-087 (Trickum ramp) complete; separate lake/woodland circuit (CR-040) complete.
- Possibly stale, needs Dan's yes/no: CR-010 slightly tighter steering ("non-blocking follow-up" in the archive).
- Deferred: physical Steam Deck / controller / save-migration checks; friend test of the packaged build on another PC.
- Future expansion and Someday lists below are unchanged.

## Previous delivery — Project cleanup and Mountain polish — 0.61.0-review1

The following records the previous 0.61 delivery. Its polish acceptance is superseded by the urgent regression correction above; older completed backlog decisions remain closed.

### ACTIVE / KNOWN

- CR-118: intermittent spoken-title clipping remains open; no later definitive human resolution. No audio retuning in this pass.
- This pass is implemented and awaits Dan's gameplay review: local Forest Forward tree grounding; Free Roam startup and shaped Start/Menu hint; ineffective Race Complete root Back prompt removed; Mountain support/prop grounding and Reverse Summit Traverse forward-facing merge.
- Targeted checks: Forest/cave preservation, controller/keyboard UI flow, representative Forward drive, 1,215 support probes, and final motorcycle/ATV production-driver traversals continuing beyond the Reverse merge. Both final traversals complete with zero resets/recoveries. Early fixture failures and local seam correction are documented in [Docs/MountainPolish/VALIDATION.md](Docs/MountainPolish/VALIDATION.md). No global physics/AI/recovery retuning.
- Safety checkpoint: clean `b29330bc4ceecf8eda2dba77697583f538db002e`. Completion source `56e63e4797d912f5e5822dee9f39eb3afc47919a` pushed/verified on origin/main. Fresh Windows build: zero errors, 21 warnings, 3m33s. Published [game-61000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-61000); all 233 Latest files match the public signed manifest. Public install/startup and unchanged Play-Racer.cmd launch of managed 61000 pass; settings restored and soundtrack preserved. [Delivery evidence](Docs/MountainPolish/PUBLICATION.md).
- Cleanup: Builds **9,565,965,293 → 7,593,020,763 bytes**; **3,578,626,950 bytes** disposable copies removed; C: free **294,350,192,640 bytes**. Current 61000 / previous 60000 retained. **STOP for Dan’s gameplay review.**

### FUTURE EXPANSION

- **Time of day and weather (added by Dan 2026-10-03; planned as the next feature, not authorized yet).** Built as additional presets on the 0.71 look system. **Dan's decisions (2026-10-03):** (1) weather is VISUAL ONLY to start: no grip/handling change, so records and ghosts stay comparable; (2) RACES use one fixed time of day per race (at least at first), no change during a race; (3) FREE ROAM / open world gets a live day-night cycle. Night needs vehicle headlights and readable arrows/gates/signs. Rain: particles, darker sky, fog, wet-look road, rain audio. Snow: particles plus a white ground tint. (4) In RACES, time of day and weather are an OPTION the player picks at race setup. (5) FREE ROAM cycle speed starts at 1 real minute = 1 game hour (a full day in 24 minutes); Dan will adjust after trying it. Make the speed a single easily changed value.
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

- Per Dan, 2026-10-02: 0.68.0-review1 accepted and closed, including the player reset rule (nearest track point, facing forward, always succeeds). Reopen only if Dan raises it.
- Per Dan, 2026-10-02: 0.66 BUG-001 (lower main route) and BUG-002 (South Face Summit jump) verified; CR-087 Trickum ramp complete; separate lake/woodland circuit (CR-040) complete. Do not reopen without Dan's request.
- Dan's Backyard Loop Forward and Reverse accepted. The old back-property dirt trail / gully concept became this course and is complete, not a future track.
- Forest Forward cave accepted for now. No additional cave work; reopen only on Dan's explicit request.
- House 3 / Forest / Laurel AI issue complete, including BUG-009 and associated mapping/stuck-AI work. A route atlas is no longer an active prerequisite.
- General all-track navigation-arrow pass removed from active backlog. This pass addresses only the reported Mountain Reverse Summit Traverse rejoin.
- Ghosts accepted; older human-test-pending wording is superseded.
- Other later accepted/closed reconciliations remain authoritative.

## History archive

- 2026-10-02 (second move): the 0.62–0.66 "Previous delivery" sections were moved verbatim to the end of the archive.
- Everything formerly below this point (historical delivery records, phases 0-9, the old bug tracker, CR-001 through CR-121, the decision log and old session handoffs) was moved VERBATIM to [PROJECT_TODO_ARCHIVE.md](PROJECT_TODO_ARCHIVE.md) on 2026-10-02 at Dan's request, to keep this file small enough to read in full every round.
- The archive is reference only. Unchecked boxes and "open/pending" labels inside it are superseded by the lists above; the latest explicit decision wins. Search it when a bug or rule needs history (known-good commits, earlier decisions, CR details). Do not delete it and do not append new work to it.
- When this file grows again, move the oldest "Previous delivery" sections to the end of the archive, verbatim.
