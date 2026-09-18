# Racer 0.5.0-review1 integrated revision

Status: implemented, integrated validation and local packaging complete; awaiting Dan's approval. This document records the current implementation and evidence, including unsuccessful trials. Prior 0.4.0 tests are historical and do not establish acceptance of this revision.

Safety checkpoint: `3b2ffe31958b9216255e7b66a3477ecba7d524e7`. The initial Git staging operation could not create `.git/index.lock`; the authorized elevated retry succeeded before content edits.

## BUG-002: diagnosis before tuning

`baseline-driving.csv` captures ordinary Update/FixedUpdate driving through the Input System using a virtual gamepad, with velocity, requested pedals/steering, suspension grounding, wipeout state and collision details. The two small vehicles use speculative CCD so the existing asymmetric vehicle contact solver can act on car contacts. On smooth authored terrain/ramp triangle boundaries, speculative contacts supplied nearly backward horizontal normals despite an upward supporting face. Positive separation was about0.3–1m. The motorcycle dropped51.297→41.891→15.903m/s in40ms, with throttle1, brake0 and before wipeout. Recorded impulses included2078/5804. ATV similarly dropped about48→30.538→13.582m/s. A lack of visible contact did not mean no collision.

`VehicleSurfaceContacts` snapshots authored static supporting mesh faces and vehicle box dimensions. For an above-surface vehicle and upward face, it corrects an inconsistent solver normal and separation against that actual face. Contacts are retained. Side/underside, obstacle and vehicle-pair contacts are untouched. No speed injection, launch impulse, ignored collision layer or car handling boost was added. Suspension, inputs and surface resistance were inspected; the observed abrupt loss was a collision impulse, not a brake command.

Identical-input contact-only repeats (`face-normal-driving.csv`) reduced the largest frame losses to≤1.92m/s motorcycle and≤0.96m/s ATV. The old impact/landing-triggered wipeout still appeared after this fix, so feedback/control state was corrected separately before the larger jump. Unity documents this speculative-CCD ghost-normal failure mode: https://docs.unity.com/en-us/engine/6000.7/manual/physics-section/physics-overview/collision-section/collision-detection/continuous/speculative-ccd . Face lookup uses the official ModifiableContactPair.GetFaceIndex API.

`physics.txt`:60/60 asymmetric contact fixtures pass after the change; maximum overlap0.003m and car path deviation0.0000m against the no-contact control. Both initiator directions, speed, side/glancing/rear and sustained contacts are covered. This does not certify every multi-vehicle pinch or traffic pileup.

## Recovery, feedback and Quit Race

Small-vehicle wipeout requires sustained overturning (up.y<0.35, speed<5m/s for1.2s). Upright bumps, predicted contacts and ordinary landings no longer cut control or repeatedly recreate recovery feedback. A brief transition hint expires; upright supported recovery clears the state. No wipeout samples occurred in the24 integrated road/hill/ramp fixtures.

R/Y first tries the current supported pose, then bounded nearby supported candidates and last-safe history within60m. All suspension supports must be static, suitably sloped authored ground/ramp; the padded body must be clear. A moving-vehicle prediction rejects imminent overlap. Recovery clears unstable motion and updates the sampling origin, preserving completed laps, earned gate/travel credit, clock and penalties. It rejects forward road displacement and forward gate-plane crossings. It adds no reset penalty. If none is safe, it remains local and retries every0.4s, with no START fallback. Full Restart is the explicit start-over path.

Pause offers distinct Resume, Restart Race, Settings, Quit Race / Return to Menu, Quit Game and penalty details. Quit Race clears race/AI/grid state, player movement and relevant race/vehicle audio, returns Ready, and retains saved settings and legitimate records. Incomplete events do not become completed records. Garage/new-race selection works without restarting the app.

## UI, roster and color rules

Normal driving has a compact elapsed lap/race stopwatch and lap/position fractions in the upper left, with a km/h speedometer in the lower right. Solo reads SOLO1/1. Penalty notices expire; Pause/Results explain elapsed plus penalty-adjusted timing. Driving no longer carries the persistent debug table or long control instructions. Garage/results retain their own menu layouts.

Each of three opponent slots accepts all four real profiles, Random or Mixed. Random permits repeats; Mixed fills distinct available choices. The resolved roster is shown before GO and persists across restart/rematch until a deliberate change/reroll. AI gets the same profile capabilities as the player. Traffic retains car profiles; solo/traffic-off remain available. Record category `street-v4-local-jump` includes player profile, resolved roster/difficulty, mode, traffic and lap count; legacy files are retained.

Six player body swatches use per-renderer MaterialPropertyBlock values on body panels only. Per-profile choices persist and apply to preview/race/results; shared materials, tires, trim, riders and opponent colors are unaffected. Mouse, keyboard and virtual-gamepad selection is covered; no physical-controller claim.

## Principal jump

Only the principal jump changed: smooth quadratic40m run,6.2m rise (previous24m/3m), about17° lip, existing continuous supported terrain landing with wider clearance markers. The road/hill profiles, bypass, gates, alternate routes and accepted homes/storefronts/woodland were preserved. `scene-preservation.txt` records the scoped scene comparison. No propulsion modification was used.

`integrated-driving.csv`/`integrated-measurements.txt`: all four profiles, two from-rest road/hill/ramp sequences each, ordinary frame simulation. Ramp approach targets36/42m/s, hill32m/s, straight road full throttle. All eight jump landings remained upright and supported. Measured entry/takeoff speeds, flight times and landing distances are recorded per approach; a landing alone is not subjective handling acceptance. Tourer loses more speed on landing (roughly5–6m/s) than the smaller vehicles; this is retained in evidence, not described as lossless driving.

## AI and retained failures

Difficulty changes corner capability use (36/52/65%), braking judgment (40/62/82%), top-speed target (88/95/99%) and consistency. It does not change vehicle engine/grip capabilities. Horizontal curvature is separated from vertical hills; crest speed respects support limits. Obstacle response includes lead-vehicle velocity. Profiles retain their distinct steering/acceleration/braking. Local stalled recovery is bounded and preserves independent progress/penalties.

The initial aggressive trial and a probe-radius regression are retained in `standalone-race-2-initial-failed` and `standalone-race-2-fast-hills-failed`: excessive recoveries, road detection by the obstacle sphere, overly fast hill approaches and misses were not counted as passes. Corrected smaller probes, crest planning and slower hill targets were tested in full races.

A further full-race failure exposed finished bikes rolling backward downhill after clearing the line because their holding brake released below1m/s. The correction parks finishers after normal-control runoff; the player now also clears the finish under normal control while opponents finish. Quit/restart removes that driver. `race-observations.txt` preserves the diagnosis and initial racecraft incidents.

AI results are actual three-lap times, speeds, braking duration, misses and recoveries from ordinary-frame Windows races with four traffic cars. The player reference driver is an instrumentation aid, NOT proof of competition against Dan. Physical-controller feel and human Hard difficulty remain for Dan's approval.

## Preservation and validation boundaries

Flow fixtures exercise virtual keyboard/mouse/gamepad navigation, every profile/swatch, persistent settings, every slot choice, stable random roster, safe grid, Quit during countdown/racing, local recovery upright/flipped/tree/slope/moving-car, completed/current-lap preservation, finish-plane abuse, unsupported fallback, checkpoint ding/miss buzz cooldown, temporary notices, prop breakage/cleanup/restart and finish runoff. Controlled pose/gate fixtures isolate state invariants; they are distinguished from ordinary driving/racing probes.

No CR-013 photo/CR-018 placement work, multiplayer, upload or distribution. New record categories preserve older records. Tests use explicit isolated save directories.

## Final integrated results

- Editor137/137 and actual committed-source Windows release137/137 flow checks pass, including physical-capability assertions for all four opponent choices, profile-sized grids and actual virtual R/Y input for every player profile. `release-flow.txt` is the final standalone report. Earlier108/118/120/121-check suites cover progressively expanded tests and1024×768,1280×720,1680×720 layouts. None imply physical-controller coverage.
- Standalone driving24/24 scenarios and8/8 repeated jumps: no wipeout samples; no severe phantom ramp braking. `JUMPS.md` gives per-profile entry/takeoff/flight measurements. Landing lateral coordinate stays−2.76 to−2.85m inside the marked±12m corridor, only about1.3m from the−1.5m approach lane; landing up.y≥0.999. Ramp-case collision logs contain only intended ground/takeoff/landing surfaces, no obstacle collision. `landing-clearance.txt` records support/pose; landing speed losses remain disclosed.
- Final three-lap mixed Windows races:12/12 racer finishes,9/9 opponents, zero DNFs. Easy0recoveries/0misses; Normal0recoveries/2ATV misses (+10s retained); Hard1ATV recovery/0misses. Full lap/mean/peak/braking data in `AI-RACES.md`. Hard tourer laps140.261–141.914s, motorcycle128.419–129.033s, ATV141.346–146.144s. Human competitiveness remains unaccepted.
- Matched single-instance1280×720 / VSync-off /120fps-cap comparison: baseline and release both8.33ms median /8.35ms p95. Peak working set435.44→467.57MiB (+32.13MiB). `PERFORMANCE.md` states hardware, settings and limitations; no uncapped-throughput claim.
- No C# exceptions/assertions/errors found in final standalone test logs. The existing stripped optional post-processing shader warnings remain; those passes are unused. Build succeeds with0errors and one intentional RuntimePipelineConfig warning.

## Release and package

Executable source commit: `c57b6cce912b256de15da74a0e7ef19495bf8b48`. Build used a verified clean source tree. Subsequent changes are documentation/evidence only; final `git diff` against this source is empty for Assets, Packages and ProjectSettings. Unity6000.6.1f1, Windows x64, non-development, no debugging/profiler/runtime Pipeline. Build report: `release-build.txt`.

Open `Assets/Scenes/StreetLoopGreybox.unity`, or run `Play-Racer.cmd` / `Builds/Latest/Racer.exe`. Versioned runtime: `Builds/Racer-0.5.0-review1-Windows`. All229 staged files verified by SHA256 after fresh ZIP extraction and in Latest. Previous Latest preserved at `Builds/Latest-before-0.5.0-review1`. Launcher now displays VERSION metadata before opening Latest. The verified Latest player was opened normally after isolated tests.

Local ZIP: `Builds/Racer-0.5.0-review1-Windows.zip`,76,355,002bytes, SHA256 `55DA6EFCFD373C95646145A22A4672277ED9705021701EA770373A9432D869EE`. `VERSION.txt` records the actual source commit. Manifest/package verification JSON and reproducible packaging scripts are in this folder. No upload/distribution performed. The completion commit contains the final documentation/evidence; its ID is reported in the delivery response and Git history rather than embedded self-referentially here.

Remaining review limits: Dan's physical-controller navigation, subjective bike/ATV feel, jump satisfaction, listening and human Hard competition; wider multi-body traffic/pinch cases and hardware/endurance coverage. Normal ATV penalties and Hard ATV local recovery remain real racecraft costs. Successful automated routes do not establish subjective acceptance.
