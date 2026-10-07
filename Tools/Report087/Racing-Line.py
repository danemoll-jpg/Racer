"""0.87 Part B: the AI's line through the Summit Climb. The carved trail (summit-points.txt, centre line) stays as it is; the
branch's points become a smoothed line that may use the trail's width (up to +-MAXOFF m off the centre, so a vehicle's
body stays inside the edges), cutting the kinks as a driver would. Elastic band: each point moves sideways toward the
straight line between its neighbours, clamped; the fork curve (first 40 m) and the last 14 m (on the main) stay on the centre line.
Writes summit-line.txt (x z, every ~0.65 m) and prints the tightest radii before / after."""
import numpy as np,os,sys
d=os.path.dirname(os.path.abspath(__file__))
P=np.array([list(map(float,l.split())) for l in open(os.path.join(d,'summit-points.txt')) if not l.startswith('#')])
xz=P[:,[0,2]];n=len(xz);s=np.concatenate([[0],np.cumsum(np.linalg.norm(np.diff(xz,axis=0),axis=1))])
t=np.gradient(xz,axis=0);t/=np.linalg.norm(t,axis=1)[:,None];nr=np.stack([t[:,1],-t[:,0]],1)
MAXOFF=float(sys.argv[1]) if len(sys.argv)>1 else 1.55
off=np.zeros(n);fixed=(s<40)|(s>s[-1]-14)
for it in range(6000):
    q=xz+nr*off[:,None]
    mid=(q[:-2]+q[2:])/2;delta=((mid-q[1:-1])*nr[1:-1]).sum(1)
    off[1:-1]+=.5*delta;off[fixed]=0;off=np.clip(off,-MAXOFF,MAXOFF)
# blend the fixed ends in over 10 m
w=np.clip(np.minimum(s-40,s[-1]-14-s)/8,0,1);off*=w
q=xz+nr*off[:,None]
def radii(p):
    a=p[:-2]-p[1:-1];b=p[2:]-p[1:-1];c=p[2:]-p[:-2]
    cr=np.abs(a[:,0]*b[:,1]-a[:,1]*b[:,0]);return np.linalg.norm(a,axis=1)*np.linalg.norm(b,axis=1)*np.linalg.norm(c,axis=1)/np.maximum(2*cr,1e-9)
def tight(p,lo,hi,step=8):
    i=np.arange(0,n,max(1,int(step/.65)));pp=p[i];ss=s[i];r=radii(pp);m=(ss[1:-1]>lo)&(ss[1:-1]<hi);return r[m].min()
for lo,hi,nm in [(0,40,'fork'),(55,95,'kink 1'),(125,160,'kink 2'),(160,217,'crest and merge')]:
    print(f"{nm}: tightest radius (8 m chords) centre {tight(xz,lo,hi):.0f} m -> line {tight(q,lo,hi):.0f} m")
print(f"largest offset {np.abs(off).max():.2f} m")
open(os.path.join(d,'summit-line.txt'),'w').write("# 0.87 Part B: the AI line through the Summit Climb (x z), Racing-Line.py, max offset %.2f m\n"%MAXOFF+"".join(f"{a:.3f} {b:.3f}\n" for a,b in q))
