"""0.86 Part C (read only): survey every chord between two points of the Forest Loop Forward main (LakeWoods) for a second
shortcut. Inputs from the Unity probes (Report085Map heights, Report086Obstacles, drive traces per class).
For each pair of main stations a < b (10 m steps) whose main distance is 60-700 m, outside the Echo Cave span and not
across the start line: the straight line between them on the ground: length, grades (4 m baseline), climb, water crossed,
trunk colliders within 2.5 m, buildings within 5 m, how close it comes to the main elsewhere; main time a->b per class
from the measured lap traces, chord time at the class's straight-line speed (its 80th-percentile lap speed, never more
than it carried on the main at a and b + 4 m/s), time saved. Writes candidates.tsv (all) and best.txt (best per area).
Usage: python Survey-ForestForward.py <survey dir> <trace dir> <out dir>"""
import sys,csv,math,os,json
import numpy as np
sdir,tdir,odir=sys.argv[1:4];os.makedirs(odir,exist_ok=True)
# heights
lines=open(os.path.join(sdir,'map-lap-LakeWoods.csv')).read().splitlines()
hdr=lines[0].split();X0=float(hdr[2]);Z0=float(hdr[4]);CELL=float(hdr[6]);W=int(hdr[8]);H=int(hdr[10])
grid=np.array([[float(v) if v else np.nan for v in l.split(',')] for l in lines[1:]])
def height(x,z):
    fx=(x-X0)/CELL-.5;fz=(z-Z0)/CELL-.5;i=int(math.floor(fx));k=int(math.floor(fz));tx=fx-i;tz=fz-k
    i=max(0,min(W-2,i));k=max(0,min(H-2,k))
    a=grid[k,i];b=grid[k,i+1];c=grid[k+1,i];d=grid[k+1,i+1]
    return (a*(1-tx)+b*tx)*(1-tz)+(c*(1-tx)+d*tx)*tz
# routes
routes={}
for l in open(os.path.join(sdir,'branches-LakeWoods.tsv')):
    name,pts=l.rstrip('\n').split('\t');routes[name]=[tuple(map(float,p.split(','))) for p in pts.split()]
main=routes['Main'];STEP=2.0;LEN=len(main)*STEP
def at(s):i=int(round(s/STEP))%len(main);return main[i]
echo=routes['Echo Cave']
# water
water=[]
for l in list(open(os.path.join(sdir,'water-LakeWoods.tsv')))[1:]:
    p=l.rstrip('\n').split('\t');name=p[0].split('/')[-1];rnd=p[1]=='True';surf=float(p[2]);cx,cz=map(float,p[4].split(','));sx,sz=map(float,p[5].split('x'));rot=math.radians(float(p[6]))
    water.append((name,rnd,surf,cx,cz,sx,sz,rot))
def in_water(x,z):
    for name,rnd,surf,cx,cz,sx,sz,rot in water:
        dx,dz=x-cx,z-cz;lx=dx*math.cos(rot)-dz*math.sin(rot);lz=dx*math.sin(rot)+dz*math.cos(rot)
        if rnd:
            if (lx/(sx/2))**2+(lz/(sz/2))**2<=1:return name
        elif abs(lx)<=sx/2 and abs(lz)<=sz/2:return name
    return None
# obstacles
obs={'trunk':[],'building':[],'prop':[]}
for r in csv.DictReader(open(os.path.join(sdir,'obstacles-LakeWoods.tsv')),delimiter='\t'):
    if r.get('sz') is None or r.get('kind') not in obs:continue
    obs[r['kind']].append((float(r['cx']),float(r['cz']),float(r['sx']),float(r['sz']),r['path']))
for k in obs:obs[k]=np.array([o[:4] for o in obs[k]]) if obs[k] else np.zeros((0,4))
# speed profiles (first lap of each trace)
prof={}
for cls,fn in (('bike','moto'),('ATV','atv'),('car','original')):
    rows=list(csv.DictReader(open(os.path.join(tdir,f'drive-LakeWoods-{fn}-Main-0-20.csv'))))
    bins=[[] for _ in range(int(LEN//5)+1)];prev=-1
    for r in rows:
        s=float(r['station']);v=float(r['speed'])
        if prev>1500 and s<500:break
        prev=s
        if v>3:bins[int(s//5)].append(v)
    v=np.array([np.median(b) if b else np.nan for b in bins]);idx=np.arange(len(v));ok=~np.isnan(v)
    v=np.interp(idx,idx[ok],v[ok]);prof[cls]=v
def main_time(cls,a,b):
    v=prof[cls];return sum(5/v[int(s//5)] for s in np.arange(a,b,5))
straight={c:float(np.percentile(prof[c],80)) for c in prof}
# candidates
ECHO=(565,1250);START=65
out=[];
def dist_to_main(x,z,excl_a,excl_b):
    best=1e9
    for i,(px,py,pz) in enumerate(main):
        s=i*STEP
        if excl_a-15<=s<=excl_b+15:continue
        d=math.hypot(px-x,pz-z)
        if d<best:best=d
    return best
mainarr=np.array(main)
for a in range(80,int(LEN)-60,10):
    for b in range(a+60,min(a+700,int(LEN)),10):
        if a<START<b:continue
        if min(b,ECHO[1])-max(a,ECHO[0])>20:continue
        pa=at(a);pb=at(b);L=math.hypot(pb[0]-pa[0],pb[2]-pa[2]);dm=b-a
        if L<40 or dm-L<25:continue
        n=max(2,int(L/2));xs=np.linspace(pa[0],pb[0],n+1);zs=np.linspace(pa[2],pb[2],n+1);ys=np.array([height(x,z) for x,z in zip(xs,zs)])
        if np.isnan(ys).any():continue
        # fix ends to the main surface
        seg=L/n;g=np.diff(ys)/seg;g4=(ys[2:]-ys[:-2])/(2*seg)
        L3=float(np.sum(np.sqrt(seg**2+np.diff(ys)**2)))
        wet=[in_water(x,z) for x,z in zip(xs,zs)];wl=sum(seg for w in wet if w);wnames=sorted(set(w for w in wet if w))
        # obstacles near the line
        def near(arr,r):
            if len(arr)==0:return 0
            ax,az=pa[0],pa[2];dx,dz=pb[0]-ax,pb[2]-az;t=np.clip(((arr[:,0]-ax)*dx+(arr[:,1]-az)*dz)/(dx*dx+dz*dz),0,1)
            d=np.hypot(arr[:,0]-(ax+t*dx),arr[:,1]-(az+t*dz))-np.maximum(arr[:,2],arr[:,3])/2
            return int(np.sum((d<r)&(t>.03)&(t<.97)))
        tr=near(obs['trunk'],2.5);bu=near(obs['building'],5);pr=near(obs['prop'],2)
        # closest approach to the main away from the ends (does the line run into / along the main?)
        mids=np.array([(x,z) for x,z in zip(xs[3:-3],zs[3:-3])])
        st=np.arange(len(main))*STEP;outside=(st<a-10)|(st>b+10);inside=(st>=a)&(st<=b)
        if len(mids):
            d=np.hypot(mainarr[:,0][None,:]-mids[:,0][:,None],mainarr[:,2][None,:]-mids[:,1][:,None])
            close=float(np.min(d[:,outside])) if outside.any() else 99
            sep=float(np.max(np.min(d[:,inside],axis=1))) if inside.any() else 0
        else: close=99;sep=0
        saved={}
        for c in prof:
            vs=min(straight[c],max(prof[c][int(a//5)],prof[c][int(b//5)])+4)
            saved[c]=main_time(c,a,b)-L3/vs
        out.append(dict(a=a,b=b,main=dm,chord=round(L,1),chord3=round(L3,1),saved_m=round(dm-L3,1),bike=round(saved['bike'],2),ATV=round(saved['ATV'],2),car=round(saved['car'],2),
            maxup=round(float(np.max(g4)),2),maxdown=round(float(np.min(g4)),2),climb=round(float(np.sum(np.clip(np.diff(ys),0,None))),1),ymin=round(float(ys.min()),1),ymax=round(float(ys.max()),1),
            ya=round(float(ys[0]),1),yb=round(float(ys[-1]),1),water=round(wl,1),waters='|'.join(wnames),trunks=tr,buildings=bu,props=pr,closest_main=round(close,1),max_sep=round(sep,1),
            pa=f'{pa[0]:.0f},{pa[1]:.0f},{pa[2]:.0f}',pb=f'{pb[0]:.0f},{pb[1]:.0f},{pb[2]:.0f}'))
keys=list(out[0].keys())
with open(os.path.join(odir,'C-candidates.tsv'),'w') as f:
    f.write('\t'.join(keys)+'\n')
    for r in sorted(out,key=lambda r:-r['car']):f.write('\t'.join(str(r[k]) for k in keys)+'\n')
# best per area: greedy, a candidate's area = its main span; skip candidates whose span overlaps a chosen one by > 50 %
def overlap(r,q):return max(0,min(r['b'],q['b'])-max(r['a'],q['a']))/min(r['main'],q['main'])
chosen=[]
for r in sorted(out,key=lambda r:-(r['car']+r['ATV']+r['bike'])):
    if r['closest_main']<12:continue
    if any(overlap(r,q)>.5 for q in chosen):continue
    chosen.append(r)
    if len(chosen)>=12:break
with open(os.path.join(odir,'C-best.txt'),'w') as f:
    f.write(f'Lap {LEN:.0f} m. Straight-line speeds used (80th percentile of the measured lap): '+', '.join(f'{c} {v:.1f} m/s' for c,v in straight.items())+'\n')
    f.write('Measured lap speed profile (median per 50 m of main, m/s): station bike/ATV/car\n')
    for s in range(0,int(LEN),50):f.write(f'  {s:5d}: '+' / '.join(f'{np.mean(prof[c][s//5:s//5+10]):.1f}' for c in prof)+'\n')
    f.write('\nBest chord per area of the lap (greedy by total seconds saved; lines that come within 12 m of the main elsewhere excluded):\n')
    for r in chosen:f.write(json.dumps(r)+'\n')
print(open(os.path.join(odir,'C-best.txt')).read())
