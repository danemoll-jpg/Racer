# 0.116 Part A: where Leviathan's line leaves the road it is on, or meets / leaves another pass of its own (twice-driven lake roads):
# every part boundary (the composer's parts) and every start / end of a stretch the line drives twice, with the turn angle there.
import json, math, sys
def load(name):
    R = json.load(open('Assets/Resources/Leviathan/%s.json' % name))
    P = [(R['pts'][i], R['pts'][i + 1], R['pts'][i + 2]) for i in range(0, len(R['pts']), 3)]
    S = [0.0]
    for i in range(1, len(P)): S.append(S[-1] + math.dist(P[i], P[i - 1]))
    return R, P, S
def at(P, S, s):
    s %= S[-1]; lo, hi = 0, len(S) - 1
    while lo + 1 < hi:
        m = (lo + hi) // 2
        if S[m] <= s: lo = m
        else: hi = m
    t = (s - S[lo]) / max(1e-6, S[lo + 1] - S[lo]); return tuple(P[lo][k] + (P[lo + 1][k] - P[lo][k]) * t for k in range(3))
def heading(P, S, a, b):
    p, q = at(P, S, a), at(P, S, b); return math.degrees(math.atan2(q[0] - p[0], q[2] - p[2]))
def turn(P, S, s, before=45, after=45):
    h1 = heading(P, S, s - before, s - 8); h2 = heading(P, S, s + 8, s + after); return (h2 - h1 + 540) % 360 - 180
if __name__ == '__main__':
    for name in sys.argv[1:]:
        R, P, S = load(name); print('==', name, 'length %.0f' % S[-1])
        for p in R['parts']:
            s = p['from']; q = at(P, S, s); print('  part %-34s from %6.0f at (%.0f,%.0f,%.0f) turn %+4.0f' % (p['name'], s, q[0], q[1], q[2], turn(P, S, s)))
        # twice-driven: stations whose point is within 10 m of a point of the line more than 150 m away along it
        step = 5; marks = []
        n = int(S[-1] // step); pts = [at(P, S, k * step) for k in range(n)]
        twice = []
        for k in range(n):
            a = pts[k]; hit = None
            for m in range(n):
                if abs(m - k) * step < 150 or abs(m - k) * step > S[-1] - 150: continue
                if math.hypot(pts[m][0] - a[0], pts[m][2] - a[2]) < 10 and abs(pts[m][1] - a[1]) < 5: hit = m * step; break
            twice.append(hit)
        for k in range(1, n):
            if (twice[k] is None) != (twice[k - 1] is None):
                s = k * step; q = pts[k]; print('  %s twice-driven at %6.0f (%.0f,%.0f,%.0f) other pass s %s turn %+4.0f' % ('starts' if twice[k] is not None else 'ends  ', s, q[0], q[1], q[2], twice[k] if twice[k] is not None else twice[k - 1], turn(P, S, s)))
