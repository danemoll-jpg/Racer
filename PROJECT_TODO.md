# Woodstock Rush
## Project Management / TODO / Astra-Codex Handoff

## Mandatory standing workflow

- **PROJECT_TODO.md is the source of truth** for project state, bugs, decisions, backlog and current work.
- **[CODEX_RULES.md](CODEX_RULES.md) contains the mandatory standing workflow/release rules.** Every future Codex task MUST read and follow it before making project changes, even when the individual prompt does not repeat those rules.
- A later explicit instruction from Dan may override a standing rule for that specific task.
- Do not silently modify or weaken CODEX_RULES.md. Changes to standing rules require an explicit instruction from Dan.
- Root AGENTS.md points future Codex tasks to both files.
- From 2026-10-02 the coding agent is Claude Code. The same two files govern it; root CLAUDE.md (created in the 0.67 round) is its discovery pointer.

## CURRENT — Reset rule rewrite, fall-through safety, Mountain fixes, landing, barriers — target 0.68.0-review1 — NOT STARTED

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

- Awaiting Dan: gameplay review of 0.68; recheck of the Reverse s 1583 bump on the current build.
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

## Previous delivery — Urgent Mountain Reverse regression restoration — 0.66.0-review1

- Authorized by Dan's debug session `2026-10-02_00-57-44-616_93acc0_4d42d5ed.zip` (BUG-001/002, metadata and screenshots reviewed). Scope is exactly these two regressions plus the permanent geometry-regression rules. Safety checkpoint: clean main `f588a2d4` (playable `a888e6a5`).
- **Known-good source:** `2b584b45` (0.63.0-review1 / game-63000). **Regressing commit:** `6af2c67d` (0.64): `Tools/Author-SolidReportTerrain.cs` regenerated the whole x 830-1100 Mountain area as voxel "continuous solid" terrain. Its carves protected only main s 798-964, Summit Traverse and the flight corridors. Its own drive spans stopped at s 955 and s 1690, short of both features.
- **BUG-001 (main route blocked, 948,127,-101) — REGRESSION, restored.** Root cause: `continuous solid 2` left two thin sheets across the lower main route at s≈1713 (x≈964) and s≈1766 (x≈1015), at the edges of the South Face flight carve where the route passes under the receiving road. 0.63 was open there. Selective restoration removed only the 394 road-facing sheet triangles inside the lower-route clearance (+0.15..+6 m). Floor, walls, upper receiving support, route line and checkpoints are unchanged. Moto AI, moto full-throttle and ATV AI complete s 1600-1820 with zero resets/recoveries (0.65: all three stopped at s 1710-1713).
- **BUG-002 (South Face Summit jump, 983,176,69) — REGRESSION, restored.** Ramp/lip/landing pavement is identical to 0.63. Root cause: the 0.64 solid was filled to 0.1-0.5 m beneath the ramp with steep lip faces. The vehicle body struck them while still on the pavement (impulses up to 652), causing the uncontrolled pitch/roll. The faces also made the AI brake to ~19 m/s. Selective restoration lowered 884 `continuous solid 0` vertices under the ramp/lip (main s 966-986) to 4 m below the pavement, restoring 0.63's free-standing ramp. Vertices within 12 m of branches were excluded. Moto full throttle at -5/0/+5 m flies with ≤0.9° roll (0.63: ≤0.8°; 0.65: up to 94°), lands on the receiving deck and continues. ATV 0.1° (0.65: roll-over). AI takeoff restored to 35.7/33.9 m/s.
- Neighbor regression check: every main/branch station in both regions compared before/after. The first wider BUG-002 attempt also deepened Summit Traverse entry support, so it was rejected and restricted. Final diff shows only the intended stations changed; Summit Traverse is identical. Retained pre-existing limitation (same in 0.63): at ~35.8 m/s the production AI flight undershoots the receiving deck and fails after the flight. The landing was not altered. [Validation](Docs/RegressionRestore/VALIDATION.md).
- **Permanent safeguards added:** CODEX_RULES.md section 5A, World Geometry Regression Protection (protected playable geometry, known-good restoration first, local over broad regeneration, multi-level clearance, jumps as protected systems including the space under the ramp, before/after neighbor checks whose spans must cover the protected features).
- **DELIVERED:** source `eec4e91131e3b3853981a6146f37ee9e0d91da84` pushed and verified on origin/main. Fresh 0.66.0-review1 Windows build succeeded (0 errors, 11 warnings, 5m04s), built in an isolated worktree at that exact commit because an idle leftover batch Unity editor holds the main project. Published [game-66000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-66000); the existing draft-lookup hiccup was resolved with `--resume-draft`. All 232 Latest files match the public signed manifest; public download/signature/install/startup pass. [Delivery evidence](Docs/RegressionRestore/PUBLICATION.md).
- Unchanged Play-Racer.cmd launched a responsive managed 66000. Original settings restored byte-for-byte; no pending updates. Latest root/current 66000/previous 65000 retained.
- Cleanup: Builds **9,683,739,471 -> 7,679,034,176 bytes**; probe worktree removed. Final C: free **305,971,154,944 bytes**. All debug report history preserved.
- **SESSION HANDOFF: STOP. Wait for Dan to verify BUG-001 and BUG-002 before any further development.** Documentation-only delivery commit follows; playable source remains `eec4e911`.

## Previous delivery — Debug session lifecycle correction — 0.65.0-review1

- Safety checkpoint: clean main `23e383f95dc7ec1cf4a0570699fbade6531a37ed`. Scope is Debug session lifecycle only; previous Mountain cleanup remains complete.
- Successful export now marks the session CLOSED in its folder and ZIP. Next F4 creates a different timestamped session starting at BUG-001. Closed sessions reject additional captures; numbering is per session, with no copied screenshots or entries.
- START NEW DEBUG SESSION closes without export, preserves history, and creates the next folder only on capture. Unexported reports require explicit confirmation, defaulting to Keep Current Session. Debug HUD/menu show session ID, state and count. Failed export retains an OPEN session for retry.
- **History retention:** never automatically delete closed DebugReport folders or exported ZIPs. Original user reports and all lifecycle test history are preserved.
- All **28 focused input/lifecycle checks PASS**, including BUG-001/002 -> export -> new BUG-001, exact prior-folder/ZIP hashes, controller/keyboard cancellation, mouse confirmation, pending-confirmation F4 guard, and archive-failure rollback. UI/HUD screenshots inspected. [Validation](Docs/DebugLifecycle/VALIDATION.md), [controls](Docs/DebugReporting/DEBUG_MODE.md). Targeted testing complete; no broad gameplay matrix.
- **DELIVERED:** source `a888e6a5b9e5fbd4377166e7a943de87cd6ed34d` pushed and verified on origin/main; fresh 0.65.0-review1 Windows build succeeded (0 errors, 20 warnings, 4m52s). Published [game-65000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-65000). All 232 Latest files match the public signed manifest; public download/signature/install/startup pass. [Delivery evidence](Docs/DebugLifecycle/PUBLICATION.md).
- Unchanged Play-Racer.cmd launched responsive managed 65000. Original settings restored byte-for-byte, soundtrack preserved, no pending updates. Complete Latest root/current 65000/previous 64000 retained.
- Cleanup: Builds **9,683,288,517 -> 7,678,725,701 bytes**; **3,634,290,858 bytes** disposable output removed. Final C: free **308,386,238,464 bytes**. Every user/test report folder and ZIP preserved.
- **SESSION HANDOFF: STOP for Dan's next debug run.** Implementation, targeted checks and delivery complete. Documentation-only delivery commit follows; playable source remains `a888e6a5`.

## Previous delivery — 25-report Mountain cleanup, Debug input and sign audit — 0.64.0-review1

- Authorized by Dan’s supplied 25-report session and four-part cleanup request. Safety checkpoint: clean main `f034febc94ba52f61b3bba2acce0a620a0e2b993`. All original comments/screenshots reviewed; [individual dispositions and evidence](Docs/ReportCleanup/VALIDATION.md).
- BUG-001 pavement remesh join; 002 climb slit; 003 entry support/shimmer; 004 ragged shoulders; 005 obsolete fork sign; 006 side-cut barrier; 007 torn multi-level support; 008 suspended creek plane/shoulder; 009 artificial pit; 010 South Face receiving support; 011 buried flight sign; 012 offroad terrain gaps — corrected with route/flight/tunnel clearance retained.
- BUG-013 buried flight sign; 014 east receiving support; 015 crossing-triangle bend contact and arrow; 016 teal tutorial; 017 oversized main tutorial; 018 Summit jump tutorial; 019 green support ribbon; 020 duplicate homeward signs; 021 duplicate flight signs; 022 west receiving support/tree dependencies/lower shortcut clearance; 023 teal tutorial; 024 two-flights tutorial; 025 misleading apron — corrected. Every item has a final coordinate view and explicit PASS disposition in validation.
- Shared cause: malformed support faces/shoulder sheets replaced by connected terrain with explicit lower-route/flight clearance; existing working road/jump line preserved. Local pavement grades corrected only at the reported seams. Complete dependent trees, signs, props and rocks grounded; obsolete creek plane removed.
- **Standing sign design principle:** instructional/navigation signs only remain when they communicate useful information not already obvious from environment, arrows, gates, minimap or established conventions. Retain useful landmarks, world character, named optional routes and nonobvious hazards. Conservative worldwide removals; no non-Mountain track redesign.
- Debug regression fixed in the existing menu: gameplay ownership had disabled the shared UI module. Interactive Debug now keeps it active with explicit enabled-action navigation, focus and glyphs. All 28 controller/keyboard/mouse checks pass, including all eight actions and real in-game ZIP export. [Opened/verified export](Docs/ReportCleanup/verified-menu-export.zip) contains Markdown, JSON and referenced PNG.
- Results red **B Main Menu** restored; direct exit precedes Results subpage handling. Short isolated ordered-gate race, alternate Lap Times tab and B-to-Main-Menu pass. Other finish/scoring behavior unchanged.
- Targeted checks: Forward 1,983 / Reverse 2,091 support probes pass; extra Reverse 3,570 pavement probes and lower tunnel pass. Navigation data unchanged; visible/collision meshes match. Motorcycle affected corridors in both directions and ATV affected support/clearance traversals complete without reset/recovery. Final reported bend repeat upright/grounded (moto air 0.02s, ATV 0s). Raw fixture limitations, including an off-line Reverse main-road roll and explicit Forward shortcut selection, are retained in validation; no global AI tuning or full-course acceptance claim.
- **DELIVERED:** source `6af2c67dcfa4aaf14a167604bf535d6514be857e` pushed and verified on origin/main; fresh 0.64.0-review1 Windows build succeeded (0 errors, 20 warnings, 4m11s). Published [game-64000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-64000). All 232 Latest files match the public signed manifest; public download/signature/install/startup pass. [Delivery evidence](Docs/ReportCleanup/PUBLICATION.md).
- Unchanged Play-Racer.cmd launched responsive managed 64000. Original settings restored byte-for-byte, soundtrack preserved, no pending updates. Complete Latest root/current 64000/previous 63000 retained.
- Cleanup: Builds **9,845,981,594 -> 7,683,982,559 bytes**; **3,791,740,033 bytes** disposable output removed. Final C: free **308,871,303,168 bytes**. Original private report preserved; verified in-game export retained in Docs.
- **SESSION HANDOFF: STOP for Dan's next debug run.** Requested implementation, targeted checks and delivery complete. No broad tuning or unrelated work. Documentation-only delivery commit follows; playable source remains `6af2c67d`.

## Previous delivery — Mountain Reverse cleanup + developer reporting — 0.63.0-review1

- Dan explicitly authorizes A1–A10 at the supplied coordinates, followed only after Phase A verification by Debug Mode / bug reporting / detached inspection. This supersedes the prior STOP for these areas only. Other courses, physics, global AI, checkpoints and recovery remain protected.
- Safety checkpoint: clean main `b9041b01a5598a3fa1ce05a44f7de9a7ab59237f`.
- Phase A complete: A1 smooth retessellated climb; A2 continuous shoulder/join support; A3 lower natural outcrops and clear sign; A4 cleared support intrusion and steep paved step; A5 local grade smoothing; A6 mountain support under the existing receiving road; A7 natural separator rocks; A8 grounded Summit outcrops; A9 intrusive vault faces removed; A10 green support shards replaced. Each root cause, correction and targeted motorcycle/ATV result is recorded in [Phase A validation](Docs/MountainCleanup/VALIDATION.md). Full-route AI limitations are explicitly retained; no global AI/physics/recovery tuning.
- Standing Mountain rule: before filling apparent empty space beneath an upper road, verify whether a lower authored route requires clearance; preserve its complete corridor and support around it. A6 includes an intentional flight gap; support the existing receiving road, not the gap.
- Phase B complete: F3 Debug Mode (default off), F4 screenshot-before-comment capture, F6/Start debug menu, Markdown/JSON/PNG sessions and ZIP export, detached camera with keyboard/controller controls and return. Debug movement invalidates the active race and rolls back records from that attempt; HUD/capture preserve eligibility. **Debug Mode suspends overall race timeout and post-finisher grace; off resumes the remaining budget.** All 28 targeted checks pass, including three saved reports. [Controls](Docs/DebugReporting/DEBUG_MODE.md), [validation](Docs/DebugReporting/VALIDATION.md).
- A1 resolved: overlapping climb contacts/grade retessellated; both vehicles complete with zero resets/recoveries (motorcycle initial-placement air 0.02s, ATV 0s).
- A2 resolved: malformed shoulder/road fans replaced with continuous support; main and affected branch traversals pass both vehicles.
- A3 resolved: obscuring slab wall replaced by lower colliding outcrops; sign visible and adjacent traversal passes both vehicles.
- A4 resolved: intrusive support and steep paved step corrected; both vehicles have zero air/resets/recoveries through the merge.
- A5 resolved: local grade transition smoothed; both vehicles grounded. Baseline jump was not reproduced on the tested line.
- A6 resolved: receiving road supported by mountain rock shoulder, intentional flight gap preserved; both vehicles pass receiving pavement and support grid has no holes. Full-flight AI test limitation remains documented.
- A7 resolved: artificial separator slabs replaced with colliding irregular outcrops; view and adjacent vehicle traversals checked.
- A8 resolved: oversized stacked slabs replaced with grounded outcrops; view and both vehicles through Summit entry checked.
- A9 resolved: vault facets cleared from existing Summit pavement; both vehicles grounded through affected stations 8–30.
- A10 resolved: green support curtains replaced by continuous local terrain; view and affected descent pass both vehicles.
- **DELIVERED:** source `2b584b4598240c3109f93cb1c142a3a658b93947` pushed and verified on origin/main; fresh Windows build succeeds (zero errors, 11 warnings, 3m57s). Published [game-63000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-63000). All 233 Latest files match the public signed manifest; public download/signature/install/startup passes. [Delivery evidence](Docs/DebugReporting/PUBLICATION.md).
- Unchanged **Play-Racer.cmd** launched responsive managed 63000. Settings restored byte-for-byte; soundtrack preserved; no pending updates. Complete Latest root runtime, current 63000 and previous 62000 retained.
- Cleanup: Builds **9,688,401,947 → 7,682,834,457 bytes**; **3,647,352,311 bytes** disposable output removed including isolated public install and temporary reports. Final C: free **309,860,352,000 bytes**.
- **SESSION HANDOFF: STOP for Dan's clean-baseline debug run.** Implementation, targeted checks and delivery complete. No additional tuning or unrelated work. Documentation-only delivery commit follows; playable source remains `2b584b45`.

## Previous delivery — Urgent Mountain Reverse main-route restoration — 0.62.0-review1

This supersedes the previous polish STOP only for the reported junction regression. No broad Mountain polish is authorized.

- The Mountain polish accidentally filled intentional lower-route clearance and flattened part of the separate lower run-up. Lower main route restored as a tunnel/mountain-cut corridor through the existing left loop, approach, crossing, exit and continuation; original lower grade restored from history. X/Z line, direction and checkpoint progression preserved.
- Upper road supported around the corridor using earth banks, rock vault/outcrops and natural boulder edge barriers. Buried Main Route sign, local support seams and low CP1 visual gate corrected; local signs/cairns/complete trees grounded. No invisible walls or route redesign.
- Summit Traverse corrected rejoin preserved exactly; old U-turn has not returned. No global AI/physics/recovery or unrelated scene/UI/audio/startup/Race Complete changes.
- Targeted tests COMPLETE: motorcycle/ATV main-route production-driver traversals pass with zero resets/recoveries; one motorcycle Summit traversal passes; 660 lower probes and all upper support probes clear; 9/9 local recovery footprints clear. Initial buried-support contact failures retained; corrected by clearing secondary faces below pavement. See [validation](Docs/MountainCut/VALIDATION.md). This is automated technical evidence, not Dan's gameplay acceptance.
- Safety checkpoint: clean main `5e11668cb53f3921812c8941b883ecf132574839`. Completion source `4470658f9711e7de5da9dc2d6750a2ca33cf9786` pushed and remotely verified on origin/main. Fresh Windows build succeeded in 3m59s with zero errors / 12 warnings. Published [game-62000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-62000); all 233 Latest files match the public signed manifest. Public download/signature/startup and unchanged Play-Racer.cmd launch of responsive managed 62000 passed; original settings restored byte-for-byte and soundtrack preserved. [Delivery evidence](Docs/MountainCut/PUBLICATION.md).
- Cleanup: Builds **9,634,691,634 → 7,641,773,145 bytes**; **3,616,266,631 bytes** disposable copies removed including the temporary public install. Final C: free **291,182,661,632 bytes**. Current 62000 / previous 61000 retained.
- SESSION HANDOFF: Delivery complete. **STOP for Dan's gameplay review.** Do not reopen completed backlog work or start more Mountain polishing. Documentation-only delivery record follows; playable source remains `4470658f`.

## Previous delivery — Project cleanup and Mountain polish — 0.61.0-review1

The following records the previous 0.61 delivery. Its polish acceptance is superseded by the urgent regression correction above; older completed backlog decisions remain closed.

### ACTIVE / KNOWN

- CR-118: intermittent spoken-title clipping remains open; no later definitive human resolution. No audio retuning in this pass.
- This pass is implemented and awaits Dan's gameplay review: local Forest Forward tree grounding; Free Roam startup and shaped Start/Menu hint; ineffective Race Complete root Back prompt removed; Mountain support/prop grounding and Reverse Summit Traverse forward-facing merge.
- Targeted checks: Forest/cave preservation, controller/keyboard UI flow, representative Forward drive, 1,215 support probes, and final motorcycle/ATV production-driver traversals continuing beyond the Reverse merge. Both final traversals complete with zero resets/recoveries. Early fixture failures and local seam correction are documented in [Docs/MountainPolish/VALIDATION.md](Docs/MountainPolish/VALIDATION.md). No global physics/AI/recovery retuning.
- Safety checkpoint: clean `b29330bc4ceecf8eda2dba77697583f538db002e`. Completion source `56e63e4797d912f5e5822dee9f39eb3afc47919a` pushed/verified on origin/main. Fresh Windows build: zero errors, 21 warnings, 3m33s. Published [game-61000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-61000); all 233 Latest files match the public signed manifest. Public install/startup and unchanged Play-Racer.cmd launch of managed 61000 pass; settings restored and soundtrack preserved. [Delivery evidence](Docs/MountainPolish/PUBLICATION.md).
- Cleanup: Builds **9,565,965,293 → 7,593,020,763 bytes**; **3,578,626,950 bytes** disposable copies removed; C: free **294,350,192,640 bytes**. Current 61000 / previous 60000 retained. **STOP for Dan’s gameplay review.**

### FUTURE EXPANSION

- **Graphics upgrade (backlog, added by Dan 2026-10-02; not authorized yet).** Minimum goal: better-looking vehicles and drivers. Dan has Blender installed. Suggested order when scheduled: (1) lighting/post-processing/material pass in the existing URP setup; (2) one pilot vehicle + driver remodel for Dan's approval before doing the rest; (3) world/scenery later, if wanted. Must preserve vehicle colliders, handling, camera clearance, color selection and driver/vehicle identity. Free CC0 assets may be proposed; no paid assets without Dan's approval.
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

- Per Dan, 2026-10-02: 0.66 BUG-001 (lower main route) and BUG-002 (South Face Summit jump) verified; CR-087 Trickum ramp complete; separate lake/woodland circuit (CR-040) complete. Do not reopen without Dan's request.
- Dan's Backyard Loop Forward and Reverse accepted. The old back-property dirt trail / gully concept became this course and is complete, not a future track.
- Forest Forward cave accepted for now. No additional cave work; reopen only on Dan's explicit request.
- House 3 / Forest / Laurel AI issue complete, including BUG-009 and associated mapping/stuck-AI work. A route atlas is no longer an active prerequisite.
- General all-track navigation-arrow pass removed from active backlog. This pass addresses only the reported Mountain Reverse Summit Traverse rejoin.
- Ghosts accepted; older human-test-pending wording is superseded.
- Other later accepted/closed reconciliations remain authoritative.

## History archive

- Everything formerly below this point (historical delivery records, phases 0-9, the old bug tracker, CR-001 through CR-121, the decision log and old session handoffs) was moved VERBATIM to [PROJECT_TODO_ARCHIVE.md](PROJECT_TODO_ARCHIVE.md) on 2026-10-02 at Dan's request, to keep this file small enough to read in full every round.
- The archive is reference only. Unchecked boxes and "open/pending" labels inside it are superseded by the lists above; the latest explicit decision wins. Search it when a bug or rule needs history (known-good commits, earlier decisions, CR details). Do not delete it and do not append new work to it.
- When this file grows again, move the oldest "Previous delivery" sections to the end of the archive, verbatim.
