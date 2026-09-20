# CR-081 + CR-090 combined implementation — in progress

Safety checkpoint: 8419e9194985955bb56a41854f1b5dce7968b270. Saved changes were committed before modifications; elevated Git retries resolved sandbox index.lock permission failures.

## Implemented source and authored content

- Automatic race/free-roam speed traps and authored-jump landing results. Existing race timing/gate/penalty rules remain independent. Activity top-ten archive supports distinct ties, duplicate IDs, one-time historical PB migration with unknown dates, course/layout/vehicle/travel-direction categories, and controller menu access.
- Local clean-lap pose recording and interpolated visual-only playback; resets, discontinuities and missed gates exclude a lap. Faster compatible completed laps replace the current ghost; old layout files remain separate. Playback toggle is in Exploration / personal best. Sampling is approximately 17 Hz (fixed-step .05-second minimum), limited to 24,000 poses / 20 minutes per lap. Ghost time uses the gate's interpolated crossing timestamp; recording uses actual Rigidbody poses, never controller input simulation.
- Twenty-four persistent acorns across neighborhood streets and mountain trails; independent save file. No currency, upgrades, unlocks or penalties.
- Connected mountain east of Kyle's lake; creek ascent and ridge return, two authored jump profiles, supported terrain extension beyond the former x=800 map boundary. Traffic stays assigned to existing streets. New mountain trees are spatially batched; old affected woods are rebuilt from complete tree colliders.
- Reference-informed low brick home, white porch/trim/shutters, shallow roof, exposed lower rear level, elevated deck/lattice/chimney, lower pool/stone landscaping, separate garage/kennel and smaller pool house at its left. Dimensions and unseen details remain approximate. House 1 remains absent; Houses 2/3 and Kyle remain.
- Test-only audio mute requires -racerTestSave and leaves normal play untouched. Runner also writes master=0 only into its newly created isolated test save. No player audio preferences were changed.

## Internal evidence so far

Current installed 0.14.0-review2 baseline: 744 Trickum traversals, 173 non-clean cases, zero failed local recoveries. Raw evidence: ../CR082-089/cr081-baseline. The probe forces Forest ambient-ramp runs to free roam, including its nominal race flag; those are not Forest-race mode proof. Prior CR-087 forward instability and reverse opposite outer-edge motorcycle failure remain open.

First new-geometry attempts exposed the old CircuitBoundary clamping the mountain vehicle back into the neighborhood. The initial probe also misread a wrapped nearest-route station as completion. All first-geometry results are therefore retained as invalid evidence of ramp success. Boundary restricted around the mountain and completion checks tightened before retesting.

Compiled boundary-fix candidate: 48/48 race speed-trap checks; 55/55 ledger/collection checks. Fourteen ledger fixture attempts verify top ten and tied ordering; one historical PB migrates once without a date. The collection run's saved file contains all 24 distinct IDs after ordinary-frame approaches; local resets returned supported positions. This is not yet cross-course/relaunch acceptance.

Rendered race trap and new jump result images were inspected. Speed result shows site/mph/medal/PB/new best. Fern Creek result shows 105.6 feet, 1.48 seconds, 470 points, medal/new best alongside a collectible notice. New centered forward ramps at 24 m/s: 8/8 traversals across all eligible Street vehicles, each with one award and successful recovery. Opposite creek approaches departed the narrow corridor: retained failures prompted broader landing support and tree clearance. Refined candidate validation remains pending.

## Compatibility and pending delivery

Authored categories: street-v13-exploration; lake-v6-exploration; street-reverse-v4-exploration; forest-reverse-v4-exploration. Layout must be frozen before final validation. Existing lap/race and activity historical data remain in their prior categories.

Target release: 0.15.0-review1. Builds/Latest remains the prior accepted runtime until final tests and complete packaging. No completion commit or human/controller/audio-listening acceptance is claimed here. Final report will supersede this progress note and retain failed cases.
