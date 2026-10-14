# 0.116 Part A: Leviathan's turn indicators. At each decision point (where the line leaves the road it is on, or leaves / rejoins another
# pass of its own; chosen from decisions.py's list): a big arrow board on the right of the road before the turn, facing the traffic, and
# chevron boards on the outside of the turn (2, or 3-4 for a sharp turn / hairpin). Offsets from the line: the road's half width + 2 m.
# Writes "signs" into Assets/Resources/Leviathan/Leviathan{Forward,Reverse}.json (the rest of each file unchanged).
import json, math, sys, os
sys.path.insert(0, os.path.dirname(__file__))
from decisions import load, at, heading, turn
DECISIONS = {
    'LeviathanForward': [
        (640, -1, 'J: off the street onto the stem'),
        (905, +1, 'leaving the lake shore for CR110 (the mountain)'),
        (963, -1, 'Climbing Ridge Cut'),
        (1268, -1, 'onto CR117'),
        (2503, +1, 'off the mountain onto the lake east side'),
        (2822, -1, 'J: off the stem onto the street (hairpin)'),
    ],
    'LeviathanReverse': [
        (3998, +1, 'J: off the street onto the stem (hairpin)'),
        (4286, +1, 'onto the shore strip'),
        (4404, -1, 'leaving the lake for CR117 (the mountain)'),
        (5820, -1, 'Downhill Ridge Cut'),
        (6043, -1, 'off the mountain: the start stretch'),
        (6081, -1, 'off the mountain: onto the strip'),
        (6195, -1, 'back onto the north shore'),
        (6512, +1, 'J: off the stem onto the street'),
    ],
}
def half_width(R, s):
    def inside(spans): return any(spans[i] <= s <= spans[i + 1] for i in range(0, len(spans or []) - 1, 2))
    if inside(R.get('narrow')): return 3.5
    if inside(R.get('mountain')): return 5.0
    lake = [p for p in R['parts'] if 'street' not in p['name']]
    if any(p['from'] - 30 <= s <= p['to'] + 30 for p in lake): return 4.5
    return 6.5
for name, decisions in DECISIONS.items():
    path = 'Assets/Resources/Leviathan/%s.json' % name
    R, P, S = load(name); signs = []; prev = -1e9
    for s, d, label in decisions:
        a = turn(P, S, s); W = half_width(R, s) + 2
        # the arrow: before the turn, on the right, facing the traffic coming to it
        D = max(20, min(50, s - prev - 10)); sa = s - D
        p = at(P, S, sa); h = heading(P, S, sa - 10, sa + 2); r = math.radians(h)
        rx, rz = math.cos(r), -math.sin(r)  # the right of travel (heading h: x = sin, z = cos)
        # alt: other spots tried in turn in the race when there is no level ground near the road's height at the first (a bank, the lake):
        # the other side of the road, 15 m nearer or further from the turn, 1 m closer to the road (x, z, road height; triples)
        alt = []
        for dd in (D, D - 15, D + 15):
            pa = at(P, S, s - dd); ha = math.radians(heading(P, S, s - dd - 10, s - dd + 2)); ax, az = math.cos(ha), -math.sin(ha)
            for side, off in ((1, W), (-1, W), (1, W - 1), (-1, W - 1)):
                if dd == D and side == 1 and off == W: continue
                alt += [round(pa[0] + ax * off * side, 2), round(pa[2] + az * off * side, 2), round(pa[1], 2)]
        signs.append({'kind': 'arrow', 'dir': d, 'pos': [round(p[0] + rx * W, 2), round(p[2] + rz * W, 2), round(p[1], 2)], 'alt': alt, 'yaw': round(h, 1), 'at': label})
        # the chevrons: on the outside of the turn, around and beyond its apex, all facing the traffic coming into the turn
        h0 = heading(P, S, s - 30, s - 10); n = 4 if abs(a) > 110 else 3 if abs(a) > 55 else 2
        for k in range(n):
            sc = s - 6 + k * (6 if abs(a) > 110 else 9); q = at(P, S, sc); hc = heading(P, S, sc - 3, sc + 3); rc = math.radians(hc)
            ox, oz = math.cos(rc) * -d, -math.sin(rc) * -d  # outside: the right for a left turn, the left for a right turn
            signs.append({'kind': 'chevron', 'dir': d, 'pos': [round(q[0] + ox * W, 2), round(q[2] + oz * W, 2), round(q[1], 2)], 'yaw': round(h0, 1), 'at': label})
        print('%s %6.0f %-48s turn %+4.0f, arrow %2.0f m before, %d chevrons, offset %.1f m' % (name, s, label, a, D, n, W))
        prev = s
    # 0.116 Part B: each shortcut's boards: its name before its entry, beside the road on the side it leaves by (facing the traffic), and
    # REJOIN ROAD 25 m before its end, on its right (facing the shortcut's traffic)
    for br in R.get('branches', []) if R.get('reverse') else []:
        bp = [(br['pts'][i], br['pts'][i + 1], br['pts'][i + 2]) for i in range(0, len(br['pts']), 3)]
        e = br['entryRoad']; pe = at(P, S, e - 30); he = heading(P, S, e - 40, e - 25); r = math.radians(he); rx, rz = math.cos(r), -math.sin(r)
        k = min(len(bp) - 1, 15); side = 1 if (bp[k][0] - pe[0]) * rx + (bp[k][2] - pe[2]) * rz > 0 else -1
        W = half_width(R, e) + 2.5
        name = br['title'].upper().replace("'", "'")
        text = (name + '  >') if side > 0 else ('<  ' + name)
        signs.append({'kind': 'board', 'text': text, 'dir': side, 'pos': [round(pe[0] + rx * W * side, 2), round(pe[2] + rz * W * side, 2), round(pe[1], 2)],
                      'alt': [round(pe[0] - rx * W * side, 2), round(pe[2] - rz * W * side, 2), round(pe[1], 2)], 'yaw': round(he, 1), 'at': br['title'] + ' entry'})
        # the rejoin: 25 m before the shortcut's end
        L = [0.0]
        for i in range(1, len(bp)): L.append(L[-1] + math.dist(bp[i], bp[i - 1]))
        j = min(range(len(L)), key=lambda i: abs(L[i] - (L[-1] - 25))); a2, b2 = bp[max(0, j - 2)], bp[min(len(bp) - 1, j + 2)]
        hb = math.degrees(math.atan2(b2[0] - a2[0], b2[2] - a2[2])); rb = math.radians(hb); off = br['halfWidth'] + 2
        signs.append({'kind': 'board', 'text': 'REJOIN  ROAD', 'dir': 1, 'pos': [round(bp[j][0] + math.cos(rb) * off, 2), round(bp[j][2] - math.sin(rb) * off, 2), round(bp[j][1], 2)],
                      'alt': [round(bp[j][0] - math.cos(rb) * off, 2), round(bp[j][2] + math.sin(rb) * off, 2), round(bp[j][1], 2)], 'yaw': round(hb, 1), 'at': br['title'] + ' rejoin'})
        print('  %s: entry board "%s" on the %s, rejoin board 25 m before its end' % (br['title'], text, 'right' if side > 0 else 'left'))
    R['signs'] = signs
    open(path, 'w').write(json.dumps(R, separators=(',', ':')))
