# Two Backyard Forward shortcut revisions — 0.43.0-review1

Safety checkpoint: clean main `88bb363b8078f890dec28734e7036ca48a70c9f2`. Dan's gameplay feedback supersedes the previous visual/map approval. Only these two shortcuts are revised. No Reverse, world/menu map, main-route redesign, global vehicle/AI tuning, invisible wall, forced reset or time penalty.

## Final local design

- Tree-Top: packed-earth launch at branch stations 15–29; a 4 m gap; first landing platform 33–48, 5.8 m wide before tapering; later 3.3 m bridges, 4.8 m resting platforms, rough mismatched boards, short rails and braces to four existing trunks. Repeated ground posts and the old broad elevated deck are gone. Exit leaves the deck at station 98 for an intentional drop toward the unchanged rejoin. The final launch tangent is .09 rather than the old .28; global physics is unchanged.
- Cabin: six adjacent long boards, stations 22–32, reach a normal small weathered shed roof at 32–41. Existing location and route entry/rejoin retained. The previous 29 m timber approach and support rows are removed. Roof walls/windows are grounded individually on the unchanged terrain.
- Roof investigation: the previous “shared” surface concatenated independent approach/roof meshes, retaining their unwelded boundary; its rising profile also changed slope through the roof. The replacement has one welded collision strip, matching board/roof heights, a constant .32 roof grade and recessed wall tops. There are no decorative-board colliders or overlapping roof colliders. The final full-car clearance inspection finds zero obstructions. This establishes the new local fix and measured behavior; the exact cause of Dan's earlier subjective slowdown was not independently reproduced.
- Both areas have broad overlapping low-poly shrubs and fern fronds. Two shortcut-only components apply smooth horizontal dissipative resistance **only while grounded on actual terrain**. Bushes have no rigid colliders, upward force, torque, propulsion or reset. Airborne vehicles and vehicles on boards/platforms are unaffected. Resistance is zero at rest, so vehicles can pull away in either direction. Existing trees remain steering obstacles.
- Ground resistance and visible cover extend underneath and to both sides. Coverage samples at lateral 0, ±5 and ±10 m pass 105/105. All 1,684 main-route centreline points have zero resistance; its usable corridor has a 4 m exclusion. There is no authored clean lane beneath or immediately beside the shortcut. A driver who chooses a much wider detour can return to the main route; no claim of an exhaustive world-wide optimal-path proof is made.

## Bounded physical results

Motorcycle, normal motor/physics inputs; setup repositioning only between independent fixtures. Ordinary gameplay never teleports. Other vehicle widths checked dimensionally, not a driving matrix.

| Check | Measured result |
|---|---|
| Final tree, medium entry | 4.96 s; first touchdown station 37.42, within first third of 33–48 platform; 122 elevated-support frames |
| Final tree, slower capped entry | 19 m/s cap at entry; 5.62 s; first touchdown 34.54; 151 support frames |
| Cabin clean | 4.26 s versus 5.06 s main; roof speed 30.59 at station 31.91 → 31.83 at 39.00 → 31.75 at lip; 1.96 s continuous air, landing around 100, beyond bushes ending around 87 |
| Tree ground −5 m line | 15.10 s, entire local branch, reached exit without reset |
| Tree ground +5 m vicinity | 19.06 s for stations 36–104 alone, steered around retained trees, no reset |
| Cabin ground −5 m vicinity | 16.04 s for stations 44–122, no reset |
| Cabin ground +5 m vicinity | Cleared bushes past station 90 at 15.06 s with four supported wheels; subsequent test pilot did not finish its fixed-side egress, see limitation below |
| Tree fall | 7.4 m fall onto traversable forest ground, four supported wheels, zero automatic resets |
| Production AI, one attempt each | Tree 106.7/110.4 m and cabin 124.2/127.9 m, zero recoveries; checkpoint/rejoin vicinity reached |
| Progression | Both entrances recognized; expected CP2/3 and CP6 entitlement, zero misses; next required gates 4 and 7 |
| Vehicle fit | Body widths .65/1.45/1.85/2.00 m fit 3.3 m technical path; eligibility unchanged |

The tree's new clean times also beat the previous measured **6.80 s** main-route segment, whose geometry and motor remain identical. Current test-pilot main attempts took 11.78/13.56 s because they departed the trail by up to 6.5 m and entered brush; **those are not claimed as clean main-route benchmarks or a 6–9 second shortcut saving**. The appropriate established comparison is approximately 1.84 s saved at medium entry against that historical 6.80 s clean run. Ground traversals are slower than either benchmark. Cabin comparison is from the same current fixture.

**Retained failures/limits:** Initial tree fixture used the obsolete >.75 s airtime assertion; the deliberately shorter jump produced .54 s, then .44 s after the final local tangent adjustment. Initial “slow” setup accelerated to 25 m/s before takeoff; the final capped 19 m/s test supersedes it. Fixed-offset ground pilots ran directly into retained trees and timed out. The follow-up uses a test-only clearance path and normal steering/reverse inputs. Both tree sides and cabin's −5 m side complete; cabin's +5 m path passes the whole bush obstacle in 15.06 s, then oscillates near an existing tree around stations 99–107, timing out at 50.02 s without reset. Full automated same-side egress on that line is **not verified**; the opposite ground route proves a complete traversable recovery. No tree was removed to make a fixture pass, no player recovery system changed, and no further pilot tuning/matrix was run.

Final geometry: **13/13** checks pass. Preservation: **49,431** old scene blocks outside the shortcut root unchanged, all main points/nine gates, six other scenes and 12 global systems unchanged. No terrain heightfield, property, dump, gully, main checkpoint or approved road changed. Temporary Editor reload settings and test-induced VehicleGlazing material change restored. Existing racing minimap consumes the preserved branch data without styling changes.

Raw evidence: `checks/` is the initial bounded pass; `followup/` contains final entry-speed and ground-path traces, including failed assertions. `geometry-checks.txt`, `geometry.json` and `preservation.json` document final data. The refreshed Backyard overhead map shows the actual final scene. No subjective gameplay acceptance is claimed. **Gameplay testing stopped; Dan reviews feel, appearance and difficult failure recovery.**
