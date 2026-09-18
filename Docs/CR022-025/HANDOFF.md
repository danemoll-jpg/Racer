# SESSION HANDOFF

**Current delivery:** CR-022–CR-025 integrated single-player update implemented; Windows 0.3.0-review1 built, tested and packaged locally; awaiting Dan's review. Final speed/handling, penalty feel and audio approval remain with Dan. CR-013/CR-018 remain deferred. Existing environment, shortcut, jump, camera and breakable scenery retained. No multiplayer.

**Safety checkpoint:** 30b82438708dc62754ff304528cf484069b94593. Initial index.lock permission denial recovered through elevated Git before edits.

**Build source:** de84d067cb14773d08309cf697037aca30522fac. VERSION.txt identifies this exact committed code. The completion commit records final validation/package documentation; its exact ID is in the delivery response and Git history (this code-source hash remains the package identity).

**Open/play:** Assets/Scenes/StreetLoopGreybox.unity. Consistent entry Play-Racer.cmd / Builds/Latest/Racer.exe is updated with the complete verified runtime. Versioned output Builds/Racer-0.3.0-review1-Windows; package Builds/Racer-0.3.0-review1-Windows.zip (76,322,288 bytes). Fresh ZIP extraction and Latest match all 228 file hashes. Previous Latest retained at Builds/Latest-before-0.3.0-review1. Nothing uploaded/distributed; Dan's download-package/friend-PC test remains deferred.

**Modes/controls:** Ready/results buttons select three AI opponents or Solo/time trial, and traffic On/Off; choices persist. Default three laps, four traffic cars. Existing RT/W acceleration, LT/S brake/reverse, stick/A-D steering, R/Y car reset, Enter/Esc/Start pause, A/Space menu confirm retained. Settings retain four volume controls, VSync/frame cap.

**Tuning:** Forward target 44→49m/s; acceleration 13.5→14.5m/s²; braking 24m/s² unchanged. Steering/grip/suspension/mass/camera unchanged. AI copies the same motor/tuning with desired pace variation; no rubber-banding or hidden speed boost.

**Miss/ranking rules:** Five seconds per ordinary gate miss. Extra cut charge max(0, sector length minus verified new forward road distance minus 25m)/10. Only new road progress near the road counts against that charge; loops/reversing/off-road distance cannot farm it away. Detect by road-relative direction/progress 18m past a gate or a later valid forward gate, charging each once. Misses do not restart laps. Normal/approved shortcut/jump remain valid. Finish must cross forward after visiting the middle 35–80% course region; repeated/wrong-way line crossings do not award laps. Teleports abandon the current lap without a penalty chain. Manual R/Y retains its old lap-abandon behavior and race clock/penalties; waits if another car occupies the reset pad.

**Timing/results/records:** Shared GO race clock; elapsed and penalties separate. HUD track position is physical progress. Provisional finish display until all finish or 90s after first finisher; 20min maximum race. Adjusted times rank finishers, DNF follows. Results include penalty totals/counts and a paged per-gate player breakdown. New street-v2-speed49-penalties categories include mode/traffic/laps; legacy records.json and settings remain compatible and untouched by tests.

**AI/traffic limits:** Three tinted existing car assets: Ember/red, Gold/yellow, Blue/blue. Simple road pursuit, braking/lookahead, clear-lane passing and obstacle query. Ground bypass only; player retains jump/shortcut. Opposing traffic uses the supported shoulder beside the narrow jump bypass. No city simulation. AI/traffic are silent rather than duplicating player engine sources. Recovery returns behind the last stable sample, never grants progress, refuses nearby cars/player; traffic recovery additionally requires distant, off-camera positions. Congestion and difficult collisions still need Dan's review.

**Evidence:** Two Editor one-lap AI/traffic races, 6/6 AI finishes, no recoveries/misses. Solo baseline completes. Editor median/p95 AI 16.72/23.48 and 16.70/22.39ms versus solo 16.67/21.42ms at 734×293, VSync1/cap60, i7-9700/GTX1660Ti. Virtual-Gamepad peak 45.60m/s; early-brake, corner entry, hill, hairpin and two jump fixtures recorded. Mailbox impact/cleanup/restart restoration pass. Route/record/cue/ranking/pause/recovery assertions are under Docs/CR022-025. Large-cut replay costs 482.36s; ordinary misses 5/10s; approved routes 0s. Initial route-cache failure, bypass deadlock and late-braking failure are documented.

**Windows results:** Visible 1280×720 three-lap race: YOU 8:01.793, Ember 8:06.893, Blue 8:09.203, Gold 8:30.308; all finished, zero misses/recoveries. Median/p95 33.33/33.37ms (frame-paced, not an uncapped CPU benchmark). Across the three AI races: 9/9 AI finishes. Visible standalone virtual-Gamepad solo/off tests reach45.60m/s; brake from45.24m/s through stop; jump tests at32.76/39.52m/s land on four wheels; mailbox cleanup/restart pass. Docs/CR022-025/VALIDATION.md contains evidence and failed attempts. No physical-controller, subjective-listening, friend-PC or downloaded-package claims; sustained adversarial pileups remain untested.

**Review checklist:** Race vs three AI with traffic; try Solo/traffic off; assess faster braking/hills/hairpin/jump; miss one/multiple gates and hear cues; verify reset/pause/restart/props; inspect adjusted standings and penalty breakdown.

---
