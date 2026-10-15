# 0.118 Part A2 (Dan: "Can we smooth out Kyle's hill to make this easier to traverse"): the 0.117 link from the end of Kyle's driveway up the
# grass bank behind the garage to the lake path's west side, made one even slope. Race only: the scene's ground tile (Ground_640_320, a 1 m
# vertex grid, the same in MountainLoop and MountainLoopReverse) is given these vertex heights when a Leviathan race loads; Free Roam and every
# other course keep the scene's ground. Along the link (Forward's line, stations s0..s1): from the driveway pad's edge to the lake path, an
# ease, one constant grade, an ease; flat across 9 m (two vehicles side by side), blending into the ground over 7 m either side. The pad
# (Kyle's drive surface), the house and everything south of the pad are left as they are.
# usage (project root): python Tools/Report118/py/bank.py <bankgrid-MountainLoop.txt (Report118Bank)>
import json, math, sys
GRID = sys.argv[1]
G, TOP = {}, {}
for ln in open(GRID):
    x, z, g, t, n = ln.split(); G[(int(x), int(z))] = float(g); TOP[(int(x), int(z))] = n
def gy(x, z, H=G):
    x0, z0 = math.floor(x), math.floor(z); fx, fz = x - x0, z - z0
    return (H[(x0, z0)] * (1 - fx) + H[(x0 + 1, z0)] * fx) * (1 - fz) + (H[(x0, z0 + 1)] * (1 - fx) + H[(x0 + 1, z0 + 1)] * fx) * fz
pad = lambda x, z: 'driveway' in TOP.get((int(round(x)), int(round(z))), '')
R = json.load(open('Assets/Resources/Leviathan/LeviathanForward.json', encoding='utf-8'))
P = [tuple(R['pts'][i:i + 3]) for i in range(0, len(R['pts']), 3)]
S = [0.0]
for i in range(1, len(P)): S.append(S[-1] + math.dist(P[i], P[i - 1]))
LINE = [(P[i][0], P[i][2], S[i]) for i in range(len(P)) if 655 <= S[i] <= 712]
HALF, BLEND = 4.5, 4.0  # 7 m: the cut face on the inside of the curve no steeper than the bank was (4 m left steps of 1.7 m)
# the eases: 3 m at the foot (onto the pad: a hollow, it presses the wheels down), 9 m at the top (a crest throws a car that is quicker than
# sqrt(9.81 * ease / grade): 3.5 m threw the check bot at 37 mph up and 26 mph down; 9 m holds it to about 42 mph)
EASE, EASE_TOP = 3.0, 9.0
# s0: where the line leaves the pad; s1: the lake path's centre (the 0.117 join)
s0 = next(s for x, z, s in LINE if s > 668 and not pad(x, z)); s1 = 698.4
def xz(s):
    for (ax, az, sa), (bx, bz, sb) in zip(LINE, LINE[1:]):
        if sa <= s <= sb: t = (s - sa) / (sb - sa); return ax + (bx - ax) * t, az + (bz - az) * t
y0 = gy(*xz(s0)); y1 = gy(*xz(s1)); L = s1 - s0; g = (y1 - y0) / (L - EASE / 2 - EASE_TOP / 2)
def prof(s):
    if s <= s0: return y0
    if s >= s1: return y1
    u = s - s0
    if u < EASE: return y0 + g * u * u / (2 * EASE)
    if u > L - EASE_TOP: v = L - u; return y1 - g * v * v / (2 * EASE_TOP)
    return y0 + g * (u - EASE / 2)
def near(x, z):
    best = (1e9, 0)
    for (ax, az, sa), (bx, bz, sb) in zip(LINE, LINE[1:]):
        dx, dz = bx - ax, bz - az; t = max(0, min(1, ((x - ax) * dx + (z - az) * dz) / (dx * dx + dz * dz)))
        d = math.hypot(x - ax - dx * t, z - az - dz * t)
        if d < best[0]: best = (d, sa + (sb - sa) * t)
    return best
# the line every 0.5 m from 6 m before the pad's edge to 6 m past the join: (x, z, its height on the slope, its weight along the line: past
# the join back into the lake path over 6 m; before the pad's edge, the lawn beside the pad where the line turns onto it, down to the pad's
# height over 6 m (0.118 check: left as it was, it stood up to 1 m over the lowered ground next to it, a ridge the bikes ran onto))
SAMPLES = []
for k in range(int(((s1 + 6) - (s0 - 6)) / .5) + 1):
    s_ = s0 - 6 + .5 * k; x_, z_ = xz(s_)
    SAMPLES.append((x_, z_, prof(s_), 1 - (lambda t: t * t * (3 - 2 * t))(min(1, (s_ - s1) / 6)) if s_ > s1 else 1 - (lambda t: t * t * (3 - 2 * t))(min(1, (s0 - s_) / 6)) if s_ < s0 else 1))
smooth = lambda t: t * t * (3 - 2 * t)
NEW = dict(G); changed = []
for (x, z), y in G.items():
    if not (498 <= x <= 534 and -26 <= z <= 6): continue
    if pad(x, z) or z < -24 or y != y: continue  # the drive surface, the house side
    # how much the point takes the slope: its strongest sample's weight (across the line: flat to HALF, blending out over BLEND; along it: the
    # sample's own). The height it takes: the samples' heights blended by how near each is, against the nearest (within 1 m of it they count
    # most): one value on the line, and between the two arms of the S-bend, on its inside, a blend of both (no step where the nearer one changes)
    ds = [(math.hypot(x - sx, z - sz), sy, sw) for sx, sz, sy, sw in SAMPLES]
    dmin = min(d for d, sy, sw in ds)
    if dmin >= HALF + BLEND: continue
    W = max((1 if d <= HALF else 1 - smooth(min(1, (d - HALF) / BLEND))) * sw for d, sy, sw in ds)
    if W <= 0: continue
    ks = [(math.exp(-((d - dmin) / 1.0) ** 2) * max(sw, 1e-3), sy) for d, sy, sw in ds]
    t = sum(k * sy for k, sy in ks) / sum(k for k, sy in ks); ny = y + (t - y) * W
    # next to the pad: never above the drive surface beside it (the drive keeps its own edge)
    for ox, oz in ((1, 0), (-1, 0), (0, 1), (0, -1)):
        if pad(x + ox, z + oz): ny = min(ny, G[(x + ox, z + oz)] + .02)
    if abs(ny - y) > .01: NEW[(x, z)] = round(ny, 3); changed.append((x, z, round(ny, 3), round(ny - y, 3)))
say = []
say.append('Kyle\'s bank (0.118): s0 %.1f (%.1f, %.1f) %.2f m -> s1 %.1f (%.1f, %.1f) %.2f m: %.1f m long, rise %.2f m, constant grade %.1f %% (eases %.1f m at the foot, %.1f m at the top: a car leaves the crest above %.0f mph)' % (s0, *xz(s0), y0, s1, *xz(s1), y1, L, y1 - y0, 100 * g, EASE, EASE_TOP, 2.237 * math.sqrt(9.81 * EASE_TOP / g)))
# the largest step between neighbouring vertices (1 m apart) on and beside the line: no ridge or lip
step = max((abs(NEW[(x, z)] - NEW[(x + dx, z + dz)]), (x, z, dx, dz)) for (x, z) in NEW if 498 <= x <= 534 and -26 <= z <= 6 and near(x, z)[0] <= HALF + BLEND
           for dx, dz in ((1, 0), (0, 1)) if (x + dx, z + dz) in NEW and NEW[(x, z)] == NEW[(x, z)] and not pad(x, z) and not pad(x + dx, z + dz))
before = max(abs(G[(x, z)] - G[(x + dx, z + dz)]) for (x, z) in NEW if 498 <= x <= 534 and -26 <= z <= 6 and near(x, z)[0] <= HALF + BLEND
             for dx, dz in ((1, 0), (0, 1)) if (x + dx, z + dz) in G and not pad(x, z) and not pad(x + dx, z + dz))
say.append('largest step between neighbouring ground vertices (1 m) within %.1f m of the line, the pad left out: %.2f m at %s (the bank there before: %.2f m)' % (HALF + BLEND, step[0], step[1][:2], before))
say.append('%d ground vertices changed: raised up to %.2f m, lowered up to %.2f m' % (len(changed), max(c[3] for c in changed), -min(c[3] for c in changed)))
# the result along the line and across it (from the new vertex heights)
worst4 = 0; worstX = 0; prev = None
rows = []
for k in range(int((s1 - s0 + 8) / 2) + 1):
    s = s0 - 4 + 2 * k; x, z = xz(s); y = gy(x, z, NEW); yo = gy(x, z)
    a = xz(max(s0 - 6, s - 1)); b = xz(s + 1); dx, dz = b[0] - a[0], b[1] - a[1]; n = math.hypot(dx, dz); rx, rz = dz / n, -dx / n
    cross = max(abs(gy(x + rx * o, z + rz * o, NEW) - y) / abs(o) for o in (-4.5, -3, 3, 4.5))
    rows.append((s, x, z, yo, y, cross)); worstX = max(worstX, cross) if s0 <= s <= s1 else worstX
for i in range(len(rows) - 2):
    worst4 = max(worst4, abs(rows[i + 2][4] - rows[i][4]) / math.hypot(rows[i + 2][1] - rows[i][1], rows[i + 2][2] - rows[i][2]))
say.append('steepest along the line over 4 m: %.1f %% (0.117: 34 %%); steepest across the 9 m (4.5 m either side): %.1f %%' % (100 * worst4, 100 * worstX))
say.append('   s      x      z   ground  new   (across)')
for r in rows: say.append('%6.1f %6.1f %6.1f %6.2f %6.2f  %4.0f %%' % (r[0], r[1], r[2], r[3], r[4], 100 * r[5]))
print('\n'.join(say))
out = {'tile': 'Ground_640_320', 'at': "Kyle's bank (0.118 Part A2)", 'verts': [v for c in sorted(changed) for v in c[:3]]}
json.dump(out, open('Tools/Report118/py/bank-reshape.json', 'w'), separators=(',', ':'))
json.dump({'s0': s0, 's1': s1, 'y0': y0, 'y1': y1, 'g': g, 'ease': EASE, 'easeTop': EASE_TOP}, open('Tools/Report118/py/bank-profile.json', 'w'))
open('Tools/Report118/py/bank.txt', 'w', encoding='utf-8').write('\n'.join(say) + '\n')
