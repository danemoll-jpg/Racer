# Map from Report069Cells: per cell, top surface class and (top - pavement) height difference.
import sys
rows=[l.rstrip('\n').split(',',2) for l in open(sys.argv[1])][1:]
cells={}
for x,z,s in rows:
    hs=[h.split(':') for h in s.split('|') if h]
    hs=[(float(a),b,float(c)) for a,b,c in hs]
    up=[h for h in hs if h[2]>0.05]
    pave=[h for h in up if 'driving surface' in h[1]]
    top=up[0] if up else None
    cells[(float(x),float(z))]=(top,pave[0] if pave else None,hs)
xs=sorted(set(k[0] for k in cells)); zs=sorted(set(k[1] for k in cells),reverse=True)
names={}
print("TOP class: P pavement on top, letter = other mesh on top over pavement (digit row shows how far above, 0.1 m), lowercase = no pavement under")
for z in zs:
    line=f"{z:7.1f} "
    for x in xs:
        top,pave,_=cells[(x,z)]
        if not top: line+=' '; continue
        if 'driving surface' in top[1]: line+='.'; continue
        k=names.setdefault(top[1],chr(65+len(names)))
        if pave:
            d=top[0]-pave[0]; line+= k if d>0.05 else '_'
        else: line+=k.lower()
    print(line)
for n,k in names.items(): print(k,n)
print("HEIGHT of other-on-top above pavement (0-9 = tenths of m, + = >1 m)")
for z in zs:
    line=f"{z:7.1f} "
    for x in xs:
        top,pave,_=cells[(x,z)]
        if top and pave and 'driving surface' not in top[1]:
            d=top[0]-pave[0]; line+= '+' if d>=1 else (str(int(d*10)) if d>0.05 else '_')
        else: line+=' ' if not pave else '.'
    print(line)
