# Woodstock Rush
## Project Management / TODO / Astra-Codex Handoff

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

## CURRENT — none (0.120 delivered below; waiting for Dan's review)

## Previous delivery — Stunt Races and the Stunt park after Dan's first drive of 0.119: points on screen, a multiplier for every trick in one flight, bumps to trick off, tricks out of the park count again, the garage shows stunt vehicles — target 0.120.0-review1 — DELIVERED as 0.120.0-review1 (build 120000, local build), awaiting Dan's review

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

## Previous delivery — Leviathan: remove the two ROAD CLOSED barricades Dan called out, smooth Kyle's hill, guide with more gates instead of arrow boards, the Reverse lake out-and-back; Chastain Rush's updated hands and hair styles — target 0.118.0-review1 — DELIVERED as 0.118.0-review1 (build 118000, local build), awaiting Dan's review — REVIEWED BY DAN (4 reports; follow-ups in 0.119)

Dan's report `2026-10-15_10-07-01-350_18f9c1` (0.117, Leviathan Forward lap 1 and Reverse laps 1–2, Drifter Twin; in `DebugReports`). Dan (10:38): "things are mostly ok. Fix these and I want to add the updated hands and hair styles from the Chastain Rush game that was added there." **Read the 4 screenshots first.** Parts A–C apply to the Leviathan races only (Free Roam and every other course unchanged). The standing rules apply, including "World changes only where Dan pointed" and the 0.97 rule: "I would rather manage where the signs are needed than rely on it to put crap wherever."

### Part A — Remove only the two barricades Dan called out
0.117 Part A had a script close the ways that meet the course (26 Forward, 33 Reverse). Dan (10:38): "unnecessary … why would this be blocked?" Then (10:47): "At this point just leave the road closed signs in except for the ones I specifically called out. Some of those actually kind of helped, for example the one on the forward track made it more clear which way to go."
1. **Remove exactly two closures**, the fence and sign together at each:
   - **BUG-001**, Forward, X=1185.99 Z=192.54, heading 170°: the fence and ROAD CLOSED sign in the open field. The gray blank board beside it also goes if it came with 0.116 or 0.117.
   - **BUG-002**, Reverse, X=−611.04 Z=−492.14, heading 155°, near the church: the fence across the dirt way ahead.
2. **Every other 0.117 closure stays exactly as it is.** Do not re-audit them, and do not add new ones.
3. **Check:** one shot at each spot, after the change.

### Part A2 — Smooth Kyle's hill
Dan: "Can we smooth out Kyle's hill to make this easier to traverse. It is kind of awkward for me as well." This is the 0.117 link from Kyle's driveway up the grass bank behind the garage to the lake path's west side. It is Forward CP 3 and Reverse CP 14, and the 0.117 notes say it is steep for 4 m. One rival got stuck there.
   **Where:** Kyle's driveway is around X≈480–500, Z≈−17 to −21 (0.117 BUG-002 / BUG-011 / BUG-014). The bank is the 30 m race-only link that 0.117's `splice.py` built from the end of the drive to the lake path, with top-downs in `Docs/Report117/Probe/`. Forward climbs it heading roughly east; Reverse comes down it.
1. **Make the climb an even, gentle slope** from the end of Kyle's drive to the lake path:
   - no step, lip or sudden pitch change;
   - no steeper than the steepest stretch of the Street Loop road;
   - wide enough for two vehicles side by side;
   - joined smoothly at both ends.
   Dan asked for this, so it is the one terrain change allowed this round, on this spot only. Kyle's house, garage and the rest of the yard stay unchanged.
2. **Keep it race-only if possible**, as the 0.117 link is. If smoothing it needs the scene's terrain changed, change only that bank, and only in the scene that has it (MountainLoop / MountainLoopReverse). Say which in the results.
3. **Nothing floats and nothing is buried:**
   - trees, fence and garage stay on the new ground;
   - the gate at the top stands on its new ground;
   - resets on the hill put you on the slope, facing the course.
4. **Check:**
   - one first-person clip each way up/down the hill at racing speed (the Drifter Twin, as Dan drives);
   - one AI race of that stretch with the vehicle types that use it, including the fastback that got stuck in 0.117: none stuck, no resets there.

### Part B — Guide with gates, not arrow boards (Reverse lake out-and-back)
Dan's two reports:
- **BUG-003**, Reverse lap 2, X=696.87 Z=11.48, heading 120°, next CP 16: "this is still confusing, even got a sign telling me I was going the wrong way. Let's use more CP as opposed to adding more arrow signs. All the arrow signs are what makes it confusing."
- **BUG-004**, Reverse lap 2, X=700.44 Z=12.79, heading 324°, next CP 20, at the same spot on the way back: "going back isn't clear either."

1. **Arrow boards out, gates in.** On Leviathan (both directions), remove the 0.116 and 0.117 race-only arrow boards. Keep the chevrons only where the turn is a real road corner. Then add gates so that **from every gate, the next one is in view** as you leave it, and every turn the course makes off a road has a gate in or just past the turn. Use more gates rather than signs. Gates count in order, as now.
2. **The Reverse lake out-and-back** (CP 16 out, CP 20 back, X≈697–700 Z≈11–13):
   - the way out (toward CP 16) and the way back (toward CP 20) are each marked by their own gates, close enough together that you never have to guess;
   - where the out and back lines cross or share road, the only gate drawn is the next one (the 0.116 rule), so the gate you see is always the one you need.
3. **No wrong-way warning on the course line.** Find out why one showed near BUG-003 while Dan was driving the course. On Leviathan, wrong-way must be judged against Leviathan's own line, on the pass of the lap he is on (as 0.117 did for the guide), never against the Mountain Loop road nearest to him.
4. **Check:** one recording, Reverse, from CP 14 to CP 21, driven along the course. It shows each next gate in view and no wrong-way warning. Add one shot at each of Dan's two spots. Forward: one list of the gates added and the boards removed (no recording needed).
5. Gates added change the courses, so the course ids move on (`leviathan-forward-v3` / `leviathan-reverse-v4`) and records start again, as in 0.117.

### Part C — Chastain Rush's updated hands and hair styles
Dan: "I want to add the updated hands and hair styles from the Chastain Rush game that was added there." Chastain Rush is Dan's sequel project on this PC: `C:\Users\danmo\chastain_rush`, and also `chastain_rush_r7`.
1. **Find them first.** Work out which of the two folders is current: its git log and its own TODO / CLAUDE.md. Then find the change that updated the rider **hands** and the **hair styles**: commit(s), scripts, meshes or Blender scripts, materials, textures. Write a short note in the results: where they came from (folder and commit), what the new hands are, and the list of hair styles. **Only read Chastain Rush.** Never change, build or commit anything in it, and leave its open editors alone.
2. **Port into Woodstock Rush:**
   - the new **hands** replace the current first-person and rider hands (the ones on the handlebars and the wheel in Dan's screenshots), for every vehicle pose: car wheel, motorcycle bars, Dirt pose, stunt vehicles, police bike;
   - the new **hair styles** are added to the garage Rider page's **Hair** row next to the current ones (same hair colors, including Gray).
   Use Woodstock Rush's own materials and render setup, so nothing shows pink or unlit. Only the hands and hair: nothing else from Chastain Rush.
3. **Saves keep working.** Hair is saved by index, so existing styles keep their indexes and the new ones go on the end. Dan's saved rider (and player 2's) must look the same after the update, except for the new hands. The hands and hair also apply to the rivals and the trailer capture's rider look.
4. **Checks:**
   - one first-person shot each: car, motorcycle, stunt bike;
   - one chase-view shot of the rider;
   - the garage Rider page with the controller only (Hair row through every style, B back), and one shot per new hair style;
   - Dan's settings and saves loaded unchanged.
   Hands sit on the grips and wheel with no gaps or clipping at rest and at full steer.

### Part D — Release
Version 0.118.0-review1. Run the usual build and release; close everything you started; leave no background task running. Write a short result list, as the standing rules say.

### Results (2026-10-15, Claude Code)
- Checkpoint `12359cd3`. Evidence [Docs/Report118/](Docs/Report118/); course files made by [Tools/Report118/py/run-all.sh](Tools/Report118/py/run-all.sh). Course ids `leviathan-forward-v3` / `leviathan-reverse-v4` (records start again).
- **A:** only Dan's two closures removed: Forward's fence and ROAD CLOSED at (1189.5, 186.2) (the gray board was its second sign's back) and Reverse's fence at (−606.6, −506.3). Every other closure kept. Shots: `Shots/spot-A-BUG-00*.jpg`.
- **A2:** Kyle's bank is one even 25 % slope with a 3 m ease at the foot and a 9 m rounded top (a car stays on the ground to about 40 mph), 9 m wide, sides blended in ([bank.txt](Docs/Report118/bank.txt)). The steepest Street Loop stretch measured the same way is 28 %; 0.117 had 34 %. It is race only: the ground tile's vertices are set when a Leviathan race loads, so Free Roam and the other courses keep the scene's ground; the trees on the bank move with it. Checks: [first-person clips](Docs/Report118/Clips/) up and down with no hop on the bank; AI races both ways (12 rivals, fastback included) with none stuck or reset on the bank; resets on the hill land on the slope, facing the course ([Player/](Docs/Report118/Player/)).
- **B:** arrow boards removed (11 Forward, 12 Reverse). Chevrons kept only at road-to-road corners (13 / 10). Gates 26→45 Forward, 24→45 Reverse: the next gate is in view from each one, there is a gate at every turn off a road, and none stands on a jump ([gates-and-boards.md](Docs/Report118/gates-and-boards.md)).
- **B, wrong way:** not reproduced. Likely cause: on the lake out-and-back both passes share a road 1.5 m apart, and three code paths in `WrongWayGuidance` could take the other pass's station (after a reset or a big position jump, when finding the line again off the road, in a shortcut fallback). That reads as driving backwards. It now uses the racer's own pass, as the guide has since 0.117. Checks: [rec-R-cp14-cp21.mp4](Docs/Report118/Clips/rec-R-cp14-cp21.mp4) (0 warnings, 0 missed gates); ±3 m lanes and resets on both passes, 0 warnings.
- Gates built for the course now stand on the ground at the course line's height, with posts reaching the ground. Kyle's driveway gate floated 0.85 m, and Reverse CP 8 sat on the trailer ramp.
- **C:** from `chastain_rush` commit `4e4cd51` (`rider.py`). Closed hands round the grips and wheel in every pose; Mohawk, Crew cut, Spiky, Buzz and Curly added after Bald (indexes 5–9); `Rider.fbx` regenerated with 0 split errors. Checks: first-person car, moto and stunt bike, the chase view and each new style ([Shots/](Docs/Report118/Shots/)); the Rider page by controller only (Hair through every style, B back); Dan's saved look unchanged ([rider.txt](Docs/Report118/Player/rider.txt)).
- **Decisions:**
  - Chastain's remodeled Short and Medium were not taken, because Dan's rider wears Medium and must look the same.
  - Random looks still draw from the old five styles, so player 2 and the rivals look as before, now with the new hands.
  - The bank keeps 0.117's S-shaped line; the heights of the drive pad and the lake path set the 25 %.
- **Release:** local build only (2026-10-15 rule; no GitHub release). Built fresh from `4409c8a5` (pushed), 0 errors ([build-release.txt](Docs/Report118/build-release.txt)). Signed locally and activated by the existing updater: `Builds/Latest` runs `versions/118000`, with 117000 kept as the previous build. The published catalog is still 117000, so the launcher keeps 118000. `Play-Racer.cmd` (unchanged) launched 0.118.0-review1, and Dan's settings were restored byte for byte ([play-racer-launch.json](Docs/Report118/play-racer-launch.json)). Cleanup: Builds 12.47 → 8.34 GB, C: free 217.0 GB.
- **For Dan to check:**
  - The hill at speed, and the sharp turn onto the pad at its foot.
  - The 45 gates' placement.
  - Rivals still reset at the lake-shore → CR110 turn, as in 0.117; once an ATV looped there.
  - BUG-002's dirt way is still closed at its other end, 49 m on.
  - The street-to-Kyle's-driveway drop (unchanged) throws a fast car 0.8 s.
  - A hat hides the top of the new styles.

## Previous delivery — Leviathan cleanup: every turn clear, wrong roads blocked, gates right, resets onto Leviathan, shortcuts with something on them (Dan's 14 reports) — target 0.117.0-review1 — DELIVERED as 0.117.0-review1 (build 117000), awaiting Dan's review — REVIEWED BY DAN ("mostly ok"; follow-ups in 0.118)

Dan's report `2026-10-14_20-53-07-878_2d1da6` (0.116, Leviathan Forward lap 3 of 4 and Reverse lap 1, Drifter Twin; ZIP in `DebugReports`). Dan (21:17): "A lot to clean up here. It's a little less confusing but there are places that aren't clear whatsoever." **Read all 14 screenshots first.** Everything here applies to the Leviathan races only (0.115 rule: "only change for race, do not change Free Roam or any other races"), world rules as always: no terrain reshaping, nothing floating, signs one-sided and depth-tested.

### Part A — The rule this round enforces
A first-time player must always know where to go, using only what's on screen. So at **every** place where Leviathan's line meets another drivable way (road, trail, driveway, lot, the Mountain Loop's or Street Loop's own routes): **either it's the course (marked: arrow sign before, chevrons on the outside, the next gate visible) or it's blocked** for the race (barricades like Street Loop Reverse's, visible, solid, one-sided signs "ROAD CLOSED"). No third case. Make a **decision-point audit**: a table of every such junction in both directions (position, what's there, marked or blocked, before/after shot). Shortcuts are the only open alternatives and are marked as shortcuts.

### Part B — Dan's reports, Forward (lap 3)
1. **BUG-001** X=467.16 Z=44.87 heading 195°: "this sign is too early." Move the turn sign to just before the turn (about 40–60 m ahead of it at racing speed), not where it suggests turning onto the wrong way.
2. **BUG-002** X=480.28 Z=−21.22 heading 93°: "really not sure which is the intended entrance. Maybe we just make it Kyle's driveway?" **Do it Dan's way:** route Leviathan through **Kyle's driveway** as the entrance here (Kyle's house, garage and yard otherwise unchanged; the route uses the drive surface, race-only markings and gate), and block the other candidate ways in. Same in Reverse where the course passes here (BUG-011 and BUG-014 are the same area: X≈480–499, Z≈−17).
3. **BUG-003** X=603.37 Z=−110.75 heading 103°: "the gate that is here is far too low." Every Leviathan gate at normal height and width on the ground of its spot (check all gates, both directions; list any other that was wrong).
4. **BUG-004** X=740.26 Z=−119.49 heading 96°: "The main route from the forward race isn't intended to be used here, so there should be a barricade." Barricade the Mountain Loop route that Leviathan doesn't use at this junction (the screenshot shows the "Optional shortcut / Climbing Ridge Cut" sign: Climbing Ridge Cut is Leviathan's **main road**, so its sign must not say "optional shortcut" on Leviathan either).
5. **BUG-005** X=716.48 Z=−78.51 heading 194°: "this part is confusing. There should be a better indicator that you need to return as opposed to going around again. Since people would be used to the Mountain Forward track, they may not realize it." After the summit: the way back to the lake marked like a main turn (big arrow sign, chevrons, the next gate in view), and the Mountain Loop's own continuation **blocked**.
6. **BUG-006** X=727.53 Z=−118.56: "I had reset because I fell down the cliff and it took me back here to do the mountain loop again." **Resets on Leviathan put you back on Leviathan's line**, at the last point you legally passed (after the last gate passed, facing the way of the course), never on the Mountain Loop's or Street Loop's own route and never behind a gate you already passed. Test: fall off at three places on the mountain and reset.

### Part C — Dan's reports, Reverse (lap 1)
7. **BUG-007** X=−640.22 Z=114.81 heading 164°: "What gate did I miss? Didn't see one." Find the gate he was charged for: it must be visible on the road where the course goes (not hidden by 0.116's "only the next gate" rule, not off the road, not under ground). When a gate is missed, say which and where ("Missed gate 5: back 120 m, left at the lake path").
8. **BUG-008** X=−622.64 Z=−242.10 heading 132°: "this should be blocked off as it is in Street Loop Reverse." Use the same barricade as Street Loop Reverse there.
9. **BUG-009** X=508.56 Z=−481.04 heading 74° (one of 0.116's new Reverse shortcuts): "is there nothing to this shortcut? Not even a jump?" Give it something: a jump that fits the place (bank, ditch, driveway kicker) on the existing ground; if the ground allows nothing, say so and replace the shortcut with a better one.
10. **BUG-010** X=531.88 Z=−44.53 heading 39°: "that shortcut behind me should be a jump." Make it a jump (same rules: fits the place, lands on a down-slope, AI can fly it).
11. **BUG-011** X=499.33 Z=−16.80 heading 355°: "just like the other way, I have no idea where to go. The lines keep changing." Also seen in his shots: the on-screen guide stuck at **"NEXT CP 14 0 ft"** with the arrow pointing down at several different places (BUG-010, 011, 012), and "NEXT CP 20 26 ft" pointing the wrong way. Fix the next-checkpoint guide and the minimap "ahead" line on twice-driven roads (they must follow the course pass you're on, not jump between passes), and the Kyle's-driveway entrance (item 2) here.
12. **BUG-012** X=577.34 Z=55.12 heading 52°: "telling me wrong way but this was where the trail is supposed to go." No wrong-way warning on the course in either direction; test both lanes of every twice-driven stretch.
13. **BUG-013** X=820.24 Z=−190.06 heading 297°: "the road to the right should be blocked, otherwise someone will take it." Block it.
14. **BUG-014** X=480.71 Z=−17.80 heading 293°: "this path is meant to go through trees?" No course line through trees: clear trunks within the drivable width + 1 m on the course line (race only, like 0.115) or move the line onto the open path; with item 2 this becomes Kyle's driveway.

### Part D — Also seen in the screenshots
15. BUG-009's screenshot: the radio's "Artist / Song" caption draws on top of the "NEXT CP" guide. Move it so nothing overlaps the guide or other HUD (all modes).
16. Rivals: after all changes, AI-only races both directions (3 × 2 laps, mixed vehicles): all finish, ≤ 2 resets each; rivals use Kyle's driveway and the new jumps.

### Part D2 — Skipping the mountain must never pay
Dan (21:20): "Also concerned that you could just skip going up the mountain at all and not sure if that will punish you enough. Need to make sure that if someone attempts this, they will suffer a huge time penalty."
0.115 added "Leviathan's lake and mountain gates must be passed in order" (crossing a later gate or the finish doesn't count until they're passed). Prove it and make it unmistakable:
1. Try every way to skip the mountain (and the summit jump) in both directions: turning back early at J, cutting along the lake, any trail, driving over the hill, using a reset. Report each attempt.
2. Skipping must **always cost more than doing it**: the lap doesn't count until the skipped gates are passed (you must drive back), and the screen says so at once, big and clear: **"MOUNTAIN MISSED — GO BACK"**, with the guide pointing to the missed gate. If a lap could somehow end without them, it costs **+60 s per missed mountain gate** (far more than the mountain takes) and the time shows as penalized in the results and is never a record.
3. A reset never moves you past a mountain gate you haven't passed (Part B item 6).
4. Rivals unaffected.
- **Check:** each skip attempt from item 1 driven with the controller, with the time it cost compared to a normal lap's mountain section.

### Part E — Checks and release
- Drive both directions as a first-time player with the controller, following only what's on screen, and record a **full-lap screen recording** of each (Dan reviews them); the decision-point audit table; before/after shots at all 14 spots.
- Free Roam and every other course unchanged (compare as in 0.115).
- Version 0.117.0-review1; usual build and release; close everything started; no background task left running.

### Results (2026-10-15, Claude Code)

- **Safety checkpoint** `653ff5af` (this section; the untracked `Assets/_Recovery/` scenes and old `Assets/Editor/Report097Temp` / `Report098Temp` left as they were). Evidence: [Docs/Report117/](Docs/Report117/). Version **0.117.0-review1, build 117000**. No scene file changed; everything below exists in a Leviathan race only (Free Roam and the other courses untouched).
- **How it is built.** The course files are made by [Tools/Report117/py/run-all.sh](Tools/Report117/py/run-all.sh) from the 0.116 ones: `splice.py` (the driveway entrance), `blocks.py` (closed ways), `extras.py` (kickers, relabelled scene signs), `signs.py` (turn boards), `previews.py` (Tracks preview), `audit.py` (the audit list). Runtime in [LeviathanCourse.cs](Assets/Scripts/LeviathanCourse.cs). Course ids `leviathan-forward-v2` / `leviathan-reverse-v3` (records start again: the courses changed). Laps: Forward 6.94 km (was 6.82), Reverse 7.04 km (was 7.16).
- **Part A — decision-point audit.** Every other road or trail line in each scene that meets Leviathan's line and leaves it (and, on Reverse, the Street Loop Forward shortcut trails) was found by script and either is Leviathan (another pass of its own road, or one of its shortcuts: marked) or is closed for the race: a timber barricade like the Mountain Loop's own "Road closed barrier 4" (solid, in 3 m panels each on its own ground, a one-sided ROAD CLOSED sign facing the traffic that would turn in; two signs back to back where a short stretch is closed from both ends). Forward 26 closed ways, Reverse 32 plus Pine Ridge's NO ENTRY ([blocks.txt](Docs/Report117/blocks.txt)). Every turn the course makes off a road has an arrow board before it and chevrons on its outside (Forward 11, Reverse 12 arrow boards). **The table:** [audit.md](Docs/Report117/audit.md), with a before (0.116) and after shot from the built player at every turn, every closed way (plus a close-up in front of each closure) and Dan's 14 spots ([Audit/](Docs/Report117/Audit/)).
  - Left open on purpose (in the table): **Hwy 92** at the two Street Loop corners (its traffic drives through; the turns now have arrow and chevron boards); the summit landing's grass slope where a "safe return" line crosses (no road there); the house driveways along the Street Loop (dead ends, as in the Street Loop races).
- **Part B, Forward.**
  1. **BUG-001** the arrow for the turn now stands 14 m before it (it stood 50 m before, just ahead of a house driveway on the other side), seen from 40–60 m.
  2. **BUG-002 Kyle's driveway.** Both directions now leave the street at Kyle's driveway (its own drive surface), climb the grass bank behind the garage (a 30 m link planned on the scene's height grid, three trunks cleared, race only) onto the lake path's west side; the stem through the trees is closed at both ends. A gate in the driveway on the way in (Forward CP 3, Reverse CP 14). Kyle's house, garage and yard are unchanged. [Top-downs](Docs/Report117/Probe/).
  3. **BUG-003** Forward's lake-shore gate stood 1.3 m into the 0.115 gully fill (bar at eye height): built gates now stand on the top ground of their spot (that one raised 2.8 m; the others moved 0.1 m or less). All Mountain Loop gates the course reuses were already right.
  4. **BUG-004** the Mountain Loop's route south from the Y is closed; the scene's "OPTIONAL SHORTCUT / CLIMBING RIDGE CUT" sign reads "LEVIATHAN / CLIMBING RIDGE CUT" in this race (and "MOUNTAIN LOOP / OPTIONAL: CLIMBING RIDGE CUT", "SUMMIT TRAVERSE / ROAD CLOSED"; Reverse: "LEVIATHAN / DOWNHILL RIDGE CUT" for its two signs).
  5. **BUG-005** after the summit: arrow board and chevrons for the turn onto the lake shore; the Mountain Loop's road on south is closed (signs both ways). Summit Traverse (where Dan skipped CP 7) is closed at both ends.
  6. **BUG-006 resets** on Leviathan put the player on Leviathan's line between the last gate passed and the next, never past the furthest point driven (after a jump's take-off, up to its landing), facing the course, never in the pit under Downhill Ridge Cut's drop. Check ([resetF](Docs/Report117/Player/resetF.txt), [resetR](Docs/Report117/Player/resetR.txt), [resetR-pit](Docs/Report117/Player/resetR-pit.txt)): pushed off the road at 3 places on each mountain and at the Downhill Ridge Cut pit: every reset on this pass of the lap, after the last gate, before the next.
- **Part C, Reverse.**
  7. **BUG-007** every Reverse gate is on the course line (offsets 0.1–0.6 m) and drawn when it is next. The one most likely missed is CP 4, the diagonal gate at the Hwy 92 corner (7.1 m either side): cutting inside the corner passes it. A missed gate now says which and where: "Missed CP 4: 120 m back +5s" (with its name when it has one, e.g. "CP 3 (Kyle's driveway)").
  8. **BUG-008** Street Loop Reverse's own block at Pine Ridge's end (its concrete lip, the same mesh, and its NO ENTRY board) is there in Leviathan Reverse.
  9. **BUG-009 Hollow Creek Hop** has a kicker on the near bank of its creek channel (0.8 m, race only): rivals take it in 3.9–4.1 s against 5.7–6.5 s by the road (saves ~2 s), no resets on it; the pad 4.3 against 6.8 s ([clip](Docs/Report117/Clips/clip-via-Hollow-Creek-Hop.mp4)).
  10. **BUG-010 Rocky Way Cut** has a kicker at the bank's edge (0.75 m), and runs on along the lake path to where the course comes up from the driveway. **For Dan:** with the driveway entrance the main road is quicker for the rivals (13.5–14.5 s against ~17 s by the shortcut), and rivals stalled rejoining after its jump (4 of 9 did not finish), so the rivals no longer choose it; the player can: the pad 7.6 s against 11.8 s by the road (saves ~4 s; [short](Docs/Report117/Player/short.txt), [clip](Docs/Report117/Clips/clip-via-Rocky-Way-Cut.mp4)). It can be switched off in the F6 menu as before.
  11. **BUG-011 the guide.** The racer's place on the course is kept on the pass of the lap it is on (between its last gate and its next): the "NEXT CP 14 0 ft", the down arrow and the jumping course-ahead line came from Rocky Way Cut landing on the stem, where the other pass was nearest. Off the course (30 m+) the arrow points at the gate itself ("TO CP n").
  12. **BUG-012** no wrong-way warning on the course: the full laps both ways ([lapF](Docs/Report117/Player/lapF.txt), [lapR](Docs/Report117/Player/lapR.txt)) and the shortcut laps had 0 warnings, 0 NEXT CP glitches except one 150 m jump in the distance when the driver left the Southwest Cut shortcut (Forward).
  13. **BUG-013** the road on to the right (CR117 north) is closed.
  14. **BUG-014** the course no longer goes through those trees (Kyle's driveway).
- **Part D.** 15. The radio's song caption is now top centre under the notices, clear of the guide line, the race panel, the minimap, the speedometer and the debug panel ([song](Docs/Report117/Player/song.txt), [shot](Docs/Report117/Shots/song-over-race-hud.jpg)). 16. Rivals: see Checks.
- **Part D2 — skipping the mountain.** Crossing any later gate or the finish while a lake / mountain gate is still to pass never counts (the 0.115 rule), and now shows at once, big, at the top: **MOUNTAIN MISSED — GO BACK — CP n first**, with the arrow at that gate, and the guide line reads "BACK CP n" with the distance to it ([Forward](Docs/Report117/Shots/skip-8-turnback-banner.jpg), [Reverse](Docs/Report117/Shots/skip-9-turnback-banner.jpg)). Attempts ([skipF](Docs/Report117/Player/skipF.txt), [skipR](Docs/Report117/Player/skipR.txt), [skipF-place](Docs/Report117/Player/skipF-place.txt), [skipR-place](Docs/Report117/Player/skipR-place.txt)), each by the pad:
  - turning back early (Forward after the lake-shore gate, Reverse after the north-shore gate) and on along the street: the banner within 1–3 s of the first later gate; no gate counted; the lap cannot end until the missed gate is driven back to. Driving back costs more than the mountain section (73 s Forward, 87 s Reverse for the road AI) plus the way out.
  - across the hill from the lake shore to the lake's east side (Forward) / the strip (Reverse): stopped at the barricades.
  - north up the lake path's west side (Forward): the barricade, widened after the pad first went round it on the grass.
  - the reset button before the mountain: put back on the course before the next gate, never past it.
  - So the +60 s fallback is never reached: a lap cannot end without the mountain gates. Rivals are unaffected.
- **Checks (built player, test saves, windowed and minimized).**
  - **Full-lap recordings** ([Clips/lap-forward.mp4](Docs/Report117/Clips/lap-forward.mp4), [Clips/lap-reverse.mp4](Docs/Report117/Clips/lap-reverse.mp4)): driven by the rivals' road AI (the pad bot misjudges the Forward summit flight and lands on the ridge, as in 0.116): Forward 245.6 s, Reverse 266.3 s, 0 missed gates, 0 wrong-way warnings. A bot cannot "follow only what's on screen"; the audit shots show what a first-time driver sees.
  - **AI-only races, 2 laps, 3 rivals, mixed vehicles, traffic** ([levF](Docs/Report117/Player/levF.txt), [levR](Docs/Report117/Player/levR.txt)): Forward 9/9 finished, 0 missed gates, resets 1–4 (three with 4: Creek Leap's landing, the Southwest Cut's end, the CR110 turn, where 0.116's run also reset). Reverse: 8 of 9 finished, 0 missed gates, resets 0–2; the fastback stuck on the driveway bank on lap 2 and its resets kept putting it back there (did not finish). Hollow Creek Hop forced ([levRf](Docs/Report117/Player/levRf.txt)): 3/3 finished, 0 resets, the hop both laps. A/B of how the rivals' place on the course is kept ([ab-*](Docs/Report117/Player/)): the rivals keep their 0.116 rule except a place past their next gate (the other pass) is pulled back.
  - **Other courses** (scenes unchanged; Mountain Loop Forward / Reverse and Street Loop Reverse, one AI lap each): all finished, 0 missed gates, resets 0–1.
  - One check player crashed in its start-up scene before any Leviathan code ran (native, once in ~40 runs); re-run fine.
- **Release.** 0.117.0-review1, build 117000:
  - Source `64866a34` (pushed); a fresh Windows build from it (0 errors, [build-release.txt](Docs/Report117/build-release.txt)).
  - Published [game-117000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-117000) (the known draft-lookup miss, completed with `--resume-draft`): Latest, 3 assets, earlier releases kept.
  - Public download, pinned signature and startup check all pass, and every one of the 236 files matches Builds/Latest ([hosted/result.json](Docs/Report117/hosted/result.json)).
  - The production updater is at 117000 with nothing newer pending ([launcher-catalog-check.json](Docs/Report117/launcher-catalog-check.json)). `Play-Racer.cmd` (unchanged) launched `Builds/Latest/versions/117000/Racer.exe` ([play-racer-launch.json](Docs/Report117/play-racer-launch.json)), and Dan's settings.json was restored byte for byte ([settings-preserved.json](Docs/Report117/settings-preserved.json)).
  - Cleanup: Builds 12.45 GB -> 8.33 GB (4.12 GB: the build output, the zip, the check build, the public-download install); C: free 200.8 -> 206.7 GB. Kept managed 117000 and 116000 ([cleanup.json](Docs/Report117/cleanup.json)). The round's temporary editor tools were removed from Assets/Editor (copies in Tools/Report117), the scratch folders deleted, every editor and player started closed, no background task left running (two Unity editors of another project, chastain_rush, left alone).
  - Commits: `653ff5af` checkpoint, `64866a34` the round (released source); this record follows.
- **For Dan to look at:** the driveway entrance (the bank behind the garage is steep for 4 m: rivals slow there, and one of 18 rivals got stuck on it); the barricades (26 / 33) and whether any should stay open; Rocky Way Cut (player only now: keep, or switch off); the two Hwy 92 corners left open for traffic.

## QUEUED — The trailer, filmed and cut by Code — target: no game version (tools and output only) — NOT STARTED (run last: after 0.113 and 0.114 are delivered and Dan says the game looks right; Claude (chat) promotes it)

Dan (2026-10-13, 19:57): "I think I am going to add the video making back to the list. It is difficult for me to make it look good, especially with some of the camera angles suggested." This reverses 0.98's "stop filming until Dan confirms"; Dan confirms by asking. Lessons from 0.97 Part C (clips discarded): footage showed broken things (the storm drain); filming must never change the game or the world.
1. **The plan is `Docs/Trailer/shot-list.md`** (2026-10-12: 20 shots + split-screen S1–S3, about 65 s; the Project copy is the same). Shots that can't be made exactly as written: the closest that looks good, and say why.
2. **Capture** with the 0.97 offline method (`Tools/Report097/Run-Trailer.ps1` and its capture mode, development build only, never in the release menus): fixed 60 fps rendering, every frame at full quality, 3840×2160 if it renders, otherwise 2560×1440; real slow motion (more frames), never stretched; game sound recorded (engines, now with other vehicles' 0.113 sounds, sirens, radio voices, rain, thunder); in-game radio music off. HUD off except S1–S3 (HUD on, both views). Same rider look and vehicle colors throughout. The game's own scenes as they are: **no world changes for filming**; if something looks wrong on camera (a bump, a floating sign, a seam), don't hide it: list it with its position for Dan as a bug and pick another angle.
3. **Each shot** its own clip, a few seconds longer than the edit needs, three takes of anything with a jump; `Trailer/Clips/NN-name.mp4` (S1–S3 as `S1-…`). A **contact sheet** (`Trailer/contact-sheet.jpg`: three frames per clip, labeled) so Dan can review fast.
4. **First cut** in shot-list order and timing: `Trailer/WoodstockRush-trailer-v2.mp4` (game sound) and `-silent.mp4` (for music); title "Woodstock Rush" over shot 20, end card = the cover art; simple cuts and short fades, nothing flashy. No music added (Dan picks a licensed track); if Dan provides one in `Trailer/Music/`, a third version with it, mixed under the game sound.
5. Close every player/editor started; nothing committed to git except the scripts; the game version and Dan's saves untouched.
- **Deliver:** the contact sheet and the cut sent first; a list of any shot that didn't match the plan and any bug seen on camera.

## History archive

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
