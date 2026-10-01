# Project cleanup and Mountain polish — 0.61.0-review1

Baseline: clean `b29330bc4ceecf8eda2dba77697583f538db002e` on main. This is the requested bounded pass; Dan's gameplay review remains pending.

## Changes

- Reconciled the current TODO into active, future, deferred, someday, and accepted work. Older implementation records remain explicitly historical. CR-118 remains open; no audio retuning.
- Forest Forward: inspected 20 trees within 35m of the supplied coordinate; reseated affected complete trees and their batched foliage. Final trunk-foot probes meet current terrain. Cave objects, shell, obstacle, materials, route and collision remain unchanged.
- Normal title dismissal now starts the existing Free Roam mode. A twelve-second prompt uses the established shaped Menu glyph, or Escape for keyboard. Start opens the existing main menu with Resume Driving and all existing destinations. B/Escape resumes; race pause behavior is retained.
- Race Complete has no existing root Back destination, so its ineffective Back prompt is hidden. Settings/subpage Back behavior remains available.
- Fixed an existing Unity constructor restriction exposed by normal Free Roam startup: AmbientVehicle creates its MaterialPropertyBlock in Initialize, rather than a MonoBehaviour field initializer.
- Mountain Forward/Reverse: earth shoulders support exposed pavement edges; intentional flight lips remain open. Original sign boards retain position/readability and their posts extend to ground. Seven Forward and eight Reverse posts corrected; nearby cairns reseated. Tree dependency inspection covered 26 Forward and two additional Reverse trees at new support, beyond the revised-tail tree pass; affected trunks/foliage/collision were reseated, and final foot gaps are zero.
- Reverse Summit entry has a continuous local apron/grade; reported gap and hanging section have earth support. First 245m of the overall shortcut retain their route. The final portion curves into eastbound main-course travel at road station 1672.47, with a 0.34-degree tangent difference. The branch is 461m versus approximately 1,227m of bypassed main road. The driving ribbon is 12m wide, with conservative 10m navigation width.
- Removed obsolete hairpin/turn-back signs and replaced only affected gold guidance. Local AI branch metadata and speed targets follow the revised line; global RoadDriver/vehicle/recovery code is unchanged. Bypassed gates are 2, 3, 4. A local merge exclusion keeps recovery anchors out of the convergence. New Reverse course identity preserves historical records; only its preview data changed semantically.

## Targeted evidence

- `preservation.json`: protected scenes and global systems match baseline; Forest component diff is restricted to trees/batched foliage, and accepted cave assets match baseline.
- `forest-trees-after.txt`: 20 local foot/terrain checks; no positive floating gap.
- `support-dependencies.txt`: newly supported Mountain tree footprint checks, final zero foot gap.
- `geometry-checks.txt`: 693 Reverse and 522 Forward branch probes at center and ±3m, zero missing support or high obstruction. Maximum route/surface differences 0.331m and 0.447m respectively.
- `local-driving.txt`: representative Forward motorcycle corridor traversal completes upright, maximum lateral distance 1.04m, maximum airborne interval 0.18s.
- `ui-checks.txt`: normal title input/release, Free Roam throttle movement, shaped controller glyph, Start/main-menu destinations, keyboard equivalent, resume, and accurate results/subpage Back behavior. Uses isolated saves and muted audio.
- `final-runtime/`: final motorcycle/ATV checks use the real production RoadDriver, its existing probabilistic shortcut choice, ordinary vehicle physics, and existing branch progress. Completion requires leaving the branch and continuing at least 20m along the main route. No teleporting across the shortcut.
- Both final runs PASS with zero resets and zero AI recoveries; maximum airborne interval is 1.00s motorcycle / 0.58s ATV. The raw maximum branch distance includes the intentional pre-entry main-road approach and post-merge continuation; it is not a measure of shortcut edge excursion.
- Saved before/after chase-height and overview images document the entry, supported shortcut and natural merge. These are technical inspections, not Dan's subjective acceptance.

## Failures retained and limits

Initial UI run exposed the MaterialPropertyBlock constructor exception, then a visual rich-text/spacing issue in the hint; both corrected. Initial manual edit-mode physics fixtures and AI runs exposed local surface interpolation seams and a too-fast final curve. Long retained triangles were subdivided only around that merge, its grade refitted continuously, and old support clamped below pavement. Final local AI curve target is 11m/s, earlier branch target 20m/s. Failure reports remain alongside final results.

The edit-mode manual multi-profile fixture is not the final acceptance evidence: it generated Unity edit-mode Destroy warnings and produced an inconsistent ATV result. Final real play-mode traversals supersede it. Test-generated material and physics-settings serialization changes are removed before delivery. Tests do not claim human handling, visual, audio, or physical Steam Deck acceptance. No broad regression matrix or unrelated track changes.

See PUBLICATION.md for completion source, fresh build, signed release, real launcher verification and cleanup.
