# Backyard Reverse local corrections — 0.47.0-review1

Safety checkpoint: clean main `2f830a934582f68fafc6e130706c09d1a921280d`. Dan accepts the overall Reverse course; this pass changes only the requested local areas and route presentation. No optional Reverse shortcuts.

## Route presentation and surfaces

**ROUTE COLOR REPRESENTS ROUTE ROLE, NOT GEOMETRY OWNERSHIP.** Every required main-route section uses the established main presentation, including sections with different Forward/Reverse physical paths. Only genuine optional shortcuts receive shortcut coloring.

The active minimap already consumes the entire Reverse RaceRoad as main. The final audit finds **373 main segments and zero optional segments**. Full-map overlay and atlas also consume this main route and zero branches; their design/behavior is unchanged. All five reported references have opaque established dirt vertex colors on their actual supporting ground.

The visible inconsistency came from a separately authored dark URP material and a sparse two-edge trail strip only 0.018m above irregular terrain. Its triangles cut through the terrain, and distance/exclusion rules left gaps at joins and approaches. Removed that redundant strip. The route now uses the established dirt vertex-color treatment directly on the terrain mesh, with sufficient local subdivision and one corresponding collision surface. No alpha overlay, raised substitute road, or paved trail. Actual final atlas and Reverse world-map layer regenerated.

## Three race-specific closures

Stacked visible timber beams block the inspected wrong Forward continuations, not the supplied reference points:

| Reference X/Z | Closure centre X/Z | Reverse centreline separation |
|---|---|---|
| 230.2 / -32.9 | 217.16 / -22.86 | 14.15m |
| 188.0 / 82.5 | 173.89 / 70.13 | 10.57m |
| 15.8 / -61.8 | 21.08 / -80.00 | 8.65m |

Exactly three closures. **No barriers at either color-only location (48.6/82.2 or 130.4/-92.4).** The component removes both visibility and collision in free roam and for a Forward course identity. The approved Forward scene is unchanged. One physical motorcycle contact per closure stops on the near side; maximum body-to-ground height 0.52–0.67m; reverse escape 28.59–36.14m. No invisible wall, penalty, reset or trap.

## Ramp approaches

- Near 188/82.5: local turn ends before X190, followed by roughly 62m of straight alignment toward the existing north-gully lip. Lip and receiving bank unchanged. Test completes the preceding turn near 22m/s, accelerates, then takes off at 31.58–31.61m/s. Motorcycle/ATV both land beyond the gully, 2.50s flight; heading error 0.50–0.52 degrees and lateral error 0.23–0.36m.
- Near 326/-11.7: gradual approach aligns with the existing pool-house axis by X334. The final drive exposed a short descending tip face that abruptly removed speed through suspension contact; replaced that local tip with continuous rising support and a clean departure edge. Launch metadata follows the actual edge at approximately X388.6 rather than X389. Intended landing remains unchanged. Motorcycle/ATV take off at 31.45–31.67m/s, land beyond the pool house, 2.22–2.24s flight; heading error 0.26 degrees and lateral error 0.02–0.03m.
- Only the two affected approach gates follow the local line shifts. Gate count, progression, completion, AI implementation and global physics/recovery remain unchanged. Existing AI road/flight metadata follows the corrected local geometry.

## Three unintended bump areas

| Reference | Actual cause and local correction | Final motorcycle / ATV |
|---|---|---|
| 416.6 / 82.1 | Abrupt cross-slope/crest join near X423–424 at the end of the old trail treatment. Locally rounded the profile while retaining its hill and grade. | 28m/s entry; air 0.16/0.14s; upward velocity 1.61m/s; minimum forward speed 27.21/26.67m/s. |
| 172.2 / -98.9 | Steep terrain shoulder joins at the south-gully runout; the protected Tree-Top landing edge also approached the north side of the driving corridor. Smoothed the local terrain join and shifted the Reverse line south by at most 1.4m. Tree-Top geometry untouched. | 28m/s entry; no airtime; minimum speed 28m/s; upward velocity 5.34/5.32m/s follows the uphill slope. |
| 235.0 / -31.3 | Sharp terrain faces where nearest-segment height blending switched between the two sides of the bend. Used one continuous local sloping support field, retaining surrounding grade and existing tree-foot support. | 23m/s bend traversal; air 0.26/0.20s; upward velocity 3.67/3.31m/s; minimum speed 22.21/22.72m/s. |

These are actual ArcadeVehicle motor/Unity collision simulations with scripted throttle and steering, not human/controller gameplay claims. First failed runs, diagnostic limitations and intermediate checks remain in `first-checks/`, `driving-checks.txt`, `recheck-driving.txt` and related CSVs. Final authoritative outcomes: ramps/northern bump in `recheck-driving.txt`; southern bump and barriers in `driving-checks.txt`; dump bend in `turn-driving.txt`. No full-lap or global AI tuning matrix was run.

## Grounding and protection

Compared 473 existing local collider supports against the checkpoint. One tree shoulder gained 0.183m; restored its original support locally without moving or removing the tree. Final map capture checks 739 added-woodland trees with maximum foot error 0.0000763m. No buildings, fences, gates, properties, water, approved shortcuts or other course geometry were moved. Terrain copies belong only to Reverse. Source preservation results are recorded separately.

Targeted driving checks have passed and stopped. Remaining subjective acceptance is Dan's gameplay review. No Backyard Reverse shortcuts added.
