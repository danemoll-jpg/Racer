"""1.3.0-review3: the one-page ramp table (Docs/Report1300/review3/ramps.md) from the entrance and jump check logs (Report106PlayerCheck.Round1300r3.cs).
usage: python review3_table.py <check dir> <out.md>   (the check dir holds r3-before-all-car, r3-before-all-moto, r3-final-car, r3-final-moto,
r3-final-hopper, r3-final-jumps2, each with its p106-check.txt)"""
import re, sys, pathlib
src = pathlib.Path(sys.argv[1]); out = pathlib.Path(sys.argv[2])
def runs(name):
    """item -> {(speed, line): (loss %, pass)}"""
    res = {}; item = None
    for line in (src / name / 'p106-check.txt').read_text(encoding='utf-8', errors='replace').splitlines():
        m = re.match(r'  (\d+) (\w+) toe ', line)
        if m: item = (m.group(1), m.group(2)); res.setdefault(item, {}); continue
        m = re.match(r'    (\d+) m/s (centre|[+-]\d m|15 deg off-line): .*loss beyond the climb (\d+)%.*-> (pass|FAIL)', line)
        if m and item: res[item][(int(m.group(1)), m.group(2))] = (int(m.group(3)), m.group(4) == 'pass')
    return res
def worst(r, speed):
    xs = [v for (s, l), v in r.items() if s == speed and l != '15 deg off-line']
    if not xs: return None
    return max(x[0] for x in xs), all(x[1] for x in xs)
def cell(b, a):
    def f(x): return '–' if x is None else f"{x[0]}%{'' if x[1] else ' ✗'}"
    return f"{f(b)} → {f(a)}"
before_car, before_moto, car, moto = runs('r3-before-all-car'), runs('r3-before-all-moto'), runs('r3-final-car'), runs('r3-final-moto')
hop = {}  # the Clay Hopper's one run (quick: no probe lines, read from its summary)
for line in (src / 'r3-final-hopper' / 'p106-check.txt').read_text(encoding='utf-8', errors='replace').splitlines():
    m = re.match(r'  (\d+) (\w+) \w+ summary: 18 m/s centre (\d+)%( FAIL)?', line)
    if m: hop[(m.group(1), m.group(2))] = {(18, 'centre'): (int(m.group(3)), not m.group(4))}
jumps = {}
for line in (src / 'r3-final-jumps2' / 'p106-check.txt').read_text(encoding='utf-8', errors='replace').splitlines():
    m = re.match(r'  (\d+) (\w+) at ([\d.]+) m/s \((clears it|20% slower)\): .*?, ([^,]*?(?:landing|table|lip)[^,]*), (WIPED OUT|upright).*-> (not trapped|TRAPPED)', line)
    if m: jumps.setdefault(m.group(1), []).append((m.group(4), float(m.group(3)), m.group(5), m.group(6), m.group(7)))
names = {'1': 'Huge table', '2': 'Huge table', '3': 'Huge table', '4': 'House jump', '5': 'House jump', '6': 'Barn (through)', '7': 'Gap jump', '8': 'Gap jump',
         '9': 'Kicker', '10': 'Kicker', '11': 'Kicker', '16': 'Flip ramp', '17': 'Turnaround', '18': 'Flip ramp'}
rows = []
for item in sorted(car, key=lambda k: int(k[0])):
    i = item[0]; bc, bm, ac, am = before_car.get(item, {}), before_moto.get(item, {}), car[item], moto.get(item, {})
    cols = []
    for s in (10, 18, 26):
        cols.append(f"car {cell(worst(bc, s), worst(ac, s))}<br>moto {cell(worst(bm, s), worst(am, s))}")
    off = lambda r: r.get((18, '15 deg off-line'))
    cols.append(f"car {cell(off(bc), off(ac))}<br>moto {cell(off(bm), off(am))}")
    h = hop.get(item, {}).get((18, 'centre')); cols.append('–' if h is None else f"{h[0]}%{'' if h[1] else ' ✗'}")
    j = jumps.get(i)
    if j: jp = '<br>'.join(f"{k}: {v:.0f} m/s, {w}, {u}, {t}" for k, v, w, u, t in j)
    else: jp = 'flip: launched, turned, upright (1.3 check)' if item[1] == 'flip' else 'driven round (1.3 check)' if item[1] == 'loop' else '–'
    rows.append(f"| {i} | {names.get(i, item[1])} | " + ' | '.join(cols) + f" | {jp} |")
head = """# 1.3.0-review3: every Free Roam ramp checked by the pad

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
"""
out.parent.mkdir(parents=True, exist_ok=True)
out.write_text(head + '\n'.join(rows) + '\n', encoding='utf-8')
print(len(rows), 'rows written to', out)
