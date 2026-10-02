# For Report069Cells output: height of the non-pavement top surface relative to the nearest pavement cell (within 3 m).
import sys,math
rows=[l.rstrip('\n').split(',',2) for l in open(sys.argv[1])][1:]
top={};pav={}
for x,z,s in rows:
    hs=[h.split(':') for h in s.split('|') if h]; hs=[(float(a),b,float(c)) for a,b,c in hs if float(c)>0.05]
    if not hs: continue
    k=(float(x),float(z))
    if 'driving surface' in hs[0][1]: pav[k]=hs[0][0]
    else: top[k]=hs[0]
xs=sorted(set(k[0] for k in top)|set(k[0] for k in pav)); zs=sorted(set(k[1] for k in top)|set(k[1] for k in pav),reverse=True)
def near(k):
    best=None
    for dx in range(-6,7):
        for dz in range(-6,7):
            q=(k[0]+dx*.5,k[1]+dz*.5)
            if q in pav:
                d=math.hypot(dx,dz)*.5
                if best is None or d<best[0]: best=(d,pav[q])
    return best
print("cells beside the pavement: digit = height above nearest pavement in 0.2 m (9 = >=1.8, '-' below by >0.2, '=' within 0.2), '.' pavement")
for z in zs:
    line=f"{z:7.1f} "
    for x in xs:
        k=(x,z)
        if k in pav: line+='.'; continue
        if k not in top: line+=' '; continue
        n=near(k)
        if not n: line+=' '; continue
        d=top[k][0]-n[1]
        line+= '=' if abs(d)<=0.2 else ('-' if d<0 else str(min(9,int(d/0.2))))
    print(line)
