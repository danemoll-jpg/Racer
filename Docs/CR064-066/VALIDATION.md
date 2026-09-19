# CR-064–066 combined review

Status: implemented, awaiting Dan's review. Windows release 0.10.0-review1. Safety checkpoint: `1192d22080a6439968ade7ee5aad9fa0ff96ce21`. Git staging initially failed on sandbox access to `.git/index.lock`; the supported elevated retry staged and committed the saved TODO changes successfully before edits.

## AI-only estimated classification

Settings > **Estimate AI at your finish** is persisted and defaults to Off. On finalizes unfinished AI after the measured human finish. Off preserves the existing full simulation and grace period; after finishing, Pause offers **Skip waiting / estimate remaining AI**. Resume continues normal AI racing.

`RacerState.IsAi` is explicit and defaults to false. Projections live in classification state, not RaceProgress: no gate crossing, lap completion, missed-gate charge, relocation or record insertion is manufactured. Existing measured finishes and DNFs are untouched. A second unfinished human prevents final results and cannot be projected. Repeated requests leave the first snapshot unchanged. Projected AI are frozen at their snapshot and removed from collision participation; restart replaces them with normal collidable simulated opponents.

Distance is remaining authored route length plus remaining full laps. Ordinary route position is constrained by the last earned/next required gate and the supported road corridor. Outside that corridor, only earned gate progress is used. An active recognized shortcut contributes its actual remaining branch length plus the main route after its exit. Unchosen future shortcuts are not presumed. Invalid/inactive laps conservatively count a full current lap. Neither straight-line distance to the finish nor heading is used as course progress.

Pace uses one-second reductions in that route distance, retaining up to 45 seconds of observations. Reverse/stationary/recovery samples, discontinuities and speeds above 115% of the profile's speed are rejected. At least six recent moving samples and movement within five seconds are required; the median resists a brief crash. Otherwise the cruising fallback is profile speed × 0.46 (Street) or 0.36 (Forest) × difficulty factor 0.80 / 1.00 / 1.08. Pace is bounded to 55% of that prior through 90% of profile speed. Remaining duration is at least 0.02 seconds. Physical elapsed finish is snapshot elapsed time plus remaining duration; existing incurred penalty seconds are added exactly once for classification. Adjusted ordering retains full precision and stable ties. Estimates do not predict future crashes, water delays, shortcut choices or penalties.

Results use both `~` and **Estimated**. The human's actual result and measured top-ten entry are retained. Existing record-category identifiers and timing rules are unchanged.

## Vehicle and driver visuals

All four profiles now use project-authored procedural models: beveled body/roof panels, continuous wheel arches, tire/rim/hub/spoke detail, headlights and tail lamps, bumpers, grille, handles/mirrors and glazed front/rear cabin openings. The longer wagon retains distinct proportions. Visible car occupants fit below the roof; hidden interiors are deliberately minimal. Bike/ATV models add frame rails, forks, swing arms, engine fins, exhausts, seats/tanks, controls, footrests and ATV racks/fenders. Helmets/visors, torsos, articulated elbows/knees, gloves and boots establish seated posture. Hand/control assemblies respond with restrained steering; the motorcycle retains its existing speed-dependent lean. No IK or detailed skeletal animation is claimed.

Fixed meshes are merged by shared material. Measured active renderers are Classic 20, GT 20, motorcycle 14, ATV 20, with 6–7 materials per vehicle and 7,996–8,820 triangles. Each renderer has one material/submesh: at most 20 vehicle color-pass submissions before culling/batching, with normal additional shadow passes. These are structural bounds, not a GPU draw-counter claim. The previous source has 15 / 16 / 16 / 24 renderers respectively (including wheel markers); maximum growth is five for Classic and four for GT, while motorcycle/ATV decrease. Eight materials are initialized/shared by the visual factory, including one unused reserved face material; none is allocated per paint change. Generated meshes have explicit runtime ownership: retiring a clone cannot destroy meshes shared with its source. This regression was caught in rendered inspection, fixed, and covered by a new clone-survival check.

Collider dimensions, rigidbody mass/center, suspension, speed, handling, profile eligibility and vehicle-contact asymmetry were not retuned. VehicleProfile, ArcadeVehicle, VehicleContact, course scenes, water and recovery source remain unchanged. No visual colliders are created. Ambient traffic retains its compatible original stylized bodies. No purchased/imported models or third-party art were used.

### 0.9.1 property-block investigation

The old blanket assertion required an empty property block on every non-body renderer. Diagnostics identify **only the six water-ripple LineRenderers**, for every profile, with Unity-owned nonempty blocks; `_BaseColor` and `_Color` are unset. There is no demonstrated trim/rider paint contamination. We did not clear legitimate water properties, alter water rendering, or exclude those renderers. The checks now capture every non-body renderer before painting and compare material identity/color, block presence, all shader-declared block values and paint-specific properties after painting. That checks actual paint isolation while retaining water coverage. All 28 profile/color invariance checks pass, and the formerly 133/137 flow suite now passes 137/137. The earlier failing reports are retained as diagnostic history.

## Radio text and timing

Actual folder names produce `<folder name> Radio` only for channel switches; root songs announce General Radio and Off announces Radio Off. Rescanning the same selected station does not introduce a channel announcement. Song text has only `Artist: ...` and `Song: ...`, with Unknown Artist and filename-without-extension fallbacks. It uses plain uGUI text (rich text disabled), grapheme-aware truncation at 48 text elements plus ellipsis, and the existing available font glyphs. The popup stays clear of the speed display and has exactly two visible song lines.

A station announcement has a 2.5-second readable hold. Existing track-start, metadata-refresh and on-demand song requests retain their five-second duration; requests during the hold begin after it. Failure messages retain four seconds. There is no periodic extra popup. Metadata results must match both the current path and load revision, preventing stale channel results, including rapid away/back switches. Controls, channel order/grouping, recursive scanning, shuffle/history, source selection, volume and staged-music policy remain unchanged.

## Evidence

`verified-art` and `verified-ai-*` contain the final-build appearance/AI checks and corrected captures; `release-radio` contains the final radio checks. Other `release-*` evidence remains valid for unchanged production behavior. Earlier candidate/v2/final folders are iteration evidence, not the delivered acceptance result. Synthetic tones and WAV metadata fixtures are locally generated test data. All saves are isolated under Temp or diagnostic folders.

| Check | Result |
|---|---:|
| AI estimates / Street | 96/96 |
| AI estimates / Forest, including shortcut and water | 96/96 |
| All four profiles × seven paints, physics isolation, clone survival | 116/116 |
| New radio formatting, real ATL-tagged WAV, async/rapid switches, natural next, Off, failures, long Unicode names | 24/24 |
| Existing complete virtual keyboard/mouse/gamepad flow, colors, recovery, runoff, restart | 137/137 |
| Forest eligibility, historical boards, traffic route and local recovery | 28/28 |
| Top-ten / record rules | 29/29 |
| Existing radio scanning/history/source/volume/control behavior | 16/16 |
| Street physical sign lettering / break / restore | 44/44 |
| Forest signs / exact cave warning / break / restore | 49/49 |

An obsolete three-station jump fixture gives 6/12 in both the unchanged 0.9.1 player and this player, with the exact same failures at 620/1020 on Street. All runs land; those sampled Street stretches do not satisfy its airtime expectation. These failures are preserved under `preservation-jumps` and `baseline-obsolete-jumps`, not erased or counted as passing. Current authored jump fixtures are recorded separately below.

Normal physical-pilot races with estimation disabled completed **6/6 checks on each course**: the human pilot and all three opponents measured two laps, with zero missed gates and one actual local AI recovery on Forest. These accelerated simulation runs establish gate/race behavior, not normal-rate rendering performance or human controller feel.

The current six authored Forest ramp locations completed **12/12 nominal-speed motorcycle/ATV runs** with takeoff, landing and forward completion. Reported airtime spans 1.62–4.30 seconds and final upright values 0.97–1.00. Initial placement/speed are diagnostic fixtures; subsequent motion is driven by pedals and suspension. Source and traces are preserved under `preservation-forest-jumps`.
The authored Street connector completed **8/8 ordinary-frame flights** across all four profiles at nominal and full-speed approaches. Every flight landed upright and credited gate 12; airtime spans 2.470–3.302 seconds. Seven had zero misses/penalties; the full-speed ATV run subsequently incurred one missed gate and the normal five-second penalty. A fresh unchanged 0.9.1 standalone run reproduces the same eight landings/connector credits and the full-speed ATV subsequent miss/five-second penalty (`baseline-authored-connector`). The same full-speed ATV miss/penalty is also present in the historical CR-041–045 `jamerson`, `jamerson-review1` and `jamerson-before-desktop` reports. See `preservation-street-jump/results.csv` and its physical traces; this existing high-speed behavior is retained rather than changing physics or weakening gate credit.

The standalone wrapper encountered one transient file-read exception while polling the completed flow report. The report reached DONE with 137 passes and zero failures; its owned idle player was stopped, the output preserved, and the remaining checks resumed. No user save/process was changed.

Final non-development Windows build: zero errors; one existing warning for absent optional RuntimePipelineConfig (the editor automation service is disabled in shipped players). Rendering measurements used 1280×720 explicit offscreen scene rendering at normal time scale, capped at 120 fps: Street 16,779 frames, median 8.334 ms / p95 11.899 ms; Forest 18,021 frames, median 8.334 ms / p95 8.568 ms. Prior 0.9.1 evidence measured medians 8.334 ms and p95 9.361 / 8.335 ms respectively. Current samples had 22 people on each course, compared with 15/16 previously; random populations and seeds differ, so the p95 increases are observed comparisons, not isolated attribution to vehicle art. Both current p95 values remain below the 16.67 ms 60 fps budget on this machine. Offscreen captures demonstrate rendered appearance and text, not Dan's subjective visual approval, actual listening or physical-controller testing. Garage capture explicitly renders its secondary preview camera because hidden standalone windows do not schedule that camera automatically.

## Dan's review checklist

- [ ] With Estimate AI On, finish ahead of near/far AI; confirm immediate Estimated standings and your actual measured result.
- [ ] With it Off, let AI finish normally, then try Pause > Skip waiting in another race. Reopen settings and restart/rematch.
- [ ] Inspect all four vehicles in garage and racing, including black; review silhouettes, wheels, occupants, hands/feet, steering/lean and camera clearance.
- [ ] Switch folder stations and Off rapidly; check station wording and readable two-line artist/song information, including untagged and long/Unicode names.
- [ ] Drive both courses, ramps, shortcuts and water; check signs/cave warning, wildlife/audio, neighborhood scenes, traffic, AI mistakes and local recovery.
- [ ] Confirm subjective sound balance and physical-controller feel on your own hardware.

Split screen remains feasibility discussion only. No networking, stunt challenges, ghost mode or external upload. All new work remains awaiting Dan's review.

## Package verification

The existing packaging workflow produced **429 files and 187 deliberately staged songs**, verified by SHA256 across the extracted ZIP, versioned Windows runtime and complete `Builds/Latest`. Older runtime/Latest/ZIP are preserved under `Builds/Preserved`; external music collections were not read/copied. `Play-Racer.cmd` is byte-for-byte unchanged. `SOURCE-SHA256.txt` enumerates 2,497 Assets/Packages/ProjectSettings files at final compilation. `package-validation-before-commit.json` records the pre-commit verification; after committing, VERSION.txt is stamped with both source IDs and the same packaging workflow is rerun. The authoritative final ZIP hash, full paths and verification are in `Builds/PACKAGE-LATEST.json` (build outputs are intentionally untracked). No external upload.
A final check launched from the packaged Builds/Latest runtime passed 96/96 AI/classification/persistence/restart assertions (packaged-smoke), with exit code zero. Final whitespace-only cleanup was included in the final compilation; production behavior was unchanged from the detailed acceptance runs.
