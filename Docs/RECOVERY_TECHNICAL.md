# Recovery placement contract

Recovery uses recent, occupied, stable course samples, not the last checkpoint or nearest arbitrary terrain. Keep the existing recovery delay/penalty and earned checkpoint credit separate from placement.

- A sample must represent grounded, upright, forward driving on the current legitimate route. Airborne travel never advances the safe anchor. After a stable landing, discard pre-landing fallback history.
- Resolve suspension support near the intended route elevation (a bounded local cast), never from above every stacked road. Validate the complete wheel footprint, slope, planar consistency, full rider/body clearance and nearby traffic before accepting or restoring a sample.
- Fit the support plane at the chassis centre. Using the highest uphill wheel as the whole vehicle's height rejects valid recent samples on slopes and causes excessive rollback.
- Shallow drainage water up to 0.18m is usable. Deeper water is excluded. Do not classify every visible water skin as a recovery hazard.
- Launch metadata must cover approach, takeoff/crest and immediate landing. `ReverseShortcutGuidance` flight stations are consumed directly by recovery. Existing named ramp faces and `JumpRecoveryExclusion` remain supported. New ramps must expose flight metadata or an exclusion; a `Ground_` name alone does not make a launch safe.
- Player pre-jump recovery needs acceleration room: legacy ramps reserve 22m of approach; the drain flight metadata reserves 32m before the lip through 6m beyond its landing station. Keep broad AI approach margins. Exclusions must be height-aware so a tunnel below a jump remains independent.
- Validate each lateral candidate against the jump exclusion as well as support and clearance. Do not shift an otherwise valid centre sample onto a ramp or wall.
- Never reintroduce coordinate-specific ramp-top spawn anchors, forward-earned teleportation, or rollback across an established landing merely because its checkpoint is older. If all recent placements are temporarily obstructed, wait for clearance.

Track edits must retain this contract and test one pre-jump failure and one established post-jump crash against actual saved support. Local underground changes additionally need a stacked-surface/shallow-water recovery check.
