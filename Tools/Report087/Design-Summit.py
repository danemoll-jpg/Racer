"""0.87 Part B: design the Summit Climb from knots (x,z), Catmull-Rom like ReverseShortcutsAuthoring.Curve (0.65 m steps).
Profile: ground along the line, smoothed, grade-limited, then crest/run-out shaping. Reports cut/fill, trees on the trail
and at its edges, and draws a map + profile. Writes design.json (points with y, plus obstacles) for the Unity author.
Usage: python Design-Summit.py <survey dir> <out dir>"""
import sys,os,json,math
import numpy as np,matplotlib;matplotlib.use('Agg');import matplotlib.pyplot as plt
sdir=sys.argv[1];out=sys.argv[2];os.makedirs(out,exist_ok=True)
exec(open(os.path.join(os.path.dirname(os.path.abspath(__file__)),'summit_common.py')).read())
D=json.load(open(os.path.join(os.path.dirname(os.path.abspath(__file__)),'summit-knots.json')))
HW=D['halfWidth']
fs=D['forkStation'];rs=D['rejoinStation']
def mpt(s):r=mainat(s);return np.array([r[1],r[3]])
A0=np.array([523.8,-254.0]);U=np.array([569.0,-69.0])-A0;U/=np.linalg.norm(U);N=np.array([U[1],-U[0]])
def al(a,l):return A0+U*a+N*l
knots=[mpt(fs-8),mpt(fs)]+[al(*k) if D.get('frame')=='al' else np.array(k,float) for k in D['knots']]+[mpt(rs),mpt(rs+8)]
for o in D.get('obstacles',[]):
    if 'a' in o:o['x'],o['z']=map(float,al(o['a'],o['l']))
def curve(k):
    p=[]
    for i in range(len(k)-1):
        a=k[max(0,i-1)];b=k[i];c=k[i+1];d=k[min(len(k)-1,i+2)];n=max(1,math.ceil(np.linalg.norm(c-b)/.65))
        for j in range(n):
            t=j/n;p.append(.5*(2*b+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t))
    p.append(k[-1]);return np.array(p)
pts=curve(knots)
# drop the lead-in on the main (first knot) : branch starts at the fork knot
i0=int(np.argmin(np.linalg.norm(pts-knots[1],axis=1)));i1=int(np.argmin(np.linalg.norm(pts-knots[-2],axis=1)));pts=pts[i0:i1+1]
seg=np.linalg.norm(np.diff(pts,axis=0),axis=1);s=np.concatenate([[0],np.cumsum(seg)]);L=s[-1]
g=np.array([height(x,z) for x,z in pts])
# heading and curvature
hd=np.degrees(np.arctan2(np.gradient(pts[:,0]),np.gradient(pts[:,1])))
dh=np.gradient(np.unwrap(np.radians(hd)))/np.maximum(np.gradient(s),1e-3);R=1/np.maximum(np.abs(dh),1e-4)
# profile: resample to 1 m
S=np.arange(0,L,1.0);S=np.append(S,L);G=np.interp(S,s,g)
y0=G[0];y1=G[-1]
P=D['profile']
gmax=P['maxGrade'];crest=P['crestS'];Rv=P['crestW']
# ground along the line (the trail's own width averaged), smoothed
Y=np.convolve(np.pad(G,(6,6),mode='edge'),np.ones(13)/13,mode='valid');Y[0]=y0
# run-out after the crest: straight to the rejoin height
yc=float(np.interp(crest,S,Y)) if P.get('crestY') is None else P['crestY']
ro_s=[crest]+[c[0] for c in P.get('runout',[])]+[L];ro_y=[yc]+[c[1] for c in P.get('runout',[])]+[y1]
for i,st in enumerate(S):
    if st>=crest:Y[i]=float(np.interp(st,ro_s,ro_y))
fixed=S>=crest
# grade limit, balanced: relax pairs symmetrically (cut the top of a too-steep step, fill its foot)
for it in range(4000):
    worst=0
    for i in range(len(S)-1):
        d=Y[i+1]-Y[i];lim=gmax*(S[i+1]-S[i])
        if d>lim:
            e=d-lim;worst=max(worst,e)
            if fixed[i+1]:Y[i]+=e
            elif i==0:Y[i+1]-=e
            else:Y[i]+=e/2;Y[i+1]-=e/2
    if worst<1e-4:break
# vertical curves: crest (convex) of radius Rv, the fork sag
def vcurve(c,R):
    i=int(np.argmin(np.abs(S-c)));g1=(Y[i]-Y[i-3])/(S[i]-S[i-3]);g2=(Y[i+3]-Y[i])/(S[i+3]-S[i]);T=abs(g1-g2)*R/2
    for j,st in enumerate(S):
        x=st-(c-T)
        if 0<=x<=2*T:Y[j]=Y[i]-g1*T+g1*x+(g2-g1)*x*x/(4*T)
    return T
def box(Y,w):
    h=w//2;return np.convolve(np.pad(Y,(h,h),mode='edge'),np.ones(2*h+1)/(2*h+1),mode='valid')
# vertical curves by moving averages: everywhere ~7 m, ~crestW m around the crest (R ~ crestW / grade change)
Y7=box(box(Y,7),7);Yc=box(box(Y,P['crestW']),P['crestW'])
w=np.clip(1-np.abs(S-crest)/(P['crestW']*1.5),0,1);w=w*w*(3-2*w)
Y=Y7*(1-w)+Yc*w;Y[0]=y0;Y[-1]=y1;Tc=P['crestW']/2
# leave the main at its own grade and blend in
inLen=P['inLen'];mg=(mainat(fs+2)[2]-mainat(fs-2)[2])/4
for i,st in enumerate(S):
    if st<inLen:w=(st/inLen)**2*(3-2*st/inLen);Y[i]=(1-w)*(y0+mg*st)+w*Y[i]
lipL=0;runGrade=(y1-yc)/(L-crest)
gr=np.gradient(Y,S)*100
# rough section
ro=D['rough']
# cut/fill over the trail width
cut=fill=0;maxcut=maxfill=0
for i,st in enumerate(S):
    d=Y[i]-G[i];
    if d>0:fill+=d*2*HW;maxfill=max(maxfill,d)
    else:cut+=-d*2*HW;maxcut=max(maxcut,-d)
# trees
def latof(x,z):
    d=np.hypot(pts[:,0]-x,pts[:,1]-z);i=int(np.argmin(d));return s[i],d[i],i
onTrail=[];edge=[]
for t in trunks:
    st,d,i=latof(t[0],t[1])
    if 0<st<L and d<HW+.6+t[2]:onTrail.append((st,d,t))
    elif 0<st<L and d<HW+4:edge.append((st,d,t))
rep=[f"length {L:.1f} m, start {pts[0]} end {pts[-1]}, ground {y0:.2f} -> {y1:.2f}",
     f"cut {cut:.0f} m3 (max {maxcut:.2f} m) fill {fill:.0f} m3 (max {maxfill:.2f} m) over the trail width only",
     f"max grade {gr.max():.1f} %, min {gr.min():.1f} %; crest at s {crest} y {yc:.2f} (crest smoothing {Rv} m); run-out grade {runGrade*100:.1f} %",
     f"tightest centreline radius {R[5:-5].min():.1f} m",f"trunk colliders on the trail ({len(onTrail)}):"]+[f"  s {a:.1f} d {b:.1f} {t[4]} at {t[0]:.1f},{t[1]:.1f} r {t[2]}" for a,b,t in sorted(onTrail)]+[f"trunks at the edges (within {HW}+4 m): {len(edge)}"]
for st in range(0,int(L)+1,5):
    i=int(np.argmin(np.abs(S-st)));j=int(np.argmin(np.abs(s-st)))
    rep.append(f"  s {st:5.0f} x {pts[j,0]:7.2f} z {pts[j,1]:8.2f} head {hd[j]:6.1f} R {min(R[j],999):5.0f} ground {G[i]:6.2f} trail {Y[i]:6.2f} ({Y[i]-G[i]:+.2f}) grade {gr[i]:5.1f}")
open(os.path.join(out,'design.txt'),'w').write("\n".join(rep));print("\n".join(rep[:6+len(onTrail)+1]))
yy=np.interp(s,S,Y)
json.dump({'halfWidth':HW,'points':[[float(x),float(y),float(z)] for (x,z),y in zip(pts,yy)],'obstacles':D.get('obstacles',[]),'rough':ro,'crest':crest,'lipLen':lipL,
  'forkStation':fs,'rejoinStation':rs},open(os.path.join(out,'design.json'),'w'),indent=0)
fig,ax=plt.subplots(1,2,figsize=(20,14),gridspec_kw={'width_ratios':[1,1.2]})
a=ax[0];a.imshow(grid,origin='lower',extent=[X0,X0+W,Z0,Z0+H],cmap='terrain',alpha=.7)
a.contour(np.arange(W)+X0+.5,np.arange(H)+Z0+.5,grid,levels=range(20,100,2),colors='k',linewidths=.3)
m=mainarr[(mainarr[:,0]>1780)&(mainarr[:,0]<2160)];a.plot(m[:,1],m[:,3],'w-',lw=3)
for r in m[::10]:a.annotate(f"{r[0]:.0f}",(r[1],r[3]),fontsize=7,color='w')
nrm=np.stack([np.gradient(pts[:,1]),-np.gradient(pts[:,0])],1);nrm/=np.linalg.norm(nrm,axis=1)[:,None]
a.plot(*(pts+nrm*HW).T,'m-',lw=.8);a.plot(*(pts-nrm*HW).T,'m-',lw=.8);a.plot(*pts.T,'m--',lw=.5)
for k in knots:a.plot(*k,'mx')
for t in trunks:a.add_patch(plt.Circle((t[0],t[1]),max(t[2],.35),color='k'))
dd=np.array([(x,z,sx) for x,z,sx,sy,by,r,v in drawn if sx>1.5]);a.scatter(dd[:,0],dd[:,1],s=dd[:,2]*4,facecolors='none',edgecolors='g',lw=.4)
for o in D.get('obstacles',[]):a.add_patch(plt.Circle((o['x'],o['z']),o['r'],color='grey'))
for st in range(0,int(L),20):j=int(np.argmin(np.abs(s-st)));a.annotate(str(st),pts[j],fontsize=8,color='m')
a.set_xlim(495,600);a.set_ylim(-265,-45);a.set_aspect('equal');a.grid(alpha=.3)
b=ax[1];b.plot(S,G,'k-',label='ground');b.plot(S,Y,'m-',label='trail');b2=b.twinx();b2.plot(S,gr,'b-',lw=.6);b2.axhline(38,color='b',ls=':');b.legend();b.grid(alpha=.3)
plt.savefig(os.path.join(out,'design.png'),dpi=90,bbox_inches='tight')
