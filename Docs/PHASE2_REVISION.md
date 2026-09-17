# Phase 2 feedback revision — 2026-09-17

**Phase 2 NOT ACCEPTED — awaiting Dan's retest. Phase 3 was not started.**

Safety checkpoint: `b8d7f62c37af9da718769d28295b417531ca118d`. This committed Dan's saved PROJECT_TODO.md before any revisions. An initial sandbox denial was resolved by the user authorizing elevated Git execution.

## Scope and reproduction

Open `Assets/Scenes/StreetLoopGreybox.unity`. Rebuild its environment with **Racer > Rebuild Phase 2 Feedback Environment**, from a saved scene outside Play mode. Generation can take several minutes. It replaces only the three named generated environment roots and adjusts the existing spawn/car placement to the revised ground height. It does not change the car prefab, runtime driving/input/suspension/camera/reset scripts, or PrototypeTrack. Rebuild discards hand edits under those generated roots; edit the generator instead. The original new-scene builder now runs this revision pass as its final step. RefreshTerrain also routes through this pass.

The annotated, plain, and satellite references were inspected. The annotated older-landscape interpretation remains authoritative. The X/Z Catmull-Rom knots and sampling counts are unchanged; **no horizontal route adjustments** were made. The entire real loop and six key house landmarks remain, with no modern subdivision streets or race systems.

## BUG-001: supporting surface

Before the fix, lateral collision sampling found a 1.061 m height change across 0.25 m at road sample 700, offset -9 m. A real-car slow perpendicular re-entry there spent 264 physics steps below the surface above it, by up to 0.496 m. This reproduced the mechanism Dan described, without altering the car. See PHASE2_REVISION_BASELINE.txt.

The revised road, shoulder, and surrounding ground form **one continuous visible and collidable heightfield**, split into 100 tiles sharing identical boundary vertices. There are no overlapping road/shoulder colliders, elevated road ribbons, slab undersides, or invisible support meshes. MeshCollider uses the same mesh as MeshRenderer. Road/shoulder/grass colors distinguish regions of that same surface. A minimal vertex-color shader keeps it a greybox. The 2 m grid makes the road edge slightly soft/stepped visually; the driving surface itself stays continuous.

The centre of the road stays level across its width, with the shoulder falling approximately 0.45 m smoothly between 6 and 16 m from the centreline. Beyond 16 m, broad terrain interpolation and site slopes take over. At House 3 only, the outer shoulder starts its downhill blend at 8 m to reveal the lower site; it remains continuous. House 3's distant hillside is deliberately steep and is not an ordinary re-entry shoulder. No vehicle changes, teleports during traversals, invisible walls, or thicker pavement were used.

## Requested changes and estimates

All values below are qualitative tuning estimates, **not surveyed geography**. Heights use arbitrary scene metres; the original approximate map scale remains 1.1 m per reference pixel. Practical values are in StreetLoopBuilder.Route (height profile) and StreetLoopRevision (shoulder width, site depths, positions, forest clearance, seeded placement).

| Item | Implementation |
|---|---|
| CR-001 | Initial climb rises from about 8 m to 82 m, previously about 50 m. |
| CR-002 | Second major drop falls about 82 to 27 m, concentrating 44 m of descent between the 1070 and 1150 reference-Y landmarks. Maximum short grade is about 54%; this intentionally strong interpretation needs Dan's feel review. |
| CR-003 | Final yellow-circle section holds near 29–30 m before dropping to about 8 m over one approximately 134 m interval, instead of spreading a subtle descent over the lower return. It remains gentler than the second drop. |
| CR-004 | Neighborhood crests/dips alternate around 76/86/77/88/82 m, with more waves through the lower neighborhood. Main and connecting roads retain modest 6–9 m variation. |
| CR-005 | House 3 moves from reference (1080,1141) to (1040,1150), farther west of the road. Site target is 28 m below the nearest road, with a steep blended hillside, supported foundation, and a clear view corridor. |
| CR-006 | Friend moves from (1220,982) to (1115,982), staying east/across the road. Site target is 3 m downhill, with a blended pad. |
| CR-007 | 20 approximate older residences, up from nine, along the western connecting road and far neighborhood/lower return. Six key landmarks preserved. |
| CR-008 | 22 varied commercial masses, 11 on each side of the northern main road, with roughly 30–38 m centre setbacks and gaps between businesses. |
| CR-009 | 6,118 simple low-poly trees, batched by spatial region, replacing the former roughly one thousand attempts. Trunks have matching box colliders; canopies are visual only. Centres stay at least 23 m from the road and 22 m from building centres. House 3 has an additional view corridor. |

## Validation

See the associated PHASE2_REVISION_*.txt reports for actual outcomes. Original PHASE2_VALIDATION.md and PHASE2_TEST_RESULTS.txt are preserved as historical evidence; their road-centre checks did not establish safe off-road re-entry or Dan's subjective approval.

Automated drives supply throttle/brake/steering to unchanged Phase 1 forces with real PhysX integration at 0.02 s. Initial placement is used only to initialise independent cases; no body-position/velocity correction is used during traversal. These tests are technical evidence, not human playtesting or approval of recognition/feel. Physical-controller hardware testing remains unperformed.

An early crossing run without steering correction completed 119/120 traversals; the remaining traversal did not finish, though it never went beneath the ground. A second straight-line run completed 118/120 but ventured onto natural slopes on bends, yielding two additional upright-threshold failures. The final driver follows the horizontal road curve while sweeping from +15 m to -15 m (and vice versa), using ordinary steering input. The earlier straight-line exploratory results remain in separate reports; their tree/slope excursions are not certified as safe shoulder paths. All earlier observations remain distinguished from final verification.

Long authoring/testing calls can exceed Unity Pipeline's fixed five-second transport deadline while Unity continues executing. Saved assets and output reports are checked independently; transport timeout messages are not counted as successful tests. Final fresh-console results are recorded separately.

Rebuild iteration also exposed stale GPU mesh data after EditorUtility.CopySerialized: colliders reflected new heights while rendering retained old vertices. The generator now explicitly assigns mesh buffers and uploads them, so visual and physical surfaces refresh together. Final views/checks use the corrected buffers.


## Final executed results

- Final scene saved, reopened, and played successfully after regeneration. Compile succeeded. Fresh Console results are in PHASE2_REVISION_CONSOLE.txt.
- Full loop, unchanged Phase 1 forces/PhysX: forward 564.1 simulated seconds, reverse 564.6 seconds; maximum centre errors 2.82/2.78 m, minimum upright 0.880 in both directions, **zero airborne steps**, zero test failures. Test speeds are 4.5 m/s in tight bends and 8 m/s otherwise. Road length is 4,674.1 m, versus historical 4,647.3 m because the vertical profile is longer; X/Z geometry is unchanged.
- **180/180 curved shoulder traversals passed:** 15 representative route locations × two sides × 3/8/15 m/s × 25/55-degree nominal approaches. Paths follow the road bend while crossing from +15 to -15 m (and vice versa). Actual approach varies with curvature. Zero body-below-surface steps and zero airborne steps in all final cases. These cover leaving/rejoining, hills/crests/descents, bends, main/connecting roads and terrain tile joins. They do not certify every off-road trajectory or top-speed recovery.
- Lateral ray sampling every fifth route sample, across +/-16 m at 0.25 m intervals: zero missing or stacked surfaces. Maximum adjacent rise 0.176 m at a sloped bend; this is a continuous grade, not the original 1.061 m slab lip. All 100 terrain colliders reference their visible mesh. Sampled road/shoulder prop overlap audit found zero blocking non-terrain colliders.
- One intermediate curved test failed the upright threshold (0.793) near House 3. Constraining the site blend against local road height corrected that overly steep transition. The final matrix passes; earlier iteration reports remain available.
- House 3: centre setback **113.54 m**, ground **32.75 m**, nearest road **60.75 m** (28 m below). Friend: centre setback **37.85 m**, ground **84.87 m**, nearest road **87.87 m** (3 m below). All estimates, not survey facts. House 3 and friend inspected from road-height views; road hills, additional residences, commerce, and forest inspected through chase-camera and overhead captures (Revision_*.png).
- Virtual keyboard W/D, S/A, arrows and R reset passed. Reset clears momentum; chase camera snaps and follows. This is automated input verification, **not physical keyboard/controller hardware testing**. Physical Xbox/controller testing was not performed.
- Live Play-mode performance: 180-frame editor samples at 734x293, vSync off. Spawn: forest on/off median **3.31/3.22 ms**, p95 **4.31/4.07 ms**. Dense neighborhood parked-car comparison: on/off median **5.01/7.07 ms**, p95 **14.31/13.98 ms**. Variability exceeds the measured forest difference; no material forest-related regression was demonstrated in these short samples. This does not establish standalone/high-resolution or full-loop GPU performance, nor compare every aspect of the old scene against the new terrain. Temporary background-play and parked-body settings were restored, and Play mode exited without saving them.
- Git comparison confirms PrototypeTrack, PrototypeCar prefab and all four Phase 1 runtime scripts remain unchanged.

## Dan's focused retest checklist — all still unaccepted

1. **BUG-001:** Leave/rejoin both sides slowly and at normal speed, shallow and oblique, at flat roads, bends, hill approaches, crests and descents. Check support and visible ground agree; no under-road travel, collision lips or launches. Distinguish the steep House 3 hillside from ordinary shoulders.
2. **CR-001:** Does the entrance now feel substantially larger uphill, including in reverse?
3. **CR-002:** Is the second yellow-circle descent deep and fast enough, yet comfortable/drivable? Review the strong 54% short grade.
4. **CR-003:** Is the final hill shorter and clearly downhill, but gentler than the second drop?
5. **CR-004:** Are neighborhood rolls frequent and perceptible, with quieter main/connecting sections?
6. **CR-005:** Is House 3 far enough back and down a steep enough hill? Judge the clear hillside view and 114 m approximate setback.
7. **CR-006:** Is the friend's house close enough, slightly below the road, and still correctly across the street?
8. **CR-007:** Are the extra connecting-road/far-end houses sufficient while retaining the sparse older pattern?
9. **CR-008:** Do businesses on both main-road sides have suitable coverage, gaps and setbacks?
10. **CR-009:** Do the childhood-house area and removed-development areas feel heavily forested, with clear shoulders/sightlines and acceptable performance?

Also drive the complete loop both ways, check familiar handling/camera/R reset, and test the physical controller separately. Record location/direction/speed of remaining issues. **Only Dan can accept Phase 2; Phase 3 remains unauthorized.**

Final fresh Console at 2026-09-17 08:02:05 UTC: compilationFailed=false, compiling=false, **0 errors and 0 warnings**. Editor left stopped with StreetLoopGreybox open. Iteration compiler/naming issues and Pipeline timeout messages were resolved or distinguished from game errors before this fresh check.

