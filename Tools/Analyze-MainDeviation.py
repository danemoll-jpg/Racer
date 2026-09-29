import json,csv,math
from pathlib import Path
d=json.loads(Path('Docs/BackyardShortcuts/baseline.json').read_text());pts=d['route']
def distance(r):
 x,z=float(r['x']),float(r['z']);best=999
 for a,b in zip(pts,pts[1:]):
  dx,dz=b['x']-a['x'],b['z']-a['z'];den=dx*dx+dz*dz
  t=max(0,min(1,((x-a['x'])*dx+(z-a['z'])*dz)/den)) if den else 0
  best=min(best,math.hypot(x-a['x']-dx*t,z-a['z']-dz*t))
 return best
r=list(csv.DictReader(open('Docs/ShortcutRevision/checks/main-Tree-Top-Trail.csv')))
for row in r[::30]:print(row['time'],row['station'],row['speed'],'main lateral',round(distance(row),2))
