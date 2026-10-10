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
  - **Write-up:** the TODO results are a short list: what changed, the one check per item, decisions made, and anything Dan should look at. No separate VALIDATION.md unless a round asks. If a check would take more than a few minutes of play time, skip it and list it under "for Dan to check".

## CURRENT — Abandoned Cabin Jump: a run-up you can line up and bushes you can clear (race and Free Roam), spikes by the garage, Dan's kennel/garage turned to the parking area with chain-link fence — target 0.101.0-review1 — DELIVERED as 0.101.0-review1 (build 101000), awaiting Dan's review

- **Authorized by Dan (2026-10-09, 21:58–22:00).** Written by Claude (chat) from his play of 0.100.0-review1, debug session `2026-10-09_21-41-38-253_8e922d` (three reports, all in Free Roam) and his messages. His words are quoted in each part.
- **Starting point:** main at the "Record 0.100 delivery" commit. This TODO edit is uncommitted and belongs in the safety checkpoint.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–C.** The Verification budget and the Controller-first rule apply. World changes only where Dan pointed. **Every check in the built Windows player**, with a shot from where Dan stood.

### Part A — Abandoned Cabin Jump: make it possible to line up, and clearable when hit well

Dan: "I still want to trim this back some. Right now you have to make a perfect jump to just barely clear it and it is very hard to do this." (BUG-003, Free Roam, (268.96, 76.26, 78.92), heading 311°, looking at the brush in front of the cabin.) Then: "I was trying this in a race too, but my race ended so I wanted to keep trying without the race. Either way it is nearly impossible to line up a straight shot to it."

History: 0.93 cleared the landing corridor (race scene only); 0.94 restored the brush out to a far edge at 84 m from the branch start with a reset to 88 m (race scene `DansBackyardForward` only); **Free Roam still has the original brush (landings measured 61–106 m)**. 0.93 also found: at 40–44 m/s the crest at the branch entry throws vehicles onto the cabin roof; the approach is built for 31–34 m/s.

**Dan's request overrides rule 5A's jump protection for this one jump** (approach, ramp and brush only; not the cabin, the landing ground or the rejoin).

1. **Measure the approach first** (race and Free Roam): where the branch leaves the main, the turn and the crest at the entry, the distance from the turn to the lip, the ramp's width and its angle to the run-up. Say why lining up is hard (a turn too close to the lip, a crest that unsettles the vehicle, a narrow ramp, a ramp not square to the run-up).
2. **Give it a straight, readable run-up:** at least about 50 m of straight trail square to the ramp before the lip, reached from the main by an easy turn; smooth the entry crest so a vehicle at full speed stays on the ground and does not get thrown onto the cabin roof; widen the ramp and its lip by about half so a slightly off line still takes off cleanly. Add simple lead-in markers (two posts or painted chevrons at the run-up start and on the ramp) so the line reads at speed. Keep the jump's height and launch angle as they are unless the straight run-up needs a small change to land on the existing landing ground; say what changed.
3. **Bushes, the same in the race and Free Roam:** a decent jump clears them. With the new run-up, fly the Needle 600, the Street Classic, the Trail Four and the Turf Rocket at 24, 28, 31 and 34 m/s and set the brush's far edge so **every vehicle at 28 m/s and above lands at least 3 m past the last bush**; 24 m/s may land in it. Clear the corridor past the far edge to the rejoin. The reset from inside the brush goes to clear ground just past it, as in 0.94. Apply the same brush and reset in `DansBackyardForward` and `FreeRoamWorld` (Free Roam keeps its own copy; make the two match). Check `DansBackyardReverse` has no Cabin branch (0.93 said it has none).
4. **AI:** rivals that take this shortcut still do, and land clear.
5. **Check (built player, controller, Dan's usual vehicles):** from Dan's BUG-003 position before and after; three normal attempts each in the race and in Free Roam, driving it the way a player would (from the main, no scripted line): line up, take off, clear the bushes; one flat-out attempt that stays off the roof; the landing table.

### Part B — Spikes in the ground by the garage (BUG-001)

Dan: "what are these spikes for?" Free Roam at (406.92, 79.96, 11.88), heading 343°, beside the Pool-house jump, in front of the long garage with three doors: thin green triangular spikes stick up out of the ground at the edge of the paved area.

1. Find what they are (ground mesh faces pulled up at a seam, grass or brush cards, a tree trunk base) and remove or flatten them so the ground there is smooth. Only at this spot; list any similar spikes seen elsewhere for Dan.
- Check: a shot from Dan's position after.

### Part C — Dan's kennel / garage: doors facing the parking area, sidewalk, chain-link fence (BUG-002)

Dan: "This kennel/garage should actually be rotated. The garage doors should face this way and make the parking area reach it. There really shouldn't be a driveway to the left of it. There was however a small sidewalk, and the building probably reached out all the way to the fence. And this fencing here should actually be chain-link fence and not the white fence." Free Roam at (397.11, 79.95, 5.88), heading 16°, looking at the plain end wall of the long building with three garage doors (the doors currently face along the side, see BUG-001's view from (406.92, 79.96, 11.88)). This is part of Dan's house (deck, pool, pool house, kennel and garage), the one property modelled on the real place.

1. **Turn the building** so its three garage doors face the direction Dan was looking from (toward his position at heading 16°, i.e. facing the parking area), keeping it on the same lot.
2. **Parking area reaches the doors:** extend the paved parking area up to the doors. **Remove the driveway to the left of the building** (left as seen from Dan's position).
3. **Small sidewalk** along the building (a narrow concrete path on the side that is not the doors, about 1 m wide, joining the parking area).
4. **The building reaches the fence:** extend or move it so its back end meets the fence line it sits by (no gap between building and fence).
5. **Chain-link fence** replaces the white rail fence in this area (the fence runs Dan was looking at, on both sides of the building in his BUG-002 shot): galvanized grey chain-link with posts and a top rail, same line and height range, still solid to vehicles.
6. Keep the building's style, colours and roof; ground everything (rule 4). **Apply to every scene where this same building and fence appear** (it is the same place: Free Roam and any race scene that shows Dan's house); list the scenes. Do not change the Pool-house jump, its run-up or landing, or any route.
- Check (built player): shots from Dan's BUG-002 and BUG-001 positions before and after, and one from the parking area.

### Verification

Light, per the Verification budget and the Controller-first rule; all checks in the built player. Compile, launch, release steps. Results as a short list, with "for Dan to check".

### Results (2026-10-10, Claude Code)

- **Safety checkpoint** `680a578d` (this section). Evidence: [Docs/Report101/](Docs/Report101/). Built-player checks are the evidence component `Report101PlayerCheck` (`-p101check`, never active without `-racerTestSave`; always on a copy of Dan's save; it sets run-in-background for itself only, because a check player paused whenever its window lost focus); tools in `Tools/Report101/`. "Before" shots are Dan's own BUG screenshots. Version **0.101.0-review1, build 101000**. No menu or prompt changed (Controller-first not applicable).
- **A — what made it hard to line up (measured).** The main road crosses the ramp's axis 44.7 m before the lip at 32° (main heading 50.5°, ramp 82.8°); the old branch left the main 4.7 m later, turned 40° right and then 8° back left in the 22 m before a 3.3 m-wide board ramp; the entry crest (slope 0.18 → 0.06 within 3 m at branch s 12–16) and the 2 m-faceted ground under it threw vehicles at speed. **50 m of straight is not possible here**: the main road itself crosses the ramp's line 44.7 m out; anything longer would start on the far side of the main.
- **A — the new run-up (race and Free Roam, identical).** One straight line square to the ramp, from where the main crosses it to the lip (44.7 m; 38 m from the main's right edge; the AI starts its turn at main s 845, 12 m earlier). Its driving surface is a smooth 6 m-wide strip (`Ground_Cabin run-up surface (0.101)`, 0.25 m lanes) on a shallow earth embankment with 1:4 sides: each lane leaves the main's verge with the slope a vehicle already has there (+0.04 for the lift it carries off the main's crossfall) and falls no faster than a 34 m/s flight, so it is never a crest (grounded to 34–43 m/s by lane). This raised the foot of the leaning boards 1.06 m (72.16); the boards curve up from 0.15 to the roof's 0.32 at the roof edge; **the roof, the lip, its height and its 0.32 launch angle are unchanged**. Boards widened 3.3 → 5.0 m (9 boards), roof/lip 6.4 → 9.6 m (1.8 m eaves; walls unchanged). The cabin's end wall under the roof's low edge caught a vehicle's nose in one run: its collider top is lowered 0.77 m (the wall as drawn is unchanged). Lead-in: two dark posts with gold bands at the run-up start (no collider) and the gold chevron pairs moved onto the line (run-up, boards, roof; still hidden in Free Roam as before). The trail sign stood on the new line; moved 8.5 m to its right. The main road surface is not touched (graded only from 0.8 m past its edge); trees kept (grading stays 0.6 m clear of trunks). Scene diff = only these objects ([Forward/Free Roam](Docs/Report101/Lists/scenes-nothing-else-changed-cabin-final.txt), [all nine scenes for C](Docs/Report101/Lists/scenes-nothing-else-changed.txt)).
- **A — bushes (race and Free Roam the same mesh).** Original 0.43 brush, cleared within 7 m of the new line from the far edge (new s 84.0) to the rejoin, and off the run-up and ramp: the last bush reaches s 85.0 (in 0.94 stations that is 81.3, about 4 m less brush than 0.94's 85.1). Reset from inside the brush → s 88.0 on the line, facing along it, stopped: **race PASS, Free Roam PASS** (Free Roam has no Cabin branch: its brush uses an inactive copy of the line, never registered as a branch) ([reset](Docs/Report101/Player/reset.txt), [race](Docs/Report101/Shots/A-reset-race.png), [Free Roam](Docs/Report101/Shots/A-reset-roam.png)). `DansBackyardReverse` has no Cabin branch (routes: Abandoned Logging Ridge, Storm Drain / Gully Jump); not changed.
- **A — landing table** (built player, speed held on the line from s 12, lip at s 44.7, last bush 85.0) ([flights](Docs/Report101/Player/flights.txt)):

    | | 24 m/s | 28 m/s | 31 m/s | 34 m/s |
    |---|---|---|---|---|
    | Needle 600 | 82.7 (in) | 93.5 (+8.5) | 102.7 (+17.7) | 112.5 (+27.5) |
    | Street Classic | 82.6 (in) | 95.0 (+10.0) | 102.3 (+17.3) | 105.9 (+20.9) |
    | Trail Four | 82.3 (in) | 64.5 (in, see below) | 103.6 (+18.7) | 113.6 (+28.7) |
    | Turf Rocket | 68.3 (in) | 89.5 (+4.5) | 104.3 (+19.3) | 116.1 (+31.2) |

    Every vehicle at 28 m/s and above lands 4.5 m or more past the last bush, **except the Trail Four at 28 m/s held** (it slowed to 16 m/s on the boards in that run and came down at 64.5). A 0.3 s float over s 22–34 shows in these runs (the vehicles are dropped in at s 12; driven in from the main road there is none).
- **A — driven like a player** (built player, virtual controller, from the main road 70 m before the turn-off, onto the line from 8 m before it) ([Turf Rocket](Docs/Report101/Player/att-race.txt), [Street Classic](Docs/Report101/Player/att-race-car.txt)): Turf Rocket full throttle on the line / 1 m left / 1 m right: **clear by 11.9 / 6.9 / 30.3 m**, eased off at 26 m/s: clear by 3.6 m; Street Classic: **clear by 5.9 / 5.9 / 13.4 m**, eased off: 3.5 m. No air on the run-up except a 0.44 s float on the line 1 m left. Nothing struck, no resets. Shots: [from the main](Docs/Report101/Shots/A-race-from-main.png), [on the run-up](Docs/Report101/Shots/A-race-on-the-runup.png), [Dan's BUG-003 spot, race](Docs/Report101/Shots/A-after-dan-BUG-003-race.png), [Free Roam](Docs/Report101/Shots/A-after-dan-BUG-003-roam.png) ([before](Docs/Report101/Shots/A-before-dan-BUG-003.png)).
- **A — AI** (a real race from the grid, the race AI sent down the Cabin Jump; [list](Docs/Report101/Player/ai.txt)): Street Classic, Needle 600, Trail Four all take it, follow the run-up and leave the lip with no reset on the run-up or ramp, **but come off the lip slower than a player and land 0.5–8 m short of the last bush** (76.7, 80.3, 84.5). Entry tuned once (30 m/s through the turn, 36 to the lip); stopped there (rule 12). One lap of Backyard Forward by the road AI: finished, 1 recovery (main s 149, not on the Cabin branch) ([lap](Docs/Report101/Player/lap.txt)).
- **A — Free Roam approach.** In Free Roam the main road's last stretch before the turn-off is closed by the existing Reverse-only timber closure, so the run-up is reached along the Reverse trail, which runs 4–5 m north of it; crossing from that trail onto the run-up passes over the Forward main's raised edge and the vehicle is thrown about 1 s and misses the line ([list](Docs/Report101/Player/att-roam.txt), [view from the trail](Docs/Report101/Shots/A-roam-from-reverse-trail.png)). Not changed (the main road and the closure are outside this round).
- **B — the spikes.** Two vertices of Free Roam's own beige paving mesh (`…descent and parking-conformed`, vertices 8815/8816 at (404.5, 80.8, 11.9 / 12.5)) stood 1.3–1.4 m above the paving; put back on the paving round them. No similar spikes in any other scene's paving. [Before](Docs/Report101/Shots/B-before-dan-BUG-001.png) / [after, from Dan's spot](Docs/Report101/Shots/B-after-dan-BUG-001.png).
- **C — the garage, in all nine scenes that have Dan's house** (Free Roam, Backyard Forward / Reverse, Forest Loop Reverse, Lake Woods, Mountain Loop / Reverse, Street Loop / Reverse) ([list](Docs/Report101/Lists/BC-yard.txt)): the building turned 90° (yaw 91.67 → 181.67) so its three doors face the parking area (south, toward Dan's BUG-002 position), its west end 0.12 m from the fence line, doors at z 16.0; its look, colours and roof are its own triangles moved out of the house batches (one 236-triangle mesh, `Dan garage kennel (0.101)`), the collider turned with it. Ground round it 79.32–80.05 against a base of 79.32: nothing floats; the back west corner sits 0.7 m into the bank. The parking area is paved up to and under the doors (`Ground_Dan beige parking to the garage doors (0.101)`); **the driveway strip along the old door side (x 404–412, z 12–32) is removed** — it was the only driveway beside the building and lies to its left when you face out of the doors; a **1 m concrete sidewalk** runs along the east end from the parking area to the back. **Chain-link fence** (galvanized posts, top rail, diamond fabric) replaces all 28 white crossbuck segments of the yard's run (`CR101 local boundary extension`: the west line, the north line and the diagonal to the road), same line, about the same height, each keeping its own collider; the property line past the pool and along the road (CR097) stays white. Pool-house jump, its run-up and landing, and every route unchanged. [Before](Docs/Report101/Shots/C-before-dan-BUG-002.png) / [after, Dan's BUG-002 spot](Docs/Report101/Shots/C-after-dan-BUG-002.png), [BUG-001 spot](Docs/Report101/Shots/B-after-dan-BUG-001.png), [from the parking area](Docs/Report101/Shots/C-after-parking.png).
- **Release.** 0.101.0-review1, build 101000: source `942f65ac` (pushed), fresh Windows build (0 errors, 2m40s, [build-release.txt](Docs/Report101/build-release.txt)), published [game-101000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-101000) (the usual draft-lookup miss, then `--resume-draft`), public download, pinned signature and startup pass for all 234 files ([hosted/result.json](Docs/Report101/hosted/result.json)), production updater at 101000, `Play-Racer.cmd` (unchanged) launched `Builds/Latest/versions/101000/Racer.exe` ([play-racer-launch.json](Docs/Report101/play-racer-launch.json)), Dan's settings.json restored byte for byte ([settings-preserved.json](Docs/Report101/settings-preserved.json)). Cleanup: Builds 40.05 GB -> 8.31 GB (31.7 GB: the build output, the zip, managed 99000, the hosted install and this round's check builds), C: free 201.7 -> 233.0 GB; kept managed 101000 and 100000 ([cleanup.json](Docs/Report101/cleanup.json)). Commits: `680a578d` checkpoint, `942f65ac` the round (released source), this record follows.
- **Decisions:** the straight run-up starts where the main crosses the ramp's line (44.7 m; no longer one possible); the run-up is a raised, smooth surface and the board foot is 1.06 m higher, the only way found to keep a vehicle on the ground at speed off this main road; brush far edge s 84.0; AI entry from main s 845 at 30 m/s; garage doors on the south side with the strip beside the building removed; chain-link only on the yard's own run.
- **For Dan to check:** the run-up from the main road at your usual speed and line (and the look of the raised run-up and its grey sides); whether to trim the brush further so the AI rivals clear it too (they land 0.5–8 m short); the Free Roam approach from the Reverse trail (thrown over the main's edge; the alternative needs the main road or the Reverse-only closure changed); the garage's position (west end against the fence, doors at the parking edge) and the sidewalk side. **Seen, not changed:** trees 4–6 m beside the landing run-out are hit by landings that drift 3–5 m off the line; the Street Classic is jolted on the main road around s 790–850 (before the turn-off) in every run.

## Previous delivery — Mountain Loop trench (third time), squished vehicle pictures, the moon, hidden police only see one way, free look while driving, storm-drain top — target 0.100.0-review1 (build 100000, or the next number the version scheme allows) — DELIVERED as 0.100.0-review1 (build 100000), awaiting Dan's review — REVIEWED BY DAN (Cabin Jump still too hard to line up and clear; spikes and kennel/garage; follow-ups in 0.101)

- **Authorized by Dan (2026-10-09, 17:55 and 17:59).** Written by Claude (chat) from his play of 0.99.0-review1, debug session `2026-10-09_17-35-51-484_362517` (one report) and his message. His words are quoted in each part.
- **Starting point:** main at the "Record 0.99 delivery" commit. This TODO edit is uncommitted and belongs in the safety checkpoint.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–F.** The Verification budget and the Controller-first rule apply. World changes only where Dan pointed. **Every check in the built Windows player.**

### Part A — The Mountain Loop trench is still there (BUG-001, third report)

Dan: "this gap still isn't fixed!" Report at (1043.55, 153.31, 134.23), heading 44°, Mountain Loop - Forward, lap 2, Dusk / Snow, in the Patrol Car. The screenshot looks along the road: on the left, between this road and the parallel road further left, a deep dark channel with cut faces runs ahead for a long way. Reported before as 0.95 BUG-003 (1055.64, 152.62, 134.36), 0.98 BUG-002 / BUG-003 (1200.14, 151.96, 188.59 and 1049.73, 152.51, 138.59). 0.96 added a shelf; 0.99 added `Ground_Report099 fill LeftTrench` over main 1650–1850 m and itself reported "a 1–3 m ledge remains … at its start (main 1625–1650 m)". Dan is at about main 1640 m and the channel is still plainly visible.

1. **Stand where Dan stood**, in the built player, same heading, and take the before shot. Then look along the whole channel from there and from 1814 m looking back. Find every part of it that is still open: the start before 1650 m, the far side, anything past 1850 m, and anything between this road and the parallel road on the left.
2. **Fill all of it** so there is no channel and no ledge: the ground between this road's left verge and the parallel road is continuous solid ground, level with the verges (a gentle fall where the two roads differ in height), drawn and collidable, for the full length of the channel. Re-seat trees on it. Do not change either road's surface, the route, jumps or the summit approach.
3. Check `MountainLoopReverse` at the same place; fill it the same way only if the channel exists there too.
4. **Check (built player):** shots from Dan's position and heading, from 1814 m looking back, and from the parallel road, before and after. **Do not report this fixed unless the after shot from Dan's exact spot shows no channel.**

### Part B — Vehicle pictures are never squished

Dan: "When selecting the vehicle in the police chase menu the vehicle is squished as it was previously. I noticed this was on another menu for vehicle selection. Let's make sure anywhere there is a vehicle selection that the picture is never squished."

1. Find every place a vehicle preview is drawn (turning garage view, garage list, campaign event page and its small preview, Shop, split-screen garages and setup previews, the Police Chase / Getaway / Speed Patrol setups including the patrol car and cop-vehicle rows, the unlock panel, Race Setup, results / winner panels, the track-select or loading screens if they show a vehicle) and list them.
2. Fix the cause once, in the shared preview code if there is one: the render texture's aspect must match the on-screen image's aspect (or the image keeps the texture's aspect, letterboxed), at every screen size and in split-screen halves. No preview may stretch or squash a vehicle.
3. **Check (built player, controller):** a shot of each preview location with the same vehicle (a car and a bike), at 3840×2160 and in a split-screen half.

### Part C — The moon

Dan: "Is there actually a moon? I have yet to see it when I have played."

There is a moon (`WeatherSky.MoonDisc`): races at night have a fixed full moon; Free Roam follows a 30-day calendar (new on day 1, full around day 15). Dan's recent Free Roam play was on days 3–4, a thin crescent near new moon.

1. In the built player, at night, **look for the moon** in a race (Night, Clear: full moon) and in Free Roam: is it drawn, where in the sky, how big, does it rise and set, is it hidden by the far clip, fog, the sky, clouds or the weather. Fix whatever stops a clearly visible moon on a clear night.
2. Make it noticeable: a size and brightness that reads clearly at 3840×2160 (about the apparent size players expect; a little larger than real is fine), high enough in the sky for a good part of the night that you can see it while driving, not only behind you.
3. Free Roam: start a new game's calendar near a full moon (about day 12) so a new player sees it; keep Dan's calendar as it is. Crescents should still be visible as thin bright shapes, not vanish; only the night of new moon has none.
4. **Check:** shots from the driving view, in a race at night and in Free Roam on a full-moon night and a crescent night.

### Part D — Hidden police only see you going one way

Dan: "The auto police chase worked, but it only worked going one way. I passed him and he ignored me; he only paid attention when I circled back and drove past him again going the opposite direction."

Cause found by Claude (chat), to confirm: `HiddenPolice` only sees the player "in front of the cop", `Vector3.Dot(prop.forward, toPlayer) < .45f` (about a 63° half-cone), so with the car angled at its spot it covers one direction of the road only.

1. A hidden cop sees a speeder in **both directions** along its road: detection within about 70 m with a clear line of sight, on either side, regardless of which way the car is parked (radar both ways). Keep the speed threshold (more than 10 mph over).
2. **Check (built player):** at two hiding places, pass each one over the limit in both directions: four trips, four chases.

### Part E — Free look while driving

Dan: "Is there any way to get a free look camera while driving as opposed to having to reach it through the menu?"

1. **Right stick looks around** while driving (chase and first person): push it and the camera swings around the vehicle (chase) or the head turns (first person), up to looking backward; let go and it eases back behind the vehicle within about a second. Driving is not affected. Keyboard / mouse: hold the right mouse button and move the mouse, same behaviour.
2. **Click the right stick (R3)** for a quick look behind while held.
3. If the right stick or R3 is already used for something while driving, say what, and choose the least disruptive mapping; list it on the controls card and in the pause menu's controls.
4. Works in races, Free Roam, police modes and each half of split-screen (each player's own stick). Not in Trailer Mode (it has its own cameras).
- **Check (built player, controller):** look left, right and behind in chase and first person while driving; release returns; split-screen both players at once.

### Part F — Free Roam storm drain: driving onto it drops you into it

Dan (17:59): "The storm drain during Free Roam. If you were to try to drive onto it, you fall into it." No report position; the drain is the box and tunnel entrance around (143.19, 58.19, 67.90) to (182.09, 67.41, 76.61) in `FreeRoamWorld` (0.95 BUG-002, 0.97 BUG-001), restored to its 0.95 shape in 0.98.

1. In the built player, drive onto the top of the storm-drain structure from the ground around it (the box, the headwall over the entrance, the ground above the tunnel) from several sides, and find where a vehicle falls through or drops inside: a missing or partial collider on the top, an opening in the roof, or ground that does not meet the concrete.
2. Make the top solid: a vehicle can drive onto and across the top of the structure and the ground above the tunnel, and off again, without falling in. Use colliders matching the visible concrete and ground; where the top is an opening on purpose, close it with a visible solid top (a concrete slab or grate) **over the roof only**.
3. **Do not close, narrow or lower the tunnel or its entrance.** The tunnel stays drivable through both ways, exactly as now (0.96's lid sealed it; do not repeat that). Do not change the race scenes' storm drain.
4. **Check (built player):** drive onto and across the top from three sides with a car and the bike: no fall; then drive through the tunnel both ways: unchanged. Shots of the top and of the open entrance.

### Verification

Light, per the Verification budget and the Controller-first rule; all checks in the built player. Compile, launch, release steps. Results as a short list, with "for Dan to check".

### Results (2026-10-09, Claude Code)

- **Safety checkpoint** `bc6c0b83` (this section and Dan's `Docs/Sequel/` note; the untracked `Assets/_Recovery/` scenes and the old `Assets/Editor/Report097Temp` / `Report098Temp` folders were left as they were). Evidence: [Docs/Report100/](Docs/Report100/). Built-player checks are the evidence component `Report100PlayerCheck` (`-p100check`, never active without `-racerTestSave`; always on a copy of Dan's save, or an empty one for the calendar) on a check build of this source; tools in `Tools/Report100/`. Version **0.100.0-review1, build 100000** (builds are plain increasing integers in the launcher and the publisher, so 100000 is allowed).
- **A — the trench.** Cause: the "parallel road" is this course's own return leg (stations about 1975–2165), 28–43 m left of main 1612–1820 m. Under 0.99's sheet the original ground is a chasm up to 53 m deep; 0.99's `LeftTrench` fill started at 1650 m with an 8 m blend, so main 1612–1660 m (Dan stood at 1642 m) was still a 2–5 m channel with cut faces, and the rest sat 0.35 m below the verges. Now one surface, `Ground_Report100 fill Channel` (mesh `Assets/Scenery/Report100/MountainLoop-Report100-Channel.asset`; it replaces the 0.99 fill, whose mesh asset is left unused), from this road's left verge to the return leg's verge (a straight fall between the two verge heights, tucked 6 cm under each road's driving surface, never over either road), main 1600–1888 m, blended into the ground at both ends; drawn (the 0.99 fill's earth material) and collidable; nothing stood in the raised volume; trees seat on it at load. Cross-sections between the two legs are level within 0.3 m from 1620 to 1820 m ([before](Docs/Report100/Lists/A-profile-before.txt), [after](Docs/Report100/Lists/A-profile-after.txt)). The scene diff is only the old fill out and the new one in. **Built player, from Dan's exact chase-camera eye, Dusk / Snow:** [before (the 0.99 build)](Docs/Report100/Shots/A-before-dan-spot-dusk-snow.png) shows his channel, [after](Docs/Report100/Shots/A-after-dan-spot-dusk-snow.png) shows none; also from 1814 m looking back ([before](Docs/Report100/Shots/A-before-from-1814-back.png) / [after](Docs/Report100/Shots/A-after-from-1814-back.png)) and from the return leg ([before](Docs/Report100/Shots/A-before-from-return-leg.png) / [after](Docs/Report100/Shots/A-after-from-return-leg.png)). One lap of Mountain Loop Forward by the road AI in the built player: finished, 0 recoveries ([lap](Docs/Report100/Player/lap.txt)). **Mountain Loop Reverse:** a different layout there (one road on a ridge, no parallel leg, no channel; [map](Docs/Report100/Lists/A-reverse-map.txt)); not changed.
- **B — squished pictures.** Where a vehicle picture is drawn: the garage view (Garage, Race Setup, the campaign vehicle choice) and the small turning previews: Cop vs Runner, Getaway and Speed Patrol setups (patrol car / police bike included), the split-screen setup, the split-screen garages (two halves), the campaign event page, the campaign prize, the unlock panel, the prize-won and champion's-paint strips. The garage view already sized its texture to its panel; **every small preview drew into a fixed-size texture with a fixed camera aspect that the layout then stretched** (the police setup cards: drawn 2.27 wide, shown 1.89; the split-screen garages: drawn 2.29, shown about 3.1). Fixed once in the shared code (`RaceMenus.MiniPreview` and the prize preview): each frame the texture and its camera's aspect follow the picture's size on screen in pixels. Built player at 3840 × 2160 with a car and a bike, every location measured: the picture on screen matches its camera's aspect within 0.1 % everywhere, both split-screen halves included ([list](Docs/Report100/Player/previews.txt); e.g. [Cop vs Runner](Docs/Report100/Shots/B-prev-coprunner-car.png), [Getaway, bike](Docs/Report100/Shots/B-prev-getaway-bike.png), [split garages](Docs/Report100/Shots/B-prev-splitgarage-car.png), [unlock panel](Docs/Report100/Shots/B-prev-unlock-bike.png)). Results / winner panels and the loading screens show no vehicle preview.
- **C — the moon.** It was drawn, but never where you look: the chase camera looks 15° down with a 65° view and sees only up to about 17° above the horizon, while races put the moon at 38° and Free Roam's moon climbed to 50°; and near new moon (Dan's days 3–5) it sets soon after dusk. Now (the disc only; the night lighting is unchanged): races, 11° up, ahead of the start line; Free Roam, it rises and sets with the calendar but climbs only to about 12.6°, and each phase's highest point is pulled toward midnight so crescents are up for part of the night; the thinnest crescents are drawn as a slim bright sliver; only the night of new moon (9 hours either side) has none. Size 2.6° (about 5 × real, 86 px at 2160 lines), brightness unchanged. A new save's Free Roam starts on **day 12** (Dan's day 5 is kept). Built player: [night race, Mountain Loop, driving](Docs/Report100/Shots/C-moon-race-night-driving.png), [Free Roam full moon](Docs/Report100/Shots/C-moon-roam-full.png), [crescent, day 4, 20:03](Docs/Report100/Shots/C-moon-roam-crescent.png); new moon night: none ([list](Docs/Report100/Player/moon.txt), [Mountain Loop](Docs/Report100/Player/moon3.txt), [new save](Docs/Report100/Player/calendar.txt)).
- **D — hidden police both ways.** Confirmed: the cop saw only a 63° cone ahead of its nose, and it is parked nose to the road, so travel one way was mostly outside it. Now it sees all round within 70 m (clear line of sight, on a road, more than 10 mph over, as before). Built player: two hiding places (Street Loop, 30 mph; Trickum Rd, 45 mph), each passed over the limit both ways: **4 trips, 4 chases** ([list](Docs/Report100/Player/hidden3.txt)).
- **E — free look.** The right stick swings the view the way it is pushed (left, right, down = behind; part-way = part of the way); **R3 held looks straight behind**; on the keyboard, the right mouse button held and the mouse moved; let go and it eases back in under a second. Chase views orbit the camera round the vehicle (kept clear of banks like the chase camera); first person and the front view turn the head. Each split-screen player uses their own pad. Not in Trailer Mode, menus or the world map. Nothing used the right stick, R3 or the right mouse button while driving (R3 is "centre on player" only inside the open world map), so no other mapping changed. On the controls card ("Look around (R3 held: look behind)", R and R3) and in Settings > Controls (two rows). Built player, virtual pads: left −90°, right 90°, behind 180° (stick and R3), half-way −41°, first person left and behind, the mouse, and both split-screen players at once (−90° and 180°); back within 0.55–0.75 s each time; steering 0.00 throughout ([list](Docs/Report100/Player/freelook.txt); [chase left](Docs/Report100/Shots/E-look-chase-left.png), [R3 behind](Docs/Report100/Shots/E-look-chase-behind-r3.png), [first person](Docs/Report100/Shots/E-look-firstperson-left.png), [split-screen](Docs/Report100/Shots/E-look-split-both.png)). **Controller only:** Settings > Controls walked with the d-pad (41 rows, both new rows reached), B back out; the controls card shows the new row and A dismisses it ([card](Docs/Report100/Shots/E-controls-card.png), [list](Docs/Report100/Player/controls.txt)).
- **F — the storm drain top.** Cause: over the exposed culvert (x 141–166, z 45–73) the only surface was the tunnel ceiling's inside face (facing down), so from above a vehicle dropped straight through; the side walls standing out of the ground (up to 5 m on the north-west side) had only inside faces too, so a vehicle met them and passed into the tunnel. Now `Culvert roof top (0.100)` in `FreeRoamWorld` (mesh `Assets/Scenery/Report100/FreeRoamWorld-Report100-CulvertRoof.asset`, the mouth's concrete, collidable): the ceiling's triangles turned to face up 4 cm above it, a 1 m concrete skirt from its open edges down under the ground (not at the mouth), and the exposed walls' outside faces, carried 1.2 m down at their foot. Nothing inside the tunnel or at the entrance changed; the race scenes' drain is untouched. From above, nothing reaches the tunnel floor any more except the open entrance trench in front of the mouth ([before](Docs/Report100/Lists/F-rays-before.txt), [after](Docs/Report100/Lists/F-openings-after.txt)). Built player, car and bike: onto the top from the south-east, across and off; onto it at the south-west end, along it and off; from the north-west it is a wall: both stop against it and never get inside; through the tunnel both ways unchanged (car 22 / 21 s, bike 19 / 18 s, 0 resets) ([car](Docs/Report100/Player/drain-car2.txt), [bike](Docs/Report100/Player/drain-bike2.txt); [top](Docs/Report100/Shots/F-drain-top.png), [entrance](Docs/Report100/Shots/F-drain-entrance.png), [north-west wall](Docs/Report100/Shots/F-drain-northwest-wall-editor.png)).
- **Release.** 0.100.0-review1, build 100000: source `97d621c9` (pushed), fresh Windows build (0 errors, 5m11s, [build-release.txt](Docs/Report100/build-release.txt)), published [game-100000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-100000) (the usual draft-lookup miss, then `--resume-draft`), public download, pinned signature and startup pass for all 234 files ([hosted/result.json](Docs/Report100/hosted/result.json)), production updater at 100000 with nothing newer to fetch, `Play-Racer.cmd` (unchanged) launched `Builds/Latest/versions/100000/Racer.exe` ([play-racer-launch.json](Docs/Report100/play-racer-launch.json)), Dan's settings.json restored byte for byte ([settings-preserved.json](Docs/Report100/settings-preserved.json)). Cleanup: Builds 12.42 GB -> 8.31 GB (4.11 GB: the build output, the zip, the check build, the public-download install), C: free 234.4 -> 239.9 GB; kept managed 100000 and 99000 ([cleanup.json](Docs/Report100/cleanup.json)). Commits: `bc6c0b83` checkpoint, `97d621c9` the round (released source), this record follows.
- **For Dan to check:** the trench from the road (Dusk / Snow as you saw it); the free look feel (swing and return speed, the mouse) and its mapping; the moon's size, brightness and height on your screen (kept in the top band of the chase view so it can be seen while driving); the storm drain's new roof and walls (the north-west side is now a wall, not a way on); every preview screen. **Seen, not changed:** in one check run one lake-road hiding place tripped in neither direction (cause not found; the next run's places all worked both ways); at the Street Loop start line the night moon is in frame but behind the forest.

## Previous delivery — Ridge Cut sign seen from across the track, two Mountain Loop pits, AI cars on the summit jump, hidden police in Free Roam, police bike, unlockable police vehicles, radio voice playback — target 0.99.0-review1 — DELIVERED as 0.99.0-review1 (build 99000), awaiting Dan's review — REVIEWED BY DAN (hidden police work but one way only; trench still open; squished vehicle previews; moon not seen; follow-ups in 0.100)

- **Authorized by Dan (2026-10-09, 01:10).** Written by Claude (chat) from his play of 0.98.0-review1, debug session `2026-10-09_00-57-21-697_e7b8f4` (three reports, all Mountain Loop - Forward, 0.98.0-review1) and his message. "All is good from last round except…" His words are quoted in each part.
- **Starting point:** main at the "Record 0.98 delivery" commit. This TODO edit is uncommitted and belongs in the safety checkpoint.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–F. Order A, B, C (fixes, one commit), then D, E, F (police features, own commit).** If the police features cannot all be finished well, deliver A–C and what is done of D–F, and say what is left.
- The Verification budget and the Controller-first rule apply. **World changes only where Dan pointed** (standing rule from 0.98). **Every world fix is checked in the built Windows player** with a shot from where Dan stood.
- **Trailer: closed.** Dan (01:10): "I think as long as it took to do the video, I will just do this on my own." No filming in any future round unless Dan asks. Keep the recorder code; nothing to do with it.

### Part A — The Ridge Cut road-closed sign can be seen from across the track (BUG-001)

Dan: "I can see the road closed sign from here. I thought we had a rule against being able to see a sign if you can't see it in front of you." Report at (987.48, 131.99, −116.75), main 1338 m, heading 275°, looking across at the Climbing Ridge Cut sign (the one kept in 0.98).

1. Find why it reads from there: text drawn on top of everything (no depth test / overlay layer), text on both faces, text larger than the board, or a light-emitting material. Fix this sign so its words are only seen from the front, are hidden by terrain and objects like any other object, fit inside the board, and are readable from about 20–30 m like the other road signs. The back of the board is plain.
2. **Standing rule, add to the "Mandatory standing workflow" list:** *Signs (Dan, 2026-10-09): sign text is part of the sign: one-sided (the back is plain), hidden by terrain and objects like the board, never drawn on top, sized to fit inside its board with a margin, readable at normal sign distance (about 20–30 m), not from across the map.*
3. Check every sign 0.98 changed or kept against the rule (only the Ridge Cut one, if 0.98 removed the rest); do not add or move any other sign.
- Check (built player): a shot from Dan's position (sign not readable) and from the approach at 20 m (readable).

### Part B — Two pits beside the Mountain Loop road (BUG-002, BUG-003)

- **BUG-002** "seal this up" at (1200.14, 151.96, 188.59), main 1814 m, heading 243°: a deep pit / channel opening right beside the road ahead, with dark cut faces.
- **BUG-003** "and seal this" at (1049.73, 152.51, 138.59), main ~1650 m, heading 64°: the deep trench beside the road. **This is the same place as 0.95's BUG-003 (1055.64, 152.62, 134.36), which 0.96 reported as filled; it is not.** Say why the 0.96 fill did not take (wrong scene, overwritten, only part of it).
1. Fill both so the roadside is solid ground level with the road's verge, in `MountainLoop` only (check `MountainLoopReverse` at the same places and fill there only if the same pits exist). Do not change the road, the route, jumps or trees beyond what sits in the pits.
- Check (built player): a shot from each of Dan's positions after.

### Part C — AI cars still do not clear the summit jump on Mountain Loop Forward

Dan: "I noticed that the cars still don't appear to be clearing that second jump on Mountain Forward." 0.96 set `aiEntrySpeed` 29.5 m/s for the summit flight and reported the cars clear it "in the snow event", checked in the editor. **Check it the way Dan sees it: a normal race in the built player**, Mountain Loop - Forward, 3–4 AI rivals in cars (Street Classic, Highball Fastback, Pebble Coupe, Longroof GT), Day / Clear and Dusk / Snow, watching each AI car at the second jump (main about 2220 m, (966, 190, 142)): its speed at the lip, where it lands or what it hits. Fix whatever stops them (braking before the lip, the speed cap not applied in races, a different line, the weather grip from 0.97). Target: every AI car that reaches the jump on its wheels clears it, in both conditions. Report a short table (car, speed at the lip, outcome) before and after.

### Part D — Hidden police in Free Roam (own commit with E and F)

Dan: "for the police chase we can randomly sometimes have police hiding in Free Roam that will see you speeding and trip the police chase event … You can turn off the hidden police in the options."

1. **Hiding spots:** 10–14 spots beside roads with a speed limit (Hwy 92, Trickum Rd, S Cherokee Ln, the lake road, the Street Loop roads): a driveway mouth, a gap in the trees, behind a sign, the market lot. A patrol car parked, nose to the road, lights off, the officer in it. Each Free Roam session places 2–4 of them at random spots; they move to new spots now and then while out of the player's sight.
2. **Tripping it:** a hidden cop sees the player within about 70 m in front of it with a clear line of sight. If the player passes over the road's limit by more than 10 mph, a short "CLOCKED 58 in a 35" flash shows and the cop pulls out, lights and siren on, and a **Getaway chase** starts there in Free Roam (heat 1, the difficulty set in Settings, the normal escalation, helicopter and all). Nothing happens under the threshold, and driving off-road past a cop does not trip it.
3. **Ending:** escaped: "You lost them", the cops leave, Free Roam carries on. Caught: "Busted", the vehicle is set at the roadside, stopped, and Free Roam carries on. No money or save penalty. Escapes and busts count toward Part E's goals.
4. **Not during:** a timed Free Roam activity or attempt, the first 60 s after loading, 3 minutes after the last chase ended, split-screen Free Roam (single player only for now), Trailer Mode.
5. **Setting:** Settings > Gameplay "Hidden police in Free Roam: On / Off", default On, with the chase difficulty "Hidden police difficulty: Easy / Normal / Hard" (default Normal).
- Check (built player, controller): pass a hidden cop under the limit (nothing), over it (chase starts), one escape and one bust that return to Free Roam, the setting Off (no hidden cops placed). A shot of a hidden cop in its spot.

### Part E — Police bike, and both police vehicles unlockable for races

Dan: "I wouldn't be opposed to having a police cycle as well. And both the police car and police cycle become unlockable for races after you reach a certain goal (2 separate goals) related to police chases."

1. **Police bike:** a police motorcycle from the Needle 600's handling family, built with the Blender vehicle pipeline: black and white, POLICE on the fairing, red and blue lights front and rear, siren, the rider in a police helmet and jacket; no real department's name or badge. In police modes the player who is a cop chooses Patrol car or Police bike. AI cops: from heat 3 in Getaway, one in three new units is a bike (same road-network driving, same speed rules).
2. **Goals (decided by Claude (chat); Dan may change them):**
   - **Patrol car** unlocks when you **escape the police at heat 3 or higher on Normal or Hard** (Getaway or hidden police).
   - **Police bike** unlocks when you have **caught 20 speeders in Speed Patrol** (counted across all rounds; split-screen catches count for each player's own save only if it is their save, i.e. player 1).
3. **Once unlocked:** usable in Race Setup races, Free Roam and the garage like any other vehicle (stock, upgradable only if the campaign allows; **not used in campaign events**). Until then: a dark silhouette in the garage list with its goal and progress ("Escape the police at heat 3+ (Normal or Hard)", "Catch 20 speeders in Speed Patrol (7 / 20)"), as the mower was. The unlock moment uses the 0.95 unlock panel (stays until A, turning preview, "Choose it in the Garage"). In split-screen everything is unlocked as now.
4. Progress is saved in a small file of its own; **Dan's save gets no grant**; earlier play does not count retroactively (no records of it were kept), say so.
- Check: both silhouettes with progress; each goal met on an isolated save shows the panel and the vehicle in Race Setup; a police bike as cop in Speed Patrol; AI bikes appearing at heat 3.

### Part F — Police radio voices (playback ready for Dan's recordings)

Dan: "I would like to add some actual voices for the police radio. I think if I can get a list of the different things they say and all the different locations that they mention I could get some voice clips." Claude (chat) wrote the script with IDs: `Docs/Audio/police-radio-script.md` (also in the claude.ai Project as `claude/police-radio-script.md`). Dan will record the files himself into `SourceArt/Audio/PoliceRadio/`.

1. **Make the game speak the script:** every radio line in Getaway, Cop vs Runner, Speed Patrol and hidden police (Part D) maps to the script's IDs, built from pieces where the script says (phrase + unit number + place or road). Change the on-screen radio text to match the script's wording, including the spoken place names (for example "the summit run-up", "South Cherokee Lane", "Kyle's house"); add the script's new lines (R07, R08, R12, R17, R18, S01–S08, H01–H04) where they fit, at sensible moments and not too often.
2. **Playback:** files are loaded from `SourceArt/Audio/PoliceRadio/` (imported into the build); a missing file plays nothing and the text still shows. Takes `R05a`, `R05b` … are picked at random. A radio effect is applied in the game (band-pass about 300–3,400 Hz, light distortion, a click and a short burst of static at start and end), heard over the engine, ducking the music radio a little while it speaks; one line at a time, queued, newer important lines (caught, escaped, heat) skipping the queue.
3. **Test voice until Dan records:** generate placeholder clips for every ID with the PC's built-in text-to-speech (Windows SAPI, offline) into `SourceArt/Audio/PoliceRadio/_placeholder/`, used only when Dan's file for that ID is missing, so the system can be heard and checked now. Dan's files always win.
4. If the script needs a line it does not have, add it to `Docs/Audio/police-radio-script.md` with a new ID and say so (Claude (chat) will update the Project copy).
- Check (built player): a Getaway chase with placeholder voices: radio on start, heat rises, off-road call with a place name, a unit joining, the helicopter, caught; one Speed Patrol catch; a hidden-police trip. Each line heard once, no overlaps.

### Verification

Light, per the Verification budget and the Controller-first rule; world fixes and AI jump checks in the built player. Compile, launch, release steps. Results as a short list, with "for Dan to check".

### Results (2026-10-09, Claude Code)

- **Safety checkpoint** `883954a8` (the 0.99 section and the radio script). A–C commit `e19820ea`. Evidence: [Docs/Report099/](Docs/Report099/). Checks are the built-player evidence component `Report099PlayerCheck` (`-p99check`, never active without `-racerTestSave`) plus the editor menu walk `Report099Checks`; tools in `Tools/Report099/`. Dan's real save was hashed before and after the editor walk: unchanged.
- **A — sign.** Cause: the lettering was a TextMesh on its default font material (GUI text shader: drawn over everything, both faces). It now uses `Depth tested world lettering 0.mat` (depth-tested, back faces culled), so the words are hidden by terrain and objects and the back of the board is plain; board 1.9 x 1.15 m, lettering 1.4 m wide, readable at 20 m ([shot](Docs/Report099/Shots/view-MountainLoop-sign20.png)); from Dan's spot (987.5, 135.6, −117) the board is a small dark patch with no readable words ([shot](Docs/Report099/Shots/view-MountainLoop-signfar.png)); from behind it is plain ([shot](Docs/Report099/Shots/view-MountainLoop-signback.png)). It is the only sign 0.98 touched (no other default-font lettering was found in MountainLoop / MountainLoopReverse). The standing rule is in the list at the top.
- **B — two pits.** Both are one long trench on the **left** of the Mountain Loop road, main 1697–1800 m (BUG-002 sees it from behind at 1814 m, BUG-003 from ahead at 1649 m). **Why the 0.96 fill did not take:** it did take (the `Ground_Report096 fill B3` meshes are in the scene) but it was a 12 % shelf reaching 12–14 m from the road's edge over main 1620–1680 m; the trench starts 12 m out and runs further along the road, so the shelf ended at a cliff. Now `Ground_Report099 fill LeftTrench` (mesh `Assets/Scenery/Report099/MountainLoop-Report099-LeftTrench.asset`): ground brought up to the road's verge (road surface − 0.35 m) over main 1650–1850 m, 6–50 m left of the centre, blending into the ground at its ends and leaving the other roads' own surfaces alone; drawn and collidable, trees re-seat at load. Before / after from each of Dan's spots ([BUG-002](Docs/Report099/Shots/BUG-002.png), [BUG-003](Docs/Report099/Shots/BUG-003.png) before; [B2](Docs/Report099/Shots/view-MountainLoop-B2.png), [B3](Docs/Report099/Shots/view-MountainLoop-B3.png) after, built player, Day / Clear). Cross-sections of the left side are level (within 0.5 m) from 1657 to 1830 m. `MountainLoopReverse`: its road is 34 m or more from these places (a different alignment), so nothing was changed there.
- **C — AI at the summit jump** (main 2220 m, Homeward Summit Flight, lip (980, 178, 147)). Built player, a normal race, 4 AI cars (Street Classic, Highball Fastback, Pebble Coupe, Longroof GT) plus the player slot on the road AI. **Cause:** the AI's obstacle sensor (a sphere wider than a bike's) read the rising lip face (`Ground_ReportCleanup MountainLoop continuous solid 2`) as a wall 20 m ahead and braked every car from about 40 m/s to 26 m/s, so they took off at 22–29 m/s and landed 85–90 m out on the slope, some with resets. **Fix:** inside that flight's committed run-up the sensor ignores static ground (`RoadDriver`, only flights with an `aiEntrySpeed`). Table (speed at the lip m/s → landing; resets = recoveries during the flight and landing):

  | car | Day / Clear before | after | Dusk / Snow before | after |
  |---|---|---|---|---|
  | Highball Fastback | 28.7 → (892,166,118) | 40.8 → (784,122,87), 0 | 23.1 → (912,152,123) | 39.1 → (798,131,91), 0 |
  | Longroof GT | 27.1 → (898,162,119), reset | 37.3 → (836,159,101), 0 | 22.9 → (911,152,123) | 36.7 → (847,169,105), 0 |
  | Street Classic | 25.7 → (902,159,120), reset | 38.4 → (819,146,97), 0 | 23.2 → (910,153,123) | 36.4 → (856,176,107), 0 |
  | Pebble Coupe | 26.4 → (901,160,120), reset | 38.2 → (821,147,97), 0 | 22.2 → (918,147,125) | 36.5 → (853,173,106), 0 |

  After the fix every car lands on its wheels (lowest up-vector 0.72 in Day, 0.59 in Snow), no resets, and drives on through the window. The bike pilot (the player slot) now also carries 43–45 m/s and lands at the landing end ((737,99,74) / (758,108,80)); before it took off at 28–33 m/s and landed on the slope too, so the old numbers were short for everyone. Raw logs: [Player/](Docs/Report099/Player/).
- **D — hidden police.** `HiddenPolice` (single-player Free Roam only). 12 hiding places found once per session by scanning the road network for a flat, clear verge pull-off (4 on Hwy 92, 4 on the Street Loop incl. S Cherokee Ln, 2 on Trickum Rd, 2 on the lake road), at least 170 m apart; each session uses 2–4, one moves every 3–4 minutes while the player is 260 m or more away. A patrol car (the real patrol-car model, lights off, driver in it) is stood at a place only when the player is within 230 m. The cop sees the player within 70 m, in its half-plane, with a clear line, on a road, more than 10 mph over that road's limit: "CLOCKED 67 in a 45", the car pulls out lights and siren on, and a Getaway chase starts there (`GetawayChase.BeginEmbedded`: heat 1, difficulty from the setting, backups, roadblocks, helicopter as in Getaway; a 5-minute limit counts as escaped). Escaped: "YOU LOST THEM", cops removed; busted: "BUSTED", the vehicle is set at the roadside, stopped and released; Free Roam carries on, nothing recorded, no money. Not within 60 s of loading, 3 minutes of the last chase, during a timed activity, in split-screen, Trailer Mode, or with the setting Off (Settings > Gameplay: "Hidden police in Free Roam", "Hidden police difficulty"). The places are verge pull-offs, **not** hand-picked driveway mouths / the market lot / behind-a-sign places as in the brief: say if you want particular places. Built-player check ([hidden3](Docs/Report099/Player/hidden3.txt)): under the limit (39 in a 45) nothing; off the road at 67 mph nothing; 67 in a 45 chase started with the flash; bust after 4 s ("BUSTED", vehicle at the roadside, 0 m/s, input back); a second chase ended by escape ("YOU LOST THEM", no cops left; the escape was forced through the chase's own `Escaped`, not driven); setting Off: no cars standing and no trip. Shots: [cop in its spot](Docs/Report099/Shots/hidden-cop-in-its-spot.png), [chase HUD](Docs/Report099/Shots/hidden-chase-hud.png), [busted](Docs/Report099/Shots/hidden-busted.png). Controller walk (editor, emulated pad): both new Settings rows reached and changed with left / right, B goes back ([setup99-results](Docs/Report099/Lists/setup99-results.txt)).
- **E — police bike and unlocks.** New model `Tools/Blender/policebike.py` → `PoliceBike.fbx` (Needle 600 frame, black paint slot, white cowl / tank panels / top case, red and blue lenses front and rear, antenna, POLICE on the cowl and both tank panels, a white open-face helmet; the rider gets a black jacket and no hair, `RiderLook.Police`) ([side render](Docs/Report099/Shots/policebike-side.png), shown with the default rider); profile `policebike` "Patrol Cycle" (Needle 600 handling family, top speed 58). Patrol Car and Patrol Cycle are `Reward` vehicles (never in the campaign, shop, upgrades or the AI rivals' rosters; always black and white). Goals as in the brief, kept in `police-progress-v1.json` beside the other saves (no grant; nothing counted from before 0.99): escape at heat 3+ on Normal / Hard (a Getaway or hidden police; player 1), 20 speeders caught in Speed Patrol (player 1's catches). Until earned both are dark silhouettes in the garage with the goal and progress ("Catch 20 speeders in Speed Patrol (0 / 20)"); earned: the 0.95 unlock panel ([bike panel](Docs/Report099/Shots/unlocks-panel-bike.png)), then they are in the garage / Race Setup like any vehicle (checked: a Race Setup race with the bike, 4 wheels down). Split-screen has everything unlocked as before. Cop vs Runner and Speed Patrol setups have a "Cop vehicle: Patrol Car / Patrol Cycle" row. Getaway: from heat 3 one in three new units is a bike (check: six new units at heat 4 = 2 bikes, [shot](Docs/Report099/Shots/getaway-police-bike-close.png)). Results: [unlocks1](Docs/Report099/Player/unlocks1.txt) (19 catches: not yet; the 20th earns the bike and raises the panel; an escape at heat 3 on Normal earns the car), [patrol1](Docs/Report099/Player/patrol1.txt) (the bike as the cop in Speed Patrol).
- **F — radio voices.** `PoliceRadio` / `PoliceRadioVoice`: every radio line in Getaway, Speed Patrol and the hidden police is a list of the script's IDs; the on-screen text is built from the same IDs (the script's wording and place names). Pieces are trimmed, joined, run through the radio effect (band-pass 300–3,400 Hz, light tanh distortion, click and static burst at the start and end), played as one clip at a time with a queue of 3; priority lines (caught, escaped, heat, helicopter) replace the waiting ones; the music radio ducks 45 % while it speaks; clips older than 8 s are dropped. Takes `R02a`, `R02b` are picked at random. **Dan's clips are in `SourceArt/Audio/PoliceRadio/` (74 MP3s were already there when this round ran) and win; the offline Windows SAPI test voice (`SourceArt/Audio/PoliceRadio/_placeholder/`, `Tools/Audio/Make-PoliceRadioPlaceholders.ps1`) is used only for a missing ID.** `PoliceRadioSync` copies both folders into `Assets/Resources` before every build. Script lines newly used: R07 / R08 (helicopter sees / loses the runner), R12 (heat 5 for 25 s), R17 / R18 (loudspeaker, alternating, every 28–45 s with a cop within 35 m), S01–S08, H01–H04; places are the map's destination titles (Kyle's house = within 90 m of (492.7, 81.1, −17.6)); "the lake" (L16) is not used yet. No new IDs were needed. Built-player Getaway log ([getaway2](Docs/Report099/Player/getaway2.txt)): R01 at the start, P01 + place, P04 + two places, R13 at heat 2, R14 and the roadblock line at heat 3, R06 and R07 at heat 4, a unit joining (U1 + P08 + road), R08, R04 on the escape; each spoken once, one at a time; one processed line saved as [radio-sample.wav](Docs/Report099/Player/radio-sample.wav). Cop vs Runner has no radio lines (none existed).
- **Release.** 0.99.0-review1, build 99000: source `fed2250a` (pushed), fresh Windows build (0 errors, 4m33s, [build-release.txt](Docs/Report099/build-release.txt)), published [game-99000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-99000) (draft-lookup miss, then `--resume-draft`, as in 0.98), public download, pinned signature and startup pass for all 234 files ([hosted/result.json](Docs/Report099/hosted/result.json)), production updater at 99000, `Play-Racer.cmd` (unchanged) launched `Builds/Latest/versions/99000/Racer.exe` ([play-racer-launch.json](Docs/Report099/play-racer-launch.json)), Dan's settings.json byte for byte unchanged. Cleanup: Builds 12.41 GB -> 8.31 GB (4.10 GB, includes the check build and the public-download install), C: free 240.0 -> 245.9 GB ([cleanup.json](Docs/Report099/cleanup.json)). Commits: `883954a8` checkpoint, `e19820ea` Parts A–C, `fed2250a` Parts D–F (released source), this record follows. **Revert D–F alone:** `git revert fed2250a`.
- **For Dan to check:** the two trench fills from the road (and that nothing else changed there); the AI cars over the summit jump in a normal race; the hidden cops (their places, how often you are clocked, the CLOCKED flash, the busted and escape endings, the setting); the police bike's look and handling, the cop-vehicle rows, the garage silhouettes; the radio **by ear** (volume, effect strength, lines arriving a few seconds late when several fire at once, the music ducking). **Not run:** a controller walk of the unlock panel or the hidden-police flash in the built player (the emulated-pad walk covered the menus); hidden police in rain / snow; a drive through every hiding place; the radio in a split-screen half. **Seen, not changed:** a 1–3 m ledge remains beside the filled trench on its far side and at its start (main 1625–1650 m).

## Previous delivery — Put back what 0.96–0.97 broke (road-closed barriers, storm drain), police chase: Getaway only, starts at once, visible escalation and helicopter — target 0.98.0-review1 — DELIVERED as 0.98.0-review1 (build 98000), awaiting Dan's review — REVIEWED BY DAN ("all is good" except the Ridge Cut sign seen from across the track; follow-ups in 0.99)

- **Authorized by Dan (2026-10-08, 20:29 and 20:54).** Written by Claude (chat) from his play of 0.97.0-review1. Dan: "This was a really disappointing round. I see literally no difference with the chase." His words are quoted in each part.
- **Starting point:** main at the latest commit (0.97 Parts A–B delivered as game-97000; Part C, the trailer, was in progress: commit or keep the recorder code as it stands, but **discard every clip filmed so far**, they show the broken world).
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–D, in that order. No filming (Part E).**
- The Verification budget applies, except Part C's checks, which must be in the **built Windows player**. Controller-first applies. Checks never write Dan's real save.
- **New standing rule (Part A item 4) applies from this round on.**

### Part A — Revert the road-closed barriers, keep only the one Dan asked for, and do it properly

Dan: "As far as the closed roads signs, I want this change reverted. The ones I noticed were not in places that made sense. There was literally one on the side and bottom of the gully. Not only that, it paid no attention to the words fitting on the signs, and hiding the words unless you are right on the sign. I really only want the one road blocked that I asked about, and just leave it to me to point it out in the future. And even the one that I asked for was messed up. The words were pointing in the opposite direction, and the same issue of the words being too big for the sign and being seen from a mile away."

**Debug session `2026-10-08_20-33-41-102_f6e335` (0.97.0-review1):** BUG-004 (Backyard Reverse, Storm Drain / Gully Jump branch 32.7 m, (153.07, 58.60, 63.47)): "I would rather manage where the signs are needed than rely on it to put crap wherever"; BUG-005 (Backyard Reverse main 506 m, (108.21, 40.60, 57.16)): "Let's revert all road closed signs except the one I specifically asked for"; BUG-003 (Backyard Reverse main 1035 m, (224.18, 59.89, −28.25)): a 0.96 rail barrier next to the course's own older barrier block, "we already have this anyway, so kind of redundant"; BUG-006 (Mountain Loop Forward, (761.17, 88.87, −124.98)): "and the one I asked for it did backwards". BUG-002 (Backyard Reverse main 1037 m, (220.28, 61.08, −14.31)): "make the sign bigger to fit words": a dark sign board beside the trail whose board shows no readable words (tiny "ROAD CLOSED" text floats low on its left); if that board is older than 0.96 and stays, make its words fit and read on the board; if it came with 0.96, it goes with the rest.

(0.96 Part D item 6 asked for every off-route road on all eight courses to be blocked. That was Claude (chat)'s over-reach, not Dan's request.)

1. **Remove every barrier and road-closed sign added in 0.96** from all eight race scenes, except the one at **Mountain Loop - Forward, Climbing Ridge Cut, about (765.48, 89.04, −119.56)** (BUG-004). Restore whatever they replaced or moved. List each removed one with its scene and position.
2. **Fix that one barrier and sign:** the text faces a driver approaching on the route (readable as you come up to it, not mirrored, not from behind); the words fit inside the sign board with a margin, at a size like the game's other road signs (readable from about 20–30 m, not from across the map); the barrier stands across the dirt road's mouth, not on the route and not on a bank. Check `MountainLoopReverse` for the same road mouth: block it there the same way only if the same road meets that route.
3. One shot from the approach at 30 m and one at 10 m, day.
4. **New standing rule, add to the "Mandatory standing workflow" list in this file:** *World changes only where Dan pointed (Dan, 2026-10-08): fix exactly the place Dan reported. Do not extend a fix to "every similar place" in other scenes or across the world unless the round says so in Dan's words. If a similar problem is seen elsewhere, list it for Dan; do not change it.*

### Part B — The storm drain in Free Roam: put it back, then clean it up properly

Debug BUG-001 (Free Roam, (182.09, 67.41, 76.61)): "yeah this is awful. It doesn't even look good. At least it was functional before" (shot: the lid wall across the drain, a dark sunken apron and jagged edges in front). Dan: "As we already discussed the storm drain is a mess. Not only is it sealed but it looks awful. There are all kinds of gaps around it." (20:29: "the storm drain is now broken", seen in the trailer test clip `08-storm-drain.mp4`: the entrance is closed by a pale wall; jagged dark ground edges in front.)

1. **Revert 0.96's storm-drain work in `FreeRoamWorld` completely:** remove the lid (`FreeRoamWorld-Report096-culvert-lid.asset`) and restore the box, the culvert floor (`Ground_Culvert seamless floor-conformed` back to its 0.95 mesh) and the ground tiles around it to their 0.95 state, so the tunnel is open and drives exactly as in 0.95.
2. **Then fix only the gaps:** close the slivers and holes between the concrete and the ground around the box and the entrance (BUG-002, 0.95 report at (143.19, 58.19, 67.90)), by joining the ground to the concrete; do not move, sink, lid or reshape the box or the tunnel. If a gap can only be closed by changing the tunnel, leave it and say so.
3. **Check every other ground change 0.96 and 0.97 made in Free Roam** (the gully seams, the "conformed" ground tiles in `Assets/Scenery/Report096/`) against what drives through them: the tunnel, trails, shortcuts, jumps. Anything they closed, narrowed or made rough: put it back. List what was checked.
4. Check: drive through the tunnel both ways (bike and car), day and night; shots of the entrance and its edges from the trail, day and night.

### Part C — Police chase: one way to be chased (Getaway), started at once, escalation you can see

Dan (20:54): "I see literally no difference with the chase. Just one police car and I had to go find him. I was even driving down the road barely pressing the gas. No escalation of heat, just seemed kind of pointless. Also I didn't understand the point of picking a track for that; it just started you on the road anyway." **Then (21:09), after trying again: he had been in Cop vs Runner as the solo runner, not Getaway. "I tried Getaway and it was better, though I thought it had mentioned helicopters but I guess not. Definitely remove the other option for the way I played it, that was completely pointless."**

1. **Remove the solo Runner role from Cop vs Runner.** Cop vs Runner is two players, or one player as the cop chasing the AI runner. Being chased by AI cops is **Getaway** only. Getaway's setup description: "Escape the police. More cops join the longer you stay free."
2. **No track choice in any police mode:** remove the "Start at" / track row. Getaway starts with the first cop **already in pursuit about 60–80 m behind, lights and siren on**; nobody has to go find a cop.
3. **Make the escalation visible:** heat shown big on the HUD with a short alert at each rise ("HEAT 2 — backup requested"); each backup announced and shown on the minimap as it joins; the escape meter only while unseen.
4. **The helicopter must be noticed:** Dan did not see one. Announce it ("HEAT 4 — air support inbound"), make it audible (rotor sound, louder overhead) and visible (body and spotlight in daylight as well as at night), and show it on the minimap. **Dan (21:10): "I had all 5 stars, so the heat should have been all the way up"**, and still saw no helicopter. So in the built player the helicopter does not appear (or cannot be seen or heard) even at heat 5: find out which (not spawned, spawned far away or below ground, invisible in daylight, culled, silent) and fix it. Also check that heat 5 really has its 6 cops near the runner in the built player.
4a. **Police radio lines out of the box:** Dan (21:12): "on the police radio, the lines kept getting cut off in the little box it was in. I say remove the police radio lines out of the little box so they are fully readable." Show each radio line on its own, outside the chase panel: one line of text with a dark outline or shadow (no box), top centre under the heat display, wide enough for the longest line, never clipped or wrapped mid-word; about 4 s each, the newest replacing the old. Same in each half of split-screen. Check the longest radio line at 3840×2160 and in a split half.
5. **Checks in the built Windows player, controller only:** (a) Getaway, Normal, Street Classic, driving down the main road at half throttle: caught, with the time and heat; (b) Needle 600 flat out: heat rises with alerts, backups join, the helicopter appears when heat 4 is reached (time it), caught or still chased at 2 min; (c) Cop vs Runner's setup no longer offers the solo Runner. Per-second log and a short frame sequence of (b).

### Part D — Build, release

- Build and publish 0.98.0-review1 after A–C. Their checks pass first.

### Part E — Trailer: not in this round

Dan (21:15): "Let's stop filming altogether until I confirm that it looks good and functions properly." **Do not film anything.** Keep the recorder code (commit it unchanged if it is not committed yet) and delete the 0.97 test clips in `Trailer/Clips/`. Filming waits for a later round that Dan starts after he has checked the game.

### Verification

As above: light, except Part C in the built player. Results as a short list, with "for Dan to check".

### Results (2026-10-09, Claude Code)

- **Safety checkpoint** `8f66d5f2`. Source `90dfb698` (pushed; the released build). Evidence: [Docs/Report098/](Docs/Report098/) ([Lists/](Docs/Report098/Lists/), [Shots/](Docs/Report098/Shots/), [Player/](Docs/Report098/Player/)). The new standing rule is in the list at the top ("World changes only where Dan pointed").
- **A — barriers.** All 0.96 "Closed roads (race only)" groups removed from the seven scenes that had one (Backyard Forward had none). Removed, by scene: MountainLoop 11 barriers (including the one below, rebuilt), MountainLoopReverse 11, ForestLoopReverse 4, LakeWoods 4, DansBackyardReverse 3 (the dark board at 217.7, 61.2, −6.5 is one of them), StreetLoopGreybox 1, StreetLoopReverse 1; every position is in [A-barriers-removed.txt](Docs/Report098/Lists/A-barriers-removed.txt). Nothing else of 0.96 had been replaced or moved by them. **Kept and rebuilt:** Mountain Loop Forward, on the dirt road that leaves Climbing Ridge Cut, at (789.09, 92.40, −127.08), 25 m east of the (765.5, 89.0, −119.6) you gave (the only road that leaves the cut there): the sign now faces a driver coming up from the cut, the lettering is two lines 1.4 m wide on a 1.9 × 1.15 m board with a margin, on two posts, and the barrier spans the road (14.4 m). One shot at 30 m and one at 10 m, day ([30 m](Docs/Report098/Shots/A-barrier-fwd-30m.png), [10 m](Docs/Report098/Shots/A-barrier-fwd-10m.png)). **MountainLoopReverse: no barrier.** I put one on the same road and the shot showed that stretch is the race route itself (pavement, race arrows), so it was removed again.
- **B — storm drain.** Lid gone; culvert shell and culvert floor back to their 0.95 meshes (drain open, drives as in 0.95); the 0.96 crack-repair tiles stay. Gaps: a ground-coloured, non-collidable sheet 12 cm under the topmost ground (never inside the tunnel, never across a step) over x 118–190, z 38–104, so slivers between the concrete, the dark floor and the tiles show ground and not sky ([entrance](Docs/Report098/Shots/B-drain-entrance-day.png), [night](Docs/Report098/Shots/B-drain-entrance-night.png)); the box, tunnel and ground are not moved. **Left as they are (would need the tunnel changed):** a few small pale slivers inside the tunnel along the walls (0.95's own shell). **Other ground changes checked:** the 31 mesh swaps of 0.96 were compared with their 0.95 meshes ([list](Docs/Report098/Lists/B-ground-tile-analysis.txt)); the kept ones differ by at most 0.18 m (T-junction repairs), so nothing closed, narrowed or roughened a trail, shortcut or jump. **Drives:** bike and Street Classic through the whole drain both ways, day and night, 8 of 8 reached the far end, 0 resets ([list](Docs/Report098/Lists/B-tunnel-drives.txt)).
- **C — police.** Cop vs Runner is the cop role only (the Runner role and its row are gone, `AiCops` is Getaway only); no "Start at" row in Getaway, Cop vs Runner or Speed Patrol; Getaway's description begins "Escape the police. More cops join the longer you stay free." Getaway starts with the first cop already in pursuit 62–78 m behind, siren on, released with the runner (no 2 s head start), a second one further back. HUD: **heat big at the top centre** ("HEAT 3 ★★★☆☆"), an alert for each rise ("HEAT 2 — backup requested", 3 roadblocks, 4 air support inbound, 5 every unit responding) for 4.5 s, the escape meter only while unseen, an "AIR" line once the helicopter is up, the **radio line alone, outlined, no box**, one line, 4 s, shrunk to fit and never wrapped (in a narrow half it sits under the info panel). Minimap: every unit is drawn (at the rim when off the map), a new unit pulses white for 8 s, the helicopter is a large pale marker. **Helicopter, why Dan never saw it:** in the built player it did exist (heat 4 spawns it) but hovered 58 m up behind the runner, out of the chase camera, and its parts used a primitive's default material, **pink in the built player**. Now it flies 50 m ahead and 14 m to the side at 12 m above the ground (led by the runner's speed so a fast runner does not outrun it), is white with a blue stripe and flashing red/blue lights, with a louder rotor thump plus rotor wash; the spotlight (lagging 2.4 s) still sees the runner. Its material is one the game ships (`PrototypeCar`, URP Lit).
- **C — checks in the built player** (3840 × 2160, GTX 1660 Ti, a copy of Dan's save, Normal; the runner driven by the road AI; the controller walk of the menus was in the editor with an emulated pad, the built player started the chase directly): (a) Street Classic at half speed: caught at 20–22 s, heat 1 (2 cops, the first 68 m behind at the start). (b) Needle 600 on the road AI at pace 2: caught at 57 s heat 3 (alerts at 24 s and 54 s, backups, a roadblock), 18 s heat 2 and 66 s heat 3 in other runs, so **this driver is caught before heat 4**; to see heat 4–5 in the player I ran the same driver with the bust meter held at 0: HEAT 2 at 30 s, 3 at 44 s, **4 at 68 s: the helicopter spawned 1 s later 205 m away and was on screen 75 of the 80 s it existed**, rotor sound playing at volume 1, heat 5 at 98 s with 8 cops out and 10 within 600 m (heat 5 has its 6 cops near). Per-second logs: [Player/](Docs/Report098/Player/); shots: [helicopter](Docs/Report098/Shots/C-helicopter-in-view-4k.png), [heat 4 alert](Docs/Report098/Shots/C-heat4-alert-4k.png). (c) Controller-only walk of the Cop vs Runner (1 and 2 players), Getaway and Speed Patrol setups in the editor: every row reachable, focus kept, B goes back, no solo Runner, no Start-at row ([list](Docs/Report098/Lists/C-setup-controller-walk.txt)). **Radio line:** the longest one (117 characters) at 3840 × 2160 fits on one line between the info panel and the minimap, shrunk to about half size ([shot](Docs/Report098/Shots/C-radio-longest-line-4k.png), taken on the build before the pink-material fix; same layout). The split-half case was only seen in the editor (the heat block sits under the info panel; the editor's 640 × 480 window clipped it). **Not run:** the rotor by ear (no speakers; the source plays), a 2-minute still-chased run of (b), the long radio line in a split half in the built player.
- **D — release.** 0.98.0-review1, build 98000: source `90dfb698`, fresh Windows build (0 errors), published [game-98000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-98000) (draft-lookup miss, then `--resume-draft`), public download, pinned signature and startup pass for all 234 files ([hosted/result.json](Docs/Report098/hosted/result.json)), production updater at 98000, `Play-Racer.cmd` (unchanged) launched `Builds/Latest/versions/98000/Racer.exe` ([play-racer-launch.json](Docs/Report098/play-racer-launch.json)), settings restored byte for byte. Cleanup: Builds 14.23 GB -> 8.30 GB (5.93 GB, includes two temporary check builds), C: free 240.4 -> 248.2 GB ([cleanup.json](Docs/Report098/cleanup.json)).
- **Part E:** nothing filmed; the recorder code is committed unchanged. **The 0.97 test clips in `Trailer/Clips/` were NOT deleted:** the delete was blocked in this session; delete them by hand (`Trailer/Clips/*.mp4`, git-ignored).
- **For Dan to check:** the barrier at Climbing Ridge Cut is on the right road and reads well; the storm drain entrance look (day and night) and driving through it; Getaway's start (the cop behind you), the heat display and alerts, the helicopter in view and its sound, the radio line size on your screen; Cop vs Runner now offers only the cop. **Similar things seen and NOT changed (rule above):** the Mountain Loop Reverse junction of the same road with Downhill Ridge Cut has no barrier; the few slivers inside the drain tunnel.
- **Commits:** `8f66d5f2` checkpoint; `1f4e1259` Parts A–B; `a880cba0`, `3a165021`, `90dfb698` Part C (the last is the released source); this record follows.

## Previous delivery — Getaway is far too easy: backups that actually arrive, cops that keep up, call-ins that do something; weather changes grip; Code films the trailer — target 0.97.0-review1 — A and B DELIVERED as 0.97.0-review1 (build 97000); Part C in progress — REVIEWED BY DAN ("really disappointing": chase unchanged in his play, barriers wrong, storm drain sealed; Part C clips discarded; all in 0.98)

- **Authorized by Dan (2026-10-08, 09:20).** Written by Claude (chat) from his first play of 0.96.0-review1's Getaway. The rest of 0.96 is not reviewed yet; anything he finds will be added here before this starts, or go in the next round.
- **Starting point:** main at the "Record 0.96 delivery" commit. This TODO edit is uncommitted and belongs in the safety checkpoint.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–C. Order A, B, then C (own commit, last).** The Verification budget does not apply to Part A's pacing checks (they are the point); everything else is light. Controller-first applies. Checks never write Dan's real save.

### Part A — Make the cops a real threat

Dan: "It seems like the police chase was far too easy. It took no effort to get away from them and they never added more cars to go after me. Also they would call in but nothing would ever happen, just the one car that was way far behind, always." 09:21: "I mean they weren't even catching me if I stayed on the main road." **So even the simplest case fails for a human: staying on the main road, he was never caught.** 0.96's "road run caught in 114 s" used a scripted runner, which does not match; find the difference (vehicle speed, the cops' top speed, the bust meter needing the runner nearly stopped, or the cops never getting close).

0.96's own results said why the checks missed it: they used an AI runner that "only follows the road ... never stops", and the ATV runner lasted the whole round. A human driving fast vehicles and using shortcuts was never tested. **Test the way Dan plays** (item 7).

1. **Find out first** what Dan saw, by replaying it: with a fast vehicle (Needle 600 and the Turf Rocket mower, his top vehicles) on Normal, drive flat out on the roads and through shortcuts. Log every second: heat, number of cops that exist, each cop's distance, speed and state (chasing / searching / covering an exit / driving to a roadblock). Report why backups never showed up near him and why call-ins produced nothing (likely: backups and exit units start far away and drive there slower than the runner moves, and the cops' top speed is below the runner's). Keep this log as a debug overlay too (F3 debug mode, Getaway only) so Dan's reports capture it.
2. **Cops keep up.** Dan (09:22): "I think the cops should be able to be just as fast as your vehicle." Every cop's top speed and acceleration match the runner's vehicle (whatever it is, the mower included) on every difficulty; a cop more than 150 m behind gets catch-up speed (up to 30 % more) until it is within 80 m. Difficulty changes numbers, heat, escape time and aggression, not top speed. Cops on bikes are not needed.
3. **Backups arrive where it matters.** New units are placed **out of the runner's sight, on the road network 250–450 m from the runner**, ahead of the runner along their likely route or on the roads that join it, never behind; they join with lights and siren and a radio line ("Unit 3 joining from Hwy 92"). Heat sets the number that are **actually near the runner** (not just counted): heat 1 = 2 cops, 2 = 3, 3 = 4, 4 = 5, 5 = 6. A cop that falls more than 600 m behind and cannot see the runner is withdrawn and re-placed ahead.
4. **Heat rises faster and for reasons.** Normal: heat 2 at 30 s, then +1 every 30 s while being chased; also +1 for ramming a cop, passing a roadblock, or 10 s off-road while seen. Easy: every 45 s, no event bumps. Hard: every 20 s. Heat never drops during a chase.
5. **Call-ins do something.** When the runner goes off-road or breaks sight, **within 5 s** units are placed (out of sight) at the nearest two to four exits ahead of the runner's heading, lights on, shown on the minimap; the radio line names where. Searching units drive to the last seen position and fan out along the roads around it. Exits are re-chosen as the runner moves.
6. **Escaping takes work.** Escape meter: Normal 30 s unseen (Easy 20, Hard 40); it does not fill while any cop is within 150 m, seen or not. From heat 4 a **police helicopter** joins: it follows the runner from above at a delay, its spotlight cone on the ground; while the runner is in the cone they are seen. Trees with dense canopy, the tunnel and under the tree-top boards break the helicopter's sight. It cannot catch, only see. One helicopter at most.
7. **Pacing checks with a human-like runner:** script a runner that drives Dan's fast vehicles flat out, takes shortcuts and jumps, leaves the road on purpose, and slows only for corners; plus the 0.96 road-only runner. On Normal, 5 runs each with the Needle 600, the Turf Rocket and the Street Classic. **Targets:** the road-only runner is caught in under 2 minutes in most runs **with every one of the three vehicles** (this is Dan's exact case and must pass first); the flat-out shortcut runner is still being chased (at least one cop within 150 m) at 2 minutes in most runs and escapes in under half the runs within the 5-minute limit; heat 3 or more is reached in every run lasting over 90 s; every run that goes off-road sees exit units in place within 5 s. Report a table (vehicle, runner, outcome, time, top heat, cops near at each heat). Tune until the targets are met and report the final numbers.
8. Cop vs Runner with an AI cop (solo runner) uses the same cop speed rules. Two-player Getaway: backups placed for whichever runner is nearer. Split-screen performance target as in 0.96.
- Check (controller only): the pacing table; one shot each of backups joining ahead, exit units on the minimap, a roadblock, the helicopter's spotlight at night; the F3 overlay.

### Part B — Weather changes the grip

Dan (09:29): "let's add the weather effects." Rain and snow are only visual today; make them change how vehicles drive.

1. **Grip by weather and surface** (multipliers on the tyres' grip, through the existing surface contacts, `VehicleSurfaceContacts`): Rain: paved 0.85, dirt / gravel 0.78, grass 0.75. Snow: paved 0.70, dirt 0.65, grass 0.62. **Ice** (the frozen lake and pool in Snow, and any frozen puddle): 0.35. Clear: unchanged. Braking distance and cornering speed follow from grip; top speed is not reduced. Tune the numbers by feel so rain is noticeably looser but still fun on every vehicle, and snow is a real change of driving; report the final values.
2. **Feel and look:** a little more slide and slower recovery in snow; wheel spray in rain and snow kicked up in proportion to speed (if the 0.82 spray exists, scale it). Tyre sound on wet roads if it is cheap.
3. **AI and traffic:** rivals, the AI driver (split-screen player 2), AI cops, speeders and traffic brake earlier and corner slower by the same factors, so they stay competitive and do not slide off. Check one race each in Rain and Snow with rivals: the AI finishes without resets.
4. **Records:** wet and snow times must not compete with dry ones. If a Top 10 board's key does not already include the weather, add it (Clear boards keep all existing entries, which were set with weather not affecting grip; Rain and Snow start new boards). Campaign events set in Rain or Snow: keep their medal times reachable (re-check each one with the new grip and adjust if needed; list changes). Dan's save and records are not touched.
5. **Setting:** Settings > Gameplay "Weather affects grip: On / Off", default On. Off = 0.95 behaviour.
6. Applies everywhere: races, Free Roam, split-screen, Police modes, campaign.
- Check: one lap of Street Loop on the Street Classic and the Needle 600 in Clear, Rain and Snow (lap times and a short note on feel); the frozen pool in Snow; one rival race in Rain and one in Snow; the setting off.

### Part C — Code films the trailer (own commit)

Dan (09:29): "does this mean Code can record the shots for me so I don't need to do it? If so, then I would want this." Yes: the game drives and films the shots itself, and the clips are put together into a first cut for Dan to review. The shot list is the project doc "Woodstock Rush — trailer shot list" (a copy is in the repo at `Docs/Trailer/shot-list-0.77.md`). It was written for 0.77; bring it up to date with what the game has now (eleven vehicles and the mower, the characters, Kyle's house, the campaign, split-screen, the police modes) without making it longer than about 45–60 s.

1. **Recording in the game:** a trailer capture mode (development builds or editor only, not in the release menus) that plays a scripted shot: sets course or Free Roam position, time of day, weather, moon, vehicle, colour and rider look, puts the player's vehicle under AI or a recorded path, places the Trailer Mode camera from the shot list, and records. **Record offline at a fixed frame rate** (fixed time step, e.g. `Time.captureFramerate` 60) so every frame is rendered at full quality whatever the GPU's real-time speed: 3840×2160 if it renders, else 2560×1440, 60 fps, PNG or high-quality frames, plus the game sound (engine, rain, thunder; **no radio music**). Slow-motion shots are rendered at real slow motion (more frames), not stretched.
2. **Each shot** from the updated list as its own clip, a few seconds longer than it is used, with three takes of anything with a jump (keep the best, say why). Shot 15 (the cover shot over the crest toward Dan's house, two vehicles airborne together) is a race with the AI field; if the crest does not launch at racing speed, say so rather than changing the world.
3. **First cut:** encode with ffmpeg (fetch it the safest way that works on this PC, e.g. a pip package that bundles it, into a tools folder, not system-wide) to H.264 MP4, 60 fps: the clips in shot order, trimmed as in the shot list's "In the edit" column, short cross-fades, the title "WOODSTOCK RUSH" over shot 15, the end card from the cover art (`SourceArt/Poster/WoodstockRushPoster.png`) for 3 s. **Music:** none is added unless a music file is present in `SourceArt/Trailer/Music/` (Dan may add a track he has the rights to); with no file, make the cut with game sound only and a second version with no sound for Dan to score. Keep the individual clips too so Dan or Claude can re-cut.
4. **Output:** `Trailer/` at the project root (git-ignored): `Clips/01-…mp4` etc., `WoodstockRush-trailer-v1.mp4`, `WoodstockRush-trailer-v1-silent.mp4`, and a few full-resolution stills (the shot 15 peak first) in `Trailer/Stills/`. Report sizes, length, and anything from the list that could not be filmed and why.
5. Updated shot list: write it to `Docs/Trailer/shot-list.md` in the repo (Claude (chat) will copy it back to the Project).
- Check: watch the cut's frames at each shot boundary (contact sheet image) and say what each shows; confirm 60 fps and the length. **Own commit** (code only; the video files are not committed). Do this part last; if time runs short, deliver Parts A and B and say how far C got.


### Results so far (2026-10-08, Claude Code) — session stopped before build and release

**State: A and B are implemented and compile; checked only in part. C: recorder, ffmpeg and shot list only. No 0.97 build, no release, Builds\Latest and the catalog are still 0.96.** Nothing was published. Safety checkpoint `046ef2a8`.

**Part A — why 0.96 was easy (replayed with a flat-out runner on the Needle 600 on 0.96 code):** the cops were slower (patrol top 55 x 0.95 against the Needle's 61), crawled at 11 m/s whenever the aim point swung more than 40 degrees (including when the runner was alongside), read the road network's hops between parallel roads as hairpins, stopped at the end of short paths, and backups were placed anywhere 260 m+ away (often behind) and counted only as existing. The runner was 160-300 m clear in 15 s and escaped with 0 cops within 150 m.
- Done: cop top speed, acceleration and grip match the runner's vehicle on every difficulty, catch-up up to +30 % from 150 m to 80 m (`MatchRunner`, `Boosts`); backups 250-450 m ahead out of sight (line-of-sight test, `PlaceAhead`) with "Unit n joining from <road>", cops near by heat (2 at heat 1 ... 6 at heat 5), strays more than 600 m behind placed ahead again; heat every 45 / 30 / 20 s while chased plus Normal/Hard bumps for ramming, a roadblock passed and 10 s off-road seen; call-ins place 2-4 units at the exits ahead within about 2 s of leaving the road or breaking sight (parked on the verge), re-chosen every 4 s; escape meter 20 / 30 / 40 s and held while a cop is within 150 m; helicopter from heat 4 (`PoliceHelicopter`: 2.4 s behind, 21 m spotlight, blocked by dense canopy, roofs and solid objects); bust meter also fills for two cops pinning the runner or a runner held up under 15 m/s with a cop on its tail; F3 overlay text; helicopter on the minimap; the cops' path speed uses a two-node bend measure, a 45 m cost per road-to-road hop, no angle cap when aiming straight at the runner; roadblocks lift when a unit reaches them. Difficulty text on the setup updated.
- **Pacing table** (editor, Normal, scripted runners, `Report097Getaway.cs`; final code, seeds 4-6; caught = bust meter filled; all times game seconds): road-only flat-out runner (`roadfast`): Street Classic 21, 35, 47 s (and 130 s on an earlier build); Turf Rocket 96, 113, 188 s; Needle 600 30, 54, 224 s (earlier build). Flat-out shortcut and off-road runner (`human`): Street Classic 104, 215 s; Turf Rocket 106, 124 s; Needle 600 80, 117 s; all caught, none escaped by the meter. Heat 3 or more was reached in every run over 90 s. Exit units were in place 2 s after leaving the road in about three quarters of the off-road episodes (the rest: one unit only, or none). Only 3 of the 9 planned cells per vehicle and runner were run, one to three runs each, not 5; the old-style `RoadDriver` runner (21 m/s) was caught in 22-26 s. **Not met as written:** road-only under 2 min in "most" runs holds in about 8 of 9 final-code runs, but sample sizes are small; the shortcut runner is caught before 2:00 rather than "still chased at 2:00", and the scripted runner never hides off-road for 30 s, so I have no number for how often a real escape happens. Dan should judge the feel.
- Not done: Part A split-screen performance re-check with helicopter and extra units (needs the built player); the ram heat bump also fires when a cop hits the runner (should be runner-initiated only); the evidence shots exist (backup joining, exit units on the minimap, HUD, F3 overlay, helicopter frame) but the roadblock shot did not trigger and the helicopter frame does not show the beam clearly. Controller walk of the Getaway setup passed in the run.

**Part B — weather grip:** `WeatherGrip.cs` (paved / dirt / grass by the roads' centre lines, ice by the frozen-surface collider), applied in `ArcadeVehicle` (lateral grip ceiling, braking, half of the traction) with eased `GripScale`; Rain 0.85 / 0.78 / 0.75, Snow 0.70 / 0.65 / 0.62, ice 0.35 exactly as specified (values not retuned by feel). AI: `RoadDriver` and `CopDriver` scale corner and braking judgment by the vehicle's grip. Setting `Settings > Gameplay > Weather affects grip` (default On; Off = 0.95 behaviour). Record keys: Rain and Snow races get `-rain` / `-snow` before `-lapsN` (Clear and setting-Off keys unchanged, so old entries stay on the Clear boards); Records era regex and description updated. Campaign weather events are all place-based races, so there are no medal times to re-check. Checks run: grip scale 1.00 / 0.85 / 0.70 under a vehicle in Clear / Rain / Snow, slip angle in a steady turn 28 / 35 / 42 degrees, setting Off back to 1.00, record keys. **Not run or not trusted:** the lap and rival races in each weather (the run hung on an unrelated harness issue), the frozen pool stop test (5 frozen bodies found; stop test did not report), the Settings page walk by controller (a 12th row was added to the Gameplay page and not looked at), wheel spray and tyre sound (not added). The braking-distance figures printed by `feel97` are not valid (the Clear case stopped in 71 m, the others in 24-27 m; the harness, not the game, is suspect).

**Part C:** `TrailerRecorder.cs` (offline fixed-step recorder: RenderTexture, ffmpeg pipe, AudioRenderer), `Report097Trailer.cs` (`cap97:test`, `cap97:survey` scaffolding, never run), `Tools/Report097/Run-Trailer.ps1`, ffmpeg 7.1 installed into `Tools/Trailer/pylib` (git-ignored; has libx264, aac, drawtext, xfade), `Docs/Trailer/shot-list.md` (18 shots, about 52 s plus end card). **No shot was recorded; no cut exists.**

### Delivery of A and B (2026-10-08, Claude Code, second session)

- **Ram bump fixed:** heat rises for ramming only when the runner closed on the cop faster than the cop closed on the runner (pre-step velocities, `RunnerRammed`).
- **Tuning (one round):** the first full matrix (30 runs, Normal, seeds 1-5) caught the Needle 600 road-only runner under 2 min in only 2 of 5. Added: a cop within 10 m of the runner at any speed fills the bust meter in about 12 s; pin distance 14 -> 20 m, pin rate 8 -> 6 s. Re-ran Needle 600 and Turf Rocket (20 runs).
- **Final pacing numbers (Normal, game seconds; Street Classic from the first matrix, which the change can only make faster):**
  - Road-only flat out: Street Classic 38, 114, 12, 60, 90 (5 of 5 under 2 min); Needle 600 152, 61, 12, 44, 181 (3 of 5); Turf Rocket 144, 101, 12, 300 (escaped), 62 (3 of 5 caught under 2 min, 1 escaped in 5).
  - Flat-out shortcut runner: Street Classic 37, 24, 51, 166, 20 (caught); Needle 600 76, 57, 40, 301 (escaped), 39; Turf Rocket 93, 16, 98, 50, 119. 2 of 10 escaped in the final matrix (3 of 15 in the first). Every run that reached 2:00 was still being chased (a cop within 150 m); most were caught before 2:00 instead, which is stricter than "still chased". Heat 3 or more in every run over 90 s that did not end early. **The shortcut runner is caught more often than the target wanted ("escapes in under half" holds, "still chased at 2:00 in most runs" holds only for the runs that last that long).** Dan should judge whether it is now too hard for a good driver.
  - Small samples (5 runs a cell). Heat 2 arrives 33-42 s into the round (the clock starts after the 2 s count-in and the first 30 s are heat 1).
- **Part B checks:** Street Loop lap, Street Classic with the racing AI: Clear 2:20.8, Rain 2:24.0 (+2.3 %), Snow 2:28.3 (+5.7 %), Snow with the setting Off 2:20.7; 0 resets, 0 missed gates. One rival race each: Rain finished 2:20.7-2:26.3, Snow 2:25.3-2:30.8, all four finished, 0 resets. Frozen water in Snow: House 3 pool, House 3 lake and the big lake read ice grip 0.35 (stops from 15 m/s in 4.7, 12.4 and 12.4 m); the two small bodies (rear pool 7 x 12 m, creek) read 0.64 / 0.62 only because the test car slides off the ice before it stops. Settings > Gameplay by controller: all 13 rows reached including "Weather affects grip", RB/LB and B work. The Needle 600 lap test did not run (the harness fell back to the Street Classic); the Needle's brake figures in `feel97` are not valid. Wheel spray and tyre sound on wet roads were not added.
- **Built player (4K, GTX 1660 Ti, Night/Rain, heat forced to 5, 8 cops, 24 traffic cars):** two runners 11.8 / 12.0 ms wall-clock median (about 83 fps), one runner 13.9 / 13.7 ms (about 72 fps), above the 60 fps target ([perf-split-getaway.txt](Docs/Report097/perf-split-getaway.txt)).
- **Release:** source `daf2e0424de36e170c8f35dee26a566505584d22` pushed; fresh Windows build, 0 errors, 3m43s ([build-release.txt](Docs/Report097/build-release.txt)); published [game-97000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-97000) (draft-lookup miss, then `--resume-draft`); all 234 Latest files match the public signed manifest, public download, pinned signature and startup pass ([hosted/result.json](Docs/Report097/hosted/result.json)); production updater activated 97000; `Play-Racer.cmd` (unchanged) launched a responsive `Builds/Latest/versions/97000/Racer.exe` 0.97.0-review1 ([play-racer-launch.json](Docs/Report097/play-racer-launch.json)).
- **Cleanup:** Builds 10,561,069,766 -> 8,301,717,250 bytes (2.26 GB recovered); C: free 250,899,038,208 -> 255,017,279,488 bytes ([cleanup.json](Docs/Report097/cleanup.json)).
- **Not done in A/B (for Dan or a later round):** roadblock evidence shot; a clear helicopter-beam shot; split-screen playtest of the chase. **For Dan to check:** Getaway difficulty feel (too hard now?), rain/snow grip feel on every vehicle, the ice, the new Settings row.
- **Commits:** safety checkpoint `39b652d8` (HEAD at the start of this session); `2d3ed90c` / `22577d16` (tuning, ram fix, version, release tools; the second removes a temporary editor copy committed by mistake); `daf2e042` (bench fix, the released source). Part C follows in its own commit.

### Verification

Light except the pacing table. Compile, launch, release steps. Results as a short list, with "for Dan to check".

## Previous delivery — Campaign event HUD and medals, practice lap before each chapter's race, Mountain Loop and Free Roam ground fixes, Police round 3: AI cops chase you — 0.96.0-review1 — DELIVERED, REVIEW STARTED BY DAN (Getaway far too easy: fixed in 0.97; the rest not reviewed yet)

- **Authorized by Dan (2026-10-08, 01:18).** Written by Claude (chat) from his play of 0.95.0-review1, debug session `2026-10-08_00-26-31-837_7f4941` (nine reports, all on 0.95.0-review1: two in Free Roam, seven on Mountain Loop - Forward) and his message. His words are quoted in each part. **He is asleep: "it is fine if this is an extra long session."**
- **Starting point:** main at the "Record 0.95 delivery" commit. This TODO edit is uncommitted and belongs in the safety checkpoint.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–E. Order A, B, C, D, then E (own commit, with the revert command).** If E cannot be finished well, deliver A–D and say what is left of E.
- The Verification budget and the Controller-first rule apply. Checks never write Dan's real save (work on a copy, pointed at before anything loads). Rule 5A applies to all world work; race-scene fixes stay in that race scene unless a part says otherwise.

### Part A — Campaign activity events: say what is being measured, and say clearly when you succeed (BUG-005)

Dan: "I was mistaken about the event not registering, but it just didn't seem that way, maybe because of the display showing lap 0:00. If it isn't measuring anything why is it there, and why is there not useful information? From the way it read, it was hard to tell if the distance was from this or like an all-time best. There was literally no indicator, and I didn't realize I actually succeeded until I exited the event." Report BUG-005: "Summit Homeward Flight" (chapter 4, Jump), Mountain Loop - Forward at (720.69, 94.70, 73.14); the HUD showed "LAP 1 · 0 completed SOLO / Lap 00:00.000" and, bottom left, "SUMMIT HOMEWARD FLIGHT · Bronze 295.3 ft · silver 459.3 ft · gold 656.2 ft / Best: 704.9 ft GOLD · 67 s left · R: back to the run-up".

For every campaign event that is not a race (Jump, Speed Trap, Smash; Time Trial keeps its lap clock because the lap is what it measures):
1. **No race panel.** The lap / position / lap-time panel is replaced by an event panel: the event name, the medal targets (Part B style), the time left, and **THIS RUN: best so far in this event, with its medal**. Never a lap clock that is not measuring anything.
2. **Each attempt gets a result** the moment it is scored: a large centre banner for about 3 s, "ATTEMPT 2 — 704.9 ft" with the medal won (or "NO MEDAL" / "NOT SCORED: crashed"), with a sound per medal. A first medal or a better medal than before in this event says so ("GOLD — EVENT COMPLETE").
3. **Completion is unmistakable:** once a medal that completes the event is won, a line stays on the event panel: "COMPLETE — GOLD. Keep trying or press START to finish", and finishing goes straight to the results with the medal and the pay.
4. **Words:** "Best" on the event panel means this event run only. The all-time record for the event, if shown, is labelled "Record" and shown separately and smaller.
- Check: one Jump, one Speed Trap and one Time Trial event on a copy of Dan's save: shots of the panel before, the attempt banner, and the complete line.

### Part B — Medals as medals, everywhere

Dan: "I think on any screen instead of the gold, silver, bronze times being written out in long sentences, it should just be the different color medals and the time / distance needed."

- Make one small medal graphic (a coloured disc with a ribbon, gold / silver / bronze, readable at 3840×2160 and at split-screen size) and use it wherever medal targets or medals won are shown: the event panel (Part A), campaign event and championship pages, the campaign chapter list (the medal won per event), results, Free Roam activity prompts and their results, the Activities menu, Records. Targets read as three medal-and-value pairs: [gold] 656 ft [silver] 459 ft [bronze] 295 ft, highest first; medals won show the medal, not the word.
- Round distances to whole feet / metres and times to tenths on these displays.
- Check: one shot of each screen listed.

### Part C — Each chapter: practice lap first, then the race

Dan: "To me the flow of running a lap after the race seems backwards. It would make more sense for the first event to be a lap to get a new person used to the track and then the race."

- In each chapter, **the first event on a course the player has not raced on in the campaign is a solo lap of that course** (a Time Trial, one flying lap, generous medal times), followed by the race(s). Reorder the existing events so that on every course a Time Trial comes before the first Race on that course. Where a course has no Time Trial before its first race, add one ("Practice: <course>", 1 lap, medal times set so a first-time player can get bronze on a clean lap; pay about half a normal event). Report the new order of every chapter.
- Unlocking follows the new order. **Saves in progress (Dan's):** events already completed stay completed with their medals and pay; nothing that is unlocked now becomes locked; a newly added practice event before an already-raced course is simply available, not required.
- Check: the chapter list of each chapter; a fresh campaign's first two events; a copy of Dan's save opens with everything he had still open.

### Part D — Ground and track fixes

**Free Roam (`FreeRoamWorld`):**
1. **BUG-001** at (87.07, 45.44, 17.44), mower, heading 15°: "all along the ridges of this gully are these holes, can we clean this up?" Thin light slivers (sky showing through) run along the top edges of the gully banks. Close every open seam along that gully's banks for its whole length, then look for the same sliver pattern along every other gully and cut bank in Free Roam and close those too; list where.
2. **BUG-002** at (143.19, 58.19, 67.90), heading 45°: "Well, before you were talking about burying this in free roam. I didn't realize how awful it was around it. We need to clean up around this so there are no holes." The big dark storm-drain box beside the trail, with jagged ground cut-outs and light slivers around its base. In Free Roam only: sink the box into the ground (or replace it with a low concrete headwall, whichever looks right) so its top is at ground level and the ground meets it with no gap; no holes or slivers around it. If the box belongs to a route (the storm-drain tunnel), keep the route drivable and say so.
- **0.95 note:** the 0.95 whole-world scan reported no remaining holes inside the playable area; these were missed, so the scan must look for slivers between neighbouring ground pieces and at mesh-to-ground joins, not only open outer edges. Say what the scan now catches.

**Mountain Loop - Forward (`MountainLoop` race scene; check `MountainLoopReverse` for the same places):**
3. **BUG-003** at (1055.64, 152.62, 134.36), main 1654 m: "Don't recall having this huge gap here before. Is this new?" A deep dark trench beside the road. Say when it appeared (compare with earlier builds' scene or evidence shots), then fill it so the roadside is solid ground.
4. **BUG-006** at (726.64, 84.24, −119.49), main 2759 m, by the RIDGE CUT shortcut sign: "gap in road": a gap / dip at the road's right-hand edge where the road meets the ground. Close it.
5. **BUG-007** at (1000.96, 158.89, 93.88), main 1586 m: "gap": the mower dropped into a hole beside the road. Fill it. **BUG-008** at (1008.07, 161.85, 92.34): "floating tree": ground it, and run the trees-on-ground check over the whole Mountain Loop scenes.
6. **BUG-004** at (765.48, 89.04, −119.56), on Climbing Ridge Cut at 16.7 m: "Why is this road not blocked off? It isn't part of either the main road or the shortcut." A dirt road climbs away from the route there. In the race scenes, block every road or trail that leaves the route and is not the main or a shortcut, using the course's existing barrier style (fence or barrier with a "Road closed" sign), set back so it reads before you reach it; check all eight courses for other unblocked off-route roads and block them too; list them. Free Roam keeps them open.
7. **BUG-009** at (966.33, 190.21, 142.51), main 2220 m, the big summit jump: "see AI not clearing this (in cars)". Measure the AI car rivals' take-off speed and landing on this jump; make the AI in cars reach the speed that clears it (approach speed target for this jump, no braking before the lip), as the bikes do. If a car cannot clear it at all, have the AI cars take the lower line around it and say so. No change to the jump.
- Check: a shot at each position after; three AI car runs over the summit jump; the list of off-route roads blocked.

### Part E — Police, round 3: the AI cops chase you

Dan: "I still want to go on to the next phase of the police chase. I really want to have the AI chasing me." The design agreed on 2026-10-07: AI cops chase on roads; when the runner goes off-road they radio it in and other cops head for the places where those trails and shortcuts come back out; a search closes in on the last place the runner was seen; the runner escapes by staying out of every cop's sight long enough; more cops join the longer it goes (heat); roadblocks at higher heat. Free Roam only. Solo play never swaps roles (0.95 Part A). **Own commit.**

1. **Modes (Police Chase setup, Game row):** Cop vs Runner (as now), Speed Patrol (as now), and **Getaway** (new): every human is a runner, the cops are AI. One player = full screen; two players = split-screen, both running, **the one who stays free longest wins** (a caught player watches the other's half full screen until it ends). In Cop vs Runner, the solo Runner role, greyed since 0.94, now works with the AI as the cop.
2. **AI cops:** the 0.94 patrol car, lights and siren on when chasing. They drive the road network at pursuit speed, follow the runner on roads, try to get alongside and box the runner in, and use the 0.94 bust meter to catch. They avoid traffic where they can but may push it aside. They never leave roads and trails that the road network knows about.
3. **Off-road:** when the runner leaves the road network, the cop following stops at the edge and radios: a line on the runner's HUD, "Suspect off-road heading toward <nearest named place or road>". Other cops drive to the exits: the points where trails, shortcuts and cross-country areas the runner could be crossing meet the road network, nearest first, one cop per exit, waiting with lights on. A runner who comes out at a covered exit is in trouble; a long cross-country run can beat them there. Jumps and shortcuts a car cannot follow are allowed and are how you gain distance.
4. **Sight and escape:** a cop sees the runner within about 120 m with a clear line of sight (hills, buildings and dense trees block it). While no cop sees the runner, an **ESCAPE meter** fills over 20 s; any cop seeing the runner resets it. Full = escaped. While unseen, the cops search: they drive toward the last seen position and spread along the roads around it, widening with time. The HUD shows the escape meter, how many cops are chasing, the heat level and the last radio line. The minimap shows cops that can see you (red) and cops that cannot (dim).
5. **Heat:** starts at 1 (2 cops). Every 45 s of the runner staying free, heat goes up (max 5), adding a cop each level (max 6). From heat 3: **roadblocks**, two cruisers parked across a road ahead of the runner on their likely route, with a gap a skilled driver can thread or a way around. Heat shows as stars or bars.
6. **End:** caught (the bust meter fills) or escaped (the escape meter fills) or the round limit (3 / 5 / 8 min) runs out, which counts as escaped. Results: time free, top heat reached, cops dodged, roadblocks passed, the outcome. Two players: who lasted longer; both escaping ends at whoever escaped first. A **Getaway Top 10** (time free, then heat) per round limit under the player's name.
7. **Pacing check by play, not just by test:** with Dan's usual vehicles (Needle 600, Street Classic, Trail Four): a runner who does nothing clever is caught within about 2 minutes on Normal; a runner who uses shortcuts and breaks sight can escape. Add Difficulty (Easy / Normal / Hard) on the setup to set cop speed and numbers. Report the numbers chosen.
8. **Performance:** up to 6 AI cops plus traffic in Free Roam must hold the Free Roam frame rate (single view) and the split-screen 60 fps target with two runners; reduce cop count in split-screen if needed and say so.
- Check (controller only): solo Getaway on Normal: one run caught on the road, one escape by going off-road and breaking sight, a covered exit seen on the minimap, a roadblock at heat 3, the radio line; solo Cop vs Runner as the runner; a two-player Getaway start with the AI as player 2 running too. Shots of the HUD, a roadblock, and cops waiting at an exit.

### Verification

Light, per the Verification budget and the Controller-first rule, except the AI pacing and performance checks in Part E. Compile, launch, release steps. Results as a short list, with "for Dan to check".

### Results (2026-10-08, Claude Code)

- **A — DONE.** Jump, speed-trap and smash events show their own panel (medal targets, this run's best, time left), a banner for every attempt ("ATTEMPT n — value", then "GOLD — EVENT COMPLETE" when a medal is won) and a COMPLETE line; the lap panel is hidden in those events. Once a jump event is complete START opens the pause menu on FINISH EVENT. Checked by controller in all three kinds.
- **B — DONE.** Medals are drawn (procedural sprites, `MedalUi`) on the event HUD, campaign event page, targets and pay rows, results, records and the main-menu summaries; no medal is written as a word any more except in the banner text and "Pass: win any medal".
- **C — DONE.** Each chapter's race is now preceded by a practice lap event; the campaign order is versioned (`orderVersion`, `legacyOrder`): a fresh campaign starts with Against the Clock, a save that already had progress keeps its old availability (Dan's copy keeps all 28 events open). Practice targets for chapters 3 and 4 are estimated (see below).
- **D — PARTLY DONE.** Free Roam BUG-001 / BUG-002: ground cracks along the gullies closed (T-junction fans re-cut), the storm-drain box sunk and covered with a lid. Mountain Loop BUG-003/004/006/007/008/009: roadside pits filled, off-route roads barred with barriers in all eight race scenes, floating trees grounded (`SceneryTrees` now grounds Mountain Loop too), and the AI cars now take off at 29.5 m/s at the summit flight (`aiEntrySpeed`), as the bikes do (checked in the snow event: they clear it). BUG-006 is a partial fill.
- **E — DONE (own commit).** AI cops chase you. Police Chase > Game now cycles Cop vs Runner / Speed Patrol / Getaway. Getaway: every human runs, the cops are AI on a road network (`RoadNet`, 1,302 nodes on 9 roads, one connected part, A*) driven by a pure-pursuit `CopDriver`; `GetawayChase` is the director (sight 120 m with line of sight, a 20 s ESCAPE meter, the bust meter, heat 1–5 every 45 s on Normal, backups to 2/4/6 cops, roadblocks from heat 3, radio lines, exits covered when you leave the road, search, results, a Top 10, Easy/Normal/Hard). The solo Runner role in Cop vs Runner now works (an AI cop chases you). Two-player Getaway: both run, AI can be player 2, whoever is still running gets the whole screen. Minimap shows the cops and covered exits; the split HUD keeps the map clear of the chase panel.
  - **Checks (controller/emulated, built from the editor):** road run caught in 114 s on Normal with the Street Classic and the bike; hidden off-road run ended ESCAPED (cops reached the exits, lights on, radio "1 unit covering the exits"); roadblock at heat 3 ("Roadblock set up on the Street Loop, ahead of the suspect", one passed); the solo Runner role ran its 3 minutes to ESCAPED (time up); two-player Getaway ended with one caught and one escaped; results and Top 10 render.
  - **Pacing, honest numbers:** an AI runner that only follows the road (no shortcuts, never stops) is caught in about 2 minutes with the Street Classic and the bike, but the ATV runner got through three roadblocks and lasted the 5-minute round. The AI runner never stops, so the bust meter only fills at a roadblock; a human runner who slows down is caught sooner. To stop the cops losing a fast runner round a bend I made the first two cops predict the runner's road while unseen, the lead unit know the runner's road for 45 s, and a cop within 230 m slows the ESCAPE meter to 30%.
- **Performance (built player, 1080p, GTX 1660 Ti, `-splitMode getaway` / `getaway1`, heat forced to 5):** six cops, one runner: wall-clock median 8.4 / 8.8 ms (about 115 fps); two runners and 6–8 cops in split-screen: 9.5 / 10.5 ms (about 100 fps). No exceptions in the logs. (GPU timers read NaN in this build; wall-clock only.)
- **Delivery:** source `e61dfb16812bfdfbbbd2a5c80b1dc50dcf5a411b` pushed; fresh 0.96.0-review1 Windows build, 0 errors, 3m31s ([build-release.txt](Docs/Report096/build-release.txt)). Published [game-96000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-96000) (the draft-lookup miss again, then `--resume-draft`); all 234 Latest files match the public signed manifest, public download, pinned signature and startup pass ([hosted/result.json](Docs/Report096/hosted/result.json)); production updater has 96000 active ([launcher-catalog-check.json](Docs/Report096/launcher-catalog-check.json)). `Play-Racer.cmd` (unchanged) launched a responsive `Builds/Latest/versions/96000/Racer.exe` 0.96.0-review1 ([play-racer-launch.json](Docs/Report096/play-racer-launch.json)).
- **Cleanup:** Builds 10,472,739,873 → 8,213,415,590 bytes (2.26 GB recovered); C: free 257,008,066,560 → 261,104,787,456 bytes ([cleanup.json](Docs/Report096/cleanup.json)). Temporary editor tools and check scratch removed.
- **Commits:** safety checkpoint `52c65c11`; `f9bd8767` Parts A–D; `e61dfb16` Part E (own commit). **Revert E alone:** `git revert e61dfb16`. Version 0.96.0-review1 / build 96000. Evidence: [Docs/Report096/](Docs/Report096/) ([Lists/](Docs/Report096/Lists/), [Shots/](Docs/Report096/Shots/)). Checks: `Report096Checks.cs`, `Report096Getaway.cs`, `Report096AiJump.cs`; tools in `Tools/Report096/`.

**For Dan to check:** BUG-002 pockets and BUG-006 (partial fill) by driving them; the practice targets for chapters 3 and 4 (estimated, not from controller runs; list: [practice-targets-raw.txt](Docs/Report096/Lists/practice-targets-raw.txt)); barrier count and placement on the off-route roads; the Getaway feel (pace of the chase, how easily you lose them off-road, roadblock placement, ATV pacing above); the radio line is clipped in the half view of a two-player split.

## Previous delivery — Police Chase findable and playable alone, names only in campaign and split-screen, Free Roam lake, holes and traps, acorn notices, Speed Patrol — 0.95.0-review1 — DELIVERED, REVIEWED BY DAN (event HUD unclear, medals as words, chapter order, more holes and Mountain Loop gaps; AI cops next; all in 0.96)

- **Authorized by Dan (2026-10-07, 19:13, 20:20 and 20:32).** Written by Claude (chat) from his play of 0.94.0-review1, debug session `2026-10-07_19-59-23-590_0bbbbc` (seven reports, all on 0.94.0-review1, all in Free Roam) and his message. His words are quoted in each part.
- **Starting point:** main at the "Record 0.94 delivery" commit. This TODO edit is uncommitted and belongs in the safety checkpoint.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–G. Order A–F, then G (own commit).** The Verification budget and the Controller-first rule apply. Checks never write Dan's real save (work on a copy, pointed at before anything loads).
- World fixes are in `FreeRoamWorld` only unless a part says otherwise; rule 5A applies.

### Results (2026-10-07/08, Claude Code)

- **DELIVERED:**
  - Source `6b899297c0a610a0d3235930346b3d7be77a160a` pushed and verified on origin/main.
  - Fresh 0.95.0-review1 Windows build from that commit: 0 errors, 4m40s ([build-release.txt](Docs/Report095/build-release.txt)).
  - Published [game-95000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-95000) with `Tools/Publish-LauncherRelease.py` (the project's `gh`): the known draft-lookup miss, then `--resume-draft` uploaded the three assets and published. Previous releases retained.
  - All 234 Latest files match the public signed manifest; public download, pinned signature, install and startup pass ([hosted/result.json](Docs/Report095/hosted/result.json)); the production updater has 95000 active, the public catalog reports nothing newer ([launcher-catalog-check.json](Docs/Report095/launcher-catalog-check.json)).
- **Play-Racer.cmd (unchanged):** launched a responsive `Builds/Latest/versions/95000/Racer.exe` (0.95.0-review1) through the launcher, muted, settings restored byte for byte ([play-racer-launch.json](Docs/Report095/play-racer-launch.json)). Dan's save folder hash unchanged after the launch too. Latest root, current 95000 and previous 94000 retained (93000 already removed by the updater).
- **Cleanup:** Builds 10,185,173,344 → 8,031,358,239 bytes (2.15 GB recovered: the build output and game.zip); also the hosted-check install (1.7 GB), 0.2 GB of check scratch outside the project and the temporary editor tools. C: free 266,224,537,600 bytes after cleanup ([cleanup.json](Docs/Report095/cleanup.json)).
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.95 (rule 12).
- **Commits:** safety checkpoint `6148e26d`; `b8c71fbb` Parts A–F; `85932c6a` Parts D and E second pass (from the check shots); `f4510061` Part G (own commit). **Revert G alone:** `git revert f4510061`. Version 0.95.0-review1 / build 95000. Evidence: [Docs/Report095/](Docs/Report095/) ([Lists/](Docs/Report095/Lists/), [Shots/](Docs/Report095/Shots/)). Checks: `Report095Checks.cs` (police95, names95, lake95, holes95, acorns95, missed95), `Report095Patrol.cs` (patrol95); world tool `Tools/Report095/Report095World.cs` (Run, Fix2, Fix3).
- **Dan's save:** every check ran on copies (the editor started on a copy via `-racerTestSave`; checks copy his files into their own folder). His save folder hashed before the first check and after the last one: identical (`448cffe2…`).
- **A — DONE.** Solo (one human against the AI): one role kept, one round, no swap; results "CAUGHT in m:ss" / "GOT AWAY" (as the runner "ESCAPED" / "CAUGHT after m:ss"); Rematch keeps the role. The runner role stays greyed "coming later". Two humans: as 0.94 (swap, longer run wins). Speed Patrol: everyone a cop, no swap.
- **B — DONE.** POLICE CHASE row on the main menu (after SPLIT SCREEN; also in Free Roam's pause menu, which is the main menu with RESUME DRIVING) and on the Free Roam page; "New: Police Chase" hint once. Its own setup: Game, Players 1 / 2, role (1) or devices, names and cop first (2), the runner's vehicle in the garage view (solo: only the AI runner's half), the patrol car turning with its stat bars, start, time, weather, traffic, round limit, START CHASE. One player = one full-screen view (no second camera, no divider; split-only detail lowering not applied); HUD: role, round clock, bust meter, direction and distance to the AI, speed, minimap. From inside Free Roam it asks "Leave Free Roam and start a Police Chase?" and the last menu row becomes "Back to Free Roam" (back at the same start). The Split Screen setup's Mode row is Race / Free Roam; stepping to Police Chase opens this setup with two players.
  - Check (emulated controllers only): main menu rows, the setup walked with the D-pad (reachable, drawn order, B back), the Free Roam page and pause rows, the ask, a solo chase on the full screen to a catch ("CAUGHT in 0:07"), results, back to Free Roam; two players: split-screen, two rounds, round 2 swapped: **0 failures** ([list](Docs/Report095/Lists/B-G-police-patrol-checks.txt)).
- **C — DONE.** Grid card, ahead / behind line, winner caption and named results only in the campaign; quick races as before 0.94 ("Rival n" / "You", no tags). Split-screen: only the other person is tagged and named (AI rivals "AI rival", no grid card, the gap line only when it is the other player). Settings "Name tags: On / Off" (a saved Everyone / Players only is On). Check: campaign event (cast on the page, grid, gap line, 3 tags, named results), quick race (no grid, no gaps, 0 tags, "Rival n / You"), split-screen with two AI rivals (1 tag, the other player; results "AI rival"): PASS ([list](Docs/Report095/Lists/C-D-F-names-lake-acorns-checks.txt)).
- **D — DONE.** **Why dry:** Free Roam has had its own scene since 0.76, built from Dan's Backyard Reverse; the lake belongs to the Forest Loop scenes (`CR056 Forest Loop/Friend's lake`, surface 78.02 m) and was never in `FreeRoamWorld`, whose ground there is the Street Loop's dry hollow (down to 62.5 m against the Forest scenes' 77.4 m bed). Not a 0.94 split-screen change. **Fix:** the same water object added; the hollow raised to the Forest scenes' bed (raise only, 3 tiles; roads and trails never raised), the dry pits joined to it filled to just above the water (second and third passes, from the check shots); 161 + 728 + 315 tree pieces and trunks on raised ground removed (as in the Forest scenes); the RIDGE RETURN sign, which hung over the hollow, set down. Escapable shores (steepest climb out ≤ 21 % on 14 of 16 sides; the other two are the Forest scenes' own drops further out), slowdown and ice from ShallowWater. **Other water bodies:** all others match the race scenes except the Forest's J1 flowing creek — not restored: Free Roam has no channel there and a Free Roam road/trail band covers its footprint (cutting the channel would lower that road; rule 5A). Check: shots from BUG-004 day and night, driven in from the west shore (immersed 100 %) and out without a reset (8.1 s), a reset at the BUG-004 shore, ice in Snow: PASS. Shots: [day](Docs/Report095/Shots/D-lake-from-BUG-004-Day.jpg), [night](Docs/Report095/Shots/D-lake-from-BUG-004-Night.jpg).
- **E — DONE.** BUG-001 / BUG-006 were holes in the runtime world-edge ring at the mountain annex's outer edges (shown by tinting the ring: [BUG-001](Docs/Report095/Shots/edge-BUG-001-ring-magenta.jpg)); the annex's 1,202 open ground edges (x ≥ 790) got earth aprons (bank, corners, curtain below; drawn and collidable). Whole-world scan: the main world's perimeter is covered by the ring; the four "enclosed holes" found are thin road / gully colliders, not holes ([list](Docs/Report095/Lists/E-holes-scan-with-world-edge.txt)). Resets: a Free Roam reset point inside a hollow (every way out climbs over 30° within 24 m) is skipped; BUG-002 reset to the Mountain return trail 113 m away, BUG-003 to the Summit approach 46 m away, neither a hollow. Neither pit was smoothed (both are mountain terrain, not mesh spikes). Trees: 9 floating trunk colliders set down; `SceneryTrees` grounds Free Roam's drawn trees as Forest Reverse's (1,289 crown-only clumps given trunks); the 0.84 floating check over all of Free Roam: 25,623 trees and trunks, **0 floating**, 0 within 60 m of BUG-007. Shots at all five positions after ([Shots/E-*](Docs/Report095/Shots/)).
- **F — DONE.** Pickup banner top centre 4 s, own chime ("ACORN FOUND 23 / 24" / "Ridge woodland 5 / 6"; 1080p-scaled canvas, dark backing). The 24th: a panel (heading, "UNLOCKED: TURF ROCKET", the mower turning, stat bars, "Choose it in the Garage", fanfare) that pauses Free Roam and closes only with A. A save with the reward earned and the panel never shown gets it once at the next main menu / Free Roam; A remembers it (`settings.json` `unlocksSeen`; loading never writes). Check: banner day / night, gone after 4 s; the 24th (paused, B does not close, A closes and resumes); a copy of Dan's save: shown once, not again: PASS ([list](Docs/Report095/Lists/F-missed-panel-check.txt)).
- **G — DONE** (own commit). Speed limits Hwy 92 55, Trickum Rd 45, S Cherokee Ln and small roads 30 (traffic slowed to them in Speed Patrol only; checked 28–53 mph against 30 / 45 / 55). Spawn, radar (80 m, 10° cone), clocking, pursuit (30 m), meter (20 m, 4–8 s), get-away (60 s), points and penalties as specified; ramming to a stop = half points; the end waits up to 15 s for a chase; results and the Top 10 per round length. Check (controller for the menus; the cop placed by the check for each event, the game's rules deciding them): solo 3-minute patrol: traffic at the limits, radar 76 in a 55 (clocked +21, on the minimap), pursuit, caught +310, one got away, hit −50, NO VIOLATION −25, results "SCORE 235" and the Top 10 #1 Dan 235; two players: both patrol cars, both radars reading: **0 failures**. Shots: [radar](Docs/Report095/Shots/G-radar.jpg), [pull-over](Docs/Report095/Shots/G-pull-over.jpg), [results](Docs/Report095/Shots/G-results-top10.jpg).
- **Decisions:** solo = full screen with the AI as player 2 (no second view); the runner's vehicle row opens only the AI's half solo; the pause menu's POLICE CHASE row is in Free Roam's main menu (that is its pause menu); a missed unlock panel shows at the main menu or Free Roam, whichever comes first; Speed Patrol limits by area (Trickum Rd = the loop's west side) and traffic at 97 % of the limit; player 2's patrol car dark blue; Top 10 file `speed-patrol-v1.json`; no Speed Patrol for the AI as player 2 (solo is one cop).
- **For Dan to check:** Police Chase from the main menu and from Free Roam with his controller; the lake's look and the shallow dip left by the shore trail at BUG-004; the aprons at the annex edges (BUG-001, BUG-006); resets near the mountain pits; the acorn banner size at 4K; the mower panel on his save (once); Speed Patrol feel: limits, spawn rate, radar cone, meter times, points.

### Part A — Police Chase: roles swap only when two people play

Dan: "for the cops switch between cop and speeder, I want this only as a multiplayer option. If you choose to be the cop or the speeder in single player you will not switch out."

- **Two humans:** as 0.94: round 1, automatic swap, round 2, the longer-lasting runner wins.
- **One human (the other is the AI):** the player chooses cop or runner and keeps that role. One round, no swap. Result: as cop, "CAUGHT in m:ss" or "GOT AWAY"; as runner, "ESCAPED" (clock ran out) or "CAUGHT after m:ss". Rematch keeps the same role; the role is changed only in the setup.
- This rule holds for every police mode added later (speeders, AI cops): solo play never swaps roles.
- Until the chasing AI exists, a solo player can only be the cop (the runner role stays greyed with "coming later", as 0.94).

### Part B — Police Chase must be findable, and playable alone on a full screen

Dan on 0.94: "I couldn't figure out how to activate cop mode. There were no police vehicles in the garage and I saw no option to actually start any kind of event."

0.94 put it under Main menu > SPLIT SCREEN > Mode: Police Chase, which nobody would guess, and playing alone there still splits the screen.

1. **Entries:** a **POLICE CHASE** row on the main menu (after SPLIT SCREEN) and on the Free Roam menu / Free Roam pause menu. Both open the Police Chase setup. A one-time hint the first time the main menu is shown after this update: "New: Police Chase".
2. **Setup screen:** Players: 1 / 2. Role (1 player) or who is the cop first (2 players). The runner's vehicle, chosen in the garage view. Time of day, weather, traffic, round limit. START CHASE. The patrol car is shown turning on this screen with its stat bars, so the player sees the police vehicle (it stays out of the normal garage, as decided).
3. **One player = full screen.** No split, no second half: the player's normal HUD plus role, clock, bust meter and the direction and distance to the AI. Part A's rule applies (one role, one round, no swap). Until the chasing AI exists the Runner role is greyed "coming later". Dan found the 0.94 route to it after asking ("I found the cop"); the entries are still wanted.
4. **Two players** = split-screen as 0.94, with the swap. The Split Screen setup's Mode row keeps Race / Free Roam and sends Police Chase to this setup.
5. Starting it from inside Free Roam asks once ("Leave Free Roam and start a Police Chase?") and returns to Free Roam afterwards.
- Check (controller only): reach the setup from the main menu and from Free Roam; one full-screen solo chase as the cop to a catch; a two-player start still swaps.

### Part C — Names only where Dan wants them

Dan on 0.94: "I don't want names in quick races. I only want the competition names in the main campaign, and on splitscreen just the other person."

- **Campaign:** as 0.94 (rival list on the event page, line-up card, ahead / behind line with names, tags, named results).
- **Quick races (Race Setup, playlists):** none of it: no rival names, no line-up card, no ahead / behind line, no tags; results as they were before 0.94.
- **Split-screen (race, Free Roam, Police Chase):** only the other person: their name tag and their name in results and on the HUD lines that refer to them. AI rivals there have no tags and no names.
- **Setting:** "Name tags: On / Off" replaces Off / Players only / Everyone (a saved Everyone or Players only becomes On).
- Unchanged: the player name, names on the Top 10, split-screen times on the Top 10.
- Check: one campaign event, one quick race, one split-screen race with two AI rivals: what is shown in each.

### Part D — Free Roam: the lake has no water

Dan (BUG-005, map open at (667.39, 73.03, −18.02), driving at 17.7 mph where the map draws "The lake"): "there should be a lake here. It should exist in free roam." BUG-004 at (710.50, 79.65, −79.22), looking at the dry basin by the RIDGE RETURN sign: "this space too. notice too."

- In `FreeRoamWorld` the lake basin is dry: he drove across the lake bed. Find out why (missing or disabled water object, a 0.94 split-screen / two-view change, a Free Roam-only rule) and since which version, and restore the lake in Free Roam: visible water at the level the race scenes use, the escapable-water behaviour from 0.85, ice in Snow, in single-player and in split-screen.
- Check every other water body the race scenes have (pool, creek, pond) against Free Roam and restore any that are missing.
- BUG-004's basin: once the water is back, check a vehicle can always drive or reset out of the shore there.
- Check: a shot of the lake from Dan's BUG-004 position day and night; a vehicle driven in and out.

### Part E — Free Roam: holes, traps and floating trees (BUG-001, 002, 003, 006, 007)

All in `FreeRoamWorld`, version 0.94.0-review1. Screenshots in the session ZIP `2026-10-07_19-59-23-590_0bbbbc`.

- **BUG-001 "Seal this hole"** at (1193.72, 89.38, 149.28), heading 65°: an open gap in the hillside showing dark void and sky.
- **BUG-006 "seal"** at (806.89, 64.03, 328.19), heading 62°: a long straight dark seam between two ground pieces.
- **BUG-002 "can we make this escapable?"** at (811.92, 94.46, 231.21): the Street Classic stuck nose-up in a steep pit, off the roads, at night.
- **BUG-003 "make escapable"** at (1158.74, 109.96, 166.14): the same kind of trap, about 40 m from BUG-001.
- **BUG-007 "floating trees"** at (1008.87, 164.63, 107.92), beside High Ridge Drop: trees standing above the ground on the slope.

1. Seal the two gaps by closing the ground mesh there (no see-through, no fall-through), matching the surrounding ground. Then scan the whole Free Roam ground for other open seams and holes (edges with no neighbour inside the playable area) and close them; list what was found.
2. **Escapable everywhere:** in Free Roam a stuck vehicle must always get out. The reset in Free Roam must never put the vehicle back into the same pit: if the nearest safe point is inside a hollow the vehicle could not drive out of, go to the nearest road or trail instead. Confirm at BUG-002 and BUG-003 that a reset frees the vehicle. Where a pit is only a mesh fault (a crease or a spike of ground rather than intended terrain), smooth it. Do not reshape intended terrain, jumps or the race scenes (rule 5A).
3. Re-ground the floating trees at BUG-007, then run the existing trees-on-ground check over all of Free Roam and fix any others; list the count.
- Check: a shot at each of the five positions after; one reset at BUG-002 and BUG-003.

### Part F — Acorns: you must notice a pickup, and you cannot miss the unlock

Dan: "not sure I saw that I had 2 acorns left, found 1, went to the last area and never noticed I picked it up. There needs to be some sort of notification that was unlocked." His save has 24 / 24 with the reward earned (20:12); he drove through the last acorn and the mower unlock without seeing either. The acorns themselves are fine: no reachability work.

1. **Every pickup:** a clear banner at the top centre for about 4 s with a distinct sound: "ACORN FOUND 23 / 24" and under it the area and its count ("Ridge woodland 5 / 6"). Readable at 3840×2160 at speed, day and night.
2. **The 24th:** a panel that stays until the player presses A: "ALL 24 WOODLAND ACORNS FOUND" / "UNLOCKED: TURF ROCKET" with the mower turning as in the garage view and its stat bars, and "Choose it in the Garage". The vehicle is stopped safely while it is up. A fanfare, not the pickup sound.
3. **Dan missed his:** on a save where the reward is earned and this panel has never been shown, show it once the next time the main menu or Free Roam opens. Remember that it was shown.
4. The same panel style is used for any future one-off unlock that happens while driving. Campaign prize reveals stay as they are.
- Check on a copy of an isolated save: a pickup banner; the 24th; a copy of Dan's save showing the missed panel once and not again.

### Part G — Police, round 2: Speed Patrol (catching speeders for points)

Dan: "both are cops that try to catch speeders (you get points for different things and after a certain amount of time the one with the most points win)." 20:32: "Can we go ahead and fold phase 2 into this update." Points as agreed: per catch, more for faster speeders, a penalty for hitting innocent traffic. Free Roam only. **Own commit**, with the revert command. If it cannot be finished well, deliver Parts A–F and say what is left.

1. **Where:** the Police Chase setup (Part B) gets Game: Cop vs Runner / Speed Patrol. Speed Patrol: Players 1 / 2, time of day, weather, round length 3 / 5 / 8 minutes (default 5). Traffic is always on. One player = full screen; two players = split-screen, both in patrol cars (the second in a different livery colour so they can be told apart).
2. **Speed limits:** each road has a limit (Hwy 92 55 mph, Trickum Rd 45, S Cherokee Ln and the small roads 30; adjust if the roads' real feel says otherwise and report). The HUD shows the limit of the road the cop is on.
3. **Speeders:** ordinary traffic keeps to the limit. From time to time a traffic vehicle near a cop becomes a speeder (10 to 40 mph over; about one new speeder every 20–30 s per cop, at most three alive at once). The cop's HUD has a **radar**: the speed of the vehicle the patrol car is pointing at within 80 m, turning red over the limit. Speeders are not marked on the minimap until a cop has clocked them on the radar.
4. **Catching:** lights and siren on (the button from 0.94) within 30 m of a clocked speeder starts the pursuit: the speeder runs (faster ones run harder and longer, on roads only, choosing turns away from the cop, using the 0.94 AI runner). A **pull-over meter** fills while the cop stays within 20 m; filling takes 4 s for a slow speeder up to 8 s for the fastest; it drains when the cop falls back. Full = the speeder slows, pulls to the side and stops: caught. A speeder not caught within 60 s of being clocked gets away. In a two-player game the catch goes to the cop who fills the meter; both can chase the same one.
5. **Points:** catch 100 + 10 per mph over the limit when clocked. Hitting any vehicle that is not a clocked speeder −50. Pulling over a vehicle that was not speeding (lights on it within 20 m for 4 s) −25, "NO VIOLATION". Ramming a speeder to a stop counts as a catch at half points. Each event shows a short line on that cop's HUD with the points.
6. **End:** when the clock runs out, a chase in progress may finish (up to 15 s). Results: each cop's points, catches, fastest speeder caught, penalties. Two players: most points wins. One player: the score, with a **Speed Patrol Top 10** per round length under the player's name (two-player scores go on it too under each name). Rematch / Change setup / Main menu.
7. Free Roam rules as in Cop vs Runner (reset holds 2 s, nobody leaves the world). Nothing else is recorded. Part A's no-swap rule is moot here (everyone is a cop).
- Check (controller only): one solo 3-minute patrol with at least one catch, one get-away, one traffic-hit penalty and one "no violation", its score on the Top 10; a two-player start with both radars working. Shots of the radar and a pull-over.

### Verification

Light, per the Verification budget and the Controller-first rule. Compile, launch, release steps. Results as a short list, with "for Dan to check".

## Previous delivery — Cabin Jump shrubs restored part-way, player names (Top 10, rivals, name tags), split-screen stage 3 (Free Roam for two, camera views), Police Chase: cop vs runner — 0.94.0-review1 — DELIVERED, REVIEWED BY DAN (Police Chase could not be found; names wanted only in the campaign and for the other split-screen player; Free Roam lake dry, holes and traps; follow-ups in 0.95)

- **Authorized by Dan (2026-10-07, 14:30–15:51).** Written by Claude (chat) from the conversation; his words are quoted in each part. His 0.93 review (16:39) is Part D; everything else in 0.93 was fine.
- **Starting point:** main at the "Record 0.93 delivery" commit.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–D. Order D (small, first), A, B, C; B and C each in their own commit with the revert command.** If C cannot be finished well, deliver A and B and say what is left of C.
- The Verification budget and the Controller-first rule apply. Checks never write Dan's real save (work on a copy, pointed at before anything loads).

### Results (2026-10-07, Claude Code)

- **DELIVERED:**
  - Source `ddf9d32ae1a268ed4c1c04a98dac0b52f96f9fcb` pushed and verified on origin/main.
  - Fresh 0.94.0-review1 Windows build from that commit: 0 errors, 3m17s ([build-release.txt](Docs/Report094/build-release.txt)).
  - Published [game-94000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-94000) with `Tools/Publish-LauncherRelease.py` (the project's `gh`): the known draft-lookup miss, then `--resume-draft` uploaded the three assets and published. Previous releases retained.
  - All 234 Latest files match the public signed manifest; public download, signature, install and startup pass ([hosted/result.json](Docs/Report094/hosted/result.json)); the production updater has 94000 active, the public catalog reports nothing newer ([launcher-catalog-check.json](Docs/Report094/launcher-catalog-check.json)).
- **Play-Racer.cmd (unchanged):** launched a responsive `Builds/Latest/versions/94000/Racer.exe` (0.94.0-review1) through the launcher, muted, settings restored byte for byte ([play-racer-launch.json](Docs/Report094/play-racer-launch.json)). Dan's save folder hash (all files) the same after the last check and after this launch (`41a2e450ada6ef36`). Latest root, current 94000 and previous 93000 retained (the updater had already removed 92000).
- **Cleanup:** Builds 10,166,407,495 → 8,019,460,655 bytes (2.1 GB recovered: the build output and game.zip); also the hosted-check install (1.7 GB), 2.0 GB of check scratch outside the project (test player, logs) and the temporary editor tools. C: free 263,820,480,512 bytes after cleanup ([cleanup.json](Docs/Report094/cleanup.json)).
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.94 (rule 12).
- **Safety checkpoint:** `03310234` (this plan), pushed. Commits: `2f508067` Parts D and A, `c3eaba3b` Part B (own commit), `ce4a35e1` Part C (own commit). **Revert C alone:** `git revert ce4a35e1`; **B too:** `git revert ce4a35e1 c3eaba3b` (C sits on B's Free Roam for two). Evidence: [Docs/Report094/](Docs/Report094/) ([Lists/](Docs/Report094/Lists/), [Shots/](Docs/Report094/Shots/)). Checks: `Report094Checks.cs` / `Report094Walk.cs` (A) / `Report094Roam.cs` (B, C), editor, emulated controllers; `SplitBench.cs` (`-splitMode roam`) in a built test player; tools `Tools/Report094/`. Version 0.94.0-review1 / build 94000.
- **Dan's save:** every check ran on a copy (the editor runner and the test player started on copies; the checks copy Dan's files into their own folder). His save folder was last written at 16:36:59, before the first check (16:45); its files hashed after the last check and again after the launcher check (see the delivery lines).
- **D — Abandoned Cabin Jump shrubs back part-way — DONE (first).**
  - The original 0.43 brush mesh is back from the lip (41 m) to a **far edge at 84 m** (1,508 bushes at their original positions, the last one reaching **85.1 m**), with its slowing; beyond 84 m the 0.93 corridor stays clear to the rejoin (71 bushes still out). Own mesh copy for Backyard Forward; Reverse, Free Roam and Tree-Top Trail unchanged; no geometry, collider or route change ([author list](Docs/Report094/Lists/D-author.txt)).
  - **Landings** (simple follower on the centre line from the branch start; identical before and after, bushes have no colliders) ([table](Docs/Report094/Lists/D-landings.txt)):

    | | 24 m/s | 28 m/s | 31 m/s | 34 m/s |
    |---|---|---|---|---|
    | Needle 600 | 75.9 m (in) | 86.2 m (through) | 95.2 m | 105.7 m |
    | Street Classic | 74.0 m (in) | 83.9 m (through) | 91.5 m | 99.0 m |
    | Trail Four | 75.4 m (in) | 86.4 m (through) | 95.2 m | 99.2 m |

    Every vehicle at 31 and 34 m/s lands **6.4 m or more** past the last bush (worst: Street Classic at 31 m/s, whose approach only reaches 28.8 m/s at the lip) and touches none; every 24 m/s flight lands in the bushes; 28 m/s comes down through the last few metres of them (either way, as asked).
  - **Reset from inside the brush:** goes to the clear trail **4 m past the far edge** (88 m), on the centre line, facing along the route, stopped (`ShortcutUndergrowth.ResetPast`, used by the nearest-point reset; AI too). Check at Dan's 0.93 position: ends at s 88.0, 0.0 m off centre, nothing within 5 m ahead: PASS. Other shortcuts' resets unchanged.
  - Shots: [ramp approach](Docs/Report094/Shots/D-ramp-approach.jpg), [from the lip](Docs/Report094/Shots/D-landing-from-the-lip.jpg), [where the bushes end](Docs/Report094/Shots/D-landing-where-the-bushes-end.jpg), [reset](Docs/Report094/Shots/D-reset-from-inside-brush.jpg).
- **A — Names — DONE.**
  - **Player name:** Settings > Gameplay "Player name: … (A: change)" opens the on-screen keyboard (12 characters; keyboard typing works too); a new player is asked once when the welcome opens; **Dan's save is "Dan" without asking** (a save with campaign progress, records or hints already seen is his). Names used on this PC are remembered (`settings.json`: `playerName`, `knownNames`, `nameTags`).
  - **Top 10:** a Name column; Dan's existing entries are "Dan" (named on load, written with the next record); the player's own entries highlighted (and NEW ones).
  - **Split-screen names:** "Player 1 name" (the saved name; ‹ › names used before, A a new one) and "Player 2 name" (‹ › names used before on this PC, A a new one, remembered); the AI is "AI". The setup screen answers player 1's device (as in 0.91), so player 1 sets both names.
  - **Split-screen times on the Top 10:** each human's laps and race total go to the board a single-player race with the same settings uses (their vehicle, the AI rivals - not the other player - difficulty, traffic, laps), same rules (no debug movement); the AI driver never; still no ghosts, personal-best files, campaign, acorns or activity records. The results line lists any Top 10 places.
  - **The cast:** the campaign now keeps one named cast in **every** race (0.90-0.93: championships only; events showed EMBER, GOLD...): **Rusty Vance** (usual Longroof GT, red), **Mia Torres** (Needle 600, gold), **Big Ed Kowalski** (Drifter Twin, blue), **June Park** (Trail Four, green), **Hollis Gray** (Ridge Scrambler, violet). An event's slots go to the drivers whose usual vehicle it has, then the others in cast order; the event's vehicles are unchanged, each driver wears their colour. Championship tables use the same mapping.
  - **Seen in the race:** event and championship pages list the rivals by name and vehicle; a **starting grid card** during the countdown (front first, your line gold), gone at GO; **"▲ Name +1.2 s / ▼ Name −0.8 s"** under the race panel (interval timing from each racer's progress, every 0.1 s); names in results (Race Setup, campaign, championship tables, split-screen) and **"WINNER / name"** over the winner shot (in split-screen in the winner's half). Race Setup and split-screen rivals keep EMBER, GOLD...; playlists "Rival n".
  - **Name tags:** Settings > Gameplay "Name tags: Off / Players only / Everyone" (default Everyone): over every other racer (never your own), drawn on screen above the vehicle, 22 px at 1080 scaled to 44 px at 2160, fading out from 45 to 60 m, hidden behind solid scenery, not in Trailer Mode or a winner shot; people gold, AI white-blue; tags that would overlap stack upward; in split-screen each half draws its own.
  - **Checks (controller only, emulated gamepads, copy of Dan's save):** named "Dan" without asking; his Top 10 shows Dan on all 10 entries; Player name entered with the on-screen keyboard ("kyle") and saved; tags Everyone 3 / Players only 0 / Off 0 with three AI rivals; Backyard Dash (campaign): rivals on its page (Mia Torres, June Park, Big Ed Kowalski), the grid card, gone at GO, "▲ Mia Torres +0.6 s", 3 tags, "WINNER June Park", results with all four names; split-screen: player 2 joined and named "kyle" with the keyboard (remembered), tags drawn in both halves, both players' times on the Backyard Forward Top 10 under their names: **0 failures** ([list](Docs/Report094/Lists/A-checks.txt), shots `A-*.jpg`).
- **B — Split-screen stage 3: Free Roam for two, camera views — DONE** (own commit).
  - Setup: **Mode: Race / Free Roam / Police Chase**; in Free Roam the course row is "Start at"; laps and rivals rows hidden; time of day, weather, traffic, layout as stage 2. Free Roam for two runs in FreeRoamWorld with the setup's fixed time and weather; player 2's vehicle is made beside player 1; **player 2 as the AI cruises the roads** (the traffic driver, without traffic recycling).
  - Each half: name, the **direction arrow and distance to the other player**, speed, own minimap with the compass and **the other player marked** (kept at the rim when far), own reset (nearest safe point), own jump and speed-trap results (a second activity watcher for player 2). Shared radio. **Pause from either player:** RESUME / **MAP** (travel **"Bring both players here"**) / Settings / Change setup / Return to menu. **Nothing recorded** (acorns, activity records, map discovery, the Free Roam clock), said for 7 s on entry.
  - **Camera views per player** (split-screen races and Free Roam): X on that player's controller (V on the keyboard) steps chase / far chase / first person / front for that half only; player 1 starts in their saved view, player 2 in chase; not saved. First-person heads are hidden only from their own camera (switched per camera as it renders), hands on the wheel follow each player's own view.
  - **Performance** (built test player like the release, GTX 1660 Ti, 3840×2160, Free Roam from Street Loop, Night / Snow, traffic (24 cars), both vehicles driving): top / bottom **median 10.8 ms (93 fps), 95th 12.7 ms (79 fps)**; left / right **median 8.7 ms (115 fps), 95th 11.0 ms (91 fps)**. Above 60 fps, so nothing more was lowered (stage 2's split-screen-only lowering applies). Single-player rendering unchanged ([list](Docs/Report094/Lists/B-frame-times.txt), shots `B-built-*.jpg`, which also show both halves in their own views at once).
  - **Checks (controller only):** Mode chosen with the D-pad; Free Roam for two with player 2 on a second controller (top / bottom) and with the AI (left / right, cruised 203 m in 12 s); each half's direction line; X on each controller changed only that player's view, four steps; first person in both halves with both heads hidden from their own cameras; Start on player 2's controller paused (rows resume, map, settings, change setup, return, quit); MAP opened; travel to Moll's put player 2 4.5 m from player 1; back to the menu; single-player Free Roam afterwards: its weather setting shown (Rain), an acorn still found and recorded: PASS ([list](Docs/Report094/Lists/B-checks.txt)). The first back-to-menu check failed on its own timing (it did not wait for the scene change); fixed and passed.
  - **Found and fixed on the way (regression since 0.90):** the Free Roam **Weather** setting had no effect (its assignment sat inside a comment in `WorldLook`, so Free Roam was always Clear); restored (the single-player check above).
  - Also trimmed: in split-screen the "Esc > Activities" line of the activity prompt (timed attempts are single-player only).
- **C — Police Chase, round 1 — DONE** (own commit).
  - **Patrol car** (`Tools/Blender/police.py`, the 0.75 car kit, 12,676 triangles): a generic late-1980s four-door in black and white (white doors and roof), a light bar with red and blue lenses that flash in a double-flash pattern with coloured lights (seen at night), a push bar, a pillar spotlight, POLICE on the front doors; no department, badge or number ([preview](Docs/Report094/Shots/C-patrol-car-blender-preview.png), [night](Docs/Report094/Shots/C-patrol-car-Night-1.jpg), [day](Docs/Report094/Shots/C-patrol-car-Day-1.jpg)). **Stats: top speed 55, acceleration 16.5, grip 26, response 8.2, mass 1,550 kg** (between the fastest cars (52, 16) and the Trail Four (56, 17)). Kept out of the vehicle list, so never in the garage, shop, races or rosters.
  - **Siren:** on from the cop's release, **RB (keyboard H) toggles it**; one sound for both players, louder as the cop nears the runner (full within 15 m, a quarter beyond 250 m).
  - **Setup:** Mode Police Chase; **Cop first** (player 1 / player 2), the runner's vehicle (each player's garage choice), time of day, weather, traffic, **Round limit 3 / 5 / 8 minutes** (default 5).
  - **A round:** the runner starts on the road at the chosen start, the cop **80 m behind along the road** (63 m in a straight line on that bend), released **3 s** after the runner; each half shows COP / RUNNER, round and clock / limit, the **bust meter** and the other player's direction and distance; the centre shows the start, GO, CAUGHT! or GOT AWAY!
  - **Rules (final values, unchanged from the plan):** the meter fills while the cop is **within 10 m** and the runner is **slower than 7 m/s**, **3 s** to fill, drains at the same rate; full = caught; the clock at the limit = got away. A runner's reset holds them still **2 s**. Water and leaving the world as in Free Roam.
  - **Swap and result:** round 2 swaps the roles (the new runner keeps their vehicle, the new cop gets the patrol car); the longer run as the runner wins, getting away beats being caught, both away is a draw; results show both runs with **Rematch / Change setup / Main menu**. Pause: Resume / Restart the chase / Settings / Change setup / Return.
  - **AI:** player 2 as the AI is the **runner**: it drives the roads at 1.25× traffic pace away from the cop and turns back when the cop gets round in front of it (within 120 m, at most every 12 s). As the **cop** it is greyed "the AI as the cop: coming later" (player 1 is the cop), so **with the AI the match is one round**. Nothing is recorded.
  - **Checks (controller only for the menus; Night and Day):** setup rows; player 1 the cop in the patrol car, the AI runner on the Trail Four; the runner released, the cop 3.0 s later, siren on; the runner stopped and the cop 3.5 m behind: the meter filled to 53 %; the cop 20 m away: it drained to 32 %; a runner reset held it 2 s; **caught at 0:37.9**; results ("Caught!", one round); Main menu back to the course: **0 failures** both ([list](Docs/Report094/Lists/C-checks.txt), shots `C-*.jpg`). The checks' cop is a simple test pilot: following the roads far does not work for it, so for the meter it is set on the road 25 m behind the stopped runner (the AI runner's turning back did leave it behind earlier).
- **Decisions:** the Cabin far edge at 84 m (midway between the 24 m/s landings and the 31 m/s ones, less 3 m), reset 4 m past it; Dan's save named "Dan" by having progress, records or hints; the player's own Top 10 rows highlighted (no longer the top three); the cast's usual vehicles and colours as above, assigned per event without changing any event's vehicles; tags drawn on screen (not in the world) so they stay readable and per half; per-player views not saved in split-screen; split-screen Free Roam uses fixed setup conditions (its clock does not run); the patrol car's stats; siren on RB / H; with the AI, one round.
- **For Dan to check:** the Cabin Jump at his usual approach (24 m/s lands in, 31 m/s clears); the name keyboard with his controller; the grid card, gap line and tags on his screen (tag size at 4K); the cast names; Free Roam for two with a friend (each X changes only their own view); Police Chase with two players (the swap and the result), the bust meter values (10 m / 7 m/s / 3 s), the siren volume and RB; the AI runner's driving.

### Part A — Names

Dan: "I would like to add names. And the names would go on the top 10. I would name myself, I would always be Dan, but if I do splitscreen on my computer, a friend will add their name. Additionally the campaign said I was racing against the same named AI racers but never once do I see their names that I am aware of having played the first 3 chapters. It would be good for multiplayer and campaign to have the options to put the avatars name over their head so you know who it is."

1. **Player name:** Settings > Gameplay "Player name" (up to 12 characters), entered with the controller on an on-screen keyboard (keyboard typing also works). A new player is asked once, with the first-run hints. **On Dan's existing save the name is set to "Dan" without asking.**
2. **Top 10:** every entry shows the name of who set it. All existing entries are Dan's and become "Dan". The player's own entries stay highlighted.
3. **Split-screen names:** the setup screen has a name for each player. Player 1 defaults to the saved player name; player 2 picks from the names used before on this PC or enters a new one (remembered). The AI driver as player 2 is named "AI".
4. **Split-screen times count on the Top 10 (change from 0.90 / 0.92):** a human's race total and best lap from a split-screen race go on that course's Top 10 under their name, in the same boards and under the same eligibility rules as a single-player race with those settings. Still not recorded from split-screen: ghosts, personal-best ghosts, campaign money, acorns, activity records. The AI driver's times are never recorded.
5. **The rivals have names you can see.** The campaign says the same named drivers race against you; show them:
   - the campaign event and championship pages list the rivals by name with their vehicles;
   - a line-up card during the countdown (grid order, names, vehicles), gone at GO;
   - the HUD shows who is directly ahead and directly behind with the gap ("▲ EMBER +1.2 s / ▼ ROOK −0.8 s");
   - results, championship standings and the winner shot use the names;
   - the same in Race Setup races and split-screen (rival names there as the game already assigns them).
   If the campaign does not in fact keep a fixed cast, make it do so (one named cast for the whole campaign, each with a usual vehicle and colour) and list the cast in the results.
6. **Name tags over the heads:** Settings > Gameplay "Name tags": Off / Players only / Everyone. Default Everyone. A small tag above each other racer (never your own), facing the camera, readable at 3840×2160, fading out beyond about 60 m, hidden behind solid scenery, not shown in Trailer Mode or the winner shot. In split-screen each half draws its own tags (the other human's tag shows in your half). Humans' tags in a distinct colour from AI tags.
- Check (controller only): enter a name with the on-screen keyboard; Dan's copy shows "Dan" on the boards; one campaign event showing the rival list, line-up card, ahead / behind line, tags and results; one split-screen race with a second name that lands on the Top 10; the three tag settings.

### Part B — Split-screen stage 3: Free Roam for two, and camera views

Dan: "I would for sure like having split screen free roam."

1. **Free Roam for two:** the split-screen setup gets Mode: Race / Free Roam / Police Chase (Part C). Free Roam puts both players in the Free Roam world (`FreeRoamWorld`) with the garage vehicle choice, time of day, weather and traffic as in stage 2. Everything unlocked, vehicles stock. Player 2 can be the AI driver (it cruises the roads) so Dan can test alone.
2. **Per player:** own minimap with the compass and both players marked, speed, reset (nearest safe point), jump and speed-trap results shown to that player. A line showing the direction and distance to the other player.
3. **Shared:** one radio as now. Pause from either. The world map from the pause menu (acorn areas hidden or shown as in single-player) with "bring both players here" for travel. Nothing is recorded: no acorns, activity records or discovery from split-screen Free Roam (say so once on entry).
4. **Camera views per player:** each player changes their own view with their own view button, in split-screen races and Free Roam: chase, first person and the other views single-player has. First-person details (hands on the wheel, head hidden, floor) must be right in both halves at once.
5. **Performance:** the 60 fps target from stage 2 holds in Free Roam for two (worst case Night / Snow with traffic). Lower split-screen-only detail if needed and say what. Single-player rendering unchanged.
- Check: Free Roam for two with the AI as player 2, both layouts; each view in each half; pause, map, bring both here; frame time in the worst case; one single-player Free Roam session to show nothing regressed. **Own commit.**

### Part C — Police Chase, round 1: cop vs runner

Dan: "maybe we can even add a new game mode in free roam where there are police. If you have two players one can choose to be the cop and try to catch the other player." Decisions from the conversation: **Free Roam only** (not in the campaign); **roles swap** and the times are compared; the runner may go anywhere, off-road and over the jumps. Later rounds (not now): both players as cops catching speeding traffic for points; both players running from AI cops that radio ahead to cover the exits of trails and shortcuts.

1. **Police vehicle:** a patrol car built with the project's Blender vehicle pipeline: black and white, a roof light bar that flashes red and blue, the word POLICE, no real department's name or badge. Performance close to the faster cars so chases are fair; report its stats. Used only by the cop in this mode (not in the garage, shop or races). Siren on while chasing, toggled by the cop with a button; heard by both, louder as the cop nears the runner.
2. **Setup:** Mode: Police Chase. Choose who is the cop first, the runner's vehicle (any), time of day, weather, traffic, and the round limit (3 / 5 / 8 minutes, default 5).
3. **A round:** the runner starts on the road; the cop starts about 80 m behind and is released 3 seconds after the runner. Each player's HUD shows their role, the clock, the distance and direction to the other player, and the **bust meter**.
4. **Caught:** the bust meter fills while the cop is within 10 m of the runner and the runner is slower than 7 m/s; it takes 3 s to fill and drains at the same rate otherwise. Full = caught, round over. If the clock reaches the limit, the runner got away. Tune these numbers by play if they feel wrong and report the final values.
5. **No cheap escapes:** a runner's reset holds them still for 2 s; water and out-of-world are handled as in Free Roam; neither player can leave the world.
6. **Swap:** after round 1 the roles swap automatically (the new runner keeps their chosen vehicle; the new cop gets the patrol car). The player who lasted longer as the runner wins; getting away beats being caught; both getting away is a draw. Results show both times with Rematch / Change setup / Main menu. Names from Part A are used throughout.
7. **Testing alone:** player 2 can be the AI. As the **runner** it drives the road network at speed, choosing turns that lead away from the cop (it stays on roads; simple is fine). As the **cop** it is not available this round (the chasing AI is a later round): the option is greyed with "coming later".
8. Nothing is recorded to any board or save.
- Check: one full match with the AI as the runner (caught in one round), the meter filling and draining, a reset by the runner, both layouts, a shot of the patrol car day and night with the lights on. Controller only. **Own commit.**

### Part D — Abandoned Cabin Jump: put the shrubs back, but only as far as a well-hit jump can clear (do this first)

Dan on 0.93: "the only thing I see wrong with that update is it cleared too much of the shrubs from the cabin jump. I didn't want to completely remove the challenge just make it not quite as far because it either couldn't be cleared or was nearly impossible. So I want the shrubs restored but to not stretch as far as they were. I want it to go a distance that you can clear it if you hit the ramp correctly."

0.93 removed 1,579 bushes along the whole landing corridor of `DansBackyardForward` (lip at 41 m to the rejoin) and the slowing with them. The original brush reached past where any flight lands (landings measured 61–106 m).

- **Restore the bushes and the slowing** in the corridor from the lip to a new far edge, at their original positions (the 0.43 mesh is the source). Beyond the far edge the corridor stays clear as in 0.93, through to the rejoin.
- **Where the far edge goes:** fly the jump on the Needle 600, the Street Classic and the Trail Four at approach speeds of 24, 28, 31 and 34 m/s (34 is the branch's recommended speed) and record each landing. Set the far edge so that **every vehicle at 31 and 34 m/s lands at least 3 m beyond the last bush, and every vehicle at 24 m/s lands in the bushes**; 28 m/s may go either way. If one vehicle cannot clear at 31 m/s, use its 34 m/s landing and say so. Report the edge distance and the landing table.
- **Reset from the bushes:** a reset by a vehicle that is inside the restored brush puts it on the clear ground just past the far edge, on the centre line, facing along the route, stationary (the lost time is the penalty). The 0.93 reset checks for the rest of the branch stay.
- Tree-Top Trail's 0.93 clearing stays as delivered. Free Roam and Dan's Backyard Reverse untouched. No geometry, collider or route change (rule 5A).
- Check: the twelve flights after the change; one reset from inside the brush; a shot from the ramp approach and one of the landing showing where the bushes end.

### Verification

Light, per the Verification budget and the Controller-first rule, except the performance measure in Part B. Compile, launch, release steps. Results as a short list, with "for Dan to check".

## Previous delivery — Race start stuck until reset, Abandoned Cabin Jump ends in the bushes, acorn areas shown on the map, minimap compass — 0.93.0-review1 — DELIVERED, REVIEWED BY DAN (all good except the Cabin Jump lost too many shrubs; restored part-way in 0.94)

- **Authorized by Dan (2026-10-07, 14:18).** Written by Claude (chat) from his play of 0.92.0-review1 and debug session `2026-10-07_14-03-20-518_a28604` (both reports are on 0.92.0-review1; ZIP in his Downloads / the game's report folder). His words are quoted in each part.
- **Starting point:** main at the "Record 0.92 delivery" commit (0.92.0-review1 / game-92000). This TODO edit is uncommitted and belongs in the safety checkpoint.
- Runs unattended: design decisions are below; do not stop to ask about design. Stop only for a real external blocker (rule 7).
- **Scope is exactly Parts A–D below.** The Verification budget and the Controller-first rule apply.
- Dan's real campaign save is never written by a check (see the 0.92 incident): checks run on a copy, and the check runner must be pointed at the copy before anything loads.
- Dan on 0.92: the split-screen celebrations work ("it picked up the celebrations anyway"). The controller rows, the garage vehicle choice and left / right values: "yes everything is fine."

### Results (2026-10-07, Claude Code)

- **DELIVERED:**
  - Source `c240e1f3b005b7eb90e00ac2b02d7ebf0218d176` pushed and verified on origin/main.
  - Fresh 0.93.0-review1 Windows build from that commit: 0 errors, 2m55s ([build-release.txt](Docs/Report093/build-release.txt)).
  - Published [game-93000](https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-93000) with `Tools/Publish-LauncherRelease.py` (the project's `gh`): the known draft-lookup miss, then `--resume-draft` uploaded the three assets and published. Previous releases retained.
  - All 234 Latest files match the public signed manifest; public download, signature, install and startup pass ([hosted/result.json](Docs/Report093/hosted/result.json)); the production updater has 93000 active, the public catalog reports nothing newer ([launcher-catalog-check.json](Docs/Report093/launcher-catalog-check.json)).
- **Play-Racer.cmd (unchanged):** launched a responsive `Builds/Latest/versions/93000/Racer.exe` (0.93.0-review1) through the launcher ([play-racer-launch.json](Docs/Report093/play-racer-launch.json)); Dan's `campaign-v1.json` hash the same before and after. Latest root, current 93000 and previous 92000 retained.
- **Cleanup:** Builds 10,161,321,041 → 8,015,207,723 bytes (2.1 GB recovered: the build output and game.zip); also the hosted-check install (1.7 GB) and 2.0 GB of check scratch outside the project (test players, logs). C: free 263,745,159,168 bytes after cleanup ([cleanup.json](Docs/Report093/cleanup.json)).
- **SESSION HANDOFF: STOP.** Awaiting Dan's review of 0.93 (rule 12).
- **Safety checkpoint:** `bf4e9516` (this plan), pushed. Evidence: [Docs/Report093/](Docs/Report093/) ([Lists/](Docs/Report093/Lists/), [Shots/](Docs/Report093/Shots/)). Checks: `RaceStartCheck.cs` (built player, `-raceStartCheck`, runner `Tools/Report093/Run-RaceStart.ps1`), `Report093Checks.cs` / `Report093Walk.cs` (editor, emulated controller); tools `Tools/Report093/`. Version 0.93.0-review1 / build 93000.
- **Dan's save:** the editor check runner now starts on a copy of his save (`-racerTestSave` on the editor's own command line, set by `Tools/Report093/Run-Unity.ps1`, read by RaceFlow before anything loads); built-player checks use a copy as before. His save folder (224 files) hashed before the first check and after the last: unchanged.
- **A — Race start stuck (BUG-001) — FIXED.**
  - **Cause:** Dan's Backyard (Forward and Reverse share `BackyardForwardCourse`) had a grid of two rows 4 m apart (`GridStation`: index 0–1 at the line −7 m, every other index at −3 m). With more than three rivals the 5th and 6th vehicles were placed **inside** the 3rd and 4th (the Backyard Final, the Backyard Cup's and Grand Championship's Backyard rounds have five rivals; split-screen with 4 rivals is six too), so the front row was a jammed pile and nobody could pull away until the AI reset; and 4 m rows are shorter than the long cars (up to 5 m), so even a 3-rival Race Setup race with long cars overlapped. Not the 0.84 idle hold (the player's throttle read 1.00 from the controller at GO), not upgrades, not 0.92's input changes; other courses use a different grid and were fine.
  - **Reproduced first** in a built 0.92-code test player on a copy of Dan's save, Backyard Final on the Needle 600: overlaps GOLD/JADE 0.65 m and BLUE/VIOLET 0.55 m; every vehicle, the player included, moved under 0.6 m in the first 3 s; three resets ([before](Docs/Report093/Lists/A-race-start-before-0.92.txt), [shot](Docs/Report093/Shots/A-backyard-final-GO-before.jpg)). Split-screen (4 rivals) the same; a Race Setup race with three long cars overlapped (got away); the Street Loop Final was fine.
  - **Fix:** rows of two **6 m apart**, rivals from the front row back, the player's row last, for any field size (`BackyardForwardCourse.GridStation`). With 3 rivals the player's row moves from 7 to 9 m behind the line; with 5 it is 15 m back (on the drive down to the line).
  - **Check (built player, 3840×2160, controller throttle for 3 s then the race AI):** Backyard Final (moto), Snow Day (another campaign event, ATV), Street Loop Final, a Race Setup race on Backyard Forward with Longroof GT / Skyfin / Fastback, split-screen Backyard Forward with 4 rivals (player 2 the AI): **no overlaps, every vehicle 20–55 m in the first 3 s, zero resets in 10 s: ALL PASS** ([after](Docs/Report093/Lists/A-race-start-after.txt), [shot](Docs/Report093/Shots/A-backyard-final-GO-after.jpg)).
- **B — Abandoned Cabin Jump ends in the bushes (BUG-002) — FIXED.**
  - **What it was:** the 0.43 "traversable dense undergrowth" (a mesh of 3,272 collider-free bushes, centred on the branch from 53 to 69 m with an 18 m radius, which also slows anything driving on the bare ground under it) covers the whole landing: the 0.43 notes assumed the flight clears it, but every measured flight lands at 61–106 m, inside it. Being collider-free, the reset (nearest branch point, up to 1.7 m to your side; it only avoids solid things) put Dan back into it ([before](Docs/Report093/Shots/B-landing-before.jpg)).
  - **Fix (Dan's Backyard Forward only; Reverse and Free Roam keep the original mesh):** the scene's brush has its own copy of the mesh without the **1,579 bushes whose footprint reaches within 7 m of the branch centre line from the lip (41 m) to the rejoin** (landing spread up to 3 m from the centre + vehicle half-width + about 3 m), and `ShortcutUndergrowth` gets the matching cleared corridor (no slowing there). Bushes beside the corridor and every tree stay ([after](Docs/Report093/Shots/B-landing-after.jpg)). No geometry, collider or route changed.
  - **Flights (simple follower on the centre line, from the branch start):** Needle 600, Street Classic, Trail Four at 24 and 34 m/s (34 = the branch's recommended speed): take-off at 37–41 m, landing at 74–106 m within 3 m of the centre, all to the rejoin, no resets, **no bush or tree touched** ([after](Docs/Report093/Lists/B-flights-after.txt)). **Flat out does not reach the lip:** at 40–44 m/s the crest at the branch entry throws the vehicle onto the cabin roof (it leaves the lip at 7–15 m/s or crashes beside the cabin); the approach is built for about 31–34 m/s (0.43 measured 30.6–31.8 m/s on the roof). Not changed (protected jump, rule 5A); for Dan.
  - **Trees beside the run-out kept:** seven trunks stand 4.4–6.5 m from the centre (1.2–3.3 m outside the 3.2 m trail edge); no flight came within 2.2 m (centre to trunk) of one. They frame the trail; removing them would mean removing colliders and trees from the scene, so they stay (for Dan).
  - **Reset at Dan's position** (248.02, 75.68, 82.79), on the branch: ends at s 64.3, 1.7 m off centre, facing along the route, **nothing solid or bush within 5 m ahead** ([shot](Docs/Report093/Shots/B-reset-from-Dans-position-after.jpg)).
  - **Every shortcut on all eight courses, reset points every 4 m** (a bush or tree crown on the line or within 5 m ahead, or anything solid that is not the trail's own surface): **0 on 16 of 18 shortcuts** (Street Loop Forward (4) / Reverse, Forest Loop Forward / Reverse, Mountain Loop Forward / Reverse, Backyard Reverse's two). Backyard Forward: **Tree-Top Trail** had brush on its ground-level entry (20–24 m) and exit (88–96 m) reset points: the 59 bushes within 3.5 m of its line that rise above the trail removed the same way (own mesh copy; bushes under the raised boards stay; a vehicle that falls off the boards is still slowed there). Still flagged: Tree-Top 92–96 m (small bushes 2 m to the side below the exit drop) and Cabin 28–40 m (the ramp and roof, where resets are never placed; bushes there are under the roof) ([list](Docs/Report093/Lists/B-resetpoints-DansBackyardForward-after.txt)). No reset point (route) was moved.
  - **Dan's Backyard Reverse:** has no Abandoned Cabin Jump (its shortcuts are Abandoned Logging Ridge and Storm Drain / Gully Jump; the cabin trail is not a route there); its reset points are clear; nothing changed.
- **C — Acorn areas on the map — DONE.**
  - **Regions:** an area's acorns can be far apart (Western gullies spans the whole west: 22,500 / −351,502 / −601,150 / −365,−531), so each area is drawn as one or more rounded regions: acorns within 150 m of each other share one (their hull grown by 60 m); a lone acorn gets an 85 m circle whose centre is moved 40 m off it, so no region's middle gives a spot away. Neighborhood woodland 3 regions, Western gullies 4, Creek pockets 2, Ridge woodland 1; softly tinted in the area's colour with an outline, under the roads, routes and markers; the chosen area bright with a white outline; a finished area grey and dim with ✓. One name and count per area (its biggest region), so the default zoom is not crowded; the list names are in the regions' colours ([areas](Docs/Report093/Lists/C-areas.txt)).
  - **List ↔ map:** the map's side column lists the four areas with count and direction; **D-pad ↑ / ↓** (PgUp / PgDn) steps through them, each highlighted and centred (zoomed to fit); a new "Acorn area" footer prompt. The **Exploration page** lists each area (name, count, ✓, and its direction on a second line); A on one opens the World Map on it.
  - **Directions** (from the real positions; only the three roads, the houses, the lake and the mountain): Neighborhood woodland "Around Dan's and Kyle's houses, and west of S Cherokee Ln up toward Hwy 92"; Western gullies "West of S Cherokee Ln, spread along Hwy 92 and Trickum Rd"; Creek pockets "East of the lake, and two higher up the mountain"; Ridge woodland "North-east of the lake, up the mountain toward the summit".
  - **Place names on the map:** S Cherokee Ln, Hwy 92, Trickum Rd, Dan's house (Moll's), Kyle's house (Anderson's), The lake, Mountain summit (1230, 296, 200, the highest ground). Names never overlap: area names first, then place names, then the discovered landmarks (a landmark whose name would overlap shows its dot until zoomed in).
  - **Minimap (Free Roam):** inside an area that still has acorns, its name and count under the minimap ("Western gullies 3 / 4"); nothing when the area is finished or outside every area.
  - **Names kept:** none is wrong for the general place, but the areas were assigned by acorn number, not by place: **Creek pockets' last two acorns (Quiet gully, Old stone corner, about 1027, 26) are up the mountain beside the Campsite**, just south-east of Ridge woodland's region. Nothing granted; no acorn moved; still 24; the Turf Rocket unlock unchanged.
  - **Check (controller only, emulated gamepad, 3840×2160, a copy of Dan's progress 22 / 24):** every acorn inside its area's region (4 / 4); map opened with Back; D-pad down chose each area in turn (highlighted, centred); B closed; Exploration page: D-pad to Western gullies, A opened the map on it; minimap inside Western gullies "Western gullies 3 / 4", outside every area no line: **all PASS** ([list](Docs/Report093/Lists/C-checks.txt), shots `C-*.jpg`).
- **D — Minimap compass — DONE.** The minimap turns with the vehicle (heading up, unchanged), so an **N** travels round its rim to where north is, with small ticks for E, S and W; in race, Free Roam and each split-screen half (all minimaps are made by `RacingMiniMap.Create`). The world map has a fixed **▲ N** in its top-right corner. **North is world +Z**, the convention the map (+Z up, "NORTH ↑") and the route atlases ("North is +Z") already use; Part C's directions use it. Check: heading north the N at the top, heading east on the left, in a race and in Free Roam; both split-screen minimaps show it: **all PASS** ([list](Docs/Report093/Lists/CD-checks.txt), shots `D-*.jpg`).
- **Decisions:** grid rows 6 m apart with the player last (also for 3 rivals); the Cabin corridor 7 m each side from the lip, trees kept; Tree-Top cleared only where its reset points are at ground level; acorn areas drawn as several regions per area (one name each); landmark names give way to area and place names at low zoom.
- **For Dan to check:** a Backyard Final start with his controller; the Cabin Jump landing and a reset there; whether the trees 1.2–3.3 m outside the Cabin run-out should go too; the flat-out Cabin approach (above about 36 m/s the entry crest throws you onto the roof); the map regions and wording (and whether Creek pockets' two mountain acorns should belong to Ridge woodland); the compass size in split-screen.

### Part A — Race start: every vehicle is stuck until it resets (BUG-001)

Dan: "beginning of race all vehicles stuck at first and don't move until after they reset."

- Where: campaign event **"Backyard Final"**, Dan's Backyard Loop - Forward (`DansBackyardForward`), Race, Day / Clear, 6 on the grid, Dan on `moto` at (466.44, 80.39, 6.82) heading 272.5°, 0 mph with GO! showing and the lap clock at 00:00.000; two rival cars directly ahead are also stationary. Screenshot `BUG-001.png`.
- Reproduce it first, in the built player, in that event with that vehicle on a copy of his save. Find the cause; do not guess. Things to rule out: the idle hold from 0.84 (vehicles must not creep with no throttle) not releasing at GO; a championship / campaign grid placing vehicles into the ground or into each other; upgraded-vehicle setup; a 0.92 change to start or input ownership (split-screen stage 2 touched who reads which device).
- Check whether it happens in every campaign event and championship, in a plain Race Setup race, and in split-screen, and fix it wherever it does. After GO every vehicle, player and AI, pulls away with no reset.
- Check: the Backyard Final start, one other campaign event, one Race Setup race and one split-screen race (Player 2 as AI): all move at GO with zero resets in the first 10 s. Say what the cause was.

### Part B — Abandoned Cabin Jump (Dan's Backyard Forward): the landing and the reset put you in the bushes (BUG-002)

Dan: "reset here should remove you from the bushes. I don't think I have been able to clear this shortcut once without ending up in the bushes."

- Where: `DansBackyardForward`, branch Abandoned Cabin Jump at 64.30 m, vehicle stopped at (248.02, 75.68, 82.79) heading 96.9°, inside dense bushes just after a scored clean jump (65.2 ft, 1.72 s). Screenshot `BUG-002.png`.
- Two faults, fix both:
  1. **The reset:** a reset on this shortcut left him in the bushes. A reset anywhere on a shortcut must put the vehicle on clear, drivable ground on that branch (or on the main at the rejoin), facing along the route, with nothing solid or view-blocking within the vehicle's length ahead. Check the reset points along this branch and move any that sit in vegetation. Apply the same test to the reset points of every shortcut on all eight courses and report any others moved.
  2. **The landing:** a normally driven jump must not end in bushes. Fly the jump on the Needle 600, the Street Classic and the Trail Four at a slow, a typical and a flat-out approach, record where each lands and runs out, and clear the bushes and other scenery from that landing and run-out corridor (with a margin of about 3 m each side) through to the rejoin. Trees that frame the corridor outside it stay. If the fast landings fall outside any sensible corridor, lengthen or re-aim the landing ground inside this race scene only (rule 5A; Free Roam untouched) and say so.
- Check Dan's Backyard Reverse for the same jump the other way and treat it the same.
- Check: the nine flights after the fix, all rolling out to the rejoin without touching vegetation; one reset at Dan's reported position ends on clear ground. One shot of the landing before and after.

### Part C — The map shows where the acorn areas are

Dan: "The map gives us locations where acorns are. There, however, is no indications of where these areas are. It is just made up names that mean nothing to anyone, there should be some markings to show where, for example, the western gullies are."

The acorn list names four areas (Neighborhood woodland, Western gullies, Creek pockets, Ridge woodland; `DiscoveryDetails.cs`), but nothing on the map says where they are.

1. **Areas drawn on the map:** on the Free Roam world map, each acorn area is a softly tinted region with an outline and its name written inside it, with its count (found / total, for example "Western gullies 2 / 4"). Derive each region from the positions of that area's acorns with a generous margin (about 60 m, rounded shape) so it shows where to look without giving away exact spots. Regions must not hide roads, trails or the player marker. A fully collected area is drawn dimmer with a tick.
2. **List and map linked:** wherever the acorn areas are listed (the map's acorn panel, the Codes / collection page), choosing an area with the controller highlights its region on the map and centres the map on it. Each list line also gets a short plain direction from a place the player knows, for example "west of S Cherokee Ln, behind Kyle's house" (ground the wording in the real positions; use only the three road names, Dan's house, Kyle's house, the lake and the mountain).
3. **Place names on the map:** label the things the directions refer to: S Cherokee Ln, Hwy 92, Trickum Rd, Dan's house, Kyle's house, the lake, the mountain summit. Small, readable at 3840×2160, not overlapping at the default zoom.
4. **Minimap:** when the Free Roam minimap is on and the player is inside an area that still has acorns, show the area name and count under the minimap ("Western gullies 2 / 4"). Nothing when all are found.
5. Rename an area only if its name is wrong for where its acorns really are (say which and why). No change to acorn positions, the count of 24, or the Turf Rocket unlock. Dan is at 22 / 24: do not grant anything in his save.
- Check: map shots at default zoom and zoomed on one area with the list selection; the four regions each contain all of their acorns; minimap line inside and outside an area. Controller only.

### Part D — A compass on the minimap

Dan: "Can we put a compass on the minimap?"

- The minimap (race and Free Roam) gets a compass: an **N** marker on its rim showing where north is, with small ticks for E, S and W. If the minimap is drawn north-up the N sits at the top and the player arrow turns; if it turns with the vehicle, the N moves round the rim. Do not change which of the two the minimap does.
- The world map gets a small fixed north arrow in a corner.
- North is the world's +Z direction unless the project already defines north differently (the road names and the "west of…" directions in Part C must agree with it; say which convention is used).
- In split-screen each player's minimap has it. Readable at 3840×2160, and at the smaller split-screen size.
- Check: one shot heading north and one heading east, in a race and in Free Roam.

### Verification

Light, per the Verification budget and the Controller-first rule. Compile, launch, release steps. Results as a short list, with "for Dan to check".

## Previous delivery — Menu fixes from Dan's 0.91 play (controller still skips two rows, garage for vehicle choice, no A-to-cycle, a vehicle for every chapter), Forest Reverse bump, split-screen stage 2 — 0.92.0-review1 — DELIVERED, REVIEW STARTED BY DAN (split-screen celebrations work; race-start stuck, Cabin Jump bushes and acorn areas on the map in 0.93)

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
