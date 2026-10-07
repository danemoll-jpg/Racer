"""0.87 Part B: loaders for the Summit Climb survey (heights grid, main, trunks, drawn tree pieces). exec with sdir set."""
import math,csv,os
import numpy as np
L=open(os.path.join(sdir,'heights-LakeWoods.csv')).read().splitlines();hd=L[0].split()
X0=float(hd[1]);Z0=float(hd[3]);W=int(hd[7]);H=int(hd[9])
grid=np.array([[float(v) if v else np.nan for v in l.split(',')] for l in L[1:]])
def height(x,z):
    fx=x-X0-.5;fz=z-Z0-.5;i=int(math.floor(fx));k=int(math.floor(fz));tx=fx-i;tz=fz-k
    i=max(0,min(W-2,i));k=max(0,min(H-2,k))
    a=grid[k,i];b=grid[k,i+1];c=grid[k+1,i];d=grid[k+1,i+1]
    return (a*(1-tx)+b*tx)*(1-tz)+(c*(1-tx)+d*tx)*tz
main=[];gates=[];branches=[]
for l in open(os.path.join(sdir,'main-LakeWoods.tsv')):
    p=l.rstrip('\n').split('\t')
    if p[0]=='M':main.append((float(p[1]),float(p[2]),float(p[3]),float(p[4]),float(p[5])))
    elif p[0]=='G':gates.append((int(p[1]),float(p[2]),float(p[3]),float(p[4]),float(p[5]),float(p[6])))
mainarr=np.array(main)
def near_main(x,z,lo=0,hi=1e9):
    m=(mainarr[:,0]>=lo)&(mainarr[:,0]<=hi);a=mainarr[m];d=np.hypot(a[:,1]-x,a[:,3]-z);i=int(np.argmin(d));return a[i,0],float(d[i]),a[i]
def mainat(s):
    i=int(round(s/2))%len(main);return main[i]
trunks=[]
for r in csv.DictReader(open(os.path.join(sdir,'trunks-LakeWoods.tsv')),delimiter='\t'):
    trunks.append((float(r['cx']),float(r['cz']),max(float(r['sx']),float(r['sz']))/2,float(r['bottom']),r['path'],r['type']))
drawn=[]
for r in csv.DictReader(open(os.path.join(sdir,'drawn-LakeWoods.tsv')),delimiter='\t'):
    drawn.append((float(r['bx']),float(r['bz']),float(r['sx']),float(r['sy']),float(r['by']),r['renderer'],int(r['verts'])))
