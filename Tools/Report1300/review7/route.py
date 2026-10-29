# 1.3.0-review7 (Dan, 2026-10-28: "Leviathan reverse. Still not clean when you finish the mountain pass ... Once you come off the mountain you
# see the arrows but not yet the gate to the left. The other, Leviathan Forward, you can see they blocked the main path and left open the
# shortcut to skip it."). Reads both course files as they are at the review7 safety checkpoint (ad63181a), so it can be run again.
#
# Part B, Leviathan Forward (review6's Reverse change, mirrored): since 0.115 the line left CR117 at Climbing Ridge Cut's entry (CR117 s 18) and
# drove the cut to CR117 s 1407 (just before CR117 CP 3); 0.117 barricaded CR117's road there (s 995 and 1299), the road with CR117 CP 2 and the
# Eastbound Gully Flight. Here:
#   - the line's piece from CR117 s 5 to CR117 s 1412 (the cut and its blends) is replaced by CR117's own centreline over that span;
#   - the cut's gate (CP 8 "Climbing Ridge Cut") is replaced by the scene's own CR117 CP 2, and a gate 35 m past the gully flight's landing;
#   - CR117's Eastbound Gully Flight comes in at its stations on the new line;
#   - Climbing Ridge Cut becomes one of the course's shortcuts (the scene's own branch, its points and settings: AI-validated as in Mountain
#     Loop, so rivals may take it in a normal race), a board at its entry and REJOIN ROAD at its end; StuntShortcuts closes it in Stunt Races
#     (it leaves the course on the mountain);
#   - the barricades across CR117 there go; the side roads off the new road are closed (Tools/Report117/py/blocks.py's rule, as in review6);
#   - the chevrons onto the cut and off it, and the relabel that called the cut Leviathan's road, go;
#   - every station after the span moves by the change in length; the course id becomes leviathan-forward-v5 (new boards; v4 kept, not shown).
# Part A, Leviathan Reverse (coming off the mountain):
#   - Dan's photo 1 spot is the end of the descent just past CP 38 (CR117 CP 6, (729, -88)): the line bends right there round the bank to
#     CP 39 (726, -24), 70 m on, out of view; a gate at the bend (s 7318), 18 m wide, seen straight ahead as you come down;
#   - at J1 (723, -4), just past CP 39, the route turns left onto the lake strip while straight on is the climb (driven earlier in the lap, so it
#     can't be closed): CP 40 is 80 m on, 37 deg left, behind the bank; a gate just round the turn (s 7405), seen as you pass CP 39;
#   - a rail (timber, slippery: a glancing hit slides you along it to the left) across the straight-on side of J1, leaving the climb's own
#     way past it open, with left chevrons on it facing the traffic from CP 39.
# usage (project root): python Tools/Report1300/review7/route.py
import json, math, os, re, subprocess, sys
sys.path.insert(0, 'Tools/Report115/py')
import leviathan as L
from leviathan import stations, at, project

BASE = 'ad63181a6'
log = []
def say(*a):
    s = ' '.join(str(x) for x in a); print(s); log.append(s)
def load(path): return json.loads(subprocess.run(['git', 'show', BASE + ':' + path], capture_output=True, text=True, encoding='utf-8').stdout)
def renumber(gates, prefix):
    for k, x in enumerate(gates):
        if k == 0: continue
        m = re.match(re.escape(prefix) + r' CP \d+(.*)$', x['name']); x['name'] = f"{prefix} CP {k}{m.group(1) if m else ' (' + x['name'] + ')'}"
def yaw_of(N, NS, s):
    p = at(N, NS, s, True); q = at(N, NS, s + 3, True); return math.degrees(math.atan2(q[0] - p[0], q[2] - p[2])) % 360, p
def clearance(line, b):
    # the nearest a barricade (its whole width) comes to a line, in plan, at that line's height (4 m)
    x, z, y = b['pos']; r = math.radians(b['yaw']); rx, rz = math.cos(r), -math.sin(r); h = b.get('half', 5); best = 1e9
    for p in line:
        if abs(p[1] - y) > 4: continue
        t = max(-h, min(h, (p[0] - x) * rx + (p[2] - z) * rz)); best = min(best, math.hypot(p[0] - x - rx * t, p[2] - z - rz * t))
    return best

# ================================================================ Part B: Leviathan Forward
OUT = 'Assets/Resources/Leviathan/LeviathanForward.json'
R = load(OUT)
P = [tuple(R['pts'][i:i + 3]) for i in range(0, len(R['pts']), 3)]; S = stations(P, True); Lold = S[-1]
sc = L.scene('MountainLoop'); cr = sc['roads'][L.CR117]['points']; Cs = stations(cr, True)
crc = sc['branches']['Climbing Ridge Cut']
C0, C1 = 5.0, 1412.0  # CR117 stations where the line is on CR117 (before the cut's entry blend, after its end)
def nearest(q, lo, hi): return min((i for i in range(len(P)) if lo <= S[i] <= hi), key=lambda i: math.dist(P[i], q))
i0 = nearest(at(cr, Cs, C0, True), 960, 1000); i1 = nearest(at(cr, Cs, C1, True), 1290, 1330)
say(f'Forward: line on CR117 s {C0:.0f}: point {i0} s {S[i0]:.1f} ({math.dist(P[i0], at(cr, Cs, C0, True)):.2f} m); CR117 s {C1:.0f}: point {i1} s {S[i1]:.1f} ({math.dist(P[i1], at(cr, Cs, C1, True)):.2f} m)')
c_a, da0 = project(cr, P[i0], True); c_b, db0 = project(cr, P[i1], True)
say(f'  on CR117 there: s {c_a:.1f} ({da0:.2f} m off), s {c_b:.1f} ({db0:.2f} m off)'); assert da0 < .6 and db0 < .6
mid = [q for q, s in zip(cr, Cs) if c_a + .5 < s < c_b - .5]
N = P[:i0 + 1] + mid + P[i1:]
NS = stations(N, True); Lnew = NS[-1]; j1 = i0 + 1 + len(mid)
delta = NS[j1] - S[i1]
say(f'Forward: replaced line s {S[i0]:.1f}-{S[i1]:.1f} ({S[i1] - S[i0]:.1f} m: Climbing Ridge Cut) by CR117 s {c_a:.1f}-{c_b:.1f} ({NS[j1] - NS[i0]:.1f} m, {len(mid)} points); lap {Lold:.1f} -> {Lnew:.1f} m (+{delta:.1f})')
def move(s):
    if s <= S[i0] + 1e-6: return s
    if s >= S[i1] - 1e-6: return s + delta
    return None
C = dict(R)
C['id'] = 'leviathan-forward-v5'; C['length'] = Lnew
C['pts'] = [round(v, 3) for q in N for v in q]
g = R['geometryStations']; C['geometryStations'] = g[:i0 + 1] + [g[i0]] * len(mid) + g[i1:]
assert len(C['geometryStations']) == len(N)
C['seat'] = [i if i <= i0 else i + (j1 - i1) for i in R['seat'] if i <= i0 or i >= i1]
parts = []
for p in R['parts']:
    a, b = move(p['from']), move(p['to'])
    if a is not None and b is not None: parts.append({'name': p['name'], 'from': round(a, 1), 'to': round(b, 1)})
    elif a is not None: parts.append({'name': p['name'], 'from': round(a, 1), 'to': round(NS[i0], 1)})
    elif b is not None: parts.append({'name': p['name'], 'from': round(NS[j1], 1), 'to': round(b, 1)})
parts.append({'name': 'CR117 Eastbound Gully Flight road (1.3.0-review7)', 'from': round(NS[i0], 1), 'to': round(NS[j1], 1)})
C['parts'] = sorted(parts, key=lambda p: p['from'])
def spans(v): return [round(move(x) if move(x) is not None else x, 1) for x in v]
C['mountain'] = spans(R['mountain']); C['narrow'] = spans(R['narrow']); C['bypass'] = spans(R['bypass'])
say(f"Forward: mountain {R['mountain']} -> {C['mountain']}; narrow {R['narrow']} -> {C['narrow']}; bypass {R['bypass']} -> {C['bypass']}")
# gates: the cut's CP 8 out, CR117 CP 2 (the scene's own) and a gate after the gully landing in
old_gates = R['gates']; k_cut = next(k for k, x in enumerate(old_gates) if x['name'].endswith('(Climbing Ridge Cut)'))
sg = sc['gates']['CR117 selected-direction guidance/CR117 CP 2']
new_gates = old_gates[:k_cut] + [L.gate_entry(sg, 'CR117 CP 2', True, 'CR117 selected-direction guidance/CR117 CP 2')] + old_gates[k_cut + 1:]
for x in new_gates:
    if 'station' in x and x['name'] != 'CR117 CP 2' and move(x['station']) is not None: x['station'] = move(x['station']); continue
    st, d = project(N, x['pos'], True, NS[i0], NS[j1]); x['station'] = st; say(f"  {x['name']} on the new road at s {st:.1f} ({d:.2f} m from the line)")
eg = next(f for f in sc['flights'] if f['name'] == 'Eastbound Gully Flight')
fa, da = project(N, at(cr, Cs, eg['approachStation'], True), True, NS[i0], NS[j1]); fb, db = project(N, at(cr, Cs, eg['endStation'], True), True, NS[i0], NS[j1])
lg = L.new_gate(N, NS, fb + 35, 'after the Eastbound Gully Flight landing'); lg.pop('seat', None); lg['station'] = fb + 35
k_land = next(k for k, x in enumerate(new_gates) if k > 0 and x['station'] > fb + 35); new_gates.insert(k_land, lg)
# old gate index -> new index (CP 8 gone: the cut's own; CR117 CP 2 in its place; the landing gate after it)
gmap = {k: (k if k < k_cut else k + 1) for k in range(len(old_gates)) if k != k_cut}
renumber(new_gates, 'Leviathan')
assert all(new_gates[k]['station'] < new_gates[k + 1]['station'] for k in range(1, len(new_gates) - 1)), 'Forward gates out of order'
C['gates'] = new_gates
say('Forward gates: ' + ', '.join(f"{k} {x['name']} s {x['station']:.0f}" for k, x in enumerate(new_gates) if k_cut - 2 <= k <= k_cut + 4))
# flights
fl = [dict(f, approachStation=move(f['approachStation']), endStation=move(f['endStation'])) for f in R['flights']]
fl.insert(0, dict(eg, approachStation=fa, endStation=fb)); C['flights'] = fl
say(f"Forward flight Eastbound Gully Flight: CR117 {eg['approachStation']:.1f}-{eg['endStation']:.1f} -> Leviathan {fa:.1f}-{fb:.1f} ({da:.2f} / {db:.2f} m); Homeward moved by +{delta:.1f}")
# the shortcuts: the Street Loop's ones (after the span) moved; Climbing Ridge Cut added
brs = []
for b0 in R['branches']:
    b1 = dict(b0); b1['entryRoad'] = move(b0['entryRoad']); b1['exitRoad'] = move(b0['exitRoad']); b1['bypassedGates'] = [gmap[k] for k in b0['bypassedGates']]
    assert b1['entryRoad'] is not None and b1['exitRoad'] is not None; brs.append(b1)
dp = crc['points']; e, de = project(N, dp[0], True, NS[i0] - 10, NS[j1]); x, dx = project(N, dp[-1], True, NS[i0], NS[j1] + 40)
br = {k: crc[k] for k in ('title', 'halfWidth', 'recommendedSpeed', 'aiValidated', 'undergroundStart', 'undergroundEnd', 'entryHeightBelow', 'entryHeightAbove', 'entrySpeed', 'entrySpeedDistance', 'entryInset', 'entryMargin')}
br |= {'from': 'Mountain Loop: ' + crc['path'] + ' (1.3.0-review7)', 'pts': [round(v, 3) for q in dp for v in q], 'aiPts': [round(v, 3) for q in crc['aiPoints'] for v in q],
       'entryRoad': e, 'exitRoad': x, 'bypassedGates': [k for k, g_ in enumerate(new_gates) if k > 0 and e < g_['station'] < x]}
C['branches'] = [br] + brs
for b1 in C['branches']: assert all(b1['entryRoad'] < new_gates[k]['station'] < b1['exitRoad'] for k in b1['bypassedGates']), b1['title']
say(f"Forward shortcut Climbing Ridge Cut: entry s {e:.1f} ({de:.2f} m) exit s {x:.1f} ({dx:.2f} m), bypasses {br['bypassedGates']}, AI-validated {crc['aiValidated']}; others: " + ', '.join(f"{b['title']} s {b['entryRoad']:.0f}-{b['exitRoad']:.0f} bypasses {b['bypassedGates']}" for b in brs))
# signs: the chevrons onto the cut and off it out; a board at its entry (on the side it leaves) and REJOIN ROAD at its end
signs = [s for s in R['signs'] if s['at'] not in ('Climbing Ridge Cut', 'onto CR117')]
yw, p = yaw_of(N, NS, e - 40); r = math.radians(yw); right = (math.cos(r), -math.sin(r))
c0 = at(dp, stations(dp), 25); side = 1 if (c0[0] - p[0]) * right[0] + (c0[2] - p[2]) * right[1] > 0 else -1
txt = 'CLIMBING RIDGE CUT  >' if side > 0 else '<  CLIMBING RIDGE CUT'
signs.append({'kind': 'board', 'text': txt, 'dir': side, 'pos': [round(p[0] + side * right[0] * 8, 1), round(p[2] + side * right[1] * 8, 1), round(p[1], 1)], 'alt': [round(p[0] - side * right[0] * 8, 1), round(p[2] - side * right[1] * 8, 1), round(p[1], 1)], 'yaw': round(yw, 1), 'at': 'Climbing Ridge Cut entry'})
dS = stations(dp); qq = at(dp, dS, dS[-1] - 25); q2 = at(dp, dS, dS[-1] - 22); yw2 = math.degrees(math.atan2(q2[0] - qq[0], q2[2] - qq[2])) % 360; r2 = math.radians(yw2)
signs.append({'kind': 'board', 'text': 'REJOIN  ROAD', 'dir': 1, 'pos': [round(qq[0] - math.cos(r2) * 6, 1), round(qq[2] + math.sin(r2) * 6, 1), round(qq[1], 1)], 'yaw': round(yw2, 1), 'at': 'Climbing Ridge Cut rejoin'})
C['signs'] = signs
C['relabel'] = [x for x in R['relabel'] if x['find'] != 'CLIMBING RIDGE CUT']
say(f"Forward signs: the 5 chevrons onto the cut and back onto CR117 out; '{txt}' board at its entry, REJOIN ROAD at its end; relabels kept: " + ', '.join(x['find'] for x in C['relabel']))
# closed ways
keep, gone = [], []
for b0 in R['blocks']:
    st = b0.get('station'); b1 = dict(b0)
    if st is not None and S[i0] - 20 < st < S[i1] + 20:
        if clearance(N, b0) < 8: gone.append(b0); continue
        b1['station'] = round(project(N, (b0['pos'][0], b0['pos'][2], b0['pos'][1]), True, NS[i0] - 20, NS[j1] + 40)[0], 1); b1['at'] += ' (kept: off Climbing Ridge Cut, 1.3.0-review7)'; keep.append(b1); continue
    if st is not None: b1['station'] = round(move(st), 1)
    keep.append(b1)
say('Forward closed ways removed (across the new road): ' + '; '.join(f"{b['at']} ({b['pos'][0]:.0f}, {b['pos'][1]:.0f})" for b in gone))
sys.argv = ['blocks', 'Docs/Report117/Probe']
src = open('Tools/Report117/py/blocks.py', encoding='utf-8').read(); helpers = src[:src.index('NOENTRY =')]
ns = {}; exec(compile(helpers.split('LINES = sys.argv[1]')[0] + helpers.split('LINES = sys.argv[1]')[1], 'blocks.py', 'exec'), ns)
near, grid, along, width, OPENW, INTO = ns['near'], ns['grid'], ns['along'], ns['width'], ns['OPEN'], ns['INTO']
gN = grid(N); own = []
for b0 in C['branches']: own += [tuple(b0['pts'][i:i + 3]) for i in range(0, len(b0['pts']), 3)]
go = grid(own)
others = [(l['path'], [tuple(l['pts'][i:i + 3]) for i in range(0, len(l['pts']), 3)]) for l in json.load(open('Docs/Report117/Probe/lines-MountainLoop.json')) if l['active']]
added, skipped = [], []; lo, hi = NS[i0] - 20, NS[j1] + 20
for pth, Q in others:
    if any(o in pth for o in OPENW) or 'Climbing Ridge Cut' in pth: continue
    on = [near(N, gN, q, 7, 4) for q in Q]
    for k in range(1, len(Q)):
        a_, b_ = on[k - 1], on[k]
        if (a_ is None) == (b_ is None): continue
        j, step = (k - 1, 1) if a_ else (k, -1)
        if any(on[m] for m in range(k, min(len(Q), k + 6))) if a_ else any(on[m] for m in range(max(0, k - 6), k)): continue
        st = NS[on[j][1]]
        if not lo <= st <= hi: continue
        why = None; d0 = next((v for kk, v in INTO.items() if kk in pth), 14)
        if 'two-flight racing line' in pth and fa <= st <= fb: skipped.append(f"s {st:.0f} {pth.split('/')[-1]}: the ground under the Eastbound Gully Flight (the same road)"); continue
        for dist in (d0, d0 + 8, d0 + 16, d0 + 24):
            q, hd = along(Q, j, step, dist)
            if q is None: why = 'too short'; break
            why = 'Leviathan drives it (another pass)' if near(N, gN, q, 9) else "Leviathan's own shortcut" if near(own, go, q, 7) else None
            if why is None and clearance(N, {'pos': [q[0], q[2], q[1]], 'yaw': hd, 'half': width(pth)}) < 8: why = 'beside the road'; continue
            if why is None or why.startswith('Leviathan drives'): break
        if why: skipped.append(f"s {st:.0f} {pth.split('/')[-1]}: {why}"); continue
        twin = next((x for x in keep + added if x['kind'] == 'barricade' and math.hypot(q[0] - x['pos'][0], q[2] - x['pos'][1]) < 6 and abs((hd - x['yaw'] + 540) % 360 - 180) < 40), None)
        if twin: twin['half'] = max(twin.get('half', 0), width(pth)); skipped.append(f"s {st:.0f} {pth.split('/')[-1]}: the same way as {twin['at']}"); continue
        twin = next((x for x in keep + added if x['kind'] == 'barricade' and math.hypot(q[0] - x['pos'][0], q[2] - x['pos'][1]) < 8 and abs((hd - x['yaw'] + 540) % 360 - 180) > 140), None)
        if twin: twin['both'] = True; twin['half'] = max(twin.get('half', 0), width(pth)); twin['at'] += ' / ' + pth.split('/')[-1] + ' (both ways)'; skipped.append(f"s {st:.0f} {pth.split('/')[-1]}: closed from its other end"); continue
        added.append({'kind': 'barricade', 'pos': [round(q[0], 2), round(q[2], 2), round(q[1], 2)], 'yaw': round(hd, 1), 'half': width(pth), 'station': round(st, 1),
                      'at': '%s, %s Leviathan at s %.0f (1.3.0-review7)' % (pth.split('/')[-1], 'leaving' if a_ else 'joining', st)})
C['blocks'] = keep + added
say('Forward closed ways added on the new road: ' + ('; '.join(f"{b['at']} ({b['pos'][0]:.0f}, {b['pos'][1]:.0f})" for b in added) or 'none'))
say('Forward not closed there: ' + ('; '.join(skipped) or 'none'))
for b0 in C['blocks']:
    if b0['kind'] != 'barricade': continue
    d = clearance(N, b0); dc = clearance(own, b0)
    if d < 8 or dc < 4: say(f"  NOTE: closed way {b0['at']} comes {d:.1f} m from the line, {dc:.1f} m from a shortcut's line")
open(OUT, 'w', encoding='utf-8', newline='\n').write(json.dumps(C, separators=(',', ':')))
say('wrote ' + OUT)

# ================================================================ Part A: Leviathan Reverse, coming off the mountain
OUT = 'Assets/Resources/Leviathan/LeviathanReverse.json'
R = load(OUT); C = dict(R)
P = [tuple(R['pts'][i:i + 3]) for i in range(0, len(R['pts']), 3)]; S = stations(P, True)
gates = [dict(x) for x in R['gates']]
add = [(7318.0, 'end of the descent: the bend after CR117 CP 6'), (7405.0, 'J1: round the left turn onto the lake strip')]
for st, why in add:
    ng = L.new_gate(P, S, st, why, hw=9.0); ng.pop('seat', None); ng['station'] = st
    k = next(k for k, x in enumerate(gates) if k > 0 and x['station'] > st); gates.insert(k, ng)
    say(f"Reverse: new gate at s {st:.0f} ({why}): {[round(v, 1) for v in ng['pos']]} facing {ng['rot'][1]:.0f}, 18 m wide, 30 m tall; before {gates[k + 1]['name']}")
old_names = [x['name'] for x in R['gates']]
renumber(gates, 'Leviathan Reverse')
gmap = {k: next(j for j, x in enumerate(gates) if x['pos'] == R['gates'][k]['pos']) for k in range(len(R['gates']))}
C['gates'] = gates
assert all(gates[k]['station'] < gates[k + 1]['station'] for k in range(1, len(gates) - 1)), 'Reverse gates out of order'
brs = []
for b0 in R['branches']:
    b1 = dict(b0); b1['bypassedGates'] = [k for k, g_ in enumerate(gates) if k > 0 and b0['entryRoad'] < g_['station'] < b0['exitRoad']]; brs.append(b1)
    say(f"Reverse shortcut {b0['title']}: bypasses {b0['bypassedGates']} -> {b1['bypassedGates']}")
C['branches'] = brs
say('Reverse gates now: ' + ', '.join(f"{k} {x['name']} s {x['station']:.0f}" for k, x in enumerate(gates) if k >= 36))
# the rail at J1: from the climb's side of the junction (723.5, 4) to the far side of the straight-on road (734, 0), facing the traffic from
# CP 39 (heading north); a glancing hit from the south slides west along it (onto the route)
w0, w1 = (723.5, 4.0), (734.0, 0.0)
cx, cz = (w0[0] + w1[0]) / 2, (w0[1] + w1[1]) / 2; dxr, dzr = w1[0] - w0[0], w1[1] - w0[1]; half = math.hypot(dxr, dzr) / 2
ryaw = math.degrees(math.atan2(-dzr, dxr)) % 360  # right = (cos yaw, -sin yaw) along the rail
y0 = at(P, S, 7382, True)[1]
rail = {'kind': 'rail', 'pos': [round(cx, 2), round(cz, 2), round(y0, 2)], 'yaw': round(ryaw, 1), 'half': round(half, 2), 'station': 7382.0,
        'at': 'J1: the climb straight on from CP 39, closed this way (the climb is driven earlier in the lap; its own way past the rail stays open) (1.3.0-review7)'}
climb = [q for q, s in zip(P, S) if 4330 < s < 4420]; lapend = [q for q, s in zip(P, S) if 7350 < s < 7420]
say(f"Reverse rail at J1: centre ({cx:.1f}, {cz:.1f}), {2 * half:.1f} m long, facing traffic heading {ryaw:.0f}; nearest the climb pass's line {clearance(climb, rail):.1f} m, the lap end's {clearance(lapend, rail):.1f} m")
C['blocks'] = R['blocks'] + [rail]
signs = list(R['signs'])
r = math.radians(ryaw); rx, rz = math.cos(r), -math.sin(r); fx, fz = math.sin(r), math.cos(r)
for t in (-.55, 0, .55):
    px, pz = cx + rx * half * t - fx * 1.0, cz + rz * half * t - fz * 1.0
    signs.append({'kind': 'chevron', 'dir': -1, 'pos': [round(px, 2), round(pz, 2), round(y0, 2)], 'yaw': 0.0, 'at': 'J1: left onto the lake strip (on the rail)'})
C['signs'] = signs
open(OUT, 'w', encoding='utf-8', newline='\n').write(json.dumps(C, separators=(',', ':')))
say('wrote ' + OUT)
os.makedirs('Docs/Report1300/review7', exist_ok=True)
open('Docs/Report1300/review7/route.txt', 'w', encoding='utf-8').write('\n'.join(log) + '\n')
