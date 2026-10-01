# Backyard Reverse optional shortcuts — technical review

Version 0.54.0-review1. Safety checkpoint: `771bb757ae1fc8383cd9b73a1b964c71ca3b4ca5`. See [original request and anchors](REQUEST.md) and [actual-geometry atlas](ATLAS.html).

Two player-only branches share one permanent prefab between Backyard scenes. Forward races close the visible logging gate and culvert grate; Reverse and free roam open them. Forward retains its original two legal shortcuts. The accepted main roads, gates, jumps, global vehicle physics, AI, recovery, HUD, racing minimap, Records and music are preserved. Reverse layout identity advances to `backyard-reverse-v2-forest-shortcuts` so records from different legal layouts remain separate without deleting history.

Logging Ridge has a narrow 4m earth line, modest rises, ruts, timber and retained forest. Its 131.93m line replaces 150.94m of main road. Local brush slows an off-line rider; the route bypasses no checkpoints. The authoritative rejoin anchor is traversed before merging onto the accepted road.

Storm Drain / Gully Jump has a 9m-wide, 6.5m-high culvert beneath the surface routes, opening into the gully at the supplied anchor. A straight launch approach leads to the supported west-bank landing and existing route. The 249.43m branch replaces 291.74m of main route and grants only CP3 after legitimate branch entry/completion. CP4 remains required. Entry height and station guards reject the surface road above. Gold denotes optional routes; dashed gold denotes the underground section in map metadata, full map, previews and atlas.

## Targeted evidence

- 23 state/geometry assertions pass (`state-checks.txt`): Forward/Reverse/free-roam gates, visible collision, branch membership, AI exclusion, checkpoint scope, entry and wrong-way rejection, main-road brush protection, and grounded ridge trees.
- Five recorded-entry assertions pass (`recorded-entry-checks.txt`): actual motorcycle and ATV paths satisfy final entry guards for both branches; only CP3 is granted.
- Motorcycle and ATV pass Logging Ridge in 7.32s / 6.06s. ATV passes the full drain in 15.64s (`third-candidate-driving.*`). Motorcycle traversed the culvert but initially overshot the landing. After reducing the local launch grade, both pass the changed launch/landing/runout section (12.86s / 7.06s; airtime 1.64s / 1.70s; `jump-driving.*`). These are separate targeted runs, not matched main-versus-shortcut lap comparisons.
- Recovery uses ordinary motor and actual brush resistance: ridge falls pass for motorcycle/ATV in 6.28s / 6.48s (`recovery-checks.txt`). Gully undershoot recovery reaches the existing northern shallow exit in 20.42s / 17.12s (`gully-recovery-checks.txt`, `atv-gully-recovery-check.txt`). Failed direct-line and channel-edge pilot attempts remain as evidence.
- Original roads and shoulders: 834 support samples have maximum height change 0 (`road-supports.txt`). New apron overlap detected in an earlier audit was trimmed from new meshes only. Culvert crossing roof cover is measured in `tunnel-crossing-cover.txt`; retained trees in `map-grounding-checks.txt`. Complete conflicting trees were removed locally and affected retained trees reseated. The Forward portal tree was removed as a complete tree after final visual inspection.
- Preservation audit (`preservation.json`): six other scenes and eleven protected global runtime sources are byte-identical; nineteen original route/gate/flight components in each Backyard scene are unchanged. No accepted road or ramp was reauthored.
- Final scene-rendered ridge, portal, interior, gully and closed-Forward-grate views were inspected. Actual-geometry world maps and eight saved-course previews were regenerated.

## Limits and stop condition

Checks used targeted scripted Unity physics with ordinary vehicle motors, not human-controller playtests. Initial failed candidates and fixture errors are retained rather than hidden. The final motorcycle drain proof is composed of its passed culvert traversal and the corrected jump/runout check; there is no claim of a final uninterrupted full race. No broad AI/weather/vehicle matrix was run. AI stays on the existing main course because both new branches have `aiValidated=false`.

The routes are about 12.6% / 14.5% shorter than the corresponding main-route sections. Actual lap-time savings, jump feel and difficulty remain for Dan's review; these tests do not establish a guaranteed time advantage. Existing automatic respawn logic is unchanged but was not exercised by the manual simulation fixture. No new reset trigger, invisible wall or forced launch was added. Targeted checks are complete; stop gameplay tuning and deliver the review build.
