# 0.77.0-review1 — Trailer Mode, Auto camera, player views: validation

Targeted checks (rule 11). Bindings and behaviour: [TRAILER_MODE.md](../TrailerMode/TRAILER_MODE.md).

**Where things are:**
- Code:
  - `Assets/Scripts/CameraViews.cs`: Part C views, first-person and front poses.
  - `CameraViews.Trailer.cs`: Trailer Mode cameras and Auto.
  - `TrailerMode.cs`: the mode, clean screen, guides, slow motion, conditions, screenshots, hint.
  - `RaceMenus.Trailer.cs`: the pause-menu page.
  - Small hooks in `ChaseCamera`, `WorldLook`, `WeatherEffects`, `RaceFlow`, `RacerSave`, `LocalRadio`, `RaceMenus.Core`.
- Checks: `Assets/Scripts/Report077Checks.cs`, run with `Tools/Report077/Run-Play.ps1`. These are editor play mode, muted, with an isolated save.
- 4K in-game probe: `Assets/Scripts/TrailerShots.cs`. It only runs when given `-trailerShots`.
- Evidence:
  - [Checks/](Checks/) holds the result files: `final-results.txt` (98 PASS, 0 FAIL), `fp-*` and `batch3-results.txt`.
  - [Views/](Views/), [Trailer/](Trailer/) and [4K/](4K/) hold the images.

## Part A — Trailer Mode

| Item | Result |
| --- | --- |
| Toggle | **PASS.** F8 and pause menu > Trailer Mode (the race pause menu and the Free Roam menu) open a page with on/off and every setting. The first-time controls hint appears once per session; F1 brings it back. |
| Clean screen | **PASS.** With the mode on, the HUD canvas is not drawn: HUD, speedometer, minimap, lap box, Free Roam text, waypoint line, notifications and radio song. The debug panel is not drawn either. H brings the HUD back and hides it again. Menus and the map still show. Turning the mode off restores everything. |
| Guides (G) | **PASS.** Street Loop race: 80 of 80 gate and arrow renderers hidden, then all 80 shown again with G. The Free Roam waypoint beacon is hidden while the waypoint stays active, including a beacon created after the mode was turned on. |
| Cameras in Free Roam | **PASS.** All nine cameras (Chase, Orbit, Side, Front, Fixed, Flyover, Free, Auto, First person) were checked while riding. None is inside geometry, all have ground below and a clear view of the vehicle ([Trailer/](Trailer/) `roam-trailer-*`). |
| Cameras in a race | **PASS.** The same cameras in a Street Loop race ([Trailer/](Trailer/) `race-trailer-*`). |
| Free camera | **PASS.** Flown straight down at 45 m/s for 3 s, it stops 0.4 m above the ground. The vehicle's controls are off while it is used and come back afterwards. |
| Slow motion | **PASS.** Summit Homeward giant jump with the motorcycle, Auto camera, 0.25× held: time scale 0.25 and physics step 5.0 ms. Flight 244.2 m in 8.04 s of game time, against 242.5 m in 8.00 s at normal speed, so the physics are unchanged. Back to 1.0× and 20 ms afterwards. Game sound is turned down while slowed. |
| Conditions (Free Roam) | **PASS.** Time of day, clock ±1 h, clock run/pause, weather, moon phase and lightning now all work. A strike came 0.33 s after "lightning now". The flow tested is pause menu > on, film, pause menu > off. `settings.json` was **byte-identical** before and after, including after a pause during filming. The Free Roam clock, calendar and weather were exactly restored. |
| Records | **PASS.** A race run in Trailer Mode finished (ATV, 2:19.9) and saved no record: best race time 0.00 before and after. The results screen says "TRAILER MODE / competitive records disabled". |
| Screenshot key | **PASS.** In the game at 3840×2160 the PNGs are 3840×2160 with no UI, at Day, Night/Rain and Dusk/Snow ([4K/](4K/)). They are saved to `Screenshots` beside `DebugReports`, and the folder opens from the pause menu. |
| HUD hidden at 4K | **PASS.** Full-screen captures of everything drawn, overlays included, show no HUD at Day, Night/Rain and Dusk/Snow ([4K/](4K/) `3840x2160-trailer-hud-hidden-*`). There is also a normal-HUD shot and the first-time hint for comparison. |
| Frame rate, mode off | **Within noise.** In the game at 3840×2160 the comparison runs the new camera components with the mode off, then switches them off. Settled pair: 36.97 ms against 36.69 ms per frame (+0.28 ms, with 95th percentiles crossing). The PC was heavily loaded during the run, so absolute numbers are far above the 0.75 bench. The first pair was taken during warm-up and is not used ([4K/trailer-3840x2160.txt](4K/trailer-3840x2160.txt)). With the mode off, the components only read one key and copy the chase camera. |

## Part B — Auto

| Item | Result |
| --- | --- |
| Direction | **PASS** over a 70 s Free Roam ride: 12 shots with no camera twice in a row. Timed shots were held 5.3–8.0 s. Shots used: Flyover, Orbit, Side, First person (once), Chase and Fixed. The Fixed shot ended early when the vehicle "rode past". There were no cuts mid-air or mid-wipeout. |
| Line of sight | Before choosing a shot, Auto checks that the camera spot has a clear view of the vehicle (terrain, trunks, buildings), and leaves a shot that stays blocked for 0.6 s. |
| Jumps | **PASS.** At the giant jump's take-off Auto cut to a low Fixed view beside the flight and made no cut until landing. |
| Hold / skip | **PASS.** No cut in 10 s while held. A skip cuts at once to another camera. |
| Slow motion and conditions during Auto | They work while Auto runs (the slow-motion jump above used Auto). |

## Part C — Player views

| Item | Result |
| --- | --- |
| Views | Chase (unchanged, the default), Far chase, First person and Front, cycled with V / X and shown in Settings > Controls. |
| Each view, each vehicle | **PASS** for all four vehicles: in Free Roam by day and at Night/Rain, and in a Street Loop race at Night/Rain. In every case the camera is clear of geometry, with lamps on and rain falling at night ([Views/](Views/)). |
| First person | The camera sits 4.5 cm in front of the rider's eyes, clear of the face. The rider's eyes, mouth, brows, hair and hat are shadows-only, so 0 head parts are drawn. Near clip 0.05 m. Motorcycle and ATV: the view looks down 16° (ATV 24°) so the bars, grips and hands are in view. Cars: the A-pillar, bonnet, dashboard and wheel are in view through the windscreen. The view leans 40 % with the motorcycle and looks a little into corners. |
| Front | Clear of the bodywork: the cars at bumper height just ahead of the body, the motorcycle and ATV just ahead of the bars. No own parts in front of the camera. |
| Jump in first person | **PASS** with the motorcycle (242.5 m) and the ATV (223.5 m) on the giant jump. The camera stayed within 0.046 m of the eyes; its turn rate followed the vehicle's. |
| Wipeout in first person | **PASS.** The view eases out to the chase camera (8.6 m from the eyes) while the vehicle tumbles. The camera turns at most 49°/s against the vehicle's 165°/s. After the reset it is first person again. |
| Persistence | **PASS.** `cameraView` is saved to `settings.json` and read back. The view is kept after loading another scene. |
| Records | **PASS.** A Street Loop race finished in first person (motorcycle, 2:05.0) saved its record (126.97 s); records stayed eligible in every view. |

## Notes

- The first-person tilt (16° / 24°) was added after the full pass, from the screenshots. The view checks were rerun for the motorcycle, ATV and Street Classic, and then the ATV again: all PASS ([Checks/](Checks/) `fp-*`). The jump and wipeout checks ran just before the tilt; the camera position did not change.
- Conditions on demand apply in Free Roam only. In races, time and weather are chosen at Race Setup as before.
- Trailer Mode stays on across scene changes until it is turned off. Settings made inside it (camera, field of view, slow motion) last until then.
