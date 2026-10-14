# 0.116: plans a drivable line on a height grid (Report116Grid) from a start to an end, following a guide polyline where it can:
# 8-neighbour Dijkstra; uphill grade at most MAXUP (over 2 cells), blocked cells (solids) and their neighbours avoided; cost grows
# with distance from the guide. Writes <out>.json (points) and <out>.png (shaded heights, blocked red, guide yellow, line cyan).
import sys, json, math, heapq
from PIL import Image, ImageDraw
def load(path):
    cells = {}
    for ln in open(path):
        x, z, y, ny, b, surf, solid = ln.split()
        cells[(round(float(x), 1), round(float(z), 1))] = (float(y), float(ny), b == '1', surf, solid)
    xs = sorted({k[0] for k in cells}); zs = sorted({k[1] for k in cells})
    return cells, xs, zs
def plan(grid, guide, start, end, maxup=.24, maxdown=.6, corridor=6, far=1.5, avoid=(), treecost=None):
    cells, xs, zs = load(grid); step = xs[1] - xs[0]; nx, nz = len(xs), len(zs)
    H = [[cells[(xs[i], zs[j])][0] for i in range(nx)] for j in range(nz)]
    # treecost: a tree trunk (solid named 'trunk') is not a wall but a cost (it can be cleared for the race); other solids stay walls
    T = [[cells[(xs[i], zs[j])][2] and 'trunk' in cells[(xs[i], zs[j])][4].lower() for i in range(nx)] for j in range(nz)]
    B = [[(cells[(xs[i], zs[j])][2] and not (treecost and T[j][i])) or math.isnan(H[j][i]) for i in range(nx)] for j in range(nz)]
    for (ax, az, r) in avoid:
        for j in range(nz):
            for i in range(nx):
                if math.hypot(xs[i] - ax, zs[j] - az) < r: B[j][i] = True
    BB = [[any(B[jj][ii] for jj in range(max(0, j - 1), min(nz, j + 2)) for ii in range(max(0, i - 1), min(nx, i + 2))) for i in range(nx)] for j in range(nz)]
    def gd(x, z):
        best = 1e9
        for k in range(len(guide) - 1):
            a, b = guide[k], guide[k + 1]; vx, vz = b[0] - a[0], b[1] - a[1]; L = vx * vx + vz * vz or 1e-9
            t = max(0, min(1, ((x - a[0]) * vx + (z - a[1]) * vz) / L)); best = min(best, math.hypot(x - a[0] - vx * t, z - a[1] - vz * t))
        return best
    G = [[gd(xs[i], zs[j]) for i in range(nx)] for j in range(nz)]
    def idx(p): return (min(range(nz), key=lambda j: abs(zs[j] - p[1])), min(range(nx), key=lambda i: abs(xs[i] - p[0])))
    s, e = idx(start), idx(end)
    dist = {s: 0}; prev = {}; pq = [(0, s)]
    nb = [(-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1)]
    while pq:
        d, c = heapq.heappop(pq)
        if c == e: break
        if d > dist.get(c, 1e18): continue
        j, i = c
        for dj, di in nb:
            jj, ii = j + dj, i + di
            if not (0 <= jj < nz and 0 <= ii < nx) or BB[jj][ii]: continue
            L = step * math.hypot(dj, di); g = (H[jj][ii] - H[j][i]) / L
            # the grade over two cells (a single-cell spike is a kerb or a seam)
            j2, i2 = jj + dj, ii + di
            g2 = (H[j2][i2] - H[j][i]) / (2 * L) if 0 <= j2 < nz and 0 <= i2 < nx and not math.isnan(H[j2][i2]) else g
            if g > maxup * 1.6 or g2 > maxup or g < -maxdown * 1.5: continue
            w = L * (1 + max(0, G[jj][ii] - corridor) * far / 10 + max(0, g) * 2) + (treecost if treecost and T[jj][ii] else 0)
            nd = d + w
            if nd < dist.get((jj, ii), 1e18): dist[(jj, ii)] = nd; prev[(jj, ii)] = c; heapq.heappush(pq, (nd, (jj, ii)))
    if e not in dist: return None, (H, B, xs, zs)
    path = [e]
    while path[-1] != s: path.append(prev[path[-1]])
    path.reverse()
    return [(xs[i], H[j][i], zs[j]) for j, i in path], (H, B, xs, zs)
def smooth(p, n=3):
    q = list(p)
    for _ in range(n):
        q = [q[0]] + [tuple((q[k - 1][d] + 2 * q[k][d] + q[k + 1][d]) / 4 for d in range(3)) for k in range(1, len(q) - 1)] + [q[-1]]
    return q
def draw(out, grid, guide, path, extra=()):
    H, B, xs, zs = grid; nx, nz = len(xs), len(zs)
    vals = [v for row in H for v in row if not math.isnan(v)]; lo, hi = min(vals), max(vals)
    im = Image.new('RGB', (nx, nz)); px = im.load()
    for j in range(nz):
        for i in range(nx):
            y = H[j][i]
            if math.isnan(y): px[i, nz - 1 - j] = (0, 0, 0); continue
            gx = (H[j][min(nx - 1, i + 1)] - H[j][max(0, i - 1)]); c = int(60 + 150 * (y - lo) / (hi - lo + 1e-6) + max(-40, min(40, gx * 20)))
            px[i, nz - 1 - j] = (200, 40, 40) if B[j][i] else (c, c, c)
    im = im.resize((nx * 3, nz * 3)); d = ImageDraw.Draw(im); st = xs[1] - xs[0]
    P = lambda q: ((q[0] - xs[0]) / st * 3, (nz - 1 - (q[-1] - zs[0]) / st) * 3)
    d.line([P((g[0], g[1])) for g in guide], fill=(255, 220, 0), width=2)
    for ex in extra: d.line([P((g[0], g[1])) for g in ex], fill=(255, 120, 255), width=2)
    if path: d.line([P((q[0], q[2])) for q in path], fill=(0, 255, 255), width=2)
    im.save(out)
