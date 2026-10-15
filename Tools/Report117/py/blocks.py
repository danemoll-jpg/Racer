# 0.117 Part A: where another drivable way of the scene (its RaceRoads and trails, Report117Probe.Lines; on Leviathan Reverse also the Street
# Loop Forward's shortcut trails, which this scene has as ground) meets Leviathan's line and leaves it, and that way is not Leviathan's own
# (another pass of it or one of its shortcuts), the way is closed for the race: a barricade across it 14 m from the junction, facing the
# traffic that would turn into it, with a one-sided ROAD CLOSED sign. Hwy 92 (CR113) stays open: its traffic drives through.
# On Leviathan Reverse, Pine Ridge's end gets Street Loop Reverse's own NO ENTRY block (Dan, BUG-008) instead.
# Writes "blocks" into Assets/Resources/Leviathan/Leviathan{Forward,Reverse}.json and the list to Docs/Report117/blocks.txt.
# usage: python Tools/Report117/py/blocks.py <lines dir>
import json, math, sys, os
LINES = sys.argv[1]
OPEN = ('CR113',)  # never closed
WIDTH = {'Forest race route': 4.5, 'House 3 valley driveway': 4.0, 'Lake shore connection': 5.0, 'Mountain return trails': 5.0, 'Summit approach': 6.0,
         'Summit Traverse': 6.5, 'two-flight racing line': 7.0, 'Mountain racing line': 7.0, 'Creek Leap': 4.5, 'Fox Gully': 4.5, 'Pine Ridge': 4.5, 'Southwest Cut': 4.5}
def width(path): return next((w for k, w in WIDTH.items() if k in path), 6.0)
INTO = {'House 3 valley driveway': 8}
# wider than the way itself where open grass beside it would let a car round it (Forward: the lake path's west side north of the driveway link)
WIDER = [('LeviathanForward', (529.9, 9.9), 9.0)]  # how far in the barricade stands (default 14 m): House 3's drive drops steeply into its valley
# junctions with no way to close: the Summit approach / safe return line runs over the summit landing's grass slope there (no road surface)
NOWAY = [('LeviathanForward', 'Summit approach', 2300, 2420), ('LeviathanReverse', 'Summit approach', 5600, 5700)]
def stations(P):
    S = [0.0]
    for i in range(1, len(P)): S.append(S[-1] + math.dist(P[i], P[i - 1]))
    return S
def grid(P, cell=12):
    g = {}
    for i, q in enumerate(P): g.setdefault((int(q[0] // cell), int(q[2] // cell)), []).append(i)
    return g
def near(P, g, q, r, dy=5, cell=12):
    best = None; cx, cz = int(q[0] // cell), int(q[2] // cell); n = 1 + int(r // cell)
    for dx in range(-n, n + 1):
        for dz in range(-n, n + 1):
            for i in g.get((cx + dx, cz + dz), []):
                d = math.hypot(P[i][0] - q[0], P[i][2] - q[2])
                if d < r and abs(P[i][1] - q[1]) < dy and (best is None or d < best[0]): best = (d, i)
    return best
def along(Q, j, step, d):
    """the point d m along polyline Q from index j (step +1 / -1), and the heading of travel there"""
    acc = 0; k = j
    while 0 <= k + step < len(Q):
        seg = math.dist(Q[k], Q[k + step])
        if acc + seg >= d:
            t = (d - acc) / seg; p = tuple(Q[k][c] + (Q[k + step][c] - Q[k][c]) * t for c in range(3)); a, b = Q[k], Q[k + step]
            return p, math.degrees(math.atan2(b[0] - a[0], b[2] - a[2])) % 360
        acc += seg; k += step
    return None, None
NOENTRY = {'kind': 'noentry', 'pos': [-608.3, -241.6, 7.4], 'yaw': 129.9, 'mesh': 'LeviathanPineRidgeExitLip', 'at': "Pine Ridge's end (Street Loop Reverse's NO ENTRY block)"}
out = []
for name, scene in (('LeviathanForward', 'MountainLoop'), ('LeviathanReverse', 'MountainLoopReverse')):
    path = 'Assets/Resources/Leviathan/%s.json' % name; R = json.load(open(path))
    P = [tuple(R['pts'][i:i + 3]) for i in range(0, len(R['pts']), 3)]; S = stations(P); g = grid(P)
    own = []  # Leviathan's own shortcuts: open, marked
    for b in R['branches']: own += [tuple(b['pts'][i:i + 3]) for i in range(0, len(b['pts']), 3)]
    go = grid(own) if own else {}
    others = [(l['path'], [tuple(l['pts'][i:i + 3]) for i in range(0, len(l['pts']), 3)]) for l in json.load(open('%s/lines-%s.json' % (LINES, scene))) if l['active']]
    if R['reverse']:
        F = json.load(open('Assets/Resources/Leviathan/LeviathanForward.json'))
        others += [('Street Loop Forward shortcut trail/' + b['title'], [tuple(b['pts'][i:i + 3]) for i in range(0, len(b['pts']), 3)]) for b in F['branches']]
    blocks = [dict(NOENTRY)] if R['reverse'] else []
    # Reverse: the stem's lake end opens off Rocky Way Cut's run along the west side (a shortcut, not the main line, so not found above)
    if R['reverse']: blocks.append({'kind': 'barricade', 'pos': [518.5, -71.6, 82.2], 'yaw': 294.0, 'half': 5.0, 'station': 4091.0, 'at': 'Lake shore connection (the stem), off Rocky Way Cut'})
    skipped = []
    for pth, Q in others:
        if any(o in pth for o in OPEN): continue
        on = [near(P, g, q, 7, 4) for q in Q]
        for k in range(1, len(Q)):
            a, b = on[k - 1], on[k]
            if (a is None) == (b is None): continue
            j, step = (k - 1, 1) if a else (k, -1)  # the last point on Leviathan's line, and the way away from it
            if any(on[m] for m in range(k, min(len(Q), k + 6))) if a else any(on[m] for m in range(max(0, k - 6), k)): continue
            st = S[on[j][1]]; why = None
            if any(n == name and k in pth and a0 <= st <= a1 for n, k, a0, a1 in NOWAY): skipped.append('  s %6.0f %-60s no road there (a line over the grass)' % (st, pth.split('/')[-1][:60])); continue
            d0 = next((v for k, v in INTO.items() if k in pth), 14)
            for dist in (d0, d0 + 8, d0 + 16):  # 14 m in; further when that point is beside the course or one of its shortcuts
                q, hd = along(Q, j, step, dist)
                if q is None: why = 'too short'; break
                why = 'Leviathan drives it (another pass)' if near(P, g, q, 9) else "Leviathan's own shortcut" if own and near(own, go, q, 7) else None
                if why is None or why.startswith('Leviathan drives'): break
            if why: skipped.append('  s %6.0f %-60s %s' % (st, pth.split('/')[-1][:60], why)); continue
            twin = next((x for x in blocks if math.hypot(q[0] - x['pos'][0], q[2] - x['pos'][1]) < 6 and abs((hd - x['yaw'] + 540) % 360 - 180) < 40), None)
            if twin: twin['half'] = max(twin.get('half', 0), width(pth)); skipped.append('  s %6.0f %-60s the same way as %s' % (st, pth.split('/')[-1][:60], twin['at'])); continue
            # the same short stretch closed from both of its ends (a junction each side): one barricade, a sign facing each way, side by side
            twin = next((x for x in blocks if x['kind'] == 'barricade' and math.hypot(q[0] - x['pos'][0], q[2] - x['pos'][1]) < 8 and abs((hd - x['yaw'] + 540) % 360 - 180) > 140), None)
            if twin: twin['both'] = True; twin['half'] = max(twin.get('half', 0), width(pth)); twin['at'] += ' / ' + pth.split('/')[-1] + ' (both ways)'; skipped.append('  s %6.0f %-60s closed from its other end by %s' % (st, pth.split('/')[-1][:60], twin['at'])); continue
            blocks.append({'kind': 'barricade', 'pos': [round(q[0], 2), round(q[2], 2), round(q[1], 2)], 'yaw': round(hd, 1), 'half': width(pth), 'station': round(st, 1),
                           'at': '%s, %s Leviathan at s %.0f' % (pth.split('/')[-1], 'leaving' if a else 'joining', st)})
    for n, (x, z), hw in WIDER:
        for b in blocks:
            if n == name and math.hypot(b['pos'][0] - x, b['pos'][1] - z) < 3: b['half'] = hw
    R['blocks'] = blocks
    open(path, 'w', encoding='utf-8', newline='\n').write(json.dumps(R, separators=(',', ':')))
    out.append('== %s: %d blocks' % (name, len(blocks)))
    for x in sorted(blocks, key=lambda x: x.get('station', 0)): out.append('  %-10s (%.0f, %.0f) yaw %3.0f  half %.1f  %s' % (x['kind'], x['pos'][0], x['pos'][1], x['yaw'], x.get('half', 0), x['at']))
    out.append('  not closed:'); out += skipped
os.makedirs('Docs/Report117', exist_ok=True); open('Docs/Report117/blocks.txt', 'w', encoding='utf-8').write('\n'.join(out) + '\n'); print('\n'.join(out))
