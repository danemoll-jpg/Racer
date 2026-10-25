"""1.3 Free Roam world: places the items (Assets/Resources/RoamWorld.json) from the survey (-p1300part survey: grid.txt, world.json) and the
course lines (Assets/Resources/WorldMaps/CoursePreviews.json). Rules (PROJECT_TODO CURRENT 1.3):
  ramps: 8-12, mostly in the middle areas, none near the edge (150 m+), 35 m+ from every race line (main and shortcuts, every course), off roads,
         nothing standing on the run-up, ramp or landing zone (trees there are cleared), not near Dan's or Kyle's house, the stunt park or existing jumps;
  house jumps over existing houses (not Dan's or Kyle's), one through a barn; smash spots 3-4; dead ends where a road meets the world's edge.
Usage: place.py <survey dir> [--write]"""
import json, math, sys, pathlib, collections
import numpy as np

root = pathlib.Path(__file__).resolve().parents[3]
survey = pathlib.Path(sys.argv[1])
head, *rows = (survey / 'grid.txt').read_text().splitlines()
_, X0, Z0, STEP, NX, NZ = head.split()[:6]; X0, Z0, STEP, NX, NZ = float(X0), float(Z0), float(STEP), int(NX), int(NZ)
cls = np.full((NZ, NX), 'N'); obs = np.zeros((NZ, NX), bool); top = np.full((NZ, NX), np.nan); gnd = np.full((NZ, NX), np.nan)
for j, row in enumerate(rows[:NZ]):
    for i, cell in enumerate(row.rstrip(',').split(',')[:NX]):
        k, t, g = cell.split(' ')
        cls[j, i] = k[0]; obs[j, i] = 'O' in k
        top[j, i] = float(t) if t != '-' else np.nan; gnd[j, i] = float(g) if g != '-' else np.nan
world = json.loads((survey / 'world.json').read_text())
# what stands above the ground beside a tree trunk is the tree (trees on an item's ground are cleared): those cells count as trees, not obstacles
trunkcell = np.zeros((NZ, NX), bool)
for tx, ty, tz, tw in world['trees']:
    for dx in (-3, 0, 3):
        for dz in (-3, 0, 3):
            j, i = int(math.floor((tz + dz - Z0) / STEP)), int(math.floor((tx + dx - X0) / STEP))
            if 0 <= j < NZ and 0 <= i < NX: trunkcell[j, i] = True
treeobs = obs & trunkcell; obs = obs & ~trunkcell
courses = json.loads((root / 'Assets/Resources/WorldMaps/CoursePreviews.json').read_text(encoding='utf-8'))['courses']

def cell(x, z): return int(math.floor((z - Z0) / STEP)), int(math.floor((x - X0) / STEP))
def inside(j, i): return 0 <= j < NZ and 0 <= i < NX
def H(x, z):
    j, i = cell(x, z)
    if not inside(j, i): return np.nan
    return gnd[j, i] if not np.isnan(gnd[j, i]) else top[j, i]

# distances (m) from race lines and from the edge, by a multi-source BFS on the grid (8-neighbour, approximate)
def bfs(seed):
    from scipy.ndimage import distance_transform_edt
    return distance_transform_edt(~seed) * STEP if seed.any() else np.full((NZ, NX), 1e9)
line = np.zeros((NZ, NX), bool); roam_line = np.zeros((NZ, NX), bool)
for c in courses:
    paths = [c['main']] + [b['points'] for b in c.get('branches', [])]
    for pts in paths:
        for a, b in zip(pts, pts[1:]):
            n = max(1, int(math.dist((a['x'], a['z']), (b['x'], b['z'])) / 2))
            for t in range(n + 1):
                x = a['x'] + (b['x'] - a['x']) * t / n; z = a['z'] + (b['z'] - a['z']) * t / n; j, i = cell(x, z)
                if inside(j, i): line[j, i] = True; roam_line[j, i] |= not c.get('raceOnly', False)
d_line = bfs(line); d_roamline = bfs(roam_line)
edge = (cls == 'E') | (cls == 'N'); edge[0, :] = edge[-1, :] = True; edge[:, 0] = edge[:, -1] = True; d_edge = bfs(edge)
d_road = bfs(cls == 'R')

# places to stay away from
avoid = [((434, -55), 140, "Dan's house"), ((505, -70), 140, "Kyle's house"), ((70, 683), 140, 'the stunt park')]
for s in world['sites']: avoid.append(((s['at'][0], s['at'][2]), s['radius'] + 40, 'activity site ' + s['id']))
def avoided(x, z): return next((w for (ax, az), r, w in avoid if math.dist((x, z), (ax, az)) < r), None)
MIDX, MIDZ = 128.0, 11.0  # the middle of the playable world (the 0.93 discovery grid's centre area)

def frame(yaw):
    f = (math.sin(math.radians(yaw)), math.cos(math.radians(yaw))); r = (f[1], -f[0]); return f, r
def rect_cells(x, z, yaw, back, ahead, half, skip=None):
    """cells of an oriented rectangle (along -back..ahead, across -half..half) around (x, z)"""
    f, r = frame(yaw); out = []
    for s in np.arange(-back, ahead + .1, STEP * .7):
        for w in np.arange(-half, half + .1, STEP * .7):
            if skip and skip(s, w): continue
            out.append(cell(x + f[0] * s + r[0] * w, z + f[1] * s + r[1] * w))
    return out
def footprint_ok(cells, allow_road=False, trunks_max=120):
    seen = set(); trunks = 0
    for j, i in cells:
        if (j, i) in seen: continue
        seen.add((j, i))
        if not inside(j, i): return False, 'outside'
        k = cls[j, i]
        if k in 'EN': return False, 'edge'
        if k == 'B': return False, 'building'
        if k == 'R' and not allow_road: return False, 'road'
        if k == 'T' or treeobs[j, i]: trunks += 1; continue
        if obs[j, i]: return False, 'something standing'
    if trunks > trunks_max: return False, 'forest'
    return True, trunks
def profile_ok(x, z, yaw, back, ahead, tol=1.1, grade=.08):
    f, _ = frame(yaw); ss = np.arange(-back, ahead + .1, 4.0); hs = np.array([H(x + f[0] * s, z + f[1] * s) for s in ss])
    if np.isnan(hs).any(): return False, 99
    a, b = np.polyfit(ss, hs, 1); res = np.abs(hs - (a * ss + b)).max()
    _, r = frame(yaw); cross = max(abs(H(x + r[0] * 6, z + r[1] * 6) - H(x - r[0] * 6, z - r[1] * 6)), 0)
    return res < tol and abs(a) < grade and cross < 1.5, res + abs(a) * 10 + cross * .5
def clear_of(x, z, back, ahead, yaw, dmin, grid):
    f, _ = frame(yaw)
    for s in np.arange(-back, ahead + .1, 4):
        j, i = cell(x + f[0] * s, z + f[1] * s)
        if not inside(j, i) or grid[j, i] < dmin: return False
    return True

LIP = lambda h, a: h / (1 - math.cos(math.radians(a))) * math.sin(math.radians(a))
SPECS = {  # kind: height, angle, width, gap, run-up, ahead (landing zone)
    'table': (6.0, 32, 12, 30), 'kicker': (2.4, 26, 9, 0), 'gap': (3.6, 30, 10, 30), 'through': (2.6, 15, 11, 5)}
chosen = []
def far_from_chosen(x, z, d=170): return all(math.dist((x, z), (c['pos'][0], c['pos'][1])) > d for c in chosen)
def middle(x, z): return math.dist((x, z), (MIDX, MIDZ))

def ramp_candidates(kind):
    h, a, w, gap = SPECS[kind]; lip = LIP(h, a); back = lip + 60; ahead = {'table': gap + 60, 'gap': gap + 60, 'kicker': 45, 'through': gap + 16 + 50}[kind]
    out = []
    for j in range(0, NZ, 3):
        for i in range(0, NX, 3):
            if cls[j, i] != 'G' or d_edge[j, i] < 150 or d_line[j, i] < 35: continue
            x, z = X0 + (i + .5) * STEP, Z0 + (j + .5) * STEP
            if avoided(x, z): continue
            for yaw in range(0, 360, 15):
                if not clear_of(x, z, back, ahead, yaw, 35, d_line) or not clear_of(x, z, back, ahead, yaw, 150, d_edge) or not clear_of(x, z, back, ahead, yaw, 10, d_road): continue
                ok, tr = footprint_ok(rect_cells(x, z, yaw, back, ahead, w * .5 + 3))
                if not ok: continue
                pk, rough = profile_ok(x, z, yaw, back, ahead)
                if not pk: continue
                if middle(x, z) > 650: continue
                out.append((rough * 2 + tr * .05 + middle(x, z) / 60, x, z, yaw, tr))
    out.sort(); return out

def take(kind, n, extra=None):
    got = 0
    for score, x, z, yaw, tr in ramp_candidates(kind):
        if got >= n: break
        if not far_from_chosen(x, z): continue
        item = dict(kind=kind, pos=[round(float(x), 1), round(float(z), 1)], yaw=yaw, trunks=tr, score=round(float(score), 2)); item.update(extra or {})
        chosen.append(item); got += 1
    return got

# house jumps: over an existing house (not Dan's or Kyle's), the kicker 9 m before its near wall, the landing 10 m past its far wall
def house_jumps(n):
    got = 0; h, a, v = 4.5, 32, 22.0
    for hs in world['houses']:
        name = hs['name']
        if any(k in name for k in ("Dan", "Kyle", "Friend across", "drive-through")): continue
        (x0, y0, z0), (x1, y1, z1) = hs['min'], hs['max']; cx_, cz_ = (x0 + x1) / 2, (z0 + z1) / 2
        g = H(cx_, cz_); height = y1 - g
        if np.isnan(g) or height > 10 or height < 3 or (x1 - x0) > 30 or (z1 - z0) > 30: continue
        why = avoided(cx_, cz_) or ('edge' if d_edge[cell(cx_, cz_)] < 120 else 'race line %.0f m' % d_line[cell(cx_, cz_)] if d_line[cell(cx_, cz_)] < 35 else None)
        if why: print('  house', name, cx_, cz_, 'skipped:', why); continue
        reasons = collections.Counter()
        best = None
        for yaw in range(0, 360, 15):
            f, r = frame(yaw)
            depth = max(abs((x - cx_) * f[0] + (z - cz_) * f[1]) for x in (x0, x1) for z in (z0, z1))
            half_w = max(abs((x - cx_) * r[0] + (z - cz_) * r[1]) for x in (x0, x1) for z in (z0, z1))
            if half_w > 13: reasons['wide'] += 1; continue  # flown over its middle: up to 26 m across
            lipx, lipz = cx_ - f[0] * (depth + 9), cz_ - f[1] * (depth + 9); gap = 2 * depth + 19
            def skip(s, w): return 3 < s < gap - 4
            ok, tr = footprint_ok(rect_cells(lipx, lipz, yaw, LIP(h, a) + 60, gap + 60, 8, skip))
            if not ok: reasons[tr] += 1; continue
            if not clear_of(lipx, lipz, LIP(h, a) + 60, gap + 60, yaw, 35, d_line) or not clear_of(lipx, lipz, LIP(h, a) + 60, gap + 60, yaw, 8, d_road): reasons['line/road'] += 1; continue
            # the flight at 22 and 27 m/s clears the roof by 1.5 m over the house's depth
            lipy = H(lipx, lipz) + h; fine = True
            for speed in (v, 27.0):
                vx, vy = speed * math.cos(math.radians(a)), speed * math.sin(math.radians(a))
                for s in np.arange(9 - 1, 9 + 2 * depth + 1, 1.0):
                    t = s / vx; y = lipy + vy * t - 4.9 * t * t
                    if y < y1 + 1.5: fine = False; break
            if not fine: reasons['flight'] += 1; continue
            pk, rough = profile_ok(lipx, lipz, yaw, LIP(h, a) + 60, 0, tol=1.3)
            if not pk: reasons['slope'] += 1; continue
            if best is None or rough < best[0]: best = (rough, lipx, lipz, yaw, gap, tr, name, height)
        if not best: print('  house', name, round(cx_), round(cz_), 'h', round(height,1), dict(reasons))
        if best and far_from_chosen(best[1], best[2], 150):
            _, lipx, lipz, yaw, gap, tr, name, height = best
            chosen.append(dict(kind='house', house=name, pos=[round(lipx, 1), round(lipz, 1)], yaw=yaw, gap=round(gap, 1), height=h, angle=a, trunks=tr, houseHeight=round(height, 1))); got += 1
            if got >= n: break
    return got

def smash_spots(n):
    got = 0; cands = []
    for j in range(0, NZ, 2):
        for i in range(0, NX, 2):
            if cls[j, i] != 'G' or d_edge[j, i] < 150 or d_line[j, i] < 30: continue
            x, z = X0 + (i + .5) * STEP, Z0 + (j + .5) * STEP
            if avoided(x, z) or middle(x, z) > 650: continue
            for yaw in range(0, 360, 45):
                ok, tr = footprint_ok(rect_cells(x, z, yaw, 24, 10, 12))
                if not ok: continue
                pk, rough = profile_ok(x, z, yaw, 24, 10, tol=.8)
                if pk: cands.append((rough + tr * .05 + middle(x, z) / 80, x, z, yaw, tr))
    cands.sort()
    for score, x, z, yaw, tr in cands:
        if got >= n: break
        if not far_from_chosen(x, z, 150): continue
        chosen.append(dict(kind='smash', pos=[round(float(x), 1), round(float(z), 1)], yaw=yaw, trunks=tr, score=round(float(score), 2))); got += 1
    return got

def dead_ends():
    """road cells within 16 m of the edge, grouped; each group's way in from the road's own line"""
    near = np.argwhere((cls == 'R') & (d_edge <= 16)); groups = []
    for j, i in near:
        x, z = X0 + (i + .5) * STEP, Z0 + (j + .5) * STEP
        for g in groups:
            if math.dist((x, z), g['c']) < 40: g['pts'].append((x, z)); g['c'] = tuple(np.mean(g['pts'], 0)); break
        else: groups.append(dict(c=(x, z), pts=[(x, z)]))
    out = []
    for g in groups:
        ex, ez = g['c']
        # the road behind: road cells 20-70 m from the end
        back = [(X0 + (i + .5) * STEP, Z0 + (j + .5) * STEP) for j, i in np.argwhere(cls == 'R') if 20 < math.dist((X0 + (i + .5) * STEP, Z0 + (j + .5) * STEP), (ex, ez)) < 70]
        if len(back) < 6: continue
        bx, bz = np.mean(back, 0); yaw = math.degrees(math.atan2(ex - bx, ez - bz)) % 360
        out.append(dict(end=[round(float(ex), 1), round(float(ez), 1)], yaw=round(yaw), cells=len(g['pts']), raceLine=round(float(d_roamline[cell(ex, ez)]))))
    return out

if __name__ == '__main__':
    print(f'middle of the world ~ ({MIDX:.0f}, {MIDZ:.0f}); grid {NX}x{NZ}')
    ends = dead_ends(); print('dead ends:'); [print(' ', e) for e in ends]
    print('houses', house_jumps(2)); print('through', take('through', 1)); print('tables', take('table', 3), 'gaps', take('gap', 2), 'kickers', take('kicker', 3)); print('smash', smash_spots(4))
    for c in chosen: print(c)
    (survey / 'placed.json').write_text(json.dumps(dict(items=chosen, ends=ends), indent=1))
