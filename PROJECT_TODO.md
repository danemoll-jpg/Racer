# Woodstock Rush
## Project Management / TODO / Astra-Codex Handoff

## Status: Woodstock Rush 1.0.0 is released (build 1000000, 2026-10-16); the game is in maintenance

- 1.0.0 is published ([game-1000000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-1000000)) and is what `Play-Racer.cmd` launches.
- New rounds come from Dan's bug reports (the hidden F3 debug tool, `DebugReports`) or his new ideas. The standing rules below stay as they are.

## Mandatory standing workflow

- **PROJECT_TODO.md is the source of truth** for project state, bugs, decisions, backlog and current work.
- **[CODEX_RULES.md](CODEX_RULES.md) contains the mandatory standing workflow/release rules.** Every future Codex task MUST read and follow it before making project changes, even when the individual prompt does not repeat those rules.
- A later explicit instruction from Dan may override a standing rule for that specific task.
- Do not silently modify or weaken CODEX_RULES.md. Changes to standing rules require an explicit instruction from Dan.
- Root AGENTS.md points future Codex tasks to both files.
- From 2026-10-02 the coding agent is Claude Code. The same two files govern it; root CLAUDE.md (created in the 0.67 round) is its discovery pointer.
- **Verification budget (explicit instruction from Dan, 2026-10-05; overrides heavier checking asked for in older rounds and narrows rule 11):** "I do want things fixed to have targeted testing. But things like racing all the tracks twice, I can take care of." "It is more about how many tokens I am spending for things I can do myself."
  - **Do:** one targeted check per change, at the place it changed, showing the thing asked for now works (the hole cannot be fallen into, the bump no longer upsets the bike, the button does its job, a new vehicle loads and sits on its wheels). Compile, launch, and the release steps as always. When a course scene's geometry is touched: the quick "nothing else changed" comparison of that scene's colliders/routes, and one lap of that one course in that one direction.
  - **Do not, unless the round explicitly asks:** race matrices (every course × direction × vehicle), repeat or confirmation runs, forced-shortcut sweeps, world-wide or all-nine-scene sweeps to re-prove a fix, before/after screenshot sets beyond one shot per fix, frame-rate tables on rounds that do not change rendering cost (one worst-view number when they do), reset batteries, or long validation documents.
  - **Dan does:** driving every course and direction, trying every vehicle everywhere, checking that a universal fix holds everywhere, and judging look and feel. He reports through the debug ZIPs.
  - **Controller first (Dan, 2026-10-07):** Dan plays with a controller. Any new or changed menu, screen or prompt must be checked once with a controller only (no mouse, no keyboard) before delivery: every control reachable, focus visible, B goes back.
  - **World changes only where Dan pointed (Dan, 2026-10-08):** fix exactly the place Dan reported. Do not extend a fix to "every similar place" in other scenes or across the world unless the round says so in Dan's words. If a similar problem is seen elsewhere, list it for Dan; do not change it.
  - **Signs (Dan, 2026-10-09):** sign text is part of the sign: one-sided (the back is plain), hidden by terrain and objects like the board, never drawn on top, sized to fit inside its board with a margin, readable at normal sign distance (about 20–30 m), not from across the map. (Mechanism: lettering uses the depth-tested, back-face-culled `Assets/Environment/Phase8/Depth tested world lettering 0.mat`, never a TextMesh's default font material.)
  - **Work in the background (Dan, 2026-10-10):** Dan uses the PC while rounds run; Unity and test players must not take focus or cover his screen. Run the editor in batch mode (`-batchmode -nographics` for scripts that need no rendering; `-batchmode` alone when they render) and never open the Unity editor window unless a step truly needs it. Start test players windowed, not full screen (`-screen-fullscreen 0 -screen-width 1280 -screen-height 720`), minimized and without activation (PowerShell `Start-Process … -WindowStyle Minimized`, or a no-activate launcher), with run-in-background on for check builds so they keep rendering; capture shots and frame sequences to files from the player itself (render texture / screen capture), not by bringing its window forward. Use 3840×2160 full screen only when a check is specifically about 4K full-screen layout, and say so in the results. Close every editor and player you started when the step is done.
  - **Player-facing text is American English (Dan, 2026-10-10).** color, gray, center, meter, favorite, tire, canceled, and so on in everything the player reads (menus, HUD, notices, signs, the launcher, shipped documents). Code names, file names and save / settings keys stay as they are.
  - **Keep this file small (Dan, 2026-10-15):** this file is read every round, so it holds only the standing rules, CURRENT, QUEUED and the **last three** "Previous delivery" sections. When a round is marked reviewed, move the oldest "Previous delivery" to the end of [PROJECT_TODO_ARCHIVE.md](PROJECT_TODO_ARCHIVE.md), verbatim, in the same commit. Search the archive only when a round points to older history; don't read it in full.
  - **Local build by default, public release only when asked (Dan, 2026-10-15; overrides CODEX_RULES sections 17–21 for ordinary rounds):** a round ends with a fresh Windows build that Dan can play from `Play-Racer.cmd` (or a launcher the round names), with his settings and saves untouched. **Do not** publish a GitHub release, run the hosted-download / signature / file-by-file checks, or update the production updater **unless the round says "publish"**, or Dan asks. If `Play-Racer.cmd` cannot run a build that wasn't published, say so once in the results and publish as before. The safety checkpoint, commits and push stay as now. Post-build cleanup is kept, but short: delete the round's temporary build output and keep the last two playable builds.
  - **Short results (Dan, 2026-10-15):** the Results for a round are **at most about 20 lines**:
    - one line per item: what changed and the one check, with a link to evidence files rather than describing them;
    - decisions made;
    - a "For Dan to check" list.
    No narrative of how it was built, no tables of runs, no repeated numbers. Detail goes in the round's `Docs/ReportNNN/` folder, if anywhere.
  - **Write-up:** the TODO results are a short list: what changed, the one check per item, decisions made, and anything Dan should look at. No separate VALIDATION.md unless a round asks. If a check would take more than a few minutes of play time, skip it and list it under "for Dan to check".

## CURRENT — Race replays from Chastain Rush: record every race, watch it with TV / chase / free cameras, keep the #1 lap and race, save any race — target 1.1.0-review1

Dan (2026-10-22, 01:51): "I implemented a Replay system in Chastain Rush. Can we add this system to Woodstock Rush". Dan on it in Chastain Rush (2026-10-22): "The replays seem to work fine." His original ask there: "automatically replace top 1 laps and races with the ability to save any race you want", and "be able to watch your race you just ran and move the camera around differently". This is a new feature for the 1.0 game: **no gameplay, world, course or balance changes** beyond the hooks replays need.

### Part A — Read Chastain Rush first (read only)
1. **Source:** `C:\Users\danmo\chastain_rush`, commit `736a8e7` ("Round 57 F: race replays"). Files:
   - `Assets/Scripts/Race/Replay/RaceReplay.cs` (ReplayData and the file format), `ReplayRecorder.cs`, `ReplayViewer.cs`;
   - `Assets/Scripts/Menus/GameMenus.Replays.cs` (WATCH / SAVE REPLAY on the finish screen, the REPLAYS page);
   - the hooks in that commit: `RaceController.cs`, `ArcadeVehicle.cs` (`Puppet`), `VehicleInput.cs`, `VehicleConfiguration.cs`, `OtherEngines.cs`, and the world events (`FenceChunk.ReplayBreak` / `ReplayRestore`, `TreeTrunks.ReplaySnap`, `ManholePlume.Erupted`, `RaceBarriers.GateOpened`);
   - its check `Assets/Scripts/Checks/Round20Runner.R57F.cs`, and the Round 57 F status in its `TODO.md` and `CLAUDE.md`.
2. **Only read it.** Never change, build or commit anything in `chastain_rush` (or `chastain_rush_r7`), and leave its open editors and players alone: Dan is working on it.
3. Chastain Rush started from Racer but is its own codebase now (namespace `ChastainRush`, `RaceController`, `GameMenus`). Port the design, and the code wherever it fits, onto Woodstock Rush's own classes: `RaceDirector` / `RaceFlow`, `RaceMenus.Finish`, `ChaseCamera`, `CameraViews.FreeLook`, `BreakableProp`, the ambient traffic, the records / Top 10 and `AtomicSave`. One line in the results: what was reused and what had to be written new.

### Part B — The same replays in Woodstock Rush
1. **Record every race** that ends on the finish screen: quick races, campaign events and championships, Stunt Races, split screen. Free roam, police chases and the other free-roam activities are not recorded.
   - Every racer (both players in split screen, every rival): pose, speed, steering, pedal, as Chastain samples them; the traffic; and the world events a race causes, so they happen again at the same moment (the `BreakableProp`s such as mailboxes, and whatever else a race can knock over or open in Woodstock Rush).
   - Engines and impacts play from the replay; the radio plays live (as Chastain does).
   - The replay uses the race's own time of day and weather. Rain and lightning don't have to match flash for flash.
   - Each racer looks as it raced: vehicle, colors, rider look (Dan's rider, player 2's, the rivals). Name tags follow the name-over-head option.
2. **Watch:** on the finish screen, **WATCH REPLAY** and **SAVE REPLAY**, with Chastain's buttons (X / V and Y / K) unless they clash with a finish-screen button already there; then pick free ones and say which.
   - Viewer controls as in Chastain: A play / pause, RT fast forward, LT rewind, X slow motion, Y camera (TV, chase, free), LB / RB racer, D-pad lap / restart / HUD (HUD off by default), View save, B leave. Keys as Chastain has them.
   - Cameras: TV (roadside cameras placed along the course that cut as the racers pass), chase on any racer, and free (Woodstock Rush's own free look). On the Mountain and Leviathan roads and in tunnels, a TV camera must never sit inside rock or terrain; where none can see the racer, fall back to a chase view as Chastain does.
   - Split screen replays play full screen, with LB / RB through both players and the rivals.
3. **Keep the #1s:** automatically, per course key and direction (and laps, as the Top 10 keys them), the replay of the #1 Top 10 lap and of the #1 Top 10 race. A new #1 replaces the old one. The best-lap replay plays just that lap (the others as they were then).
   - Stunt Races have their own records: keep their #1 by that record (no lap replay where there is no lap board).
   - Split screen counts on the Top 10 (Dan's rule), so its #1s keep replays too.
   - Records set before this version have no replay. The page only lists replays that exist.
4. **SAVE** any race from the finish screen or from the viewer, named by course, direction, date and time.
5. **A REPLAYS page** on the main menu (and the race / pause menus where it fits, as in Chastain): the kept #1s and the saved ones; watch or delete (the #1s can't be deleted there, only replaced). Fully usable with the controller, focus visible, B back.
6. **Files:** beside the records in the save folder (`Replays/best/`, `Replays/saved/`), with Woodstock Rush's own extension (not `.crreplay`), written the safe way the other saves are. Report the size of one 3-lap replay.
7. **Old replays:** mark each replay with the build and the course version (Leviathan's ids move on when its gates change). The page says when one is older: still playable, but the world may differ.
8. **Untouched:** Dan's settings, both campaign saves and the records load exactly as before; the debug tool stays hidden; Trailer Mode works as before.

### Check (exe, windowed, in the background, muted; a test save, never Dan's real records)
- Street Loop forward, 3 laps, 3 rivals, Normal traffic, smashing a mailbox or two. Finish and watch: positions match the race within a small error (report it), breaks happen on time, each camera works, every control works by pad (controller only) and keys. Save the race, find it on the page, and play it from there.
- #1s: in the test save, beat a lap and a race record and see the #1 replays replaced; a slower run leaves them alone.
- One Stunt Race replay, watched briefly (the tricks play). One split-screen race replay. One TV camera shot on a Leviathan stretch.
- Pictures: one per camera kind, the finish screen buttons, the REPLAYS page.

### Release
Version 1.1.0-review1, local build only (2026-10-15 rule). When Dan accepts it, a later round publishes 1.1.0. Close everything started; no background task left running. Short results (at most about 20 lines) with "For Dan to check".

## Previous delivery — Woodstock Rush 1.0: the finished game, published; then the trailer, filmed and cut — target 1.0.0 — DELIVERED (1.0.0 published, build 1000000; trailer first cut made), awaiting Dan's review — DAN MOVED ON (2026-10-22: replays from Chastain Rush in 1.1; his trailer review is still open)

Dan (2026-10-16, 01:12): "I am ready to finish this one up and just come back when I see bugs or want to add something. Lets call this 1.0 and go ahead and create the trailer." 0.121 is accepted; there are no new debug reports. This round makes **no gameplay, world or balance changes**. Part A is the release, Part B the trailer (the former QUEUED section, folded in here).

### Part A — Version 1.0.0, published
1. **Version 1.0.0** (no "-review" suffix). Pick a build number above 121000 that the updater orders after it, and say which. Everything that shows the version (main menu, debug HUD, launcher, results log, debug reports) reads 1.0.0.
2. **Publish:** this round says "publish", so run the full release as before 2026-10-15: GitHub release in `woodstock-rush-releases` (Latest, earlier releases kept), the hosted download, signature and file-by-file checks, and the production updater at the 1.0.0 build. `Play-Racer.cmd` launches 1.0.0, and Dan's settings and both campaign saves are untouched (byte for byte).
3. **Housekeeping, no game changes:** delete leftover temporary editor tools from `Assets/Editor` (`Report097Temp`, `Report098Temp` and any other `Report*Temp`) when no build uses them, and list what was removed. Leave `Assets/_Recovery/` alone and say whether Dan can delete it.
4. **Keep the debug tool, hidden** (Dan, 2026-10-16 01:14: "Lets keep the debug just keep it hidden. So i can access it if I find something"). The debug tool stays in 1.0 and works exactly as now: F3 mode, F4 capture, the F6 debug menu, sessions that survive quitting, export to ZIP, and reports in `DebugReports`.
   - **Hidden by default:** every launch starts with debug mode off, whatever it was when the game was last closed. Nothing debug-related shows on screen, in the menus, on the loading screens or in the trailer: no debug panel, no "F3 / F4" hints, no Debug rows in Settings.
   - **Still reachable:** the same keys turn it on (F3), and nothing else changes. An OPEN session is still resumed when debug mode is turned on.
   - Don't list it in any player-facing help or controls screen.
   - **Check:** launch 1.0.0: no debug UI anywhere. Press F3: the panel shows, F4 makes a report into the open session, F6 opens the menu. Quit and relaunch: hidden again, and the session is still open.
5. **TODO:** after the release, this file's top says the game is at 1.0 and in maintenance. New rounds come from Dan's bug reports or new ideas, and the standing rules stay as they are.
- **Check:** the published build launches through `Play-Racer.cmd`, its main menu shows 1.0.0 with no debug UI, and one quick race starts and finishes.

### Part B — The trailer, filmed and cut by Code (tools and output only)
Dan (2026-10-13, 19:57): "I think I am going to add the video making back to the list. It is difficult for me to make it look good, especially with some of the camera angles suggested." Dan (2026-10-16): "go ahead and create the trailer." This reverses 0.98's "stop filming until Dan confirms". Lessons from 0.97 Part C (clips discarded): the footage showed broken things (the storm drain), and filming must never change the game or the world.

1. **The plan is `Docs/Trailer/shot-list.md`** (2026-10-12: 20 shots plus split-screen S1–S3, about 65 s; the Project copy is the same). The game gained a lot after it was written, so **replace up to 3 of the weaker shots** with the new features. Keep the total at about 65–75 s, and add each new shot to `shot-list.md` (marked "added 1.0"):
   - Leviathan's summit flight;
   - a Stunt Race with rivals tricking in the air (a BACKFLIP + ROLL);
   - the new rider look (hands, hair) in a close chase shot.
   For a shot that can't be made exactly as written, use the closest one that looks good and say why.
2. **Capture** with the 0.97 offline method (`Tools/Report097/Run-Trailer.ps1` and its capture mode, in a development build of 1.0.0 only, never in the released menus):
   - fixed 60 fps rendering, every frame at full quality, 3840×2160 if it renders, otherwise 2560×1440;
   - real slow motion (more frames), never stretched;
   - game sound recorded: engines (other vehicles too), sirens, radio voices, rain, thunder; the in-game radio music off;
   - HUD off, except S1–S3 (HUD on, both views);
   - the same rider look and vehicle colors throughout;
   - the game's own scenes as they are. **No world changes for filming.** If something looks wrong on camera (a bump, a floating sign, a seam), don't hide it: list it with its position for Dan as a bug and pick another angle.
3. **Each shot** is its own clip, a few seconds longer than the edit needs, with three takes of anything with a jump: `Trailer/Clips/NN-name.mp4` (S1–S3 as `S1-…`). Make a **contact sheet** (`Trailer/contact-sheet.jpg`: three frames per clip, labeled) so Dan can review fast.
4. **First cut** in shot-list order and timing:
   - `Trailer/WoodstockRush-trailer-v2.mp4` (game sound) and `-silent.mp4` (for music);
   - the title "Woodstock Rush" over the last shot, and the end card is the cover art;
   - simple cuts and short fades, nothing flashy;
   - no music added (Dan picks a licensed track). If Dan has put one in `Trailer/Music/`, make a third version with it, mixed under the game sound.
5. Close every player and editor started. Commit nothing to git except the scripts and the shot list. The 1.0.0 release and Dan's saves stay untouched.
- **Deliver:** the contact sheet and the cut first; a list of any shot that didn't match the plan and any bug seen on camera.

### Results
Short (at most about 20 lines): the version, build and release links; what housekeeping removed; the trailer files; "For Dan to check".

### Results (2026-10-16, Claude Code)
- **1.0.0 published:** build **1000000** (1.0.0 → 1,000,000, above 121000), source `c82ebccf` (checkpoint `57f0c86f`), [game-1000000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-1000000) Latest, earlier releases kept; public download, pinned signature, startup and all 235 files match `Builds/Latest` ([hosted/result.json](Docs/Report1000/hosted/result.json)); updater at 1000000, 121000 kept as the previous. `Play-Racer.cmd` (unchanged) launched 1.0.0 ([play-racer-launch.json](Docs/Report1000/play-racer-launch.json)); settings and both campaign saves byte for byte ([settings-preserved.json](Docs/Report1000/settings-preserved.json)). Cleanup: Builds 10.63 → 8.35 GB, C: free 201.5 GB.
- **Version shown:** the main menu now reads "Version 1.0.0" under the title (it showed no version before); debug reports, the results log and the launcher take it from the build.
- **Debug hidden:** the tool was already off at every launch (never saved); no F3 hints, Debug rows or help entries exist in a release build. The shipped README no longer lists the keys and `Review/DEBUG_MODE.md` is no longer shipped. Check in the released build ([phase 1](Docs/Report1000/Player/debug-check-phase1.txt), [phase 2](Docs/Report1000/Player/debug-check-phase2.txt), [menu](Docs/Report1000/Player/menu-2.png)): nothing at launch; F3 panel, F4 report into the session, F6 menu; relaunch hidden, session still OPEN; a 1-lap race started and finished (131.7 s). A test-save launch now keeps its reports in the test save, never in `DebugReports`.
- **Housekeeping:** removed `Assets/Editor/Report097Temp` and `Report098Temp` (copies stay in Tools/Report097, Report098). `Assets/_Recovery/` left alone: Unity crash-recovery copies of an unsaved scene (Sep 18 – Oct 9), in no build: Dan can delete it.
- **Trailer** (not in git: `Trailer/`): [contact sheet](Trailer/contact-sheet.jpg), `Trailer/WoodstockRush-trailer-v2.mp4` (73.8 s, 4K 60, game sound) and `-silent.mp4`; 41 clips in `Trailer/Clips` (3 takes of every jump). No track in `Trailer/Music/`, so no music version. Shot list: 6 → 6N rider close, 9 → 9N Leviathan summit, 12 → 18N Stunt Race BACKFLIP + ROLL ([shot-list.md](Docs/Trailer/shot-list.md)). Tools: [Tools/Report1000/Trailer](Tools/Report1000/Trailer) (capture in the editor's play mode, batch mode; `cut.py`, `sheet.py`, `edit.json`).
- **Decisions:** the recorder's sound now follows the picture (it ran 28% long); split-screen and garage shots are composited from every camera plus the HUD; 18N's rivals were given only BACKFLIP + ROLL in memory for the clip.
- **Not as planned:** 3 ends on the Needle 600 Champion (the normal garage never shows stunt vehicles); 9N and 20 are in daylight (race shots keep the race's own light); 20's car is Dan's red Street Classic, and the crest gives only a short hop; 9N shows the rider's flight, the field behind; S2's cop is driven along the road, not chasing; 14 has no radio line; 1 is dim.
- **Bugs seen on camera:** none in the world.
- **For Dan to check:** the cut and the takes (swap in-points or takes in `Tools/Report1000/Trailer/edit.json`, then `python Tools/Report1000/Trailer/cut.py`); pick a licensed track for `Trailer/Music/`; 1.0.0 from `Play-Racer.cmd`.

## Previous delivery — Leviathan Forward Stunt Race: rivals driving in circles at CP 44 (Hwy 92) — target 0.121.0-review1 — DELIVERED as 0.121.0-review1 (build 121000, local build), awaiting Dan's review — REVIEWED BY DAN (accepted; the game is called 1.0)

Dan's report: session `2026-10-15_15-52-55-667_6d34cd` (now CLOSED and exported as `…_6d34cd_1615a117.zip` in `DebugReports`). BUG-001 to BUG-004 were done in 0.119; **only BUG-005 is new** (on 0.120). **Look at its screenshot first.**

**BUG-005** Leviathan Forward, **Stunt Race**, Clay Hopper, lap 2 of 2, Day / Clear, X=290.96 Z=560.12, heading 82°, at the Hwy 92 intersection. The HUD says "NEXT CP 44 0 ft". Dan was 1st, 228.7 s ahead of June Park. Dan: "why they all just driving around in a circle?" In the shot, four rivals (Rusty Vance, Hollis Gray, Mia Torres, Big Ed Kowalski) mill around in the intersection instead of going on.

1. **Find the cause first** and say it in one or two lines. Reproduce it with an AI-only Leviathan Forward Stunt Race (2 laps) and, for comparison, one normal Leviathan Forward race. Things to rule out:
   - CP 44 not counting for rivals (or for anyone) where it stands. "0 ft" means the guide thinks Dan is on top of it.
   - A rival stuck behind the in-order gate rule (0.115), driving back for a gate it missed.
   - A 0.120 stunt bump near here throwing the rivals off their line.
   - The rivals losing their place on the course near the twice-driven roads (0.117 / 0.118).
   - Stunt Race rival logic, such as trick or bail recovery, putting rivals back on the wrong pass.
2. **Fix that cause only.** The rivals go through this intersection and finish, in Stunt Races and normal races. Don't move gates or change the course unless the cause is there. If something has to move, keep it as close as possible and say what moved.
3. If the cause also affects normal Leviathan races or the other courses, fix it there too and say so. Don't change anything else.
4. **Check:** one AI-only Leviathan Forward Stunt Race (2 laps, 5 rivals, mixed stunt vehicles): every rival finishes, none circles at CP 44, ≤ 2 resets each. One normal Leviathan Forward AI race: all finish. One shot at Dan's spot as rivals pass through.

### Release
Version 0.121.0-review1, local build only (2026-10-15 rule). Close everything started; no background task left running. Short results (at most about 20 lines) with "For Dan to check".

### Results (2026-10-15, Claude Code)
- Checkpoint `c0ebb1e9`. Dan's race was the Stunt Chapter's **Stunt Final** (Hard, 2 laps, 5 rivals; his results log). Evidence [Docs/Report121/](Docs/Report121/); check `-p106kind final121` ([Report106PlayerCheck.Round121.cs](Assets/Scripts/Report106PlayerCheck.Round121.cs)), tools [Tools/Report121/](Tools/Report121/).
- **Cause:** 0.120's Kicker 6 stood 85 m before CP 44; at Hard its flights carry 100–134 m (0.120 worked them out at 53 m), so rivals flew over / beside CP 44 (3–13 m up, 8–24 m right) and it did not count. High over the line the road station jumps more per step than the verified-progress rule allows, so verified progress froze in the air: the miss was never charged, the rival's station stayed pinned 60 m past the gate and the AI turned back to it again and again. Reproduced: all 5 rivals circling 12–16 min, never finishing ([before](Docs/Report121/Player/before-stunt-final.txt), [diagnosis](Docs/Report121/Player/before-diag-lap1.txt)). Not a gate rule, a reset or Stunt Race rival logic.
- **Fix 1 (Stunt Races only):** Kicker 6 moved to Hwy 92 west, (−105.3, 525.9), station 6510, same size / side / heading, 246 m back; flights now land before CP 43 ([bumps.md](Docs/Report121/bumps.md)). No gate or course change.
- **Fix 2 (every race):** when a racer lands back on the line after a flight, its verified progress moves up to where it is, never further than it flew (+12 m) ([RaceDirector.cs](Assets/Scripts/RaceDirector.cs)). A gate flown past is charged +5 s on landing instead of leaving the racer stuck. Check with the old bump spot and this fix: rivals still fly over CP 44, are charged, all finish ([code-fix-only](Docs/Report121/Player/code-fix-only-old-bump.txt)).
- **Check:** Stunt Final, AI only (2 laps, 5 rivals, Clay Hoppers and Skywhips, Hard): every rival finished, none waited more than 4 s at CP 44, every CP 44 pass on the ground and counted ([after](Docs/Report121/Player/after-stunt-final.txt)); normal Leviathan Forward, Hard, 5 rivals: all finished, ≤ 1 reset each ([normal](Docs/Report121/Player/normal-race.txt)); [shot at Dan's spot](Docs/Report121/Shots/spot-BUG-005-stunt.jpg) as June Park drives through.
- **Decisions:** the bump moved back past CP 43 rather than removed (the nearest spot whose flights can't reach a gate; s 6540 still landed some 30 m short of CP 43 and two rivals missed it); other bumps near gates listed, not moved ("world changes only where Dan pointed").
- **For Dan to check:**
  - The new Kicker 6 spot on Hwy 92 west; and the other bumps with a gate soon after them ([bumps.md](Docs/Report121/bumps.md)).
  - Resets: Hollis Gray reset 3 times in the final check (Kyle's bank and the lake-shore → CR110 turn, the known 0.118 spots, also before the fix); one rival in one earlier run reset once at the new Kicker 6 landing.
  - Big kicker 5 (Leviathan Forward) flies over CP 30 at 15–18 m without a miss being charged; not looked into.
- **Release:** local build only (no GitHub release). Built fresh from `4061b15f` (pushed), 0 errors ([build-release.txt](Docs/Report121/build-release.txt)); signed locally and activated by the existing updater: `Builds/Latest` runs `versions/121000`, 120000 kept as the previous ([runtime-identity.json](Docs/Report121/runtime-identity.json)). `Play-Racer.cmd` (unchanged) launched 0.121.0-review1 ([play-racer-launch.json](Docs/Report121/play-racer-launch.json)); your settings restored byte for byte ([settings-preserved.json](Docs/Report121/settings-preserved.json)). Cleanup: Builds 12.47 → 8.35 GB, C: free 204.1 GB.

## Previous delivery — Stunt Races and the Stunt park after Dan's first drive of 0.119: points on screen, a multiplier for every trick in one flight, bumps to trick off, tricks out of the park count again, the garage shows stunt vehicles — target 0.120.0-review1 — DELIVERED as 0.120.0-review1 (build 120000, local build), awaiting Dan's review — REVIEWED BY DAN (BUG-005 in 0.121)

Dan (2026-10-15, 18:58), after playing 0.119 (no debug report this time; his words are the spec):
> 1) It was supposed to show your total points during the race but it is not there
> 2) On the ramps you really have only a couple of times to get a score, the multiplier should go up for every stunt type you do during a jump
> 3) perhaps we can add some bumps on the stunt tracks in order to be able to get some extra point
> 4) The stunt course itself: we had a rule that stunts completed out of the arena still counted but I think it has reverted here. There is no counter for your current stunt and multiplier
> 5) You can select the garage but it is pointless because it only shows all the other vehicles you can't use, the stunt course vehicles aren't there...at least I didn't see them

Standing rules apply (controller first, verification budget, signs, American spelling). Normal races, their records and the normal campaign chapters stay unchanged.

### Part A — One trick HUD everywhere tricks score
Items 1 and 4 (second half). The Stunt Race, the Stunt chapter's events and the Stunt park event (solo and split screen, each player their own) show:
1. **Total stunt points** for the race or event, always on screen, updating as tricks land. In Stunt Races, the trick time bonus earned so far next to it ("−4.5 s"). In split-score races, also your live stunt-point place (as the 0.119 spec asked; it was missing).
2. **The current trick and multiplier**, live: the trick names as they happen in the air ("BACKFLIP + ROLL"), the multiplier ("x2"), the points on the line, and the chain countdown on the ground. Then "LANDED +1,240" or "BAILED".
3. Placed clear of the position / lap panel, the NEXT CP line, the minimap and the speedometer; smaller in races than in the park (0.119 item 6). Both split-screen views.
- First find out why the total and the counter don't show (missing, hidden, or behind another panel) and say which in one line.
- **Check:** one shot each: Stunt Race mid-trick, Stunt Race after a landing, the park mid-combo, a split-screen view.

### Part B — The multiplier goes up for every trick type in one flight
Item 2. Dan: "the multiplier should go up for every stunt type you do during a jump."
1. Within one flight, each **different** trick type added (flip, roll, 360 spin, and any other type the game has) raises the multiplier by 1: backflip alone x1, backflip + roll x2, backflip + roll + 360 x3. Repeating the same type adds its points, not multiplier.
2. Chaining on the ground (the 5 s chain) keeps working as now on top of it.
3. **One scoring system:** the same rule for the player and the rivals, in Stunt Races, the Stunt chapter and the Stunt park.
4. Because scores go up, re-tune the trick time bonus cap in Stunt Races (still "can't win by tricks alone") and the Stunt chapter targets (Summit Tricks, the park medals if they change). Old park records stay on the boards as they are. Say the new numbers.
5. Rivals use it on long flights (a two-type combo sometimes, more on Hard).
- **Check:** one flight with three types landed: x3 shown and scored; one with the same type twice: x1.

### Part C — Bumps to trick off on the stunt courses
Item 3. Dan: "perhaps we can add some bumps on the stunt tracks in order to be able to get some extra points."
1. On each Stunt Race course (Mountain Loop Forward / Reverse, Leviathan Forward / Reverse), add **kickers and bumps that exist in Stunt Races only** (never in normal races or Free Roam), so there are more chances for air between the big jumps.
2. About 4–6 per course, on the course or just beside it, so you choose to hit one or not. Each fits its place (a dirt mound, a driveway lip, a bank), stands on its own ground, lands on a down-slope, nothing floating, and doesn't block the line or a gate. Not on or right before the existing big jumps.
3. Big enough for one trick type with a stunt vehicle at racing speed; one or two bigger ones per course for two types.
4. Rivals can take them and trick off them; none gets stuck or reset on one.
5. List every bump with its position, so Dan can say which to remove or move.
- **Check:** a top-down of each course with the bumps marked; one clip over one bump with a trick landed; one AI Stunt Race on one course with no rival reset at a bump.

### Part D — Tricks that leave the park count again (regression)
Item 4 (first half). 0.107 Part B, accepted: "If a trick takes you out of the park, I feel like it should still count." A flight that **takes off inside the park counts wherever it lands** (Stunt park event solo and split screen, the Stunt chapter's park event, Free Roam best combo). A flight that takes off outside the park still doesn't count in the event.
- Find which round reverted it and say so in one line; put it back without undoing anything else that round did.
- **Check:** in the park event, a backflip that lands outside the fence on the Hwy 92 side is added; reset puts you back in the park.

### Part E — The garage shows stunt vehicles for stunt events
Item 5. When the garage is opened from a Stunt Race setup (Quick Race and split screen), a Stunt chapter event or the Stunt park event, it shows **only the stunt vehicles**: the Clay Hopper, the Skywhip 250, the Clay Hopper Cup and any later one, with their stats and paint. Owned ones selectable; not-yet-unlocked ones as silhouettes (standing rule). The choice saves as the stunt vehicle (0.119 keeps it apart from the normal one) and is the default next time.
- The normal garage (normal races, normal chapters) never shows stunt vehicles, as now.
- **Check:** controller only: Stunt Race setup > garage shows the stunt vehicles, pick the Skywhip, start, it's the Skywhip; the same from a Stunt chapter event; B back.

### Release
Version 0.120.0-review1, local build only (2026-10-15 rule). Close everything started; no background task left running. Short results (at most about 20 lines) with "For Dan to check".

### Results (2026-10-15, Claude Code)
- Checkpoint `605e8d38`. Evidence [Docs/Report120/](Docs/Report120/) ([Shots/](Docs/Report120/Shots/), [Player/](Docs/Report120/Player/)); checks [Run-Checks.ps1](Tools/Report120/Run-Checks.ps1).
- **A, why missing:** the race panel only ever showed the seconds off ("TRICKS −x s"), never the points; the live combo was the small bottom-left activity line, with nothing in the air; in the Stunt chapter's park event the 0.96 rule that hides activity lines in non-race campaign events hid every trick line.
- **A:** race panel (solo and each split-screen view): `STUNT 12,340 pts · −4.5 s` (Split score: `· 2nd in tricks`), rising as tricks land. New trick panel ([TrickHud.cs](Assets/Scripts/TrickHud.cs)) on the left of each view, wherever tricks score: live trick names, `x2`, points on the line; then `LANDED +n` / `BAILED`; `COMBO x3 · pts · 3.2 s`. Shots: [race mid-trick](Docs/Report120/Shots/A-race-mid-trick-4.jpg), [race after a landing](Docs/Report120/Shots/A-race-after-landing-9.jpg), [park mid-combo](Docs/Report120/Shots/A-park-mid-combo.jpg), [split screen](Docs/Report120/Shots/A-split-both-views.jpg).
- **B:** a flight's points (tricks and jump) × its number of different types (flip, roll, spin; barrel roll is a roll); the 5 s chain on top; one function for players and rivals (rivals gain BACKFLIP + ROLL, FRONTFLIP + 360, BACKFLIP + ROLL + 360). Check ([park](Docs/Report120/Player/park.txt), [race](Docs/Report120/Player/race4.txt)): backflip + roll x2 (2,448), double backflip x1 (1,157), backflip + roll + 360 x3 (10,305) shown and scored; rivals landed x2 and x3.
- **B, numbers:** trick bonus 0.5 s per 2,000 pts (was per 500; at 1,000 a single lap reached the cap), cap still 8 s a lap; Summit Tricks 3,000 / 6,000 / 9,500 (one / two / three types; was 3,100 / 3,600 / 4,100); park medals unchanged; old records untouched.
- **C:** bumps in Stunt Races only ([StuntBumps.cs](Assets/Scripts/StuntBumps.cs), [StuntBumps.json](Assets/Resources/StuntBumps.json), placed by [bumps.py](Tools/Report120/py/bumps.py) from course probes): Mountain Loop Forward 2, Reverse 3, Leviathan Forward 6, Reverse 6, each listed in [bumps.md](Docs/Report120/bumps.md) with [top-downs](Docs/Report120/Bumps/). Kicker 1.5 m over 4.5 m, big 3 m over 7 m, park dirt, on one half of the road; kept clear of gates, the big jumps and their run-ups, the marked jumps (High Ridge Drop, Fern Creek Leap…) and Leviathan's twice-driven roads. Checks: [clip](Docs/Report120/Clips/bump-ML-forward-1-backflip.mp4) (backflip landed, +879); AI races ([ML Forward, Hard](Docs/Report120/Player/race4.txt): 3 rival flights off bumps, 0 resets; [Leviathan Reverse](Docs/Report120/Player/race9.txt): 17 flights off bumps, 1 reset 282 m from any bump); a normal race in the same scene: no bumps.
- **D, cause:** no round reverted it: 0.107's rule still held for a lone flight ([0.119 check](Docs/Report120/Player/base-0.119-event107.txt)), but any later hop that took off outside (down the bank after landing) joined the same combo and, by the 0.106 combo rule, threw the whole combo out. Now such a hop is "not counted" on its own (in Free Roam the park combo is banked first). Check: backflip out to the Hwy 92 side +709, then the hop "OUTSIDE THE PARK: NOT COUNTED", reset back in the park ([shot](Docs/Report120/Shots/D-hop-outside.jpg)).
- **E:** the Stunt chapter's garage page (A on the vehicle row) listed only non-reward vehicles, so it showed the normal ones as "not allowed"; it now shows the stunt vehicles (locked ones as silhouettes); the park event's vehicle row is stunt vehicles only; every stunt choice saves as the stunt vehicle. Controller only ([garage](Docs/Report120/Player/garage.txt)): Stunt Race garage, Clay Hopper → Skywhip 250, race started in it; Stunt Opener garage Skywhip / Cup (locked) / Clay Hopper, USE, B back; Chapter 1's garage unchanged ([shot](Docs/Report120/Shots/E-campaign-garage-ch5.jpg)).
- **Decisions:** the multiplier covers the jump points too; the race total counts the open combo; the Mountain Loops get 2 and 3 bumps, not 4–6 (most of their lap is a big jump, its run-up or a marked jump, and the rest climbs); landings are level or downhill, never uphill; one 360 needs about 1.5 s, so the small kickers suit the player and rivals trick mostly off the big ones; the park event no longer offers normal vehicles.
- **For Dan to check:** the bump spots and sizes ([bumps.md](Docs/Report120/bumps.md)); the bonus rate and Summit Tricks targets; whether park medals need raising now that x2 / x3 flights exist.
  - Seen, not changed: 0.117's Leviathan kickers (Rocky Way Cut, Hollow Creek Hop) draw pale gray-white (the ground material needs vertex colors they lack).
  - The split-screen vehicle pick still hides locked vehicles (no silhouettes), as before.
  - Each bump's far side is a steep face, as Leviathan's 0.117 kickers; none stands where the course comes back the other way.
- **Release:** local build only (no GitHub release). Built fresh from `3414957b` (pushed), 0 errors ([build-release.txt](Docs/Report120/build-release.txt)); signed locally and activated by the existing updater: `Builds/Latest` runs `versions/120000`, 119000 kept as the previous ([runtime-identity.json](Docs/Report120/runtime-identity.json)). `Play-Racer.cmd` (unchanged) launched 0.120.0-review1 ([play-racer-launch.json](Docs/Report120/play-racer-launch.json)); the first try stopped on the launcher's menu because input arrived during its countdown. Your settings restored byte for byte ([settings-preserved.json](Docs/Report120/settings-preserved.json)). Cleanup: Builds 11.62 → 7.77 GB, C: free 188.8 GB.

## History archive

- 2026-10-16 (twelfth move): 0.121 reviewed, so the 0.118 "Previous delivery" section was moved verbatim to the end of the archive. The QUEUED trailer section was folded into the 1.0 CURRENT section.
- 2026-10-15 (eleventh move): 0.120 reviewed, so the 0.117 "Previous delivery" section was moved verbatim to the end of the archive.
- 2026-10-15 (tenth move): 0.119 reviewed, so the 0.116 "Previous delivery" section was moved verbatim to the end of the archive.
- 2026-10-15 (ninth move): 0.118 reviewed, so the 0.115 "Previous delivery" section was moved verbatim to the end of the archive.
- 2026-10-15 (eighth move, Dan's instruction to save tokens): the 0.61–0.114 "Previous delivery" sections (41) were moved verbatim to the end of the archive. From now on keep only the last three "Previous delivery" sections here; when a round is marked reviewed, move the oldest one to the end of the archive in the same commit.
- 2026-10-04 (seventh move): the 0.73 and 0.74 "Previous delivery" sections were moved verbatim to the end of the archive.
- 2026-10-04 (sixth move): the 0.72 "Previous delivery" section was moved verbatim to the end of the archive.
- 2026-10-04 (fifth move): the 0.71 "Previous delivery" section was moved verbatim to the end of the archive.
- 2026-10-03 (fourth move): the 0.69 and 0.70 "Previous delivery" sections were moved verbatim to the end of the archive.
- 2026-10-03 (third move): the 0.67 and 0.68 "Previous delivery" sections were moved verbatim to the end of the archive.
- 2026-10-02 (second move): the 0.62–0.66 "Previous delivery" sections were moved verbatim to the end of the archive.
- Everything formerly below this point (historical delivery records, phases 0-9, the old bug tracker, CR-001 through CR-121, the decision log and old session handoffs) was moved VERBATIM to [PROJECT_TODO_ARCHIVE.md](PROJECT_TODO_ARCHIVE.md) on 2026-10-02 at Dan's request, to keep this file small enough to read in full every round.
- The archive is reference only. Unchecked boxes and "open/pending" labels inside it are superseded by the lists above; the latest explicit decision wins. Search it when a bug or rule needs history (known-good commits, earlier decisions, CR details). Do not delete it and do not append new work to it.
- When this file grows again, move the oldest "Previous delivery" sections to the end of the archive, verbatim.
