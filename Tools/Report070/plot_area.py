# Top-down map of the crest area: terrain height (grey), pavement (brown), named barriers (red), route centre lines,
# and climb trajectories (airborne blue, grounded green) from Report070Checks climb CSVs.
import sys,csv,glob
from PIL import Image,ImageDraw
area,out=sys.argv[1],sys.argv[2];trajs=sys.argv[3:]
S=6
g=list(csv.DictReader(open(area+'/area-grid.csv')))
xs=[float(r['x']) for r in g];zs=[float(r['z']) for r in g];x0,x1,z0,z1=min(xs),max(xs),min(zs),max(zs)
ys=[float(r['y']) for r in g];ymin,ymax=min(ys),max(ys)
W,H=int((x1-x0)*S)+40,int((z1-z0)*S)+40;img=Image.new('RGB',(W,H),'white');d=ImageDraw.Draw(img)
P=lambda x,z:(20+(x-x0)*S,20+(z1-z)*S)
for r in g:
    x,z,y=float(r['x']),float(r['z']),float(r['y']);n=r['collider']
    v=int(60+180*(y-ymin)/(ymax-ymin+1e-6))
    c=(170,120,80) if 'driving surface' in n else (220,40,40) if 'barrier' in n else (v,v,v)
    a=P(x-.5,z+.5);d.rectangle([a,(a[0]+S,a[1]+S)],fill=c)
rt=list(csv.DictReader(open(area+'/area-routes.csv')))
cols={'Main':(0,160,160),'Summit Traverse':(230,160,0)}
for name in set(r['route'] for r in rt):
    pts=[r for r in rt if r['route']==name];c=cols.get(name,(120,0,160))
    for i in range(1,len(pts)):
        if float(pts[i]['s'])-float(pts[i-1]['s'])<1.5:d.line([P(float(pts[i-1]['x']),float(pts[i-1]['z'])),P(float(pts[i]['x']),float(pts[i]['z']))],fill=c,width=2)
    for r in pts:
        if float(r['s'])%10==0:d.text(P(float(r['x']),float(r['z'])),f"{name[:2]}{float(r['s']):.0f}",fill=c)
for t in trajs:
    rows=list(csv.DictReader(open(t)))
    for i in range(1,len(rows)):
        a,b=rows[i-1],rows[i]
        if not(x0<float(b['x'])<x1 and z0<float(b['z'])<z1):continue
        c=(0,0,255) if b['wheels']=='0' else (0,170,0)
        d.line([P(float(a['x']),float(a['z'])),P(float(b['x']),float(b['z']))],fill=c,width=2)
        if i%10==0 and b['wheels']=='0':d.text(P(float(b['x']),float(b['z'])),f"{float(b['hAbove']):.0f}",fill=(0,0,200))
for i in range(int(x0),int(x1)+1,10):d.text(P(i,z1+2),str(i),fill='black')
for i in range(int(z0),int(z1)+1,10):d.text(P(x0-3,i),str(i),fill='black')
img.save(out)
