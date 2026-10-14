# 0.113 Part C: plans Trickum Rd (Hwy 92 to the Jamerson / S Cherokee Ln junction) and the west end of S Cherokee Ln (the real Jamerson Rd
# piece and Cherokee Ln to just past the meeting hall) "like the real road today", and writes Assets/Resources/Hwy92/TrickumRoadside.json,
# which the game builds at load in every world scene (Racer.Hwy92Roadside with resource Hwy92Roadside.Trickum), as 0.104/0.105 did for Hwy 92.
# Placement only from OpenStreetMap (SourceArt/Reference/Trickum113/osm-trickum.json) and USGS NAIP (naip-trickum.jpg); every name invented.
# Game side from the 0.113 survey (Tools/Report113/Report113Survey.cs: roads, objects in every world scene, the ground grid).
# Mapping: a real point becomes (s along its real road, d across it); the game point is the game road (the Street Loop centre line) at s * K,
# d across (inside the real carriageway scaled to the game's 4.5 m half width, beyond it K per metre). Trickum: real Hwy 92 junction -> game
# Hwy 92 corner, real Jamerson junction -> game junction. S Cherokee Ln (game) = real Jamerson Rd from Trickum + Cherokee Ln, from the junction.
import json, math, random, sys, base64, zlib, os
import numpy as np
import shapely
from shapely.geometry import Polygon, LineString, Point, MultiPolygon, box
from shapely.ops import unary_union
ROOT = 'C:/Users/danmo/Racer'
REF = ROOT + '/SourceArt/Reference/Trickum113/'
SV = os.environ.get('SURVEY', 'C:/Users/danmo/AppData/Local/Temp/report113/survey/')
OUT = ROOT + '/Assets/Resources/Hwy92/TrickumRoadside.json'
LIST = ROOT + '/Docs/Report113/trickum-plan.txt'
rng = random.Random(113)
log = []
def say(s): log.append(s); print(s)

# ------------------------------------------------------------------ the real roads (local metres around 34.086, -84.48, as 0.104)
LAT0, LON0 = 34.086, -84.48
def ll2xy(lat, lon): return ((lon - LON0) * 111320 * math.cos(math.radians(LAT0)), (lat - LAT0) * 110574)
osm = json.load(open(REF + 'osm-trickum.json'))
W = {w['id']: w for w in osm['ways']}
def wxy(w): return [ll2xy(p['lat'], p['lon']) for p in w['geometry']]
def chain(ids):
    pts = []
    for i in ids:
        g = wxy(W[i])
        if pts and math.dist(pts[-1], g[-1]) < math.dist(pts[-1], g[0]): g = g[::-1]
        pts += g if not pts else g[1:]
    return pts
# Trickum Rd, Hwy 92 (node 67300077) south to Jamerson Rd (node 67707169)
RT = LineString(chain([132057972, 400647815, 695831807, 38341241]))
# Jamerson Rd east from Trickum to Cherokee Ln, then Cherokee Ln
cher = wxy(W[9105236])
RJ = LineString(chain([38341240, 1316552008]) + cher[1:])
HWY_REAL = unary_union([LineString(wxy(w)) for w in osm['ways'] if w['tags'].get('name') == 'Highway 92' and len(w['geometry']) > 1])
say(f'real Trickum Rd {RT.length:.0f} m from {np.round(RT.coords[0]).tolist()} to {np.round(RT.coords[-1]).tolist()}; real Jamerson + Cherokee path {RJ.length:.0f} m')

# ------------------------------------------------------------------ the game roads (the Street Loop race road's centre line, FreeRoamWorld)
def survey_roads(scene):
    out = {}; cur = None
    for l in open(SV + f'roads-{scene}.txt'):
        if l.startswith('#'): cur = l[2:].split(' forestTrail')[0].strip(); out.setdefault(cur, []); continue
        i, x, y, z = l.split(); out[cur].append((int(i), float(x), float(y), float(z)))
    return out
main = sorted(survey_roads('FreeRoamWorld')['Phase 3 - Race Systems'])
JUNCTION = np.array([-614.0, -544.0])
idx = [i for i, x, y, z in main]; P = {i: (x, z) for i, x, y, z in main}
k_j = min(idx, key=lambda i: math.dist(P[i], JUNCTION))
k_t = min((i for i in idx if P[i][0] < -540 and P[i][1] > 500), key=lambda i: abs(P[i][1] - 541))
step = 1 if k_t < k_j else -1
GT = LineString([P[i] for i in range(k_t, k_j + step, step) if i in P])
k_e = min((i for i in idx if P[i][1] < -500 and P[i][0] > -240), key=lambda i: abs(P[i][0] + 200))
step2 = 1 if k_j < k_e else -1
GC = LineString([P[i] for i in range(k_j, k_e + step2, step2) if i in P])
if GT.coords[0][1] < GT.coords[-1][1]: GT = LineString(GT.coords[::-1])
KT = GT.length / RT.length
say(f'game Trickum Rd {GT.length:.0f} m from {np.round(GT.coords[0]).tolist()} to {np.round(GT.coords[-1]).tolist()}: K {KT:.3f}; game S Cherokee Ln {GC.length:.0f} m to {np.round(GC.coords[-1]).tolist()}')
KC = KT
GAME_HALF = 4.5; REAL_HALF = 6.0

def frame(line, s):
    s = min(max(s, 0), line.length); a = line.interpolate(max(0, s - 1)); b = line.interpolate(min(line.length, s + 1))
    t = np.array([b.x - a.x, b.y - a.y]); t /= max(np.linalg.norm(t), 1e-9); p = line.interpolate(s)
    return np.array([p.x, p.y]), t, np.array([-t[1], t[0]])   # left of travel
def sd(line, p):
    p = Point(p); s = line.project(p); q, t, n = frame(line, s); return s, float(np.dot(np.array([p.x, p.y]) - q, n))
def lat(d, K): return d * GAME_HALF / REAL_HALF if abs(d) <= REAL_HALF else math.copysign(GAME_HALF + (abs(d) - REAL_HALF) * K, d)
def unlat(dg, K): return dg * REAL_HALF / GAME_HALF if abs(dg) <= GAME_HALF else math.copysign(REAL_HALF + (abs(dg) - GAME_HALF) / K, dg)
def r2g(p, road):
    if road == 'T': s, d = sd(RT, p); q, t, n = frame(GT, s * KT); return q + n * lat(d, KT)
    s, d = sd(RJ, p); q, t, n = frame(GC, s * KC); return q + n * lat(d, KC)
def which(p):
    pt = Point(p); dt, dj = RT.distance(pt), RJ.distance(pt)
    return 'T' if dt <= dj else 'J'
# ranges: Trickum from where the Hwy 92 plan stops (230 m from the real Hwy 92) to the junction; Cherokee to just past the meeting hall
J_END = 430.0
def in_scope(p, road, depth):
    s, d = sd(RT if road == 'T' else RJ, p)
    if abs(d) > depth: return False
    if road == 'T': return HWY_REAL.distance(Point(p)) >= 230 and s <= RT.length + 5
    return 0 <= s <= J_END
say(f'junctions: real Trickum end {np.round(RT.coords[-1]).tolist()} -> game {np.round(r2g(RT.coords[-1], "T"), 1).tolist()}; real Jamerson/Cherokee {np.round(cher[0]).tolist()} -> game {np.round(r2g(cher[0], "J"), 1).tolist()}')

# ------------------------------------------------------------------ the game's constraints
courses = json.load(open(ROOT + '/Assets/Resources/WorldMaps/CoursePreviews.json'))['courses']
race = []; branches = []
for c in courses:
    race.append(LineString([(p['x'], p['z']) for p in c['main']]))
    for b in c.get('branches', []):
        ln = LineString([(p['x'], p['z']) for p in b['points']]); race.append(ln); branches.append(ln)
BAND = box(-780, -660, -150, 430)
RACE = unary_union([r.intersection(BAND.buffer(60)) for r in race])
BRANCH = unary_union([b.intersection(BAND.buffer(60)) for b in branches])
GAME_ROAD = unary_union([GT, GC])
SOLID_CLEAR = RACE.buffer(10)                       # no solid within 10 m of any race line (0.104 rule)
FLAT_CLEAR = GAME_ROAD.buffer(GAME_HALF + .1).union(BRANCH.buffer(6))   # nothing drawn on the road or on a shortcut's trail
prot = []; protected = []
SCENES = ['FreeRoamWorld', 'StreetLoopGreybox', 'StreetLoopReverse', 'DansBackyardForward', 'DansBackyardReverse', 'LakeWoods', 'ForestLoopReverse', 'MountainLoop', 'MountainLoopReverse']
KEEP_CLEAR = ('Breakable sign', 'direction signs', 'SPEED CAMERA', 'JUMP CHALLENGE', 'NO ENTRY', 'Jump approach sign', 'Trickum pavement jump', 'Acorn',
              'Connector Jump', 'raised pavement works', 'Decorative road continuations', 'Southwest woodland shortcut', 'arcade sites', 'Speed trap',
              'exploration world', 'Pine Ridge', 'Granite', 'Street name', 'RaceGate', 'gate', 'Gate', 'CR091', 'Checkpoint', 'Hidden', 'shortcut', 'Shortcut', 'Jamerson')
old_res = []
for sn in SCENES:
    for l in open(SV + f'objects-{sn}.txt'):
        f = l.rstrip('\n').split('\t'); x, y, z, ex, ez, ey, kind, act = f[:8]; path = f[-1]
        x, z, ex, ez = float(x), float(z), float(ex), float(ez)
        if act == 'off' or not BAND.buffer(20).contains(Point(x, z)): continue
        if path.startswith('Remembered houses and approximate buildings/Approximate older residence') and sn == 'FreeRoamWorld': old_res.append((x, z, ex, ez, f[1])); continue
        if path.startswith(('Woods replacing', 'World cleanup additional woodland', 'Remembered houses', 'CR113 continuous', 'CR034-039 Woodland routes/Highway edge',
                            'CR034-039 Woodland routes/Broken lane', 'CR034-039 Woodland routes/Trail edge')): continue
        if 'BIG' in f[8:9]: continue
        if any(k in path for k in KEEP_CLEAR):
            r = max(ex, ez, 1.0) + 4
            prot.append(Point(x, z).buffer(r)); protected.append((path, x, z, r))
# the trailer jump (0.112) from its run-up to past its landing, and its flight over the road, with a wide margin
JUMP = LineString([(-628, -160), (-628, 60)]).buffer(16)
prot.append(JUMP)
# the junction corners (street name signs, 0.79) and the Existing Southwest Cut's mouth
for c in [(-614, -544), (-606, 541)]: prot.append(Point(c).buffer(24))
PROT = unary_union(prot)
say(f'protected: {len(protected)} objects in the band (+4 m each), the trailer jump corridor, the two junction corners; race lines {len(race)}')

# the Hwy 92 plan's buildings and paved areas (built in every scene already): nothing new on or within 6 m of them
hw = json.load(open(ROOT + '/Assets/Resources/Hwy92/Hwy92Roadside.json'))
hwb = [Polygon(np.array(p['pts']).reshape(-1, 2)) for b in hw['buildings'] for p in b['parts'] if len(p['pts']) >= 6]
HWYPLAN = unary_union([q.buffer(6) for q in hwb if q.is_valid]).union(box(-800, 420, -400, 900))

# the ground (2 m grid, FreeRoamWorld): height and what the top surface is
gx0, gz0 = -780, -660
rows = [l.split() for l in open(SV + 'grid-FreeRoamWorld.txt')]
G = np.array([[float(c.split(',')[0]) if c.split(',')[0] != 'nan' else np.nan for c in r] for r in rows])
def gh(x, z):
    j = int(round((x - gx0) / 2)); i = int(round((z - gz0) / 2))
    return G[i, j] if 0 <= i < G.shape[0] and 0 <= j < G.shape[1] else np.nan
def relief(poly):
    mnx, mnz, mxx, mxz = poly.bounds; hs = [gh(x, z) for x in np.arange(mnx, mxx + 1, 2) for z in np.arange(mnz, mxz + 1, 2) if poly.buffer(1).contains(Point(x, z))]
    hs = [h for h in hs if not np.isnan(h)]; return (max(hs) - min(hs)) if hs else 99.0

# ------------------------------------------------------------------ buildings
HOUSE_COLOURS = 0
NAMES = {'church': ['Hillside Fellowship Church'], 'hall': ['Laurel Ridge Meeting Hall'], 'apartments': [], 'office': ['Professional Offices', 'Medical Offices'],
         'senior': ['Oak Terrace'], 'retail': ['Shops']}
def kind_of(t, area):
    b = t.get('building', ''); a = t.get('amenity', '')
    if a == 'place_of_worship' and t.get('denomination') == 'jehovahs_witness': return 'hall'
    if b == 'church' or a == 'place_of_worship': return 'church'
    if b in ('garage', 'garages', 'shed', 'roof', 'carport'): return None
    if b == 'apartments': return 'apartments'
    if b in ('commercial', 'office'): return 'office'
    if b == 'retail': return 'retail'
    if b in ('school', 'public'): return 'civic'
    if area < 40: return None
    if area > 900: return 'apartments' if b in ('yes', 'residential') else 'office'
    return 'house'
def map_poly(poly, road):
    return Polygon([tuple(r2g(c, road)) for c in list(poly.exterior.coords)[:-1]]).buffer(0)
shifts = []
buildings = []; houses = []; dropped = {'hwy92 plan': 0, 'race lines': 0, 'protected': 0, 'hills': 0, 'overlap': 0}
placed = []
for w in osm['ways']:
    t = w['tags']
    if 'building' not in t: continue
    g = wxy(w)
    if len(g) < 4: continue
    poly = Polygon(g).buffer(0)
    if poly.is_empty or poly.geom_type != 'Polygon': continue
    c = (poly.centroid.x, poly.centroid.y); road = which(c)
    if not in_scope(c, road, 115 if road == 'T' else 110): continue
    k = kind_of(t, poly.area)
    if not k: continue
    gp = map_poly(poly, road)
    if gp.is_empty or gp.geom_type != 'Polygon' or gp.area < 25: continue
    if gp.intersects(HWYPLAN): dropped['hwy92 plan'] += 1; continue
    if (gp.intersects(SOLID_CLEAR) or relief(gp) > 4.0) and k in ('hall', 'church'):
        # Dan named these: the smallest local shift (3 m steps, up to 45 m) that keeps the building 10 m off every race line; its lot follows
        from shapely import affinity
        opts = sorted(((dx, dz) for dx in range(-60, 61, 3) for dz in range(-60, 61, 3)), key=lambda o: o[0] ** 2 + o[1] ** 2)
        for dx, dz in opts:
            q = affinity.translate(gp, dx, dz)
            if not q.intersects(SOLID_CLEAR) and not q.intersects(PROT) and not any(q.buffer(1.5).intersects(o) for o in placed) and GAME_ROAD.distance(q) > GAME_HALF + 8 and relief(q) <= 4.0:
                shifts.append((np.array(poly.centroid.coords[0]), dx, dz)); say(f'  {k} (OSM {w["id"]}) moved {dx:+d}, {dz:+d} m to keep 10 m off a race line (the Pine Ridge trail) and on level ground'); gp = q; break
    if gp.intersects(SOLID_CLEAR): dropped['race lines'] += 1; continue
    if gp.intersects(PROT): dropped['protected'] += 1; continue
    if relief(gp) > 4.0: dropped['hills'] += 1; continue
    if any(gp.buffer(1.5).intersects(q) for q in placed): dropped['overlap'] += 1; continue
    placed.append(gp)
    # the edge most facing its road is the front
    pts = list(gp.exterior.coords)[:-1]
    if Polygon(pts).exterior.is_ccw: pts = pts[::-1]   # clockwise from above: each edge's right-hand normal is its outside (the builder's rule)
    line = GT if road == 'T' else GC
    s0 = line.project(Point(gp.centroid)); q, tt, nn = frame(line, s0); toward = q - np.array([gp.centroid.x, gp.centroid.y]); toward /= max(np.linalg.norm(toward), 1e-9)
    fronts = []; best = -2; bi = 0
    for i in range(len(pts)):
        a = np.array(pts[i]); b = np.array(pts[(i + 1) % len(pts)]); e = b - a; L = np.linalg.norm(e)
        if L < .05: fronts.append(0); continue
        out = np.array([e[1], -e[0]]) / L
        score = float(np.dot(out, toward)) + (L > 6) * .05
        fronts.append(1 if score > .3 else 0)
        if score > best: best, bi = score, i
    fronts[bi] = 2
    area = gp.area
    h = {'house': rng.choice([3.4, 3.8, 5.6, 6.0]) if area > 110 else rng.choice([3.2, 3.5]), 'apartments': 9.5, 'office': 6.5, 'retail': 5.0,
         'church': 7.5, 'hall': 5.2, 'civic': 6.5}[k]
    name = NAMES.get(k, [''])[0] if k in ('church', 'hall') else ''
    tri = []
    if k != 'house':
        # a flat roof: a fan of the footprint (convex enough for these footprints; concave ones triangulated by shapely)
        for tr in shapely.constrained_delaunay_triangles(gp).geoms:
            tri += [round(v, 2) for x, z in list(tr.exterior.coords)[:-1] for v in (x, z)]
    rec = dict(kind=('civic' if k == 'hall' else k), name=name, h=h, tenants=[], osm=w['id'],
               parts=[dict(pts=[round(v, 2) for x, z in pts for v in (x, z)], fronts=fronts, roof=tri)])
    buildings.append(rec)
    if k == 'house': houses.append((gp, road, w['id']))
say(f'buildings: {len(buildings)} ({sum(1 for b in buildings if b["kind"] == "house")} houses); left out {dropped}')

# ------------------------------------------------------------------ side streets, entrances, drives, parking, sidewalks
streets = []; walls = []; lights = []; police = []; asphalt = []; white = []; sidewalk = []
SUBDIVISIONS = ['Maple Hollow', 'Cedar Run', 'Briar Glen', 'Fox Hollow', 'Pine Knoll', 'Stone Creek', 'Laurel Bend', 'Quail Ridge', 'Hickory Chase', 'Mill Pond']
sub_i = 0
def mapped_line(coords, road, step=3.0):
    ln = LineString(coords); n = max(2, int(ln.length / step))
    return LineString([tuple(r2g((ln.interpolate(i / n, normalized=True).x, ln.interpolate(i / n, normalized=True).y), road)) for i in range(n + 1)])
for w in osm['ways']:
    t = w['tags']; hw_ = t.get('highway')
    if hw_ not in ('residential', 'unclassified', 'living_street'): continue
    g = wxy(w)
    if len(g) < 2: continue
    ln = LineString(g)
    for road, R in (('T', RT), ('J', RJ)):
        if ln.distance(R) > 12 or t.get('name', '').startswith(('Trickum', 'Jamerson', 'South Cherokee')): continue
        end = g[0] if Point(g[0]).distance(R) < Point(g[-1]).distance(R) else g[-1]
        if not in_scope(end, road, 30): continue
        coords = g if end == g[0] else g[::-1]
        # the first 70 m (real) in from the road
        cut = LineString(coords); keep = [cut.interpolate(d) for d in np.arange(0, min(cut.length, 70), 3)]
        if len(keep) < 3: continue
        gl = mapped_line([(p.x, p.y) for p in keep], road)
        st = gl.buffer(3.4, cap_style=2).difference(FLAT_CLEAR)
        if st.is_empty or gl.intersects(PROT) or gl.buffer(3.4).intersects(JUMP): continue
        streets.append(st)
        # the entrance: a low wall each side of the mouth, 8 m in from the road edge, the name on the face toward the road
        line = GT if road == 'T' else GC
        a = np.array(gl.coords[0]); b = np.array(gl.interpolate(10).coords[0]); u = (b - a) / max(np.linalg.norm(b - a), 1e-9); side = np.array([-u[1], u[0]])
        mouth = np.array(gl.interpolate(16).coords[0]); name = SUBDIVISIONS[sub_i % len(SUBDIVISIONS)]; sub_i += 1
        for sg in (1, -1):
            # a wall 5 m long across the mouth's side (parallel to the main road), from 4.6 to 9.6 m off the street's centre line
            e0 = mouth + side * sg * 4.6; e1 = mouth + side * sg * 9.6
            wp = LineString([tuple(e0), tuple(e1)])
            if wp.buffer(1).intersects(SOLID_CLEAR) or wp.buffer(1).intersects(PROT) or any(wp.buffer(1).intersects(q) for q in placed): continue
            walls.append(dict(a=[round(e0[0], 2), round(e0[1], 2)], b=[round(e1[0], 2), round(e1[1], 2)], facing=[round(-u[0], 3), round(-u[1], 3)], h=1.1, label=name))
            placed.append(wp.buffer(.5))
        # a street light 14 m in, on the right
        lp = np.array(gl.interpolate(20).coords[0]) - side * 5.2
        if not Point(lp).buffer(1).intersects(SOLID_CLEAR.union(PROT)): lights += [round(lp[0], 2), round(lp[1], 2), round(math.degrees(math.atan2(side[0], side[1])), 1)]
        if len(police) < 3 and road == 'T':
            pp = np.array(gl.interpolate(22).coords[0]); police.append(dict(pos=[round(pp[0], 2), round(pp[1], 2)], forward=[round(-u[0], 3), round(-u[1], 3)]))
STREETS = unary_union(streets) if streets else Polygon()
say(f'side streets {len(streets)}, entrance walls {len(walls)}, lights {len(lights) // 3}, hidden-police places {len(police)}')

drives = []; mail = []
def drive_for(house, road):
    line = GT if road == 'T' else GC
    c = np.array(house.centroid.coords[0]); s0 = line.project(Point(c)); q, tt, nn = frame(line, s0)
    sgn = 1 if np.dot(c - q, nn) > 0 else -1
    start = q + nn * sgn * (GAME_HALF + .15)
    # to the middle of the house's front, from the road edge, straight
    tgt = np.array(nearest_point(house, start))
    ln = LineString([tuple(start), tuple(tgt)])
    return ln, start, sgn, tt, nn
def nearest_point(poly, p):
    from shapely.ops import nearest_points
    return nearest_points(poly.exterior, Point(p))[0].coords[0]
# real driveways first (OSM service=driveway), else a straight drive from the road edge to the house front
osm_drives = []
for w in osm['ways']:
    t = w['tags']
    if t.get('highway') == 'service' and t.get('service') in ('driveway', None, '') and len(w['geometry']) >= 2:
        g = wxy(w); end = g[0] if Point(g[0]).distance(RT) < Point(g[-1]).distance(RT) else g[-1]
        road = which(end)
        if not in_scope(end, road, 25): continue
        osm_drives.append((mapped_line(g if end == g[0] else g[::-1], road), road))
for house, road, oid in houses:
    best = None
    for gl, rd in osm_drives:
        if gl.distance(house) < 3: best = gl; break
    if best is None:
        ln, start, sgn, tt, nn = drive_for(house, road)
        if ln.length > 70: continue
        best = ln
    poly = best.buffer(1.5, cap_style=2).difference(FLAT_CLEAR).difference(house)
    if poly.is_empty or poly.intersects(PROT) or any(poly.intersects(h) for h, _, _ in houses if h is not house): continue
    drives.append(poly)
    # the mailbox beside the drive's mouth, 1.2 m off the road edge, facing the road
    line = GT if road == 'T' else GC
    m0 = np.array(best.coords[0]); s0 = line.project(Point(m0)); q, tt, nn = frame(line, s0); sgn = 1 if np.dot(m0 - q, nn) > 0 else -1
    mb = q + nn * sgn * (GAME_HALF + 1.2) + tt * 2.3
    if not Point(mb).buffer(.6).intersects(PROT): mail += [round(mb[0], 2), round(mb[1], 2), round(math.degrees(math.atan2(-nn[0] * sgn, -nn[1] * sgn)), 1)]
DRIVES = unary_union(drives) if drives else Polygon()
# parking lots (OSM amenity=parking) by the church and the hall, with stall lines along their long side
lots = []
for w in osm['ways']:
    t = w['tags']
    if t.get('amenity') != 'parking' or len(w['geometry']) < 4: continue
    g = wxy(w); c = Polygon(g).centroid; road = which((c.x, c.y))
    if not in_scope((c.x, c.y), road, 140): continue
    gp = map_poly(Polygon(g).buffer(0), road)
    # a lot next to a moved hall / church moves with it
    for c0, dx, dz in shifts:
        if np.linalg.norm(np.array(Polygon(g).centroid.coords[0]) - c0) < 90:
            from shapely import affinity
            gp = affinity.translate(gp, dx, dz); break
    gp = gp.difference(FLAT_CLEAR).difference(unary_union(placed)).difference(BRANCH.buffer(6))
    if gp.is_empty or gp.intersects(PROT): continue
    lots.append(gp)
LOTS = unary_union(lots) if lots else Polygon()
# sidewalks only where the real road has them (OSM footway=sidewalk beside it), stopping at every drive and street (0.105)
for w in osm['ways']:
    t = w['tags']
    if t.get('footway') != 'sidewalk' or len(w['geometry']) < 2: continue
    g = wxy(w); m = Polygon(LineString(g).buffer(.01)).centroid; road = which((m.x, m.y))
    if sd(RT if road == 'T' else RJ, (m.x, m.y))[1] ** 2 > 25 ** 2 or road == 'J' and not in_scope((m.x, m.y), road, 25): continue
    sw = mapped_line(g, road).buffer(.8, cap_style=2).difference(FLAT_CLEAR).difference(DRIVES.buffer(.3)).difference(STREETS.buffer(.3)).difference(LOTS).difference(PROT).difference(HWYPLAN)
    if not sw.is_empty and sw.area > 4: sidewalk.append(sw)
say(f'drives {len(drives)}, mailboxes {len(mail) // 3}, parking lots {len(lots)}, sidewalk pieces {len(sidewalk)}')

# ------------------------------------------------------------------ yard trees and fences
taken = unary_union(placed + [DRIVES, STREETS, LOTS] + sidewalk)
trees = []; fences = []
for house, road, oid in houses:
    line = GT if road == 'T' else GC
    c = np.array(house.centroid.coords[0]); s0 = line.project(Point(c)); q, tt, nn = frame(line, s0); away = (c - q) / max(np.linalg.norm(c - q), 1e-9)
    for k in range(rng.choice([4, 5, 6, 7, 8])):
        p = c + away * rng.uniform(-4, 28) + tt * rng.uniform(-18, 18)   # a wooded lot: round the sides and back of the house
        pt = Point(p)
        if pt.buffer(2.5).intersects(taken) or line.distance(pt) < GAME_HALF + 6 or pt.intersects(PROT): continue
        solid = not pt.buffer(1).intersects(SOLID_CLEAR)
        trees += [round(p[0], 2), round(p[1], 2), round(rng.uniform(.85, 1.35), 2), 1 if solid else 0]
    # a privacy fence round the back yard of about one house in four
    if rng.random() < .27:
        mnx, mnz, mxx, mxz = house.bounds; ext = list(house.exterior.coords)
        far = sorted(ext, key=lambda e: -np.dot(np.array(e) - q, away))[:2]
        a, b = np.array(far[0]), np.array(far[1])
        fa = a + away * 12; fb = b + away * 12
        fl = LineString([tuple(a), tuple(fa), tuple(fb), tuple(b)])
        if not fl.buffer(.5).intersects(taken.difference(house.buffer(.6))) and not fl.intersects(PROT) and fl.length > 8:
            fences.append(dict(pts=[round(v, 2) for x, z in fl.coords for v in (x, z)], h=1.8, style='privacy', solid=not fl.buffer(1).intersects(SOLID_CLEAR)))
# the real fences by the road (OSM barrier=fence)
for w in osm['ways']:
    if w['tags'].get('barrier') != 'fence' or len(w['geometry']) < 2: continue
    g = wxy(w); m = LineString(g).interpolate(.5, normalized=True); road = which((m.x, m.y))
    if not in_scope((m.x, m.y), road, 100): continue
    fl = mapped_line(g, road, 2).difference(FLAT_CLEAR.buffer(1.5)).difference(PROT)
    for piece in (fl.geoms if hasattr(fl, 'geoms') else [fl]):
        if piece.length > 4 and not piece.buffer(.5).intersects(unary_union(placed)):
            fences.append(dict(pts=[round(v, 2) for x, z in piece.coords for v in (x, z)], h=1.4, style='rail', solid=not piece.buffer(1).intersects(SOLID_CLEAR)))
say(f'yard trees {len(trees) // 4} ({sum(trees[3::4])} solid), fences {len(fences)}')

# ------------------------------------------------------------------ the old placeholders and the old woods where today's lots are
remove_at = []
for x, z, ex, ez, y in old_res:
    if GAME_ROAD.distance(Point(x, z)) < 95: remove_at += [round(x, 2), round(z, 2)]
cleared = unary_union([q.buffer(7) for q in placed] + [DRIVES.buffer(2.5), STREETS.buffer(3), LOTS.buffer(2.5)] + [s.buffer(1) for s in sidewalk])
cx0, cz0 = -790, -670; cw, chh = 660, 1110
mask = np.zeros((chh, cw), np.uint8)
from shapely.prepared import prep
pc = prep(cleared)
for j in range(chh):
    for i in range(cw):
        if pc.contains(Point(cx0 + i + .5, cz0 + j + .5)): mask[j, i] = 1
bits = np.packbits(mask.flatten()); packed = base64.b64encode(zlib.compress(bits.tobytes(), 9)).decode()
say(f'old placeholder houses removed: {len(remove_at) // 2}; cleared ground for the lots {int(mask.sum())} m2 (old trees there go)')

import shapely
def tris(geom):
    out = []
    for g in (geom.geoms if hasattr(geom, 'geoms') else [geom]):
        if g.is_empty or g.geom_type != 'Polygon' or g.area < .05: continue
        for t in shapely.constrained_delaunay_triangles(g).geoms:
            if t.area > .005: out += [round(v, 2) for x, z in list(t.exterior.coords)[:-1] for v in (x, z)]
    return out
flat = dict(asphalt=tris(DRIVES.union(LOTS)), street=tris(STREETS), sidewalk=tris(unary_union(sidewalk)) if sidewalk else [], island=[], white=[], yellow=[])
plan = dict(version='0.113', title='Trickum Rd roadside', source='OpenStreetMap contributors (ODbL) and USGS NAIP imagery, placement only; every name invented',
            buildings=buildings, canopies=[], flat=flat, lights=lights, trees=trees, shrubs=[], pylons=[], signals=[], police=police,
            remove=[], removeNamed=['Approximate older residence'], removeAt=remove_at, keepInPlot=[], plot=[],
            clear=dict(x0=cx0, z0=cz0, w=cw, h=chh, bits=packed), walls=walls, fences=fences, mailboxes=mail)
os.makedirs(os.path.dirname(LIST), exist_ok=True)
json.dump(plan, open(OUT, 'w'), separators=(',', ':'))
say(f'wrote {OUT} ({os.path.getsize(OUT) // 1024} KB)')
with open(LIST, 'w') as f:
    f.write('\n'.join(log) + '\n\nbuildings (game x/z centre, kind, name, height):\n')
    for b in buildings:
        pts = np.array(b['parts'][0]['pts']).reshape(-1, 2); f.write(f"  {pts[:, 0].mean():7.1f} {pts[:, 1].mean():7.1f} {b['kind']:10s} {b['name']:28s} {b['h']}\n")
    f.write('\nentrances:\n' + ''.join(f"  {w['label']} at {w['a']}\n" for w in walls[::2]))
    f.write('\nprotected objects kept clear (+4 m):\n' + ''.join(f'  {x:7.1f} {z:7.1f} r {r:4.1f} {p}\n' for p, x, z, r in protected))
# a top-down drawing of the plan in game x/z
from PIL import Image, ImageDraw
S = 1.0; im = Image.new('RGB', (int(660 * S), int(1110 * S)), (40, 60, 35)); d = ImageDraw.Draw(im)
def px(x, z): return ((x - cx0) * S, (cz0 + chh - z) * S)
def poly(g, col):
    for q in (g.geoms if hasattr(g, 'geoms') else [g]):
        if q.geom_type == 'Polygon' and not q.is_empty: d.polygon([px(x, z) for x, z in q.exterior.coords], fill=col)
poly(cleared, (70, 95, 55)); poly(STREETS, (90, 90, 95)); poly(DRIVES, (120, 120, 120)); poly(LOTS, (110, 110, 115))
for s in sidewalk: poly(s, (200, 200, 190))
for ln in [GT, GC]: d.line([px(x, z) for x, z in ln.coords], fill=(60, 60, 60), width=9)
for r in race:
    c = r.intersection(BAND)
    for q in (c.geoms if hasattr(c, 'geoms') else [c]):
        if q.geom_type == 'LineString' and len(q.coords) > 1: d.line([px(x, z) for x, z in q.coords], fill=(0, 190, 255), width=1)
for b in buildings: d.polygon([px(*p) for p in np.array(b['parts'][0]['pts']).reshape(-1, 2)], fill=(230, 220, 200) if b['kind'] == 'house' else (230, 120, 90), outline=(0, 0, 0))
for w in walls: d.line([px(*w['a']), px(*w['b'])], fill=(200, 60, 40), width=3)
for i in range(0, len(trees), 4): x, z = trees[i], trees[i + 1]; d.ellipse([px(x - 2, z + 2), px(x + 2, z - 2)], fill=(30, 110, 30))
for f_ in fences: d.line([px(*p) for p in np.array(f_['pts']).reshape(-1, 2)], fill=(160, 120, 70), width=1)
for p_ in police: d.ellipse([px(p_['pos'][0] - 3, p_['pos'][1] + 3), px(p_['pos'][0] + 3, p_['pos'][1] - 3)], outline=(40, 80, 255), width=2)
for x, z, r in [(x, z, r) for p, x, z, r in protected]: d.ellipse([px(x - r, z + r), px(x + r, z - r)], outline=(255, 210, 0))
for i in range(0, len(remove_at), 2): x, z = remove_at[i], remove_at[i + 1]; d.line([px(x - 3, z - 3), px(x + 3, z + 3)], fill=(255, 0, 0), width=2); d.line([px(x - 3, z + 3), px(x + 3, z - 3)], fill=(255, 0, 0), width=2)
im.save(ROOT + '/Docs/Report113/trickum-plan-topdown.png')
say('top-down drawing: Docs/Report113/trickum-plan-topdown.png')
