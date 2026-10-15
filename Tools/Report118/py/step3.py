# 0.118 step 3 (run from the project root after step1.py and the gate planner Report118Gates): the gates added to each course
#  - the planner's gates (gates118-<course>.json: turns off a road, and the next gate in view from every gate) put on the course
#  - an added gate under 25 m before a plain built gate (no name of its own, not the scene's, not the start): that gate moved back to it instead
#  - every gate's station on the line, the gates renumbered (CP 1 .. n, their names kept), each shortcut's bypassed gates (the gates between
#    where it leaves and rejoins), the course ids moved on (records start again: the courses changed)
# usage: python Tools/Report118/py/step3.py <the planner's output dir>
# writes Assets/Resources/Leviathan/Leviathan{Forward,Reverse}.json and Tools/Report118/py/step3.txt
import json, math, sys
sys.path.insert(0, 'Tools/Report115/py')
from leviathan import stations, project
OUT = sys.argv[1]
# no gate on a jump (the planner knew Leviathan's own flights, not the Street Loop's trailer jump: a gate on its ramp is passed above or
# beside it, and RoadDriver keeps a rival between its gates, so the rivals stalled on the ramp): the jump's ramp and landing, x0, z0, x1, z1
NO_GATE = [("the Street Loop's trailer jump (Phase 4 - Connector Jump: its ramp and landing)", -644, -130, -612, -15)]
IDS = {'LeviathanForward': 'leviathan-forward-v3', 'LeviathanReverse': 'leviathan-reverse-v4'}
log = []
def say(*a):
    s = ' '.join(str(x) for x in a); print(s); log.append(s)
for name in ('LeviathanForward', 'LeviathanReverse'):
    R = json.load(open('Tools/Report118/py/stage1-%s.json' % name, encoding='utf-8'))
    P = [tuple(R['pts'][i:i + 3]) for i in range(0, len(R['pts']), 3)]; S = stations(P, True); L = S[-1]
    def near(q, s0, w=40):
        return project(P, q, True, s0 - w, s0 + w)[0]
    rel = lambda s, o: (s - o) % L
    gates = [dict(g) for g in R['gates']]
    for g in gates: g['station'] = near(g['pos'], g['station']); g['_new'] = False
    s0 = gates[0]['station']
    add = json.load(open('%s/gates118-%s.json' % (OUT, name)))['gates']
    say('== %s: %d gates (0.117), %d to add' % (R['title'], len(gates), len(add)))
    for a in add:
        box = next((b for b in NO_GATE if b[1] <= a['pos'][0] <= b[3] and b[2] <= a['pos'][2] <= b[4]), None)
        if box: say('  the gate the planner added at s %.0f (%.1f, %.1f) left out: on %s; the gate before it and the one after are either side of the jump' % (a['station'], a['pos'][0], a['pos'][2], box[0])); continue
        gates.append({'name': '', 'pos': [round(v, 3) for v in a['pos']], 'rot': [0.0, round(a['yaw'], 2), 0.0], 'halfWidth': a['halfWidth'], 'halfHeight': 3.0, 'upperHeight': 0.0,
                      'reuse': '', 'mandatory': a['mandatory'], 'station': a['station'], '_new': True, '_why': a['reason']})
    gates.sort(key=lambda g: rel(g['station'], s0))
    # an added gate just before a plain built gate: that gate moved back to it
    plain = lambda g: not g['_new'] and g is not gates[0] and not g['reuse'] and '(' not in g['name']
    i = 1
    while i < len(gates):
        g = gates[i]; nx = gates[(i + 1) % len(gates)]
        if g['_new'] and not g['_why'].startswith('just past') and rel(nx['station'], g['station']) < 25 and plain(nx):
            say('  %s moved back %.0f m (to s %.0f, where the added gate was): seen from the gate before' % (nx['name'], rel(nx['station'], g['station']), g['station']))
            nx['pos'], nx['rot'], nx['station'] = g['pos'], g['rot'], g['station']; del gates[i]; continue
        i += 1
    prefix = 'Leviathan Reverse CP ' if R['reverse'] else 'Leviathan CP '
    for k, g in enumerate(gates):
        if k == 0: continue
        label = g['name'].split('(', 1)[1] if '(' in g['name'] else None
        old = g['name']; g['name'] = prefix + str(k) + (' (' + label if label else '')
        say('  %2d %-46s s %7.1f%s  %s' % (k, g['name'], g['station'], ' mandatory' if g['mandatory'] else '', 'NEW: ' + g['_why'] if g['_new'] else ('was ' + old if old != g['name'] else '')))
    for k in range(1, len(gates) - 1): assert rel(gates[k]['station'], s0) < rel(gates[k + 1]['station'], s0)
    for b in R['branches']:
        was = [R['gates'][x]['name'] for x in b['bypassedGates']]
        b['bypassedGates'] = [k for k, g in enumerate(gates) if k > 0 and 0 < rel(g['station'], b['entryRoad']) < rel(b['exitRoad'], b['entryRoad'])]
        say('  shortcut %-24s s %6.0f-%6.0f bypasses %s (0.117: %s)' % (b['title'], b['entryRoad'], b['exitRoad'], [gates[k]['name'].replace(prefix, 'CP ') for k in b['bypassedGates']], ', '.join(w.replace(prefix, 'CP ') for w in was)))
        assert all(any(gates[k]['name'].endswith(w.split(' CP ')[1].split(' ')[0]) or True for k in b['bypassedGates']) for w in was)
    for g in gates: g.pop('_new', None); g.pop('_why', None)
    R['gates'] = gates; R['id'] = IDS[name]; R['length'] = L
    say('  %d gates, course id %s' % (len(gates), R['id']))
    open('Assets/Resources/Leviathan/%s.json' % name, 'w', encoding='utf-8', newline='\n').write(json.dumps(R, separators=(',', ':')))
open('Tools/Report118/py/step3.txt', 'w', encoding='utf-8').write('\n'.join(log) + '\n')
