# 0.78.0-review1 — World scenery upgrade and rider gestures: validation

Round of 2026-10-04 (Claude Code). Scope: PROJECT_TODO.md "CURRENT — World scenery upgrade + rider gestures", Parts A–B.
Checks: `Assets/Scripts/Report078Checks.cs` (play mode, muted, isolated save) run with `Tools/Report078/Run-Play.ps1`;
the 0.77 Trailer Mode checks (`Report077Checks`) re-run on the new scenery; 4K frame times with
`Tools/Report078/Bench-Scenery.ps1` (`ConditionsBench -conditionsScenery`). Evidence: [Scenery/](Scenery/),
[Gestures/](Gestures/), [Bench/](Bench/).

## Part B — Rider gestures

| Item | Result |
|---|---|
| Arm rig | **PASS.** `Tools/Blender/rider.py` now exports each arm as upper arm / forearm / hand with their origins on the shoulder, elbow and wrist (fifth name key U/L/H + side; 28 more triangles per rider). `RiderArms` hangs them on a three-joint chain per side; a two-bone IK blended in rotation space moves them; at rest nothing changes. Every pose (Moto, ATV, Car), both bodies, every shirt type (T-shirt, Long sleeve, Jacket checked across the runs). |
| Fist wave, player | **PASS** on all four vehicles × both bodies × chase and first person (16 checks): the button starts it (emulated controller RB; the batch editor gives keyboard input only to a focused Game view, so F is checked as a binding: `<Keyboard>/f`, `<Gamepad>/rightShoulder`). Left fist raised 0.5 m beside the head on the bikes, out of the driver's window in the cars (fist at x ≈ −1.0 m, outside the body); in first person the fist comes up ahead of the face and is in the camera view; the right hand never moves (0.000 m); the gesture ends after 1.7 s; controls stay on. A second press during the gesture / cooldown is ignored (2.2 s cooldown after it). Shown in Settings > Controls ("Driving / Fist wave") and the Trailer Mode help (F). In Trailer Mode RB keeps its 0.5× toggle (use F). |
| AI fist wave | **PASS.** Street Loop race with 3 AI: the player rammed EMBER from behind; EMBER shook a fist (first attempt). Rule: hit by another vehicle that was doing the hitting with a knock of ≥ 1.2 m/s (or ≥ 4 m/s closing speed), 75 % chance, after 0.35–0.85 s, aimed toward the offender, not during its own wipe-out, then 7–11 s before it can react again; also after recovering from a wipe-out someone caused within 10 s. [Gestures/shunt-ai-fist.jpg](Gestures/shunt-ai-fist.jpg) |
| Victory | **PASS.** Player win (Street Loop, motorcycle, test pilot): only YOU celebrated (both fists up, pumped three times, no-handed; the bike coasts upright as before). AI win (player waiting at the start): only EMBER celebrated. Counts per race: winner 1, everyone else 0. Cars pump a fist out of the window. The results panel covers part of the player's celebration; the camera flow was not redesigned (the TODO allowed framing only if no redesign was needed). |
| Classic models | **PASS.** No arm rig; the button, `Wave()` and `Celebrate()` do nothing. |
| No effect on driving | **PASS.** The same 6 s of scripted driving from the same pose, three runs: two without gestures end 0.0000 m / 0.000° / 0.0000 m/s apart, the third with the button spammed and a celebration ends 0.0000 m / 0.000° / 0.0000 m/s from them. Gestures only move visual transforms. |

## Part A — World scenery (visual only)

How it is built: nothing in any scene file was changed. `SceneryWorld` (attached by `RaceFlow`) builds the new scenery at
load when **Settings > Display > Scenery** is New (default, saved as `classicScenery`), hides the old renderers with
`forceRenderingOff` (their colliders untouched) and draws the new kit; Classic shows the old renderers again.

| Family | What changed |
|---|---|
| Trees and bushes | Blender kit `Tools/Blender/scenery_trees.py` (render sheet [Scenery/Kit/trees-sheet.png](Scenery/Kit/trees-sheet.png), 3 passes): Round, Oak, Maple, Pine, Poplar, Young, Bush, Shrub, each with a near and a simplified far version; faceted clump crowns with visible branches, tapered trunks with root flare. `SceneryTrees` reads the old forest meshes, finds every crown and bark box, matches crowns to their trunk collider (or the bark box of a visual-only tree) and fits a tree: trunk exactly over the trunk collider (same place, height, width), crown inside the old crown's bounds. Species by crown shape, height and patch (about 18 % Round, 27 % Oak, 18 % Maple, 23 % Pine, 14 % Poplar in FreeRoamWorld). `Racer/Foliage` shader: wind sway (stronger in rain), patch tint, snow on crowns, the world's lighting; near trees cast shadows. |
| Rocks | Rocks, boulders, outcrops, cairns and fire-ring stones get a faceted, weathered mesh swapped in place: the old shape pushed outward only (≤ 12 cm), so it always covers its collider. Cave, vault and portal rock is left as it is. |
| Buildings | `SceneryBuildings`: 43 houses and shops redesigned from their own colliders (walls on the wall collider, each roof drawn on its roof collider's own shape, chimney, flat roof, parapets and steps on theirs) with siding or brick courses, corner boards, base trim, framed windows with sills, mullions and shutters, doors, fascias, gutters, downspouts, ridge caps, porch brackets, shop fascias and striped awnings, and windows that light at night. Styles vary by name (9 wall colours, 6 roofs, shutters, door colours). **Dan's house** keeps its own model; only its windows light at night. **Kyle's house** (the "Friend across street" building; the archive says the friend is Kyle) keeps its model and gets gutters, downspouts, ridge cap, corner boards and lit windows. Fox Gully (the drive-through house) is left as it is. Shop names stay. The town's merged batches are replaced by copies without the redesigned buildings' triangles. |
| Props | Boards, posts, rails, mailboxes, barricades and other box-built props get bevelled edges inside the same box (mesh swapped in place, so breakable props break and return exactly as before; sign text unchanged). The fence sections (one merged mesh each) are unchanged. There are no stepping stones in the world. |
| Ground and roads | `SurfaceLighting.hlsl`, only when New: grass in tonal patches with a fine blade speckle near the camera, dirt and gravel with pebbles, asphalt aggregate with sparse hairline cracks and darker repairs; `MarkedGround`: darker wheel tracks in each lane, a slightly crumbled edge, crisper paint. Classic is byte-for-byte the old shader path. `SceneryGround`: grass tufts, a few flowers and leaf litter under crowns within 54 m of the camera, on grass only (never on roads, trails or driveways), no collision. |
| Water edges | `SceneryWater`: every lake and creek gets a bank strip laid on the ground from just inside the water to 1.8 m out, coloured from wet mud to the ground's own colour, plus reed clumps with cattails and pebbles. Pools and the storm drain are excluded; water and Snow ice untouched. |

### Collision and races unchanged

| Check | Result |
|---|---|
| Colliders, every scene | **PASS** in all nine scenes: every static collider (name, type, enabled, trigger, layer, world bounds to the mm) identical with New, Classic and New again — FreeRoamWorld 13,602, Street Loop F 13,761, Street Loop R 13,604, Lake Woods 13,521, Forest Loop R 13,323, Mountain Loop 13,349, Mountain Loop R 13,277, Backyard F 13,681, Backyard R 13,602. No scene, route or collider file is in the change set. Classic shows every old renderer again. |
| Races, one per course family | Street Loop (motorcycle), Mountain Loop (motorcycle) and Forest Loop Reverse (ATV), 3 AI each, New then Classic (final run): everyone finished, **0 missed gates** in both; times within normal race-to-race spread (Street YOU 2:07.45 / 2:07.73, Mountain YOU 1:57.99 / 1:57.60). Backyard Forward: the test autopilot misses Backyard gates with New **and** Classic (known since 0.76; the AI code and scene are unchanged) — not a scenery effect. |
| Free Roam giant jump | Summit Homeward from rest: New 242.5 m in 8.00 s, Classic 242.5 m in 8.00 s (identical; 0.77: 242.5 m). |
| Trailer Mode cameras, conditions | **PASS** (the 0.77 checks re-run with New): every trailer camera placed outside geometry with ground below and a clear view; 4K captures at Day, Night/Rain and Dusk/Snow without HUD; lightning strike on demand, clock, moon; settings.json byte-identical after a session. |
| Switch | Scenery New / Classic / New in play: no reload needed, colliders identical each time (above). |

### Found and fixed before delivery

- One terrain tile in the race scenes (CR117 outer ravine) shares the forest's vegetation shading; the first tree reader took it for foliage, hid it and turned its triangles into "crowns" (a giant crown beside the Mountain Loop start). The reader now never takes anything with a collider or named Ground. Every scene's replaced-renderer list was then checked: only forest batches, mountain woods, the Backyard Reverse undergrowth (now bushes; it never had collision) and the town's building batches.
- Terrain colours were copied once per sample (shores 3.5 s at load); now read once per mesh.

### Evidence

Classic | New pairs from fixed viewpoints, Day, Night and Day/Snow: [Scenery/Pairs/](Scenery/Pairs/) — every course scene
from the start, and in FreeRoamWorld: Dan's house, Kyle's house, House 3, the businesses, the dump and gullies, the lake,
the mountain summit and a forest road.

### Frame rate (3840×2160, GTX 1660 Ti, GPU median)

GPU median at 3840×2160 ([Bench/scenery.txt](Bench/scenery.txt); full-screen shots in [Bench/](Bench/)). Classic is the old
scenery in the same build (= 0.77: Street Day 3.77 ms here vs 3.72 ms in 0.75/0.77's bench). Views: the three 0.71 views,
a view inside the densest woods and the mountain summit (both FreeRoamWorld).

| View | Day/Clear Classic → New | Night/Rain (strikes) Classic → New | Night/Snow Classic → New |
|---|---|---|---|
| Street | 3.77 → 6.81 ms (147 fps) | 4.56 → 7.78 ms (129 fps) | 4.61 → 7.82 ms (128 fps) |
| Forest | 3.33 → 6.33 ms (158 fps) | 4.12 → 7.15 ms (140 fps) | 4.12 → 7.14 ms (140 fps) |
| Mountain | 3.09 → 4.07 ms (246 fps) | 3.41 → 4.30 ms (233 fps) | 3.42 → 4.30 ms (232 fps) |
| Woods | 3.54 → 6.10 ms (164 fps) | 3.63 → 6.08 ms (164 fps) | 3.63 → 6.06 ms (165 fps) |
| Summit | 5.58 → 8.35 ms (120 fps) | 5.74 → 8.54 ms (117 fps) | 5.73 → 8.57 ms (117 fps) |

**PASS: worst view 8.57 ms = 117 fps (95th percentile 9.08 ms = 110 fps), above 100 fps everywhere.** The new scenery
costs about 1–3 ms of GPU time. Reduced to get there (first pass: [Bench/scenery-first-pass.txt](Bench/scenery-first-pass.txt),
worst 8.93 ms): far trees draw no trunk beyond 450 m (their crowns hide it), trees are drawn to 1,100 m (was 1,400 m), and the
detailed, shadow-casting trees are those within 90 m (was 110 m). Tree draw calls: 160–220 per frame.

Build time of the new scenery at scene load (editor): 0.7–1.3 s per scene (trees 0.5–1.1 s).
