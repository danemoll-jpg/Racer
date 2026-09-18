# CR-026 / CR-027 integrated review

Safety checkpoint: 5435e3219c2df43491d35a9f4864d60313af4031.
The initial Git add failed creating .git/index.lock; the supported elevated retry succeeded before content edits.

## Roster and ownership

Four stable profiles: original (Street Classic), tourer (Longroof GT), moto (Needle 600), atv (Trail Four).
The original car restores a captured copy of its saved motor fields, collider, mass and original visual visibility. No course/environment edits. The Longroof has a longer body/wheelbase and slower turn-in. Bike and ATV use shorter wheelbases, faster steering/yaw response, stronger lateral acceleration limits and narrower/shorter suspension support footprints. Bike uses two narrow pairs of support probes, not a full tire simulation. Both retain accessible arcade stabilization. Motorcycle visual lean is capped at 16.25 degrees; it does not rotate the collider.

One player Rigidbody/input/respawn/audio owner is reconfigured while in Garage. Changes are rejected during driving. Preview is graphics only, with a separate render camera and no listener. AI/traffic clones remove player input, respawn and engine sources. Cars use the selected car profile; small-vehicle races use Street Classic opponents, visibly disclosed in Garage. No speed matching, rubber-banding or nearby teleporting.

Hold S/LT to brake, then continue holding through zero to reverse. Steering reverses with direction of travel. R/Y uses the existing safe-pad reset and current-lap abandonment; clock and penalties continue. Impact wipeout temporarily cuts throttle/steering authority; severe landing also triggers it. Motorcycle thresholds are lower than ATV. No ragdolls or random flip mechanic.

## Measured flat-ground handling

Independent 50 Hz local physics fixtures, 20 seconds full throttle after settling; same 20 m/s initial speed and 30% steering for turn-in. This is simulated input, not a physical-controller test.

| Profile | Speed after 20s | Yaw after 0.5s | Minimum upright over 5s turn |
|---|---:|---:|---:|
| Street Classic |45.815 m/s|0.814 rad/s|1.000|
| Longroof GT |48.102 m/s|0.674 rad/s|1.000|
| Needle 600 |57.024 m/s|1.257 rad/s|1.000|
| Trail Four |52.458 m/s|1.229 rad/s|1.000|

Motorcycle: +24.5% measured straight speed and +54.4% yaw response. ATV: +14.5% speed and +51.0% response. Baseline motor capabilities are unchanged; the differences are not achieved by weakening it or adding extreme steering angles. Low/high steering angles: original 33/10, bike and ATV 29/9 degrees. Their shorter wheelbase, response and grip work together. All four brake into reverse in the 3s pedal fixture.

## Contact rule and failures recovered

This is an explicit arcade rule: on car/small-vehicle solver pairs, the car has zero effective inverse mass/inertia while the small vehicle retains its normal response. Both colliders remain solid. Car/car, small/small, road and obstacle contact properties are untouched. The class rule is attached to vehicle configuration, independent of player/AI/traffic role.

Unity 6.6 sweep CCD did not honor the desired mass scaling on the initial high-speed impact in these tests. The first two failed matrices are retained. Small vehicles use speculative CCD. Cars switch from their original sweep CCD to speculative CCD only while a small vehicle is within 20m in the same physics scene, restoring sweep CCD when clear. No road materials/layers are changed, no contact is ignored, and no corrective impulse or pose teleport is used. The final matrix compares a contacted car against an independently simulated no-contact car.

Final physics.txt: 60/60 passed, including actual collision events, both directions, 8/25/45m/s, rear/side/glancing/stationary contact and 10s sustained throttle. Flat controlled contact fixtures do not prove every multi-body pileup or roadside pinch scenario safe.

API basis: Unity's contact-pair mass properties and worker-thread contact modification API:
https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/modifiablemassproperties
https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/modifiablecontactpair

## Grid, difficulty and first race run

Diagnosis: old opponents spawned 9/18/27m behind the player. New grid puts the player 32m before START, with opponents 8/15/22m ahead, alternating lanes; all remain before START. Painted slots show footprints. Actual player-camera projections put all three opponents on screen. Shared GO timing and independent progress retained.

Easy/Normal/Hard use lateral cornering targets 7.5/11/14 m/s2 and braking-judgment values 8/12/15 m/s2. Easy/Normal vary early lifting; Hard is consistent. These are driver decisions under identical motor capability, not boosts. Existing obstacle queries, clear-lane passing and safe recovery remain.

First ordinary-frame one-lap race series, AI and four traffic enabled:
- Easy: AI 168.789 / 170.817 / 173.328s; player autopilot 188.498s; all finish, zero penalties/recoveries. Median/p95 16.66/17.86ms.
- Normal: AI 154.943 / 157.538 / 161.164s; player autopilot DNF; zero AI penalties/recoveries. Median/p95 16.67/17.85ms.
- Hard: AI 150.151 / 153.269 / 155.522s; player autopilot 157.890s; all finish, zero penalties/recoveries. Median/p95 16.67/17.82ms.

The Normal player failure is retained in races-initial.txt. Finishers previously braked to a stop near the finish; they now drive 55m toward a shoulder before stopping. Final rerun evidence will be recorded separately. Autopilot pace does not establish human difficulty or Dan's subjective approval. Faster small vehicles have an explicit capability advantage over the car opponents.

## Saves, cues and menu checks

flow.txt: 23 passing checks cover virtual keyboard/gamepad/mouse garage navigation, all four selections and persistence, Escape/back, locked countdown, pause, rejection of driving-time changes, three AI/four traffic, one listener/six player sources, silent AI/traffic, player cue ownership/cooldown, independent AI progress, master mute, restart and results/garage/race-again volume retention.

physics.txt also verifies old settings populate original vehicle/Normal defaults while preserving volumes/mode; isolated profile records do not mix. New category identity includes street-v3-garage, stable profile ID, mode, difficulty for AI races, traffic and laps. Old record files and settings are retained; no legacy times are relabeled as new-rule records.

Tests use Temp directories or explicit -racerTestSave paths. No user records are written by validation. Physical controller, subjective audio/handling, external download and friend-PC tests remain deferred.
