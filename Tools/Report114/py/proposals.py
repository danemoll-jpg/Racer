# 0.114 Part B: the two combined-course proposals (Street Loop + Mountain), joined from roads that exist in FreeRoamWorld only
# (Docs/Report114/Probe/routes-*.txt, dumped by Tools/Report114/Report114Survey.cs). Writes Assets/Resources/TrackProposals/TrackProposals.json
# (the centreline every 2 m, numbered corners, the start, each shortcut's line and entrance) and Docs/Report114/proposals.txt (lengths, shares).
# Run from the project root: python Tools/Report114/py/proposals.py
import json, math, os

PROBE = 'Docs/Report114/Probe'
STREET, LAKE, EXPL, SUMMIT = 'Phase 3 - Race Systems', 'CR081 mountain exploration/Lake shore connection', \
    'CR081 mountain exploration/Mountain return trails / exploration only', 'CR094 summit launch/Summit approach and safe return'

def load(f):
    roads, cur = {}, None
    for l in open(os.path.join(PROBE, f), encoding='utf-8'):
        if l.startswith('# road'): cur = l.split(' forestTrail')[0][7:]; roads[cur] = []
        elif l.startswith('# branch'): cur = 'B:' + l[9:].split(' |')[0]; roads[cur] = []
        elif l.startswith('#'): cur = None
        elif cur is not None and l.strip():
            p = l.split(); roads[cur].append((float(p[1]), float(p[2]), float(p[3])))
    return roads

def stations(p):
    s = [0.0]
    for i in range(1, len(p)): s.append(s[-1] + math.hypot(p[i][0] - p[i - 1][0], p[i][2] - p[i - 1][2]))
    return s

def at(p, s, d):
    d = max(0.0, min(s[-1], d)); i = 0
    lo, hi = 0, len(s) - 1
    while lo + 1 < hi:
        m = (lo + hi) // 2
        if s[m] <= d: lo = m
        else: hi = m
    t = 0 if s[hi] == s[lo] else (d - s[lo]) / (s[hi] - s[lo])
    a, b = p[lo], p[hi]
    return tuple(a[k] + (b[k] - a[k]) * t for k in range(3))

def seg(p, a, b, closed, step=2.0):
    """points of road p from station a to b (b < a: driven backwards); on a closed road a run may wrap past the end"""
    s = stations(p); L = s[-1] + (math.hypot(p[0][0] - p[-1][0], p[0][2] - p[-1][2]) if closed else 0)
    if closed: q = p + [p[0]]; s = stations(q)
    else: q = p
    out = []; n = max(1, int(abs(b - a) / step))
    for k in range(n + 1):
        d = a + (b - a) * k / n
        if closed: d %= L
        out.append(at(q, s, d))
    return out, L

def nearest(p, x, z):
    s = stations(p); best = min(range(len(p)), key=lambda i: (p[i][0] - x) ** 2 + (p[i][2] - z) ** 2); return s[best]

fr = load('routes-FreeRoamWorld.txt'); fwd = load('routes-StreetLoopGreybox.txt'); rev = load('routes-StreetLoopReverse.txt')
street, lake, expl, summit = fr[STREET], fr[LAKE], fr[EXPL], fr[SUMMIT]
# the junctions (measured: Docs/Report114 notes), as stations on each road
J_street = nearest(street, 474.5, -27.0)                      # the Lake shore connection leaves the Street Loop (s ~ 630)
L_south_end, L_north_end = 0.0, stations(lake)[-1]             # its two ends at J
L_to_expl0 = nearest(lake, 688.5, -96.0)                       # lake south shore meets the exploration loop's s0 (690, -95)
L_at_697 = nearest(lake, 697.0, 44.0)                          # lake north-east shore meets the exploration loop at (697, 44)
E_summit = nearest(expl, 994.0, 111.0)                         # the summit approach leaves the exploration loop
E_landing = nearest(expl, 832.0, 109.0)                        # the summit approach rejoins after the jump's landing
E_lake = nearest(expl, 697.0, 44.0)                            # back down at the lake
S_rejoin = nearest(summit, 832.0, 109.0)
gate_s = nearest(street, 319.67, 512.81)                       # the Street Loop START FINISH
S_len = stations(street)[-1] + math.hypot(street[0][0] - street[-1][0], street[0][2] - street[-1][2])

def join(parts):
    pts = []; spans = []
    for name, (q, _) in parts:
        a = len(pts); pts += q if not pts else q[1:]; spans.append((name, a, len(pts) - 1))
    return pts, spans

def length(pts): return stations(pts)[-1]

A_parts = [('street', seg(street, gate_s, J_street, True)),
           ('lake', seg(lake, L_south_end, L_to_expl0, True)),
           ('mountain', seg(expl, 0, E_summit, True)),
           ('mountain', seg(summit, 0, S_rejoin, False)),
           ('mountain', seg(expl, E_landing, E_lake, True)),
           ('lake', seg(lake, L_at_697, L_north_end, True)),
           ('street', seg(street, J_street, gate_s + S_len, True))]
B_parts = [('street', seg(street, gate_s, J_street - S_len, True)),   # Street Loop Reverse: the same road driven backwards
           ('lake', seg(lake, L_north_end, L_at_697, True)),
           ('mountain', seg(expl, E_lake, 0, True)),
           ('lake', seg(lake, L_to_expl0, L_south_end, True)),
           ('street', seg(street, J_street, gate_s, True))]

def corners(pts):
    s = stations(pts); h = []
    for i in range(len(pts)):
        a, b = pts[max(0, i - 1)], pts[min(len(pts) - 1, i + 1)]; h.append(math.atan2(b[0] - a[0], b[2] - a[2]))
    def turn(i, w=10):  # heading change over +-20 m
        a, b = h[max(0, i - w)], h[min(len(h) - 1, i + w)]; d = (b - a + math.pi) % (2 * math.pi) - math.pi; return math.degrees(d)
    out = []; i = 0
    while i < len(pts):
        t = abs(turn(i))
        if t >= 40:
            j = i
            while j + 1 < len(pts) and abs(turn(j + 1)) >= 40: j += 1
            k = max(range(i, j + 1), key=lambda m: abs(turn(m)))
            if not out or s[k] - s[out[-1][0]] > 60: out.append((k, turn(k)))
            i = j + 1
        else: i += 1
    return out

def branch_entry(b): return b[0]

def proposal(pid, title, color, parts, shortcuts, notes):
    pts, spans = join(parts)
    total = length(pts)
    share = {}
    for name, a, b in spans: share[name] = share.get(name, 0) + length(pts[a:b + 1])
    cs = corners(pts)
    f = (pts[3][0] - pts[0][0], pts[3][2] - pts[0][2]); fl = math.hypot(*f)
    return {'id': pid, 'title': title, 'color': color, 'length': round(total, 1), 'share': {k: round(v, 1) for k, v in share.items()},
            'pts': [round(v, 2) for p in pts for v in p], 'start': [round(pts[0][0], 2), round(pts[0][1], 2), round(pts[0][2], 2), round(f[0] / fl, 3), round(f[1] / fl, 3)],
            'corners': [{'n': n + 1, 'i': k, 'turn': round(t, 0)} for n, (k, t) in enumerate(cs)],
            'shortcuts': [{'title': t, 'pts': [round(v, 2) for p in q for v in p]} for t, q in shortcuts], 'notes': notes}

north_arc, _ = seg(expl, E_summit, E_landing, True)
A = proposal('A', 'Proposal A: Street Loop + Summit Jump', [1.0, .45, .0], A_parts,
             [('Creek Leap', fwd['B:Creek Leap']), ('Fox Gully', fwd['B:Fox Gully']), ('Pine Ridge', fwd['B:Pine Ridge']), ('Existing Southwest Cut', fwd['B:Existing Southwest Cut']),
              ('North Arc (skips the summit)', north_arc)],
             'Street Loop Forward; at the Lake shore junction round the lake south shore onto the Mountain return trail, the ridge, the summit approach and the summit jump, down to the lake and back to the Street Loop')
B = proposal('B', 'Proposal B: Street Loop Reverse + Ridge Loop', [.0, .9, 1.0], B_parts,
             [('Granite Creek Cut', rev['B:Granite Creek Cut']), ('Laurel Switchbacks', rev['B:Laurel Switchbacks'])],
             'Street Loop Reverse; at the Lake shore junction round the lake north shore, the Mountain return trail the other way (north arc, east slope, ridge), the lake south shore back to the Street Loop')
os.makedirs('Assets/Resources/TrackProposals', exist_ok=True)
json.dump({'version': '0.114', 'source': 'Tools/Report114/py/proposals.py from Docs/Report114/Probe/routes-*.txt', 'proposals': [A, B]},
          open('Assets/Resources/TrackProposals/TrackProposals.json', 'w', encoding='utf-8'), separators=(',', ':'))
with open('Docs/Report114/proposals.txt', 'w', encoding='utf-8') as o:
    for P in (A, B):
        o.write(f"{P['title']}: {P['length']:.0f} m; " + ', '.join(f"{k} {v:.0f} m ({100 * v / P['length']:.0f}%)" for k, v in P['share'].items()) + f"; {len(P['corners'])} numbered corners\n")
        for sc in P['shortcuts']:
            q = sc['pts']; o.write(f"  shortcut {sc['title']}: {length([(q[i], q[i + 1], q[i + 2]) for i in range(0, len(q), 3)]):.0f} m, entrance ({q[0]:.0f}, {q[2]:.0f})\n")
        for c in P['corners']: i = c['i']; o.write(f"  corner {c['n']}: ({P['pts'][3 * i]:.0f}, {P['pts'][3 * i + 2]:.0f}) turn {c['turn']:.0f} deg\n")
print(open('Docs/Report114/proposals.txt').read())
print('junction stations: street J', round(J_street), 'gate', round(gate_s), 'lake->expl', round(L_to_expl0), 'lake@697', round(L_at_697), 'expl summit', round(E_summit), 'landing', round(E_landing), 'lake', round(E_lake), 'summit rejoin', round(S_rejoin))
