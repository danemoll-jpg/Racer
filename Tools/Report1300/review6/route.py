# 1.3.0-review6 Part A (Dan: "You are forced to go this way as opposed to the second jump, which kind of kills the point of taking the path with
# the big jump for stunt scores"): Leviathan Reverse's route goes back over the Mountain Loop Reverse's own main road with the second big jump.
# Since 0.115 (Tools/Report115/py/leviathan.py) the line left CR117 at Downhill Ridge Cut's entry (CR117 s 1493, just past CR117 CP 3) and
# drove the cut to its end on the start stretch (CR117 s 12); the CR117 road it bypasses (CR117 CP 4, CP 5, the Westbound Gully Flight, CP 6,
# CR117 s 1493-3031) was barricaded as ROAD CLOSED in 0.117. Here, in the current course file (every later round's work kept):
#   - the line's piece from CR117 s 1470 to CR117 s 30 (the cut and the blends onto / off it) is replaced by CR117's own centreline over that
#     span (its points as they are in MountainLoopReverse.unity: Docs/Report115/Probe, the scene unchanged since 0.112);
#   - the cut's gate (CP 35 "Downhill Ridge Cut") is replaced by the scene's own CR117 CP 4, CP 5 and CP 6; the later gates renumbered;
#   - the flight "Downhill Ridge Cut drop" (on the line) goes; CR117's Westbound Gully Flight comes in at its stations on the new line;
#   - Downhill Ridge Cut becomes one of the course's shortcuts (the scene's own branch: its points and settings, not AI-validated as in the
#     Mountain Loop Reverse), bypassing the three gates, with a shortcut board at its entry and REJOIN ROAD at its end;
#   - the closed ways (barricades) in the replaced span are found again on the new line (Tools/Report117/py/blocks.py's rule), the others kept;
#   - the turn chevrons onto the cut and off it, and the scene-sign relabels that called the cut Leviathan's road, go;
#   - every station after the span moves by the change in length; the course id becomes leviathan-reverse-v5 (the lap is longer: new boards).
# usage (project root): python Tools/Report1300/review6/route.py   (reads the course file at 1c8c6bb3, so it can be run again)
import json, math, os, subprocess, sys
sys.path.insert(0, 'Tools/Report115/py')
import leviathan as L
from leviathan import stations, at, project, piece

OUT = 'Assets/Resources/Leviathan/LeviathanReverse.json'
log = []
def say(*a):
    s = ' '.join(str(x) for x in a); print(s); log.append(s)

raw = subprocess.run(['git', 'show', '1c8c6bb32:' + OUT], capture_output=True, text=True, encoding='utf-8').stdout
R = json.loads(raw)
P = [tuple(R['pts'][i:i + 3]) for i in range(0, len(R['pts']), 3)]; S = stations(P, True); Lold = S[-1]
sc = L.scene('MountainLoopReverse'); cr = sc['roads'][L.CR117]['points']; Cs = stations(cr, True); CL = Cs[-1]
drc = sc['branches']['Downhill Ridge Cut']

C0, C1 = 1470.0, 30.0  # CR117 stations: the line is on CR117 there (before the cut's entry blend, after its end on the start stretch)
def nearest(q, lo, hi):
    return min((i for i in range(len(P)) if lo <= S[i] <= hi), key=lambda i: math.dist(P[i], q))
i0 = nearest(at(cr, Cs, C0, True), 5650, 5850); i1 = nearest(at(cr, Cs, C1, True), 5950, 6100)
say(f'line on CR117 s {C0:.0f}: point {i0} s {S[i0]:.1f} ({math.dist(P[i0], at(cr, Cs, C0, True)):.2f} m); CR117 s {C1:.0f}: point {i1} s {S[i1]:.1f} ({math.dist(P[i1], at(cr, Cs, C1, True)):.2f} m)')
assert math.dist(P[i0], at(cr, Cs, C0, True)) < .6 and math.dist(P[i1], at(cr, Cs, C1, True)) < .6
# CR117 from just past the line's point i0 to just before i1 (its own vertices; wrapping its start)
c_a, _ = project(cr, P[i0], True); c_b, _ = project(cr, P[i1], True)
mid = [q for q, s in zip(cr, Cs) if s > c_a + .5] + [q for q, s in zip(cr, Cs) if s < c_b - .5]
N = P[:i0 + 1] + mid + P[i1:]
NS = stations(N, True); Lnew = NS[-1]; j1 = i0 + 1 + len(mid)  # N[j1] is old P[i1]
delta = NS[j1] - S[i1]
say(f'replaced: line s {S[i0]:.1f}-{S[i1]:.1f} ({S[i1] - S[i0]:.1f} m: the cut) by CR117 s {c_a:.1f}-{CL:.1f}-{c_b:.1f} ({NS[j1] - NS[i0]:.1f} m, {len(mid)} points); lap {Lold:.1f} -> {Lnew:.1f} m (+{delta:.1f})')
def move(s):
    if s <= S[i0] + 1e-6: return s
    if s >= S[i1] - 1e-6: return s + delta
    return None  # inside the replaced span

C = dict(R)
C['id'] = 'leviathan-reverse-v5'; C['length'] = Lnew
C['pts'] = [round(v, 3) for q in N for v in q]
g = R['geometryStations']; C['geometryStations'] = g[:i0 + 1] + [g[i0]] * len(mid) + g[i1:]
assert len(C['geometryStations']) == len(N)
C['seat'] = [i if i <= i0 else i + (j1 - i1) for i in R['seat'] if i <= i0 or i >= i1]
# parts
parts = []
for p in R['parts']:
    a, b = move(p['from']), move(p['to'])
    if a is not None and b is not None: parts.append({'name': p['name'], 'from': round(a, 1), 'to': round(b, 1)})
    elif a is not None: parts.append({'name': p['name'], 'from': round(a, 1), 'to': round(NS[i0], 1)})
    elif b is not None: parts.append({'name': p['name'], 'from': round(NS[j1], 1), 'to': round(b, 1)})
parts.append({'name': 'CR117 Westbound Gully Flight road (1.3.0-review6)', 'from': round(NS[i0], 1), 'to': round(NS[j1], 1)})
C['parts'] = sorted(parts, key=lambda p: p['from'])
def spans(v): return [round(move(x) if move(x) is not None else x, 1) for x in v]
C['mountain'] = spans(R['mountain']); C['narrow'] = spans(R['narrow'])
say(f"mountain span {R['mountain']} -> {C['mountain']}; narrow {R['narrow']} -> {C['narrow']}")

# gates: the cut's CP 35 out, CR117 CP 4, 5, 6 in (the scene's own), the rest renumbered
old_gates = R['gates']; k_cut = next(k for k, x in enumerate(old_gates) if x['name'].endswith('(Downhill Ridge Cut)'))
new_gates = old_gates[:k_cut]
for n in (4, 5, 6):
    path = f'CR117 selected-direction guidance/CR117 CP {n}'; sg = sc['gates'][path]
    e = L.gate_entry(sg, f'CR117 CP {n}', True, path); new_gates.append(e)
new_gates += old_gates[k_cut + 1:]
import re
for k, x in enumerate(new_gates):
    if k == 0: continue
    label = x['name']; m = re.match(r'Leviathan Reverse CP \d+(.*)$', label)
    rest = m.group(1) if m else f' ({label})'
    x['name'] = f'Leviathan Reverse CP {k}{rest}'
for k, x in enumerate(new_gates):
    if 'station' in x and move(x['station']) is not None: x['station'] = move(x['station']); continue
    st, d = project(N, x['pos'], True, NS[i0], NS[j1]); x['station'] = st
    say(f"  {x['name']} on the new road at s {st:.1f} ({d:.2f} m from the line)")
assert all(new_gates[k]['station'] < new_gates[k + 1]['station'] for k in range(1, len(new_gates) - 1)), 'gates out of order'
say(f"START FINISH s {old_gates[0]['station']:.1f} -> {new_gates[0]['station']:.1f}")
# Part B: a gate right after the Westbound Gully Flight's landing, seen as you come down (the next one, CR117 CP 6, is 250 m on round a bend)
wg0 = next(f for f in sc['flights'] if f['name'] == 'Westbound Gully Flight')
land_s, _ = project(N, at(cr, Cs, wg0['endStation'], True), True, NS[i0], NS[j1])
k_land = next(k for k, x in enumerate(new_gates) if k > 0 and x['station'] > land_s)
lg = L.new_gate(N, NS, land_s + 35, 'after the Westbound Gully Flight landing'); lg.pop('seat', None); lg['station'] = land_s + 35
new_gates.insert(k_land, lg)
for k, x in enumerate(new_gates):
    if k == 0: continue
    m = re.match(r'Leviathan Reverse CP \d+(.*)$', x['name']); x['name'] = f"Leviathan Reverse CP {k}{m.group(1) if m else ' (' + x['name'] + ')'}"
say(f"gate after the gully landing: {new_gates[k_land]['name']} at s {land_s + 35:.1f} (landing ends at s {land_s:.1f}), {new_gates[k_land]['pos']}")
# Part C: the gates a Stunt Race's kicker throws you over (Review5: every racer missed CP 17, rivals CP 21 and CP 43: each 36-40 m past a kicker,
# 3 m tall): as tall as the mountain gates (30 m), so a jump through them counts; the Stunt Race bumps (Resources/StuntBumps.json) stay as they are
bumps = next(c for c in json.load(open('Assets/Resources/StuntBumps.json'))['courses'] if c['course'] == 'leviathan-reverse')['bumps']
for bp in bumps:
    bs, bd = project(N, (bp['pos'][0], bp['pos'][2], bp['pos'][1]), True)
    for k, x in enumerate(new_gates):
        if k > 0 and bs < x['station'] < bs + 120 and x['upperHeight'] < 30:
            say(f"  {x['name']}: {x['station'] - bs:.0f} m past the Stunt Race's {bp['at']} ({bp['height']} m): upper height {x['upperHeight']} -> 30 m")
            x['upperHeight'] = 30.0
# Part C (the traced AI Stunt Race after the first fix: the player's road AI missed CP 11, a rival CP 21): the gates racers pass beside, 18 m wide
# (the road with its shoulders: a car landing off a kicker, 3 m off the line, or swinging wide is still in it)
for k in (11, 17, 21):
    new_gates[k]['halfWidth'] = max(new_gates[k]['halfWidth'], 9.0); say(f"  {new_gates[k]['name']}: 18 m wide")
k46 = next(k for k, x in enumerate(new_gates) if k > 40 and abs(x['pos'][0] - 449.9) < 1 and abs(x['pos'][2] - 112.6) < 1)
new_gates[k46]['halfWidth'] = max(new_gates[k46]['halfWidth'], 9.0); say(f"  {new_gates[k46]['name']}: 18 m wide")
C['gates'] = new_gates
cut_gates = [k for k, x in enumerate(new_gates) if '(CR117 CP 4)' in x['name'] or '(CR117 CP 5)' in x['name'] or '(CR117 CP 6)' in x['name']]
say('gates: ' + ', '.join(f"{k} {x['name']} s {x['station']:.0f}" for k, x in enumerate(new_gates) if k >= k_cut - 1))

# flights
fl = [f for f in R['flights'] if f['name'] != 'Downhill Ridge Cut drop']
for f in fl:
    f['approachStation'] = move(f['approachStation']); f['endStation'] = move(f['endStation'])
wg = next(f for f in sc['flights'] if f['name'] == 'Westbound Gully Flight')
a, da = project(N, at(cr, Cs, wg['approachStation'], True), True, NS[i0], NS[j1]); b, db = project(N, at(cr, Cs, wg['endStation'], True), True, NS[i0], NS[j1])
fl.append(dict(wg, approachStation=a, endStation=b)); C['flights'] = fl
say(f"flight Westbound Gully Flight: CR117 {wg['approachStation']:.1f}-{wg['endStation']:.1f} -> Leviathan {a:.1f}-{b:.1f} ({da:.2f} / {db:.2f} m); 'Downhill Ridge Cut drop' removed")

# Downhill Ridge Cut: a shortcut (the scene's own branch)
dp = drc['points']; e, de = project(N, dp[0], True, NS[i0], NS[j1]); x, dx = project(N, dp[-1], True, NS[i0], NS[j1] + 40)
br = {k: drc[k] for k in ('title', 'halfWidth', 'recommendedSpeed', 'aiValidated', 'undergroundStart', 'undergroundEnd', 'entryHeightBelow', 'entryHeightAbove', 'entrySpeed', 'entrySpeedDistance', 'entryInset', 'entryMargin')}
br |= {'from': 'Mountain Loop Reverse: ' + drc['path'] + ' (1.3.0-review6)', 'pts': [round(v, 3) for q in dp for v in q], 'aiPts': [round(v, 3) for q in drc['aiPoints'] for v in q],
       'entryRoad': e, 'exitRoad': x, 'bypassedGates': [k for k, g_ in enumerate(new_gates) if k > 0 and e < g_['station'] < x]}
for b0 in R['branches']: assert move(b0['entryRoad']) == b0['entryRoad'] and move(b0['exitRoad']) == b0['exitRoad'] and max(b0['bypassedGates']) < k_cut
C['branches'] = R['branches'] + [br]
say(f"shortcut Downhill Ridge Cut: entry s {e:.1f} ({de:.2f} m) exit s {x:.1f} ({dx:.2f} m), bypasses {br['bypassedGates']}, AI-validated {drc['aiValidated']}")

# signs: the chevrons onto the cut and off it go; a board at its entry and its end
def yaw_at(s):
    p = at(N, NS, s, True); q = at(N, NS, s + 3, True); return math.degrees(math.atan2(q[0] - p[0], q[2] - p[2])) % 360, p
signs = [s for s in R['signs'] if s['at'] not in ('Downhill Ridge Cut', 'off the mountain: the start stretch')]
yw, p = yaw_at(e - 40); r = math.radians(yw); right = (math.cos(r), -math.sin(r))
# the cut leaves to the left: its board on the left of the road, 40 m before
signs.append({'kind': 'board', 'text': '<  DOWNHILL RIDGE CUT', 'dir': -1, 'pos': [round(p[0] - right[0] * 8, 1), round(p[2] - right[1] * 8, 1), round(p[1], 1)], 'alt': [round(p[0] + right[0] * 8, 1), round(p[2] + right[1] * 8, 1), round(p[1], 1)], 'yaw': round(yw, 1), 'at': 'Downhill Ridge Cut entry'})
qq = at(dp, stations(dp), stations(dp)[-1] - 25); q2 = at(dp, stations(dp), stations(dp)[-1] - 22); yw2 = math.degrees(math.atan2(q2[0] - qq[0], q2[2] - qq[2])) % 360; r2 = math.radians(yw2)
signs.append({'kind': 'board', 'text': 'REJOIN  ROAD', 'dir': 1, 'pos': [round(qq[0] - math.cos(r2) * 6, 1), round(qq[2] + math.sin(r2) * 6, 1), round(qq[1], 1)], 'yaw': round(yw2, 1), 'at': 'Downhill Ridge Cut rejoin'})
C['signs'] = signs
C['relabel'] = [x for x in R['relabel'] if x['find'] not in ('FOLLOW GOLD', 'SHORTCUT 2')]
say('signs: the 4 chevrons for the cut out; boards at its entry and end; relabels kept: ' + ', '.join(x['find'] for x in C['relabel']))

def clearance(line, b):
    # the nearest a barricade (its whole width) comes to a line, in plan, at that line's height (4 m)
    x, z, y = b['pos']; r = math.radians(b['yaw']); rx, rz = math.cos(r), -math.sin(r); h = b.get('half', 5); best = 1e9
    for p in line:
        if abs(p[1] - y) > 4: continue
        t = max(-h, min(h, (p[0] - x) * rx + (p[2] - z) * rz)); best = min(best, math.hypot(p[0] - x - rx * t, p[2] - z - rz * t))
    return best
# closed ways: kept outside the span (stations moved); found again inside it on the new line (blocks.py's rule)
keep, gone = [], []
for b0 in R['blocks']:
    st = b0.get('station')
    b1 = dict(b0)
    if st is not None and S[i0] - 20 < st < S[i1] + 20:
        # in the replaced span: the ones across CR117 go; the ones closing a way off the cut (now the shortcut) stay, at their new station
        if clearance(N, b0) < 8: gone.append(b0); continue
        b1['station'] = round(project(N, (b0['pos'][0], b0['pos'][2], b0['pos'][1]), True, NS[i0], NS[j1] + 40)[0], 1); b1['at'] += ' (kept: off Downhill Ridge Cut, 1.3.0-review6)'; keep.append(b1); continue
    if st is not None: b1['station'] = round(move(st), 1)
    keep.append(b1)
say('closed ways removed (on or beside the replaced span): ' + '; '.join(f"{b['at']} ({b['pos'][0]:.0f}, {b['pos'][1]:.0f})" for b in gone))
sys.argv = ['blocks', 'Docs/Report117/Probe']
src = open('Tools/Report117/py/blocks.py', encoding='utf-8').read()
helpers = src[:src.index('NOENTRY =')]
ns = {}; exec(compile(helpers.split('LINES = sys.argv[1]')[0] + helpers.split('LINES = sys.argv[1]')[1], 'blocks.py', 'exec'), ns)
near, grid, along, width, OPENW, INTO = ns['near'], ns['grid'], ns['along'], ns['width'], ns['OPEN'], ns['INTO']
g = grid(N); own = []
for b0 in C['branches']: own += [tuple(b0['pts'][i:i + 3]) for i in range(0, len(b0['pts']), 3)]
go = grid(own)
others = [(l['path'], [tuple(l['pts'][i:i + 3]) for i in range(0, len(l['pts']), 3)]) for l in json.load(open('Docs/Report117/Probe/lines-MountainLoopReverse.json')) if l['active']]
added, skipped = [], []
lo, hi = NS[i0] - 20, NS[j1] + 20
for pth, Q in others:
    if any(o in pth for o in OPENW) or 'Downhill Ridge Cut' in pth: continue
    on = [near(N, g, q, 7, 4) for q in Q]
    for k in range(1, len(Q)):
        a_, b_ = on[k - 1], on[k]
        if (a_ is None) == (b_ is None): continue
        j, step = (k - 1, 1) if a_ else (k, -1)
        if any(on[m] for m in range(k, min(len(Q), k + 6))) if a_ else any(on[m] for m in range(max(0, k - 6), k)): continue
        st = NS[on[j][1]]
        if not lo <= st <= hi: continue
        why = None; d0 = next((v for kk, v in INTO.items() if kk in pth), 14)
        # (review6: under the Westbound Gully Flight the scene's line is the ground below the flight's arc: the same road, not a way off it)
        if 'two-flight racing line' in pth and any(f['approachStation'] <= st <= f['endStation'] for f in fl if f['name'] == 'Westbound Gully Flight'): skipped.append(f"s {st:.0f} {pth.split('/')[-1]}: the ground under the Westbound Gully Flight (the same road)"); continue
        for dist in (d0, d0 + 8, d0 + 16, d0 + 24):
            q, hd = along(Q, j, step, dist)
            if q is None: why = 'too short'; break
            why = 'Leviathan drives it (another pass)' if near(N, g, q, 9) else "Leviathan's own shortcut" if near(own, go, q, 7) else None
            # review6: the whole barricade 8 m clear of the line (at CR117 CP 4 the way in runs beside the road)
            if why is None and clearance(N, {'pos': [q[0], q[2], q[1]], 'yaw': hd, 'half': width(pth)}) < 8: why = 'beside the road'; continue
            if why is None or why.startswith('Leviathan drives'): break
        if why: skipped.append(f"s {st:.0f} {pth.split('/')[-1]}: {why}"); continue
        twin = next((x for x in keep + added if x['kind'] == 'barricade' and math.hypot(q[0] - x['pos'][0], q[2] - x['pos'][1]) < 6 and abs((hd - x['yaw'] + 540) % 360 - 180) < 40), None)
        if twin: twin['half'] = max(twin.get('half', 0), width(pth)); skipped.append(f"s {st:.0f} {pth.split('/')[-1]}: the same way as {twin['at']}"); continue
        twin = next((x for x in keep + added if x['kind'] == 'barricade' and math.hypot(q[0] - x['pos'][0], q[2] - x['pos'][1]) < 8 and abs((hd - x['yaw'] + 540) % 360 - 180) > 140), None)
        if twin: twin['both'] = True; twin['half'] = max(twin.get('half', 0), width(pth)); twin['at'] += ' / ' + pth.split('/')[-1] + ' (both ways)'; skipped.append(f"s {st:.0f} {pth.split('/')[-1]}: closed from its other end"); continue
        added.append({'kind': 'barricade', 'pos': [round(q[0], 2), round(q[2], 2), round(q[1], 2)], 'yaw': round(hd, 1), 'half': width(pth), 'station': round(st, 1),
                      'at': '%s, %s Leviathan at s %.0f (1.3.0-review6)' % (pth.split('/')[-1], 'leaving' if a_ else 'joining', st)})
C['blocks'] = keep + added
say('closed ways added on the new road: ' + ('; '.join(f"{b['at']} ({b['pos'][0]:.0f}, {b['pos'][1]:.0f})" for b in added) or 'none'))
say('not closed there: ' + ('; '.join(skipped) or 'none'))
# every closed way vs the new line: none across it
for b0 in C['blocks']:
    if b0['kind'] != 'barricade': continue
    d = clearance(N, b0); dc = clearance(own, b0)
    if d < 8 or dc < 4: say(f"  NOTE: closed way {b0['at']} comes {d:.1f} m from the line, {dc:.1f} m from a shortcut's line")
open(OUT, 'w', encoding='utf-8', newline='\n').write(json.dumps(C, separators=(',', ':')))
os.makedirs('Docs/Report1300/review6/leviathan', exist_ok=True)
open('Docs/Report1300/review6/leviathan/route.txt', 'w', encoding='utf-8').write('\n'.join(log) + '\n')
say('wrote ' + OUT)
