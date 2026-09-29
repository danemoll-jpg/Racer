# Backyard Forward: two optional forest shortcuts

Implementation and bounded checks complete. Human gameplay review remains Dan's. Release delivery is recorded separately in PUBLICATION.md.

Safety checkpoint: clean main `79797bc57e49da9f5de6a8cb86f972c82bd672d4`. Reference: Dan's supplied Forest Shortcuts.png; black strokes were treated as conceptual corridors. No Reverse, world-map upgrade or general menu work.

| Route | Entry XYZ | Rejoin XYZ | Branch / bypassed main | Required gate entitlement |
|---|---|---|---|---|
| Tree-Top Trail | 218.677, 61.391, -5.618 | 169.336, 45.248, -101.692 | 110.432 / 144.710 m | CP2 and CP3 |
| Abandoned Cabin Jump | 183.793, 67.299, 78.258 | 309.951, 76.235, 95.375 | 127.856 / 151.130 m | CP6 |

Entry/rejoin stations on the immutable main navigation line: Tree-Top 260.638 / 405.348 m; Cabin 861.641 / 1012.771 m. Both entitlement planes are 20 m into the optional route. Actual final samples: geometry.json.

Tree-Top: offroad approach, curved timber launch from branch station15 to35, unsupported 12m gap, receiving deck from47, narrow 4.7m planks/bridges, occasional 6m tree platforms, grounded posts and diagonal braces, then a supported descent to the existing trail. Launch edge tapers to2.4m before widening to5.2m. The widest existing car body is2.0m; current Forest eligibility remains motorcycle/ATV. No rails or automatic fall-reset trigger. Existing recovery is unchanged. Launch lip Y61.175; receiving platform Y63.325. Branch-local AI targets26m/s for the approach and19m/s on the deck; global strategy, probabilities and vehicle physics are unchanged.

Cabin: wooded approach; tangent-continuous angled boards; small weathered timber cabin at stations32–41 (approximately one-quarter of127.9m); roof is part of the takeoff. Boards and roof share one collision surface. Grounded wall boards, boarded windows and the existing stylized materials communicate abandonment. No terrain flattening. Actual successful jump lands on the continued wooded route near station115; ground trail continues to the rejoin. No invisible launch force.

## Targeted results — stop testing

- **Tree-Top final:** clean corner-aware main segment6.80s, shortcut5.12s, approximately**1.68s saved (25%)**. Intentional entry flight0.82s;78 fixed frames physically supported on the elevated receiving/descent surface; no reset or missed gate, next required gate4. Exit trace ends supported with0.13m lateral error. Final evidence: checks/.
- **Cabin final:** main4.88s, shortcut4.30s, approximately**0.58s saved (12%)**. Roof crossed at racing speed;2.50s maximum continuous air; physical landing, zero reset/missed gate, next required gate7. Evidence: cabin-final-tree-flight/. Its tree-top results are explicitly superseded by checks/.
- These are short motor/physics-driven motorcycle comparisons from matched26m/s entry conditions, using pedals and steering. No position driving or global physics changes. Tree comparison includes corner planning and branch-local speed guidance. They are approximate segment timings, not a guarantee for every vehicle, speed or human line.
- One production RoadDriver commitment per final branch: both reach the rejoin area without recovery. Tree maximum continuous air1.84s; cabin0.96s. Commitment is test-only to exercise the branch; production probability is untouched. No full-race/AI consistency claim. Existing stuck/recovery mechanisms remain available.
- Deliberate tree-top fall:10.8m descent onto intact forest ground, four grounded wheels, zero automatic resets. Supported recovery behavior remains unchanged; no new trap walls or reset volumes.
- Dimensional checks cover motorcycle, ATV and both car bodies. No driving matrix or change to course vehicle eligibility.
- Both routes register in the existing RaceDirector.Branches data. Real movement earns the explicit bypass gates, valid progress and rejoin. The unchanged RacingMiniMap.CacheRoutes consumes those same active branch samples and renders the existing amber3px shortcut lines; no minimap redesign or new camera. Source/data integration verified; no separate new minimap screenshot claimed.
- **25/25 final grounding/surface/clearance checks pass.** Existing Takeoff/Landing contact conventions handle timber mesh internal edges. The final launch-edge taper retains the exact tested centreline and heights, with measured minimum0.11m separation from the5.3m physical main-trail corridor. No driving rerun after this edge-only clearance change. See grounding.txt and entry-shoulder.txt.
- All1,684 main route points, nine gates, six other courses, main terrain/roads, dump, gully jumps, properties and existing global systems preserved.49,426 existing scene blocks unchanged;14 individual existing tree objects removed, with only their corresponding vegetation components trimmed in two copied meshes. No forest-wide clearing or terrain height change. See preservation.json.
- Updated5040x3750 [visual map](../BackyardForward/TRACK_MAP.png) uses a fresh orthographic render and exported final samples; teal main, gold optional routes, arrows, entries/rejoins, takeoffs, major features and50m X/Z grid. Final structure captures: tree-top.png and cabin.png.

## Earlier failures retained

Initial unregistered timber mesh contacts caused speed loss; the tree launch also had an interpolation lip. Existing surface-contact naming, continuous roof support and smooth local profiles resolved them. The first fixture seeded gate progress before its setup teleport, which correctly invalidated that seeded lap; seeding now happens after settling. The first airborne tree trace crossed the branch endpoint without ever driving the deck; this was not accepted despite its overly weak initial traversal assertion. A stricter elevated-support check led to the level receiver and controlled descent. A main-route full-throttle departure was also rejected as a clean timing comparison; the final comparison uses corner-aware pedals.

Editor compilation/reload timeouts and duplicated temporary fixtures produced no accepted results. A guarded single fixture and temporary full scene/domain reload resolved this. Original Editor setting and the known test-induced VehicleGlazing blend mutation were restored. No unrelated traffic, physics, UI or recovery fix was shipped. Earlier reports remain in first-checks/, fixture-failure-checks/, tree-flight-miss/ and cabin-final-tree-flight/ for traceability.

Dan reviews fun, visual taste, difficulty and imperfect driving. Gameplay checks have stopped. Reverse, authoritative WORLD_MAP/full menu-map visuals and general menu cleanup remain deferred; the small racing minimap stays minimal.
