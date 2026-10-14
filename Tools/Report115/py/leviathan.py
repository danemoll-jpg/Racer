# 0.115 Leviathan: composes the two course lines from roads that already exist in the Mountain Loop scenes (Docs/Report115/Probe/scene-*.json,
# dumped read-only by Tools/Report115/Report115Dump.cs). No new roads: the Street Loop road (identical in every scene), the Lake shore connection,
# the old Mountain racing line (CR110) / exploration trail where they join, the scene's own Mountain line (CR117), Climbing Ridge Cut (forward) /
# Downhill Ridge Cut (reverse) as the main road. Joins are short blends between those centrelines; the parts driven both ways (the 65 m stem at
# J, and the lake's south shore: the lake's north side can't be reached from the mountain in either Mountain scene) are split into a lane per
# direction (3 m right of the centreline).
# Writes Assets/Resources/Leviathan/Leviathan{Forward,Reverse}.json (read at runtime by LeviathanCourse.cs) and Docs/Report115/leviathan.txt.
# Run from the project root: python Tools/Report115/py/leviathan.py   (then Tools/Report115/Report115Snap.cs seats the blended points on the ground)
import json, math, os, sys

PROBE = 'Docs/Report115/Probe'
OUT = 'Assets/Resources/Leviathan'
P3 = 'Phase 3 - Race Systems'; LAKE = 'CR081 mountain exploration/Lake shore connection'
CR117 = 'CR117 main flights and alternatives/CR117 mandatory two-flight racing line'; CR110 = 'CR110 mountain race geometry/Mountain racing line'
TRAIL = 'CR081 mountain exploration/Mountain return trails / exploration only'
LANE = 1.5      # each direction's line, right of the shared centreline, where a part is driven both ways and wide enough (not the stem)
STEM = 95.0     # the stem: lake s 0-95 (and its last 95 m), driven both ways on its centreline (trees 3 m off it); the rivals keep to the middle
report = []
def say(*a):
    s = ' '.join(str(x) for x in a); print(s); report.append(s)

def V(v): return (v['x'], v['y'], v['z'])
def scene(sn):
    d = json.load(open(f'{PROBE}/scene-{sn}.json', encoding='utf-8'))
    o = {'roads': {}, 'branches': {}, 'gates': {}, 'flights': [], 'director': None, 'rd': None}
    for e in d:
        t = e['type']
        if t == 'Director': o['director'] = e; continue
        if t == 'GateTree': continue
        m = e['data']['MonoBehaviour']
        if t == 'RaceRoad': o['roads'][e['path']] = dict(m, points=[V(p) for p in m['points']])
        elif t == 'WoodlandRoute': o['branches'][m['title']] = dict(m, points=[V(p) for p in m['points']], aiPoints=[V(p) for p in m.get('aiPoints', [])], path=e['path'])
        elif t == 'RaceGate': o['gates'][e['path']] = dict(m, pos=tuple(e['pos']), rot=tuple(e['rot']), path=e['path'])
        elif t == 'MountainFlights': o['flights'] = m['flights']
        elif t == 'RaceDirector': o['rd'] = m
    o['gate_order'] = [o['gates'][g] for g in o['director']['gates']]
    return o

def dist(a, b): return math.dist(a, b)
def flat(a, b): return math.hypot(a[0] - b[0], a[2] - b[2])
def stations(p, closed=False):
    s = [0.0]
    for i in range(1, len(p)): s.append(s[-1] + dist(p[i], p[i - 1]))
    if closed: s.append(s[-1] + dist(p[0], p[-1]))
    return s
def at(p, s_list, d, closed=False):
    q = p + [p[0]] if closed else p
    L = s_list[-1]; d = d % L if closed else max(0.0, min(L, d))
    lo, hi = 0, len(q) - 1
    while lo + 1 < hi:
        m = (lo + hi) // 2
        if s_list[m] <= d: lo = m
        else: hi = m
    t = 0 if s_list[hi] == s_list[lo] else (d - s_list[lo]) / (s_list[hi] - s_list[lo])
    return tuple(q[lo][k] + (q[hi][k] - q[lo][k]) * t for k in range(3))
def project(p, q, closed=False, lo_s=None, hi_s=None):
    """3D nearest point station (RaceRoad.Project); optional station window"""
    s = stations(p, closed); n = len(p); best = (1e18, 0)
    for i in range(n if closed else n - 1):
        a = p[i]; b = p[(i + 1) % n]; v = [b[k] - a[k] for k in range(3)]; vv = sum(x * x for x in v) or 1e-9
        t = max(0, min(1, sum((q[k] - a[k]) * v[k] for k in range(3)) / vv)); c = [a[k] + v[k] * t for k in range(3)]
        st = s[i] + t * (s[i + 1] - s[i])
        if lo_s is not None and not (lo_s <= st <= hi_s): continue
        d2 = sum((q[k] - c[k]) ** 2 for k in range(3))
        if d2 < best[0]: best = (d2, st)
    return best[1], math.sqrt(best[0])
def piece(p, a, b, closed=False, step=2.0):
    """points of polyline p from station a to b (a<b; on a closed road b may pass the end), every ~step m, endpoints exact"""
    s = stations(p, closed); n = max(1, int(round(abs(b - a) / step)))
    return [at(p, s, a + (b - a) * k / n, closed) for k in range(n + 1)]
def heading(p, i):
    a = p[max(0, i - 1)]; b = p[min(len(p) - 1, i + 1)]; d = math.hypot(b[0] - a[0], b[2] - a[2]) or 1
    return ((b[0] - a[0]) / d, (b[2] - a[2]) / d)
def offset(p, fn):
    """lateral offset to the right of travel (Unity: right = (fz, -fx)); fn(i, s) -> metres"""
    s = stations(p); out = []
    for i, q in enumerate(p):
        fx, fz = heading(p, i); d = fn(i, s[i]); out.append((q[0] + fz * d, q[1], q[2] - fx * d))
    return out
def corridor(name):
    """a corridor from tools/widest.py: smoothed centreline points and, per point, its clearance (m to the nearest edge)"""
    c = json.load(open(f'Docs/Report115/{name}.json')); pts = [tuple(q) for q in c['points']]; cl = list(c['clearance'])
    cl = (cl + [cl[-1]] * len(pts))[:len(pts)]
    # each lane 1.5 m off the centre where there is room (a car is 2 m wide; 1 m kept to the edge), less where the corridor is tight
    lane = [max(0.0, min(LANE, min(cl[max(0, i - 3):i + 4]) - 2.5)) for i in range(len(pts))]
    return pts, lane
def ramp(x, a, b):  # 0 at a, 1 at b, smooth
    if b == a: return 1.0 if x >= b else 0.0
    t = max(0.0, min(1.0, (x - a) / (b - a))); return t * t * (3 - 2 * t)
def blend(A, B, cut_a, cut_b, step=2.0):
    """A then B: A cut back by cut_a m, B starts cut_b m in, joined by a cubic over the gap (tangents of A's and B's ends)"""
    sa = stations(A); sb = stations(B)
    A2 = piece(A, 0, sa[-1] - cut_a); B2 = piece(B, cut_b, sb[-1])
    p0 = A2[-1]; p3 = B2[0]; ta = heading(A2, len(A2) - 1); tb = heading(B2, 0)
    g = flat(p0, p3); k = g * .42
    p1 = (p0[0] + ta[0] * k, p0[1], p0[2] + ta[1] * k); p2 = (p3[0] - tb[0] * k, p3[1], p3[2] - tb[1] * k)
    n = max(2, int(round(g * 1.25 / step))); mid = []
    for j in range(1, n):
        t = j / n; u = 1 - t
        x = u ** 3 * p0[0] + 3 * u * u * t * p1[0] + 3 * u * t * t * p2[0] + t ** 3 * p3[0]
        z = u ** 3 * p0[2] + 3 * u * u * t * p1[2] + 3 * u * t * t * p2[2] + t ** 3 * p3[2]
        y = p0[1] + (p3[1] - p0[1]) * (t * t * (3 - 2 * t))
        mid.append((x, y, z))
    return A2, mid, B2

class Line:
    """the course centreline as tagged parts: (name, points, synthetic) -- synthetic points are seated on the ground by Report115Snap"""
    def __init__(self): self.parts = []
    def add(self, name, pts, synth=False):
        if self.parts and self.parts[-1][1] and pts and dist(self.parts[-1][1][-1], pts[0]) < .05: pts = pts[1:]
        if pts: self.parts.append((name, list(pts), synth))
    def join(self, name, nxt, cut_a, cut_b, nxt_name, nxt_synth=False):
        """blend the last part into nxt"""
        lname, last, ls = self.parts.pop()
        A2, mid, B2 = blend(last, nxt, cut_a, cut_b)
        self.parts.append((lname, A2, ls)); self.parts.append((name, mid, True)); self.parts.append((nxt_name, B2, nxt_synth))
    def join_keep(self, name, nxt, cut_a, cut_b, nxt_name):
        """as join, but nxt's own points are kept as they are after cut_b (a dense line with a flight drawn in it)"""
        lname, last, ls = self.parts.pop(); ns = stations(nxt); k = next(i for i in range(len(nxt)) if ns[i] >= cut_b)
        A2, mid, _ = blend(last, nxt, cut_a, ns[k])
        self.parts.append((lname, A2, ls)); self.parts.append((name, mid, True)); self.parts.append((nxt_name, nxt[k:], False))
    def points(self):
        pts = []; tags = []; synth = []
        for name, p, s in self.parts:
            for q in p:
                if pts and dist(pts[-1], q) < .3: continue
                pts.append(q); tags.append(name); synth.append(s)
        if dist(pts[-1], pts[0]) < .3: pts.pop(); tags.pop(); synth.pop()
        return pts, tags, synth

def spans(tags, s, want):
    """station ranges of the parts whose name starts with want"""
    out = []; cur = None
    for i, t in enumerate(tags):
        if t.startswith(want):
            if cur is None: cur = [s[i], s[i]]
            cur[1] = s[i]
        elif cur is not None: out.append(cur); cur = None
    if cur is not None: out.append(cur)
    return out

def gate_entry(g, name, mandatory=False, reuse=None):
    return {'name': name, 'pos': list(g['pos']), 'rot': list(g['rot']), 'halfWidth': g['halfWidth'], 'halfHeight': g['halfHeight'],
            'upperHeight': g['upperHeight'], 'reuse': reuse or '', 'mandatory': mandatory}
def new_gate(pts, s, st, name, hw=7.0):
    """a gate across the line at station st (Mountain-style opening; seated by Report115Snap)"""
    p = at(pts, s, st, True); q = at(pts, s, st + 3, True); yaw = math.degrees(math.atan2(q[0] - p[0], q[2] - p[2])) % 360
    return {'name': name, 'pos': [p[0], p[1] + 1.5, p[2]], 'rot': [0.0, yaw, 0.0], 'halfWidth': hw, 'halfHeight': 5.0, 'upperHeight': 30.0, 'reuse': '', 'mandatory': True, 'seat': True}

def street_branches(sl, street, cut0, cut1, offset_after, gate_shift):
    """the Street Loop's own shortcuts on the Leviathan line: entry / exit stations moved past the spur, bypassed gates renumbered"""
    out = []
    for title, b in sl['branches'].items():
        e = b['entryRoad'] + (offset_after if b['entryRoad'] > cut0 else 0); x = b['exitRoad'] + (offset_after if b['exitRoad'] > cut0 else 0)
        out.append({k: b[k] for k in ('title', 'halfWidth', 'recommendedSpeed', 'aiValidated', 'undergroundStart', 'undergroundEnd', 'entryHeightBelow', 'entryHeightAbove', 'entrySpeed', 'entrySpeedDistance', 'entryInset', 'entryMargin')}
                   | {'points': [list(q) for q in b['points']], 'aiPoints': [list(q) for q in b['aiPoints']], 'entryRoad': e, 'exitRoad': x,
                      'bypassedGates': [g + gate_shift(g) for g in b['bypassedGates']], 'from': b['path']})
    return out

def flight_entries(sc, line_pts, line_s, cr, names, lo, hi):
    crs = stations(cr, True); out = []
    for f in sc['flights']:
        if f['name'] not in names: continue
        a, da = project(line_pts, at(cr, crs, f['approachStation'], True), True, lo, hi); b, db = project(line_pts, at(cr, crs, f['endStation'], True), True, lo, hi)
        say(f"  flight {f['name']}: CR117 {f['approachStation']:.1f}-{f['endStation']:.1f} -> Leviathan {a:.1f}-{b:.1f} (projection {da:.2f} / {db:.2f} m)")
        out.append(dict(f, approachStation=a, endStation=b))
    return out

def build_forward():
    sc = scene('MountainLoop'); sl = scene('StreetLoopGreybox')
    street = sc['roads'][P3]['points']; lake = sc['roads'][LAKE]['points']; cr = sc['roads'][CR117]['points']; c110 = sc['roads'][CR110]['points']
    crc = sc['branches']['Climbing Ridge Cut']
    S = stations(street, True); Ls = stations(lake); Cs = stations(cr, True)
    J = lake[0]; sJ, dJ = project(street, J, True); say(f'Forward: J {J} on the street at s {sJ:.1f} ({dJ:.2f} m)')
    # 1. the Street Loop Forward from its first point to J
    line = Line(); line.add('street-a', piece(street, 0, sJ, True))
    # 2. the stem (its centreline, both ways) and the lake path's south shore east (its own lane: the way back uses it too), to where CR110 leaves
    #    it. In this scene a creek gully crosses that shore (Free Roam has flat ground there): Dan (2026-10-14) chose to fill it for the Leviathan
    #    race only, a ground patch copied from Free Roam's that LeviathanCourse adds at load (Resources/Leviathan/LeviathanForwardGullyFill).
    l_cr110, _ = project(lake, (690.1, 81.0, -95.0)); say(f'  lake meets CR110 at lake s {l_cr110:.1f}')
    out = offset(piece(lake, 0, l_cr110), lambda i, s: LANE * ramp(s, STEM, STEM + 14))
    line.join('J-out', out, 14, 10, 'stem-out+south-shore', True)
    # 3. CR110 from the lake to Climbing Ridge Cut (an existing mountain driving surface), then Climbing Ridge Cut (the main road here)
    c0, d0 = project(c110, (690.1, 81.0, -95.0)); c1, d1 = project(c110, crc['points'][0])
    say(f'  CR110 from s {c0:.1f} ({d0:.2f} m) to s {c1:.1f} ({d1:.2f} m: Climbing Ridge Cut start)')
    seg110 = piece(c110, c0, c1) if c1 > c0 else list(reversed(piece(c110, c1, c0)))
    line.join('lake-CR110', seg110, 16, 12, 'CR110')  # a wider arc onto CR110 (rivals ran off its outside at street speed)
    line.add('Climbing Ridge Cut', crc['points'])
    # 4. CR117 from Climbing Ridge Cut's exit through CP3, CP4, CP5, the summit flight and its landing, to CP6
    x0, dx = project(cr, crc['points'][-1], True); say(f"  Climbing Ridge Cut ends on CR117 at s {x0:.1f} ({dx:.2f} m; its exitRoad {crc['exitRoad']:.1f})")
    cp6 = sc['gates']['CR117 selected-direction guidance/CR117 CP 6']; s6, _ = project(cr, cp6['pos'], True)
    line.add('CR117', piece(cr, x0, s6 + 12, True))
    # 5. off CR117 just past CP6 down onto the lake's east side (south), the south shore west (its other lane), the stem back (its centreline) and
    #    the hairpin onto the Street Loop. (The lake's north side can't be reached from the mountain in this scene: CR117's raised descent walls
    #    off its north-east corner.)
    l_east, de = project(lake, (727.0, 79.5, -18.0)); say(f'  lake east side reached at lake s {l_east:.1f} ({de:.2f} m)')
    back = list(reversed(piece(lake, 0, l_east))); L0 = stations(back)[-1]; shared = l_east - (l_cr110 + 4)
    back = offset(back, lambda i, s: LANE * ramp(s, shared - 14, shared) * (1 - ramp(s, L0 - STEM - 14, L0 - STEM)))
    line.join('CP6-lake', back, 10, 6, 'east-side+south-shore+stem-back', True)
    # the hairpin: the return lane leaves the stem 30 m before J and turns inside the Y onto the Street Loop 30 m past J
    rest = piece(street, sJ, S[-1], True)
    line.join('J-hairpin', rest, 30, 30, 'street-b')
    pts, tags, synth = line.points(); s = stations(pts, True)
    say(f'  Leviathan Forward: {len(pts)} points, lap {s[-1]:.1f} m')
    return sc, sl, street, sJ, pts, tags, synth, s

def build_reverse():
    sc = scene('MountainLoopReverse'); sl = scene('StreetLoopReverse')
    rev = sl['roads']['Reverse course authoring/Reverse main route']; street = rev['points']; geo = rev['geometryStations']
    lake = sc['roads'][LAKE]['points']; cr = sc['roads'][CR117]['points']; trail = sc['roads'][TRAIL]['points']; drc = sc['branches']['Downhill Ridge Cut']
    S = stations(street, True); Ls = stations(lake); Cs = stations(cr, True)
    J = lake[0]; sJ, dJ = project(street, J, True); say(f'Reverse: J {J} on the Street Loop Reverse at s {sJ:.1f} ({dJ:.2f} m)')
    line = Line(); line.add('street-a', piece(street, 0, sJ, True))
    # in: the stem, the lake's west side north and its north shore east (the lake shore line), then the strip between the water and the hill at
    # the lake's north-east corner (Docs/Report115/corridor-ne-reverse.json: the widest drivable path, 10 m+ wide) to the CR117 start stretch.
    # The way back uses the same: in this scene the lake connects to the mountain there only (its south-east corner is walled by Downhill Ridge
    # Cut's jump barrier and deck embankments; the lake shore line's own north-east corner runs under a hill raised in this scene).
    corr, lanes = corridor('corridor-ne-reverse')
    l_n, dn = project(lake, corr[0]); say(f'  north shore left at lake s {l_n:.1f} ({dn:.2f} m) for the north-east strip ({len(corr)} points)')
    inn = list(reversed(piece(lake, l_n, Ls[-1]))); Li = stations(inn)[-1]
    # the stem's south-west side is open, its north-east side a row of trees: the rivals' way in keeps 1.5 m to the south-west there
    inn = offset(inn, lambda i, s: LANE * (ramp(s, 4, 14) * (1 - ramp(s, 55, 70)) + ramp(s, STEM, STEM + 14)))
    line.join('J-hairpin', inn, 42, 42, 'stem+west-side+north-shore-in', True)  # wider than forward's: this lane is the stem's south-west side
    line.join('shore-strip', offset(corr, lambda i, s: lanes[i]), 6, 4, 'north-east-strip-in', True)
    e0, de = project(cr, corr[-1], True); d_in, ddi = project(cr, drc['points'][0], True)
    say(f"  CR117 joined at s {e0:.1f} ({de:.2f} m); Downhill Ridge Cut starts on CR117 at s {d_in:.1f} ({ddi:.2f} m; entryRoad {drc['entryRoad']:.1f})")
    # CR117 (reverse) from its start stretch up to CP1, CP2, the South Face summit flight, its landing and CP3, then Downhill Ridge Cut (the main road
    # here) to its end on the start stretch
    line.join('strip-CR117', piece(cr, e0, d_in if d_in > e0 else d_in + Cs[-1], True), 6, 10, 'CR117')
    # onto Downhill Ridge Cut with a blend (its first 12 m lie over the main road; joined point to point it left a 60-degree kink)
    line.join_keep('CR117-DRC', drc['points'], 4, 16, 'Downhill Ridge Cut')
    d_out, ddo = project(cr, drc['points'][-1], True); x_out, _ = project(cr, (724.4, 79.0, -4.0), True)
    say(f'  Downhill Ridge Cut ends on CR117 at s {d_out:.1f} ({ddo:.2f} m); the start stretch is left at s {x_out:.1f}')
    line.add('CR117-start-stretch', piece(cr, d_out, x_out, True))
    # out: west onto the strip (its other lane), the north shore and the west side (their other lane), the stem, onto the Street Loop Reverse north
    k = min(range(len(corr)), key=lambda i: math.hypot(corr[i][0] - 710.0, corr[i][2] - 4.0))
    back = list(reversed(corr[:k + 1]))
    back_lanes = list(reversed(lanes[:k + 1])); line.join('stretch-strip', offset(back, lambda i, s: back_lanes[i]), 6, 2, 'north-east-strip-out', True)
    out = piece(lake, l_n, Ls[-1]); Lo = stations(out)[-1]
    out = offset(out, lambda i, s: LANE * (1 - ramp(s, Lo - STEM - 14, Lo - STEM)))
    line.join('strip-shore', out, 4, 6, 'north-shore+west-side+stem-out', True)
    rest = piece(street, sJ, S[-1], True)
    line.join('J-out', rest, 14, 10, 'street-b')
    pts, tags, synth = line.points(); s = stations(pts, True)
    say(f'  Leviathan Reverse: {len(pts)} points, lap {s[-1]:.1f} m')
    return sc, sl, street, sJ, pts, tags, synth, s, geo

def geometry_stations(pts, tags, street, street_geo=None, park=None):
    """per point: its Street Loop (forward) station, for RaceRoad.HighwayBlend; off the street: the station at J (no highway blend)"""
    S = stations(street, True); out = []; last = 0
    for q, t in zip(pts, tags):
        if t.startswith('street'):
            st, _ = project(street, q, True); g = st if street_geo is None else street_geo_at(street, street_geo, S, st)
            last = g
        out.append(last if park is None or t.startswith('street') else park)
    return out
def street_geo_at(street, geo, S, st):
    # the Reverse main route's own geometryStations (forward street stations) interpolated at station st
    lo, hi = 0, len(street)
    while lo + 1 < hi:
        m = (lo + hi) // 2
        if S[m] <= st: lo = m
        else: hi = m
    a = geo[lo]; b = geo[(lo + 1) % len(street)]; L = 4674.11767578125
    d = (b - a + L * .5) % L - L * .5; t = (st - S[lo]) / max(1e-6, S[lo + 1] - S[lo])
    return (a + d * t) % L

def write(name, course):
    os.makedirs(OUT, exist_ok=True)
    with open(f'{OUT}/{name}.json', 'w', encoding='utf-8', newline='\n') as f: json.dump(course, f, separators=(',', ':'))
    say(f'  wrote {OUT}/{name}.json')

def r3(q): return [round(q[0], 3), round(q[1], 3), round(q[2], 3)]
def flat3(pts): return [round(v, 3) for q in pts for v in q]
def runtime(course):
    # the layout JsonUtility reads (no nested arrays): points flat x,y,z,...; parts and mountain spans as objects / flat pairs
    c = dict(course); c['pts'] = flat3(c.pop('points')); c['mountain'] = [v for m in c['mountain'] for v in m]
    c['parts'] = [{'name': t, 'from': a, 'to': b} for t, a, b in c['parts']]
    for b in c['branches']: b['pts'] = flat3(b.pop('points')); b['aiPts'] = flat3(b.pop('aiPoints'))
    return c

def main():
    # ---------------------------------------------------------------- Forward
    sc, sl, street, sJ, pts, tags, synth, s = build_forward()
    L = s[-1]; S = stations(street, True)
    # the spur length: the Leviathan station of any street point past J minus its Street Loop station
    i_b = tags.index('street-b'); sb_lev = s[i_b]; sb_street, _ = project(street, pts[i_b], True); shift = sb_lev - sb_street
    say(f'  street past J: Street Loop s + {shift:.1f} (the spur and the hairpin)')
    gates = []; sg = sl['gate_order']
    for i, g in enumerate(sg[:3]): gates.append(gate_entry(g, 'Leviathan ' + ('START FINISH' if i == 0 else f'CP {i}')))
    spur = [('lake south shore', None), ('Climbing Ridge Cut', None), ('CR117 CP 3', 'CR117 selected-direction guidance/CR117 CP 3'), ('CR117 CP 4', 'CR117 selected-direction guidance/CR117 CP 4'),
            ('CR117 CP 5', 'CR117 selected-direction guidance/CR117 CP 5'), ('CR117 CP 6', 'CR117 selected-direction guidance/CR117 CP 6'), ('lake east side', None)]
    def spur_station(label):
        if label == 'lake south shore': sp = spans(tags, s, 'stem-out+south-shore')[0]; return sp[0] + (sp[1] - sp[0]) * .7
        if label == 'Climbing Ridge Cut': sp = spans(tags, s, 'Climbing Ridge Cut')[0]; return (sp[0] + sp[1]) * .5
        sp = spans(tags, s, 'east-side+south-shore+stem-back')[0]; return sp[0] + 28
    n = 3
    for label, reuse in spur:
        n += 1
        if reuse: g = sc['gates'][reuse]; gates.append(gate_entry(g, f'Leviathan CP {n - 1} ({label})', True, reuse))
        else: gates.append(new_gate(pts, s, spur_station(label), f'Leviathan CP {n - 1} ({label})'))
    for i, g in enumerate(sg[3:], start=3): n += 1; gates.append(gate_entry(g, f'Leviathan CP {n - 1}'))
    for k, g in enumerate(gates):
        st, d = project(pts, g['pos'], True); g['station'] = st; say(f"  gate {k:2d} {g['name']:40s} s {st:7.1f} ({d:.2f} m from the line){' mandatory' if g['mandatory'] else ''}{' reuses '+g['reuse'] if g['reuse'] else ''}")
    assert all(gates[k]['station'] < gates[k + 1]['station'] for k in range(1, len(gates) - 1)), 'gates out of order'
    branches = street_branches(sl, street, sJ, None, shift, lambda g: 0 if g < 3 else len(spur))
    for b in branches:
        e, de = project(pts, b['points'][0], True); x, dx = project(pts, b['points'][-1], True)
        say(f"  shortcut {b['title']}: entry {b['entryRoad']:.1f} exit {b['exitRoad']:.1f} (its ends project to {e:.1f} / {x:.1f}), bypasses {b['bypassedGates']} ({', '.join(gates[g]['name'] for g in b['bypassedGates'])})")
    # the Mountain rules (its 24 m/s approach speed) from 80 m before the turn onto CR110, so rivals arrive at that turn at mountain speed
    m = spans(tags, s, 'lake-CR110')[0][0] - 80, spans(tags, s, 'CP6-lake')[0][1]
    flights = flight_entries(sc, pts, s, sc['roads'][CR117]['points'], ['Homeward Summit Flight'], m[0], m[1])
    bypass = [sl['roads'][P3]['bypassStart'] + shift, sl['roads'][P3]['bypassEnd'] + shift]
    spawn = sl['director']['spawn']; spawnYaw = sl['director']['spawnYaw']
    course = {'version': 1, 'id': 'leviathan-forward-v1', 'title': 'Leviathan - Forward', 'scene': 'MountainLoop', 'reverse': False,
              'points': [r3(q) for q in pts], 'seat': [i for i, f in enumerate(synth) if f], 'parts': [[t, round(a, 1), round(b, 1)] for t, a, b in part_list(tags, s)],
              'geometryStations': [round(g, 2) for g in geometry_stations(pts, tags, street, park=sJ)], 'geometryLength': S[-1], 'bypass': bypass,
              'mountain': [list(m)], 'narrow': narrow_spans(pts, s, sc), 'gates': gates, 'branches': branches, 'flights': flights, 'spawn': spawn, 'spawnYaw': spawnYaw,
              'highwayTraffic': 16, 'length': L, 'fills': ['LeviathanForwardGullyFill']}
    say(f'  mountain span {m[0]:.1f}-{m[1]:.1f}, bypass (trailer lane) {bypass[0]:.1f}-{bypass[1]:.1f}, length {L:.1f} m')
    write('LeviathanForward', runtime(course))
    # ---------------------------------------------------------------- Reverse
    sc, sl, street, sJ, pts, tags, synth, s, geo = build_reverse()
    L = s[-1]; S = stations(street, True)
    i_b = tags.index('street-b'); sb_street, _ = project(street, pts[i_b], True); shift = s[i_b] - sb_street
    sg = sl['gate_order']; gates = []
    # the Street Loop Reverse's gates up to J (CP 1-13), the spur, then CP 14-16
    jg = next(k for k, g in enumerate(sg) if k > 0 and project(street, g['pos'], True)[0] > sJ)
    say(f'  first street gate past J: {sg[jg]["path"]}')
    for i, g in enumerate(sg[:jg]): gates.append(gate_entry(g, 'Leviathan Reverse ' + ('START FINISH' if i == 0 else f'CP {i}')))
    spur = [('lake north shore', None), ('CR117 CP 1', 'CR117 selected-direction guidance/CR117 CP 1'), ('CR117 CP 2', 'CR117 selected-direction guidance/CR117 CP 2'),
            ('CR117 CP 3', 'CR117 selected-direction guidance/CR117 CP 3'), ('Downhill Ridge Cut', None), ('lake north-east strip', None)]
    def spur_station(label):
        if label == 'lake north shore': sp = spans(tags, s, 'stem+west-side+north-shore-in')[0]; return sp[1] - 40
        if label == 'Downhill Ridge Cut': sp = spans(tags, s, 'Downhill Ridge Cut')[0]; return sp[0] + (sp[1] - sp[0]) * .25  # before the drop's run-up (mid-cut is in the air)
        sp = spans(tags, s, 'north-east-strip-out')[0]; return (sp[0] + sp[1]) * .5
    n = jg
    for label, reuse in spur:
        if reuse: g = sc['gates'][reuse]; gates.append(gate_entry(g, f'Leviathan Reverse CP {n} ({label})', True, reuse))
        else: gates.append(new_gate(pts, s, spur_station(label), f'Leviathan Reverse CP {n} ({label})'))
        n += 1
    for g in sg[jg:]: gates.append(gate_entry(g, f'Leviathan Reverse CP {n}')); n += 1
    for k, g in enumerate(gates):
        st, d = project(pts, g['pos'], True); g['station'] = st; say(f"  gate {k:2d} {g['name']:48s} s {st:7.1f} ({d:.2f} m from the line){' mandatory' if g['mandatory'] else ''}{' reuses '+g['reuse'] if g['reuse'] else ''}")
    assert all(gates[k]['station'] < gates[k + 1]['station'] for k in range(1, len(gates) - 1)), 'gates out of order'
    m = spans(tags, s, 'strip-CR117')[0][0], spans(tags, s, 'stretch-strip')[0][1]
    flights = flight_entries(sc, pts, s, sc['roads'][CR117]['points'], ['South Face Summit Flight'], m[0], m[1])
    # Downhill Ridge Cut (the main road here) drops off a deck edge at (740.5, 92.9, -146.4) over a row of rocks to land at (712, 84, -119); its own
    # line is drawn as that ~25 m/s arc. The Mountain Loop's rivals never drove it (it was not AI-validated): without flight data they reach the
    # edge at 11 m/s, drop short and stop at the rocks. As the summit flights: a straight run-up along its axis, take-off at 28 m/s (clears the
    # rocks by ~1 m, lands where the line does).
    dp = sc['branches']['Downhill Ridge Cut']['points']
    def near(x, z): return min(range(len(dp)), key=lambda i: math.hypot(dp[i][0] - x, dp[i][2] - z))
    lip, land = dp[near(740.6, -146.4)], dp[near(712.0, -119.0)]; ax = (land[0] - lip[0], land[2] - lip[2]); n = math.hypot(*ax); ax = (ax[0] / n, ax[1] / n)
    start = (lip[0] - ax[0] * 60, lip[1], lip[2] - ax[1] * 60)
    a0, _ = project(pts, at(dp, stations(dp), max(0, stations(dp)[near(740.6, -146.4)] - 60)), True, m[0], m[1]); a1, _ = project(pts, land, True, m[0], m[1])
    flights.append({'name': 'Downhill Ridge Cut drop', 'start': {'x': start[0], 'y': start[1], 'z': start[2]}, 'lip': {'x': lip[0], 'y': lip[1], 'z': lip[2]},
                    'landingEnd': {'x': land[0], 'y': land[1], 'z': land[2]}, 'forward': {'x': ax[0], 'y': 0.0, 'z': ax[1]}, 'approachStation': a0, 'endStation': a1 + 8,
                    'aiTakeoffSpeed': 28.0, 'aiEntrySpeed': 0.0})
    say(f"  flight Downhill Ridge Cut drop: lip {tuple(round(v, 1) for v in lip)} landing {tuple(round(v, 1) for v in land)} axis {ax[0]:.2f},{ax[1]:.2f}; Leviathan {a0:.1f}-{a1 + 8:.1f}, take-off 28 m/s")
    say('  shortcuts: none (Granite Creek Cut and Laurel Switchbacks run on ground reshaped only in StreetLoopReverse.unity; in MountainLoopReverse.unity it is not there)')
    course = {'version': 1, 'id': 'leviathan-reverse-v1', 'title': 'Leviathan - Reverse', 'scene': 'MountainLoopReverse', 'reverse': True,
              'points': [r3(q) for q in pts], 'seat': [i for i, f in enumerate(synth) if f], 'parts': [[t, round(a, 1), round(b, 1)] for t, a, b in part_list(tags, s)],
              'geometryStations': [round(g, 2) for g in geometry_stations(pts, tags, street, geo, park=street_geo_at(street, geo, S, sJ))], 'geometryLength': 4674.11767578125, 'bypass': [0, 0],
              'mountain': [list(m)], 'narrow': narrow_spans(pts, s, sc), 'gates': gates, 'branches': [], 'flights': flights, 'spawn': sl['director']['spawn'], 'spawnYaw': sl['director']['spawnYaw'],
              'highwayTraffic': 16, 'length': L}
    say(f'  mountain span {m[0]:.1f}-{m[1]:.1f}, length {L:.1f} m')
    write('LeviathanReverse', runtime(course))
    os.makedirs('Docs/Report115', exist_ok=True); open('Docs/Report115/leviathan.txt', 'w', encoding='utf-8').write('\n'.join(report) + '\n')

def narrow_spans(pts, s, sc):
    """stations within 18 m of the stem's centreline (lake s 0-95), both passes and the joins at J"""
    lake = sc['roads'][LAKE]['points']; Ls = stations(lake); stem = piece(lake, 0, STEM, False, 1.0)
    near = [min(flat(q, r) for r in stem) < 9 and abs(q[1] - at(lake, Ls, project(lake, q)[0])[1]) < 6 for q in pts]
    out = []; cur = None
    for i, f in enumerate(near):
        if f and cur is None: cur = [s[i], s[i]]
        elif f: cur[1] = s[i]
        elif cur is not None: out.append(cur); cur = None
    if cur is not None: out.append(cur)
    out = [[a - 15, b + 15] for a, b in out]; say(f'  narrow (stem) spans: {[[round(a), round(b)] for a, b in out]}')
    return [v for p in out for v in p]

def part_list(tags, s):
    out = []; cur = None
    for i, t in enumerate(tags):
        if cur and cur[0] == t: cur[2] = s[i]
        else:
            if cur: out.append(cur)
            cur = [t, s[i], s[i]]
    out.append(cur); return out

if __name__ == '__main__': main()
