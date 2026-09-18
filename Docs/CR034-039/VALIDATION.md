# Woodland arcade update — 0.6.0-review1

CR-034–039 are one combined delivery awaiting Dan's review. CR-040 remains backlog; no second circuit or lake was built. Safety checkpoint: `1cf4d6f0f3d3ab9da5e87ec331a704fea169f694`. The final build's `VERSION.txt` identifies its executable source commit; package verification and completion evidence are recorded alongside this report.

## What changed

- Seventh garage swatch: Black, persisted per vehicle. Body materials receive isolated property blocks; tires, glass, trim, riders and opponent shared materials retain their own appearance. Readable dark highlights remain.
- Original synthesized wood, chain-link, mailbox and sign Foley; three timbres and five pitch offsets, speed-sensitive gain, four spatial voices, 75ms onset cooldown. Sounds are prepared before racing. Master/Vehicle controls and pause/quit behavior apply. [Audio source/license notes](AUDIO-SOURCES.md).
- Dan's west-side chain-link; white large-X side/front stretches for Houses 2/3 with open central access; two extra roadside sections. Existing trigger yielding, 24 moving pieces, four-second cleanup and occupied-pad restoration remain. No solid fragment physics.
- Northern Hwy 92: 16.4m asphalt, four 4.1m lanes, supported shoulders, 70m transitions, edge/median/broken-lane marks, widened checkpoint spans. Traffic uses inner/outer lanes in both directions and a local 17→29m/s ceiling, reduced for bends/merges/obstacles. Racing passes within the highway carriageway. Neighborhood traffic retains the old base ceiling.
- Road-name signs use exactly Hwy 92, South Cherokee Lane and Jamerson Rd. Mapping follows the supplied area references: northern main corridor, eastern neighborhood, southern connector. Two-sided lettering fits the breakable boards.
- Three brown woodland routes with amber entrance signs and edge markers, retaining the old southwest cut. [Annotated map](map.html), [route coordinates](routes.txt), [all measured timings](ROUTE-TIMINGS.md).

## Route rules and directions

| Route | Entrance / rejoin road station | Legal bypass | Driving character |
|---|---|---|---|
| Creek Leap | 900 → 1500m; before CP4 on South Cherokee Lane | CP4, 5, 6 | Turn into the woods, straighten for creek takeoff, land generously, brake for the hilly rejoin. About 120km/h approach. |
| Fox Gully | 1580 → 2150m; before CP7 off Jamerson Rd | CP7, 8, 9 | Rolling descent, bends, road crossing and rising/crest exit. Enter about 85–95km/h; accelerate between bends. |
| Pine Ridge | 2220 → 3000m; before CP10 | CP10, 11, 12 | Descending woodland ridge with a longer diagonal jump and western-return rejoin. Enter about 125–132km/h; straight-ramp target 144km/h. |
| Existing Southwest Cut | 2603.9 → 2755.1m | No original gate skipped | Retained route and terrain; same evidence/recovery model. |

Stations are immutable centreline distances, not HUD distances. North is up on the map. Follow race direction down the eastern neighborhood, west through the southern connector, then north on the western return.

A directed entrance activates a per-racer tracker only at the expected checkpoint sequence. Contiguous forward travel earns evidence and maps branch distance to normal course distance for standings. Ground evidence includes supported shoulders; the airborne envelope is one metre wider. Large steps, off-corridor travel and unobserved forward jumps cannot earn progress. Reversing lowers current standing without increasing earned distance. Recovery seeks safe branch support at or behind current/earned progress, never ahead. Physical gates on shared entrance pavement still count.

Only a verified forward exit resolves the designated bypass list. Clean exits have no missed-gate buzz, missed-gate time or cut-distance charge, and normal tracking resumes. Leaving the branch for the ordinary road early or reversing out abandons the attempt: unvisited gates resume ordinary penalties. Entry alone is not a course-skip permit. Forward finish crossing and a complete gate sequence remain mandatory. The same tracker is used for player and AI; all autonomous stunt-selection flags remain false pending independent AI stunt-driving validation. Main-road fallback is deliberate.

## Difficulty and measurements

Easy/Normal/Hard now use 46/65/82% corner capability, 55/78/94% braking judgment, 91/98/100% speed targets and 26/29/31m/s steep-hill targets. These are driver choices within unchanged vehicle motors, grip, mass and acceleration. Removed the overly large minimum-curvature cap on straights and included the immediate bend in anticipation. No boosts, catch-up teleports or rubber-banding.

[Full race and sector tables](AI-RACES.md) retain clean and deliberate-mistake results. Six full mixed three-lap traffic races finished 24/24 racers, zero DNFs and zero autonomous racer recoveries; one Normal ATV missed one gate (+5s). The additional Hard traffic-off race finished 4/4, zero misses/recoveries. The manually injected player recovery is separately logged. Hard's mistake costs approximately six seconds on the first lap, but traffic encounters change later gaps; the Normal mistake run can finish faster overall. The automated reference driver is not evidence that a human difficulty target has been met.

All four vehicles were driven over every branch using two clean approaches, one deliberate brake/recovery attempt, and two matched road controls. The final motorcycle creek check resolves earlier landing-shoulder false rejections; earlier failures remain available. Creek and Ridge save several seconds. Gully's conservative advantage is smaller, around half to one second; faster completed car attempts saved about two seconds with more crest risk. See every outcome, including failed attempts, in [route tables](ROUTE-TIMINGS.md).

## Actual checks and evidence

- 77/77 rule/body/road-envelope fixtures: all four paint profiles, multiple-gate bypasses, no shortcut penalties, complete forward laps, entrance teleport rejection, off-route excursions, reversing and backward recovery. Standalone candidate5 and final targeted reruns are retained.
- 137/137 visible standalone virtual-input garage/race-flow checks: black preview/persistence, trim isolation, roster/grid, pause/settings, Quit Race, restart, results, records, ordinary cut penalties, finish abuse, all-profile local R/Y recovery, actual mailbox contact, cleanup and restoration. [Final release flow log](flow/standalone-flow.txt); previous candidate evidence is preserved in flow-candidate.
- 33/33 final release material/highway checks: finite bounded PCM, distinct variants, duplicate suppression, exactly four shared voices, live volume mute; all four prop materials yield once and restore; all four lane/direction traffic cases traverse both transitions. Highway peaks 27.40–28.85m/s, maximum highway lane errors 0.74–1.73m, low-speed time 0.13–0.15s per car. [System checks](systems/checks.txt), [traffic trace](systems/highway.csv); previous candidate evidence is preserved in systems-candidate.
- [House/shop transforms before](sites-before.txt) and [after](sites-after.txt) match. Local authoring touched 26 terrain tiles / 17,060 vertices and removed 115 corridor trunks; surrounding dense woodland remains. Shared visible/collision meshes and affected forest batches were refreshed. No legacy environment rebuild.
- Before/after ordinary-frame performance uses a separate, single visible 1280×720 player, VSync off, 120fps cap, same original-car roster and four traffic vehicles. Baseline: median 8.33ms / p95 8.35ms / peak working set 465.4MiB. Release: median 8.33ms / p95 8.38ms / peak working set 480.1MiB (+14.7MiB, 3.1%). Both 60-second samples had zero missed gates or recoveries. [Baseline](perf-baseline.txt), [release](perf-release.txt), [settings and memory](performance.json). Capped frame figures do not establish uncapped throughput; peak working set is process memory, not an allocation profile.

Initial failed geometry and controller variants are retained in editor-initial-failed, candidate1, candidate2-original and candidate3-*; they led to road-height blending, straight flight alignment, natural-crest braking and the supported-shoulder rule. The material suite caught missing mailbox prefab overrides; those are now explicitly saved. Hidden helpers do not produce useful visual screenshots; review the visible-flow, candidate3-original, candidate6-moto-creek and authored detail captures instead. Unity's generated YAML trailing spaces are left under Editor ownership; source/doc whitespace is checked separately.

## Final release verification

Executable source commit: `a4eaa16dd86e96a300cfc16a54c342e1ec043763`. Build succeeded with zero errors. The two build warnings concern Unity's future collision pre-baking requirement for generated meshes and the intentionally unconfigured Runtime Pipeline. Startup also retains the baseline's stripped depth-of-field/Panini shader warnings. No runtime exceptions were found in the final route, flow or systems logs.

The committed-source player passed 77/77 invariant checks and the final motorcycle Creek runs: clean 14.133/13.900s, brake/recovery 16.199s, road controls 21.197/21.170s. All five completed with zero misses/penalties; both clean runs and the recovery run resolved one legal branch exit. Earlier Editor/candidate road-control gate misses remain historical evidence, not final release failures.

The local ZIP contains 231 files, 78,596,925 bytes, SHA256 `B969D104DF46A80F335ED79C301DB3DC5F4D8317461726473D1AB1ECFBAF8957`. Extracted ZIP and complete Builds/Latest independently match every manifest hash. The self-contained Woodland-map.svg, controls, source metadata and licenses are included. Previous Latest is preserved in Builds/Latest-before-0.6.0-review1. [Package verification](package-verification.json).

Play-Racer.cmd was executed successfully: it printed the correct version/source and started the responding Builds/Latest/Racer.exe process. Startup reached graphics, physics, input and scene loading. The launcher smoke check did not drive or alter the user's records; automated tests used isolated saves. The final documentation/evidence commit follows the executable source commit without changing runtime source or scene.

## Limits and one review checklist

Fun, human difficulty, physical-controller feel and subjective smash mix still need Dan's review. Helpers use virtual input. Autonomous AI stays on the main road; Gully's safe time advantage is modest; excessive entry speed can miss a real road gate or spoil a landing. Historical records/settings remain intact under the new `street-v5-woodland` categories. No multiplayer, external upload or distribution.

- [ ] Launch Play-Racer.cmd; confirm 0.6.0-review1 and try Black on all four vehicles.
- [ ] Smash chain-link, white X fences, mailboxes and signs; check sound/volume and restart restoration.
- [ ] Drive Hwy 92 both ways, check merges/frontage and road-sign readability.
- [ ] Find all three amber trail entrances; compare clean runs, a poor landing and R/Y recovery without false penalties.
- [ ] Race mixed vehicles on Easy/Normal/Hard; judge whether mistakes and clean shortcuts create the intended challenge.
