# Dan's Backyard route atlas

Open [ATLAS.html](ATLAS.html) for separately selectable views, or [Forward.png](Forward.png) / [Reverse.png](Reverse.png) directly. Teal shows each direction's main route; gold shows optional shortcuts in either direction; dashed gold marks the underground drain. Reverse now has Logging Ridge and Storm Drain / Gully Jump.

The images use actual saved Unity scene renders and exported route/gate/flight coordinates. North is +Z. They show the dump, continuous gully, property geography, permanent McFadden water, major jumps and direction arrows. Forward retains its approved route, Tree-Top Trail and Abandoned Cabin Jump. Its original anchor coordinates remain in PROJECT_TODO.md and Docs/WorldCleanup/REFERENCE_ANCHORS.md.

Reverse starts northbound on South Cherokee Lane near the established property area. It follows the natural northern return through the forest to the westbound north-gully jump, then the west bank to the eastbound south-gully jump. It bypasses the dump through the wooded corridor around (231.4, -25.6), continues toward (345.6, -8.8), climbs the new pool-house ramp and lands into the existing property/driveway return to the street.

The receiving-bank points exported in geometry.json mark the supported corridor, not a forced airborne touchdown. Measured motorcycle/ATV landings are recorded in VALIDATION.md. The full menu map retains all existing physical roads, trails and shortcuts independently of which race overlay is selected.

Regenerate after geometry changes: Capture-ReverseAtlas.cs through Unity CLI, then Tools/Render-ReverseAtlas.py. For the full world map use Capture-ReverseWorldMaps.cs, Render-ReverseWorldMap.py, Render-ReversePermanentNetwork.py and Import-PermanentNetwork.cs.

## Standing route-color rule

ROUTE COLOR REPRESENTS ROUTE ROLE, NOT GEOMETRY OWNERSHIP. All required main-route segments use the same main-route presentation even when Forward and Reverse use different physical paths. Only genuine optional shortcuts use shortcut coloring.

Latest local Reverse corrections and targeted evidence: [VALIDATION](../BackyardReverseCorrections/VALIDATION.md).

Latest regeneration: Capture-ReverseShortcutsMaps.cs, Render-ReverseShortcutsWorld.py, Render-PermanentNetwork.py and Render-ReverseShortcutsAtlas.py. [Shortcut checks and limitations](../ReverseShortcuts/VALIDATION.md).
