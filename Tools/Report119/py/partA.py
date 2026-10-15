# 0.119 Part A (run from the project root): Dan's three course reports on 0.118, applied to the 0.118 course files (git c936eee4).
#  BUG-001 (Forward): the closure across the Forest race route where it forks off the lake's south shore (block "leaving Leviathan at s 2734") out.
#  BUG-002 (Forward): CP 18 moved from the foot of Kyle's bank (side-on to the lake path, 196 deg) onto Kyle's drive where the bank's hook comes
#    out, straight ahead of Dan's spot (526.29, -25.40) across the bank and facing the way he drives (west). The line is unchanged: the ground
#    straight ahead of his spot is the unsmoothed side of the bank beside Kyle's house (about 4 m down in 6 m), and the bank stays as 0.118 left it.
#    Course id leviathan-forward-v4 (records start again).
#  BUG-003 (Reverse): the closure at the Southwest Cut's other end (block "leaving Leviathan at s 2052") out.
# usage: python Tools/Report119/py/partA.py   -> Assets/Resources/Leviathan/Leviathan{Forward,Reverse}.json, Tools/Report119/py/partA.txt
import json, math, sys
sys.path.insert(0, 'Tools/Report115/py')
from leviathan import project
log = []
def say(*a):
    s = ' '.join(str(x) for x in a); print(s); log.append(s)
def load(name):
    path = 'Assets/Resources/Leviathan/%s.json' % name; R = json.load(open(path, encoding='utf-8'))
    return path, R, [tuple(R['pts'][i:i + 3]) for i in range(0, len(R['pts']), 3)]
def drop_block(R, at, x, z):
    k = [i for i, b in enumerate(R['blocks']) if b['at'] == at and math.hypot(b['pos'][0] - x, b['pos'][1] - z) < 2]
    assert len(k) == 1, (at, k)
    b = R['blocks'].pop(k[0]); say('  closure removed (panels and sign): (%.2f, %.2f) yaw %.0f half %.1f: %s' % (b['pos'][0], b['pos'][1], b['yaw'], b['half'], b['at']))

path, R, P = load('LeviathanForward'); say('== ' + R['title'])
drop_block(R, 'Forest race route, leaving Leviathan at s 2734', 573.37, -83.51)
g = R['gates'][18]; assert g['name'] == 'Leviathan CP 18', g['name']
s0 = g['station']
# on the line where it runs west along Kyle's drive, just past the foot of the bank (the drive's own ground, 6 m before the drive narrows)
want = (501.5, 80.0, -22.9)
st, off = project(P, want, True, s0, s0 + 60)
i = min(range(len(P)), key=lambda j: math.hypot(P[j][0] - want[0], P[j][2] - want[2]))
a, b = P[i - 2], P[i + 2]; yaw = math.degrees(math.atan2(b[0] - a[0], b[2] - a[2])) % 360
dan = (526.29, -25.40); view = math.degrees(math.atan2(P[i][0] - dan[0], P[i][2] - dan[1])) % 360
say('  CP 18 moved: (%.1f, %.1f) yaw %.0f s %.0f -> (%.1f, %.1f) yaw %.0f s %.0f (%.1f m off the drawn point); from Dan\'s spot %.0f m at %.0f deg (his heading 296)'
    % (g['pos'][0], g['pos'][2], g['rot'][1], s0, P[i][0], P[i][2], yaw, st, off, math.hypot(P[i][0] - dan[0], P[i][2] - dan[1]), view))
g['pos'] = [round(P[i][0], 3), round(P[i][1], 3), round(P[i][2], 3)]; g['rot'] = [0.0, round(yaw, 2), 0.0]; g['station'] = st
nx = R['gates'][19]; assert R['gates'][17]['station'] < st < nx['station']
R['id'] = 'leviathan-forward-v4'; say('  course id ' + R['id'])
open(path, 'w', encoding='utf-8', newline='\n').write(json.dumps(R, separators=(',', ':')))

path, R, P = load('LeviathanReverse'); say('== ' + R['title'])
drop_block(R, 'Existing Southwest Cut, leaving Leviathan at s 2052', -588.74, -535.79)
gs = [(k, x['station']) for k, x in enumerate(R['gates']) if 1964 <= x['station'] <= 2052]
say('  gates between the Southwest Cut\'s two ends (s 1964-2052): %s (CP 9 s %.0f before it, CP 10 s %.0f after): driving it skips no gate' % (gs or 'none', R['gates'][9]['station'], R['gates'][10]['station']))
open(path, 'w', encoding='utf-8', newline='\n').write(json.dumps(R, separators=(',', ':')))
open('Tools/Report119/py/partA.txt', 'w', encoding='utf-8').write('\n'.join(log) + '\n')
