import re,os,sys
f=sys.argv[1]
g={}
for l in open(f):
    m=re.match(r"([-\d.]+),([-\d.]+): [^|]*?\] ([-\d.]+) n([-\d.]+)",l)
    if m: g[(float(m[1]),float(m[2]))]=(float(m[3]),float(m[4]))
xs=sorted({k[0] for k in g}); zs=sorted({k[1] for k in g})
print("z/x    "+" ".join(f"{x:6.1f}" for x in xs))
for z in zs: print(f"{z:6.1f} "+" ".join(f"{g.get((x,z),(0,0))[0]:6.2f}" for x in xs))
print()
for z in zs: print(f"{z:6.1f} "+" ".join(f"{g.get((x,z),(0,0))[1]:6.3f}" for x in xs))
