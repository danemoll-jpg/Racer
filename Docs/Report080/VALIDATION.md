# 0.80.0-review1 — validation

Clean-up from Dan's 0.79 review (debug session `2026-10-05_14-24-33-951_f0ab5f`, 10 reports), fist wave on LB, cars
allowed on every course. Targeted checks (rule 11), run in the editor in play mode, muted, isolated save
(`Assets/Scripts/Report080Checks.cs`, runners in `Tools/Report080/`). Screenshots: [Shots/](Shots/) (Dan's 0.79
screenshots are `before-BUG-0xx-dan-0.79.jpg`); tables and logs: [Lists/](Lists/).

## Part A — the 10 reports

| Report | Cause | Fix | Result |
| --- | --- | --- | --- |
| BUG-001/002/003 posts in the road (FreeRoamWorld) | Old shortcut edge markers from 0.6 (`CR034-039 Woodland routes/Trail edge`, amber, 9.3 m either side of the Pine Ridge / Fox Gully / Creek Leap branches) and Phase 5 (`Southwest woodland shortcut/Non-colliding path edge`, yellow). Where those shortcuts run along or across roads, the posts land on the pavement. None has a collider. | `RoadPosts` (runs at load, before the scenery is built): any small upright object (≤ 1.2 m footprint, 0.2–6 m tall) whose base is on a paved road or driveway is removed in FreeRoamWorld; old shortcut edge markers within 4 m of the pavement go too (no shortcut racing in Free Roam; Dan saw these as the dark posts). | **PASS.** 22 removed in FreeRoamWorld (5 Phase 5, 17 Trail edge), including all three reported spots; 0 posts on pavement afterwards (the sweep runs at every load). Course scenes (5A): the same 9 posts stand on pavement in every course scene and are shortcut **race markers, kept** (Street Loop Reverse also keeps its 2 branch entrance markers, Mountain Loop the Summit Traverse sign post: 11 and 10), listed in [load-report-course-scenes.txt](Lists/load-report-course-scenes.txt) and [-2](Lists/load-report-course-scenes-2.txt). |
| BUG-004/005 lane markings, Trickum Rd / Hwy 92 | Three paint sets on top of each other: the 0.6 set (`Highway edge / median`, `Broken lane line`) follows the Street Loop race line, so in the 70 m merge zones it curves from the side road across the lanes (its median is Trail-amber, which reads red at night: the "red/white strip"); the CR113 highway set (double yellow, lane dashes) runs straight along Hwy 92 and overlaps it; and the terrain shader (`Racer/MarkedGround`) draws the old two-lane edge lines and centre dashes wherever the terrain shows. | `JunctionPaint` (paint only, every scene): keeps CR113 and the 0.6 pieces that lie on Hwy 92's own lines (median 0.12 m, lanes 4.1 m, edges 8 m) and hides the 188 that are off them or duplicate CR113; the old median is drawn CR113 yellow; the terrain's own markings are switched off on Hwy 92's pavement (3,381 vertices, in a copy of the drawn mesh); continuous white edge lines along Hwy 92 where none existed, broken at each side road's mouth and turning into it with a 9 m curve; a stop line across the side road's lane into Hwy 92. | **PASS.** One clean layout at Trickum Rd and along Hwy 92 to BUG-005 (day and night shots); S Cherokee Ln / Hwy 92 had the same doubling and got the same treatment; S Cherokee Ln / Trickum Rd has no doubling (only the Jamerson continuation's centre dashes), unchanged. No road shape or collider changed. |
| BUG-006 the giant-jump landing | The landing and return ribbon (`CR094 summit launch/Ground_CR103 supported return`) was drawn with "Summit packed earth", a shiny URP Lit red-brown unlike every other mountain road (dirt painted into the terrain). | `MountainDirt` (look only): drawn with the terrain's own material in the trails' dirt colour, blending over its outer 3 m into the colour of the terrain underneath. Shape and collider (its own mesh) untouched. | **PASS.** Giant jump re-run as in 0.71 (full throttle from rest, coasting after landing): **motorcycle 242.5 m** (0.71: 242.5–243.5), ATV 223.5 m (0.71: 223.5–224.0); both land upright and roll out 87 / 107 m. |
| BUG-007 summit road | Same material on `Ground_CR103 supported connection` and the run-up `Ground_Summit authored launch`. | Same treatment (whole strip dirt). | **PASS.** These three meshes are the only mountain roads in FreeRoamWorld that were not terrain-painted dirt; all now one dirt look. They appear in all six Street-Loop-world scenes too and get the same look there. |
| BUG-008 hole (Mountain Loop Reverse) | The 0.70 crest barrier outcrop is an open rock shell: its lower rim hangs up to 16 m above the slope, and beside it a narrow V-shaped trench (up to 5.3 m) between its flank and the shoulders. Dan was wedged under the rim. | A rock face from every hanging rim edge down to 0.6 m into the ground (253 edges, collidable); the trench filled to its spill level and its narrow slots raised to their lower rim (182 m², collidable, shoulder material). | **PASS.** Dan's spot is now inside solid rock. Escape test: a motorcycle and an ATV dropped at 21 points within 10 m: 20 ride straight back onto the course; the 21st starts on top of the barrier outcrop itself and slides off its far side, 25 m from the road (not reachable from the course). |
| BUG-009 bump | The trail surface is smooth; at the start of the Downhill Ridge Cut the earth banks meet its edges with 10–50 cm notches (left edge, s 12–24) and steps. | A smooth collidable strip over each edge, s 0–24: tucked under the trail, flush at the edge, then straight to the bank (71 stations; the right side s 0–10 opens into the main road junction and was left). Stops before the jump run-up. | **PASS.** Downhill Ridge Cut jump unchanged: 30 m/s motorcycle 1.16 s air, ATV 1.22 s — identical before and after; the rides along the edges are clean. |
| BUG-010 floating road | The reverse runway / entry deck sits 1.2 m above its support with no bank under its edges, so the slab shows open air. | Earth embankment from 5 cm under each driving-surface edge outward and down at 1:1 to the ground, one smooth welded surface. Applied mountain-wide wherever an edge had 0.4–8 m of open air under it. | **PASS.** 1,360 edge points embanked. Left, listed in [mountain-decks-MountainLoopReverse.txt](mountain-decks-MountainLoopReverse.txt): 110 deeper gaps (ravines, flight gaps, bridges) and 1,573 protected places (jump recovery zones, 30 m around a flight's lip-to-landing, above another route, under a roof). |
| Item 8: same geometry elsewhere | — | MountainLoop (Forward) has none of the BUG-008/009/010 geometry (no crest outcrop, no reverse runway); FreeRoamWorld has no race mountain roads at all. | Nothing to change there. |

**Grounding (section 4):** 27,525 trees, trunks and objects checked against the new 0.80 ground: one crown-only tree
clump lies 12 m under the new embankment at (861, 99, −58), fully covered and not visible; one non-colliding gate
marker post's foot is 0.31 m into the embankment at (966.5, 163.2, 143.6). Three gate markers stand on the deck edge
where the embankment now is below them (before: open air). [buried-MountainLoopReverse.txt](Lists/buried-MountainLoopReverse.txt)

**Races (item 8, 5A.6):** Mountain Loop Reverse (after all changes) and Forward, player motorcycle on the AI driver vs
motorcycle / ATV / motorcycle: everyone finished, **0 missed gates**; jumps made (no failed flight; one reset per racer
in Reverse, cars and bikes alike). [partC-races-mountain-final.tsv](Lists/partC-races-mountain-final.tsv)

## Part B — fist wave on LB: PASS

- Bindings: `<Gamepad>/leftShoulder`, `<Keyboard>/f` (RB removed). Settings > Controls reads the same action.
- Emulated controller, motorcycle and car: **LB while driving starts a wave; RB does nothing.**
- Trailer Mode: **LB held = 0.25×, RB toggles 0.5×, neither waves** (F still waves there).
- LB is "previous" in menus and the map, which own the input then (no wave). Docs: `TRAILER_MODE.md` updated.

## Part C — cars on every course

Both cars (Street Classic, Longroof GT) can now be chosen on all eight courses, for the player and for AI; motorcycle
and ATV availability unchanged; records stay per vehicle. `CarAccess` is where a shortcut or a course would be closed
to cars; **nothing needed closing**, so both its lists are empty (no sign or minimap entry was needed).

Evidence:
- **Every shortcut ridden** by both cars and the motorcycle (simple follower at the shortcut's own speed):
  [partC-shortcut-rides.tsv](Lists/partC-shortcut-rides.tsv). All through for cars, including the **Echo Cave**
  (half-width 4.2 m) and the narrow Backyard trails (Tree-Top 1.65 m, Logging Ridge 2.4 m, Storm Drain 2.65 m),
  except Laurel Switchbacks and the Reverse Downhill Ridge Cut, which the motorcycle fails in the same places
  (follower limits). Laurel at 12–15 m/s: Street Classic through.
- **Car AI forced through every AI shortcut** (the race driver steered into it):
  [partC-shortcut-ai-forced.tsv](Lists/partC-shortcut-ai-forced.tsv), controls in
  [partC-shortcut-ai-forced-controls.tsv](Lists/partC-shortcut-ai-forced-controls.tsv). Through: Granite Creek Cut,
  Echo Cave, Fern Gully, Summit Traverse, Tree-Top Trail, Storm Drain, Logging Ridge (0–1 resets); Laurel Switchbacks
  1 reset (the motorcycle needs 1 too); Abandoned Cabin Jump 2 resets (the bike controls fell earlier on the course).
  McFadden Cut's entrance plane is 58 m inside the trail right after the start, beyond this setup; the AI cars took it
  on their own in the races. The Reverse Downhill Ridge Cut is not AI-validated (no AI takes it).
- **Resets (0.68 rule)**: a car put down 2.5 m outside every shortcut and the main road at 4 stations, in all 8 scenes,
  both cars: **80/80 respawned onto the course**, upright (4 came to rest tilted 0.82–0.88 on a banked or sloping
  spot, one 1.1 m off a trail edge). [partC-resets.tsv](Lists/partC-resets.tsv)
- **Races**: one per course, direction and car, the player's car on the AI driver, 3 AI including **two car AI**.

| Course | Direction | Street Classic | Longroof GT | Notes |
| --- | --- | --- | --- | --- |
| Street Loop | Forward | pass | pass | car AI took all four shortcuts |
| Street Loop | Reverse | pass | pass | AI car took Laurel Switchbacks (1 reset) |
| Forest Loop | Forward | pass | pass | cars through the Echo Cave (player and 3 AI) |
| Forest Loop | Reverse | pass | pass | McFadden Cut and Fern Gully by cars |
| Mountain Loop | Forward | pass | pass | |
| Mountain Loop | Reverse | pass | pass | rerun after the Part A geometry |
| Dan's Backyard | Forward | pass (cars finish, see note) | pass (see note) | AI missed 1 gate here in some runs |
| Dan's Backyard | Reverse | pass | pass (see note) | |

All races: everyone finished, nobody stuck for good, lap times recorded; 0 missed gates on Street, Forest and Mountain.
**Note — Dan's Backyard:** the AI driver misses Backyard gates now and then for every vehicle: the bikes-only control
field in the same build missed 1–3 gates per race (the motorcycle player autopilot too). This is the known Backyard
autopilot limitation recorded since 0.76 (see Report076 / Report078), not a car problem.
[partC-races.tsv](Lists/partC-races.tsv) (3× speed), [partC-races-backyard-1x.tsv](Lists/partC-races-backyard-1x.tsv)

**Shortcuts closed to cars:** none. **Courses still restricted:** none.

## What changed, and what did not

- **Scene files changed:** `MountainLoopReverse.unity` only (four new collidable meshes, `Ground_Report080 …`, assets
  in `Assets/Track/Report080/`; nothing existing edited). FreeRoamWorld and every other scene file are unchanged.
- **Runtime (every scene, visual only):** the post sweep (only objects without colliders were removed), the Hwy 92
  paint, the summit dirt look. No collider, route, checkpoint or `CoursePreviews.json` change outside Mountain Loop
  Reverse.
- **Code:** `CarAccess`, `RaceDirector` / `RaceFlow` / `RacePlaylists` / `RaceMenus` (car rule), `RoadDriver` and the
  branch entry credit (closed shortcuts; none closed), `VehicleInput` (LB), `RoadPosts`, `JunctionPaint`,
  `MountainDirt`, `Scenery` (calls them).

## Frame rate

**PASS.** 3840×2160, GTX 1660 Ti, GPU median, `ConditionsBench -conditionsScenery` on a scratch test player
([Bench/scenery.txt](Bench/scenery.txt)); Chrome was open.

| View | 0.80 New | 0.79 New | 0.80 Classic |
| --- | --- | --- | --- |
| Street Day / Night Rain / Night Snow | 6.82 / 7.81 / 7.86 ms | 6.75 / 7.75 / 7.85 ms | 3.80 / 4.60 / 4.58 ms |
| Forest | 6.32 / 7.15 / 7.19 ms | 6.35 / 7.16 / 7.17 ms | 3.30 / 4.10 / 4.09 ms |
| Mountain | 4.11 / 4.32 / 4.31 ms | 4.11 / 4.37 / 4.38 ms | 3.09 / 3.44 / 3.45 ms |
| Woods | 7.05* / 6.77 / 6.79 ms | 6.68 / 6.59 / 6.62 ms | 3.87 / 3.89 / 3.92 ms |
| Summit | 7.49 / 7.66 / 7.69 ms | 8.36 / 8.41 / 8.43 ms | 4.76 / 4.91 / 4.93 ms |

**Worst view 7.86 ms = 127 fps** (0.79: summit 8.43 ms = 119 fps). A first bench put the summit at 9.95 ms
(100 fps): the summit roads now use the terrain shader, and the terrain under them was shaded too. They are now drawn
before the terrain (same material, earlier queue), so the hidden terrain fails the depth test; the summit got faster
than 0.79. *The woods Day run was interrupted when the player window lost focus.
