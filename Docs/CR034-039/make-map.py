from pathlib import Path
import json,html,base64
D=Path(__file__).resolve().parent;d=json.loads((D/'map-data.json').read_text())
def p(v):return ((v[0]+710)*1600/1360,(670-v[2])*1600/1360)
def line(v):return ' '.join(f'{p(q)[0]:.1f},{p(q)[1]:.1f}' for q in v)
s=['<svg xmlns="http://www.w3.org/2000/svg" width="1600" height="1770" viewBox="0 0 1600 1770"><rect width="1600" height="1770" fill="#112427"/><image href="overview.png" y="0" width="1600" height="1600"/><style>text{font-family:Arial,sans-serif;fill:white} .label{font-size:24px;font-weight:bold;paint-order:stroke;stroke:#102325;stroke-width:7px;stroke-linejoin:round}</style>']
colors={'Creek Leap':'#49e3ed','Fox Gully':'#ffcb54','Pine Ridge':'#e699ff','Existing Southwest Cut':'#dde5db'}
for b in d['branches']:
 c=colors[b['title']];s.append(f'<polyline points="{line(b["points"])}" stroke="{c}" stroke-width="8" fill="none" opacity=".9"/>')
 x,y=p(b['points'][0]);s.append(f'<circle cx="{x}" cy="{y}" r="13" fill="{c}" stroke="#132d30" stroke-width="4"/>')
 x,y=p(b['points'][-1]);s.append(f'<rect x="{x-8}" y="{y-8}" width="16" height="16" fill="{c}" stroke="#132d30" stroke-width="3"/>')
for g in d['gates']:
 x,y=p(g['p']);s.append(f'<text class="label" x="{x+13}" y="{y-6}" style="font-size:18px">{g["index"] if g["index"] else "START"}</text>')
for x,y,label in [(490,130,'Hwy 92 · four lanes'),(1210,620,'South Cherokee Lane'),(490,1520,'Jamerson Rd'),(1190,1150,'1  Creek Leap · CP 4–6'),(750,1240,'2  Fox Gully · CP 7–9'),(270,1270,'3  Pine Ridge · CP 10–12'),(100,1400,'Existing cut')]:
 s.append(f'<text x="{x}" y="{y}" class="label">{html.escape(label)}</text>')
s.append('<text x="45" y="1650" font-size="32" font-weight="bold">RACER · WOODLAND REVIEW MAP</text><text x="45" y="1690" font-size="22">North ↑  ·  Race direction: down the eastern road, west along the southern road, north on the western return</text><text x="45" y="1730" font-size="22">● Entrance   ■ Verified rejoin   Numbers: original checkpoints   ·   Houses and store positions retained</text></svg>')
(D/'annotated-overview.svg').write_text(''.join(s).replace('href="overview.png"','href="data:image/png;base64,'+base64.b64encode((D/'overview.png').read_bytes()).decode()+'"'),encoding='utf8')
(D/'map.html').write_text('<!doctype html><html><meta charset="utf-8"><title>Racer woodland review map</title><style>body{margin:0;background:#112427}img{width:100%;max-width:1600px;display:block;margin:auto}</style><img src="annotated-overview.svg" alt="Racer woodland shortcut map"></html>',encoding='utf8')
