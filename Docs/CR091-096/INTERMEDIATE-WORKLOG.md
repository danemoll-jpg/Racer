# CR-091–096 validation — candidate work log

Safety checkpoint: a89577e9b0d3dfb4bf61652dd6ea71bae39bcdd1.
Baseline review3 evidence is retained under Docs/CR081-090 and Docs/CR082-089. This pass does not retroactively accept it.

All tests below use compiled Windows players, isolated Temp/cr091-* saves and temporary audio mute. Screenshots use explicitly rendered cameras/canvases. Virtual input is not a physical controller or human driving/listening test.

## Intermediate evidence (not final acceptance)

- First summit geometry: 60 physical cases, 11 incomplete, 19 with upright Y below .65, no awards, all local resets successful. First playable was driven before refinement. Initial broad coverage was on Street; later matrices cover all variants/modes.
- Initial finish fixture: eight reverse-wrap accounting failures, subsequently fixed. The inspection2 fixture's four Southwest Cut failures were traced to its incorrect seeded expected checkpoint, corrected in inspection3. Its 60 Street checks then passed, including sampled legal branch travel followed by an airborne missed finish. Fixtures seed earlier gate progress; full physical races are separate evidence.
- Inspection: all 24 acorns approached physically, all local resets successful; cold relaunch retained all IDs and identical fog across four tracks. Not every possible approach angle is asserted safe.
- Inspection2 centered summit: all four 32m/s runs completed, recovered and scored, minimum upright .935–.946, approximately 3.1–3.2 seconds airborne. Wider coverage found a south-side runout dip and hard landings at 40m/s; further geometry correction follows. The hard-landing scoring threshold remains unchanged.
- Inspection3: the summit return was driven by all four Street profiles; Kyle downhill passed, but uphill failed for all four. A longer authored descending driveway replaces that short steep transition.
- Rendered sign inspection found stale overlapping text. Two causes: restoration re-enabled intentionally disabled renderers, and a historical build hook recreated obsolete metric labels. Both corrected; clean-cache build validation required.
- Inspection2 regression found deleted fence-smash target references after fence replacement. The activity is rebound to nearby new breakable sections; final rule/physical checks required.
- Inspection2 regression reproduced the tourer obstruction timing failure and two AI finish-grace DNFs during the motorcycle player race. Player races/ghosts are reported separately; successful players do not imply every AI finished. A shorter repeated-stall retry is under validation.
- The inspection2 folders named shortcuts-* accidentally run the physical wrong-way/recovery harness. They are guidance evidence, not shortcut-driving proof. Final shortcut checks use the actual reverseReview routes harness.

## Pending

Final candidate repeats, all-variant new-system checks and actual shortcut/race regression, serial neighborhood/mountain performance, complete Windows packaging/hash verification, updated playtest checklist and completion commit. No blanket passing claim is made for pending or failed cases.

The first release candidate (0.16.0-review1, evidence folders `final` and `final-regression`) is NOT the delivered build. Actual Forest racing exposed an unarmed frontier after the offset START projection; witnessed required checkpoints now anchor progress. The next full Forest smoke completed all four racers' laps. The revised driveway passed both directions for all four Street profiles. The tourer obstruction case passed in 33.23 seconds with two recoveries.

That candidate still exposed a 40m/s pre-lip hop at the launch tangent discontinuity, a low-upright motorcycle return descent, and one failed acorn-24 approach into a tree. The final candidate smooths the tangent, joins the existing ridge at matching elevation, and widens only that local acorn access clearing. Cold-reload checks expecting 24 consequently failed after the 23/24 collection run; those are not evidence of lost saved items. A complete rerun is required.

The second candidate (0.16.0-review2, folders verified / verified-regression) passes all four summit returns and both Kyle driveway directions for each Street profile. The continuous fence-smash run reaches gold with all four profiles. Its partial summit matrix is stable but several 40m/s landings narrowly exceed the unchanged 18m/s hard-landing threshold (original center: 18.195m/s); the supported landing and return crossing are raised for review3. Interrupted matrix evidence remains partial, not a completed all-variant pass.
