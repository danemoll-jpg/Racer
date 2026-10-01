# Forest Forward cave obstacle restoration — 0.60.0-review1

Safety checkpoint: clean main `0b94bd492ac7a4ff45d50a251b2035428ec41de6`. Current request supersedes the previous review stop. `CODEX/_RULES.md` in the pasted request does not exist; the mandatory root `CODEX_RULES.md` was followed.

## Exact history restoration

Accepted obstacle source: `5985a27c17e9245c5aa3e44727c1cd5a72bba5dd`, **0.58.0-review1 / game-58000**. Its authoring source is `Tools/Author-FinalTwo.cs`; mesh assets are `Assets/Track/FinalTwo`. The later `e08d19e1` / 0.59 authoring scaled rock heights to 0.65, 0.49, 0.52 and 0.52 of the original. The four original mesh assets and matching MeshCollider components remained unchanged.

Restored the actual historical local positions, rotations and full scales of **Wall-side fallen slab, Central fractured boulder, Opening-side fallen stone, Broken rear slab**. No replacement obstacle was designed. Heights return to approximately 3.9, 3.3, 2.65 and 2.25m; original footprints, station 207–217, supported bases and approximately 3.8m right opening remain. All four retain their exact visible mesh collision. `history.json` records extracted source values and `preservation.json` verifies the saved result against history.

The original AI rightward alignment (+3.05m with approach/rejoin transitions), local 8m look-ahead and rockfall recovery exclusion were already retained in 0.59 and are byte-preserved. No global AI/vehicle/recovery changes. No Reverse routing, arrows or checkpoint changes.

## Visual correction and collision audit

The restored full-size obstacle passes current chase-camera visibility and exterior containment checks, so **no additional shrink, displacement or substitute slab was necessary**. Removing the surrounding eye-level decorative ledges clears the sight line while retaining the actual obstacle difficulty. Current cave shell, ceiling, natural appearance, light, ambience, floor, jump, entrance/exit, route and wooded hillside are unchanged. Terrain and its grounded dependents are unchanged.

- **A — Shell:** existing connected enclosure/collision and ceiling detail retained. No shell holes cut or leaks hidden with added rocks.
- **B — Cave-in:** four historical substantial rocks, matching solid MeshColliders; no invisible replacement boxes or obsolete colliders.
- **C — Edge hazards:** 166 old wall ledges recomposed into 45 grounded, colliding boulders; 121 former decorative pieces removed. Independent deterministic variation in station, side distance, yaw and dimensions; stretches around the cave-in and jump/landing remain clear. Four existing faceted rock meshes supply steep wheel-height faces and bevelled crowns. No neat paired rows or new slalom route. Visible dimensions approximately 1–1.85m wide, .85–1.4m tall, 1.2–2.7m deep. Collision uses the same mesh and transform.
- **D — Rubble:** 166 small decorative stones grounded, with actual vertex height capped to .16m and local horizontal extent .65 by .8m; may remain non-colliding. Initial renderer-bounds height adjustment left tilted stones too tall; final correction uses yaw-only orientation and actual vertices. No large non-colliding ground stones remain.

All 49 substantial rocks have matching enabled non-trigger mesh collision. All 6,468 sampled rock vertices lie beneath the existing hill. All 45 edge boulders are supported. The ATV line clearance envelope has zero edge-rock contacts. Existing recovery validates ground support and collider overlap; cave-in exclusion remains active and no new recovery point was authored.

## Bounded verification

- Motorcycle and ATV: aligned passage complete at 27m/s target, minimum 26.76m/s, upright dot .998, no airtime. Both poor straight lines physically stop at the rockfall, remain upright (.998), no airtime; minimum speed approximately zero.
- Three representative ATV edge contacts (boulders 6, 23, 40): real blocking/deceleration to approximately zero; upright minimum .992–.995, no airtime, maximum upward speed 1.30m/s. No ceiling launch or flip.
- Production RoadDriver ATV: existing probabilistic Forward shortcut selection succeeds; full cave entry to exit reaches station 529.8/533.7, zero resets and zero AI recoveries, maximum lateral deviation 1.15m. The existing intentional cave jump produces 1.58 seconds airtime. No global tuning.
- Normal motorcycle/ATV ChaseCamera approach views at stations 185/197 inspected; opening visible in advance. Interior views at 110/180/195/203/270/430 and entrance/exit inspected; centre line remains readable. Three normal outside hill viewpoints show no cave tube, rockfall protrusion or internal construction meshes. Light terrain shoulders are existing supported ground, not new shell openings.
- All route/checkpoint/AI/recovery component data matches accepted 0.58. Hillside, shell/floor/material assets, Forest Reverse, Backyard scenes, global physics/AI/recovery, ambience and launcher script are unchanged. Only intended wall-ledge objects were removed.

These are automated local physics, production-driver and saved-camera checks, not Dan's handling or visual acceptance. No full laps, Reverse cave runs, unrelated tracks, audio tests or broad matrices. Stop gameplay testing; await Dan after release.

## Authoring/fixture notes

Two initial authoring calls failed before save (ephemeral JsonUtility type parsing and Unity fake-null component handling); reloaded the untouched saved baseline and completed the corrected authoring. AI fixture startup initially preceded completion of script import; no gameplay run occurred until compilation finished. Preservation comparison normalized signed-zero floats and UTF-8 source reads; no source/geometry workaround. Final checks above describe the saved deliverable.

Source commit/push, fresh runtime, signed publication/catalog, Latest, production launcher and disk cleanup evidence are recorded in PUBLICATION.md after execution.
