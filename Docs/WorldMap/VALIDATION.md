# 0.44.0-review1 — focused validation

Safety checkpoint: clean `main` at `73b0399b260cbf1d400f55d73bbd2d9f2d605828` before edits. The current request supersedes the previous full-world/menu-map backlog entry. Backyard Reverse and broader menu cleanup remain deferred.

## Tree-Top vegetation

- Removed only 83 of 621 combined Tree-Top shrub/fern clusters whose actual X/Z footprint intruded within 4.25 m of the approved main centerline. Retained 538; nearest remaining foliage is 4.294 m from center. This gives at least 1.644 m beyond the nominal 5.3 m trail. See `bush-clearance.txt` and `tree-top.png`.
- All **1,450** main-route body-position probes at center and ±1/±2 m, sampled every 0.5 m through the affected section, have zero slowdown. All remaining foliage is outside the clearance boundary; therefore main-line and slightly off-center vehicles do not contact these bushes. No terrain, road or platform changes were needed.
- Ground samples beneath the deck and at ±5/±10 m: **73 dense resistance samples, two correctly excluded main-trail samples, zero unexplained gaps**. The two are 3.132 m and 1.530 m from the main route, so slowing those would contradict the requested clear main trail. `geometry-initial.txt` retains the initial overly broad “all samples dense” assertion; `bypass-exceptions.json` records the actual positions, and the final check explicitly accounts for main-route clearance.
- The resistance component and all scene data are byte-identical to the checkpoint. No new fast ground lane was introduced by removing visual plants; resistance has the same footprint and force. Accepted shortcut collision, takeoff, deck, landing, route/gates and AI metadata are unchanged.
- This is local geometry/coverage verification, not a new driving-time claim. Prior unchanged-system measurements remain **4.96 s successful Tree-Top**, **6.80 s main baseline**, **15.10 s ground −5 m**; their original fixture limitations remain in `Docs/ShortcutRevision/VALIDATION.md`. No subjective tuning or broad driving matrix was repeated.

## COMPLETE RACE / CR-064

The existing post-finish button was present under a different label. Only its player-facing text changed to **COMPLETE RACE**. It remains centered near the top of the post-finish waiting screen with automatic initial selection and visible cursor. The existing guard requires a finished human, Racing waiting state, unfinished AI and classification not finalized. The click still calls the existing `FinalizeUnfinishedAi()` implementation.

The focused fixture uses isolated saves and legitimate checkpoint/lap progression through `RaceProgress.Cross`, then clicks the real button. It checks visibility before/after finish, final Results state, only unfinished AI estimated, existing human/AI measured times unchanged, estimated labels and exclusion from measured boards, repeated activation and the all-measured completion case. It does not alter estimation math, driving, penalties or records.

## Full-world cartography and full menu map

- Seven saved scenes rendered through Unity; no scene saved by capture. Bounds derive from actual ground/outer-road mesh bounds. Full-world source extent includes the farther eastern Mountain region.
- Master `WORLD_MAP.png`: **10,700 × 6,680** with margins; clean map area **10,400 × 6,240** over **X −1350…1650, Z −850…950**. The documentation grid is 200 m. Actual regional scene sources and their explicit composition are documented in [README.md](README.md); this is not a claim that differing course scenes have identical terrain.
- Full menu uses its exact active scene's **5200-pixel-wide** saved-world image and bounds. No technical coordinate grid in-game. Aspect, player heading, waypoint and destination positioning share the image transform. Existing discovery save/grid IDs and landmark/travel logic are preserved. Unexplored geography remains visibly dimmed; it is not marked visited.
- Current-course overlays use actual main/shortcut/gate data, default off, selectable through **Show / hide course**. Separate transparent documentation overlays align with `WORLD_BASE.png`.
- Focused UI check covers actual image load, six destination coordinate round trips, player-marker alignment, wheel zoom, drag pan, waypoint state, course toggle and closing back to racing. Images are captured for visual inspection. Travel confirmation/arrival logic is unchanged; no extra fast-travel driving matrix.
- Initial UI fixture stopped at image capture because the new custom overlay lacked `CanvasRenderer`. Added its required component and reran the focused fixture. Initial nine successful checks retained in `ui-initial.txt`. The subsequent 26/26 state checks passed, but their screenshots were obscured by startup artwork because the fixture advanced the flow directly. A synthetic title-dismissal attempt did not advance in the headless Editor and was stopped. The isolated fixture now skips unrelated startup artwork/audio (production startup is unchanged) for the final visual check. Final results are in `ui-checks.txt` / `ui-done.txt`.

## Protected content

`preservation.json` verifies every existing scene is byte-identical and the sole changed world asset is the Tree-Top foliage mesh. All other track assets, main route/gates, buildings, dump, ravine, Cabin shortcut and accepted jump structures are unchanged. CR-064, AI, vehicle physics and recovery sources are unchanged. **RacingMiniMap.cs and every serialized minimap setting are unchanged.** Play-Racer.cmd is unchanged.

Build/publication/launcher/cleanup results are recorded separately in `PUBLICATION.md`. Once the focused checks pass, gameplay testing stops and delivery proceeds.

The headless Editor stalled on the final capture-only startup attempts; they were stopped. The final visual check uses the required fresh standalone Windows build with `-racerTestSave`, `-racerSkipTitle`, and `-worldMapCheck`. This fixture is never activated by normal gameplay. Existing 26/26 state results stand; final player results/captures are recorded before publication.
