# Forest Forward cave hillside and sight lines — 0.59.0-review1

Safety checkpoint: clean main `acb259163904b2ca84e05f75e133b1370dd9537e`. Dan's requested pass supersedes the previous review stop. Only Forest Forward / LakeWoods is authored.

## Exterior

A broad asymmetric earth surface bridges the excavated cave trench and joins the existing wooded slopes. The 419m interior shell, floor, jump and clearance remain in place beneath it. Width varies independently of the cave between roughly 60–80m; the hill uses the adjoining terrain heights and broad swells rather than tracing the roof. Sloping earth/rock portal banks retain recognizable openings. Saved approach, exit and three exterior views were inspected; the freestanding tube is concealed.

The first pass exposed incorrect face winding and an overly broad tree predicate. Original tree transforms/mesh references were recovered from the safety checkpoint in Unity, then only actual hill dependents were reseated. The first support audit also detected overlap with the main trail. Cover now feathers into original terrain and omits triangles within its protected corridor; exposed border triangles are buried into the original shoulder. Failed first evidence is retained in first-local-checks.txt.

Final checks: 3,180 main-route/edge probes, zero cover obstruction; 679 shell probes, zero exposed roof; 95 trees over final cover, zero buried feet. Across 85 changed existing trees, maximum final foot gap is 0.0543m, including six final reseats after edge trimming. Complete trunk/crown mesh components and colliders move together. The collider dependency inspection found only the intended underground rockfall beneath cover; no affected external signs/buildings/props. No shrubs required relocation. Removed 58 unused intermediate tree meshes.

## Rockfall and driving

Keep all four colliding rocks and the original approximately 3.8m right passage. Wall-side height becomes about 2.54m; the three central/opening/rear blocks become about 1.62m, 1.38m and 1.17m. Their footprints, supported bases and matching visible mesh colliders remain. No obsolete collider remains. Thirty-six decorative wall slabs along the obstacle approach become low supported side stones, removing their eye-line obstruction. The rest of the accepted cave interior remains unchanged.

Three approach sight-line checks pass. Saved actual ChaseCamera views at stations 185 and 197 for both vehicle configurations were inspected; the intended passage and continuation are visible. Initial capture setup omitted edit-mode vehicle initialization; corrected before final captures. These are rendered camera fixtures, not human playtest acceptance.

- Motorcycle clean line: complete at 27m/s target; minimum 26.76m/s, minimum upright dot 0.998, zero airtime; maximum tracking error 0.417m.
- ATV clean line: complete; minimum 26.76m/s, upright 0.998, zero airtime; maximum tracking error 0.416m.
- Straight poor line: both physically stop at the rocks, upright 0.998, zero airtime; no random flip or invisible collision on the successful line.
- Production RoadDriver ATV: existing probabilistic Forward branch selection passes; local approach/opening/exit completes to station 265.5, maximum lateral error 0.53m, zero resets/recoveries/airtime.

Driving uses actual vehicle motor and physics with automated inputs. AI runs ordinary play frames. Tests are muted and local; no full laps, Reverse routing, global tuning or broad matrix. Subjective difficulty and final appearance await Dan.

## Preservation and delivery

141 existing scene records change: local rock/tree transforms, affected combined tree mesh references and scene root list. No existing objects removed. All existing route, checkpoint, guidance and AI component data is unchanged. Forest Reverse, Backyard Reverse, global driver/motor/recovery, rat audio and Play-Racer.cmd are byte-preserved. Original terrain assets are unchanged; the added cover has its own matching mesh collision.

Targeted gameplay checks are complete. Source commit/push, fresh build, signed release/catalog, complete Latest, real production launcher and cleanup are recorded in PUBLICATION.md after execution. No further gameplay tuning is authorized before Dan's review.
