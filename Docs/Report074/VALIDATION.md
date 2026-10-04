# 0.74.0-review1 — validation (targeted, rule 11)

Evidence folders: [Views/](Views/) (editor and in-game screenshots), [Checks/](Checks/) (play-mode results, ride logs),
[Profiles/](Profiles/) (driveway surface profiles), [Bench/](Bench/) (3840×2160 frame times and condition screenshots).
Authoring notes: `partA-author-notes.txt` (all scenes; its Backyard Reverse driveway pass was redone in
`partA-dbr2-author-notes.txt`), `partA-b4-author-notes.txt`, `partA-dbr-partC-author-notes.txt` (Part C section),
`partB-author-notes.txt`. Tools in `Tools/Report074/`; play-mode checks `Assets/Scripts/Report074Checks*.cs`.

## Part A — 6 reports

| Item | Disposition |
|---|---|
| BUG-001 tree in the brick-house driveway | **PASS.** Street Loop Forward only (no other scene has that tree): trunk collider and its two batched pieces moved 6 m south onto the grass, re-grounded (−0.42 m). Nothing within 2.5 m of the new spot. [after](Views/BUG-001-after.png) |
| BUG-002 tall diamond sign at the top of the House 3 driveway | **PASS.** "Simple bend warning" (post, panel, chevrons) removed in all 8 course scenes. |
| BUG-003 House 3 driveway | **PASS (Forest Loop Reverse: see note).** Details below. |
| BUG-004 holes at the trail edge, Mountain Forward s 12 | **PASS.** One smooth collidable surface (0.71 junction method) over the edge and the bank beside it (x 728–744, z −121…−141), tucked 3 cm under the pavement edge; stacked sheets under it lowered 0.8 m. 0.71 Climbing Ridge Cut patch untouched (kept surface). From straight above the patch edge shows as a shading step in the grass (no step in height); from the road it reads smooth ([after](Views/BUG-004-after.png)). Mountain Reverse has pavement there (different meshes): unchanged. |
| BUG-005 torn lifted lip, Mountain Forward s 1503 | **PASS.** Smooth surface over the left bank (z 21–42), flush with the road edge (−0.88…+1.03 m vs the old top). The pavement itself was already smooth at 1 m (residuals 0); not changed. Mountain only. |
| BUG-006 old cave in Free Roam from Mountain Forward | **PASS** — see Part C. |

**BUG-003 — what was done.** The straight driveway (0.30) dropped 49 m in 81 m through a slot cut into the valley wall:
steepest 2 m grade **205 % (64°)** at its lip. It is replaced, in the seven scenes that had it, by the original winding
driveway (the pre-0.30 line, still the driveway in Street Loop Reverse, whose ground was intact everywhere):
- the slot is filled back to the hillside (fill only, blended exactly as 0.30 cut it; the old dirt colouring repainted);
- the drive follows the original line down the slope, round the House 3 lake's west tip and along its north shore (the
  lake was added on top of the old line in 0.31; the new line keeps 8 m from the pool), ending at the house;
- centre profile = the ground smoothed over ±8 m, a level start at the gate rising smoothly into the slope (Hermite ease,
  no crest), terrain benched to a 7 m gravel surface (≤3 m cut / 3.5 m fill, the lake and pool kept out);
- trees standing in the new driving width (1–7 per scene) moved out to 6.5 m from the centre (re-grounded); fences and the entrance sign follow the
  ground; the "House 3 valley driveway" road follows the new line.
- **Steepest grade after: 32 % (18°) at the natural hillside** (ground along the original line, which the profile
  follows); profile check: no crest or dip over 8 grade points within 3 m in six scenes ([profiles](Profiles/)).
- Street Loop Reverse: unchanged (it never had the straight driveway; Laurel protected).
- **Forest Loop Reverse (exception):** the Forest Reverse gap-jump embankment lies on the original line. There the drive
  crosses that trail at grade where its south shoulder is lowest and slides off it westward; nothing within 5.5 m of the
  trail centre is reshaped. The trail's own raised edge stays (protected jump approach), so going up the drive a vehicle
  hops 0.36–0.8 s onto it, and the descent off the embankment reaches 38–46 % for a few metres.

**Rides** (pace 10 m/s ≈ 22 mph, autopilot, both directions):

| Scene | Moto | ATV | Original (car) | Tourer (car) |
|---|---|---|---|---|
| Street Loop Forward (Dan's report) | down/up PASS (air ≤ 0.28 s) | PASS | PASS | PASS |
| Lake Woods | PASS | — | PASS | — |
| Dan's Backyard Reverse | PASS | — | PASS | — |
| Forest Loop Reverse | down PASS, up hop 0.36 s | hops 0.44 / 0.80 s, no stall (≥ 5.5 m/s) | down PASS, up hop 0.38 s | down PASS, up hop 0.38 s |

No ride stalled (slowest uphill 8.9 m/s in Street Loop Forward, 5.5 m/s in Forest Reverse), wiped out or reset. Street
Loop Forward at 13 m/s (29 mph) downhill: one 0.40 s hop. Before: the straight driveway's 205 % lip launched and its
55–60 % slot made climbing marginal ("almost undriveable"); after: ≤ 34 % driven everywhere outside Forest Reverse.

**5A.6 (Climbing Ridge Cut entrance):** the entrance line (743,−128 → 766,−108) at 20 m/s (moto) and 16 m/s (ATV): PASS,
max grade 7 %, no air. BUG-004 / BUG-005 lines at 20 and 26 m/s (moto, ATV): PASS, no air over 0.32 s, no wipe-out.

## Part B — the storm-drain tunnel in every Free Roam

- Dan's Backyard Forward/Reverse already have it (open in Free Roam since 0.54): unchanged.
- In Street Loop F/R, Lake Woods, Forest Loop Reverse and Mountain Loop F/R it is copied from Backyard Reverse as Free
  Roam content (`FreeRoamOnly`): culvert, floor, mouths, 12 lights, channels, inlets, the 0.57 storm flow (36 water
  volumes: frozen in Snow by the 0.73 ice system) and debris, gully takeoff and west-bank landing, the rat encounter and
  litter (not the LOGGING RIDGE sign). The other courses have no gully and closed mouths there, so three terrain tiles take
  the Backyard Reverse shape in Free Roam only (hybrid tiles: Backyard triangles on the ≈7,360 m² where the grounds differ
  and that connect to the tunnel); trees on the changed ground are re-grounded and trees standing in the tunnel line are
  hidden — in Free Roam only. In races the scenes are exactly as before. No route of those scenes crosses the changed
  ground (route points within 20 m are excluded).
- Map: landmark **"Storm drain tunnel"** at the upper entrance (all 8 scenes; fast travel as for the other landmarks).
- **Rides** through the tunnel (Free Roam): Street Loop Forward moto Clear and Snow, ATV back; Mountain Loop moto Clear and
  Snow, ATV back in Snow; Backyard Reverse moto; after the final Part B pass, moto in Mountain Loop, Mountain Reverse,
  Lake Woods, Forest Loop Reverse and Street Loop Reverse: all PASS (186 m, no air over 0.12 s, no wipe-out).
- Upper entrance in every changed scene: [Street F](Views/tunnel-entrance.png), [Street R](Views/tunnel-entrance-StreetLoopReverse.png),
  [Lake Woods](Views/tunnel-entrance-LakeWoods.png), [Forest R](Views/tunnel-entrance-ForestLoopReverse.png),
  [Mountain F](Views/tunnel-entrance-MountainLoop.png), [Mountain R](Views/tunnel-entrance-MountainLoopReverse.png); in
  Snow: [Street F](Views/tunnel-entrance-snow.png). (A tree first stood in the mouth in the Lake Woods / Forest / Mountain
  scenes: a second copy of it lives in the CR056 "Forest detail batch"; that batch is now included, and it is hidden in Free Roam too.)
- Backyard Reverse race credit: the Backyard scenes' drain, its branch data, gates and checkpoints are byte-identical to
  0.73 (only the House 3 area, the sign and the new map landmark changed in those scene files); the Free Roam ride there passes.

## Part C — one cave everywhere

- Mountain Loop and Mountain Loop Reverse: the pre-0.57 cave (enclosure, 40 rock buttresses, 20 warm wayfinding lamps)
  replaced by the Lake Woods (Forest Forward) cave: flight-chamber enclosure, rockfall, 45 edge boulders, 166 fallen
  stones, gravel floor and 122 patches, 61 puddles, 42 roots, 42 formations, 3 drip sources, the 0.59 hillside cover.
  Terrain under the cave is identical in Lake Woods and the Mountain scenes (0 differences on a 3 m grid).
- Bats: the Forest Forward bat swarm now also flies in Free Roam in every scene that has the cave.
- **Forest Loop Reverse: left as is** — its own Fern Gully branch runs through the cave (8 route points under the roof) and
  its terrain there is shaped for the Reverse course (it has the 0.56 Reverse cave and atmosphere).
- Side by side: [Forest Forward](Views/cave-forest-forward.png) / [Mountain](Views/cave-mountain-bug006.png) (identical);
  ride through the cave in Free Roam, Mountain F and R: complete, no wipe-out or reset (the 1.4 s flight is the accepted
  cave flight-chamber jump).

## Part D — rain sound and storms

- Rain: a 24 s seamless loop made at start-up: low rounded bed (two softly filtered layers), slow swells and two heavier
  gusts, light patter (soft droplet ticks, a few heavier drips), all under 5 kHz (no hiss); 0.42 of the Ambience volume
  (was 0.55); low-passed to 650 Hz under cover.
- Storm: 3-minute Rain recordings (Street Loop Forward, Free Roam, autopilot-free camera; [results](Checks/storm-shots-results.txt)):

| | Strikes / 180 s | Gaps (s) | Near (< 600 m) / far | Close pairs (< 5 s) | Lowest flash peak |
|---|---|---|---|---|---|
| Day (12:00) | 13 (13 bolts, 13 thunders, 14 distant rumbles) | 2.6 – 24.7 | 6 / 7 | 2 | 0.40 |
| Night (00:00) | 12 (12 thunders, 13 distant rumbles) | 8.1 – 24.3 | 2 / 10 | 0 | 0.52 |
| Lightning flashes Off, Day / Night (60 s) | 4 / 5 | — | — | — | 0.00 (bolts 4 / 5, thunder kept) |

  Bolts are drawn in the sky at their distance and bearing (visible by day and night; a few branches each at night).
  Gaps are irregular in 8–25 s (20 % chance of a quick second strike); every strike flashes clearly.
- Thunder: near (< 600 m) a sharp crack arriving in ~1–2 s, far a long low roll 3–5 s later (sound at 343 m/s), volume
  up to 1.0 of Ambience (rain lower than before); distant rumbles every 7–16 s with a faint glow in the clouds.
- Bolts: a forked bolt (main channel and 2–5 branches) at the strike's bearing and distance, from just under the lowest
  clouds to the ground, so nearer clouds never hide it; shown with the flash, by day and night
  ([day](Views/lightning-bolt-day.png), [night](Views/lightning-bolt-night.png), held lit for the screenshot;
  [results](Checks/bolt-results.txt)). Found while checking the 4K shots: the first version started the bolt at 470 m,
  inside the cloud layer, so by day only a stub showed under the clouds — fixed (top ≤ 395 m, slightly wider far bolts,
  unused branches of an earlier bolt no longer reappear). Strike timing and counts are unchanged by that fix.
- Comfort: never more than two pulses per strike; "Lightning flashes: Off" → no screen flash, bolts and thunder stay.

## Part E — Dawn

- Race setup Time of Day: **Dawn / Day / Dusk / Night** (saved value of Dawn = 3; older saves keep their choice).
- Dawn: sun 8° in the east (azimuth 96°, Dusk is 286°), pink-to-pale-gold light, bluish ambient shadows, lavender-pink
  haze, light ground mist lying in hollows (fades as the cycle reaches Day); headlights at half as at Dusk.
- Free Roam passes through it (about 04:36–07:36, strongest around 06:00). One Dawn race (Street Loop Forward, 1 lap, AI field): start, 14 checkpoints,
  finish, results, record saved — PASS.
- Screenshots (3840×2160, [Bench/](Bench/)): Dawn and Dusk from the same four course views —
  [street dawn](Bench/street-dawn-clear.jpg) / [dusk](Bench/street-dusk-clear.jpg),
  [forest dawn](Bench/forest-dawn-clear.jpg) / [dusk](Bench/forest-dusk-clear.jpg),
  [backyard dawn](Bench/backyard-dawn-clear.jpg) / [dusk](Bench/backyard-dusk-clear.jpg),
  [mountain dawn](Bench/mountain-dawn-clear.jpg) / [dusk](Bench/mountain-dusk-clear.jpg); Dawn also in Rain and Snow;
  Free Roam passing through it: [05:00](Bench/freeroam-cycle-0500.jpg) … [06:30](Bench/freeroam-cycle-0630.jpg) …
  [08:00](Bench/freeroam-cycle-0800.jpg). Dawn reads cool lavender-pink with pink clouds; Dusk stays warm amber.

## Part F — continuous Free Roam clock, 30-day calendar, moon

[results](Checks/clock-results.txt) — all PASS (Street Loop Forward, isolated save):

| Check | Result |
|---|---|
| First ever start (no saved value) | Day 1 08:00 |
| Leave by starting a race at Day 3 21:30 | saved to settings.json; Free Roam resumes at Day 3 21:30 (race time does not advance it) |
| Leave by opening the menu at Day 9 13:15 (the Free Roam pause menu is the main menu) | saved on opening; resumes at Day 9 13:15 |
| Quit the game at Day 17 04:45 | saved (read back from settings.json) |
| Cross midnight on Day 14 | Day 15 00:00; moon phase moves continuously (0.47, 99 % lit) |
| Day 30 at midnight | wraps to Day 1 (new moon) |
| HUD / debug conditions | "FREE ROAM Day 1 00:00 / …", "Free Roam Day 1 00:00 (Night) / Clear" |
| Unreadable saved value (NaN hour, day 77) | Day 1 08:00 |

Moon: phase = (day − 1 + hour/24) / 30 (day 1 new, day 15–16 full); the moon disc shows the phase and rises/sets with
it; night light comes from the moon by its illumination, or a dim sky glow when it is down (00:30: day 1 0.18, day 8 0.18,
day 15 0.40 moonlight, day 22 0.33). Night races always use a full moon. Moon-phase screenshots: [day 1](Bench/moon-day01.jpg) (new, not drawn), [4](Bench/moon-day04.jpg), [8](Bench/moon-day08.jpg) (half), [15](Bench/moon-day15.jpg) (full), [22](Bench/moon-day22.jpg), [27](Bench/moon-day27.jpg).

## Part G — map waypoints

[results](Checks/waypoint-results.txt) — all PASS (Street Loop Forward, Free Roam):

| Check | Result |
|---|---|
| Place with the map cursor (Waypoint: F / gamepad Y) | waypoint at (540, 23, 536) |
| Place again | the old one is replaced |
| Clear (Backspace / left stick press) | cleared |
| Mouse click in an unexplored (fogged) area | placed at (−981, 0, −767) |
| Right-click | cleared |
| In the world | a tall beacon beam; HUD bottom-centre "WAYPOINT 198 ft" with an arrow turning toward it |
| Drive within 15 m | "Destination reached", waypoint cleared |
| In a race | not shown, no effect |

Not saved (a new session starts without one). Clicking a landmark still selects it for fast travel.
[beam by day](Views/waypoint-beam-day.png) / [at night](Views/waypoint-beam-night.png)

## Frame rate (3840×2160, GTX 1660 Ti, GPU median)

GPU median frame time, GTX 1660 Ti, 3840×2160 ([Bench/conditions.txt](Bench/conditions.txt); 0.73 from Docs/Report073/VALIDATION.md):

| View | Day/Clear 0.73 → 0.74 | Dawn/Clear 0.74 | Night/Rain 0.73 → 0.74 (strikes every 2 s) | Night/Snow 0.73 → 0.74 |
|---|---|---|---|---|
| Street | 3.76 → 3.76 ms (266 fps) | 4.60 ms (217 fps) | 4.53 → 4.51 ms (222 fps) | 4.57 → 4.55 ms (220 fps) |
| Forest | 3.36 → 3.40 ms (294 fps) | 4.13 ms (242 fps) | 4.23 → 4.18 ms (239 fps) | 4.23 → 4.23 ms (237 fps) |
| Mountain | 3.13 → 3.01 ms (332 fps) | 3.07 ms (325 fps) | 3.55 → 3.30 ms (303 fps) | 3.54 → 3.34 ms (300 fps) |

**PASS.** Strikes cost nothing measurable. Dawn (the ground mist and low sun) costs about as much as Night (lamps): its
worst view, Street 4.60 ms, is within 0.03 ms of 0.73's worst (Street Night/Snow 4.57 ms). Motorcycle grid (player + 3 AI,
Day/Clear chase): 3.96 ms (0.73: 3.93 ms).

## Not changed / notes

- Weather and time of day stay visual and audio only. No vehicle physics, AI or race data changed.
- Street Loop Reverse: driveway unchanged (original winding one); Forest Loop Reverse: cave unchanged (its course needs it).
