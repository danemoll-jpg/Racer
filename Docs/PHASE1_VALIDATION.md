# Phase 1 validation — 2026-09-17

Unity 6000.6.1f1, existing URP/Input System versions. Scene: PrototypeTrack.
Phase 0 accepted baseline: 12b1dff. Phase 1 remains subject to Dan's acceptance.

## Automated checks

Executed `Racer.Editor.PrototypeValidation.Run()` in Play mode against the saved
car and real scene colliders, using PhysX steps of 0.02 seconds. All 20 checks passed.

| Check | Result |
|---|---|
| Virtual Xbox RT / left-stick analog bindings | Pass |
| Gamepad input drives and steers car | Pass |
| Virtual Xbox LT brake/reverse binding | Pass |
| Xbox Y resets flipped car, clears momentum | Pass |
| Camera snaps immediately on reset | Pass |
| W/D throttle and steering | Pass |
| S/A reverse and steering | Pass |
| Arrow-key fallback | Pass |
| R resets car | Pass |
| Four seconds acceleration | 26.45 m/s |
| One second braking afterward | 2.16 m/s |
| Three further seconds held reverse | -9.08 m/s |
| Recover 7 m/s lateral slide within one second | Below 1 m/s sideways |
| Ten seconds full-lock cornering | Upright throughout; minimum up dot 1.000 |
| Gentle ramp, 15 m/s approach | Peak 2.70 m; upright landing |
| Gentle ramp, 25 m/s approach | Peak 3.16 m; upright landing |
| Large ramp, 15 m/s approach | Peak 6.58 m; upright landing |
| Large ramp, 25 m/s approach | Peak 7.48 m; upright landing |
| Fall below -15 m | Returns to spawn |
| Moving-car camera proximity/aim | Pass |

Ramp runs used 40% throttle during approach/jump/landing, with 4.8 seconds of
simulation per run. All finished at 0.64–0.65 m body height. No inversion occurred:
minimum up dot was 0.99 on gentle ramps and 0.96 on large ramps. Maximum angular
speed was 4.99 rad/s. These checks cover straight approaches at the listed speeds.

## Additional direct checks

- Real-time Update/FixedUpdate/LateUpdate integration with virtual gamepad:
  60% throttle and 25% stick input moved the car from (0, 1.1, -45) to
  (2.65, 0.65, -16.00), reaching 16.64 m/s; all four suspension probes grounded;
  camera distance 10.66 m while accelerating. Temporary background play was restored.
- Rendered chase-camera view inspected: car and test geometry visible, correct
  chase composition and no missing/pink materials.
- Saved scene reopened: no missing scripts; prefab, physics material, spawn
  reference and camera target resolve correctly.
- Compilation completed successfully. Final actual Unity Console: **0 errors,
  0 warnings**. Pipeline's historical counters retain the earlier fixed asset-file
  extension diagnostic and obsolete test-API warnings; these are not current errors.
- No standalone player build or physical controller hardware test was performed.

## Dan's acceptance tests (pending)

- Drive for several minutes using an actual Xbox-compatible controller. Confirm
  analog triggers, steering deadzone, Y reset, and disconnect/reconnect behavior.
- Judge whether steering, grip, braking and brake-to-reverse feel fun and forgiving.
- Try figure-eights, quick direction changes, and both ramps at varied speeds and
  angles. Assess landing recovery and use Y/R after deliberately getting stuck.
- Judge camera comfort while reversing, turning, jumping and landing.

Known limitations: static wheel visuals, approximate suspension, fixed starting-pad
reset, and possible flips after severe collisions or ramp-edge impacts. No known
unresolved runtime bug in the tested scenarios. Subjective acceptance is not marked
complete, and no Phase 2 work has begun.
