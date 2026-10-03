# 0.71 Part C — lighting and atmosphere: "Clear Day"

## What it is

One shared look, applied to every course scene at load by code (`Assets/Scripts/WorldLook.cs`). No scene was edited for it. It covers races, Free Roam, the garage and the menus alike, because they are all states of the same scene.

- **Presets.** Everything the look sets is one `LookPreset`: sun elevation/azimuth/colour/intensity/shadow strength; trilight ambient; procedural sky tint, ground colour, exposure, atmosphere and sun size; linear haze colour and distances; ground-shader response; water smoothness; exposure, contrast, saturation, colour filter and bloom.
  - `LookPresets` holds exactly one preset, **Clear Day**.
  - `LookPreset.Lerp(a, b, t)` interpolates every field (sun azimuth by angle), and `WorldLook.Apply(preset)` can be called every frame. A dusk/night/rain/snow preset and a Free Roam day-night cycle blending between presets can therefore be added later without rework (not built).
- **Sun:** the scene's directional light, 47° high from 218°, warm white, soft shadows at strength 0.9.
- **Ambient:** sky / horizon / ground trilight, so shaded sides keep their colour.
- **Sky and horizon:** the procedural sky gets a deeper blue. Below the horizon it takes the haze colour, and linear haze (180–1,600 m) in the same colour closes the gap. The grey-brown void at the world's edge is gone; compare the right side of [mountain-loop-forward-lower-route](Look/mountain-loop-forward-lower-route.jpg).
- **Ground surfaces:** the world's own vertex-colour ground shader (`Racer/GreyboxGround`, `Racer/MarkedGround`, shared `SurfaceLighting.hlsl`) now supports haze. It reads six global values the look sets:
  - stronger sun relative to ambient (more shape on hills and trees);
  - slightly lifted shadows (never black);
  - broad soft patchiness on grass and finer grain on dirt, so grass, dirt and road read as different surfaces;
  - a faint sun sheen on paved road only.
  - With the look off these values are 0 and the shader output is identical to 0.70.
- **Water:** the URP Lit water materials get a smoother runtime copy (the assets are not changed), and each lake gets a sky-only reflection probe rendered once, so lakes reflect the sky.
- **Post-processing** (one global volume): Neutral tonemapping, +0.1 exposure, +12 contrast, +12 saturation, mild bloom (0.2 above 1.05). Nothing else: no motion blur, depth of field, film grain, lens dirt, chromatic aberration or vignette.
- **Anti-aliasing:** the existing 2× MSAA (unchanged).
- **Pipeline asset (`PC_RPAsset`):** HDR on (needed for tonemapping and bloom) and shadow distance 40 → 80 m (4 cascades unchanged). Shadows near the vehicle no longer pop at 40 m.
- **Dropped — ambient occlusion.** URP screen-space AO (half resolution, depth-reconstructed normals, after opaques) cost ~1.4 ms per frame at 3840×2160: +45% frame time on its own. It was removed completely.
- **Caves and tunnels:** the cave surfaces use their own unlit shader and keep their authored brightness. Tunnel ground uses the ground shader with shadows lifted. Neither got darker.
- **Readability:** arrows, gates, sign lettering and the minimap are unlit and get only the gentle grade. The HUD and menus are screen-space UI and are untouched.
- **Vehicles:** in the [garage](Look/garage.jpg) the vehicle colour reads the same (teal stays teal), with a little more shape.

## Evidence

- Same build, same fixed viewpoints:
  - left: `-lookOff` with the 0.70 pipeline values (HDR off, 40 m shadows), i.e. the 0.70 rendering;
  - right: Clear Day.
- Captured by `Assets/Scripts/LookBench.cs` (command-line opt-in `-lookBench <folder>`, requires `-racerTestSave`, muted, quits when done). The pairs are in [Look/](Look/):

| Course | View |
|---|---|
| Street Loop Forward / Reverse | [forward](Look/street-loop-forward.jpg), [reverse](Look/street-loop-reverse.jpg) |
| Forest Loop Forward / Reverse | [forward](Look/forest-loop-forward.jpg), [Echo Cave approach](Look/forest-loop-forward-cave.jpg), [reverse](Look/forest-loop-reverse.jpg) |
| Mountain Loop Forward / Reverse | [forward](Look/mountain-loop-forward.jpg), [lower route](Look/mountain-loop-forward-lower-route.jpg), [reverse](Look/mountain-loop-reverse.jpg) |
| Dan's Backyard Forward / Reverse | [forward](Look/backyard-forward.jpg), [reverse](Look/backyard-reverse.jpg) |
| Garage | [vehicle preview](Look/garage.jpg) |

## Frame rate (3840×2160, GTX 1660 Ti, release player, vsync and frame cap off)

The desktop at times throttles frame presentation (a 30 Hz display, an occluded window): runs then sat at ~30 fps both before and after, whatever the look. So the GPU time of each frame (Unity `FrameTimingManager`) is the measure. Medians over 12 s, several runs per side; all runs are in [frame-rate.txt](frame-rate.txt).

| View | 0.70 GPU ms | Clear Day GPU ms | Change | Clear Day GPU-limited fps |
|---|---|---|---|---|
| Street Loop Forward (start) | 3.49 | 3.85 | +10% | 260 |
| Forest Loop Forward (start) | 3.21 | 3.63 | +13% | 275 |
| Mountain Loop Forward (s 400) | 3.01 | 3.47 | +15% | 288 |

- **60 fps:** met everywhere by a wide margin (the slowest 95th-percentile frame is 4.9 ms, about 200 fps).
- **~10% of 0.70:**
  - Street is at the budget; Forest and Mountain are 3–5 points over. The look adds a near-constant 0.35–0.45 ms, which is a larger share of the lightest scenes.
  - No single remaining setting costs more than 10%:
    - the whole post-processing chain (HDR target, tonemapping/grade, bloom) measured ~0.35 ms;
    - shadow distance 60 / 80 / 110 m measured the same within noise;
    - bloom alone was below the noise.
  - AO was the one setting far over budget, and it was dropped.
  - If Dan wants the frame time closer to 0.70, the lever is the post-processing chain. Subtracting its measured ~0.35 ms gives an estimated ~0–4% over 0.70 (estimated, not measured on the final build). That would lose tonemapping, grade and bloom but keep sun, sky, haze, surfaces and water.
