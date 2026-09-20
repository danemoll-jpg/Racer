# Route fixes + arcade challenges investigation

Safety checkpoint: `13f989a408aaa6d085167d7cb480d9886af1997c`.

## Reproduction and geometry

`baseline/` used the existing authored reverse Trickum ramp and unchanged motor at ordinary frame speed, with every Street vehicle. Initial placement is before the approach; the trace after placement is actual throttle/steering/physics, not swept teleport traversal. Center and right-edge 12 m/s runs completed. They did not validate the left side.

`baseline-side/` reproduced three failures at the real left gravel shoulder (original, tourer, motorcycle); ATV completed with substantial tilt. Street Classic stopped at approximately `(-633.43, 8.58, -68.80)`, speed 0.006 m/s, four suspension contacts and normal suspension lift. Contact was `Graded aggregate shoulder`, normal approximately `(-0.68,-0.02,0.74)`. The screenshot shows the old forward 6.2 m segmented shoulder extending above the reverse surface. This is a collider/geometry obstruction, not insufficient engine power. The diagnostic records velocity, suspension lift, alignment torque and contact normals. No handling constants, boost or global collision switches were changed.

The initial reverse-only repair replaced those obsolete shoulder segments with a continuous supported mesh, two-metre side bevels and a 24 m rollout. The existing transform and approach are retained. `fixed-side/` repeats the four low-speed cases; all clear the former obstruction and recover. Side-edge runs still produce visible banking; completion alone does not establish subjective comfort.

## Recovery and route guidance

The prior reset searched arbitrary radial ground around the vehicle, which could retain a hill-bottom position or choose unrelated pavement. Recovery now searches behind earned branch progress or the last sampled supported course station, checks suspension support and full body clearance, predicts nearby vehicle motion, and searches bounded earlier alternatives. It selects the upper driving surface instead of terrain under a ramp. Free roaming separately records the recent supported main road, ambient street or branch and travel direction. Race resets rebase sampling without crossing gates or changing penalties/laps/elapsed time.

The prior AI progress guard only covered reverse branches. Main-road and branch lack of progress now receive bounded reversing attempts, then supported recovery. Repeated unsuccessful recoveries retreat farther and try another lane, with cooldowns. Estimates remain separate from measured finishes.

Wrong-way guidance retains the five-second evidence threshold and grounded signed motion rather than body heading. Off-route distance cannot indefinitely invalidate samples. Forest street excursions use the local street tangent toward the last associated trail position; route/branch state remains separate from guidance and shortcut entitlement.

## Failed and limited evidence

- Candidate1 rules: 20/21 on each course. The pause assertion compared fixed-step `Time.time` with render-frame `Time.time`; it was corrected to sample after a render-frame boundary. The original failures remain.
- Candidate1 recovery: Street 64/64, Forest 26/26, Street Reverse 56/56, Forest Reverse 28/28. These are controlled recovery fixtures, including synthetic displacement and occupied pads; they are not human driving into every hazard. Later tests add actual authored trunk/water/ramp locations and post-reset physical settling.
- Candidate1 speed tests: some straight-line test approaches crossed curves and struck trees before reaching the trap. These are retained as failed driving attempts. The subsequent pilot follows the authored centreline; no scoring test is credited merely because a previous attempt left a nonzero speed value.
- Some diagnostic tests invoke scoring events directly to check distinct-prop accounting and persistence. Those are expressly labelled RULE and are separate from physical driving.
- The first build report counted a CLI main-thread timeout as an error during an otherwise successful build. The subsequent build completed with zero errors and the two existing warnings. Build reports are retained.

Human driving, physical-controller validation, enjoyment, perceived difficulty and actual listening remain Dan's review. Automated screenshots and emulated input do not imply that review has occurred.

Forward regression later reproduced seven edge-contact failures against its original stepped shoulder. The same continuous-bevel treatment now retains its 40m/6.2m launch profile and adds supported sides/rollout. Candidate4 retains the complete new matrix and the earlier failed matrices remain available.

Final integration exposed two additional defects. Recovery clearance now includes the active rendered vehicle envelope (roof, mirrors and rider), with the wheel-contact region checked separately by support rays. The 282 hazard assertions and 80 local roaming assertions passed with this larger envelope. A persistent-obstruction follow-up was added after a full Street race showed repeated recoveries at the northwest junction. AI now owns its recovery retry bookkeeping, alternates clear departure lanes and briefly retains the actual recovered lateral line before returning to its usual line. This changes navigation choices, not vehicle physics or available power.

Jump scoring now freezes distance and airtime at first touchdown; the stability interval confirms the landing without adding subsequent bounce travel. Corrected forward Forest motorcycle calibration at the three measured approach paces is 25.71 / 41.93 / 60.66 metres, replacing the inflated earlier 115.85 metre measurement. ATV remains 22.46 / 30.00 / 38.62 metres. Reverse Forest is 22.61 / 36.36 / 43.76 metres (motorcycle) and 20.91 / 33.54 / 44.33 metres (ATV).

A records stress test exposed an intermittent atomic replacement failure. The shared save helper now retries bounded transient filesystem failures while retaining the prior file/backup. Final records assertions passed 29/29; a deliberately locked backup produced one retry and then preserved the old backup and new current file. Personal save data was not used by these tests.

The baseline pilot's input law is explicit in ArcadeRampProbe: throttle is clamped from half the speed deficit, and braking is 0.2 only above target speed + 1m/s. At the reproduced 0.006m/s shoulder snag with a 12m/s target this requests full throttle and zero brake. Thus the stopped frame is not explained by commanded braking. Steering, motor/velocity, contact normals, suspension lift and alignment were inspected; the obstructing original shoulder collider was corrected.

Review7 further separates the continuously tracked local road station from the last supported safe sample. The old implementation could freeze both after more than 90m outside its safe corridor and recover back to the ramp from the northern junction. The new tracking cursor advances through continuous motion, while only supported in-corridor positions become reset anchors. Large discontinuities do not advance that cursor, and reset still does not award progress.
