# 0.75.0-review1 — validation

Blender vehicles for the whole garage + rider customization. Targeted checks (rule 11); Dan judges the look (rule 12).
Evidence: [Models/](Models/) (render sheets), [Game/](Game/) (in-game shots), [Checks/](Checks/) (play-mode check
results), [Bench/](Bench/) (3840×2160 frame times). Tools: `Tools/Blender/` (models, renders), `Tools/Report075/`
(checks runner, sheets, bench, release); play-mode checks `Assets/Scripts/Report075Checks.cs`.

## Part A — New Blender models for every garage vehicle: PASS

The garage roster is four vehicles: Needle 600 (motorcycle, Blender since 0.73), Trail Four (ATV), Street Classic and
Longroof GT (cars). New: `Tools/Blender/trailfour.py`, `cars.py` (both cars from one parametric car), shared helpers in
`kit.py` (`needle600.py` untouched) → `SourceArt/Blender/*.blend`, `Assets/Resources/VehicleModels/*.fbx`.

| Vehicle | Passes | Triangles (vehicle) | Worst case with rider | Notes |
|---|---|---|---|---|
| Needle 600 | 0.73 model | 6,524 | 15,584 | 0.73 rider replaced by the new rider (Moto pose) |
| Trail Four | 2 | 9,740 | 18,800 | fenders, racks, footboards, A-arms, knobbly tyres; handlebars steer about the column |
| Street Classic | 2 | 10,628 | 19,688 | coupe: cabin tub, glass, seats, dash, steering wheel turns; driver at the left seat |
| Longroof GT | 2 | 10,816 | 19,876 | wagon: long roof, roof rack, three windows a side |

(Worst case = woman, long hair, jacket, the heaviest hat; the default rider is lighter.)

- Fitted to the existing vehicles: wheel centres, wheel radius 0.33, ground line, collider, physics, ride height and the
  chase camera offset are unchanged (garage check logs collider size/centre and camera per vehicle). Wheels spin; front
  wheels steer as before; the ATV bars and the car steering wheels turn with the visual steering.
- Paint on bodywork only: Black and Red each recolour only the `paint` renderers; every other renderer, and every rider
  part, is unchanged (`VehiclePaint.UnpaintedUnchanged`) — all four vehicles PASS.
- Lamps: head and tail lamps use the shared lamp materials; glow level 1.00 in every Night race.
- Wipe-out and reset: motorcycle and ATV wipe out and reset upright by day and night; cars reset upright; model and rider
  intact afterwards (8/8).
- Model: Classic / New is now one setting for every vehicle (still saved as `newMotorcycle`, so 0.73/0.74 choices carry
  over), default New, shown for every vehicle; switching both ways works and is saved for all four; AI follow it.
  Classic models are untouched. Ambient traffic always keeps the classic car (20/20 traffic cars classic).
- Render sheets old vs new (Blender final revision, game classic, game new): [Needle 600](Models/vehicle-moto-old-vs-new.jpg),
  [Trail Four](Models/vehicle-atv-old-vs-new.jpg), [Street Classic](Models/vehicle-original-old-vs-new.jpg),
  [Longroof GT](Models/vehicle-tourer-old-vs-new.jpg); details (rear, cabin / rider close-up) `Models/vehicle-*-detail.jpg`.

Race laps (Street Loop, 1 lap, player driven by the AI pilot, 3 AI + traffic; [Checks/races-8-laps.txt](Checks/races-8-laps.txt)):

| Player | Day | Night | AI field (Day / Night) |
|---|---|---|---|
| Needle 600 | 2:20.01, 0 missed | 2:13.37, 0 missed | ATV, Longroof, Street / Moto, ATV, Street |
| Trail Four | 2:35.38, 0 missed | 2:28.26, 0 missed | Moto, Street, Longroof / ATV, Longroof, Moto |
| Street Classic | 2:35.82, 0 missed | 2:36.07, 0 missed | Longroof, Moto, ATV / Street, ATV, Moto |
| Longroof GT | 2:25.61, 0 missed | 2:35.88, 0 missed | Street, ATV, Moto / Longroof, Moto, Street |

Every lap finished with results and a race record saved. (Times include the wipe-out/reset test mid-lap.) The first run
flagged hidden copies of the player's model inside traffic cars: a clone made in the same frame the player's vehicle was
rebuilt inherited its not-yet-destroyed retired visual (a pre-existing quirk, invisible). `VehicleConfiguration.Apply` now
removes inherited retired visuals; confirmation race: traffic 20/20 classic, 0 failures
([Checks/race-traffic-fix-confirmation.txt](Checks/race-traffic-fix-confirmation.txt)).

## Part B — Rider customization: PASS

- One parametric rider (`Tools/Blender/rider.py` → `Rider.fbx`) in three poses (Moto, Atv, Car), assembled in Unity from
  parts (`VehicleVisual.Rider`, `RiderLook`): body Man / Woman (same height class), 6 skin tones, hair Short / Medium /
  Long / Ponytail / Bald in 8 colours, hat None / Flat cap / Baseball cap / Beanie / Cowboy hat, shirt T-shirt / Long
  sleeve / Jacket, pants Jeans / Shorts; hat, shirt and pants colours are the vehicle swatches (incl. Black and White).
  Jeans show the swatch as a darker denim.
- Hair and hats never pass through each other: every hair style is split at a hat band; under a hat only the part below
  the band shows. Blender check: every head point above the band lies under every hat (4 hats × 3 poses, 0 outside;
  [Checks/blender-hat-hair-clip-check.txt](Checks/blender-hat-hair-clip-check.txt)); in game 50/50 hat × hair × body
  combinations show the right parts. Sheets: [hair](Models/rider-hair.jpg), [hats](Models/rider-hats.jpg),
  [hats with each hair](Models/rider-hats-with-hair.jpg), [bodies in each pose](Models/rider-bodies.jpg),
  [shirts and pants](Models/rider-shirts-pants.jpg), [in game](Models/rider-ingame-hat-hair.jpg). Three revision passes.
- Garage: new row "Rider…" opens the Rider page ([Game/rider-page.jpg](Game/rider-page.jpg)): live preview framed on the
  rider on the selected vehicle (through the side window in a car), Randomize and Back beside it, ten option rows in the
  existing ‹ › row style (left/right or D-pad steps both ways, select/click steps forward; mouse, keyboard, controller).
  Every row changes the preview and the player's vehicle and is saved at once (10/10); stepping back restores the
  default; Randomize gives a new saved look; a fresh read of settings.json returns the chosen look (relaunch).
- Default = the 0.73 rider's identity: man, medium skin, short dark-brown hair, flat cap (Black), blue T-shirt, jeans.
  Older saves without a rider get this default.
- AI riders: random per race from the race seed (stable for the race; also in results, which show names only), different
  shirt colours across the field, both bodies whenever there are two or more AI, never the player's exact look. 9 race
  fields checked, all varied (look of every AI listed in the check results).
- Classic models: the old rider; the Rider page says customization needs the New models and offers the Model row
  ([Game/rider-page-classic.jpg](Game/rider-page-classic.jpg)).
- The ghost is built from the player's current rider (not separately tested). No record category changes (categories in the race results are the 0.74 form).
- Ambient people and traffic drivers unchanged.

## Frame rate (3840×2160, GTX 1660 Ti, GPU median)

GPU median, GTX 1660 Ti, 3840×2160 ([Bench/conditions.txt](Bench/conditions.txt); 0.74 from Docs/Report074/VALIDATION.md). The
race grid in these views now shows the new models (player Street Classic; AI Longroof GT, Needle 600, Trail Four).

| View | Day/Clear 0.74 → 0.75 | Dawn/Clear 0.74 → 0.75 | Night/Rain (strikes) 0.74 → 0.75 | Night/Snow 0.74 → 0.75 |
|---|---|---|---|---|
| Street | 3.76 → 3.72 ms (269 fps) | 4.60 → 4.50 ms | 4.51 → 4.60 ms (217 fps) | 4.55 → 4.60 ms (217 fps) |
| Forest | 3.40 → 3.40 ms (294 fps) | 4.13 → 4.10 ms | 4.18 → 4.13 ms | 4.23 → 4.15 ms |
| Mountain | 3.01 → 3.09 ms (324 fps) | 3.07 → 3.26 ms | 3.30 → 3.41 ms | 3.34 → 3.43 ms (292 fps) |

Mixed grid from the chase camera, New vs Classic models in the same build: Day/Clear 4.17 vs 4.11 ms, Night/Clear 4.82 vs
4.75 ms (+0.06–0.07 ms, about 1.5 %). Worst view 4.60 ms = 217 fps (0.74 worst 4.60 ms). **PASS** (within noise).
Shots: [Bench/models-grid-*.jpg](Bench/). The first measured pass stalled when the full-screen player lost window focus
(it pauses in the background); it was re-run with the window kept in front.

## Not changed

- No physics, collider, camera, AI driving, route, scenery or audio change. 0.74 work untouched.
- `PROJECT_TODO.md` gained Dan's queued 0.76 section during this round; it is not part of 0.75 and was not started.
