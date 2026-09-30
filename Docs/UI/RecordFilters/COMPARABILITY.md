# Records comparability audit — 2026-09-30

Safety checkpoint: clean main `966fa46016227fbedeaf0a9f85bb13d39a26b7de`.
Dan generally accepts the current UI direction; general polish is deferred. This focused request supersedes the older plan's configuration browser requirement.

Inspected `RaceDirector.Category`, `RaceProgress`, `RecordBoards`, `DriverVariation`, `AiFinishEstimate` and the actual saved archive read-only. Before implementation: 219 entries (161 lap / 58 race), 85 category strings; archive SHA-256 `0E4986AB04345F3EE81DC21F41CC8A38841366AB0F1B2D5471BEA01FFE286797`. 61 legacy best files were previously inventoried; this task fingerprints them again.

| Encoded dimension | Lap view | Race view | Reason |
|---|---|---|---|
| Course identity, direction, layout/rules version | Exact partition | Exact partition | Routes, gate requirements and legal shortcuts changed; no cross-era equivalence inferred |
| Race lap count | Combine | Required filter / partition | Individual completed adjusted laps use the same timing; race totals cover different distances. Existing lap insertion already strips the suffix |
| Player vehicle/profile | All by default; optional actual profile filter | Same | Overall fastest is intentional; profiles differ in performance, not clock units or penalty accounting |
| Solo / three AI, AI roster | All | All | Changes competition/collisions; does not rescale the player's timer or gate penalties |
| Difficulty | All | All | Drives AI variation and unfinished AI estimates; those estimates are not local player records |
| Traffic | All | All | Changes obstacles, not timing or eligibility; an overall fastest board includes either condition |
| Date, insertion order, legacy identity, attempt ID | Presentation and stable tie ordering | Same | Retain actual metadata; do not infer driver identity or missing per-attempt penalties |

Only recognized full suffix grammars can aggregate. Unknown metadata remains in an exact-category historical group, never silently merged. Each exact layout has its own History choice; raw identity is secondary in Details. Default is current layout, never automatic fallback to incompatible history. Empty current boards explain Filters / Record era.

The new layer only reads retained entries and sorts by full seconds then original order. It does not call Add/Migrate/Write, change identities, remove entries, or alter timing/eligibility/finish celebrations. Existing per-category Top 10 retention means previously discarded attempts cannot be reconstructed; no fabricated history. Existing legacy best files and constructor migration behavior remain unchanged.
