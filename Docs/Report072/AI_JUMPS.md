# 0.72 Part B — AI riders lost after big jumps (Mountain Loop Forward and Reverse)

Real races, normal AI field (EMBER moto, GOLD moto, BLUE ATV, Normal difficulty), 3 laps, the player parked off the course, run at 4× time scale (physics step unchanged). Every AI sample every 0.25 s and every AI recovery (from / to, distance, diagnostic) logged: `checks/partB-*`. Per-jump table: `Tools/Report072/Analyze-AiJumps.py` (an attempt = crossing the lip within 25 m of the flight line; success = no recovery and still moving above the world 15 s later, or past the landing). Some Reverse laps take the Summit Traverse shortcut, which skips the South Face jump, so South Face attempts are fewer than laps.

## What caused the disappearances

1. **Healthy riders were reset in mid-air (the main cause, both directions).** `RoadDriver.TrackRecoveryProgress` called a rider "off route" when it was more than 6 m above (or `halfWidth + 3` beside) its projected route point, fixed a rejoin target at that moment and counted "no progress". The big flights last 5.6–6.4 s far above the route, so after 5 s `racerStuck` fired while the rider was still in the air, about one second before touchdown — Dan's "landed and then disappeared". The old recovery (earned anchor) then put it **400–670 m back**, sometimes over 1 km, which takes it off the minimap. In the 0.71 races this happened on almost every flight: Reverse — all three rivals DNF, 219 recoveries; Forward — BLUE DNF, 54 recoveries. Not a recent regression: this code predates the Mountain courses; the long Mountain flights expose it.
2. **Rivals had no fall-through failsafe.** `RaceDirector` disables `VehicleRespawn` on rival clones, so the 0.68 "below the world → nearest point" failsafe never ran for them. A rival that fell through the world fell for ever (one fell to y −16,000) and vanished; the old stuck counter only caught it by accident.
3. **Real jump failures (the approach):**
   - Forward Homeward: take-offs at 31.4 m/s overshoot onto the upper deck and crash (3 of 9 in the 0.71 race); 29.0–29.3 m/s land and continue.
   - Reverse South Face: 36.4–36.9 m/s come up short of the receiving deck (the known s ≈ 1181–1183 undershoot); 37.2 m/s and above land.
   - Forward Homeward run-up starts in a U-turn: pure pursuit steers less as the target swings past 90°, and the flight "commit" (flight-axis steering, full throttle) began mid-turn, so a rider that ran wide went straight off the deck edge.

**Minimap:** a rival is hidden only when it is DNF (race time limit) or outside the map window (~±155 m). Recovering rivals and rivals classified by `AiFinishEstimate` (frozen where they stopped) stay drawn. So rivals vanished only through long teleports and falls through the world, both fixed.

## Fix (AI and recovery only; no geometry, no global AI retune)

- `RoadDriver.TrackRecoveryProgress`: a rider in the air (no wheels down, moving > 3 m/s, not more than 15 m below the course) is neither progressing nor stuck; on touchdown it is judged afresh. Off the near band, progress tracked near the last station (`ProjectNear`, within 12 m vertically and `halfWidth + 15`) counts as progress, so a rider landing on a deck where the global projection resolves to another level is not stuck.
- `RoadDriver`: the fall-through failsafe for rivals (below `fallResetHeight` → recover at once); `VehicleRespawn.RecordSafePosition()` keeps the last position above the world for them.
- `VehicleRespawn.TryRecoverLocal`: racing AI uses the player's 0.68 nearest-usable-track-point rule (`RecoverNearest`): facing forward, never across START/FINISH, a missed jump restored at/after its landing, never back over a ramp it reached from outside. No laps, gates or positions are awarded (a gate jumped over is a missed gate, as for the player).
- Approach (`MountainFlights` per-flight AI pedal data + `RoadDriver`): Homeward AI take-off limit 29.5 m/s; South Face: run-up entry speed 31 m/s (instead of the general 24), the run-up driven at full throttle (no pace / consistency lift) and on the line that carries the most speed to the lip (lane −4.5, x ≈ 994.5; on the other lines the lip speed scattered 36.5–37.8 m/s and < 37 lands short); a flight is committed to only once the rider faces it; on the Mountain courses a target more than 90° off is steered at full lock.

## Results

Before = 0.71 code (one 3-lap race per direction). After = final code (two 3-lap races per direction).

| Course | Flight | Before: success | After: attempts | After: landed and continued | After: success |
|---|---|---|---|---|---|
| Forward | Eastbound Gully Flight | 6 / 57 (11%) | 18 | 17 | **94%** |
| Forward | Homeward Summit Flight | 6 / 9 (67%) | 18 | 18 | **100%** |
| Reverse | South Face Summit Flight | 1 / 141 (1%) | 12 | 12 | **100%** |
| Reverse | Westbound Gully Flight | 0 / 79 (0%) | 18 | 18 | **100%** |

(Before-attempt counts include the retries after each reset back before the jump.)

| | Before | After |
|---|---|---|
| AI recoveries (Forward / Reverse) | 54 / 219 (one race each) | 4 / 2 (two races each) |
| Distance moved by a recovery | median 409 m, max 574 m (along the route up to 1,062 m back) | Forward 0, 5, 72, 76 m; Reverse 62, 36 m (all to the nearest usable point) |
| Rivals finishing | Forward 2/3, Reverse 0/3 | Forward 6/6, Reverse 6/6 |

Forward recoveries after: 2 × 0–5 m (a stuck nudge at s 1612) and 2 × the Homeward run-up U-turn (rider stalled on the outer corner, put back 72–82 m, just before the run-up's launch exclusion). Reverse: one rival left the South Face receiving deck after landing (put back 62 m on, at the landing), one at the crest (36 m back). The one Forward Gully "fail" was a rival standing still 15 s after landing, without a recovery.

Intermediate runs (kept for the record, `Temp` logs): after the detection and recovery fix alone, Reverse South Face was 4/6 and Homeward 4/4; with entry speed 27, South Face 11/13 then 6/8; with full throttle, 12/15; with the line, 12/12.

Final races: `checks/partB-fwd-after3-results.txt`, `checks/partB-rev-after5-results.txt` (every rival 3/3 laps, finished, drawn on the minimap); finishing orders `checks/partB-finish-orders.txt` (Forward: GOLD, EMBER, BLUE and GOLD, EMBER, BLUE; Reverse: EMBER, GOLD, BLUE and GOLD, BLUE, EMBER — the third rider always finishes; the log stops when all three have). Tables: `checks/partB-jumps-before.txt`, `checks/partB-jumps-after.txt`; per-sample logs `checks/partB-*-race*.csv.gz`.
