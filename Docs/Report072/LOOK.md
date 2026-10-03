# 0.72 Part C — time of day and weather

Visual and audio only: no grip, handling, AI or physics change; records and ghosts keep the same categories (the race category string is unchanged). Built on the 0.71 `WorldLook` / `LookPreset` system (`Assets/Scripts/WorldLook.cs`), with `WeatherEffects.cs` (rain, snow, stars, rain sound, lamp level), `VehicleLights.cs` (lamps) and the Racer ground shader (`Assets/Track/StreetLoop/SurfaceLighting.hlsl`).

## What there is

- **Presets.** Day = the accepted 0.71 Clear Day, unchanged. Dusk: warm sun 9° up in the west-north-west, long shadows, amber haze, lamps at half. Night: a pale blue moon as the main light (drawn as the sky's disc), dark blue sky and haze, a star field, lifted exposure so it stays raceable, lamps on. Weather is applied on top of any of them (9 combinations): **Rain** — overcast grey sky (brightness follows the time of day), heavier haze (fog 55–560 m), weak soft shadows, darker and glossier wet road plus a headlight glint on it, falling rain streaks around the camera, a rain loop on the Ambience volume; **Snow** — pale overcast sky, white haze (50–520 m), open ground and gentle slopes white through the ground shader, upward foliage dusted, paved roads stay dark and dirt trails stay darker (tracked snow), falling snow.
- **No rain or snow under cover**: when something is within 45 m straight above the camera — a collider, or a cave / tunnel / canopy / ceiling mesh (tested triangle by triangle, since cave roofs are often visual-only; no physics is added) — the falling weather stops and clears and the rain sound is muffled. Inside the Forest cave at night in rain: covered, 0 falling particles (`Look/conditions-run.txt`, `forest-cave-night-rain.jpg`).
- **Night support**: every vehicle (player, rivals, traffic) gets a forward spot light (no shadows; the player's is "important" so it always lights the ground near the player) and its own headlamp / tail-lamp materials glow; the ground shader now takes additional lights, so headlights light the road. Course arrows and checkpoint gate markings glow in their own colour as it gets dark (day = no emission, the 0.71 look). Sign text was already unlit and stays readable; sign boards are lit by headlights.
- **Race setup**: Time of Day: Day / Dusk / Night and Weather: Clear / Rain / Snow (rows after Traffic), defaults Day / Clear, saved in settings.json, controller / keyboard / mouse like the other rows. The race keeps them for the whole race. Menus and the garage always show Clear Day.
- **Free Roam**: Weather on the Free Roam page (default Clear). A live day: `WorldLook.FreeRoamHoursPerRealMinute = 1` (one value), starting at 08:00 each time; the clock is in the Free Roam HUD line ("FREE ROAM 08:00 / …"). The sun is up 06:00–20:00 (rising ENE, setting WNW, 58° at its highest), the moon has the night; the presets blend through Night → Dawn → Day → Dusk → Night; the main light fades to nothing within 2° of the horizon, so the switch between sun and moon never shows; lamps come on by themselves as it gets dark (from about 18:40).
- **Debug Mode**: the debug HUD shows the conditions; every bug report's Markdown ("Conditions: …") and JSON (`conditions`) record them.

## Frame rate (3840×2160, GTX 1660 Ti, GPU median; the 0.71 viewpoints, in a race with the rivals on the grid and lamps on every vehicle)

| View | Condition | GPU median | fps | vs 0.71 Clear Day |
|---|---|---|---|---|
| street | Day/Clear | 3.78 ms | 265 | -2% |
| street | Night/Clear | 4.39 ms | 228 | +14% |
| street | Day/Rain | 3.88 ms | 258 | +1% |
| street | Night/Snow | 4.47 ms | 224 | +16% |
| forest | Day/Clear | 3.39 ms | 295 | -7% |
| forest | Night/Clear | 4.10 ms | 244 | +13% |
| forest | Day/Rain | 3.51 ms | 285 | -3% |
| forest | Night/Snow | 4.20 ms | 238 | +16% |
| mountain | Day/Clear | 3.42 ms | 292 | -1% |
| mountain | Night/Clear | 3.71 ms | 270 | +7% |
| mountain | Day/Rain | 3.42 ms | 292 | -1% |
| mountain | Night/Snow | 3.51 ms | 285 | +1% |

0.71 Clear Day reference (menu state): Street 3.85 ms, Forest 3.63 ms, Mountain 3.47 ms. This round measures in a race with the rivals on the grid and lamps on every vehicle (24 in Street with traffic, 8 elsewhere). Day/Clear equals 0.71; night adds up to ~0.7 ms (headlights in the ground shader, glowing markings, stars); rain/snow ~0.1 ms. Every combination is far above 60 fps. Raw: `frame-rate.txt`. A first warm-up pass was throttled by the desktop (30 Hz) and is not used.

## Evidence

Screenshots `Look/` (1920×1080): `<course>-<time>-<weather>.jpg` for street, forest, backyard and mountain (9 each), `sheet-<course>.jpg` contact sheets, `forest-cave-night-clear.jpg` / `-rain.jpg` (inside the Forest cave at night, no rain), `freeroam-cycle-0800 … 0500.jpg`. Rule 12: one considered implementation; Dan judges the look.
