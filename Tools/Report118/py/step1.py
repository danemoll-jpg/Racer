# 0.118 step 1 (run from the project root after bank.py): from the 0.117 course files (git 16ddd31c, so it can be run again)
#  Part A : Dan's two closures removed (BUG-001 Forward, BUG-002 Reverse), every other one kept as it is
#  Part A2: Kyle's bank: the ground heights (bank-reshape.json) on both courses, the line's heights on the link set to the new ground
#  Part B : the arrow boards removed; the chevrons kept only where the turn is a corner between two real roads; the turns off a road listed
#           (turns118.json) for the gate planner (Report118Gates)
# usage: python Tools/Report118/py/step1.py <bankgrid-MountainLoop.txt (Report118Bank)>
# writes Tools/Report118/py/stage1-Leviathan{Forward,Reverse}.json, turns118.json and step1.txt
import json, math, subprocess, sys
log = []
def say(*a):
    s = ' '.join(str(x) for x in a); print(s); log.append(s)
def old(name):
    raw = subprocess.run(['git', 'show', '16ddd31c:Assets/Resources/Leviathan/%s.json' % name], capture_output=True, text=True, encoding='utf-8').stdout
    return json.loads(raw)
def pts(R): return [tuple(R['pts'][i:i + 3]) for i in range(0, len(R['pts']), 3)]
def stations(P):
    S = [0.0]
    for i in range(1, len(P)): S.append(S[-1] + math.dist(P[i], P[i - 1]))
    return S
RESHAPE = json.load(open('Tools/Report118/py/bank-reshape.json'))
NEW = {(round(RESHAPE['verts'][i]), round(RESHAPE['verts'][i + 1])): RESHAPE['verts'][i + 2] for i in range(0, len(RESHAPE['verts']), 3)}
GRID = {}
for ln in open(sys.argv[1]):  # the scene's ground on the bank (Report118Bank)
    x, z, g, t, n = ln.split(); GRID[(int(x), int(z))] = float(g)
def ground(x, z):
    H = lambda a, b: NEW.get((a, b), GRID.get((a, b)))
    x0, z0 = math.floor(x), math.floor(z); fx, fz = x - x0, z - z0
    return (H(x0, z0) * (1 - fx) + H(x0 + 1, z0) * fx) * (1 - fz) + (H(x0, z0 + 1) * (1 - fx) + H(x0 + 1, z0 + 1) * fx) * fz
CHANGED = set(NEW)
def on_bank(x, z): return any((math.floor(x) + dx, math.floor(z) + dz) in CHANGED for dx in (0, 1) for dz in (0, 1))
# the scene lines, to tell a road from a trail at a turn
ROADS = ('Phase 3 - Race Systems', 'CR113 highway through route', 'Mountain racing line', 'CR117 mandatory two-flight racing line', 'Climbing Ridge Cut', 'Downhill Ridge Cut')
def scene_lines(sc):
    out = []
    for l in json.load(open('Docs/Report117/Probe/lines-%s.json' % sc)):
        p = l['pts']; out.append((l['path'], [(p[i], p[i + 1], p[i + 2]) for i in range(0, len(p), 3)]))
    return out
def which(lines, q):
    """the scene line under q: a real road when one is within 6 m (the Mountain Loop's exploration trails run on its roads), else the nearest"""
    best = (1e9, None); road = (1e9, None)
    for path, P in lines:
        for a, b in zip(P, P[1:]):
            dx, dz = b[0] - a[0], b[2] - a[2]; L2 = dx * dx + dz * dz or 1e-6
            t = max(0, min(1, ((q[0] - a[0]) * dx + (q[2] - a[2]) * dz) / L2)); y = a[1] + (b[1] - a[1]) * t
            d = math.hypot(q[0] - a[0] - dx * t, q[2] - a[2] - dz * t)
            if abs(y - q[1]) < 4 and d < best[0]: best = (d, path)
            if abs(y - q[1]) < 4 and d < road[0] and any(path.endswith(r) for r in ROADS): road = (d, path)
    return road if road[0] < 6 else best
def at(P, S, s):
    s %= S[-1]
    for i in range(len(P) - 1):
        if S[i] <= s <= S[i + 1]:
            t = (s - S[i]) / max(1e-6, S[i + 1] - S[i]); return tuple(P[i][k] + (P[i + 1][k] - P[i][k]) * t for k in range(3))
    return P[-1]
def project(P, S, q, yaw=None):
    """station of q (x, z) on the line; with yaw: only where the line runs that way (within 70 deg): not the other pass of a twice-driven road"""
    best = (1e9, 0)
    for i in range(len(P) - 1):
        a, b = P[i], P[i + 1]; dx, dz = b[0] - a[0], b[2] - a[2]; L2 = dx * dx + dz * dz or 1e-6
        if yaw is not None:
            h = math.degrees(math.atan2(dx, dz)) % 360
            if abs((h - yaw + 180) % 360 - 180) > 70: continue
        t = max(0, min(1, ((q[0] - a[0]) * dx + (q[1] - a[2]) * dz) / L2)); d = math.hypot(q[0] - a[0] - dx * t, q[1] - a[2] - dz * t)
        if d < best[0]: best = (d, S[i] + (S[i + 1] - S[i]) * t)
    return best[1], best[0]

SPOTS = {'LeviathanForward': (1185.99, 192.54, 169.8), 'LeviathanReverse': (-611.04, -492.14, 155.4)}
TURNS = {}
for name, sc in (('LeviathanForward', 'MountainLoop'), ('LeviathanReverse', 'MountainLoopReverse')):
    R = old(name); P = pts(R); S = stations(P); lines = scene_lines(sc)
    say('== %s (%s), lap %.1f m' % (R['title'], sc, S[-1]))
    # Part A: the closure in front of Dan's spot (within 25 m, ahead of his heading)
    x, z, h = SPOTS[name]; fx, fz = math.sin(math.radians(h)), math.cos(math.radians(h))
    cand = [(math.hypot(b['pos'][0] - x, b['pos'][1] - z), i, b) for i, b in enumerate(R['blocks']) if (b['pos'][0] - x) * fx + (b['pos'][1] - z) * fz > 0]
    d, i, b = min(cand); assert d < 25, d
    say("  Part A: removed the closure %.1f m ahead of Dan's spot (%.2f, %.2f): at (%.2f, %.2f) yaw %.0f half %.1f%s: %s" % (d, x, z, b['pos'][0], b['pos'][1], b['yaw'], b.get('half', 0), ', signs both ways' if b.get('both') else '', b['at']))
    R['blocks'] = [bb for j, bb in enumerate(R['blocks']) if j != i]
    say('  Part A: %d other closures kept as they are' % len(R['blocks']))
    # Part A2: the bank
    R['reshape'] = [RESHAPE]; n = 0; P2 = []
    for q in P:
        if on_bank(q[0], q[2]): y = round(ground(q[0], q[2]), 3); n += abs(y - q[1]) > .01; P2.append((q[0], y, q[2]))
        else: P2.append(q)
    R['pts'] = [round(v, 3) for q in P2 for v in q]; P = P2
    say('  Part A2: the bank on this course; %d line points on it set to the new ground' % n)
    # Part B: the turn signs grouped by decision point
    groups = {}
    for sg in R['signs']: groups.setdefault(sg['at'], []).append(sg)
    keep = []; turns = []
    for label, ss in groups.items():
        if all(s['kind'] == 'board' for s in ss): keep += ss; continue  # a shortcut's own boards (its name, REJOIN ROAD): not turn signs
        arrow = next(s for s in ss if s['kind'] == 'arrow'); ch = [s for s in ss if s['kind'] == 'chevron']
        sa, da = project(P, S, (arrow['pos'][0], arrow['pos'][1]), arrow['yaw'])
        cx = sum(s['pos'][0] for s in ch) / len(ch); cz = sum(s['pos'][1] for s in ch) / len(ch)
        # the turn: the point of the line, in the 150 m after the arrow, nearest the chevrons (on the outside of the turn)
        st = min((math.hypot(at(P, S, s)[0] - cx, at(P, S, s)[2] - cz), s) for s in [sa + k for k in range(0, 151)])[1]
        before = which(lines, at(P, S, st - 35)); after = which(lines, at(P, S, st + 35))
        road = lambda w: w[1] is not None and w[0] < 6 and any(w[1].endswith(r) for r in ROADS)
        corner = road(before) and road(after)
        turns.append({'at': label, 'station': round(st % S[-1], 1), 'arrowStation': round(sa, 1), 'corner': corner, 'before': before[1], 'after': after[1]})
        say('  %-55s turn at s %7.1f (arrow s %7.1f): %s -> %s: %s' % (label, st % S[-1], sa, (before[1] or '-').split('/')[-1], (after[1] or '-').split('/')[-1],
            'ROAD CORNER: arrow removed, %d chevrons kept' % len(ch) if corner else 'off a road: arrow and %d chevrons removed, a gate in / just past the turn' % len(ch)))
        if corner: keep += ch
    say('  Part B: arrow boards removed: %d; chevrons kept %d of %d' % (sum(s['kind'] == 'arrow' for s in R['signs']), sum(s['kind'] == 'chevron' for s in keep), sum(s['kind'] == 'chevron' for s in R['signs'])))
    R['signs'] = keep; TURNS[name] = turns
    open('Tools/Report118/py/stage1-%s.json' % name, 'w', encoding='utf-8', newline='\n').write(json.dumps(R, separators=(',', ':')))
json.dump(TURNS, open('Tools/Report118/py/turns118.json', 'w'), indent=1)
open('Tools/Report118/py/step1.txt', 'w', encoding='utf-8').write('\n'.join(log) + '\n')
