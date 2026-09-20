# CR-081 + CR-090 combined implementation

Safety checkpoint: 8419e9194985955bb56a41854f1b5dce7968b270. Saved changes were committed before modifications; elevated Git retries resolved sandbox index.lock permission failures.

## Implemented source and authored content

- Automatic race/free-roam speed traps and authored-jump landing results. Existing race timing/gate/penalty rules remain independent. Activity top-ten archive supports distinct ties, duplicate IDs, one-time historical PB migration with unknown dates, course/layout/vehicle/travel-direction categories, and controller menu access.
- Local clean-lap pose recording and interpolated visual-only playback; resets, discontinuities and missed gates exclude a lap. Faster compatible completed laps replace the current ghost; old layout files remain separate. Playback toggle is in Exploration / personal best. Sampling is approximately 17 Hz (fixed-step .05-second minimum), limited to 24,000 poses / 20 minutes per lap. Ghost time uses the gate's interpolated crossing timestamp; recording uses actual Rigidbody poses, never controller input simulation.
- Twenty-four persistent acorns across neighborhood streets and mountain trails; independent save file. No currency, upgrades, unlocks or penalties.
- Connected mountain east of Kyle's lake; creek ascent and ridge return, two authored jump profiles, supported terrain extension beyond the former x=800 map boundary. Traffic stays assigned to existing streets. New mountain trees are spatially batched; old affected woods are rebuilt from complete tree colliders.
- Reference-informed low brick home, white porch/trim/shutters, shallow roof, exposed lower rear level, elevated deck/lattice/chimney, lower pool/stone landscaping, separate garage/kennel and smaller pool house at its left. Dimensions and unseen details remain approximate. House 1 remains absent; Houses 2/3 and Kyle remain.
- Test-only audio mute requires -racerTestSave and leaves normal play untouched. Runner also writes master=0 only into its newly created isolated test save. No player audio preferences were changed.

## Historical implementation checkpoints

Accepted 0.14.0-review2 baseline: 744 Trickum traversals, 173 non-clean cases, zero failed local recoveries. Raw evidence: ../CR082-089/cr081-baseline. The probe forces Forest ambient-ramp runs to free roam, including its nominal race flag; those are not Forest-race mode proof. Prior CR-087 forward instability and reverse opposite outer-edge motorcycle failure remain open.

First new-geometry attempts exposed the old CircuitBoundary clamping the mountain vehicle back into the neighborhood. The initial probe also misread a wrapped nearest-route station as completion. All first-geometry results are therefore retained as invalid evidence of ramp success. Boundary restricted around the mountain and completion checks tightened before retesting.

Compiled boundary-fix candidate: 48/48 race speed-trap checks; 55/55 ledger/collection checks. Fourteen ledger fixture attempts verify top ten and tied ordering; one historical PB migrates once without a date. The collection run's saved file contains all 24 distinct IDs after ordinary-frame approaches; local resets returned supported positions. This is not yet cross-course/relaunch acceptance.

Rendered race trap and new jump result images were inspected. Speed result shows site/mph/medal/PB/new best. Fern Creek result shows 105.6 feet, 1.48 seconds, 470 points, medal/new best alongside a collectible notice. New centered forward ramps at 24 m/s: 8/8 traversals across all eligible Street vehicles, each with one award and successful recovery. Opposite creek approaches departed the narrow corridor: retained failures prompted broader landing support and tree clearance. Subsequent candidates and final results follow in VALIDATION.md.

## Final corrections and compatibility

Authored categories: street-v13-exploration; lake-v6-exploration; street-reverse-v4-exploration; forest-reverse-v4-exploration. Layout and source were frozen at `f1719c6448800607b0eb7bc613245878eb2ebe3f` before building and validating review2. Existing lap/race and activity historical data remain in their prior categories. Activity rules are activities-v2; ghost compatibility includes clean-lap-v1/handling-cr087-v1, course, direction and vehicle. Vehicle handling was not changed.

The final pass restores original terrain support around race roads and authorized shortcuts from the safety checkpoint. This fixes an introduced depression near the reverse shortcut without regenerating gates or changing branch arrays. The shore connection and mountain return routes follow supported terrain; obstructing old trees were removed as complete collider/crown/trunk groups, including the separate Forest capsule batches. Nearby fences and household scenario positions were regrounded. All four route-support audits have zero sampled obstructions; continuous physical driving is separately reported in VALIDATION.md.

The home no longer overlaps its retired batched predecessor. Street names were corrected by location, retaining the separate southwest TO Jamerson Road destination sign. House 1 and the unwanted road behind the hairpin remain removed. House 3 has a descending wooded entrance; Kyle's drive connects the neighborhood to both lake returns. Future Kyle photos are not a dependency.

Natural brief airborne crossings previously suppressed Forest speed cameras through the grounded jump warmup condition. Cameras now use their own swept crossing, velocity agreement, direction, rearming and reset guards. Jump landing guards remain separate. AI clones cannot feed their collision callbacks into the player's jump result.

Continuous review2 lake driving then exposed a second boundary problem: the shore route crossed the old street closure plane before reaching the mountain rectangle. Review3 exempts the authored lake-return corridors as well. It leaves race gate credit and penalty logic independent. Both mountain and shore loops now complete continuous driving in both directions, dry and with supported recovery. This final source is checkpoint `f3d732d676ef4f7f7ae57b329855cad4f4729a4e`.

Release: **0.15.0-review3**, compiled successfully with zero build errors on 2026-09-20 at 10:41:31 UTC. Runtime assembly SHA-256: `EE44F9706AC6E2274DFC6A9C27973EC8FB19118E9BC1C3A76D2FF7A6A6FB94B9`. Full source hashes are in release-source-manifest.json. Earlier review1/review2 manifests are retained separately. Complete runtime, Latest and ZIP packaging verified 437 files, 187 playable staged songs and two preserved M4A originals; older builds remain under Builds/Preserved. Final compiled evidence and unresolved cases are in VALIDATION.md; this implementation description is not human/controller/audio-listening acceptance.

Completion checkpoint: `7e70659cc41c41d3f124fc949f601443f17ba5c3`. Launch Builds/Latest/Racer.exe or Play-Racer.cmd. Versioned ZIP: Builds/Racer-0.15.0-review3-Windows.zip.
