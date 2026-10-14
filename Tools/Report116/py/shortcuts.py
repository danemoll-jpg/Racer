# 0.116 Part B: Leviathan Reverse's own shortcuts (MountainLoopReverse.unity), composed from the lines planned on the scene's ground
# (plan.py over Report116Grid's height grids, along the Street Loop Forward trails that this scene also has, driven the other way).
# Each line is smoothed, resampled every 2 m and seated on the grid's ground; its entry / exit stations are on Leviathan Reverse's line
# (3D stations, as RaceRoad measures them) and the gates between them are its bypassed gates. Writes the branches into
# Assets/Resources/Leviathan/LeviathanReverse.json and the Tracks preview (CoursePreviews.json, LeviathanReverse only).
# usage: shortcuts.py <grid dir> <grid dir 2> <grid dir 3>
import json, math, sys, os
sys.path.insert(0, os.path.dirname(__file__))
from plan import load
G = sys.argv[1]
LEV = 'Assets/Resources/Leviathan/LeviathanReverse.json'
R = json.load(open(LEV))
line = [(R['pts'][i], R['pts'][i + 1], R['pts'][i + 2]) for i in range(0, len(R['pts']), 3)]
st = [0.0]
for i in range(1, len(line)): st.append(st[-1] + math.dist(line[i], line[i - 1]))
def station(q, lo=0, hi=1e9):
    # the nearest point of the line between stations lo and hi (the line passes the stem and the lake shore twice)
    best, s = 1e18, 0
    for i in range(len(line) - 1):
        if st[i + 1] < lo or st[i] > hi: continue
        a, b = line[i], line[i + 1]; v = [b[k] - a[k] for k in range(3)]; L = sum(x * x for x in v) or 1e-9
        t = max(0, min(1, sum((q[k] - a[k]) * v[k] for k in range(3)) / L)); d = sum((q[k] - a[k] - v[k] * t) ** 2 for k in range(3))
        if d < best: best, s = d, st[i] + t * (st[i + 1] - st[i])
    return s, math.sqrt(best)
def smooth(p, n):
    q = [list(x) for x in p]
    for _ in range(n):
        q = [q[0]] + [[(q[k - 1][d] + 2 * q[k][d] + q[k + 1][d]) / 4 for d in range(3)] for k in range(1, len(q) - 1)] + [q[-1]]
    return q
def resample(p, step=2.0):
    out = [p[0]]; acc = 0
    for a, b in zip(p, p[1:]):
        d = math.dist((a[0], a[2]), (b[0], b[2])); t = step - acc
        while t <= d: out.append([a[k] + (b[k] - a[k]) * t / d for k in range(3)]); t += step
        acc = d - (t - step)
    if math.dist((out[-1][0], out[-1][2]), (p[-1][0], p[-1][2])) > .5: out.append(list(p[-1]))
    return out
def ground(cells, xs, zs, x, z):
    st_ = xs[1] - xs[0]; i = (x - xs[0]) / st_; j = (z - zs[0]) / st_; i0, j0 = int(i), int(j); fx, fz = i - i0, j - j0
    def h(a, b): return cells[(round(xs[min(len(xs) - 1, a)], 1), round(zs[min(len(zs) - 1, b)], 1))][0]
    return (h(i0, j0) * (1 - fx) + h(i0 + 1, j0) * fx) * (1 - fz) + (h(i0, j0 + 1) * (1 - fx) + h(i0 + 1, j0 + 1) * fx) * fz
SPECS = [
    # (title, planned line, grid, rivals' speed on it (m/s), half width); grid dir: argv[1] (grid1: creek, pine), argv[2] (grid2: jcut, sw)
    # Rejected (0.116 checks): Ridgeback Climb (Pine Ridge driven uphill: 7-9 s slower than the road for every rival) and Creekbed Scramble
    # (Creek Leap the other way, round the leap's bank: at best 1 s faster, rivals stalled at the creek crossing).
    # Woodcutter's Corner (the Southwest Cut's trail the other way) was also rejected: rivals as quick round the corner (0 s), the pad 1.2 s.
    ('Hollow Creek Hop', 'plan-u-3380', 'ubend', 22, 4.5, (3300, 3620)),
    ('Rocky Way Cut', 'plan-j-3900', 'jcut', 18, 4.0, (3850, 4150)),  # its outbound pass (the stem is driven back at 6400-6500)
]
G2 = sys.argv[2] if len(sys.argv) > 2 else G; G3 = sys.argv[3] if len(sys.argv) > 3 else G
branches, previews = [], []
for title, plan, grid, speed, hw, (lo, hi) in SPECS:
    gd = G2 if grid in ('jcut', 'sw') else G3 if grid == 'ubend' else G
    path = json.load(open(os.path.join(gd, plan + '.json')))
    cells, xs, zs = load(os.path.join(gd, 'grid-%s.txt' % grid))
    # a smooth line through the planned path: control points every 14 m (the grid path's 2 m stair steps and the detours' kinks
    # smoothed out), a Catmull-Rom spline through them, resampled every 2 m
    q = resample(smooth(resample(path, 2), 6), 14)
    q = [q[0]] + q[1:-1] + [list(path[-1])] if math.dist((q[-1][0], q[-1][2]), (path[-1][0], path[-1][2])) > 4 else q
    def cr(p0, p1, p2, p3, t): return [0.5 * ((2 * p1[d]) + (-p0[d] + p2[d]) * t + (2 * p0[d] - 5 * p1[d] + 4 * p2[d] - p3[d]) * t * t + (-p0[d] + 3 * p1[d] - 3 * p2[d] + p3[d]) * t ** 3) for d in range(3)]
    ext = [q[0]] + q + [q[-1]]; dense = []
    for k in range(1, len(ext) - 2):
        for t in range(10): dense.append(cr(ext[k - 1], ext[k], ext[k + 1], ext[k + 2], t / 10))
    dense.append(q[-1]); p = resample(dense, 2)
    for q in p: q[1] = ground(cells, xs, zs, q[0], q[2])
    entry, de = station(p[0], lo, hi); exit_, dx = station(p[-1], lo, hi)
    gates = [k for k, g in enumerate(R['gates']) if entry < g['station'] < exit_]
    L = sum(math.dist(p[k], p[k - 1]) for k in range(1, len(p)))
    # the entry plane (entryInset along the shortcut) where the shortcut has left the road: more than its half width + the entry margin
    # + 1.5 m from the line, so driving on along the main road never reads as taking it
    margin = 4; inset = 0; acc = 0
    for k in range(1, len(p)):
        acc += math.dist((p[k][0], p[k][2]), (p[k - 1][0], p[k - 1][2]))
        if station(p[k], lo, hi)[1] > hw + margin + 1.5: inset = acc + 2; break
    flat = [round(v, 3) for q in p for v in q]
    branches.append({'title': title, 'from': 'Leviathan Reverse (0.116): ' + plan, 'pts': flat, 'aiPts': flat, 'entryRoad': entry, 'exitRoad': exit_,
                     'halfWidth': hw, 'recommendedSpeed': speed, 'undergroundStart': 0, 'undergroundEnd': 0, 'entryHeightBelow': 3, 'entryHeightAbove': 12,
                     'entrySpeed': 0, 'entrySpeedDistance': 0, 'entryInset': round(inset, 1), 'entryMargin': margin, 'bypassedGates': gates, 'aiValidated': True})
    previews.append({'points': [{'x': q[0], 'y': q[1], 'z': q[2]} for q in p], 'undergroundStart': 0.0, 'undergroundEnd': 0.0})
    print(f'{title}: {L:.0f} m ({len(p)} points), entry plane {inset:.0f} m in, entry s {entry:.0f} ({de:.1f} m off the line), exit s {exit_:.0f} ({dx:.1f} m off), main road {exit_ - entry:.0f} m, '
          f'{exit_ - entry - L:.0f} m shorter, bypasses gates {gates}; ground {min(q[1] for q in p):.1f}-{max(q[1] for q in p):.1f}')
R['branches'] = branches
R['id'] = 'leviathan-reverse-v2'
open(LEV, 'w').write(json.dumps(R, separators=(',', ':')))
P = 'Assets/Resources/WorldMaps/CoursePreviews.json'
# text surgery on the LeviathanReverse entry only, so every other course stays byte for byte as it is
raw = open(P, encoding='utf-8', newline='').read(); i = raw.index('"scene": "LeviathanReverse"')
a = raw.index('"branches": [', i); depth = 0; k = raw.index('[', a)
for j in range(k, len(raw)):
    depth += raw[j] == '['; depth -= raw[j] == ']'
    if depth == 0: break
NL = chr(10); indent = raw[raw.rindex(NL, 0, a) + 1:a]
body = json.dumps(previews, indent=4).replace(NL, NL + indent)
raw = raw[:a] + '"branches": ' + body + raw[j + 1:]
b = raw.index('"id": "leviathan-reverse-v', i); e = raw.index('"', b + 7)
raw = raw[:b] + '"id": "' + R['id'] + raw[e:]
open(P, 'w', encoding='utf-8', newline='').write(raw)
