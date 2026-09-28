# Forgiving recovery and situational AI strategy

Safety checkpoint: clean main `18589a4133ec5fb907b64625541fbe27236058eb`. Earlier standing-rule checkpoint: `ca84ef9e62d21b4031e0d18e4f2f08241aca7933`. Standing rules and discovery pointer were already pushed in `18589a41`.

## Recovery

History already sampled at 0.1 seconds and searched newest-first. The reproduced distant-reset cause was recording exclusion: Forest rejected the entire 35 m approach before a launch window; legacy ramps scanned 35 m ahead, and mountain runways excluded 60 m before their start. Main-route Forest exclusions could also apply while following an optional branch. Player exclusions now retain only the immediate wheelbase-plus-2 m approach envelope, actual launch/ramp exclusions, and footprint/support/clearance validation. Optional branch recording does not inherit main-route Forest station windows. AI retains its previous exclusion distances and stability threshold.

Player stable supported forward travel becomes eligible after 0.2 seconds and 1 m, with rolling 0.1-second samples. Successfully established landings still retire pre-jump history. Actual launch portions, unsupported terrain, water, obstacles, other vehicles and unstable poses remain rejected. Placement follows the route bearing with supported slope alignment and existing lateral-clearance alternatives. The existing 0.8-second reset delay and 0.5-second occupied-pad retry remain unchanged; no new penalty or slowdown.

Newest-first validation now retains sub-metre player samples, rejects previous-lap history, and removes rejected newer samples after selecting an older one, preventing a repeat reset from returning forward to them. Placement does not invoke checkpoint/lap advancement; normal origin sampling and shortcut entitlement remain intact. RecoveryDiagnostic records earned progress, selected route/station, age, backward displacement, total displacement, rejected count and reasons without HUD additions.

One Forest scene and its existing vehicle were used for controlled grounded/support fixtures against actual colliders and production recovery. These are deterministic placement fixtures, not claims of human driving. Baseline/new progress losses from a crash 3 m beyond the segment end:

| Case | Baseline | New |
|---|---:|---:|
| Main | 3.08 m | 3.08 m |
| Supported jump approach | 34.09 m | 4.16 m |
| Supported post-jump runout | 3.06 m | 3.06 m |
| Well into House 3 Detour | 3.00 m | 3.00 m |

An added obstruction rejects the newest station 64.96 and selects 63.96, only 1 m earlier. Repeating reset cannot advance to rejected history. Previous-lap samples are rejected. Gate, lap and earned shortcut entitlement remain unchanged by placement. Fixture timestamps are synthetic and can produce zero diagnostic sample ages; gameplay records real Time.time. Files: recovery-before.txt and recovery-after.txt.

## AI

Previous code automatically selected any nearby aiValidated branch for Hard racers or Normal small vehicles; Easy racers and other Normal vehicles could not choose one. This was a vehicle/difficulty switch rather than an independent tactical choice.

Each recreated race driver now owns a Guid-seeded System.Random and a persistent risk offset in [-0.04,+0.04]. Probability is 0.12 + 0.34 times normalized race rank + up to 0.12 for a 200 m trailing gap + risk, clamped to [0.06,0.65]. Neutral mid-pack is about 29%; leading is about 12%; far-behind last is about 58%. No difficulty, pace, motor or physics adjustment is introduced. Normal races receive fresh driver random state; diagnostics can initialize explicit seeds.

Each current-scene, active, aiValidated route receives one decision per lap in the existing 55 m approach, with a 5 m minimum remaining entry margin and the production expected-gate eligibility check. Main decisions are remembered as well as shortcut decisions. The existing branch-following code follows a selected route; other branches and later laps receive new decisions. House 3 Detour participates without metadata/geometry changes.

Bounded deterministic sample: three drivers, 24 opportunities each, 72 per situation. Shortcut counts: leading 10, neutral 24, far-behind 45. All patterns differ and vary by lap; neutral main is favored, leaders can take shortcuts, and trailing drivers can stay on main. Production approach fixture: MAIN / MAIN / SHORTCUT, stable over 20 repeated updates. Actual fixed-step physics approach with normal RoadDriver pedals and ArcadeVehicle motor: selected House 3 Detour, entered and reached 133.08 m, one decision/commitment, zero recoveries, zero misses. No full-race matrix. See ai-checks.txt and ai-driving.txt.

## Scope and limitations

No scene, track geometry, shortcut rule, gate, AI recovery system, motor, audio or UI redesign. Only previously aiValidated routes can be selected; this task does not certify previously unvalidated geometry. Detailed gameplay feel belongs to Dan. No broad regression matrix or subjective tuning.

Unity evaluation initially used a five-second default that timed out during scene/compile work. One headless Editor restart was performed during investigation; subsequent bounded evaluation used its supported 50-second argument. A fixture initially reflected an unloaded method; explicit compilation corrected that setup. These are tooling/fixture failures, not gameplay test passes. Final targeted results above passed.
