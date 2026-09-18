# Mixed-vehicle race measurements

Full three-lap races: reference Street Classic, Longroof GT, Needle 600, Trail Four; Easy/Normal/Hard; four traffic vehicles. Matched mistake variants brake the reference player for three seconds at CP8, then request local recovery. A separate Hard run removes traffic. Helpers use isolated saves, VSync off and a 60fps cap. Concurrent helper frame figures are not performance benchmarks. The reference player uses the same difficulty policy as opponents; this does not establish human difficulty.

| Run | Vehicle | Lap seconds | Race / adjusted seconds | Misses | AI recoveries | Finished |
|---|---|---|---|---|---|---|
| 0-clean-traffic | YOU reference pilot original | 159.281/157.258/159.158 | 478.336 / 478.336 | 0 | 0 | True |
| 0-clean-traffic | EMBER tourer | 158.362/156.982/166.040 | 483.594 / 483.594 | 0 | 0 | True |
| 0-clean-traffic | GOLD moto | 143.383/143.035/143.429 | 431.438 / 431.438 | 0 | 0 | True |
| 0-clean-traffic | BLUE atv | 154.207/151.280/151.200 | 457.881 / 457.881 | 0 | 0 | True |
| 0-mistake-traffic | YOU reference pilot original | 161.537/158.770/157.297 | 480.256 / 480.256 | 0 | 0 | True |
| 0-mistake-traffic | EMBER tourer | 158.763/160.700/157.040 | 478.725 / 478.725 | 0 | 0 | True |
| 0-mistake-traffic | GOLD moto | 143.383/143.044/143.420 | 431.450 / 431.450 | 0 | 0 | True |
| 0-mistake-traffic | BLUE atv | 154.747/151.440/152.400 | 459.793 / 459.793 | 0 | 0 | True |
| 1-clean-traffic | YOU reference pilot original | 152.184/144.622/146.438 | 445.884 / 445.884 | 0 | 0 | True |
| 1-clean-traffic | EMBER tourer | 145.447/144.465/144.277 | 436.412 / 436.412 | 0 | 0 | True |
| 1-clean-traffic | GOLD moto | 132.298/131.684/131.296 | 396.873 / 396.873 | 0 | 0 | True |
| 1-clean-traffic | BLUE atv | 159.760/144.104/146.202 | 446.272 / 451.272 | 1 | 0 | True |
| 1-mistake-traffic | YOU reference pilot original | 149.944/144.699/144.722 | 441.993 / 441.993 | 0 | 0 | True |
| 1-mistake-traffic | EMBER tourer | 146.530/144.579/144.318 | 437.637 / 437.637 | 0 | 0 | True |
| 1-mistake-traffic | GOLD moto | 132.278/131.661/131.259 | 396.780 / 396.780 | 0 | 0 | True |
| 1-mistake-traffic | BLUE atv | 144.450/141.232/141.547 | 428.423 / 428.423 | 0 | 0 | True |
| 2-clean-clear | YOU reference pilot original | 142.998/139.174/140.685 | 425.406 / 425.406 | 0 | 0 | True |
| 2-clean-clear | EMBER tourer | 139.172/138.163/138.159 | 417.725 / 417.725 | 0 | 0 | True |
| 2-clean-clear | GOLD moto | 126.377/125.860/125.820 | 379.623 / 379.623 | 0 | 0 | True |
| 2-clean-clear | BLUE atv | 145.359/138.700/141.519 | 426.784 / 426.784 | 0 | 0 | True |
| 2-clean-traffic | YOU reference pilot original | 141.731/145.138/139.723 | 429.212 / 429.212 | 0 | 0 | True |
| 2-clean-traffic | EMBER tourer | 138.642/138.141/138.101 | 417.092 / 417.092 | 0 | 0 | True |
| 2-clean-traffic | GOLD moto | 127.126/125.879/125.902 | 380.484 / 380.484 | 0 | 0 | True |
| 2-clean-traffic | BLUE atv | 142.274/147.072/139.129 | 429.666 / 429.666 | 0 | 0 | True |
| 2-mistake-traffic | YOU reference pilot original | 147.794/139.935/139.457 | 429.813 / 429.813 | 0 | 0 | True |
| 2-mistake-traffic | EMBER tourer | 138.642/138.142/138.099 | 417.098 / 417.098 | 0 | 0 | True |
| 2-mistake-traffic | GOLD moto | 127.106/125.885/125.953 | 380.527 / 380.527 | 0 | 0 | True |
| 2-mistake-traffic | BLUE atv | 149.674/139.948/139.575 | 430.395 / 430.395 | 0 | 0 | True |

[Race summary](race-summary.csv) includes peak/mean speeds and braking time. [Sector observations](race-sectors.csv) derive next-gate transitions from 0.25s traces; endpoints are approximate to that sampling resolution. Raw pace logs retain throttle, brake, target, ground contact, coordinates and misses. The manually injected player recovery is recorded in each mistake.txt; it is not counted in the AI recovery counter.

The Hard mistake adds about six seconds to the reference player’s first lap. Final gaps are not monotonic because braking changes subsequent traffic encounters: the Normal mistake run can finish faster than the clean run. Report both; do not interpret that outcome as catch-up assistance. There are no boosted vehicle parameters, teleport catch-up or rubber-banding. All autonomous branch flags remain false: racers use the safe main road until their stunt controller is independently validated. Legal branch rules already apply identically to every RacerState. Human difficulty and subjective racecraft remain Dan’s review.