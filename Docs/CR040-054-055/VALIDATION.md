# CR-040 / CR-054 / CR-055 — 0.7.0-review1

Implemented together; **awaiting Dan's review**. Safety checkpoint: `522917554b0bbc7c8eebf664790b2c8573d0a88f`. Staging and commit lock permission failures were retried through supported elevation. No changes were discarded, hooks/signing bypassed, history rewritten, or files uploaded.

## Courses and preservation

Main menu > Track selects the original Street Loop or the separate Lake & Woods scene. Lake-v1 is approximately 1.91 km: start beside the friend's neighborhood, pass Dan's house on the retained road, then take the wooded dirt loop. Three supported gully jumps and creek crossings have graded approaches/landings. Birch Hollow is the optional shortcut; its explicit CP03 bypass is penalty-free. It enters at 740 m and rejoins at 1250 m; the next required gate is at about 1370 m. Seven active gates include START/FINISH. Original-course gates are inactive and absent from the new director's gate array; old shortcut components are not active race routes in this scene.

The original scene and StreetLoop/Woodland assets are unchanged. The new scene uses 21 independent sculpted terrain meshes and separate forest batches. Saved before/after positions and rotations of all 46 house/building roots match. Handling profiles, difficulty/AI constants, gate/penalty logic, recovery, traffic variety, vehicle colors, destructibles, compact HUD and Play-Racer.cmd match the checkpoint; see `preservation.txt`. The new circuit has local traffic rather than a nonexistent highway pool. The original retains its highway/local traffic population.

Physical standalone tests use actual rigidbodies, suspension and controls. They are automated pilots, not human driving:

- Both tracks: all four vehicle profiles finished two laps, with zero missed gates and zero automatic recoveries. An explicit local recovery preserved player gate progress and penalties. See `street-drive-1` and `lake-drive-2`.
- Original shortcuts: 96/96 branch/road attempts completed across four vehicles and four routes; zero misses/penalties. The combined matrix excludes repeated partial tourer rows from the first process. Existing rule suite: 77/77. See `original-route-matrix.csv` and `street-*` route folders.
- Lake shortcut: 24/24 attempts completed; zero misses/penalties, including imperfect entrance lines, braking/reversing and eight explicit local recoveries. Lake invariant suite: 23/23. See `lake-routes`.
- Lake jumps: 12/12 targeted runs (four vehicles × three jumps) reached upright supported landings. Initial approach speed was 32 m/s, followed by real motor/steering control. Measured airtime 1.48–3.10 s and peak height above the route 7.08–12.56 m. See `jumps/checks.txt`.

These matrices ran at 3× simulation time with ordinary physics steps; they are not foreground frame-rate benchmarks. Full race and route telemetry preserves the measured outcomes.

The first lake candidate failed: a shortcut approach overlapped a jump and removed lateral support; a protected approximate-house area also interrupted the trail. The rejoin moved beyond the landing, and the route moved farther from that house with smoothly blended preservation margins. `lake-drive-1` retains the failed telemetry; it is not counted as a pass. The corrected race, route and jump evidence supersedes it.

Sequential normal-speed, one-lap rendering probes explicitly rendered a 1280x720 offscreen camera with a 120 Hz target. All four vehicles finished each race without misses. Street: median 8.334 ms, p95 8.441 ms, peak process working set 528.54 MiB. Lake: median 8.334 ms, p95 8.350 ms, peak 555.17 MiB. These capped offscreen measurements show no observed regression in this probe; they are not a foreground presentation/physical-display FPS guarantee. See street-rendered and lake-rendered. Earlier street-regular hidden update-only timings are not rendering evidence.

## Record boards

Main menu or Results > Records / Top 10 provides Lap/Race selection, previous/next saved category, and a shortcut to the current configuration. The category identifies course version, vehicle, mode/difficulty, resolved roster and traffic; total races also identify lap count. Lap categories omit lap count. Ten rows show rank, adjusted time, vehicle and UTC date. New current-attempt entries and the leading personal best are highlighted.

`top-ten-v1.json` is separate from legacy best files. Attempt GUID + lap number/race identity prevents duplicate insertion; full double precision and persisted insertion sequence order ties. Completed laps use the finalized `LastLap`; completed totals use the frozen `AdjustedTime`, without adding penalties again. Incomplete events cannot insert totals. Records persist when menus reopen, saves reload or later racing is abandoned.

The final suite passed **29/29** checks (including preventing compatibility-best re-import duplicates): both boards beyond ten entries, ties and near-ties, persistence, duplicate identities, correct lap attribution, exactly-once penalties, incomplete totals, category isolation and migration. See `final-rules`.

Only known complete street-v8-landings legacy category schemas migrate. Their original files remain untouched; dates stay unknown, including after JSON reload. Older/incompatible files remain historical files rather than receiving invented categories, attempts or dates. `final-ui` contains the populated board render; `legacy-ui` checks a migrated unknown date after reload.

## Radio and packaging

Fresh preferences use portable Music beside the executable. Explicit saved source/custom-folder and Off choices are honored. Immediate child folder names are channels; nested artists/albums remain in their parent channel; root songs form General. The cycle is channels in predictable order, then Off. Track shuffle avoids immediate repeats where possible, with independent channel histories. Switching stops the previous load/voice; scanning remains asynchronous and bounded. A missing selected folder resolves to the first available channel on rescan; Off remains Off.

Final generated-fixture radio suite: **16/16**. It covers root/nested folders, empty/corrupt channels, zero/one/multiple channels, cycle/Off, shuffle/history, rapid switching, repeated rescans, removal/rename, saved source/channel/volume, and virtual D-pad gameplay/menu isolation. No unhandled exception or error appears in the final radio run. D-pad Down / M cycles; Left/Right / brackets select songs; Up / I shows channel/title; Settings offers equivalents.

Extracted fixture ZIP: **4/4**, including default portable source, Country/Rock channel names, nested Rock album playback and nonzero decoded DSP samples. The first audio measurement read an unprimed Unity output buffer and failed; it is retained in `portable`. The corrected primed-buffer measurement is in `portable-final`. This proves decoded output, not speaker-level listening.

Packaging fixtures verify that staged channel hierarchy is preserved, unstaged Latest music is backed up and excluded, and adding a generated song plus repackaging requires no Unity compilation. The fixture contains only generated tones. Personal staged audio stays outside Git. The release packages only deliberately staged supported files from BundleMusic; custom collections are never read/copied by packaging. Previous complete Latest/runtime/ZIP are preserved under Builds/Preserved before replacement. See RADIO.md for the exact procedure.

## Build evidence

Unity reports BuildResult.Succeeded. Its report includes one control-server timeout logged while the main thread was building, plus the expected disabled optional Runtime Pipeline warning; neither is a compiler/player failure. The final compiled player passed the 29-check records suite. Source hashes are shipped in SOURCE-SHA256.txt.

## Dan's review checklist

- [ ] Select each track; race multiple laps with all four vehicles and AI. Review fun, readable jump approaches/landings and local recovery.
- [ ] Take marked shortcuts, including wide/airborne/reverse/recovery exits: expect no bypass penalty. Deliberately miss a required gate: expect one +5 seconds.
- [ ] Open Lap and Race boards; inspect category labels, ordering, dates, new entries/PBs and persistence after quitting/relaunching.
- [ ] Listen to bundled/custom folder channels and nested albums; cycle every channel and Off, skip/back, rescan, adjust volume, and test the physical controller/menu separation.
- [ ] Repackage staged songs, extract the entire ZIP elsewhere, and verify portable playback. Recheck garage/colors, HUD, traffic, destructibles and race flow.

Human driving, actual listening, native folder-picker interaction and physical-controller testing remain unperformed. Automated success does not close earlier human reports such as BUG-004. No difficulty retuning or multiplayer expansion is claimed.

Release packaging verified 428 files across versioned runtime, Latest and extracted ZIP, including 187 deliberately staged songs. All staged source hashes remained unchanged. The previous full runtime/Latest were preserved. package-precommit.json records the pre-commit package; the final commit-stamped package report is Builds/PACKAGE-LATEST.json.

