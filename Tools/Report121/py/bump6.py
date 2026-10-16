# 0.121 (BUG-005): lists the places 0.120's rules allow for one bump in a station range (bump6.py <probe dir> <from> <to>); from 0.120's bumps.py. 0.120 Part C: places the Stunt Race bumps on the four Stunt Race courses from the course probes (Report106PlayerCheck -p106kind probe120:
# line.csv, gates.csv, flights.csv, speed.csv in <probe dir>/<name>/). Writes the bumps file (Assets/Resources/StuntBumps.json format) and a list.
#   python bumps.py <probe root> <out json> <out list.md>
# A bump is an earth wedge (LeviathanCourse.Kicker) beside the course line, on one half of the road (the line itself stays clear), on even
# ground with nothing standing there, on a straight, away from the gates, the big jumps and the start, never where another part of the course
# passes within 15 m (Leviathan drives some roads twice: a bump must never be met from behind). Its flight is worked out from the rivals' speed
# there (the probe's AI lap) over the ground ahead: it must land on level ground or a down-slope (never uphill), clear of anything standing, within the course's width.
import csv, json, math, sys, os

COURSES = [("probe4", "mountain-forward", "Mountain Loop Forward"), ("probe5", "mountain-reverse", "Mountain Loop Reverse"),
           ("probe8", "leviathan-forward", "Leviathan Forward"), ("probe9", "leviathan-reverse", "Leviathan Reverse")]
NORMAL = dict(height=1.5, length=4.5, width=3.6, air=1.5)  # one trick type at racing speed (a 360 takes about 1.2-1.5 s at the stick rates)
BIG = dict(height=3.0, length=7.0, width=4.0, air=2.3)     # two trick types
G = 9.81

def rows(path):
    with open(path, newline='') as f: return list(csv.DictReader(f))

def num(v):
    try: return float(v)
    except: return float('nan')

def place(folder, title):
    line = rows(os.path.join(folder, 'line.csv')); gates = rows(os.path.join(folder, 'gates.csv')); flights = rows(os.path.join(folder, 'flights.csv'))
    speed = rows(os.path.join(folder, 'speed.csv'))
    L = [{k: num(v) for k, v in r.items()} for r in line]; n = len(L); length = L[-1]['s'] + 3
    S = lambda s: L[int(round((s % length) / 3)) % n]
    # the rivals' speed: the probe lap's speed by station (median of the samples within 6 m), 22 m/s where none
    sp = sorted((num(r['s']), num(r['speed'])) for r in speed)
    def speed_at(s):
        near = [v for (q, v) in sp if abs(q - s) < 6 and v == v]
        return sorted(near)[len(near) // 2] if near else 22.0
    gate_s = [num(g['s']) for g in gates]
    # each big jump: its approach (CODEX_RULES 5A: the approach is part of the jump), take-off, flight and landing, to just past it
    def lip_s(f): x, z = num(f['lipx']), num(f['lipz']); return min(L, key=lambda r: (r['x'] - x) ** 2 + (r['z'] - z) ** 2)['s']
    fl = [(min(num(f['approach']), lip_s(f) - 200), num(f['end'])) for f in flights]  # its whole approach, and at least 200 m before the lip
    # the marked jump sites ("High Ridge Drop" and the like) and Leviathan's shortcut kickers: none from 180 m before to 60 m past one
    jp = os.path.join(folder, 'jumps.csv'); site_s = [num(j['s']) for j in rows(jp)] if os.path.exists(jp) else []
    def excluded(s):
        if s < 220 or s > length - 90: return 'start / finish'
        for a, e in fl:
            if a - 120 <= s <= e + 40: return 'a big jump'
        for j in site_s:
            d = (j - s) % length
            if d <= 180 or d >= length - 60: return 'a marked jump'
        for g in gate_s:
            if abs(s - g) < 30: return 'a gate'
        return None
    def heading(r): return math.degrees(math.atan2(r['fx'], r['fz']))
    def turn(s0, s1):
        h0 = heading(S(s0)); worst = 0
        for q in range(int(s0), int(s1), 3): worst = max(worst, abs((heading(S(q)) - h0 + 180) % 360 - 180))
        return worst
    pts = [(r['s'], r['x'], r['z']) for r in L]
    def other_pass(x, z, s):
        return min((math.hypot(px - x, pz - z) for (ps, px, pz) in pts if min(abs(ps - s), length - abs(ps - s)) > 80), default=1e9)
    def g_at(r, lat):
        cols = [-7, -5, -3, 0, 3, 5, 7]; vals = [r['g%d' % c] for c in cols]
        for i in range(len(cols) - 1):
            if cols[i] <= lat <= cols[i + 1]:
                a, b = vals[i], vals[i + 1]; t = (lat - cols[i]) / (cols[i + 1] - cols[i]); return a + (b - a) * t
        return float('nan')
    def thing(r, side):  # something standing on that half of the road or just beyond it
        return r['o%d' % (3 * side)] > 0 or r['o%d' % (6 * side)] > 0
    found = []; why = {}; airs = []
    def no(r): why[r] = why.get(r, 0) + 1
    for i in range(n):
        r = L[i]; s = r['s']
        if excluded(s): no(excluded(s)); continue
        if turn(s - 20, s + 35) > 18: no('bend'); continue
        for side in (1, -1):
            for kind, k in (('big', BIG), ('normal', NORMAL)):
                lat = side * 3.2
                lip_g = g_at(r, lat); start = S(s - k['length']); start_g = g_at(start, lat)
                if not (lip_g == lip_g and start_g == start_g): no('no ground'); continue
                ramp_slope = (lip_g - start_g) / k['length']
                if abs(ramp_slope) > .16 or abs(g_at(r, side * 5) - g_at(r, side * 1)) > .5: no('slope'); continue  # near-level ground under it, little cross-slope
                if any(thing(S(q), side) for q in range(int(s - 12), int(s + 6), 3)): no('something there'); continue
                v = min(speed_at(s), 28.0)
                if v < 14: no('slow'); continue
                ang = math.atan(k['height'] / k['length']) + math.atan(ramp_slope)
                vy, vx = v * math.sin(ang), v * math.cos(ang); y0 = lip_g + k['height']
                t = 0; landed = None
                while t < 6:
                    t += .02; d = vx * t; y = y0 + vy * t - .5 * G * t * t; q = S(s + d); gy = g_at(q, lat * .6)
                    if gy != gy: break
                    if y <= gy: landed = (t, d, q); break
                if not landed: no('no landing'); continue
                t, d, q = landed
                down = (g_at(S(s + d + 6), lat * .6) - g_at(S(s + d - 6), lat * .6)) / 12  # the landing's slope along the course
                airs.append((round(t,2), kind, round(s), side))
                if t < k['air']: no('short air ' + kind); continue
                if down > .01 or down < -.30: no('landing slope'); continue
                if any(thing(S(qq), side) or S(qq)['o0'] > 0 for qq in range(int(s), int(s + d + 20), 3)): no('something in the flight'); continue
                fx, fz = r['fx'], r['fz']; rx, rz = fz, -fx
                x, z = r['x'] + rx * lat, r['z'] + rz * lat
                if other_pass(x, z, s) < 15: no('other pass'); continue
                found.append(dict(s=s, side=side, kind=kind, k=k, t=t, d=d, down=down, v=v, x=x, z=z, y=lip_g, yaw=heading(r)))
                break
    return found

if __name__ == '__main__':
    lo, hi = float(sys.argv[2]), float(sys.argv[3])
    for c in place(sys.argv[1], ''):
        if lo <= c['s'] <= hi: print(f"s {c['s']:.0f} {c['kind']} side {c['side']} x {c['x']:.2f} z {c['z']:.2f} y {c['y']:.2f} yaw {c['yaw']:.1f} air {c['t']:.2f} s lands {c['d']:.0f} m on, {abs(c['down'])*100:.0f}% down at {c['v']*2.237:.0f} mph")
