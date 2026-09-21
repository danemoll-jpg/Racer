# CR-101â€“104 correction evidence (in progress)

Safety checkpoint: `a4dfebb23ba65d2e5d9227b2b5c18d0cbaea578b`.
Git staging initially failed with sandbox `.git/index.lock` permission denied; the supported elevated retry and commit succeeded. No lock was removed, hook bypassed, or history rewritten.

Only the four requested corrections and their direct dependencies are in scope. Existing unrelated acceptance states and failures remain unchanged. All ordinary player checks use isolated saves with temporary Master=0. No physical controller, Deck or subjective listening validation is claimed.

## References and baseline

Inspected Dan's 22_43_54 property drawing and all three Racer screenshots (22_30_11, 22_29_30, 22_29_06). Drawing-right is interpreted relative to the road-facing house, not cardinal north. House world position is `(415.800,83.820,-13.000)`; its front vector is approximately `(1,0,-.03)`.

One baseline motorcycle natural-approach run was performed with ordinary motor throttle/brake/steering, from stationary, at timeScale=1 in the compiled unchanged geometry. It followed the authored approach points and reached the runway after 12.8 seconds, with minimum up 0.973. **It did not reproduce Dan's reported pit and did not include the subsequent flight.** The fixture's PASS applies only to reaching those points; it does not establish that the human-reported ramp was working. No second baseline run was made. Raw trace and chase view are in `baseline/motorcycle-natural`.

## Audio source comparison

Downloads source and `Assets/TitleSource/Woodstock Rush Spoken.mp3` are byte-identical: SHA-256 `A01FFFBD63FEC95D762A46F775746798C51317358D129AC139673D34041446C6`.
Existing FFmpeg decoder produces 84,010 stereo frames at 48 kHz (1.750208 seconds), exactly matching `Voice.wav` length. Maximum difference is 0.0000305179, consistent with existing 16-bit PCM quantization. The last 100 ms RMS is 0.000227709; final sample is approximately 0.0000008644. This proves that this decode-to-WAV step did not shorten the provided recording; it does not establish phonetic completeness or listening acceptance.

StartupTitle has no automatic dismissal timeout. Speech stops on an intentional fresh input. Actual listener-mix and imported-clip results are recorded below.

## First playable geometry

Heading derives from summit reference `(990,0,150)` to the actual house: normalized `(-0.961990,0,-0.273083)`. Runway origin `(1192.018,152,207.348)`, lip `(980.380,177.920,147.269)`. Authored runway is 220 m in horizontal station; test start at station 5 gives 215 m to the lip. No acceleration, boost, injected launch velocity, gravity or scoring changes.

Geometry has been generated in all four saved variants. Physical run outcomes and copy comparisons are recorded below.

## Intermediate results retained

The first candidate used a flag already owned by the old correction harness. That accidentally started unrelated legacy checks alongside the flight fixture; the process ended before the targeted flights completed. A stop was attempted as soon as the collision was identified. The flag is now uniquely `-cr101Check`. The invalid mixed run in `first-playable/eight-flights` is not acceptance evidence for any system.

`first-playable-isolated/eight-flights` contains the actual eight first-playable runs, before property/sign detail work. Launch speeds were 36.44â€“42.84 m/s from rest. All went toward the house. The original landing generated poor touchdown/rollout behavior, particularly both cars and the offset ATV; centered ATV did not settle in the corridor. All failures are retained.

After the catch-slope correction, `corrected/eight-flights` passed both Tourer, motorcycle and ATV lines. Original-car world-up readings near 0.59â€“0.62 tripped the initial vertical-up threshold. A ground-relative alignment check is being added to distinguish pitch on a 0.75-grade descent from instability; this does not alter vehicle physics. Touchdown is now separated from airborne clearance rather than counting suspension/body contact at the intended landing as an in-flight strike. The first-contact location must be beyond the summit, and any earlier contact fails the flight.

The changed Driveway 1 traversal passed from the road through the moved-building area (minimum up 0.888). The moved summit destination and activity lip checks passed. The single aborted-approach reset passed. The natural connector and return failed and are being repaired; these failures are preserved in `corrected/local-driving`. No repeat of the passing driveway/reset is required.

## Complete voice output and skip

Dan confirmed that the original recording contains the complete word. The old startup DSP capture reached sample 84,010 without advancing or starting radio. Source-to-mix analysis found the background mix RMS during the 1.20â€“1.65 s portion was 0.007082, versus speech RMS 0.007728: masking is supported by the measured mix, rather than decode truncation.

The corrected startup schedules the unchanged voice on the DSP clock, waits its decoded sample duration plus 200 ms, then starts the unchanged looping theme. Source gain, saved preferences and shipped volume/mute settings are unchanged. Intentional skip still cancels pending playback.

`corrected-audio/full-phrase` captures the complete actual listener mix without input. Runtime-imported PCM is exactly identical to `Voice.wav` (maximum difference zero, all 84,010 frames). Captured voice correlation is 0.999999869; residual background RMS during the ending is 0.000016754, versus speech RMS 0.007742. No clipping. The title stayed active and radio was absent. `corrected-audio/intentional-skip` interrupted at observed sample 32,416, then entered the menu once with radio attached there. Both checks remuted immediately after capture. These are waveform/DSP and virtual-input checks, not OS-endpoint or subjective listening validation.

## Property and sign verification

`followup/views/property-top.png` and `driveway1-ground.png` were visually inspected in the compiled game. Road is at the bottom of the top view; the house stays parallel to it, pool behind, small pool house alongside near Driveway 1, garage across that driveway. Local side/rear fence extensions close the expanded layout. House art, second driveway and neighboring arrangements were retained. `corrected/local-driving/changed-driveway1.csv` passed the changed driveway from the road. No neighborhood or race tests were run for this edit.

All ten `followup/views/01...05...` front/rear images were inspected. All five boards contain their readable lettering with margins; fronts face supported driver approaches. Rears are blank. A dedicated summit font material/shader uses backface culling; the existing shared sign shader was not modified, so no unrelated signage suite or dependency check was needed.

## Supported connection and return

`followup/connection` passed all 167 natural-entry waypoints, minimum world-up 0.9704. The prior outer bend exceeded the world boundary; the revised bend stays inward on a continuous explicit surface. `followup/return-woods` passed all 131 return waypoints in LakeWoods, minimum world-up 0.8922. Its dense support mesh fixes the coarse-ground gap. The prior changed-driveway, single aborted-reset, moved safe destination and lip-volume checks passed and were not unnecessarily repeated.

## Flight measurement details

The normal motor receives throttle/brake/steering at timeScale 1, with zero initial velocity. It does not inject launch speed or apply a special boost. Starts are runway station 5 at `(1187.208, approximately 153, 205.982)` (vehicle suspension changes actual Y); offset runs start 3 m sideways. Horizontal run-up is 215 m; integrated surface length is 221.795 m. Heading is derived from actual summit-to-house coordinates, approximately yaw 254.152 degrees.

The trace records every 20 ms physics step, including nine terrain samples beneath the collider's world-axis bounds, wheel contacts, position and velocity. No early terrain contact between the lip and station 330 is allowed; first contact must be on the neighborhood-side catch slope beyond station 330. This is sampled collision/clearance evidence, not a continuous mathematical swept-volume proof. Minimum bottom clearance over stations 230–330 was 3.501 m for the original car, 4.877 m for Tourer, 22.673 m for the motorcycle and 19.396 m for ATV, across both lines. See `mountain-corridor-clearance.json` and the complete traces, not just the apex screenshots.

| Profile | Takeoff speed (m/s) | Approximate first landing world X/Y/Z | Airtime (s) |
|---|---:|---|---:|
| Original car | 36.47 | 853 / 175 / 111–114 | 4.94–4.96 |
| Tourer | 36.44 | 851 / 174 / 111–114 | 4.96 |
| Motorcycle | 42.80–42.84 | 760 / 109 / 85–87 | 7.52 |
| ATV | 40.86–40.91 | 783 / 122 / 91–94 | 7.00–7.02 |

Landings remain hundreds of metres short of the house and its fences. Side and normal chase captures accompany each flight. Both Tourer/bike/ATV lines passed corrected geometry; the original car settled but tripped an initial world-up threshold on the steep catch slope. The follow-up uses support-relative alignment to diagnose this, and only counts the normal when at least two current suspension rays are supported (the preceding-frame wheel count alone can be stale). The remaining targeted result is recorded below.

The moved local activity volume and mapped destination align with the new lip/run-up. The original car's existing jump award remains zero. No award thresholds or broader activity/record suites were changed; this residual is separate from physical clearance and supported recovery.

## Copies and build diagnostics

Final audit sampled 789 stations/sides per course. Forward/reverse Street are identical; LakeWoods/ForestReverse are identical. Run-up, flight corridor and landings match across both pairs through station 479. Underlying-terrain differences begin at station 481 in rollout/return surroundings (maximum 0.036 m). The shared explicit return surface was driven in LakeWoods, and final centered ATV also ran there. Identical reverse copies received geometry checks, not races. See `geometry-consistency.json`.

The clean release build succeeded. It counted one Editor `/api/exec` status-query timeout during the busy build, not a compiler/player error. Four warnings identify optional RuntimePipelineConfig absence, pre-baked collision import guidance and two obsolete calls in unchanged ThreeFeatureValidation. See `release-diagnostics.txt`. No unrelated systems were reopened.

The original offset diagnostic logged its worst alignment at station 354.29, first two-wheel landing contact: car up `(0.03,1.00,0.01)` against catch-slope normal `(-0.58,0.80,-0.16)`. A brief brake delay did not change that initial angle. Both runs completed 66 consecutive stable grounded frames; the minimum was an incoming-to-slope mismatch, not a sustained rollover. The diagnostic now assesses support alignment after 0.5 s of suspension settling, retaining body clearance, contact location, world-up and stable-rollout requirements. Geometry/vehicle physics were unchanged for this correction. Earlier FAIL results remain in `final/original-offset*`.

## Delivery build and targeted conclusion

`build-delivery-build.txt`: successful Windows build, zero errors, three warnings. It contains the unchanged tested geometry/audio plus the corrected settling diagnostic. `delivery/original-offset` passes with minimum post-settling support alignment 0.99039 and 66 consecutive stable grounded frames; its pre-settling touchdown is still documented above. The original centered run already passed the stricter earlier support test, so it was not repeated again. Together with the corrected Tourer/bike/ATV pairs, all eight requested lines cleared the mountain and completed supported landings. The final centered bike and ATV checks use this delivery binary.

No subjective listening or physical-controller/Deck validation was performed. Dan confirmed the original full word; the complete actual DSP output and intentional skip are verified. The source, imported recording and StartupTitle behavior are unchanged between the audio-verified candidate and delivery build. Existing jump awards remain zero in the final original/bike/ATV runs; this is retained as a residual rather than changing unrelated scoring rules.
