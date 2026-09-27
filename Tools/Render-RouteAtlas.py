"""Maps only exported Unity scene data; X/Z metres, +Z north."""
import json, pathlib, math
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
from matplotlib.patches import Circle
ROOT=pathlib.Path(__file__).resolve().parents[1]
OUT=ROOT/'Docs/RouteAtlas'
source=OUT/'routes-after.json'
if not source.exists(): source=OUT/'routes-before.json'
courses=json.loads(source.read_text())['courses']
detail=json.loads((OUT/'detail-before.json').read_text())
colors=['#246ea0','#288248','#7453a5','#bb4b55','#a16d15','#128f94']
def line(ax,points,**kw):
    return ax.plot([p['x'] for p in points],[p['z'] for p in points],**kw)
def arrows(ax,points,color,step=150):
    dist=0
    for a,b in zip(points,points[1:]):
        dx=b['x']-a['x']; dz=b['z']-a['z']; d=math.hypot(dx,dz);dist+=d
        if dist>step and d>0:
            ax.annotate('',xy=(b['x']+dx/d*9,b['z']+dz/d*9),xytext=(b['x']-dx/d*9,b['z']-dz/d*9),arrowprops=dict(arrowstyle='-|>',color=color,lw=1.4));dist=0
def setup(title,size=(14,11)):
    fig,ax=plt.subplots(figsize=size);fig.patch.set_facecolor('#f7f5ed');ax.set_facecolor('#f7f5ed')
    ax.set_aspect('equal');ax.grid(alpha=.2);ax.set_xlabel('World X (metres)');ax.set_ylabel('World Z (metres); +Z = north');ax.set_title(title,loc='left',pad=18,fontsize=17)
    return fig,ax
def landmarks(ax,small=False):
    marks=[('L01 House 3',418,-160),('L02 Driveway road mouth',515.18,-132.87),('L03 Driveway house end',436.47,-152.33),('L04 Granite east fork',600.57,-161.74),('L05 Granite west rejoin',200.89,-175.39),('L06 Laurel launch',346.6,-176.13)]
    if not small:marks=[marks[0],marks[3],marks[4]]
    for name,x,z in marks:
        ax.plot(x,z,'o',ms=4,color='#303b39');ax.annotate(name,(x,z),xytext=(6,8),textcoords='offset points',fontsize=8,bbox=dict(facecolor='#f7f5ed',alpha=.85,edgecolor='none',pad=1))
    return marks
def routes(ax,course,color,labels=True):
    for route in course['routes']:
        if route['kind']=='environment-road':line(ax,route['points'],color='#aaa69c',lw=.6,alpha=.5)
    for route in course['routes']:
        p=route['points'];kind=route['kind']
        if kind=='main':p=p+[p[0]];line(ax,p,color=color,lw=2.1,label=course['scene']+' main' if labels else None);arrows(ax,p,color)
        elif kind=='shortcut':
            line(ax,p,color='#cb791e',lw=1.7,ls='--',label=route['name']+' optional' if labels else None);arrows(ax,p,'#cb791e',240)
            ax.plot(p[0]['x'],p[0]['z'],'o',color='#cb791e',ms=5);ax.plot(p[-1]['x'],p[-1]['z'],'D',color='#cb791e',ms=5)
        elif kind=='driveway':line(ax,p,color='#807570',lw=1.3,label='House 3 driveway (property)' if labels else None)
    for i,f in enumerate(course.get('features',[])):
        if f['kind']=='recovery-exclusion':continue
        p=f['start'];ax.plot(p['x'],p['z'],'^',color='#aa3345',ms=6);ax.annotate('J'+str(i+1),(p['x'],p['z']),xytext=(4,-12),textcoords='offset points',fontsize=8,color='#aa3345')
    for i,p in enumerate(course['gates']):
        ax.plot(p['x'],p['z'],'s',color=color,ms=5)
        if labels:ax.annotate('S/F' if i==0 else f'CP{i}',(p['x'],p['z']),xytext=(5,5),textcoords='offset points',fontsize=8,color=color)
for i,c in enumerate(courses):
    fig,ax=setup(c['scene']+' | '+c['id']);routes(ax,c,colors[i]);landmarks(ax)
    ax.legend(loc='upper left',fontsize=8);fig.text(.08,.025,'Solid: required main course · Dashed amber: optional · Circle: entrance · Diamond: rejoin · Square: gate · Triangle: jump\nGrey: existing environment roads. Jump names and route IDs are indexed in ATLAS.md. Each course has its own collision world.',fontsize=9)
    fig.savefig(OUT/(c['scene']+'.png'),dpi=170,bbox_inches='tight');plt.close(fig)
fig,ax=setup('WOODSTOCK RUSH | current authored route atlas',size=(16,13))
for i,c in enumerate(courses):
    main=c['routes'][0]['points'];line(ax,main+[main[0]],color=colors[i],lw=1.25,alpha=.75,label=c['scene'])
    for r in c['routes']:
        if r['kind']=='shortcut':line(ax,r['points'],color=colors[i],ls='--',lw=1)
landmarks(ax)
named=[('L09 Trickum Road',-626.6,492.6),('L10 Jamerson connection',-599.1,-563.7),('L11 South Cherokee / Hwy 92',292,575.3),('L12 Kyle wooded drive',463.3,-22.8),('L13 Lake shore',541.9,-92.6),('L14 Summit',921.6,161.6),('L15 Pool house',397.16,-.45)]
for name,x,z in named:
    ax.plot(x,z,'o',ms=3,color='#4c514c');ax.annotate(name,(x,z),xytext=(5,6 if name not in ('L12 Kyle wooded drive','L13 Lake shore') else -14),textcoords='offset points',fontsize=7,bbox=dict(facecolor='#f7f5ed',alpha=.8,edgecolor='none',pad=1))
ax.legend(loc='lower left',fontsize=9);fig.text(.08,.025,'Solid = MAIN · Dashed = OPTIONAL · World X/Z metres, shared coordinate reference\nRoutes shown together for planning; each course is a separate saved scene with potentially different elevations. Named-road markers use actual physical sign positions.',fontsize=10)
fig.savefig(OUT/'Overall.png',dpi=190,bbox_inches='tight');plt.close(fig)
fig,ax=setup('HOUSE 3 / FOREST / LAUREL | current routes and protected zone',size=(16,11))
for scene,color in [('ForestLoopReverse','#7453a5'),('StreetLoopReverse','#246ea0')]:
    c=next(c for c in courses if c['scene']==scene)
    for r in c['routes']:
        if r['kind']=='main':line(ax,r['points'],color=color,lw=1.8,label=scene+' main')
        elif r['name'] in ('Granite Saddle','Laurel Switchbacks'):
            line(ax,r['points'],color='#cb791e' if r['name']=='Granite Saddle' else '#b24a68',lw=2.5,ls='--',label=r['name']+' (optional)');arrows(ax,r['points'],'#b24a68' if 'Laurel' in r['name'] else '#cb791e',85)
        elif r['kind']=='driveway':line(ax,r['points'],color='#77746d',lw=2,ls='-' if scene=='ForestLoopReverse' else ':',label='Straight driveway (five scenes)' if scene=='ForestLoopReverse' else 'Preserved Laurel-scene driveway')
before=json.loads((OUT/'routes-before.json').read_text())['courses']
old=next(c for c in before if c['scene']=='ForestLoopReverse')['routes'][0]['points']
local=[p for p in old if 195<p['x']<605 and p['z']< -162]
line(ax,local,color='#8b819b',lw=1.3,ls=':',label='Former required Forest Reverse detour (retired)')
landmarks(ax,True)
zone=next(c for c in detail if c['scene']=='StreetLoopReverse')['zones'][0]
line(ax,[zone['start'],zone['end']],color='#b24a68',lw=7,alpha=.3,label='Laurel authored launch/recovery exclusion (not full protected envelope)')
lip=(345.94,-178.02);landing=[(376.58,-90.48),(389.86,-52.54)];ax.plot([lip[0],landing[-1][0]],[lip[1],landing[-1][1]],color='#b24a68',lw=1.5,ls='-.',label='Physical Laurel flight bearing (preserved)')
from matplotlib.patches import Polygon
axis=(.33035,.94386);side=(axis[1],-axis[0]);near=landing[0];far=(landing[-1][0]+axis[0]*60,landing[-1][1]+axis[1]*60)
envelope=[(q[0]+side[0]*w,q[1]+side[1]*w) for q,w in [(near,-30),(near,30),(far,30),(far,-30)]]
ax.add_patch(Polygon(envelope,facecolor='#b24a68',alpha=.12,edgecolor='#b24a68',ls='--',label='Approx. off-centre landing/runout envelope; all underlying scene unchanged'))
for i,(x,z) in enumerate(landing):ax.plot(x,z,'x',color='#b24a68');ax.annotate(f'L{7+i:02d} geometric landing\nY={80.03 if i==0 else 78.40:.2f}m',(x,z),xytext=(7,5),textcoords='offset points',fontsize=8)
ax.set_xlim(165,640);ax.set_ylim(-350,55);ax.legend(loc='lower right',fontsize=7)
fig.text(.08,.015,'Solid purple: current MAIN Granite Saddle. Dotted purple: retired requirement; physical terrain remains.\nLaurel route metadata and physical straight jump differ: both are shown, neither changed. Landing envelope is a conservative planning annotation, not a measured gameplay limit.\nStreet Reverse driveway stays unchanged where the requested straight line would cross protected Laurel. Same X/Z coordinates as all atlas views.',fontsize=9)
fig.savefig(OUT/'House3-Forest-Laurel.png',dpi=190,bbox_inches='tight');plt.close(fig)
fig,ax=plt.subplots(figsize=(14,6));
for c in detail:
    samples=c['graniteSamples'];s=[0]
    for a,b in zip(samples,samples[1:]):s.append(s[-1]+math.hypot(b['point']['x']-a['point']['x'],b['point']['z']-a['point']['z']))
    ax.plot(s,[v['hits'][0]['y'] if v['hits'] else float('nan') for v in samples],label=c['scene']+' actual support')
ax.plot(s,[v['point']['y'] for v in samples],ls='--',color='#555555',label='Granite authored navigation height')
ax.set(xlabel='Distance along Granite Saddle from east entrance (m)',ylabel='World elevation Y (m)',title='Same X/Z line, different scene terrain; stale route heights are visible');ax.grid(alpha=.2);ax.legend();fig.savefig(OUT/'Granite-height-comparison.png',dpi=170,bbox_inches='tight');plt.close(fig)
print('Wrote overall, six course views, close-up and height comparison to',OUT)
lines=['# Route atlas — 0.30.0-review1','', 'Derived from saved Unity scenes and collider samples. World X/Y/Z in metres; +Z north. Each course is a separate scene; the overall overlay is not one shared collision world.','', '## Views','', '- Overall.png: all six main routes and optional branches.','- One PNG per scene: active direction, start/finish, gates, optional entrances/rejoins, jumps and environment roads.','- House3-Forest-Laurel.png: promoted main route, retired detour, both driveway states, physical Laurel launch/flight and approximate practical landing/runout area.','- Granite-height-comparison.png: original navigation heights versus scene-specific ground; the promoted route now follows current support.','- House3-Forest-Laurel-before.png: retained pre-change reference.','', '## Stable local landmarks','']
for name,x,z in landmarks(plt.subplots()[1],True):lines.append(f'- {name}: X={x:.2f}, Z={z:.2f}.')
plt.close('all')
lines+=['- L07: Laurel geometric landing at 32 m/s: (376.58, 80.03, -90.48).','- L08: Laurel geometric landing at 38 m/s: (389.86, 78.40, -52.54).']
lines += [f'- {name}: X={x:.2f}, Z={z:.2f}; actual scene physical sign/landmark position.' for name,x,z in named]
lines+=['', 'L07/L08 come from the existing verified physical ramp data in Docs/FocusedRecovery/verification.txt; invariance checks prove it is unchanged. The dashed landing/runout annotation adds 30m lateral allowance and 60m runout for planning; it does not define or restrict gameplay recovery. The entire Street Reverse pre-existing scene is preserved, including terrain and fencing beyond that annotation.','', '## Current identities and checkpoints','']
for c in courses:
    lines += [f"### {c['scene']} — `{c['id']}`",'']
    for r in c['routes']:
        if r['kind']=='environment-road':continue
        p=r['points'];lines.append(f"- {r['kind']}: {r['name']}; {len(p)} control points; from ({p[0]['x']:.2f}, {p[0]['y']:.2f}, {p[0]['z']:.2f}) to ({p[-1]['x']:.2f}, {p[-1]['y']:.2f}, {p[-1]['z']:.2f}).")
    for i,p in enumerate(c['gates']):lines.append(f"- {'S/F' if i==0 else 'CP'+str(i)}: ({p['x']:.2f}, {p['y']:.2f}, {p['z']:.2f}).")
    for i,f in enumerate(c.get('features',[])):lines.append(f"- J{i+1}: {f['name']} ({f['kind']}); start {f['start']}; end {f['end']}.")
    lines.append('')
lines+=['## Relationships and limitations','', '- Granite Saddle exists as an authored branch only in Forest Reverse. Dan explicitly chose Reverse-only promotion; Forward retains its own terrain and Echo Cave.','- Promoted Granite retains all original X/Z points. Its old navigation heights were stale by up to about 21m; only navigation Y was aligned to current colliders.','- Laurel physical launch is approximately (345.94, 80.27, -178.02), while the Granite crossing near that X/Z is roughly Y=39–40 in its separate Forest scene. This overlay is not a physical collision between those scenes.','- The proposed straight House 3 line intersects the retained Laurel metadata corridor near X=500, Z=-137. No new driveway was built in Street Reverse; its terrain, driveway and Laurel data are unchanged.','- Old driveway overlays touching race roads were retained locally to avoid altering accepted collision surfaces. Broad terrain restoration along the old winding trace was intentionally avoided.','- Woodland visible outside the mapped corridors remains available for future planning, but no candidate route, entrance, jump or footprint is selected or built. Dan will choose after reviewing this atlas.','', 'Raw references: routes-before.json, routes-after.json, detail-before.json, promotion.txt, house3-cleanup.txt, guidance.txt, targeted-checks.txt and preservation.txt.']
(OUT/'ATLAS.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
