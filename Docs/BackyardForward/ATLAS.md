> Current status: Dan approved Backyard Forward after the 0.45 review. Its historical anchors and atlas evidence below are preserved. The current separately selectable Forward/Reverse atlas is [here](../BackyardReverse/ATLAS.html).

# Dan's Backyard — Forward

## Approved course and two implemented optional shortcuts

**Dan approved Forward, including the corrected dense garbage dump, on 2026-09-29.**

Open **[TRACK_MAP.png](TRACK_MAP.png)** (5040 × 3750) to review the actual final geometry and both optional shortcuts. It combines a fresh orthographic render of the current saved `DansBackyardForward.unity` scene with its exported route and checkpoint coordinates. Trees, terrain, roads, property pavement, buildings and dense refuse are the actual implemented world. Teal is the main route, white arrows show travel, blue circles identify CP1–8, and amber diamonds identify major jumps. The grid is Unity X/Z in metres at 50 m intervals, with +Z up. The line indicates the navigation path, not corridor width.

The current scene render before annotations is [SHORTCUT_OVERHEAD.png](SHORTCUT_OVERHEAD.png); [map-scene.json](map-scene.json) contains the freshly exported route, road and gate data. Tools/Capture-ShortcutTrackMap.cs and Tools/Render-ShortcutTrackMap.py reproduce this documentation from the saved final scene. The render itself does not modify the scene. Gold lines/arrows are optional routes; gold circles are entries and squares are rejoins.

**Tree-Top Trail and Abandoned Cabin Jump are revised for 0.43.0; Dan gameplay review is pending. Reverse remains NOT YET IMPLEMENTED.** [Current technical evidence](../ShortcutRevision/VALIDATION.md) and [final geometry](../ShortcutRevision/geometry.json). Previous shortcut evidence is historical.

| Shortcut | Entry X/Z | Rejoin X/Z | Length | Bypassed main | Approximate clean saving |
|---|---|---|---|---|---|
| Tree-Top Trail | 218.677 / -5.618 | 169.336 / -101.692 | 110.432m | 144.710m; CP2/3 | 1.84s versus historical unchanged-main6.80s; final shortcut4.96s |
| Abandoned Cabin Jump | 183.793 /78.258 | 309.951 /95.375 | 127.856m | 151.130m; CP6 | 0.80s (5.06 vs4.26s) |

Tree-top dirt launch is15–29m into its branch, followed by a4m gap, a wider33–48m receiving platform, narrow3.3m rough bridges and small platforms braced to four existing trees, then an exit drop from98m. Six leaning boards at22–32m reach the cabin roof at32–41m; one welded collision strip preserves measured momentum through a1.96s bush-clearing flight. Dense shrubs/ferns cover the ground under and around both stunts; ground resistance is traversable and inactive on the main trail, platforms and airborne vehicles. No further existing trees removed; main route, gates and terrain preserved. Ground checks and a disclosed cabin-side automated egress limitation are in the validation record. Times are representative local checks, not guaranteed human lap improvements.

The [full-world visual atlas](../WorldMap/README.md) and richer full menu map are now implemented from actual saved geometry. The documentation master combines regional scene sources with exact X/Z registration; each menu map uses its exact active scene. The approved small racing minimap remains unchanged. Tree-Top foliage was locally cleared to at least 4.294m from the main-route centerline; resistance and shortcut geometry are unchanged. See [targeted validation](../WorldMap/VALIDATION.md).

[Forward main route and nine anchors](Forward.svg) is generated from the saved `DansBackyardForward` scene geometry. It shows the physical dump, the two crossings of one continuous ravine, and the return onto existing South Cherokee Lane. That historical SVG shows the main route only; use TRACK_MAP.png for current shortcuts. No Reverse is implemented.

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

## 0.45 world presentation cleanup
Visible temporary anchor markers removed; all nine coordinates retained in [reference table](../WorldCleanup/REFERENCE_ANCHORS.md). Forward route, dump, ravine, Tree-Top and Cabin geometry remain accepted and unchanged. Map refreshed for added woodland and corrected property names. **Backyard Reverse is upcoming, not abandoned, and not built in this pass.**

