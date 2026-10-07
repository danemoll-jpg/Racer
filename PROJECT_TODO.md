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
  - **Write-up:** the TODO results are a short list: what changed, the one check per item, decisions made, and anything Dan should look at. No separate VALIDATION.md unless a round asks. If a check would take more than a few minutes of play time, skip it and list it under "for Dan to check".

## CURRENT — Menu fixes from Dan's 0.91 play (controller still skips two rows, garage for vehicle choice, no A-to-cycle, a vehicle for every chapter), Forest Reverse bump, split-screen stage 2 — 0.92.0-review1 — DELIVERED, AWAITING DAN'S REVIEW

- **Authorized by Dan (2026-10-07, 10:38).** Written by Claude (chat) from his play of 0.91.0-review1 and debug session `2026-10-06_22-05-15-141_e9fd05`. That session has two reports: **BUG-001 is from 0.87 and Dan says to ignore it**; BUG-002 is on 0.91.0-review1 and is Part E. His words are quoted in each part: "I want to fold these things into the next round (I assume phase 2 of split screen)."
- **Starting point:** main at the "Record 0.91 delivery" commit (0.91.0-review1 / game-91000). This TODO edit is uncommitted and belongs in the safety checkpoint.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–F below. Order: A, B, C, D, E, then F (split-screen stage 2, own commit).**
- The Verification budget and the Controller-first rule apply. **For this round, controller checks are made in the built Windows player, not only in the editor** (see Part A).
- Dan's real campaign save keeps loading and is never reset; test on a copy.

### Results (2026-10-07, Claude Code)

- **DELIVERED:**
  - Source `a90f5f87e9be9fd2401b709e39b711e0a13b2626` pushed and verified on origin/main (Parts A–E `dfe14f8b`, Part F `a90f5f87`; revert Part F alone with `git revert a90f5f87`).
  - Fresh 0.92.0-review1 Windows build from that commit: 0 errors, 3m08s ([build-release.txt](Docs/Report092/build-release.txt)).
  - Published [game-92000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-92000) with `Tools/Publish-LauncherRelease.py` (the project's `gh`): the known draft-lookup miss, then `--resume-draft` uploaded the three assets and published. Previous releases retained.
  - All 234 Latest files match the public signed manifest; public download, signature, install and startup pass ([hosted/result.json](Docs/Report092/hosted/result.json)); the production updater activated 92000 ([launcher-catalog-check.json](Docs/Report092/launcher-catalog-check.json)).
- **Play-Racer.cmd (unchanged):** launched a responsive `Builds/Latest/versions/92000/Racer.exe` (0.92.0-review1) through the launcher, muted, settings restored byte for byte ([play-racer-launch.json](Docs/Report092/play-racer-launch.json)); Dan's `campaign-v1.json` unchanged by it (hash before / after: the game loads it without writing; he sees the "CAMPAIGN UPDATED" notice for the Pebble Coupe on his first visit to the main menu, and it is saved when he presses OK). Latest root, current 92000 and previous 91000 retained.
- **Cleanup:** Builds 10,152,089,755 → 8,009,399,030 bytes (2.1 GB recovered: the build output and game.zip); also the hosted-check install (1.7 GB), 2.1 GB of check scratch outside the project (test players, logs) and the temporary editor tools. C: free 248,950,501,376 bytes after cleanup.
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.92 (rule 12).
- **Safety checkpoint:** `ae4fd754` (this plan), pushed. Commits: `dfe14f8b` Parts A–E, `a90f5f87` Part F (own commit). Evidence: [Docs/Report092/](Docs/Report092/) ([Lists/](Docs/Report092/Lists/), [Shots/](Docs/Report092/Shots/)). Checks: `MenuNavCheck.cs` (built player, `-menuNavCheck`), `Report092Checks.cs` / `Report092Walk.cs` (editor, emulated controller); tools `Tools/Report092/`. Version 0.92.0-review1 / build 92000.
- **Incident, put right:** the first Part D code granted the prize vehicles and **saved the campaign at once when it was loaded**. The editor check runner starts on Dan's real save folder before switching to its own, so at 11:22 an editor check rewrote Dan's real `campaign-v1.json` as version 3 (Pebble Coupe added, money unchanged), which the installed 0.91 game cannot read. **Restored at once from the game's own `campaign-v1.json.bak`** (his 10:25 save, hash-identical to it; nothing else in his save folder had been written), and the code changed so **loading never writes** (the grant is saved when the player presses OK on the notice, or with the next result). Every later check run hashed his save before and after: unchanged. The overwritten copy is kept in `Temp/report092-dan-campaign-as-written-by-check.json` (local only).
- **A — Controller rows — DONE.**
  - **Reproduced the way Dan plays, in the built player:** cold launch, title, Free Roam, Start, on a copy of his save (campaign in progress), at 3840×2160 full screen with the pointer resting over the menu, the presses driven **through the connected Xbox controller's own XInput device** as well as with an added test gamepad, D-pad and left stick. The 0.91 build went down every row in drawn order on the Free Roam menu, the main menu, Race Setup and the race pause menu ([before](Docs/Report092/Lists/A-built-0.91-real-device.txt)): its links were right, so the fault Dan saw is not in them. What remained that can make a press land on the wrong row, or look as if it skipped one: (1) Unity's UI module steps once per navigation **event**, and each device bound to Navigate reports its own, so one press seen twice (two devices for one pad, an input mapper, stick and D-pad together) moved two rows; (2) **the CAMPAIGN row's own colour is almost the highlight's colour**, and a row under a resting mouse pointer lit up like the focused one, so the focus on CAMPAIGN (and next to a hovered row) was hard to see.
  - **Fix:** while a menu page is up the menu moves the focus itself: one held direction from every allowed device together, **one row per press**, repeating after 0.45 s held, along the page's links (rows as drawn); the UI module keeps A / B. **The focused row has a white frame and a teal marker**; the pointer highlights (and selects) a row only while the mouse is being used, so only one row is ever lit. (`RaceMenus.Navigation.cs`)
  - **The structural check tests order:** `RaceMenus.NavigationOrderFault()`: from the first drawn row, down (rows) times must visit the rows in drawn order and come back to the top; logged as `MENU NAVIGATION … order` in the editor / development builds, and run **in the built player** by `MenuNavCheck` on every page it shows.
  - **Check (built 0.92 test player, real-device path, 4K):** Free Roam menu, main menu, Race Setup, Settings (all four tabs), Garage, Split Screen setup, Records, a campaign event page, the vehicle choice (garage view) and the race pause menu, each down from the top with the D-pad (the menus also with the stick): **every row in drawn order, CAMPAIGN and SPLIT SCREEN included, the structural order check clean on every page: ALL PASS**; the one-time campaign notice came up on the way and was closed with A ([list](Docs/Report092/Lists/A-built-0.92-walk.txt), frames per press in the evidence run).
- **B — Vehicles chosen in the garage view — DONE.**
  - **Campaign:** the event / championship page's vehicle row: **A opens the garage view** (the 0.76 turning preview, name and class, the stat bars with campaign upgrades in gold, ‹ Colour ›) over the owned vehicles; those the event does not allow are listed after the others as a dark silhouette with the reason and cannot be used; **Shop… is one row away** and comes back to it; USE THIS VEHICLE returns to the event page, which shows **the chosen vehicle as a small turning preview with its name** (and the final's prize beside it). Left / right on the row still steps through the allowed ones.
  - **Defaults to the last driven:** the campaign save (version 3) keeps the vehicles last driven in the campaign; each event and championship page opens on the most recent one the event allows, else (before any is recorded, as on Dan's save) the Race vehicle if allowed, else the first allowed.
  - **Split-screen:** VEHICLES AND COLOURS… opens **the players' garages, side by side**: each half has the turning preview, name and class, the stock stat bars and ‹ Colour ›, worked by that player's own device at once (left / right vehicle, up / down colour, A ready, player 2's B takes its ready back); with the AI as player 2, player 1 chooses both halves in turn. The setup screen shows both vehicles as small previews. Rider look stays as set in the main garage.
  - **Checks (controller):** event page › garage view › next vehicle › Shop and back › USE › START EVENT (ran in that vehicle) › left › another event opened on it: PASS; split-screen garages with the AI as player 2 and with two controllers at once (both changed together, ready / unready, back to the setup): PASS ([lists](Docs/Report092/Lists/), shots `B-*.jpg`).
- **C — One way to choose — DONE.** Every value row is `‹ value ›`, changed with left / right (D-pad, stick, A / D and the arrow keys, or the arrows clicked), **held to repeat**; **A never changes a value** (it opens the list where a row has one: the campaign vehicle row opens the garage view). The footer shows the focused row's controls (‹ › Change, A Choose… or Select, B Back).
  - **Converted (were A to cycle):** Race Setup Laps, Mode, Difficulty, Traffic, Time of Day, Weather; Free Roam Weather; Opponents slots; Garage vehicle, Model and a new **Colour** row (the swatches now only show the colours; the mouse can still click one); Rider page Model; Settings: Estimate AI, Unlock everything, Hints, Split screen layout, VSync, Lightning flashes, Scenery, Music channel, Library source; Records Ghost (two places); pause / Free Roam menu Camera view; Activities activity; Trailer page (on / off, slow motion, HUD, panel, guides, time of day, clock running, weather, moon); split-screen Player 2: AI driver (its vehicle and colour rows replaced by the garages). Steps that already used left / right lost their A step (garage rider rows, Shop vehicle, Records track / vehicle / site, playlist entry laps, volumes, frame cap, split device / course / laps / layout).
  - **Check (controller, every value row of each screen: A, then right, then left):** Race Setup, Opponents, Garage, Rider, Settings (4 tabs), campaign event page and vehicle choice, Shop, Split Screen setup, Records (5 tabs), playlist entry editor: **53 value rows, A changed none** (the campaign vehicle row's A opened its garage view, as intended; the check first counted that as a change, its own bug, fixed); right changed each value ([list](Docs/Report092/Lists/C-value-rows.txt)).
- **D — Every chapter final awards a vehicle — DONE.**
  - Prizes: chapter 1 Needle 600, **chapter 2 Pebble Coupe**, chapter 3 Ridge Scrambler, chapter 4 Highball Fastback, **awarded for passing the final (top three)** (0.90 gave them for 1st only; "passing chapter 2's final awards the Pebble Coupe"). The campaign screen shows the highlighted final's prize beside the map (a silhouette marked PRIZE until won, then the vehicle), the event page shows a PRIZE card, the results show a won prize revealed.
  - **Grand Championship:** also the **champion's paint** (metallic gold with a white "1" roundel on each side), on any owned vehicle via the garage Colour row once won (and in split-screen).
  - **Money:** the Pebble Coupe left the Shop; **the Forest Final's first-win bonus $4,000 → $1,400** (the prize replaces most of it). Four to buy: Sundown Roadster $7,000, Skyfin Cruiser $8,000, Longroof GT $10,000, Drifter Twin $16,000 ($41,000 together), plus upgrades (a fully upgraded Street Classic $8,800, Needle 600 $33,600): money still matters. Winning everything now pays $115,150 (was $117,750), a typical run about $58,400 (was about $61,000). Prices otherwise unchanged.
  - **Retroactive:** on load, a final already passed grants its prize, and a vehicle bought before it became a prize is refunded (kept); said once ("CAMPAIGN UPDATED", one OK), then saved.
  - **Checks:** on a copy of Dan's save: Pebble Coupe granted, money unchanged ($22,630, he never bought it), the file untouched until OK, version 3 after; prizes shown for all four finals; 2nd in the Forest Final on a fresh campaign awards the Pebble Coupe; a 0.91 save that bought it: $1,000 → $7,000, kept; the champion's paint on a car and a motorcycle (roundels on vehicle and garage preview), removed by another colour: all PASS.
- **E — Forest Loop Reverse bump (BUG-002) — DONE.**
  - **Found:** riding the main from 1400 to 1580 m on the Needle 600 at 30 m/s on three lines, the bike left the ground for **1.06–1.34 s from about 1512 m** (176, 49, 289), some 20 m past where Dan stopped. The surface scan (every 0.25 m across, 0.5 m along) shows why: **at 1508–1510 m the ground mesh steps up about 24 cm across the whole road** (twice the approach gradient for 2 m; gradient 0.113 before, 0.080 after) in `Ground_480_480` (`ForestLoopReverse-baseline-protected-Ground_480_480.asset`, this scene only, unchanged since 2026-09-20).
  - **Fix:** the road band of that chunk re-shaped between 1500 and 1518 m as one smooth curve joining the surface before and after with their own slopes, ±8 m, fading to the untouched ground by ±11 m (346 vertices; up to 9.7 cm raised, 27.1 cm lowered); normals recomputed only around them; the scene file unchanged. Nothing grounded stands in the band (trees and ferns 12 m or more off the centre, the cave sign before 1495 m).
  - **Check:** after, the same three lines at 30 m/s and the centre at 38 m/s: **no flight from 1512 m** (longest air after the lake jump 0.18 s at 38 m/s, near 1545 m), no resets, min up 0.97–0.99; the authored lake jump unchanged (1.36 s from 1404 m); one lap with three AI (Needle 600, Longroof GT, Trail Four, Ridge Scrambler): all through, no missed gates, no resets. The whole main (0–2103 m) scanned the same way: every other full-width kink is an authored jump (a crest then its landing 7–8 m on: 493, 716/724, 1023/1030, 1404/1411, 1685/1693 m); no other stray step ([lists](Docs/Report092/Lists/)).
- **F — Split-screen stage 2 — DONE** (own commit).
  - **Setup:** AI rivals None–4, their difficulty (Easy / Normal / Hard) and vehicles (Mixed: each once first; Random), Time of day (Dawn / Day / Dusk / Night), Weather (Clear / Rain / Snow), Traffic, as well as laps, course (with the reverse variants) and layout. Kept for rematches and "Change setup".
  - **Race:** the rivals race both players in one field: positions for everyone in both HUDs, rivals as small white dots on both minimaps. **Once both players have finished** the rivals still running get the game's finish estimate (as single-player's Complete Race), and the 90 s cut-off now counts from the first **player** home (a rival finishing first no longer starts it).
  - **Conditions in both views:** each view has its own falling rain / snow, stars, moon and dawn mist, on its own layer hidden from the other view (so none shows doubled or in one half); one cloud layer centred between the players; one lightning flash for both, one thunder; headlamps on every vehicle; wet / snow looks, ice and the Snow scenes (which now play when either player is near) are world-wide. Traffic recycles out of both players' sight and reach.
  - **Results:** every finisher including the AI (estimated ones marked), both players highlighted with their best laps; still nothing recorded (Top 10, ghosts, activities, acorns, campaign); everything unlocked and stock.
  - **Lowered in split-screen only:** falling rain / snow 70 % as dense; shadow distance 60 % (built players); the 0.90 far clip 1,400 m and LOD bias × 0.7 stay. Single-player rendering unchanged.
  - **Frame time** (test player like the release, GTX 1660 Ti, 3840×2160, Forest Loop Forward, Night / Snow, 4 rivals, traffic, both cars driven by the AI, 40 s per layout): top / bottom **median 14.0 ms (71 fps), 95th percentile 17.3 ms (58 fps)**; left / right median 10.8 ms (92 fps), 95th 13.9 ms (72 fps). The median meets 60 fps in both; **top / bottom dips just under 60 fps at its 95th percentile**. Runs vary more than each lowering step gained (one run measured 91 / 79 fps top / bottom), so tuning stopped there (rule 12) ([list](Docs/Report092/Lists/F-frame-times.txt), [shot](Docs/Report092/Shots/F-bench-Night-Snow-top-bottom-4K.jpg)).
  - **Checks (player 2 the AI driver):** Street Loop Forward Day / Clear, 4 rivals, traffic on, top / bottom; Forest Loop Forward Night / Snow, 2 rivals, traffic, left / right; Mountain Loop Forward Dusk / Rain, 1 rival, top / bottom: each racing with the full field and the conditions, each view's own falling weather (rain 3,150 / snow 5,000 particles per view), lamps on at night, pause menu with the controller, results with everyone, REMATCH (same field and conditions), Change setup (every choice kept): all PASS ([list](Docs/Report092/Lists/F-split-races-after-fix.txt)). Found and fixed by them: player 2 (AI) was cut off as DNF by a rival's finish (the cut-off rule above). Then one single-player race (Street Loop Forward, 3 AI: all four finished, results; the race-AI pilot standing in for the player missed one gate) and one campaign event (started from its page in the chosen vehicle): fine.
  - **Item 10 (added by Dan during the round): celebrations.** The race winner (a player or an AI rival; with no rivals and player 2 the AI driver, whichever of the two finishes first) raises both fists in split-screen as in single-player. **A player who wins gets the 2.5 s winner shot in their own half** before their finished panel (that half's HUD waits for it), the other half untouched and still racing; an AI winner celebrates on the road and no camera is taken from anyone. Each player waves a fist on their own controller's LB (keyboard player: F); AI drivers still shake a fist when rammed (unchanged). Check: player 1 won (Needle 600 against the AI driver held back 25 s): fists, the shot in player 1's half, player 2's half still racing: PASS; the AI driver won while player 1 waited: fists on the road, no camera taken from player 1: PASS; LB waved player 1's fist: PASS ([shots](Docs/Report092/Shots/F-celebration-player1-wins.jpg), [AI](Docs/Report092/Shots/F-celebration-ai-wins.jpg)); the Night / Snow race run again after the change: PASS ([list](Docs/Report092/Lists/F-celebration-and-night-snow.txt)).
- **Decisions:** prizes for passing a final (not only winning); the champion's paint once the Grand Championship is won (also offered while Testing is on); "loading never writes" kept (the grant is saved on OK); value rows' A does nothing unless the row has a list; the garage colour is a row (swatches display only); split-screen shadow lowering only in built players.
- **For Dan to check:** the menus with his own controller (the one-row-per-press step and the focus frame); the garage pickers' feel; the prize change (top three wins a final's vehicle) and the new payout; the Forest Reverse stretch at 1.5 km on a bike; split-screen with rivals at night / in snow on his screen; the hop just after the lake jump's landing (1452–1466 m, 0.3–0.6 s) is part of that authored jump and was left alone. One worst-case bench run ended while reloading the course for its second layout with no error or crash report; the next two runs completed.

### Part A — The controller still skips CAMPAIGN and SPLIT SCREEN

Dan (on 0.91): "Scrolling through the menu still skips both online and campaign unless you keep scrolling down and around. This needs to be fixed." ("Online" here is the SPLIT SCREEN row; there is no online mode.)

0.91 reported this fixed (up / down follow the rows as drawn; a structural check; a controller-only walk). Dan still gets the old behaviour in the released build: going down from the top passes over the two rows, and they are reached only by carrying on round past the bottom. So the 0.91 fix or its check did not cover what he does.

1. **Reproduce it the way Dan plays: in the built player** (`Builds/Latest`), from a cold launch, with a real or XInput-emulated controller, using **both the D-pad and the left stick**, starting from whatever row is focused when the main menu first appears, on a copy of his save (campaign in progress, so the status line is present). Also from the pause menu and the Free Roam pause menu. Do not accept an editor-only result.
2. Find why the build differs from 0.91's check (for example: the explicit links are replaced by Unity's automatic navigation at runtime or after the page is rebuilt; the first-focused row is set before the rows are reordered; stick navigation takes a different path from the D-pad; the two-line CAMPAIGN row or the status text is a separate selectable; rows are re-sorted after the links are made; a different code path builds the menu on first show than on return). Fix the cause.
3. Required behaviour, main menu and every other page: pressing down from any row goes to the row drawn directly below it, up to the row directly above, with wrap only at the ends; the first press of down from the top row lands on the second drawn row. The focused row is always visibly highlighted.
4. Make the 0.91 structural check test **order**, not only reachability: for each page, the sequence produced by pressing down repeatedly from the top must equal the rows in drawn order. Run it in the built player through the check runner.
5. Check: a short screen recording or frame sequence from the built player showing down, down, down… from the top of the main menu with the highlight on each row in turn, CAMPAIGN and SPLIT SCREEN included.

### Part B — Choose vehicles in the garage, not from a line of text

Dan: "Both the splitscreen and the campaign should actually take you to the garage so you can see the vehicle and its stats. Just words are kind of lame."

1. **Campaign:** on an event or championship page, choosing the vehicle opens the garage view (the 0.76 rotating preview, the name, class, the stat bars with campaign upgrades shown in their second tone, colour) limited to the vehicles the player owns that fit the event; ineligible owned ones appear dimmed with the reason; locked ones do not appear here. Confirm returns to the event page with that vehicle shown as a small preview and name. The Shop is one press away from that garage view.
2. **Split-screen:** each player picks in a garage view of their own: the rotating preview, stat bars (stock), colour, every vehicle unlocked. On the setup screen each player's slot shows their chosen vehicle's preview, not just its name. With two human players both can be choosing at once, each in their own half of the screen with their own device; with the AI as Player 2, Player 1 also picks the AI's vehicle.
3. Reuse the existing garage code and layout; do not build a second garage. Rider look stays as set in the main garage.
4. **Campaign defaults to the last vehicle driven (Dan, 10:43): "campaign should default to the last driven vehicle."** Each event and championship page opens with the vehicle the player last drove in the campaign already selected, saved in the campaign save. If that vehicle is not allowed in this event (class rule) or is no longer owned, use the most recently driven one that is allowed, else the first eligible. The player only opens the garage view when they want to change it.
5. Check with a controller: pick a vehicle for a campaign event and for both split-screen slots; start a second event and see the same vehicle preselected.

### Part C — One way to choose everywhere: scroll, never press A to cycle

Dan: "There are some selection screens where you click through using the A button instead of scrolling through. Lets make this consistent: scroll through and not clicking A to cycle through options."

1. **Rule for every menu in the game:** up / down moves between rows. A row that holds a value (a vehicle, colour, course, laps, difficulty, weather, time of day, device, split direction, a setting, On / Off) shows it as `‹ value ›` and is changed with **left / right** (D-pad or stick; A / D and arrows on the keyboard; clicking the arrows with the mouse). **A never cycles a value.** A activates buttons and opens sub-pages only; B goes back.
2. Where a row has many options (courses, vehicles, playlists), A may open a list to scroll through and pick from; it must not step to the next option.
3. Find every row that currently changes on A (0.91's own walk names "Unlock everything", "Hints toggled with A", colour and model rows, split-screen rows, race setup rows) and convert them; list what was converted. Hold-to-repeat on left / right for long lists. A short hint line at the bottom of menus shows the controls for the focused row ("‹ › change · A select · B back").
4. Check with a controller: one pass through Race Setup, Garage, Settings (all tabs), Campaign, Shop, Split Screen setup, the playlist editor and the Records screen, confirming no row changes on A.

### Part D — Every chapter final awards a vehicle

Dan: "I feel like the end of every chapter should unlock a vehicle (but maybe that is the championship that does this?)"

Today: chapter 1's final awards the Needle 600, chapter 2's a money bonus, chapter 3's the Ridge Scrambler, chapter 4's the Highball Fastback; championships pay money.

1. **Every chapter final awards a vehicle:** chapter 1 Needle 600; **chapter 2 the Pebble Coupe** (replacing most of the money bonus; keep a small one); chapter 3 Ridge Scrambler; chapter 4 Highball Fastback. The event page and the campaign screen show the prize as a silhouette with "Prize" before it is won, and the win shows the vehicle revealed.
2. **Championships stay the big money**, and the **Woodstock Grand Championship** also awards a one-off champion's paint scheme (gold with a number roundel) usable on any owned vehicle. That leaves four vehicles to buy (Longroof GT, Sundown Roadster, Skyfin Cruiser, Drifter Twin), so money still matters for vehicles as well as upgrades. Re-check prices and payouts so that holds; say what changed.
3. **Retroactive:** on loading a save, grant any prize whose final is already passed and say so once. If Dan had already bought a vehicle that is now a prize, refund its price.
4. Check on a copy of Dan's save: prizes shown for all four finals; passing chapter 2's final awards the Pebble Coupe.

### Part E — Forest Loop Reverse: a stray bump that flips bikes (BUG-002)

"somewhere in this area there is a stray bump that has made me flip more than once" at about (157.4, 46.3, 277.0), `ForestLoopReverse`, race, lap 1, next checkpoint 4, main progress 1489 m, Needle 600, facing 95°. The screenshot is taken stopped on the dirt main with a rock cutting and cave mouth to the right. He does not know the exact spot.

1. Search the main's driving surface from about 80 m before to 80 m after that point for what throws a bike: tilted sliver triangles between near-duplicate vertices (the 0.83 Mountain cause), a mesh or collider edge standing above the surface, a seam between two ground pieces, a buried sheet poking through (the 0.85 cause), a root or prop collider. Ride it on a motorcycle at race speed on several lines across the width and log vertical jolts to find it.
2. Fix it so the surface is continuous. Section 5A: keep the race line, gates, jumps and the nearby cave as they are; change only what is needed. If the same fault is found elsewhere on this course by the same scan, fix those too and list them.
3. Check: the motorcycle through the stretch at race speed on three lines, before / after jolts; one lap with AI; collider comparison.

### Part F — Split-screen stage 2

Dan: "(I assume phase 2 of split screen)". Stage 1 (0.90, with 0.91's controller join) is two players, one race, Day / Clear, no rivals, chase camera, nothing recorded. Stage 2 makes it a full race night. Stage 3 (later) is Free Roam for two, per-player camera views.

1. **AI rivals:** the setup screen gets Rivals: 0–4 and their difficulty and vehicle mix, as in Race Setup. Both humans and the AI share one race with correct positions for everyone in both HUDs and on both minimaps.
2. **Conditions:** time of day (Dawn / Day / Dusk / Night) and weather (Clear / Rain / Snow) selectable, each working correctly in both views at once: sky, lighting, headlights for every vehicle, rain and snow following each camera, wet and snow looks, ice on water, lightning and thunder (one thunder, both views flash), the Snow scenes. Anything that was built around one camera gets a two-view version; nothing shows in only one half or doubled.
3. **Traffic:** on / off, as in Race Setup.
4. **Vehicle choice in the garage** for each player (Part B), including colour.
5. **Race options:** laps, and the course's reverse variants as now; a rematch keeps everything; "change setup" returns with choices kept.
6. **Results:** all finishers including AI, with each human highlighted; best lap for each human.
7. **Still not recorded:** no Top 10, ghosts, campaign money or acorns from split-screen. Everything unlocked, vehicles stock.
8. **Performance:** two views with weather, night lighting, traffic and rivals is the heaviest thing the game does. Target 60 fps or better at 3840×2160 on the GTX 1660 Ti in the worst case (Night / Snow, traffic on, 4 rivals, the heaviest course view). Lower split-screen-only detail as needed (draw and shadow distance, particle counts, ground detail, traffic count) and say what was lowered. Single-player rendering must not change. Report the measured worst case.
9. **Checks (may exceed the budget a little; core systems):** with Player 2 as the AI driver: one race Day / Clear with 4 rivals and traffic; one Night / Snow; one Dusk / Rain; both split directions; pause, rematch, change setup; frame time in the worst case; then one single-player race and one campaign event to show nothing regressed. All menus by controller. Shots of both layouts in Night / Snow. **Own commit**, with the revert command.
10. **Winner's celebration and fist wave (added by Dan, 2026-10-07):** Dan: "split screen didn't have the celebration." Stage 1 switched the gestures off; switch them on. The race winner (a human or an AI rival) raises both fists as in single-player. When a human wins, that player's half shows the 2.5 s winner shot (front three-quarter, head and hair visible) before their finished panel; the other half is untouched and keeps racing. When an AI or the other human wins, no camera is taken from anyone still racing; the winner simply celebrates on the road, and repeats it in a human's half when that human finishes only if they are the winner. With no rivals and Player 2 as the AI driver, whoever finishes first of the two celebrates. Each human also gets the fist wave on their own controller's Left shoulder button (keyboard player: F), and AI drivers shake a fist when rammed, as in single-player. Check: one race won by player 1 and one won by the AI driver, a shot of each.

### Verification

Light, per the Verification budget and the Controller-first rule, except where a part says otherwise. Compile, launch, release steps. Results as a short list, with "for Dan to check".

## Previous delivery — Controller can't reach Campaign or Split Screen; split-screen controller join; campaign jump never scores — 0.91.0-review1 — DELIVERED, REVIEWED BY DAN (the controller still skips two main-menu rows in the built game; follow-ups in 0.92)

### Results (2026-10-07, Claude Code)

- **DELIVERED:**
  - Source `afc470c66ead0eacada0b15ad7f03f264b676bf1` pushed and verified on origin/main.
  - Fresh 0.91.0-review1 Windows build from that commit: 0 errors, 4m45s ([build-release.txt](Docs/Report091/build-release.txt)).
  - Published [game-91000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-91000) with `Tools/Publish-LauncherRelease.py` (the project's `gh`): three assets uploaded and published in one pass. Previous releases retained.
  - All 234 Latest files match the public signed manifest; public download, signature, install and startup pass ([hosted/result.json](Docs/Report091/hosted/result.json)); the production updater activated 91000 ([launcher-catalog-check.json](Docs/Report091/launcher-catalog-check.json)).
- **Play-Racer.cmd (unchanged):** launched a responsive `Builds/Latest/versions/91000/Racer.exe` (0.91.0-review1) through the launcher, muted, settings restored byte for byte ([play-racer-launch.json](Docs/Report091/play-racer-launch.json)). The first attempt took longer than the script's 50 s to show its window (the game did start, from 91000); the second passed. Latest root, current 91000 and previous 90000 retained (89000 already retired by the updater).
- **Dan's save:** while this round ran, Dan played on (0.90): his real `campaign-v1.json` now has The Opening Jump passed (gold, 52.6 m), Rain in the Pines, The Long Way Back and the Forest Final: chapter 3, $22,630. This round only ever read it (the checks used a copy taken at the start); his result stays as recorded.
- **Cleanup:** Builds 10,151,860,021 → 8,009,238,152 bytes (2.1 GB recovered: the build output and game.zip); also the hosted-check install (1.7 GB), 45 MB of check scratch and the temporary editor tools. C: free 251,640,315,904 bytes after cleanup.
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.91 (rule 12).

- **Safety checkpoint:** `8add3a0d` (this plan), pushed. Evidence: [Docs/Report091/](Docs/Report091/) (Lists/, Shots/). Checks: `Report091Checks.cs`, `Report091Walk.cs` (editor only, emulated controller = an InputSystem test gamepad; menus with D-pad / A / B / LB / RB / Start, driving with stick and triggers). Version 0.91.0-review1 / build 91000.
- **A — Controller reaches every row — DONE.**
  - **Cause:** up / down linked the rows by their row number, not by where they are drawn. 0.90 drew CAMPAIGN (row 10) at the top and SPLIT SCREEN (row 11) under FREE ROAM, so the D-pad went RACE › FREE ROAM › GARAGE … and the two rows were reached only after QUIT / TRAILER, out of order (the pause menu's Camera view row had the same fault). **Fix:** up / down now follow the rows as drawn, top to bottom (left to right inside a row group), with wrap-around, on every page (`RaceMenus.cs`).
  - **Structural check:** every time a menu page is shown in the editor or a development build, `RaceMenus.UnreachableRows()` follows each row's up / down / left / right links from the first row and logs `MENU NAVIGATION … not reachable` for any visible usable row it cannot reach; the 0.91 check runner fails on any such log (`navsummary91`).
  - **Found by the walk and fixed:** B in the Shop opened from the campaign left the Garage on its Shop page, so GARAGE from the main menu then opened the Shop; B in the Shop is now its Back row.
  - **Walked with the controller only** (lists `Lists/walk-*.txt`): main menu (normal and Free Roam's, with RESUME DRIVING / Trailer / Camera view), campaign screen (actions, chapters, events, championship rows), event page (vehicle row with left / right, START EVENT), championship page (start; in progress: Resume, standings, Restart, Abandon and their confirmations), Shop (vehicle, buy, upgrades, upgrade confirmation), New Campaign confirmation, Continue, campaign race pause menu, championship race pause menu, a championship round's result and standings › NEXT RACE, a jump event's results › CONTINUE, welcome panel › START THE CAMPAIGN › the controls card (dismissed with A), Settings › Gameplay (Unlock everything, Hints toggled with A, Show hints again, Split screen) with RB / LB tabs, Garage (vehicles, colours with left / right, Model, Rider, Shop, Done), split-screen setup, pause and results. Every row reached in drawn order, focus never lost, B one level back each time; the structural check clean on every page shown.
- **B — Split-screen: a controller can be Player 1 — DONE.**
  - **Cause:** Dan could only open Split Screen with the mouse (Part A), and the screen made the device that opened it Player 1 for the rest of the session, so Player 1 was the keyboard with no way to change it.
  - Whoever opens it from the main menu is Player 1 on the device they used (a controller, or the keyboard / mouse); "Change setup" from the results keeps the players. Each player row shows its device (a glyph with the controller's number, or KB) and **left / right on it changes the device** (the keyboard and every connected controller, never the other player's; Player 2 also cycles through the AI driver). **A or Start on a controller no player holds, or Enter on the keyboard, joins it** to the first free slot (Player 2 when it is the AI). Player 2 with no device is the AI driver, so one player can start at once. Swap and the AI switch stay. Two slots never hold the same device.
  - The setup screen answers only Player 1's device (Player 2's Back is ignored); Player 2's own device changes only Player 2's vehicle (left / right) and colour (up / down). **The pause menu and the results now answer either player's device** (0.90 allowed only the device that paused; this round's spec says either player); the pause line still names who paused. In the race each vehicle reads only its own device (unchanged from 0.90, checked).
  - Also fixed: the device restriction's new signature threw when cleared (`RestrictMenuDevices(null)`), found by the check.
- **C — Campaign jumps — DONE.**
  - **Confirmed first** (Dan's save copy, controller held flat out, before any change): Street Classic 123.6 m, Trail Four 150.0 m, Needle 600 153.0 m, all landing upright and not wiped out, all thrown away: vertical impact 23.8–25.5 m/s against the 18 m/s limit (the Needle also by a 31 m/s side contact). Every attempt then waited out the 40 s with "Jump not scored / unstable, wet or hard landing".
  - **A jump counts if the vehicle survives it** (campaign events and Free Roam alike, `ArcadeActivities.cs`): it takes off from the site going its way, comes down on its wheels upright, is not wiped out and not in the water. The 18 m/s impact limit and the 21 m/s / steep-contact rejections are gone for ordinary jumps; the two summit flights keep their own rules (steep contact, level settling, 0.75 s).
  - **Found and fixed:** at moderate speeds a hop off the ramp's lip (under 0.25 s in the air) was taken as the jump ("too short") and the real flight right after it was never measured. A hop that takes off again before settling now starts the flight; a hop alone is silent and does not use up the take-off.
  - **Why, every time:** "SCORED: <distance> / <MEDAL>", or "NOT SCORED: Landed on your side / Landed upside down / Wiped out on landing / Landed in the water / Too short to count / Left the course / Not level on the landing (summit) / Hit a wall or a steep bank (summit)", and "Took off outside the marked area" for a real flight beside the event's (or Free Roam attempt's) jump.
  - **Several attempts:** a jump event has no laps and runs to its time limit: the best scored jump counts, a failed one never ends it. **Reset (Y / R) puts the vehicle back at the run-up**; driving round works too. Pause menu **END EVENT** ends it early keeping the best (asks first when there is none yet). The HUD shows the event, its three targets, the best so far with its medal, the seconds left and the reset key. Event page text says so.
  - **Targets re-set** ([jump-targets.txt](Docs/Report091/Lists/jump-targets.txt), controller runs): The Opening Jump (Trail Four / Needle / Street Classic): 18 m/s 24–25 m, 22 m/s 31–39 m, 26 m/s 38–41 m, 30 m/s 50–55 m, flat out 124 / 150 / 153 m → **bronze 30 m, silver 60 m, gold 110 m** (was 25 / 35 / 44), time limit **120 s** (was 40). Summit Homeward Flight: 30 m/s ≈ 94 m, 36 m/s 114–122 m, flat out Street Classic 150 m, Trail Four 215 m, Ridge Scrambler 215 m, **Needle 600 239 m (now lands clean)** → **90 / 140 / 200 m** (was 120 / 170 / 210), **150 s** (was 50).
  - **Free Roam jump targets unchanged:** flat out, the Trickum pavement jump (110 m, impact 15.9 m/s) and High Ridge Drop (59 m, 12.1 m/s) were inside the old limits already, and the summit's rules did not change, so the rule change does not move them. Records kept.
  - **Other event kinds (C.5):** the speed trap (200 m standing start, 45 s, its direction), the smash event and the time trials' flying-lap start have no such trap: Dan passed the Hwy 92 trap (gold), Against the Clock (gold) and Cave Run (bronze) by playing; the smash scores each distinct prop with no landing rule (0.90 scripted run 9 props). Nothing changed there.
- **Checks** (muted editor play mode, isolated save, Dan's `campaign-v1.json` only ever copied): all PASS — main menu / settings / garage / campaign screens walk; welcome › controls card; Free Roam menu (12 rows in order, CAMPAIGN and SPLIT SCREEN open with A); split-screen: opened with the controller (Player 1 = controller, Player 2 = AI by default), Laps with the D-pad, a lap driven with the controller (2:47.5, no missed gates), results, REMATCH, pause, END RACE; then the keyboard joined with Enter, its arrow changed only Player 2's vehicle, its Esc did not leave the setup, Player 2's row to the AI and back, in the race the trigger moved only Player 1 and W only Player 2, paused from the keyboard and worked by the controller; The Opening Jump flat out in each of Dan's three vehicles (one started through the menus with the controller): scored gold 123.6 / 149.6 / 153.0 m, the event went on with the HUD line, Pause › END EVENT with A: passed, Rain in the Pines open; the championship round finished, NEXT RACE; reset to the run-up; structural check clean on every page shown.
- **For Dan to check:** the new jump targets and time limits (rule 12); the jump HUD line and the NOT SCORED reasons in play; split-screen with two real controllers (the second was emulated); that driving round the Forest course in a jump event is fine (reset is the quick way back).

- **Authorized by Dan (2026-10-07, 08:11).** Written by Claude (chat) from his first play of 0.90.0-review1. His words are quoted in each part. These block him from playing the campaign and split-screen, so this is a short fix round.
- **Starting point:** main at the "Record 0.90 delivery" commit (0.90.0-review1 / game-90000). This TODO edit is uncommitted and belongs in the safety checkpoint.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–C below.** The Verification budget applies. **Every menu check in this round is done with a controller only, no mouse and no keyboard**, because that is how Dan plays and that is what 0.89 and 0.90 missed.
- Dan's real campaign save (`campaign-v1.json`, version 2: chapter 2, $15,770, owns Street Classic, Trail Four, Needle 600) must keep loading. Never reset or overwrite it; test on a copy.

### Part A — The controller skips CAMPAIGN and SPLIT SCREEN on the main menu

Dan: "Splitscreen can only be selected by mouse not controller (the controller just would skip that menu item)." "Also can't select campaign with the controller."

1. On the main menu, D-pad / stick navigation passes over the CAMPAIGN and SPLIT SCREEN rows; only the mouse can pick them. Find why (likely the 0.90 status line or the new rows being built outside the navigable row list, a non-selectable element taking the slot, or explicit navigation links that skip them) and fix it so every main-menu row is reached in order, top to bottom, with wrap-around as the other menus do, and A selects it.
2. Same check on the pause menu and the Free Roam pause menu (where CAMPAIGN also appears).
3. Then walk **every screen added in 0.89 and 0.90 with a controller only**: campaign screen (chapters, events, championships, Continue, Shop, New Campaign, Back), event page (vehicle row, START EVENT), results and payout, championship standings / Next race / Resume / Restart / Abandon, the Shop (buy, upgrades), the welcome panel and controls card, Settings rows added (testing switch, hints), split-screen setup and results. Every control must be reachable and usable with D-pad / stick, A, B (back), and LB / RB where tabs exist; focus must always be visible and never lost; B always goes back one level. Fix everything found and list it.
4. Make it structural where it is cheap: one check that fails if any menu page has a visible interactive row that controller navigation cannot reach.
5. Check: the walk above, done with an emulated or real controller only; list of screens walked.

### Part B — Split-screen: a controller must be able to be Player 1

Dan: "Couldn't use the controller as the first player (only keyboard option exists, with no option to switch)."

1. On the split-screen setup screen, **whoever opens it is Player 1 on the device they used**: opened with a controller, Player 1 is that controller. Each player slot shows its device (with the controller's glyph or "Keyboard") and can be changed: a slot's device row cycles through the available devices (each connected controller, keyboard), and pressing A / Enter on an unassigned device joins it to the first free slot. Two slots can never hold the same device.
2. Supported pairs: controller + controller, controller + keyboard (either way round), and one human on any device with "Player 2: AI driver" on. With one controller and no second device, Player 2 defaults to the AI driver so Dan can start at once.
3. The whole setup screen (vehicles, colours, course, laps, split direction, AI switch, Start) is driven by Player 1's device; Player 2's device controls only Player 2's own choices. The results screen and the pause menu work from either player's device.
4. In the race each device drives only its own vehicle (confirm the controller drives Player 1 when assigned so, not Player 2).
5. Check with a controller only: open Split Screen, be Player 1, AI as Player 2, race a lap, finish, rematch, quit; then controller as Player 1 with keyboard as Player 2.

### Part C — Campaign: "The Opening Jump" never registers a clean jump

Dan: "Stuck on the campaign. First jump requires you to make a jump, but no matter how I land it doesn't register a clean jump for there even when I land perfectly straight in the middle of the road."

Likely cause, from the code and 0.90's own note: `ArcadeActivities` rejects a jump when the vertical impact speed is over 18 m/s (and when wet, tilted or wiped out), and 0.90 measured that this jump only lands "clean" between about 16 and 28 m/s: "faster lands unclean". A player naturally takes a jump event flat out, so every attempt is thrown away as a hard landing, with only the generic "Jump not scored / unstable, wet or hard landing". Confirm on a copy of Dan's save with a controller, driving it as a player would, before changing anything.

1. **A jump counts if the vehicle survives it.** For jump scoring everywhere (campaign events and Free Roam activities): a jump is scored when the vehicle takes off from the site, comes down on its wheels (upright), is not wiped out, and drives on. **Remove the hard-landing rejection** (the 18 m/s impact limit) for ordinary jumps; keep the wipeout, upside-down and wrong-direction rejections; keep the summit flights' own rules. A landing that ends in the water does not count, but say so specifically.
2. **Say why, every time.** Replace the generic message with the actual reason: "Landed on your side", "Wiped out on landing", "Landed in the water", "Took off outside the marked area", "Too short to count", and for a scored jump the distance and medal. In a campaign jump event also show the three targets and the best so far on the HUD during the attempt.
3. **More than one attempt per run.** A campaign jump event does not end on the first jump: the player may go round and jump again until the time limit, with the best scored jump counting, and can end the event early from the pause menu keeping the best. A failed attempt never ends the event.
4. **Re-set the targets from flat-out runs** of vehicles a player owns at that point (Street Classic, Trail Four, Needle 600) now that fast jumps count: bronze reachable on a clean ordinary attempt, gold needing a committed fast run. Do the same review for chapter 4's Summit Homeward Flight (0.90 noted the Needle 600 "did not land clean flat out") and the Free Roam jump sites' medal targets if this change moves them a lot; say what changed. Existing Free Roam jump records stay.
5. Check the other event kinds for the same trap (a result the player cannot get by playing naturally): the speed-trap events (direction, the 45 s limit, the 200 m run-up), the smash event, the time trials' flying-lap start. Fix what is found.
6. Check, with a controller, on a copy of Dan's save: take The Opening Jump flat out in each of his three vehicles: scored, medal shown, event passed, next event unlocked.

### Verification

Light, per the Verification budget, with the controller-only rule above. Compile, launch, release steps. Results as a short list, with "for Dan to check".

## Previous delivery — Locked-vehicle silhouettes, new-player hints, campaign round 2 (championships, chapters 2–4, upgrades), split-screen stage 1 — 0.90.0-review1 — DELIVERED, REVIEW STARTED (controller navigation and the campaign jump fixed in 0.91)

- **Authorized by Dan (2026-10-07, 02:53).** Written by Claude (chat). His words are quoted in each part. He is asleep while this runs and his weekly limit has just reset: a long round is fine.
- **Starting point:** main at the "Record 0.89 delivery" commit; playable source `a9f40b34` (0.89.0-review1 / game-89000). This TODO edit is uncommitted and belongs in the safety checkpoint.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–D below. Order: A, B, C (campaign), then D (split-screen). Commit C and D separately so either can be reverted alone.**
- The Verification budget applies in full. Dan tests by playing.
- Dan has now played chapter 1 through, so campaign round 2 **is** in this round (Part C), with the championships he asked for.

### Results (2026-10-07, Claude Code)

- **DELIVERED:**
  - Source `3c3bceb4defb86a058e1174ae0c6612480ce62e1` pushed and verified on origin/main (Parts A–C `0c0ddd00`, Part D `65bc5326`).
  - Fresh 0.90.0-review1 Windows build from that commit: 0 errors, 2m42s ([build-release.txt](Docs/Report090/build-release.txt)).
  - Published [game-90000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-90000) with `Tools/Publish-LauncherRelease.py` (the project's `gh`): the known draft-lookup miss, then `--resume-draft` uploaded the three assets and published. Previous releases retained.
  - All 234 Latest files match the public signed manifest; public download, signature, install and startup pass ([hosted/result.json](Docs/Report090/hosted/result.json)); the catalog reports no pending game or music update.
- **Play-Racer.cmd (unchanged):** launched a responsive `Builds/Latest/versions/90000/Racer.exe` (0.90.0-review1) through the launcher, muted; settings restored byte for byte; Dan's `campaign-v1.json` unchanged (hash before / after). Latest root, current 90000 and previous 89000 retained (88000 already retired by the updater).
- **Cleanup:** Builds 10,151,700,323 → 8,009,092,440 bytes (2.1 GB recovered); also the hosted-check install (1.7 GB), 1.7 GB of check scratch outside the project (bench player, logs) and the temporary editor tools. C: free 267,456,528,384 bytes after cleanup.
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.90 (rule 12).

- **Safety checkpoint:** `563ee8e8` (this plan), pushed. Commits: `0c0ddd00` Parts A–C, `65bc5326` Part D (split-screen, revertible alone; it also carries two small fixes to A–C files: the controls card's key order and a check). Version 0.90.0-review1 / build 90000.
- Evidence: [Docs/Report090/](Docs/Report090/) ([Shots/](Docs/Report090/Shots/), [Lists/](Docs/Report090/Lists/)). Code: `PadlockMark.cs`, `Hints.cs`, `RaceMenus.Hints.cs`, `CampaignData.cs` / `Campaign.cs` / `CampaignRun.cs` / `RaceMenus.Campaign.cs` / `RaceMenus.Shop.cs`, `SplitScreen.cs`, `SplitHud.cs`, `RaceMenus.Split.cs`, `SplitBench.cs` (evidence only: needs `-splitBench` and `-racerTestSave`); checks `Report090Checks*.cs`; tools `Tools/Report090/`. No VALIDATION.md.
- **A — Locks — DONE.** Every locked vehicle (campaign or acorn) is a dark silhouette in the Garage (Race and Free Roam) and in the Shop, with a padlock badge and the line saying how to get it; the Shop keeps the stat bars and the price ([garage](Docs/Report090/Shots/A-garage-locked-silhouette.png), [shop](Docs/Report090/Shots/A-shop-locked.png)). Locked tracks (Tracks and the playlist editor) are dimmed with a padlock and the one-line reason; the map draws a locked course's route grey and says how to reach it ([shot](Docs/Report090/Shots/A-tracks-locked-grey.png)). No leaks: RACE on the main menu of a locked course's scene (reachable after Free Roam) now opens Race Setup on an open course; the other paths (results retry / next, playlists, Free Roam → Race, a saved choice, Start Race) were already guarded in 0.89. Main menu line under CAMPAIGN, e.g. "Chapter 1: Street Loop · 2 of 7 events · $4,200" (the chapter's events plus its championship); with Testing on "TESTING: everything unlocked" ([normal](Docs/Report090/Shots/A-main-menu-status.png), [testing](Docs/Report090/Shots/A-main-menu-testing.png)).
- **B — New-player hints — DONE.** A new player (no campaign progress, Testing off) gets a welcome panel after the title instead of going straight into Free Roam: four lines, START THE CAMPAIGN (chapter 1's first event page) and Look around first (Free Roam) ([shot](Docs/Report090/Shots/B-welcome.png)). CAMPAIGN is the main menu's default selection until a campaign event has been run. A controls card holds the first event's countdown until one press (steer, accelerate, brake / reverse, reset, camera view, pause, from the real bindings, controller glyphs or keys) ([shot](Docs/Report090/Shots/B-controls-card.png)); a player who has already run campaign events (Dan) does not get it. In-play hints, once each, small, timed and one at a time: stuck or off the course ([shot](Docs/Report090/Shots/B-hint-stuck.png)), the first shortcut ahead (the OPTIONAL SHORTCUT signs and the gold minimap line), the first money, the first thing opened, the first Free Roam, the first locked item. Settings > Gameplay: Hints On / Off and Show hints again; New Campaign offers to show them again. Never in Trailer Mode or split-screen.
- **C — Campaign round 2 — DONE.**
  - **Save:** `campaign-v1.json` keeps its name and is now version 2 inside (upgrades, championships, completion, money earned, time racing). A 0.89 (version 1) save loads and carries on unchanged and is written as version 2 at its next save; loading never writes. **Dan's real save (a copy) checked:** $13,000, Street Classic / Trail Four / Needle 600, chapter 1 all won, chapter 2 open at "Into the Woods", **the Street Cup open**; saved and reloaded as version 2 with the same progress. His real file was only read.
  - **Chapters 2–4** as specified, with these choices: chapter 2's activity is **The Opening Jump** (the Forest opening jump on the Forward course); chapter 3's is **Fence Line Smash** (no jump site lies on the Backyard courses; the fence-line smash site is beside the Backyard road; a new smash event kind: 45 s, distinct props); chapter 4's is the **Summit Homeward Flight** (the mountain's giant flight). "Two Wheels Only" takes motorcycles and the ATV. The Forest Final's prize is a $4,000 first-win bonus. Prizes: Ridge Scrambler (Backyard Final), Highball Fastback (Summit Final). Rivals: chapter 2 Normal, chapter 3 Normal then Hard, chapter 4 Hard.
  - **Targets** ([campaign-targets.txt](Docs/Report090/Lists/campaign-targets.txt)): time trials from AI flying laps (Normal / Hard): Forest: Trail Four 1:12.6 / 1:11.8, Needle 1:01.9 / 1:06.8 → bronze 1:16, silver 1:09, gold 1:04; Backyard: Trail Four 1:06.5 / 0:58.5, Needle 1:11.1 → 1:12 / 1:04 / 0:59; Mountain: 1:58.7–2:03.6 → 2:14 / 2:04 / 1:58. The Opening Jump at held speeds: Trail Four 21.6 / 28.8 / 34.8 / 46.1 m at 16 / 20 / 24 / 28 m/s, faster lands unclean; Street Classic the same → 25 / 35 / 44 m. The summit flight flat out: Street Classic 150 m, Trail Four 215 m (the Needle did not land clean flat out) → 120 / 170 / 210 m. Smash: the site's own 3 / 6 / 10 props (a scripted drive got 9, silver).
  - **Championships:** Street Cup (3 races on Street Forward / Reverse), Woodland Cup (4, incl. Night and Dusk / Rain), Backyard Cup (6, incl. Night, Rain, Snow), Woodstock Grand Championship (all eight courses). Each opens when its chapter's final is passed. One owned vehicle (with its upgrades) for the whole series; five named rivals (Rusty Vance, Mia Torres, Big Ed Kowalski, June Park, Hollis Gray) in vehicles suited to each cup; points 10 / 7 / 5 / 3 / 2 / 1; ties by wins, then the better last race. After each race: the race result and the standings table, then Next race. Saved after every race; Resume from the campaign screen (Continue goes to it); Restart and Abandon ask first; no single-race retry (a race left from the pause menu counts as did not finish; if that was the last round, the final standings still show). Pays by final position (half on a replay), a first-win bonus and a ★ trophy on the campaign screen. Rows at the end of each chapter with the trophy or "Round n of m · Resume". Winning the Grand Championship shows **Champion of Woodstock** (the poster, the final table, time racing, money earned) and marks the campaign complete ([shot](Docs/Report090/Shots/C-champion-of-woodstock.png)).
  - **Upgrades:** per owned vehicle in the Shop (also Garage > Shop…), three levels each: Top speed +3 %, Acceleration +6 %, Grip +5 %, Handling (steering response) +6 % per level over stock, confirmed before buying; the Shop's bars show the upgraded part in gold. Campaign events and championships only: Race, Free Roam, split-screen and the Top 10 boards are stock (the vehicle is put back to stock when an event ends).
  - **Economy** (rule 12; Dan judges). Events pay 1st / gold, then 60 / 40 / 25 / 15 / 10 % by place, or 65 / 40 % for silver / bronze; replays half; the first-win bonus once.

    | Chapter | Events (1st or gold / first-win bonus) | Win everything | Typical (2nd / silver, final won) |
    |---|---|---|---|
    | 1 | unchanged from 0.89 | $13,000 | $7,700 |
    | 2 | 1,500/750 · 1,300/650 · 1,300/650 · 1,700/850 · 1,700/850 · Final 2,800/4,000 | $18,050 | $11,420 |
    | 3 | 1,800/900 · 1,600/800 · 2,000/1,000 · 2,000/1,000 · 1,600/800 · Final 3,400/1,700 + Scrambler | $18,600 | $10,660 |
    | 4 | 2,200/1,100 · 2,000/1,000 · 2,400/1,200 · 2,600/1,300 · 2,000/1,000 · Final 4,200/2,100 + Fastback | $23,100 | $13,220 |
    | Cups | Street 4,000/2,000 · Woodland 6,000/3,000 · Backyard 8,000/4,000 · Grand 12,000/6,000 (1st; then 60/40/25/15/10 %) | $45,000 | $18,000 (2nd) |

    The whole campaign: about $61,000 typical, $118,000 winning everything. Vehicle prices unchanged (Pebble $6,000 … Drifter $16,000). Upgrade levels cost 10 / 18 / 28 % of the vehicle's value (its price; Street Classic $4,000, Trail Four $5,000, Needle $15,000, Scrambler $13,000, Fastback $14,000): Street Classic $400 / $700 / $1,100 per stat ($8,800 for everything), Trail Four $11,200, Longroof GT $22,400, Needle 600 $33,600. Typical by the end of chapter 2 (about $21,500 with a Street Cup 2nd): a first car plus a second one or several upgrade levels; one play-through winning most events buys about half the garage and a fully upgraded favourite.
- **D — Split-screen, stage 1 — DONE.** Main menu SPLIT SCREEN → setup: player 1 is the device that opened it; player 2 joins with Start on another controller or Enter on the keyboard; Swap the players' devices; "Player 2: AI driver" (the normal race AI drives player 2's vehicle, with its own camera, HUD, laps and finish); each player's vehicle (all ten, plus the mower once earned, no campaign lock) and colour; course (all eight), laps, Screen top / bottom (default, player 1 on top) or left / right (also Settings > Gameplay, remembered). Each player's vehicle reads only its own device. Two chase cameras with a thin divider; in top / bottom they tip down 9° during flights longer than 0.45 s. A HUD per half (lap, position, lap and race time, penalties, speed, countdown, GO, wrong way with the reset key, recovering, finished with place and time, a minimap with both players). Race: the two on the grid (no AI rivals, Day / Clear, no traffic); gates, laps, shortcuts and nearest-point resets per player; it ends when both have finished or 90 s after the first; results show both (place, total, best lap) with Rematch / Change setup / Main menu. One radio: the D-pad on any controller (or [ ] I N), the first press wins, another within 0.5 s is ignored; both engines 2D at 60 % volume; the one listener stays on player 1's camera (3D ambient sounds are heard from there). Pause from either player; then only that device works the pause menu (Back / Start from the other are ignored). A dropped controller pauses with "PLAYER n'S CONTROLLER IS DISCONNECTED" and Resume off until a controller is back (it is given to that player). Nothing is written to the Top 10 boards, personal bests, ghosts, activity records, acorns or the campaign (the world-map discovery file is written as in any race). **Off or single in stage 1:** first person and the other views (chase only), Trailer Mode, the winner shot, gestures (fist, celebration), hints, the AI finish estimate and Complete Race; the loading screen is unchanged. **Lowered in split-screen only:** far clip 1,400 m, LOD bias × 0.7; the trees' near detail and the ground tufts follow both views. Single-player rendering is unchanged.
  - **Frame time** (a test player like the release, 3840×2160, GTX 1660 Ti, both vehicles driven by the AI, 40 s per layout; [list](Docs/Report090/Lists/split-frame-times.txt), shots `D-bench-*.jpg`): worst view Forest Loop Forward top / bottom 10.13 ms median (99 fps), 95th percentile 11.43 ms (87 fps); Street 8.50 / 10.51 ms, Mountain 6.78 / 10.23 ms; left / right is cheaper (5.5–7.8 ms median). Target 60 fps: met.
- **Checks** (muted editor play mode on an isolated save; lists `Docs/Report090/Lists/checks-*.txt`): **A** all PASS (silhouettes and padlocks, the grey route, 7 padlocked rows, the status line in both states, RACE in Mountain Loop's scene opened Race Setup on Street Loop). **B** fresh save: title → welcome → Start the campaign → the event page → the controls card holding the countdown → one press → racing → off the course → "Stuck or off the course? Press R …"; CAMPAIGN the default selection; the Settings rows: PASS. **C** Two Wheels Only lists motorcycles and the ATV; the Street Cup played start to finish by the race AI on the Trail Four with a quit to the campaign screen after round 1 and Resume (round 2 on Street Reverse at Dusk, then round 3): 4th overall, $1,000 paid and saved; the ending (Grand Championship rounds 1–7 recorded, the last left early): Champion of Woodstock, campaign complete; an upgrade felt: Street Classic 0–30 m/s 3.37 s → 2.68 s and the Hwy 92 trap 41.1 → 44.9 m/s with Top speed and Acceleration at level 3, stock again (49.0 / 14.5) after the event; the smash event 9 props; Dan's save copy as above: PASS. **D** setup (join with Start on controller 2, player 1's own Start ignored, 10 vehicles with a 2-vehicle campaign); a two-player Street Loop Forward race top / bottom (player 2 on its controller path, 2 laps each, results); Mountain Loop Forward with Player 2: AI driver, left / right (1 lap); the shared radio from each side; pause from each side; controller 2 unplugged and plugged back in; a Longroof GT with every campaign upgrade at level 3 driven stock; player 2 on the keyboard driving a lap by key presses (2:18.5, no missed gates or resets); back to one full-screen camera and player 1's input on every device; then a normal single-player race (4 cars, finished) and a campaign event start: all PASS.
  - Found by the checks and fixed: the main menu's remembered selection overrode the CAMPAIGN default; the campaign screen came back on the newest chapter after a championship round (it now opens on the chapter of the event or championship just run); the controls card's steer keys read "D A".
- **Decisions:** the campaign save keeps the name `campaign-v1.json` (version 2 inside); chapter 3's activity is the smash site (no backyard jump exists); no single-race retry in a championship (leaving early = did not finish); the controls card is for new players only; championships cannot be run with Testing on (each round loads a scene and Testing saves nothing); the split-screen listener is on player 1's camera; detail is lowered only in split-screen.
- **For Dan to check:** the payouts, upgrade prices and how much the upgrades are felt; the new medal targets (especially the two jumps and the summit flight); the championships' length and rival pace (the AI pilot finished 4th in the Street Cup); the welcome text and hints (Settings > Gameplay > Show hints again shows them on his save; the controls card only appears for a new campaign); split-screen with a real second controller (hot-plugging was emulated) and the radio / pause rules; the Fence Line Smash start and the summit flight's run-up.

### Part A — Locks: silhouettes for every locked vehicle, and state that is obvious

Dan (03:43): "ok... actually now I could see it was locked off. All locked cars should be silhouetted like the lawnmower." So the 0.89 locks do work with "Unlock everything (testing)" off; what misled him was that locked vehicles are drawn in their real colours. (History: on first launch he believed everything was unlocked and turned the testing switch on himself; Claude chat set it back to off in his `settings.json` at 02:58, original kept as `settings.before-claude-2026-10-07.json`. He has since played chapter 1 through, so his save now has a real `campaign-v1.json`: **keep it and never reset or overwrite it.**)

1. **Every locked vehicle is shown as a silhouette, exactly like the locked mower:** in the Garage (Race and Free Roam), the Shop and anywhere else a vehicle is previewed. Dark silhouette on the rotating preview, a padlock mark, and the line saying how to get it (price in the Shop, or the prize event). Its real look is revealed when it is bought or won. In the Shop the stat bars and price stay visible so the player can choose what to save for.
2. **Locked tracks** in Tracks and the playlist editor: clearly dimmed with a padlock and the one-line reason; the map panel shows the route in grey.
3. **No leaks:** walk every path (main menu, pause menu, Free Roam → Race, playlists, results → retry / next, the map, a saved last choice) and make sure a locked course or vehicle can never be started; Race Setup and the Garage open on something owned.
4. **Status line on the main menu** under CAMPAIGN: for example "Chapter 2: Forest Loop · 3 of 7 events · $4,200". With the testing switch on it reads "TESTING: everything unlocked" instead, so that state can never be mistaken again.
5. Check: shots of the Garage, the Shop and Tracks with locked items; the main menu line in both states.

### Part B — New-player hints that lead to the campaign

Dan: "When a new person starts the game, it should give some quick new player hints and direct them to the race events."

1. **First launch as a new player** (no campaign progress; Dan now has progress, so check this on a fresh isolated save): after the title, a short welcome panel in the menu style, three or four lines: what the game is, that events earn money and open tracks and vehicles, and one button, **Start the campaign**, which goes to chapter 1's first event. A second, smaller choice: "Look around first" (Free Roam). The main menu then has CAMPAIGN highlighted as the default selection until the first event has been finished.
2. **Controls card** shown once before the first event starts (behind or after the loading screen, dismissed with one press): steer, accelerate, brake / reverse, reset, camera view, pause; drawn for the device in use (controller glyphs or keys), from the real bindings.
3. **In-play hints, once each, small and timed, never blocking:** the first time the player is stuck or off course for a few seconds ("Hold <reset> to return to the track"); the first shortcut fork ("Gold arrows mark a shortcut"); the first time money is earned ("Spend it in the Shop"); the first time a track or vehicle is unlocked; the first time Free Roam is entered (map, minimap, activities); the first time a locked item is selected (how to get it). Keep the list short; no hint repeats once shown.
4. **Settings > Gameplay:** "Hints: On / Off" and "Show hints again". Starting a New Campaign offers to show them again. Hints never appear in Trailer Mode or split-screen.
5. Write hint text plainly, in the game's existing tone, true to the actual controls and features.
6. Check: a fresh save from launch to the first event's start, with shots of the welcome panel, the controls card and one in-play hint.

### Part C — Campaign round 2: championships, chapters 2–4, upgrades

Dan (03:43): "I actually went through the whole first section and saw that there was no other events for the other tracks yet. I want there to be championship race events as more tracks are unlocked."

He has finished chapter 1 on his real save. Build the rest of the campaign on the 0.89 framework. **His existing `campaign-v1.json` must load and carry on** (money, owned vehicles, chapter 1 results, the Needle 600 if won): migrate the save format if needed, never reset it.

**1. Championships (the new event type Dan asked for).**
- A championship is a series of races on the tracks unlocked so far, scored on points, with one overall winner. One per chapter, opening when that chapter's final is passed:
  - **Street Cup** (after chapter 1): Street Loop Forward and Reverse.
  - **Woodland Cup** (after chapter 2): the two Street Loop and the two Forest Loop courses.
  - **Backyard Cup** (after chapter 3): those four plus Dan's Backyard Forward and Reverse.
  - **Woodstock Grand Championship** (after chapter 4): all eight courses.
- Rules: the player enters with one owned vehicle and keeps it for the whole championship (with its upgrades). The same named rivals (5) race every round, in vehicles suited to the series, so there is a table to fight for. Points per race for places 1–6: 10, 7, 5, 3, 2, 1. Laps and conditions are set per round and vary (at least one night or weather round in each cup from the Woodland Cup on). After each race: that race's result and the **standings table**, then "Next race". Ties on points are broken by most wins, then best last race.
- Progress is saved after every race: the player can quit to the menu and **resume the championship** later from the next race, or abandon it (asks first). Retrying a single race inside a championship is not allowed; restarting the whole championship is.
- Pays by final championship position (a large payout, the biggest money in the campaign), with a first-win bonus and a trophy mark on the campaign screen. Winning is not required to open the next chapter (the chapter final does that), but the Grand Championship is the campaign's last event.
- Since Dan has already passed chapter 1's final, the **Street Cup is available to him as soon as this lands**.
- Campaign screen: championships appear as their own rows at the end of each chapter with the trophy state; a championship in progress shows "Round n of m" and Resume.

**2. Chapters 2–4, complete.** Same pattern as chapter 1 (six events each, each needing the one before, the last a final that opens the next chapter), using each area's own courses, shortcuts, sites and character. Use these, adjusting details where the course makes another choice clearly better and saying so:
- **Chapter 2, Forest Loop:** (1) Into the Woods: race, Forest Loop Forward, Day / Clear. (2) Cave Run: time trial, Forest Forward, three targets. (3) a Forest activity event (speed trap or jump on that course). (4) Rain in the Pines: race, Forest Forward, Rain, Dusk. (5) The Long Way Back: race, Forest Loop Reverse (unlocks it). (6) Forest Final: race, Forest Forward, 5 rivals. Prize: a large money bonus.
- **Chapter 3, Dan's Backyard:** (1) Backyard Dash: race, Backyard Forward. (2) time trial, Backyard Forward. (3) Two Wheels Only: race for motorcycles and the ATV on Backyard Reverse (unlocks it). (4) Snow Day: race, Backyard Forward, Day / Snow (the sledding and broom-hockey scenes are out). (5) an activity event on the Backyard's jump or smash site. (6) Backyard Final. Prize: **Ridge Scrambler**.
- **Chapter 4, Mountain Loop:** (1) First Ascent: race, Mountain Forward, Dawn. (2) time trial, Mountain Forward. (3) Downhill: race, Mountain Reverse (unlocks it). (4) Whiteout: race, Mountain Forward, Night / Snow. (5) a Mountain activity event. (6) Summit Final: race, Mountain Forward, 5 rivals at the campaign's hardest. Prize: **Highball Fastback**.
- Rival speed, field vehicles and targets step up by chapter; set time-trial and activity targets from measured runs of vehicles a player could plausibly own at that point.
- **Ending:** winning the Woodstock Grand Championship shows a short "Champion of Woodstock" screen (the poster, the final table, total time played, money earned) and marks the campaign complete; everything stays replayable.

**3. Upgrades.**
- Bought per owned vehicle in the Shop / Garage: **three levels each for Top speed, Acceleration, Grip and Handling**. Each level is a modest, noticeable step; prices rise per level and scale with the vehicle's price; fully upgrading a starter makes it competitive late on but never better than the best vehicles fully upgraded. Stat bars show the stock value and the upgraded part in a second tone.
- **Campaign only:** upgrades apply in campaign events and championships. Race, Free Roam, split-screen and the existing Top 10 boards always use stock vehicles.
- Rivals in later chapters are tuned on the assumption of some upgrades, not all.

**4. Economy.** One considered set of numbers (rule 12; Dan judges): by the end of chapter 2 a player can afford a second bought vehicle or a few upgrade levels, not everything; the whole campaign played through once, winning most events, buys roughly half the garage plus a fully upgraded favourite; replays (half pay) and championships make the rest reachable. Put the payout, price and upgrade tables in the results.

**5. Checks (light):** each new event type run once; one championship played start to finish on an isolated save with a quit and resume in the middle; one upgrade bought and felt (a timed straight before / after); Dan's real campaign save copied, loaded and shown to carry on with the Street Cup available; the "for Dan to check" list.

### Part D — Split-screen, stage 1: two players, one race

Dan: "we can go ahead and start the first part of split screen." His earlier decisions (2026-10-06, in the backlog entry): the point is to play with a friend on one PC; **one shared radio that either player can control**, never two; **it must be testable by Dan alone**; **top / bottom by default with a setting for left / right**; **everything is unlocked in split-screen**.

Stage 1 is the plumbing and one good race. Stages 2 and 3 (all conditions, AI rivals, the full garage for both players, Free Roam for two, per-player views and gestures) come later; build so they fit.

1. **Menu:** main menu entry **SPLIT SCREEN** → a setup screen: Player 1 and Player 2 each join by pressing a button on their device (controller 1, controller 2, or keyboard); each picks a vehicle from **all eleven-minus-reward vehicles plus the mower if earned, all unlocked regardless of the campaign**, and a colour; then the course (all eight selectable, all unlocked), laps, and split direction. Start when both are ready.
2. **Devices:** any two of: controller, second controller, keyboard. Each player's input drives only their vehicle. A device dropping out pauses the race with a clear message until it is back or the race is ended.
3. **Solo testing (required):** on the setup screen a switch "Player 2: AI driver". With it on, the game's AI drives player 2's vehicle through the normal race AI, with player 2's camera, HUD, laps, resets and finish all running for real. Dan can also join player 2 on the keyboard while playing player 1 on the controller.
4. **Screen:** two cameras, **top / bottom by default** (player 1 on top), **left / right as a setting** on the setup screen and in Settings, remembered. Each half has its own chase camera (the normal chase view only in stage 1), with the camera tilting down slightly during long flights so the landing stays in view in the wide top / bottom layout. A thin divider between the halves.
5. **HUD per half:** speed, lap, position, lap time, wrong-way and reset prompts, countdown, a small minimap showing both players; sized and placed for the half's shape. No campaign, hint or Free Roam text.
6. **Race rules:** both players on the grid with no AI rivals in stage 1 (unless "Player 2: AI driver" is on, which is still just the two vehicles). Gates, laps, shortcuts, resets (the nearest-point rule) and finish work per player. The race ends when both have finished, or a set time after the first finishes. Results screen shows both: place, total time, best lap. Rematch / change setup / quit.
7. **Conditions in stage 1:** Day / Clear only, no traffic. (Weather, night and traffic need per-view work; stage 2.)
8. **Sound:** one listener arrangement that works for two views; both engines audible, each mixed down so neither drowns the other; **one radio**, and either player's radio buttons change station or skip, the first press winning and a second press within about half a second ignored.
9. **Pause:** either player can pause; the pause menu is controlled by whoever paused.
10. **Records:** split-screen results are not written to the Top 10 boards, ghosts, activities, acorns or the campaign in stage 1.
11. **Things built around one camera, in stage 1:** first person, the other camera views, Trailer Mode, the winner shot, gestures and the loading-screen route inset are simply off or single in split-screen; say which. Nothing may break or show in the wrong half.
12. **Performance:** two views roughly double the drawing cost. Target 60 fps or better at 3840×2160 on the GTX 1660 Ti on the worst course view in Day / Clear; if needed, lower per-view detail in split-screen only (draw distance, shadow distance, ground detail) and say what was lowered. Single-player rendering must not change.
13. **Everything single-player keeps working exactly as before.** The game assumed one human in many places (reset, guidance, gates, HUD, audio, camera, input); change those to per-player without altering single-player behaviour.
14. **Checks (this part may check a little more than the budget, since it touches the core):** split-screen has everything unlocked and uses stock vehicles (no campaign upgrades); a full two-player race on Street Loop – Forward and one on a Mountain course with "Player 2: AI driver" on, both finishing with correct laps and results; one race with the keyboard as player 2 driven by hand for a lap; both split directions; the shared radio from each side; pause from each side; a device unplugged and replugged if that can be emulated; frame time in the worst view; then one normal single-player race and one campaign event to show nothing regressed. Shots of the setup screen and both layouts.

### Verification

Light, per the Verification budget, except Part D as its own list says. Compile, launch, release steps. Results as a short list, with "for Dan to check".

## Previous delivery — Campaign, round 1 of 3 (framework, locking, money, vehicle shop, Street Loop chapter); remaining flickering car surfaces — 0.89.0-review1 — DELIVERED, DAN COULD NOT TEST IT (everything appeared unlocked on his PC; see 0.90 Part A)

- **Authorized by Dan (2026-10-06 / 07).** Written by Claude (chat). Dan reviewed 0.88.0-review1 and had nothing further ("i dont have anything left over"); he kept the Granite Saddle jump as it is. Part F is the one addition from that review.
- **Starting point:** main at the "Record 0.88 delivery" commit (0.88.0-review1 / game-88000). This TODO edit is uncommitted and belongs in the safety checkpoint.
- Runs unattended: the design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–F below.** The Verification budget applies in full.

**Dan's request (2026-10-06, 22:36):** "A single player campaign. The game starts with one track available (the street loop) and limited vehicles, as you go through the campaign you will unlock the different tracks (based on whatever tracks that you encounter in the campaign). You will also occasionally unlock some vehicles this way. But mostly you will earn money for race events which you can use to buy more vehicles and generic upgrades to your vehicle stats."

**Dan's decisions (2026-10-06, answered in chat):**
- **Locking:** "I would like it to be locked, but with free roam and splitscreen having everything unlocked. I need a separate mode for testing though." **Corrected by Dan at 22:45: "Wait, I want the locked vehicles to stay locked in free roam."** So: the normal race modes follow campaign progress; **Free Roam always has the whole world open, but only the vehicles the player owns**; **split-screen, when it exists, has everything unlocked**; and there is a **testing mode that unlocks everything**.
- **Upgrades apply in the campaign only.** Races outside the campaign use stock vehicles and keep today's records. Campaign times get their own board.
- **Events are chosen from a menu list by chapter** (not by driving to markers).

- **DELIVERED:**
  - Source `a9f40b347de3c6474acf23cc1792524658bec63e` pushed and verified on origin/main.
  - Fresh 0.89.0-review1 Windows build: 0 errors, 4m06s ([build-release.txt](Docs/Report089/build-release.txt)).
  - Published [game-89000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-89000) with `python Tools/Publish-LauncherRelease.py` (the project's `gh` on PATH): the known draft-lookup miss after the draft was created, then `--resume-draft` uploaded the three assets and published. Previous releases retained.
  - All 234 Latest files match the public signed manifest. Public download, signature, install and startup pass ([hosted/result.json](Docs/Report089/hosted/result.json)). The catalog reports no pending game or music update.
- **Play-Racer.cmd (unchanged):** launched a responsive `Builds/Latest/versions/89000/Racer.exe` (0.89.0-review1) through the launcher, muted, settings restored byte for byte; no campaign file was written to Dan's save. Latest root, current 89000 and previous 88000 retained (87000 had already been retired by the updater).
- **Cleanup:** Builds 10,151,346,143 -> 8,008,845,775 bytes (2.1 GB recovered); also the hosted-check install (1.7 GB), about 78 MB of check scratch outside the project and the temporary editor tools. C: free 268,866,433,024 bytes after cleanup.
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.89 (rule 12).

### Results (2026-10-07, Claude Code)

- **Safety checkpoint:** `1c0829f2` (this plan), pushed. Version 0.89.0-review1 / build 89000.
- Evidence: [Docs/Report089/](Docs/Report089/) ([Lists/](Docs/Report089/Lists/), [Shots/](Docs/Report089/Shots/)). Code: `Campaign.cs` (save and rules), `CampaignData.cs` (chapters, events, prices), `CampaignRun.cs` (the running event), `RaceMenus.Campaign.cs`, `RaceMenus.Shop.cs`; checks `Assets/Scripts/Report089Checks.cs`; tools `Tools/Report089/` (incl. `coplanar_parts.py`). No VALIDATION.md.
- **A — Campaign save and menu — DONE.**
  - **CAMPAIGN** is the main menu's first entry (in the Free Roam pause menu it follows RESUME DRIVING / Trailer / Camera). It opens the campaign screen: title with the money, a status line, **Continue** (next unpassed event's page; "Start" before any save), **Shop**, **New Campaign** (asks first when there is progress; says records, acorns, settings and Free Roam are untouched), **Back**; chapters down the left (locked ones say what opens them), the selected chapter's events beside them (name, type, best / "Locked"); the Tracks map panel shows the highlighted event's route with its course, type, conditions, entry rule, payout and best. Keyboard, controller and mouse (hover moves the map like Tracks). [Screen](Docs/Report089/Shots/A-campaign-screen.png), [new](Docs/Report089/Shots/A-campaign-screen-new.png).
  - Save: `campaign-v1.json` beside the other saves, atomic (`AtomicSave`), `version 1`: money, owned vehicles, opened courses, chapter, per-event result (runs, passed, won, bonus paid, best place / time / score / medal). An unreadable file is left untouched and not overwritten (New Campaign replaces it).
  - Event page: details (chapter, course, type, conditions, entry, payout, targets, pass rule, prize, best, lock), a ‹ vehicle › row of owned vehicles that fit, START EVENT ([shot](Docs/Report089/Shots/A-event-page.png)). Starting goes through the normal loading screen ("Campaign: <event>") into the event's course; the event's laps, rivals, difficulty, traffic, time of day and weather are applied for the run only (Race's own settings are never written and are restored after). Results-and-payout page: place or medal, money earned (replay: half pay), first-win / first-gold bonus, anything unlocked (next event, chapter, prize, course now open in Race), pass / not passed, money; CONTINUE (campaign screen) and Retry event, with the standings for races ([race](Docs/Report089/Shots/A-results-event1.png), [trap](Docs/Report089/Shots/A-results-event2-trap.png), [time trial](Docs/Report089/Shots/A-results-event3-timetrial.png)). Ending an event from the pause menu (or Back on the results) returns to the campaign screen; an abandoned run pays nothing and records nothing. Debug / Trailer movement in a run: "DEBUG RUN — no result or payout".
  - Event kinds as built: **race** (rival count from the event; the grid now takes 3 or 5 rivals, the player 7 m further back per extra rival); **time trial**: one flying lap, solo, starting 150 m before the START line (the lap clock starts at the line); **speed trap**: standing start 200 m before the trap on the course road, 45 s to reach it, the first scored crossing of that trap is the result. Pass = top three in a race, bronze or better otherwise.
- **B — Locking — DONE.** With no campaign save the player is a new campaign player: Street Loop – Forward and the Street Classic and Trail Four.
  - Race: Tracks lists all eight, locked ones dimmed with one line ("LOCKED: reach it in chapter 2, Forest Loop"; Street Loop – Reverse: "reach it in \"Wrong Way Round\" (chapter 1)"); picking one only says how to get it. Race Setup on a locked course's scene (reachable from a Free Roam start there) shows it locked with START disabled; Start Race itself refuses a locked course; "Race" from Free Roam opens on an open course. Playlists: the editor marks locked courses and refuses them; a playlist with a locked course will not start. AI rosters keep every vehicle.
  - Garage (Race and Free Roam): owned vehicles are selectable; the others follow in the list as locked (shown in their real colours with "LOCKED: Buy it in the Shop for $6,000" / "Prize: win \"Street Loop Final\""; the mower keeps its acorn silhouette). A saved choice that is not owned is driven as the first owned vehicle without rewriting Dan's saved choice ([Race](Docs/Report089/Shots/B-garage-locked-race.png), [Free Roam](Docs/Report089/Shots/B-garage-locked-roam.png), [Tracks](Docs/Report089/Shots/B-tracks-locked.png)).
  - Free Roam: the whole world and every activity; its start-location list (Tracks from the Free Roam page) offers all eight courses ([shot](Docs/Report089/Shots/B-roam-tracks-open.png)); only owned vehicles (plus the mower once earned).
  - **Testing:** Settings > Gameplay "Unlock everything (testing)", default Off: every course and vehicle in Race and Free Roam; campaign events can still be run but nothing is saved or paid; the Shop does not sell. **F6 debug menu** (Debug Mode only) has a row: Campaign +$5,000 · Unlock campaign (every vehicle, course, chapter and event) · Mark event won (the event highlighted on the campaign screen, else the next one; applies its prize / next event / chapter, no money) · Reset campaign. These four write the campaign save even in Testing.
  - **For Dan's install (no surprise):** on first launch of 0.89 Race offers only Street Loop – Forward and the Street Classic / Trail Four, and Free Roam only those two vehicles (the whole world stays open). Your records, Top 10s, ghosts, acorns (22/24), settings and Free Roam clock are untouched; Settings > Gameplay > Unlock everything (testing) gives everything back at once.
- **C — Money and the Shop — DONE.** One set of numbers (rule 12):

  | Event | Pays (1st / gold) | Then | First win / gold bonus |
  |---|---|---|---|
  | 1 First Lap (race, 2 laps, 3 Easy rivals) | $1,200 | $720 / $480 / $300 | $600 |
  | 2 Hwy 92 Speed Trap (bronze 34, silver 39, gold 44 m/s) | $1,000 | silver $650, bronze $400 | $500 |
  | 3 Against the Clock (bronze 2:30, silver 2:20, gold 2:14) | $1,000 | silver $650, bronze $400 | $500 |
  | 4 Night Shift (race, 3 laps, 3 Easy rivals, night, traffic) | $1,400 | $840 / $560 / $350 | $700 |
  | 5 Wrong Way Round (Reverse, 2 laps, 3 Normal rivals, dusk rain) | $1,400 | $840 / $560 / $350 | $700 |
  | 6 Street Loop Final (3 laps, 5 Normal rivals) | $2,500 | $1,500 / $1,000 / $620 / $380 / $250 | $1,500 + Needle 600 |

  Replays pay half; the bonus is paid once. Winning everything first time: $13,000; a typical run (2nd / silver / silver / 2nd / 2nd, then winning the final): about $7,700. **Prices:** Pebble Coupe $6,000, Sundown Roadster $7,000, Skyfin Cruiser $8,000, Longroof GT $10,000 (all from chapter 1), Drifter Twin $16,000 (from chapter 2: "Locked" until then). Starters: Street Classic, Trail Four. Prizes: Needle 600 (win the chapter 1 final), Ridge Scrambler (chapter 3), Highball Fastback (chapter 4) — defined now, shown as prizes in the Shop. The mower is not in the Shop.
  - Shop (campaign screen and Garage > Shop…): all ten non-reward vehicles with the turning preview, stat bars, status (OWNED / Price / not enough money yet / PRIZE from "<event>" / Locked from chapter 2), BUY with a confirmation ([shop](Docs/Report089/Shots/C-shop-roadster.png), [prize](Docs/Report089/Shots/C-shop-prize.png), [confirm](Docs/Report089/Shots/C-shop-confirm.png)). Upgrades not built; the Shop page and the save have room for them.
  - Medal targets were measured through the real event flow ([campaign-targets.txt](Docs/Report089/Lists/campaign-targets.txt)): trap at full throttle from the event start — Street Classic 41.1 m/s, Trail Four 46.1; flying lap by the race AI — Street Classic 2:18.5 (Normal) / 2:13.5 (Hard), Trail Four 2:16.4 / 2:12.2. Gold on the trap needs the Trail Four (or better) flat out.
- **D — Chapter 1: Street Loop — DONE** as specified (six events, each needing the one before passed; the final's top three opens chapter 2, first pays the bonus and the Needle 600). Rival vehicles: 1 Street Classic / Pebble / Roadster; 4 Roadster / Longroof / Trail Four; 5 Pebble / Skyfin / Trail Four; final Trail Four / Longroof / Roadster / Pebble / Drifter Twin. Event 5 opens Street Loop – Reverse in Race when the campaign first puts the player on it. Chapters 2–4 show as locked placeholders with their names and what opens them (chapter 2 once open: "its events arrive in the next update").
- **E — Campaign records — DONE.** Best place (and time), time or speed with medal per event in the campaign save, shown on the event rows, the map caption and the event page. Campaign runs never write the Top 10 boards, personal bests, clean-lap ghosts or activity records.
- **F — Flickering vehicle surfaces — FIXED in the Blender sources, all models re-scanned clean.** A new part-level scan (`Tools/Report089/coplanar_parts.py`, runs each model's own script and names the parts) found the 0.88 overlaps were partly misplaced: 0.88 read the imported model space turned round, so its "tail lamps" and "chrome trims at the back" are at the **front**. Found and fixed ([before](Docs/Report089/Lists/F-parts-before.txt) / [after](Docs/Report089/Lists/F-parts-after.txt), imported FBX re-scan [F-fbx-coplanar-after.txt](Docs/Report089/Lists/F-fbx-coplanar-after.txt): 0 everywhere):
  - Street Classic, Longroof GT: grille face on the plane of the headlamp bezel fronts and the grille bars → grille set 1 cm back, bars end short of the inner bezels. Longroof GT alloy spokes 2 mm from the hub face → 4–5 mm clear.
  - Highball Fastback: headlamp fronts on the grille face → 1 cm proud; side stripe ran through the arch flares → between the arches.
  - Sundown Roadster: the two knock-off spinner bars on one plane → 4 mm apart.
  - Ridge Scrambler, Needle 600: bar pad's underside 1.7 mm from the handlebar's → the pad wraps the bar. Needle 600 also: axle ends flush with the lugs → 7 mm past; the chain lay on the rear sprocket's face → 7 mm outboard; fork tube tops on the upper clamp's face → 5 mm inside.
  - Trail Four: top cooling fin's underside on the tank's → 6 mm below. Turf Rocket: a spring coil's top on the seat pan's → clear.
  - Traffic pickup: rear arch shadows on the bed sides, tailgate / cab back level with the bed sides → 5 mm / 1 cm apart. Traffic van: bonnet block's underside on the body's → 1 cm up.
  - Already clean: Pebble Coupe, Skyfin Cruiser, Drifter Twin, traffic sedan and wagon. Visual only: no collider, handling or seat change. Check: orbit by day and at night with headlights, the eight changed player vehicles ([sheets](Docs/Report089/Shots/), `F-<vehicle>-orbit-day-night.jpg`).
- **Checks** ([A-campaign-flow.txt](Docs/Report089/Lists/A-campaign-flow.txt), [B-locks.txt](Docs/Report089/Lists/B-locks.txt); muted editor play mode, isolated save with a lap record, a Top 10 entry and 22 acorns): new campaign → event 1 driven by the race AI on the Trail Four: 2nd of 4, $720, event 2 opened, Race settings restored; event 2 (speed trap, Street Classic flat out) silver 41.1 m/s, $650; event 3 (time trial, AI) silver 2:16.3, $650; F6 Mark event won (event 4); event 5 started: Street Loop – Reverse scene, Reverse opened, Dusk / Rain, then ended from the pause menu with no result; the final: 6 cars, grid 46 / 38 … 10 m behind the line, every rival's lap started after GO; Shop refused the Pebble at $2,020, F6 +$5,000, bought ($1,020 left, saved, in the garage); Shop Back → campaign; records, board and acorn files byte-identical after all runs; F6 Unlock and Reset. Locks: Tracks 7 of 8 locked, picking one refused, Race garage refuses the Needle 600, Free Roam drives the Street Classic for a saved Needle 600 and offers all eight start courses, Testing on = 10 vehicles / 8 courses / no campaign file, off again = 2 / 1. All PASS.
  - Found by the checks and fixed: a direct course pick (`SelectCourseEntry`) did not refuse a locked course (only the menu row did); the label rows on the campaign screen were cut off.
- **Decisions:** course unlock when the campaign first puts the player on it (at the event's start); pass = top three / bronze; no payout for an abandoned run; time trial and speed trap start on the road a run-up before the line / trap (no new geometry); the mower stays out of the campaign entirely; a saved vehicle choice the campaign does not allow is not rewritten (Testing brings it straight back); campaign-locked vehicles show in colour in the garage (the mower keeps its silhouette); the event difficulty is chapter 1's Easy → Normal (no Hard yet, for later chapters).
- **For Dan to check:** whether the payouts and prices feel right (a typical chapter 1 buys one cheaper car); the trap and time-trial medal targets; the final's 5 Normal rivals incl. the Drifter Twin; that nothing on the campaign screen feels cramped at his resolution; the vehicle fronts (grilles, lamps) and wheels for any flicker left.

### The whole design (rounds 1–3), so round 1 is built to carry it

- **Chapters, one per area, in this order:** 1 Street Loop → 2 Forest Loop → 3 Dan's Backyard → 4 Mountain Loop. Each chapter is a list of events on that area's courses; its last event (the "final") opens the next chapter. A course becomes available everywhere else the first time the campaign puts the player on it; forward first, the reverse version as a later event of the same chapter.
- **Event types, all from things the game already has:** race against AI (laps, field size and rival vehicles set per event); time trial against bronze / silver / gold targets; activity events (speed traps, jumps, smash runs) with the same three targets; conditions events (a set time of day and weather: night, rain, snow, dawn); class events (cars only, bikes only, ATV).
- **Money:** every event pays by result. Races pay by finishing place (most for first, something for every finisher); timed and scored events pay by medal. The first time an event is won or golded it pays a one-off bonus. Replays pay a reduced amount so grinding works but progress is better. One currency, shown as dollars.
- **Vehicles:** the player starts owning two modest vehicles. A few are prizes for specific events; the rest are bought. Ownership is per campaign save.
- **Upgrades (round 2):** bought per vehicle, three levels each for Top speed, Acceleration, Grip and Handling, each level a modest step, priced upward, capped so a fully upgraded starter is competitive late on but not better than the best vehicles fully upgraded.
- **The riding lawnmower** stays the acorn reward (0.88), outside the shop and the campaign.
- **Rivals** get faster by chapter and use vehicles that suit the event; they never use upgrades beyond what the event defines.

### Round 1 scope (this round)

**Part A — Campaign save and menu**
1. Main menu gets **CAMPAIGN** as its first entry: Continue / New Campaign (with a confirmation before overwriting) and the chapter list.
2. A separate campaign save (own file, written atomically like the other saves, versioned): money, owned vehicles, events done with best result, unlocked courses, current chapter. Losing or resetting it never touches records, acorns, settings or Free Roam.
3. The campaign screen: chapters down one side (locked ones shown locked with what opens them), the selected chapter's events listed with type, course, conditions, entry rule, payout and the player's best result/medal; the course map panel from the Tracks screen shows the event's route. Money shown at the top. Controller and mouse.
4. Starting an event goes through the normal loading screen and race flow with the event's settings fixed; the player picks only from vehicles they own that fit the event. After it: a results-and-payout screen (place or medal, money earned, bonus, anything unlocked), then back to the campaign screen.

**Part B — Locking**
1. With a campaign in progress, **Race** (the normal quick race / Tracks / playlists) offers only the courses the campaign has unlocked and only the vehicles the player owns, always stock (no upgrades, today's records).
2. **Free Roam:** the whole world and all its activities are always open, as now, whatever the campaign has reached. **Vehicles are locked the same way as in Race:** only owned vehicles can be chosen (plus the riding lawnmower once the acorns have earned it), always stock. Locked ones show as locked with how to get them. (Split-screen, when built, has every vehicle and course unlocked.)
3. A locked course or vehicle shows in the lists as locked with one line saying how to get it; it is never just missing.
4. **Testing mode:** Settings gets "Unlock everything (testing)": while on, Race and Free Roam offer every course and vehicle regardless of the campaign, and nothing done in that state changes the campaign save. Default off. Also add campaign entries to the F6 debug menu, working only in debug mode: add money, unlock all campaign content, mark the selected event won, reset the campaign.
5. **Dan's existing install:** with no campaign save present the game starts as a new player (Street Loop and the two starter vehicles in Race; the two starter vehicles in Free Roam, with the whole world open). His records, acorns, settings and Free Roam clock are untouched, and the testing switch gives him everything back at once. Say this in the results so it is not a surprise.

**Part C — Money and the vehicle shop**
1. Payouts as designed above; pick one considered set of numbers so chapter 1 earns enough for one cheaper vehicle by its end (rule 12; Dan judges).
2. **Shop** (from the campaign screen and the garage): all vehicles with the 0.76 rotating preview, stat bars, price, and Owned / Buy / Prize from "<event>" / Locked. Prices follow how good the vehicle is. Buying asks for confirmation.
3. **Starters:** Street Classic and Trail Four. **Prize vehicles (placed in later chapters; define them now in data):** Needle 600 for the chapter 1 final, Ridge Scrambler in chapter 3, Highball Fastback in chapter 4. Everything else is bought. The mower is not in the shop.
4. Upgrades are **not** in this round; leave the data and UI room for them.

**Part D — Chapter 1: Street Loop**
Build chapter 1 complete, six events, in this order, each needing the one before:
1. **First Lap:** race, Street Loop Forward, 2 laps, 3 easy rivals, Day / Clear, any owned vehicle.
2. **Hwy 92 Speed Trap:** activity event on the Street Loop's speed trap, bronze / silver / gold.
3. **Against the Clock:** time trial, Street Loop Forward, 1 flying lap, three targets set from measured laps of the starter vehicles.
4. **Night Shift:** race, Street Loop Forward, 3 laps, 3 rivals, Night / Clear with traffic.
5. **Wrong Way Round:** race, Street Loop Reverse (unlocks it), 2 laps, 3 rivals, Dusk / Rain.
6. **Street Loop Final:** race, Street Loop Forward, 3 laps, 5 rivals at this chapter's hardest, Day / Clear. Winning (top three is enough to pass; first pays the bonus and the prize) awards the **Needle 600** and opens chapter 2.
Chapters 2–4 appear on the campaign screen as locked placeholders with their names; their events come in round 2.

**Part E — Campaign records**
Campaign events record best place / time / score per event in the campaign save and show it on the event row. They do not write to the existing Top 10 boards.

### Part F — Remaining flickering surfaces on vehicles

0.88 fixed the cabin-floor flicker and reported smaller overlaps of the same kind that were outside that part. Dan (2026-10-07): "code said that there may be some other cars with issues like the one just fixed, we can wrap this in there."

1. Fix the ones 0.88 listed: the Highball Fastback's tail lamps lying on its tail panel; the small chrome trims at the back of the Street Classic and the Longroof GT; the wheel chrome on the Sundown Roadster and the Longroof GT; and the two traffic bodies.
2. Then check every vehicle model for the same fault (two surfaces facing the same way at the same depth) with a script over the meshes, all eleven player vehicles including the Turf Rocket and the four traffic bodies, and fix whatever it finds, in the Blender sources. List what was found per vehicle.
3. Visual only: no collider, handling or seat change. Check: orbit each changed vehicle by day and at night with headlights.

### Not in this round
Upgrades; chapters 2–4; story text, cutscenes or characters; difficulty settings; split-screen.

### Checks (light, per the Verification budget)
- New campaign → play event 1 → payout → event 2 unlocks; one event of each type started and finished once.
- Buy a vehicle with enough money; refused without.
- Locked Race lists with a campaign in progress; Free Roam: whole world open, only owned vehicles selectable; testing switch on / off; the F6 entries.
- Dan's existing records and acorn count still there after starting a campaign.
- Results: the payout and price table, and a "for Dan to check" list.

### Planned next
- **Round 2 (superseded: delivered in 0.90 with the championships Dan asked for):** upgrades; chapters 2 (Forest), 3 (Dan's Backyard), 4 (Mountain) with their events, reverse courses, conditions events and the two remaining prize vehicles; an ending when the Mountain final is won.
- **Round 3 (after that):** balancing from Dan's play (payouts, prices, targets, rival speed), and polish.

## Previous delivery — Kyle's driveway, yard and mailbox; flashing car floorboards; Forest Reverse lake-jump landing (study only); acorn reward riding lawnmower — 0.88.0-review1 — DELIVERED, REVIEWED BY DAN (nothing further; Granite Saddle jump kept)

- **DELIVERED:**
  - Source `a7d017321e109fb9d96d409e5bb0c0a36141eade` pushed and verified on origin/main.
  - Fresh 0.88.0-review1 Windows build: 0 errors, 2m37s ([build-release.txt](Docs/Report088/build-release.txt)).
  - Published [game-88000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-88000) with `python Tools/Publish-LauncherRelease.py` (the project's `gh` put on PATH): the known draft-lookup miss after the draft was created, then `--resume-draft` uploaded the three assets and published. Previous releases retained.
  - All 234 Latest files match the public signed manifest. Public download, signature, install and startup pass ([hosted/result.json](Docs/Report088/hosted/result.json)). The catalog reports no pending game or music update.
- **Play-Racer.cmd (unchanged):** launched a responsive `Builds/Latest/versions/88000/Racer.exe` (0.88.0-review1) through the launcher, muted, settings restored byte for byte. Latest root, current 88000 and previous 87000 retained; 86000 had already been retired by the updater.
- **Cleanup:** Builds 10,151,368,259 -> 8,008,926,954 bytes (2.1 GB recovered); also the hosted-check install, about 190 MB of check scratch outside the project and the temporary editor tools. C: free 270,695,219,200 bytes after cleanup.
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.88 (rule 12).

### Results (2026-10-07, Claude Code)

- **Safety checkpoint:** `85a56cfb` (this plan), pushed. Part A `b5bb60e4`, Part B `e36d981e`, **Part C alone `e689cf15`** (study only, nothing to revert), Part D `81b20897`. Version 0.88.0-review1 / build 88000.
- Evidence: [Docs/Report088/](Docs/Report088/) ([Lists/](Docs/Report088/Lists/), [Shots/](Docs/Report088/Shots/)). Tools `Tools/Report088/`, `Tools/Blender/mower.py`; checks `Assets/Scripts/Report088Checks.cs`. No VALIDATION.md.
- **A — Kyle's place — DONE, all nine scenes** ([A-kyle-build.txt](Docs/Report088/Lists/A-kyle-build.txt)). House model untouched.
  - **One driveway at Dan's BUG-002 spot:** from the street edge at (476.8, −21.0) straight down to the 0.87 garage apron: 30.6 m, 4.8 m wide, one gravel ("Driveway gravel", as before), heading 95°. Even grade 22.7 % (12.8°) with 1.8 m rounded ends. At the mouth it takes the street's cross-fall (−18 %, the street is steep there) and is level by 8 m, so both edges meet the street. Ground cut / filled under it with 1:1.5 / 1:2 batters.
  - **Everything else removed:** the old drive ribbon, the 0.87 garage leg and the CR113 gravel "edge join" (26 × 36 m of gravel). The ground under them and their batters (about 790 m²) is refilled smooth from the ground round it (no shelf or step left). Route points and the map's "Anderson's" arrival now follow the new drive (arrival on the apron as before).
  - **Lawn:** flat at 2.31 m above the pad (two 0.17 m steps below the porch floor at 2.65) from the house front to the foot of the bank (about z 7–19 in the house frame), filling the hollow there; the bank meets it in a plain toe. One broad step (4 × 0.45 m, top 2.48, concrete like the porch, solid) in front of the door.
  - **Trees:** 3–4 per scene stood in the drive's line; re-planted 6–9 m beside it. 11–14 more planted on its left (north), where the old drive was (copies of nearby trees, with trunks). The frontage trees between house and street stay except in the drive's opening.
  - **Mailbox:** the CR-015 roadside mailbox (trigger collider, breakaway, as the others), on the verge left of the mouth, 1 m off the street edge, door to the street.
  - **Dan-and-Kyle vignette:** it stood on the new drive's line, so it moved to the verge beside the mouth, near the mailbox.
  - Race data: the drive's mouth is at the street edge, off the race surface. Nearest gate 30–101 m away; "Fence line smash" props 57 m; no branch within 40 m. Every scene's routes are identical before and after except Kyle's own drive points.
  - Checks: [Dan's three spots](Docs/Report088/Shots/) (`A-dan-BUG-00n.png`, with `-before`), [above](Docs/Report088/Shots/A-above.png), [straight down](Docs/Report088/Shots/A-straight-down.png), [mailbox](Docs/Report088/Shots/A-mailbox.png). Street → garage → street in Free Roam ([A-driveway-drive.txt](Docs/Report088/Lists/A-driveway-drive.txt)): Skyfin Cruiser and Needle 600 both ways, nothing touched, 0 resets, steepest pitch 13–14°, roll 10° at the mouth. Rule-4 grounding, nine scenes ([A-grounding.txt](Docs/Report088/Lists/A-grounding.txt)): about 975 points each, nothing floating; drive within 0.08–0.11 m of the ground; the one flag is 6 cm of grass over the far corner of the apron in five scenes. Collider comparison per scene ([A-collider-comparison/](Docs/Report088/Lists/A-collider-comparison/)): only the property (drive, ground tiles there, trees, mailbox, step).
  - How it got here: a first build mis-sampled the four scenes that share an already-edited ground tile and placed planted trunks before their templates were re-seated; all of it was reverted and rebuilt once in one pass.
- **B — car floorboards — FIXED in all six cars** (Street Classic, Longroof GT, Sundown Roadster, Highball Fastback, Pebble Coupe, Skyfin Cruiser). Cause: the carpet's top and the body shell's cabin floor were both at H.y − 0.30, coplanar (z-fighting). In `Tools/Blender/cars.py` (and the Roadster's floor in `cars81.py`) the carpet now straddles the shell floor, its top 1.5 cm above it. A scan of every vehicle model for same-facing coplanar overlaps ([before](Docs/Report088/Lists/B-coplanar-before.txt) / [after](Docs/Report088/Lists/B-coplanar-after.txt)): the floor overlap is gone in all six; the bikes, the ATV and the traffic kit never had it.
  - **Pebble Coupe roof:** the roof was there but covered only the front 0.6 m; behind it a 1.1 m nearly clear rear glass lay over the rear seats, so from above-behind it looked open. The roof now runs back over the rear seats (to −0.85; the rear glass is shorter and steeper). The other closed cars show their cabins through clear rear glass as intended (the Fastback by design).
  - Visual only. Checks: orbit by day and night with headlights and one above-behind shot per car ([before](Docs/Report088/Shots/B-cars-above-behind-day-before.png) / [after](Docs/Report088/Shots/B-cars-above-behind-day-after.png), `B-<car>-orbit-day/night.png`).
  - Not fixed (outside this part, reported): smaller same-facing overlaps elsewhere: the Fastback's tail lamps on its tail panel, small chrome trims at the back of the Street Classic and Longroof GT, wheel chrome on the Roadster and Longroof, two traffic bodies.
- **C — Granite Saddle landing — NOT BUILT (item 7); measurements and options in [C-granite-study.txt](Docs/Report088/Lists/C-granite-study.txt).**
  - The lip (72.3 m, rising 21°) throws vehicles over the pool and lake to a rim trail about 30 m lower that rises gently westward; they come down at 35–45° after 3.7–4.0 s.
  - Landing loads: Granite Saddle 550–1094 m/s² against 276 / 347 (bike / car) on the course's first main jump.
  - A landing that matches the real lip speeds (30–37 m/s) would need a hill 25–31 m high on the lake's west shore and a surface descending about 120–140 m below today's rim trail. No straight face, no flatter or lower lip changes that (flatter lips drop the slow vehicles in the lake). So nothing changed; AI stays off; course id unchanged.
  - Options for Dan in the study: leave it; a landing hill for typical race speed only; or make it a drop into the lake shallows.
  - **Dan's decision (2026-10-07): option 1, "I want to keep it." The Granite Saddle jump stays exactly as it is. Closed: do not rebuild, lower or re-propose it. AI stays off the line unless Dan asks for it.**
- **D — the acorn reward riding mower — DONE.** "Turf Rocket", class "Mower" (`Tools/Blender/mower.py`, [Blender sheet](Docs/Report088/Shots/D-mower-blender-sheet.png)): lawn tractor with bonnet, grille, headlights, steering wheel, big sprung seat, small front and big knobbly rear tyres, mid deck with a side chute, tow hitch; paint selectable; no make or badge.
  - **Unlock:** read from the acorn save (`woodland-acorns-v1.json` gets `rewardEarned`; a save at 24 found counts too). Until then the garage list shows it as a dark silhouette, "LOCKED: Find all 24 Woodland Acorns (n/24)", not selectable; the player and the AI cannot get it. The 24th acorn shows "ALL 24 WOODLAND ACORNS FOUND! / Unlocked: the Turf Rocket riding mower / Choose it in the Garage" for 10 s. Restart Acorn Hunt keeps it. Dan's save was never written.
  - **Numbers (rule 12, one set):** top speed 61, acceleration 18, grip 32, handling 12 (all equal-best with the Needle 600), mass 760 kg and car-class contact (it holds its own against the cars), upright 27 and air stability 0.24 for flat landings, wheelbase 1.6, track 0.5. Garage bars rescale to eleven (the mower is full on the first four).
  - Rider: the cars' seated pose on the centre line, hands on its wheel (0.85 logic), feet on footboards; all customization, fist wave and first person (bonnet and wheel in view).
  - Everywhere: every course and Free Roam; records per vehicle as before; activity medal targets use the Needle 600's. AI: Random / Mixed fields include it only now and then (one in four when drawn), only once earned.
  - Touches: the engine note is the shared engine audio pitched up (1.75). **No grass clippings:** the game has no ground-surface types (the roads are part of the ground tiles), so it was not cheap.
  - Checks on an isolated save ([D-mower-checks.txt](Docs/Report088/Lists/D-mower-checks.txt)): locked at 23/24 (not eligible, choosing it leaves the Street Classic, [locked in the garage](Docs/Report088/Shots/D-garage-locked.png)); the 24th acorn unlocks it ([message](Docs/Report088/Shots/D-celebration.png)); selected in the garage; kept after Restart Acorn Hunt and a fresh load. Loads with 4 wheels down, rider seated, fist wave, headlights at night ([day](Docs/Report088/Shots/D-vehicle-mower-day.png), [night](Docs/Report088/Shots/D-vehicle-mower-night.png)); first person: 640 frames, head hidden correctly. Summit giant jump: 252 m in 8.2 s, lowest up after landing 0.98. One Street Loop race lap with AI: 2:15.9, 0 missed gates, 0 resets (the Needle 600 AI 2:06.4, Street Classic 2:21.5, Trail Four 2:18.0). The 0.81 vehicle check flags "wheel bottom 0.28 m above ground": it counts the hub caps; the tyres sit on the road.
- **Decisions:** A: lawn two steps *below* the porch (Dan: "the area in front of the house just like two steps down"), the vignette moved, a step block rather than reshaping the porch; B: the carpet raised 1.5 cm rather than the shell floor lowered (the shell floor sits on the underbody on the Pebble); C: nothing built; D: equal-best stats rather than above the best.
- **For Dan to check:** the new drive and lawn from the street and the house; the mower's feel (absurdly quick but controllable?) and whether one in four is "now and then" for the AI; the Granite Saddle options; the Pebble Coupe's new roof line.


- **Authorized by Dan (2026-10-06).** Written by Claude (chat) from his review of 0.87.0-review1: debug session `2026-10-06_21-33-52-142_17cb14` (4 reports, all on 0.87.0-review1, `FreeRoamWorld`) and his message of 22:08: "the only thing I have is cleaning up Kyle's driveway, and yard, and adding a mailbox. The house itself looks fine. There is also a weird graphic glitch on the floorboard when looking at cars from certain angles. Other than that, nothing." The Summit Climb and the launcher icon drew no comment.
- **Starting point:** main at the "Record 0.87 delivery" commit; playable source `c8357cc8` (0.87.0-review1 / game-87000). This TODO edit is uncommitted and belongs in the safety checkpoint. More parts may be added before Dan starts it.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–D below (C and D added at 22:12 at Dan's choice). Order: A, B, then C (own commit), then D.** The Verification budget in "Mandatory standing workflow" applies in full.

### Part A — Kyle's place: one straight driveway, a tidy yard, a mailbox

**The house model is accepted. Do not change it.** Only the ground, drive, trees and props around it.

Dan's three reports, taken in order (the second replaces part of the first):
- BUG-001 at (472.0, 82.0, -0.6), facing 104°: "I want to add more trees to the left of the driveway. Clean up the driveway so that it is a straight, not broken driveway, all the way down to the parking area by the garage doors."
- BUG-002 at (477.7, 85.9, -21.1), facing 80°: "actually lets move these trees and move the driveway here, straight down to the driveway." (The view is from the road edge straight down toward the garage side of the house, through a stand of trees.)
- BUG-003 at (492.7, 81.1, -17.6), facing 154°: "other than the straight driveway down to the garage lets just get rid of all this other random road and make it grass... also make the area in front of the house just like two steps down and the grassy area should be flat at the base of the hill. Also I kept forgetting to say that Kyle's house needs a mailbox."

What to build:
1. **One driveway, in the BUG-002 position.** It leaves the road at about (477.7, -21.1) and runs in a single straight line down the hill to the parking apron at the two garage doors. One continuous gravel surface at one width, even gradient, clean edges, no kink, no patched join, no change of material part-way. Remove the trees standing in that line (re-plant them beside it). Keep the pitch drivable for every vehicle (0.87 measured 11–13° on the old line; do not make it steeper than that if the ground allows, cutting or filling the drive itself as needed).
2. **Remove every other piece of road, drive or track on the property:** the old drive and its dog-leg, the cut-off leg to the front door, the 0.87 link and its edge-join patch, and any other gravel or pavement there. Turn all of it to grass matching the lawn, with the ground smoothed so no trace or step is left. Move the map's "Anderson's" arrival point and any route points that followed the old drive onto the new one.
3. **More trees to the left of the driveway** (as seen driving down it from the road) so the house stays tucked away, and keep the woods dense between the house and the road everywhere except the drive's own opening.
4. **Front yard:** a flat lawn at the base of the hill in front of the house, and the front of the house reached by going **two steps down** from the yard/parking level to the porch level (two broad steps across the approach, not a flight). The hill behind the lawn ends in a clean toe, not a ramp into the porch. No floating or buried parts on the house: re-run the 0.87 grounding check after the ground changes.
5. **Mailbox:** a standard roadside mailbox on a post at the mouth of the driveway, on the verge, on the side a mail carrier would reach from the road, not on the drive or the road. Same style as the other mailboxes in the world; no collider or a breakaway one.
6. The Dan-and-Kyle scene at this house stays; move it only if the yard change needs it.
7. **Scenes:** 0.87 changed this property in all nine scenes through shared ground tiles. Make this the same everywhere, but read each course scene's route data first (section 5A): BUG-002's position is at the road edge, so check that the new driveway mouth, its apron and the removed trees do not touch a race line, AI line, gate or the nearby "Fence line smash" activity. If they would in some scene, keep that scene's old mouth and report it.
8. Check: one shot from each of Dan's three positions, one from above showing the single straight drive and the yard, one of the mailbox; a car and a motorcycle driven road → garage → road; collider comparison per scene listing only this property.

### Part B — Flashing on car floorboards (BUG-004)

"when you look at some cars there is some weird flashing on the floorboards" at (320.7, 26.0, 471.3), Pebble Coupe, chase view, Day Clear. Dan's message: "a weird graphic glitch on the floorboard when looking at cars from certain angles."

1. Find the cause in the car models: almost certainly two surfaces at the same height inside the cabin (floor pan and a second floor, carpet, seat base or the chassis top) fighting for the same pixels, or a shadow-only/hidden rider part flickering against the floor. Fix it in the Blender sources so there is one floor surface with clear separation from anything above or below it, in **all six 0.81/0.75 cars** (check each; list which had it), and in the traffic kit if it shows there.
2. **Also look at the Pebble Coupe's roof.** In Dan's screenshot the Pebble Coupe is seen from above and behind with the seats and floor in plain view, as if it had no roof or the roof were see-through from outside. It is meant to be a closed coupe. If the roof is missing, single-sided, or being hidden by the first-person head/cabin logic while in chase view, fix it; if the open top is deliberate, say so in the results. Check the other closed cars for the same thing.
3. Visual only: no collider, handling or seat-position change. Check: orbit the camera (Trailer Mode) round each car by day and at night with headlights; one shot per car from above and behind.

### Part C — Forest Loop Reverse: a proper landing for the Granite Saddle lake jump

Dan (2026-10-06, 22:12) chose this from Claude's list: "Bikes and cars clear the lake jump now but land hard, and AI is still kept off it. A rebuilt landing would finish that line."

State after 0.86: on the optional Granite Saddle line in `ForestLoopReverse`, all classes reach the lip with speed (the dip at x ≈ 532 was smoothed) and clear the pool and lake, but come down hard on the west rim trail near x ≈ 323 (0.85 measured a Needle 600 jolt of 674 m/s²); AI is off for the line. 0.85 judged that a matching landing for the present lip needs a face matching a 30–40° descent from a 25–30 m drop, i.e. heavy earthworks.

1. **Goal:** every one of the ten vehicles, arriving at normal race speed, flies the water and lands on a downhill face that matches its descent, rolls out under control and carries speed back to the main. Landing loads in line with the other accepted jumps on this course (measure one, such as J1, as the reference).
2. **Design the jump as a whole, not just the landing.** Choose whatever combination gives a clean result with the least disruption: lower or flatten the lip so the flight is longer and shallower; and/or build a landing mound or cut a landing face on the west rim; and/or shift the touchdown zone. Earthworks in `ForestLoopReverse` only are allowed (Dan's standing permission for this scene, including small changes to the House 3 driveway at the crossing; simple and direct, never winding). The flight must still clear the sunk House 3 pool and the lake with margin.
3. Keep the main (McFadden Cut), every gate, Fern Gully and the fork and rejoin points where they are. Re-seat trees and props on changed ground; nothing floating or buried.
4. **Coming up short** still ends in the water and can be driven out (0.85/0.86). A slow approach must not hit a wall.
5. **AI on:** when all classes land cleanly, switch AI on for the line for the classes that make it reliably, at the usual shortcut rate.
6. New course-rule ID for Forest Loop Reverse only if the optional line's timing changes materially; say which.
7. If no version of this can be made to work without redrawing the route, change nothing and report the measurements.
8. **Checks (heavier allowed, race geometry):** each class through the jump five times at race speed with landing loads and roll-out speed, once slow (water, drives out); three races with AI; collider and route comparison; shots of approach, flight and landing. **Own commit**, with the revert command.

### Part D — Acorn reward: the riding lawnmower

Dan: "a riding lawnmower would be cool, but with like top stats since it is an unlockable."

1. **Unlock:** finding all 24 Woodland Acorns unlocks an eleventh vehicle. Until then it shows in the garage list as a locked silhouette with "Find all 24 Woodland Acorns (n/24)". The moment the 24th is collected: a celebration message naming the unlock, and it is selectable from then on. Read the unlock from the existing acorn save (no new progress to lose); restarting the acorn hunt from the menu does not re-lock it once earned. Dan is at 22/24 and will earn it by play: **do not grant it in his save.** For testing use an isolated save.
2. **The vehicle:** a riding lawnmower (lawn tractor) built in Blender in the game's style: bonnet with a little grille and headlights, steering wheel, big sprung seat, small front wheels and large rear tyres, mowing deck slung under the middle with a side discharge chute, rear tow hitch. Selectable paint colour like the other vehicles. An invented name in the house style (for example "Turf Rocket"); no real make, badge or logo.
3. **Rider:** the parametric rider seated upright with hands on the wheel (the 0.85 hands-follow-the-wheel logic), fully visible, with all customization, gestures and first person working. First person sees the bonnet and wheel.
4. **Top stats, because it is the reward:** the best or equal-best of all eleven in top speed, acceleration, grip and handling response, with enough weight to hold its own in contact, and stable over jumps and landings. It should feel absurdly quick for a mower but controllable, not a twitchy joke. Garage stat bars rescale to include it. Rule 12: one considered set of numbers; Dan judges.
5. **Where it counts:** allowed on every course and in Free Roam. Its own class label ("Mower"). Records stay per vehicle, so its times do not displace other vehicles' on a per-vehicle view; in the all-vehicles Top 10 it appears like any other with its name shown. Activity medal targets use the nearest existing vehicle's, as 0.81 did.
6. **AI:** rivals do not drive it unless the player has unlocked it; after that it may appear in Random / Mixed fields occasionally.
7. **Touches, only if cheap:** a mower-like engine note made from the existing engine audio, and a puff of grass clippings from the chute while driving on grass. No gameplay effect.
8. Check: on an isolated save, collect the 24th acorn and see the unlock; the locked state at 23; the mower loads, sits on its wheels, rider seated, headlights at night, fist wave, first person; one lap of a road course and one mountain jump; a Blender render sheet.

### Verification

Light, per the Verification budget, except Part C, which may check as its own list says. Compile, launch, release steps. Results as a short list, with "for Dan to check".

## Previous delivery — Kyle's house from Dan's photo, Forest Forward Summit Climb shortcut (with a challenge), launcher icon — 0.87.0-review1 — DELIVERED, REVIEWED BY DAN (house accepted; yard, drive and mailbox in 0.88)

- **DELIVERED:**
  - Source `c8357cc8da262d3d377deef0115255b65b48273f` pushed and verified on origin/main (Part B alone is `ad4b1374`, Part A `1b9c5581`).
  - Fresh 0.87.0-review1 Windows build: 0 errors, 2m40s ([build-release.txt](Docs/Report087/build-release.txt)). The 110 warnings are the existing obsolete-API notes of a full recompile and the usual mesh-collider note.
  - Published [game-87000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-87000) with `python Tools/Publish-LauncherRelease.py`: the known draft-lookup miss after the draft was created, then `--resume-draft` uploaded the three assets and published. Previous releases retained.
  - All 234 Latest files match the public signed manifest. Public download, signature, install and startup (with the new launcher) pass ([hosted/result.json](Docs/Report087/hosted/result.json)). The catalog reports no pending game or music update.
- **Play-Racer.cmd (unchanged):** launched a responsive `Builds/Latest/versions/87000/Racer.exe` (0.87.0-review1) through the new launcher, muted, settings restored byte for byte. Latest root, current 87000 and previous 86000 retained; 85000 had already been retired by the updater.
- **Launcher:** `Builds/Latest/WoodstockRushLauncher.exe` is the rebuilt one with the ATV icon (sha256 `ac5b88c9…`); release staging kept it byte for byte.
- **Cleanup:** Builds 10,146,731,046 -> 8,003,768,811 bytes (2.1 GB recovered); also the 1.7 GB hosted-check install, about 100 MB of check scratch outside the project and the temporary editor tools. C: free 270,845,972,480 bytes after cleanup.
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.87 (rule 12).

### Results (2026-10-06, Claude Code)

- **Safety checkpoint:** `dd41e56b` (this plan and the reference photo), pushed. **Part B alone in `ad4b1374`** (revert: `git revert ad4b1374`); Part A in `1b9c5581`; Part C and the results: completion commit `c8357cc8`. Version 0.87.0-review1 / build 87000.
- Evidence: [Docs/Report087/](Docs/Report087/) ([Lists/](Docs/Report087/Lists/), [Shots/](Docs/Report087/Shots/), [Icons/](Docs/Report087/Icons/)). Tools `Tools/Report087/`, `Tools/Blender/kyles_house.py`; checks `Assets/Scripts/Report087Checks.cs`. No VALIDATION.md.
- **B — Summit Climb (Forest Loop Forward, `LakeWoods` only) — BUILT.** Design: [B-design.txt](Docs/Report087/Lists/B-design.txt), [map and profile](Docs/Report087/Shots/B-design-map-and-profile.png); build: [B-summit-author.txt](Docs/Report087/Lists/B-summit-author.txt).
  - Leaves the main at s 1828 on a 46–57 m radius left curve (the main carries straight on past the sign), climbs 217 m up the wooded hillside (40 → 82 m, at most 38 %) and joins the main's last straight at s 2096, about 90 m before the line. Bypasses CP4 (no gate moved). 45 m+ from the House 3 driveway and arch.
  - Challenge: a 5.5 m chute (main 10 m); 14 trees off the trail and the merge sight line, 25 edge trees planted (copies of nearby trees, drawn and with trunks) where the forest left gaps, so the chute is lined with trunks about 1–1.5 m off its edges. **Kink 1** (s 55–85): a 6.5 m offset bend; the old line runs into a 4.6 m boulder (1.2–1.9 m off the edge) and a planted tree on the inside. **Kink 2** (s 125–155): the bend back, round existing trees with planted ones inside. **Rough middle** (s 85–125, the steepest part): low mounds (5–9 cm) left and right, two flat rock slabs raised 0.10 m in the ground with bevelled edges, 8 drawn roots. **Blind crest**: the 38 % climb rolls onto the plateau over a 97 m radius (bikes at 30 m/s go light; cars stay down), then 4–6 % to the merge; the merge is offset left of the straight-ahead line and opens onto the main from the inside with the hook in view.
  - Ground: the two ground tiles there are now LakeWoods-only copies (`Assets/Track/SummitClimb`); balanced cut / fill (about 480 / 620 m³ with batters, deepest 1.9 m). Trees and bushes on changed ground re-seated.
  - Dressing: gold "SUMMIT CLIMB <" sign at the fork (copy of the Echo Cave sign), 5 gold arrows on the branch, 3 teal ones on the main at the fork and rejoin, gold entrance and edge markers (no colliders), dirt trail colour.
  - AI: `aiValidated` on, with an AI line inside the trail (up to 1.2 m off the centre, so the autopilot takes the kinks at about a 38–40 m radius); speed cap 33.
  - Course id `lake-v8-summit-climb` (old times stay as legacy); the seven Forest Forward activity sites keep their v7 results (`ArcadeActivities.ActivityCourse`). Course previews regenerated (only this course changed); the minimap reads the branch itself.
  - **Balance** (race autopilot, one lap each, main s 1826 → 2110; [B-laps-main.txt](Docs/Report087/Lists/B-laps-main.txt), [B-laps-final.txt](Docs/Report087/Lists/B-laps-final.txt), [traces](Docs/Report087/Lists/B-traces/)):

    | | Main | Summit Climb, clean | Saved |
    |---|---|---|---|
    | Needle 600 | 10.58 s | 8.73 s | **1.85 s** |
    | Trail Four | 12.77 s | 8.88 s | **3.89 s** |
    | Skyfin Cruiser | 11.20 s | 9.17 s | **2.03 s** |
    | Highball Fastback | 10.95 s | 8.75 s | **2.20 s** |

    The ATV saves more than the 2 s aimed for: the main's hook (37 % climb, slow hairpin at the top) costs the ATV about 2 s more than the bikes and cars, and the shortcut is about equally fast for all of them. Not tuned further (rule 12).
  - **Mistakes** (same laps, [B-mistakes-races-shots.txt](Docs/Report087/Lists/B-mistakes-races-shots.txt)): wide crest (arriving 2 m right, straight on over it): Needle 600 +2.79 s on the clean run (0.94 s slower than the main), Fastback +1.37 s. Turning into kink 1 0.45 s late: Skyfin +0.05 s, Trail Four +0.55 s (glancing touches), Fastback +7.6 s, Needle 600 +14.4 s (hits a trunk at speed; the autopilot's stuck-reset brings it back); kink 2 late: Needle 600 +5.8 s, Fastback +7.1 s. Going straight on at kink 1 without steering: +6 to +11.5 s. Every mistake recovered (autopilot reset at most twice); nobody stuck. The cost spread is wide: slow, grippy vehicles barely pay, fast ones pay a lot, and the autopilot's reset is slower than a player backing off.
  - The weakest car (Skyfin Cruiser) from a standing start at the foot of the climb: over the crest in 6.8 s, slowest 22.6 m/s after the first 40 m.
  - **AI races** (three, 4 racers each): 0 missed gates, 0 resets, nobody stalled; the Summit Climb taken by the Ridge Scrambler (won), Drifter Twin, Trail Four, Longroof GT and the autopilot's Trail Four.
  - Collider / route comparison ([B-collider-route-comparison.txt](Docs/Report087/Lists/B-collider-route-comparison.txt)): only the two ground tiles, 14 trunk colliders removed and 2 re-seated, 25 edge trunks and the boulder added; routes identical except the course id and the new branch. Shots: [fork](Docs/Report087/Shots/B-fork-from-main.png), [kink 1](Docs/Report087/Shots/B-kink1.png), [rough middle](Docs/Report087/Shots/B-rough-middle.png), [kink 2](Docs/Report087/Shots/B-kink2.png), [crest from below](Docs/Report087/Shots/B-crest-from-below.png), [crest to merge](Docs/Report087/Shots/B-crest-top-to-merge.png), [merge](Docs/Report087/Shots/B-merge-sight-to-hook.png).
  - How it got here: a first version (straight-on fork, solid slabs, 55 m crest) was slower than the main (the autopilot braked to 10 m/s at the fork, stopped for the slabs, and flew 32 m off the crest); a second still lost the cars at the kinks. Both were reverted before the commit.
- **A — Kyle's house — BUILT, all nine scenes.** Model: `Tools/Blender/kyles_house.py` → `Assets/Scenery/KylesHouse/KylesHouse.fbx` (about 6,100 triangles; [Blender previews](Docs/Report087/Shots/A-blender-preview-photo.png)), coloured per slot for the 0.78 building shader (windows and lamps lit at night); build: [A-kyle-build.txt](Docs/Report087/Lists/A-kyle-build.txt).
  - From the photo: the long low side-gabled ranch, grey lap siding, white trim, gutters and downspouts, charcoal shutters, grey roof at a low pitch, the front-facing gable wing on the photo's left, the covered porch across the rest (four slim posts, low slab, white door and storm door, lantern, double-hung windows with grilles, wicker chairs and a bench), brick foundation showing, the red brick chimney with its stepped shoulder and cap on the right end wall. From Dan's description: the screened porch on the right (its door and three steps on its front, onto the front terrace), the walk-out lower level on the left and back with two garage doors on the left, the raised back deck with a sliding glass door and wooden steps down to the yard.
  - Placement: same position and orientation. The site was a flat pad dug into the slope (street side 1–9 m higher, a 4–6 m bank 3 m behind the back wall), so the lower floor is on the pad and the house rose 2.45 m; the front is filled to the front grade (a terrace, 1:2 back to the ground), so it is one storey from the front and the porch one low step up; behind, the bank is cut back to the pad for the deck and stairs (1:1). Still in its hollow below the road: [from the road](Docs/Report087/Shots/A-kyle-from-the-road.png), [from the drive entrance](Docs/Report087/Shots/A-kyle-from-the-drive-entrance.png).
  - Driveway: the old last leg to the front door is cut off (clipped straight across) and the drive continues as gravel down to a 7.6 × 6.8 m apron at the two garage doors. Its route points and the map's "Anderson's" arrival (now on the apron) follow.
  - Colliders: the old House 3 prefab instance, foundation and steps replaced by 22 shapes in the new outline (walls, wing, porch floor and posts, porch roof, roofs, chimney, screened porch, deck, its posts, railings and stairs; garage doors closed). No race line passes within 37 m (Street Loop main at the street, Laurel Switchbacks 42 m), so every scene has the full new outline.
  - Shown in both scenery settings (the 0.78 detail overlay is skipped for it). The two-men vignette and the turkeys are unchanged (20 m away by the street). Trees: 0–1 removed per scene (in the new footprint or drive), 1–2 re-seated on the terrace.
  - Ground tiles shared between scenes (all among these nine) were edited once; the drive's edge-join patch is edited the same way.
  - Checks: shots [photo angle](Docs/Report087/Shots/A-kyle-photo-angle.png) ([next to the photo](Docs/Report087/Shots/A-kyle-photo-vs-game.png), [night](Docs/Report087/Shots/A-kyle-photo-angle-night.png)), [left side with the garage doors](Docs/Report087/Shots/A-kyle-left-garage.png), [back with the deck](Docs/Report087/Shots/A-kyle-back-deck.png), [right side with the screened porch](Docs/Report087/Shots/A-kyle-right-screened-porch.png). Grounding check, all nine scenes ([A-grounding.txt](Docs/Report087/Lists/A-grounding.txt)): walls, porch, chimney, screened porch, deck posts, stairs, steps, garage doors and 24–27 trunks within 30 m: nothing floating, nothing buried. The one flag: grass 6–16 cm over one corner of the gravel where the new leg joins the old drive. Driven from the street to the garage in Free Roam, Skyfin Cruiser and Needle 600: on the apron, nothing touched, steepest pitch 11–13° ([A-driveway-drive.txt](Docs/Report087/Lists/A-driveway-drive.txt)). Collider comparison per scene ([A-collider-comparison/](Docs/Report087/Lists/A-collider-comparison/)): only the house, its drive, the edge-join patch, the ground tile and the re-seated trees; routes, gates and jumps identical in all nine.
- **C — launcher icon — DONE (no signing or release-procedure change).**
  - `Launcher/WoodstockRush.ico` (16–256 px, from the ATV PNG) and `Launcher/launcher.rc`, compiled by `Tools/Build-Launcher.ps1` with the SDK's rc.exe and linked. The launcher window's class icon is set from it too.
  - Rebuilt launcher in `Builds/Launcher` and `Builds/Latest` (sha256 `ac5b88c9…`; the old one, `bc86641f…`, had no icon resource). Embedded icon: [C-new-launcher-icon.png](Docs/Report087/Icons/C-new-launcher-icon.png), the ATV.
  - Manifest format, updater components and keys unchanged.
  - Starter package not rebuilt: `Tools/Package-LauncherStarter.py` is not one step any more (its constants are 0.21-era and its draft folder no longer exists), so a new player still gets the iconless launcher until the starter is rebuilt.
- **Decisions:**
  - B: the climb's line, kinks and crest radius as above; slabs in the ground rather than solid blocks; an AI line inside the trail; activity results kept on v7.
  - A: house lifted onto the pad with a front terrace rather than digging a walk-out into the bank; drive to the garage replaces the leg to the front door; new house in both scenery settings.
  - C: window icon set as well as the exe icon.
- **For Dan to check:**
  - the Summit Climb's feel: kink severity, how the crest reads, the merge with traffic, the mistake costs (fast vehicles pay heavily for a clipped trunk), and the ATV's larger saving;
  - Kyle's house against the real thing: the terrace in front, the garage drive, the corner of grass at the drive joint;
  - the launcher's icon in Explorer (Windows may show the old generic icon from its cache: Win+R, `ie4uinit.exe -show`).

- **Authorized by Dan (2026-10-06).** Written by Claude (chat). Part A (Kyle's house) was queued during 0.86; Parts B and C follow 0.86's survey and icon findings and Dan's choice at 18:13.
- **Starting point:** main at the "Record 0.86 delivery" commit (0.86.0-review1 / game-86000). This TODO edit and `SourceArt/Reference/KylesHouse-front-2026-10-06.jpg` are uncommitted and belong in the safety checkpoint.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–C below. Order: B (own commit), then A, then C.**
- The Verification budget applies, except Part B, which may check as its own list says.

### Part A — Kyle's house

Dan (2026-10-06, with a photo of the front): "this is the front of Kyle's house. It is two stories, which is visible on the left side and back of the house. On the right side is a screened-in porch. In the back it has a raised wooden porch (with a sliding glass door into the top floor). The left side of the house has two garage doors on the bottom level with a driveway leading up to it. The area is generally surrounded by trees and barely visible from the road as it sits down the hill a little, as it is placed currently."

This is the backlog item "Kyle's house from photos" (the building named "Friend across street - blue circle"; 0.78 kept its shape and added detail only). Dan has now supplied the reference, so it may be redesigned.

**Reference photo:** `SourceArt/Reference/KylesHouse-front-2026-10-06.jpg` (placed by Claude; uncommitted until the safety checkpoint). Look at it before modelling. "Left" and "right" below are as seen facing the front, as in the photo.

**What the photo shows (front):** a single-storey-looking ranch from the front, long and low. Light grey horizontal lap siding, white trim, white gutters and downspouts, dark charcoal-grey shutters on every front window, grey shingle roof at a low pitch. Brick foundation showing at the bottom left. The roof is a long side-gable running left to right, with a front-facing gable projecting forward at the left third (the left wing sits a little forward of the rest). To the right of that gable a **covered front porch** runs along the rest of the front under the main roof's eave: four slim white square posts, a concrete or brick porch floor one low step above the ground, a white front door with a storm door near the middle, a wall lantern beside it, double-hung white windows with grilles either side. White wicker chairs and a small bench on the porch. A **red brick chimney** rises on the outside of the right-hand end wall, stepped at the shoulder, with a metal cap. Overgrown shrubs along the front; a gravel parking area in front at the right.

**What Dan describes that the photo does not show:**
- **Two storeys on the left side and the back:** the ground falls away, so the lower level is exposed there (a walk-out lower floor). From the front it stays one storey.
- **Left side, lower level:** two garage doors side by side, with the driveway leading up to them.
- **Right side:** a screened-in porch (framed screens, its own low roof), beside the chimney end.
- **Back:** a raised wooden deck at the upper floor's level on posts, with a sliding glass door from the upper floor onto it, and **wooden steps from the deck down into the yard** (confirmed by Dan). Dan has only this one photo: "the rest can be derived based on the description. Doesn't need to be perfect." So design the left, right and back in keeping with the front and do not stop for more reference.

**Build it:**
1. Model it in Blender with the approved pipeline, in the game's stylized low-poly style, to the standard of the 0.78 buildings (roof overhangs, trim, frames, gutters, lit windows at night). It should be recognisably this house from the front photo.
2. **Keep it where it is now:** same position, same orientation, sitting down the hill a little, surrounded by trees and barely visible from the road. Use the slope that is there for the walk-out lower level on the left and back; adjust the ground immediately around the house only as far as needed for the garage doors, the driveway meeting them and the deck posts (rule 4: everything grounded, no floating or buried parts, foundation to the ground on every side).
3. **Driveway:** the existing drive to this house continues to the two garage doors on the left side. Gravel, as in the photo.
4. Footprint may change to fit the real shape (the L of the front gable, the screened porch on the right, the deck behind). Colliders match the new shape: walls, porch posts, chimney, deck (the deck solid enough to stand on, its underside open or closed as is simplest and safe), garage doors closed.
5. **Which scenes:** `FreeRoamWorld` certainly. In the course scenes the same building exists: replace it there too so the world matches, **but first read each scene's route data** (section 5A): if a race line, shortcut, jump or AI line passes close enough that the new footprint, deck, porch or driveway could touch it, keep the old collider outline on that side in that scene and report it.
6. The two-men vignette at Kyle's (Dan and Kyle, 0.83) stays, moved to the front porch or the gravel in front if the new shape needs it. Trees stay dense around the house; remove only those inside the new footprint or driveway.
7. Scenery: New shows the new house; Classic may keep showing the old one.
8. Check: one shot from the photo's angle next to the photo, one of the left side with the garage doors, one of the back with the deck, one of the right side with the screened porch, one from the road showing it is still tucked away; collider comparison per scene listing only this house and its surround.

### Part B — Forest Loop Forward second shortcut: the Summit Climb, with a real challenge

Dan (2026-10-06, 18:13) chose **candidate C, the Summit Climb**, from `Docs/Report086/ForestForward-shortcut-options.md`: "I like the idea of the shortcut saving time, but it must have some challenge." The survey gave it about 3 s saved per class (bike 2.8, ATV 2.9, car 3.1), the least earthwork, and described it as a dead-straight climb with nothing to miss. **Build it, but not as a free 3 seconds.** Read the survey's section C and its two views first.

Build in `LakeWoods` (Forest Loop Forward) only; section 5A applies to everything already there.

1. **Line (from the survey):** leaves the main just before CP4 at about s 1850, goes up the wooded hillside (48 m up over about 190 m) and rejoins the main's last straight on the plateau about 90 m before the finish line (about s 2090). It bypasses CP4, listed as bypassed the way Echo Cave bypasses CP2; no gate is moved. Keep well clear of the House 3 driveway and its hilltop arch (45 m or more to the west).
2. **The challenge, designed in (Claude's proposal, accepted in principle by Dan's "must have some challenge"):**
   - **A narrow woodland chute.** About half the main's width, with the forest left standing close on both sides: solid trunks at the edges, so a sloppy line clips a tree. Remove only the trees on the trail itself.
   - **Two kinks.** Not dead straight: route the trail round two large trees (or a tree and a boulder) as offset bends, one low and one about two thirds up, each needing a real steering input and a lift or brake for a car at full speed. Bikes may take them flat with a good line.
   - **A rough middle.** The steepest 40–50 m is rooted and rocky ground (low bumps and one or two exposed slabs, not walls) that unsettles a vehicle that is not pointed straight. Ease the gradient here only as far as the survey said was needed (about 38 %) so the weakest car still climbs it.
   - **A blind crest.** Where the climb breaks onto the plateau vehicles go light or take a short hop. The rejoin lies slightly off the straight-ahead line, so the driver has to set the vehicle up before the crest; arriving crooked or too fast runs wide into the trees. Keep it a hop, not a jump: nothing lands on rising ground.
   - **The merge.** It joins the main's last straight from the inside with clear sight of traffic coming round the hook; shape it so two vehicles can merge without a wall between them.
3. **Balance:** a clean run still saves a worthwhile amount: aim for about 2–2.5 s for bikes and cars and about 2 s for the ATV. One clipped tree, a botched kink or a wide crest should cost about as much as the shortcut saves, so it is a real choice. Nobody gets stuck: every mistake is recoverable by driving on, and the reset puts a vehicle back on the shortcut or the main as the nearest-point rule already does.
4. **AI:** validated for all classes at the usual shortcut rate, driving it cleanly most of the time; an occasional AI mistake there is fine and in keeping.
5. **Dressing:** a gold shortcut sign at the fork ("SUMMIT CLIMB"), gold arrows, the standard edge markers, a dirt trail surface like the other forest trails with roots and rock showing in the rough part. The main stays the obvious choice at the fork. Minimap, track-select map and course preview show it as the second gold shortcut.
6. New course-rule ID for Forest Loop Forward (old times stay as legacy). No other scene changes.
7. **Checks (heavier allowed, new race geometry):** timed runs for a bike, the ATV, the weakest car (Skyfin Cruiser) and the fastest car: clean (time saved against the main) and one deliberate mistake each (time lost); the weakest car climbs it from a standing start at the bottom; three races with AI (0 missed gates, nobody stuck, AI uses it); collider and route comparison listing what was added; shots of the fork, each kink, the rough section, the crest and the merge. **Own commit**, with the revert command in the results.

### Part C — Launcher icon (optional, small)

0.86 found `WoodstockRushLauncher.exe` has no icon at all, so Windows shows the generic program icon until the game window opens, and that giving it the ATV needs no signing keys: an `.ico` from the ATV PNG and a resource script in `Tools/Build-Launcher.ps1`, a rebuilt launcher, and replacing `Builds/Latest/WoodstockRushLauncher.exe` (which release staging normally leaves alone) plus a new starter package for anyone else.

Do the first two and replace the launcher in `Builds/Latest` on Dan's PC so his own shortcut shows the ATV. Do not change the manifest format, the updater's component list or any key. Rebuild the starter package only if `Tools/Package-LauncherStarter.py` does it in one step without publishing anything new; otherwise leave it and say so. If anything here turns out to need a signing or release-procedure change, skip this part and report.


## Previous delivery — House 3 pool truly in-ground, ATV icon, Forest Forward second-shortcut survey, Forest Reverse jump approach — 0.86.0-review1 — DELIVERED, REVIEWED BY DAN (chose shortcut C; built in 0.87)

- **DELIVERED:**
  - Source `d491d500c7b8cf832b7f1d0a37eeb233b12cba2c` pushed and verified on origin/main (Part D alone is `8477896a`).
  - Fresh 0.86.0-review1 Windows build: 0 errors, 2m41s ([build-release.txt](Docs/Report086/build-release.txt)). The 110 warnings are the existing obsolete-API notes of a full recompile and the usual mesh-collider note.
  - Published [game-86000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-86000) with `python Tools/Publish-LauncherRelease.py`: the known draft-lookup miss after the draft was created, then `--resume-draft` uploaded the three assets and published. Previous releases retained.
  - All 234 Latest files match the public signed manifest. Public download, signature, install and startup pass ([hosted/result.json](Docs/Report086/hosted/result.json)). The catalog reports no pending game or music update.
- **Icon (Part B):** the 256 px icon extracted from the build's `Racer.exe`, `Builds/Latest/versions/86000/Racer.exe` and `Builds/Latest/Racer.exe` is the ATV in all three, byte-identical ([Icons/](Docs/Report086/Icons/)). The 0.85 `Racer.exe` it replaced was the blue car, which is what Dan saw.
  - `WoodstockRushLauncher.exe` has **no icon resource**, so Windows shows the generic program icon. While the game runs, its window and taskbar button use `Racer.exe`'s ATV.
  - Giving the launcher the ATV would need three things, none involving the signing keys:
    1. an `.ico` made from the ATV PNG and a resource script compiled into the launcher in `Tools/Build-Launcher.ps1`;
    2. a rebuilt launcher;
    3. a way to get it to players. It is not an updatable component (only `game` and `soundtrack` are), so that means a new starter package (`Tools/Package-LauncherStarter.py`) and replacing `Builds/Latest/WoodstockRushLauncher.exe`, which release staging deliberately keeps unchanged.
  - Not done: it is outside the existing release procedure. Say if you want it as its own item.
  - **Windows icon cache:** Explorer may keep showing the car for `Builds\Latest\Racer.exe`, the same path as before. One step refreshes it: run `ie4uinit.exe -show` (Win+R).
- **Play-Racer.cmd (unchanged):** launched a responsive `Builds/Latest/versions/86000/Racer.exe` (0.86.0-review1), muted, settings restored byte for byte. Latest root, current 86000 and previous 85000 retained; 84000 had already been retired by the updater.
- **Cleanup:** Builds 10,127,987,158 -> 7,992,014,436 bytes (2.1 GB recovered); also the 1.7 GB hosted-check install, about 50 MB of check scratch outside the project and the temporary editor tools. C: free 272,837,627,904 bytes.
- **Not mine, left as found:** while this round ran, Claude (chat) added the "QUEUED NEXT — Kyle's house" section below and `SourceArt/Reference/KylesHouse-front-2026-10-06.jpg`. The section is kept as written and committed with this record; the photo is left uncommitted for its own round's safety checkpoint.
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.86 and his choice of a Part C option (rule 12).

### Results (2026-10-06, Claude Code)

- **Safety checkpoint:** `f3ec43d0` (this TODO plan and the two ATV icon files), pushed. Part A in `1e05a110`; **Part D alone in `8477896a`** (revert: `git revert 8477896a`); Part C (proposal only) in `0e5de373`. Version 0.86.0-review1 / build 86000.
- Evidence: [Docs/Report086/](Docs/Report086/) ([Lists/](Docs/Report086/Lists/), [Shots/](Docs/Report086/Shots/)). Tools `Tools/Report086/`, checks `Assets/Scripts/Report086Checks.cs`. No VALIDATION.md.
- **A — House 3 pool in-ground — DONE (all nine scenes).** Measured first: the ground under the pool was its own floor (33.35), the old coping 34.89; the basin floor north of it is level at 33.01–33.08; south, south-west and south-east the pool already sat against the House 3 hill (ground 35–41 at the coping, a near-vertical face behind it). So it was sunk (preferred), not filled around: [A-pool.txt](Docs/Report086/Lists/A-pool.txt).
  - The 0.85 beach entry and outside apron are removed.
  - The pool pieces (water, coping, shell) moved down by D, chosen per scene so the coping top sits 2 cm above that scene's basin floor: D 1.86 m in seven scenes, 1.79 in StreetLoopReverse, 1.81 in FreeRoamWorld. Coping top is now 33.03, water 32.83, floor 31.49.
  - Position, outline, size, depth, materials, water, ice (it follows the water) and the reset are unchanged.
  - The ground tile under the pool (one mesh asset per scene; DansBackyardForward/Reverse share one, edited once) is lowered by D inside the pool's footprint.
  - Outside, ground that stood above the coping is brought down to it: flush within 0.4 m, and the hill foot is lowered by D fading out over 3 m, so the hill keeps its shape and meets the coping. The largest cut is 3.3 m, at the very edge of the south face. Ground below the coping is never touched and nothing is raised.
  - Inside: a 15 % shallow-end slope across the full 18 m width of the north end, from the floor to the coping (dry for its last 1.3 m), in the pool's pale finish. Nothing stands above the coping.
  - Nothing else stands on the changed ground. The only hits were orphaned, undrawn batch vertices.
  - **Granite Saddle (Forest Reverse):** no surface under the flight was raised (every height change is a lowering), so it can only gain clearance. The Part D runs fly over it and land on the trail as before.
  - Checks:
    - Shots: [from Dan's position](Docs/Report086/Shots/A-pool-from-dan.png), [from above](Docs/Report086/Shots/A-pool-from-above.png), [top](Docs/Report086/Shots/A-pool-top.png).
    - Driven in from the basin and reversed back out up the shallow end, Free Roam ([A-checks.txt](Docs/Report086/Lists/A-checks.txt)): Needle 600 out in 11.0 s, Trail Four 8.5 s, Skyfin Cruiser 7.3 s.
    - From the deepest point forwards (as 0.85): out up the slope in 3.3 / 3.4 / 4.0 s. The other three headings face walls, as designed.
    - Collider and route comparison, all nine scenes ([A-collider-comparison.txt](Docs/Report086/Lists/A-collider-comparison.txt)): only the pool pieces and that one ground tile changed; routes, gates, jumps and branches are identical.
- **B — ATV icon — DONE (result under DELIVERED).**
  - `Assets/Branding/WoodstockRushIcon.png` and `SourceArt/Poster/WoodstockRushIcon.png` are identical (622,075 bytes, md5 `feb18da4…`), and the image shows the teal ATV with its rider (no car).
  - The build step force-reimports the icon before building. The Player Settings references are unchanged.
  - **The launcher** (`WoodstockRushLauncher.exe`) is compiled from `Launcher/*.cpp` with no icon resource at all, so Windows shows its generic program icon. It is not an updatable release component (only `game` and `soundtrack` are), and the release staging deliberately keeps the installed launcher byte for byte. Changing it is outside the existing release procedure, so it is not changed (see DELIVERED for what it would need).
- **C — Forest Forward second shortcut — PROPOSAL, nothing built:** [ForestForward-shortcut-options.md](Docs/Report086/ForestForward-shortcut-options.md).
  - Method: the whole lap was surveyed (5,000+ main-to-main lines), timed with the speed each class actually holds on each grade of the measured lap.
  - Finding: away from Echo Cave the main is near the straight line everywhere. Round the House 3 bowl, its near-vertical edges cap any line at about 50 m / 2 s. The real saving is the hook before the finish.
  - **A Ridge Jump** (the required ridge chord, over the tongue behind the pool): about 1.8–2.2 s, about 5,000 m³ of earthwork. The ATV (23 m/s at the lip) cannot clear the same jump as the bikes and cars (31 m/s), so it would need a long built landing and the ATV stays on the main.
  - **B Channel Run and Crest Jump:** works for all ten vehicles on a natural landing with about 1,300 m³, but saves only about 0.5 s.
  - **C Summit Climb** (no jump): about 2.8–3.1 s for every class, about 700 m³, bypasses CP4. **Code would build C**; if the second shortcut must have a ramp, B.
  - No lake or dock jump is possible on this lap: the Friend's lake lies outside the lap, J1 already jumps the creek, and the House 3 lake would need a 20–25 m cut.
- **D — Granite Saddle approach (Forest Reverse) — DONE.**
  - Measured first ([D-approach.txt](Docs/Report086/Lists/D-approach.txt)): the trail has no dip at x ≈ 532. It bends right at the foot of the climb while its outside edge falls away: −28 % across the trail, then a 40–60 % side slope into lower ground 3–4 m below. Vehicles carried wide land on that slope; 0.85's ATV was 8–9 m wide there at 11 m/s.
  - Fix: a local fill on the outside of the bend only (s 62–106, x 520–555; [D-fill.txt](Docs/Report086/Lists/D-fill.txt)). The trail's left half comes up to the centre-line height rising 4 % outward; the shoulder is level to 7 m from the centre; then a 1:1.2 batter to the hillside. Fill only, up to 3.55 m, about 820 m³.
  - Never within the main's half-width + 4 m: the McFadden Cut main, CP1 and its sign run below the bend. One ground mesh changed; no objects or trees stood in the fill.
  - Re-measured the three classes from the branch at full throttle (a player's approach, flying and slow entry):

    | | Lowest speed through the bend | Lip speed |
    |---|---|---|
    | Trail Four | 31.2–31.5 m/s (was 23.9–30.2, running 3.3 m wide instead of up to 6.6) | 35.7–35.9 (was 33.0–34.1) |
    | Street Classic | 29.4 (was 26.8–29.1) | 33.0–33.1 |
    | Needle 600 | 34.3–34.6 (was 27.6–29.8) | 34.4–34.6 |

    All three clear the pool and lake. ATV and car land on the trail at x 308–323.
  - The race autopilot (as 0.85 measured) brakes the ATV on the final ramp by its own speed planning, so it still reaches the lip at 21 m/s. **AI stays off this line** (hard landing; not rebuilt this round).
  - Checks: collider comparison ([D-collider-comparison.txt](Docs/Report086/Lists/D-collider-comparison.txt): only that mesh; routes identical). One race lap of Forest Loop Reverse with AI: 4 finishers, 0 missed gates, 0 resets.
- **Decisions:** the pool was sunk, not the lawn raised. The pool's slope is 15 % (gentler than 0.85's 18.5 %). The Part D fill is outside the bend only, and Part D was measured with a full-throttle follower as well as the autopilot. The launcher is unchanged. In Part C, Code recommends C.
- **For Dan to check:**
  - the in-ground pool from his spot, and the hillside behind it, now lowered to meet the coping;
  - the ATV icon on `Racer.exe` (Windows may show the old one from its icon cache; see DELIVERED);
  - the Granite Saddle bend at x ≈ 532 on the ATV. At full throttle the **faster Needle 600 now catches air over the crest just before the ramp, takes off slightly diagonally and lands beside the trail at about (312, −223)**: upright, no reset, it rejoins. Ride it.
  - Choose a Part C option.
- Not done: no laps of the other course scenes after the pool change; Part A's own check list replaces them, and the change is off every race line.

- **Authorized by Dan (2026-10-06).** Written by Claude (chat) from his review of 0.85.0-review1: debug session `2026-10-06_15-52-45-505_44e69e` (1 report, on 0.85.0-review1, `FreeRoamWorld`) and his message of 16:01. He raised nothing against 0.85 Parts A, B or C.
- **Starting point:** main at the "Record 0.85 delivery" commit; playable source `f0e98977` (0.85.0-review1 / game-85000). This TODO edit and the two replaced icon files (Part B) are uncommitted and belong in the safety checkpoint.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–D below.** The Verification budget in "Mandatory standing workflow" applies in full.
- Permissions: scene edits and publishing are allowed by Dan's user settings (see the 0.85 note). Scene edits are the intended method.

### Part A — The House 3 pool is an in-ground pool: no ramp (BUG-001)

"i don't like the ramp. Its an inground pool. Why cant there just be a slope out?" at (401.4, 33.6, -185.9), `FreeRoamWorld`.

What 0.85 built: the House 3 pool is a box whose rim stands about 1.9 m above the basin floor, so the way out became a 19 m wide paved apron ramping up the outside to the rim plus a beach entry inside (`Docs/Report085/Shots/F-pool-beach-entry.png`). It reads as an above-ground tank with a loading ramp.

1. **Make it truly in-ground.** The coping sits flush with the ground around it on all four sides, the water surface just below the coping, the pool's depth below ground. Do it by sinking the pool into the basin floor (preferred: the ground is level there) or by bringing the surrounding ground up to the coping in a natural, gently graded lawn; whichever leaves the bowl, the lake shore and the trails least changed. No wall, tank side or apron visible above ground.
2. **Remove the 0.85 outside apron entirely.**
3. **The way out is just a slope inside the pool:** the floor at one end rises smoothly to the coping (a shallow end running out to nothing), full width, gentle enough for the weakest of the ten vehicles, in the pool's own surface colour. Nothing sticks up above the coping.
4. Keep its position, outline and size, a narrow pale coping, the water, the ice in Snow and the reset as they are. All nine scenes carry this pool: make the same change in each. Race scenes: section 5A; the Forest Reverse Granite Saddle flight passes over it, so keep that line clear and say what the lower pool does to it (it should only add clearance).
5. Check: one shot from Dan's position and one from above; a motorcycle, the ATV and the Skyfin Cruiser driven in and out; collider comparison per scene listing only the pool and its surround.

### Part B — The icon is the car; it must be the ATV

Dan: "Noticed that the icon being used is the car not the ATV as we discussed." "the icon is exactly the one you first proposed."

**Cause (Claude chat's mistake, found 2026-10-06 16:07):** the file swap to the ATV on the night of 0.84 did not take effect on Dan's PC, so `SourceArt/Poster/WoodstockRushIcon.png` and the copy 0.84 made at `Assets/Branding/WoodstockRushIcon.png` were still the first proposal, the blue car (876,726 bytes). **Both files have now been replaced with the ATV crop (622,075 bytes, md5 `feb18da4bc898d80503378769a849136`), read back from Dan's disk and looked at: both show the ATV.** These two changed files are uncommitted and belong in the safety checkpoint.

1. Confirm both files are the ATV (a teal quad with a rider in a blue shirt and dark hat, no car) and identical. Reimport `Assets/Branding/WoodstockRushIcon.png` so Unity does not build from its cached car import; the Player Settings icon references stay as 0.84 set them.
2. After the build, extract the icon embedded in `Builds\Latest\versions\<n>\Racer.exe` and in `Builds\Latest\Racer.exe`, save them as PNGs in the report folder and confirm they show the ATV.
3. Also extract the icon of `Builds\Latest\WoodstockRushLauncher.exe` (Dan starts the game through it). If it is the Unity logo, a car or anything but the ATV, give it the ATV icon too, **only** if that can be done inside the existing release procedure: never regenerate, replace or expose signing keys (rule). Otherwise report what it shows and what changing it would need.
4. Tell Dan plainly if Windows may still show the old icon from its cache, and the one step to refresh it.

### Part C — Forest Loop Forward second shortcut: survey and propose, do not build

0.85 could not build the Lake Dock Jump: the House 3 lake and pool sit in a closed bowl, the far side is the pool and a hill rising 20–30 m, there is no bank that falls away for a landing, and any line through there is at best about 40 m (1.3–1.5 s) shorter than the main. Dan (16:01): "since it didn't do the shortcut help me figure out what we want." Dan still wants Forest Forward to have two shortcuts like the other courses, and "keeps thinking ramp".

This part produces a proposal for Dan to choose from. **Build nothing; change no scene.**

1. Survey the whole Forest Loop Forward lap in `LakeWoods`, not just the House 3 stretch: for every pair of points on the main where a line across open, drivable ground would be meaningfully shorter or faster than the main, measure the saving (metres and seconds at measured race speeds per class), the ground along it (grades, water, trees, buildings), and what would have to be built.
2. Propose the best **three** candidate shortcuts, at least two of them with a jump, each as:
   - a top-down map picture of the lap with the main, the existing Echo Cave shortcut and the candidate drawn on it, and one or two ground-level views of the spot as it is now;
   - a one-paragraph description a player would understand (where it leaves, what you do, where it rejoins);
   - numbers: length saved, seconds saved per class, approach speed at any jump and the gap and landing that speed supports, how much earth would move, what happens if you miss;
   - risks to existing course features (5A) and to AI.
3. One candidate must be the "ridge jump" 0.85 mentioned (the chord south of the House 3 ridge, no lake), worked out properly. If a lake or dock themed jump is possible anywhere on the lap (the Friend's lake, the J1 creek), include it, since Dan liked that idea.
4. Rank them and say which Code would build and why. Put it all in `Docs/Report086/ForestForward-shortcut-options.md` with the pictures, and summarise in the TODO results.

### Part D — Forest Loop Reverse: let the ATV carry speed to the Granite Saddle jump

0.85 measured the optional Granite Saddle jump after the 0.84 route change: bikes and cars reach the lip at about 31 m/s, clear the pool and lake and land on the trail (hard, but nobody thrown); the Trail Four loses most of its speed in the dip at the foot of the climb (x ≈ 532, down to 11 m/s), reaches the lip at 20 m/s and drops in the lake. 0.85 offered a small local fix.

1. Smooth the dip at x ≈ 532 on the Granite Saddle approach in `ForestLoopReverse` so the ATV (and anything else) keeps its speed up the climb. Local reshaping of that dip only; the main, gates, the lip, the landing, Fern Gully and the driveway are not touched.
2. Re-measure the three classes to the lip. If all clear the water from a normal approach, leave AI off this line anyway (hard landing) and say so; the landing is not rebuilt in this round.
3. Own commit. Check: the three approach traces and one full run each; collider comparison.

### Verification

Light, per the Verification budget: the one check named in each part, compile, launch, release steps. Results as a short list, with "for Dan to check".

## Previous delivery — Forest Forward Lake Dock Jump shortcut (not built), Forest Reverse lake jump (left), escapable pools and lakes, trees off the shortcuts, hands on the steering wheel, Mountain Forward road edge — 0.85.0-review1 — DELIVERED, REVIEWED BY DAN (follow-ups in 0.86)

- **DELIVERED:**
  - Source `f0e9897767f491b49a4993d9675f910be143fd5b` pushed and verified on origin/main (Part F alone is `9502ece4`).
  - Fresh 0.85.0-review1 Windows build: 0 errors, 2m12s ([build-release.txt](Docs/Report085/build-release.txt)); the 110 warnings are the existing obsolete-API notes of a full recompile and the usual mesh-collider note.
  - Published [game-85000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-85000) with `python Tools/Publish-LauncherRelease.py` in one pass (no draft-lookup miss this time). Previous releases retained.
  - All 234 Latest files match the public signed manifest; public download, signature, install and startup pass ([hosted/result.json](Docs/Report085/hosted/result.json)); catalog reports no pending game or music update.
- **Play-Racer.cmd (unchanged):** launched a responsive `Builds/Latest/versions/85000/Racer.exe` (0.85.0-review1), muted, settings restored byte for byte. Latest root, current 85000 and previous 84000 retained.
- **Cleanup:** Builds 10,127,487,845 -> 7,991,676,375 bytes (2.1 GB recovered); plus the hosted check install and 60 MB of check scratch outside the project. C: free 274,210,373,632 bytes.
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.85 and his choice for D / E (rule 12).

### Results (2026-10-06, Claude Code)

- **Safety checkpoint:** `228a4037` (this TODO plan), pushed. **Part F alone in `9502ece4`** (revert: `git revert 9502ece4`). A, B, C and this record: completion commit (see DELIVERED). Version 0.85.0-review1 / build 85000.
- Evidence: [Docs/Report085/](Docs/Report085/) ([Lists/](Docs/Report085/Lists/), [Shots/](Docs/Report085/Shots/)). Tools `Tools/Report085/`, checks `Assets/Scripts/Report085Checks.cs`. No VALIDATION.md.
- **F — escapable water — DONE.** Census of every ShallowWater in all nine scenes ([F-water-census.tsv](Docs/Report085/Lists/F-water-census.tsv)) and a drive-out test (vehicle set down at the deepest point, full throttle, 4 or 8 headings, 25 s each; Needle 600, Trail Four, Skyfin Cruiser as the weakest car): before [F-escape-before.txt](Docs/Report085/Lists/F-escape-before.txt), after [F-escape-after.txt](Docs/Report085/Lists/F-escape-after.txt).
  | Water (scenes) | Before | After |
  |---|---|---|
  | **House 3 swimming pool** (all nine) | **no**: 1.5 m walls inside, 0/4 headings for bike, ATV, car | **yes**: all three drive out up the north end in 3.1–3.8 s |
  | Dan's rear pool (all nine) | yes: its coping is a solid slab at the water level, nothing to fall into | unchanged |
  | House 3 lake (all nine) | yes: 6–8 of 8 headings per class (the misses face the pool wall or the hill) | unchanged |
  | Friend's lake (Forest, Mountain scenes) | yes: 5–6 of 8 (gentle shores 3–28 %; one steep hillside side in Mountain Forward) | unchanged |
  | J1 creek (Forest Forward, Mountain) | yes: 3–4 of 4 | unchanged |
  | Creek surface, Fern creek bed (all) | yes: 4 of 4 | unchanged |
  | Backyard / Free Roam storm-drain ripple strips | shallow channel driven along (0.84 AI races through it) | unchanged |
  Fix (scene edit, all nine scenes, nothing else in them changes): a beach entry across the full 18 m width of the pool's north end (the floor rises at 18.5 % to the coping; the last metre is a dry beach) and outside it a paved apron down to the basin floor at 19 % (19.2 m wide, solid to the ground), both in the pool's pale coping. Water, walls, coping, ice in Snow and the reset are as before. Shots: [beach entry](Docs/Report085/Shots/F-pool-beach-entry.png).
- **A — trees on the Forest Reverse shortcut paths — FIXED.** Cause: the 0.79 "nothing on drivable ground" rule only knew the main's trail, so 0.84 Part K's crown-only clumps given trunks came down on the optional lines. The rule (`SceneryTrees`) now also treats every optional line of a course scene as drivable, within its own half-width + 1.5 m and 2.5 m of its height. A first version also widened the mains by the street half-width; it left out 90 trees *beside* the narrow Backyard Reverse trail, so mains stay on the 0.79 trail-surface test. Check over all nine scenes ([A-trees-check.txt](Docs/Report085/Lists/A-trees-check.txt)): left out on a drivable surface or line: Street 9 / 10, Forest Forward 22, **Forest Reverse 89 (54 on the optional lines: 46 Fern Gully, 8 Granite Saddle)**, Mountain 22 / 22, Backyard 11 / 12, Free Roam 12; drawn and still on a line 0 everywhere; trunk colliders on a line 0 everywhere. Shots along both lines ([Fern Gully](Docs/Report085/Shots/line-ForestLoopReverse-Fern_Gully-200.png), Granite Saddle 60/250/330).
- **B — hands on the steering — DONE.** `RiderGestures`: with no gesture each wrist goes, by the existing two-bone arm IK, to its rest place on the rim / grip turned by the same rotation as the wheel / bars, the elbow bent as at rest, the hand rolling with the wheel; at zero steering this is exactly the old pose. The fist wave / celebration swing from and back to the wheel; the free hand keeps steering. AI riders get it (same component); traffic not touched. Visual only. Check ([B-hands.txt](Docs/Report085/Lists/B-hands.txt), shots): Street Classic at full left / centre / full right, first person and outside: both wrists 0.175 m from the hub all three ways; Needle 600 and Trail Four: hands on the grips, the hand on the far grip 1.4–2 cm short of it at full lock (arm at full reach).
- **C — Mountain Forward road edge (BUG-001) — FIXED for running 2 m wide; see "for Dan".** Measured first: running 2–6 m wide of the road edge at Dan's spot threw a bike into the air for 2.5–3.5 s and down the hillside (Drifter Twin 24 m/s, Needle 600 30 m/s). Not a step in the top surface: two older sheets under the smooth road / trail-edge patch (CR133 earth banks, MountainPolish supported shoulders) climb steeply under it and poke 4 cm through at the patch's edge; and `VehicleSurfaceContacts` measured every body contact against its face's infinite plane, so the steep buried facets that speculative contacts pick up at speed made the body look sunk in them (impulses to 2,500). Fixes: (1) `VehicleSurfaceContacts` never reports a contact deeper than PhysX measured it (the genuine supporting face is unchanged); (2) within 100 m of the spot each way, vertices of those two sheets covered by the road / patch / shoulder layers (up to 3 m under, 0.6 m over them) set 3 cm under that layer ([C-sheets.txt](Docs/Report085/Lists/C-sheets.txt); two mesh assets, Mountain Loop only). Tried and dropped: reshaping only, and a new collider shoulder over the edge (both left the body hitting the buried facets). Checks: ride wide ([before](Docs/Report085/Lists/C-ride-wide-before.txt) / [after](Docs/Report085/Lists/C-ride-wide-after.txt)): Drifter Twin 24 m/s on the edge line 0 s air; **2 m outside the edge 0.32 s air, jolt 47 m/s² (was 2.5 s, 231–546)**; collider comparison: only those two meshes changed, routes / gates / jumps identical; one lap of Mountain Loop Forward with AI: 4 finishers, 0 missed gates (the autopilot reset once, as in 0.83); one lap of Street Loop with AI (shared vehicle code): 0 missed, 0 resets. Still airborne: a Needle 600 at 30 m/s 2.6 m outside the edge leaves the convex shoulder smoothly and lands beyond it, where the hillside steepens about 6 m past the edge; lines 4–6 m outside start there already. No embankment was built (not asked).
- **D — Forest Forward Lake Dock Jump — NOT BUILT (cannot meet the spec in this ground; for Dan to choose).** Measured ([height map](Docs/Report085/Shots/DE-height-map-LakeWoods.png), [bowl from the south](Docs/Report085/Shots/DE-bowl-from-south.png)): the House 3 lake and pool sit in a closed bowl at 33 m. The west bank drops 16 m to the lake; 3–4 m east of the lake is the pool (rim 1.9 m above the basin floor), 2–12 m beyond it the House 3 hill rises to 54–65 m (the 0.84 hump); the only low exits are south / south-west, back along the main's own line. So a flight across the lake (either axis) lands on the pool, the hill or into the closed basin; there is no far bank that falls away for a downhill landing or a run-out back to the main. Distance: main CP3 → CP4 is 410 m, the straight chord 355 m; any line through the lake is at best ~40 m (about 1.3–1.5 s) shorter, and only if it goes through the hill. Speeds on the main there: all three classes about 31.8 m/s leaving CP3. Options: (1) cut a pass ~20–25 m deep through the House 3 hill (Forest Forward only) and jump the lake's north half onto a built landing north of the pool, out through the cut to CP4 (big earthworks, ~1.5 s at best); (2) a smaller jump shortcut on the chord south of the ridge, without the lake (~1 s); (3) leave Forest Forward with one shortcut.
- **E — Forest Reverse Granite Saddle lake jump — LEFT AS IT IS (part E item 4).** Measured on the race autopilot from station 100 at 25 m/s ([DE-approach-speeds.txt](Docs/Report085/Lists/DE-approach-speeds.txt), traces): Needle 600 and Street Classic hold about 31 m/s up the climb (the AI's branch speed), reach the 70.6 m lip at 31 m/s, clear the pool and lake and land on the trail at x ≈ 323 (Needle jolt 674 m/s², nobody thrown); the Trail Four loses most of its speed in the dip at the foot of the climb (x ≈ 532: 11 m/s) and reaches the lip at 20 m/s and drops into the lake (it can drive out, Part F). On lap 1 every class arrives slower (the fork is 38 m after the start line). A "proper downhill landing" here needs the landing face to match a 30–40° descent from a 25–30 m drop for 28–34 m/s, i.e. a trench 15–20 m deep cut into the west rim plus a long climb back out, or a much lower lip with a 60 m+ gap that these speeds cannot clear. AI stays off. Options for Dan: smooth the dip at x ≈ 532 so the Trail Four keeps its speed (small, local) and leave the landing as is; or a larger rebuild with the earthworks above.
- **Decisions:** House 3 pool exit on the north end (open basin floor, clear of the jump lines); Dan's pool unchanged (it cannot be entered); the race-line tree rule covers optional lines only; Part C fixed in the contact code plus two Mountain Forward meshes, no new collider; D and E not built (reasons above).
- **For Dan to check:** the House 3 pool beach entry and apron; hands in first person in a car and on the bikes (and the wave); Mountain Forward running wide after the start (bikes at full speed can still overshoot the shoulder where the hillside steepens); Forest Reverse shortcuts reading clear; choose D / E options.

- **Authorized by Dan (2026-10-06).** Written by Claude (chat) from his review of 0.84.0-review1: debug session `2026-10-06_12-34-02-995_3e8886` (1 report, on 0.84.0-review1) and his message of 12:54. Parts D–F were added at 13:01 after he chose the dock jump and reported being trapped in a pool.
- **Starting point:** main at the "Record 0.84 delivery" commit; playable source `7f341522` (0.84.0-review1 / game-84000). This TODO edit is uncommitted and belongs in the safety checkpoint.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–F below. Order: F, then D, then E (D and E each in their own commit), then A, B, C.** The Verification budget in "Mandatory standing workflow" applies in full.
- **Permissions:** Dan's user settings (`~/.claude/settings.json`, `autoMode.allow`) now allow Unity editor scripts and batch commands that modify and save scene, mesh, prefab, asset and build files in this project, and publishing to `danemoll-jpg/woodstock-rush-releases`. Scene edits are the intended method where a part needs them; do not build load-time workarounds to avoid one. If something is still refused, say exactly what and stop on that item only, carrying on with the rest.

### Part A — Trees standing on the Forest Loop Reverse shortcut paths

Dan: "I notice that there were trees scattered on both shortcut paths on forest reverse. They don't stop the driver though."

Trees with no collision now stand on the driving surface of both optional lines in `ForestLoopReverse` (Granite Saddle and Fern Gully; check McFadden Cut, now the main, too). Most likely cause: 0.84 Part K gave about 1,300 floating crown-only clumps a trunk down to the ground and set 164 visual-only trunks down, and some of those came down on a trail; the 0.79 "nothing on drivable ground" check may not treat the optional lines as drivable in this scene.

1. Remove every tree, trunk, bush and clump (drawn or collidable) from the driving surface of the main and of every optional line in `ForestLoopReverse`, with a margin each side so the trail reads as open. Trees beside the trail stay.
2. Make the drivable-surface check cover main and optional lines in every course scene and `FreeRoamWorld`, run it once over all nine, fix what it finds, and report the counts per scene.
3. Check: one shot along each Forest Reverse optional line.

### Part B — First person in cars: hands turn with the steering wheel

Dan: "noticed in the first POV in cars, the steering wheel moves but the hands don't. Is it possible to fix this?"

1. In all six cars the driver's hands hold the wheel rim and move with it as it turns, with the arms following (the 0.78 arm rig and IK already exist for the gestures). Visible in first person and from outside. Limit wheel rotation shown so the arms never cross or stretch unnaturally; hands may slide on the rim at large angles if that looks better than a tangle.
2. The fist wave still works: the waving hand leaves the wheel and returns to its place on the rim; the other hand keeps steering.
3. Motorcycles and the ATV: confirm the hands stay on the grips as the bars turn in first person and outside; fix the same way if they do not.
4. AI drivers and traffic drivers get the same where it is free; do not spend time on traffic.
5. Purely visual: no steering, handling or input change. Check: first-person shots at full left, centre and full right in one car and on one motorcycle.

### Part C — Mountain Loop Forward road edge (BUG-001)

"fix this" at (740.0, 86.9, -132.5), `MountainLoop`, lap 1, next checkpoint 2, 29 m into the lap, facing 199°. The screenshot shows the bike stopped just off the right-hand edge of the red-brown road on the outside of the bend: the road ends in a hard straight edge with a grey-green shelf beside and below it that does not join the road smoothly, with the hillside dropping away beyond. A vehicle running wide drops off the lip onto the shelf.

1. Find what is wrong there (a step between the road slab and the shoulder, a gap, a shoulder mesh at the wrong height) and make the road edge meet its shoulder in one smooth surface, wide enough that running slightly wide is recoverable. Same care as the 0.83 Mountain Forward fixes: section 5A, keep the race line, gate and the shortcut fork just ahead exactly as they are.
2. Look along the next 100 m each way for the same fault and fix it the same way.
3. Check: ride wide over that edge on a motorcycle at race speed; collider comparison for the scene; one lap of Mountain Loop Forward with AI.

### Part D — Forest Loop Forward: a second shortcut, the Lake Dock Jump

Dan (2026-10-06): "Forest forward currently just has the one shortcut whereas the other tracks all have two. I think there should be a shortcut that is roughly the same place the reverse shortcut is but I am out of new ideas. I keep thinking ramp but I am not sure." Claude offered three ideas; **Dan chose A, the dock jump** ("A is good"). His map screenshot shows the place: the main's long southern curve between the two gates either side of House 3, and the straight line across its chord past House 3, the pool and the lake.

Build it in `LakeWoods` (Forest Loop Forward) only. This is new race geometry asked for by Dan; section 5A applies to everything already there.

1. **Line:** leaves the main where the southern curve begins, runs straight across the chord past House 3 to the lake, and rejoins the main after the curve, so it is clearly shorter than the main. Read the route data first: do not move the main, its gates, the Echo Cave shortcut or any existing jump. Use existing ground and the existing straight path where they serve; the House 3 driveway keeps its straight line (never the winding 0.74 version).
2. **The jump:** a wooden boat dock on the near shore that rises gently along its length and ends in a kicker over the water, like a dock built as a ramp: planks, posts into the lake bed, a rope or rail on the sides, a "LAKE DOCK JUMP" shortcut sign in the gold style. The flight crosses the lake (or its narrow arm) to a **proper landing**: a wide downhill landing slope on the far bank that matches the flight angle, then a smooth run-out back to the main.
3. **It must work at the speed vehicles actually arrive with.** Measure the approach speed of each class on the shortcut first (bikes, ATV, cars; all ten vehicles), then size the gap, lip angle and landing from those numbers so that a clean approach at normal race speed clears it with margin in every vehicle, and only a slow, crooked or hesitant approach comes up short. (The Reverse jump on this ground failed exactly this: vehicles arrived at 19–29 m/s for a jump that needed about 34.) No speed pad or boost; the run-up itself must give the speed.
4. **Coming up short is survivable:** the vehicle lands in the water, which slows it as water already does (ice in Snow, same slowdown), and can drive out on its own by a shelving bank (Part F). It costs time; it does not need a reset.
5. **Risk and reward:** taken well it saves a worthwhile few seconds over the main; missed, it is slower than staying on the main. Report the measured time saved per class.
6. **AI:** validated for AI with the usual shortcut choice rate, only for vehicle classes that clear it reliably in testing; otherwise leave that class on the main and say so.
7. Minimap, track-select map and course preview show it as the second gold shortcut; fork and rejoin get the standard gold arrows and signs; the main stays the obvious straight-on choice at the fork. Free Roam map overlays updated if they draw this course.
8. New course-rule ID for Forest Loop Forward (old times stay as legacy). `FreeRoamWorld` and the other scenes are not changed.
9. **Checks (heavier allowed, new race geometry):** each class through the jump five times at race speed (cleared / short) and once deliberately slow (lands in the water, drives out); landing loads sensible, no vehicle thrown or flipped; three races with AI (0 missed gates, nobody stuck); collider and route comparison listing exactly what was added; shots of the fork, the dock, mid-flight and the landing. **Own commit**, with the revert command in the results.

### Part E — Forest Loop Reverse: make the Granite Saddle pool-and-lake jump work

Offered by Claude alongside Part D; it is the same water from the other side. 0.84 measured that no vehicle reaches the lip fast enough (19–29 m/s against about 34 needed): bikes and cars hit the 55 % far bank and are thrown back, the ATV drops in the lake. It is now on the optional line with AI switched off.

1. In `ForestLoopReverse` only, rebuild this jump by the same method as Part D: measure real approach speeds, then reshape the approach, lip and landing so every vehicle clears it at normal race speed with margin, onto a proper downhill landing instead of the 55 % bank. Ease the 34 % climb on the approach where that is what kills the speed. The House 3 driveway may be adjusted at the crossing as Dan allowed for this scene (simple and direct, never winding). The main (McFadden Cut), gates and Fern Gully stay as they are.
2. Coming up short lands in the water and can drive out (Part F).
3. When it clears reliably, switch AI on for it for the classes that make it, at the usual rate.
4. If it cannot be made to work without redrawing the route, leave it as it is and report why.
5. Same checks as Part D. **Own commit.**

### Part F — Every pool, pond and lake must be drivable out of

Dan: "we need to make the pool escapable. I ended up in the pool and could only reset to get out."

1. Find every body of water a vehicle can get into, in all nine scenes: Dan's pool, the House 3 pool and basin, the lake and its arms, ponds, creek pools, gully and storm-drain water, the mountain water. For each, test whether each vehicle class placed in its deepest part can drive out without a reset.
2. Where it cannot: give it a way out that looks like it belongs.
   - **Swimming pools:** a shallow end that slopes up to the deck (a beach-style entry or wide shallow steps a wheel rolls over), across the full width of one end, gentle enough for the weakest climber among the ten vehicles. Dan's own pool is the accurate one: keep its position, outline, deck and size; only the floor at one end slopes up.
   - **Lakes, ponds, basins:** at least a shelving bank (about 25 % or gentler) on the sides a vehicle is likely to arrive at or leave by, especially under the Part D and E jumps; no vertical lips at the waterline.
3. The water still slows vehicles exactly as now, and ice in Snow is unchanged. The broom-hockey scene on Dan's frozen pool still fits.
4. Race scenes: section 5A. Do not change a race line or gate; these edits are in and around the water. `FreeRoamWorld` gets the same fixes.
5. The reset still works in water as now. Check: for each water body fixed, one vehicle of each class driven in and out; a list of every water body with before / after (escapable yes / no).

### Verification

Light, per the Verification budget, except Parts D and E, which may check as their own lists say. Compile, launch, release steps. Results as a short list, with "for Dan to check".

## Previous delivery — Forest Loop Reverse main route, small things (icon, no idle creep, Forest activities into Free Roam, fences, cave rock, title audio, Start Race pause, Backyard AI gates), roadster windshield — 0.84.0-review1 — DELIVERED, REVIEWED BY DAN (follow-ups in 0.85)

- **Authorized by Dan (2026-10-06).** Written by Claude (chat). Parts A–I were queued during 0.83; Parts J–L come from his review of 0.83.0-review1 (debug session `2026-10-06_05-51-11-694_49829f`, 1 report, on 0.83.0-review1) and his message of 06:01. He raised nothing else against 0.83.
- **Starting point:** main at the last 0.83 commit ("Remove the temporary 0.83 editor tools…"); playable source `da4c8d8c` (0.83.0-review1 / game-83000). This TODO edit and `SourceArt/Poster/WoodstockRushIcon.png` are uncommitted and belong in the safety checkpoint.
- **Runs unattended overnight; Dan is asleep and said "This can be a long update".** Design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–L below. Order: J first (the important one, own commit), then K, B, L, then the rest in any order.**
- The Verification budget in "Mandatory standing workflow" applies, except where a part says it may check more.
- Publishing: `.claude/settings.local.json` allows the publish script (it worked in 0.83).

- **DELIVERED:**
  - Source `7f3415227bb695e24ffea03902c86fd98f13bf22` pushed and verified on origin/main (Part J alone is `e0338a9d`).
  - Fresh 0.84.0-review1 Windows build: 0 errors, 2m16s ([build-release.txt](Docs/Report084/build-release.txt)); the 110 warnings are the existing obsolete-API notes of a full recompile and the usual mesh-collider note. `Racer.exe` carries the new icon (extracted and checked); the running window reads "Woodstock Rush" (it shows "Racer" for a moment while the first scene loads).
  - Published [game-84000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-84000) with `python Tools/Publish-LauncherRelease.py` (the known draft-lookup miss after the draft was created, then `--resume-draft` uploaded the three assets and published, without needing Dan). Previous releases retained.
  - All 234 Latest files match the public signed manifest; public download, signature, install and startup pass ([hosted/result.json](Docs/Report084/hosted/result.json)).
  - Title voice (Part D): five cold launches of the build, each "played to the end (2.75 s clip); frames with no scene listener 0; window focus lost 0 times" ([title-cold-launches.json](Docs/Report084/title-cold-launches.json)). Not reproduced; Dan to say if he still hears it cut.
- **Play-Racer.cmd (unchanged):** launched a responsive `Builds/Latest/versions/84000/Racer.exe` (0.84.0-review1), muted, settings restored byte for byte; catalog reports no pending game or music update. Latest root, current 84000 and previous 83000 retained.
- **Cleanup:** Builds 10,127,476,363 -> 7,991,660,292 bytes (2.1 GB recovered); plus the 1.7 GB hosted check install and 89 MB of check scratch outside the project. C: free 274,166,718,464 bytes.
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.84 (rule 12).

### Results (2026-10-06, Claude Code)

- **Safety checkpoint:** `071b21d4` (this TODO plan), pushed before any change. Work commit `4b81a89a`; **Part J alone in `e0338a9d`** (revert it with `git revert e0338a9d`); completion commit: see DELIVERED. Version 0.84.0-review1 / build 84000.
- Evidence: [Docs/Report084/](Docs/Report084/): [Shots/](Docs/Report084/Shots/) (one per check), [Lists/](Docs/Report084/Lists/) (check results, J profile / traces / census, K trunk list, C sites, H gate survey), [J chart](Docs/Report084/J-height-and-speed-before.png). Tools `Tools/Report084/`, checks `Assets/Scripts/Report084Checks.cs`. No VALIDATION.md (Verification budget).
- **Auto-mode note:** the first scene-editing script (Part K) and a load-time workaround for Part I were refused by Claude Code's auto-mode safety check; Claude stopped and asked. Dan gave standing approval in chat for editor scripts that modify and save scene / mesh / asset files for this session, Part K's trunk lowering and Part I's removal, and chose Part J option 1. The load-time workaround was removed; K and I are scene edits.
- **J — Forest Loop Reverse main route — DONE (Dan's option 1: McFadden Cut is the main, Granite Saddle the optional line).** Measured first (no geometry changed): the "hump" is the approach to the 0.31 House 3 pool-and-lake jump on Granite Saddle: from CP1 (station 205, 42 m) a 34 % climb to 64 m, the ramp to the lip at 71.6 m (station 337), the flight over the pool and lake (water at 33 m) and a 55 % lake-exit bank at x 352. At race speed every vehicle reached the lip at 19-29 m/s (the 0.31 jump was built for about 34): the Needle 600 and the Street Classic landed on the exit bank and were thrown back (speed -13 / -4 m/s), the Trail Four fell into the lake; McFadden Cut held about 31 m/s. Lowering the hump would have removed the jump and put the main through 60 m of water, so per the plan no geometry changed and Dan chose. Now (route data, gate, jumps, guidance and signs only; no mesh, terrain or collider change): the main between the L04 fork and the L05 rejoin is McFadden Cut (main 2076.8 -> 2103.3 m); Granite Saddle (with the pool-and-lake flight) is the optional branch, bypasses CP1, `aiValidated` off so AI never takes it; CP1 moved onto McFadden Cut at the same fraction of the stretch (555.6, 43.9, -206.7 -> 564.3, 37.8, -221.5), gate order unchanged; the pool-and-lake flight is off the main jump list, the later jumps, Fern Gully and gates remapped; 4 teal arrows on Granite turned gold, the 4 McFadden gold arrows teal, signs re-lettered (McFADDEN CUT / MAIN COURSE, GRANITE SADDLE / OPTIONAL, Granite Saddle / OPTIONAL, MAIN COURSE / McFADDEN CUT), the one MAIN ^ cue on the Granite side hidden; course previews regenerated (only Forest Loop Reverse changed); records ID `forest-reverse-v8-mcfadden-main` (old times stay on the board). Checks: census before/after: colliders differ only in the 235 Part K trunks, routes only in the main, the Granite/McFadden branch, Fern Gully's stations, CP1, the jump stations and the course ID; motorcycle, ATV and car down the new main at race speed: 0 resets, longest air 1.4 s (was 3.4-3.6 s), nobody thrown back; one run down Granite Saddle: through, 0 resets (hard landing as before, 1009 m/s²); three AI races: all finished, 0 missed gates, nobody on Granite Saddle. Shots: before / after from Dan's position and at the fork.
- **K — floating trees (BUG-001) — FIXED.** Cause: later edits lowered the ground under trees (the 0.31 pool basin and lake-exit bank, the hill smoothing, the cuttings) and left them hanging (up to 10 m); also 0.78's trunkless crown-only clumps hang in the air. Scene: 235 trunk colliders lowered onto the ground under them (none on a route; list in Lists/K-trunks.txt). Drawn (ForestLoopReverse only): visual-only trunks set down (164), crown-only clumps over 0.5 m up given a trunk to the ground (about 1,300), clumps hanging over a trail left out. Checks: floating check over the scene: 1 low crown (0.35 m) left, none within 250 m of the report; the 0.79 grounding check passes (buildings 47/47, posts/rocks 72, nothing on drivable ground, colliders identical New/Classic). Shot from Dan's heading.
- **B — no idle creep — FIXED.** Causes: no holding force at rest (a 15 % slope pulled a stopped vehicle along at 2.5 mph; coasting from 20 mph never stopped there) and a resting trigger read as throttle. Now, on the player's own vehicle only, with no throttle / brake / reverse below 5 m/s the ground holds it (slope pull cancelled, a stopping force fading in from nothing at 5 m/s); both triggers have a 10 % dead zone (full press still 1). AI and traffic unchanged. Check (car, ATV, motorcycle; flat and 15 % downhill; from a stop and 20 mph): 0 mph and staying there in every case (before: 2.5 mph creep on the slope); Needle 600 Street Loop autopilot lap 2:06.878 before / 2:06.858 after; a pad at 6-9 % reads 0.
- **L — Sundown Roadster windscreen — FIXED.** The top rail was 8 cm below the driver's eyes (13 degrees below the eye line, across the middle of the view). Blender (`cars81.py`): raked further back and raised to 0.86 m, about 17 degrees above the eye line, same slim chrome posts and rail; eye point unchanged. The other nine vehicles: nothing crosses the centre of the view (A-pillars at the side, bars and mirrors low); no change. Shots: before / after and all ten.
- **A — icon — DONE.** `Assets/Branding/WoodstockRushIcon.png` (byte copy of the poster crop) is the default and every Standalone icon size (16-1024). Window title set to "Woodstock Rush" at startup (`WindowTitle.cs`; productName stays "Racer"). The launcher and Play-Racer.cmd create no shortcut, so nothing else changed.
- **C — Forest jump and speed traps in Free Roam — DONE.** New FreeRoamWorld sites (the course-scene sites and their results stay as they were; in Free Roam the ids jump-01 / speed-0 / speed-1 are the Trickum sites, so these are forest-*): **Pool-house jump** `forest-jump-01` at the Dan's Backyard Reverse pool-house launch (388.2, 86.1, -3.3), flying east by Dan's house (targets 12 / 22 / 32 m); **S Cherokee hill speed trap** `forest-speed-0` half way down the S Cherokee Ln hill (318.6, 43.7, 404.7), both ways (18 / 26 / 34 m/s); **Hwy 92 east speed trap** `forest-speed-1` on Hwy 92 east of the S Cherokee junction (481.0, 18.9, 561.4), both ways (20 / 28 / 36 m/s). Signs as at the other sites; each a map destination (found within 45 m) and on the minimap. Check (motorcycle, ATV, car): each trap both ways scored (57-63 mph), the jump scored 29-32 m at 30 m/s, the on-site prompt shown.
- **D — spoken title clipping (CR-118) — NOT REPRODUCED; safeguards added.** In the editor no frame of the phrase was without an AudioListener, pressed mid-phrase or not. Added: the voice has top priority; while the phrase plays the title keeps its own listener if the scene has none (scene changes) and the player keeps running unfocused (launched from the launcher, Run In Background is off), restored afterwards; Player.log gets one line on how it played (frames without a scene listener, focus losses). Cold launches: see DELIVERED.
- **E — fences — DONE.** The merged fence sections (wood crossbucks with X braces, the neighbours' board fence, roadside chain-link: 146 in Free Roam) are rebuilt piece by piece with chamfered edges inside their outline, three slightly different wood tones on the wooden ones, posts lengthened to the ground (532); lines, heights, rails, braces and colliders unchanged (Scenery: New only). Checks: shots by Dan's driveway and on a 6.6 m slope (new / classic); static colliders identical New / Classic / New (13,614).
- **F — cave rock — DONE (decoration only).** Restyled: the cave decoration rock with no collider of its own (fallen stones, hanging formations, buttresses, shoulder outcrops, ceiling stones; 208 pieces in Forest Forward, 68 in Forest Reverse, and the same Echo Cave pieces in the Mountain scenes), pulled inward only (at most 12 cm), so nothing grows into an opening or the driving space. **Left as they are (risk):** the collidable cave-edge boulders, the Mountain Cut rock vault and portal outcrops (driving surface), floors, puddles, roots. Scene files untouched. Checks: Echo Cave (Forest Forward) and the Forest Reverse cave section (Fern Gully) on a motorcycle and a car: through, 0 resets; mouth and night shots. The Echo Cave is not on any Forest Reverse route, so it is driven one way only.
- **G — Start Race pause — FIXED.** START RACE now brings the loading screen up at once and builds the rivals and traffic behind it (the countdown waits for it, as after a scene load). Check (three presses): the press 1 ms, screen up at once, the countdown starts 0.75-0.97 s later with its full 3 s; the 0.56-0.67 s build frame is behind the screen (before: the menu froze about 0.47 s and the next frame took 0.5 s).
- **I — stray trunk — DONE.** The trunk collider at (207.98, 72.35, 89.76) removed from Dan's Backyard Reverse and from Forward (it was there too). Nothing else in those scenes changed.
- **H — Backyard AI gates — IMPROVED, NOT ZERO (for Dan).** Every gate sits on the trail centre, square to it, 3.3 m half-width. Causes found: (1) on tight bends the AI's look-ahead cut the inside by 4-7 m past the gate (Forward CP 1, 2, 3; Reverse CP 5); (2) the Dirt crest (CP 1) and the Downhill kicker (CP 3) send riders the way they face at the lip, landing 6-25 m wide; (3) at the end of the Storm Drain / Gully Jump shortcut the AI aimed back at the shortcut's last point and cut the CP 4 hairpin (Reverse). Fixes, Dan's Backyard only: the AI steers for its next gate's centre once within its look-ahead, and from the start of a jump approach when the gate is beyond the jump; at a shortcut's end it carries on along the main; detection half-width widened where the line passes just outside (Forward CP 1 to 9 m, CP 2 and Reverse CP 5 to 7.5 m; drawn gates unchanged). No track geometry moved. Results, three AI-led races each way (four racers each; the player on the race autopilot): **Reverse 0 missed gates in all three** (before: 3-4 per three races, CP 4 and CP 5); **Forward 4 missed gates in three races** (before: about 15; CP 1, 2, 3 and 8 now clean in the final set), 3 of them at CP 6. **Left for Dan:** Forward CP 6 (a racer that strays from the main onto the lower Abandoned Cabin trail half way along, or sticks there after a reset) and CP 8 (motorcycles running wide on the last bend before the finish); cars still reset more on Backyard Forward than bikes.
- **Decisions:** Part J option 1 (Dan); Free Roam site ids forest-* (id clash) and names for the new places; Pool-house jump at the Reverse course's launch; fence tints only on wooden fences; cave rock only where it has no collider; H fixes limited to Dan's Backyard.
- **For Dan to check:** Forest Loop Reverse on McFadden Cut and how the fork reads; the Granite Saddle jump as an optional line; the three Free Roam sites; first person in the Roadster; idle hold on his slopes; the fences and cave look; the Backyard AI.

### The plan as authorized

### Part A — Windows icon

Dan (2026-10-06): "I want to change the Windows Icon so it isn't the unity logo also."

1. The icon artwork is in the project: `SourceArt/Poster/WoodstockRushIcon.png` (1024×1024, the ATV and rider from the poster, cropped by Claude; Dan chose the ATV on 2026-10-06 and may revisit it). The source crop is small, so the large sizes are soft; that is accepted. Import it and set it as the default icon in Player Settings so `Racer.exe`, the window, the taskbar and Alt-Tab show it at every size Windows asks for (16 to 256).
2. Also give the launcher the same icon where one applies: a desktop/Start shortcut if the launcher or `Play-Racer.cmd` creates one; do not change how the launcher works, its signing, or the release manifest format.
3. **Do not change `productName` ("Racer") or the company name**: the save folder (`AppData\LocalLow\DefaultCompany\Racer`), records, settings and debug reports depend on them. The window title may be set to "Woodstock Rush" at runtime if that can be done without touching `productName`; otherwise leave it and report.
4. Check: after the build, the exe in `Builds\Latest` shows the new icon in Explorer and on the taskbar while running. Windows caches icons, so judge from a freshly built path or after clearing the cache, and tell Dan if he may need to do the same.

**More items for this round (Dan, 2026-10-06: "Maybe we can handle a bunch of the small things next"). The numbered list above is Part A; the parts below are B–I, and J–L were added from the 0.83 review.**

### Part B — Vehicles must not creep with no throttle

Dan: "When you don't have the gas pushed, the vehicles tend to move anyway, they shouldn't."

1. With no throttle, no brake and no reverse input, a vehicle that is stopped stays stopped, on flat ground and on ordinary slopes (roads, driveways, trail grades), like an automatic holding itself. A moving vehicle coasts down and comes fully to rest; it does not keep crawling at a few mph.
2. Find the cause before changing numbers (idle drive torque, a minimum-speed floor, analogue trigger or stick noise read as throttle, wheel friction too low at rest, slope force with no holding force). If it is input noise, add a proper dead zone for both triggers and the keyboard path.
3. Do not change how the vehicles drive once the player is on the throttle: same acceleration, top speed, grip and coasting feel at speed, so lap times and records stay comparable. On very steep ground (the mountain's steepest faces, jump ramps) a stopped vehicle may still slide; that is fine.
4. Applies to all ten vehicles. AI and traffic behaviour unchanged.
5. Check: on flat road and on S Cherokee's hill, release everything from a stop and from 20 mph on a car, the ATV and a motorcycle: 0 mph and staying there. One Street Loop lap time on the Needle 600 against the same lap before the change, to show driving is unchanged.

### Part C — Forest jump and speed traps moved into Free Roam

0.76 reported that three Free Roam activities sit on race-only Forest trails and so do not exist in `FreeRoamWorld`: the Forest opening jump and Forest speed traps 1 and 2. Dan (2026-10-06): the Forest cave "was causing issues since it was in the area of backyard so I thought we were getting rid of it; move the jump and speed traps somewhere that is visible on free roam."

1. **The cave stays out of Free Roam** (it remains in the Forest races only). Settled; do not raise it again.
2. Give the jump and the two speed traps new homes in `FreeRoamWorld`, on ground that exists there, each in a place a player will actually see while roaming: beside or on a real road or a well-used trail, not hidden in woods. Speed traps on stretches where real speed is possible (Hwy 92 and a long run of Trickum Rd or S Cherokee Ln are the obvious candidates); the jump where there is already a natural launch or where an existing ramp-like feature can be used. **Do not build new terrain or ramps for this**; choose spots that work as they are.
3. Each shows on the Free Roam map and minimap as a site, with the usual on-site prompt, and works with every vehicle. Keep their ids, names (rename only if "Forest" no longer fits the place), medal targets adjusted to the new spot, and existing saved results where an id is unchanged.
4. Report where each one went (map position) so Dan can find them. Check: run each once.

### Part D — Spoken title clipping (CR-118)

The spoken title at startup is sometimes cut off. With 0.82's loading screen now up before the first frame, find where the clip is started or stopped early (scene change, audio source destroyed, another sound taking the channel, the loading screen) and make it always play in full. Check: five cold launches, heard in full each time; if it never reproduces, say so and close it.

### Part E — Fences

The 0.78 scenery round left the fences as the original merged blockout meshes. Give them the same treatment as the rest of the scenery (Scenery: New only): proper posts, rails and the diagonal braces where they have them, wood colour variation, posts that follow the ground (no floating or buried sections). Same lines, heights and colliders as now: visual only. Check: one shot of the fence by Dan's driveway and one on a slope.

### Part F — Cave, vault and portal rock

Also left unchanged in 0.78. Bring the cave / vault / portal rock in the Forest and Mountain race scenes up to the faceted-rock look of the other 0.78 rocks. Visual only, colliders and openings exactly as they are (section 5A), headlights and the cave atmosphere still working. **Dan (2026-10-06): fine "as long as it doesn't mess it up again"; the caves have been broken by well-meant changes before (0.74).** So: change only what is drawn. Do not edit, move, rebuild or re-save any cave collider, trigger, route, gate or the cave geometry in the scene files; draw the new look over or in place of the old renderers at load, as 0.78 did for other scenery, behind the Scenery: New switch so Classic still shows the original. The new rock must not narrow any opening or hang into the driving space: keep it on or behind the existing surfaces. If a piece cannot be restyled without that risk, leave that piece as it is and say so. Check: the collider comparison for each scene touched (identical), drive through each cave once each way on a motorcycle and a car without touching anything new, one shot at each cave mouth and one inside with headlights.

### Part G — The pause when pressing Start Race

0.82 left a pause of about 0.3 s when START RACE is pressed while the AI vehicles are built. Build them behind the loading screen (or spread the work over frames before the countdown) so the press responds at once. Check: press it and watch.

### Part H — AI missing gates on Dan's Backyard

0.80 and 0.76 both noted that AI rivals sometimes miss a gate on Dan's Backyard, forward and reverse, with any vehicle. Find which gates and why (AI line passes outside the gate, a gate set too narrow for the line, a shortcut merge) and fix it by correcting the AI line or the gate's tolerance at that spot. Do not move track geometry. Check: three AI-only or AI-led races on each direction, 0 missed gates; this one check is allowed to be heavier because the fault is intermittent.

### Part I — One stray trunk on the Backyard Reverse trail

0.79 kept a hidden tree-trunk collider at (208.0, 72.35, 89.8) in Dan's Backyard Reverse, 2.3 m from the race line, because it was in a race scene. **Dan has approved removing it (2026-10-06).** Remove that collider (and the same one in Dan's Backyard Forward if it is there too). Nothing else in those scenes changes.

### Part J — Forest Loop Reverse: make the main route feel like the main route

Dan (2026-10-06, after racing 0.83): "I still feel that the main track for Forest reverse is awkward mainly due to the hump that you have to hit going there. The shortcut is the much more natural way to go. Is there something that can be done to make the main route less weird and actually make the drivers think that is the intended route?" He has given the whole night to this round: "This can be a long update."

**Background Code must read first (search the archive, do not guess):** the main here is the **Granite Saddle** line, promoted to main in `forest-reverse-v6-granite-main` (east fork L04 near (600, -162) to west rejoin L05 near (200, -175), passing House 3), later `forest-reverse-v7-water-detour`. Commit `14240a16` put the House 3 driveway support across that trail and created a hill on it; a "hill fix" followed and Dan accepted it only as "imperfect but humanly drivable for now … temporary usability acceptance, not acceptance of the whole area". `Docs/RouteAtlas/Granite-height-comparison.png` shows the old profile in this scene: a climb from about 42 m to about 64 m and then a drop to about 35 m within roughly 100 m, where the authored line wanted about 43 m. Also read what 0.74 did to the House 3 driveway and what 0.76 restored: **Dan hated the 0.74 result; the straight House 3 driveway he chose stays.** Dan's screenshot (session `2026-10-06_05-51-11-694_49829f`, BUG-001) is taken on the main at (285.6, 45.5, -196.7), main progress 522 m, with the gold shortcut forking right on the minimap.

**Step 1 — measure and say what is wrong.** In `ForestLoopReverse`, sample the real driving surface along the main from L04 to L05 and along the shortcut(s) that fork from it: height profile, grade, crest sharpness (where a vehicle at race speed leaves the ground or bottoms out), width, and sight line at the fork. Identify "the hump" precisely (station, height, what object causes it) and which shortcut Dan means. Record both profiles in one chart.

**Step 2 — fix the hump on the main (race-scene geometry change, explicitly requested by Dan; section 5A applies).**
- Keep the main's X/Z line, gates, checkpoints, start/finish, jumps (J1 near the west rejoin stays a jump) and the shortcut exactly where they are. Change heights only, and only within the hump's span plus the blend each side.
- Target: the main through here drives as a flowing fast trail. No crest that throws a vehicle or hides the road beyond it, no drop it falls off, no wall it climbs: grade no steeper than about 12 %, smooth vertical curves so all ten vehicles stay on their wheels at race speed, and a width at least equal to the main elsewhere.
- Preferred method: a **cutting**. Lower the trail through the hump toward the authored line and shape banked earth sides (same look as the nearby banks), so the trail passes through the rise instead of over it. If the hump is the House 3 driveway's embankment, the driveway keeps its present straight line and slope and is carried over the cutting on a short, simple timber or concrete deck with posts (collidable, wide enough for a car), or meets the trail at a level crossing, whichever leaves the driveway least changed. **Dan (2026-10-06, 06:05): changing the driveway or moving a gate is fine "if it only needs to change it in the race instance but not in the free roam world".** So in `ForestLoopReverse` only, if the cutting needs it, the House 3 driveway may be regraded, shortened or shifted near the crossing, and a gate or checkpoint on this stretch may be moved to suit the reshaped trail. Limits: keep the driveway a simple, direct driveway (never the winding 0.74 version, never a loop round the lake), keep the lap the same route with the same gate order, and prefer the smallest change that makes the main flow.
- Trees, rocks, signs and props on changed ground are re-seated or removed so nothing floats or is buried (rule 4). Colliders match what is drawn.
- Only `ForestLoopReverse` changes. `LakeWoods` (Forest Forward), `FreeRoamWorld` and every other scene stay untouched.
- **If this cannot be done cleanly even with that freedom** (it would break a jump or the shortcut, or need the route itself redrawn), change no geometry: report the measurements and the options, including the alternative of making the natural route the main and Granite Saddle the optional line, for Dan to choose.

**Step 3 — make it read as the intended route (no blocking of the shortcut).**
- At the fork, the main must be the obvious straight-on choice: its mouth at full width with a continuous surface, teal chevrons or arrows placed where a driver looks on approach, and the view down the main open (clear sight-blocking bushes or props on changed ground).
- The shortcut reads as a side trail you choose: a visibly narrower mouth or a change of surface at its entrance, and the gold shortcut marking and sign as elsewhere. It stays fully drivable and as fast as it is now.
- AI: the main line through the reshaped section is followed cleanly by all vehicle classes; AI shortcut choice rates unchanged.

**Records:** the main's surface changes, so give Forest Loop Reverse a new course-rule ID (as v6 and v7 did); old times stay visible as legacy.

**Checks (this part may use heavier checking than the budget, because it changes a race course):** the before/after height chart; collider and route comparison for the scene listing exactly what changed; each of a motorcycle, the ATV and a car through the section at race speed on the main without leaving the ground or resetting; three races of Forest Loop Reverse with AI (0 missed gates, nobody stuck at the old hump); one run through the shortcut; before/after shots from Dan's screenshot position and from the fork. **Make this part its own commit** so it can be reverted alone, and say how in the results.

### Part K — Floating trees on Forest Loop Reverse (BUG-001)

"floating trees" at (285.6, 45.5, -196.7), `ForestLoopReverse`, Dawn / Clear, session `2026-10-06_05-51-11-694_49829f`, on 0.83.0-review1. A tree trunk hangs in the air above the trail ahead with nothing under it, and crowns nearby sit off the ground. Find why (a trunk seated on ground that this scene does not have, a crown-only clump from 0.78, a tree left behind by earlier terrain edits here) and seat or remove them. Then run the 0.79 tree and prop grounding check over this scene and fix whatever it finds, since this area has had its ground changed several times. Do this after Part J's reshaping so the result is final. Check: one shot from the reported position.

### Part L — Sundown Roadster: windshield blocks the first-person view

Dan: "for the sundown roadster, the windshield is too low so if you are using first person pov, the top of windshield blocks your vision."

1. In first person in the Sundown Roadster the windshield's top frame crosses the middle of the view. Fix it in the Blender model: a taller, properly raked windshield whose top frame sits clearly above the driver's eye line, with clear glass, so the road ahead is seen through the glass and the frame is at the top edge of the view. Keep it looking like a 1960s roadster (a slim chrome frame, not a tall modern screen). Adjust the first-person eye point only if needed and keep it at the rider's eyes.
2. Then look once through first person in the other nine vehicles and fix any where a frame, roof edge, mirror, handlebar or gauge blocks the centre of the view the same way; list what was changed.
3. No handling or collider change. Check: one first-person shot from each car on a straight road.


**Closed by Dan on 2026-10-06, do not carry forward:** CR-010 tighter steering ("ancient history"); skip-ahead time in Free Roam; burying the storm-drain box.

## Previous delivery — Scene characters (Dan, Kyle, brother), always-on track map, poster loading screen, winner's head, plain Top 10 records, two Mountain Forward fixes — 0.83.0-review1 — DELIVERED, REVIEWED BY DAN (follow-ups delivered in 0.84)

- **Authorized by Dan (2026-10-06).** Written by Claude (chat) from his review of 0.82.0-review1: debug session `2026-10-06_03-13-11-334_72b40c` (2 reports, both on 0.82.0-review1, Mountain Loop Forward, race, Night / Snow, Drifter Twin) and his written requests, quoted in each part.
- **Starting point:** main at the "Record 0.82 delivery" commit; playable source `27a3fa98` (0.82.0-review1 / game-82000). This TODO edit and the new file `SourceArt/Poster/WoodstockRushPoster.png` are uncommitted and belong in the safety checkpoint.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–G below.** The Verification budget in "Mandatory standing workflow" applies in full.
- **Publishing:** Dan has added `.claude/settings.local.json` (via Claude chat, 2026-10-06) allowing `python Tools/Publish-LauncherRelease.py` so the publish step should no longer be blocked or need him. If it is still blocked, do not wait silently: say exactly which command was refused and what rule would allow it.

- **DELIVERED:**
  - Source `da4c8d8ca239417cd5a96173b630f7fe6338a62f` pushed and verified on origin/main.
  - Fresh 0.83.0-review1 Windows build: 0 errors, 3m00s ([build-release.txt](Docs/Report083/build-release.txt)); the 106 warnings are the existing obsolete-API compiler warnings of a full recompile and the usual mesh-collider note, none from this round's runtime code.
  - Published [game-83000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-83000) with `python Tools/Publish-LauncherRelease.py` (allowed by Dan's `.claude/settings.local.json`, gh from `Builds/PublisherTools` on PATH): the known draft-lookup miss after the draft was created, then `--resume-draft` uploaded the three assets and published, without needing Dan. Previous releases retained.
  - All 234 Latest files match the public signed manifest; public download, signature, install and startup pass ([hosted/result.json](Docs/Report083/hosted/result.json)).
- **Play-Racer.cmd (unchanged):** launched a responsive `Builds/Latest/versions/83000/Racer.exe` (0.83.0-review1), muted, settings restored byte for byte; no pending update. Latest root, current 83000 and previous 82000 retained.
- **Cleanup:** Builds 10,121,260,414 → 7,985,672,669 bytes (2.1 GB recovered); plus the 1.7 GB hosted check install and the 58 MB check scratch outside the project. C: free 275,077,238,784 bytes.
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.83 (rule 12). QUEUED NEXT (0.84) waits for Dan to start it.

### Results (2026-10-06, Claude Code)

- **Safety checkpoint:** `5469c1b7` (this TODO plan and the poster), pushed before any change. Version 0.83.0-review1 / build 83000.
- Evidence: [Docs/Report083/](Docs/Report083/): [Shots/](Docs/Report083/Shots/) (one per check), [Lists/](Docs/Report083/Lists/) ([check results](Docs/Report083/Lists/checks.txt), [A/B surface edit](Docs/Report083/Lists/AB-surface.txt)). No VALIDATION.md (Verification budget).
- Code: A/B the mesh asset `Assets/Track/ReportCleanup/forward-bend-final.asset` (MountainLoop only; authored by `Tools/Report083/Report083Author.cs`); C `ScenePeople`, `RiderLook`, `VehicleVisual`, Blender `Tools/Blender/rider.py` + `kit.py` (`Rider.fbx` / `Rider.blend` rebuilt); D `RaceMenus.TrackPreview.cs` (rewritten), `RaceMenus.Core`, `RaceMenus.Shell`, `WorldMapCourseOverlay`; E `LoadingScreen`, `Assets/Resources/LoadingPoster.png` (byte copy of the poster; import settings by `Tools/Report083/Report083Import.cs`); F `CameraViews`, `CameraViews.Trailer`, `WinnerShot`; G `RaceMenus.RecordsResults`, `RecordBoards`, `RaceMenus.Core/Shell`. Checks `Report083Checks.cs`, tools `Tools/Report083/`.
- **A — the spot that flips everyone (BUG-001) — FIXED.** Cause: on the summit deck around the hairpin (forward s 1850–1960), pairs of mesh vertices a few cm apart sit 1–3 cm apart in height, so the triangles between them are slivers tilted up to 30°. `VehicleSurfaceContacts` gives a body contact the touched face's own normal, so a bike touching a sliver gets pushed sideways and up. Measured before: motorcycle at 32 m/s through the hairpin exit thrown off its wheels at (1255.9, 152.6, 231.8), 0.5 s in the air, up-vector down to 0.79, the contact on a sliver with normal (0.26, 0.96, 0.06), impulse 349. Fix: within 45 m of the spot, each cluster of vertices (within 0.15 m across, under 6 cm apart in height) gets one height, its mean: 18 clusters, 71 vertices, largest change 1.8 cm; faces tilted over 8° there 6 → 0. Not a lip in the race line; no 0.80/0.81 object was involved. Check: Needle 600 at 32 and 36 m/s and Drifter Twin at 28 m/s through the hairpin, all wheels down the whole way, up-vector 1.00, largest vertical speed 0.08 m/s.
- **B — broken road surface (BUG-002) — FIXED.** Cause: where the north road piece meets the south piece just before the inner corner, the two pieces' edges did not meet: a wedge-shaped gap (nothing at the outer edge, 19 cm wide at the inner edge) with the north side up to 10.5 cm lower, so the ground showed through as a strip across the road, and three fins stood at the inner corner. Fix: the north piece's first-row inner vertex put on the south piece's edge, its next three rows' inner ends raised 7.9 / 5.3 / 2.6 cm, the fins put level with the road (78 vertices in all, A and B, one moved sideways 0.19 m). A wider smoothing of the inner apex was tried and dropped: it opened new cracks and jolted bikes (110 m/s²). Check: shots of the strip before / after (day) and the reported view at night; Needle 600 and Drifter Twin at 30 m/s through it: jolt 7–8 m/s², upright (same as before the change). The light-blue patch in Dan's shot is the seated teal turn arrow, unchanged.
- **A/B race check:** the scene file is unchanged (only the mesh asset; no collider, route, gate or jump object touched). Mountain Loop Forward, one lap with AI (Drifter Twin on the race autopilot vs Needle 600, Ridge Scrambler, Trail Four), run three times: 4 finishers each time, 0 missed gates in the last two runs and 0 resets in the last; one run had the autopilot and an AI tangle at the hairpin apex and both were reset, and the first run had one AI reset and one missed gate (place not logged). A run on the original mesh for comparison: the autopilot reset once elsewhere (s 2312). Racing contact; for Dan to watch.
- **C — scene characters — PASS.** Dan: light skin, brown hair, no hat, black T-shirt with a chest print (a plain white ring with a simple bird of our own inside, no lettering), blue jeans. Kyle (was "the friend"): very light skin, black hair, no cap, black leather jacket (folded collar wings, lapel edges, slight sheen) worn open over a white T-shirt, blue jeans. The brother: light skin, dark-brown hair in the medium cut (Dan's is short brown), the 0.81 white T-shirt and shorts, no cap. Builds as in 0.81. Winter: Dan a black jacket with the print on the left chest and a red beanie, Kyle the leather jacket and a teal beanie, the brother violet; hair colour shows under the beanies. New rider parts in Blender (`Leather` shirt option, `Emblem` parts; worst-case rider 8,916 / 9,340 triangles), hair / hat checks all clean. Free extra: "Leather jacket" is now a fourth Shirt option in the garage's Rider page (AI riders can get it too); the chest print is not a garage option. Check: football (all three, both sides), coffee (Dan and Kyle), sled and broom hockey in Snow; each character built with the right parts.
- **D — track map always visible — PASS.** The "Preview highlighted track" row and its page are gone. Tracks now opens wide like the garage: the map in a fixed panel on the left, the list on the right. The map follows the highlight (keyboard, controller, and mouse hover, which now also moves the highlight in this list) with the course's route (cyan main, gold shortcuts, white direction arrows, the start as a white square), its name, lap length and the Race Setup lap count; Back shows the plain map. The map and overlay are built once; a highlight change only re-aims and redraws, worst frame 20–22 ms while moving one row per frame through the list three times (editor). Check: shots with Street Loop Forward, Mountain Loop Reverse and Back highlighted.
- **E — poster loading screen — PASS.** The poster (unaltered, byte copy, uncompressed, no mipmaps, bilinear) fills the screen and covers other aspect ratios by cropping evenly. The loading information sits on a dark gradient along the bottom (LOADING, the title, conditions / vehicle / rivals on one line, the step, the bar, the tip), clear of the title lettering and the two vehicles; the route map is a small inset bottom-right (races only). Check: a race (Mountain Loop Reverse) and Free Roam: the bar runs forward to the end and the screen fades; one shot each.
- **F — winner's head and the bald head when switching views — FIXED.** Cause: first person hides the player's hair, hat, eyes and mouth (shadows only); the winner shot disabled the camera-views component while they were hidden, so they stayed hidden; and the hiding was switched by the chosen view, not by where the camera was, so a blend into first person showed a bald rider and a blend out showed the inside of the head. Now the head parts are hidden exactly while the frame's actual camera (after any blend, ease-out or Trailer Mode camera) is within 17 cm of the head centre, decided in the same LateUpdate that places the camera; the winner shot shows the whole rider and first-person hiding returns afterwards. AI heads were never hidden. The finish panel uses the race camera (no other outside camera). Checks: player win in first person (head drawn in the shot 3 of 3, hidden again after); AI win with first person selected (winner's and player's heads drawn); a chase-view race (the AI won it twice, so the chase-view player win was not seen; the head is never hidden in chase). View cycling 16 times on the Needle 600 and the Street Classic: 1,280 frames, the head hidden only with the camera inside it (at most 0.16 m from its centre), never a bald head, never the inside of the head.
- **G — plain Top 10 — PASS.** Why it was blank: besides the narrow slice, the record filter recognised only the four original vehicles, so every entry set with one of the six 0.81 vehicles (as the vehicle or in the AI roster) fell into its own "unrecognised historical" era and never showed. Records now opens straight on the best laps of the track last raced (or the first with times), all vehicles, every race length, every layout version and era: rank, time, vehicle, date (legacy entries marked "legacy", NEW as before). One large ‹ track › control (left / right through the eight courses; "(no times yet)" in the line when empty), LAP / RACE tabs (RACE: full-race totals of every length, with a Laps column), an optional ‹ vehicle › filter offering only vehicles with an entry there. Filters, era, laps and "Current race" rows and their pages are gone. Every empty board says why in one line ("No lap times on Mountain Loop — Forward yet. Finish a lap there to set one."). Speed traps / jumps: ‹ site › control, every configuration together, same one-line empty message. Display only; nothing stored changes. During a race (pause menu) the track stays the current one. Check: a fixture in the isolated test save (5 Street Loop laps incl. a Drifter Twin, a Roadster and a legacy entry; 2 races; 1 Mountain lap): opened filled with 5 rows, stepped through all eight tracks (6 explained empty), RACE showed 2 with laps; shots.
- **Decisions:** A and B fixed in the forward mesh only (FreeRoamWorld and Reverse were not reported and were not changed); Kyle's skin "very light" and the brother's hair "dark brown, medium" to keep the three distinct; Dan's winter look a black jacket with the print (Dan's brief allowed either); hover moves the highlight only in the Tracks list; Records now always opens on LAP; activity boards combine every configuration.
- **For Dan to check:** the summit hairpin and the strip at B on Mountain Loop Forward at race speed (and the AI tangles at the hairpin apex); the three characters' looks, especially the print and the leather jacket up close; the Tracks map with a controller and the mouse; the poster loading screen on his monitor; switching views and the winner shot in first person; Records with his real saved times.

### The plan as authorized

### Part A — BUG-001: a spot that flips everyone (Mountain Loop Forward)

"this is causing everyone to flip" at (1253.0, 152.5, 226.3), `MountainLoop`, lap 1, next checkpoint 6. Same kind of fault as the 0.81 Reverse bump (a lip or step in the driving surface that throws bikes). Find the lip or seam at that spot (check first for a collider edge or mesh seam standing above the surface, including anything added by 0.80/0.81 work that also loads in this scene), and make the surface continuous so a motorcycle at race speed stays upright. Section 5A: keep the race line and any jump as they are. Check: ride it at race speed on a motorcycle a few times; one lap of Mountain Loop Forward with AI; collider comparison for the scene.

### Part B — BUG-002: broken road surface (Mountain Loop Forward)

"fix this" at (989.1, 132.0, -115.6), `MountainLoop`, lap 2, next checkpoint 3. In the screenshot the road ahead has a visible break running across it: a strip where the surface is missing or mismatched, with a lighter band showing through and a blue patch beside it. Find what it is (gap between road pieces, a z-fighting or misplaced strip, a marking mesh lying across the road) and repair it so the road looks and drives as one continuous surface. Same 5A care and the same single check as Part A (the lap covers both).

### Part C — The three scene characters

Dan (2026-10-06): "I want to change the scene characters in this way: 1) Me - White, with brown hair and wearing a black Ramones t-shirt (I know it won't actually be the Ramones) but it would have a white circle with an eagle in the middle (or really whatever close it can do but remain vague) and blue jeans. 2) Kyle - White, with black hair and black leather jacket with blue jeans. 3) my brother - white, with brown hair, whatever clothes he is wearing doesn't matter."

Change the three definitions in `ScenePeople.cs` (and add what the rider model needs to show them):

1. **Dan:** light skin, brown hair, no hat. **Black T-shirt with a chest emblem: a plain white ring with a simple generic bird / eagle silhouette inside it.** Keep it vague and original: no lettering, no band name, no copy of any real logo's layout. Blue jeans.
2. **Kyle** (the character called "the friend" in 0.81; rename him Kyle): light skin, black hair, no cap. **Black leather jacket** (a jacket shape with collar and a slight sheen, open over a plain dark or white T-shirt), blue jeans.
3. **The brother:** light skin, brown hair (a slightly different shade or cut from Dan's so they are not twins), any plain clothes that are clearly different from the other two (keep the 0.81 white T-shirt; long trousers or shorts as it is now).
4. Keep their different heights/builds from 0.81 so they can be told apart. In the Snow scenes (sled, broom hockey) they keep winter jackets and beanies as in 0.81, but recognisable: Dan's emblem on his jacket chest or a black jacket, Kyle in the black leather jacket, hair colour visible under the beanie.
5. The emblem and the leather jacket are for these scene characters. If adding them as rider-customization options for the player is free, fine; do not spend time on it.
6. Check: one close shot of the three together (the football scene), one of a two-person scene, one Snow scene.

### Part D — Track select: the map is always visible

Dan: "On the track select screen - Instead of having a button to show preview keep the map visible at all times and change the route on the map to the one that is highlighted."

1. Remove the "Preview highlighted track" row and the separate preview page. The course map is part of the track select screen itself, always shown beside the list (the 0.76 garage layout is the model: list on one side, a fixed panel on the other).
2. As the highlight moves through the list (keyboard, controller or mouse hover), the map immediately shows that course's route: main route and shortcuts in the existing overlay colours, with direction (forward or reverse) evident, for example a start marker and direction arrows. The course's name and its length or lap count sit with the map if that information already exists.
3. Works for all eight courses and for the playlist/any other entries in that list (entries without a route show the plain map). No stutter when moving quickly through the list.
4. Check: one shot with a Street Loop entry highlighted and one with a Mountain entry.

### Part E — The game poster on loading screens

Dan: "On the load screens I want to put the game poster."

1. The poster is in the project at `SourceArt/Poster/WoodstockRushPoster.png` (1672×941, 16:9; placed there by Claude from the image Dan supplied on 2026-10-04). Import it into the game and use it as the full-screen background of every loading screen, scaled to fill 16:9 without stretching (crop evenly on other aspect ratios).
2. The loading information from 0.82 stays, laid over the poster so the poster remains the picture: a dark gradient band along the bottom carrying what is loading, conditions and vehicle, the progress bar and the tip. Keep the poster's title lettering (upper centre) and the two vehicles (centre) uncovered. The route map from 0.82 becomes a small inset in a bottom corner, or is dropped if it crowds the poster.
3. The image is only 1672 px wide and will be shown at 3840: use good filtering and no sharpening tricks; a slight softness is accepted.
4. Do not alter the artwork itself. Check: one shot of a race loading screen and one of Free Roam.

### Part F — Winner's celebration: head missing

Dan: "On the victory dance, the driver is missing their face and whatever is on their head (including hair)."

In the 0.82 winner shot the rider has no face, hair or hat. Likely cause: first-person view hides the player's head parts so the camera does not see inside them, and the winner shot (an outside camera) does not show them again; check also AI winners and riders in Shorts (0.78 noted the "eyes" material slot is shared). Fix so the whole rider is visible in the winner shot for the player and for AI, whatever camera view the player was using, and restore the first-person hiding afterwards. Check the same for any other outside camera that can show the player while first person is selected (Trailer Mode cameras, the finish panel). **Also (Dan, 2026-10-06): "when you are switching POV, you briefly see a bald head. Can we fix this also."** When cycling camera views (V / X), the head parts are hidden or shown a moment out of step with the camera move, so an outside view shows a bald, faceless rider for an instant (or first person shows the inside of the head). Switch the head parts in the same frame the camera actually changes (and if the view change is blended, keep the head visible until the camera is inside it), so no view ever shows a bald head. Check: win a race in first person and in chase view; one AI win. Cycle through all four views several times while watching the rider.

### Part G — Records: a plain Top 10 that is never mysteriously blank

Dan: "I still find the top 10 menu confusing. I just want to see top 10 scores there and I never understand why I see blank lists all the time."

Why it is blank today: the Records screen (`RaceMenus.RecordsResults.cs`) shows one narrow slice at a time: one track and direction × lap times or race totals × one lap count × one vehicle × an era filter. Most slices have no entries, more so now that there are ten vehicles, so the list is usually empty even though records exist.

1. **Opening Records shows a filled Top 10 straight away:** the best lap times on the track last raced (or the first track that has any records), **all vehicles together**, all race lengths, all eras. Columns: rank, time, vehicle, date. No filter has to be touched to see scores.
2. **One obvious control to change track** (left / right through the eight courses, name and direction shown large). Tracks with no times yet say so in the track name line.
3. **One toggle: Best laps / Best races.** Best races lists full-race totals across all lap counts, with the lap count shown as a column, instead of making the player pick a lap count first.
4. **Vehicle filter is optional**, defaults to All vehicles, and offers only vehicles that actually have an entry on that track. The era filter and the separate "record filters" page go away from the main view (keep legacy entries in the list, marked as now).
5. **An empty list always explains itself** in one plain line, for example "No times on Mountain Loop — Reverse yet. Finish a lap there to set one." Never a bare empty table.
6. The same simplification for activity records if they use the same screen. No stored record is deleted, changed or re-ranked; this is display only. The post-race results screen still highlights a new record as now.
7. Check: open Records from the main menu and step through the tracks; one shot of a filled board and one of an explained empty one.

### Verification

Light, per the Verification budget: the one check named in each part, compile, launch, release steps. Results as a short list, with "for Dan to check".

## Previous delivery — Loading screens, stutter check, junction lines fix, garage stat bars, winner camera — 0.82.0-review1 — DELIVERED, REVIEWED BY DAN (follow-ups in 0.83)

- **DELIVERED:**
  - Source `27a3fa98e9d357241e114034ebdadad8af8a6a99` pushed and verified on origin/main.
  - Fresh 0.82.0-review1 Windows build: 0 errors, 2 warnings, 2m31s ([build-release.txt](Docs/Report082/build-release.txt)).
  - Published [game-82000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-82000). Claude Code's auto-mode safety check blocked the publish after the draft was created (known draft-lookup miss); Dan ran `--resume-draft` himself, which uploaded the three assets and published.
  - All 234 Latest files match the public signed manifest; public download, signature, install and startup pass ([hosted/result.json](Docs/Report082/hosted/result.json)).
- **Play-Racer.cmd (unchanged):** launched a responsive `Builds/Latest/versions/82000/Racer.exe` (0.82.0-review1), muted, settings restored byte for byte. Latest root, current 82000 and previous 81000 retained.
- **Cleanup:** Builds 10,100,166,113 → 7,973,741,181 bytes; plus 22.2 GB of benchmark players and captures outside the project. C: free 273,517,780,992 bytes.
- **Auto-mode note for future rounds:** Dan may add `.claude/settings.local.json` allowing `Bash(python Tools/Publish-LauncherRelease.py:*)` so the publisher is not blocked.
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.82 (rule 12).

### Results (2026-10-06, Claude Code)

- **Safety checkpoint:** `f5607a3d` (this TODO plan), pushed before any change. Version 0.82.0-review1 / build 82000.
- Evidence: [Docs/Report082/](Docs/Report082/): [Shots/](Docs/Report082/Shots/) (one per check), [Lists/](Docs/Report082/Lists/) (check results, junction paint report, hitch bench before/after, profiles). No VALIDATION.md (Verification budget).
- Code: A `JunctionPaint`; B `LoadingScreen.cs` (new), `RaceFlow` (every scene change goes through it; the start waits for the world), `SceneryWorld` (built one step per frame), `VehicleInput`, `StartupTitle`, `WorldMapCourseOverlay`; C `RaceRoad`, `ShortcutUndergrowth`, `VehicleRespawn`, `SmashAudio`, `WeatherEffects`, `VehicleSurfaceContacts`, `ExplorationMap`; D `RaceMenus.GarageStats.cs` (new), `RaceMenus`; E `WinnerShot.cs` (new), `RaceFlow`, `RaceMenus.Finish`. Checks `Report082Checks.cs`, bench `HitchBench.cs` (`-hitchBench`), tools `Tools/Report082/`.
- **A — junction lines (BUG-001) — PASS.** Cause: the 0.80 corner curves had a fixed 9 m radius, but the asphalt corners at both Hwy 92 mouths are square, so the curves lay on the grass / dirt; at S Cherokee the side-road edge lines came from a single width probe and ran up the verge. Now the side road is traced from its real asphalt every metre (the first 8 m, where the terrain colour blends into the verge, follow the line and width of the clean part beyond), Hwy 92's paved edge is measured beside the mouth, and each corner uses the largest radius (9 m down to 0.6 m) whose whole curve, with 0.5 m to spare, is on pavement (Cherokee 0.6 / 0.6 m, Trickum 0.6 / 4 m). By construction: every new line is cut into pieces of 1 m at most and only pieces with pavement within 1 m across the line are kept (terrain asphalt is stored every 2 m, so its edge is only known to about 1 m; the reported lines were 3 m and more out); kept old 0.6 pieces and CR113 triangles off pavement are dropped the same way (3 old pieces, 0 CR113 triangles). S Cherokee / Trickum has no junction paint: nothing to fix. Paint only: no road, collider or route change. Shots: the three junctions from above, the reported view before / after, and a Hwy 92 stretch showing its edge lines kept.
- **B — loading screens — PASS.** A full-screen loading screen (menu look) for every scene change: the first load after launch (up before the first frame), a course from Tracks, playlist races, Free Roam, back to the menu or to a race from Free Roam. It shows what is loading ("Mountain Loop — Reverse", "Free Roam"), conditions, vehicle and rivals (race) or start course, weather and vehicle (Free Roam), the course route on the world map (the course-select preview; none for Free Roam), a bar that follows the real work (the async scene load 0–30 %, then each world-building step SceneryWorld reports: clearing, people, trees, buildings, rocks and props, shores, paving, street signs, road paint, world edge; then vehicles and the first rendered frames) and a rotating tip (reset R / Y, camera V / X, fist F / LB, minimap J / B, map M / View with waypoint F / Y, Trailer Mode F8, time and weather options). It stays up until RaceFlow has started and the first frames render smoothly (at least 8 frames, until 4 in a row under 50 ms, 4 s at most), then fades in 0.35 s. Behind it the countdown, menus and vehicle input wait; the title voice starts after it. SceneryWorld now builds over several frames (same steps, same order) and RaceFlow starts the race / Free Roam / title / menu once it is Ready. The smash sounds and the launch-surface list are prepared behind it (Part C). A restart in the same scene loads no scene, so it shows no screen (decision). Check: a race (Mountain Loop Reverse via Tracks) and Free Roam: the bar only moves forward, through every step to the end, then the screen fades; shots.
- **C — stutter — FIXED (it was not only other programs).** Bench (release player, 3840×2160, GTX 1660 Ti): a Street Loop race start with 3 AI and traffic, and a 69 s Free Roam drive with traffic along Hwy 92 and S Cherokee Ln on an autopilot; every frame over 16.7 ms listed, causes from development-player profiler captures. Causes found and fixed, each with the same result as before:
  - `RaceRoad.Project` / `ProjectNear` tested every road segment on every call (each traffic car, AI and the guidance call them several times per physics step): now a grid search that finds the same segment (checked bit for bit against the old scans on 520,000 points in three scenes; about 11× faster).
  - `ShortcutUndergrowth` projected every moving vehicle onto the main route every step, even far from the two Backyard bush patches: the patch distance is tested first.
  - At GO every car scanned every collider with culture-aware name tests (`VehicleRespawn.UnsafeJump`, about 0.8 s): one ordinal scan per scene, prepared behind the loading screen. At START the smash sounds were synthesised again, and the rain and thunder at every scene load: now once per session (the same seeded sounds).
  - Every car did its 0.1 s safe-position search in the same physics step (a 20–30 ms frame ten times a second in Free Roam): AI and traffic now take turns within the 0.1 s; the player's timing is unchanged.
  - Loading: the 0.80 terrain-marking pass projected all ~213k road vertices of the world onto Hwy 92 (about 5 s of every load): now only vertices within 64 m of it (checked in three scenes: no vertex outside is ever unmarked); `VehicleSurfaceContacts` called TransformPoint three times per triangle: now once per vertex (the same call, so the same points; the batched TransformPoints was tried and not used because it differs in the last bits). The exploration map's 3-second save while exploring now writes on a worker thread.
  - **Before (0.81) → after (0.82):** Free Roam drive: median frame 25.7 → 7.6 ms, frames over 16.7 ms 1,539 of 2,233 → 2–22 of about 8,400, worst 177 → 17–21 ms (three quiet runs); race start: 182 → 3–4 slow frames, worst 793 → 270–305 ms; Free Roam load: one frozen 14.5 s frame → 6.8 s behind the loading screen, longest frame 0.7 s. Two runs while other programs were busy on the PC (median 8.8–10.6 ms everywhere): 182–208 slow frames, worst 48–57 ms, so other programs do still cost frames.
  - **Frame rate, worst view** (Street Loop grid, 24 traffic cars, chase camera): Night / Snow 9.42 → 8.33 ms GPU (120 fps), Day 9.00 → 7.43 ms (135 fps). No visual change.
  - Left as is: a pause of about 0.3 s when pressing START RACE while the three AI vehicles are built (`RaceDirector.CreateCars`); it is not a scene load, so there is no loading screen there.
- **D — garage stat bars — PASS.** Five bars under the Vehicle row (Top speed = Speed, Acceleration, Grip, Handling = Response, Weight / contact = Mass), each scaled from the lowest of the ten vehicles (a short bar) to the highest (full). Display only: no selection, the preview is not covered, no handling change. Shots: Sundown Roadster, Drifter Twin, Pebble Coupe, Ridge Scrambler.
- **E — winner celebration — PASS.** When the player's race ends with opponents: a 2.5 s shot of the winner from the front three-quarter side, following them; the finish panel, the "complete race" prompt and the results wait for it; A / Space / click skips it. If an AI won earlier (the player was still racing, so the camera could not leave them), that winner raises both fists again for the shot (decision). Order, times and records untouched. Checks: a player win (camera 5.3 m in front, celebration on, panel held back, back after) and an AI win (EMBER 125.47 s, player 127.21 s).
- **Decisions:** the corner radius is chosen per corner from the asphalt (square corners get a tight corner, not a curve on the grass); 1 m pavement tolerance for terrain asphalt; no loading screen on a same-scene restart; the AI winner repeats its celebration at the player's finish; Part C changes only where the result is the same, except the AI / traffic taking turns in the 0.1 s safe-position sampling (timing only).
- **For Dan to check:** the corners at both Hwy 92 junctions; the loading screen look and its tips; how smooth Free Roam and race starts feel on his PC (closing other heavy programs to compare); the garage bars with a controller; the winner shot timing.

### The plan as authorized


- **Authorized by Dan (2026-10-05).** Written by Claude (chat) from his first look at 0.81.0-review1 (debug session `2026-10-05_22-13-37-776_382761`, 1 report, on 0.81.0-review1) and his message: "things are a little stuttery but maybe because there are other processes on my computer. Now it seems to take a little bit of loading. Can we have loading screens or just something to show that something is loading as opposed to a spinning circle. Is there anything else we can roll into this?" Parts D and E are Claude's suggestions in answer to that question; Dan may strike them.
- **Starting point:** main at the "Record 0.81 delivery" commit; playable source `badc4857` (0.81.0-review1 / game-81000). This TODO edit is uncommitted and belongs in the safety checkpoint. Dan has not finished reviewing 0.81; do not change the look or handling of the new vehicles, traffic or people here.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–E below.** The Verification budget in "Mandatory standing workflow" applies in full.

### Part A — Junction lines at S Cherokee Ln / Hwy 92 (BUG-001)

"ummm I think something is wrong with these lines" at (311.6, 8.7, 546.7), `FreeRoamWorld`, Day Clear. In the screenshot the white edge lines from the 0.80 `JunctionPaint` leave the pavement: one runs across the grass verge and round the outside of the corner, another runs straight up the grass hillside toward the trees, and the curved corner line sits out on the dirt instead of on the road edge.

1. Fix this junction so every painted line lies on the pavement and follows the real edge of the asphalt: edge lines along the road edges, corner curves on the paved corner, a stop line across S Cherokee Ln, nothing on grass or dirt.
2. Make it impossible by construction: `JunctionPaint` (and any other line painting) must only draw where there is paved surface under the line; clip or drop anything else. Then look once at the other two junctions (Trickum Rd / Hwy 92, S Cherokee Ln / Trickum Rd) and fix the same fault if present.
3. Paint only; no road, collider or route change. Check: one shot of each of the three junctions from above the corner.

### Part B — Loading screens

Dan: loading now takes a little while (the 0.78 scenery, the 0.81 world edge, people and vehicles are built at load) and all he sees is a spinning circle.

1. A proper full-screen loading screen whenever a scene loads: starting a race, starting Free Roam, restarting, returning to the menu, and the first load after launch.
2. Content: the game's look (dark backing in the menu style), the name of what is loading ("Mountain Loop — Reverse", "Free Roam"), the chosen conditions and vehicle for a race, a real progress bar that moves with actual load progress (scene load, then the at-load building steps: scenery, world edge, people, vehicles), and one short rotating tip from a small list of true tips (controls and features that exist: reset, camera view V / X, fist wave F / LB, minimap J / B, Trailer Mode F8, map waypoints, weather and time options). A course picture if one already exists in the project (the route preview from the course select is fine); do not generate new art.
3. It stays up until the world is fully built and the first frames have rendered, so the player never sees scenery popping in or a frozen first second; then a short fade. Music/radio behaviour during loading as now. No input needed to continue.
4. Do the slow work while it is up: move anything that currently causes a hitch just after the start (shader warm-up, first-use model or audio loads, building scenery) to behind the loading screen.
5. Check: start one race and Free Roam and watch the bar reach the end and fade; one shot of the screen.

### Part C — Stutter

Dan: "things are a little stuttery but maybe because there are other processes on my computer." 0.81 reported its worst view at 9.42 ms (106 fps) with traffic, a smaller margin than before, and average frame time does not show hitches.

1. Measure hitches, not the average: one 60–90 s Free Roam drive along Hwy 92 and S Cherokee Ln with traffic, and one race start on Street Loop, at 3840×2160, recording every frame over 16.7 ms and what the main thread was doing (Unity profiler markers: GC collections, shader compilation, instantiation, scenery/LOD work, traffic or people spawning, physics spikes).
2. Fix the top causes found (typical: per-frame allocations causing GC, first-time shader variants, objects created during play, mesh or collider building during play). Report before/after: number of frames over 16.7 ms and the worst frame.
3. If the recording is clean (no spikes beyond a handful), say so plainly, do not invent work, and tell Dan it is likely other programs on his PC (Chrome and OBS on the GPU cost about a third in the 0.79 bench).
4. Frame rate in the worst view must not get worse. No visual downgrade without saying which and why.

### Part D — Vehicle stat bars in the garage (suggested by Claude)

There are now ten vehicles in one stepper row and their differences are only in a line of text.

- On the vehicle screen show five simple bars for the selected vehicle: Top speed, Acceleration, Grip, Handling (response), Weight / contact strength, drawn from the real profile numbers and scaled across all ten so they compare honestly. Class label (Car / Motorcycle / ATV) stays.
- Readable at 4K, works with controller and mouse, does not cover the rotating preview. Display only: no handling numbers change.
- Check: one shot of the garage with two different vehicles.

### Part E — Show the winner's celebration (suggested by Claude)

0.78 noted that the results panel partly covers the winner's two-fist celebration and the camera was not changed.

- When the race is won, hold the results panel back for about 2.5 seconds and put the camera on the winner from the front three-quarter side (player or AI) so the celebration is seen, then bring the results up as now. Skippable with the confirm button. No change to finishing order, times or records.
- Check: one player win and one AI win.

### Verification

Light, per the Verification budget: the one check named in each part, compile, launch, release steps. Results as a short list, with "for Dan to check".

## Previous delivery — New vehicles (four cars incl. a convertible, two motorcycles), Blender traffic cars, detailed people in the scripted scenes, and three 0.80 fixes — 0.81.0-review1 — DELIVERED, DAN'S REVIEW IN PROGRESS (junction lines, loading and stutter follow in 0.82)

- **DELIVERED:**
  - Source `badc48577f94a2840ee28e641cb2a9ccfe9e8031` pushed and verified on origin/main.
  - Fresh 0.81.0-review1 Windows build: 0 errors, 2 warnings, 2m43s.
  - Published [game-81000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-81000) (known draft-lookup miss, completed with `--resume-draft`).
  - All 234 Latest files match the public signed manifest; public download, signature, install and startup pass.
  - [Delivery evidence](Docs/Report081/PUBLICATION.md).
- **Play-Racer.cmd (unchanged):** launched a responsive managed 81000, muted, with settings restored byte-for-byte; no pending updates. Latest root, current 81000 and previous 80000 retained.
- **Cleanup:**
  - Builds 10,092,961,132 → 7,966,638,546 bytes.
  - Removed the 1.73 GB hosted check install and 1.87 GB of scratch outside the project.
  - C: free 257,447,268,352 bytes.
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.81 (rule 12): the six new vehicles and their handling, the traffic kit, the three characters in the scripted scenes, the world-edge hills, the Mountain Loop Reverse bump. A documentation-only delivery commit follows; the playable source remains `badc4857`.

### Results (2026-10-05, Claude Code)

- **Safety checkpoint:** `c78abde8` (this TODO plan), pushed before any change. Part 0 work commit `15c9d8a9`. Version 0.81.0-review1 / build 81000.
- Evidence: [Docs/Report081/](Docs/Report081/): [Models/](Docs/Report081/Models/) (one Blender render sheet per new vehicle, and the traffic kit), [Shots/](Docs/Report081/Shots/) (one in-game shot per fix, each vehicle by day / night / fist wave and in the garage, traffic by day and night, each people scene), [Lists/](Docs/Report081/Lists/) (check results, frame rate). No VALIDATION.md (Verification budget).
- Code: Part 0 `RoadPosts.RetireNameBoards`, `WorldEdge.cs`, `Tools/Report081/Report081Author.cs`; Part A/B `VehicleProfile` (per-vehicle data now on the profile: model, rider pose, seat, steering pivot, chassis values), `VehicleConfiguration`, `VehicleVisual`, `CameraViews`, garage `RaceMenus`, `RaceFlow` (colours per vehicle, AI roster choices), `ActivitySite`, `RecordBoards`; Part C `AmbientVehicle` + `VehicleVisual.TrafficModel`; Part D `ScenePeople.cs`, `AmbientLife`, `SnowScenes`. Blender: `Tools/Blender/cars81.py`, `bikes.py`, `traffic.py`, `rider.py` (new poses Dirt, Cruiser, Stand, Sit, Sled); sources in `SourceArt/Blender/`, FBX in `Assets/Resources/VehicleModels/`. Checks `Report081Checks.cs`, `ConditionsBench -conditionsTraffic`.
- **0.1 BUG-001 road-name boards — PASS.** They were three breakable sign props: "Trickum Road" (Hwy 92, the reported one), "South Cherokee Lane / JUNCTION: Hwy 92" and "TO Jamerson Road" (S Cherokee / Trickum; Jamerson is no longer a named road). Removed at load in every scene. Their only collider is the smash trigger a vehicle drives through, so no race contact changes (5A); none is used by a smash activity. The shortcut speed boards (Creek Leap, Fox Gully, Pine Ridge) stay. Shot of the corner.
- **0.2 BUG-002 end of the world — PASS.** `WorldEdge`, built at load in every scene: beyond the ground's outer edge, rolling hills and ridgelines out to about 1.8 km, drawn with the ground's own material (time of day, weather and snow work as on the ground), with a band of simple cone trees (no colliders) along the near hills. The ground rises as a 10 m earth bank (collidable) and an invisible wall stands 150 m high behind it; the 0.68 fall reset is still the backstop. Only places with no ground under them AND within 30 m of the world's outer bounds count as outside, so ravines and jump gaps inside the world are untouched; 0 collider cells near any race line in the course scenes. About 105k triangles, 9.4k cone trees, 0.4–0.5 s added to each scene load. Checks: the reported position now faces a wooded bank; summit, west, south and night views have a horizon; full throttle at the edge (moto at 22 and 30 m/s, Street Classic at 30 m/s): stopped 38–42 m out on the bank, no reset, no fall.
- **0.3 BUG-003 bump — PASS.** Cause: the 0.80 deck embankment (BUG-010) had also been built at seams INSIDE the driving surface (two road pieces meeting), as strips starting 5 cm under the road; their top edge caught the wheels (a 225 m/s² vertical jolt, bikes flipped at exactly 997.8, 152.6, 319.7). The 83 such strips (mountain-wide, all under a continuing road) were removed from the embankment mesh; the other 1,201 strips are unchanged ([list](Docs/Report081/BUG-003-embankment.txt)). Motorcycle at 30, 34 and 38 m/s through the spot: max jolt 3 m/s², no air, upright (before: flipped every time). One race of Mountain Loop Reverse with AI (Needle 600 + Trail Four, Ridge Scrambler, Drifter Twin): all finish, 0 missed gates, the Summit Traverse taken. Scene files: none changed; only the embankment mesh asset.
- **A — four cars — PASS.** Sundown Roadster (1960s roadster, top down, wire wheels), Highball Fastback (1960s fastback, hood scoop, ducktail), Pebble Coupe (1970s rear-engined compact, round lamps on the wings, louvred engine lid), Skyfin Cruiser (1950s finned cruiser, two-tone roof and side spear in a cream second tone, chrome, whitewalls, tail fins). 9.9–10.3k triangles each, 3 Blender passes at most. Same rider (Car pose) and gestures; the convertible's fist wave is over the door. Each: loads, rider seated, wheels on the ground (same ride height as the existing cars), headlights at night, fist wave. Street Loop race with the Roadster and three car AI (Pebble, Skyfin, Fastback): all finish, 0 missed gates.
- **B — two motorcycles — PASS.** Ridge Scrambler (dirt bike: tall, long forks with guards, 21-inch knobbly front, beak fender, high bars, upright rider) and Drifter Twin (cruiser: raked chrome forks, big round lamp, wide pulled-back bars, teardrop tank, V-twin, long pipes, valanced fenders, fat rear tyre, feet forward). New rider poses Dirt and Cruiser. Each passes the same load / wheels / rider / lights / fist-wave check; both raced as AI on Mountain Loop Reverse.
- **Handling (rule 12, one set, Dan judges):** Roadster speed 50, accel 14.8, grip 26, response 9, mass 1000. Fastback 52 / 16 / 24 / 6.8 / 1400. Pebble 49 / 14 / 27.5 (best car grip) / 8.8 / 950. Skyfin 50.5 / 13 / 23 / 6 / 1750 (heaviest: strongest contact). Scrambler 56 / 17.5 / 31 / 11.5 / 200 with longer suspension (0.75 m), more upright strength and air stability (landings). Drifter 58 / 16 / 30 / 9.5 / 320 (heaviest bike), less visual lean.
- **Garage — PASS.** All ten listed with the 0.76 rotating preview; the vehicle list is now one row "‹ Vehicle: name (class, n of 10) ›" (left / right or select steps), title and description name the vehicle. Colours are saved per vehicle (old saves keep theirs). AI Random / Mixed and the roster choices include the new vehicles; all ten are allowed on every course (0.80). Activity medal targets of a new vehicle use its nearest original (cars: Street Classic or Longroof GT; bikes: Needle 600). Records stay per vehicle.
- **C — traffic — PASS.** Blender kit: sedan, station wagon, pickup, van (4.1–4.7k triangles), with lamps that glow at night and a driver silhouette; the same four body types, colours, collider sizes, counts and behaviour as before; Model: Classic shows the old blocks. Street Loop: 24 traffic cars, all on the kit, by day and by night.
- **D — people — PASS.** Three characters, defined in one place (`ScenePeople.cs`): **Dan** (light skin, short dark-brown hair, no hat, blue T-shirt, blue jeans; height 1.0), **the friend** (tan skin, medium black hair, gold baseball cap, teal long sleeve, black jeans; taller, slimmer 1.06 / 0.96), **the brother** (light skin, short auburn hair, red flat cap, white T-shirt, gold shorts; shorter, stockier 0.95 / 1.10). Winter (sled, broom hockey): the same people in jackets (Dan red, friend gold, brother violet) and beanies. Coffee at the fence and the two at Kyle's = Dan + friend; football = all three (adults, as before); campsite = Dan + friend seated on the logs; sled = Dan in front, friend behind holding his shoulders, then both walking it back up (legs swing); broom hockey = all three. Cups, cigarette and brooms are in the hands (the arm rig; the drink / smoke gesture bends the elbow to the mouth). Timing, placement, random selection and no colliders unchanged; the highway / residential walkers are unchanged. No other scripted scenes found. One shot of each scene.
- **Frame rate (3840×2160, GTX 1660 Ti): PASS.** Street Loop grid with 24 traffic cars and the new vehicles (Skyfin + Roadster, Fastback, Drifter AI), chase camera: Day 9.00 ms = 111 fps, Night/Snow 9.42 ms = 106 fps (worst). Different view from the 0.80 scenery bench (street view 7.86 ms), so not directly comparable; the margin is smaller than before.
- **Decisions:** "inside the range the existing cars cover" read as the range of the existing vehicles (the cruiser's "strongest contact" needs more mass than the Longroof GT; the fastback's "strong acceleration" more than the cars'); invented names Sundown Roadster, Highball Fastback, Pebble Coupe, Skyfin Cruiser, Ridge Scrambler, Drifter Twin; garage list as one stepper row (ten rows did not fit); the "TO Jamerson Road" board removed with the other two (S Cherokee Ln runs to Trickum, 0.79); the people follow no Classic / New switch (always the new figures).
- **For Dan to check:** the look of the six vehicles, the traffic and the people (rule 12); the handling numbers; the world-edge hills (look, the bank and wall feel, the summit horizon) on every side; Mountain Loop Reverse at the old bump; the garage stepper with a controller; that AI fields with the new vehicles race well on every course (only Street Loop and Mountain Loop Reverse were run); frame rate on his machine.

### The plan as authorized

- **Authorized by Dan (2026-10-05).** Written by Claude (chat). Parts A–C were queued earlier today; Part 0 is from his review of 0.80.0-review1 (debug session `2026-10-05_19-55-15-336_432f96`, 3 reports, all on 0.80.0-review1: "just a couple of things"); Part D is his request of the same evening.
- **Starting point:** main at the "Record 0.80 delivery" commit; playable source `1397143a` (0.80.0-review1 / game-80000). This TODO edit is uncommitted and belongs in the safety checkpoint.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts 0 and A–D below. Do Part 0 first.**
- **The Verification budget in "Mandatory standing workflow" applies to this round in full.** Keep checking light; Dan will test.

### Part 0 — Three fixes from the 0.80 review

1. **BUG-001 — old road-name sign** at (-621.0, 8.2, 503.2), `FreeRoamWorld`: "we can remove this sign now". The large dark board reading "Trickum Road" on a post, by the Trickum Rd / Hwy 92 corner. The green street signs from 0.79 replace it. Remove it, and any other old board-style road-name signs of the same kind (for S Cherokee, Hwy 92), in every scene; if one has a collider near a race line in a course scene, hide the visual and report it (5A). Check: one shot of that corner.
2. **BUG-002 — the end of the world** at (1185.6, 80.4, 326.8), `FreeRoamWorld`: "can we do something to prevent the end of the world?" From the mountain the terrain simply stops at a straight edge with empty haze beyond, and the player can ride up to it. Decision:
   - **Look:** the world never visibly ends. Beyond the playable edge add a ring of distant, non-collidable terrain (rolling wooded hills / ridgelines, low detail, fading into the existing haze) all the way round, so every view from the mountain and the roads has a horizon. Cheap geometry; no trees with colliders.
   - **Edge:** the player cannot ride off. A natural barrier where it reads well (steep bank, dense tree line, rock) and, behind it, an invisible wall and the normal reset as a backstop, so nobody falls out of the world.
   - Apply in `FreeRoamWorld` and in the course scenes (same world; visuals outside the playable area, so no route is affected; 5A still applies to anything near a race line).
   - Check: one shot from the reported position, and ride at the edge once there.
3. **BUG-003 — bump** at (997.8, 152.5, 319.2), Mountain Loop Reverse: "there must be a bump here. I flipped and watched 2 other cycles flip". This is on the reverse runway/entry deck area where 0.80 added the embankment (BUG-010) and near the 0.80 edge covers, so first check whether a 0.80 mesh (`Ground_Report080 …`) or its collider pokes above the driving surface or leaves a lip there; otherwise find the step in the original surface. Make the driving surface continuous so bikes at race speed stay upright. Check: ride it at race speed on the motorcycle a few times, one lap of Mountain Loop Reverse with AI, and the collider comparison for that scene.

Dan (2026-10-05): "I am thinking we could fold into Blender NPC cars and while we are at it maybe add a couple more vehicles? I would want some cars that maybe look a little more sporty. Maybe a convertible. I would want a couple of cars inspired like these old vintage cars [1960s–70s sports and muscle cars, a 1950s finned cruiser]. Other motorcycles would be cool but I don't know how they would look different."

Same pipeline and standard as 0.75 (`F:\blender\blender.exe`, `Tools/Blender/`, `SourceArt/Blender/`, FBX in `Assets/Resources/VehicleModels/`, render–look–revise, at most three passes each), same stylized low-poly style, with the parametric rider visible and gestures (0.78) working in each.

**Important: original designs only.** "Inspired by" means the era and the body type. No real make or model names, badges, logos, or a copy of any one real car's exact shape; each gets an invented name in the style of the existing ones (Needle 600, Trail Four, Street Classic, Longroof GT).

### Part A — Four new player cars

1. **1960s long-bonnet roadster, a convertible** with the top down: long hood, short tail, low windscreen, rider visible from the chest up. Light and nimble.
2. **1960s fastback pony/muscle coupe:** long hood, sloping fastback roof, wide stance. Strong acceleration, heavier steering.
3. **1970s compact rear-engined sports coupe:** short, rounded, sloping tail. Quick turn-in, best car grip.
4. **1950s finned cruiser:** big, chrome, two-tone paint, tailfins. Slow to turn, top stability and strongest contact.

Each gets its own handling profile inside the range the existing cars already cover, different enough to feel distinct (rule 12: one considered set of numbers, Dan judges). Paint colours selectable as for the existing cars; the convertible must work with every hat and hair option and in Rain and Snow (no roof is fine). All four available wherever 0.80 allows cars. Fist wave: over the door on the convertible.

### Part B — Two more motorcycles

How they differ from the Needle 600 (a sport bike), so they read as different at a glance:
1. **Dirt bike:** tall, long suspension, high front fender, knobbly tyres, upright rider. Best off-road and on landings, lower top speed.
2. **Cruiser:** long and low, wide bars, big rear tyre, relaxed feet-forward rider. Stable and strong on contact for a bike, slower to lean.

Each needs its rider pose (the rider rig from 0.78 supports new poses).

### Part C — Traffic (NPC) cars in Blender

Replace the blocky ambient traffic vehicles with a small kit in the same style: sedan, pickup truck, van, station wagon/hatchback, each in several paint colours, with headlights and tail lights at night and a simple driver silhouette. Same size class, colliders, behaviour and counts as the current traffic; visuals only. They follow the Model: Classic / New switch.

### Part D — The people in the scripted scenes

Dan: "since we are about to do a blender pass. Can we put a little more detail into the people in the random scripted scenes. Two of the guys (my friend and I) are in all of them, so you can just replicate the same two people, and the ones with three people include my brother so you can repeat him too for the 3 people scenes."

The scenes are the scripted/random vignettes built from the `AmbientLife` figures: the household vignettes (two men with coffee at Dan's fence, two men at Kyle's, three playing football in Dan's yard), the campsite (two seated by the fire), and the Snow scenes (two on the sled, three at broom hockey). List any others found and treat them the same way.

1. **Three recurring characters, built once and reused:** "Dan", "the friend" and "the brother". Every two-person scene is Dan and the friend; every three-person scene is Dan, the friend and the brother. The same person must be recognisably the same in every scene.
2. **More detail:** replace the plain ambient figures in these scenes with the 0.75 parametric rider standard (same Blender rider, same stylized look): proper head and face, hair, hands, clothes with shape. Give the three clearly different looks (height/build, hair, clothing colours) so they can be told apart at a glance from a passing vehicle. **Do not try to make them look like real people; no likeness is known.** Pick three plain, distinct looks, record them in one place in code so Dan can change hair, skin, clothes and build later with a few values, and report what was chosen.
3. Clothing fits the scene: ordinary clothes at the fence and campsite, winter coats and hats on the sled and at broom hockey; the same person keeps the same hair, build and face throughout. If a scene shows them as kids (the football game), keep that scene's ages as they are now: the same three, smaller.
4. Poses and props as now (cups, football, sled, brooms, sitting by the fire), using the 0.78 arm rig so hands actually hold things. Keep each scene's behaviour, timing, placement, random selection and the no-collider rule exactly as they are. Other ambient people elsewhere are not changed.
5. Check: one shot of each scene (forced), close enough to see the three.

### Garage and checks (light, per the Verification budget)

- Garage lists all new vehicles with the 0.76 rotating preview; names, class and description for each. AI fields use the new vehicles too, where the course allows the class.
- Each new vehicle: load it once on one road, confirm wheels on the ground, rider seated, headlights on at night, fist wave works. One Blender render sheet per vehicle. No per-course runs.
- Traffic: one look at Street Loop traffic by day and by night.
- One worst-view frame-rate number at 3840×2160 with traffic (rendering cost changes this round); it must stay above 100 fps.
- Everything else is listed under "for Dan to check" in the results.

## Previous delivery — Clean-up from the 0.79 review, fist wave on LB, cars allowed on every course — 0.80.0-review1 — DELIVERED, REVIEWED BY DAN (three follow-ups in 0.81)

- **DELIVERED:**
  - Source `1397143ad3e061d460b3ad54cfa0478189178cd1` pushed and verified on origin/main.
  - Fresh 0.80.0-review1 Windows build: 0 errors, 2 warnings, 2m24s.
  - Published [game-80000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-80000) (known draft-lookup miss, completed with `--resume-draft`).
  - All 236 Latest files match the public signed manifest; public download, signature, install and startup pass.
  - [Delivery evidence](Docs/Report080/PUBLICATION.md).
- **Play-Racer.cmd (unchanged):** launched a responsive managed 80000, muted, with settings restored byte-for-byte; no pending updates. Latest root, current 80000 and previous 79000 retained.
- **Cleanup:**
  - Builds 10,069,775,295 → 7,952,276,087 bytes.
  - Removed the 1.72 GB hosted check install and 1.88 GB of scratch outside the project.
  - C: free 258,123,685,888 bytes.
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.80 (rule 12): the Hwy 92 junction paint, the summit dirt look, the Mountain Loop Reverse fills / embankments, LB fist wave, cars on every course. QUEUED NEXT (new vehicles) waits for Dan to start it. A documentation-only delivery commit follows; the playable source remains `1397143a`.

### Results (2026-10-05, Claude Code)

- **Safety checkpoint:** `88f3a761` (this TODO plan), pushed before any change. Version 0.80.0-review1 / build 80000.
- Evidence: [Docs/Report080/VALIDATION.md](Docs/Report080/VALIDATION.md); [Shots/](Docs/Report080/Shots/) (Dan's 0.79 screenshots as before, after shots day / night); [Lists/](Docs/Report080/Lists/) (posts, paint, races, rides, resets, escape, grounding).
- Code: `RoadPosts` (A.1), `JunctionPaint` (A.2), `MountainDirt` (A.3–4), called from `Scenery`; `VehicleInput` (B); `CarAccess`, `RaceDirector`, `RaceFlow`, `RacePlaylists`, `RaceMenus`, `RoadDriver` (C). Geometry: `Tools/Report080/Report080Author*.cs` (A.5–7). Checks: `Report080Checks.cs`, `Tools/Report080/`.
- **Scene files:** only `MountainLoopReverse.unity` changed (4 new collidable meshes `Ground_Report080 …`, assets in `Assets/Track/Report080/`). Every other scene file, collider and route is identical to 0.79; the A.1–A.4 changes are visual and made at load.
- **A.1 posts — PASS.** They were old shortcut edge markers (0.6 `Trail edge`, Phase 5 `Non-colliding path edge`), no colliders. FreeRoamWorld: 22 removed (all three reported spots; also those within 4 m of the pavement, which Dan saw as the dark posts); 0 left on pavement. Course scenes (5A): the same posts are shortcut race markers and were **kept** (9 in every course scene; 11 in Street Loop Reverse with its 2 branch entrance markers, 10 in Mountain Loop with the Summit Traverse sign post), listed.
- **A.2 lane paint — PASS.** Three overlapping sets: the 0.6 set follows the race line (curving across the lanes in the merge zones; its amber median was the "red/white strip"), the CR113 highway set, and the terrain shader's own two-lane markings. `JunctionPaint`: keeps CR113 and the 0.6 pieces on Hwy 92's own lines, hides 188 others, median drawn yellow, terrain markings off on Hwy 92 (3,381 vertices, drawn-mesh copy), continuous edge lines, mouth with 9 m corner curves and a stop line at Trickum Rd and S Cherokee Ln. S Cherokee / Trickum had no doubling (unchanged). Paint only, every scene.
- **A.3–4 summit roads — PASS.** The landing, the inward connection and the run-up used "Summit packed earth" (shiny red-brown URP Lit); now the terrain material in the trails' dirt colour, the landing blending into the terrain's colour at its outer 3 m. Look only. Giant jump: moto 242.5 m, ATV 223.5 m (0.71: 242.5–243.5 / 223.5–224.0), upright, roll-out 87 / 107 m. These were the only non-terrain mountain roads in FreeRoamWorld.
- **A.5 BUG-008 — PASS.** The 0.70 crest outcrop is an open shell; its rim hung up to 16 m over the slope, beside a V trench up to 5.3 m deep. Rock face under 253 rim edges + trench filled (182 m²), collidable. Escape test 20/21 (the 21st starts on top of the outcrop itself).
- **A.6 BUG-009 — PASS.** Trail surface smooth; 10–50 cm notches where the banks meet the Downhill Ridge Cut's edges (s 12–24 left). Smooth edge cover s 0–24; jump unchanged (1.16 / 1.22 s air before and after).
- **A.7 BUG-010 — PASS.** Reverse runway deck 1.2 m over its support with open edges. Mountain-wide: 1,360 edge points with 0.4–8 m of air got a smooth 1:1 earth embankment; 110 deeper gaps (ravines, flight gaps, bridges) and 1,573 protected places left and listed.
- **A.8:** Mountain Loop Forward and FreeRoamWorld do not have the BUG-008/009/010 geometry. Mountain races forward and reverse with AI: 0 missed gates, jumps made.
- **B — PASS.** LB waves while driving, RB does nothing; Trailer Mode LB 0.25× hold and RB 0.5× toggle unchanged, no wave; F unchanged. `TRAILER_MODE.md` updated.
- **C — PASS, nothing closed.** Both cars on all 8 courses, player and AI. Every shortcut ridden by both cars (including the Echo Cave and the 1.65–2.65 m Backyard trails), car AI forced through each AI shortcut, resets 80/80, one race per course × direction × car with two car AI: all finish, 0 missed gates on Street / Forest / Mountain. Dan's Backyard: AI misses a gate now and then for every vehicle (bikes-only control the same) — the known autopilot limitation since 0.76, not cars. `CarAccess` lists are empty (where a shortcut or course would be closed); no sign / minimap entry needed.
- **Decisions:** A.1 course-scene markers kept (5A); the old Hwy 92 median recoloured to the CR113 yellow so the centre line is one colour; A.7 applied mountain-wide (Dan asked to check every elevated section) with 5A protection; Part C close/restrict machinery kept minimal and empty since every test passed.
- **Limitations / for Dan (rule 12):** a crown-only tree clump is fully covered by the new embankment at (861, 99, −58); one gate marker foot 0.31 m into it; the embankment follows the existing irregular road edge; Backyard AI gate misses (pre-existing); Laurel Switchbacks / Cabin Jump cost cars (and bikes) a reset when forced.
- **Frame rate (3840×2160): PASS.** Worst view Street Night / Snow 7.86 ms = 127 fps; summit 7.69 ms (0.79: 8.43 ms). The summit dirt meshes are drawn before the terrain so the terrain under them is not shaded (a first bench without that read 9.95 ms).

- **Authorized by Dan (2026-10-05).** Written by Claude (chat) from his review of 0.79.0-review1: debug session `2026-10-05_14-24-33-951_f0ab5f`, 10 reports, all on 0.79.0-review1 (BUG-001–007 in `FreeRoamWorld`, BUG-008–010 in `MountainLoopReverse`, race, Night / Snow). "a little more clean up." He raised nothing against the 0.79 foundations, signs, clock, minimap or camera panel.
- **Starting point:** main at the "Record 0.79 delivery" commit; playable source `d00e017e` (0.79.0-review1 / game-79000). This TODO edit is uncommitted and belongs in the safety checkpoint.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–C below.** The new vehicles and Blender traffic cars are the next round (QUEUED NEXT below); do not start them here.

### Part A — Clean-up (the 10 reports)

Screenshots are in the session folder. Positions are the vehicle's.

1. **BUG-001, 002, 003 — posts standing in the road** (`FreeRoamWorld`): "remove the pole from the middle of the road" at (-259.4, 24.9, -577.3); "also this one" at (-581.6, 6.6, -551.0) (a yellow post and a dark one on the pavement at the S Cherokee / Trickum corner); "and these 2" at (-619.2, 6.6, -499.9) (a yellow post and a small one on Trickum Rd). Remove them. Then make it universal: no post, bollard, marker, sign post or prop stands on a paved road or driveway in `FreeRoamWorld`; check automatically and report the count. In the course scenes apply section 5A: if one of these is a race marker or has a collider near a race line, leave it and report it.
2. **BUG-004, 005 — lane markings at Trickum Rd / Hwy 92**: "can we clean up the lane markings right here?" at (-609.0, 8.4, 523.8), and "the lane markings from bug-004 to here need to be fixed" at (-501.3, 8.4, 534.1). The junction shows overlapping and crossing lines (several painted sets on top of each other, lines running diagonally across lanes, a red/white striped strip across the corner). Repaint that junction and the stretch of Hwy 92 between the two positions as one clean, believable layout: continuous edge lines, lane lines that line up with the rest of Hwy 92, a proper stop line and turn where Trickum joins, nothing doubled. Paint only; no road shape or collider change. Then check the other two junctions (S Cherokee / Hwy 92, S Cherokee / Trickum) for the same doubling and fix them the same way.
3. **BUG-006 — the giant-jump landing** at (786.5, 98.9, 81.9) (`FreeRoamWorld`): "what exactly is this I am driving on? Is it needed? Can we just make this match the roads and grass around it". Dan then recognised it (2026-10-05): "the mystery surface road is the landing for the big mountain jump" (the flat landing built in 0.71). **It stays: do not move, reshape, shrink or change the collision of the landing, and the jump must still land as it does now (about 243 m on the motorcycle).** Only its look changes so it stops reading as a strange dark red-brown slab: surface it as the same dirt as the other mountain roads, blended into grass at the edges so it sits in the hillside naturally. Re-run the giant jump to confirm distance and a clean landing.
4. **BUG-007 — mountain road surface** at (1179.4, 152.5, 169.3): "This road should be just a dirt road just like the other roads in the mountain". Make it the same dirt road look. Then make every mountain road in `FreeRoamWorld` one consistent dirt look (list any others found).
5. **BUG-008 — hole** in Mountain Loop Reverse at (1034.2, 154.7, 148.3): "fix this so there is no hole to fall into". A gap between rock/road pieces beside the course that a vehicle can drop into. Close it with solid ground that matches. Dan has asked for this race-scene change; section 5A applies: read the route data, keep the race line, jump and AI line as they are, fill only the gap.
6. **BUG-009 — bump** at (808.3, 96.8, -189.9), Mountain Loop Reverse: "clean this up so it doesn't bump driver out of control". Smooth the seam/step in the driving surface there so a vehicle at race speed stays settled (same method as the 0.69 edge smoothing). Keep the line and the intended character of the section.
7. **BUG-010 — floating road** seen from (1050.5, 152.3, 294.0), Mountain Loop Reverse, looking across the gap: "fix that road you see across from me so that it isn't floating". The road deck opposite is a thin slab with open air beneath. Give it ground: a rock/earth embankment or cliff under it down to the terrain, visual with matching collision where a vehicle could reach. Then check every elevated road section on the mountain the same way (Dan's history: the mountain had "hanging roads") and support any other that visibly floats.
8. For 5–7: make the same fix at the same places in `MountainLoop` (forward) and `FreeRoamWorld` where the same geometry exists, and run a race lap forward and reverse with AI to show nothing changed for the race (0 missed gates, jumps still made).

### Part B — Fist wave on the left shoulder button

Dan: "Can we make the fist waving the Left shoulder button instead of the right? Your right finger will be on the trigger so it makes it awkward to hit it."

- Controller fist wave moves from RB to **LB**. Keyboard F stays. Update Settings > Controls, hints and docs.
- Check for clashes while driving (LB is "previous" in menus and the map, which is fine). In Trailer Mode LB keeps its 0.25× hold and RB its 0.5× toggle; F waves there, as now.

### Part C — Cars allowed on every course

Dan: "it is a bit of a problem that we have four different tracks, but only one allows cars. The lanes are a lot more wide on most races that I wonder if we can move the car restrictions? The only place I can think of that might be a problem is the cave shortcut."

1. Remove the vehicle restriction so both cars (and every future car) can be chosen on all eight courses, for the player and for AI. Motorcycle and ATV availability is unchanged.
2. **Do not reshape any course to make cars fit** (section 5A). Instead find out, by driving each course forward and reverse with each car and with a car AI field, where a car cannot get through or cannot make a jump: width, overhead clearance, turn radius, jump distance, ramp break-over.
3. Where a **shortcut or alternate** does not work for a car (the Forest cave shortcut is the expected case): cars simply do not use it. Car AI never takes it; for the player it is marked as bikes/ATV only on the minimap legend and with a small sign or marking at its entrance, and a car that goes in anyway is handled by the normal reset. The main route stays open to everything.
4. Where the **main route** does not work for a car (too narrow, a jump a car cannot clear): do not alter the track. Make the smallest non-geometry adjustment if one exists (AI line, car AI speed at that jump). If none works, that course keeps its car restriction for now; report the exact spot, with a screenshot and what would have to change, for Dan to decide. Expect most courses to pass.
5. Mixed fields: an AI field may mix cars, ATVs and motorcycles where allowed, as on Street Loop today. Records stay separate per vehicle as now, so no existing record is affected.
6. Reset (0.68 rule) must work for cars everywhere, including off narrow trails.
7. Verify per course and direction: one race with the player in each car and at least two car AI: finishes, 0 missed gates, no car stuck for good, lap time recorded. Table in the validation report: course × direction × car → pass / shortcut closed to cars / still restricted (why).

### Verification (targeted, rule 11)

- Part A: before/after screenshots for each of the 10 reports from the reported positions (day, so they are easy to see), the automatic post-on-road count, and the mountain race laps in item 8.
- Part B: LB waves, RB does nothing while driving, Trailer Mode unchanged.
- Part C: the table above.
- All other course scenes, colliders and routes identical to 0.79 (state which scene files changed and why). Frame rate in the 0.78 worst view stays above 100 fps at 3840×2160.
- Standard rule steps: TODO update, commit, push, build, publish, Play-Racer.cmd check, cleanup, final report.

## Previous delivery — 0.78 scenery fixes (floating buildings, trees in driveways, road colour), street signs, on-screen camera controls, Free Roam HUD cleanup and minimap — 0.79.0-review1 — DELIVERED, REVIEWED BY DAN (clean-up items in 0.80)

- **DELIVERED:**
  - Source `d00e017ec244737ef0ce43a7539eab9d3cc309f8` pushed and verified on origin/main.
  - Fresh 0.79.0-review1 Windows build: 0 errors, 2 warnings, 2m17s.
  - Published [game-79000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-79000) (known draft-lookup miss, completed with `--resume-draft`).
  - All 236 Latest files match the public signed manifest; public download, signature, install and startup pass.
  - [Delivery evidence](Docs/Report079/PUBLICATION.md).
- **Play-Racer.cmd (unchanged):** launched a responsive managed 79000, muted, with settings restored byte-for-byte; no pending updates. Latest root, current 79000 and previous 78000 retained.
- **Cleanup:**
  - Builds 10,067,841,381 → 7,951,064,918 bytes.
  - Removed the 1.72 GB hosted check install, 2.66 GB of scratch outside the project, and nine test screenshots.
  - C: free 262,017,355,776 bytes.
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.79 (rule 12):
  - foundations, the street signs, the road grey, the Free Roam clock and minimap (J / B), the camera hint and the Trailer Mode controls panel;
  - also still 0.77 / 0.78.
  A documentation-only delivery commit follows; the playable source remains `d00e017e`.

### Results (2026-10-05, Claude Code)

- **Safety checkpoint:** `4d7a941d` (this TODO plan), pushed before any change. Version 0.79.0-review1 / build 79000.
- Evidence:
  - [Docs/Report079/VALIDATION.md](Docs/Report079/VALIDATION.md);
  - [Shots/](Docs/Report079/Shots/): before / after, roads, signs day and night, 4K HUD, menus, Trailer panel;
  - [Lists/](Docs/Report079/Lists/): per-building gaps, posts and rocks, trees per scene, check results;
  - [Bench/](Docs/Report079/Bench/).
- Code:
  - D: `SceneryBuildings` (foundations, steps, chimney, downspouts), `SceneryProps` (sloped posts, re-seated clue cairns).
  - E: `SceneryTrees` (`OnDrivable`, FreeRoamWorld trunk removal), `SceneryGround` (tufts never on roads).
  - F: `SceneryPaving` + `Assets/Scenery/Paved.shader` (`Resources/Scenery/Paved.mat`).
  - G: `StreetSigns` (`Resources/Scenery/SignLettering.mat`).
  - H: `TrailerMode.Panel.cs`, `TrailerMode` (H / L3 / F1 / B), `RaceHud` (camera hint), `RaceMenus.Core` / `.Trailer` (rows), `CameraViews`.
  - A–C: `RaceHud` (clock box, filtered text), `MoonIcon`, `ArcadeActivities` / `ExplorationCollection` (HUD text), `RacingMiniMap` (Free Roam, J / B), save field `roamMinimapHidden`.
  - Checks: `Report079Checks.cs`, `Tools/Report079/`.
- **No course scene, collider or route changed:**
  - Scene files and `CoursePreviews.json` are identical to 0.78.
  - Every static collider is identical New / Classic / New in all nine scenes.
  - Only `FreeRoamWorld` loses the trunk colliders standing on drivable surfaces (E.3).
- **D — PASS.**
  - Cause: the 0.78 house visual is drawn from the wall collider (level floor), and the old foundation was trimmed from the merged batches.
  - Every generated building now stands on a brick or block foundation down to below the lowest ground under its footprint (sampled every metre). Steps, an exterior chimney and the downspouts reach the ground. Visual only.
  - Automatic perimeter check: **47/47 buildings with a 0.00 m gap in all nine scenes** (Dan's, Kyle's and Fox Gully included).
  - The four reported houses were 1.85–2.50 m up; the worst in the world was 5.30 m.
  - Posts and rocks: 0 floating, 0 half buried in every scene. Fixed: the House 3 mailbox post, the Mountain summit "05 return" sign post (0.9 m), and clue cairns whose ground was raised after placement.
- **E — PASS.**
  - The driveway trunks were always there as invisible "Roadside woodland trunk" colliders. 0.78 drew trunks on every trunk collider, so they appeared.
  - Universal rule: 163 trees, bushes or clumps found on drivable surfaces across the nine scenes, 162 fixed:
    - FreeRoamWorld: removed with their colliders;
    - course scenes: visual hidden, collider kept, only where no race line is within 12 m.
  - **Kept and reported (5A):** one trunk collider at (208.0, 72.35, 89.8) on the Backyard Reverse trail, 2.3 m from its race line.
  - Ground detail: 0 on a drivable surface.
- **F — PASS.**
  - Cause: Hwy 92's continuous pieces, the decorative roads and the roadworks used URP Lit "asphalt" materials (darker and bluer, no wet look). The driveway material was 0.065 grey.
  - With Scenery New they now use the ground shader's road lighting in the terrain road colour; driveways get a shade lighter grey.
  - Seam on Hwy 92: 0–1/255 apart at Day / Night / Rain (Classic 12–54).
- **G — PASS.**
  - Signs at S Cherokee Ln / Hwy 92 (326.5, 549.5), Trickum Rd / Hwy 92 (−615.0, 531.0) and S Cherokee Ln / Trickum Rd (−625.5, −543.8), in all nine scenes.
  - Each is on a side-road corner, off pavement, 7.3–11.9 m from any course line, with no collider.
  - **Unsigned public road:** the decorative "Jamerson Rd west" continuation, from the S Cherokee / Trickum corner west to about (−1000, −550), centre about (−800, −550).
- **H — PASS.**
  - `V / X: camera — <view>` hint above the speedometer when driving starts and when the view changes.
  - Pause menu rows **Camera view** and **TRAILER / PHOTO MODE (F8)**, the latter also on the main menu.
  - Trailer controls panel: clickable; controller B focuses it, then D-pad / A; H / L3 hide it with the HUD; F1 shows or hides it; "H: show controls" appears briefly while hidden; absent from P / F12 screenshots.
  - Every 0.77 binding is kept. `TRAILER_MODE.md` updated.
- **A–C — PASS.**
  - Day / clock box top left with a moon disc (4K shots: Day, Night, Snow, Dawn, Dusk / Rain), clear of the minimap and speedometer.
  - Other text only while relevant: an activity start (it also becomes the selected activity), an attempt, a result, an acorn for 5 s, the menu line for 8 s at the start.
  - The minimap shows in Free Roam (roads, trails, sites, waypoint). **J / controller B** toggles it; the choice is saved, default on.
  - The race HUD is unchanged apart from the camera hint.
- **Frame rate (3840×2160): PASS.** Worst view (summit, Night / Snow) 8.43 ms = 119 fps (0.78: 8.57 ms). A first run with Chrome / OBS on the GPU read about a third slower for New and Classic alike; Dan closed them for the clean run.
- **Decisions recorded:**
  - Road colour, foundations and tree hiding apply with Scenery New; Classic stays the original look. Street signs and the HUD changes apply in both.
  - B is the controller minimap key: it does nothing while driving and is not a Trailer key. In Trailer Mode B focuses the panel instead, and the minimap hides with the HUD there.
  - H / L3 hide the HUD and the panel together. Trailer Mode now opens with the panel shown, which replaces the 0.77 first-time hint.
  - The "show controls" line ignores the driving keys, so recordings stay clean while riding.
  - Hint durations count shown time (at most 0.1 s a frame), so a loading hitch does not use them up.
- **Limitations / for Dan's eye (rule 12):**
  - The look of the foundations, the signs and the panel.
  - The Hwy 92 / S Cherokee sign stands on the east corner of S Cherokee Ln; the inner corner is too close to the race line.
  - Mountain-summit edge boulders overhang the cliff by design (they rest on the ground).
  - A tree hidden in a course scene still has its collider (rare, ≥ 12 m from any race line).

- **Authorized by Dan (2026-10-04 and 2026-10-05).** Written by Claude (chat). Parts A–C were queued on 2026-10-04; Parts D–H come from Dan's review of 0.78.0-review1 on 2026-10-05 (debug session `2026-10-05_08-47-44-658_ac0f19`, 7 reports, all on 0.78.0-review1 in `FreeRoamWorld`). His overall verdict on the new scenery: "generally looks better".
- **Starting point:** main at the "Record 0.78 delivery" commit; playable source `1f2de343` (0.78.0-review1 / game-78000). This TODO edit is uncommitted and belongs in the safety checkpoint.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–H below. Order: D, E, F first (they fix 0.78), then G, H, then A, B, C.**
- The 0.78 visual-only rule still holds for D–G: no terrain, road, collider or route changes in any course scene.

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

### Part D — Buildings must sit on the ground (0.78 review, BUG-001, 003, 004, 005) — DO THIS FIRST

Dan (2026-10-05, on 0.78.0-review1, debug session `2026-10-05_08-47-44-658_ac0f19`, all in `FreeRoamWorld`): "nearly every house was floating in the air besides the main houses. I didn't take pictures of all of them but this needs to be a universal fix."

What the screenshots show: on sloping ground the new house visual is a level box whose floor sits at the high side of the slope, so the downhill side hangs in the air with open space underneath and the front steps hover. Examples at X=576.5 Z=-414.3, X=491.5 Z=-589.9, X=396.7 Z=-501.5, X=256.7 Z=-413.8.

1. **Universal fix, not per house.** Every generated building (houses, garages, sheds, businesses, outbuildings) in all nine scenes gets a foundation/skirt that runs from the floor down to below the lowest ground point under its footprint (sample the terrain under every corner and along the edges, add a margin). Brick, block or stone foundation that suits the house. No daylight under any wall from any side.
2. Steps, porches, stoops, chimneys, posts, gutters' downpipes and any other attached piece reach the ground the same way (extend the steps or add a landing; never leave them hovering).
3. Do not move or reshape terrain, and do not change any collider or route (0.78's visual-only rule and section 5A still apply). If a foundation would be visibly solid where the collider is not, keep it within the existing footprint.
4. **Prove it for every building, automatically:** a check that, for each building in each scene, casts down from points around the base perimeter and reports the largest gap between the bottom of the visual and the ground. Target 0 gaps; list every building with its result in the validation report. Fix the cause until the list is clean; do not rely on spot checks.
5. Dan's house and Kyle's house: he reports these look right. Run the same check on them but do not redesign them.
6. Apply the same check to other generated ground-standing scenery from 0.78 (fences, mailboxes, signs, posts, props, rocks): nothing floats, nothing is half buried.

### Part E — No trees in driveways or roads (BUG-002)

"trees in driveway" at X=454.0 Z=-29.9 (`FreeRoamWorld`): tree trunks stand in the middle of the paved driveway by the fence.

1. Find how they got there: a 0.78 visual placed where a classic crown-only clump had no trunk, or a tree that was always there. Say which.
2. Universal rule: no tree, bush or ground-detail clump stands on any road, driveway, trail or other drivable surface, in any scene. Check every tree against the drivable surfaces automatically and report the count found and fixed.
3. If the offending tree has a collider that a race route or AI line passes near, do not remove the collider in a course scene: hide the visual only where safe, and report it (section 5A). In `FreeRoamWorld` remove it properly.

### Part F — Roads one consistent grey (BUG-006)

"can we make the road the same color. It should be more like the grey color" at X=169.6 Z=551.4 on Hwy 92, Night Clear. The road changes from a lighter grey section to a darker blue-black section at a hard seam; the driveway in BUG-002 is near black.

1. All paved roads use one consistent asphalt colour, the lighter grey one, with no visible seams where road pieces or scenes' sections meet, in every time of day and weather. Check the cause (two materials, vertex colour, the New ground branch, wetness) and fix it at the source.
2. Paved driveways: the same family of grey (a slightly different shade is fine), not black.
3. Lane lines and edges stay as readable as now. Dirt trails and gravel are not changed.

### Part G — Green street-name signs at junctions (BUG-007)

"lets put road signs. this road being turned onto is south cherokee lane, and turning off of Hwy 92. I would like the standard green road crossing signs. Would like this on every road." (junction at X=313.6 Z=549.4)

1. Standard US street-name signs: a post at the corner with two green blades with white lettering at right angles, each blade parallel to the road it names. Readable from a vehicle approaching at speed, and lit well enough by headlights at night.
2. Put one at every junction of named roads in the world, in all nine scenes (same world everywhere). Post on the verge, never on a drivable surface, never in a race line, AI line or a jump landing; the post has no collider (or a breakaway one that cannot affect a vehicle). Read the route data before placing each one (section 5A).
3. Names, confirmed by Dan (2026-10-05). There are three real roads and these are the only names to use:
   - **S Cherokee Ln** — South Cherokee Lane, Dan's road. Wherever the project says "Cherokee Lane" it is this road.
   - **Hwy 92** — the four-lane road.
   - **Trickum Rd** — the other road.
   - **No Jamerson Road signs.** In reality S Cherokee Ln runs into Jamerson briefly before Trickum, but that stretch is too short to count: treat it as S Cherokee Ln all the way to Trickum Rd.
   So the signed junctions are S Cherokee Ln / Hwy 92, S Cherokee Ln / Trickum Rd, and Trickum Rd / Hwy 92 if they meet in the world, plus any other place two of these three cross. Driveways, trails and race-only paths get no sign. **Do not invent names.** If some other paved public road exists that is none of the three, give it no sign and list it with a map position in the final report.
4. Verify with screenshots of each signed junction, day and night.

### Part H — Cameras reachable from the screen

Dan: "there should be an obvious way of getting to the cameras. It should be on the screen somehow so I don't have to reference notes to know how to pull them up and use them."

1. **Normal play:** a small on-screen hint that names the camera control and the current view, for example `V / X: camera — Chase`. Show it for a few seconds when driving starts and whenever the view changes, then fade. Also a "Camera view" row in the pause menu that cycles the views, and the binding listed in Settings > Controls.
2. **Trailer Mode:** an obvious entry in the pause menu (and the main menu if cheap) named so a newcomer finds it ("Trailer / Photo Mode"), with its key (F8) shown beside it.
3. **Inside Trailer Mode:** an on-screen control panel that makes the notes unnecessary: the nine cameras by number and name with the current one highlighted, and the other controls grouped and labelled (camera action, auto shot, slow motion, HUD, arrows/gates, screenshot, time, clock ±, pause clock, weather, moon, lightning). Selectable by mouse click and by controller as well as by key. It hides with H so recordings stay clean, and a single small line (`H: show controls`) reappears briefly when any key is pressed while hidden, then fades. Nothing from this panel appears in screenshots taken with P/F12.
4. Keep every existing 0.77 binding working. Update `Docs/TrailerMode/TRAILER_MODE.md`.

### Verification

- 4K screenshots of Free Roam: normal driving (clock only), at an activity start, during an attempt, just after collecting an acorn, minimap on and minimap off, and Trailer Mode HUD off.
- Confirm race HUDs are identical to 0.78 apart from the camera hint in Part H.
- Parts D and E: the automatic per-building gap list and the tree-on-drivable-surface count, clean in all nine scenes; before/after screenshots at the four reported houses and the driveway. Static colliders and routes identical to 0.78 in every course scene.
- Part F: screenshots along Hwy 92 at Day, Night and Rain showing no seam. Part G: signed junctions day and night. Part H: screenshots of the hint, the pause-menu rows and the Trailer Mode panel shown and hidden.
- Frame rate at 3840×2160 in the 0.78 worst view stays above 100 fps.
- Standard rule steps: TODO update, commit, push, build, publish, Play-Racer.cmd check, cleanup, final report.

## Previous delivery — World scenery upgrade + rider gestures (fist wave, victory celebration) — 0.78.0-review1 — DELIVERED, REVIEWED BY DAN ("generally looks better"; floating buildings, trees in a driveway and road colour fixed in 0.79.0-review1)

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
- Small, not scheduled: weather affecting grip; CR-118 spoken-title clipping; Mountain berm leftovers; CR-010 steering (needs Dan's yes/no).
- Later: traffic vehicles in Blender; vehicle stats and more vehicles; acorn-completion special vehicle; stunt track (route shown on the map before building); Trickum course; split-screen/online; VR.
- Deferred: Steam Deck checks; friend test of the packaged build.

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
- Multiplayer / split-screen. **Dan's decisions so far (2026-10-06):** the reason is to play with a friend on one PC; **one shared radio, which either player can control** (never two radios); it must be testable by Dan alone (AI can drive player 2's seat, and keyboard + controller); top/bottom split by default with a setting for left/right. Staging suggested by Claude: (1) two-player race on one course, chase cameras, basic HUDs; (2) all courses, garage for both, weather/night, results, AI rivals; (3) two-player Free Roam, per-player views and gestures.

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
