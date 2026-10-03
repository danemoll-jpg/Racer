# 0.70 barrier — Mountain Loop Reverse summit crest (BUG-005 / BUG-006)

Replaces the 0.69 BUG-005 earth berm at this place (the 0.69 table is kept in [Report069/BARRIERS.md](../Report069/BARRIERS.md) for history). The 0.69 BUG-008 berm (Forward, Climbing Ridge Cut rejoin) and the 0.68 berms are unchanged.

## 1. Reproduction first (Dan's real approach)

Full throttle up the Reverse main climb, steering at the route (a rider's normal line), muted. Harness `Assets/Scripts/Report070Checks.cs` case `climb`. Raw CSVs per step: `checks/climb-*`.

| Run (0.69 geometry) | Takes off | Highest above the road | Crosses the outside edge | Ends |
|---|---|---|---|---|
| Moto from s 140 at 10 m/s | crest, 33.2 m/s | 7.9 m at s 447.5 | s 457.6, 6.4 m above the road | 87 m off the right side, 40 m below |
| ATV from s 140 at 10 m/s | 32.5 m/s | 7.5 m at s 441 | s 448.4, 6.9 m | 50–62 m off, in the gore / below |
| Moto from rest at s 200 | 34.5 m/s | 9.5 m (CoM) crossing the edge | s 452.9, 9.5 m | 86 m off |
| ATV from rest at s 200 | 31.9 m/s | 7.5 m | s 448.4, 6.8 m | 50 m off |
| Moto from s 300 at 25 m/s | 32.8 m/s | 8.6 m | s 456.0, 7.4 m | 81 m off |
| ATV from s 300 at 25 m/s | 31.3 m/s | 7.0 m | s 454.2, 5.2 m | 66 m off, 50 m below |

Dan's BUG-006 capture (s 449, 32.0 m/s, about 9 m above the road) is reproduced: every full-throttle run leaves the crest at 31–35 m/s, crosses the outside of the bend 4–9.5 m above the road between main s 448 and 462, and lands 50–88 m away. None touched the 0.69 berm (1.8 m).

## 2. What was built

The outside of the bend is the road-level plateau (the gore) between the main road and the Summit Traverse entrance, ending in a cliff to the east. The flights cross that gore.

- **One natural rock outcrop filling the gore** (`Ground_Report070 barrier summit crest outcrop (0.70 BUG-005/006)`, `Tools/Report070/Report070Author.cs` → `Crest`):
  - **Main-road face:** 85° rock cliff 7 m beyond the main road's right pavement edge, main s 454.5–494. It stands just behind the OPTIONAL SHORTCUT / SUMMIT TRAVERSE sign, so the sign stays in front of it and readable. The 7 m in front of it is the existing road-level plateau (runoff).
  - **Summit Traverse face:** 85° cliff 1 m beyond the Summit Traverse's left pavement edge, s 15.5–42.
  - The two faces meet in one corner behind the sign. Each face's solid is clipped where it would enter the other road's pavement or the runoff in front of the other face.
  - Top 12 m above the pavement edge (deterministic rock faceting varies it by up to ±0.8 m), 3 m crown, back face 1:0.7 down to the ground. Far ends taper over 6 m; the corner end is capped. Collidable and grounded.
- **Size from the measurements:** highest crossing 9.5 m (body centre) plus about 1 m of vehicle above that plus margin gives 12 m. The face sits where the lowest flights are still 3.8–8.8 m up.
- **0.69 berm removed** here (absorbed into the outcrop).
- **Kept:** the driving width, the Summit Traverse entrance (its pavement and the run onto it), the sign (not moved), and the crest itself (unchanged). The Summit Traverse has no flight window. Log: [partC-outcrop-log.txt](partC-outcrop-log.txt).

## 3. Verification (after)

| Case | Result |
|---|---|
| Full throttle, moto, from s 140 / rest at s 200 / s 300 / s 380 | All 4 stopped by the outcrop and stay on the summit plateau. Max lateral 14.2–15.1 m from the centre (the face is 14 m out), end height 158.8–163.3 (road level). Two wipe out against the cliff (minUp −1.0); the one from rest rides on and completes. |
| Full throttle, ATV, same 4 starts | All 4 stopped on the plateau (max lateral 13.9–18.5 m, end 158.9–161.5). Two land in the pre-existing dip beside the sign, 3–4 m below the road, still on the summit (the nearest-point reset restores). |
| Before, the same 8 runs | All 8 left the track: 50–88 m out, 24–50 m below. |
| Clean line main s 400–520, moto / ATV | complete, upright (minUp 0.71 / 0.15), one brush each with the outcrop (0.69: one brush with the berm). |
| Summit Traverse entrance s 0–60, moto / ATV | complete, no contact. |
| 0.69 no-steering overshoots | s 442 at 30 m/s: moto and ATV kept beside the bend (0.69: ATV fell 15.6 m off the berm end). s 436 at 32 m/s: moto kept; ATV deflected back west and ends on the main road's lower section (s ≈ 960) at road height, 39 m from where it left. |

**Limitations for Dan's review:**
- A rider who flies into the cliff wipes out and is reset at the nearest point (TODO: acceptable outcome).
- The full-throttle clean line still lands in the runoff and brushes the cliff once.
- There is a pre-existing 3–5 m dip in the gore right beside the sign. Two ATV runs ended in it, contained.
