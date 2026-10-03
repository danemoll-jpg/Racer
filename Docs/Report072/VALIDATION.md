# 0.72.0-review1 — validation

Round: PROJECT_TODO "CURRENT — Time of day and weather + AI lost after jumps + 1 report". Targeted checks (rule 11). Evidence files are in this folder; tools in `Tools/Report072/`, play-mode checks in `Assets/Scripts/Report072Checks.cs` and `Report072CondChecks.cs`, look evidence runner `Assets/Scripts/ConditionsBench.cs` (command-line opt-in only).

| Item | Where | Disposition |
|---|---|---|
| Part A BUG-001 | Street Loop Forward, Free Roam (692.07, 79.81, 92.50), the torn dirt patch | **PASS.** One clean, connected, collidable trail; spiked pale bank beside it smoothed; 6/6 rides. |
| Part A box perimeter | 0.71 reshaped box (launch frame s 316–545, \|x\| ≤ 50) + 14 m, all six scenes | **PASS.** Folds 0, terrain through the trail 0, visible-vs-collision mismatches 0. Remaining scan hits are natural slopes / creases (listed below). |
| Part A 5A.6 | Summit Homeward Flight landing and run-out | **PASS with a note.** 6 flights land where 0.71 landed, same distances, upright, no reset, open run-out; lean during the coasting run-out slightly larger in some runs. |
| Part B | AI after big jumps, Mountain Loop Forward and Reverse | see [AI_JUMPS.md](AI_JUMPS.md) |
| Part C | Time of day and weather | see [LOOK.md](LOOK.md) and the Part C section below |

## Part A — Free Roam terrain tear (BUG-001)

**What it was** (not the terrain): the Summit Homeward **supported-return ribbon** (CR-103 `Ground_CR103 supported return`, re-seated by 0.71). It is a 14 m strip with vertices only at its two edges. At its hairpin (around (692, 80, 100)) the inner edge doubles back, so quads fold into bow-ties (back faces on top), and the two legs overlap each other for ~25 m at different heights (up to 0.5 m). That is the overlapping grey slivers, the dark crack and the raw edge in Dan's view. The "pale faceted terrain poking up" beside it was a spiked terrain bank (up to ~84 m, then a 4–5 m cliff to the forest floor at x 677–694, z 109.5–119.5), hidden by the catch mound until 0.71, plus four stray crown fragments of the batched trees. Scan before: `scan/scan-before-StreetLoopGreybox.txt`; views `views/BUG-001-before.png`, `views/TOP-hairpin-before.png`, `views/SPIKE-before.png`.

**Fix** (all six Street-Loop-world scenes: StreetLoopGreybox, StreetLoopReverse, LakeWoods, ForestLoopReverse, DansBackyardForward, DansBackyardReverse; log `author-notes.txt`):
1. The ribbon is rebuilt as the union of the same 14 m strip around its own centre line, on a 0.5 m grid with an interpolated outline (no stair-steps), ends on the original end lines. Where 0.71 was broken (the hairpin and the overlap of the legs) the surface follows the ground (smoothed 1 m, never below it, + 4 cm); inside the protected Homeward landing / run-out corridor (launch \|x\| < 12, s 432–570) it keeps the 0.71 driving surface exactly; 10 m blend between. The collider is the top surface only; a 35 cm skirt tucks the visible edge under the ground (visual only — a collidable skirt was measured to launch a landing vehicle, see below). Terrain-tile vertices under the rebuilt part go 12 cm under it.
2. The spiked bank is replaced by the smooth surface spanning the ground around it (45–180 nodes per scene); the two trees standing in it move with the ground (trunk collider + their batched pieces); the trail edge there follows the lowered ground.
3. Tree pieces 0.71 left behind: 6 whole crowns whose trunks 0.71 removed (run-out corridor, s 662–686) are removed; 6 crown lobes that did not move with their re-grounded trunks move with them; 4–6 stray crown fragments (≤ 12 triangles, no trunk) beside the trail are removed.
Jump approach, lip and the open landing/run-out are as delivered.

**Results**
- Scan of the whole box + 14 m, all six scenes (`scan/scan-*-after.txt`): folds 0, terrain through the trail 0, visible-vs-collision > 25 cm 0. The remaining hits are the steep natural slopes and creases at the box edge (s 317 x 40; x +54 s 395; x −49 s 408; x 49 s 502; x −55 s 520); they predate 0.71 or are gentle creases, render as ordinary slopes, and are not tears — left (rule 3).
- Rides (`checks/partA-ribbon-rides.txt`): moto and ATV along the trail centre both ways through the hairpin, and across Dan's position: 6/6 complete, no stop, no reset, min up ≥ 0.93, largest contact impulse ≤ 82.
- Views after: `views/BUG-001-after.png`, `TOP-hairpin-after.png`, `LOW-hairpin-after.png`, `SPIKE-after.png`, `HAIRPIN-outer-edge-after.png`, `RUNOUT-after.png`.

**5A.6 — Summit Homeward Flight** (`checks/partA-homeward-after.txt`; 0.71 reference `Docs/Report071/checks/partA-homeward-after.txt`; the same run with the 0.71 ribbon collider put back, `checks/partA-homeward-0.71-surfaces.txt`, reproduces 0.71 exactly): 3 moto + 3 ATV full-throttle flights: take-off, flight distance (moto 242.5–244.0 m, ATV 223.5–224.6 m) and touchdown points equal 0.71; all upright, no reset, no obstacle, stop on the open run-out (83–111 m). Min up after landing 0.67–0.98 (0.71: 0.96–0.98): the coasting run-out over the trail leans more in some runs; no roll-over or wipe-out.
- Found and fixed during the round: a collidable skirt on the trail edge launched a landing moto ~5 m and rolled it (min up −1.00); a 1.5 cm edge taper formed a small kicker at the first leg's end line. Both removed; the run-out surface inside the corridor is the 0.71 one.
- Process note: one author pass (a wrong mesh-name check) overwrote the StreetLoopGreybox ribbon and two tile meshes before the scene was saved; they were restored from git HEAD and the deterministic passes replayed, matching the first run exactly (12,368 vertices, 638 tile vertices lowered, spike 45 nodes).

## Part B — AI riders lost after big jumps (Mountain Loop Forward and Reverse)

Full account: [AI_JUMPS.md](AI_JUMPS.md). Summary:
- **Cause:** healthy rivals were reset in mid-air. The off-route detector counted a 5.6–6.4 s flight as "no progress" and recovered the rider about one second before touchdown, then the old earned-anchor recovery sent it 400–670 m (up to 1,062 m) back, off the minimap. Second cause: rivals had no fall-through failsafe (`VehicleRespawn` is disabled on rival clones), so a rival that fell through the world fell for ever. Real jump failures on top: Homeward take-offs at 31.4 m/s overshoot onto the upper deck; South Face take-offs below ~37 m/s land short; the Homeward run-up U-turn could send a rider off the deck.
- **Before (0.71 code), one 3-lap race each:** Eastbound Gully 6/57, Homeward 6/9, South Face 1/141, Westbound Gully 0/79 (attempts include retries after each reset); 273 recoveries (median 409 m); Reverse: all three rivals DNF; Forward: BLUE DNF.
- **After (final code), two 3-lap races each direction:** Eastbound Gully 17/18 (94%), Homeward 18/18, South Face 12/12, Westbound Gully 18/18 — **PASS (≥ 9/10 every jump)**. Recoveries: 6 in 4 races, all to the nearest usable point (0–76 m). Every rival finished every race and stayed on the minimap (only DNF hides a rival).
- **Minimap:** hides a rival only when it is DNF or outside the map window; recovering rivals and `AiFinishEstimate`-classified rivals stay drawn. **PASS.**
- Race results, AI difficulty elsewhere and the `AiFinishEstimate` rules are unchanged. No geometry changed (per-flight AI pedal data only in the two Mountain scenes).

## Part C — Time of day and weather

Full account and frame-rate table: [LOOK.md](LOOK.md); screenshots `Look/`.

| Check | Result |
|---|---|
| Race setup rows Time of Day / Weather, defaults Day / Clear; controller (D-pad + A), keyboard (arrows + Space), mouse; Free Roam Weather row | **PASS** (`checks/partC-street-menu-nightrain-cycle-debug.txt`) |
| Options persist across relaunch (settings.json read back by a fresh save) | **PASS** |
| Menus and garage stay Clear Day; back to Clear Day after a race | **PASS** |
| Night / Rain race, Street Loop Forward (player driven by the AI driver, normal AI field): start, 14 checkpoint changes, 0 missed gates, finish, results, record saved in the unchanged category; lamps on 24 vehicles | **PASS** |
| Dusk / Snow race, Forest Loop Forward: start, checkpoints, finish, results, record saved; lamps at half | **PASS** (`checks/partC-forest-dusksnow.txt`; the autopilot needed one recovery on the way, as on Night/Rain) |
| Free Roam: starts 08:00, clock in the HUD line, dusk → night 18:30 → 21:00 (Dusk → Night), lamps on at 18:40 | **PASS** (`checks/partC-street-menu-nightrain-cycle-debug.txt`, `partC-cycle.csv`) |
| Free Roam light: no pops | **PASS (explained).** Largest change 0.33° and 0.045 intensity per game minute — the steady sunset fade around 19:40 (sun and moon move 0.25° per game minute; the light is off within 2° of the horizon, so the swap between sun and moon is invisible). The check's first threshold (0.03 / game minute) was tighter than a normal sunset and was widened to 0.06; final run PASS. |
| No rain or snow inside caves / tunnels | **PASS** — inside the Forest cave at night in rain: covered, 0 falling particles (`Look/conditions-run.txt`) |
| Debug HUD and bug report Markdown / JSON record the conditions | **PASS** — a report written during a Night / Snow race carries 'Conditions: Night / Snow' in the Markdown and `conditions` in the JSON |
| Frame rate 3840×2160, all four measured combinations ≥ 60 fps | **PASS** — see LOOK.md |
