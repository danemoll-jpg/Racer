# 0.117: Leviathan's turn signs after the Kyle's-driveway entrance. The 0.116 boards stay where their turns are unchanged; the J boards (the stem,
# the hairpins) go, and every turn of the new way at J gets its own (both passes); also the turn off CR117 onto the lake shore after the summit
# (Forward, BUG-005) and the turns onto / off Hwy 92 at the Street Loop's corners. Same boards as 0.116 (Tools/Report116/py/signs.py: an arrow
# board on the right before the turn, chevrons on its outside, one-sided, no colliders), with the arrow's distance chosen per turn: BUG-001's
# arrow stood 50 m before the driveway, before a house driveway on the other side; it now stands just before the turn (14 m), where nothing
# else leaves the road, and is seen from 40-60 m. Rocky Way Cut's REJOIN board moves to its new end.
# usage: python Tools/Report117/py/signs.py
import json, math, sys, os
sys.path.insert(0, 'Tools/Report116/py')
from decisions import at, heading, turn
def load(name):
    R = json.load(open('Assets/Resources/Leviathan/%s.json' % name)); P = [tuple(R['pts'][i:i + 3]) for i in range(0, len(R['pts']), 3)]
    S = [0.0]
    for i in range(1, len(P)): S.append(S[-1] + math.dist(P[i], P[i - 1]))
    return R, P, S
def station(P, S, q, lo, hi):
    best = (1e18, 0)
    for i in range(len(P) - 1):
        if S[i + 1] < lo or S[i] > hi: continue
        a, b = P[i], P[i + 1]; vx, vz = b[0] - a[0], b[2] - a[2]; L = vx * vx + vz * vz or 1e-9
        t = max(0, min(1, ((q[0] - a[0]) * vx + (q[1] - a[2]) * vz) / L)); d = (q[0] - a[0] - vx * t) ** 2 + (q[1] - a[2] - vz * t) ** 2
        if d < best[0]: best = (d, S[i] + t * (S[i + 1] - S[i]))
    return best[1]
def boards(R, P, S, s, d, label, D, W):
    out = []; a = turn(P, S, s)
    p = at(P, S, s - D); h = heading(P, S, s - D - 10, s - D + 2); r = math.radians(h); rx, rz = math.cos(r), -math.sin(r)
    alt = []
    for dd in (D, D - 6, D + 8):
        pa = at(P, S, s - dd); ha = math.radians(heading(P, S, s - dd - 10, s - dd + 2)); ax, az = math.cos(ha), -math.sin(ha)
        for side, off in ((1, W), (-1, W), (1, W - 1), (-1, W - 1)):
            if dd == D and side == 1 and off == W: continue
            alt += [round(pa[0] + ax * off * side, 2), round(pa[2] + az * off * side, 2), round(pa[1], 2)]
    out.append({'kind': 'arrow', 'dir': d, 'pos': [round(p[0] + rx * W, 2), round(p[2] + rz * W, 2), round(p[1], 2)], 'alt': alt, 'yaw': round(h, 1), 'at': label})
    h0 = heading(P, S, s - 30, s - 10); n = 4 if abs(a) > 110 else 3 if abs(a) > 55 else 2
    for k in range(n):
        sc = s - 6 + k * (6 if abs(a) > 110 else 9); q = at(P, S, sc); hc = heading(P, S, sc - 3, sc + 3); rc = math.radians(hc)
        ox, oz = math.cos(rc) * -d, -math.sin(rc) * -d
        out.append({'kind': 'chevron', 'dir': d, 'pos': [round(q[0] + ox * W, 2), round(q[2] + oz * W, 2), round(q[1], 2)], 'yaw': round(h0, 1), 'at': label})
    print('  %6.0f %-56s turn %+4.0f, arrow %2.0f m before, %d chevrons, offset %.1f m' % (s, label, a, D, n, W))
    return out
NEW = {
    'LeviathanForward': [
        ((475.5, -21.5), (600, 700), -1, "J: left into Kyle's driveway", 14, 6.5),
        ((526.0, -9.5), (650, 760), +1, 'off the driveway link: right onto the lake path', 16, 5.5),
        ((526.0, -9.5), (2800, 2950), -1, 'off the lake path: left to the driveway', 22, 5.5),
        ((475.5, -21.5), (2850, 2960), -1, "out of Kyle's driveway: left onto the street", 12, 5.5),
        ((712.0, -84.0), (2560, 2680), +1, 'off the mountain road: right onto the lake shore', 40, 6.5),
        ((-628.0, 533.0), (5850, 5990), +1, 'right onto Hwy 92', 50, 6.5),
        ((321.0, 551.0), (6760, 6900), +1, 'right off Hwy 92 to the start', 50, 9.0),
    ],
    'LeviathanReverse': [
        ((475.5, -21.5), (3990, 4060), +1, "J: right into Kyle's driveway", 14, 6.5),
        ((526.0, -8.5), (4040, 4130), -1, 'off the driveway link: left onto the lake path', 16, 5.5),
        ((526.0, -8.5), (6290, 6390), +1, 'off the lake path: right to the driveway', 22, 5.5),
        ((475.5, -21.5), (6330, 6430), +1, "out of Kyle's driveway: right onto the street", 12, 5.5),
        ((321.0, 551.0), (0, 90), -1, 'left onto Hwy 92', 30, 6.5),
        ((-628.0, 533.0), (880, 1000), -1, 'left off Hwy 92', 50, 9.0),
    ],
}
DROP = ('J: off the street onto the stem', 'J: off the stem onto the street', 'Rocky Way Cut rejoin')
for name, decisions in NEW.items():
    R, P, S = load(name); print('==', name)
    labels = {d[3] for d in decisions}
    signs = [x for x in R['signs'] if not any(x['at'].startswith(d) for d in DROP) and x['at'] not in labels]  # (run again: its own boards replaced)
    print('  kept from 0.116: %d boards (%s)' % (len(signs), ', '.join(sorted({x['at'] for x in signs}))))
    for apex, (lo, hi), d, label, D, W in decisions:
        s = station(P, S, apex, lo, hi)
        # the turn's apex: the largest heading change within 15 m of the given point's station
        bend = lambda x: abs((heading(P, S, x, x + 10) - heading(P, S, x - 10, x) + 540) % 360 - 180)
        s = max((s + k for k in range(-15, 16)), key=bend)
        signs += boards(R, P, S, s, d, label, D, W)
    if R['reverse']:
        b = next(x for x in R['branches'] if x['title'] == 'Rocky Way Cut'); bp = [tuple(b['pts'][i:i + 3]) for i in range(0, len(b['pts']), 3)]
        L = [0.0]
        for i in range(1, len(bp)): L.append(L[-1] + math.dist(bp[i], bp[i - 1]))
        j = min(range(len(L)), key=lambda i: abs(L[i] - (L[-1] - 30))); a2, b2 = bp[max(0, j - 2)], bp[min(len(bp) - 1, j + 2)]
        hb = math.degrees(math.atan2(b2[0] - a2[0], b2[2] - a2[2])); rb = math.radians(hb); off = b['halfWidth'] + 2
        signs.append({'kind': 'board', 'text': 'REJOIN  ROAD', 'dir': 1, 'pos': [round(bp[j][0] + math.cos(rb) * off, 2), round(bp[j][2] - math.sin(rb) * off, 2), round(bp[j][1], 2)],
                      'alt': [round(bp[j][0] - math.cos(rb) * off, 2), round(bp[j][2] + math.sin(rb) * off, 2), round(bp[j][1], 2)], 'yaw': round(hb, 1), 'at': 'Rocky Way Cut rejoin'})
        print('  Rocky Way Cut: REJOIN ROAD board 30 m before its new end')
    R['signs'] = signs
    open('Assets/Resources/Leviathan/%s.json' % name, 'w', encoding='utf-8', newline='\n').write(json.dumps(R, separators=(',', ':')))
