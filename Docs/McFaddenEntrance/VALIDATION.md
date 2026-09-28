# McFadden / Rocky Way Acres entrance — 0.35.0-review1

Dan's authoritative Street Loop reference is **X=511.6, Y=81.0, Z=-139.5**. This is House #3 / McFadden's property.

The existing Rocky Way Acres sign previously followed an obsolete entrance relationship, approximately (488.90,86.16,-124.48). It now marks the beginning of the current driveway at (508.381,83.556,-134.554) in Street Loop. The original board, lettering and two-post design remain; no second sign was created. Posts straddle the opening with the board overhead, face the road, and are individually grounded 0.06m into terrain. Horizontal post-centre distances from the main road centre are 7.924m and 5.804m; with road half-width 4.5m and post half-width 0.2m, the minimum road-edge clearance is 1.104m.

Only the road-side driveway render mesh is clipped at the existing main-road edge (80 affected triangles per scene). Its original collision mesh remains unchanged, preserving the accepted smoothly drivable transition. No main-road, terrain, route, driveway alignment, vehicle physics or unrelated object changed. All six existing property instances receive the same two local corrections, preserving Street Reverse's distinct driveway alignment and Laurel geometry.

Final visual inspection: existing sign corresponds to the current driveway mouth, posts grounded, entrance and main lane clear, road overlay removed, continuous supported seam without a hole. [Road view](after-road.png) and [overhead view](after-overhead.png). Historical views are retained as before-road.png and before-overhead.png.

One muted motorcycle drive-through used the production vehicle and colliders with ordinary 0.02-second simulation steps. Passed from the road through the entrance to 12m into the driveway; minimum measured speed 5.939m/s after settling, maximum continuous airborne time 0.02s. No collision lip, sudden slowdown or ramp introduced. This is a bounded automated physics traversal, not a claim of human gameplay acceptance. No further gameplay testing was run.

Saved-scene object comparison against clean checkpoint `eb65014b114f3e86fe5126b3fcb26b2f6b0f3110` confirms exactly four changed blocks per property scene: sign transform, two post transforms and driveway MeshFilter. Every collider, road, terrain and unrelated scene block is identical; no objects added/deleted. See preservation.json, post-checks.txt and drive-through.txt.

The initial inspection command exceeded the CLI's five-second response window while the editor finished normally; its completed captures were used without rerunning the inspection. The drive fixture's incidental vehicle-material and physics-settings serialization changes were restored to checkpoint contents before completion. No accepted system was reopened.
