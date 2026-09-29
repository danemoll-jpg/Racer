import csv,json,math
from pathlib import Path
for p in Path('Docs/ShortcutRevision/checks').glob('*.csv'):
 rows=list(csv.DictReader(p.open()))
 print(p.name,len(rows))
 if not rows or 'x' not in rows[0]:continue
 for s in [20,28,32,35,39,41,45,50,65,80,100,120]:
  r=min(rows,key=lambda r:abs(float(r['station'])-s))
  print({k:r[k] for k in ['time','station','speed','wheels','x','y','z']})
PY=0
