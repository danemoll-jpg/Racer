import json,pathlib,html,math
root=pathlib.Path(__file__).resolve().parents[1];out=root/'Docs/Backyard'
def xy(p):return 70+(p['x']-45)*2.1,700-(p['z']+150)*2.1
def line(points,color,width=2,dash=''):
 coords=' '.join(f'{x:.1f},{y:.1f}' for x,y in map(xy,points))
 return f'<polyline points="{coords}" fill="none" stroke="{color}" stroke-width="{width}" stroke-dasharray="{dash}"/>'
atlas=json.loads((root/'Docs/ForestWaterJump/routes-current.json').read_text())
base=next(c for c in atlas['courses'] if c['scene']=='StreetLoopGreybox')
for name in ('DansBackyard','DansBackyardReverse'):
 d=json.loads((out/f'{name}-geometry.json').read_text());parts=['<svg xmlns="http://www.w3.org/2000/svg" width="1150" height="800" viewBox="0 0 1150 800">','<rect width="1150" height="800" fill="#f5f2e7"/>',f'<text x="40" y="36" font-family="sans-serif" font-size="23">Dan’s Backyard — {"Reverse" if "Reverse" in name else "Forward"}</text>','<text x="40" y="62" font-family="sans-serif" font-size="14">Saved scene geometry · world X/Z metres · north is up · teal main / gold optional / grey existing road</text>']
 for r in base['routes']:
  if r['kind']=='main':parts.append(line(r['points'],'#b3b4ac',4))
 for s in d['surfaces']:
  if 'escape' in s['name'] or 'recoverable' in s['name']:parts.append(line(s['points'],'#a09884',5))
 parts.append(line(d['main']+[d['main'][0]],'#138b87',3));parts.append(line(d['shortcut'],'#c28619',3,'7 4'))
 parts.append('<defs><marker id="direction" markerWidth="7" markerHeight="7" refX="5" refY="3" orient="auto"><path d="M0,0 L6,3 L0,6" fill="none" stroke="#075b59" stroke-width="1.5"/></marker></defs>')
 traveled=0
 for a,b in zip(d['main'],d['main'][1:]):
  traveled+=math.hypot(b['x']-a['x'],b['z']-a['z'])
  if traveled>65:
   ax,ay=xy(a);bx,by=xy(b);parts.append(f'<line x1="{ax}" y1="{ay}" x2="{bx}" y2="{by}" stroke="#075b59" marker-end="url(#direction)"/>');traveled=0
 for i,p in enumerate(d['gates']):
  x,y=xy(p);parts.append(f'<rect x="{x-3:.1f}" y="{y-3:.1f}" width="6" height="6" fill="#14625f"/><text x="{x+5:.1f}" y="{y-4:.1f}" font-family="sans-serif" font-size="11">{"S/F" if i==0 else "CP"+str(i)}</text>')
 for label,x,z in [('Property start / finish',463.6,8),('Property hill',422.7,9),('Lower parking',400,9.7),('Dump takeoff',331.4,16.2),('Dump landing',258.6,8.2),('Deep gully crossing',96.1,-108.6),('Shallow crossing',96.1,-83),('Return anchor',456.4,67.4)]:
  px,py=xy(dict(x=x,z=z));parts.append(f'<circle cx="{px}" cy="{py}" r="4" fill="#343a35"/><text x="{px+6}" y="{py+16}" font-family="sans-serif" font-size="11">{html.escape(label)}</text>')
 parts.append('<text x="40" y="772" font-family="sans-serif" font-size="13">Separate new scenes based on Street Loop world. Existing Forest Reverse / Fern Gully terrain is unchanged.</text></svg>')
 (out/f'{name}.svg').write_text(''.join(parts),encoding='utf-8')
 length=sum(math.dist(tuple(a.values()),tuple(b.values())) for a,b in zip(d['main'],d['main'][1:]+d['main'][:1]));print(name,round(length,1),'metres',len(d['gates']),'gates')
