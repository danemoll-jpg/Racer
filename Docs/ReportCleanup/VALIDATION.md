# Debug report cleanup - 0.64.0-review1

Authoritative input: Dan's `2026-10-01_19-34-39-371_4a7f21/errorreport.zip`, SHA-256 `61da4b63efa05bf926ad93c0eed6761fe23a7168f904b6d42c4c28cdc6a39704`. All 25 screenshots and the Markdown report were reviewed. This supplied ZIP contains Markdown and screenshots; it did not contain JSON. The separate in-game export check verifies the current product's JSON output too.

Safety checkpoint: clean `f034febc94ba52f61b3bba2acce0a620a0e2b993` on main.

## Individual dispositions

All 25 supplied screenshots and final authored-scene views were inspected at the recorded coordinates. PASS below means the reported defect has a specific correction and targeted technical evidence; visual taste and full human gameplay acceptance remain Dan's review. Before/after images are named BUG-NNN-baseline.png and BUG-NNN-after.png in this directory.

| Report | Disposition / correction |
|---|---|
| 001 | PASS - Previous pavement remesh ended at X=940 with a mismatched grade. Restore the existing Summit cross-section through that boundary; continuous surrounding terrain. |
| 002 | PASS - Climb road/terrain slit. Replace local malformed support with continuous terrain. |
| 003 | PASS - Torn Summit entry support, unsafe exposed sides and shimmering gold marking. Terrain, natural edge protection and marking seating. |
| 004 | PASS - Ragged Summit shoulders and obstructive/repeated geometry. Local terrain and dependency cleanup. |
| 005 | PASS - Obsolete LEFT FORK / SUMMIT TRAVERSE instruction. Remove sign. |
| 006 | PASS - Unintended side cut between nearby roads. Grounded natural barrier, preserving both roads. |
| 007 | PASS - Exposed triangular support curtains around stacked road layers. Replace malformed faces while preserving the lower driving corridor. |
| 008 | PASS - Floating descent shoulder and an obsolete blue Fern creek bed plane at (880.24,111.49,-84.92). Rebuilt connected support, grounded complete trees, and removed the suspended noncolliding creek plane. Route arrows retained. |
| 009 | PASS - Narrow artificial pit at hairpin. Recoverable natural slope between the existing road arms. |
| 010 | PASS - Southbound receiving road appears unsupported. Support its existing receiving section; preserve the preceding flight corridor. |
| 011 | PASS - Buried duplicate BIG FLIGHT AHEAD sign. Remove sign. |
| 012 | PASS - Open terrain seams/voids beside Summit descent. Replace malformed local support with a continuous recoverable landform. |
| 013 | PASS - Buried duplicate BIG FLIGHT AHEAD sign. Remove sign. |
| 014 | PASS - Eastbound receiving ramp appears unsupported. Support receiving pavement without filling the flight gap. |
| 015 | PASS - Bend seam and misleading arrow orientation. Corrected crossing pavement triangles on inconsistent grades; final contact adjustment <=0.167m, on top of the initial <=0.252m grade correction. Relocated/oriented the marking on the following straight; seated support below final pavement. Both vehicles remain upright and grounded in the final bend check. |
| 016 | PASS - Buried redundant MAIN ROUTE / FOLLOW TEAL ARROWS sign. Remove sign. |
| 017 | PASS - Overlarge MAIN ROUTE tutorial at shortcut junction. Remove it; preserve the named optional shortcut label. |
| 018 | PASS - Repeated SUMMIT JUMP / FOLLOW THE RUN-UP sign. Remove sign. |
| 019 | PASS - Exposed narrow green ribbon outside turn. Replace malformed support with connected terrain. |
| 020 | PASS - Redundant BIG JUMP / HOMEWARD JUMP instructions. Remove signs. |
| 021 | PASS - Repeated BIG FLIGHT / CLEAR THE SUMMIT instructions. Remove signs. |
| 022 | PASS - Westbound receiving ramp, tree intrusion and exposed supports above gold shortcut. Support receiving pavement and clear dependent trees while retaining the lower shortcut corridor. |
| 023 | PASS - Repeated MAIN ROUTE / FOLLOW TEAL ARROWS instruction. Remove sign. |
| 024 | PASS - Repeated MAIN ROUTE / TWO BIG FLIGHTS tutorial. Remove it; preserve shortcut identity. |
| 025 | PASS - Misleading open apron at Summit shortcut entry. Natural barrier outside the usable junction. |

## Technical verification

- Final support probes: Forward 1,983 and Reverse 2,091; zero pavement intrusions and zero missing receiving support probes. Both directions retain identical road/branch point data and matching visible/collision meshes. The receiving heads are derived from the original authored road sections, never the airborne navigation interpolation.

- Additional Reverse clearance: 3,570 pavement probes, zero intrusions/missing floors; zero lower-tunnel clearance failures; Summit rejoin tangent 0.344 degrees; obsolete slab count zero. Retained vault edges were trimmed/seated only where they protruded through the upper pavement.

- Production-force motorcycle drives cover the affected main-road spans in both directions and Summit Traverse. ATV drives cover receiving pavement, the lower tunnel and Summit clearance. All completed without resets or AI recovery. CSVs and raw results are under driving/.

- The first Forward motorcycle bend test caught a real contact defect. After the local crossing-triangle correction, the bounded 710-790 repeat passes: motorcycle maximum air 0.02s, ATV 0s, both minimum up 1.00, no reset/recovery. See bend-drive/MountainLoop/driving-done.txt.

- Test limitations retained explicitly: these are automated local technical drives, not human playthroughs or full-flight trajectory certification. Forward's probabilistic shortcut-selection fixture did not select Summit automatically; the test explicitly directed the existing driver onto it and both traversals completed. Reverse's wider 1480-1690 main-road sample included an off-line roll (minimum up -0.89) that recovered without reset. This is not claimed as a clean full-course AI acceptance and no global AI/physics/recovery changes were made. The reported Summit join and receiving/tunnel checks are separately covered.

- Rebuilt malformed terrain faces as connected earth/rock volumes with lower-route and flight clearances. Initial sheet/volume attempts that failed visual checks were discarded; unused intermediate meshes were pruned. Signs, props, outcrops and complete tree assemblies were checked/reseated against final supporting surfaces. No buildings/fences occupy the local Forward bend repair.

## Debug menu and Results

The shared InputSystemUIInputModule was disabled whenever Debug owned gameplay input. The existing menu now retains the shared UI module while interactive, blocks gameplay, preserves focus, skips disabled actions, and displays established input glyphs. No replacement menu was introduced.

All 28 real Input System event checks pass (ui-done.txt): keyboard arrows/Enter/Escape; controller D-pad/stick/A/B; mouse hover/click; all eight actions; disabled Fly/Return/Export applicability. The short isolated ordered-gate race reached production Results, switched to Lap Times, showed the red B Main Menu glyph and returned directly to Ready/Main Menu with B. The test used ordered gate events to focus on Results behavior, not a second full-course drive. Records/scoring/classification logic is unchanged.

The in-game Export action created a ZIP, opened successfully with BUG_REPORT.md, bugs.json and its referenced Screenshots/BUG-001.png inside the session directory. Verified copy: [verified-menu-export.zip](verified-menu-export.zip), SHA-256 DA7BF13DD6315CA72EA3F84E767B7FD88F2D2DEB3CF3BE9FE47A5938B551C1EC. Original test-export.txt records the generated path; its disposable test location is removed after release. The first test assertion wrongly assumed root-level files; the product archive was valid and the assertion was corrected.

## Conservative worldwide sign audit

Instructional/navigation signs remain only when they communicate useful information not already obvious from environment, arrows, gates, minimap or established conventions. Landmark/world-character signs are preserved. This is the requested design principle, not a change to CODEX_RULES.md.

Initial removals: Mountain Forward 13, Reverse 8; street Forward 4/Reverse 4, Forest Forward (LakeWoods) 6/Reverse 4, Backyard Forward 4/Reverse 4. Other courses share the Mountain world; four repeated jump tutorial boards were removed from those shared areas. The two extra Forest removals taught the existing amber-arrow convention. Final Mountain detail pass also removed remaining MAIN ROUTE/BIG JUMP tutorial signs. Full object/text/location inventory: sign-dispositions.txt.

Non-Mountain scene diffs contain sign subtree removal and Unity serialization of existing route-field defaults; no changed terrain mesh, collider, road/structure transform, accepted Forest cave or Backyard geometry. See non-mountain-scene-diff.txt. Useful shortcut names, Summit Camp, landmarks and nonobvious hazard guidance remain.

Final bend screenshot also caught an Editor render-buffer cache mismatch after serialized mesh replacement. A fresh mesh asset uploaded the same passing final vertices; the corrected final view matches the collision surface.

## Delivery

Fresh build, source push, signed publication, real Play-Racer.cmd verification and post-release cleanup are recorded in PUBLICATION.md after delivery. No vehicle physics, global AI/recovery, playlists, music, record scoring or startup flow changes. The safety checkpoint and original user report are preserved.

