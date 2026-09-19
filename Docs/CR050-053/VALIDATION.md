# CR-050–053 — 0.6.3-review1

Implemented together; **awaiting Dan's review**. BUG-004 and earlier human/controller/listening checks remain open. Checkpoint: `04447830beeb1563ab94f71c60bfec3bb1ce4520`. Git lock permission failures were retried through supported elevation. No discarded changes, history rewrite, hook/signing bypass or external distribution.

## Gate placement and accounting

Live Editor inspection verified displayed CP numbers equal array indices (START is index 0). Before: CP06 at 1426.91 m, CP07 at 1540.25 m, CP09 at 2119.85 m, CP10 at 2185.03 m. CP06/09 were already in the appropriate bypass lists, but the following mandatory gates left only 40/35 m beyond Creek/Fox exits. Previous driving probes stopped at the exit and did not validate those approaches.

* Move CP06 to 1680.02 m, **180 m after Creek's 1500 m rejoin**; consolidate old CP07 into it.
* Consolidate old CP09/10 into **new CP08 at 2329.70 m**, about 180 m after Fox's 2150 m rejoin. No gate remains in the narrow 70 m Fox-to-Pine entrance gap.
* There are now 17 numbered checkpoints plus START. Labels, transforms used by swept detection, road stations, required checkpoint counts, AI progression, HUD/ledger numbers and lap completion all consume the updated array. Creek bypasses 4/5, Fox 6/7, Pine 8/9/10; Southwest remains an optional route with no bypassed gates. Drivers choosing the following shortcut earn its own entitlement before its bypassed gates.
* Road and shortcut geometry, house placement and recovery mechanics are unchanged. The existing latched credit survives deviation/fall/reverse/recovery. Unrelated misses still use the sole five-second charge path. New records use `street-v8-landings`; old records are retained separately.

See [actual-geometry gate map](gate-map.svg), [before stations/labels](gates-before.txt), [after stations/labels](gates-after.txt), and rendered [Creek before](creek-before.png)/[after](creek-after.png), [Fox before](fox-before.png)/[after](fox-after.png). Removed gates are removed from detection and progression, not merely hidden.

## Automated driving and preservation

**96/96 extended ordinary-frame attempts finished with zero misses and zero penalties**: four vehicles × four routes × four shortcut variants plus two road alternatives. Every attempt now continues past the next road gate, rather than ending at the shortcut exit. All profiles completed 24 attempts; each performed eight local recoveries (32 total). Initial pose/speed is seeded; subsequent movement uses normal Update/FixedUpdate, virtual pedal/stick input, production physics and production race sampling. Clean/imperfect lines, airborne motion, physical excursions/reversing and recovery are covered. Swept deep-fall fixtures supplement actual driving; this is not exhaustive human playtesting.

Raw aggregate: `route-matrix.csv`; individual physical traces, excursion measurements and 77/77 per-suite invariants: `routes/`. The first extended candidate exposed a **test pilot bug**: it used the next branch's context to steer back to the old exit. The corrected pilot tracks its intended branch and ends after the next gate. Initial invalid results remain under `Temp/CR050-tests/creek` and `fox`, excluded from the 96 passing results.

* **111/111** updated rule/accounting/recovery and dynamic crossing checks: `rules/`. Two stale legacy expectations (old Jamerson CP14 and old record-category text) were updated to CP12 and street-v8-landings; the underlying physical crossing tests also pass.
* **99/99** combined standalone boundary, entitlement, ledger, controls and decoded MP3/WAV/Ogg DSP checks: `combined-checks.txt`. Actual per-profile fixed-step ordinary misses remain one +5; zero authorized charges; pause ledger count/sum reconciles. Synthetic corrupt files intentionally produce decoder diagnostics.
* **137/137** preservation-flow checks: `flow/standalone-flow.txt` (virtual keyboard/gamepad/mouse; garage, colors, mixed roster, recovery, saved controls/volumes, restart/Quit Race/results and destructible restoration).
* `preservation.txt`: of 14,305 original scene transforms, only the two moved gates change pose; the race root's child list changes due to removal of 12 gate-group transforms. Handling/profile/recovery/audio/destructible/road/shortcut/progress source files listed there are byte-equivalent after newline normalization. Difficulty constants were not edited.

## Traffic

Original procedural sedan, longroof wagon/SUV, open-bed pickup and tall cargo van bodies replace the identical ambient body. Four cached body variants per pooled car use shared materials, nine plausible paints and weighted shuffled selection (6 sedan / 4 wagon / 3 pickup / 3 van per randomized deck); recent identical bodies are discouraged and consecutive paint choices differ. The deck is reshuffled, not a fixed visual sequence. Appearance remains stable until initialization or offscreen recycling. Conservative body-matched boxes are at most 2.08 m wide and 4.3 m long; wheels/cab/body fit the envelope. Main-road population remains 16 highway + 4 local, with original driver pace/lane constants. All 16 decorative continuation cars use the same art pool.

[Standalone body gallery](traffic/body-gallery.png) verifies the distinct geometry; this controlled gallery is **not** a congestion test. Actual race telemetry/screenshots are recorded separately in `traffic/`. The first Normal three-lap run had zero misses for all racers, three finishes and one Longroof DNF after a collision-related delay. This result is retained in `traffic/first-race.txt`; it is not a clean four-racer pass or evidence of human difficulty acceptance. The repeat race finished **4/4, zero misses and zero racer/traffic recoveries**, with 148 offscreen highway recycles and zero detected visible appearance changes. All four bodies and nine paints appeared. The previous-build comparison also finished 4/4, with one racer recovery. Current vs previous race times (seconds): original 430.820 / 443.005; tourer 428.669 / 439.403; motorcycle 392.522 / 404.458; ATV 426.953 / 422.630. These vary with contacts and are not a difficulty retune or statistical guarantee. A 2.52 m moving near-lane centre-distance minimum is not a collider-penetration measurement; traffic contacts remain possible. Final/baseline race results are recorded alongside the initial DNF. Decorative wheel support was then aligned by 0.45 m to the new shared art; population and motion were unchanged.

A final sequential visible standalone comparison (1280×720, VSync off, 120 fps cap, Normal, one lap, 20 traffic) finished 4/4 with zero misses/recoveries in both builds. Previous/current median: **8.33/8.33 ms**; p95: **8.35/8.40 ms**; peak process working set: **502.1/513.2 MiB**. This capped single-run comparison is not a hardware-wide performance guarantee. See `traffic/performance-comparison.json`.

## Recursive and portable music

Final expanded collection suite: **41/41**, including root with multiple artists/albums, artist root, flat folder, recursion On/Off, source/root/preference persistence, addition/removal rescan, cancellation/stale-result rejection, shuffle across artists and actual nested WAV DSP output. The supported codecs and existing metadata reader/filename fallback/volume/control behavior are retained; MP3/Ogg decoded output is also covered by the 99-check combined suite.

**12,000 paths** were discovered and installed by the actual radio without the former 2,048 truncation. Discovery performs no audio decode or bulk metadata reads. The synthetic large-library path files are deliberately not claimed as 12,000 playable songs. While a standalone pilot drove: discovery max observed callback gap 27.32 ms; full radio scan/install max 43.19 ms; first whole-collection shuffle request 6.32 ms. These are concurrent/background-window diagnostic timings, not foreground GPU benchmarks or a subjective hitch guarantee. `collection/scan-frames.txt` preserves exact measurements.

Directory-loop fixture: three supported tracks found once, one junction skipped (`link-loop-test.txt`). Empty/missing roots, removed/corrupt files, known format/size boundaries and cancellation are exercised. Actual ACL-protected folders were not created; the same exception-handling path reports inaccessible directories. Explicit limits and OS-call cancellation caveat are documented in [RADIO.md](RADIO.md): 100,000 tracks, 250,000 entries, 20,000 directories or 30 seconds; a limit reports unvisited remainder as unknown. Playback skips remain visible in scan status.

Two generated tones in nested `BundleMusic` artist/album directories were packaged, extracted elsewhere and played with nonzero decoded DSP output (**4/4 portable checks**). Rebuild/repackage retained staged hashes. An unstaged generated song added directly to Latest/Music was preserved in the previous complete runtime and excluded from the ZIP. Only the two deliberately staged files were included; external custom libraries were never copied. `music-package-tests.json` records all checks. Generated staging files were moved to `Temp/Generated-bundled-validation-retained`; **Dan has not staged songs**, and the delivery Music folder is empty except instructions.

## Delivery and remaining review

Final packaged executable smoke check passed **77/77** invariants and exited normally with the empty bundled folder (`delivery-smoke.txt`). Empty-package checks also verify loose unstaged audio is preserved in the backup and excluded from the ZIP (`empty-package-tests.json`). Windows build succeeded with **zero errors**; one warning reports intentionally absent optional Runtime Pipeline configuration. The initial full asset build also retained the existing future collider-prebake warning. Final build report: `delivery-build-status.json`. The first candidate's API timeout and uncompiled-editor-code warning are historical, not counted as a clean final build.

`Play-Racer.cmd` is unchanged. Versioned runtime and ZIP: `Builds/Racer-0.6.3-review1-Windows`; complete regular runtime: `Builds/Latest`. Every packaged file is SHA256-compared with the extracted ZIP, versioned runtime and Latest. Runtime `SOURCE-SHA256.txt` binds Unity source/assets/settings/packages to the tested source. Final completion commit stamping and package verification are recorded in `Builds/Latest/VERSION.txt` and `Builds/PACKAGE-LATEST.json`; the pre-completion verification record is explicitly labeled as such here.

Add selected songs to project **BundleMusic**, then double-click **Package-Racer.cmd**, or run `pwsh -NoProfile -File Tools/Package-Racer.ps1 -Version 0.6.3-review1`. No Unity recompile is needed. The tool preserves old complete runtimes and ZIPs under Builds/Preserved before replacement, rolls back a failed replacement when possible, and never copies a selected custom collection. Read RADIO.md for migration of songs added directly to Latest.

Dan's short checklist:

1. Drive the revised Creek/Fox exits at speed, wide and airborne with each vehicle; try falls/reversing/local recovery over several laps.
2. Expect zero authorized bypass misses. Deliberately miss one ordinary gate: exactly one +5; pause/results ledger and notice totals must agree.
3. Inspect sedan/wagon/pickup/van paints on highway, local roads and continuations; watch for popping, clearance problems or congestion.
4. Select a collection root, one artist and a flat folder; toggle subfolders, add/remove tracks, rescan and use Next/Previous/toggle/volume while driving.
5. Add selected bundled songs, package, extract elsewhere and listen. Check the physical controller, native folder picker and subjective audio balance.
6. Recheck handling, garage/colors, compact HUD, race flow, destructibles and saved records/settings. Lake/woods, stunt challenges, ghosts and multiplayer were not started.

No subjective listening, physical controller test, native picker interaction or Dan acceptance is claimed. Scripted success does not close earlier outstanding human reports.
