import json,pathlib,html,math
root=pathlib.Path(__file__).resolve().parents[1];out=root/'Docs/BackyardForward'
d=json.loads((out/'geometry.json').read_text());base=json.loads((out/'baseline.json').read_text())
def xy(p):return 65+p['x']*1.9,375-p['z']*1.9
def line(points,color,width=3):return '<polyline points="'+' '.join(f'{x:.1f},{y:.1f}' for x,y in map(xy,points))+f'" fill="none" stroke="{color}" stroke-width="{width}"/>'
p=['<svg xmlns="http://www.w3.org/2000/svg" width="1200" height="820" viewBox="0 0 1200 820">','<rect width="1200" height="820" fill="#edf0e4"/>','<defs><clipPath id="map"><rect x="45" y="90" width="1020" height="615"/></clipPath></defs>','<text x="45" y="40" font-family="sans-serif" font-size="25">Dan’s Backyard — Forward</text>','<text x="45" y="66" font-family="sans-serif" font-size="14">Actual saved route · nine hard anchors · north (+Z) up · metres</text>','<g clip-path="url(#map)">']
for r in base['roads']:
    if r['name']=='Phase 3 - Race Systems':p.append(line(r['points'],'#a4a8a0',13))
g=d['gully'];p.append(line([g['south'],g['north']],'#9d8061',48));p.append(line([g['south'],g['north']],'#624e3b',12))
mid={k:(d['dump']['launch'][k]+d['dump']['landing'][k])/2 for k in ['x','y','z']};x,y=xy(mid);p.append(f'<ellipse cx="{x}" cy="{y}" rx="44" ry="29" fill="#baa083"/>')
p.append(line(d['route']+[d['route'][0]],'#147c77',4));p.append('</g>')
for i,a in enumerate(d['anchors'],1):
    x,y=xy(a);p.append(f'<circle cx="{x:.1f}" cy="{y:.1f}" r="10" fill="#efb947" stroke="#513e1d"/><text x="{x:.1f}" y="{y+4:.1f}" text-anchor="middle" font-family="sans-serif" font-size="12">{i}</text>')
for gate in d['gates']:
    x,y=xy(gate['position']);p.append(f'<rect x="{x-3}" y="{y-3}" width="6" height="6" fill="#247af0"/>')
for label,at,dx,dy in [('Dump',mid,-20,-45),('First big crossing',d['anchors'][6],-50,31),('Second crossing',d['anchors'][7],-75,-25),('South Cherokee Lane',{'x':473,'z':40},10,0)]:
    x,y=xy(at);p.append(f'<text x="{x+dx}" y="{y+dy}" font-family="sans-serif" font-size="14">{html.escape(label)}</text>')
p += ['<text x="45" y="745" font-family="sans-serif" font-size="14">Teal: Forward main route · Blue squares: checkpoints · Brown: one continuous ravine and dump</text>','<text x="45" y="770" font-family="sans-serif" font-size="14">1 Start / finish · 2 Hill · 3 Flat parking · 4 Dirt begins · 5 Corrected dump launch · 6 Landing</text>','<text x="45" y="793" font-family="sans-serif" font-size="14">7 First gully jump · 8 Second jump back · 9 Forest return · Reverse / shortcuts deferred</text></svg>']
(out/'Forward.svg').write_text(''.join(p),encoding='utf-8')
print('Atlas generated from actual saved Forward route and feature dimensions.')
