import pathlib,json,math,base64
p=pathlib.Path('Docs/CR016-017')
def pt(x,z):return 600+(x-390)*1000/260,120+500-(z-53)*1000/260
ax,az=352,105.6;bx,bz=415.8,-1.1;theta=math.atan2(bz-az,bx-ax)
poly=[]
for x,z,start in [(bx,bz,theta-math.pi/2),(ax,az,theta+math.pi/2)]:
 for i in range(61):
  a=start+i*math.pi/60;poly.append(pt(x+27*math.cos(a),z+27*math.sin(a)))
points=' '.join(f'{x:.2f},{y:.2f}' for x,y in poly)
for label in ['before','after']:
 data=base64.b64encode((p/(label+'.png')).read_bytes()).decode();title='BEFORE | 9,003 m² yard' if label=='before' else 'AFTER | 2,251 m² yard · 25.0%'
 parts=[f'<svg xmlns="http://www.w3.org/2000/svg" width="1200" height="1120" viewBox="0 0 1200 1120"><rect width="1200" height="1120" fill="#14242b"/><image x="0" y="120" width="1200" height="1000" href="data:image/png;base64,{data}"/><g font-family="Arial,sans-serif"><text x="30" y="45" fill="white" font-size="30">{title}</text><text x="30" y="84" fill="#d0dcd7" font-size="19">Dashed gold: original yard boundary · horizontal ground area · narrow access excluded</text><text x="1090" y="45" fill="white" font-size="27">N ↑</text><polygon points="{points}" fill="none" stroke="#ffe197" stroke-width="4" stroke-dasharray="10 8"/>']
 if label=='after':
  cx,cy=pt(bx,bz);r=26.76703*1000/260
  parts.append(f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="none" stroke="#88ffee" stroke-width="5"/>')
  x,y=pt(349.7807,103.581429);u,v=pt(415.8,-13)
  parts.append(f'<path d="M{x},{y} L{u},{v}" stroke="#fff" stroke-width="3" stroke-dasharray="5 7"/><circle cx="{x}" cy="{y}" r="10" fill="none" stroke="white" stroke-width="3"/>')
  marks=[(415.8,-13,'Dan — relocated',760,850),(349.78,103.58,'Former house site — reforested',30,270)]
 else:marks=[(349.7807,103.581429,'Dan — original location',45,320)]
 marks += [(417.409,-55.317,'House #2 — unchanged',730,1040),(511.5,-35.2,'Friend — unchanged',895,900)]
 for x,z,txt,lx,ly in marks:
  u,v=pt(x,z);w=len(txt)*10.4+22
  parts.append(f'<path d="M{u},{v} L{lx},{ly-12}" stroke="#fff" stroke-width="2"/><rect x="{lx-6}" y="{ly-35}" width="{w}" height="43" rx="7" fill="#14242b" fill-opacity=".94"/><text x="{lx+5}" y="{ly-7}" font-size="19" fill="white">{txt}</text>')
 parts.append('</g></svg>');(p/(label+'-annotated.svg')).write_text(''.join(parts),encoding='utf8')
