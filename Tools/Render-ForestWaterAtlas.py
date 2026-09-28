"""Final 0.31 atlas from saved scene export, with main and restored optional routes."""
import pathlib,json,math
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
from matplotlib.patches import Ellipse,Rectangle,Polygon
root=pathlib.Path(__file__).resolve().parents[1];out=root/'Docs/ForestWaterJump'
courses=json.loads((out/'routes-current.json').read_text())['courses']
geo=json.loads((out/'geometry.json').read_text());flights=json.loads((out/'flights.json').read_text())
palette=['#246ea0','#288248','#7453a5','#bb4b55','#a16d15','#128f94']
def xy(p):return p['x'],p['z']
def line(ax,ps,**kw):ax.plot([p['x'] for p in ps],[p['z'] for p in ps],**kw)
def setup(title,size=(14,11)):
    fig,ax=plt.subplots(figsize=size);fig.patch.set_facecolor('#f7f5ed');ax.set_facecolor('#f7f5ed');ax.set_aspect('equal');ax.grid(alpha=.2);ax.set(xlabel='World X (metres)',ylabel='World Z (metres); +Z north',title=title);return fig,ax
def arrows(ax,ps,color):
    distance=0
    for a,b in zip(ps,ps[1:]):
        dx=b['x']-a['x'];dz=b['z']-a['z'];d=math.hypot(dx,dz);distance+=d
        if distance>140 and d:
            ax.annotate('',xy=xy(b),xytext=(b['x']-dx/d*17,b['z']-dz/d*17),arrowprops=dict(arrowstyle='-|>',color=color));distance=0
def water(ax):
    x,y,z=geo['pool'];w,h=geo['poolSize'];ax.add_patch(Rectangle((x-w/2,z-h/2),w,h,facecolor='#16afcd',edgecolor='#526368',label='Swimming pool'))
    x,y,z=geo['lake'];w,h=geo['lakeSize'];ax.add_patch(Ellipse((x,z),w,h,facecolor='#328b9a',edgecolor='#326675',label='Lake'))
def routes(ax,c,color):
    for r in c['routes']:
        kind=r['kind'];p=r['points']
        if kind=='environment-road':line(ax,p,color='#a5a5a0',lw=.7,alpha=.6)
        elif kind=='driveway':line(ax,p,color='#686b64',lw=1.4,label='House 3 driveway')
        elif kind=='main':line(ax,p+[p[0]],color=color,lw=1.8,label='MAIN — '+c['scene']);arrows(ax,p,color)
        elif kind=='shortcut':line(ax,p,color='#cc7d19',lw=1.7,ls='--',label='OPTIONAL — '+r['name']);arrows(ax,p,'#cc7d19');ax.plot(*xy(p[0]),'o',color='#cc7d19');ax.plot(*xy(p[-1]),'D',color='#cc7d19')
    for i,g in enumerate(c['gates']):ax.plot(*xy(g),'s',ms=4,color=color);ax.annotate('S/F' if i==0 else 'CP'+str(i),xy(g),xytext=(4,4),textcoords='offset points',fontsize=7)
    for f in c.get('features',[]):
        if f['kind']=='recovery-exclusion':continue
        ax.plot(*xy(f['start']),'^',color='#bb4351',ms=5)
    for m in c['landmarks']:
        if m['name']=='Original house 3':ax.plot(*xy(m['position']),'*',color='#292c34',ms=9);ax.annotate('House 3',xy(m['position']),fontsize=8)
    if c['scene']=='ForestLoopReverse':water(ax)
for c,color in zip(courses,palette):
    fig,ax=setup(c['scene']+' — 0.31.0-review1 | '+c['id']);routes(ax,c,color);ax.legend(loc='upper left',fontsize=7)
    fig.text(.09,.025,'Solid: MAIN · Dashed amber: OPTIONAL · Circle: entrance · Diamond: rejoin · Square: gate · Triangle: jump\nCurrent Forest main remains main; the existing southern House 3 Detour is optional. Separate scenes share this X/Z reference.',fontsize=9)
    fig.savefig(out/(c['scene']+'.png'),dpi=150,bbox_inches='tight');plt.close(fig)
fig,ax=setup('All six selectable courses — current saved routes')
for c,color in zip(courses,palette):
    for r in c['routes']:
        if r['kind'] in ('main','shortcut'):line(ax,r['points'],color=color,lw=1.2,ls='-' if r['kind']=='main' else '--',label=c['scene'] if r['kind']=='main' else None)
ax.legend(fontsize=8);fig.savefig(out/'Overall.png',dpi=150,bbox_inches='tight');plt.close(fig)
fig,ax=setup('House 3 / Forest / Laurel — 0.31.0-review1',(16,11))
forest=next(c for c in courses if c['scene']=='ForestLoopReverse');street=next(c for c in courses if c['scene']=='StreetLoopReverse')
routes(ax,forest,'#7453a5')
for r in street['routes']:
    if r['name'].startswith('Laurel'):
        line(ax,r['points'],color='#be526c',lw=2,ls='--',label='Laurel Pass (protected; separate scene)')
        # Approach corridor is a planning annotation, not a change to collision/recovery.
        line(ax,r['points'],color='#be526c',lw=14,alpha=.12)
old=json.loads((root/'Docs/RouteAtlas/routes-before.json').read_text())['courses'];old=next(c for c in old if c['scene']=='ForestLoopReverse')['routes'][0]['points']
local=[p for p in old if 195<p['x']<605 and p['z']< -162]
for f in flights:
    ax.plot([p[0] for p in f['points']],[p[2] for p in f['points']],color='#0d8490',ls='-.',lw=1,alpha=.65)
    p=f['landing'];ax.plot(p[0],p[2],'x',color='#0d8490');ax.annotate(str(f['speed'])+' m/s landing',(p[0],p[2]),xytext=(0,-17),textcoords='offset points',fontsize=7)
launch=geo['lip'];ax.plot(launch[0],launch[2],'^',color='#0d8490',ms=9);ax.annotate('Water jump launch',(launch[0],launch[2]),xytext=(0,-26),textcoords='offset points',fontsize=8)
ax.annotate('Pool',(416,-197),xytext=(435,-244),arrowprops=dict(arrowstyle='-',color='#326675'),fontsize=9)
ax.annotate('Lake',(384,-194.1),xytext=(365,-251),arrowprops=dict(arrowstyle='-',color='#326675'),fontsize=9)
laurel_lip=(346.60,-176.13);axis=(.33035,.94386);side=(axis[1],-axis[0]);near=(376.58,-90.48);far=(389.86+axis[0]*60,-52.54+axis[1]*60)
env=[(q[0]+side[0]*w,q[1]+side[1]*w) for q,w in [(near,-30),(near,30),(far,30),(far,-30)]]
ax.add_patch(Polygon(env,facecolor='#be526c',alpha=.13,edgecolor='#be526c',ls='--',label='Laurel off-centre landing/runout planning envelope'))
ax.plot([laurel_lip[0],389.86],[laurel_lip[1],-52.54],color='#be526c',ls='-.',label='Laurel physical flight')
ax.plot(*laurel_lip,'^',color='#be526c');ax.annotate('Laurel launch',laurel_lip,xytext=(-65,15),textcoords='offset points',fontsize=8)
ax.set_xlim(185,625);ax.set_ylim(-345,45);ax.legend(loc='lower left',fontsize=7)
fig.text(.08,.015,'Shared world X/Z reference. Every original Laurel scene object and asset remains unchanged; the complete scene is protected, beyond this planning envelope.\nPurple remains MAIN. Amber House 3 Detour is the existing southern road restored to OPTIONAL status; no physical shortcut geometry changed.\nBlue dashed lines show geometric flights; one actual motorcycle flight also cleared both waters and landed at (309, 42, -196). No receiving road/platform.',fontsize=9)
fig.savefig(out/'House3-Forest-Laurel.png',dpi=170,bbox_inches='tight');plt.close(fig)
print('Wrote overall, six course maps and local detail from current route export.')
lines=['# Route atlas — 0.31.0-review1','','[Overall](Overall.png) · [House 3 / Forest / Laurel detail](House3-Forest-Laurel.png)','','Saved scene routes, world X/Y/Z metres; +Z north. Forward/reverse use their own scene geometry. Solid = main; dashed amber = optional; circles/diamonds = entrances/rejoins; squares = gates; triangles = jumps. Grey environment roads show intersections.','','Forest Reverse keeps the current Granite main line. The existing southern **House 3 Detour** is optional, with CP1 bypass entitlement. No detour geometry was changed. `forest-reverse-v7-water-detour` is the new timing identity. The separate future shortcut remains deferred.','','House 3 is at (418,32.75,-160); the existing straight driveway is preserved. Pool: (416,34.65,-197), 18x16m. Lake: (384,33.35,-194.10), 38x23m. Ramp lip: approximately (434.13,72.39,-198.11). One continuous westward flight crosses both; no intermediate road/platform. The actual bounded motorcycle landed near (309.10,42.46,-195.98).','','Laurel physical launch: approximately (346.60,80.27,-176.13); physical geometric landings near (376.58,80.03,-90.48) and (389.86,78.40,-52.54). The pink landing/runout annotation includes 30m lateral and 60m runout allowance. Protection is stronger than that annotation: its entire Street Reverse scene, shared assets, approach, flight, imperfect landings, runout, recovery and navigation are unchanged.','','## Selectable courses','']
for c in courses:
    lines += [f"### [{c['scene']}]({c['scene']}.png)",f"Course ID: `{c['id']}`",'']
    for r in c['routes']:
        if r['kind']=='environment-road':continue
        lines.append(f"- {r['kind']}: {r['name']}; {len(r['points'])} control points; entrance {r['points'][0]}; end/rejoin {r['points'][-1]}.")
    for i,g in enumerate(c['gates']):lines.append(f"- {'Start/finish' if i==0 else 'CP'+str(i)}: {g}.")
    for f in c.get('features',[]):lines.append(f"- {f['name']} ({f['kind']}): {f['start']} → {f['end']}.")
    lines.append('')
lines += ['## Evidence','','`routes-current.json`, `geometry.json`, `flights.json`, `shortcut-restoration.txt`, `shortcut-check.txt`, `navigation-check.txt`, `preservation.txt`, `vehicle-check.txt`, `progression-check.txt`. Detailed human driving remains Dan’s review. The water scene is separate from Laurel’s collision world.']
(out/'ATLAS.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
