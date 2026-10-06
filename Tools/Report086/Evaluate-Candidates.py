"""0.86 Part C (read only): numbers for the proposed Forest Loop Forward shortcuts.
Speed model per class from the measured lap (autopilot, drive traces): the median speed it held on main stretches of
similar grade (20 m baseline, +-4 %); a jump's flight is timed from its launch speed and angle. Ground lines are timed on
the ground profile (or on the designed profile where the trail would be regraded), flights by projectile motion.
Usage: python Evaluate-Candidates.py <survey dir> <trace dir> <out txt>"""
import sys,os,math,csv
import numpy as np
sdir,tdir,outp=sys.argv[1:4]
exec(open(os.path.join(os.path.dirname(__file__),'survey_common.py')).read())
G=9.81
# --- measured speed per grade, per class
ys_main=np.array([p[1] for p in main]);st=np.arange(len(main))*STEP
grade=np.zeros(len(main))
for i in range(len(main)):
    a=max(0,i-5);b=min(len(main)-1,i+5);grade[i]=(ys_main[b]-ys_main[a])/((b-a)*STEP)
model={};prof={}
for cls,fn in (('bike','moto'),('ATV','atv'),('car','original')):
    rows=list(csv.DictReader(open(os.path.join(tdir,f'drive-LakeWoods-{fn}-Main-0-20.csv'))))
    sp=[];prev=-1
    for r in rows:
        s=float(r['station']);v=float(r['speed'])
        if prev>1500 and s<500:break
        prev=s
        if v>3 and int(r['wheels'])>0:sp.append((s,v))
    sp=np.array(sp);g_at=np.interp(sp[:,0],st,grade);prof[cls]=sp
    tab={}
    for gb in range(-50,55,5):
        sel=np.abs(g_at-gb/100)<=.04
        if sel.sum()>=15:tab[gb]=float(np.median(sp[sel,1]))
    model[cls]=tab
def speed(cls,g):
    t=model[cls];keys=sorted(t);gb=g*100
    if gb<=keys[0]:return t[keys[0]]
    if gb>=keys[-1]:return t[keys[-1]]
    return float(np.interp(gb,keys,[t[k] for k in keys]))
def main_time(cls,a,b):
    sp=prof[cls];sel=(sp[:,0]>=a)&(sp[:,0]<=b);ss=sp[sel]
    # time from the trace itself: distance / speed per 5 m
    t=0
    for s in np.arange(a,b,5):
        near=ss[np.abs(ss[:,0]-s-2.5)<4]
        v=np.median(near[:,1]) if len(near) else speed(cls,0)
        t+=5/v
    return t
# --- a candidate: list of legs. ('ground', [(x,z),...], designed_profile or None) / ('flight', launch_speed_from, angle_deg, (x0,y0,z0)->(x1,y1,z1))
def ground_leg(pts,step=2.0,clamp=None):
    xs=[];zs=[]
    for (x0,z0),(x1,z1) in zip(pts,pts[1:]):
        L=math.hypot(x1-x0,z1-z0);n=max(1,int(L/step))
        for i in range(n):xs.append(x0+(x1-x0)*i/n);zs.append(z0+(z1-z0)*i/n)
    xs.append(pts[-1][0]);zs.append(pts[-1][1]);ys=np.array([height(x,z) for x,z in zip(xs,zs)])
    return np.array(xs),ys,np.array(zs)
def leg_time(cls,xs,ys,zs,maxgrade=None):
    t=0;L=0;maxg=0
    for i in range(len(xs)-1):
        d=math.hypot(xs[i+1]-xs[i],zs[i+1]-zs[i]);dy=ys[i+1]-ys[i];g=dy/max(d,1e-6)
        if maxgrade is not None:g=max(-maxgrade,min(maxgrade,g))
        l=math.hypot(d,g*d);L+=l;t+=l/speed(cls,g);maxg=max(maxg,abs(g))
    return t,L
def flight(v,ang,dx,dy):
    """time and landing angle for a launch at speed v, angle ang (deg), to land dx further, dy lower (dy>0 = drop)."""
    c=math.cos(math.radians(ang));s=math.sin(math.radians(ang))
    t=dx/(v*c);y=v*s*t-.5*G*t*t;vy=v*s-G*t;return t,y,math.degrees(math.atan2(-vy,v*c))
def needed(dx,dy,ang):
    """launch speed to fly dx horizontally and come down dy lower at launch angle ang."""
    c=math.cos(math.radians(ang));s=math.sin(math.radians(ang))
    den=2*c*c*(dx*s/c+dy)
    return math.sqrt(G*dx*dx/den) if den>0 else float('nan')
out=open(outp,'w')
def P(*a):print(*a);print(*a,file=out)
P('Speed held on the main by grade (m/s, median of the measured lap; grade in %):')
for cls in model:P(f'  {cls:4s}: '+'  '.join(f'{k:+d}:{v:.0f}' for k,v in sorted(model[cls].items())))
P()
# --- a flight over the real ground (plus an optional designed landing surface): where each class comes down.
def fly(v,ang,lip,head,surface=None,dt=.01):
    """lip=(x,y,z); head=(dx,dz) unit; surface(d)-> designed height or None. Returns d, y, ground slope, impact angle, t."""
    c=math.cos(math.radians(ang));s=math.sin(math.radians(ang));t=0
    while t<6:
        t+=dt;d=v*c*t;y=lip[1]+v*s*t-.5*G*t*t;x=lip[0]+head[0]*d;z=lip[2]+head[1]*d
        gy=height(x,z)
        if surface:
            sy=surface(d)
            if sy is not None:gy=sy
        if y<=gy and d>2:
            g1=(height(x+head[0]*1,z+head[1]*1) if not surface or surface(d+1) is None else surface(d+1))-gy
            fall=math.degrees(math.atan2(-(v*s-G*t),v*c));slope=math.degrees(math.atan(g1))
            return d,gy,g1,fall+slope,t
    return None
def ground_time(cls,pts,clamp):
    xs,ys,zs=ground_leg(pts);return leg_time(cls,xs,ys,zs,clamp)
def head_of(a,b):dx,dz=b[0]-a[0],b[1]-a[1];L=math.hypot(dx,dz);return (dx/L,dz/L)
def at_s(s):p=at(s);return (p[0],p[2])
# --- a candidate: ground legs (timed on the ground, or as regraded to within +-clamp) and flights (timed by projectile
# motion from the run-up speed; where each class really comes down is listed separately below).
def evaluate(name,a,b,legs):
    P(f'== {name}: leaves the main at s {a} ({at(a)[0]:.0f},{at(a)[1]:.1f},{at(a)[2]:.0f}), rejoins at s {b} ({at(b)[0]:.0f},{at(b)[1]:.1f},{at(b)[2]:.0f}); main between them {b-a} m')
    total={c:0.0 for c in model};length=0
    for leg in legs:
        if leg[0]=='ground':
            _,pts,clamp,label=leg;xs,ys,zs=ground_leg(pts)
            for c in model:
                t,L=leg_time(c,xs,ys,zs,clamp);total[c]+=t
            length+=L;raw=np.diff(ys)/np.maximum(1e-6,np.hypot(np.diff(xs),np.diff(zs)))
            P(f'   ground {label}: {L:.0f} m, height {ys[0]:.1f} -> {ys[-1]:.1f} (min {ys.min():.1f}, max {ys.max():.1f}), steepest now {raw.max()*100:+.0f} % / {raw.min()*100:+.0f} %'+(f'; timed as regraded to within +-{clamp*100:.0f} %' if clamp else ''))
        else:
            _,label,(x0,y0,z0),(x1,y1,z1),ang,vfrom=leg
            dx=math.hypot(x1-x0,z1-z0);dy=y0-y1;length+=dx
            P(f'   JUMP {label}: lip ({x0:.0f},{y0:.1f},{z0:.0f}) -> landing ({x1:.0f},{y1:.1f},{z1:.0f}): gap {dx:.0f} m, {dy:+.1f} m lower, lip {ang:.0f} deg')
            for c in model:
                v=vfrom(c);need=needed(dx,dy,ang);t,_,land=flight(max(v,need),ang,dx,dy);total[c]+=t
                P(f'      {c:4s}: run-up speed {v:.0f} m/s, needs {need:.1f} m/s to reach the landing ({"clears, margin "+format(v-need,".1f")+" m/s" if v>=need else "SHORT by "+format(need-v,".1f")+" m/s"})')
    P(f'   shortcut length {length:.0f} m against {b-a} m of main: {b-a-length:.0f} m shorter')
    for c in model:
        mt=main_time(c,a,b);P(f'   {c:4s}: main {mt:.1f} s, shortcut {total[c]:.1f} s -> saves {mt-total[c]:.1f} s')
    P()
def report_flight(label,lip,head,ang,speeds,surface=None):
    P(f'   {label}: lip ({lip[0]:.0f},{lip[1]:.1f},{lip[2]:.0f}) at {ang:+.0f} deg, heading {math.degrees(math.atan2(head[0],head[1]))%360:.0f} deg')
    for cls,v in speeds:
        r=fly(v,ang,lip,head,surface)
        if r:d,gy,g1,imp,t=r;P(f'      {cls:9s} {v:4.1f} m/s: comes down {d:5.1f} m out at y {gy:.1f} on ground sloping {g1*100:+.0f} %, {t:.2f} s in the air, meets the ground at {imp:.0f} deg ({"smooth" if imp<12 else "firm" if imp<22 else "hard" if imp<32 else "VERY HARD"})')
run=lambda g:(lambda c:speed(c,g))
A=[('ground',[at_s(1520),(300,-209),(350,-212)],.35,'along the bowl\'s south-west arm'),
   ('ground',[(350,-212),(372,-213)],.25,'down into the bowl (built 25 % ramp in place of the 11 m drop)'),
   ('ground',[(372,-213),(447,-217)],.35,'across the bowl floor south of the lake and pool, then up the tongue behind the pool (regraded, now up to 57 %)'),
   ('flight','Ridge Jump off the tongue over the east hollow',(452,55.5,-217.6),(512,42.7,-221.1),8,run(.35)),
   ('ground',[(512,-221.1),at_s(1890)],.35,'down the far rim to CP4 (built landing, run-out)')]
evaluate('A  Ridge Jump (CP3 to CP4, over the tongue behind the House 3 pool)',1520,1890,A)
B=[('ground',[at_s(1676),(380,-268),(398,-260)],.30,'off the main after J6 and down the inner face of the rim into the channel (built 30 % ramp, about 14 m down)'),
   ('ground',[(398,-260),(413,-257),(440,-262),(470,-250),(484,-243.4)],None,'along the low channel at the foot of the rim'),
   ('flight','Crest Jump over the low crest',(484,41.3,-243.4),(509,36.9,-235.6),10,run(.10)),
   ('ground',[(509,-235.6),at_s(1885)],None,'down the natural slope, rejoining just before CP4')]
evaluate('B  Channel Run and Crest Jump (below the south rim to CP4)',1676,1885,B)
C=[('ground',[at_s(1850),at_s(2090)],.38,'straight up the hillside west of the main\'s climb (steepest 50 m regraded)')]
evaluate('C  Summit Climb (cuts the hook before the finish; no jump)',1850,2090,C)
D=[('ground',[at_s(1690),(380,-276),(397,-258)],.35,'down the rim into the south pocket (built chute; a jump cannot land here)'),
   ('ground',[(397,-258),(458,-191),(513,-130),(541,-100),at_s(2090)],.35,'climb up the House 3 hill and across its top')]
evaluate('D  Rim Chute and House 3 Hill (surveyed, not proposed)',1690,2090,D)
P('== Flights over the ground as it is now (no built landing): where each class comes down from the lip')
ridge_lip=(452,55.5,-217.6);rh=head_of((452,-217.6),(512,-221.1))
for ang in (8,15):report_flight(f'A Ridge Jump, lip {ang} deg',ridge_lip,rh,ang,[('slow',18),('ATV',23),('bike/car',31),('fast',36)])
crest=(484,41.3,-243.4);ch=head_of((484,-243.4),(509,-235.6))
for ang in (8,10,14):report_flight(f'B Crest Jump, lip {ang} deg (1.5 m kicker on the crest)',crest,ch,ang,[('slow',15),('ATV',26),('bike/car',31),('fast',35)])
bl=(380,50.5,-276);bh=head_of((380,-276),(397,-258))
report_flight('D Rim Drop if it were a jump (flat lip)',bl,bh,0,[('slow',15),('ATV',29),('bike/car',31)])
out.close()
