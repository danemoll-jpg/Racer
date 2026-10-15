# 0.117: the Tracks preview (CoursePreviews.json) of both Leviathans from their new lines (the driveway entrance), gates and shortcuts.
# Text surgery on the two Leviathan entries only (the file's last two), so every other course stays byte for byte as it is.
# usage: python Tools/Report117/py/previews.py
import json
P = 'Assets/Resources/WorldMaps/CoursePreviews.json'
raw = open(P, encoding='utf-8', newline='').read(); old = json.loads(raw)
ent = {c['scene']: c for c in old['courses'] if c['scene'].startswith('Leviathan')}
def xyz(q): return {'x': round(q[0], 3), 'y': round(q[1], 3), 'z': round(q[2], 3)}
new = []
for scene, name in (('Leviathan', 'LeviathanForward'), ('LeviathanReverse', 'LeviathanReverse')):
    R = json.load(open('Assets/Resources/Leviathan/%s.json' % name)); e = dict(ent[scene])
    e['id'] = R['id']; e['main'] = [xyz(R['pts'][i:i + 3]) for i in range(0, len(R['pts']), 3)]
    e['gates'] = [xyz(g['pos']) for g in R['gates']]
    e['branches'] = [{'points': [xyz(b['pts'][i:i + 3]) for i in range(0, len(b['pts']), 3)], 'undergroundStart': 0.0, 'undergroundEnd': 0.0} for b in R['branches']]
    new.append(json.dumps(e, indent=4).replace('\n', '\r\n'))
a = raw.index('"scene": "Leviathan"'); a = raw.rindex('{', 0, a)
b = raw.rindex(']'); b = raw.rindex('}', 0, b) + 1
out = raw[:a] + ',\r\n'.join(new) + raw[b:]
json.loads(out)
open(P, 'w', encoding='utf-8', newline='').write(out)
print('previews: Leviathan %d / LeviathanReverse %d points' % (len(json.loads(out)['courses'][-2]['main']), len(json.loads(out)['courses'][-1]['main'])))
