# 0.117 Part A / E: the decision-point audit's list: every turn the course makes off a road (its arrow boards), every way closed for the race
# (its blocks), and Dan's 14 report spots. For each, a driver's-view spot: 35 m before it on the course (Dan's: his own position and heading),
# with the course station there on the new line and on the 0.116 line (the race's gate progress is set from it for the shot: the next gate drawn
# and the HUD's NEXT CP as a driver there would have them). Writes Docs/Report117/audit-items.json (read by Report106PlayerCheck audit117).
# usage: python Tools/Report117/py/audit.py
import json, math, subprocess
def load(R):
    P = [tuple(R['pts'][i:i + 3]) for i in range(0, len(R['pts']), 3)]; S = [0.0]
    for i in range(1, len(P)): S.append(S[-1] + math.dist(P[i], P[i - 1]))
    return P, S
def at(P, S, s):
    s %= S[-1]; i = max(0, min(len(S) - 2, next((k for k in range(len(S) - 1) if S[k + 1] > s), len(S) - 2))); t = (s - S[i]) / max(1e-6, S[i + 1] - S[i])
    return tuple(P[i][c] + (P[i + 1][c] - P[i][c]) * t for c in range(3))
def hdg(P, S, s): a, b = at(P, S, s - 4), at(P, S, s + 4); return math.degrees(math.atan2(b[0] - a[0], b[2] - a[2])) % 360
def station(P, S, x, z, yaw, maxd=25):
    """the station nearest (x, z) on the pass whose heading is within 60 degrees of yaw"""
    best = None
    for i in range(len(P) - 1):
        a, b = P[i], P[i + 1]; vx, vz = b[0] - a[0], b[2] - a[2]; L = vx * vx + vz * vz or 1e-9
        t = max(0, min(1, ((x - a[0]) * vx + (z - a[2]) * vz) / L)); d = math.hypot(x - a[0] - vx * t, z - a[2] - vz * t)
        if d > maxd: continue
        h = math.degrees(math.atan2(vx, vz)) % 360
        if abs((h - yaw + 540) % 360 - 180) > 60: continue
        if best is None or d < best[0]: best = (d, S[i] + t * (S[i + 1] - S[i]))
    return best[1] if best else None
BUGS = [('BUG-001', 'F', 467.16, 44.87, 195.12), ('BUG-002', 'F', 480.28, -21.22, 93.46), ('BUG-003', 'F', 603.37, -110.75, 103.09), ('BUG-004', 'F', 740.26, -119.49, 96.23),
        ('BUG-005', 'F', 716.48, -78.51, 194.23), ('BUG-006', 'F', 727.53, -118.56, 110.07), ('BUG-007', 'R', -640.22, 114.81, 164.43), ('BUG-008', 'R', -622.64, -242.10, 132.10),
        ('BUG-009', 'R', 508.56, -481.04, 74.25), ('BUG-010', 'R', 531.88, -44.53, 39.24), ('BUG-011', 'R', 499.33, -16.80, 354.91), ('BUG-012', 'R', 577.34, 55.12, 51.51),
        ('BUG-013', 'R', 820.24, -190.06, 296.52), ('BUG-014', 'R', 480.71, -17.80, 293.20)]
out = {}
for name, tag in (('LeviathanForward', 'F'), ('LeviathanReverse', 'R')):
    R = json.load(open('Assets/Resources/Leviathan/%s.json' % name)); P, S = load(R)
    O = json.loads(subprocess.run(['git', 'show', 'f05957ef:Assets/Resources/Leviathan/%s.json' % name], capture_output=True, text=True, encoding='utf-8').stdout); PO, SO = load(O)
    items = []
    def add(kind, label, s_turn, x=None, z=None, yaw=None):
        if x is None:
            s = s_turn - 35; p = at(P, S, s); x, z, yaw = p[0], p[2], hdg(P, S, s)
        sn = station(P, S, x, z, yaw); so = station(PO, SO, x, z, yaw)
        if sn is None: sn = station(P, S, x, z, yaw, 60) or station(P, S, x, z, (yaw + 90) % 360, 60) or station(P, S, x, z, (yaw + 180) % 360, 60)
        items.append({'kind': kind, 'label': label, 'x': round(x, 2), 'z': round(z, 2), 'yaw': round(yaw, 1), 'sNew': round(sn, 1) if sn is not None else -1, 'sOld': round(so, 1) if so is not None else -1})
    for sg in R['signs']:
        if sg['kind'] != 'arrow': continue
        s = station(P, S, sg['pos'][0], sg['pos'][1], sg['yaw'], 20)
        if s is None: print('  no station for the arrow', sg['at']); continue
        # the turn: the arrow stands D m before it; find the sharpest bend within 70 m after the arrow
        add('turn', sg['at'], s + 10)  # 25 m before its arrow board: the board, its chevrons and the turn in view
    for b in R['blocks']:
        st = b.get('station')
        if b['kind'] == 'noentry': add('closed', b['at'], 0, -622.64, -242.10, 132.10); continue
        add('closed', b['at'], st)
        # and close up: 12 m in front of it on the closed way, facing it (as a driver turning in would see it)
        yr = math.radians(b['yaw']); fx, fz = math.sin(yr), math.cos(yr)
        items.append({'kind': 'closeup', 'label': b['at'], 'x': round(b['pos'][0] - fx * 12, 2), 'z': round(b['pos'][1] - fz * 12, 2), 'yaw': b['yaw'], 'sNew': round(st, 1), 'sOld': -1, 'y': b['pos'][2]})
    for bug, d, x, z, yaw in BUGS:
        if d == tag: add('report', bug, 0, x, z, yaw)
    items.sort(key=lambda it: it['sNew'])
    out[name] = items; print(name, len(items), 'items;', sum(1 for i in items if i['sNew'] < 0), 'without a station on the new line,', sum(1 for i in items if i['sOld'] < 0), 'on the 0.116 line')
open('Docs/Report117/audit-items.json', 'w', encoding='utf-8').write(json.dumps(out, indent=1))
