# 0.73.0-review1 — validation (targeted, rule 11)

Debug session `2026-10-03_20-51-21-371_00b27e` (2 feature requests, captured on 0.72.0-review1 `b6b54925`) plus Dan's written requests. Both screenshots were looked at before placing anything. Play-mode checks: `Assets/Scripts/Report073Checks.cs` (editor only, muted, isolated save); 4K evidence: `Assets/Scripts/ConditionsBench.cs` (command-line opt-in); tools in `Tools/Report073/`, Blender scripts in `Tools/Blender/`.

## Part A — Snow: frozen water, two winter scenes

| Item | Result |
|---|---|
| **A1 every water body freezes in Snow** | **PASS.** All 8 scenes inventoried ([checks/](checks/)): Dan's pool, House 3 pool, House 3 lake, Friend's lake, J1 creek, Woodland (Creek Leap) creek, Fern creek, the storm-drain flow in the Backyard culvert (+36 drain footprints). In Snow each shows a pale blue-white matte ice with a soft sheen and a snow dusting, no animation (the drain's moving-water shader is replaced), no sky reflection (environment reflections off), no ripples or splash sounds. Clear and Rain: the original materials, unchanged. Clear/Snow (Day and Night) pairs in [Water/](Water/). |
| **A2 riding on ice** | **PASS.** In Snow each level water body gets a collidable top face the shape of the water (Fern creek is a 22° sloped channel whose face is buried except where it overhangs drops, and the drain footprints are 3 cm deep: no ice collider there, a vehicle stays on the ground as now). The slowdown is computed as if the vehicle stood the same height above the bed under the ice, so it is identical to the water's; no grip change; AI uses the same vehicle code. Straight crossing of the House 3 lake (0.52 m deep), 72 m line ([lake-crossing-results.txt](checks/lake-crossing-results.txt)): moto over water 1.78 s Clear / 1.80 s Snow (whole line 6.27 / 6.29 s); ATV 2.00 / 1.96 s (whole line 7.00 / 6.47 s — in Clear the ATV also climbs out of the 0.5 m lake bed at the far bank). Body stays on top in Snow (lowest +0.43 / +0.49 m above the surface; Clear −0.17 / −0.22 m). The Forest Forward race route jumps the J1 creek 4.3 m clear, so it is unchanged (6.59 / 6.58 s). |
| **A2 Snow race over water** | **PASS.** Forest Loop (LakeWoods) Day/Snow, 1 lap, 3 AI: start, 5 checkpoint changes, 0 misses, finish 01:12.612, results screen, record written in the normal category ([lakewoods-storm-race-results.txt](checks/lakewoods-storm-race-results.txt)). |
| **A3 sledding (BUG-001)** | **PASS.** Two people on one red sled on the grass slope on the right of the Street Loop road (11 m right of the road centre, 6.5 m off the pavement, road stations ~805–850 below Dan's position). They slide down (6.5 s), stand, turn the sled and pull it back up, sit, repeat. Sled height range 45.3–64.2 m, clearance to the ground −0.03..+0.02 m. [Scenes/](Scenes/) (BUG-001 view Snow/Clear/Rain, sliding, walking up, night). |
| **A4 broom hockey (BUG-002)** | **PASS.** Three people with brooms on the frozen pool beside the deck, knocking an orange ball round (ball travelled 102 m in 48 s); figures exactly on the ice (error 0.000 m). Household scenes are 45 m away and untouched. Day and Night in [Scenes/](Scenes/). |
| Shared rules | **PASS.** Built from the AmbientLife figures (same body and materials, no colliders, LOD cull). Present in Free Roam Snow and in a Snow race; absent in Clear and Rain and in the menus. Placed by world position, so every scene of the shared world that has the slope and the pool shows them (seen in Backyard Reverse and Forest Loop Forward; the pool and slope exist in all 8 course scenes). |

## Part B — Thunder and lightning in Rain

| Item | Result |
|---|---|
| Night/Rain race, 3+ strikes | **PASS.** 3 strikes in 112 s, gaps 43.1 and 49.7 s (scheduler: 20–60 s), 1 / 2 / 2 pulses, peak flash 0.96 (night), thunder played 3 times after a 0.8–4.7 s delay on the Ambience volume. [Storm/](Storm/) flash vs between. |
| Comfort | Never more than two pulses (≈0.11 s and 0.10 s, second weaker); strength follows darkness (day ≈ ⅓ of night). |
| "Lightning flashes: Off" | **PASS.** Settings > Display, default On, remembered. Off: strike and thunder still happen, flash 0.00. |
| Under cover | **PASS.** In the Forest cave: no flash, thunder muffled (low-pass 520 Hz, volume 0.40). |
| No gameplay effect / menus | Look and sound only; back in the menu the look is Clear Day and no flash remains. |

## Part C — Clouds

| Item | Result |
|---|---|
| 9 condition views with clouds | **PASS (look for Dan).** Street, Forest, Backyard, Mountain sheets in [Look/](Look/): Clear = scattered fair-weather clouds (warm at Dusk, moonlit at Night, stars still visible), Rain = dark overcast lit from inside by lightning, Snow = pale even overcast (a flat deck and a haze-coloured horizon ring close the overcast). Day/Clear only gains the scattered clouds. |
| Free Roam dusk → night | **PASS.** 18:00–23:00 strip ([sheet-freeroam-dusk-to-night.jpg](Look/sheet-freeroam-dusk-to-night.jpg)); clouds follow the light continuously. |
| Sky only | Lowest cloud geometry ≈ 375 m (overcast) / ≈ 410 m (clear); highest ground or tree in any scene 197 m. No cloud shadows (not worth their cost). |
| Frame rate 3840×2160 | **PASS.** See table below; within ±0.1 ms of 0.72 except Mountain Day/Clear, which is faster (3.13 vs 3.42 ms). |

GPU median frame time, GTX 1660 Ti, 3840×2160 ([frame-rate.txt](frame-rate.txt); 0.72 from Docs/Report072/frame-rate.txt):

| View | Day/Clear 0.72 → 0.73 | Night/Rain 0.73 | Night/Snow 0.72 → 0.73 |
|---|---|---|---|
| Street | 3.78 → 3.76 ms (266 fps) | 4.53 ms (221 fps) | 4.47 → 4.57 ms (219 fps) |
| Forest | 3.39 → 3.36 ms (297 fps) | 4.23 ms (236 fps) | 4.20 → 4.23 ms (236 fps) |
| Mountain | 3.42 → 3.13 ms (319 fps) | 3.55 ms (282 fps) | 3.51 → 3.54 ms (283 fps) |

## Part D — Blender motorcycle and rider (pilot)

| Item | Result |
|---|---|
| Pipeline | Blender 3.6.1 found at `F:\blender\blender.exe` (not the default path). `Tools/Blender/needle600.py` builds bike + rider and exports `Assets/Resources/VehicleModels/Needle600.fbx`; source `SourceArt/Blender/Needle600.blend`; `Tools/Blender/render_views.py` renders it. Three revision passes (rev 1: fork too tall and tread knobs off the tyre; rev 2: fixed; rev 3: clean sleeve cuffs), then stopped. Renders in [Models/renders/](Models/renders/). |
| Model | 11,592 triangles (bike + rider, classic generated one: 16,468); 14 material slots mapped onto the game's shared materials. Dirt bike: knobbly tyres, spoked rims, hub, brake disc, sprocket, fork + triple clamps, swing arm, shock, frame, engine with fins, exhaust and silencer, tank, radiator shrouds, side panels, seat, fenders, number plate with headlight, tail light, pegs, bars with grips and levers, chain. Rider: helmet-free, flat cap, face and hair, hands on the grips, feet on the pegs, seated; identity colours as the classic rider (player blue shirt). |
| Old vs new | [render-sheet-old-vs-new.png](Models/render-sheet-old-vs-new.png) (Blender row + both models as the game draws them; the classic is generated in code so it has no Blender source). |
| Fitted to the vehicle | Wheel centres (0, −0.20, ±0.825), radius 0.33, collider/physics/camera untouched. Front end (fork, bars, fender, plate, lamp, wheel) turns about the fork axis with the steering; wheels spin as before; rider and bars ride in the existing steering pose; lean unchanged. |
| Garage option | **PASS.** "Model: New (Classic / New)" under the colour swatches for the motorcycle, default New, remembered (fresh save reads it), switched both ways. AI motorcycles follow it (3/3 new in Day and Night races; rider shirts plum / green / blue). |
| Colour incl. black | **PASS.** Black and Red go on the 2 bodywork renderers only; the rider keeps his own materials. |
| Day / Night race, wipe-out | **PASS.** Lamps on at night (level 1.00); wipe-out (rolled over) then reset: upright 0.96, wipe-out cleared, model intact. Garage, Day/Night chase and close side views, New and Classic, in [Models/game/](Models/game/). |
| Frame rate | Grid with player + 3 AI motorcycles, 4K Day/Clear chase: New 3.93 ms vs Classic 3.94 ms. |
| ATV and cars | Not changed. |
