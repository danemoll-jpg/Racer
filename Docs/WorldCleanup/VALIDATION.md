# World / map / presentation cleanup — 0.45.0-review1

Baseline: clean main `132bc5719436a5cebdcc76c3c16d1816e088ee13`.

## Implemented scope

- Removed only the temporary Backyard anchor-validation object subtree in every shipped scene where it remained. The current Backyard scene had already removed its copy. All nine coordinates remain in [REFERENCE_ANCHORS.md](REFERENCE_ANCHORS.md), the existing atlas, authoring array and runtime Forward course references. Backyard Forward and its shortcuts remain accepted. **Reverse is upcoming, not abandoned; no Reverse course was built.**
- All seven scene `courseName` strings use the existing standardized playlist display table. Default RaceDirector value is `Street Loop - Forward`. Course IDs, scene IDs, playlist indices, save and record keys are unchanged.
- Moll's keeps the old `home` discovery ID. Roger's uses existing House 2; McFadden's uses existing House 3; Anderson's uses the existing Kyle house/property, with the old driveway direction sign renamed. No house transforms or geometry changed. All four map/travel labels point to supported frontage positions; new `property-*` IDs inherit old visited map cells, while unvisited properties remain hidden. Acorn IDs remain unchanged.
- Added 720 irregularly distributed woodland trees per scene, plus 22 focused Anderson frontage trees. Existing tree meshes/material style reused; extra render geometry is spatially batched. Tree trunks are seated by raycasts against final saved `Ground_` meshes. No terrain reshaping. General candidates exclude roads, driveways, branches, gates, acorn access spurs, buildings and the full protected Backyard stunt/dump/ravine/recovery zone. The focused frontage pass uses road/driveway distances and actual collision queries on the existing slope.
- Full menu map uses a shared 5200 × 3120 world image and muted permanent-network traces from all existing course data, independent of current race. The optional current route overlay remains separate. Existing landmark/acorn discovery rules, visited-cell dimming, waypoints, panning, zooming and travel remain. The small racing minimap is unchanged.
- Complete Race uses the existing submit action's display strings: keyboard Space and, when present, the connected controller's south-button label. Button and waiting banner explicitly describe the action. No new input scheme, estimator, completion handler or record behavior. Existing visibility guard is retained.
- Western Gullies remains audit-only. See [findings and exact coordinates](WESTERN_GULLIES.md).

## Evidence completed before build

- [preservation.json](preservation.json): all original scene blocks preserved except authorized marker removals, display/landmark fields and renamed sign text. All existing track mesh assets unchanged. Race flow, route/gate/shortcut/AI/physics/recovery and minimap code preserved. Initial line-ending/encoding mismatch in the checker was corrected; `git diff --exit-code` is used for protected source.
- [implementation.json](implementation.json): exact added positions, removed-marker renderer counts, property destination positions, per-scene totals.
- [anderson-screen.json](anderson-screen.json): 22 additional frontage trees per scene. Initial broad clearance pass left only 2–4 frontage trees and the roof exposed; visual inspection prompted the bounded final frontage pass. Final [road view](anderson-road-final.png) shows substantial tree screening; [overhead](anderson-overhead-final.png) confirms the driveway remains open, with sign/entrance retained. No endless subjective tuning or driving matrix.
- [western-terrain.json](western-terrain.json): actual mesh height samples and four inspected views. No recognizable ravine unifies the four acorn sites. The collection grouping is index-based; no separate region trigger or destination exists.
- Initial audit JSON export hit a Vector3 self-reference error; explicit XYZ serialization corrected it. Original diagnostic retained in `audit-error.txt`; successful complete seven-scene data is `audit.json`.

## Remaining targeted checks / delivery

Map capture, full-map/UI/finish/discovery fixture, final Windows build and publication evidence are recorded below when complete. This file does not yet claim those results. Physical controller/human driving and subjective density acceptance remain Dan's review; no full-race/all-vehicle/AI matrix is planned.

## Final authoring/map checks

All seven scenes have **742 added trees** (720 general + 22 focused frontage). Every added trunk was raycast-checked against its final supporting mesh during capture; maximum error **0.000106812 m**. `grounding-checks.txt` retains the initial Backyard check and completed seven-scene pass. No floating trunks were found in inspected views. World capture's first PNG overwrite failed with Windows error 1224; same-directory atomic replacement resolved it without geometry changes (`map-capture-initial-error.txt`).

The regenerated master and Backyard atlas were visually inspected. `network-inventory.json` records **24,616 unique exported segments**, including all established shortcut sources. Shared menu bounds X -1350…1650 / Z -850…950; separate current-course overlay remains unchanged. Per-scene images and all three regional master sources regenerated. PNG/atlas writes use atomic replacement where Windows mapped-image handles prevented truncation.

The final focused runtime fixture uses the fresh Windows build, isolated saves, muted audio and `-racerSkipTitle -worldCleanupCheck`. This avoids the prior documented headless Editor startup/capture stalls. It verifies the real Space submit action, bound controller label, finish guards/results, shared map alignment/controls, property arrivals, migration, and the existing four acorn triggers. Results will be recorded before publication. This is not a new AI/driving benchmark.
