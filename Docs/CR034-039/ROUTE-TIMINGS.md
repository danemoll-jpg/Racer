# Measured route comparisons

Ordinary Unity FixedUpdate motor physics and virtual pedals/steering; no position or force driving after initialization. Two clean attempts (second target +2m/s), one deliberate 1.2s brake plus local recovery, two road controls per route/profile. Both alternatives start 12m before the entrance with the same initial velocity and end at the authored rejoin (branch exit tolerance 3m). Initial velocity is a controlled fixture condition. Traffic/opponents off; isolated saves. Entry speed is measured after braking, not the injected starting value. A failed attempt is retained as a failure, never a usable time.

| Route | Vehicle | Clean shortcut seconds | Road seconds | Recovery attempt | Clean entry km/h | Clean exits |
|---|---|---|---|---|---|---|
| Creek Leap | original | 14.680 / 14.550 | 22.522 / 22.557 | 17.441 | 121 / 120 | 2/2 |
| Creek Leap | tourer | 14.860 / 14.849 | 22.623 / 22.597 | 17.582 | 119 / 119 | 2/2 |
| Creek Leap | moto | 14.133 / 13.900 | 21.197 / 21.170 | 16.199 | 121 / 127 | 2/2 |
| Creek Leap | atv | 14.194 / 13.898 | 21.440 / 21.496 | 16.414 | 121 / 126 | 2/2 |
| Fox Gully | original | 19.466 / 19.101 | 19.936 / 19.903 | 21.399 | 87 / 92 | 2/2 |
| Fox Gully | tourer | 19.540 / 19.169 | 19.936 / 19.895 | 21.559 | 87 / 91 | 2/2 |
| Fox Gully | moto | 18.851 / 18.349 | 19.320 / 19.280 | 20.417 | 90 / 92 | 2/2 |
| Fox Gully | atv | 18.992 / 18.480 | 19.471 / 19.460 | 20.698 | 87 / 91 | 2/2 |
| Pine Ridge | original | 18.104 / 17.735 | 23.756 / 23.760 | 20.192 | 127 / 131 | 2/2 |
| Pine Ridge | tourer | 18.154 / 17.756 | 23.658 / 23.659 | 20.225 | 127 / 131 | 2/2 |
| Pine Ridge | moto | 17.441 / 16.944 | 21.366 / 21.336 | 18.909 | 129 / 129 | 2/2 |
| Pine Ridge | atv | 17.579 / 17.102 | 22.051 / 22.088 | 19.285 | 127 / 131 | 2/2 |

Every row and source folder: [comparison-runs.csv](comparison-runs.csv). Times above are raw elapsed seconds, excluding penalties. Motorcycle Creek road controls can miss CP4 by understeering outside the ordinary gate; their actual penalties remain in the CSV. Earlier unsuccessful terrain/controller candidates remain in candidate1–candidate3; candidate5 and candidate6 retain the strict landing-envelope failures that motivated the final shoulder fix. The final motorcycle creek rerun supersedes that case only. Initial and failed variants are not claims about the final route.

Creek: about 120–126km/h into the approach, align with the takeoff, then lift/brake for the hilly road rejoin. Gully: enter around 85–95km/h, accelerate between bends, brake for the crossing/exit crest. Ridge: approach around 125–132km/h after the entry bend; the sign’s 144km/h is a desired straight-ramp target, not a minimum entrance speed. Small vehicles turn more readily but have less forgiving airborne/landing response. Gully’s conservative time saving is modest; the earlier successfully completed car attempts at higher crest speed saved roughly two seconds, with greater risk.

These are repeatable technical opportunities and failure costs, not a judgment that the routes are fun for a human. No clean legal exit receives a shortcut surcharge. An excursion outside the bounded corridor freezes evidence until the driver returns behind the earned point or recovers locally. Returning to the ordinary road early abandons the branch; unvisited gates then follow ordinary rules.