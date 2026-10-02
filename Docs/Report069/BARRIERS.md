# 0.69 barriers — crest-then-bend places (Part B)

Same design as the 0.68 berms ([Report068/BARRIERS.md](../Report068/BARRIERS.md)):
- natural earth berm on the OUTSIDE of the bend;
- inner toe 0.6 m outside the pavement edge, with an 85° rock inner face 1.8 m above the pavement edge;
- 0.8 m crown, outer face down to the ground, ends tapered over 8 m;
- grounded, with a matching collider.

Stations whose berm would enter another route's corridor are left open. Built by `Tools/Report069/Report069Author.cs` (`Barriers`); log in [partB-barriers-log.txt](partB-barriers-log.txt).

| Report | Scene / place | Berm | Kept open |
|---|---|---|---|
| BUG-005 | Mountain Loop Reverse, main s 446–486, right side (outside of the left bend just past the summit crest), from about (998, 163, 121) to (1018, 161, 151) | 35 stations, 351 tris | Summit Traverse entrance (gold arrow): every station that would enter its corridor was skipped |
| BUG-008 | Mountain Loop Forward, main s 1390–1434, right side, opposite the Climbing Ridge Cut rejoin, from about (1016, 137, -82) to (1043, 144, -33) | 45 stations, 544 tris | main driving width and the rejoin itself |

**Other direction (checked, not needed):**
- **BUG-005 in Forward:** the same road is climbed into the bend, with the crest after it, so it is not crest-then-bend. The Summit Traverse junction is at a different place in that scene.
- **BUG-008 in Reverse:** the main road is straight through s 1395–1450, and the Reverse branch here leaves the main road instead of merging into a bend. Nothing to deflect.

## Verification

Full throttle, muted; harness `Assets/Scripts/Report069Checks.cs`. Raw results: `checks/`.

| Case | Before (0.68 geometry) | After |
|---|---|---|
| BUG-008: Climbing Ridge Cut overshoot from s 268 at 28 m/s, no steering, moto | over the edge, fell 40.1 m | berm contact; stays at rejoin level (0.0 m drop), 21 m/s |
| same, ATV | fell 39.3 m | berm contact; 0.0 m drop, stopped beside the road |
| same from s 276 at 24 m/s, moto | fell 41.5 m | berm contact; 0.0 m drop |
| BUG-008: clean line main s 1360–1460, moto / ATV | complete, no contact | complete, no contact (unchanged) |
| BUG-008: clean line Climbing Ridge Cut s 240–296, moto | complete | complete, no contact (unchanged) |
| BUG-005: main overshoot from s 436 at 32 m/s, no steering, moto | fell 43.3 m | berm contact; 7.3 m below the start (deflected back across the crest) |
| same, ATV | fell 42.7 m | berm contact; 1.9 m |
| BUG-005: overshoot from s 442 at 30 m/s, moto | fell 23.7 m | berm contact; 2.0 m, stopped beside the bend |
| same, ATV | 2.5 m, ended off the pavement | berm contact; ran along the berm and dropped 15.6 m past its end (s 486) |
| BUG-005: clean line main s 400–520, moto | complete, but rolled over at the crest (minUp −0.46) | complete, upright (minUp 0.88); **brushes the berm once** |
| same, ATV | complete (minUp 0.15) | complete (minUp 0.58); brushes the berm once |
| Summit Traverse entrance, clean line s 0–60, moto | complete | complete, no contact (entrance open) |

**Limitations for Dan's review:**
- At BUG-005 an ATV overshooting from s 442 at 30 m/s is carried along the berm and off its far end.
- The full-throttle line brushes the BUG-005 berm, as 0.68 recorded for the Downhill Ridge Cut berm.
- Both berms were left as built: rule 12, Dan judges the feel.
