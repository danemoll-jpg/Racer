# 0.76.0-review1 — validation

Scope: PROJECT_TODO.md "CURRENT — Restore the races, one dedicated Free Roam world, audible storms, garage screens".
Checks were run in the editor (muted, isolated save; for progress checks the isolated save was a copy of Dan's own save) and
in Windows test players built from this source (listener at −60 dB, isolated saves). Tools: `Tools/Report076/`, play
checks `Assets/Scripts/Report076Checks.cs`, storm evidence `Assets/Scripts/StormProbe.cs` (`-stormProbe`), garage
evidence `Assets/Scripts/GarageShots.cs` (`-garageShots`), route export `Assets/Scripts/Editor/CourseRouteExport.cs`.

## Part A — every race back to its 0.73 state around the House 3 driveway — PASS

| Item | Disposition |
|---|---|
| Restore method | Document-by-document restore of the 8 course scenes to 0.73 (`a877a39d`; 0.72 `859d068a` has the same scenes) with `Tools/Report076/restore_scenes.py`: every scene document reverts to 0.73 unless it belongs to a kept 0.74 fix. The 23 shared mesh assets that 0.74 BUG-003 edited were checked out from 0.73. |
| Kept 0.74 fixes | BUG-001 tree (Street Loop Forward), BUG-002 diamond sign removed (all 8), Mountain Forward BUG-004/005 patches and trunk, the 0.74 Part C cave in Mountain F/R. |
| Proof: scenes | After the restore each course scene differs from 0.73 only by those fixes (`Audit/`, restore log). |
| Proof: everything a race scene loads | Of the 944–1,817 files each course scene references, the only non-script files that differ from 0.73 are the kept fixes' meshes (`StreetLoopGreybox-2-0` tree; Mountain Forward BUG-004/005 terrain). |
| Proof: route data | Regenerated route export vs the previous catalogue: Forest Loop Reverse main route, gates and shortcuts identical point for point (0 differences). |
| Views | House 3 west view in Street Loop Forward is pixel-identical to the 0.74 "before" (0.73) image (mean difference 0.00). Top view shows the straight 0.73 drive, slot and lake again (`Views/BUG-003-*-076.png`, `Views/cmp-top.png`). |
| Forest Loop Reverse lap with the AI field | 1 lap, 3 AI: finished 1:17.6, 0 missed gates, all three AI finished, record saved (the test autopilot needed 2 recoveries). |
| Other races | Street Loop Forward solo, Mountain Loop Forward solo: finished, 0 missed gates, records saved. Backyard Forward / Reverse with 3 AI: all AI finished; the test autopilot (the player car driven by the AI driver) missed 1 gate in each. Both scenes and everything they load are identical to 0.73 and the AI/vehicle code is unchanged since 0.72, so this is the autopilot's line, not a change in the courses. |
| Unused 0.74 assets | 79 orphaned 0.74 meshes deleted (Free-Roam-only tunnel tiles and trees, driveway meshes of the race scenes); the 0.74 Backyard Reverse driveway meshes moved to `Assets/Track/FreeRoamWorld/`. |

## Part B — one dedicated Free Roam world — PASS (with the list below for Dan)

| Item | Disposition |
|---|---|
| `FreeRoamWorld` | New scene, a copy of the 0.75 Dan's Backyard Loop Reverse (dump, gullies, storm drain with lights, flow, rats and Snow ice, the 0.74 winding driveway). Its own copies of the 5 shared meshes the winding driveway needs (`Assets/Track/FreeRoamWorld/`). Course id `free-roam-world`. In the build list. |
| Always loads | Free Roam started from any of the 8 courses loads `FreeRoamWorld`; the player starts at that course's own start, on the ground, no reset; the menu shows that course's name; the course's vehicle rule applies (Street: all four, others: motorcycle / ATV). 8/8 PASS. |
| Leaving | Race Setup, Start Race and Return to Menu go back to the selected course's scene (race page / race starts / menu); Tracks goes to the chosen course. The Free Roam clock is saved on the way out and resumes (8/8 + resume PASS). |
| Race-only things | Gates hidden and grid / barriers off as Free Roam does today; additionally the Backyard races' own direction arrows are hidden here (29 teal Reverse arrows, 20 gold Forward shortcut chevrons). Signs stay. |
| Winding driveway (Free Roam only) | 0.74 moved seven trees off the line with only one of each tree's three canopy lobes: 5 lobes were left floating over the drive (one in the driving line). They now follow their trees. 3 trunkless crowns hanging 8–23 m in the air near the lake end (already in the 0.73 scene, now beside the drive) removed. Nothing left in the driving width. Rides down and up: motorcycle 36.5 / 36.7 s, Street Classic car 36.9 / 37.1 s at 9 m/s, never below 7.7 m/s, no wipe-out or reset. |
| Storm drain / dump / gullies | Tunnel end to end (motorcycle and ATV): complete, no wipe-out or reset, no slowdown (0.3–0.4 s under half pace). Backyard Reverse main line through the dump and gullies (1,256 m): complete. At Dan's 0.74 BUG-001 position: clean ground, no holes (`Views/FRW-bug001-076.png`). At BUG-002: no holes or slivers, no slowdown; the concrete outside of the long culvert is visible there (see "Storm-drain culvert" below). |
| Activities and records | The world has 7 activity sites (Trickum jump, both Trickum / 92 speed traps, fence smash, Fern Creek Leap, High Ridge Drop, Summit Homeward). Each shows the best of its own results and of the same site's results in every course scene where it existed (3–8 earlier keys, read only; nothing in the save is changed); new results are saved under the world. With Dan's save: every personal best ≥ the Street Loop Forward one. |
| Acorns, map, landmarks | Same 24 acorns and ids: 22/24 found, unchanged; explored map cells 4,491 unchanged; 11 landmarks (incl. Campsite, Summit Homeward, Storm drain tunnel). |
| Clock, weather, waypoint, fast travel | Rain in Free Roam: look rain 1.00 with the clock running; waypoint guide; fast travel to the Campsite: PASS. |
| Map: course routes as overlays | The map already drew a course's route from `Resources/WorldMaps/CoursePreviews.json` (exported from the course scenes) and did not load a scene from the map; it was "Tracks" in the menu that loaded another course (and with it, before 0.76, another copy of the world). Now: Track on the map toggles any number of courses ([x] list); main route cyan, shortcuts gold, direction chevrons, start/finish bar; Forest and Mountain routes dashed with the legend "Dashed: race-only route (no road in Free Roam)". Choosing never loads a scene or moves the player (0.000 m); the choice and the shown/hidden state are remembered (checked after leaving Free Roam and starting it from another course). Route data is regenerated from the scenes with `Racer/Export course routes` (`CourseRouteExport`); it now also holds each course's start and vehicle rule. Several routes at once: points closer than 2 px merge and off-screen parts are skipped (UI mesh limit). |

### Free Roam content that is not in Free Roam World (for Dan)

From the audit of all 8 course scenes (`Audit/compare.txt`, `Audit/world-notes.txt`):

1. **The Forest Loop Forward cave (Echo Cave) and its bats.** The accepted cave is a 380 m roofed cut along the Forest Loop trail trench (−54…160, −98…270). In the Backyard world the same ground is the original hillside, and the trench runs right beside the dump, the gullies and the storm drain. Bringing it in would mean grafting the Forest race terrain into the Backyard area (what made the 0.74 holes); by your rule (Backyard version first, never the Forest race version) it stays in the Forest races. Its "Cave warning sign" goes with it.
2. **Forest course activities:** "Forest opening jump" and "Forest speed trap 1/2" (Forest Loop Forward and Reverse, also present in the Mountain scenes) sit on the Forest Loop trails, which are race-only. Their records stay in the save and on the Records board.
3. **South Face Summit Flight** (Mountain Loop Reverse) — on the Mountain race roads; race-only.
4. **The Mountain race roads** themselves (CR110/CR117/CR122/CR133 roads, flights, supports, banks) — race-only. The mountain, its paths to the top, the Summit Homeward jump (0.71 landing, 0.72 return trail) and the Campsite are in the world.
5. **Reverse-course direction guidance** of Street Loop Reverse and Forest Loop Reverse (arrows, House 3 Detour guidance) and Forest Loop Reverse's own cave atmosphere — race-only.
6. **Street Loop Reverse's speed-trap and jump placements** (same Trickum jump and roads as Forward; the world uses the Forward placements, and their Reverse results count).

### Storm-drain culvert (needs Dan's eye)

In Free Roam World the storm drain is exactly the accepted Backyard Reverse version. Along its north half (z ≈ 44–64) the concrete box of the long culvert stands up to 3.8 m out of the ground beside the dirt path (`Views/FRW-bug002-076.png`, `Views/FRW-drain-top-076.png`); the same is true in the Backyard Reverse race scene. An earth cover over it was tried in Free Roam World only and removed again: on this terrain a quick cover gave blocky steps and odd mounds, worse than the plain culvert, and no object was left changed (trees and meshes checked identical to Backyard Reverse). If Dan wants the box buried in Free Roam, it is a separate shaping job along the culvert line.

## Part C — thunder and lightning — PASS

Reproduced as Dan plays (Free Roam, Rain, motorcycle at cruising speed, radio on with the bundled "Punk Rock Classics", Dan's own volumes: master 1, music 1, ambience 1, vehicle 0.75; Lightning flashes On), two minutes per case, with the 0.75 storm code ("before") and the 0.76 code ("after"), in test players built from this source.

**Why he did not perceive it (0.75):**
- Thunder was almost all sub-bass: above 150 Hz (what ordinary laptop, monitor and TV speakers play) the clips peaked at −25 to −27 dBFS — the same level as the engine's audible part and at or below the radio's (bundled songs −23 to −29 dBFS above 150 Hz at the radio volume; measured offline with ffmpeg). Far thunder was cut further (70 % volume, low-pass 1.4 kHz).
- Bolts were lit for 0.10–0.13 s and came down in a random direction: only 1/19 (race), 5/18 and 3/14 lit bolts were within 45° of the view in the first runs.
- Day flashes were weaker by design (×0.62).
- Not the cause: storms run in Free Roam's blended look (rain 1.00); thunder was never culled (at most 11 of 32 voices playing); "under cover" was never true in the open; Dan's settings do not suppress anything.

**Changes:** thunder synthesis gets a mid-band roll (≈90–450 Hz) and a quieter rattle band (≈300–1200 Hz) under the same swells, gently saturated (audible band +12 dB at the same peak); near 1.0 / far 0.9 volume, far low-pass 2.8 kHz; thunder and rumble high voice priority; while thunder rolls the engine dips 30 % and the radio 40 %, recovering over 2 s. Strikes nearer on average (260–1400 m); four bolts in five come down within 38° of the view; the bolt is twice as thick, stays lit through the stroke (0.2 s), flickers back for a return stroke when there is one, and fades as an afterglow by ≈0.9 s, brighter by day; the flash is as strong by day as by night with a 0.16 s main pulse and a soft afterglow (still at most two pulses). Lightning flashes: Off still removes only the screen flash.

| Session (2 min) | Strikes | First strike | Bolts in view | Bolt lit | Flash peak | Thunder above 150 Hz vs engine |
|---|---|---|---|---|---|---|
| Free Roam Day/Rain — before | 9 | 11.0 s | 5 of 10 | 0.13 s | 0.68 | −26…−29 dBFS (offline) vs engine ≈ −28 full band: about level; below the radio |
| Free Roam Day/Rain — after | 8 | 9.9 s | 6 of 8 | 1.04 s | 0.95 | −11.9…−17.3 dBFS vs −35.7…−39.6: **+22…+27 dB** |
| Free Roam Night/Rain — before | 7 | 6.7 s | 1 of 2 | 0.20 s | 0.66 | as above |
| Free Roam Night/Rain — after | 11 | 5.6 s | 11 of 11 | 0.96 s | 1.11 | −11.0…−20.9 vs −36.7…−40.4: **+18…+29 dB** |
| Race Day/Rain — before | 12 | 3.5 s | 0 of 17 | 0.11 s | 0.71 | as above |
| Race Day/Rain — after | 7 | 5.9 s | 5 of 7 | 1.07 s | 0.90 | −11.0…−17.3 vs −33.4…−38.1: **+20…+27 dB** |

Full-band thunder peaks after: −2.5…−8.2 dBFS (engine full band −28…−34). The radio (−23…−29 above 150 Hz) is dipped 4.4 dB under thunder, so thunder stands 10–20 dB above it. Levels are signal levels (clip RMS × voice volume; filters not included), read in the running game. Files: `Storm/`.

## Part D — garage screens — PASS

- **Cause of the squashed vehicle:** the preview render texture was 640×280 (2.29:1) but drawn into a 620×240 box (2.58:1), stretching the picture sideways; the camera also looked down from above at a fixed −30° turn.
- **Now:** in the garage the preview has its own fixed panel on the left of a wider card (as wide as the screen allows); only the option list on the right scrolls. The render texture is resized to the panel's real pixel size (1800×1488 at 3840×2160, 612×705 at 1280×1024; aspect equal to the panel in every shot) and the camera's aspect follows it. The camera frames the whole vehicle with its rider from a three-quarter view (11° down) with a small margin; colour, Classic / New and vehicle changes rebuild the same preview at once.
- **Rotate:** right stick, Q / E or mouse drag, full 360°; after 3 s untouched it turns slowly by itself. The right stick no longer moves the menu selection while the garage is open; left stick, D-pad, arrows and mouse clicks are unchanged. Hint "Q / E Rotate" (keyboard) or "Rotate" with the stick glyph (controller) in the footer.
- **Rider page:** the same fixed panel, framed on the whole rider from head to feet (a car's body is hidden there so the whole outfit shows); the preview never moves (0.0 px with the first, a middle and the last row selected) and never overlaps the list; the list scrolls. The optional per-part close-up was not added.
- Evidence (`Garage/`): every vehicle at 3 angles at 3840×2160 (`sheet-garage-4k.jpg`), the Rider page on the motorcycle and on a car with the first / middle / last row (`sheet-rider-4k.jpg`), the same at 1280×1024 (`sheet-1280.jpg`), geometry logs `garage-*.txt`.

## Notes

- Test-only limits: the simple ride controller cannot take the Backyard Forward trail's gully jumps at a steady pace (it stops in a gully at (279, 62, 11) at 9 m/s and at (82, 34, −109) at 17 m/s); the same spot in the Backyard Forward race scene looks the same and the AI drivers finish that race (`Views/cmp-hold.png`).
- Editor build list now also lists both Mountain scenes and `FreeRoamWorld` (the release build already named its scenes itself), so the editor can load every course by name.
