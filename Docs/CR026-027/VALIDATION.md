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

The Normal player failure is retained in races-initial.txt. Finishers previously braked to a stop near the finish; they now drive 55m toward a shoulder before stopping. The complete Windows rerun below resolves that failure. Autopilot pace does not establish human difficulty or Dan's subjective approval. Faster small vehicles have an explicit capability advantage over the car opponents.

## Saves, cues and menu checks

flow.txt: 24 passing checks cover virtual keyboard/gamepad/mouse garage navigation, all four selections and persistence, Escape/back, locked countdown, pause, rejection of driving-time changes, three AI/four traffic, one listener/six player sources, silent AI/traffic, player cue ownership/cooldown, independent AI progress, master mute, restart and results/garage/race-again volume retention.

physics.txt also verifies old settings populate original vehicle/Normal defaults while preserving volumes/mode; isolated profile records do not mix. New category identity includes street-v3-garage, stable profile ID, mode, difficulty for AI races, traffic and laps. Old record files and settings are retained; no legacy times are relabeled as new-rule records.

Tests use Temp directories or explicit -racerTestSave paths. No user records are written by validation. Physical controller, subjective audio/handling, external download and friend-PC tests remain deferred.


## Final ordinary-frame Windows races

The visible 1280x720 standalone ran one complete lap per difficulty, three opponents plus four traffic cars, default Street Classic. Full evidence: standalone-races.txt and grid-0/1/2.png. Editor handling tests ran concurrently, so these frame times include shared machine load.

| Difficulty | BLUE | GOLD | EMBER | Reference player | Finishes | Misses / recoveries |
|---|---:|---:|---:|---:|---:|---:|
| Easy |2:48.779|2:50.803|2:52.827|2:54.931|4/4|0 / 0|
| Normal |2:34.826|2:37.426|2:40.887|2:42.734|4/4|0 / 0|
| Hard |2:30.138|2:32.635|2:35.188|2:36.938|4/4|0 / 0|

Median/p95: Easy 33.30/33.37ms; Normal and Hard 33.33/33.37ms. All nine AI and three player runs completed. Hard's quickest opponent is 18.641s (11.0%) quicker than Easy's. Normal/Hard provide measurable pace differences and beat the reference autopilot; human competitiveness still requires Dan's review. No recovery-triggering stuck incidents; transient slowing is not separately timed. This is a one-lap benchmark, not an endurance claim for every profile/mode. The previous 0.3 review ran a three-lap Windows race at 33.33/33.37ms, so there is no demonstrated frame-paced regression; different test lengths prevent a strict hardware benchmark comparison.

## Road handling rerun and limitations

handling.txt is an ordinary-frame virtual-Gamepad suite covering acceleration/braking/reverse, 40m/s corner entry, hills, hairpin, off-road/re-entry, 33/40m/s jump entries, reset, pause and restart for each profile. Initial failed/weak results remain in handling-initial.txt. The harness originally passed desired steering directly into the stick deadzone and braked the faster vehicles too late for the finite straight. Correcting the input mapping and starting small-vehicle braking earlier fixes the test assumptions without reshaping the road or weakening the baseline.

| Vehicle | Road peak | Minimum upright in tested road scenarios | 33 / 40m/s jump longest air |
|---|---:|---:|---:|
| Street Classic |45.63m/s|0.963|1.80 / 2.03s|
| Longroof GT |47.73m/s|0.964|1.73 / 2.03s|
| Needle 600 |56.31m/s|0.962|1.42 / 1.48s|
| Trail Four |52.01m/s|0.955|1.33 / 1.03s|

All scenarios ended with four support probes grounded, and no ordinary-turn flips. Four probes on the bike are two close lateral pairs, not four visible wheels. Original car road peak was 45.60m/s in the prior build and 45.63 here, consistent with unchanged handling. Each profile paused, reset and restarted with six player audio sources and one listener. The mailbox physically broke, moving debris cleaned up, and restart restored it.

Limits: the motorcycle hill fixture slowed to 1.20m/s after running wide (9.60m maximum 3D road distance); it remained upright, but the automated pursuit is not a clean high-speed hill-driving proof. ATV's 40m/s jump reached 25.74m maximum 3D road distance before ending upright/grounded: landing stability passed, route retention did not. The 33m/s ATV jump stayed within 7.32m. These are recorded failures/limitations, not hidden by course changes. Do not treat every jump angle/speed as safe. No physical controller, human lap, subjective sound-mix approval or multi-body roadside squeeze claim.

## Source and build coverage

Full races, contact matrix and road suite used gameplay source 11af5399cc924c55bb8d8c7cea53e9036d90b3db. Final source 7a06eaa81ac6130e8b487ad54ce285ec9f2e3656 adds wheel motion markers and standalone validation entry points only. Final standalone sample and garage checks use that final source. The complete environment scene remains unchanged. Unity's recovered scene backup was retained under Assets/_Recovery, is not active and is not included in the build.

Final Windows build: 0.4.0-review1, Unity 6000.6.1f1, non-development, no script debugging/profiler/runtime Pipeline. Build succeeded in 17.72s, 207,213,009 reported bytes, zero errors; one intentional warning that no RuntimePipelineConfig exists (remote runtime control excluded). The earlier full build also reported an existing 103-mesh future pre-bake warning, retained in history; no current runtime failure was associated with it.

## Final executable checks and package

Final source standalone-flow.txt: 24/24 checks passed. Four final garage screenshots are garage-original/tourer/moto/atv.png. Keyboard/mouse/gamepad are virtual Input System devices, not physical-controller testing. The final single-instance 60-second Normal sample (standalone-sample.txt) ran with AI/traffic at median33.30/p9533.37ms, zero misses/recoveries; deliberately stopped before finishing. Interaction screenshots at12/36/60 seconds document visible nearby opponents. No exceptions/errors in the standalone race, sample or flow logs.

Local Windows review ZIP: Builds/Racer-0.4.0-review1-Windows.zip, 76,335,940 bytes, SHA256 08D176ABAD0E2A55354041DF147D4559CA7032D38CD2280504431EC36FCD2D1C. Package.ps1 is the repeatable versioned workflow; refuses overwriting an existing stage/ZIP and excludes logs/debug/validation output. VERSION.txt identifies actual source 7a06eaa81ac6130e8b487ad54ce285ec9f2e3656. package-files.json contains229 runtime/readme/license hashes. Verification evidence is package-verification.txt. Previous Latest retained at Builds/Latest-before-0.4.0-review1. No uploads/distribution; download and friend-PC review remain deferred.

Open scene Assets/Scenes/StreetLoopGreybox.unity, or run Play-Racer.cmd / Builds/Latest/Racer.exe. This is one integrated CR-026/027 update awaiting Dan's approval. Existing houses/yards/store/woodland/roads/jump/shortcut/breakable scenery and missed-gate continuation remain intact. New vehicle art is original procedural low-poly geometry; see VEHICLE-ASSET-NOTICES.txt and retained existing notices. No paid/imported third-party vehicle assets.
