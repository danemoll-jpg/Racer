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

## CURRENT — Woodstock Rush 1.0: the finished game, published; then the trailer, filmed and cut — target 1.0.0 — Part A DELIVERED (1.0.0 published, build 1000000); Part B in progress

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

## Previous delivery — Leviathan: Dan's 4 reports on 0.118 (two more barricades out, CP 18 straight ahead, stars behind the mountain); Stunt Races (stunt vehicles only, own records) and the Stunt chapter in the campaign — target 0.119.0-review1 — DELIVERED as 0.119.0-review1 (build 119000, local build), awaiting Dan's review — REVIEWED BY DAN (5 notes from play; follow-ups in 0.120)

Dan (2026-10-15, 16:16): "please pull in my latest bug report and lets prepare it for the next round. Also, maybe we go ahead and implement the stunt vehicle only races along with the stunt campaign chapter." So this round is Part A (his report) **and** the former QUEUED NEXT (Parts B–C), in that order. The proposed defaults in Parts B–C are taken as decided unless marked "Dan to check".

Dan's report: session `2026-10-15_15-52-55-667_6d34cd` (0.118.0-review1, Drifter Twin, 4 reports; still OPEN, not zipped: read it from the folder in `DebugReports`). **Read the 4 screenshots first.** Standing rules apply, including "World changes only where Dan pointed" and the 0.97 sign rule. Part A applies to the Leviathan races only, except A4 (the night sky).

### Part A — Dan's 4 reports (0.118)

1. **BUG-001** Leviathan Forward, lap 2, X=570.63 Z=−88.85, heading 354°, next CP 17 (the gate is in view to the left). A timber barricade with a ROAD CLOSED sign straight across the way ahead. Dan: "this barrier should be removed. It is straight in the path where you are supposed to go. Seems AI is getting hung up here."
   - Remove this closure (all its panels and its sign).
   - Find out why the course line or the rivals' line ran into a closure here and say so in one line. If another 0.117 closure touches Leviathan's line or a rival lane, **list it for Dan; do not remove it.**
   - **Check:** one shot at the spot; one AI race of that stretch with no rival stuck or reset there.
2. **BUG-002** Leviathan Forward, lap 2, X=526.29 Z=−25.40, heading 296°, next CP 18 (112 ft away). Dan: "this gate should just be straight ahead from here instead of where it is."
   - Move CP 18 so it stands **straight ahead of this spot** (along about 296°), on the course, facing the way Dan is driving, on its own ground (0.118 rule).
   - If the course line hooks away here and that is why the gate sits off to the side, straighten the line from CP 17 to CP 19 on existing drivable ground (race only), so you drive straight at the gate. Kyle's bank (0.118) stays as it is.
   - The next gate is still in view from it (0.118 rule). Course id moves on to `leviathan-forward-v4` (records start again).
   - **Check:** one shot from Dan's spot; one rival lap through it with no reset.
3. **BUG-003** Leviathan Reverse, lap 1, X=−589.61 Z=−531.84, heading 140°, next CP 10, by the church. A fence across the dirt way ahead. Dan: "this one should have been removed too." It is the other end of 0.118's BUG-002 dirt way (0.118's note: "still closed at its other end, 49 m on").
   - Remove this closure (fence and sign). Every other closure stays as it is.
   - If the open dirt way now lets a racer skip a gate, the in-order gate rule must still hold; say whether it does.
   - **Check:** one shot at the spot.
4. **BUG-004** Leviathan Reverse, lap 2, X=459.83 Z=−515.92, heading 135°, **Night / Clear**. Dan: "stars are in front of the mountain." Stars show as dots over the hillside.
   - Stars belong to the sky: always behind terrain, trees, buildings and clouds, never drawn over them. Find the cause (for example a star dome or particles with depth test off, drawn after the world, or closer than the far hills) and say which in one line.
   - This is a sky fix, not a world change, so fix it wherever the night sky is drawn (Free Roam and every course).
   - **Check:** one night shot at Dan's spot and one in Free Roam with a hill against the sky.

### Part B — Stunt Races

Dan (2026-10-14, 19:24–19:26): "With all the air you get on the mountain jumps, it would be a fun place to do stunts" … "or we can have special stunt-only mountain races with their own separate records and it would mix well with a stunt campaign." Stunt vehicles stay out of normal races, the existing campaign chapters and their records (fairness, balancing).

1. A race type **Stunt Race**: stunt vehicles only (Clay Hopper, Skywhip 250, and any later stunt vehicle) for the player and every rival; air steering on; the stunt park's landing rules (0.107: 1.5 s recovery window, bail only on a real roof or side landing).
2. **Courses: Mountain Loop Forward / Reverse and Leviathan Forward / Reverse**, the same routes, gates, shortcuts and in-order rules as the normal races (Leviathan as left by Part A).
3. **Scoring: position by finish time, with a trick bonus.** Each landed trick or combo knocks time off your race time (0.5 s per 500 trick points), capped so a race can't be won by tricks alone, shown live ("−2.5 s TRICK BONUS").
   - **Rivals do tricks too** (Dan, 19:28: "the rivals can't do stunts?"). At the big jumps whose flights are known (the summit flights, Downhill Ridge Cut's flight, the Street Loop jumps on Leviathan), a rival picks a trick its airtime allows (flip, roll, 360, or a combo on long flights) by its skill and the difficulty, rotates through it with the same air steering the player has, and lands it.
   - An occasional bail (more on Easy, rare on Hard) costs it the bonus and a little time, never a stuck reset.
   - Rivals earn the same trick bonus. Their tricks show briefly over them ("BACKFLIP") when near the player. Tune so you have to trick to win.
4. **Own records:** separate Records / Top 10 boards for Stunt Races (race time with bonus, best lap, best combo per course), never mixed with the normal boards. Ghosts per Stunt Race course. Split-screen counts on the Top 10 as for other races.
5. Race > Tracks: a "Stunt Race" option shown once a stunt vehicle is owned (the Clay Hopper is available from the start, 0.107). Vehicle choice goes through the garage, as for other races. Split-screen supported. Controller-first.
6. Trick pop-ups smaller in races than in the park, never covering the position / lap HUD, the NEXT CP line or the minimap.

### Part C — Stunt chapter in the campaign

1. A separate **Stunt chapter**, listed after Chapter 4 and open once Chapter 4 is finished. It gives the Clay Hopper for the campaign at its start. Its own events, rivals on stunt vehicles, and its own prize at the end: a stunt-vehicle **Cup edition** (like 0.111's), with the unlock message.
2. **Events (7):**
   1. a Stunt Race on Mountain Loop Forward;
   2. a trick-score challenge at the summit jump (best combo in 3 tries);
   3. the 10-minute Stunt event at the park (a medal target);
   4. a Stunt Race on Mountain Loop Reverse;
   5. "Land it clean": fly the summit jump N times without a bail;
   6. a Stunt Race on Leviathan Reverse;
   7. the final: a 2-lap Stunt Race on Leviathan Forward.
3. **Split score for Stunt chapter races** (Dan, 19:32: "you get 10 points for finishing first place and 10 points for scoring the most stunt points"):
   - each race ranks everyone twice: by **finish** (race time, no trick time bonus here) and by **stunt points** (all landed tricks and combos in the race);
   - each ranking pays 10 / 7 / 5 / 3 / 2 / 1 points by place (the championship scale); the event result is the **total** (max 20); ties go to the better finish;
   - passing = top 3 by total, winning = 1st, as in other campaign races;
   - the results screen shows both columns and the total; the HUD shows your live finish place and stunt-point place.
   Quick Stunt Races (Part B) keep the trick time bonus by default, with **Split score** offered as a setting there too.
4. Prize money and the new-player hints work as in the other chapters. Its own balancing through the results log (tagged `stunt`), same method as 0.112.

### Checks (built player, controller, test saves; the verification budget applies)
- Part A: the checks listed with each item.
- One Stunt Race on each of the four courses (AI may drive); a trick bonus applied and shown; rivals land tricks at the known jumps, with at least one bail seen; no rival stuck.
- Records on their own boards; the normal boards unchanged.
- From a test save: the Stunt chapter opening after Chapter 4, its first two events and the final, the split score on a results screen, the prize message.
- Normal races, records and the existing chapters unchanged (one normal Leviathan Forward race, rivals on normal vehicles).
- Long play checks go on the "For Dan to check" list.

### Release
Version 0.119.0-review1, local build only (2026-10-15 rule; no GitHub release). Close everything started; leave no background task running. Short results (at most about 20 lines), with "For Dan to check".

### Results (2026-10-15, Claude Code)
- Checkpoint `c936eee4`. Evidence [Docs/Report119/](Docs/Report119/) ([Shots/](Docs/Report119/Shots/), [Player/](Docs/Report119/Player/)); course files by [partA.py](Tools/Report119/py/partA.py) ([partA.txt](Docs/Report119/partA.txt)); checks [Run-Checks.ps1](Tools/Report119/Run-Checks.ps1).
- **A1** the closure at (573, −84) is out. Why rivals ran into it: it closed the Forest race route 14 m up its shallow fork off the lake's south shore, straight ahead as you come along the shore, its west end 10 m from the line where rivals swing wide. Closures within 7 m of the line, kept: Forward (718, −33), (740, −21), (488, −47); Reverse (718, −109), (149, −310), (−250, −567), (488, −47). Check: [shot](Docs/Report119/Shots/spot-A1-BUG-001-F.jpg); an AI race (3 rivals) through it and CP 18: none stuck or reset there ([aiF](Docs/Report119/Player/aiF.txt)).
- **A2** CP 18 is on Kyle's drive (500.8, −22.9) facing west: from Dan's spot 26 m, 20° left of his heading, square on; CP 19 in view from it. The line is unchanged: straight ahead of his spot is the bank's unsmoothed side by the house (4 m down in 6 m), and the 0.118 bank stays. Course id `leviathan-forward-v4`. Check: [shot](Docs/Report119/Shots/spot-A2-BUG-002-F.jpg), the AI race above.
- **A3** the fence at the Southwest Cut's other end is out. No gate lies between its two ends (CP 9 before, CP 10 after), so the in-order rule holds; it cuts the corner (about 1 s, as 0.116 measured). Check: [shot](Docs/Report119/Shots/spot-A3-BUG-003-R.jpg).
- **A4** cause: the stars sit on a dome 600 m out, the world-edge hills about 1.8 km out, so the depth-tested stars drew over them. Each star is now drawn at the far plane ([NightStars.shader](Assets/Environment/NightStars.shader)): only open sky shows it, everywhere. Check: [race, night](Docs/Report119/Shots/spot-A4-BUG-004-R-night.jpg), [Free Roam](Docs/Report119/Shots/roam-A4-roam-night-dan-spot.jpg).
- **B** Stunt Races ([StuntRace.cs](Assets/Scripts/StuntRace.cs)): Race Setup > Race type and Scoring (trick bonus / split score); Tracks shows the four courses; the garage the stunt vehicles; every rival on a Clay Hopper or Skywhip 250, turning tricks at the long flights with the player's air steering, bailing more on Easy; bonus 0.5 s per 500 pts, at most 8 s a lap; records, ghosts and best combo under `stunt-<course>`; split screen too. Check: a race on each course ([4](Docs/Report119/Player/stunt4.txt), [5](Docs/Report119/Player/stunt5.txt), [8](Docs/Report119/Player/stunt8.txt), [9](Docs/Report119/Player/stunt9.txt)): [bonus shown](Docs/Report119/Shots/stunt-hud-bonus-8.jpg), [rival trick](Docs/Report119/Shots/stunt-rival-trick-9.jpg), bails seen, [split score](Docs/Report119/Shots/stunt-results-9.jpg), normal boards unchanged.
- **C** Stunt chapter (5): opens after the Summit Final and gives the Clay Hopper; 7 events (new kinds Trick, Clean, Park); split score in its races; prize the Clay Hopper Cup; results log tag `stunt`. Check ([campaign](Docs/Report119/Player/campaign.txt)): opens on a copy of Dan's save with its notice; Opener (split score) and Summit Tricks (3 tries); the park event starts and returns to the campaign; the Final pays the [prize panel](Docs/Report119/Shots/campaign-prize-message.jpg).
- Controller only: every new row reachable, B back ([menus](Docs/Report119/Player/menus.txt)).
- **Decisions:** CP 18 moved, not the line (no drivable ground straight down); the bonus counts every banked combo, jump points too (capped); rivals trick on any flight long enough for one; the stunt vehicle choice is saved apart from the normal one; Summit Tricks 3,100 / 3,600 / 4,100 pts and Land It Clean 3 / 4 / 5 in a row are first guesses.
- **For Dan to check:**
  - The bonus scale and the stunt targets (`stunt` in the results log).
  - Mountain Loop Forward: two Skywhips tangled once at the summit run-up (1 race of 3; both reset once).
  - Land It Clean, the park event to a medal, and a split-screen Stunt Race were not driven in the checks.
  - The moon is drawn 900 m out and may show in front of the far hills when low (not changed).
  - Your save shows "CAMPAIGN UPDATED: Chapter 5: Stunt is open" once.
- **Release:** local build only (2026-10-15 rule; no GitHub release). Built fresh from `b461caca` (pushed), 0 errors ([build-release.txt](Docs/Report119/build-release.txt)); signed locally and activated by the existing updater: `Builds/Latest` runs `versions/119000`, 118000 kept as the previous ([runtime-identity.json](Docs/Report119/runtime-identity.json)). `Play-Racer.cmd` (unchanged) launched 0.119.0-review1; your settings restored byte for byte, campaign saves untouched ([play-racer-launch.json](Docs/Report119/play-racer-launch.json)). Cleanup: Builds 12.47 → 8.35 GB, C: free 214.1 GB.

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
