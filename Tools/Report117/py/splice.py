# 0.117 Part B item 2 / Part C items 11 and 14 (Dan: "Maybe we just make it Kyle's driveway?"): Leviathan's way between the street and the lake
# path goes through Kyle's driveway in both directions, instead of the stem trail through the trees. The rest of each line is kept as it is:
# the two passes at J are cut out of the 0.116 line and replaced by
#   the street -> Kyle's descending driveway (its own surface, its RaceRoad centreline) -> a link over the grass bank behind the garage (planned on
#   the scene's height grid, Report117Grid; 16 % at most) -> the lake path's west side (its centreline, a lane per direction where both passes use it)
# joined by the 0.115 composer's blends (Tools/Report115/py/leviathan.py). Everything held as a station (gates, shortcuts, flights, the mountain and
# narrow spans, the trailer lane, parts, geometry stations) is moved to the new line by position. Each direction gets a gate in the driveway; Rocky
# Way Cut (Reverse) lands on the west side as before and now runs on along it to where the course comes up from the driveway.
# Reads the 0.116 lines from git (so it can be run again), writes Assets/Resources/Leviathan/Leviathan{Forward,Reverse}.json.
# usage (project root): python Tools/Report117/py/splice.py <lines dir (Report117Probe.Lines)> <grid file (Report117Grid, J)>
import json, math, os, sys, subprocess
sys.path.insert(0, 'Tools/Report115/py'); sys.path.insert(0, 'Tools/Report116/py')
from leviathan import stations, at, project, piece, heading, offset, blend, Line, flat, part_list, ramp
from plan import plan, smooth
LINES, GRID = sys.argv[1], sys.argv[2]
LANE = 1.5
P3 = 'Phase 3 - Race Systems'; LAKE = 'CR081 mountain exploration/Lake shore connection'; KYLE = 'Kyle descending driveway'
log = []
def say(*a):
    s = ' '.join(str(x) for x in a); print(s); log.append(s)
def old(name):
    raw = subprocess.run(['git', 'show', 'f05957ef:Assets/Resources/Leviathan/%s.json' % name], capture_output=True, text=True, encoding='utf-8').stdout
    R = json.loads(raw); P = [tuple(R['pts'][i:i + 3]) for i in range(0, len(R['pts']), 3)]
    return R, P
def lines(scene):
    out = {}
    for l in json.load(open('%s/lines-%s.json' % (LINES, scene))):
        p = l['pts']; out[l['path']] = [tuple(p[i:i + 3]) for i in range(0, len(p), 3)]
    return out
def idx_at(P, S, s): return min(range(len(P)), key=lambda i: abs(S[i] - s))
def proj_win(P, S, q, lo, hi):
    """station of q on the closed line P between stations lo and hi"""
    return project(P, q, True, lo, hi)
def ground_grid():
    cells = {}
    for ln in open(GRID):
        x, z, y, ny, b, surf, solid = ln.split(); cells[(int(round(float(x))), int(round(float(z))))] = float(y)
    return cells
GR = ground_grid()
def gy(x, z):
    x0, z0 = math.floor(x), math.floor(z); fx, fz = x - x0, z - z0
    h = lambda a, b: GR.get((a, b), float('nan'))
    return (h(x0, z0) * (1 - fx) + h(x0 + 1, z0) * fx) * (1 - fz) + (h(x0, z0 + 1) * (1 - fx) + h(x0 + 1, z0 + 1) * fx) * fz
def resample(p, step=2.0):
    s = stations(p); n = max(1, int(round(s[-1] / step))); return [at(p, s, s[-1] * k / n) for k in range(n + 1)]

# the link over the bank behind Kyle's garage: the planner's line (16-20 % at most), smoothed, every 2 m, on the ground
# (the planner's line, Report117Grid at 1 m, maxup .2: north of the garage's trees, then east up the bank where it is lowest; its corners
# rounded by hand into one S: control points on the planner's line)
CTRL = [(505.0, -21.4), (508.6, -20.4), (510.6, -17.2), (511.4, -12.6), (514.4, -9.6), (519.0, -8.8), (523.0, -9.0), (527.0, -9.0)]
def catmull(c, n=12):
    e = [c[0]] + c + [c[-1]]; out = []
    for k in range(1, len(e) - 2):
        for t in range(n):
            t /= n; out.append(tuple(.5 * (2 * e[k][d] + (-e[k - 1][d] + e[k + 1][d]) * t + (2 * e[k - 1][d] - 5 * e[k][d] + 4 * e[k + 1][d] - e[k + 2][d]) * t * t + (-e[k - 1][d] + 3 * e[k][d] - 3 * e[k + 1][d] + e[k + 2][d]) * t ** 3) for d in range(2)))
    return out + [c[-1]]
LINK = resample([(x, 0.0, z) for x, z in catmull(CTRL)], 2.0); LINK = [(q[0], gy(q[0], q[2]), q[2]) for q in LINK]
say('link: %d points, %.1f m, %.1f -> %.1f m high, steepest %.0f %% over 4 m' % (len(LINK), stations(LINK)[-1], LINK[0][1], LINK[-1][1],
    100 * max((LINK[k + 2][1] - LINK[k][1]) / max(.1, flat(LINK[k + 2], LINK[k])) for k in range(len(LINK) - 2))))

def remap(R, Pold, Sold, P, S, windows):
    """moves everything held as a station on the old line to the new one: by the position of the old point, projected within a window
    around the old station plus the length change before it (windows: [(old station from, shift)])"""
    def shift(s):
        d = 0
        for a, sh in windows:
            if s >= a: d = sh
        return d
    def m(s, w=90):
        q = at(Pold, Sold, s, True); e = s + shift(s); st, dd = proj_win(P, S, q, e - w, e + w)
        return st
    return m

def finish(name, R, Pold, line, extra_gate, gate_after, branch_fix=None):
    P, tags, synth = line.points(); S = stations(P, True); Sold = stations(Pold, True)
    say('  %s: %d points, lap %.1f m (0.116: %.1f m)' % (name, len(P), S[-1], Sold[-1]))
    # the station shift after each splice: the new station of an old point past it, by position (a point well past the splice)
    windows = []
    for a, b in sorted(R['_splices']):
        q = at(Pold, Sold, b + 30, True); best = min(range(len(P)), key=lambda i: math.dist(P[i], q)); windows.append((a, S[best] - (b + 30)))
    say('  shifts after the splices:', [(round(a), round(d, 1)) for a, d in windows])
    m = remap(R, Pold, Sold, P, S, windows)
    # gates: by position on the new line (the driveway gate inserted)
    gates = [dict(g) for g in R['gates']]
    for g in gates: g['station'] = m(g['station'])
    eg = extra_gate; k = gate_after + 1
    eg['station'] = proj_win(P, S, eg['pos'], eg['_lo'], eg['_hi'])[0]; del eg['_lo'], eg['_hi']
    gates.insert(k, eg)
    prefix = 'Leviathan Reverse CP ' if R['reverse'] else 'Leviathan CP '
    for i, g in enumerate(gates):
        if i == 0: continue
        label = g['name'].split('(', 1)[1] if '(' in g['name'] else None
        g['name'] = prefix + str(i) + (' (' + label if label else '')
    for i in range(1, len(gates) - 1): assert gates[i]['station'] < gates[i + 1]['station'], ('gates out of order', i, gates[i]['name'], gates[i]['station'], gates[i + 1]['station'])
    for i, g in enumerate(gates): say('    gate %2d %-46s s %7.1f%s' % (i, g['name'], g['station'], ' mandatory' if g['mandatory'] else ''))
    R['gates'] = gates
    for b in R['branches']:
        b['entryRoad'] = m(b['entryRoad']); b['exitRoad'] = m(b['exitRoad'])
        b['bypassedGates'] = [x + (1 if x >= k else 0) for x in b['bypassedGates']]
    if branch_fix: branch_fix(R, P, S)
    for b in R['branches']: say('    shortcut %-24s entry %7.1f exit %7.1f bypasses %s' % (b['title'], b['entryRoad'], b['exitRoad'], b['bypassedGates']))
    for f in R['flights']:
        f['approachStation'] = m(f['approachStation']); f['endStation'] = m(f['endStation'])
        say('    flight %-28s %7.1f-%7.1f' % (f['name'], f['approachStation'], f['endStation']))
    R['mountain'] = [m(x) for x in R['mountain']]
    if R['bypass'][1] > 0: R['bypass'] = [m(x) for x in R['bypass']]
    # narrow: the driveway, the link and the joins at the street and the lake path (both passes)
    nar = []; cur = None
    for i, t in enumerate(tags):
        if t in ('driveway', 'link', 'J-drive', 'drive-link', 'link-drive', 'drive-J', 'drive-street'):
            if cur is None: cur = [S[i], S[i]]
            cur[1] = S[i]
        elif cur is not None and S[i] - cur[1] > 30: nar.append(cur); cur = None
    if cur: nar.append(cur)
    R['narrow'] = [v for a, b in nar for v in (round(a - 15, 1), round(b + 15, 1))]
    say('    narrow (driveway and link):', R['narrow'], ' mountain:', [round(x) for x in R['mountain']], ' bypass:', [round(x) for x in R['bypass']])
    # geometry stations and part names: an old point keeps its own (the points are copied as they are); a new point takes its neighbour's
    key = lambda q: (round(q[0], 2), round(q[1], 2), round(q[2], 2))
    olds = {key(q): i for i, q in enumerate(Pold)}
    def oldpart(i):
        so = Sold[i]
        for p in R['parts']:
            if p['from'] - 1 <= so <= p['to'] + 1: return p['name']
        return 'old'
    geo_old = R['geometryStations']; geo = []; names = []
    for i, q in enumerate(P):
        j = olds.get(key(q))
        geo.append(geo_old[j] if j is not None else (geo[-1] if geo else geo_old[0]))
        names.append(oldpart(j) if j is not None else tags[i])
    R['geometryStations'] = [round(g, 2) for g in geo]
    R['parts'] = [{'name': t, 'from': round(a, 1), 'to': round(b, 1)} for t, a, b in part_list(names, S)]
    R['pts'] = [round(v, 3) for q in P for v in q]; R['seat'] = []; R['length'] = S[-1]
    del R['_splices']
    return R, P, S

def forward():
    R, Pold = old('LeviathanForward'); Sold = stations(Pold, True); L = lines('MountainLoop')
    street, lake, kyle = L[P3], L[LAKE], L[KYLE]; Ss, Ls = stations(street, True), stations(lake)
    A, B, C, D = 612, 738, 2740, 2849  # the outbound splice (street at J .. the south shore), the return splice (south shore .. street south of J)
    iA, iB, iC, iD = (idx_at(Pold, Sold, x) for x in (A, B, C, D))
    say('Forward: splices %s' % [(x, tuple(round(v) for v in Pold[i])) for x, i in zip((A, B, C, D), (iA, iB, iC, iD))])
    line = Line(); line.add('old', Pold[:iA + 1])
    s0, _ = project(street, Pold[iA], True); s1, _ = project(street, (473.6, 85.0, -14.0), True)
    line.add('street-J', piece(street, s0, s1, True)[1:])
    drive = piece(kyle, 0, stations(kyle)[-1]); line.join('J-drive', drive, 6, 7, 'driveway')
    line.join('drive-link', LINK, 4, 3, 'link')
    # the west side south: its centreline from the link to the stem's end, the outbound lane 1.5 m to its right (west)
    w0, _ = project(lake, (525.5, 83, -8)); w1, _ = project(lake, (535.5, 81, -71))
    west = offset(piece(lake, w0, w1), lambda i, s: LANE * ramp(s, 4, 14))
    line.join('link-west', west, 3, 3, 'west-side-out', True)
    line.join('west-shore', Pold[iB:iC + 1], 6, 10, 'old-b')
    back = list(reversed(piece(lake, w0, w1))); back = offset(back, lambda i, s: LANE * (1 - ramp(s, stations(back)[-1] - 14, stations(back)[-1] - 4)))
    line.join('shore-west', back, 10, 6, 'west-side-back', True)
    line.join('west-link', list(reversed(LINK)), 3, 3, 'link')
    line.join('link-drive', list(reversed(drive)), 3, 4, 'driveway')
    t0, _ = project(street, (474.0, 85.4, -24.0), True); t1, _ = project(street, Pold[iD], True)
    line.join('drive-street', piece(street, t0, t1, True), 7, 6, 'street-J')
    line.add('old', Pold[iD + 1:])
    R['_splices'] = [(A, B), (C, D)]
    gate = {'name': '(Kyle\'s driveway)', 'pos': [493.0, 0, -21.7], 'rot': [0.0, 92.0, 0.0], 'halfWidth': 5.0, 'halfHeight': 3.0, 'upperHeight': 24.0, 'reuse': '', 'mandatory': False, '_lo': 600, '_hi': 700}
    gate['pos'][1] = gy(493.0, -21.7) + 1.5
    R, P, S = finish('Forward', R, Pold, line, gate, 2)
    R['id'] = 'leviathan-forward-v2'
    return R

def reverse():
    R, Pold = old('LeviathanReverse'); Sold = stations(Pold, True); L = lines('MountainLoopReverse')
    street, lake, kyle = L[P3], L[LAKE], L[KYLE]
    def near_s(q, lo, hi): return proj_win(Pold, Sold, q, lo, hi)[0]
    E = near_s((499.0, 88, -96.0), 3930, 3990); F = near_s((527.5, 83, 2.0), 4100, 4160)
    G = near_s((526.6, 83, 2.0), 6340, 6400); H = near_s((473.2, 82, -2.0), 6520, 6560)
    iE, iF, iG, iH = (idx_at(Pold, Sold, x) for x in (E, F, G, H))
    say('Reverse: splices %s' % [(round(x), tuple(round(v) for v in Pold[i])) for x, i in zip((E, F, G, H), (iE, iF, iG, iH))])
    line = Line(); line.add('old', Pold[:iE + 1])
    # the street north from the 0.116 hairpin to J (the Street Loop's centreline, driven north: its stations go south)
    s0, _ = project(street, Pold[iE], True); s1, _ = project(street, (473.8, 85.6, -27.0), True)
    north = list(reversed(piece(street, s1, s0, True)))
    line.join('street-J', north, 4, 4, 'street-J')
    drive = piece(kyle, 0, stations(kyle)[-1]); line.join('J-drive', drive, 7, 7, 'driveway')
    line.join('drive-link', LINK, 4, 3, 'link')
    line.join('link-west', Pold[iF:iG + 1], 3, 6, 'old-b')
    line.join('west-link', list(reversed(LINK)), 6, 3, 'link')
    line.join('link-drive', list(reversed(drive)), 3, 4, 'driveway')
    t0, _ = project(street, (474.0, 85.4, -24.0), True); t1, _ = project(street, Pold[iH], True)
    up = list(reversed(piece(street, t1, t0, True)))
    line.join('drive-street', up, 7, 4, 'street-J')
    line.add('old', Pold[iH + 1:])
    R['_splices'] = [(E, F), (G, H)]
    gate = {'name': '(Kyle\'s driveway)', 'pos': [493.0, 0, -21.7], 'rot': [0.0, 92.0, 0.0], 'halfWidth': 5.0, 'halfHeight': 3.0, 'upperHeight': 24.0, 'reuse': '', 'mandatory': False, '_lo': 3950, '_hi': 4150}
    gate['pos'][1] = gy(493.0, -21.7) + 1.5
    def rocky(R, P, S):
        # Rocky Way Cut lands on the west side as before; it now runs on north along it (the outbound lane) to the course coming up from the driveway
        b = next(x for x in R['branches'] if x['title'] == 'Rocky Way Cut'); bp = [tuple(b['pts'][i:i + 3]) for i in range(0, len(b['pts']), 3)]
        ss = stations(lake); w0, _ = project(lake, bp[-1]); w1, _ = project(lake, (526.2, 83, -13.0))
        on = offset(list(reversed(piece(lake, w1, w0))), lambda i, s: LANE)
        g = next(x for x in R['gates'] if 'driveway' in x['name']); lo = g['station']
        j = next(i for i in range(len(P)) if S[i] > lo + 20 and P[i][2] > -2.0)  # the main line on the west side, just past the turn north
        tail = [P[k] for k in range(j, j + 8)]
        ext = bp + on[1:]; A2, mid, B2 = blend(ext, tail, 6, 0)
        pts = A2 + mid + B2
        b['pts'] = [round(v, 3) for q in pts for v in q]; b['aiPts'] = list(b['pts'])
        b['exitRoad'] = S[j + 7]
        b['bypassedGates'] = [i for i, x in enumerate(R['gates']) if b['entryRoad'] < x['station'] < b['exitRoad']]
        say('    Rocky Way Cut now %.0f m (0.116: %.0f m), on along the west side to s %.0f' % (stations(pts)[-1], stations(bp)[-1], b['exitRoad']))
    R, P, S = finish('Reverse', R, Pold, line, gate, 13, rocky)
    R['id'] = 'leviathan-reverse-v3'
    return R

if __name__ == '__main__':
    for name, fn in (('LeviathanForward', forward), ('LeviathanReverse', reverse)):
        R = fn(); open('Assets/Resources/Leviathan/%s.json' % name, 'w', encoding='utf-8', newline='\n').write(json.dumps(R, separators=(',', ':')))
    os.makedirs('Docs/Report117', exist_ok=True); open('Docs/Report117/splice.txt', 'w', encoding='utf-8').write('\n'.join(log) + '\n')
