# 0.79.0-review1 — validation

Scope: PROJECT_TODO.md section "CURRENT — 0.78 scenery fixes …", Parts A–H. Done in the order the TODO set: D, E, F, then G and
H, then A, B and C. Evidence was captured in editor play mode, muted, with an isolated save (`Assets/Scripts/Report079Checks.cs`;
runners in `Tools/Report079/`). Screenshots are in [Shots/](Shots/), and the per-scene lists in [Lists/](Lists/).

No terrain, road, collider or route was changed in any course scene:
- The scene files are byte-identical to 0.78 (`git diff 1f2de343 -- Assets/Scenes` is empty), and so is `Resources/WorldMaps/CoursePreviews.json`.
- In every scene, every static collider is identical with Scenery New, Classic and New again (13,283–13,763 colliders per scene).
- The only collider change is in `FreeRoamWorld`: the trunks standing on drivable surfaces are removed there (Part E.3).

## Part D — buildings stand on the ground: PASS

- **Cause:** 0.78 draws each generated building from its wall collider, whose floor is level. On sloping ground the floor
  sits at the high side, so the downhill side hung in the air. The old model's foundation was in the town's merged batches,
  which 0.78 trims around every redesigned building.
- **Fix (universal, `SceneryBuildings`):**
  - Every generated building stands on a foundation from its floor down to 0.3 m below the lowest ground under its
    footprint. The ground is sampled at every corner and every metre along the edges, plus the centre.
  - The foundation is brick under brick houses and coursed concrete block elsewhere, 3 cm proud of the wall and within
    the footprint.
  - Entrance steps stand on a block down to the ground. An exterior chimney and the four downspouts reach the ground.
  - No collider is touched.
- **Proof, for every building in every scene:**
  - The check walks round the base 1.5 cm outside the walls, every 0.5 m (84–196 points per building).
  - At each point it finds the lowest drawn surface above the ground (an upward ray against a temporary copy of the
    drawn meshes) and compares it with the ground there.
  - **Result: 47/47 buildings with a 0.00 m largest gap in all nine scenes.** This includes Dan's house, Kyle's house
    ("Friend across street") and Fox Gully, which keep their own models, measured per wall collider.
  - Lists: `Lists/grounding-buildings-<scene>.txt`.
- **The four reported houses:**
  - (576.5, −414.3) = residence at 583, −429: floor was 1.85 m above the low side.
  - (491.5, −589.9) = house behind the southern hairpin: 1.89 m.
  - (396.7, −501.5) = residence at 385, −506: 2.46 m.
  - (256.7, −413.8) = residence at 244, −412.5: 2.50 m.
  - The worst in the world was 5.30 m (residence at 88, −341).
  - Before / after: `Shots/before-*.jpg` (0.78 code) and `Shots/after-*-new.jpg` / `-classic.jpg`.
- **Other ground-standing scenery (D.6):**
  - Posts are judged at the lowest ground under their corners. Two real cases were found and fixed:
    - the House 3 mailbox post, 6 cm on a slope;
    - the Mountain summit "05 return" sign post, 0.9 m.
    A post now gets its own drawn mesh lengthened to the ground (up to 1.5 m; visual only).
  - Rocks are judged for daylight under their drawn underside at nine points, and boulders for being half buried.
  - Clue cairns whose ground was raised after they were placed are drawn re-seated, stones together (visual only; the
    stones have no collider). One cairn by Hwy 92 was half under the later verge.
  - **Result: floating 0, half buried 0 in every scene** (73–127 posts and rocks per scene).
  - Not counted:
    - outcrops and fire-ring stones, which are set into the ground by design;
    - posts fixed to a structure (the shortcut railing on its ramp).

## Part E — no trees on drivable surfaces: PASS

- **How the driveway trees got there (BUG-002, 454, −29.9):**
  - They were always there as trees: two "World cleanup additional woodland / Roadside woodland trunk" colliders stand on
    the black driveway, at (447.4, −31.7) and (434.0, −30.6).
  - In Classic their trunks were invisible (only the canopy was drawn).
  - 0.78 drew a trunk on every trunk collider, which made them appear. So it was not a crown-only clump.
- **Rule (all scenes, `SceneryTrees`).** What stands under a tree's base within 1.5 m is drivable if it is:
  - paved terrain (the road colour);
  - an asphalt, gravel or concrete surface;
  - a road, driveway, parking, lane or trail mesh;
  - dirt terrain inside a trail's width.
  Verges and shoulders are not.
- **Results, per scene (found → removed or hidden):**
  - FreeRoamWorld 12 → 12. Removed properly, collider too, including the two driveway trunks, the Jamerson decorative-road
    trunk and nine on the Lake shore trail.
  - Street Loop F 9 → 9, Street Loop R 10 → 10, Lake Woods 22 → 22, Forest Loop R 23 → 23.
  - Backyard F 11 → 11, Backyard R 12 → 11, Mountain F 22 → 22, Mountain R 22 → 22.
  - **Total 163 found, 162 fixed.**
  - In the course scenes, the collider stays and only the visual goes, when no race line or shortcut of that course is
    within 12 m. Otherwise the tree stays and is reported.
  - **Kept and reported (section 5A):** one trunk collider at (208.0, 72.35, 89.8) on the Backyard Reverse trail, 2.3 m
    from its race line (in FreeRoamWorld it is removed).
- **Ground detail:** grass tufts, flowers and leaf litter only go on grass-coloured terrain, and now also never where the
  drivable test says road. 984–2,457 placed per scene during the check; 0 on a drivable surface.
- Lists: `Lists/trees-drivable-<scene>.txt`.

## Part F — one asphalt grey: PASS

- **Cause:** the terrain's painted roads are drawn by the ground shader (Racer/MarkedGround) in the vertex colour
  0.24 / 0.25 / 0.26. The separate paved meshes use URP Lit materials of their own: Hwy 92's continuous four-lane pieces
  ("CR113 highway asphalt"), the decorative road continuations ("Asphalt"), the roadworks takeoff ("Raised roadworks
  asphalt") and "Reverse asphalt". They light differently (darker and bluer), with no wet darkening or road detail, so
  every join showed a seam. The driveway's "Local black asphalt" was 0.065: near black.
- **Fix (`SceneryPaving`, `Assets/Scenery/Paved.shader`):** with Scenery New those renderers use the ground shader's own
  road lighting in the terrain road colour. Driveways use a slightly lighter grey of the same family (0.27 / 0.275 / 0.29).
  - The colour is passed raw, like the vertex colours (a Color property would be linearised).
  - Lane lines and edges are separate meshes and unchanged; gravel and dirt are unchanged. Classic is unchanged.
  - 9 renderers in FreeRoamWorld.
- **Measured** (the mean pixel colour of the road 6 m either side of Dan's seam at X 163.6 on Hwy 92):
  - New: 0–1/255 apart at Day, Night and Rain.
  - Classic: 12–54/255 apart.
  - Driveway against the road next to it: 5 (Day), 1 (Night), 7 (Rain) /255; before, 87 / 46 / 50.
  - Shots: `Shots/roads-*`.

## Part G — street-name signs: PASS

- **Signs at three junctions**, in all nine scenes:
  - S Cherokee Ln / Hwy 92: post at (326.5, 549.5), 7.3 m from the nearest race line.
  - Trickum Rd / Hwy 92: post at (−615.0, 531.0), 9.9 m.
  - S Cherokee Ln / Trickum Rd: post at (−625.5, −543.8), 11.9 m.
- **Placement** (`StreetSigns`, built at load from the shared course route data):
  - The junction is found from the Street Loop route, which runs on exactly these three roads, with Hwy 92 on its own
    through-route centreline.
  - The post goes on a corner of the side road, 2.5 m beyond the paved edge measured on the spot (stepped further out
    while still on pavement).
  - It must be off every drivable surface, at least 6.5 m from every course's main line and shortcuts, and clear of
    anything standing within 1.5 m.
  - No collider.
- **The sign:** a 3.5 m steel post with two green blades (white border, white lettering on both faces), each blade
  parallel to the road it names, with capitals about 0.26 m tall. The lettering stays bright at night (as with
  retroreflective signs).
- **Other paved public road:** the decorative "Jamerson Rd west" continuation, running west from the S Cherokee Ln /
  Trickum Rd corner to about (−1000, −550), centre about (−800, −550). It is none of the three, so it has no sign, as
  instructed. Driveways and trails have none either.
- Shots, day and night, approach and close: `Shots/sign-*`.

## Part H — cameras reachable from the screen: PASS

- **Normal play:**
  - `V / X: camera — <view>` appears above the speedometer for 4 s of shown time when driving starts and whenever the
    view changes, then fades. Checked at a race start, at a Free Roam start and on X.
  - The pause menu (races and Free Roam) has **Camera view: <view> (V / X)**, which cycles the view.
  - Settings > Controls lists "Camera / Change view (or pause menu)".
- **Trailer / Photo Mode:**
  - It is **TRAILER / PHOTO MODE (F8)** in the race pause menu, the Free Roam menu and the main menu.
  - Its page is titled "Trailer / Photo Mode" and has a "Controls panel" row.
- **The controls panel** (`TrailerMode.Panel.cs`):
  - Contents: the nine cameras by number and name, with the current one highlighted; then the camera action,
    Auto hold, slow motion, hide, guides, screenshot, and in Free Roam the time, clock ±1 h, clock pause, weather, moon
    and lightning. Each entry shows its key, or its controller button.
  - Checked: shown on entry; clicking "3 Side tracking" selected it; clicking Weather changed it; controller B gave the
    panel the focus on "1 Chase", and A used it; B returned to driving.
  - L3 / H hid the panel and the HUD together. A key while hidden showed `H: show controls` briefly.
  - A P / F12 screenshot taken with the panel open renders the camera alone (`Shots/trailer-screenshot-with-panel-open.jpg`).
  - Every 0.77 binding is unchanged (`Docs/TrailerMode/TRAILER_MODE.md` updated).

## Parts A–C — Free Roam screen and minimap: PASS

- **Clock:**
  - Day and time are in their own box at the top left (Day 19 px, time 38 px bold, white with an outline, on a 58 %
    dark backing), with a moon-phase disc.
  - 4K shots at Day, Night, Snow, Dawn and Dusk / Rain: `Shots/hud-roam-normal-*.jpg`.
  - Measured clear of the minimap and the speedometer. It hides with the HUD in Trailer Mode.
- **Normal driving:** the only Free Roam text is the clock (plus the speedometer and minimap).
  - At an activity start: its name and how to start it. Being there also makes it the pause menu's selected activity, so
    the instruction starts that one.
  - During an attempt: the existing timer line. Afterwards: the result, for a few seconds.
  - An acorn found: `WOODLAND ACORNS n/24` for 5 s. The menu hint shows for 8 s after Free Roam begins.
  - The nearest-activity / distance line is gone; it remains on the map and in the pause menu.
- **Minimap:**
  - The race minimap now also draws in Free Roam: the world's roads (paved, light grey) and trails (tan) from the scene's
    own road centrelines, the activity sites, and the waypoint (held at the window's edge when further away).
  - **J** / controller **B** toggles it while driving. B is free while driving and not a Trailer Mode key; it is ignored
    when it closes a menu or the map, and in Trailer Mode B works the controls panel.
  - Saved as `roamMinimapHidden`; the default is shown. Checked: B off, saved in the settings file; B on again.
  - Races are unchanged (same routes, legend and rivals).
- **Race HUD:** identical to 0.78 except the camera hint. The 0.77 "View: …" banner is replaced by the hint.

## Frame rate (3840×2160, GTX 1660 Ti): PASS

GPU median, measured with `ConditionsBench -conditionsScenery` on a scratch test player ([Bench/scenery.txt](Bench/scenery.txt)).

| View | 0.79 New | 0.78 New | 0.79 Classic |
| --- | --- | --- | --- |
| Street Day / Night Rain / Night Snow | 6.75 / 7.75 / 7.85 ms | 6.81 / – / 7.82 ms | 3.71 / 4.56 / 4.60 ms |
| Forest | 6.35 / 7.16 / 7.17 ms | 6.33 / – / 7.14 ms | 3.32 / 4.13 / 4.14 ms |
| Mountain | 4.11 / 4.37 / 4.38 ms | 4.07 / – / 4.30 ms | 3.11 / 3.44 / 3.45 ms |
| Woods | 6.68 / 6.59 / 6.62 ms | – | 4.10 / 4.09 / 4.12 ms |
| **Summit (worst)** | **8.36 / 8.41 / 8.43 ms = 119 fps** | 8.57 ms | 5.58 / 5.75 / 5.77 ms |

A first run was taken while Chrome and OBS were using the GPU; it read about a third slower for New and Classic alike
(kept as `Bench/scenery-while-chrome-obs-ran.txt`). Dan closed them and the table above is the clean run.

## Scenery build time

1.2–1.5 s per scene with New, as in 0.78. The ground tests use non-allocating raycasts.
