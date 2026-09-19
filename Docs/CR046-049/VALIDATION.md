# Combined review — 0.6.2-review1

Implemented; awaiting Dan's review. Safety checkpoint: `5cc1d94fe84a9035db34dc688588bee940dc986e`. Git staging initially hit sandbox permission denial; the authorized elevated retry succeeded. Nothing was discarded or history rewritten.

## Shortcut accounting / BUG-004 / CR-046

The exact fourteen-misses/about-six-notices incident was not reproduced. Source inspection found throttled/replaced feedback and route-context-dependent credit that could hide charges or lose entitlement during imperfect travel. These are corrected; the prior automated success is not treated as Dan's acceptance.

RaceProgress.Miss is the sole production charge mutation: one ordered genuine gate miss is five seconds, advancing the expected gate prevents duplication. Restart clears the ledger, count and totals together. Each entry records lap, checkpoint, reason, branch context, time and seconds. Pause/results show four readable entries per page, reconciled totals and page count. Charges have a separate five-second aggregate banner; sound throttling cannot suppress their count. `-raceTrace` opts into transition/penalty logging outside the HUD.

Recognized entry immediately grants only the route's explicitly authored contiguous bypass gates. The earned credit cannot be revoked by a fall, deviation, stop, reverse or local recovery. Branch position is a separate recovery/standings aid. Forward exit completes the branch; sustained separated main-road travel or a rejoin beyond its exit deliberately ends context. Neither transition retroactively charges bypassed gates. Later unrelated gates, teleport invalidation, forward swept passage, bounded airborne envelopes and armed finish/lap requirements remain enforced.

CP07 is at1540m; CP10 at2185m; CP11 at2545m; CP12 at2830m; CP13 at3080m. Tight gaps between adjoining shortcuts give35–40m clearance; other exits have more. Bypass lists were regenerated from the relocated gate stations. Record category `street-v7-entitlement` keeps these runs separate while preserving historical records/settings.

## House momentum / BUG-008

Reproduced before tuning with ordinary pedal/stick driving and contact telemetry: motorcycle entrance speed29.77→16.65m/s, zero braking, a2881.5N·s supporting-floor impulse. Speed recovered inside then fell to17.90m/s at the launch lip and17.65m/s after takeoff. The continuous house floor was missing from the existing internal-edge supporting-normal correction; the lower exit facade extended beyond the launch surface.

Registered that floor with the existing correction and inset/lowered only the lower facade beneath the lip. No boost, motor/difficulty change, suspension retune or global collision disable. Standalone follow-up measured29.85m/s before entry,29.75 inside,29.53 at lip and29.02 after takeoff. Repeated motorcycle, ATV and both-car route runs finish successfully. Local recovery, breakable-glass restoration and vehicle-class collision rules remain in place. Raw contact evidence: `gully-motorcycle.csv`, `gully-atv.csv`, and `../CR034-039/combined-baseline/gully-contacts.csv`.

## Tire sound / CR-047

Lower useful slip onset, stronger progressive tire gain and grounded forward-braking feedback; loose surfaces favor gravel scrub, small vehicles use a distinct pitch. Reverse acceleration and airborne travel do not trigger braking squeal. Existing Master/Vehicle controls remain. A final listener ceiling bounds exceptional peaks without changing ordinary levels. Actual tire-source samples and listener DSP were captured while engine/radio played; this is playback evidence, not just event counts. See `standalone/checks.txt`, `mix.txt` and `engine-tires-radio.wav`. Subjective audibility/satisfaction on Dan's speakers remains pending.

## Roads / CR-048

Four continuations at the actual junctions: Hwy92 east/west, southern South Cherokee Lane, western Jamerson Rd. Supported road ribbons follow the existing terrain; shoulders, markings, raised closure barriers/fences and readable gantries communicate the boundary. Separate decorative traffic operates beyond the closure between covered recycling endpoints, never through the closed barrier. Original race/traffic paths remain. Height-independent boundary enforcement precedes race sampling and rejects outside recovery candidates; legitimate centreline and shortcut driving remain inside. The first boundary probe exposed overlap with the highway turns; moved closures outward and connected aprons before delivery.

Existing-transform audit:13658 original transforms, only six changed positions (five gates and the intended lower house facade). Houses/yards were not moved; no original terrain edits. `highway-junction.png` is a rendered visual check. No new playable track.

## Radio / CR-049

See [RADIO.md](RADIO.md) for setup, controls, formats, behavior and licenses. One streaming clip/request,2048-path library,32-track previous history, bounded4MiB metadata reads and one metadata worker. MP3/Ogg file cap256MiB; WAV64MiB. Directory scan and metadata work are asynchronous; a cancelled request cannot install stale audio. Shuffle avoids immediate repeats, including after previous-history navigation. Local paths only, read-only metadata/originals, plain-text tags. Default folder creation, folder chooser/open/rescan, Music volume and radio preference are integrated into Settings. Music continues consistently through pause/menus/results/restart/Quit Race; Quit Game stops it.

Generated MP3/WAV/Vorbis tones with tagged/untagged examples verify decoded output in the standalone. Empty/missing/corrupt/removed files were tested. The first DSP read is primed before measurement: an earlier test reported a false first-track failure for both WAV and MP3. That was a measurement-buffer issue, not established decoder failure. The experimental custom WAV reader was removed; final playback uses Unity's native decoder.

## Validation results

* Accounting/reconciliation/recovery-boundary invariants: 116/116.
* Standalone combined gate/boundary/radio/actual-DSP checks: 99/99; raw labels distinguish swept fixtures from real FixedUpdate crossings.
* Preservation flow: 137/137 (virtual keyboard/gamepad/mouse navigation, selection/colors, mixed profiles, local recovery, settings, destructibles, restart/results cleanup).
* Physical route matrix below:96/96 finished,64 branch exits,32 road alternatives,32 local recoveries, zero missed gates/penalties. Initial pose is seeded; subsequent motion uses normal frame/FixedUpdate pedal/stick inputs, real contacts and production race sampling. No trajectory teleport or velocity forcing during the drives.

| Vehicle | Runs | Finished | Misses | Local recoveries | Max branch deviation |
|---|---:|---:|---:|---:|---:|
| original | 24 | 24 | 0 | 8 | 8.16 m |
| tourer | 24 | 24 | 0 | 8 | 8.09 m |
| moto | 24 | 24 | 0 | 8 | 10.82 m |
| atv | 24 | 24 | 0 | 8 | 9.72 m |

* Additional measured departure/reverse/local-recovery series: 16/16 finished with zero misses; see `../CR034-039/combined-reverse-final/`. Negative body-forward speed is recorded before recovery (some Pine Ridge departures are airborne/backwards-facing, not grounded reverse driving). Physical departures reached35.2m lateral; motorcycle Fox Gully departure was airborne with up-vector0.50. Completion is recorded in CSV, never inferred from the rule count.
* Three-lap Normal race, mixed original/tourer/motorcycle/ATV and20 existing traffic cars:4/4 finished, zero misses, one original-car recovery. Race times443.005/439.404/404.458/422.683s. Difficulty constants unchanged. `mixed-race/results.txt` includes all lap times.
* Scene positions audited; penalty page/music Settings rendered and visually reviewed at1280×720. New player build succeeded with zero errors; current build warning is the intentionally absent optional Runtime Pipeline configuration. Earlier full asset builds also reported Unity's future collider-prebake warning.

The physical matrix/full race ran the same course, collision, vehicle and entitlement implementation before final radio/probe-only refinements; final combined checks run the delivered candidate. Flow/reverse checks use the same gameplay implementation before the last radio/UI refinements. Invalidated Editor runs after domain reload are not counted. Prior baseline/intermediate results are retained separately. Hidden standalone frame timings are not foreground GPU/FPS acceptance: `skip-rescan-frames.txt` reports observed callback delays, and full-race timing is diagnostic only. No noticeable-hitch guarantee for large personal libraries or a specific audio device is claimed.

## Remaining review / combined playtest checklist

No physical controller, subjective listening, native folder-picker interaction or Dan acceptance is claimed. Scripted swept deep-fall fixtures supplement physical departures/airborne/recovery runs; these do not exhaust every human line. The exact earlier14-miss incident remains unidentified. Keep BUG-004/008 and CR-046–049 awaiting Dan review.

1. Drive every shortcut wide/airborne, stop/reverse/fall and use R/Y locally over multiple laps. Bypassed gates should stay free. Deliberately miss an ordinary road gate: one+5s; pause/results count and ledger must agree with notifications.
2. Repeat the glass-house jump with motorcycle/ATV, then both cars. Check entry/lip momentum, glass impacts, landing and recovery.
3. With engine and personal music playing, slide/brake on road and loose ground. Judge squeal prominence; normal straight travel/flight should be quiet. Try Master, Vehicle and Music volume/mute.
4. Inspect all three road continuations (both highway ends). Try driving/jumping/following traffic/resetting outside; closures must hold without false race charges.
5. Open Settings/Music, add local songs, Rescan and choose another folder. Try D-pad right/left/up/down and ]/[ /I/M during driving; menu navigation must remain intact. Test skip/rescan hitches and artist/title display with your files.
6. Recheck garage/colors, mixed opponents, current difficulty, forest routes, traffic, destructibles, restart/Quit Race/results, and historical saves/records. Photo-based/exact house placement and lake/woods track remain backlog; no multiplayer.

Packaging: see `PACKAGE-VERIFICATION.txt` and the versioned runtime's VERSION.txt/SOURCE-SHA256.txt. The complete runtime and ZIP are kept locally; nothing is uploaded or externally distributed.

Final review also corrected Music Settings so asynchronous scans, folder selections and track metadata refresh in place without resetting menu selection. The packaged Latest runtime passed its standalone 77/77 rule/boot smoke check. Unity-generated scene YAML retains empty-name trailing spaces; code/documentation diff checks are clean. Corrupt-audio tests intentionally emit FMOD unsupported-format diagnostics; no unhandled exceptions occurred.
