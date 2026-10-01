# Mountain Reverse / Phase A

Safety baseline: clean main `b9041b01a5598a3fa1ce05a44f7de9a7ab59237f`.
Phase A targeted gate completed before implementation of Phase B.

| Issue | Root cause and local correction | Targeted evidence |
|---|---|---|
| A1, 814.9 / 118 / 68.4 | Overlapping pavement fans and secondary support contacts on the climb; refit local endpoint-continuous grade and retessellate the original pavement footprint. | Motorcycle air 0.02s (initial placement), ATV 0s; both complete, zero resets/recoveries. Baseline air 0.82 / 1.18s. |
| A2, 874.2 / 113.7 / -91.2 | Malformed support curtains and overlapping road fans exposed a void. Continuous local mountain shoulder and one coherent paved join replace them. | Main and affected branch traversals pass both vehicles; support probes clear. |
| A3, 804.7 / 95 / -162.9 | Closed-cut slab wall obscured the sign. Replaced by lower irregular colliding rock outcrops. | Coordinate view shows sign clear; motorcycle/ATV adjacent main route traversed. |
| A4, 826.2 / 99.8 / -119.7 | Secondary support faces crossed the roadway; the driving mesh also contained a steep step. Remove offending support and rebuild the bounded merge surface with a gradual grade. | Main 1480–1675 passes both vehicles with zero airborne time/resets/recoveries. |
| A5, 1276.6 / 109.7 / -279.4 | Local tessellated grade transition; refit with endpoint-tangent continuity. | Baseline test did not reproduce a jump on the production-driver line; revised motorcycle/ATV checks remain grounded. |
| A6, 983.3 / 145.4 / -302.6 | Coordinate lies in the intentional Westbound Gully Flight; the receiving road lacked mountain mass beneath it. Add an irregular rock shoulder under the existing receiving road. | Receiving segment 2570–2710 passes both vehicles, zero air/resets/recoveries; landing support grid has no missing samples. Existing road and flight gap preserved. |
| A7, 748.1 / 85.7 / -111.2 | Artificial shortcut separator slabs. Replace with irregular colliding rock outcrops. | Coordinate view and adjacent motorcycle/ATV traversal. |
| A8, 965.8 / 165.7 / 131.6 | Stacked Summit screen slabs formed oversized towers. Replace grouped columns with grounded irregular rock outcrops. | Coordinate view plus both vehicles through Summit entry. |
| A9, 1001.7 / 163.1 / 103.8 | Rock-vault support facets protruded through Summit pavement. Remove only secondary faces within the existing road corridor. | Both vehicles grounded through affected branch station 8–30; support-clearance probes pass. |
| A10, 885.7 / 116.3 / -46.9 | Thin terrain/support curtains appeared as green shards. Replace bounded patch with continuous terrain; clear pavement contacts. | Coordinate view and affected branch descent pass both vehicles. |

`geometry-checks.txt`: 1,525 pavement probes, no support intrusions or missing pavement; lower tunnel checks clear; original main and Summit navigation coordinates unchanged; final rejoin tangent 0.344 degrees; old slab count zero. The old U-turn has not returned. Complete trees, signs and cairns were inspected/reseated (`dependencies-after.txt`).

Driving evidence: `baseline-runtime`, `initial-runtime`, `second-runtime`, `final-runtime`, `summit-final`, `merge-final`. Earlier failed candidates are deliberately retained. Final main tests and affected Summit descent both pass with zero resets/recoveries. The merge-only fixture's branch-selection assertion is inapplicable because it begins already on the branch; its actual entered/completed/physics checks pass.

Limitations: full-Summit production-driver attempts can lose control near station 275–313, before the bounded repair. This untouched section is not claimed fixed. A full flight approach also hit the existing AI airborne-recovery threshold before touching the landing; the receiving-road check therefore starts at the receiving pavement. No global physics, AI or recovery changes were made to force these broad attempts to pass. Automated checks establish local technical behavior, not Dan's visual/gameplay acceptance.

Standing Mountain rule: before filling apparent empty space beneath an upper road, verify whether a lower authored route needs that clearance; preserve its full corridor and support around it.
