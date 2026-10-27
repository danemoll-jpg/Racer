# 1.3.0-review3: every Free Roam ramp checked by the pad

Entrance loss = speed lost to the entrance beyond the climb itself, between 5 m before the toe and the ramp's face (pass: no more than about 10%, no hit on
a vertical face, no wreck or reset; ✗ = failed). Worst of the centre line and 3 m to each side, before (1.3.0-review2) → after (1.3.0-review3); stock car
(original) and motorcycle (moto); the Clay Hopper once at 18 m/s on the centre line (after). Jumps: the stock car at the speed that lands it on the landing by
the flight sums, and 20% slower (gap, house and table jumps): where it came down and whether it could drive away.

**Fix (all ramps, shared code, RoamWorld.Extrude and VehicleSurfaceContacts):** the toe now follows the ground across its width (it was laid at one height,
so on ground falling away sideways it stood up to 0.4-0.6 m proud: ramps 2, 3, 4, 5, 8, 11) and starts 3 m earlier under the ground; the surface is in
strips so its panels stay flat where it twists; and the ramps' faces are now known to the vehicles' contact correction (built after the vehicles, they
were never in it, so a motorcycle caught on the seams of the face and stopped: the "stop dead" on 4 and 8, and on 9, 10, 11). Nothing past the lip changed.

| Item | Kind | 10 m/s | 18 m/s | 26 m/s | 15° off-line, 18 m/s | Clay Hopper | Jump |
|---|---|---|---|---|---|---|---|
| 1 | Huge table | car 0% → 8%<br>moto 9% → 0% | car 3% → 0%<br>moto 4% → 0% | car 10% → 0%<br>moto 0% → 0% | car 2% → 0%<br>moto 0% → 0% | 0% | clears it: 22 m/s, on the landing slope or past it (42 m from the lip), upright, not trapped |
| 2 | Huge table | car 0% → 0%<br>moto 22% ✗ → 0% | car 1% → 5%<br>moto 100% ✗ → 0% | car 3% → 0%<br>moto 100% ✗ → 0% | car 0% → 0%<br>moto 0% → 0% | 2% | clears it: 22 m/s, on the landing slope or past it (33 m from the lip), upright, not trapped |
| 3 | Huge table | car 4% → 2%<br>moto 100% ✗ → 1% | car 0% → 1%<br>moto 10% ✗ → 0% | car 1% → 0%<br>moto 34% ✗ → 1% | car 2% → 0%<br>moto 0% → 1% | 0% | clears it: 22 m/s, on the landing slope or past it (32 m from the lip), upright, not trapped |
| 4 | House jump | car 11% ✗ → 6%<br>moto 100% ✗ → 6% | car 4% → 3%<br>moto 100% ✗ → 2% | car 2% → 4%<br>moto 100% ✗ → 2% | car 1% → 1%<br>moto 9% → 2% | 4% | clears it: 24 m/s, on the landing (45 m from the lip), upright, not trapped<br>20% slower: 20 m/s, came down 10 m from the lip (the landing starts at 36 m), upright, not trapped |
| 5 | House jump | car 3% → 0%<br>moto 100% ✗ → 0% | car 0% → 2%<br>moto 100% ✗ → 1% | car 3% → 3%<br>moto 5% ✗ → 7% | car 0% ✗ → 1% ✗<br>moto 0% → 3% | 0% | clears it: 23 m/s, on the landing (46 m from the lip), upright, not trapped<br>20% slower: 19 m/s, came down 11 m from the lip (the landing starts at 34 m), upright, not trapped |
| 6 | Barn (through) | car 4% → 3%<br>moto 2% → 0% | car 0% → 0%<br>moto 9% → 1% | car 4% → 0%<br>moto 0% → 0% | car 0% → 0%<br>moto 1% → 1% | 2% | clears it: 32 m/s, 20 m from the lip, upright, not trapped |
| 7 | Gap jump | car 6% → 17% ✗<br>moto 2% → 0% | car 0% → 2%<br>moto 13% ✗ → 5% | car 2% → 2%<br>moto 24% ✗ → 1% | car 3% → 0%<br>moto 10% → 0% | 4% | clears it: 22 m/s, on the landing (31 m from the lip), upright, not trapped<br>20% slower: 18 m/s, came down 22 m from the lip (the landing starts at 30 m), upright, not trapped |
| 8 | Gap jump | car 9% → 0%<br>moto 4% → 0% | car 5% → 0%<br>moto 1% → 3% | car 0% → 2%<br>moto 40% ✗ → 3% | car 3% → 0%<br>moto 4% → 0% | 0% | clears it: 24 m/s, on the landing (43 m from the lip), upright, not trapped<br>20% slower: 19 m/s, came down 24 m from the lip (the landing starts at 30 m), upright, not trapped |
| 9 | Kicker | car 0% → 0%<br>moto 16% ✗ → 0% | car 0% → 0%<br>moto 35% ✗ → 0% | car 2% → 3%<br>moto 58% ✗ → 5% | car 0% → 0%<br>moto 0% → 0% | 0% | clears it: 18 m/s, 18 m from the lip, upright, not trapped |
| 10 | Kicker | car 1% → 2%<br>moto 21% ✗ → 2% | car 2% → 0%<br>moto 39% ✗ → 7% | car 1% → 3%<br>moto 56% ✗ → 1% | car 0% → 0%<br>moto 0% → 0% | 0% | clears it: 18 m/s, 22 m from the lip, upright, not trapped |
| 11 | Kicker | car 100% ✗ → 0%<br>moto 100% ✗ → 10% ✗ | car 100% ✗ → 4%<br>moto 100% ✗ → 0% | car 100% ✗ → 1%<br>moto 100% ✗ → 1% | car 100% ✗ → 0%<br>moto 100% ✗ → 1% | 0% | clears it: 18 m/s, 23 m from the lip, upright, not trapped |
| 16 | Flip ramp | car 0% → 9%<br>moto 1% → 2% | car 7% → 5%<br>moto 100% ✗ → 3% ✗ | car 18% ✗ → 70% ✗<br>moto 100% ✗ → 2% | car 7% → 0%<br>moto 0% → 0% | 0% | flip: launched, turned, upright (1.3 check) |
| 17 | Turnaround | car 4% → 1%<br>moto 2% → 3% | car 0% → 0%<br>moto 0% → 2% | car 0% → 0%<br>moto 1% → 1% | car 0% → 0%<br>moto 0% → 0% | 0% | driven round (1.3 check) |
| 18 | Flip ramp | car 0% → 11% ✗<br>moto 0% → 1% | car 10% → 1%<br>moto 12% ✗ → 0% | car 25% ✗ → 0%<br>moto 36% ✗ → 0% | car 8% → 0%<br>moto 0% → 2% | 0% | flip: launched, turned, upright (1.3 check) |
