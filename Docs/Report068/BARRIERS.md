# 0.68 Part E — barriers after Mountain jumps

Dan's request: guardrails or natural formations where a jump is followed closely by a turn or drop-off, to keep riders from flying off the track. This applies to the Mountain tracks only.

## How jumps were found

- `Tools/Report068/Report068Jumps.cs` lists every pavement gap of 2 m or more on every Mountain route, in both directions (lip → landing), plus every authored jump activity near a route.
- For each jump it then measures, over the next 100–120 m:
  - the route's heading change;
  - the outside of the turn;
  - the edge drop 2 m outside each side.
- Raw output: `jumps-MountainLoop.txt` and `jumps-MountainLoopReverse.txt`.

## Mountain jumps

| # | Scene | Jump (route) | Lip → landing (station, Unity XYZ) | After the landing | Built? |
|---|---|---|---|---|---|
| 1 | Forward | Homeward Summit Flight (main) | s 2205 (980.4, 177.9, 147.3) → deck s 2316 (873.7, 177.8, 117.0) | Full-throttle riders land on the steep descent at s 2416–2473 (measured). A 68° **left** bend follows at s 2483–2563, with a drop to lake level outside it. | **Yes**: berm on the outside (right), main s 2476–2545 |
| 2 | Forward | High Ridge Drop activity (Summit Traverse entry) | (1011.0, 163.7, 93.7), Summit Traverse s 0–30 | A 71° **left** turn within 30 m; both edges drop 1.3–1.8 m | **Built, then removed**: it sits at the shortcut entrance, touched the full-throttle line and stopped an ATV in verification. Suggestion only |
| 3 | Reverse | South Face Summit Flight (main) | s 982 (990.0, 177.4, 65.4) → s 1134 (990.0, 158.8, -85.3) | A long descent, then a 110° **right** bend at s 1281–1346 | **Yes**: berm on the outside (left), main s 1279–1350 |
| 4 | Reverse | Downhill Ridge Cut jumps (branch) | s 94 (740.3, 92.8, -146.2) → s 112; s 117 → s 129 (715.2, 84.9, -121.8) | The left edge drops 7.1–7.5 m right after landing; an 86° **right** turn at s 159–189 | **Yes**: berm on the outside (left), branch s 112–192 |
| 5 | Forward | Eastbound Gully Flight (main) | s 459 (1069.7, 131.4, -275.0) → s 560 (1170.3, 123.8, -275.0) | Straight for 185 m; the first bend is at s 745 | No (straight run-out). Suggestion only |
| 6 | Reverse | Westbound Gully Flight (main) | s 2462 (1005.4, 131.1, -300.0) → s 2563 (904.8, 123.9, -300.0) | Straight for 200 m | No (straight run-out) |
| 7 | Reverse | Summit Traverse jump / Fern Creek Leap | Summit Traverse s 398 (866.8, 112.0, -85.1) → s 414 (878.1, 114.0, -96.1) | A 29° left bend while the shortcut **merges into the main road** (main s 1633–1673 runs alongside) | No. The outside of that bend is the main road; a barrier would block it (5A.4) |
| 8 | Reverse | High Ridge Drop activity (Summit Traverse s 30) | (1011.1, 161.7, 93.7) | Gentle: under 15° within 50 m | No. Suggestion only |
| 9 | Reverse | Main s 34 → 100 "gap" | (725.9, 79.0, -27.0) → (726.9, 80.7, 38.4) | Not a jump: the start area is paved with a different surface | No |

## Barrier design (three built sites)

- **Form:** a natural earth berm, continuous along the outside edge. The steep inner face uses the rock colour; the crown and outer slope use the earth colour.
- **Cross-section:**
  - inner toe 0.6 m outside the pavement edge (1.4 m on the narrow Downhill Ridge Cut), so it is never in the driving width;
  - near-vertical (85°) rock inner face up to 1.8 m above the pavement edge (2.4 m at the Homeward bend, where riders arrive off the steep descent); the first 65°/1.5 m profile let an ATV climb and vault it;
  - 0.8 m crown;
  - outer face back down to the ground.
- **Deflection:** one continuous smooth face deflects a vehicle along the road. There are no individual rocks to catch on or launch from, and the height tapers to zero over 8 m at both ends.
- **Ground contact:** grounded on the existing slope, with a collider matching the visible mesh (`Ground_Report068 barrier <site>`).
- **Where it stops:**
  - no station enters another route's corridor (shortcut entrances and exits, other parts of the lap, lower routes);
  - no station goes into a flight corridor, because each berm starts after its landing;
  - stations where a natural bank is already more than 1.2 m high get no berm.
  - The skipped stations are logged in `partE-barriers-log.txt`.
- **Dependent objects:**
  - no sign or post lies in any footprint;
  - two tree trunks at Downhill Ridge Cut now stand in the berm's outer slope. They read as trees on the bank.

## Other perilous places noticed (suggestions for Dan, not built)

- Forward, Eastbound Gully Flight: the 59° bend at main s 745 is 185 m after the landing. A barrier on its outside (left) is an option if riders overshoot it.
- Forward, the west edge of the plateau beside the Homeward bend (x ≈ 673–677, z 50–110): it drops about 14 m to lake level. It is now behind the new berm.
- Reverse, High Ridge Drop at Summit Traverse s 30–120: a gradual 55° left turn with 1.7 m edge drops on both sides.
- Forward, High Ridge Drop (Summit Traverse s 8–60): a 71° left turn right after the kicker at the shortcut entrance. A barrier there must sit further out than the 0.6 m tried, or be a guardrail beyond the shoulder.

## Verification

See `VALIDATION.md` Part E for the full-throttle clean-line and no-steering overshoot results, moto and ATV, at each site.
