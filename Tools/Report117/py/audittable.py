# 0.117 Part A: the decision-point audit table (Docs/Report117/audit.md) from the audit list and the built player's audit runs.
# usage: python Tools/Report117/py/audittable.py
import json, os, re
items = json.load(open('Docs/Report117/audit-items.json'))
def hud(run):
    p = 'Docs/Report117/Player/%s.txt' % run; out = {}
    if os.path.exists(p):
        for ln in open(p, encoding='utf-8'):
            m = re.match(r"\s+(\d\d) \S+\s+.*next CP (\d+), gates drawn \[([\d,]*)\], HUD '([^']*)'", ln)
            if m: out[int(m.group(1))] = (m.group(2), m.group(4))
    return out
DECIDE = {'turn': 'the course: arrow board before the turn, chevrons on its outside, the next gate drawn', 'closed': 'not the course: closed (barricade, ROAD CLOSED)',
          'closeup': '(the same closure, 12 m in front of it)', 'report': "Dan's report spot"}
L = ['# Leviathan decision-point audit (0.117)', '',
     'Every place where Leviathan meets another drivable way: either the course (marked) or closed for the race. Shots: driver\'s view from the',
     'built player, the car stopped at the spot with the race progress a driver there would have (the next gate drawn, the HUD\'s NEXT CP).',
     '"before" = 0.116 (its course files and code paths); "after" = 0.117. A dash: the spot is not on that version\'s line (the new driveway way).', '',
     'Left open on purpose: Hwy 92 at the Street Loop corners (its traffic drives through; the turns are marked), the other pass of the course',
     'itself (the lake roads Leviathan drives both ways: marked, and the wrong-way warning), Leviathan\'s own shortcuts (gold, named boards),',
     'house driveways along the Street Loop (dead ends, as in the Street Loop races). The list of junctions and why each is or is not closed:',
     '[blocks.txt](blocks.txt).', '']
for name, tag, c in (('LeviathanForward', 'F', 8), ('LeviathanReverse', 'R', 9)):
    after, before = hud('audit%d' % c), hud('audit%d-before' % c)
    L += ['## ' + ('Forward' if tag == 'F' else 'Reverse'), '', '| # | what | where (x, z, heading) | decision | after: next CP / HUD | shots |', '|---|---|---|---|---|---|']
    for k, it in enumerate(items[name]):
        a = after.get(k); b = 'Audit/audit-%s-%02d-%s-before.jpg' % (tag, k, it['kind'])
        shots = '[after](Audit/audit-%s-%02d-%s-after.jpg)' % (tag, k, it['kind']) + (' [before](%s)' % b if os.path.exists('Docs/Report117/' + b) else ' –')
        L.append('| %d | %s | %.0f, %.0f, %.0f° | %s | %s | %s |' % (k, it['label'].replace('|', '/'), it['x'], it['z'], it['yaw'], DECIDE[it['kind']],
                                                                ('CP %s / %s' % a) if a else '', shots))
    L.append('')
open('Docs/Report117/audit.md', 'w', encoding='utf-8').write('\n'.join(L) + '\n'); print(len(L))
