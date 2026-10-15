# 0.117 Part A: every place where another drivable line of the scene (RaceRoad / WoodlandRoute, dumped by Report117Probe.Lines) meets
# Leviathan's line and leaves it (or crosses it): its position, Leviathan's station there, the other line's heading away from it.
# python Tools/Report117/py/junctions.py <lines-dir> [Forward|Reverse]
import json, math, sys
def load_lev(name):
    R = json.load(open('Assets/Resources/Leviathan/Leviathan%s.json' % name))
    P = [(R['pts'][i], R['pts'][i + 1], R['pts'][i + 2]) for i in range(0, len(R['pts']), 3)]
    S = [0.0]
    for i in range(1, len(P)): S.append(S[-1] + math.dist(P[i], P[i - 1]))
    return R, P, S
def grid(P, cell=12):
    g = {}
    for i, q in enumerate(P): g.setdefault((int(q[0] // cell), int(q[2] // cell)), []).append(i)
    return g
def near(P, g, q, r=7, dy=4, cell=12):
    best = None
    cx, cz = int(q[0] // cell), int(q[2] // cell)
    for dx in (-1, 0, 1):
        for dz in (-1, 0, 1):
            for i in g.get((cx + dx, cz + dz), []):
                d = math.hypot(P[i][0] - q[0], P[i][2] - q[2])
                if d < r and abs(P[i][1] - q[1]) < dy and (best is None or d < best[0]): best = (d, i)
    return best
def run(dirn, scene):
    R, P, S = load_lev(dirn); g = grid(P)
    L = json.load(open('%s/lines-%s.json' % (sys.argv[1], scene)))
    out = []
    for l in L:
        if not l['active']: continue
        p = l['pts']; Q = [(p[i], p[i + 1], p[i + 2]) for i in range(0, len(p), 3)]
        on = [near(P, g, q) for q in Q]
        # transitions on <-> off (the other line joins / leaves Leviathan's), with at least 12 m off on that side
        for k in range(1, len(Q)):
            a, b = on[k - 1], on[k]
            if (a is None) == (b is None): continue
            j = k - 1 if a else k  # the last point on Leviathan
            side = range(k, min(len(Q), k + 6)) if a else range(max(0, k - 6), k)
            if any(on[m] for m in side): continue  # a short gap only
            hit = on[j]; q = Q[j]; o = Q[min(len(Q) - 1, j + 5)] if a else Q[max(0, j - 5)]
            hd = math.degrees(math.atan2(o[0] - q[0], o[2] - q[2])) % 360
            out.append((S[hit[1]], q, l['path'], hd, 'leaves' if a else 'joins'))
    out.sort()
    # merge within 25 m of station
    merged = []
    for e in out:
        if merged and abs(e[0] - merged[-1][0][0]) < 25: merged[-1].append(e)
        else: merged.append([e])
    print('==', dirn, scene, 'lap %.0f' % S[-1], len(merged), 'junction groups')
    for grp in merged:
        s0 = grp[0][0]
        print('  s %6.0f (%.0f,%.0f,%.0f): ' % (s0, *grp[0][1]) + '; '.join('%s %s heading %03.0f' % (e[2].split('/')[-1][:40], e[4], e[3]) for e in grp))
run('Forward', 'MountainLoop'); run('Reverse', 'MountainLoopReverse')
