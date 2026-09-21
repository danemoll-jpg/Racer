# CR-105–111 targeted validation

CR-105–111 targeted gameplay validation is complete. Final Windows player: 0.19.0-review3. Package identity and its single portable startup are recorded separately in DELIVERY.md and package-final-verification.json. Historical failures below are retained, with their specific corrections and reruns.

Safety checkpoint: `542a61629c4a8ea0ef0763064b7c01c9c8d2c33f`, verified clean before edits. Staging and commit each initially failed on sandbox index.lock permissions and succeeded using supported elevation.

Scope: only the requested continuation/traffic, three mailboxes, lap selection, saved playlists, fast-travel confirmation, two mountain courses and Kyle-property turkeys. Ghost human testing remains pending; no ghost tests or rule changes. No old-course race, difficulty/seed matrix, broad records, wildlife, map, collectibles or title suites.

All test players require an isolated `-racerTestSave` directory. They launch with test-only listener mute and isolated master=0. The sole planned audible exception is a turkey-only window capped at 1.2 seconds, with other game sources temporarily muted and an automatic return to silence. Delivered audio preferences remain normal.

Observed authoring/tool failures retained:

- Sandbox could not read Unity discovery metadata; elevated read found the ready Editor.
- Live commands timed out during/background to assembly reload. Foreground inspection verified saved edit mode, and a subsequent command succeeded.
- The first `mountain-geometry` request was read by the preceding compiled job handler before the new handler loaded: `Unknown job mountain-geometry`. Retried with a fresh request after compilation. This is an authoring dispatch failure, not a passing geometry test.

Physical tests use ordinary frame simulation and throttle/brake/steering on the existing motor. Test setup placement is distinct from traversals. Automated checks do not imply physical Deck/controller use, subjective listening, or Dan's acceptance.

Final outcomes appear at the end; earlier entries distinguish failed candidates from the delivered behavior.

First playable results (before direction signs): forward and reverse giant flights passed centered and +3 m off-center lines for motorcycle/ATV. Forward motorcycle off-center initially failed during rollout with a deliberately delayed braking fixture; immediate normal braking passed without geometry changes. Full clearance, reachable run-up speed and supported stable landing CSVs/screenshots are retained in first/. This does not yet claim final compiled confirmation.

Initial narrow feature checks passed selector 1–5/wrap, solo Unlimited conversion and remembered finite preference reload; playlist edits/definition reload and incompatible player prompt; named fast-travel No/Yes/revalidation failure and one-shot closure; affected highway eastbound junction and three open continuation points. Turkey-only 1.2-second sound output was followed by verified listener remute. No subjective listening or physical controller claim.

Observed failures under correction:
- Mailbox screenshots were obscured by the ready menu; recapture those three views in free roam.
- Turkey population fixture left the group in camera view, preventing normal hidden reselection; turn the fixture camera away and rerun only presence/movement/placement, without another audible check.
- First full forward mountain race did not activate START at the tight closing bend (0 laps, 0 missed gates, repeated recovery). Stopped that test instance after the first failed case. Move only the new-course start planes to supported straights and rerun the affected new-course races. No old-course gate rules changed.
- First mountain-details request reached the older compiled dispatch handler and reported Unknown job; unique retry after compilation succeeded.

Corrected-player evidence (Assembly-CSharp SHA256 1D76C8C82E1E53A33526584455AE3C656D73B0E9A925F1DF7CE64CBAD588D94B):
- Forward motorcycle 1-lap finish/category and all three AI finishes passed. Ridge entrance tracking failed after a late, fast turn; new mountain-only approach timing/speed corrected. A missed gate remains under diagnosis; penalty logging added to the next affected run.
- Reverse run crossed START but the relocated plane lay inside the ridge's rejoin interval and gate order was wrong relative to that origin. Stopped that failed run. Moved reverse START to station 100 outside both choices and sorted only its new-course gate array.
- Mailbox recapture confirmed Dan and House 2 outside fences/clear/grounded. House 3's post was obscured at the steep fence corner; physical ray diagnostic confirms its original ground support, and it is being moved 2 m toward the shoulder for clear access and visibility.
- Turkey present/absent, startle escape and nonblocking checks passed. Three-second idle-walk sample hit the resting phase; broadened just this fixture to one 22-second idle cycle, stopping as soon as walking is observed. No second audible test.
- Summit score still zero on a stable centered motorcycle landing. The lip's support rays drop before the previous grounded-wheel sample, resetting warmup. Summit-only warmup now permits the observed supported steep ramp attitude; other jumps retain their original limits. Actual contact-normal impact, wipeout, wetness and settled support checks remain.

Remaining branch traversals passed in corrected/: forward motorcycle/ATV approach 167/167 points and return 131/131; reverse both profiles approach 209/209 and return 301/301. Each direction's single supported local recovery passed. These use ordinary motor inputs from setup placement; CSVs retain speed, ground contacts and controls. Final geometry is unchanged by the later new-course gate, driver approach and mailbox edits.

The forward missed gate was the descending post-rejoin CP at (786.67,108.46,62.25): CSV shows the driver airborne approximately 9 m south of its opening. Added anticipatory downhill braking only on mountain course IDs. The accepted global gate/finish rules are unchanged.

Approved cover source SHA256: 2070F4D996567CD3ACE865C52FD30E8EDCBCC34F519DE80C677BAF2AED0C32B4. Package-Racer always copies this source as WoodstockRush-Cover.png, independently of version, and includes it in full manifest/ZIP verification.

Release candidate 1 (F5A1C34C8F1F01C42519630A46C157B2BA05966B7391F07806A4F98F9B6BEDAA):
- Forward motorcycle 1 lap and ATV 5 laps completed with no recovery. Ridge entry and exit recognized for player/AI; no penalty within that alternate. All AI finished those events. One later CP04 miss was observed in each player event (ATV lap 4); moved that checkpoint from the descending bend onto station 940's supported straight, with a local crossing check required.
- Playlist restart, next direction/position, final event completion, quit and saved definition reload passed. Reverse middle event failed to finish (16 recoveries), so its completion/AI assertions failed and are retained. Retest only that event and its next/quit transition after repair; no repeat of the passed 5-lap event.
- Reverse obstruction physics audit found Ground_Mountain_2_2 forming a steep bank at the ridge entry where the broad early catch slope overlapped it. Remove the unused s330–370 catch-slope section and clear only the local road/ridge junction. Original first contacts were s425 ATV / s449 motorcycle. Because landing geometry changed, rerun its centered and +3 m lines for both eligible profiles; retain original successes and this collision failure.
- House 3's adjusted mailbox is now visibly grounded outside the fence with a clear post and access. Dan/House 2 remain unchanged since their passing screenshots. All three visual checks complete.
- Turkey idle walking passed over one idle cycle; screenshot also shows head-down peck pose, grounded legs and clear driveway. Presence/absence, startle and single brief audio/remute were already passed. No more turkey checks needed.
- Explicit incompatible player/AI roster prompt preserved all choices until selection; choosing ATV changed only incompatible entries. Cancel returned to setup and retained the definition.

Candidate 1 runtime is retained separately. Final delivery advances to 0.19.0-review2 so the failed candidate is preserved intact.

Local repair audit: the former reverse junction hit had normal.y=0.31 and blocked the approach. After repair the same road-facing cast reaches only the supported ridge surface (normal.y=0.99); the former wall hit is absent. This is a collision query, not a replacement for the pending normal-input reverse race and changed-landing flights.

Results screenshot review found playlist position appended below the fixed-height details area and therefore clipped. Moved its current/total position into the heading. The next corrected reverse results screenshot verifies this directly; no extra old-menu suite.


## Final targeted outcomes

Final compiled Assembly-CSharp SHA256: `C0E21EF8E43D64A96FBE9C0BF10B90F87A30ADBB177213DD9DAECA127FF3EDC7`. Build succeeded with 0 errors / 4 warnings. All six scene binaries are byte-identical between the tested review2 geometry/UI player and review3; see course-binary-continuity.json. Review3 changes only the named giant-flight award policy plus version metadata.

| Request | Actual targeted result |
|---|---|
| CR-105 | Three former continuation closures have no closure collision and accept continuation positions. Highway eastbound traffic passes the affected Cherokee junction straight. No old-course race/AI suite. |
| CR-106 | Dan and House 2 visual checks passed in corrected/mailbox-views. House 3's shoulder correction passed visual inspection in release1/remaining-local. Posts grounded outside fences and clear of gates/road; prefab behavior retained. |
| CR-107 | 1–5 selector/wrap, solo Unlimited conversion, remembered finite preference and disk reload passed. Forward motorcycle 1-lap and ATV 5-lap finishes/categories passed. Reverse ATV completed 6 Unlimited laps, no race finish/total record, then exited to menu. |
| CR-108 | Three-entry mixed-direction/lap definition editing, reorder/remove/add and disk reload passed; current-entry restart, next title/position, final event completion and quit/retention passed. Failed reverse middle event was repaired and retested separately, including results → final 5-lap entry → quit. Results screenshot clearly shows 2/3. Explicit incompatible player/AI selection preserved choices until approval and changed only incompatible profiles. |
| CR-109 | Named confirmation, No/cancel without movement, one Yes travel with map/modal closure and arrival feedback, repeated confirmation consumption, and one now-unavailable destination with explanation passed. Back uses the same cancellation path. No fog/collectible suite. |
| CR-110 | Four requested profile/direction runs covered by forward motorcycle 1 lap, forward ATV 5 laps, reverse motorcycle 1 lap and reverse ATV Unlimited 6 laps. Ridge entry/exit recognized with no branch-credit penalties. Both profiles drove each remaining summit approach/return locally; one local recovery per direction passed. AI completed one short race in each direction. Both giants passed centered/+3 m lines for both profiles; reverse repeated only because its landing geometry changed. Final compiled centered forward motorcycle and reverse ATV confirmed physical clearance, stable landing and positive awards. Local moved forward CP04 accepted a normal-input crossing with zero misses; its prior gates were explicitly fixture setup. Six course/playlist IDs and directly affected categories checked. |
| CR-111 | One present/absent turkey selection, grounded recognizable placement, idle walk/peck, startle and nonblocking/clear driveway observations passed. One turkey-only audible window automatically remuted. No other species/audio suite. |

Run limitations are explicit: automated motor controls, not physical controller/Deck testing or Dan's subjective acceptance. Forward 1- and 5-lap runs each had one ordinary CP04 miss before its local relocation; the relocated gate passed its narrow compiled check. Corrected reverse motorcycle completed with zero misses and 3 safe recoveries. Unlimited ATV completed 6 laps with one ordinary post-rejoin gate miss and 3 safe recoveries. These are not claimed as clean/no-reset laps. No ghost test was run; HUMAN ghost review remains pending.

The retained review2 score failures came from ordinary impact rejection despite supported settled landings. Final policy applies only to `summit-homeward` and `summit-southface`: 0.75 s sustained supported settling instead of ordinary impact limits; wall contact, wipeout, water, direction, distance and airtime rejection remain. Other jumps retain their prior limits. Final isolated records show homeward motorcycle 229.7276 m and south-face ATV 205.3689 m, both awarded gold. South-face contact-normal impact was 21.47 m/s, just above the old ordinary 21 m/s rejection, with contact normal up=0.86 and a stable supported landing. No old activity-board suite.

Geometry ranges: final reverse four-line tests had 7.02–7.52 s airtime, positive minimum terrain clearance 0.2446–0.5927 m, 66 stable landing frames and support alignment ≥0.998. Final homeward motorcycle clearance was 0.6761 m with 7.52 s airtime and 66 stable frames. Reachable launch speeds came from actual 215 m horizontal run-ups from rest, using the ordinary motor.

All ordinary tests were temporarily muted in isolated Temp saves. Only the single requested turkey sound window was audible; automatic remute passed. No saved personal audio/music preferences were changed, and ordinary release launches do not enable the test mute. No uploads, multiplayer, vehicle/stat/unlock or driver-customization work was performed.
