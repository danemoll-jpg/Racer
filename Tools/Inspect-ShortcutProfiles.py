import json,math
from pathlib import Path
d=json.loads(Path('Docs/BackyardShortcuts/geometry.json').read_text())
for b in d['branches']:
 s=0;rows=[]
 for a,p in zip(b['points'],b['points'][1:]):
  ds=math.hypot(p['x']-a['x'],p['z']-a['z']);s+=ds
  if s<45:rows.append((round(s,2),round((p['y']-a['y'])/ds,2)))
 print(b['title'],rows)
