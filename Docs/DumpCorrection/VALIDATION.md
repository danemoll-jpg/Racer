# Local garbage dump correction — 0.39.0-review1

Dan accepted the overall Forward route, forest/trail, both major gully crossings, start direction and checkpoint presentation. The dump was the sole remaining issue. Reverse and optional shortcuts remain deferred. Safety checkpoint: clean main `c27b59c777bb1fa7e01c225b0e1a13cab0039201`.

## Actual implementation

Only the dump and directly affected scenery changed. Its excavation is an ellipse within the existing footprint: 49m along the jump and 46m across, centered 25.5m along the accepted bearing from (309.2,15.7). The level central floor is Y=61.5, with smooth sloped sides blending into existing ground. Measured launch Y=80.339 and landing Y=67.690 give an 18.839m launch-to-floor drop and 6.190m floor-to-landing rise. The hillside and existing raised launch account for the larger near-side drop. Launch/landing X/Z and supporting terrain remain unchanged.

Sixty irregular lightweight pieces depict rusted drums/lids, weathered boards, containers, metal sheets/beams and old rubber wheels. They have no colliders; terrain provides all physical support. The central corridor and far escape slope remain open. Dark floor soil fades into the retained rim. Two interior trees were removed; nine affected perimeter trees and their batched visual geometry were regrounded.

Original terrain triangles are retained: no hole, substitute floor, invisible wall, reset volume, artificial penalty or global physics/recovery/AI change. All other terrain vertices, route/gate/navigation data and established scenes are preserved. The atlas footprint does not materially change, so the historical 0.38 map is not regenerated; this document supersedes its old dump depth/junk description.

## Targeted results

[Driving evidence](editor-checks/done.txt) and [trace](editor-checks/pace.csv) use one motorcycle, ordinary motor physics/pedals/steering, 0.02-second steps, muted audio and isolated saves. Placement/initial velocity establishes each bounded approach. No teleport, boost or reset occurs during a drive. No full lap or vehicle/course matrix was run.

- Normal 32m/s approach clears the bowl without interior contact, with 2.16s flight, and lands on the existing far route. Existing CP2 progression passes without a missed gate.
- A short 8m/s approach falls into the bowl, regains wheel support and climbs out at full throttle without reset. Between along -2m and +55m, this costs 6.72s versus 2.28s clearing: **4.44s lost through driving alone**.
- A separate four-waypoint floor maneuver and escape passes, with 461 supported physics frames and lateral travel inside the bowl.
- Normal player recovery remains active: zero reset events across all three drives.
- One actual production AI clears with 2.16s flight, zero recoveries and no short contact. AI behavior is unchanged.
- Junk has no snag/trap/launch colliders; every piece touches the final terrain.
- [Eight geometry/dependency checks](geometry-checks.txt) pass: identical visible/collision mesh; unchanged topology; every vertex outside the ellipse unchanged; substantial measured floor/rims; every route point unchanged; affected tree grounding offsets preserved; no junk colliders/triggers; all junk grounded. Maximum grounding error rounds to zero.

Views inspected: [launch edge](approach.png), [bowl](bowl.png), [inside toward landing](floor.png). [Before](before-heights.csv) and [after](after-heights.csv) profiles record actual collider heights. Dan judges appearance/gameplay acceptance.

Unity required a full script recompile, so an early fixture launch could not resolve its type; no result was claimed from that attempt. The bounded run after compilation passed all eight driving assertions. Existing ambient-traffic initialization errors occurred in the Editor before the fixture disabled that unrelated system; it was not modified. Gameplay testing stopped after the requested checks passed. Release checks are limited to build, signed inventory, public startup and production launcher identity.

## Delivery

Completion source, build, publication, launcher and cleanup will be recorded in PUBLICATION.md. Until then delivery remains pending.
