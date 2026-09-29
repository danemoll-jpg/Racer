# Dan's Backyard targeted validation

Safety checkpoint: clean `main` at `777b62427c2d527df78d6dc1a27db7086f345778`.

The supplied gully anchor is authoritative: **X 96.1, Z -108.6**. The initial comparison with the combined Forest Reverse atlas was incorrect and its conflict question was withdrawn. New scenes use the Street Loop world. Fern Gully geometry remains unchanged.

## Scope and preservation

- Forward and Reverse have separate scenes, identities, records and menu entries. The first six playlist indices remain unchanged.
- New terrain and scenery meshes are private copies under `Assets/Track/Backyard`.
- `preservation.json` verifies that the six existing scenes change only two mesh references apiece, for the explicitly requested local Kyle driveway repair. All remaining scene blocks are unchanged.
- Vehicle forces, accepted recovery tuning and AI shortcut probabilities are unchanged. New strict courses alone use ordered checkpoint handling and nearby-station AI pursuit to avoid snapping onto their own return trail.
- The existing minimap consumes the new route and registered optional ridge branch. Course arrows have no colliders.

## Findings corrected during verification

The initial new-scene save did not retain the strict-gate flag. It is now saved on both courses. Strict mode also needed to bypass the older missed-gate reconciliation loop; otherwise a skipped checkpoint could hang the main thread. The guard now skips that loop only for strict courses.

The reverse-facing deep gully lip was not physically climbable by the representative AI motorcycle. Reverse now uses its own nearby grounded southern crossing. Forward retains the jump and right turn at the supplied location.

The dump landing exposed an AI navigation error: after bouncing, unrestricted nearest-road projection could select the nearby homeward leg. Only strict-course racing pursuit now follows the current road station within a bounded window; recognized branches provide their own station. A longer level landing runout precedes the forest descent. Its initial terrain blend intruded into the new homeward trail, so that return leg was moved northwest and terrain blending now considers all nearby new trail surfaces. No global speed, handling or shortcut probability was changed.

Earlier failed/interrupted runs are retained in `editor-checks`, `final-editor-checks`, `corrected-editor-checks` and `runout-editor-checks`. They are diagnostic history, not acceptance results. Final acceptance evidence will be recorded below after the corrected run.

## Save isolation

The first fixture isolated settings but omitted the separate record-board service, creating six synthetic Backyard rows. They were removed with exact category/date guards and before/after verification of all real records, with the originals backed up locally. Five corresponding test rows were removed from the backup file. See `fixture-record-cleanup.json` and `fixture-backup-cleanup.json`. No real records were removed. Subsequent fixtures isolate both services; the restarted Editor and fresh runtime also use dedicated `-racerTestSave` directories.

## Kyle crossing

The local correction removes a maximum 0.7915m height mismatch, within 4m of the supplied X/Z and fading to the original height by 10m. A live motorcycle crossing at 16m/s passed: minimum speed 15.82m/s and maximum airborne time 0.00s. The earlier slow baseline did not reproduce a launch; this verifies the final transition rather than claiming an exact reproduction of Dan's original driving conditions.

## Acceptance and delivery

**Editor acceptance PASS.** `verified-editor-checks/checks.txt` records the Forward physical lap, all eight core checks and Kyle crossing. `verified-reverse-checks/done.txt` records all eleven Reverse core/physical/menu checks. Both full motorcycle laps completed with **zero recoveries and zero missed gates** (approximately 58 seconds Forward and 61 seconds Reverse). The batch Editor's end-of-frame screenshot wait was removed from the opt-in fixture after the Forward run; Reverse ran separately, avoiding a redundant Forward drive. Earlier menu action verification passed Forward-to-Reverse; the final fresh runtime checks both directions again.

Viewed the final property, dump and Reverse gully screenshots. The rendered atlas reflects the saved final routes: 1093.3m Forward and 1157.3m Reverse, ten gates each. Final existing-scene preservation was verified. The known Editor-only glazing material mutation was restored; no material/vehicle physics change remains.

Fresh-runtime core/menu verification and publication evidence are recorded after building. Detailed gameplay feel remains Dan's playtest; targeted gameplay acceptance is complete and no broad regression matrix is planned.
