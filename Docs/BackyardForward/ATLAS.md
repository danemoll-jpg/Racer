# Dan's Backyard — Forward

## Approved course: visual shortcut-planning map

**Dan approved Forward, including the corrected dense garbage dump, on 2026-09-29.**

Open **[TRACK_MAP.png](TRACK_MAP.png)** (5040 × 3750) and zoom in to plan entry/rejoin locations. It combines a fresh orthographic render of the current saved `DansBackyardForward.unity` scene with its exported route and checkpoint coordinates. Trees, terrain, roads, property pavement, buildings and dense refuse are the actual implemented world. Teal is the main route, white arrows show travel, blue circles identify CP1–8, and amber diamonds identify major jumps. The grid is Unity X/Z in metres at 50 m intervals, with +Z up. The line indicates the navigation path, not corridor width.

The complete scene render before annotations is [APPROVED_OVERHEAD.png](APPROVED_OVERHEAD.png); [map-scene.json](map-scene.json) contains the freshly exported route, road and gate data. Tools/Capture-ApprovedTrackMap.cs and Tools/Render-ApprovedTrackMap.py reproduce this documentation. No geometry, trees, roads, gates, navigation or gameplay were changed to make the map. No additional regional maps are needed at this resolution.

**Optional shortcuts: PLANNING NEXT, awaiting Dan's map review and chosen entry/rejoin points. Reverse: NOT YET IMPLEMENTED.** No proposed shortcut is drawn or built.

[Forward main route and nine anchors](Forward.svg) is generated from the saved `DansBackyardForward` scene geometry. It shows the physical dump, the two crossings of one continuous ravine, and the return onto existing South Cherokee Lane. No Reverse or optional shortcut route is shown or implemented.

| Anchor | Feature | X | Z |
|---|---|---:|---:|
| 1 | Start / finish | 463.6 | 8.0 |
| 2 | Hill start | 422.7 | 9.0 |
| 3 | Flat parking start | 409.5 | 9.3 |
| 4 | Pavement ends / dirt begins | 391.9 | 9.7 |
| 5 | Corrected dump launch | 309.2 | 15.7 |
| 6 | Dump landing | 258.6 | 8.2 |
| 7 | First big ravine crossing | 96.1 | -108.6 |
| 8 | Second crossing / jump back | 107.7 | 60.7 |
| 9 | Forest return | 456.4 | 67.4 |

All nine X/Z locations lie exactly on the saved route. The gate and compact starting grid are near anchor 1 on the existing straight driveway. The start gate is approximately four metres west of the anchor, allowing all racers to start facing straight into the driveway. Anchors 3–4 retain the accepted flat concrete apron. The initial dirt path transitions into a 5.3m usable wooded trail.

The approved dump has the corrected supported bowl and a level central floor at Y=61.5, with sloped escape boundaries. Its 2,100 overlapping non-colliding refuse pieces visually fill the floor/lower slopes, with the existing dump-only grounded resistance. Launch and far landing X/Z remain anchors 5 and 6. The original nine-object description and older dump image are historical; see the fresh visual map and [approved dense-dump evidence](../DumpRefuse/VALIDATION.md).

The ravine runs from approximately Z=-172 to Z=130, linking anchors 7 and 8. The first crossing travels west and the second east. Nominal excavated widths are 38m and 30m, with 12m/10m depth below the original terrain at those sections. Actual rim-to-bottom differences vary with the existing hillside; see [measured terrain profiles](terrain-profiles.csv). The floor follows the hillside and rises into shallow mouths at the ends.

The route joins South Cherokee Lane near Z=63 and follows the actual existing road centreline back to the driveway. No parallel return road was added. Teal arrows are conformed to the final ground and have no colliders. Ordinary checkpoints reuse the accepted blue template; the start/finish template appears once.

Rendered views: [start](start.png), [flat parking](parking.png), [dump](dump.png), [first crossing](big-gully.png), [same ravine](same-ravine.png), [second crossing](second-crossing.png), [wooded trail](woods.png), [return](return.png).

Historical [rejected Backyard atlas](../Backyard/ATLAS.md) is not the current design. The [rollback atlas](../YardReset/ATLAS.md) records the accepted starting baseline; its old anchor-5 coordinate and eight-marker plan are superseded by the nine anchors above.
