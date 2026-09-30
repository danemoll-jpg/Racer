# Western Gullies: implementation audit

## Identity and trigger

There is **no Western Gullies fast-travel landmark or bounded discovery region** in the seven shipped scenes. The player-facing `Western gullies: n/4` line is an **acorn collection grouping**. `DiscoveryAuthoring.Collectibles()` in `Assets/Scripts/Editor/DiscoveryDetails.cs` assigns this string to collection indices 8–11 (`i < 12` after the first eight), rather than deriving a region from terrain. Each scene's `ExplorationCollection.sites` is the authoritative saved runtime data.

`ExplorationCollection.FixedUpdate()` collects an individual site only in free roam, while the flow is Racing, with valid continuous movement and no save error. The swept previous/current vehicle position must pass within **2.8 m in 3D** of the site. Teleport-like movement is rejected. The individual stable ID is written to `woodland-acorns-v1.json`. Notification: `ACORN FOUND / <individual title>`, followed by the collection count. There is no separate “Western Gullies discovered” event.

## Exact sites and access

Coordinates below are the Street Forward saved data (the complete seven-scene export is [audit.json](audit.json)). Acorn centers are 1 m above supporting terrain.

| ID / notification title | Acorn X, Y, Z | Road access X, Y, Z |
|---|---|---|
| woodland-09 / Behind the pines | 22.024, 8.524, 499.764 | 20.275, 7.962, 529.713 |
| woodland-10 / Woodland nook | -351.022, 9.321, 502.413 | -349.960, 8.739, 532.394 |
| woodland-11 / Quiet gully | -600.639, 9.251, 150.319 | -630.635, 8.826, 149.854 |
| woodland-12 / Old stone corner | -365.014, 7.903, -531.187 | -368.338, 7.313, -561.002 |

The set spans approximately X -600.639…22.024 / Z -531.187…502.413; **these are the sites' extents, not trigger-region bounds**. The first two are about 30 m south of the northern highway, the third about 30 m east of Trickum Road, and the fourth about 30 m north of the southern road. Authoring places a three-stone cairn approximately 28% along each access spur and grades/clears a narrow route to the pocket.

## Full-map representation

The collection summary always lists the group and its count. Each individual acorn gets the existing small map marker only after collection and visitation of its cell (`ExplorationMap.Draw`). Western Gullies has no separate label, destination or travel point. The map improvement does not alter this behavior or reveal uncollected tokens.

## Physical terrain and reachability

Terrain screenshots, height samples, and targeted trigger results are recorded with the final validation. No terrain, trigger, collection ID, group name, or access spur is changed to make the name fit. The actual Backyard ravine is a separate feature and is not evidence for a western gully at these four locations.

Saved-world inspection found **no distinct ravine at these four collection sites**. The first two are pockets beside commercial highway buildings; Quiet gully is a shallow residential roadside area; Old stone corner is a woodland pocket beside the southern road and rising forest terrain. An 80 × 80 m grid sampled every 5 m gives terrain elevation ranges 7.426–8.849 m, 8.023–8.928 m, 6.225–8.954 m, and 5.639–28.531 m respectively. The fourth range reflects the adjacent rising slope; a height range alone is not evidence of a gully. These are actual mesh raycasts, not a guessed location.

Evidence: [height samples](western-terrain.json), views [09](western-woodland-09.png), [10](western-woodland-10.png), [11](western-woodland-11.png), [12](western-woodland-12.png). Terrain, roads, buildings, existing acorn access spurs, saved site IDs and trigger code are unchanged. Added general roadside woodland preserves the acorn access corridors.

**Recommendation for Dan's decision:** treat “Western gullies” as an inaccurate collection-category label; consider renaming/reorganizing it in a later task. Do not interpret it as the accepted Backyard ravine. No rename, removal, gully construction or trigger redesign was performed here.

## Trigger and access result

The fresh Windows 0.45 candidate collected **all four existing sites** using continuous grounded position samples from each documented road access to the acorn. All four normal `ExplorationCollection.FixedUpdate` swept-distance triggers fired and saved their original IDs. No trigger defect was found. A 0.65 m-radius capsule sampled along each approach found **no solid non-ground obstruction**. This supports normal motorcycle access; the test is a bounded positional/physics query, not a claim of human driving or an all-vehicle traversal trial. No collection logic was modified. The unrelated Anderson travel-point failure in that same fixture does not invalidate these four passing checks.
